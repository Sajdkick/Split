using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CreateShape : MonoBehaviour {

    // Use this for initialization
    void Start () {

        Vector3[] vertices = new Vector3[4];
        //Top Left
        vertices[0] = new Vector3(-1, 1, 0);
        //Top Right
        vertices[1] = new Vector3(1, 1, 0);
        //Bottom Right
        vertices[2] = new Vector3(1, -1, 0);
        //Bottom Left
        vertices[3] = new Vector3(-1, -1, 0);

    }


    // Update is called once per frame
    void Update () {

    }
}
