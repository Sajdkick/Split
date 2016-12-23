using UnityEngine;
using System.Collections;

public class MathF : MonoBehaviour {

    public static bool DoesLinesIntersect(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4)
    {

        double A1 = point2.y - point1.y;
        double B1 = point1.x - point2.x;
        double C1 = A1 * point1.x + B1 * point1.y;

        double A2 = point4.y - point3.y;
        double B2 = point3.x - point4.x;
        double C2 = A2 * point3.x + B2 * point3.y;

        double det = A1 * B2 - A2 * B1;

        if (det != 0)
        {

            double x = (B2 * C1 - B1 * C2) / det;
            double y = (A1 * C2 - A2 * C1) / det;

            double minX1 = Mathf.Min(point1.x, point2.x);
            double maxX1 = Mathf.Max(point1.x, point2.x);
            double minY1 = Mathf.Min(point1.y, point2.y);
            double maxY1 = Mathf.Max(point1.y, point2.y);

            double minX2 = Mathf.Min(point3.x, point4.x);
            double maxX2 = Mathf.Max(point3.x, point4.x);
            double minY2 = Mathf.Min(point3.y, point4.y);
            double maxY2 = Mathf.Max(point3.y, point4.y);

            bool X1 = minX1 <= x && x <= maxX1;
            bool Y1 = minY1 <= y && y <= maxY1;
            bool X2 = minX2 <= x && x <= maxX2;
            bool Y2 = minY2 <= y && y <= maxY2;

            bool intersect = X1 && Y1 && X2 && Y2;

            return intersect;

        }

        return false;

    }

    public static bool LineIntersection(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4, out Vector2 intersection)
    {

        double A1 = point2.y - point1.y;
        double B1 = point1.x - point2.x;
        double C1 = A1 * point1.x + B1 * point1.y;

        double A2 = point4.y - point3.y;
        double B2 = point3.x - point4.x;
        double C2 = A2 * point3.x + B2 * point3.y;

        double det = A1 * B2 - A2 * B1;

        if (det != 0)
        {

            double x = (B2 * C1 - B1 * C2) / det;
            double y = (A1 * C2 - A2 * C1) / det;

            double minX1 = Mathf.Min(point1.x, point2.x);
            double maxX1 = Mathf.Max(point1.x, point2.x);
            double minY1 = Mathf.Min(point1.y, point2.y);
            double maxY1 = Mathf.Max(point1.y, point2.y);

            double minX2 = Mathf.Min(point3.x, point4.x);
            double maxX2 = Mathf.Max(point3.x, point4.x);
            double minY2 = Mathf.Min(point3.y, point4.y);
            double maxY2 = Mathf.Max(point3.y, point4.y);

            bool X1 = minX1 <= x && x <= maxX1;
            bool Y1 = minY1 <= y && y <= maxY1;
            bool X2 = minX2 <= x && x <= maxX2;
            bool Y2 = minY2 <= y && y <= maxY2;

            bool intersect = X1 && Y1 && X2 && Y2;

            if (intersect)
            {

                intersection = new Vector2((float)x, (float)y);
                return true;

            }

        }

        intersection = Vector2.zero;
        return false;

    }

}
