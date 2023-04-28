using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using Cysharp.Threading.Tasks;
using System;

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

    void Start()
    {

        
        InitializeVoronoiDiagram();
        DrawVoronoiDiagram();
    }
    private  void DrawVoronoiDiagram()
    {
        foreach (var center in wrapper.VoronoiCellMap.Keys)
        {
            var region = voronoi.Region(center);
            GameObject go = Instantiate(prefab, transform);
            go.GetComponent<VoronoiCell>().Initialize(center, this, land);

            wrapper.VoronoiCellObjectMap[center] = go;

            if (wrapper.VoronoiCellMap[center] == 2)
            {
                go.GetComponent<MeshRenderer>().material = ocean;
            }
            else if (wrapper.VoronoiCellMap[center] == 0)
            {
                go.GetComponent<MeshRenderer>().material = land;
            }
            List<Vector3> list = new List<Vector3>();
            for (int j = 0; j < region.Count; j++)
            {
                list.Add(new Vector3(region[j].x, region[j].y, 0));
            }
            go.GetComponent<MeshFilter>().mesh = CreateMesh(list.ToArray());
            //await UniTask.Delay(TimeSpan.FromSeconds(0.01f));
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
        voronoi = new Voronoi(points, new Rectf(0, 0, width, height),LloydrelaxationInteration, wrapper.VoronoiCellMap);
        wrapper.SetVoronoi(voronoi);
        wrapper.RefreshAllCorner();
    }
    public void UpdateVoronoiCell(Vector2f pos, int type)
    {
        wrapper.VoronoiCellMap[pos] = type;
        wrapper.VoronoiCellObjectMap[pos].GetComponent<MeshRenderer>().material = type==0?land:ocean;
        //wrapper.RefreshCorner(pos);
        RefreshCellState();
    }

    private void RefreshCellState()
    {
        foreach (var center in wrapper.VoronoiCellMap.Keys)
        {
            if(wrapper.VoronoiCellMap[center] == 0)
            {
                bool isCoast = false;
                foreach (var item in voronoi.SitesIndexedByLocation[center].NeighborSites())
                {
                    if (wrapper.VoronoiCellMap[item.Coord] == 2)
                    {
                        Debug.Log("has ocean set it to coast");
                        isCoast = true;
                        break;
                    }
                }
                if (isCoast)
                {
                    wrapper.VoronoiCellObjectMap[center].GetComponent<MeshRenderer>().material = coast;
                }
                else
                {
                    wrapper.VoronoiCellObjectMap[center].GetComponent<MeshRenderer>().material = land;
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (Application.isPlaying&&voronoi.Edges!=null)
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
