using csDelaunay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterWrapper
{
    public int Index;

    public Vector2f Point;
    public CellType type;
    public BiomeType biomeType;
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
    public int riverVolume;// volume of water, or 0
    public List<Vector2> noisyPoints;

    public EdgeWrapper(int index, Vector2f midpoint)
    {
        this.index = index;
        this.midpoint = midpoint;
        noisyPoints = new List<Vector2>();
    }
}
public class CornerWrapper
{
    public int index;

    public Vector2f point;
    public CellType type;
    public BiomeType biomeType;
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