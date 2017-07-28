using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillAttributeResult{
	DRAW = 0,
	LOSE = 1,
	WIN = 2,
}

public class SkillManager {

	public static SkillAttributeResult GetSkillResult(SkillAttribute card1, SkillAttribute card2){
		int c1 = (int)card1;
		int c2 = (int)card2;
		int result = (c1 - c2 + 3) % 3;
		
		return (SkillAttributeResult)result;
	}

	private List<SkillData> m_Skills = new List<SkillData>();
	public SkillAttributeResult result;


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