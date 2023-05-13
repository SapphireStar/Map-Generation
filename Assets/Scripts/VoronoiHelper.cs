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

    public static int GetAllAdvancedType()
    {
        return
            (int)CellType.SubTropicalDesert |
            (int)CellType.TemperateDesert |
            (int)CellType.Scorched |
            (int)CellType.Bare |
            (int)CellType.Tundra |
            (int)CellType.GrassLand |
            (int)CellType.ShrubLand |
            (int)CellType.TropicalSeasonalForest |
            (int)CellType.TemperateDeciduousForest |
            (int)CellType.TropicalRainforest |
            (int)CellType.TemperateRainforest |
            (int)CellType.Taiga |
            (int)CellType.Snow |
            (int)CellType.Lake;
    }
    public static CellType ResetCellType(CellType type)
    {
        type ^= (CellType)GetAllAdvancedType();
        type &= (CellType)~GetAllAdvancedType();
        return type;
    }

    public static CellType GetBiomes(CellType type)
    {
        CellType basetypes = CellType.Land | CellType.Water | CellType.Coast | CellType.Ocean | CellType.River | CellType.Border;
        type ^= basetypes;
        type &= ~basetypes;
        return type;
    }
}
