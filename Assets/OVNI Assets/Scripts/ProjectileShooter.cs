﻿using UnityEngine;
using System.Collections;

public class ProjectileShooter : MonoBehaviour {

    GameObject projectilePrefab;

    public float speed = 20;
	// Use this for initialization
	void Start () {
        projectilePrefab = Resources.Load("projectile") as GameObject;
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
        
            GameObject projectile = Instantiate(projectilePrefab) as GameObject;

            // Direction of the projectile
            Vector3 positions2 = Input.mousePosition;
            positions2.z = 3;
            Vector3 position = Camera.main.ScreenToWorldPoint(positions2);
            projectile.transform.position = position;

           // Velocity
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.velocity = Camera.main.transform.forward * speed;
        }

	
	}
}
