using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType{
	NORMAL_ATTACK = 0,  // 通常攻撃
	COUNTER_ATTACK,     // カウンター攻撃
	GUARD,              // ガード
    GUARD_BREAK_ATTACK, // ガード破壊攻撃        
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
	public static Dictionary<string, SkillData> PLAYER_DATAS = new Dictionary<string, SkillData>(){
		{"Jab", new SkillData("ジャブ", "Jab", 10, 20, ActionType.NORMAL_ATTACK)},
		{"Hikick", new SkillData("キック","Hikick", 15, 30, ActionType.NORMAL_ATTACK)},
		{"Spinkick", new SkillData("スピンキック", "Spinkick", 20, 50, ActionType.GUARD_BREAK_ATTACK)},
		{"Counter", new SkillData("カウンター", "Counter", 30, 0, ActionType.COUNTER_ATTACK)},
		{"Guard", new SkillData("ガード", "Guard", 5, 0, ActionType.GUARD)},
	};

    public static Dictionary<string, SkillData> ENEMY_DATAS = new Dictionary<string, SkillData>(){
        {"Jab", new SkillData("ジャブ", "Jab", 10, 20, ActionType.NORMAL_ATTACK)},
        {"Hikick", new SkillData("キック","Hikick", 15, 30, ActionType.NORMAL_ATTACK)},
        {"Spinkick", new SkillData("スピンキック", "Spinkick", 20, 50, ActionType.GUARD_BREAK_ATTACK)},
        {"Counter", new SkillData("カウンター", "Counter", 30, 0, ActionType.COUNTER_ATTACK)},
        {"Guard", new SkillData("ガード", "Guard", 5, 0, ActionType.GUARD)},
    };
}