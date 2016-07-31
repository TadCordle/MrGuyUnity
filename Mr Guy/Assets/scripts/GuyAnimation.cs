﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuyAnimation : MonoBehaviour
{
    public bool FacingLeft = false;
    public GameObject body, head, armLeftTop, armLeftBottom, handLeft, armRightTop, armRightBottom, handRight,
                      legLeftTop, legLeftBottom, footLeft, legRightTop, legRightBottom, footRight;

    private FixedJoint2D jnt_body;
    private HingeJoint2D jnt_head, jnt_armLeftTop, jnt_armLeftBottom, jnt_handLeft, jnt_armRightTop, jnt_armRightBottom, jnt_handRight,
                         jnt_legLeftTop, jnt_legLeftBottom, jnt_footLeft, jnt_legRightTop, jnt_legRightBottom, jnt_footRight;
    private Rigidbody2D rb_body, rb_head, rb_armLeftTop, rb_armLeftBottom, rb_handLeft, rb_armRightTop, rb_armRightBottom, rb_handRight,
                        rb_legLeftTop, rb_legLeftBottom, rb_footLeft, rb_legRightTop, rb_legRightBottom, rb_footRight;

    private Animator anim;
    private List<Joint2D> joints;
    private List<Rigidbody2D> rigidbodies;
    new private Transform transform;

	// Use this for initialization
	void Awake ()
    {
        transform = GetComponent<Transform>();
        anim = GetComponent<Animator>();
        joints = new List<Joint2D>();
        rigidbodies = new List<Rigidbody2D>();

        jnt_body = body.GetComponent<FixedJoint2D>();

        joints.Add(jnt_head = head.GetComponent<HingeJoint2D>());
        joints.Add(jnt_armLeftTop = armLeftTop.GetComponent<HingeJoint2D>());
        joints.Add(jnt_armLeftBottom = armLeftBottom.GetComponent<HingeJoint2D>());
        joints.Add(jnt_handLeft = handLeft.GetComponent<HingeJoint2D>());
        joints.Add(jnt_armRightTop = armRightTop.GetComponent<HingeJoint2D>());
        joints.Add(jnt_armRightBottom = armRightBottom.GetComponent<HingeJoint2D>());
        joints.Add(jnt_handRight = handRight.GetComponent<HingeJoint2D>());
        joints.Add(jnt_legLeftTop = legLeftTop.GetComponent<HingeJoint2D>());
        joints.Add(jnt_legLeftBottom = legLeftBottom.GetComponent<HingeJoint2D>());
        joints.Add(jnt_footLeft = footLeft.GetComponent<HingeJoint2D>());
        joints.Add(jnt_legRightTop = legRightTop.GetComponent<HingeJoint2D>());
        joints.Add(jnt_legRightBottom = legRightBottom.GetComponent<HingeJoint2D>());
        joints.Add(jnt_footRight = footRight.GetComponent<HingeJoint2D>());

        rigidbodies.Add(rb_body = body.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_head = head.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_armLeftTop = armLeftTop.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_armLeftBottom = armLeftBottom.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_handLeft = handLeft.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_armRightTop = armRightTop.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_armRightBottom = armRightBottom.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_handRight = handRight.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_legLeftTop = legLeftTop.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_legLeftBottom = legLeftBottom.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_footLeft = footLeft.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_legRightTop = legRightTop.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_legRightBottom = legRightBottom.GetComponent<Rigidbody2D>());
        rigidbodies.Add(rb_footRight = footRight.GetComponent<Rigidbody2D>());
    }

    public void SetPhysicsEnabled(bool enabled)
    {
        jnt_body.enabled = enabled;
        anim.Stop();
        foreach (Rigidbody2D rb in rigidbodies)
            rb.isKinematic = !enabled;
    }

    public void SetFacingLeft(bool newFacingLeft)
    {
        if (FacingLeft && !newFacingLeft)
        {
            FacingLeft = false;
            transform.parent.localScale = new Vector2(-transform.parent.localScale.x, transform.parent.localScale.y);
        }
        else if (!FacingLeft && newFacingLeft)
        {
            FacingLeft = true;
            transform.parent.localScale = new Vector2(-transform.parent.localScale.x, transform.parent.localScale.y);
        }
    }

    public void SetCrouch(bool crouch)
    {
        anim.SetBool("Crouched", crouch);
    }
}
