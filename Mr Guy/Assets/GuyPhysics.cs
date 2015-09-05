using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GuyPhysics : MonoBehaviour
{
    public const float MOVE_ACCEL_GROUND = 100f;
    public const float MAX_HSPEED_GROUND = 9f;
    public const float MAX_HSPEED_AIR = 6f;
    public const float JUMP_POWER = 17f;
    public const float UNJUMP_FORCE = 80f;

    public const float JUMP_FORGIVENESS = 0.08f;

    private Rigidbody2D rigidbody;
    private Transform transform;

    private CircleCollider2D collider_feet;
    private BoxCollider2D collider_torso;

    private bool movingLeft, movingRight;

    private bool onGround;
    private bool rightTouchingGround, leftTouchingGround;
    private Vector2 currGroundDir;

    private bool jumping, jumpFlag;
    private float jumpForgiving;
    
	void Awake () 
    {
        rigidbody = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        collider_feet = transform.GetChild(0).GetChild(0).GetComponent<CircleCollider2D>();
        collider_torso = transform.GetChild(0).GetChild(1).GetComponent<BoxCollider2D>();
	}

    void Start()
    {
        rigidbody.drag = 0;
        
        onGround = OnGround();

        movingLeft = false;
        movingRight = false;
        jumping = false;
        jumpFlag = false;
        jumpForgiving = JUMP_FORGIVENESS;
    }
	
    void Update()
    {
        if (!onGround)
            jumpForgiving = Mathf.Max(0, jumpForgiving - Time.deltaTime);

        onGround = OnGround();
        if (onGround)
        {
            jumpForgiving = JUMP_FORGIVENESS;
            if (!jumping)
                jumpFlag = false;
        }
        collider_feet.sharedMaterial.friction = onGround ? 0.1f : 0f;

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

        if (Input.GetKey(KeyCode.W))
            Jump();
        else
            jumping = false;

        if (jumpFlag && !jumping && rigidbody.velocity.y > 0f)
        {
            rigidbody.AddForce(Vector2.down * UNJUMP_FORCE);
        }
    }

    public void MoveLeft()
    {
        movingLeft = true;
        if (rigidbody.velocity.x > -(onGround ? MAX_HSPEED_GROUND : MAX_HSPEED_AIR))
            rigidbody.AddForce(((leftTouchingGround || onGround) && currGroundDir.y / currGroundDir.x <= 0.9f ? -currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.left) * MOVE_ACCEL_GROUND);
    }

    public void MoveRight()
    {
        movingRight = true;
        if (rigidbody.velocity.x < (onGround ? MAX_HSPEED_GROUND : MAX_HSPEED_AIR))
            rigidbody.AddForce(((rightTouchingGround || onGround) && currGroundDir.y / currGroundDir.x >= -0.9f ? currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.right) * MOVE_ACCEL_GROUND);
    }

    public void StopMoving()
    {
        movingLeft = false;
        movingRight = false;
        rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, new Vector2(0, !onGround || jumping ? rigidbody.velocity.y : 0), (onGround ? 0.5f : 0.3f));
    }

    public void Jump()
    {
        if (jumpForgiving > 0 && !jumping)
        {
            jumpFlag = true;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, JUMP_POWER);
        }
        jumping = true;
    }

    public bool OnGround()
    {
        RaycastHit2D[] hitsLeft = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.left * 0.5f, Vector2.down, 0.9f);
        RaycastHit2D[] hitsMiddle = Physics2D.RaycastAll((Vector2)collider_feet.transform.position, Vector2.down, 1f);
        RaycastHit2D[] hitsRight = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.right * 0.5f, Vector2.down, 0.9f);
        RaycastHit2D[] hits = hitsMiddle.Union(hitsLeft).Union(hitsRight).ToArray<RaycastHit2D>();

        int groundCount = 0;
        float normals = float.MaxValue;
        leftTouchingGround = rightTouchingGround = false;
        currGroundDir = Vector2.right;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.gameObject.CompareTag("Ground")) // TODO: gonna need a way to unify tags of things we can stand on
            {
                groundCount++;
                if (hitsLeft.Contains(h))
                    leftTouchingGround = true;
                if (hitsRight.Contains(h))
                    rightTouchingGround = true;

                if (Mathf.Abs(h.normal.x / h.normal.y) < normals)
                {
                    normals = Mathf.Abs(h.normal.x / h.normal.y);
                    currGroundDir = new Vector2(h.normal.normalized.y, -h.normal.normalized.x);
                }
            }
        }
        return groundCount > 0 && normals < 1.6f;
    }
}
