using UnityEngine;
using System.Collections;

public class MovingPlatform : MonoBehaviour
{
    public Transform point1, point2;
    public float MaxSpeed = 5f;
    public float SlowDownDist = 5f;
    public float WaitTime = 1f;

    private Vector2 pos1, pos2;
    private int cycle;
    private float waitTime;
    new private Rigidbody2D rigidbody;

	// Use this for initialization
	void Start ()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        pos1 = point1.position;
        pos2 = point2.position;
        cycle = 1;
        waitTime = WaitTime;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        Vector2 targetPos = (cycle == 1 ? pos2 : pos1);
        float targetDist = (targetPos - rigidbody.position).magnitude;
        Vector2 dir = (cycle == 1 ? (pos2 - pos1) : (pos1 - pos2)).normalized;

        if (waitTime < 0f)
        {
            if (targetDist > SlowDownDist)
            {
                if (SlowDownDist > 0)
                {
                    Vector2 accel = MaxSpeed * MaxSpeed / (2 * SlowDownDist) * dir;
                    rigidbody.velocity += accel * Time.fixedDeltaTime;
                }
                else
                    rigidbody.velocity = MaxSpeed * dir;
            }
            else
            {
                Vector2 decel = rigidbody.velocity.sqrMagnitude / (2 * targetDist) * dir;
                rigidbody.velocity -= decel * Time.fixedDeltaTime;
            }

            if (rigidbody.velocity.magnitude > MaxSpeed)
                rigidbody.velocity = rigidbody.velocity.normalized * MaxSpeed;
        }
        else
        {
            rigidbody.velocity = Vector2.zero;
            waitTime -= Time.fixedDeltaTime;
        }

        if (targetDist <= 0.01f)
        {
            cycle = (cycle == 1 ? 2 : 1);
            waitTime = WaitTime;
            rigidbody.velocity = Vector2.zero;
        }
	}
}
