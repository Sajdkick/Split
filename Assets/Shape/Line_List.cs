using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//Stores a single intersection and the poins connected to it.
public class Intersection
{
    public Point[] points = new Point[2];
    public Vector2 intersection;

}
//Stores information about the points that makes up an intersection list.
public class Point
{

    //The position of the point.
    public Vector2 position;
    //The right and left normals.
    public Vector2[] normals = new Vector2[2];
    //The points index in the Intersection List.
    public int index;

    //The intersection of the line segment created with the next and previous points.
    public List<Intersection>[] intersections = new List<Intersection>[2];
    
    //Get the intersection closest to the point.
    public Intersection GetClosestIntersection(int direction)
    {

        int index = 0;
        float min_distance = Vector2.Distance(position, intersections[direction][0].intersection);
        for (int i = 1; i < intersections[direction].Count; i++)
        {

            float new_distance = Vector2.Distance(position, intersections[direction][i].intersection);
            if (new_distance < min_distance)
            {

                min_distance = new_distance;
                index = i;

            }

        }

        return intersections[direction][index];

    }

}

//Stores points, used for intersection.
public class Intersection_List {

    public List<Point> points;
    public PolygonCollider2D collider;

    //Create two intersection lists.
    public static void Create_Lists(Vector2[] path1, PolygonCollider2D collider1, Vector2[] path2, PolygonCollider2D collider2, out Intersection_List list_1, out Intersection_List list_2)
    {

        list_1 = new Intersection_List(path1, collider1);
        list_2 = new Intersection_List(path2, collider2);

        list_1.SetIntersections(list_2);
        list_1.SortIntersections();
        list_2.SortIntersections();

        //list_1.Draw();
        //list_2.Draw();

    }

    Intersection_List(Vector2[] path, PolygonCollider2D poly_collider)
    {

        points = new List<Point>();
        for (int i = 0; i < path.Length; i++)
        {

            Point point = new Point();
            point.position = path[i];
            point.index = i;
            point.intersections[0] = new List<Intersection>();
            point.intersections[1] = new List<Intersection>();
            points.Add(point);

        }

        collider = poly_collider;
        GenerateNormals();
        //SortIntersections();

    }

    //Sets the normals for the points in the list.
    public void GenerateNormals()
    {

        if (points.Count > 1)
        {

            float dx = points[1].position.x - points[0].position.x;
            float dy = points[1].position.y - points[0].position.y;

            //We create a point that's been moved along one of the two possible normals, if this point is inside the collider
            //we use the other one.
            Vector2 normal1 = new Vector2(-dy, dx).normalized;
            Vector2 normal2 = new Vector2(dy, -dx).normalized;
            Vector2 mid_point = (points[0].position + (points[1].position - points[0].position) * 0.5f);
            Vector2 test_point1 = mid_point + normal1 * 0.00001f;
            Vector2 test_point2 = mid_point + normal2 * 0.00001f;

            //We check if our test point is inside the collider.
            bool test1 = collider.OverlapPoint(test_point1);
            bool test2 = collider.OverlapPoint(test_point2);

            if (test1 == test2)
                test1 = test2;

            for (int i = 0; i < points.Count; i++)
            {

                dx = points[(i + 1) % points.Count].position.x - points[i].position.x;
                dy = points[(i + 1) % points.Count].position.y - points[i].position.y;

                //The normal depends on if the test_point was inside the collider.
                Vector2 normal;
                if (test1)
                    normal = new Vector2(dy, -dx).normalized;
                else normal = new Vector2(-dy, dx).normalized;

                points[i].normals[1] = normal;
                points[(i + 1) % points.Count].normals[0] = normal;

            }
        }
    }

    //Checks for intersections between two intersection lists.
    public void SetIntersections(Intersection_List other)
    {

        //We loop through all the points of list 1.
        for (int i = 0; i < points.Count; i++)
        {

            //We loop through all points of list 2.
            for (int j = 0; j < other.points.Count; j++)
            {

                int index_1 = i;
                int index_2 = (i + 1) % points.Count;
                int index_3 = j;
                int index_4 = (j + 1) % other.points.Count;

                Vector2 point_1 = points[index_1].position;
                Vector2 point_2 = points[index_2].position;
                Vector2 point_3 = other.points[index_3].position;
                Vector2 point_4 = other.points[index_4].position;

                Vector2 intersection_point;
                //We check for an intersection.
                if (MathF.LineIntersection(point_1, point_2, point_3, point_4, out intersection_point)) {

                    Intersection intersection1 = new Intersection();
                    intersection1.points[0] = points[index_1];
                    intersection1.points[1] = points[index_2];
                    intersection1.intersection = intersection_point;

                    Intersection intersection2 = new Intersection();
                    intersection2.points[0] = other.points[index_3];
                    intersection2.points[1] = other.points[index_4];
                    intersection2.intersection = intersection_point;

                    intersection1.points[0].intersections[1].Add(intersection2);
                    intersection1.points[1].intersections[0].Add(intersection2);
                    intersection2.points[0].intersections[1].Add(intersection1);
                    intersection2.points[1].intersections[0].Add(intersection1);

                }

            }

        }

    }

    //Sorts the intersections.
    public void SortIntersections()
    {

        for(int i = 0; i < points.Count; i++)
        {

            points[i].intersections[0] = points[i].intersections[0].OrderBy(x => Vector2.Distance(points[i].position, x.intersection)).ToList();
            points[i].intersections[1] = points[i].intersections[1].OrderBy(x => Vector2.Distance(points[i].position, x.intersection)).ToList();

        }

    }

    //Draws the intersection list, for debugging purposes.
    public void Draw()
    {

        //Goes through all the points
        for (int i = 0; i < points.Count; i++)
        {

            //Draws the normal.
            Vector2 mid_point = (points[i].position + (points[(i + 1) % points.Count].position - points[i].position) * 0.5f);
            Debug.DrawRay(mid_point, points[i].normals[1], Color.black, 10);

            //If there's an intersection.
            if (points[i].intersections[1].Count != 0)
            {

                //We draw this line as red.
                Debug.DrawLine(points[i].position, points[(i + 1) % points.Count].position, Color.red, 10);
                //Vector2 last_point = points[i].intersections[1][0].intersection;
                //for (int j = 0; j < points[i].intersections[1].Count; j++)
                //{

                //    Debug.DrawLine(last_point, points[i].intersections[1][j].intersection, new Color(0, Random.Range(0f, 1f), Random.Range(0f, 1f)), 10);
                //    last_point = points[i].intersections[1][j].intersection;

                //}

                //Debug.DrawLine(last_point, points[(i + 1) % points.Count].position, Color.red, 10);

            }
            //If it doesnt intersect we draw it green.
            else Debug.DrawLine(points[i].position, points[(i + 1) % points.Count].position, Color.green, 10);

        }

    }
}
