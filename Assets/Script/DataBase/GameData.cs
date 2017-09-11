using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour{

	public static int killPillers{get;set;}
	public UnityEngine.UI.Text killPillerText;

	private static Transform camera;
	private static Transform battleBoard;
	private static Transform pillerGenerator;

	void Awake(){
		killPillers = 0;
	}

	public static Transform GetCamera(){
		if (camera == null){
			camera = GameObject.Find("Main Camera").transform;
		}

		return camera;
	}

	public static Transform GetBattleBoard(){
		if (battleBoard == null){
			battleBoard = GameObject.Find("BattleBoard").transform;
		}

		return battleBoard;
	}

	public static Transform GetPillerGenerator(){
		if (pillerGenerator == null){
			pillerGenerator = GameObject.Find("PillerGenerator").transform;
		}

		return pillerGenerator;
	}
}