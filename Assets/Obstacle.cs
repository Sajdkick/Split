using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour {

    MeshFilter meshFilter;
    PolygonCollider2D collider;

    // Use this for initialization
    void Awake() {

        meshFilter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<PolygonCollider2D>();

        gameObject.AddComponent<MeshRenderer>().material = (Material)Resources.Load("Obstacle_Material");
        gameObject.AddComponent<Rigidbody2D>().isKinematic = true;

        transform.parent = Editor.level.transform;
        tag = "Obstacle";

    }

    public static void CreateObstacle(Vector3[] vertices, bool sharp = false)
    {

        GameObject obstacle = new GameObject();
        obstacle.AddComponent<Obstacle>().SetMesh(vertices);

    }

    // Update is called once per frame
    void Update () {
	
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

    }

}
