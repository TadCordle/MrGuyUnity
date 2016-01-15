using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GuyPhysics : MonoBehaviour
{
    public const float MOVE_ACCEL_GROUND = 100f;
    public const float MAX_HSPEED_GROUND = 9f;
    public const float MOVE_ACCEL_AIR = 80f;
    public const float MAX_HSPEED_AIR = 6f;
    public const float MOVE_ACCEL_SWIMMING = 55f;
    public const float MAX_HSPEED_SWIMMING = 7f;
    public const float MOVE_ACCEL_SWING = 40f;
    public const float MAX_SWING_SPEED = 15f;

    public const float JUMP_POWER = 17f;
    public const float UNJUMP_FORCE = 80f;
    public const float JUMP_FORGIVENESS = 0.08f;

    public const float SWIM_POWER = 80f;
    public const float MAX_SWIM_VSPEED = 9f;
    public const float MAX_SWIM_TIME = 0.2f;

    public const float ROPE_CLIMBING_TIME = 0.1f;

    public bool HasWaterShoes { get; set; }

    public bool MovingLeft { get; set; }
    public bool MovingRight { get; set; }
    public bool Jumping { get; set; }
    public bool GrabbingRope { get; set; }
    public bool IsHoldingRope { get { return holdingRope; } }
    public bool ClimbingUp { get; set; }
    public bool ClimbingDown { get; set; }

    private bool swimming;
    private float swimTime;

    new private Rigidbody2D rigidbody;
    new private Transform transform;

    private CircleCollider2D collider_feet;
    private BoxCollider2D collider_torso;

    private bool onGround;
    private bool rightTouchingGround, leftTouchingGround;
    private Vector2 currGroundDir;
    private Vector2 currGroundVelocity;

    private bool jumping, jumpFlag;
    private float jumpForgiving;

    public GameObject ropeCollision;
    public HingeJoint2D ropeHinge;
    private Rigidbody2D targetRope;
    private bool holdingRope;
    private float ignoreRopeTime;
    private float climbTime;
    
	void Awake () 
    {
        ropeCollision = null;
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

        jumping = false;
        jumpFlag = false;
        jumpForgiving = JUMP_FORGIVENESS;
    }
	
    void Update()
    {
        if (!onGround)
            jumpForgiving = Mathf.Max(0, jumpForgiving - Time.deltaTime);

        onGround = OnGround();
        if (onGround || holdingRope)
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
            if (MovingRight)
                StopMoving();
            else
                MoveLeft();
        else if (MovingRight)
            MoveRight();
        else
            StopMoving();

        if (ClimbingUp)
            ClimbUpRope();
        if (ClimbingDown)
            ClimbDownRope();

        if (climbTime > 0f)
        {
            ropeHinge.connectedAnchor = ropeHinge.anchor = Vector2.Lerp(rigidbody.position - targetRope.position, Vector2.zero, climbTime);
            climbTime -= Time.deltaTime;
        }
        else
        {
            if (GrabbingRope && ignoreRopeTime <= 0f)
            {
                if (!holdingRope)
                {
                    holdingRope = GrabRope();
                }
            }
            else
            {
                ignoreRopeTime = 0f;
                ropeHinge.enabled = false;
                holdingRope = false;
            }
        }

        // TODO: Fix this
        if (ignoreRopeTime > 0f)
            ignoreRopeTime -= Time.deltaTime;

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

        if (jumpFlag && !jumping && rigidbody.velocity.y > 0f && !swimming && !holdingRope)
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
        if (!holdingRope)
        {
            float maxSpeed = onGround ? MAX_HSPEED_GROUND : (swimming ? MAX_HSPEED_SWIMMING : MAX_HSPEED_AIR);
            Vector2 actualDirection = (leftTouchingGround || onGround) && currGroundDir.y / currGroundDir.x <= 0.9f ? -currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.left;
            if (rigidbody.velocity.x > -maxSpeed)
                rigidbody.AddForce(actualDirection * (onGround ? MOVE_ACCEL_GROUND : MOVE_ACCEL_AIR));
        }
        else
        {
            if (rigidbody.velocity.y < 0 && rigidbody.velocity.x > -MAX_SWING_SPEED)
                rigidbody.AddForce(Vector2.left * MOVE_ACCEL_SWING);
        }
    }

    private void MoveRight()
    {
        if (!holdingRope)
        {
            float maxSpeed = onGround ? MAX_HSPEED_GROUND : (swimming ? MAX_HSPEED_SWIMMING : MAX_HSPEED_AIR);
            Vector2 actualDirection = (rightTouchingGround || onGround) && currGroundDir.y / currGroundDir.x >= -0.9f ? currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.right;
            if (rigidbody.velocity.x < maxSpeed)
                rigidbody.AddForce(actualDirection * (onGround ? MOVE_ACCEL_GROUND : MOVE_ACCEL_AIR));
        }
        else
        {
            if (rigidbody.velocity.y < 0 && rigidbody.velocity.x < MAX_SWING_SPEED)
                rigidbody.AddForce(Vector2.right * MOVE_ACCEL_SWING);
        }
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
            if (Mathf.Abs(currGroundDir.y / currGroundDir.x) <= 0.9f && !holdingRope)
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
            rigidbody.velocity = new Vector2(rigidbody.velocity.x /* * 3f*/, JUMP_POWER); // TODO: incorporate speed of platform
        }
        else if (swimming && (swimTime <= 0 && rigidbody.velocity.y <= 0 || rigidbody.velocity.y > 0) && rigidbody.velocity.y < MAX_SWIM_VSPEED)
        {
            rigidbody.AddForce(Vector2.up * SWIM_POWER);
        }

        if (holdingRope)
        {
            ignoreRopeTime = 0.5f;
            ropeHinge.enabled = false;
            holdingRope = false;
        }

        jumping = true;
    }

    private bool GrabRope()
    {
        if (!ropeCollision) return false;

        ropeHinge.connectedBody = ropeCollision.GetComponent<Rigidbody2D>();
        ropeHinge.anchor = Vector2.zero;
        ropeHinge.connectedAnchor = Vector2.zero;
        ropeHinge.enabled = true;
        rigidbody.mass = 100f;
        return true;
    }

    private void ClimbUpRope()
    {
        if (ropeHinge.enabled && climbTime <= 0f)
        {
            Rigidbody2D up = ropeHinge.connectedBody.GetComponent<RopeLink>().up;
            if (up != null)
            {
                climbTime = ROPE_CLIMBING_TIME;
                targetRope = up;
                ropeHinge.connectedBody = targetRope;
                ropeHinge.connectedAnchor = ropeHinge.anchor = rigidbody.position - targetRope.position;
            }
        }
    }

    private void ClimbDownRope()
    {
        if (ropeHinge.enabled && climbTime <= 0f)
        {
            Rigidbody2D down = ropeHinge.connectedBody.GetComponent<RopeLink>().down;
            if (down != null)
            {
                climbTime = ROPE_CLIMBING_TIME;
                targetRope = down;
                ropeHinge.connectedBody = targetRope;
                ropeHinge.connectedAnchor = ropeHinge.anchor = rigidbody.position - targetRope.position;
            }
        }
    }

    public bool OnGround()
    {
        if (rigidbody.mass != 1f)
            rigidbody.mass = 1f;
        RaycastHit2D[] hitsLeft = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.left * 0.5f, Vector2.down, 0.9f);
        RaycastHit2D[] hitsMiddle = Physics2D.RaycastAll((Vector2)collider_feet.transform.position, Vector2.down, 1f);
        RaycastHit2D[] hitsRight = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.right * 0.5f, Vector2.down, 0.9f);
        RaycastHit2D[] hits = hitsMiddle.Union(hitsLeft).Union(hitsRight).ToArray<RaycastHit2D>();

        int groundCount = 0;
        float normals = float.MaxValue;
        leftTouchingGround = rightTouchingGround = false;
        currGroundDir = Vector2.right;
        currGroundVelocity = Vector2.zero;
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
                    currGroundVelocity = h.rigidbody != null ? h.rigidbody.velocity : Vector2.zero;
                }
            }
        }
        return groundCount > 0 && normals < 1.6f;
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Water"))
        {
            if (rigidbody.mass != 1f)
                rigidbody.mass = 1f;
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
