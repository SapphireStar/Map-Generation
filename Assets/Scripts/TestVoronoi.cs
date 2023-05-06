using System.Collections.Generic;
using csDelaunay;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TestVoronoi
{
    
    public int resolutionX;
    public int resolutionY;
    public int width;
    public int height;
    public int LloydrelaxationInteration;

    Voronoi voronoi;
    public List<Vector2f> centers;
    public List<Vector2f> corners = new List<Vector2f>();

    public Dictionary<Vector2f, CornerWrapper> CornersLookup = new Dictionary<Vector2f, CornerWrapper>();
    public Dictionary<Vector2f, CenterWrapper> CentersLookup = new Dictionary<Vector2f, CenterWrapper>(); 
   
    public TestVoronoi(int resolutionX, int resolutionY, int width, int height, int lloydIteration)
    {
        this.resolutionX = resolutionX;
        this.resolutionY = resolutionY;
        this.width = width;
        this.height = height;
        this.LloydrelaxationInteration = lloydIteration;

        InitializeVoronoi();
        InitializeCenters();
        InitializeCorners();
    }
    private void InitializeVoronoi()
    {
        centers = new List<Vector2f>();
        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                Vector2f point = new Vector2f(UnityEngine.Random.Range((float)i * (width / resolutionX), (float)(i + 1) * (width / resolutionX)),
                                        UnityEngine.Random.Range((float)j * (height / resolutionY), (float)(j + 1) * (height / resolutionY)));
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

        Debug.Log(CentersLookup) ;
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
                    Debug.Log("create new corner");
                    //create cornerwrapper, if corner is not duplicated, and add corner to current centerwrapper's corners,
                    //and add current centerwrapper to new cornerwrapper's touches
                    CornerWrapper newCorner = CreateCorner(centerCorners[i]);
                    CornersLookup[centerCorners[i]] = newCorner;
                    newCorner.index = corners.Count;
                    this.corners.Add(centerCorners[i]);
                    newCorner.touches.Add(center);
                    center.corners.Add(newCorner);
                }
            }

        }
    }

    private void InitializeEdges()
    {

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
                if (Mathf.Abs(nCorners.point.x - corner.x) < 1e-6
                && Mathf.Abs(nCorners.point.y - corner.y) < 1e-6)
                {
                    nCorners.touches.Add(center);
                    center.corners.Add(nCorners);
                    return true;
                }
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
    /*    private void OnDrawGizmos()
        {
            if (corners != null&&Application.isPlaying)
            {
                foreach (var item in corners)
                {
                    Gizmos.DrawSphere(new Vector3(item.x, item.y, 0), 0.05f);
                }

                Gizmos.color = Color.red;
                foreach (var item in centersLookup.Values)
                {
                    for (int i = 0; i < item.corners.Count - 1; i++)
                    {
                        Gizmos.DrawLine(new Vector3(item.corners[i].point.x, item.corners[i].point.y, 0),
                                        new Vector3(item.corners[i + 1].point.x, item.corners[i + 1].point.y, 0));
                    }
                    if (item.corners.Count > 1)
                    {
                        Gizmos.DrawLine(new Vector3(item.corners[0].point.x, item.corners[0].point.y, 0),
    new Vector3(item.corners[item.corners.Count - 1].point.x, item.corners[item.corners.Count - 1].point.y, 0));
                    }

                }

            }
        }*/
}
