using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileObject : MonoBehaviour {

	// Sprite Dictionary
	[SerializeField] [HideInInspector] protected List<string> stateIDs = new List<string> ();
	[SerializeField] [HideInInspector] protected List<Sprite> centerSprites = new List<Sprite> ();
	[SerializeField] [HideInInspector] protected List<Sprite> aboveSprites = new List<Sprite> ();
	[SerializeField] [HideInInspector] protected List<Sprite> belowSprites = new List<Sprite> ();
	[SerializeField] [HideInInspector] protected List<Sprite> leftSprites = new List<Sprite> ();
	[SerializeField] [HideInInspector] protected List<Sprite> rightSprites = new List<Sprite> ();

	// Terrain Grid Reference
	[SerializeField] public TileObject aboveTile;
	[SerializeField] public TileObject belowTile;
	[SerializeField] public TileObject leftTile;
	[SerializeField] public TileObject rightTile;

	#region Properties

	public List<string> StateIDs {
		get {
			return stateIDs;
		}
		set {
			stateIDs = value;
		}
	}

	public List<Sprite> CenterSprites {
		get {
			return centerSprites;
		}
		set {
			centerSprites = value;
		}
	}

	public List<Sprite> AboveSprites {
		get {
			return aboveSprites;
		}
		set {
			aboveSprites = value;
		}
	}

	public List<Sprite> BelowSprites {
		get {
			return belowSprites;
		}
		set {
			belowSprites = value;
		}
	}

	public List<Sprite> LeftSprites {
		get {
			return leftSprites;
		}
		set {
			leftSprites = value;
		}
	}

	public List<Sprite> RightSprites {
		get {
			return rightSprites;
		}
		set {
			rightSprites = value;
		}
	}

	#endregion
	
	#region Sprite Getters
	
	protected string getSpriteName (Vector3 pos) {
		if (pos == Vector3.up) {
			return "above";
		} else if (pos == Vector3.down) {
			return "below";
		} else if (pos == Vector3.left) {
			return "left";
		} else if (pos == Vector3.right) {
			return "right";
		} else {
			return "null";
		}
	}
	
	protected int getStateIndex (string state) {
		if (stateIDs.Contains (state)) {
			return stateIDs.IndexOf (state);
		} else {
			return 0;
		}
	}
	
	protected Sprite getSpriteImage (Vector3 pos, string state) {

		int stateIndex = getStateIndex (state);
		if (pos == Vector3.zero) {
			return centerSprites [stateIndex];
		} else if (pos == Vector3.up) {
			return aboveSprites [stateIndex];
		} else if (pos == Vector3.down) {
			return belowSprites [stateIndex];
		} else if (pos == Vector3.left) {
			return leftSprites [stateIndex];
		} else if (pos == Vector3.right) {
			return rightSprites [stateIndex];
		} else {
			return centerSprites [stateIndex];
		}
	}

	public GameObject GetSprite (Vector3 pos)  {
		if (pos == Vector3.zero) {
			return gameObject;
		}
		foreach (Transform child in transform) { 
			if (child.name == getSpriteName(pos)) {
				return child.gameObject;
			}
		}
		return null;
	}

	public GameObject CreateSubSprite (Vector3 pos) {

		// if there is no sprite at the position, return
		if (getSpriteImage (pos, "default") == null) {
			return null;
		}

		GameObject newGo = new GameObject (getSpriteName(pos));
		SpriteRenderer sr = newGo.AddComponent<SpriteRenderer>();

		// set its sprite
		sr.sprite = getSpriteImage (pos, "default");
		sr.sortingOrder = GetComponent<SpriteRenderer> ().sortingOrder;

		// set its position
		newGo.transform.SetParent (transform);
		newGo.transform.localPosition = pos;
		return newGo;
	}

	// Set Sprite
	public void SetSprite (Vector3 pos, Sprite newSprite) {
		GameObject go = GetSprite (pos);
		if (go == null) {
			go = CreateSubSprite (pos);
		}
		go.GetComponent<SpriteRenderer>().sprite = newSprite;
	}

	#endregion

	#region Tile Placing Functions

	public void SetTile (TileObject above, TileObject below, TileObject left, TileObject right) {

		// set each of the tile references
		aboveTile = SetTileReference (above, Vector3.up);
		if (above != null) {
			above.belowTile = above.SetTileReference (this, Vector3.down);
		}
		belowTile = SetTileReference (below, Vector3.down);
		if (below != null) {
			below.aboveTile = below.SetTileReference (this, Vector3.up);
		}
		leftTile = SetTileReference (left, Vector3.left);
		if (left != null) {
			left.rightTile = left.SetTileReference (this, Vector3.right);
		}
		rightTile = SetTileReference (right, Vector3.right);
		if (right != null) {
			right.leftTile = right.SetTileReference (this, Vector3.left);
		}
	}

	public TileObject SetTileReference (TileObject tileReference, Vector3 pos) {
		Transform child = transform.FindChild (getSpriteName(pos));

		if (tileReference != null && child != null) {
			DestroyImmediate (child.gameObject);
		} else if (tileReference == null && child == null) {
			CreateSubSprite (pos);
		}

		return tileReference;
	}

	public void RemoveTileReferences () {
		if (aboveTile != null) {
			aboveTile.belowTile = aboveTile.SetTileReference (null, Vector3.down);
		}
		if (belowTile != null) {
			belowTile.aboveTile = belowTile.SetTileReference (null, Vector3.up);
		}
		if (leftTile != null) {
			leftTile.rightTile = leftTile.SetTileReference (null, Vector3.right);
		}
		if (rightTile != null) {
			rightTile.leftTile = rightTile.SetTileReference (null, Vector3.left);
		}
	}

	#endregion
}
