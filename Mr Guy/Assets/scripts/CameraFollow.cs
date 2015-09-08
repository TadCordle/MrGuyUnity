using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
    public const float DEFAULT_ZOOM = -20f;

    public Transform player;
    public Transform target;
    public float playerTrackingX;
    public float playerTrackingY;
    public float zoom;

    private Transform transform;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        playerTrackingX = 0f;
        playerTrackingY = 0f;
        zoom = DEFAULT_ZOOM;
        transform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector3 midpoint = (Vector3.right * playerTrackingX * player.position.x + Vector3.up * playerTrackingY * player.position.y) +
                           (Vector3.right * (1 - playerTrackingX) * target.position.x + Vector3.up * (1 - playerTrackingY) * target.position.y) + 
                           Vector3.forward * zoom;
        transform.position = Vector3.Lerp(transform.position, midpoint, 0.1f);
	}
}
