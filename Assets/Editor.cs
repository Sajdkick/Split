using UnityEngine;
using System.Collections.Generic;

public class Editor : MonoBehaviour {

    static public GameObject level;

    public GameObject Draw_Toggle;
    public float zoom_speed = 2;
    int mode = 0;

	// Use this for initialization
	void Start () {

        level = new GameObject("Level");

	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.R))
            LoadMainShape();

        CameraControls();

        if (Input.GetKeyDown(KeyCode.S))
        {

            SaveLevel("Last_Level");

        }
        if (Input.GetKeyDown(KeyCode.L))
        {

            LoadLevel("Last_Level.txt");

        }

    }

    public void DrawMode()
    {

        mode = 0;

        Draw_Toggle.gameObject.active = true;
        GetComponent<Drawer>().Enable();
        GetComponent<Selecter>().Disable();

    }
    public void SelectMode()
    {

        mode = 1;

        Draw_Toggle.gameObject.active = false;
        GetComponent<Drawer>().Disable();
        GetComponent<Selecter>().Enable();

    }

    public void CameraControls()
    {

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {

            EnableCameraMode();

        }
        if (Input.GetKey(KeyCode.LeftShift))
        {

            Camera.main.transform.Translate(Vector3.right * Input.GetAxis("Mouse X"));
            Camera.main.transform.Translate(Vector3.up * Input.GetAxis("Mouse Y"));
            Camera.main.orthographicSize += Input.GetAxis("Mouse ScrollWheel") * zoom_speed;

        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {

            DisableCameraMode();

        }

    }
    public void EnableCameraMode()
    {

        switch (mode)
        {

            case 0:
                GetComponent<Drawer>().isEnabled = false;
                break;
            case 1:
                GetComponent<Selecter>().isEnabled = false;
                break;

        }

    }
    public void DisableCameraMode()
    {

        switch (mode)
        {

            case 0:
                GetComponent<Drawer>().isEnabled = true;
                break;
            case 1:
                GetComponent<Selecter>().isEnabled = true;
                break;

        }

    }

    void SaveLevel(string name)
    {

        List<string> string_lines = new List<string>();
        for (int i = 0; i < level.transform.childCount; i++)
        {

            Vector2[] path = level.transform.GetChild(i).GetComponent<PolygonCollider2D>().GetPath(0);
            for (int j = 0; j < path.Length; j++)
            {

                Vector3 vertex = level.transform.GetChild(i).localToWorldMatrix.MultiplyPoint3x4(path[j]);

                string_lines.Add(vertex.x.ToString());
                string_lines.Add(vertex.y.ToString());

            }

            if(level.transform.GetChild(i).tag == "Obstacle")
            {

                string_lines.Add("O");
                string_lines.Add("O");

            }
            else if(level.transform.GetChild(i).tag == "Spike")
            {

                string_lines.Add("S");
                string_lines.Add("S");

            }

        }

        System.IO.File.WriteAllLines(name + ".txt", string_lines.ToArray());

    }
    void LoadLevel(string path)
    {

        System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo();
        format.NegativeSign = "-";
        format.NumberDecimalSeparator = ".";
        string[] string_lines = System.IO.File.ReadAllLines(path);


        List<Shape> loaded_shapes = new List<Shape>();
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < string_lines.Length / 2; i++)
        {

            //print(float.Parse(lines[i * 2]) + ":" + float.Parse(lines[i * 2 + 1]));

            if (string_lines[i * 2] == "O")
            {

                Obstacle.CreateObstacle(vertices.ToArray());
                vertices.Clear();

            }
            else if (string_lines[i * 2] == "S")
            {

                Spike.CreateSpike(vertices.ToArray());
                vertices.Clear();

            }
            else
            {

                vertices.Add(new Vector3(float.Parse(string_lines[i * 2]), float.Parse(string_lines[i * 2 + 1]), 0));

            }

        }

    }
    public void LoadMainShape()
    {

        GameObject[] old_shapes = GameObject.FindGameObjectsWithTag("Shape_Handler");

        foreach (GameObject go in old_shapes)
        {
            Destroy(go);
        }

        LoadShapeFromFile("main.txt");

    }
    void LoadShapeFromFile(string path)
    {

        System.Globalization.NumberFormatInfo format = new System.Globalization.NumberFormatInfo();
        format.NegativeSign = "-";
        format.NumberDecimalSeparator = ".";
        string[] string_lines = System.IO.File.ReadAllLines(path);


        List<Shape> loaded_shapes = new List<Shape>();
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < string_lines.Length / 2; i++)
        {

            //print(float.Parse(lines[i * 2]) + ":" + float.Parse(lines[i * 2 + 1]));

            if(string_lines[i * 2] == "X")
            {

                loaded_shapes.Add(Shape.CreateShape(vertices));
                vertices.Clear();

            }
            else
            {

                vertices.Add(new Vector3(float.Parse(string_lines[i * 2]), float.Parse(string_lines[i * 2 + 1]), 0));

            }

        }

        if(loaded_shapes.Count > 1)
        {
            Shape_Handler handler = loaded_shapes[0].transform.parent.GetComponent<Shape_Handler>();
            for(int i = 1; i < loaded_shapes.Count; i++)
            {

                handler.Merge(loaded_shapes[i]);

            }

        }

    }
    
}
