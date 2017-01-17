using UnityEngine;
using System.Collections.Generic;

public class Shape_Handler : MonoBehaviour {

    static int counter = 0;

    public int id;
    // Use this for initialization
    void Awake () {

        id = counter;
        counter++;

        gameObject.AddComponent<Rigidbody2D>();
        gameObject.layer = 1;
	
	}

    //Used for delaying the merge of newly split shapes.
    public int lifetime = 0;
    int fadetime = 30;

    void Update()
    {

        if (transform.childCount == 0)
            Destroy(gameObject);

        lifetime++;

        for(int i = 0; i < transform.childCount; i++)
        {

            transform.GetChild(i).GetComponent<Renderer>().material.color = Color.green * (((float)lifetime * 0.5f) / fadetime) + Color.green * 0.5f;

        }

    }

    public void Split(Vector2 point3, Vector2 point4)
    {

        lifetime = 0;

        List<Shape> all_shapes = new List<Shape>();
        for (int i = 0; i < transform.childCount; i++)
        {

            List<Shape> new_shapes = transform.GetChild(i).GetComponent<Shape>().Split(point3, point4);

            for(int j = 0; j < new_shapes.Count; j++)
            {

                all_shapes.Add(new_shapes[j]);

            }

        }

        for (int i = 0; i < all_shapes.Count; i++)
        {

            float dx = point4.x - point3.x;
            float dy = point4.y - point3.y;

            if (!isLeft(point3, point4, all_shapes[i].collider.bounds.center))
            {

                all_shapes[i].transform.Translate(new Vector2(dy, -dx) * 0.01f);

            }
            else all_shapes[i].transform.Translate(new Vector2(-dy, dx) * 0.01f);


        }

        for (int i = 0; i < all_shapes.Count; i++)
        {

            for (int j = 0; j < all_shapes.Count; j++)
            {

                //if (all_shapes[i].transform.parent != all_shapes[j].transform)
                {

                    if (all_shapes[i].Intersecting(all_shapes[j]))
                    {

                        //if (isLeft(point3, point4, all_shapes[i].collider.bounds.center) == isLeft(point3, point4, all_shapes[j].collider.bounds.center))
                        for (int x = 0; x < all_shapes[j].transform.parent.childCount; x++)
                        {

                            all_shapes[j].transform.parent.GetChild(x).transform.parent = all_shapes[i].transform.parent;

                        }

                    }

                }

            }

        }


    }
    public bool isLeft(Vector2 a, Vector2 b, Vector2 c)
    {
        return ((b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x)) > 0;
    }

    public void Merge(Shape new_shape)
    {

        //Destroy(new_shape.transform.parent.gameObject);

        List<Transform> children = new List<Transform>();
        for (int i = 0; i < new_shape.transform.parent.childCount; i++)
        {

            children.Add(new_shape.transform.parent.GetChild(i));

        }
        for (int i = 0; i < children.Count; i++)
        {

            children[i].parent = transform;

        }


    }

    void OnTriggerStay2D(Collider2D stay)
    {

        if (transform.parent != stay.transform.parent)
        {

            if (lifetime > fadetime && stay.gameObject.layer == 1)
            {

                Shape_Handler other_handler = stay.transform.parent.GetComponent<Shape_Handler>();

                if (other_handler.lifetime > fadetime && other_handler.id > id)
                    Merge(stay.GetComponent<Shape>());

            }

        }

    }

    //Saves the piece to a file.
    public void SaveToFile(string name)
    {

        List<string> string_lines = new List<string>();
        for (int i = 0; i < transform.childCount; i++)
        {

            Vector2[] path = transform.GetChild(i).GetComponent<PolygonCollider2D>().GetPath(0);
            for (int j = 0; j < path.Length; j++)
            {

                Vector3 vertex = transform.localToWorldMatrix.MultiplyPoint3x4(path[j]);

                string_lines.Add(vertex.x.ToString());
                string_lines.Add(vertex.y.ToString());

            }

            string_lines.Add("X");
            string_lines.Add("Y");

        }

        System.IO.File.WriteAllLines(name + ".txt", string_lines.ToArray());

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

}
