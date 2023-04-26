using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;

public class GenerateVoronoi : MonoBehaviour
{
    [SerializeField]
    private int resolutionX;
    public int ResolutionX 
    {
        set
        {
            resolutionX = value;
            InitializeVoronoiDiagram();
        } 
    }
    [SerializeField]
    private int resolutionY;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private int LloydrelaxationInteration;

    [SerializeField]
    private Material material;
    [SerializeField]
    private GameObject prefab;


    Voronoi voronoi;
    List<Vector2f> points;

    //Delaunay edge
    List<Edge> edges;

    //Voronoi Diagram's Voronoi edge
    List<LineSegment> voronoiDiagram;

    //Voronoi Cell's node point
    Dictionary<Vector2f,Site> sites;
    // Start is called before the first frame update
    void Start()
    {

        InitializeVoronoiDiagram();
        DrawVoronoiDiagram();
    }
    private void DrawVoronoiDiagram()
    {
        List<List<Vector2f>> regions = voronoi.Regions();
        for (int i = 0; i < regions.Count; i++)
        {
            GameObject go = Instantiate(prefab, transform);
            go.GetComponent<MeshRenderer>().material = material;
            List<Vector3> list = new List<Vector3>();
            for (int j = 0; j < regions[i].Count; j++)
            {
                list.Add(new Vector3(regions[i][j].x, regions[i][j].y, 0));
            }
            go.GetComponent<MeshFilter>().mesh = CreateMesh(list.ToArray());
        }
    }
    
    private void OnDrawGizmos()
    {
        if (edges != null)
        {
            //foreach (var item in voronoiDiagram)
            //{
            //    Gizmos.DrawLine(new Vector3(item.p0.x, item.p0.y, 0)
            //                    , new Vector3(item.p1.x, item.p1.y, 0));
            //}
            //foreach (var item in edges)
            //{
            //    if(item.LeftSite != null)
            //        Gizmos.DrawSphere(new Vector3(item.LeftSite.x, item.LeftSite.y, 0), 0.05f);
            //    if(item.RightSite != null)
            //        Gizmos.DrawSphere(new Vector3(item.RightSite.x, item.RightSite.y, 0), 0.05f);

            //}
            //foreach (var item in sites.Values)
            //{
            //    Gizmos.DrawSphere(new Vector3(item.x, item.y, 0), 0.05f);
            //    List<Vector2f> region = item.Region(new Rectf(0, 0, width, height));
            //    for (int i = 0; i < region.Count-1; i++)
            //    {
            //        Gizmos.DrawLine(new Vector3(region[i].x, region[i].y, 0)
            //                        , new Vector3(region[i + 1].x, region[i + 1].y, 0));
            //    }
            //}
        }

    }
    private void InitializeVoronoiDiagram()
    {
        points = new List<Vector2f>();
        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                points.Add(new Vector2f(Random.Range(0f, (float)width), Random.Range(0f, (float)height)));
            }
        }

        voronoi = new Voronoi(points, new Rectf(0, 0, width, height),LloydrelaxationInteration);
        edges = voronoi.Edges;
        voronoiDiagram = voronoi.VoronoiDiagram();
        sites = voronoi.SitesIndexedByLocation;
    }
    void Update()
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
