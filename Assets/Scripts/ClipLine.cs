using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClipLine
{
    [Flags]
    enum OutCode
    {
        Inside = 0,
        Left = 1,
        Right = 2,
        Bottom = 4,
        Top = 8
    }
    private static OutCode ComputeOutCode(float x, float y, Rectf r)
    {
        var code = OutCode.Inside;

        if (x < r.x) code |= OutCode.Left;
        if (x > r.width ) code |= OutCode.Right;
        if (y < r.y) code |= OutCode.Top;
        if (y > r.height) code |= OutCode.Bottom;

        return code;
    }
    private static OutCode ComputeOutCode(Vector2f p, Rectf r) { return ComputeOutCode(p.x, p.y, r); }
    private static Vector2f CalculateIntersection(Rectf r, Vector2f p1, Vector2f p2, OutCode clipTo)
    {
        var dx = (p2.x - p1.x);
        var dy = (p2.y - p1.y);

        var slopeY = dx / dy; // slope to use for possibly-vertical lines
        var slopeX = dy / dx; // slope to use for possibly-horizontal lines

        if (clipTo.HasFlag(OutCode.Top))
        {
            return new Vector2f(
                p1.x + slopeY * (r.y - p1.y),
                r.y
                );
        }
        if (clipTo.HasFlag(OutCode.Bottom))
        {
            return new Vector2f(
                p1.x + slopeY * (r.y - r.height - p1.y),
                r.y - r.height
                );
        }
        if (clipTo.HasFlag(OutCode.Right))
        {
            return new Vector2f(
                r.x + r.width,
                p1.y + slopeX * (r.x + r.width - p1.x)
                );
        }
        if (clipTo.HasFlag(OutCode.Left))
        {
            return new Vector2f(
                r.x,
                p1.y + slopeX * (r.x - p1.x)
                );
        }
        throw new ArgumentOutOfRangeException("clipTo = " + clipTo);
    }
    public static Tuple<Vector2f, Vector2f> ClipSegment(Rectf r, Vector2f p1, Vector2f p2)
    {
        // classify the endpoints of the line
        var outCodeP1 = ComputeOutCode(p1, r);
        var outCodeP2 = ComputeOutCode(p2, r);
        var accept = false;

        while (true)
        { // should only iterate twice, at most
          // Case 1:
          // both endpoints are within the clipping region
            if ((outCodeP1 | outCodeP2) == OutCode.Inside)
            {
                accept = true;
                break;
            }

            // Case 2:
            // both endpoints share an excluded region, impossible for a line between them to be within the clipping region
            if ((outCodeP1 & outCodeP2) != 0)
            {
                break;
            }

            // Case 3:
            // The endpoints are in different regions, and the segment is partially within the clipping rectangle

            // Select one of the endpoints outside the clipping rectangle
            var outCode = outCodeP1 != OutCode.Inside ? outCodeP1 : outCodeP2;

            // calculate the intersection of the line with the clipping rectangle
            var p = CalculateIntersection(r, p1, p2, outCode);

            // update the point after clipping and recalculate outcode
            if (outCode == outCodeP1)
            {
                p1 = p;
                outCodeP1 = ComputeOutCode(p1, r);
            }
            else
            {
                p2 = p;
                outCodeP2 = ComputeOutCode(p2, r);
            }
        }
        // if clipping area contained a portion of the line
        if (accept)
        {
            return new Tuple<Vector2f, Vector2f>(p1, p2);
        }

        // the line did not intersect the clipping area
        return null;
    }
}
