using csDelaunay;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static VoronoiHelper;

public enum CellType
{
    //Base Property
    Land = 1,
    Water = 2,
    Ocean = 4,
    Coast = 8,
    Border = 16,
    River = 32,

    //Advanced Biomes
    SubTropicalDesert = 64,
    TemperateDesert = 128,
    Scorched = 256,
    Bare = 512,
    Tundra = 1024,
    GrassLand = 2048,
    ShrubLand = 4096,
    TropicalSeasonalForest = 8192,
    TemperateDeciduousForest = 16384,
    TropicalRainforest = 32768,
    TemperateRainforest = 65536,
    Taiga = 131072,
    Snow = 262144,
    Lake = 524288,

}
public enum BiomeType
{
    //Advanced Biomes
    Ocean = 32,
    SubTropicalDesert = 64,
    TemperateDesert = 128,
    Scorched = 256,
    Bare = 512,
    Tundra = 1024,
    GrassLand = 2048,
    ShrubLand = 4096,
    TropicalSeasonalForest = 8192,
    TemperateDeciduousForest = 16384,
    TropicalRainforest = 32768,
    TemperateRainforest = 65536,
    Taiga = 131072,
    Snow = 262144,
    Lake = 524288,
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
    [SerializeField]
    private CellMaterialData cellMaterialData;

