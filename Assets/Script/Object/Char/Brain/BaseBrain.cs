using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseBrain : MonoBehaviour {
	protected Vector3 m_Velocity;
	protected PlanetID m_PlanetID;

	// 更新
	public abstract void BrainUpdate();

	public void SetVelocity(Vector3 velocity){
		m_Velocity = velocity;
	}

	public Vector3 GetVelocity(){
		Vector3 tmp = m_Velocity;
		m_Velocity = Vector3.zero;
		return tmp;
	}

	public void SetPlanetID(PlanetID id){
		m_PlanetID = id;
	}

	public PlanetID GetPlanetID(){
		return m_PlanetID;
	}
}
