using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {

#region static変数
	public static float BATTLE_START_TIME = 1.0f;
	public static float SKILLBATTLE_PLAY_TIME = 1.0f;
	public static float SKILLBATTLE_RESULT_TIME = 1.0f;
#endregion

#region enum

	public enum Battle{
		BATTLE_START,
		SKILL_BATTLE_START,
		SKILL_BATTLE_CHOICE,
		SKILL_BATTLE_PLAY,
		SKILL_BATTLE_RESULT,
		SKILL_BATTLE_END,
		BATTLE_END,
		NONE,
	}

	enum SkillChoiceFlick{
		RIGHT = 0, 	// パー
		LEFT  = 1,	// チョキ
		UP    = 2,	// グー
		DOWN,
		NONE,
	}

#endregion

#region メンバ変数
	// スキルバトル用
	List<GameObject> skillCards = new List<GameObject>();

	// 状態
	public Phase<Battle> battle = new Phase<Battle>();
	SkillChoiceFlick choiceSkill = SkillChoiceFlick.NONE;
	SkillChoiceFlick eChoiceSkill = SkillChoiceFlick.NONE;

	// 他オブジェクト
	CameraController3 camera;
#endregion

	void Awake(){
		battle.Change(Battle.NONE);

		camera = GameData.GetCamera().GetComponent<CameraController3>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		battle.Start();
		switch(battle.current){
			// バトルモード開始
			case Battle.BATTLE_START:{
				// エンカウント演出

				if (battle.IsFirst()){
					GameData.GetBattleBoard().FindChild("Battle_Encount").gameObject.SetActive(true);
				}

				// スキルバトルスタートへ
				if (camera.phase_battle.current == CameraController3.Battle.SKILL_BATTLE_START){
					GameData.GetBattleBoard().FindChild("Battle_Encount").gameObject.SetActive(false);
					battle.Change(Battle.SKILL_BATTLE_START);
				}
			}
			break;
			// スキルバトル開始
			case Battle.SKILL_BATTLE_START:{
				// スキル表示演出

				if (battle.IsFirst()){
					Transform skillChoiceBoard = GameData.GetBattleBoard().FindChild("SkillChoiiceBoard");
					skillChoiceBoard.gameObject.SetActive(true);
					for(int i = 0; i < 3; i++){
						GameObject skillCard = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
						SkillChoiceFlick name = (SkillChoiceFlick)i;
						skillCard.transform.SetParent(skillChoiceBoard.FindChild(name.ToString()), false);
						if (i == 0) skillCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "パー";
						else if (i == 1) skillCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "チョキ";
						else skillCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "グー";

						skillCards.Add(skillCard);
					}
				}

				choiceSkill = SkillChoiceFlick.NONE;
				// スキル選択へ
				if (TouchController.GetTouchType() == TouchController.TouchType.Frick){
					Vector3 flickvel = Vector3.ProjectOnPlane(TouchController.GetFlickVelocity(), Vector3.up).normalized;
					Vector3 isDown = Vector3.ProjectOnPlane(flickvel, Vector3.right).normalized;
					if (Vector3.Dot(Vector3.forward, isDown) != 1){
						choiceSkill = SkillChoiceFlick.DOWN;
					}
					// Vector3.rightからのアングルでフリック方向を決定
					float angle = Mathf.Acos(Vector3.Dot(flickvel, Vector3.right)) * Mathf.Rad2Deg;
					// 右
					if (angle < 45.0f){
						choiceSkill = SkillChoiceFlick.RIGHT;
					}
					// 上
					else if (angle < 135.0f){
						if (choiceSkill == SkillChoiceFlick.DOWN){
							break;
						}
						choiceSkill = SkillChoiceFlick.UP;
					}
					// 左
					else choiceSkill = SkillChoiceFlick.LEFT;

					// スキル選択へ
					battle.Change(Battle.SKILL_BATTLE_CHOICE);
				}

			}
			break;
			// スキル選択
			case Battle.SKILL_BATTLE_CHOICE:{
				// スキル選択演出

				if (battle.IsFirst()){
					foreach(var card in skillCards){
						GameObject.Destroy(card);
					}
					GameData.GetBattleBoard().FindChild("SkillChoiiceBoard").gameObject.SetActive(false);
					battle.Change(Battle.SKILL_BATTLE_PLAY);
				}
			}
			break;
			// スキルバトルプレイ
			case Battle.SKILL_BATTLE_PLAY:{
				// バトル演出

				if (battle.IsFirst()){
					Transform skillBattleBoard = GameData.GetBattleBoard().FindChild("SkillBattleBoard");
					skillBattleBoard.gameObject.SetActive(true);

					// プレイヤースキルカード生成Prefabs/SkillCard
					GameObject pCard = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
					int pChoiceNum = (int)choiceSkill;
					if (pChoiceNum == 0) pCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "パー";
					else if (pChoiceNum == 1) pCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "チョキ";
					else pCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "グー";
					Transform playerPoint = skillBattleBoard.FindChild("PlayerPoint");
					pCard.transform.SetParent(playerPoint, false);

					// エネミーカード作成
					GameObject eCard = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
					int eChoiceNum = Random.Range(0, 3);
					if (eChoiceNum == 0) eCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "パー";
					else if (eChoiceNum == 1) eCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "チョキ";
					else eCard.GetComponentInChildren<UnityEngine.UI.Text>().text = "グー";
					eCard.transform.SetParent(skillBattleBoard.FindChild("EnemyPoint"), false);
					
					skillCards.Add(pCard);
					skillCards.Add(eCard);

					// 勝負結果
					UnityEngine.UI.Text result = GameData.GetBattleBoard().FindChild("SkillBattleResultBoard").FindChild("Result").GetComponent<UnityEngine.UI.Text>();
					if (pChoiceNum == 0){ // P = パー
						if (eChoiceNum == 0) result.text = "DRAW"; 		// E = パー
						else if (eChoiceNum == 1) result.text = "LOSE"; // E = チョキ
						else result.text = "WIN";						// E = グー
					}
					else if(pChoiceNum == 1){ // P = チョキ
						if (eChoiceNum == 0) result.text = "WIN"; 		// E = パー
						else if (eChoiceNum == 1) result.text = "DRAW"; // E = チョキ
						else result.text = "LOSE";						// E = グー
					}
					else { // P = グー
						if (eChoiceNum == 0) result.text = "LOSE"; 		// E = パー
						else if (eChoiceNum == 1) result.text = "WIN"; 	// E = チョキ
						else result.text = "DRAW";						// E = グー
					}
				}

				if (battle.phaseTime >= SKILLBATTLE_PLAY_TIME){
					foreach(var card in skillCards){
						GameObject.Destroy(card);
					}
					GameData.GetBattleBoard().FindChild("SkillBattleBoard").gameObject.SetActive(false);
					battle.Change(Battle.SKILL_BATTLE_RESULT);
				}
			}
			break;
			// スキルバトル結果
			case Battle.SKILL_BATTLE_RESULT:{
				if (battle.IsFirst()){
					GameData.GetBattleBoard().FindChild("SkillBattleResultBoard").gameObject.SetActive(true);
				}

				if (battle.phaseTime >= SKILLBATTLE_RESULT_TIME){
					GameData.GetBattleBoard().FindChild("SkillBattleResultBoard").gameObject.SetActive(false);
					battle.Change(Battle.SKILL_BATTLE_END);
				}
			}
			break;
			// スキルバトル終了
			case Battle.SKILL_BATTLE_END:{
				if (battle.IsFirst()){
					GameData.GetPlayer().GetComponent<FPlayerController>().state.Change(FPlayerController.State.SKILL_BATTLE_END);
					GameData.GetEnemy().GetComponent<FEnemyController>().state.Change(FEnemyController.State.SKILL_BATTLE_END);
					GameData.GetCamera().GetComponent<CameraController3>().phase_battle.Change(CameraController3.Battle.SKILL_BATTLE_END);
				}
			}
			break;
			// バトル終了
			case Battle.BATTLE_END:{
				battle.Change(Battle.NONE);

				GameData.GetPlayer().GetComponent<FPlayerController>().state.Change(FPlayerController.State.FLICK_MOVE);
				GameData.GetEnemy().GetComponent<FEnemyController>().state.Change(FEnemyController.State.ASCENSION);
				GameData.GetCamera().GetComponent<CameraController3>().phase.Change(CameraController3.CameraPhase.NORMAL);
			}
			break;
		}
		battle.Update();
	}

	void LateUpdate(){
		// バトルモード条件
		if (battle.current == Battle.NONE && GameData.GetCamera().GetComponent<CameraController3>().IsChangePullCamera()){
			battle.Change(Battle.BATTLE_START);

			GameData.GetPlayer().GetComponent<FPlayerController>().state.Change(FPlayerController.State.BATTLE);
			GameData.GetEnemy().GetComponent<FEnemyController>().state.Change(FEnemyController.State.BATTLE);
			GameData.GetCamera().GetComponent<CameraController3>().phase.Change(CameraController3.CameraPhase.BATTLE);
		}
	}
}
