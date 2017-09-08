using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType{
	JAB,
	HIKICK,
	SPINKICK,
	COUNTER,
	DEFENSE,
}

public enum ActionType{
	ATTACK = 0,
	COUNTER = 1,
	DEFENSE = 2,
}

public enum AnimationType{
	NORMAL_ATTACK = 0,
	COUNTER_ATTACK,
	NONE,
}

public class SkillData{
		public SkillData(string name, string anmName, int ap, int attack, ActionType type){
			_name = name;
			_anmName = anmName;
			_ap = ap;
			_attack = attack;
			_type = type;
		}
		public string _name{get;set;}
		public string _anmName{get;set;}
		public int _ap{get;set;}
		public int _attack{get;set;}
		public ActionType _type{get;set;}
}

public class SkillDataBase{
	public static Dictionary<SkillType, SkillData> DATAS = new Dictionary<SkillType, SkillData>(){
		{SkillType.JAB, new SkillData("ジャブ", "Jab", 10, 20, ActionType.ATTACK)},
		{SkillType.HIKICK, new SkillData("キック","Hikick", 15, 30, ActionType.ATTACK)},
		{SkillType.SPINKICK, new SkillData("スピンキック", "Spinkick", 20, 50, ActionType.ATTACK)},
		{SkillType.COUNTER, new SkillData("カウンター", "Counter", 30, 0, ActionType.COUNTER)},
		//{SkillType.DEFENSE, new SkillData("ガード", "Defecse", 5, 0, ActionType.DEFENSE)},
	};
}