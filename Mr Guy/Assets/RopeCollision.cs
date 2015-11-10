using UnityEngine;
using System.Collections;

public class RopeCollision : MonoBehaviour {

    public GuyPhysics guyPhysics;
    public GameObject guy;

	// Use this for initialization
	void Start () {

	}

    void OnTriggerEnter2D(Collider2D collider) 
    {
        guyPhysics.ropeCollision = collider.gameObject;
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        guyPhysics.ropeCollision = null;
    }
}
