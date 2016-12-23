using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shape : MonoBehaviour {

    MeshFilter meshFilter;
    PolygonCollider2D collider;
    PolygonCollider2D trigger;

    static int id = 0;
    static List<GameObject> shapeList = new List<GameObject>();

    Line_List line_list;

    bool merging;

    // Use this for initialization
    void Awake() {

        meshFilter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<PolygonCollider2D>();

        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<Rigidbody2D>();

        gameObject.name = id.ToString();
        id++;

        merging = false;

        gameObject.layer = 1;

        shapeList.Add(gameObject);

    }

    void Start()
    {

        AddTrigger();

    }

    public static void CreateShape(List<Vector3> outside, List<List<Vector3>> holes)
    {
        GameObject shape = new GameObject();
        shape.AddComponent<Shape>().SetMesh(outside.ToArray());
        shape.GetComponent<Shape>().GenerateNormals();
    }

    void GenerateNormals()
    {

        Vector2[] path = GetShapePath(this);
        

    }

    void AddTrigger()
    {

        GameObject child = new GameObject();
        child.transform.SetParent(transform);

        PolygonCollider2D trigger = child.AddComponent<PolygonCollider2D>();
        trigger.isTrigger = true;
        trigger.SetPath(0, collider.GetPath(0));

    }

    public static GameObject GetShape(int index)
    {

        return shapeList[index];

    }

    public static void SplitAll(Vector2 point3, Vector2 point4)
    {

        int nrOfShapes = shapeList.Count;
        int count = 0;
        for(int i = 0; i < nrOfShapes; i++)
        {

            if(shapeList[i - count].GetComponent<Shape>().Split(point3, point4))
            {

                count++;

            }

        }

    }

    public bool Split(Vector2 point3, Vector2 point4)
    {

        List<Vector3> newVertices = new List<Vector3>();
        
        //Used to remember the index of the splits
        List<int> splits = new List<int>();
        //Used to remember the order of the splits
        List<Vector3> unorderedSplits = new List<Vector3>();

        Vector2[] path = collider.GetPath(0);
        for (int i = 0; i < path.Length; i++)
        {

            path[i] = transform.localToWorldMatrix.MultiplyPoint3x4(path[i]);

        }
        //Find all the intersections.
        for (int i = 0; i < path.Length; i++)
        {

            Vector2 point1 = path[i];
            Vector2 point2 = path[(i + 1) % path.Length];

            newVertices.Add(point1);

            float determinant = (point1.x - point2.x) * (point3.y - point4.y) - (point1.y - point2.y) * (point3.x - point4.x);
            if (determinant == 0)
                continue;

            float x = (point1.x * point2.y - point1.y * point2.x) * (point3.x - point4.x) - (point1.x - point2.x) * (point3.x * point4.y - point3.y * point4.x);
            x /= determinant;

            float y = (point1.x * point2.y - point1.y * point2.x) * (point3.y - point4.y) - (point1.y - point2.y) * (point3.x * point4.y - point3.y * point4.x);
            y /= determinant;

            //?????????
            //if (Vector3.Cross(point1, new Vector2(x, y)).magnitude > 0.001f) continue;
            //if (Vector3.Dot(point1 - point2, new Vector2(x, y) - point2) < 0.01f) continue;
            //if (Vector3.Dot(point2 - point1, new Vector2(x, y) - point1) < 0.01f) continue;
            //We check if the split is on a vertex
            //if (Vector3.Distance(point1, new Vector2(x, y)) < 0.01f) continue;
            //if (Vector3.Distance(point2, new Vector2(x, y)) < 0.01f) continue;

            //All you need?
            if (Vector3.Dot(point1 - new Vector2(x, y), point2 - new Vector2(x, y)) < 0.00001f) {

                newVertices.Add(new Vector3(x, y));
                splits.Add(i + splits.Count + 1);
                unorderedSplits.Add(new Vector3(x, y));

            }

        }

        if (splits.Count <= 1)
            return false;


        List<Vector3> listStart = new List<Vector3>();
        List<Vector3> listEnd = new List<Vector3>();

        //We find the starting point
        float maxDist = -1;
        int index = -1;
        Vector3 point = unorderedSplits[0];
        for (int j = 0; j < unorderedSplits.Count; j++)
        {
            float distance = Vector3.Distance(point, unorderedSplits[j]);
            if (distance > maxDist)
            {

                maxDist = distance;
                index = j;

            }

        }

        point = unorderedSplits[index];
        unorderedSplits.Remove(point);
        listStart.Add(point);

        bool end = true;
        while(unorderedSplits.Count != 0)
        {

            maxDist = -1;
            index = -1;
            for (int j = 0; j < unorderedSplits.Count; j++)
            {
                float distance = Vector3.Distance(point, unorderedSplits[j]);
                if (distance > maxDist)
                {

                    maxDist = distance;
                    index = j;

                }

            }

            if(end)
            {

                point = unorderedSplits[index];
                unorderedSplits.Remove(point);
                listEnd.Add(point);

            }
            else
            {

                point = unorderedSplits[index];
                unorderedSplits.Remove(point);
                listStart.Add(point);

            }

            end = !end;

        }

        List<Vector3> orderedSplits = new List<Vector3>();

        for(int i = 0; i < listStart.Count; i++)
        {

            orderedSplits.Add(listStart[i]);

        }

        for (int i = listEnd.Count - 1; i >= 0; i--)
        {

            orderedSplits.Add(listEnd[i]);

        }

        //Split the mesh where they intersect.
        List<List<Vector3>> vertexLists = new List<List<Vector3>>();
        vertexLists.Add(new List<Vector3>());

        int nrOfPoints = newVertices.Count;
        int counter = 0;
        for (int i = splits[0]; i <= nrOfPoints + splits[0]; i++)
        {

            int j = i % nrOfPoints;

            vertexLists[counter].Add(newVertices[j]);

            //If we split here.
            if (counter + 1 < splits.Count && j == splits[counter + 1])
            {

                counter++;
                vertexLists.Add(new List<Vector3>());
                vertexLists[counter].Add(newVertices[j]);

            }

        }

        //We find all the voids
        List<List<Vector3>> voids = new List<List<Vector3>>();
        //We loop through all the "pieces"
        for (int i = 0; i < vertexLists.Count; i++)
        {

            //We loop through all the odd splits because the odd numbers are exit splits.
            for(int j = 1; j < orderedSplits.Count - 1; j += 2)
            {

                //If it contains the split j
                if (vertexLists[i].Contains(orderedSplits[j]))
                {

                    //We go through the even splits and try to find a entry split
                    //that is contained in the same "piece"
                    for (int k = j + 1; k < orderedSplits.Count - 1; k += 2) {

                        if (vertexLists[i].Contains(orderedSplits[k])) {

                            voids.Add(vertexLists[i]);
                            j = k;
                            break;

                        }

                    }

                }

            }

        }

        for(int i = 0; i < voids.Count; i++)
        {

            vertexLists.Remove(voids[i]);

        }

        PuzzleMaster(vertexLists, orderedSplits, voids);

        //GameObject shape = new GameObject();
        //shape.AddComponent<Shape>().SetMesh(vertexLists[i].ToArray());
        shapeList.Remove(this.gameObject);
        Destroy(this.gameObject);

        return true;

    }

    public void Merge2(Shape shape)
    {

        //Get the paths from the shapes
        Vector2[] path1 = GetShapePath(this);
        Vector2[] path2 = GetShapePath(shape);

        List<Line>[] line_lists = new List<Line>[2];
        line_lists[0] = line_list.lines;
        line_lists[1] = shape.line_list.lines;

        //DrawLineList(line_lists[0]);
        //DrawLineList(line_lists[1]);

        //We decide which point to start on, we make sure it's not within the bounds of a shape.
        int point_index = 0;
        if (shape.collider.OverlapPoint(line_lists[0][0].point[0]))
        {
            point_index = 1;
        }

        int start_index = point_index;
        int direction = 1;
        int target_list = 0;
        Vector2 start_point = line_lists[target_list][start_index].point[0];
        List<Vector2> vertices = new List<Vector2>();
        do
        {
            List<Vector2> traversed_points;
            int exit_index;
            bool finished = TraverseLineList(line_lists[target_list], point_index, start_point, direction, out exit_index, out traversed_points);

            Vector2 last_point = traversed_points[0];
            for(int i = 0; i < traversed_points.Count; i++)
            {

                Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
                Debug.DrawLine(last_point, traversed_points[i], color, 10);
                vertices.Add(traversed_points[i]);
                last_point = traversed_points[i];

            }

            if (!finished)
            {

                //We get the intersection and add it to the vertex list.
                Intersection intersection = line_lists[target_list][exit_index].GetClosestIntersection(direction);
                vertices.Add(intersection.intersection);

                //This switches the target line.
                target_list = 1 - target_list;

                //We switch to the correct index on the new line.
                point_index = intersection.intersection_index;

                //We dont calculate if we want to change direction at the moment. TODO

            }
            else break;

            //TODO
            //We must make it possible to find a line segment from an intersection, at the moment we can only get the line.
            //Maybe we could make it so every line knows about its neighbours? That would probably work!
            //We must adjust the LineSegment to contain three lines, that way you could

        } while (true);

        List<Vector3> vertex_list = new List<Vector3>();

        for(int i = 0; i < vertices.Count; i++)
        {

            vertex_list.Add(vertices[i]);

        }

        CreateShape(vertex_list, new List<List<Vector3>>());

        shapeList.Remove(this.gameObject);
        Destroy(this.gameObject);
        shapeList.Remove(shape.gameObject);
        Destroy(shape.gameObject);


    }
    private Vector2[] GetShapePath(Shape shape)
    {

        //Get the paths from the shapes
        Vector2[] path = shape.collider.GetPath(0);
        for (int i = 0; i < path.Length; i++)
        {

            path[i] = shape.transform.localToWorldMatrix.MultiplyPoint3x4(path[i]);

        }
        return path;

    }
    private bool TraverseLineList(List<Line> line_list, int start_index, Vector2 target_point, int direction, out int exit_index, out List<Vector2> traversed_points)
    {

        traversed_points = new List<Vector2>();
        traversed_points.Add(line_list[start_index].point[1 - direction]);

        start_index -= 1 + direction;
        if (start_index < 0)
            start_index = line_list.Count - 1;
        bool intersection = line_list[start_index].intersects;

        if (!intersection)
        {

            int i;
            int ic = (-1 + 2 * direction);
            for (i = start_index + ic; i != start_index; i += ic)
            {

                int list_index = i % line_list.Count;
                int point_index = 1 - direction;

                if (i < 0)
                    i = line_list.Count - 1;

                exit_index = list_index;
                if (line_list[list_index].intersects)
                {

                    return false;

                }

                //If we've reached our target point we break;
                if (line_list[list_index].point[point_index].Equals(target_point))
                {

                    return true;

                }

                Vector2 new_point = line_list[list_index].point[point_index];
                traversed_points.Add(new_point);

            }

            exit_index = i % line_list.Count;

        }
        else
        {

            exit_index = start_index;
            return true;

        }

        return true;

    }
    public void Merge(Shape shape)
    {

        //Get the paths from the shapes
        Vector2[] path = collider.GetPath(0);
        for (int i = 0; i < path.Length; i++)
        {

            path[i] = transform.localToWorldMatrix.MultiplyPoint3x4(path[i]);

        }

        Vector2[] path2 = shape.collider.GetPath(0);
        for (int i = 0; i < path2.Length; i++)
        {

            path2[i] = shape.transform.localToWorldMatrix.MultiplyPoint3x4(path2[i]);

        }

        //Find the vertices that are overlaped.
        List<int> pathOverlaps = new List<int>();
        for (int i = 0; i < path.Length; i++)
            if (shape.collider.OverlapPoint(path[i]))
                pathOverlaps.Add(i);

        List<int> path2Overlaps = new List<int>();
        for (int i = 0; i < path2.Length; i++)
            if (collider.OverlapPoint(path2[i]))
                path2Overlaps.Add(i);

        //We find the intersecting lines.
        List<int> pathIntersect = new List<int>();
        for (int i = 0; i < path.Length; i++)
            if (pathOverlaps.Contains(i) != pathOverlaps.Contains((i + 1) % path.Length))
                pathIntersect.Add(i);

        List<int> path2Intersect = new List<int>();
        for (int i = 0; i < path2.Length; i++)
            if (path2Overlaps.Contains(i) != path2Overlaps.Contains((i + 1) % path2.Length))
                path2Intersect.Add(i);     

        //We draw the intersectors.
        for (int i = 0; i < pathIntersect.Count; i++)
        {
            int index = pathIntersect[i];
            Debug.DrawLine(path[index], path[(index + 1) % path.Length], Color.red, 10);
        }
        for (int i = 0; i < path2Intersect.Count; i++)
        {
            int index = path2Intersect[i];
            Debug.DrawLine(path2[index], path2[(index + 1) % path2.Length], Color.green, 10);
        }

        //We find the intersected.
        List<int> pathIntersected = new List<int>();
        for (int i = 0; i < path2Intersect.Count; i++)
        {

            Vector2 point1 = path2[path2Intersect[i]];
            Vector2 point2 = path2[(path2Intersect[i] + 1) % path2Intersect.Count];

            for (int j = 0; j < path.Length; j++)
            {

                Vector2 point3 = path[j];
                Vector2 point4 = path[(j + 1) % path.Length];

                if (MathF.DoesLinesIntersect(point1, point2, point3, point4))
                {

                    pathIntersected.Add(j);
                    Debug.DrawLine(point3, point4, Color.black, 10);

                }

            }

        }
        //We find the intersected.
        List<int> path2Intersected = new List<int>();
        for (int i = 0; i < pathIntersect.Count; i++)
        {

            Vector2 point1 = path[pathIntersect[i]];
            Vector2 point2 = path[(pathIntersect[i] + 1) % pathIntersect.Count];

            for (int j = 0; j < path2.Length; j++)
            {

                Vector2 point3 = path2[j];
                Vector2 point4 = path2[(j + 1) % path2.Length];

                if (MathF.DoesLinesIntersect(point1, point2, point3, point4))
                {

                    path2Intersected.Add(j);
                    Debug.DrawLine(point3, point4, Color.black, 10);

                }

            }

        }



        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < path.Length; i++)
        {

            vertices.Add(path[i]);

        }

        for (int i = 0; i < path2.Length; i++)
        {

             vertices.Add(path2[i]);

        }

        CreateShape(vertices, new List<List<Vector3>>());

        shapeList.Remove(this.gameObject);
        Destroy(this.gameObject);
        shapeList.Remove(shape.gameObject);
        Destroy(shape.gameObject);

    }

    void SetCollider(Poly2Mesh.Polygon polygon)
    {

        Vector2[] outsidePath = new Vector2[polygon.outside.Count];
        for (int i = 0; i < polygon.outside.Count; i++)
            outsidePath[i] = new Vector2(polygon.outside[i].x, polygon.outside[i].y);

        collider.SetPath(0, outsidePath);

        for (int i = 0; i < polygon.holes.Count; i++)
        {

            List<Vector3> hole = polygon.holes[i];
            Vector2[] holePath = new Vector2[hole.Count];
            for (int j = 0; j < hole.Count; j++)
                holePath[j] = new Vector2(hole[j].x, hole[j].y);

            collider.SetPath(1 + i, holePath);

        }

    }
    void SetMesh(Vector3[] vertices)
    {

        int nrOfPoints = vertices.Length;
        Vector2[] path = new Vector2[nrOfPoints];
        for (int i = 0; i < nrOfPoints; i++)
            path[i] = new Vector2(vertices[i].x, vertices[i].y);

        collider.SetPath(0, path);

        Triangulator triangulator = new Triangulator(collider.points);
        int[] triangles = triangulator.Triangulate();

        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = triangles;

        if(path.Length != 0)
            line_list = new Line_List(path);

    }

    void PuzzleMaster(List<List<Vector3>> pieces, List<Vector3> splits, List<List<Vector3>> voids)
    {

        for (int i = 0; i < pieces.Count; i++) {

            //DrawPiece(pieces[i].ToArray(), Color.green);

        }

        for(int i = 0; i < pieces.Count; i++)
        {

            List<Vector3> piece = pieces[i];

            //A list containing the splits for the current piece
            List<int> matches = GetSplits(piece, splits);

            if (matches.Count != 1)
            {

                Color color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
                //DrawPiece(piece.ToArray(), color);

                //We check for a simple piece that contains no voids.
                //If the two splits in the piece are after one another.
                if (matches[1] - matches[0] == 1)
                {
                    GameObject shape = new GameObject();
                    shape.AddComponent<Shape>().SetMesh(piece.ToArray());
                }
                else
                {

                    int splitIndex = matches[0];
                    //We go through all the voids
                    for (int j = 0; j < voids.Count; j++)
                    {

                        //We check if the void contains the next split.

                        if (voids[j].Contains(splits[splitIndex + 1]))
                        {

                            Vector2 a = voids[j][1];
                            Vector2 b = piece[1];
                            Vector2 p1 = splits[splitIndex];
                            Vector2 p2 = splits[splitIndex + 1];

                            //We check if the void is on the correct side of the split.
                            if (((p1.y - p2.y) * (a.x - p1.x) + (p2.x - p1.x) * (a.y - p1.y)) * ((p1.y - p2.y) * (b.x - p1.x) + (p2.x - p1.x) * (b.y - p1.y)) > 0)
                            {

                                //DrawPiece(voids[j].ToArray(), color);

                                //We add this void to the piece
                                AddVoid(voids[j], piece);

                                //The piece can contain multiple voids, so we get the next split
                                //and loop through the voids again
                                splitIndex = GetSplits(voids[j], splits)[1];
                                j = -1;

                            }

                        }

                        if (splitIndex + 1 == matches[matches.Count - 1])
                            break;

                    }

                    GameObject shape = new GameObject();
                    shape.AddComponent<Shape>().SetMesh(piece.ToArray());

                }

            }
            else {

                GameObject shape = new GameObject();
                shape.AddComponent<Shape>().SetMesh(piece.ToArray());

            }

        }

    }

    List<int> GetSplits(List<Vector3> piece, List<Vector3> splits)
    {

        List<int> matches = new List<int>();
        for(int i = 0; i < splits.Count; i++)
        {

            if (piece.Contains(splits[i]))
            {

                matches.Add(i);

            }

        }

        return matches;

    }

    //Adds a void to the piece
    List<Vector3> AddVoid(List<Vector3> theVoid, List<Vector3> piece)
    {

            for (int k = theVoid.Count - 1; k >= 0; k--)
            {

                piece.Insert(0, theVoid[k]);

            }

        return piece;

    }

    void DrawPiece(Vector3[] piece, Color color) {

        for (int i = 0; i < piece.Length - 1; i++) {

            Debug.DrawLine(piece[i], piece[i + 1], color * ((i + 1)/(float)piece.Length), 10);

        }

    }

    //Saves the piece to a file.
    void SaveToFile()
    {

        Vector2[] path = collider.GetPath(0);
        string[] string_lines = new string[path.Length];
        for (int i = 0; i < path.Length; i++)
        {

            string_lines[i] = transform.localToWorldMatrix.MultiplyPoint3x4(path[i]).ToString();

        }

        System.IO.File.WriteAllLines("/SavedShapes/Shape" + id + ".txt", string_lines);

    }

    //Saves the piece to a file.
    static void SaveToFile(Vector3[] vertices, string name)
    {

        string[] string_lines = new string[vertices.Length * 2];
        for (int i = 0; i < vertices.Length; i++)
        {

            print(vertices[i].x.ToString() + ":" + vertices[i].y.ToString());

            string_lines[i * 2] = vertices[i].x.ToString();
            string_lines[i * 2 + 1] = vertices[i].y.ToString();

        }

        System.IO.File.WriteAllLines(name + ".txt", string_lines);

    }

    public static void LoadFromFile(string path) {

        System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo();
        format.NegativeSign = "-";
        format.NumberDecimalSeparator = ".";
        string[] string_lines = System.IO.File.ReadAllLines(path);



        List<Vector3> vertices = new List<Vector3>();
        for(int i = 0; i < string_lines.Length / 2; i++)
        {

            //print(float.Parse(lines[i * 2]) + ":" + float.Parse(lines[i * 2 + 1]));

            vertices.Add(new Vector3(float.Parse(string_lines[i * 2]), float.Parse(string_lines[i * 2 + 1]), 0));

        }

        CreateShape(vertices, new List<List<Vector3>>());

    }

    void Merge3(Shape shape)
    {

        //Get the paths from the shapes
        Vector2[] path = collider.GetPath(0);
        for (int i = 0; i < path.Length; i++)
        {

            path[i] = transform.localToWorldMatrix.MultiplyPoint3x4(path[i]);

        }

        Vector2[] path2 = shape.collider.GetPath(0);
        for (int i = 0; i < path2.Length; i++)
        {

            path2[i] = shape.transform.localToWorldMatrix.MultiplyPoint3x4(path2[i]);

        }


    }

    void OnTriggerStay2D(Collider2D stay) {

        if (!merging && stay.gameObject.layer == 1) {

            print("Merging");
            merging = true;
            stay.GetComponent<Shape>().merging = true;
            line_list.SetIntersections(stay.GetComponent<Shape>().line_list);
            Merge2(stay.GetComponent<Shape>());

        }

    }

}
