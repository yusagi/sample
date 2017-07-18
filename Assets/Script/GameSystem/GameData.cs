using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour{

	public static float hitStopTimer{get;set;}

	public static int killPillers{get;set;}
	public UnityEngine.UI.Text killPillerText;

	public float GAME_TIME = 60.0f;
	private static float gameTimer{get;set;}
	public UnityEngine.UI.Text gameTimerText;

	private static Transform camera;
	private static Transform player;
	private static Transform enemy;
	private static Transform planet;
	private static Transform battleArea;

	void Awake(){
		killPillers = 0;
		gameTimer = GAME_TIME;
	}

	void Update(){
		if (hitStopTimer >= 0.0f)
			hitStopTimer += Time.deltaTime;

		//CountGameTime();

		killPillerText.text = killPillers.ToString();
	}

	// ゲームタイム
	void CountGameTime(){
		if (gameTimer <= 0.0f){
			player.GetComponent<PlayerController>().enabled = false;
		}
		gameTimer -= Time.deltaTime;
		float time = ((int)gameTimer) + (float)((int)((gameTimer - ((int)gameTimer)) * 10) * 0.1f);
		time = Mathf.Max(time, 0);
		gameTimerText.text = time.ToString();
	}

	public static void HitStopStart(){
		hitStopTimer = 0.0f;
	}

	public static void HitStopEnd(){
		hitStopTimer = -1.0f;
	}

	public static Transform GetCamera(){
		if (camera == null){
			camera = GameObject.Find("Main Camera").transform;
		}

		return camera;
	}
	public static Transform GetPlayer(){
		if (player == null){
			player = GameObject.Find("Player").transform;
		}

		return player;
	}
	public static Transform GetEnemy(){
		if (enemy == null){
			enemy = GameObject.Find("Enemy").transform;
		}

		return enemy;
	}
	public static Transform GetPlanet(){
		if (planet == null){
			planet = GameObject.Find("Planet").transform;
		}

		return planet;
	}

	public static Transform GetBattleArea(){
		if (battleArea == null){
			battleArea = GameObject.Find("BattleArea").transform;
		}

		return battleArea;
	}
}