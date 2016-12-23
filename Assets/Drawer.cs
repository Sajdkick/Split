using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Drawer : MonoBehaviour {

    static bool drawing = false;

    LineRenderer lineRenderer;

    List<Vector3> points;

    float minDistance = 1f;

    //What are 
    uint mode;

    // Use this for initialization
    void Start () {

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
        lineRenderer.SetColors(Color.red, Color.red);
        lineRenderer.SetWidth(0.2F, 0.2F);
        lineRenderer.SetVertexCount(0);

        points = new List<Vector3>();
        mode = 0;

        Physics2D.IgnoreLayerCollision(1, 1);

    }

    // Update is called once per frame
    void Update () {

        drawing = points.Count != 0;

        AddPoints();
        DrawLine();

        if (Input.GetKey(KeyCode.Escape))
        {
            points.Clear();
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {

            mode++;
            mode %= 3;

            print(mode);

        }

        if (Input.GetKeyUp(KeyCode.W))
        {

            Shape.LoadFromFile("137.1177.txt");
            
        }

    }

    void AddPoints()
    {

        if (Input.GetMouseButtonUp(0))
        {

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            //If we got more than two points we can close the shape.            
            if (points.Count >= 2)
            {

                if (mode == 2)
                {

                    points.Add(mousePos);
                    Create();

                }
                else {

                    //We make sure we dont cross any lines.
                    bool add = true;
                    for (int i = 0; i < points.Count - 2 && add; i++)
                    {

                        Vector2 point1 = points[i];
                        Vector2 point2 = points[i + 1];
                        Vector2 point3 = points[points.Count - 1];
                        Vector2 point4 = mousePos;

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

                            //float dist1 = PointToLineDistance(point1, point2, point4);
                            //float dist2 = PointToLineDistance(point3, point4, point1);
                            //float dist3 = PointToLineDistance(point3, point4, point2);

                            //bool tooClose = dist1 < minDistance || dist2 < minDistance || dist3 < minDistance;

                            if (intersect)
                                add = false;

                        }

                    }


                    if (add)
                    {
                        //If we try to create a point close to the first one, we close it.
                        if (Vector2.Distance(points[0], mousePos) < 0.3f)
                        {

                            Create();

                        }
                        else
                            points.Add(mousePos);

                    }

                }

            }
            else
            {

                points.Add(mousePos);

            }

        }

    }

    void DrawLine()
    {

        if(points.Count == 1)
        {

            lineRenderer.SetVertexCount(2);
            lineRenderer.SetPosition(0, points[0]);
            lineRenderer.SetPosition(1, points[0] * 0.9f);

        }
        else
        {

            lineRenderer.SetVertexCount(points.Count);
            lineRenderer.SetPositions(points.ToArray());

        }


    }

    void Create() {

        switch (mode) {

            case 0:
                Shape.CreateShape(points, new List<List<Vector3>>());
                break;
            case 1:
                Obstacle.CreateObstacle(points.ToArray());
                break;
            case 2:
                Spike.CreateSpike(points.ToArray());
                break;

        }

        points.Clear();

    }

    float PointToLineDistance(Vector2 lineStart, Vector2 lineEnd, Vector2 point) {

        float u = (point.x - lineStart.x) * (lineEnd.x - lineStart.x) + (point.y - lineStart.y) * (lineEnd.y - lineStart.y);
        u /= Mathf.Sqrt((lineEnd - lineStart).magnitude);

        float x = lineStart.x + u * (lineEnd.x - lineStart.x);
        float y = lineStart.y + u * (lineEnd.y - lineStart.y);

        return Vector2.Distance(new Vector2(x, y), point);

    }

}
