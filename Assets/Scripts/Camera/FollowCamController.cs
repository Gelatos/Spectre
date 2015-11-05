using UnityEngine;
using System.Collections;

public class FollowCamController : MonoBehaviour {

	[SerializeField]
	protected Transform follow;

	// Update is called once per frame
	void Update () {
		Vector3 pos = follow.position;
		pos.z = transform.position.z;
		pos.y += 0.5F;
		transform.position = pos;
	}
}
