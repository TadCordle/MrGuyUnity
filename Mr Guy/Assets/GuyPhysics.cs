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

    public const float MAX_HSPEED_SWIMMING = 7f;
    public const float MOVE_ACCEL_SWIMMING = 65f;
    public const float SWIM_POWER = 80f;
    public const float MAX_SWIM_VSPEED = 10f;
    public const float MAX_SWIM_TIME = 0.2f;

    public bool HasWaterShoes { get; set; }

    public bool MovingLeft { get; set; }
    public bool MovingRight { get; set; }
    public bool Jumping { get; set; }

    private bool swimming;
    private float swimTime;

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
        swimTime = MAX_SWIM_TIME;
        
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
        rigidbody.gravityScale = 3f;
        if (MovingLeft)
            MoveLeft();
        else if (MovingRight)
            MoveRight();
        else
            StopMoving();

        if (onGround || swimming)
        {
            if (rigidbody.velocity.x < -MAX_HSPEED_GROUND)
                rigidbody.AddForce(Vector2.right * (swimming ? MOVE_ACCEL_SWIMMING : MOVE_ACCEL_GROUND) / 3f);
            else if (rigidbody.velocity.x > MAX_HSPEED_GROUND)
                rigidbody.AddForce(Vector2.left * (swimming ? MOVE_ACCEL_SWIMMING : MOVE_ACCEL_GROUND) / 3f);
        }

        if (!onGround && swimming)
        {   
            if (rigidbody.velocity.y < -MAX_SWIM_VSPEED)
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, -MAX_SWIM_VSPEED);
        }

        if (Jumping)
            Jump();
        else
            jumping = false;

        if (jumpFlag && !jumping && rigidbody.velocity.y > 0f && !swimming)
        {
            rigidbody.AddForce(Vector2.down * UNJUMP_FORCE);
        }

        if (swimming && swimTime > 0)
        {
            swimTime -= Time.deltaTime;
        }
    }

    private void MoveLeft()
    {
        if (rigidbody.velocity.x > -(onGround ? MAX_HSPEED_GROUND : (swimming ? MAX_HSPEED_SWIMMING : MAX_HSPEED_AIR)))
            rigidbody.AddForce(((leftTouchingGround || onGround) && currGroundDir.y / currGroundDir.x <= 0.9f ? -currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.left) * MOVE_ACCEL_GROUND);
    }

    private void MoveRight()
    {
        if (rigidbody.velocity.x < (onGround ? MAX_HSPEED_GROUND : (swimming ? MAX_HSPEED_SWIMMING : MAX_HSPEED_AIR)))
            rigidbody.AddForce(((rightTouchingGround || onGround) && currGroundDir.y / currGroundDir.x >= -0.9f ? currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.right) * MOVE_ACCEL_GROUND);
    }

    private void StopMoving()
    {
        if (onGround)
        {
            if (!jumping)
                rigidbody.gravityScale = 0f;
            if (Mathf.Abs(currGroundDir.y / currGroundDir.x) <= 0.9f)
                rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, new Vector2(0, jumping ? rigidbody.velocity.y : 0), 0.5f);
        }
        else
        {
            if (Mathf.Abs(currGroundDir.y / currGroundDir.x) <= 0.9f)
                rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, new Vector2(0, rigidbody.velocity.y), 0.3f);
            else
                rigidbody.AddForce(-currGroundDir * MOVE_ACCEL_GROUND / 10f);
        }
    }

    private void Jump()
    {
        if (jumpForgiving > 0 && !jumping)
        {
            jumpFlag = true;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x /* * 3f*/, JUMP_POWER);
        }
        else if (swimming && (swimTime <= 0 && rigidbody.velocity.y <= 0 || rigidbody.velocity.y > 0) && rigidbody.velocity.y < MAX_SWIM_VSPEED)
        {
            rigidbody.AddForce(Vector2.up * SWIM_POWER);
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

    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Water"))
        {
            if (!swimming)
                swimTime = MAX_SWIM_TIME;
            swimming = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Water"))
        {
            swimTime = MAX_SWIM_TIME;
            swimming = false;
        }
    }
}
