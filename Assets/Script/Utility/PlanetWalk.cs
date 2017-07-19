using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetWalk {

	public bool isActive{get;set;}
	private Transform my;

	public PlanetWalk(Transform obj, float up = 0.0f){
		my = obj;
		isActive = true;
		PoseControl(up);
	}
	
	// Update is called once per frame
	public void Update (float up) {
		if (isActive)
			PoseControl(up);
	}

	// 姿勢制御
	void PoseControl(float up)
	{
		Transform planet = GameData.GetPlanet();
		my.position = planet.position + (my.up * planet.localScale.y);

		RaycastHit hitInfo;
		Physics.Raycast(my.position, (planet.position - my.position).normalized, out hitInfo, Mathf.Infinity, (int)Layer.PLANET);
		Vector3 front = Vector3.ProjectOnPlane(my.forward, hitInfo.normal);
		if (front.magnitude < UtilityMath.epsilon)
			front = Vector3.ProjectOnPlane(my.up, hitInfo.normal);
		Quaternion rotate = Quaternion.LookRotation(front, hitInfo.normal);
		my.rotation = rotate;
		my.position = hitInfo.point + (my.up * my.localScale.y * 0.1f);

		my.position += my.up * up;
	}
}
