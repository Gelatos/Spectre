using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TileTool : EditorWindow {

	//_______________________________________________ [PROTECTED VARIABLES]
	
	protected enum EditType
	{
		Select,
		Paint, // for tile laying
		Erase // for deletion
	}
	
	protected enum EditMode
	{
		Brush,
		Box
	}

	protected class EditHistory
	{
		TileObject newTileObject;
		TileObject oldTileObject;
		Vector3 location;
	}

	// Determine editing capabilities
	protected EditType editType = EditType.Select;
	protected EditMode editMode = EditMode.Brush;
	protected bool isVisible = false;
	protected Vector2 scrollPos;

	// Editing
	protected int selectedTerrainCategory;
	protected TileObject selectedTile;
	protected Transform tileParent;
	protected static Dictionary <int, Dictionary <int, TileObject>> tileGrid = new Dictionary<int, Dictionary<int, TileObject>> ();
	protected int spriteLayerOrder = 0;


	//_______________________________________________ [GUI FUNCTIONS]
	
	protected void OnGUI () {

		// set this script to visible
		if (!isVisible) {
			if (tileParent != null) {
				InitTileGrid ();
			}
		}
		isVisible = true;


		
		// ______________________________________ [ TILE OPTIONS ]

		GUILayout.Label("Tile Settings", EditorStyles.boldLabel);
		GUILayout.BeginVertical("box");
		
		EditorGUILayout.BeginHorizontal ();
		Transform tempTerrainHolder = (Transform) EditorGUILayout.ObjectField ("Tile Parent", tileParent, typeof (Transform), true);
		if (tileParent != tempTerrainHolder) {
			tileParent = tempTerrainHolder;
			tileParent.position = Vector3.zero;
			if (tileParent != null) {
				InitTileGrid ();
			}
		}
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		spriteLayerOrder = EditorGUILayout.IntField ("Sprite Order Layer", spriteLayerOrder);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Refresh Tiles") && tileParent != null) {
			InitTileGrid ();
			RefreshGridTiles ();
		}
		EditorGUILayout.EndHorizontal ();
		
		GUILayout.EndVertical ();
		GUILayout.Space (5);

		
		
		// ______________________________________ [ PAINTING ]

		GUILayout.Box(GUIContent.none, "box", GUILayout.ExpandWidth(true), GUILayout.Height(1f));
		GUILayout.Label("Tile Painting", EditorStyles.boldLabel);
		
		
		
		
		// ______________________________________ [ EDIT OPTIONS ]
		
		// show the edit types
		GUIStyle editTypeStyle = new GUIStyle ();
		editTypeStyle.padding = new RectOffset (5, 5, 1, 1);
		EditorGUILayout.BeginVertical (editTypeStyle);
		GUIStyle editTypeButtonStyle = GUI.skin.button; 
		editTypeButtonStyle.margin = new RectOffset (0, 0, 0, 0);
		EditType tempEditType = (EditType) GUILayout.SelectionGrid ((int)editType, 
		                                                            Enum.GetNames (typeof(EditType)), 
		                                                            Enum.GetValues (typeof(EditType)).Length, 
		                                                            editTypeButtonStyle, 
		                                                            GUILayout.Height (22));
		if (editType != tempEditType) {
			editType = tempEditType;
			if (tileParent != null) {
				InitTileGrid ();
			}
		}
		EditorGUILayout.EndVertical ();
		
		// show the edit modes
		GUIStyle editModeStyle = new GUIStyle ();
		editModeStyle.padding = new RectOffset (5, 5, 1, 1);
		EditorGUILayout.BeginVertical (editModeStyle);
		GUIStyle editModeButtonStyle = GUI.skin.button; 
		editModeButtonStyle.margin = new RectOffset (0, 0, 0, 0);
		editMode = (EditMode) GUILayout.SelectionGrid ((int)editMode, 
		                                               Enum.GetNames (typeof(EditMode)), 
		                                               Enum.GetValues (typeof(EditMode)).Length, 
		                                               editModeButtonStyle, 
		                                               GUILayout.Height (16));
		EditorGUILayout.EndVertical ();



		// ______________________________________ [ CATEGORIES ]
		string[] terrainCategoryFullPaths = AssetDatabase.GetSubFolders ("Assets/Prefabs/Environment/Tiles");
		string[] terrainCategories = new string[terrainCategoryFullPaths.Length];
		for (int i = 0; i < terrainCategories.Length; i++) {
			terrainCategories[i] = Path.GetFileName (terrainCategoryFullPaths[i]);
		}
		
		EditorGUILayout.BeginHorizontal ();
		selectedTerrainCategory = EditorGUILayout.Popup ("Tile Category", selectedTerrainCategory, terrainCategories);
		EditorGUILayout.EndHorizontal ();



		// ______________________________________ [ TILES ]
		GUIStyle prefabSelectionGroup = new GUIStyle (GUI.skin.box);
		prefabSelectionGroup.padding = new RectOffset (1, 1, 1, 1);
		prefabSelectionGroup.margin = new RectOffset (10, 10, 10, 0);
//		EditorGUILayout.BeginVertical(prefabSelectionGroup);
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, prefabSelectionGroup, GUILayout.Width (this.position.width - 20), GUILayout.Height (120));

		// add in prefabs in the chosen category
		GUIStyle prefabButtonGroup = new GUIStyle (GUI.skin.box);
		prefabButtonGroup.padding = new RectOffset (1, 1, 2, 1);
		prefabButtonGroup.margin = new RectOffset (0, 0, 0, 0);
		prefabButtonGroup.border = new RectOffset (3, 3, 3, 3);
		Color originalColor = GUI.backgroundColor;

		DirectoryInfo info = new DirectoryInfo(terrainCategoryFullPaths[0]);
		FileInfo[] fileInfo = info.GetFiles();

		int buttonsPerLine = (int)((this.position.width - 20) / 40);
		int buttonIncrementer = 0;
		bool foundSelected = false;
		EditorGUILayout.BeginHorizontal();

		for (int i = 0; i < fileInfo.Length; i++) {

			// get the Terrain Object
			string[] splitOptions = new string[1];
			splitOptions[0] = "Assets/";
			string path = fileInfo[i].FullName.Split (splitOptions, StringSplitOptions.None)[1];
			TileObject to = (TileObject)AssetDatabase.LoadAssetAtPath ("Assets/" + path, typeof(TileObject));

			if (to != null) {
				buttonIncrementer++;
				if (buttonIncrementer > buttonsPerLine) {
					buttonIncrementer = 0;
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
				}
				GUIContent buttonContent = new GUIContent (to.CenterSprites[0].texture);

				if (selectedTile != null && selectedTile.name == to.name) {
					foundSelected = true;
					GUI.backgroundColor = new Color (0, (210F/255F), (210F/255F));
				} else {
					GUI.backgroundColor = Color.black;
				}
				if (GUILayout.Button (buttonContent, prefabButtonGroup, GUILayout.Width (36), GUILayout.Height (36))) {
					selectedTile = to;
					foundSelected = true;
				}
			}
		}
		EditorGUILayout.EndHorizontal();

		GUI.backgroundColor = originalColor;

		EditorGUILayout.EndScrollView();

		if (!foundSelected) {
			selectedTile = null;
		}
	}

	#region Scene GUI

	protected void SceneGUI (SceneView sceneView) {

		// This will have scene events including mouse down on scenes objects
		if (editType != EditType.Select && isVisible) {

			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			// get the mouse event
			Event e = Event.current;
			if (e.type == EventType.mouseDown || e.type == EventType.mouseDrag) {

				// find out where the user clicked
				Ray ray = Camera.current.ScreenPointToRay(e.mousePosition);
				Vector3 pos = ray.GetPoint (ray.origin.z * -1);
				int xKey = (int)Mathf.Floor (pos.x);
				int yKey = (int)Mathf.Floor (pos.y * -1);

				// what selection mode are we using?
				switch (editMode) {
				case EditMode.Brush:

					// if we're using a brush, our selected tile is wherever the mouse is right now in the grid
					if (editType == EditType.Paint) {
						SetTileObject (xKey, yKey, selectedTile);
					} else if (editType == EditType.Erase) {
						RemoveTileObject (xKey, yKey);
					}
					break;
				}
			}
		}
	}
	
	[MenuItem ("Window/Tile Tool")]
	public static void  Init () {

		TileTool window = (TileTool)EditorWindow.GetWindow (typeof (TileTool));
		window.Show();
	}

	#endregion
	
	#region MONO FUNCTIONS
	
	//_______________________________________________ [MONO FUNCTIONS]
	
	protected void OnEnable () {

		SceneView.onSceneGUIDelegate += SceneGUI;

	}
	
	protected void OnDisable () {

		SceneView.onSceneGUIDelegate -= SceneGUI;
	}

	protected void OnLostFocus () {

		isVisible = false;
	}

	#endregion

	#region Terrain Grid

	//_______________________________________________ [Tile GRID FUNCTIONS]

	protected void InitTileGrid () {
		
		tileGrid.Clear ();
		
		// populate the terrainGrid 
		int xKey = 0;
		int yKey = 0;
		
		foreach (Transform child in tileParent) {
			// first set the keys for x and y and the terrain object component
			xKey = (int) Mathf.Floor (child.position.x);
			yKey = (int) Mathf.Floor (child.position.y);
			TileObject to = child.GetComponent<TileObject>();
			
			// we can't proceed if there is no terrain object
			if (to != null) {
				AddTileToGrid (xKey, yKey, to);
			}
		}
	}

	protected void AddTileToGrid (int xKey, int yKey, TileObject to) {

		// if the xKey does not exist in the dictionary, add it
		if (!tileGrid.ContainsKey (xKey)) {
			tileGrid.Add (xKey, new Dictionary<int, TileObject> ());
		}
		
		// now add the terrain object to the sub dictionary
		try {
			tileGrid[xKey].Add (yKey, to);
		} catch {
			Debug.LogError ("Index of Tile (" + xKey + ", " + yKey + ") already exists.");
		}
	}
	
	protected void RefreshGridTiles () {

		int xKey = 0;
		int yKey = 0;
		
		foreach (Transform child in tileParent) {
			// first set the keys for x and y and the terrain object component
			xKey = (int) Mathf.Floor (child.position.x);
			yKey = (int) Mathf.Floor (child.position.y);
			SetTileObject (xKey, yKey, null);
		}
	}

	protected void SetTileObject (int xKey, int yKey, TileObject newTile) { 

		// we can't proceed without a tile grid;
		if (tileParent == null) {
			return;
		}
		if (tileGrid == null) {
			InitTileGrid ();
		}

		// get the tiles around this new tile
		TileObject toAbove = null;
		if (tileGrid.ContainsKey (xKey)) {
			tileGrid[xKey].TryGetValue ((yKey + 1), out toAbove);
		}
		TileObject toBelow = null;
		if (tileGrid.ContainsKey (xKey)) {
			tileGrid[xKey].TryGetValue (yKey - 1, out toBelow);
		}
		TileObject toLeft = null;
		if (tileGrid.ContainsKey (xKey - 1)) {
			tileGrid[xKey - 1].TryGetValue (yKey, out toLeft);
		}
		TileObject toRight = null;
		if (tileGrid.ContainsKey (xKey + 1)) {
			tileGrid[xKey + 1].TryGetValue (yKey, out toRight);
		}

		string tileName = "";
		if (newTile != null) {
			tileName = newTile.name + " (" + xKey + ", " + yKey + ")";
		}

		// get the tile object we're working with
		if (tileGrid.ContainsKey (xKey) && tileGrid [xKey].ContainsKey (yKey)) {
			if (newTile == null) {
				// no new tile? Then just refresh the one at its current place
				tileGrid[xKey][yKey].SetTile (toAbove, toBelow, toLeft, toRight, spriteLayerOrder);
				return;
			}

			// if a tile exists at that index, kill it
			if (tileGrid [xKey] [yKey].name != tileName) {
				DestroyImmediate (tileGrid [xKey] [yKey].gameObject);
				tileGrid [xKey].Remove (yKey);
			} else {
				return;
			}
		}

		// instatiate the new tile
		if (newTile != null) {
			TileObject to = (TileObject)Instantiate (newTile, new Vector3 (xKey + 0.5F, yKey + 0.5F, 0.0F), Quaternion.Euler (Vector3.zero));
			to.name = tileName;
		
			// set up the tile
			to.SetTile (toAbove, toBelow, toLeft, toRight, spriteLayerOrder);
			to.transform.SetParent (tileParent);
			AddTileToGrid (xKey, yKey, to);
		}
	}

	protected void RemoveTileObject (int xKey, int yKey) {
		if (tileGrid.ContainsKey (xKey) && tileGrid [xKey].ContainsKey (yKey)) {
			tileGrid[xKey][yKey].RemoveTileReferences ();
			DestroyImmediate (tileGrid [xKey] [yKey].gameObject);
			tileGrid [xKey].Remove (yKey);
		}
	}

	#endregion
}
