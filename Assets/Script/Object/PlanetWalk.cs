using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetWalk : MonoBehaviour {

	public float up = 0.5f;

	void Awake(){
	}

	// Use this for initialization
	void Start () {
		PoseControl();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
		PoseControl();
	}

	// 姿勢制御
	void PoseControl()
	{
		Transform planet = GameData.GetPlanet();
		transform.position = planet.position + (transform.up * planet.localScale.y);

		RaycastHit hitInfo;
		Physics.Raycast(transform.position, (planet.position - transform.position).normalized, out hitInfo, Mathf.Infinity, (int)Layer.PLANET);
		Vector3 front = Vector3.ProjectOnPlane(transform.forward, hitInfo.normal);
		if (front.magnitude < UtilityMath.epsilon)
			front = Vector3.ProjectOnPlane(transform.up, hitInfo.normal);
		Quaternion rotate = Quaternion.LookRotation(front, hitInfo.normal);
		transform.rotation = rotate;
		transform.position = hitInfo.point + (transform.up * transform.localScale.y * 0.1f);

		transform.position += transform.up * up;
	}
}
