using UnityEngine;
using System.Collections.Generic;

public struct Intersection
{

    public int intersection_index;
    public Vector2 intersection;

}
public class Line
{
    public Line[] link = new Line[2];

    //The position of the vertex.
    public Vector2[] point = new Vector2[2];

    public void SetIntersects(bool value) { intersects = value; }

    public bool intersects;
    public List<Intersection> intersections = new List<Intersection>();
    public Intersection GetClosestIntersection(int direction)
    {

        int index = 0;
        float min_distance = Vector2.Distance(point[1 - direction], intersections[0].intersection);
        for (int i = 1; i < intersections.Count; i++)
        {

            float new_distance = Vector2.Distance(point[1 - direction], intersections[i].intersection);
            if (new_distance < min_distance)
            {

                min_distance = new_distance;
                index = i;

            }

        }

        return intersections[index];

    }

}

public class Line_List {

    public List<Line> lines;

    public Line_List(Vector2[] path)
    {

        lines = new List<Line>();
        for (int i = 0; i < path.Length; i++)
        {

            Line line = new Line();
            line.point[0] = path[i];
            line.point[1] = path[(i + 1) % path.Length];
            line.intersects = false;
            lines.Add(line);

        }

        //We link the lines.
        if (lines.Count > 1)
        {

            {
                lines[0].link[1] = lines[1];
                lines[0].link[0] = lines[lines.Count - 1];
            }

            for (int i = 1; i < lines.Count; i++)
            {
                lines[i].link[1] = lines[(i + 1) % lines.Count];
                lines[i].link[0] = lines[i - 1];
            }

        }

    }
 
    public void SetIntersections(Line_List other)
    {

        for (int i = 0; i < lines.Count; i++)
        {

            //We loop through all lines of path2.
            for (int j = 0; j < other.lines.Count; j++)
            {

                Line line1 = lines[i];
                Line line2 = other.lines[j];

                Vector2 intersection_point;
                //We check for an intersection.
                if (MathF.LineIntersection(line1.point[0], line1.point[1], line2.point[0], line2.point[1], out intersection_point))
                {

                    Intersection intersection1;
                    intersection1.intersection_index = j;
                    intersection1.intersection = intersection_point;

                    line1.intersects = true;
                    line1.intersections.Add(intersection1);

                    Intersection intersection2;
                    intersection2.intersection_index = i;
                    intersection2.intersection = intersection_point;

                    line2.intersects = true;
                    line2.intersections.Add(intersection2);

                }

            }

        }

    }

    public void Draw()
    {

        for (int i = 0; i < lines.Count; i++)
        {

            if (lines[i].intersects)
            {

                lines[i].intersections.Sort(delegate (Intersection a, Intersection b)
                {
                    return Vector2.Distance(lines[i].point[0], a.intersection)
                    .CompareTo(
                      Vector2.Distance(lines[i].point[0], b.intersection));
                });

                Debug.DrawLine(lines[i].point[0], lines[i].intersections[0].intersection, Color.red, 10);
                Vector2 last_point = lines[i].intersections[0].intersection;
                for (int j = 0; j < lines[i].intersections.Count; j++)
                {

                    Debug.DrawLine(last_point, lines[i].intersections[j].intersection, new Color(0, Random.Range(0f, 1f), Random.Range(0f, 1f)), 10);
                    last_point = lines[i].intersections[j].intersection;

                }

                Debug.DrawLine(last_point, lines[i].point[1], Color.red, 10);

            }

            else Debug.DrawLine(lines[i].point[0], lines[i].point[1], Color.green, 10);

        }

    }
}
