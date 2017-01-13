using UnityEngine;
using System.Collections;

public class Editor : MonoBehaviour {

    public GameObject Draw_Toggle;

    int mode = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void DrawMode()
    {

        mode = 0;

        Draw_Toggle.gameObject.active = true;
        GetComponent<Drawer>().Enable();

    }
    public void SelectMode()
    {

        mode = 1;

        Draw_Toggle.gameObject.active = false;
        GetComponent<Drawer>().Disable();
    }

}
