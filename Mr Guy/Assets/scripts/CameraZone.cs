using UnityEngine;
using System.Collections;

public class CameraZone : MonoBehaviour 
{
    public Transform focalPoint;
    public float playerTrackingX;
    public float playerTrackingY;
    public float cameraZoom;

    private Transform player;
    private CameraFollow camera;

	// Use this for initialization
	void Start () 
    {
        camera = FindObjectOfType<CameraFollow>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
	}

    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            camera.playerTrackingX = playerTrackingX;
            camera.playerTrackingY = playerTrackingY;
            camera.target = focalPoint;
            camera.zoom = cameraZoom;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Player"))
        {
            camera.playerTrackingX = 0;
            camera.playerTrackingY = 0;
            camera.target = player;
            camera.zoom = CameraFollow.DEFAULT_ZOOM;
        }
    }
}
