using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor (typeof(TileObject))]
public class TileObjectEditor : Editor {

	string newStateID = "";

	public override void OnInspectorGUI () {
		
		TileObject to = target as TileObject;

		Sprite defaultSprite = AssetDatabase.LoadAssetAtPath <Sprite> ("Assets/Sprites/Environment/Default/default.png");

		// start with the State IDs
		if (to.StateIDs.Count == 0) {
			to.StateIDs.Add ("default");
			to.CenterSprites.Add (defaultSprite);
			to.AboveSprites.Add (defaultSprite);
			to.BelowSprites.Add (defaultSprite);
			to.LeftSprites.Add (defaultSprite);
			to.RightSprites.Add (defaultSprite);
		}

		EditorGUILayout.BeginHorizontal ();
		GUIContent stateIdLabel = new GUIContent ("State IDs", "State IDs\nStates the terrain can be in.");
		EditorGUILayout.LabelField (stateIdLabel);
		EditorGUILayout.EndHorizontal ();

		// display the first one
		EditorGUILayout.BeginHorizontal ();
		EditorGUILayout.TextField (to.StateIDs[0]);
		EditorGUILayout.EndHorizontal ();

		// fill out the rest of the IDs
		for (int i = 1; i < to.StateIDs.Count; i++) {
			EditorGUILayout.BeginHorizontal ();
			to.StateIDs[i] = EditorGUILayout.TextField (to.StateIDs[i]);
			if (GUILayout.Button ("-", GUILayout.Width (50), GUILayout.Height (14))) {
				to.StateIDs.RemoveAt (i);
				to.CenterSprites.RemoveAt (i);
				to.AboveSprites.RemoveAt (i);
				to.BelowSprites.RemoveAt (i);
				to.LeftSprites.RemoveAt (i);
				to.RightSprites.RemoveAt (i);
				break;
			}
			EditorGUILayout.EndHorizontal ();
		}

		// add the "Add IDs" bar
		EditorGUILayout.BeginHorizontal ();
		GUIContent addStateID = new GUIContent ("Add State IDs", "Add a State ID by filling in the field here.");
		newStateID = EditorGUILayout.TextField (addStateID, newStateID);
		if (GUILayout.Button ("+", GUILayout.Width (50), GUILayout.Height (16))) {
			to.StateIDs.Add (newStateID);
			to.CenterSprites.Add (to.CenterSprites[0]);
			to.AboveSprites.Add (to.AboveSprites[0]);
			to.BelowSprites.Add (to.BelowSprites[0]);
			to.LeftSprites.Add (to.LeftSprites[0]);
			to.RightSprites.Add (to.RightSprites[0]);
			newStateID = "";
		}
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.Space ();

		// display each sprite type
		GUIStyle terrainGroupStyle = new GUIStyle (GUI.skin.box);
		terrainGroupStyle.padding = new RectOffset (12, 1, 1, 1);

		for (int i = 0; i < to.StateIDs.Count; i++) {
			EditorGUILayout.BeginVertical (terrainGroupStyle);

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("State: " + to.StateIDs[i]);
			EditorGUILayout.EndHorizontal ();


			
			EditorGUILayout.BeginHorizontal ();
			float labelWidth = 60;
			float textureLabelWidth = 150;
			float wOffset = 0;
			float hOffset = 0;
			Rect rt = GUILayoutUtility.GetRect(96, 0);

			// above sprite
			wOffset = (32 - to.AboveSprites[i].texture.width) * 0.5F;
			hOffset = 32 - to.AboveSprites[i].texture.height;
			EditorGUI.DrawPreviewTexture (new Rect(rt.x + labelWidth + textureLabelWidth + wOffset + 42, rt.y + hOffset, 
			                                       to.AboveSprites[i].texture.width, to.AboveSprites[i].texture.height), (to.AboveSprites[i].texture as Texture));
			// left sprite
			wOffset = 32 - to.LeftSprites[i].texture.width;
			hOffset = (32 - to.LeftSprites[i].texture.height) * 0.5F;
			EditorGUI.DrawPreviewTexture (new Rect(rt.x + labelWidth + textureLabelWidth + wOffset + 10, rt.y + 32 + hOffset, 
			                                       to.LeftSprites[i].texture.width, to.LeftSprites[i].texture.height), (to.LeftSprites[i].texture as Texture));
			// center sprite
			EditorGUI.DrawPreviewTexture (new Rect(rt.x + labelWidth + textureLabelWidth + 42, rt.y + 32, 
			                                       to.CenterSprites[i].texture.width, to.CenterSprites[i].texture.height), (to.CenterSprites[i].texture as Texture));
			// right sprite
			hOffset = (32 - to.RightSprites[i].texture.height) * 0.5F;
			EditorGUI.DrawPreviewTexture (new Rect(rt.x + labelWidth + textureLabelWidth + 74, rt.y + 32 + hOffset, 
			                                       to.RightSprites[i].texture.width, to.RightSprites[i].texture.height), (to.RightSprites[i].texture as Texture));
			// below sprite
			wOffset = (32 - to.BelowSprites[i].texture.width) * 0.5F;
			EditorGUI.DrawPreviewTexture (new Rect(rt.x + labelWidth + textureLabelWidth + wOffset + 42, rt.y + 64, 
			                                       to.BelowSprites[i].texture.width, to.BelowSprites[i].texture.height), (to.BelowSprites[i].texture as Texture));
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Center", GUILayout.Width (labelWidth));
			to.CenterSprites[i] = (Sprite)EditorGUILayout.ObjectField (to.CenterSprites[i], typeof (Sprite), false, GUILayout.Width (textureLabelWidth));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Above", GUILayout.Width (labelWidth));
			to.AboveSprites[i] = (Sprite)EditorGUILayout.ObjectField (to.AboveSprites[i], typeof (Sprite), false, GUILayout.Width (textureLabelWidth));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Below", GUILayout.Width (labelWidth));
			to.BelowSprites[i] = (Sprite)EditorGUILayout.ObjectField (to.BelowSprites[i], typeof (Sprite), false, GUILayout.Width (textureLabelWidth));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Left", GUILayout.Width (labelWidth));
			to.LeftSprites[i] = (Sprite)EditorGUILayout.ObjectField (to.LeftSprites[i], typeof (Sprite), false, GUILayout.Width (textureLabelWidth));
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Right", GUILayout.Width (labelWidth));
			to.RightSprites[i] = (Sprite)EditorGUILayout.ObjectField (to.RightSprites[i], typeof (Sprite), false, GUILayout.Width (textureLabelWidth));
			EditorGUILayout.EndHorizontal ();

			EditorGUILayout.EndVertical ();
		}
		
		base.OnInspectorGUI ();



	}
}
