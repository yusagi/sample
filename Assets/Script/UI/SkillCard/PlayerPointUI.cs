using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPointUI : MonoBehaviour {
	private GameObject m_Player;

	public void SetPlayerObject(GameObject player){
		m_Player = player;
	}

	public GameObject GetPlayerObject(){
		return m_Player;
	}
}
