using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCore : MonoBehaviour {
	protected BaseBrain m_Brain;

	public void SetBrain(BaseBrain brain){
		gameObject.AddComponent(brain.GetType());
		m_Brain = GetComponent<BaseBrain>();
	}

	public BaseBrain GetBrain(){
		return m_Brain;
	}
}
