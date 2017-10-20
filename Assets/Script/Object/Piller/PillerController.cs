using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillerController : MonoBehaviour {

	// 地上からの高さ調整
	public float HEIGHT_FROM_GROUND;

    // 惑星
    private Transform m_Planet;

    void Start()
    {
        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, m_Planet.position, m_Planet.localScale.y * 0.5f, HEIGHT_FROM_GROUND);
    }

    void Update(){
	}

    public void SetPlanet(Transform planet){
        m_Planet = planet;
    }
}