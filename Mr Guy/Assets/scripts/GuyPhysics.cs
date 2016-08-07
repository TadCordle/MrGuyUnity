using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GuyPhysics : MonoBehaviour
{
    public const float MOVE_ACCEL_GROUND = 100f;
    public const float MAX_HSPEED_GROUND = 9f;
    public const float MOVE_ACCEL_AIR = 80f;
    public const float MAX_HSPEED_AIR = 7f;
    public const float MOVE_ACCEL_SWIMMING = 55f;
    public const float MAX_HSPEED_SWIMMING = 7f;
    public const float MOVE_ACCEL_SWING = 40f;
    public const float MAX_SWING_SPEED = 15f;
    public const float MOVE_ACCEL_ROLL = 13f;
    public const float MAX_ROLL_SPEED = 800f;

    public const float JUMP_POWER = 17f;
    public const float UNJUMP_FORCE = 80f;
    public const float JUMP_FORGIVENESS = 0.08f;

    public const float SWIM_POWER = 80f;
    public const float MAX_SWIM_VSPEED = 9f;
    public const float MAX_SWIM_TIME = 0.2f;

    public const float ROPE_CLIMBING_TIME = 0.1f;
    public const float MAX_IGNORE_ROPE_TIME = 0.3f;
    public static Vector2 ROPE_GRAB_OFFSET = Vector2.up * 1.2f;

    public bool HasWaterShoes { get; set; }

    public bool MovingLeft { get; set; }
    public bool MovingRight { get; set; }
    public bool Jumping { get; set; }
    public bool Crouching { get; set; }
    public bool GrabbingRope { get; set; }
    public bool IsHoldingRope { get { return holdingRope; } }
    public bool ClimbingUp { get; set; }
    public bool ClimbingDown { get; set; }
    public bool Dead { get { return dead; } }

    private bool swimming;
    private float swimTime;

    new private Rigidbody2D rigidbody;
    new private Transform transform;

    private CircleCollider2D collider_feet, collider_head;
    private BoxCollider2D collider_torso;

    private bool onGround;
    private bool rightTouchingGround, leftTouchingGround;
    private Vector2 currGroundDir;
    private Vector2 currGroundVelocity;
    private bool wasOnMovingPlatform;

    private bool jumping, jumpFlag;
    private float jumpForgiving;

    public bool crouched;
    private bool dead;

    public GameObject ropeCollision;
    public HingeJoint2D ropeHinge;
    private Rigidbody2D targetRope;
    private bool holdingRope;
    private float ignoreRopeTime;
    private float climbTime;
    private GuyAnimation guyAnimation;
    
	void Awake () 
    {
        ropeCollision = null;
        rigidbody = GetComponent<Rigidbody2D>();
        transform = GetComponent<Transform>();
        guyAnimation = GetComponentInChildren<GuyAnimation>();
        collider_feet = transform.GetChild(0).GetChild(0).GetComponent<CircleCollider2D>();
        collider_torso = transform.GetChild(0).GetChild(1).GetComponent<BoxCollider2D>();
        collider_head = transform.GetChild(0).GetChild(2).GetComponent<CircleCollider2D>();
	}

    void Start()
    {
        if (guyAnimation)
            guyAnimation.SetPhysicsEnabled(false);

        rigidbody.drag = 0;
        swimTime = MAX_SWIM_TIME;
        
        onGround = OnGround();
        jumpForgiving = JUMP_FORGIVENESS;
    }
	
    void Update()
    {
        // Tick coyote timer
        if (!onGround)
            jumpForgiving = Mathf.Max(0, jumpForgiving - Time.deltaTime);

        onGround = OnGround();
        if (onGround || holdingRope)
        {
            if (guyAnimation)
                guyAnimation.SetMidair(false, false);

            // You can jump again
            jumpForgiving = JUMP_FORGIVENESS;
            if (!jumping)
                jumpFlag = false;
        }
        else
        {
            if (guyAnimation)
                guyAnimation.SetMidair(rigidbody.velocity.y > 0, rigidbody.velocity.y <= 0);
        }

        // Make sure you can't stand/roll up walls
        collider_feet.sharedMaterial.friction = onGround ? 0.1f : 0f;
        if (crouched) collider_feet.sharedMaterial.friction *= 10000f;

        // Workaround to update friction because unity is bad
        collider_feet.enabled = false;
        collider_feet.enabled = true;

        // Stand straight up
        float closestRotation = transform.localRotation.eulerAngles.z;
        closestRotation = closestRotation > 180 ? closestRotation - 360 : closestRotation;
        if (!crouched && !dead)
            rigidbody.MoveRotation(Mathf.Lerp(closestRotation, 0, 0.5f));
        rigidbody.gravityScale = 1f;

        // Handle horizontal movement
        if (!dead)
        {
            if (MovingLeft)
                if (MovingRight)
                    StopMoving();
                else
                    MoveLeft();
            else if (MovingRight)
                MoveRight();
            else
                StopMoving();

            SetCrouch(Crouching);

            if (ClimbingUp)
                ClimbUpRope();
            if (ClimbingDown)
                ClimbDownRope();
            if (!ClimbingUp && !ClimbingDown && guyAnimation)
                guyAnimation.SetClimbDir(0);
        }

        // Disable upper colliders while crouching
        collider_head.isTrigger = collider_torso.isTrigger = crouched || Mathf.Abs(closestRotation) > 40f;
        
        if (climbTime > 0f)
        {
            // Climb to next rope segment
            ropeHinge.anchor = Vector2.Lerp(rigidbody.position - targetRope.position + ROPE_GRAB_OFFSET, Vector2.zero, climbTime);
            ropeHinge.connectedAnchor = Vector2.Lerp(rigidbody.position - targetRope.position, Vector2.zero, climbTime);
            climbTime -= Time.deltaTime;
        }
        else
        {
            // Attach to rope if grabbing
            if (GrabbingRope)
            {
                if (!holdingRope && ignoreRopeTime <= 0f)
                {
                    holdingRope = GrabRope();
                    if (!holdingRope)
                        UngrabRope();
                }
            }
            else
                UngrabRope();
        }

        if (ignoreRopeTime > 0f)
            ignoreRopeTime -= Time.deltaTime;

        // Decelerate to speed cap if not midair
        if (onGround || swimming)
        {
            if (rigidbody.velocity.x < -MAX_HSPEED_GROUND + (crouched ? 0 : currGroundVelocity.x))
                rigidbody.AddForce(Vector2.right * (swimming ? MOVE_ACCEL_SWIMMING : (crouched ? MOVE_ACCEL_GROUND / 5f : MOVE_ACCEL_GROUND)) / 3f);
            else if (rigidbody.velocity.x > MAX_HSPEED_GROUND + (crouched ? 0 : currGroundVelocity.x))
                rigidbody.AddForce(Vector2.left * (swimming ? MOVE_ACCEL_SWIMMING : (crouched ? MOVE_ACCEL_GROUND / 5f : MOVE_ACCEL_GROUND)) / 3f);
        }

        // Cap falling speed in water
        if (!onGround && swimming)
        {   
            if (rigidbody.velocity.y < -MAX_SWIM_VSPEED * (crouched ? 2.5f : 1f))
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, -MAX_SWIM_VSPEED * (crouched ? 2.5f : 1f));
        }

        if (Jumping && !dead)
        {
            Jump();
        }
        else
        {
            jumping = false;
            if (guyAnimation)
                guyAnimation.SetSwimming(false);
        }

        // Variable jump height
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
            if (guyAnimation)
                guyAnimation.SetFacingLeft(true);
            wasOnMovingPlatform = false;
            float maxSpeed = onGround ? MAX_HSPEED_GROUND - (crouched ? 0 : currGroundVelocity.x) : (swimming ? MAX_HSPEED_SWIMMING : MAX_HSPEED_AIR);
            Vector2 actualDirection = (leftTouchingGround || onGround) && currGroundDir.y / currGroundDir.x <= 0.9f ? -currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.left;
            if (crouched)
            {
                if (rigidbody.angularVelocity < MAX_ROLL_SPEED)
                    rigidbody.AddTorque(MOVE_ACCEL_ROLL);
                if (!onGround && rigidbody.velocity.x > -maxSpeed)
                    rigidbody.AddForce(actualDirection * MOVE_ACCEL_AIR);
            }
            else
            {
                if (onGround && guyAnimation)
                    guyAnimation.SetRunning(true);
                if (rigidbody.velocity.x > -maxSpeed)
                    rigidbody.AddForce(actualDirection * (onGround ? MOVE_ACCEL_GROUND : MOVE_ACCEL_AIR));
            }
        }
        else
        {
            if (rigidbody.velocity.x > 0 || rigidbody.velocity.y < 0 && rigidbody.velocity.x > -MAX_SWING_SPEED)
                rigidbody.AddForce(Vector2.left * MOVE_ACCEL_SWING);
        }
    }

    private void MoveRight()
    {
        if (!holdingRope)
        {
            if (guyAnimation)
                guyAnimation.SetFacingLeft(false);
            wasOnMovingPlatform = false;
            float maxSpeed = onGround ? MAX_HSPEED_GROUND + (crouched ? 0 : currGroundVelocity.x) : (swimming ? MAX_HSPEED_SWIMMING : MAX_HSPEED_AIR);
            Vector2 actualDirection = (rightTouchingGround || onGround) && currGroundDir.y / currGroundDir.x >= -0.9f ? currGroundDir * Mathf.Pow(currGroundDir.x, 4) : Vector2.right;
            if (crouched)
            {
                if (rigidbody.angularVelocity > -MAX_ROLL_SPEED)
                    rigidbody.AddTorque(-MOVE_ACCEL_ROLL);
                if (!onGround && rigidbody.velocity.x < maxSpeed)
                    rigidbody.AddForce(actualDirection * MOVE_ACCEL_AIR);
            }
            else
            {
                if (onGround && guyAnimation)
                    guyAnimation.SetRunning(true);
                if (rigidbody.velocity.x < maxSpeed)
                    rigidbody.AddForce(actualDirection * (onGround ? MOVE_ACCEL_GROUND : MOVE_ACCEL_AIR));
            }
        }
        else
        {
            if (rigidbody.velocity.x < 0 || rigidbody.velocity.y < 0 && rigidbody.velocity.x < MAX_SWING_SPEED)
                rigidbody.AddForce(Vector2.right * MOVE_ACCEL_SWING);
        }
    }

    private void StopMoving()
    {
        if (crouched)
            return;

        if (guyAnimation)
            guyAnimation.SetRunning(false);
        if (onGround)
        {
            if (!jumping && !crouched)
                rigidbody.gravityScale = 0f;
            if (Mathf.Abs(currGroundDir.y / currGroundDir.x) <= 0.9f)
                rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, new Vector2((crouched ? 0 : currGroundVelocity.x), jumping ? rigidbody.velocity.y : (crouched ? 0 : currGroundVelocity.y)), 0.5f);
        }
        else
        {
            if (!wasOnMovingPlatform)
            {
                if (Mathf.Abs(currGroundDir.y / currGroundDir.x) <= 0.9f && !holdingRope)
                    rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, new Vector2(0, rigidbody.velocity.y), 0.3f);
                else
                    rigidbody.AddForce(-currGroundDir * MOVE_ACCEL_GROUND / 10f);
            }
        }
    }

    private void Jump()
    {
        if (jumpForgiving > 0 && !jumping)
        {
            jumpFlag = true;
            rigidbody.velocity = new Vector2(rigidbody.velocity.x /* * 3f*/, JUMP_POWER + currGroundVelocity.y); // TODO: incorporate speed of platform
        }
        else if (swimming && (swimTime <= 0 && rigidbody.velocity.y <= 0 || rigidbody.velocity.y > 0) && rigidbody.velocity.y < MAX_SWIM_VSPEED && !crouched)
        {
            if (swimming && guyAnimation)
                guyAnimation.SetSwimming(true);
            rigidbody.AddForce(Vector2.up * SWIM_POWER);
        }
        else if (!swimming)
        {
            if (guyAnimation)
                guyAnimation.SetSwimming(false);
        }

        if (holdingRope && !jumping)
        {
            UngrabRope();
            ignoreRopeTime = MAX_IGNORE_ROPE_TIME;
        }

        jumping = true;
    }

    private bool GrabRope()
    {
        if (!ropeCollision || crouched || ignoreRopeTime > 0f) return false;

        if (guyAnimation)
            guyAnimation.SetHoldingRope(true);

        ropeHinge.connectedBody = ropeCollision.GetComponent<Rigidbody2D>();
        ropeHinge.anchor = ROPE_GRAB_OFFSET;
        ropeHinge.connectedAnchor = Vector2.zero;
        ropeHinge.enabled = true;
        rigidbody.mass = 100f;
        return true;
    }

    private void UngrabRope()
    {
        if (guyAnimation)
            guyAnimation.SetHoldingRope(false);
        ignoreRopeTime = 0f;
        ropeHinge.enabled = false;
        holdingRope = false;
        rigidbody.mass = 1f;
    }

    private void ClimbUpRope()
    {
        if (ropeHinge.enabled && climbTime <= 0f)
        {
            Rigidbody2D up = ropeHinge.connectedBody.GetComponent<RopeLink>().up;
            if (up != null)
            {
                if (guyAnimation)
                    guyAnimation.SetClimbDir(1);
                climbTime = ROPE_CLIMBING_TIME;
                targetRope = up;
                ropeHinge.connectedBody = targetRope;
                ropeHinge.anchor = rigidbody.position - targetRope.position + ROPE_GRAB_OFFSET;
                ropeHinge.connectedAnchor = rigidbody.position - targetRope.position;
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
                if (guyAnimation)
                    guyAnimation.SetClimbDir(-1);
                climbTime = ROPE_CLIMBING_TIME;
                targetRope = down;
                ropeHinge.connectedBody = targetRope;
                ropeHinge.anchor = rigidbody.position - targetRope.position + ROPE_GRAB_OFFSET;
                ropeHinge.connectedAnchor = rigidbody.position - targetRope.position;
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
                    wasOnMovingPlatform = currGroundVelocity.sqrMagnitude >= 0.01f;
                }
            }
        }
        return groundCount > 0 && normals < 1.6f;
    }

    public void SetCrouch(bool crouch)
    {
        if (crouch)
        {
            if (!crouched)
            {
                rigidbody.gravityScale = 1f;
                crouched = !holdingRope;
            }
        }
        else if (crouched)
        {
            crouched &= !CanStandUp();
            if (!crouched)
            {
                rigidbody.angularVelocity = 0f;
                if (onGround)
                    rigidbody.gravityScale = 0f;
            }
        }
        if (guyAnimation)
        {
            guyAnimation.SetCrouch(crouched);
        }
    }

    public void Die()
    {
        // TODO: Fix ragdoll when facing left
        MoveRight();

        dead = true;
        SetCrouch(false);
        UngrabRope();
        if (guyAnimation)
            guyAnimation.SetPhysicsEnabled(true);
    }

    public bool CanStandUp()
    {
        RaycastHit2D[] hitsLeft = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.left * 0.5f, Vector2.up, 1.4f);
        RaycastHit2D[] hitsMiddle = Physics2D.RaycastAll((Vector2)collider_feet.transform.position, Vector2.up, 1.5f);
        RaycastHit2D[] hitsRight = Physics2D.RaycastAll((Vector2)collider_feet.transform.position + Vector2.right * 0.5f, Vector2.up, 1.4f);
        RaycastHit2D[] hits = hitsMiddle.Union(hitsLeft).Union(hitsRight).ToArray<RaycastHit2D>();

        int ceilCount = 0;
        bool middleHit = false;
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider.gameObject.CompareTag("Ground")) // TODO: gonna need a way to unify tags of things we can stand on
            {
                ceilCount++;
                if (hitsMiddle.Contains(h))
                    middleHit = true;
            }
        }
        return ceilCount <= 1 && !middleHit;
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
