using UnityEngine;
using System.Collections;

public class GuyControls : MonoBehaviour 
{
    private GuyPhysics guy;

	// Use this for initialization
	void Start () 
    {
        guy = GetComponent<GuyPhysics>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        guy.MovingLeft = Input.GetKey(KeyCode.A);
        guy.MovingRight = Input.GetKey(KeyCode.D);

        guy.Jumping = Input.GetKey(KeyCode.W);

        if (Input.GetKeyDown(KeyCode.R))
            Application.LoadLevel(Application.loadedLevel);

        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
	}
}
