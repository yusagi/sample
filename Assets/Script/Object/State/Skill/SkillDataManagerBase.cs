using System.Collections.Generic;
using UnityEngine;

public class SkillDataManagerBase{

	protected List<SkillData> m_SkillDataList = new List<SkillData>();

	public virtual void SkillDataIni(){

	}

	public virtual void SkillDataFin(){

	}

	public virtual void AddSkillData(SkillData data){
		m_SkillDataList.Add(data);
	}

	public virtual SkillData GetSkillData(int id){
		if (!IsDataCheck(id)){
			Debug.LogError("out of range m_SkillDataList[id]");
			return null;
		}
		return m_SkillDataList[id];
	}

	public virtual List<SkillData> GetSkillDataList(){
		return m_SkillDataList;
	}

	protected bool IsDataCheck(int id){
		if (id >= 0 && id < m_SkillDataList.Count){
			return true;
		}

		return false;
	}
}