using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {

#region static変数
	public static BattleManager _instance;

	public static float BATTLE_ENCOUNT_TIME = 0.5f;
	public static float SKILLBATTLE_PLAY_TIME = 1.0f;
	public static float SKILLBATTLE_RESULT_TIME = 1.0f;
	public static float SKILLBATTLE_CHOICE_TIME = 5.0f;
	public static float BATTLE_START_INTERVAL = 3.0f;
#endregion

#region enum

	public enum Battle{
		BATTLE_ENCOUNT,
		SKILL_BATTLE_START,
		SKILL_BATTLE_CHOICE,
		SKILL_BATTLE_PLAY,
		SKILL_BATTLE_RESULT,
		SKILL_BATTLE_END,
		BATTLE_END,
		BATTLE_FORCE_END, // 強制終了
		NONE,
	}

	enum SkillChoiceFlick{
		POINT1 	= 0,
		POINT2  = 1,
		POINT3  = 2,
		POINT4	= 3,
		NONE,
	}

	public enum DBGCameraType{
		NORMAL,
		PULL,
		LOOKAT,
		STOP,
	}

#endregion

#region メンバ変数

	// スキルバトル用
	List<GameObject> skillCards = new List<GameObject>();

	// 状態
	public Phase<Battle> battle = new Phase<Battle>();
	SkillChoiceFlick choiceSkill = SkillChoiceFlick.NONE;
	SkillChoiceFlick eChoiceSkill = SkillChoiceFlick.NONE;

	// フラグ
	public bool skillBattleResultEnd{get;set;}

	// タイマー
	float battleInterval = -1;

	// スロー
	public float slowTimer{get;set;}
	public float SLOW_TOTAL_TIME = 5.0f;
	public float SLOW_TIME = 5.0f;
	public float SLOW_START = 1f;
	public float SLOW_END = 0.01f;
	public float SLOW_TIME2 = 5.0f;
	public float SLOW_START2 = 1f;
	public float SLOW_END2 = 0.01f;

	public EaseType slowType = EaseType.LINEAR;

	// デバグ
	public float DBG_SKILLBATTLE_PLAY_TIME = 1.0f;
	public DBGCameraType DBG_BATTLE_CAMERA_TYPE = DBGCameraType.NORMAL;
	public bool DBG_IS_SKILLBATTLE_RESULT_STOP = true;
	public bool DBG_IS_CAMERA_STOP = false;
	

#endregion

	public static void BattleForceEnd(){
		switch(_instance.battle.current){
			case Battle.BATTLE_ENCOUNT:{
				_instance.BattleStartFin();
			}
			break;
			case Battle.SKILL_BATTLE_START:
			case Battle.SKILL_BATTLE_CHOICE:{
				_instance.SkillBattleChoiceFin();
			}
			break;
			case Battle.SKILL_BATTLE_PLAY:{
				_instance.SkillBattlePlayFin();
			}
			break;
			case Battle.SKILL_BATTLE_RESULT:{
				_instance.SkillBattleResultFin();
			}
			break;
		}

		_instance.battle.Change(Battle.BATTLE_FORCE_END);
		_instance.battle.Start();
	}

	void Awake(){
		_instance = this;
		battle.Change(Battle.NONE);
		skillBattleResultEnd = false;
		BattleBoardData.Initialize();
		slowTimer = SLOW_TOTAL_TIME;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (battleInterval > 0){
			battleInterval -= Time.deltaTime;
		}
	}

	void LateUpdate(){
		SKILLBATTLE_PLAY_TIME = DBG_SKILLBATTLE_PLAY_TIME;

		BattleUpdate();

		RushContinuesCheck();
	}

	// Battle更新
	void BattleUpdate(){
		// タイマー更新
		if (battle.current != Battle.NONE){
			slowTimer = Mathf.Max(slowTimer - Time.deltaTime, 0);
		}

		switch(battle.current){
			// 強制終了
			case Battle.BATTLE_FORCE_END:{
				battle.Change(Battle.BATTLE_END);
				battle.Start();
				return;
			}
			break;
			// バトルエンカウント
			case Battle.BATTLE_ENCOUNT:{
				// エンカウント演出

				if (battle.IsFirst()){
					//BattleBoardData.battle_Encount.SetActive(true);
				}

				// SKILL_BATTLE_STARTへ移行
				if (battle.phaseTime >= BATTLE_ENCOUNT_TIME){
					BattleStartFin();
					battle.Change(Battle.SKILL_BATTLE_START);
					battle.Start();
					//BattleBoardData.skillChoiceBoard.SetActive(true);
					return;
				}
			}
			break;
			// スキルバトル開始
			case Battle.SKILL_BATTLE_START:{
				// スキル表示演出

				if (battle.IsFirst()){
					Transform skillChoiceBoard = BattleBoardData.skillChoiceBoard.transform;
					choiceSkill = SkillChoiceFlick.NONE;
					int i = 0;
					foreach(var card in GameData.GetPlayer().GetComponent<PlayerController>().skillManager.GetSkillCards()){
						GameObject skillCard = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
						string name = ((SkillChoiceFlick)i).ToString();
						skillCard.transform.SetParent(skillChoiceBoard.FindChild(name), false);
						skillCard.GetComponentInChildren<UnityEngine.UI.Text>().text = card._name;
						skillCards.Add(skillCard);
						i++;
					}
				}

				// スキル選択へ
				if (BattleBoardData.skillChoiceBoard.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime > 1){
					battle.Change(Battle.SKILL_BATTLE_CHOICE);
					battle.Start();
					return;
				}
			}
			break;
			// スキル選択
			case Battle.SKILL_BATTLE_CHOICE:{
				// スキル選択演出
				if (battle.IsFirst()){
					// タイマー表示
					BattleBoardData.battleTimer.gameObject.SetActive(true);
				}
				

				// // スキル選択へ
				// if (TouchController.GetTouchType() == TouchController.TouchType.Frick){
				// 	Vector3 flickvel = Vector3.ProjectOnPlane(TouchController.GetFlickVelocity(), Vector3.up).normalized;
				// 	Vector3 isDown = Vector3.ProjectOnPlane(flickvel, Vector3.right).normalized;
				// 	if (Vector3.Dot(Vector3.forward, isDown) != 1){
				// 		choiceSkill = SkillChoiceFlick.DOWN;
				// 	}
				// 	// Vector3.rightからのアングルでフリック方向を決定
				// 	float angle = Mathf.Acos(Vector3.Dot(flickvel, Vector3.right)) * Mathf.Rad2Deg;
				// 	// 右
				// 	if (angle < 45.0f){
				// 		choiceSkill = SkillChoiceFlick.RIGHT;
				// 	}
				// 	// 上
				// 	else if (angle < 135.0f){
				// 		if (choiceSkill == SkillChoiceFlick.DOWN){
				// 			break;
				// 		}
				// 		choiceSkill = SkillChoiceFlick.UP;
				// 	}
				// 	// 左
				// 	else choiceSkill = SkillChoiceFlick.LEFT;
				// }

				BattleBoardData.battleTimer.text = Mathf.RoundToInt(SKILLBATTLE_CHOICE_TIME - battle.phaseTime).ToString();

				if (battle.phaseTime >= SKILLBATTLE_CHOICE_TIME){
					// SKILL_BATTLE_PLAYへ移行
					if (choiceSkill != SkillChoiceFlick.NONE){
						SkillBattleChoiceFin();
						battle.Change(Battle.SKILL_BATTLE_PLAY);
						battle.Start();
						return;
					}
					else{
						BattleForceEnd();
					}
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
					SkillData pSkill = GameData.GetPlayer().GetComponent<PlayerController>().skillManager.GetSkillCards()[pChoiceNum];
					pCard.GetComponentInChildren<UnityEngine.UI.Text>().text = pSkill._name;
					Transform playerPoint = skillBattleBoard.FindChild("PlayerPoint");
					pCard.transform.SetParent(playerPoint, false);

					// エネミーカード作成
					GameObject eCard = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
					int eChoiceNum = Random.Range(0, 3);
					SkillData eSkill = GameData.GetEnemy().GetComponent<EnemyController>().skillManager.GetSkillCards()[eChoiceNum];
					eCard.GetComponentInChildren<UnityEngine.UI.Text>().text = eSkill._name;
					eCard.transform.SetParent(skillBattleBoard.FindChild("EnemyPoint"), false);
					
					skillCards.Add(pCard);
					skillCards.Add(eCard);

					// 勝負結果
					//GameData.GetPlayer().GetComponent<PlayerController>().skillManager.result = SkillManager.GetSkillResult(pSkill._type, eSkill._type);
					//GameData.GetEnemy().GetComponent<EnemyController>().skillManager.result = SkillManager.GetSkillResult(eSkill._name, pSkill._type);
				}


				//  SKILL_BATTLE_RESULTへ移行
				if (battle.phaseTime >= SKILLBATTLE_PLAY_TIME){
					SkillBattlePlayFin();
					battle.Change(Battle.SKILL_BATTLE_RESULT);
					battle.Start();
					return;
				}
			}
			break;
			// スキルバトル結果
			case Battle.SKILL_BATTLE_RESULT:{
				// BATTLE_ENDへ移行
				if (skillBattleResultEnd == true){
					SkillBattleResultFin();
					battle.Change(Battle.BATTLE_END);
					battle.Start();
					return;
				}
			}
			break;
			// スキルバトル終了
			case Battle.SKILL_BATTLE_END:{
			}
			break;
			// バトル終了
			case Battle.BATTLE_END:{
				BattleEndFin();
				battle.Change(Battle.NONE);
				battle.Start();
			}
			break;
		}
		battle.Update();
	}
public bool a = false;
	// バトルモード突入、継続チェック
	void RushContinuesCheck(){

if (Input.GetKeyDown(KeyCode.Space))
	a = !a;
		// どのカメラタイプでエリア指定するか
		bool tmpIsBattle = false;
		switch(DBG_BATTLE_CAMERA_TYPE){
			case DBGCameraType.NORMAL: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInNormalCameraFramework(); break;
			case DBGCameraType.PULL: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInPullCameraFramework(); break;
			case DBGCameraType.LOOKAT: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInNormalCameraFramework(); break;
			case DBGCameraType.STOP: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInCameraFramework(); break;
			default: Debug.LogError("out of ragne DBG_BATTLE_CAMERA_TYPE"); break;
		}

		// バトルモード条件
		if (a){
			// バトルモード中ならリターン
			if (battle.current != Battle.NONE){
				return;
			}
			if (GameData.GetEnemy().GetComponent<EnemyController>().state.current == EnemyController.State.ASCENSION)
				return;
			if (GameData.GetPlayer().GetComponent<PlayerController>().state.current == PlayerController.State.ASCENSION)
				return;
			
			// バトルエンカウント
			battle.Change(Battle.BATTLE_ENCOUNT);
			battle.Start();

			// プレイヤー、エネミー、カメラがバトルに突入
			GameData.GetPlayer().GetComponent<PlayerController>().state.Change(PlayerController.State.BATTLE);
			GameData.GetEnemy().GetComponent<EnemyController>().state.Change(EnemyController.State.BATTLE);
			GameData.GetCamera().GetComponent<CameraController>().phase.Change(CameraController.CameraPhase.BATTLE);
		}
		else{
			// 特定フェーズ
			if (battle.current == Battle.BATTLE_ENCOUNT || battle.current == Battle.SKILL_BATTLE_START || battle.current == Battle.SKILL_BATTLE_CHOICE){
				// 強制終了
				BattleForceEnd();
			}

			// バトルリザルト
			if (battle.current == Battle.SKILL_BATTLE_RESULT){
				// 引き分け
				if (GameData.GetPlayer().GetComponent<PlayerController>().skillManager.result == SkillAttributeResult.DRAW){
					BattleForceEnd();
				}
			}
		}
	}
	
	// BATTLE_START終了処理
	void BattleStartFin(){
		BattleBoardData.battle_Encount.gameObject.SetActive(false);
	}

	// SKILL_BATTLE_CHOICE終了処理
	void SkillBattleChoiceFin(){
		foreach(var card in skillCards){
			GameObject.Destroy(card);
		}
		BattleBoardData.skillChoiceBoard.SetActive(false);
		BattleBoardData.battleTimer.gameObject.SetActive(false);
	}

	// SKILL_BATTLE_PLAY終了処理
	void SkillBattlePlayFin(){
		foreach(var card in skillCards){
			GameObject.Destroy(card);
		}
		GameData.GetBattleBoard().FindChild("SkillBattleBoard").gameObject.SetActive(false);
	}

	// SKILL_BATTLE_RESULT終了処理
	void SkillBattleResultFin(){
		skillBattleResultEnd = false;
	}

	// BATTLE_END終了処理
	void BattleEndFin(){
		battleInterval = BATTLE_START_INTERVAL;
		slowTimer = SLOW_TOTAL_TIME;
	}

	// BATTLE_FORCE_END終了処理
	void BattleForceEndFin(){
		
	}
}
