using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiCellsWrapper
{
    public Voronoi VoronoiDiagram;
    public Dictionary<Vector2f, CornerWrapper> CornerMap;
    public Dictionary<Vector2f,int> VoronoiCellMap;
    public Dictionary<Vector2f, GameObject> VoronoiCellObjectMap;
    public VoronoiCellsWrapper()
    {
        VoronoiCellMap = new Dictionary<Vector2f, int>();
        VoronoiCellObjectMap = new Dictionary<Vector2f, GameObject>();
        CornerMap = new Dictionary<Vector2f, CornerWrapper>();

    }
    public void SetVoronoi(Voronoi voronoi)
    {
        VoronoiDiagram = voronoi;
    }
    public void AddCell(Vector2f region, int type)
    {
        VoronoiCellMap[region] = type;

    }
    bool m_initialized = false;
    public void RefreshCorner(Site site, Rectf bound)
    {
        if (!m_initialized)
        {
            foreach (var item in site.Edges)
            {
                var leftVertex = item.LeftVertex;
                var rightVertex = item.RightVertex;
                if (leftVertex != null && rightVertex != null)
                {
                    var newVertices = ClipLine.ClipSegment(bound, leftVertex.Coord, rightVertex.Coord);
                    leftVertex.Coord = newVertices.Item1;
                    rightVertex.Coord = newVertices.Item2;
                }
                if (leftVertex != null)
                {
                    CornerMap[leftVertex.Coord] = new CornerWrapper(leftVertex.Coord, 0);
                }
                if (rightVertex != null)
                {
                    CornerMap[rightVertex.Coord] = new CornerWrapper(rightVertex.Coord, 0);
                }
            }
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
    public void RefreshAllCorner(Rectf bound)
    {
        foreach (var site in VoronoiDiagram.SitesIndexedByLocation.Values)
        {
            RefreshCorner(site,bound);
        }
        m_initialized = true;
    }
    public void AssignType(Vector2f region, int type)
    {
        VoronoiCellMap[region] = type;
    }
}
public class VoronoiCellWrapper
{
    public Site Site;
    public List<Vector2f> Corners;
    public VoronoiCellWrapper(Site site, List<Vector2f> corners)
    {
        this.Site = site;
    }
}
public class CornerWrapper
{

    public Vector2f Pos;
    public int Type;
    public List<Site> touches;
    public CornerWrapper(Vector2f Pos, int type)
    {
        this.Pos = Pos;
        this.Type = type;
    }
}