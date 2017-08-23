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
    void Start()
    {
        rigidbody = new Rigidbody_grgr(transform);
        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameManager.m_Planet.transform.position, GameManager.m_Planet.transform.localScale.y * 0.5f, HEIGHT_FROM_GROUND);
    }

    void Update(){
		rigidbody.Update();
	}

#endregion

}