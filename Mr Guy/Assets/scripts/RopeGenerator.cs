using UnityEngine;
using System.Collections;

public class RopeGenerator : MonoBehaviour {

    public GameObject LengthIndicator;
    public Rigidbody2D parentSegment;
    public GameObject linkPrefab;

	// Use this for initialization
	void Awake ()
    {
        Transform transform = GetComponent<Transform>();
        int numSegments = (int)Mathf.Abs(LengthIndicator.transform.localPosition.y * 2f);
        parentSegment.GetComponent<HingeJoint2D>().connectedAnchor = transform.position + Vector3.up * 0.25f;
        Rigidbody2D previous = parentSegment;
        for (int i = 1; i < numSegments; i++)
        {
            GameObject next = GameObject.Instantiate(linkPrefab);
            next.GetComponent<Transform>().SetParent(transform);
            next.transform.localPosition = new Vector3(0f, -0.5f * i, 0f);
            next.GetComponent<HingeJoint2D>().connectedBody = previous;
            previous = next.GetComponent<Rigidbody2D>();
        }
	}
}
