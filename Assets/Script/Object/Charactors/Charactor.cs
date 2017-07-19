using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charactor : MonoBehaviour {

#region メンバ変数

	// 付属クラス
	public PlanetWalk planetWalk;
	public Rigidbody_grgr rigidbody;

	// 地上からの高さ調整
	public float PLANET_HEIGHT;

#endregion

#region Unity関数

	protected void Awake(){
		planetWalk = new PlanetWalk(transform);
		rigidbody = new Rigidbody_grgr(transform);
	}

	protected void Update(){
		rigidbody.Update();
		planetWalk.Update(PLANET_HEIGHT);
	}

#endregion

}