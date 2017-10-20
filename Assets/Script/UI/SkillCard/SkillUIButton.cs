using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUIButton : MonoBehaviour{
	public PlayerPointUI m_PlayerUIField;
	public SkillBattleManager m_SkillBattleManager;

	public void Choice(){
		m_SkillBattleManager.AddChoice(m_PlayerUIField.GetPlayerObject(), transform.GetChild(0));
	}
}