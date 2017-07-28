using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType{
	PUNCH,
	KICK,
	HIGH_KICK,
	COUNTER,
	DEFENSE,
}

public enum ActionType{
	ATTACK,
	COUNTER,
	DEFENSE,
}

public class SkillData{
		public SkillData(string name, int ap, int attack, ActionType type){
			_name = name;
			_ap = ap;
			_attack = attack;
			_type = type;
		}
		public string _name{get;set;}
		public int _ap{get;set;}
		public int _attack{get;set;}
		public ActionType _type{get;set;}
}

public class SkillDataBase{
	public static Dictionary<SkillType, SkillData> DATAS = new Dictionary<SkillType, SkillData>(){
		{SkillType.PUNCH, new SkillData("P", 10, 20, ActionType.ATTACK)},
		{SkillType.KICK, new SkillData("K", 15, 30, ActionType.ATTACK)},
		{SkillType.HIGH_KICK, new SkillData("HK", 20, 50, ActionType.ATTACK)},
		{SkillType.COUNTER, new SkillData("C", 30, 0, ActionType.COUNTER)},
		{SkillType.DEFENSE, new SkillData("D", 5, 0, ActionType.DEFENSE)},
	};
}