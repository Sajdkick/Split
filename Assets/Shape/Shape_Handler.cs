using UnityEngine;
using System.Collections.Generic;

public class Shape_Handler : MonoBehaviour {

	// Use this for initialization
	void Start () {

        gameObject.AddComponent<Rigidbody2D>();
        gameObject.layer = 1;
	
	}

    //Used for delaying the merge of newly split shapes.
    int lifetime = 0;
    int fadetime = 30;
    void Update()
    {

        if (transform.childCount == 0)
            Destroy(gameObject);

        lifetime++;

        for(int i = 0; i < transform.childCount; i++)
        {

            transform.GetChild(0).GetComponent<Renderer>().material.color = Color.green * (((float)lifetime * 0.5f) / fadetime) + Color.green * 0.5f;

        }

    }

    public void Split(Vector2 point3, Vector2 point4)
    {

        List<Shape> all_shapes = new List<Shape>();
        for (int i = 0; i < transform.childCount; i++)
        {

            List<Shape> new_shapes = transform.GetChild(i).GetComponent<Shape>().Split(point3, point4);

            for(int j = 0; j < new_shapes.Count; j++)
            {

                all_shapes.Add(new_shapes[j]);

            }

        }

        for(int i = 0; i < all_shapes.Count; i++)
        {

            for(int j = i; j < all_shapes.Count; j++)
            {

                if (all_shapes[i].collider.IsTouching(all_shapes[j].collider))
                {

                    all_shapes[i].transform.parent.GetComponent<Shape_Handler>().Merge(all_shapes[j]);

                }

            }

        }

    }

    public void Merge(Shape new_shape)
    {

        //Destroy(new_shape.transform.parent.gameObject);

        new_shape.transform.parent = transform;

    }

     void OnTriggerStay2D(Collider2D stay)
    {

        if (lifetime > fadetime && stay.gameObject.layer == 1 && stay.transform.parent.GetComponent<Shape_Handler>().lifetime > fadetime)
        {

            Merge(stay.GetComponent<Shape>());

        }

    }

}
