using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor (typeof(AttackController))]
public class AttackControllerEditor : Editor {

	//_______________________________________________ [PROTECTED VARIABLES]
	
	protected string pathToAttacks = "Assets/Scripts/Attacks/";

	protected List <string> AttackTypeList = new List<string> () {
		{"Simple Melee Attack"}
	};

	protected Type GetAttackType (string scriptName) {

		Attack atk = GetAttack (scriptName);
		if (atk != null) {
			Type output = atk.GetType ();
			ScriptableObject.DestroyImmediate (atk);
			return output;
		}

		return null;
	}

	protected Attack GetAttack (string scriptName) {

		switch (scriptName) {
		case "Simple Melee Attack":
			return ScriptableObject.CreateInstance <SimpleMeleeAttack>();
		}
		
		return null;
	}


	public override void OnInspectorGUI () {

		AttackController ac = target as AttackController;

		// ======================================= [Components and Animation]
		
		EditorGUILayout.BeginHorizontal ();
		GUIContent movementLabel = new GUIContent ("Movement Controller", 
		                                           "Movement Controller\n" +
		                                           "Movement controller of the attacker");
		ac.MovementControl = (MovementController)EditorGUILayout.ObjectField (movementLabel, ac.MovementControl, typeof(MovementController), true);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		GUIContent animatorLabel = new GUIContent ("Animator", 
		                                           "Animator\n" + 
		                                           "Animator component of the character.");
		ac.Anim = (Animator)EditorGUILayout.ObjectField (animatorLabel, ac.Anim, typeof(Animator), true);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		GUIContent animatorLayerLabel = new GUIContent ("Animation Layer",
		                                                "Animation Layer\n" +
		                                                "The layer the attack animations are on.");

		ac.AnimationLayer = EditorGUILayout.IntField (animatorLayerLabel, ac.AnimationLayer);
		EditorGUILayout.EndHorizontal ();

		// ======================================= [Attacks]

		// Section Label
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.LabelField ("Attacks");
		if (GUILayout.Button ("reset", GUILayout.Width (50), GUILayout.Height (16))) {
			ac.AttackDataList = new List<AttackController.AttackData> ();
		}
		EditorGUILayout.EndHorizontal ();

		GUIStyle attacksGroupStyle = new GUIStyle (GUI.skin.box);
		attacksGroupStyle.padding = new RectOffset (12, 1, 1, 1);
		
		EditorGUILayout.BeginVertical (attacksGroupStyle);
		
		// Attacks
		if (ac.AttackDataList == null) {
			ac.AttackDataList = new List<AttackController.AttackData> ();
		}

		// ======================================= [Create List of Attacks]

		if (ac.AttackDataList.Count != 0) {
			for (int i = 0; i < ac.AttackDataList.Count; i++) {
				AttackController.AttackData adata = CreateAttackListing (ac.AttackDataList[i]);
				if (adata == null) {
					ScriptableObject.DestroyImmediate (ac.AttackDataList[i].atk);
					ac.AttackDataList.RemoveAt (i);
					break;
				} else {
					ac.AttackDataList[i] = adata;
				}
			}
		}

		// ======================================= [Add Attack Button]

		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("+", GUILayout.Width (26), GUILayout.Height (20))) {
			
			// create new attack data
			AttackController.AttackData ad = new AttackController.AttackData (ScriptableObject.CreateInstance<SimpleMeleeAttack> ());
			
			// add it to the attack data list
			ac.AttackDataList.Add (ad);
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();

	}

