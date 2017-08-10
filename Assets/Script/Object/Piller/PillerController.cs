using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillerController : MonoBehaviour {

#region メンバ変数

	// 付属クラス
	public Rigidbody_grgr rigidbody;

	// 地上からの高さ調整
	public float HEIGHT_FROM_GROUND;

#endregion

#region Unity関数

	protected void Awake(){
		rigidbody = new Rigidbody_grgr(transform);
		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
	}

	protected void Update(){
		rigidbody.Update();
	}

#endregion

}