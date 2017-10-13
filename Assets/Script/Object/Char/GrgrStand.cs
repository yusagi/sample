using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrgrStand : MonoBehaviour {
	
	public void Stand(Vector3 center, float radius, float up){

		Vector3 rayStart = center + (transform.up* (radius + 0.1f));
		RaycastHit hitInfo;
		Physics.Raycast(rayStart, -transform.up, out hitInfo, Mathf.Infinity, (int)LayerMask.PLANET);

		transform.position = hitInfo.point + (transform.up * up);
	}
}
