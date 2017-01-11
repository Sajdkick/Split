using UnityEngine;
using System.Collections;

public class Shape_Handler : MonoBehaviour {

	// Use this for initialization
	void Start () {

        gameObject.AddComponent<Rigidbody2D>();
        gameObject.layer = 1;
	
	}

    //Used for delaying the merge of newly split shapes.
    int lifetime = 0;
    void Update()
    {

        if (transform.childCount == 0)
            Destroy(gameObject);

        lifetime++;

    }

    public void Split(Vector2 point3, Vector2 point4)
    {

        for (int i = 0; i < transform.childCount; i++)
        {

            transform.GetChild(i).GetComponent<Shape>().Split(point3, point4);

        }

    }

    public void Merge(Shape new_shape)
    {

        //Destroy(new_shape.transform.parent.gameObject);

        new_shape.transform.parent = transform;

    }


    void OnTriggerEnter2D(Collider2D stay)
    {

        if (lifetime > 60 && stay.gameObject.layer == 1 && transform != stay.transform.parent.transform)
        {

            Merge(stay.GetComponent<Shape>());

        }

    }

}
