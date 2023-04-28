using csDelaunay;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Land = 0,
    Water = 1,
    Ocean = 2,
    Coast = 3,
}
public class GenerateVoronoi : MonoBehaviour
{
    [SerializeField]
    private int resolutionX;
    [SerializeField]
    private int resolutionY;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private int LloydrelaxationInteration;

    [SerializeField]
    private Material land;
    [SerializeField]
    private Material water;
    [SerializeField]
    private Material ocean;
    [SerializeField]
    private Material coast;

    [SerializeField]
    private GameObject prefab;


    Voronoi voronoi;
    VoronoiCellsWrapper wrapper;
    List<Vector2f> points;
    List<CenterWrapper> centers;

    void Start()
    {
        //centers = new List<CenterWrapper>();
        //InitializeVoronoiDiagram();
        //DrawVoronoiDiagram();
    }
    private  void DrawVoronoiDiagram()
    {
        foreach (var center in wrapper.CenterLookup.Values)
        {
            //var region = center.Corners;
            //GameObject go = Instantiate(prefab, transform);
            //go.GetComponent<VoronoiCell>().Initialize(center.Site.Coord, this, land);

            //center.gameobject = go;

            //if (center.Type == 2)
            //{
            //    go.GetComponent<MeshRenderer>().material = ocean;
            //}
            //else if (center.Type == 0)
            //{
            //    go.GetComponent<MeshRenderer>().material = land;
            //}
            //List<Vector3> list = new List<Vector3>();
            //for (int j = 0; j < region.Count; j++)
            //{
            //    list.Add(new Vector3(region[j].x, region[j].y, 0));
            //}
            //go.GetComponent<MeshFilter>().mesh = CreateMesh(list.ToArray());
        }

    }
    
