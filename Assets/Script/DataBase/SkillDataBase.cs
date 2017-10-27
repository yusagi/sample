using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// バトルの強弱のルール
public enum ActionType{
	RED = 0,  	// 赤
	GREEN,		// 緑
	BLUE,		// 青
    GUARD,      // ガード
	NONE,		// なし
}

// 追撃属性
public enum ChaseType{
	BASE = 0,	// 初動技
	CHASE_1,	// 追撃1
	CHASE_2,	// 追撃2
	CHASE_END,	// 追撃終了(これは使用しない)
	NONE,		// なし
}

// アーツ
public enum ArtsType{
	FIGHTER,	// 体術タイプ
	SWORDMAN,	// 剣術タイプ
	GUNNER,		// 銃術タイプ
	NONE,		// なし
}

public class SkillData{
		public SkillData(string name, string anmName, int ap, int attack, ActionType actionType, ChaseType chaseType, ArtsType artsType){
			_name = name;
			_anmName = anmName;
			_ap = ap;
			_attack = attack;
			_actionType = actionType;
			_chaseType = chaseType;
			_artsType = artsType;
		}
		public string _name{get;set;}
		public string _anmName{get;set;}
		public int _ap{get;set;}
		public int _attack{get;set;}
		public ActionType _actionType{get;set;}
		public ChaseType _chaseType{get;set;}
		public ArtsType _artsType{get;set;}
}

// データの名前
public enum SKILL_NAME{
	JAB_RED_BASE_FIGHTER,
	JAB_RED_CHASE1_FIGHTER,
	JAB_RED_CHASE2_FIHGTER,

	HIKICK_BLUE_BASE_FIGHTER,
	HIKICK_BLUE_CHASE1_FIGHTER,
	HIKICK_BLUE_CHASE2_FIGHTER,

	SPINKICK_GREEN_BASE_FIGHTER,
	SPINKICK_GREEN_CHASE1_FIGHTER,
	SPINKICK_GREEN_CHASE2_FIGHTER,

	SWORD_THRUST_BLUE_BASE_SWORDMAN,
	SWORD_THRUST_BLUE_CHASE1_SWORDMAN,

	SWORD_SLASH_GREEN_BASE_SWORDMAN,
	SWORD_SLASH_GREEN_CHASE1_SWORDMAN,
	SWORD_SLASH_GREEN_CHASE2_SWORDMAN,

	SWORD_SWIND_RED_CHASE1_SWORDMAN,
	SWORD_SWIND_RED_CHASE2_SWORDMAN,

	GUARD
}

public class SkillDataBase{
	public static Dictionary<string, SkillData> SKILL_DATAS = new Dictionary<string, SkillData>(){
		// ジャブ
        {"JAB_RED_BASE_FIGHTER", 	new SkillData("ジャブ",  "Jab", 0, 20, ActionType.RED, ChaseType.BASE,    ArtsType.FIGHTER)},
		{"JAB_RED_CHASE1_FIGHTER", 	new SkillData("ジャブ1", "Jab", 0, 20, ActionType.RED, ChaseType.CHASE_1, ArtsType.FIGHTER)},
		{"JAB_RED_CHASE2_FIHGTER", 	new SkillData("ジャブ2", "Jab", 0, 20, ActionType.RED, ChaseType.CHASE_2, ArtsType.FIGHTER)},

		// ハイキック
        {"HIKICK_BLUE_BASE_FIGHTER",   new SkillData("キック","Hikick", 0, 30, ActionType.BLUE, ChaseType.BASE,    ArtsType.FIGHTER)},
        {"HIKICK_BLUE_CHASE1_FIGHTER", new SkillData("キック1","Hikick", 0, 30, ActionType.BLUE, ChaseType.CHASE_1, ArtsType.FIGHTER)},
        {"HIKICK_BLUE_CHASE2_FIGHTER", new SkillData("キック2","Hikick", 0, 30, ActionType.BLUE, ChaseType.CHASE_2, ArtsType.FIGHTER)},

		// スピンキック
        {"SPINKICK_GREEN_BASE_FIGHTER",		new SkillData("スピンキック", "Spinkick", 0, 50, ActionType.GREEN, ChaseType.BASE, ArtsType.FIGHTER)},
        {"SPINKICK_GREEN_CHASE1_FIGHTER",	new SkillData("スピンキック1", "Spinkick", 0, 50, ActionType.GREEN, ChaseType.CHASE_1, ArtsType.FIGHTER)},
        {"SPINKICK_GREEN_CHASE2_FIGHTER",	new SkillData("スピンキック2", "Spinkick", 0, 50, ActionType.GREEN, ChaseType.CHASE_2, ArtsType.FIGHTER)},

		// 剣突き
		{"SWORD_THRUST_BLUE_BASE_SWORDMAN",   new SkillData("剣突き",  "Jab", 0, 20, ActionType.BLUE, ChaseType.BASE,    ArtsType.SWORDMAN)},
		{"SWORD_THRUST_BLUE_CHASE1_SWORDMAN", new SkillData("剣突き1", "Jab", 0, 20, ActionType.BLUE, ChaseType.CHASE_1, ArtsType.SWORDMAN)},

		// 剣斬り
		{"SWORD_SLASH_GREEN_BASE_SWORDMAN",   new SkillData("剣斬り",   "Hikick", 0, 20, ActionType.GREEN, ChaseType.BASE, ArtsType.SWORDMAN)},
		{"SWORD_SLASH_GREEN_CHASE1_SWORDMAN", new SkillData("剣斬り1",  "Hikick", 0, 20, ActionType.GREEN, ChaseType.CHASE_1, ArtsType.SWORDMAN)},
		{"SWORD_SLASH_GREEN_CHASE2_SWORDMAN", new SkillData("剣斬り2",  "Hikick", 0, 20, ActionType.GREEN, ChaseType.CHASE_2, ArtsType.SWORDMAN)},

		// 剣大振り
		{"SWORD_SWIND_RED_CHASE1_SWORDMAN", new SkillData("剣大振り1",  "Spinkick", 0, 20, ActionType.RED, ChaseType.CHASE_1, ArtsType.SWORDMAN)},
		{"SWORD_SWIND_RED_CHASE2_SWORDMAN", new SkillData("剣大振り2",  "Spinkick", 0, 20, ActionType.RED, ChaseType.CHASE_2, ArtsType.SWORDMAN)},

		{"GUARD", 		new SkillData("ガード", "Guard", 5, 0, ActionType.GUARD, ChaseType.NONE, ArtsType.NONE)},
	};
}

