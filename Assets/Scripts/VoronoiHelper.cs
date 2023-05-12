using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoronoiHelper
{
    public static bool IsLand(CellType type)
    {
        if((type&CellType.Land) > 0)
        {
            return true;
        }
        return false;
    }
    public static bool IsCoast(CellType type)
    {
        if ((type & CellType.Land) > 0 && (type & CellType.Coast)>0)
        {
            return true;
        }
        return false;
    }

    public static bool IsOcean(CellType type)
    {
        if((type&CellType.Water)>0 && (type & CellType.Ocean) > 0)
        {
            return true;
        }
        return false;
    }
    public static bool IsLake(CellType type)
    {
        if ((type & CellType.Water) > 0 && (type & CellType.Ocean) == 0)
        {
            return true;
        }
        return false;
    }
    public static bool IsRiver(CellType type)
    {
        if ((type & CellType.River) > 0)
        {
            return true;
        }
        return false;
    }
}
