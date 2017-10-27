using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPointUI : MonoBehaviour {
	private GameObject m_Player;

	public void SetPlayerObject(GameObject player){
		m_Player = player;
	}

	public GameObject GetPlayerObject(){
		return m_Player;
	}
	
	public void AllButtonEnable(){
		foreach(Transform obj in transform){
			obj.GetComponent<Button>().enabled = true;
		}
	}
	
	public void AllButtonDisable(){
		foreach(Transform obj in transform){
			obj.GetComponent<Button>().enabled = false;
		}
	}
}
