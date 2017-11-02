using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUIButton : MonoBehaviour{
	public PlayerPointUI m_PlayerUIField;

	
	public void Choice(){
		if (!m_PlayerUIField.GetSelectEnable()){
			return;
		}
		if (transform.childCount > 0){
			m_PlayerUIField.m_SkillBattleManager.AddChoice(m_PlayerUIField.GetPlayerObject(), transform.GetChild(0));
			m_PlayerUIField.m_SkillBattleManager.PlayerFieldSelectDisable();
		}
	}
}