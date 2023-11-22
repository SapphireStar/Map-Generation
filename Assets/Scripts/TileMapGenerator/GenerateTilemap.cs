using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GenerateTilemap : MonoBehaviour
{
    public Tilemap testmap;
    public Tile testtile;
    public GenerateVoronoi voronoiDiagram;
    // Start is called before the first frame update
    void Start()
    {

    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {

        }

    }
    public void GenerateTiles(bool isOneByOne)
    {
        if (isOneByOne)
        {
            GenerateTilesOneByOne().Forget();
        }
        else
        {
            foreach (var item in voronoiDiagram.voronoiWrapper.CentersLookup.Values)
            {
                GenerateTiles(item, testtile, 25, 25).Forget();
            }
        }
    }
    public async UniTask GenerateTilesOneByOne()
    {
        foreach (var item in voronoiDiagram.voronoiWrapper.CentersLookup.Values)
        {
            await GenerateTiles(item, testtile, 25, 25);
        }
    }
    async UniTask GenerateTiles(CenterWrapper center, Tile tile, int width, int height)
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var corner in center.corners)
        {
            points.Add(new Vector3(corner.point.x * (width), corner.point.y * height, 0));
        }
        points.Add(new Vector3(center.corners[0].point.x * width, center.corners[0].point.y * height, 0));
        List<Vector3Int> tilesPositions = new List<Vector3Int>();

        //put high load operations in sub threads
        await UniTask.SwitchToThreadPool();
        for (int i = ((int)center.Point.x* width - width); i < ((int)center.Point.x * width + width * 2); i++)
        {
            for (int j = ((int)center.Point.y * height - height); j < ((int)center.Point.y * height + height * 2); j++)
            {                
                Vector3 cur = new Vector3(i, j, 0);
                if (CheckInPolygon(cur, points))
                {
                    tilesPositions.Add(Vector3Int.CeilToInt(cur));
                }

            }    
        }
        await UniTask.SwitchToMainThread();
        
        foreach (var item in tilesPositions)
        {
            //Stop generating when game stopped
            if (Application.isPlaying == false)
            {
                return;
            }
            testmap.SetTile(item, tile);
        }
    }
    public bool CheckInPolygon(Vector3 target, List<Vector3> points)
    {
        int flag = 0;
        for (int i = 0; i < points.Count-1; i++)
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
        if (CrossProduct(target, points[points.Count - 1], points[0]) < 0)
        {
            if (flag > 0)
            {
                return false;
            }
        }
        else if (CrossProduct(target, points[points.Count - 1], points[0]) > 0)
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
