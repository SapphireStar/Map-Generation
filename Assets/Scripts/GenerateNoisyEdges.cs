using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateNoisyEdges
{
    public void BuildNoisyEdge(List<EdgeWrapper> edges, float NOISY_LINE_TRADEOFF)
    {
        foreach (var edge in edges)
        {
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
            float minlength = 10;
            //if()

        }
    }
    
    public void BuildNoisyLineSegment(Vector2 upperA, Vector2 upperB, Vector2 lowerA, Vector2 lowerB, float minlength)
    {

    }
}
