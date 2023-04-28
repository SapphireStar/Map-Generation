using csDelaunay;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TestVoronoi : MonoBehaviour
{
    List<Vector2f> points;
    public int resolutionX;
    public int resolutionY;
    public int width;
    public int height;
    public int LloydrelaxationInteration;

    Voronoi voronoi;
    Dictionary<Vector2f, CornerWrapper> cornersLookup = new Dictionary<Vector2f, CornerWrapper>();
    Dictionary<Vector2f, CenterWrapper> centersLookup = new Dictionary<Vector2f, CenterWrapper>();
    // Start is called before the first frame update
    void Start()
    {
        InitializeVoronoi();
        InitializeCorners();
       // checkDuplicatedCorners().Forget();
    }
    private void InitializeVoronoi()
    {
        points = new List<Vector2f>();
        for (int i = 0; i < resolutionX; i++)
        {
            for (int j = 0; j < resolutionY; j++)
            {
                Vector2f point = new Vector2f(UnityEngine.Random.Range((float)i * (width / resolutionX), (float)(i + 1) * (width / resolutionX)),
                                        UnityEngine.Random.Range((float)j * (height / resolutionY), (float)(j + 1) * (height / resolutionY)));
                points.Add(point);
            }
        }
        voronoi = new Voronoi(points, new Rectf(0, 0, width, height), LloydrelaxationInteration, null);

        foreach (var item in points)
        {
            centersLookup[item] = new CenterWrapper(centersLookup.Count, item);
        }
    }
    
    List<Tuple<Vector2f, Vector2f>> edges = new List<Tuple<Vector2f, Vector2f>>();
    private void InitializeCorners()
    {
        foreach (var item in centersLookup.Values)
        {
            List<Vector2f> neighbours = voronoi.NeighborSitesForSite(item.Point);
            List<Vector2f> corners = voronoi.Region(item.Point);
            //check duplicated corners, if corner duplicated, then not create new cornerwrapper,
            //instead, add old cornerwrapper to current center, and add current centerwrapper to cornerwrapper's touches
            foreach (var center in neighbours)
            {
                CenterWrapper wrapper = centersLookup[center];
                item.corners.AddRange(CheckDuplicateCorners(wrapper, corners));

            }
            //create cornerwrapper, if corner is not duplicated, and add corner to current centerwrapper's corners,
            //and add current centerwrapper to new cornerwrapper's touches
            foreach (var corner in corners)
            {
                CornerWrapper newCorner = CreateCorner(corner);
                newCorner.index = corners.Count;
                this.corners.Add(corner);
                newCorner.touches.Add(item);
                item.corners.Add(newCorner);
            }

        }
        Debug.Log(centersLookup);
    }

    private List<CornerWrapper> CheckDuplicateCorners(CenterWrapper wrapper, List<Vector2f> corners)
    {
        List<CornerWrapper> result = new List<CornerWrapper>();
        foreach (var item in wrapper.corners)
        {
            for (int i = 0; i < corners.Count; i++)
            {
                if(Mathf.Abs(item.point.x - corners[i].x) < 1e-6
                && Mathf.Abs(item.point.y - corners[i].y) < 1e-6)
                {
                    result.Add(item);
                    corners.Remove(corners[i]);
                    i--;
                }
            }
        }
        return result;
    }

    private CornerWrapper CreateCorner(Vector2f point)
    {
        CornerWrapper wrapper = new CornerWrapper(point);
        cornersLookup[point] = wrapper;
        return wrapper;
    }

    JobHandle handle;
    NativeArray<Vector2f> input;
    NativeArray<float> result;
    public struct MyJob : IJob
    {
        public NativeArray<Vector2f> input;
        public void Execute()
        {
            for (int i = 0; i < input.Length; i++)
            {
                var corner1 = input[i];
                for (int j = 0; j < input.Length; j++)
                {
                    if (Mathf.Abs(corner1.x - input[j].x) < 1e-6 && Mathf.Abs(corner1.y - input[j].y) < 0.000001f && i != j)
                    {
                        Debug.Log($"corner1:({corner1.x},{corner1.y}) corner2:({input[j].x},{input[j].y}). i:{i} j:{j}");

                        Debug.Log("duplicated corner detected");
                    }
                }
            }
            input.Dispose();
        }
    }

    List<Vector2f> corners = new List<Vector2f>();
    private async UniTaskVoid checkDuplicatedCorners()
    {
        input = new NativeArray<Vector2f>(corners.ToArray(), Allocator.Persistent);
        MyJob jobdata = new MyJob
        {
            input = input
        };
        await jobdata.Schedule();
        input.Dispose();
    }
    private void OnDrawGizmos()
    {
        if (corners != null)
        {
            foreach (var item in corners)
            {
                Gizmos.DrawSphere(new Vector3(item.x, item.y, 0), 0.05f);
            }
        }

    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
