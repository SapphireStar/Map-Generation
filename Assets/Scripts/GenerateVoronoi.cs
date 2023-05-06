using csDelaunay;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public enum CellType
{
    Land = 1,
    Water = 2,
    Ocean = 4,
    Coast = 8,
    Border = 16,
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
    List<Vector2f> points;
    List<CenterWrapper> centers;

    VoronoiDiagramWrapper voronoiWrapper;
    void Start()
    {
        voronoiWrapper = new VoronoiDiagramWrapper(resolutionX, resolutionY, width, height, LloydrelaxationInteration);
        DrawVoronoiDiagram();
    }
    private void DrawVoronoiDiagram()
    {
        foreach (var center in voronoiWrapper.CentersLookup.Values)
        {
            //var center = voronoiWrapper.CentersLookup[item];
            GameObject go = Instantiate(prefab, transform);
            go.GetComponent<VoronoiCell>().Initialize(center.Point, this, land);
            center.gameobject = go;

            go.GetComponent<MeshRenderer>().material = land;
            if ((center.type&CellType.Ocean) != 0)
            {
                go.GetComponent<MeshRenderer>().material = ocean;
            }   
            
            List<Vector3> list = new List<Vector3>();
            for (int j = 0; j < center.corners.Count; j++)
            {
                list.Add(new Vector3(center.corners[j].point.x, center.corners[j].point.y, 0));
            }
            go.GetComponent<MeshFilter>().mesh = CreateMesh(list.ToArray());
            //await UniTask.Delay(25, false, PlayerLoopTiming.Update, this.GetCancellationTokenOnDestroy());
        }

    }

    #region obsolete
    /*    private void InitializeVoronoiDiagram()
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
        }*/
    #endregion

    List<EdgeWrapper> landedges = new List<EdgeWrapper>();
    public void UpdateVoronoiCell(Vector2f pos, CellType type)
    {
        //wrapper.CenterLookup[pos].SetType(type);
        voronoiWrapper.CentersLookup[pos].type = CellType.Land;
        voronoiWrapper.CentersLookup[pos].gameobject.GetComponent<MeshRenderer>().material = type==CellType.Land?land:ocean;
        if ((type & CellType.Land) != 0)
        {
            var edges = voronoiWrapper.CentersLookup[pos].borders;
            foreach (var item in edges)
            {
                landedges.Add(item);
            }
        }
        RefreshCellState();
    }

    private void RefreshCellState()
    {
        foreach (var center in voronoiWrapper.CentersLookup.Keys)
        {
            if((voronoiWrapper.CentersLookup[center].type&CellType.Land) != 0)
            {
                bool isCoast = false;
                foreach (var item in voronoiWrapper.CentersLookup[center].neighbours)
                {
                    if ((voronoiWrapper.CentersLookup[item.Point].type & CellType.Ocean) != 0)
                    {
                        Debug.Log("has ocean set it to coast");
                        isCoast = true;
                        break;
                    }
                }
                if (isCoast)
                {
                    voronoiWrapper.CentersLookup[center].type = CellType.Land | CellType.Coast;
                    voronoiWrapper.CentersLookup[center].gameobject.GetComponent<MeshRenderer>().material = coast;
                }
                else
                {
                    voronoiWrapper.CentersLookup[center].type = CellType.Land | CellType.Coast;
                    voronoiWrapper.CentersLookup[center].gameobject.GetComponent<MeshRenderer>().material = land;
                }
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (voronoiWrapper == null) return;
        foreach (var center in voronoiWrapper.CentersLookup.Values)
        {
            Gizmos.DrawSphere(new Vector3(center.Point.x, center.Point.y,0), 0.05f);
            for (int i = 0; i < center.corners.Count-1; i++)
            {
                Gizmos.DrawLine(new Vector3(center.corners[i].point.x, center.corners[i].point.y, 0),
                                new Vector3(center.corners[i+1].point.x, center.corners[i+1].point.y, 0));
            }
            Gizmos.DrawLine(new Vector3(center.corners[0].point.x, center.corners[0].point.y, 0),
                new Vector3(center.corners[center.corners.Count - 1].point.x, center.corners[center.corners.Count - 1].point.y, 0));
        }

        Gizmos.color = Color.red;
        if (landedges.Count > 0)
        {
            foreach (var item in landedges)
            {
                Gizmos.DrawLine(new Vector3(item.v0.point.x, item.v0.point.y, 0), new Vector3(item.v1.point.x, item.v1.point.y, 0));
            }
        }

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
