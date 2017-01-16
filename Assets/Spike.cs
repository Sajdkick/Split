using UnityEngine;
using System.Collections;

public class Spike : MonoBehaviour {

    MeshFilter meshFilter;
    PolygonCollider2D collider;

    Vector3 direction;

    // Use this for initialization
    void Awake() {

        meshFilter = gameObject.AddComponent<MeshFilter>();
        collider = gameObject.AddComponent<PolygonCollider2D>();

        gameObject.AddComponent<MeshRenderer>();
        gameObject.AddComponent<Rigidbody2D>().isKinematic = true;

        transform.parent = Editor.level.transform;
        tag = "Spike";

    }

    // Update is called once per frame
    void Update () {
	
	}

    public static void CreateSpike(Vector3[] vertices) {

        GameObject spike = new GameObject();
        spike.AddComponent<Spike>().SetMesh(vertices);

        Vector2[] path = new Vector2[3];
        for (int i = 0; i < 3; i++)
            path[i] = new Vector2(vertices[i].x, vertices[i].y);

        Vector3 point1 = vertices[0];
        Vector3 point2 = vertices[1];
        Vector3 point3 = vertices[2];

        Vector3 direction = (point3 - point1);
        direction = ((point2 - point1).normalized + (point2 - point3).normalized).normalized;

        spike.GetComponent<Spike>().direction = direction;

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
        GetComponent<MeshRenderer>().material = (Material)Resources.Load("Spike_Material");

        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = (Material)Resources.Load("Spike_Material2");
        lineRenderer.SetColors(Color.black, Color.black);
        lineRenderer.SetWidth(0.2F, 0.2F);
        lineRenderer.SetVertexCount(2);

        lineRenderer.SetPosition(0, vertices[0] - Vector3.forward);
        lineRenderer.SetPosition(1, vertices[2] - Vector3.forward);

    }

    Vector3 GetSpikePoint()
    {

        return transform.localToWorldMatrix.MultiplyPoint3x4(meshFilter.mesh.vertices[1]);

    }

    void OnCollisionEnter2D(Collision2D coll)
    {

        ContactPoint2D[] contactPoints = coll.contacts;

        bool split = false;
        for (int i = 0; i < contactPoints.Length && !split; i++)
        {

            if (Vector3.Distance(contactPoints[i].point, GetSpikePoint()) < 0.1f)
                split = true;

        }

        if (split && coll.relativeVelocity.magnitude > 1.0f)
        {

            coll.gameObject.GetComponent<Shape_Handler>().Split(GetSpikePoint(), GetSpikePoint() + direction);

        }

    }

    void OnCollisionStay2D(Collision2D coll)
    {

        ContactPoint2D[] contactPoints = coll.contacts;

        bool split = false;
        for (int i = 0; i < contactPoints.Length && !split; i++)
        {

            if (Vector3.Distance(contactPoints[i].point, GetSpikePoint()) < 0.1f)
                split = true;

        }

        if (split && coll.relativeVelocity.magnitude > 2.0f)
        {

            coll.gameObject.GetComponent<Shape_Handler>().Split(GetSpikePoint(), GetSpikePoint() + direction);

        }

    }

}
