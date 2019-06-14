using UnityEngine;
using System.Collections;

public class CircleLineRenderer : MonoBehaviour {

    private int segments = 20;
    private float xradius;
    private float yradius;
    private LineRenderer lineRenderer;


    void Start() {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(segments + 1);
        lineRenderer.useWorldSpace = false;

        GameObject whiteBall = GameObject.Find("WhiteBall");

        xradius = whiteBall.GetComponent<SphereCollider>().radius * 0.95f;
        yradius = xradius;

        CreatePoints();

    }


    void CreatePoints() {
        float x;
        float y;
        float z = 0f;

        float angle = 20f;

        for (int i = 0; i < (segments + 1); i++) {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * xradius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * yradius;

            lineRenderer.SetPosition(i, new Vector3(x, y, z));

            angle += (360f / segments);
        }
    }


}
