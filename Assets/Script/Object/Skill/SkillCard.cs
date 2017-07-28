using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillAttribute{
	GOO,
	TYOKI,
	PAR,
}

public class SkillCard {
	private string m_Name;
	private SkillAttribute m_Attribute;

	public SkillCard(string name, SkillAttribute attribute){
		m_Name = name;
		m_Attribute = attribute;
	}

	public string GetName(){
		return m_Name;;
	}

	public SkillAttribute GetAttribute(){
		return m_Attribute;
	}
}