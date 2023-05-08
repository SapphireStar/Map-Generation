using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCellsWrapper
{
    public Voronoi VoronoiDiagram;
    public Dictionary<Vector2f, CornerWrapper> CornerMap;
    public Dictionary<Vector2f,int> VoronoiCellMapHelper;
    public Dictionary<Vector2f, CenterWrapper> CenterLookup;
    public Dictionary<Vector2f, GameObject> VoronoiCellObjectMap;
    public VoronoiCellsWrapper()
    {
        VoronoiCellMapHelper = new Dictionary<Vector2f, int>();
        CenterLookup = new Dictionary<Vector2f, CenterWrapper>();
        VoronoiCellObjectMap = new Dictionary<Vector2f, GameObject>();
        CornerMap = new Dictionary<Vector2f, CornerWrapper>();

    }
    public void SetVoronoi(Voronoi voronoi)
    {
        VoronoiDiagram = voronoi;
    }
    public void AddCell(Vector2f region, int type)
    {
        VoronoiCellMapHelper[region] = type;

    }
    bool m_initialized = false;
    public void RefreshCorner(Edge edge, Rectf bound, int type)
    {
        if (!m_initialized)
        {
            
        }
        else
        {
            //foreach (var item in site.Edges)
            //{
            //    var leftVertex = item.LeftVertex;
            //    var rightVertex = item.RightVertex;
            //    if (leftVertex != null)
            //    {
            //        CornerMap[leftVertex.Coord] = new CornerWrapper(leftVertex.Coord, 0);
            //    }
            //    if (rightVertex != null)
            //    {
            //        CornerMap[rightVertex.Coord] = new CornerWrapper(rightVertex.Coord, 0);
            //    }
            //}
        }
    }
    public void RefreshAllCorner(Rectf bound, int type)
    {
        foreach (var edge in VoronoiDiagram.Edges)
        {
            RefreshCorner(edge,bound, type);
        }
        m_initialized = true;
    }
    public void AssignType(Vector2f region, int type)
    {
        VoronoiCellMapHelper[region] = type;
    }
}
public class CenterWrapper
{
    public int Index;

    public Vector2f Point;
    public CellType type;
    public string biome;
    public float elevation;
    public float moisture;
    public GameObject gameobject;

    public List<CenterWrapper> neighbours;
    public List<EdgeWrapper> borders;
    public List<CornerWrapper> corners;
    public CenterWrapper(int index, Vector2f point)
    {
        Index = index;
        this.Point = point;
        neighbours = new List<CenterWrapper>();
        borders = new List<EdgeWrapper>();
        corners = new List<CornerWrapper>();

        type |= (CellType.Ocean|CellType.Water);
    }
}
public class EdgeWrapper
{
    public int index;
    public CenterWrapper d0;//Delaunay edge
    public CenterWrapper d1;
    public CornerWrapper v0;//Voronoi edge
    public CornerWrapper v1;
    public Vector2f midpoint;// halfway between v0,v1
    public int river;// volume of water, or 0

    public EdgeWrapper(int index, Vector2f midpoint)
    {
        this.index = index;
        this.midpoint = midpoint;
    }
}
public class CornerWrapper
{
    public int index;

    public Vector2f point;
    public CellType type;
    public string biome;
    public float elevation;
    public float moisture;

    public List<CenterWrapper> touches;
    public List<EdgeWrapper> edges;
    public List<CornerWrapper> adjacents;

    public int river;
    public CornerWrapper downslope;
    public CornerWrapper watershed;
    public int watershed_size;
    public CornerWrapper(Vector2f point)
    {
        this.point = point;
        touches = new List<CenterWrapper>();
        edges = new List<EdgeWrapper>();
        adjacents = new List<CornerWrapper>();

        type |= (CellType.Ocean | CellType.Water);
    }
}