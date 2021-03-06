﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovementController : MonoBehaviour {
	
	//_______________________________________________ [PUBLIC VARIABLES]

	[HideInInspector] public bool isFacingRight = true;
	[HideInInspector] public bool isJumping = false;
	[HideInInspector] public bool isFalling = false;
	[HideInInspector] public bool isDashing = false;
	[HideInInspector] public bool grounded = false;
	public LayerMask alwaysGroundLayers;
	public int[] passThroughGroundLayers;
	public int[] ignoreGroundLayers;

	// force variables
	public float walkSpeedForce = 30.0f;
	public float maxWalkSpeed = 1.0f;
	public float dashSpeedForce = 60.0F;
	public float maxDashSpeed = 3.0F;
	public float jumpForce = 250.0f;
	
	// components
	public Transform groundCheck;
	public Transform[] groundSideChecks;
	
	//_______________________________________________ [PROTECTED VARIABLES]

	// movement variables
	protected float moveAxis = 0;
	protected bool startJump = false;
	protected bool hasJumped = true;
	protected bool canDash = false;
	protected int currentGroundLayer = 0;
	protected List<string> movementBlockers = new List<string> ();
	
	// components
	protected Animator animator;
	protected Rigidbody2D rbody;
	protected LayerMask layer;

	#region Mono Functions

	protected virtual void Awake () {

		Application.targetFrameRate = 60;
//		Time.timeScale = 0.1F;

		// set components
		animator = GetComponent<Animator>();
		rbody = GetComponent<Rigidbody2D>();
		layer = gameObject.layer;

		// set the ignored layers
		foreach (int i in passThroughGroundLayers) {
			Physics2D.IgnoreLayerCollision (gameObject.layer, i, false);
		}
	}

	protected virtual void Update () {

		// prevent the character's collider from tipping over
		transform.localEulerAngles = Vector3.zero;

		MovementAnimations ();

		GroundCheck ();
	}
	
	void FixedUpdate () {
		
		GroundCheck ();

		// Ignore collisions on pass through layers
		foreach (int i in passThroughGroundLayers) {
			if (currentGroundLayer == i) {
				Physics2D.IgnoreLayerCollision (gameObject.layer, i, false);
			} else {
				Physics2D.IgnoreLayerCollision (gameObject.layer, i, true);
			}
		}

		// Perform movement
		float maxSpeed = maxWalkSpeed;
		float force = walkSpeedForce;
		if (isDashing) {
			maxSpeed = maxDashSpeed;
			force = dashSpeedForce;
		}
//		if (moveAxis * rbody.velocity.x <= maxSpeed) {
//			rbody.AddForce (Vector2.right * moveAxis * force);
//		}
		
//		if (Mathf.Abs (rbody.velocity.x) > maxSpeed) {
		rbody.velocity = new Vector2 (moveAxis * maxSpeed, rbody.velocity.y);
//		}

		// Determine if we need to flip the character
		if (moveAxis > 0 && !isFacingRight) {
			Flip ();
		} else if (moveAxis < 0 && isFacingRight) {
			Flip ();
		}

		// Add force for jumping
		if (startJump) {
			animator.SetBool ("Jump", true);
			rbody.AddForce (new Vector2(0f, jumpForce));
			startJump = false;
		}
	}

	#endregion

	#region Animation Functions

	protected virtual void Flip () {

		isFacingRight = !isFacingRight;
		Vector3 scale = transform.localScale;
		scale.x *= -1;
		transform.localScale = scale;
	}

	protected virtual void MovementAnimations () {

		animator.SetBool ("Grounded", grounded);

		// determine if the jump has started by checking if enough force has been made
		if (!hasJumped && rbody.velocity.y > 1.0F) {
			hasJumped = true;
		}

		// animate jumping and falling
		if (grounded && !isJumping && isFalling) {
			isFalling = false;
			animator.SetBool ("Jump", false);
			animator.SetBool ("Fall", false);

		} else if (rbody.velocity.y < 1.0F) {

			if (hasJumped) {
				isJumping = false;
				animator.SetBool ("Jump", false);
			}
			if (!grounded) {
				isFalling = true;
				animator.SetBool ("Fall", true);
			}
		}
		
		// animate walking
		if (!isJumping && !isFalling && grounded && Mathf.Abs (rbody.velocity.x) > 0.1f) {
			animator.SetBool ("Walk", true);
		} else {
			animator.SetBool ("Walk", false);
		}

		// animate dash 
		animator.SetBool ("Dash", isDashing);
	}

	protected virtual void GroundCheck () {
		
		currentGroundLayer = 0;
		if (!hasJumped) {
			grounded = false;
		}

		// check if the character is grounded
		grounded = Physics2D.Linecast (transform.position, groundCheck.position, alwaysGroundLayers);
		if (!grounded) {
			foreach (Transform gc in groundSideChecks) {
				grounded = Physics2D.Linecast (groundCheck.position, gc.position, alwaysGroundLayers);
				if (grounded) {
					break;
				}
			}
			if (!grounded) {
				foreach (int i in passThroughGroundLayers) {
					grounded = Physics2D.Linecast (groundCheck.position, groundCheck.position, 1 << i);
					if (!grounded) {
						foreach (Transform gc in groundSideChecks) {
							grounded = Physics2D.Linecast (groundCheck.position, gc.position, alwaysGroundLayers);
							if (grounded) {
								break;
							}
						}
					}
					if (grounded) {
						currentGroundLayer = i;
						break;
					}
				}
			}
		}

		if (!grounded && Mathf.Abs (rbody.velocity.y) < 0.01F) {
			grounded = true;
		}
	}

	#endregion

	#region Action Functions

	public virtual void Jump () {

		if (grounded && movementBlockers.Count == 0) {
			startJump = true;
			isJumping = true;
			isFalling = false;
			hasJumped = false;
		}
	}
	
	public virtual void Dash () {
		canDash = true;
	}

	public virtual void Move (float axis) {

		if (grounded && movementBlockers.Count == 0) {
			moveAxis = axis;

			if (canDash && Mathf.Abs (rbody.velocity.x) >= (maxWalkSpeed * 0.5F)) {
				isDashing = true;
			} else {
				isDashing = false;
			}
		}

		// cancel the dash if the movement axis is too low.
		if (isDashing && !isJumping && Mathf.Abs (axis) < 0.05F) {
			isDashing = false;
		}

		canDash = false;
	}

	public void AddMovementBlocker (string id) {

		Vector2 newVel = new Vector2 (0.0F, rbody.velocity.y);
		rbody.velocity = newVel;
		movementBlockers.Add (id);
	}

	public void RemoveMovementBlocker (string id) {

		if (movementBlockers.Contains (id)) {
			movementBlockers.Remove (id);
		}
	}

	#endregion
}
