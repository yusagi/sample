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
		Vector3 rayStart = planet.position + (my.rotation * Vector3.up * (planet.localScale.y * 0.5f + 0.1f));
		RaycastHit hitInfo;
		Physics.Raycast(rayStart, -my.up, out hitInfo, Mathf.Infinity, (int)Layer.PLANET);
		my.position = hitInfo.point + (my.up * my.localScale.y * 0.5f);

		my.position += my.up * up;
	}
}