    private void InitializeVoronoiDiagram()
    {
        wrapper = new VoronoiCellsWrapper();

        points = new List<Vector2f>();
        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                Vector2f point = new Vector2f(UnityEngine.Random.Range((float)i * (width / resolutionX), (float)(i + 1) * (width / resolutionX)),
                                        UnityEngine.Random.Range((float)j * (height / resolutionY), (float)(j + 1) * (height / resolutionY)));
                points.Add(point);

                wrapper.AddCell(point, 2);
            }
        }
        voronoi = new Voronoi(points, new Rectf(0, 0, width, height),LloydrelaxationInteration, wrapper.VoronoiCellMapHelper);
        wrapper.SetVoronoi(voronoi);
        InitializeCenters();
        InitializeCorners();

    }
    private void InitializeCenters()
    {
        foreach (var item in voronoi.SitesIndexedByLocation.Values)
        {
            var center = new CenterWrapper(centers.Count, item.Coord);
            centers.Add(center);
            wrapper.CenterLookup[item.Coord] = center;
        }


        //add neighbour centers
        foreach (var center in centers)
        {
            foreach (var site in voronoi.SitesIndexedByLocation[center.Point].NeighborSites())
            {
                center.neighbours.Add(wrapper.CenterLookup[site.Coord]);
            }
        }
    }
    private void InitializeCorners()
    {
        //wrapper.RefreshAllCorner(new Rectf(0, 0, width, height), 2);
        //foreach (var item in voronoi.Edges)
        //{
        //    CornerWrapper LeftCorner = null;
        //    CornerWrapper RightCorner = null;
        //    if(item.LeftVertex!=null)
        //        wrapper.CornerMap.TryGetValue(item.LeftVertex.Coord,out LeftCorner);
        //    if(item.RightVertex!=null)
        //        wrapper.CornerMap.TryGetValue(item.RightVertex.Coord, out RightCorner);

        //    if (LeftCorner!=null)
        //    {
        //        LeftCorner.edges.Add(item);
        //        if (RightCorner!=null)
        //        {
        //            LeftCorner.neighbours.Add(RightCorner);
        //        }
        //    }
        //    if (RightCorner!=null)
        //    {
        //        RightCorner.edges.Add(item);
        //        if (LeftCorner!=null)
        //        {
        //            RightCorner.neighbours.Add(LeftCorner);
        //        }
        //    }
        //}
        //Debug.Log(wrapper.CornerMap);
    }

    public void UpdateVoronoiCell(Vector2f pos, int type)
    {
        //wrapper.CenterLookup[pos].SetType(type);
        //wrapper.CenterLookup[pos].gameobject.GetComponent<MeshRenderer>().material = type==0?land:ocean;
        RefreshCellState();
    }

    private void RefreshCellState()
    {
        //foreach (var center in wrapper.VoronoiCellMapHelper.Keys)
        //{
        //    if(wrapper.CenterLookup[center].Type == 0)
        //    {
        //        bool isCoast = false;
        //        foreach (var item in voronoi.SitesIndexedByLocation[center].NeighborSites())
        //        {
        //            if (wrapper.CenterLookup[item.Coord].Type == 2)
        //            {
        //                Debug.Log("has ocean set it to coast");
        //                isCoast = true;
        //                break;
        //            }
        //        }
        //        if (isCoast)
        //        {
        //            wrapper.CenterLookup[center].SetType((int)CellType.Coast);
        //            wrapper.CenterLookup[center].gameobject.GetComponent<MeshRenderer>().material = coast;
        //        }
        //        else
        //        {
        //            wrapper.CenterLookup[center].SetType((int)CellType.Land);
        //            wrapper.CenterLookup[center].gameobject.GetComponent<MeshRenderer>().material = land;
        //        }
        //    }
        //}
    }
    
    private void OnDrawGizmos()
    {

        if (Application.isPlaying&&voronoi!=null)
        {
            foreach (var item in voronoi.Edges)
            {
                if (item.LeftVertex != null && item.RightVertex != null)
                {
                    var newvertices = ClipLine.ClipSegment(new Rectf(0, 0, width, height), item.LeftVertex.Coord, item.RightVertex.Coord);
                    if (newvertices != null)
                    {
                        item.LeftVertex.Coord = newvertices.Item1;
                        item.RightVertex.Coord = newvertices.Item2;
                    }
                }

                if (item.LeftVertex!=null)
                {
                    Gizmos.DrawSphere(new Vector3(item.LeftVertex.x, item.LeftVertex.y, 0), 0.05f);
                }
                if (item.RightVertex!=null)
                {
                    Gizmos.DrawSphere(new Vector3(item.RightVertex.x, item.RightVertex.y, 0), 0.05f);
                }
            }
        }

        //if (wrapper != null)
        //{
        //    foreach (var item in wrapper.CornerMap.Values)
        //    {
        //        if(item.Type == 0)
        //        {
        //            Gizmos.color = Color.red;
        //            Gizmos.DrawSphere(new Vector3(item.Pos.x, item.Pos.y, 0), 0.1f);
        //        }
        //        else
        //        {
        //            Gizmos.color = Color.blue;
        //            Gizmos.DrawSphere(new Vector3(item.Pos.x, item.Pos.y, 0), 0.05f);
        //        }
                
        //    }
        //}
    }
    private void Update()
    {

    }
    Mesh CreateMesh(Vector3[] vertices)
    {
        Mesh mesh = new Mesh();
        var uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            if(i%2==0)
                uvs[i] = new Vector2(0, 0);
            else
                uvs[i] = new Vector2(1, 1);
        }

        var tris = new int[3 * (vertices.Length - 2)];
        int C1 = 0;
        int C2 = 1;
        int C3 = 2;
        for (int i = 0; i < tris.Length; i+=3)
        {
            tris[i] = C1;
            tris[i + 1] = C2;
            tris[i + 2] = C3;

            C2++;
            C3++;
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = tris;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.Optimize();

        mesh.name = "mymesh";
        return mesh;
    }

}
