using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateNoisyEdges
{
    public static void BuildNoisyEdge(Dictionary<Vector2f,EdgeWrapper> edges, float NOISY_LINE_TRADEOFF)
    {
        foreach (var edge in edges.Values)
        {
            if (edge.v0 == null || edge.v1 == null || edge.d0 == null || edge.d1 == null)
            {
                continue;
            }
                
            //do the first segmentation
            Vector2 upperA = Vector2.Lerp(new Vector2(edge.v0.point.x, edge.v0.point.y),
                         new Vector2(edge.d0.Point.x, edge.d0.Point.y),
                         NOISY_LINE_TRADEOFF);
            Vector2 upperB = Vector2.Lerp(new Vector2(edge.v0.point.x, edge.v0.point.y),
                         new Vector2(edge.d1.Point.x, edge.d1.Point.y),
                         NOISY_LINE_TRADEOFF);
            Vector2 lowerA = Vector2.Lerp(new Vector2(edge.v1.point.x, edge.v1.point.y),
                         new Vector2(edge.d0.Point.x, edge.d0.Point.y),
                         NOISY_LINE_TRADEOFF);
            Vector2 lowerB = Vector2.Lerp(new Vector2(edge.v1.point.x, edge.v1.point.y),
                         new Vector2(edge.d1.Point.x, edge.d1.Point.y),
                         NOISY_LINE_TRADEOFF);
            float minlength = 0.2f;
            if (edge.d0.biomeType != edge.d1.biomeType)
            {
                minlength = 0.2f;
            }
            //if is ocean then don't need to create the noisy edge
            if (VoronoiHelper.IsOcean(edge.d0.type) && VoronoiHelper.IsOcean(edge.d1.type))
            {
                minlength = 100;
            }
            if (VoronoiHelper.IsCoast(edge.d0.type) && VoronoiHelper.IsCoast(edge.d1.type))
            {
                minlength = 0.2f;
            }
            if (edge.riverVolume > 0)
            {
                minlength = 0.2f;
            }
            edge.noisyPoints.Clear();
            edge.noisyPoints.AddRange(BuildNoisyLineSegment(new Vector2(edge.v0.point.x, edge.v0.point.y), upperA, 
                                                     new Vector2(edge.midpoint.x, edge.midpoint.y), upperB, 
                                                     minlength));
            edge.noisyPoints.AddRange(BuildNoisyLineSegment(new Vector2(edge.midpoint.x, edge.midpoint.y), lowerA,
                                                     new Vector2(edge.v1.point.x, edge.v1.point.y), lowerB,
                                                     minlength));
        }
    }
    
    public static List<Vector2> BuildNoisyLineSegment(Vector2 A, Vector2 B, Vector2 C, Vector2 D, float minlength)
    {
        List<Vector2> points = new List<Vector2>();

        points.Add(A);
        Subdivide(A, B, C, D, minlength, points);
        points.Add(C);
        return points;
    }
    private static void Subdivide(Vector2 A, Vector2 B, Vector2 C, Vector2 D, float minlength, List<Vector2> points)
    {

        if (Vector2.Distance(A, C) < minlength || Vector2.Distance(B, D) < minlength)
        {
            return;
        }
        float p = Random.Range(0.2f, 0.8f);
        float q = Random.Range(0.2f, 0.8f);
        Vector2 upperA = Vector2.Lerp(A, D, p);
        Vector2 lowerA = Vector2.Lerp(B, C, p);
        Vector2 upperB = Vector2.Lerp(A, B, q);
        Vector2 lowerB = Vector2.Lerp(D, C, q);

        //midpoint
        Vector2 H = Vector2.Lerp(upperA, lowerA, q);

        float s = Random.Range(0f, 0.4f);
        float t = Random.Range(0f, 0.4f);

        Subdivide(A, Vector2.Lerp(upperB, B, s), H, Vector2.Lerp(upperA, D, t), minlength, points);
        points.Add(H);
        Subdivide(H, Vector2.Lerp(lowerA, B, s), C, Vector2.Lerp(lowerB, D, t), minlength, points);

    }
}
