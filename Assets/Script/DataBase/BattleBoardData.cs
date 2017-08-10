using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleBoardData{
	public static GameObject battle_Encount;
	public static GameObject skillChoiceBoard;
	public static Text battleTimer;


	public static void Initialize(){
		if (battle_Encount == null){
			battle_Encount = GameData.GetBattleBoard().FindChild("Battle_Encount").gameObject;
		}
		if (skillChoiceBoard == null){
			skillChoiceBoard = GameData.GetBattleBoard().FindChild("SkillChoiiceBoard").gameObject;
		}
		if (battleTimer == null){
			battleTimer = GameData.GetBattleBoard().FindChild("BattleTimer").GetComponent<Text>();
		}
	}
}