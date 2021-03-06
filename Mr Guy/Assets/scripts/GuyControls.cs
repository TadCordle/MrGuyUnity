﻿using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GuyControls : MonoBehaviour 
{
    private GuyPhysics guy;

	// Use this for initialization
	void Awake () 
    {
        guy = GetComponent<GuyPhysics>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        guy.MovingLeft = Input.GetKey(KeyCode.LeftArrow);
        guy.MovingRight = Input.GetKey(KeyCode.RightArrow);

        guy.Jumping = Input.GetKey(KeyCode.UpArrow) && !guy.IsHoldingRope || Input.GetKey(KeyCode.Z);
        guy.GrabbingRope = Input.GetKey(KeyCode.X);
        guy.Crouching = Input.GetKey(KeyCode.DownArrow) && !guy.IsHoldingRope || Input.GetKey(KeyCode.C);

        guy.ClimbingUp = Input.GetKey(KeyCode.UpArrow);
        guy.ClimbingDown = Input.GetKey(KeyCode.DownArrow);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // DEBUG
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetSceneAt(0).name);

        if (Input.GetKeyDown(KeyCode.K))
            guy.Die();
    }
}
