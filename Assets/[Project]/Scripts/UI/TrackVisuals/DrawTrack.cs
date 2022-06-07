using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.VectorGraphics;

public class DrawTrack : MonoBehaviour
{
    public Shape shape;

    public void DrawNewShape(List<Vector2> shapePoints)
    {
        shape = new Shape();
        shape.Fill = null;
        BezierContour[] Contours = new BezierContour[1];
        Contours[0].Closed = false;

        Contours[0].Segments = new BezierPathSegment[1] {
            new BezierPathSegment {
                P0 = shapePoints[0],
                P1 = shapePoints[1],
                P2 = shapePoints[2]
            }
        };

        shape.Contours = Contours;
        shape.PathProps = new PathProperties {
            Stroke = new Stroke() { Color = Color.white, HalfThickness = 1f },
            Tail = PathEnding.Round,
            Head = PathEnding.Round,
        };
    }
}