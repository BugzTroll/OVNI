using UnityEngine;
using System.Collections;

public class DrawLine : MonoBehaviour {


    private LineRenderer lineRenderer;

    private float counter;
    private float dist;

    public Transform planete1;
    public Transform planete2;

    public float lineDrawSpeed = 6f;


	// Use this for initialization
	void Start ()
    {

    }
	
	// Update is called once per frame
	void Update ()
    {
        Draw();
    }

    void Draw()
    {
        //if (GameManager.Instance.LevelProgression.Contains(GameManager.GameLevel.Planete1))
        //{
        //    lineRenderer = GetComponent<LineRenderer>();
        //    lineRenderer.SetPosition(0, planete1.position);
        //    lineRenderer.SetWidth(0.45f, 0.45f);
        //    dist = Vector3.Distance(planete1.position, planete2.position);

        //    if (counter < dist)
        //    {
        //        counter += 0.1f / lineDrawSpeed; // Vitesse
        //        float x = Mathf.Lerp(0, dist, counter);
        //        Vector3 pointA = planete1.position;
        //        Vector3 pointB = planete2.position;
        //        Vector3 pointAlongLine = x * Vector3.Normalize(pointB - pointA) + pointA;

        //        lineRenderer.SetPosition(1, pointAlongLine);
        //    }
        //}
    }
}
