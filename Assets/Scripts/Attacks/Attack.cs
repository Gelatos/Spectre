using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class Attack : ScriptableObject
{
	//_______________________________________________ [PUBLIC VARIABLES]

	// animator
	public Animator anim;
	public MovementController movement;
	public int layer;
	
	// attack information
	public string attackName = "";
	public string attackID = "";
	public string animationStateName = "";
	public int cancelFrame = 0;
	public bool limitsMovement = true;

	// editor variables
	public bool showInEditor = true;

	//_______________________________________________ [PUBLIC VARIABLES]

	protected AnimationClip clip;
	protected bool isActive;
	
	//_______________________________________________ [SETUP]

	public Attack () {

	}
	
	public Attack (string name) {
		attackName = name;
	}

	public void SetUp (Animator animator, int animationLayer, MovementController movementController) {
		anim = animator;
		layer = animationLayer;
		movement = movementController;
	}

	public override string ToString ()
	{
		return attackName;
	}

#if UNITY_EDITOR

	public virtual void EditorGUISetup () {
		
		// add the attack name
		EditorGUILayout.BeginHorizontal ();
		GUIContent attackNameLabel = new GUIContent ("Attack Name",
		                                             "Attack Name\n" +
		                                             "The name of the attack. This is only used for readability purposes.");
		string tempattackName = EditorGUILayout.TextField (attackNameLabel, attackName);
		if (attackName != tempattackName) {

			// convenience functions to fill out attack id and animation state names
			if (attackID == attackName) {
				attackID = tempattackName;
			}
			if (animationStateName == attackName) {
				animationStateName = tempattackName;
			}
			attackName = tempattackName;
		}
		EditorGUILayout.EndHorizontal ();
		
		// add the attack id
		EditorGUILayout.BeginHorizontal ();
		GUIContent attackIDLabel = new GUIContent ("Attack ID",
		                                           "Attack ID\n" +
		                                           "An ID associated with this attack to help identify attacks in the combo system.\n\n" +
		                                           "This ID can be shared with multiple attacks to help give options when comboing.");
		attackID = EditorGUILayout.TextField (attackIDLabel, attackID);
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space ();
		
		// add the animation state name
		EditorGUILayout.BeginHorizontal ();
		GUIContent animationStateNameLabel = new GUIContent ("Anim State Name",
		                                                     "Animation State Name\n" +
		                                                     "The name of the animation state in the supplied animator to decide what animation to play.");
		animationStateName = EditorGUILayout.TextField (animationStateNameLabel, animationStateName);
		EditorGUILayout.EndHorizontal ();
		
		// add the cancel frame
		EditorGUILayout.BeginHorizontal ();
		GUIContent cancelFrameLabel = new GUIContent ("Cancel Frame",
		                                              "Cancel Frame\n" +
		                                              "The frame in the animation when the animation can be cancelled into a new action.");
		cancelFrame = EditorGUILayout.IntField (cancelFrameLabel, cancelFrame);
		EditorGUILayout.EndHorizontal ();
		
		// add the limits movement bool
		EditorGUILayout.BeginHorizontal ();
		GUIContent limitsMovementLabel = new GUIContent ("Limits Movement",
		                                                 "Limits Movement\n" +
		                                                 "Determine if the attacker can move while this attack is in effect.");
		cancelFrame = EditorGUILayout.IntField (limitsMovementLabel, cancelFrame);
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space ();

	}

	public void CopyComponents (Attack copiedAttack) {
		anim = copiedAttack.anim;
		layer = copiedAttack.layer;
		attackName = copiedAttack.attackName;
		animationStateName = copiedAttack.animationStateName;
		attackID = copiedAttack.attackID;
		cancelFrame = copiedAttack.cancelFrame;
		showInEditor = copiedAttack.showInEditor;
	}

#endif
	
	//_______________________________________________ [ATTACK FUNCTIONS]
	
	public virtual void Activate () {
		if (!isActive) {
			anim.SetLayerWeight (layer, 1);
			anim.Play (animationStateName, layer);
			isActive = true;
			movement.AddMovementBlocker (attackID);
		}

	}
	
	public virtual void UpdateFrame () {

	}

	public virtual void Deactivate () {
		anim.SetLayerWeight (layer, 0);
		isActive = false;
		movement.RemoveMovementBlocker (attackID);
	}
	
}
