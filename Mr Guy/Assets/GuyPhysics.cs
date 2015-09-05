using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GuyPhysics : MonoBehaviour 
{
    public const float MOVE_ACCEL_GROUND = 90f;
    public const float MAX_HSPEED_GROUND = 8f;
    public const float MAX_HSPEED_AIR = 5f;
    public const float JUMP_POWER = 15f;

    private Rigidbody2D rigidbody;
    private Transform transform;

    private CircleCollider2D collider_feet;
    private BoxCollider2D collider_torso;

    private bool onGround;
    private float currGroundNormal;

	// Use this for initialization
	void Awake () 
    {
        rigidbody = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        collider_feet = transform.GetChild(0).GetChild(0).GetComponent<CircleCollider2D>();
        collider_torso = transform.GetChild(0).GetChild(1).GetComponent<BoxCollider2D>();
	}

    void Start()
    {
        onGround = OnGround();
    }
	
	// Update is called once per frame
	void Update ()
    {
        onGround = OnGround();

        float closestRotation = transform.localRotation.eulerAngles.z;
        rigidbody.MoveRotation(Mathf.Lerp(closestRotation > 180 ? closestRotation - 360 : closestRotation, 0, 0.5f));
        if (Input.GetKey(KeyCode.A))
            MoveLeft();
        else if (Input.GetKey(KeyCode.D))
            MoveRight();
        else
            StopMoving();

        if (onGround)
        {
            if (rigidbody.velocity.x < -MAX_HSPEED_GROUND)
                rigidbody.AddForce(Vector2.right * MOVE_ACCEL_GROUND / 3f);
            else if (rigidbody.velocity.x > MAX_HSPEED_GROUND)
                rigidbody.AddForce(Vector2.left * MOVE_ACCEL_GROUND / 3f);
        }

        if (Input.GetKeyDown(KeyCode.W))
            Jump();
    }

    public void MoveLeft()
    {
        rigidbody.drag = 0;
        if (rigidbody.velocity.x > -(onGround ? MAX_HSPEED_GROUND : MAX_HSPEED_AIR))
            rigidbody.AddForce(Vector2.left * MOVE_ACCEL_GROUND);
    }

    public void MoveRight()
    {
        rigidbody.drag = 0;
        if (rigidbody.velocity.x < (onGround ? MAX_HSPEED_GROUND : MAX_HSPEED_AIR))
            rigidbody.AddForce(Vector2.right * MOVE_ACCEL_GROUND);
    }

    public void StopMoving()
    {
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, rigidbody.velocity - Vector2.right * rigidbody.velocity.x, 0.4f);
    }

    public void Jump()
    {
        if (onGround)
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, JUMP_POWER);
        }
    }

    public bool OnGround()
    {
        RaycastHit2D[] hitsLeft = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.left * 0.5f, Vector2.down, 0.9f);
        RaycastHit2D[] hitsMiddle = Physics2D.RaycastAll((Vector2)collider_feet.transform.position, Vector2.down, 1f);
        RaycastHit2D[] hitsRight = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.right * 0.5f, Vector2.down, 0.9f);
        RaycastHit2D[] hits = hitsMiddle.Union(hitsLeft).Union(hitsRight).ToArray<RaycastHit2D>();

        Debug.Log(hitsLeft.Length + "  " + hitsMiddle.Length + "  " + hitsRight.Length);

        int groundCount = 0;
        float normals = float.MaxValue;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.gameObject.CompareTag("Ground")) // TODO: gonna need a way to unify tags of things we can stand on
            {
                groundCount++;
                if (Mathf.Abs(h.normal.x / h.normal.y) < normals)
                {
                    normals = Mathf.Abs(h.normal.x / h.normal.y);
                    currGroundNormal = normals;
                }
            }
        }
        return groundCount > 0 && normals < 1.3f;
    }
}
