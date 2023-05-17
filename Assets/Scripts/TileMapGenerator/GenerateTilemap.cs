using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateTilemap : MonoBehaviour
{
    public Tilemap testmap;
    public Tile testtile;
    // Start is called before the first frame update
    void Start()
    {
        Vector3 a = new Vector3(3.5f,0,0);
        Vector3 b = new Vector3(0, 3.5f, 0);
        Vector3 c = new Vector3(3.5f, 7, 0);
        Vector3 d = new Vector3(7, 3.5f, 0);

        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                Vector3 cur = new Vector3(i, j, 0);
                if(CheckInPolygon(cur,new Vector3[] { a*10,b * 10, c * 10, d * 10 }))
                {
                    testmap.SetTile(Vector3Int.CeilToInt(cur), testtile);
                }
            }
        }
        
    }
    public bool CheckInPolygon(Vector3 target, Vector3[] points)
    {
        int flag = 0;
        for (int i = 0; i < points.Length-1; i++)
        {
            if(CrossProduct(target, points[i], points[i + 1]) < 0)
            {
                if (flag > 0)
                {
                    return false;
                }
                flag = -1;
            }
            else if (CrossProduct(target, points[i], points[i + 1]) > 0)
            {
                if(flag < 0)
                {
                    return false;
                }
                flag = 1;
            }
        }
        if (CrossProduct(target, points[points.Length-1], points[0]) < 0)
        {
            if (flag > 0)
            {
                return false;
            }
        }
        else if (CrossProduct(target, points[points.Length - 1], points[0]) > 0)
        {
            if (flag < 0)
            {
                return false;
            }
        }
        return true;
    }
    public float CrossProduct(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 v0 = (end - start).normalized;
        Vector3 v1 = (point - start).normalized;
        return Vector3.Cross(v0, v1).z;
    }
}
