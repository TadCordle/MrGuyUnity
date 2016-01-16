using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
    public const float DEFAULT_ZOOM = -20f;

    private GameObject player;
    private GuyPhysics playerPhysics;
    private Transform playerTransform;
    private Transform playerFeetTransform;
    public GameObject target;
    public float playerTrackingX;
    public float playerTrackingY;
    public float zoom;

    new private Transform transform;

	// Use this for initialization
	void Start () 
    {
        player = GameObject.Find("Player");
        playerTransform = player.GetComponent<Transform>();
        playerFeetTransform = playerTransform.GetChild(0).GetChild(0).GetComponent<Transform>();
        playerPhysics = player.GetComponent<GuyPhysics>();
        target = player;
        playerTrackingX = 0f;
        playerTrackingY = 0f;
        zoom = DEFAULT_ZOOM;
        transform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () 
    {
        Transform t1 = playerPhysics.crouched ? playerFeetTransform : playerTransform;
        Transform t2 = target == player ? t1 : target.transform;
        Vector3 midpoint = (Vector3.right * playerTrackingX * t1.position.x + Vector3.up * playerTrackingY * t1.position.y) +
                           (Vector3.right * (1 - playerTrackingX) * t2.position.x + Vector3.up * (1 - playerTrackingY) * t2.position.y) + 
                           Vector3.forward * zoom;
        transform.position = Vector3.Lerp(transform.position, midpoint, 0.1f);
	}
}