	/// <summary>
	/// Creates the attack listing.
	/// </summary>
	/// <returns>The attack listing.</returns>
	/// <param name="atd">Atd.</param>
	protected AttackController.AttackData CreateAttackListing (AttackController.AttackData atd) {

		EditorGUILayout.BeginHorizontal ();
		atd.atk.showInEditor = EditorGUILayout.Foldout (atd.atk.showInEditor, atd.atk.attackName);
		if (GUILayout.Button ("-", GUILayout.Width (20), GUILayout.Height (14))) {
			ScriptableObject.DestroyImmediate (atd.atk);
			atd = null;
		}
		EditorGUILayout.EndHorizontal ();

		if (atd != null && atd.atk.showInEditor) {

			GUIStyle attacksStyle = new GUIStyle (GUI.skin.box);
			attacksStyle.padding = new RectOffset (12, 1, 5, 5);
			
			EditorGUILayout.BeginVertical (attacksStyle);

			// get all of the attack scripts
			int thisScriptIndex = 0;
			
			// get all of the attack scripts
			GUIContent attackScriptLabel = new GUIContent ("Attack Script",
			                                               "Attack Script\n" +
			                                               "The script the attack will be executing when the attack is used.");
			GUIContent[] attackScriptOptions = new GUIContent[AttackTypeList.Count];
			int index = 0;
			foreach (string key in AttackTypeList) {
				attackScriptOptions[index] = new GUIContent(key);
				if (atd.atk.GetType () == GetAttackType (key)) {
					thisScriptIndex = index;
				}
				index++;
			}

			// list the attack scripts
			int tempAttackScriptIndex = EditorGUILayout.Popup (attackScriptLabel, thisScriptIndex, attackScriptOptions);
			if (tempAttackScriptIndex != thisScriptIndex) {
				Attack tempAtk = Instantiate (atd.atk);
				Attack newAtk = GetAttack (attackScriptOptions[tempAttackScriptIndex].text);
				atd.atk = Instantiate (newAtk);
				atd.atk.CopyComponents (tempAtk);
				ScriptableObject.DestroyImmediate (tempAtk);
				ScriptableObject.DestroyImmediate (newAtk);
			}

			// add the attack's components
			atd.atk.EditorGUISetup ();

			// add the attack inputs
			GUIStyle attacksInputStyle = new GUIStyle (GUI.skin.box);
			attacksInputStyle.padding = new RectOffset (12, 1, 1, 5);

			EditorGUILayout.Space ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Activation Parameters");
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginVertical (attacksInputStyle);
			EditorGUILayout.BeginHorizontal ();
			GUIContent triggerLabel = new GUIContent ("Trigger",
			                                          "Trigger\n" +
			                                          "The option for a trigger that needs to happen in order for this attack to start.");
			AttackController.AttackTrigger tempTrigger = (AttackController.AttackTrigger) EditorGUILayout.EnumPopup (triggerLabel, atd.atkTrig);
			if (atd.atkTrig != tempTrigger) {
				atd.atkTrig = tempTrigger;
			}
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			GUIContent comboLabel = new GUIContent ("Combo Label",
			                                        "Combo Label\n" +
			                                        "If this attack is triggered after another attack, add it's ID here. Otherwise leave this section blank.");
			string tempComboID = EditorGUILayout.TextField (comboLabel, atd.comboStarterID);
			if (atd.comboStarterID != tempComboID) {
				atd.comboStarterID = tempComboID;
			}
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.BeginHorizontal ();
			
			// if there are no attack inputs
			if (atd.input.Count == 0) {
				AttackController.AttackInput ai = new AttackController.AttackInput ();
				ai.Pos = AttackController.AttackPositionState.Neutral;
				atd.input.Add (ai);
			}

			GUIContent posDirLabel = new GUIContent ("Position State and Input Directions",
			                                         "Position State\n" +
			                                         "The state the character's movement controller has to be in for this attack to activate.\n\n" +
			                                         "Input Direction\n" +
			                                         "The directional influence the attacker is using while in the selected position state.\n" +
			                                         "For a player this would be the joystick position when pressing the attack button.");
			EditorGUILayout.LabelField (posDirLabel);
			if (atd.input.Count < Enum.GetValues(typeof(AttackController.AttackPositionState)).Length) {
				if (GUILayout.Button ("+", GUILayout.Width (20), GUILayout.Height (14))) {
					AttackController.AttackInput ai = new AttackController.AttackInput ();
					if (atd.InputContains (AttackController.AttackPositionState.Neutral) == -1) {
						ai.Pos = AttackController.AttackPositionState.Neutral;
					} else if (atd.InputContains (AttackController.AttackPositionState.Dash) == -1) {
						ai.Pos = AttackController.AttackPositionState.Dash;
					} else if (atd.InputContains (AttackController.AttackPositionState.Air) == -1) {
						ai.Pos = AttackController.AttackPositionState.Air;
					}
					atd.input.Add (ai);
				}
			}
			EditorGUILayout.EndHorizontal ();

			// add the attack inputs
			for (int posIndex = 0; posIndex < atd.input.Count; posIndex++) {
				
				EditorGUILayout.BeginHorizontal ();
				GUIContent posStateLabel = new GUIContent ("Input",
				                                           "Input\n" +
				                                           "An input necessary to perform this attack.");
				atd.input[posIndex].showData = EditorGUILayout.Foldout (atd.input[posIndex].showData, posStateLabel);
				AttackController.AttackPositionState tempAttackPosition = (AttackController.AttackPositionState) EditorGUILayout.EnumPopup (atd.input[posIndex].Pos);
				if (atd.input[posIndex].Pos != tempAttackPosition && atd.InputContains (tempAttackPosition) == -1) {
					atd.input[posIndex].Pos = tempAttackPosition;
				}
				if (GUILayout.Button ("-", GUILayout.Width (20), GUILayout.Height (14))) {
					atd.input.RemoveAt (posIndex);
					break;
				}
				EditorGUILayout.EndHorizontal ();

				if (atd.input[posIndex].showData) {
					for (int dirIndex = 0; dirIndex < Enum.GetValues(typeof(AttackController.AttackDirection)).Length; dirIndex++) {
						EditorGUILayout.BeginHorizontal ();
						if (atd.input[posIndex].Dir.Contains ((AttackController.AttackDirection)dirIndex)) {
							bool tempDir = EditorGUILayout.ToggleLeft (((AttackController.AttackDirection)dirIndex).ToString (), true);
							if (!tempDir) {
								int dir = atd.input[posIndex].Dir.IndexOf ((AttackController.AttackDirection)dirIndex);
								atd.input[posIndex].Dir.RemoveAt (dir);
								break;
							}
						} else {
							bool tempDir = EditorGUILayout.ToggleLeft (((AttackController.AttackDirection)dirIndex).ToString (), false);
							if (tempDir) {
								atd.input[posIndex].Dir.Add ((AttackController.AttackDirection)dirIndex);
								break;
							}
						}
						EditorGUILayout.EndHorizontal ();
					}
				}

			}

			EditorGUILayout.EndVertical ();

			EditorGUILayout.EndVertical ();
		}

		return atd;
	}
}
