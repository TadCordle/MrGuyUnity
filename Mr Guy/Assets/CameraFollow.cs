using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
    public const float DEFAULT_ZOOM = -20f;

    public Transform player;
    public Transform target;
    public float playerTracking;
    public float zoom;

    private Transform transform;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        playerTracking = 0f;
        zoom = DEFAULT_ZOOM;
        transform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector3 midpoint = (2 * playerTracking * player.position + 2 * (1 - playerTracking) * target.position) / 2f + Vector3.forward * zoom;
        transform.position = Vector3.Lerp(transform.position, midpoint, 0.1f);
	}
}
