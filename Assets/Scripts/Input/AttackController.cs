using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AttackController : MonoBehaviour {

	//_______________________________________________ [PUBLIC VARIABLES]

	public enum AttackPositionState
	{
		Neutral,
		Dash,
		Air
	}

	public enum AttackDirection
	{
		Neutral,
		Up,
		Down,
		Side
	}

	public enum AttackTrigger
	{
		Attack,
		Special
	}
	
	[Serializable]
	public class AttackInput 
	{
		public bool showData = true;
		public AttackPositionState Pos;
		public List<AttackDirection> Dir = new List<AttackDirection> ();
	}

	[Serializable]
	public class AttackData
	{
		public Attack atk;
		public string comboStarterID;
		public AttackTrigger atkTrig;
		public List<AttackInput> input;

		public AttackData (Attack newAtk) {
			atk = newAtk;
			atk.attackName = "New Attack";
			comboStarterID = "";
			atkTrig = AttackTrigger.Attack;
			input = new List<AttackInput> ();
		}

		public int InputContains (AttackPositionState pos) {

			for (int i = 0; i < input.Count; i++) {
				if (input[i].Pos == pos) {
					return i;
				}
			}

			return -1;
		}

	}

	//_______________________________________________ [PROTECTED VARIABLES]
	
	// Components
	[SerializeField][HideInInspector] protected MovementController movementControl;
	[SerializeField][HideInInspector] protected Animator anim;

	// Animation
	[SerializeField][HideInInspector] protected int animationLayer;
	protected bool isAttacking;
	protected bool canCancel;
	protected Attack currentAttack;

	// Attack data
	[SerializeField] [HideInInspector] public List<AttackData> AttackDataList;
	protected Dictionary<AttackPositionState, Dictionary<AttackDirection, Dictionary<AttackTrigger, Dictionary<string, Attack>>>> InputAttackData;

	// Frames
	protected float framesPerSec;
	protected float activeFrame;

	#region Getters and Setters

	public MovementController MovementControl {
		get {
			return movementControl;
		}
		set {
			movementControl = value;
		}
	}

	public Animator Anim {
		get {
			return anim;
		}
		set {
			anim = value;
		}
	}

	public int AnimationLayer {
		get {
			return animationLayer;
		}
		set {
			animationLayer = value;
		}
	}

	#endregion

	#region Mono Functions
	
	protected virtual void Awake () {
		
		// set components
		if (anim == null) {
			anim = GetComponent<Animator>();
		}
		if (movementControl == null) {
			movementControl = GetComponent<MovementController> ();
		}

		// create the input attack dictionary
		InputAttackData = new Dictionary<AttackPositionState, Dictionary<AttackDirection, Dictionary<AttackTrigger, Dictionary<string, Attack>>>> ();

		// add the attack states, directions, and triggers
		for (int atkState = 0; atkState < Enum.GetValues(typeof(AttackPositionState)).Length; atkState++) {
			InputAttackData.Add ((AttackPositionState)atkState, new Dictionary<AttackDirection, Dictionary<AttackTrigger, Dictionary<string, Attack>>> ());

			for (int atkDir = 0; atkDir < Enum.GetValues(typeof(AttackDirection)).Length; atkDir++) {
				InputAttackData[(AttackPositionState)atkState].Add ((AttackDirection)atkDir, new Dictionary<AttackTrigger, Dictionary<string, Attack>> ());

				for (int atkTrigger = 0; atkTrigger < Enum.GetValues(typeof(AttackTrigger)).Length; atkTrigger++) {
					InputAttackData[(AttackPositionState)atkState][(AttackDirection)atkDir].Add ((AttackTrigger)atkTrigger, new Dictionary<string, Attack> ());
				}
			}
		}

		// setup the attacks
		foreach (AttackData ad in AttackDataList) {
			ad.atk.SetUp (anim, animationLayer, movementControl);
		}
		
		// add the attacks from the Keyed Attack Data to the Input Attack Data
		for (int i = 0; i < AttackDataList.Count; i++) {
			
			// add the neutral inputs
			if (AttackDataList[i].input.Count != 0) {
				foreach (AttackInput ai in AttackDataList[i].input) {
					foreach (AttackDirection ad in ai.Dir) {
						InputAttackData[ai.Pos][ad][AttackDataList[i].atkTrig].Add (AttackDataList[i].comboStarterID, AttackDataList[i].atk);
					}
				}
			}
		}

	}

	protected virtual void Update () {

		if (!isAttacking) {
			return;
		}

		currentAttack.UpdateFrame ();

		// determine if the attack is complete
		if (!anim.GetCurrentAnimatorStateInfo (1).IsName (currentAttack.animationStateName)) {
			currentAttack.Deactivate ();
			FinishAttack ();
		}
	}

	#endregion

	#region Attacks
	
	public virtual void StartAttack (AttackDirection direction, AttackTrigger trigger) {

		// if we're attacking and can't cancel, then return
		if (isAttacking && !canCancel) {
			return;
		}

		// get the movement controller state
		AttackPositionState state;
		if (movementControl.isJumping || movementControl.isFalling) {
			state = AttackPositionState.Air;
		} else if (movementControl.isDashing) {
			state = AttackPositionState.Dash;
		} else {
			state = AttackPositionState.Neutral;
		}

		if (InputAttackData[state][direction][trigger].Count != 0) {

			if (isAttacking && InputAttackData[state][direction][trigger].ContainsKey (currentAttack.attackID)) {

				// go to the combo action this combination is associated with
				currentAttack = InputAttackData[state][direction][trigger][currentAttack.attackID];

			} else if (InputAttackData[state][direction][trigger].ContainsKey ("")) {

				// go to the non-combo action this combination is associated with
				currentAttack = InputAttackData[state][direction][trigger][""];

			} else {

				// there is no attack available for this attack, return
				return;
			}

			// start the attack
			isAttacking = true;
			canCancel = false;
			currentAttack.Activate ();
		}
	}

	protected void FinishAttack () {

		currentAttack = null;
		isAttacking = false;
		canCancel = false;
	}

	#endregion
}
