using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPointUI : MonoBehaviour {

	enum FLICK_VEL{
		UP = 0,
		RIGHT,
		DOWN,
		LEFT,
	}

	public SkillBattleManager m_SkillBattleManager;
	public bool isFlickSelect = false;
	private GameObject m_Player;

	private bool m_SelectEnable = false;

	void Update(){
		FlickSelectUpdate();
	}

	void FlickSelectUpdate(){
		if (isFlickSelect == false){
			return;
		}
		Vector3 flickVel = InputManager.GetFlickVelocity().normalized;
		if (flickVel.magnitude < Vector3.kEpsilon){
			return;
		}
		float angle;
		FLICK_VEL velocity;
		// 前後判断
		angle = Mathf.Acos(Vector3.Dot(flickVel, Vector3.forward)) * Mathf.Rad2Deg;
		if (angle < 90){
			velocity = FLICK_VEL.UP;
		}
		else{
			velocity = FLICK_VEL.DOWN;
		}
		
		angle = Mathf.Acos(Vector3.Dot(flickVel, Vector3.right)) * Mathf.Rad2Deg;
		if (angle < 45){
			velocity = FLICK_VEL.RIGHT;
		}
		else if (angle > 135){
			velocity = FLICK_VEL.LEFT;
		}

		transform.GetChild((int)velocity).GetComponent<SkillUIButton>().Choice();
	}

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

	public void SelectEnable(){
		m_SelectEnable = true;
	}
	public void SelectDisable(){
		m_SelectEnable = false;
	}
	public bool GetSelectEnable(){
		return m_SelectEnable;
	}
}