    public VoronoiDiagramWrapper voronoiWrapper;
    public void Awake()
    {
        voronoiWrapper = new VoronoiDiagramWrapper(resolutionX, resolutionY, width, height, LloydrelaxationInteration);
        DrawVoronoiDiagram();
    }
    void Start()
    {

    }
    private void DrawVoronoiDiagram()
    {
        foreach (var center in voronoiWrapper.CentersLookup.Values)
        {
            //var center = voronoiWrapper.CentersLookup[item];
            GameObject go = Instantiate(prefab, transform);
            go.GetComponent<VoronoiCell>().Initialize(center.Point, this, land);
            center.gameobject = go;

            //go.GetComponent<MeshRenderer>().material = land;
            if ((center.type&CellType.Ocean) != 0)
            {
                go.GetComponent<MeshRenderer>().material = ocean;
            }   
            
            List<Vector3> list = new List<Vector3>();
            for (int j = 0; j < center.corners.Count; j++)
            {
                list.Add(new Vector3(center.corners[j].point.x, center.corners[j].point.y, 0));
            }
            go.GetComponent<MeshFilter>().mesh =  CreateMesh(list.ToArray());
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
        voronoiWrapper.CentersLookup[pos].type = type;
        UpdateCorners(voronoiWrapper.CentersLookup[pos], type);
        
        RefreshCellState();

        AssignElevation();
        AssignMoisture();
        AssignBiomes();
        //AssignPostProcess();

        DrawEffect();
    }
    private void AssignElevation()
    {
        Debug.Log("Assigning Elevation");
        RefreshCornerElevation();
        RefreshLakeElevation();
        RefreshOceanElevation();
        RefreshCenterElevation();
    }
    private void AssignMoisture()
    {
        Debug.Log("Assigning Moisture");
        RefreshRiverVolume();
        RefreshCornerMoisture();
        RefreshCellMoisture();
    }

    private void AssignBiomes()
    {
        Debug.Log("Assigning Biomes");
        RefreshLandCellBiomes();
    }

    private void AssignPostProcess()
    {
        Debug.Log("Assigning PostProcess");
        RefreshNoisyEdge();
        //RefreshCellMesh();
    }
    private void UpdateCorners(CenterWrapper center, CellType type)
    {
        foreach (var corner in center.corners)
        {
            corner.type = type;
        }
    }

    private void RefreshCellState()
    {
        foreach (var center in voronoiWrapper.CentersLookup.Keys)
        {
            checkCellCoast(center);
            
        }

    }
    private void checkCellCoast(Vector2f center)
    {
        if (IsLand(voronoiWrapper.CentersLookup[center].type))
        {
            bool isCoast = false;
            foreach (var item in voronoiWrapper.CentersLookup[center].neighbours)
            {
                if ((voronoiWrapper.CentersLookup[item.Point].type & CellType.Ocean) != 0)
                {
                    isCoast = true;
                    break;
                }
            }
            if (isCoast)
            {
                voronoiWrapper.CentersLookup[center].type = CellType.Land | CellType.Coast;
                //If the cell is coast, then change its corners that close to ocean to coast,
                //so that can use DFS to traverse these corners to center of the map and update the elevation
                foreach (var item in voronoiWrapper.CentersLookup[center].corners)
                {
                    foreach (var touch in item.touches)
                    {
                        if (IsOcean(touch.type))
                        {
                            item.type = CellType.Land | CellType.Coast;
                            break;
                        }
                    }
                }
            }
            else
            {
                voronoiWrapper.CentersLookup[center].type = CellType.Land;
                foreach (var item in voronoiWrapper.CentersLookup[center].corners)
                {
                    //If old corner CellType is Ocean, then set it to Land
                    if (IsOcean(item.type))
                    {
                        item.type = CellType.Land;
                    }
                }
            }
        }
    }
    private void RefreshLakeElevation()
    {
        foreach (var wrapper in voronoiWrapper.CentersLookup.Values)
        {
            if (VoronoiHelper.IsLake(wrapper.type))
            {
                float minElevation = int.MaxValue;
                foreach (var item in wrapper.corners)
                {
                    
                    if (item.elevation < minElevation)
                    {
                        minElevation = item.elevation;
                    }
                }
                foreach (var item in wrapper.corners)
                {
                    item.elevation = minElevation;
                }
            }
        }
    }
    //下面的代码实现了将涂为Ocean的Cell的海拔重新设为0，同时，为了让结果更加真实，
    //一个Cell如果从Land被涂成了Ocean，那么新的Cell的corner如果同时属于类型为Land的Cell，
    //那么corner的海拔不会变化，制造出悬崖的效果。
    private void RefreshOceanElevation()
    {
        foreach (var wrapper in voronoiWrapper.CentersLookup.Values)
        {
            if (VoronoiHelper.IsOcean(wrapper.type))
            {
                foreach (var item in wrapper.corners)
                {
                    if (!checkIsLandCorner(item))
                    {
                        item.elevation = 0;
                    }   
                }
            }
        }
    }
    private bool checkIsLandCorner(CornerWrapper corner)
    {
        foreach (var item in corner.touches)
        {
            if ((item.type & CellType.Land) > 0)
            {
                return true;
            }
        }
        return false;
    }
    private void RefreshCornerElevation()
    {
        float curElevation = 1;
        HashSet<CornerWrapper> traversedCorner = new HashSet<CornerWrapper>();
        Queue<CornerWrapper> queue = new Queue<CornerWrapper>();
        foreach (var item in voronoiWrapper.CornersLookup.Values)
        {
            if ((item.type & CellType.Coast) != 0 )
            {
                queue.Enqueue(item);
            }
        }
        while (queue.Count > 0)
        {
            UpdateCornerElevationInQueue(queue, traversedCorner, ref curElevation);
        }
        RedistributeCornerElevation();
    }
    private void UpdateCornerElevationInQueue(Queue<CornerWrapper> queue, HashSet<CornerWrapper> traversedCorners, ref float curElevation)
    {
        List<CornerWrapper> curElevationCorners = new List<CornerWrapper>();
        while (queue.Count > 0)
        {
            CornerWrapper corner = queue.Dequeue();
            corner.elevation = curElevation;
            curElevationCorners.Add(corner);
            traversedCorners.Add(corner);
        }
        foreach (var item in curElevationCorners)
        {
            foreach (var neighbour in item.adjacents)
            {
                if ((neighbour.type & CellType.Land) > 0 && (!traversedCorners.Contains(neighbour)))
                {
                    queue.Enqueue(neighbour);
                }
            }
        }
        curElevation++;
    }


    List<CornerWrapper> landcorners = new List<CornerWrapper>();
    //Redistribute corners' elevation, to make more low elevation cells
    private void RedistributeCornerElevation()
    {
        float SCALE_FACTOR = 1f;
        landcorners.Clear();
        foreach (var item in voronoiWrapper.CornersLookup.Values)
        {
            //add all cells except ocean to landcorners, otherwise ocean will be included in the calculating of elevation
            if (!IsOcean(item.type))
            {
                landcorners.Add(item);
            }
        }
        landcorners.Sort((corner1,corner2) =>
        {
            return corner1.elevation.CompareTo(corner2.elevation);
        });

        for (int i = 0; i < landcorners.Count; i++)
        {
            float y = (float)i / (float)(landcorners.Count - 1);
            float x = Mathf.Sqrt(SCALE_FACTOR) - Mathf.Sqrt(SCALE_FACTOR * (1 - y));
            if (x > 1f)
            {
                x = 1f;
            }
            landcorners[i].elevation = x;
        }
    }

    //Refresh center's elevation according to its corners' average elevation

    private void RefreshCenterElevation()
    {
        //由于海边悬崖的机制，导致OceanCell的corner不一定海拔为0，因此需要手动将所有Ocean的海拔设为0
        foreach (var item in voronoiWrapper.CentersLookup.Values)
        {
            if ((item.type & CellType.Ocean) > 0)
            {
                item.elevation = 0;
            }
            else
            {
                float sum = 0;
                foreach (var corner in item.corners)
                {
                    sum += corner.elevation;
                }
                item.elevation = sum / item.corners.Count;
            }
        }
    }

    private void RefreshRiverVolume()
    {
        foreach (var item in voronoiWrapper.EdgesLookup.Values)
        {
            item.riverVolume = 0;
        }
        //Must reset river property before creating river seperately, otherwise the corner with river added will be reset,
        //even it is newly added in current loop
        foreach (var item in voronoiWrapper.CornersLookup.Values)
        {
            //reset every corner's River property
            if ((item.type & CellType.River) > 0)
            {
                item.type ^= CellType.River;
            }
        }
        foreach (var item in voronoiWrapper.CornersLookup.Values)
        {
            //Because of redistribution of elevation, need to modify the offset condition of generating a river
            if (item.elevation > 0.8f)
            {
                if (UnityEngine.Random.Range(0f, 1f) > 0.95f)
                {
                    CreateRiver(item);
                }
            }
        }
    }
    private void CreateRiver(CornerWrapper corner)
    {
        HashSet<Vector2f> traversedCorner = new HashSet<Vector2f>();
        CornerWrapper cur = corner;
        cur.type |= CellType.River;
        traversedCorner.Add(cur.point);
        while (cur.elevation > 0)
        {
            CornerWrapper min = null;
            foreach (var item in cur.adjacents)
            {
                if (traversedCorner.Contains(item.point))
                {
                    continue;
                }
                traversedCorner.Add(item.point);
                if (min == null || item.elevation <= min.elevation/* || 
                    (item.elevation<min.elevation &&
                    Vector2f.DistanceSquare(item.point,cur.point) > Vector2f.DistanceSquare(min.point, cur.point))*/)
                {
                    min = item;
                }
            }
            //If all the ajacent corners are traversed and not reach the ocean, means it can't find the way to ocean,
            //then stop creating river
            if (min == null)
            {
                break;
            }
            min.type |= CellType.River;
            voronoiWrapper.EdgesLookup[new Vector2f((cur.point.x + min.point.x) / 2, (cur.point.y + min.point.y) / 2)].riverVolume++;
            cur = min;
        }
    }

    private void RefreshCornerMoisture()
    {
        Queue<CornerWrapper> corners = new Queue<CornerWrapper>();
        foreach (var item in voronoiWrapper.CornersLookup.Values)
        {
            item.moisture = 0;
            if (IsRiver(item.type) || IsLake(item.type))
            {
                item.moisture = 1f;
                corners.Enqueue(item);
            }
        }
        while (corners.Count > 0)
        {
            CornerWrapper corner = corners.Dequeue();
            foreach (var item in corner.adjacents)
            {
                float newMoisture = 0.9f * corner.moisture;
                if (newMoisture > item.moisture/* && newMoisture>0.5f*/)
                {
                    item.moisture = newMoisture;
                    corners.Enqueue(item);
                }
            }
        }
    }
    private void RefreshCellMoisture()
    {
        foreach (var item in voronoiWrapper.CentersLookup.Values)
        {
            float moisture = 0;
            foreach (var corner in item.corners)
            {
                moisture += corner.moisture;
            }
            item.moisture = moisture / item.corners.Count;
        }
    }

    //Create a two dimention array to store the CellType for the specific elevation and moisture
    List<List<BiomeType>> CellTypeHelper = new List<List<BiomeType>>()
    {
        new List<BiomeType>(){
            BiomeType.SubTropicalDesert,
            BiomeType.GrassLand,
            BiomeType.TropicalSeasonalForest,
            BiomeType.TropicalSeasonalForest,
            BiomeType.TropicalRainforest,
            BiomeType.TropicalRainforest 
        },
        new List<BiomeType>(){
            BiomeType.TemperateDesert,
            BiomeType.GrassLand,
            BiomeType.GrassLand,
            BiomeType.TemperateDeciduousForest,
            BiomeType.TemperateDeciduousForest,
            BiomeType.TemperateRainforest
        },
        new List<BiomeType>(){
            BiomeType.TemperateDesert,
            BiomeType.TemperateDesert,
            BiomeType.ShrubLand,
            BiomeType.ShrubLand,
            BiomeType.Taiga,
            BiomeType.Taiga
        },
        new List<BiomeType>(){
            BiomeType.Scorched,
            BiomeType.Bare,
            BiomeType.Tundra,
            BiomeType.Snow,
            BiomeType.Snow,
            BiomeType.Snow
        }
    };

    //saturate Centers' elevation and moisture, then get the Biomes through CellTypeHelper,
    //and get the material from CellMaterialData
    private void RefreshLandCellBiomes()
    {
        foreach (var item in voronoiWrapper.CentersLookup.Values)
        {
            item.biomeType = 0;
            if (!IsOcean(item.type))
            {
                int elevation = (int)((item.elevation) * CellTypeHelper.Count/* / 10*/);
                if (elevation > CellTypeHelper.Count - 1)
                {
                    elevation = CellTypeHelper.Count - 1;
                }
                int moisture = (int)(item.moisture * CellTypeHelper[0].Count);
                if (moisture > CellTypeHelper[0].Count - 1)
                {
                    moisture = CellTypeHelper[0].Count - 1;
                }
                //Make Coast cells as SubTropicalDesert in default
                BiomeType biomes;
                if (IsCoast(item.type))
                {
                    item.biomeType |= BiomeType.SubTropicalDesert;
                }
                //Make Lake cells as Lake in default
                else if (IsLake(item.type))
                {
                    item.biomeType |= BiomeType.Lake;
                }
                else
                {
                    item.biomeType |= CellTypeHelper[elevation][moisture];
                }
                biomes = item.biomeType;
                item.gameobject.GetComponent<MeshRenderer>().material = cellMaterialData.FindMaterial(biomes);
            }
            else
            {
                item.gameobject.GetComponent<MeshRenderer>().material = cellMaterialData.FindMaterial(BiomeType.Ocean);
            }
        }
    }

    private void RefreshNoisyEdge()
    {
        //Create noisyEdges
        GenerateNoisyEdges.BuildNoisyEdge(voronoiWrapper.EdgesLookup, 0.5f);
    }

    private void RefreshCellMesh()
    {
        foreach (var item in voronoiWrapper.CentersLookup.Values)
        {
            if(!IsOcean(item.type))
            {
                List<Vector3> points = new List<Vector3>();
                foreach (var point in CreateNoisyEdgeMesh(item))
                {
                    points.Add(point);
                }
                item.gameobject.GetComponent<MeshFilter>().mesh = CreateMesh(points.ToArray());
            }
        }
    }
    private List<Vector2> CreateNoisyEdgeMesh(CenterWrapper center)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < center.corners.Count-1; i++)
        {
            Vector2f edge = new Vector2f((center.corners[i].point.x + center.corners[i + 1].point.x) / 2,
                (center.corners[i].point.y + center.corners[i + 1].point.y) / 2);
            points.AddRange(voronoiWrapper.EdgesLookup[edge].noisyPoints);
        }

