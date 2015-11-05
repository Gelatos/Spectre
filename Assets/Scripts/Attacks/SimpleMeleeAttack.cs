using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class SimpleMeleeAttack : Attack {

	//_______________________________________________ [PUBLIC VARIABLES]

	[Serializable]
	public class AttackFrameData
	{
		public int activeFrame;
		public GameObject attackFrame;
	}

	public bool showAttackFrameData = true;
	public List<AttackFrameData> attackFrameData;
	
	//_______________________________________________ [PROTECTED VARIABLES]

	protected int[] attackFrames;
	
	//_______________________________________________ [SETUP]

#if UNITY_EDITOR
	
	public override void EditorGUISetup () {
		base.EditorGUISetup ();
		
		// add the attack name
		EditorGUILayout.BeginHorizontal ();
		GUIContent attackNameLabel = new GUIContent ("Attack Frame Data",
		                                             "Attack Frame Data\n" +
		                                             "The attack frames that activate on a frame in the animation.");
		showAttackFrameData = EditorGUILayout.Foldout (showAttackFrameData, attackNameLabel);
		if (GUILayout.Button ("+", GUILayout.Width (20), GUILayout.Height (14))) {
			attackFrameData.Add (new AttackFrameData ());
		}
		EditorGUILayout.EndHorizontal ();

		if (showAttackFrameData) {

			if (attackFrameData == null) {
				attackFrameData = new List<AttackFrameData>();
			}

			foreach (AttackFrameData afd in attackFrameData) {
				EditorGUILayout.BeginHorizontal ();

				int tempFrame = EditorGUILayout.IntField (afd.activeFrame, GUILayout.Width (50));
				if (afd.activeFrame != tempFrame) {
					afd.activeFrame = tempFrame;
				}

				GameObject tempAttackFrame = (GameObject)EditorGUILayout.ObjectField (afd.attackFrame, typeof (GameObject), true);
				if (afd.attackFrame != tempAttackFrame) {
					afd.attackFrame = tempAttackFrame;
				}

				if (GUILayout.Button ("-", GUILayout.Width (20), GUILayout.Height (14))) {
					attackFrameData.Remove (afd);
					break;
				}

				EditorGUILayout.EndHorizontal ();
			}
		}
	}

#endif
	
	//_______________________________________________ [ATTACK FUNCTIONS]
	
	public override void Activate () {
		base.Activate ();
	}
	
	public override void UpdateFrame () {
		
	}

}