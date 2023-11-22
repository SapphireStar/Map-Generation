using System.Collections.Generic;
using csDelaunay;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class VoronoiDiagramWrapper
{
    
    public int resolutionX;
    public int resolutionY;
    public int width;
    public int height;
    public int LloydrelaxationInteration;

    Voronoi voronoi;
    public List<Vector2f> centers= new List<Vector2f>();
    public List<Vector2f> Corners = new List<Vector2f>();
    public List<Vector2f> edges = new List<Vector2f>();

    public Dictionary<Vector2f, CornerWrapper> CornersLookup = new Dictionary<Vector2f, CornerWrapper>();
    public Dictionary<Vector2f, CenterWrapper> CentersLookup = new Dictionary<Vector2f, CenterWrapper>();
    public Dictionary<Vector2f, EdgeWrapper> EdgesLookup = new Dictionary<Vector2f, EdgeWrapper>();
   
    public VoronoiDiagramWrapper(int resolutionX, int resolutionY, int width, int height, int lloydIteration)
    {
        this.resolutionX = resolutionX;
        this.resolutionY = resolutionY;
        this.width = width;
        this.height = height;
        this.LloydrelaxationInteration = lloydIteration;

        InitializeVoronoi();
        InitializeCenters();
        InitializeCorners();
        InitializeEdges();
        int d0count = 0;
        int d1count = 0;
        foreach (var item in EdgesLookup.Values)
        {
            if (item.d0 == null)
            {
                d0count++;
            }
            if (item.d1 == null)
            {
                d1count++;
            }
        }
        Debug.Log(d0count+" "+d1count);
    }
    private void InitializeVoronoi()
    {
        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                Vector2f point = new Vector2f(UnityEngine.Random.Range((float)i * ((float)width / (float)resolutionX), (float)(i + 1) * ((float)width / (float)resolutionX)),
                                        UnityEngine.Random.Range((float)j * ((float)height / (float)resolutionY), (float)(j + 1) * ((float)height / (float)resolutionY)));
                centers.Add(point);
            }
        }
        voronoi = new Voronoi(centers, new Rectf(0, 0, width, height), LloydrelaxationInteration, null);

        foreach (var item in voronoi.SiteCoords())
        {
            CentersLookup[item] = new CenterWrapper(CentersLookup.Count, item); 
        }
        
    }
    
    private void InitializeCenters()
    {
        foreach (var item in CentersLookup.Values)
        {
            List<Vector2f> neighbours = voronoi.NeighborSitesForSite(item.Point);
            foreach (var neighbour in neighbours)
            {
                item.neighbours.Add(CentersLookup[neighbour]);
            }
        }

        Debug.Log(CentersLookup);
    }

    private void InitializeCorners()
    {
        foreach (var center in CentersLookup.Values)
        {
            List<Vector2f> centerCorners = voronoi.Region(center.Point);
            List<Vector2f> neighbours = voronoi.NeighborSitesForSite(center.Point);
            //check duplicated corners, if corner duplicated, then not create new cornerwrapper,
            //instead, add old cornerwrapper to current center, and add current centerwrapper to cornerwrapper's touches
            for (int i = 0; i < centerCorners.Count; i++)
            {
                if(!CheckDuplicateCorners(neighbours, center, centerCorners[i]))
                {
                    //create cornerwrapper, if corner is not duplicated, and add corner to current centerwrapper's corners,
                    //and add current centerwrapper to new cornerwrapper's touches
                    CornerWrapper newCorner = CreateCorner(centerCorners[i]);
                    CornersLookup[centerCorners[i]] = newCorner;
                    newCorner.index = Corners.Count;
                    this.Corners.Add(centerCorners[i]);
                    newCorner.touches.Add(center);
                    if (!checkcorner(newCorner.point, center))
                        center.corners.Add(newCorner);
                }
            }

        }
    }
    private bool CheckDuplicateCorners(List<Vector2f> neighbours, CenterWrapper center, Vector2f corner)
    {
        foreach (var neighbourCoord in neighbours)
        {
            CenterWrapper neighbour = CentersLookup[neighbourCoord];
            foreach (var nCorners in neighbour.corners)
            {
                //traverse the corners around the center, check whether there are duplicated corners,
                //delete duplicated corners, and add the corner to both current center and the traversed center
                if (Mathf.Abs(nCorners.point.x - corner.x) < 1e-4
                && Mathf.Abs(nCorners.point.y - corner.y) < 1e-4)
                {
                    checkcorner(nCorners.point, center);
                    nCorners.touches.Add(center);
                    center.corners.Add(nCorners);
                    return true;
                }
            }
        }
        return false;
    }
    private bool checkcorner(Vector2f corner, CenterWrapper center)
    {
        foreach (var item in center.corners)
        {
            float result1 = Mathf.Abs(item.point.x - corner.x);
            float result2 = Mathf.Abs(item.point.y - corner.y);
            if (result1 < 1e-4 && result2<1e-4)
            {
                return true;
            }
        }
        return false;
    }
    private CornerWrapper CreateCorner(Vector2f point)
    {
        CornerWrapper wrapper = new CornerWrapper(point);
        CornersLookup[point] = wrapper;
        return wrapper;
    }
    private void InitializeEdges()
    {
        foreach (var item in CentersLookup.Values)
        {
            for (int i = 0; i < item.corners.Count - 1; i++)
            {
                checkDuplicatedEdges(item, item.corners[i], item.corners[i + 1]);
            }
            checkDuplicatedEdges(item, item.corners[item.corners.Count - 1], item.corners[0]);
        }
    }

    private void checkDuplicatedEdges(CenterWrapper center,CornerWrapper corner1, CornerWrapper corner2)
    {
        Vector2f curEdgeCenter = new Vector2f((corner1.point.x + corner2.point.x) / 2,
                                      (corner1.point.y + corner2.point.y) / 2);
        if (EdgesLookup.ContainsKey(curEdgeCenter))
        {
            EdgesLookup[curEdgeCenter].d1 = center;
        }
        else
        {
            EdgeWrapper newEdge = new EdgeWrapper(EdgesLookup.Count, curEdgeCenter);
            newEdge.d0 = center;
            newEdge.v0 = corner1;
            newEdge.v1 = corner2;
            EdgesLookup[newEdge.midpoint] = newEdge;
            //Intialize center's edges property
            corner1.edges.Add(newEdge);
            corner2.edges.Add(newEdge);
            corner1.adjacents.Add(corner2);
            corner2.adjacents.Add(corner1);
        }
        //Initialize center's borders property
        center.borders.Add(EdgesLookup[curEdgeCenter]);
    }



    #region JobSystem
    JobHandle handle;
    NativeArray<Vector2f> input;
    NativeArray<float> result;
    public struct MyJob : IJob
    {
        public NativeArray<Vector2f> input;
        public void Execute()
        {
            for (int i = 0; i < input.Length; i++)
            {
                var corner1 = input[i];
                for (int j = 0; j < input.Length; j++)
                {
                    if (Mathf.Abs(corner1.x - input[j].x) < 1e-6 && Mathf.Abs(corner1.y - input[j].y) < 0.000001f && i != j)
                    {
                        Debug.Log($"corner1:({corner1.x},{corner1.y}) corner2:({input[j].x},{input[j].y}). i:{i} j:{j}");

                        Debug.Log("duplicated corner detected");
                    }
                }
            }
            input.Dispose();
        }
    }
    #endregion
}
