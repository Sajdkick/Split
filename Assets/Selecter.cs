using UnityEngine;
using System.Collections;

public class Selecter : MonoBehaviour {

    public float rotate_speed = 15;

	// Use this for initialization
	void Start () {



	}

    GameObject selected;
    Color old_color;
    Vector3 old_mousepos;
	// Update is called once per frame
	void Update () {

        if (isEnabled)
        {

            Select();
            if (selected)
            {

                if (Input.GetKeyDown(KeyCode.Delete))
                {

                    Destroy(selected);
                    selected = null;

                }
                else Move();

            }

        }

        old_mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    }

    public bool isEnabled = false;
    public void Enable()
    {

        isEnabled = true;

    }
    public void Disable()
    {

        isEnabled = false;
        Deselect();

    }
    void Select()
    {

        if (Input.GetMouseButtonDown(0))
        {

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit)
            {

                print("Hit!");

                if (hit.gameObject.layer != 1)
                {

                    if (hit.gameObject != selected)
                    {

                        Deselect();
                        Set_Selected(hit.gameObject);

                    }

                }

            }
            else Deselect();

        }

    }
    void Set_Selected(GameObject target)
    {

        old_color = target.GetComponent<Renderer>().material.color;
        target.GetComponent<Renderer>().material.color += Color.green * 0.5f;

        selected = target;

    }
    void Deselect()
    {

        if (selected)
        {

            selected.GetComponent<Renderer>().material.color = old_color;
            selected = null;

        }

    }

    void Move(){

        if (Input.GetMouseButton(0))
        {

            Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selected.transform.Translate(selected.transform.worldToLocalMatrix * (mousepos - old_mousepos));

        }

        selected.transform.RotateAround(selected.transform.localPosition, Vector3.forward, Input.GetAxis("Mouse ScrollWheel") * rotate_speed);

    }

}
