using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SkillManager {
	private List<SkillData> m_Skills = new List<SkillData>();

	public void AddSkill(SkillData card){
		m_Skills.Add(card);
	}
	
	public string GetSkillName(int cardId){
		if (m_Skills.Count - 1 < cardId)
			Debug.LogError("out of range[SkillCards]");

		return m_Skills[cardId]._name;
	}

	public ActionType GetAttribute(int cardId){
		if (m_Skills.Count - 1 < cardId)
			Debug.LogError("out of range[SkillCards]");

		return m_Skills[cardId]._type;
	}

	public List<SkillData> GetSkillCards(){
		return m_Skills;
	}
}