        //Close the circle
        Vector2f lastedge = new Vector2f((center.corners[center.corners.Count-1].point.x + center.corners[0].point.x) / 2,
    (center.corners[center.corners.Count - 1].point.y + center.corners[0].point.y) / 2);
        points.AddRange(voronoiWrapper.EdgesLookup[lastedge].noisyPoints);

        return points;
    }
    private void DrawEffect()
    {

    }
    public void OnGUI()
    {
        foreach (var item in voronoiWrapper.CentersLookup.Values)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(new Vector3(item.Point.x, -item.Point.y + height, 0));
            GUI.Label(new Rect(pos.x, pos.y, Screen.width, Screen.height), item.elevation.ToString());
        }
        /*        foreach (var item in voronoiWrapper.CornersLookup.Values)
                {
                    Vector3 pos = Camera.main.WorldToScreenPoint(new Vector3(item.point.x, -item.point.y + height, 0));
                    GUI.Label(new Rect(pos.x, pos.y, Screen.width, Screen.height), item.elevation.ToString());
                }*/
        /*        foreach (var item in voronoiWrapper.CentersLookup.Values)
                {
                    Vector3 pos = Camera.main.WorldToScreenPoint(new Vector3(item.Point.x, -item.Point.y + height, 0));
                    GUI.Label(new Rect(pos.x, pos.y, Screen.width, Screen.height), item.moisture.ToString());
                }*/
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (voronoiWrapper == null) return;
        foreach (var center in voronoiWrapper.CentersLookup.Values)
        {
            Gizmos.DrawSphere(new Vector3(center.Point.x, center.Point.y, 0), 0.05f);
            for (int i = 0; i < center.corners.Count - 1; i++)
            {
                Gizmos.DrawLine(new Vector3(center.corners[i].point.x, center.corners[i].point.y, 0),
                                new Vector3(center.corners[i + 1].point.x, center.corners[i + 1].point.y, 0));
            }
            Gizmos.DrawLine(new Vector3(center.corners[0].point.x, center.corners[0].point.y, 0),
                new Vector3(center.corners[center.corners.Count - 1].point.x, center.corners[center.corners.Count - 1].point.y, 0));
        }

        /*        foreach (var item in voronoiWrapper.EdgesLookup.Values)
                {
                    for (int i = 0; i < item.noisyPoints.Count - 1; i++)
                    {
                        Gizmos.DrawLine(item.noisyPoints[i], item.noisyPoints[i + 1]);
                    }
                }*/

        Gizmos.color = Color.red;
        if (landedges.Count > 0)
        {
            foreach (var item in landedges)
            {
                Gizmos.DrawLine(new Vector3(item.v0.point.x, item.v0.point.y, 0), new Vector3(item.v1.point.x, item.v1.point.y, 0));
            }
        }

        Gizmos.color = Color.blue;
        foreach (var item in voronoiWrapper.EdgesLookup.Values)
        {
            
            if (item.riverVolume > 0)
            {
                Gizmos.DrawLine(new Vector3(item.v0.point.x, item.v0.point.y, 0),
    new Vector3(item.v1.point.x, item.v1.point.y, 0));
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
