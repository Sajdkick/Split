using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shape : MonoBehaviour {

    MeshFilter meshFilter;
    PolygonCollider2D collider;
    PolygonCollider2D trigger;

    static int id = 0;
    static List<GameObject> shapeList = new List<GameObject>();

    // Use this for initialization
    void Awake() {

        meshFilter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<PolygonCollider2D>();

        gameObject.AddComponent<MeshRenderer>();

        gameObject.name = id.ToString();
        id++;

        GameObject parent = new GameObject();
        parent.AddComponent<Shape_Handler> ();

        gameObject.transform.parent = parent.transform;
        gameObject.layer = 1;

        shapeList.Add(gameObject);

    }

    void Start()
    {

        AddTrigger();

    }

    //Used for delaying the merge of newly split shapes.
    int lifetime = 0;
    void Update()
    {

        lifetime++;

    }

    public static void CreateShape(List<Vector3> path)
    {
        GameObject shape = new GameObject();
        shape.AddComponent<Shape>().SetPath(path.ToArray());
    }

    //Set the shapes path.
    void SetPath(Vector3[] vertices)
    {

        //Get the path in 2d points.
        int nrOfPoints = vertices.Length;
        Vector2[] path = new Vector2[nrOfPoints];
        for (int i = 0; i < nrOfPoints; i++)
            path[i] = new Vector2(vertices[i].x, vertices[i].y);

        //Create the collider for the shape.
        collider.SetPath(0, path);

        //Create the mesh for the shape.
        Triangulator triangulator = new Triangulator(collider.points);
        int[] triangles = triangulator.Triangulate();
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = triangles;



    }

    //Creates an identical trigger to the collider, used for merge checks.
    void AddTrigger()
    {

        GameObject child = new GameObject();
        child.transform.SetParent(transform);

        PolygonCollider2D trigger = child.AddComponent<PolygonCollider2D>();
        trigger.isTrigger = true;
        trigger.SetPath(0, collider.GetPath(0));

    }

    //Gets a shape from the global shape list.
    public static GameObject GetShape(int index)
    {

        return shapeList[index];

    }

    //Does a split check agains all shapes.
    public static void SplitAll(Vector2 point3, Vector2 point4)
    {

        //int nrOfShapes = shapeList.Count;
        //int count = 0;
        //for(int i = 0; i < nrOfShapes; i++)
        //{

        //    if(shapeList[i - count].GetComponent<Shape>().Split(point3, point4))
        //    {

        //        count++;

        //    }

        //}

    }

    //Split the shape.
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

        //We create the new shapes and merge the new pieces if they overlap.
        List<Shape> new_shapes = PuzzleMaster(vertexLists, orderedSplits, voids);

        //GameObject shape = new GameObject();
        //shape.AddComponent<Shape>().SetMesh(vertexLists[i].ToArray());
        shapeList.Remove(this.gameObject);
        Destroy(this.gameObject);

        return true;

    }

    //Gets the shape from a path.
    private Vector2[] GetShapePath(Shape shape)
    {

        //Get the paths from the shape.
        Vector2[] path = shape.collider.GetPath(0);
        for (int i = 0; i < path.Length; i++)
        {

            //Converts from local to world space.
            path[i] = shape.transform.localToWorldMatrix.MultiplyPoint3x4(path[i]);

        }

        return path;

    }

    //Sews together pieces and voids and creates shapes.
    List<Shape> PuzzleMaster(List<List<Vector3>> pieces, List<Vector3> splits, List<List<Vector3>> voids)
    {

        List<Shape> new_shapes = new List<Shape>();

        for(int i = 0; i < pieces.Count; i++)
        {

            List<Vector3> piece = pieces[i];

            //A list containing the splits for the current piece
            List<int> matches = GetSplits(piece, splits);
            
            //If there's only one split, we can turn the piece into a shape directly.
            if (matches.Count == 1)
            {

                GameObject shape = new GameObject();
                shape.AddComponent<Shape>().SetPath(piece.ToArray());
                new_shapes.Add(shape.GetComponent<Shape>());

            }
            else
            {

                //We check for a simple piece that contains no voids.
                //If the two splits in the piece are after one another.
                if (matches[1] - matches[0] == 1)
                {
                    GameObject shape = new GameObject();
                    shape.AddComponent<Shape>().SetPath(piece.ToArray());
                    new_shapes.Add(shape.GetComponent<Shape>());
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
                    shape.AddComponent<Shape>().SetPath(piece.ToArray());
                    new_shapes.Add(shape.GetComponent<Shape>());

                }

            }

        }

        return new_shapes;

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

    //Draws a piece, used for debugging.
    void DrawPiece(Vector3[] piece, Color color) {

        for (int i = 0; i < piece.Length - 1; i++) {

            Debug.DrawLine(piece[i], piece[i + 1], color * ((i + 1)/(float)piece.Length), 10);

        }

    }

    void DrawPoint(Vector3 point, Color color)
    {

        Debug.DrawLine(point - Vector3.right * 0.1f, point + Vector3.right * 0.1f, color, 10);
        Debug.DrawLine(point - Vector3.up * 0.1f, point + Vector3.up * 0.1f, color, 10);

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

        CreateShape(vertices);

    }

}
