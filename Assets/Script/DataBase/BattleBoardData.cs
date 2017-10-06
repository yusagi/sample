using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleBoardData{
	public static SkillChoiceBoardController skillChoiceBoard;


	public static void Initialize(){
		if (skillChoiceBoard == null){
			skillChoiceBoard = GameData.GetBattleBoard().FindChild("SkillChoiiceBoard").GetComponent<SkillChoiceBoardController>();
		}
	}
}