using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {

#region static変数
	public static BattleManager _instance;

	public static float BATTLE_ENCOUNT_TIME = 0.0f;
	public static float SKILLBATTLE_PLAY_TIME = 0.5f;
	public static float SKILLBATTLE_RESULT_TIME = 1.0f;
	public static float SKILLBATTLE_CHOICE_TIME = 5.0f;
	public static float BATTLE_START_INTERVAL = 4.0f;
	public static float SKILLBATTLE_ANIMATION_TIME = 3.0f;
	
	public static int MAX_CHOICES = 4;
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

	public enum ResultPhase{
		FIRST = 0,
		SECOND = 1,
		THIRD = 2,
		FOURTH = 3,
	}

	enum SkillChoicePoint{
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

	// 状態
	public Phase<Battle> battle = new Phase<Battle>();
	public Phase<ResultPhase> resultPahse = new Phase<ResultPhase>(); 

	// フラグ
	public bool skillBattleStartEnd{get;set;}
	public bool skillBattleResultEnd{get;set;}

	// タイマー
	float battleInterval = -1;

	// スロー
	public float SLOW_START = 0.3f;
	public float SLOW_END = 0.01f;
	public float SLOW_END2 = 0.5f;
	public float SLOW_START_TIME = 2.0f;
	public float SLOW_KEEP_TIME = 1.0f;
	public float SLOW_END_TIME = 2.0f;
	public float slowSpeed{get;set;}

	// デバグ
	public float DBG_SKILLBATTLE_PLAY_TIME = 1.0f;
	public DBGCameraType DBG_BATTLE_CAMERA_TYPE = DBGCameraType.NORMAL;
	public bool DBG_IS_SKILLBATTLE_RESULT_STOP = true;
	public bool DBG_IS_CAMERA_STOP = false;
	public float time = 3.0f;
	public UnityEngine.UI.Text hps;
	public UnityEngine.UI.Text currentAP;

#endregion

	public static void BattleForceEnd(){
		switch(_instance.battle.current){
			case Battle.BATTLE_ENCOUNT:{
				_instance.BattleEncountFin();
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
		skillBattleStartEnd = false;
		skillBattleResultEnd = false;
		BattleBoardData.Initialize();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (battleInterval > 0){
			battleInterval -= Time.deltaTime;
		}
		SKILLBATTLE_ANIMATION_TIME = time;
		SKILLBATTLE_PLAY_TIME = DBG_SKILLBATTLE_PLAY_TIME;
		hps.text = "Player " + GameData.GetPlayer().GetComponent<PlayerController>().hp + "   " + "Enemy " + GameData.GetEnemy().GetComponent<EnemyController>().hp;
	}

	void LateUpdate(){

		BattleUpdate();

		RushContinuesCheck();
	}

	// Battle更新
	void BattleUpdate(){
		switch(battle.current){
			// 強制終了
			case Battle.BATTLE_FORCE_END:{
				battle.Change(Battle.BATTLE_END);
				battle.Start();
				return;
			}
			// バトルエンカウント
			case Battle.BATTLE_ENCOUNT:{
				// エンカウント演出

				if (battle.IsFirst()){
					BattleBoardData.battle_Encount.SetActive(true);
					BattleBoardData.skillChoiceBoard.SetActive(true);
				}

				// SKILL_BATTLE_STARTへ移行
				if (battle.phaseTime >= BATTLE_ENCOUNT_TIME){
					BattleEncountFin();
					battle.Change(Battle.SKILL_BATTLE_START);
					battle.Start();
					return;
				}
			}
			break;
			// スキルバトル開始
			case Battle.SKILL_BATTLE_START:{
				// スキル表示演出
				SkillChoiceBoardController skillChoiceBoard = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
				if (battle.IsFirst()){
					// プレイヤーカード生成
					PlayerController player = GameData.GetPlayer().GetComponent<PlayerController>();
					int pCardNum = player.skillManager.GetSkillCards().Count;
					for (int i = 0; i < MAX_CHOICES; i ++){
						var card = player.skillManager.GetSkillCards()[Random.Range(0, pCardNum)];
						GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
						instance.name = card._name;
						instance.GetComponent<SkillCardUI>().m_Data = card;
						skillChoiceBoard.AddCardObject(instance);
					}

					// 敵カード選択
					EnemyController enemy = GameData.GetEnemy().GetComponent<EnemyController>();
					int eCardNum = enemy.skillManager.GetSkillCards().Count;
					for (int i = 0; i < MAX_CHOICES; i++){
						skillChoiceBoard.AddChoice(enemy.skillManager.GetSkillCards()[Random.Range(0, eCardNum)], SkillChoiceBoardController.DataType.ENEMY);
					}

					// プレイヤーのAPを設定
					skillChoiceBoard.m_PlayerAP = (int)(GameData.GetPlayer().GetComponent<PlayerController>().rigidbody.GetSpeed() * 3.6f);

					// カード選択コルーチンスタート
					StartCoroutine(ChoiceCard());
				}

				// APUI表示
				currentAP.text = skillChoiceBoard.m_PlayerAP.ToString() + "AP";

				// スキル選択へ
				if (skillBattleStartEnd){
					BattleBoardData.skillChoiceBoard.SetActive(false);
					skillBattleStartEnd = false;
					battle.Change(Battle.BATTLE_END);
					battle.Start();
					return;
				}
			}
			break;
			// スキル選択
			case Battle.SKILL_BATTLE_CHOICE:{
				// スキルチョイスボードコントローラー取得
				SkillChoiceBoardController skillChoiceBoard = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
				bool fullChoice = skillChoiceBoard.IsFullChoice();

				// スキル選択演出
				if (battle.IsFirst()){

					// 敵カード選択
					EnemyController enemy = GameData.GetEnemy().GetComponent<EnemyController>();
					int cardNum = enemy.skillManager.GetSkillCards().Count;
					for (int i = 0; i < MAX_CHOICES; i++){
						skillChoiceBoard.AddChoice(enemy.skillManager.GetSkillCards()[Random.Range(0, cardNum)], SkillChoiceBoardController.DataType.ENEMY);
					}

					// プレイヤーのAPを設定
					skillChoiceBoard.m_PlayerAP = (int)(GameData.GetPlayer().GetComponent<PlayerController>().rigidbody.GetSpeed() * 3.6f);
				}
				
				currentAP.text = skillChoiceBoard.m_PlayerAP.ToString() + "AP";


				if (battle.phaseTime >= SKILLBATTLE_CHOICE_TIME || fullChoice){
					SkillBattleChoiceFin();
					battle.Change(Battle.SKILL_BATTLE_PLAY);
					battle.Start();
					return;
					
				}
				
			}
			break;
			// スキルバトルプレイ
			case Battle.SKILL_BATTLE_PLAY:{
				// バトル演出

				if (battle.IsFirst()){
					BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().Battle();
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
				if (battle.IsFirst()){
					SkillChoiceBoardController skillChoice = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
					resultPahse.Change(ResultPhase.FIRST);
					resultPahse.Start();
					break;
				}
		
				ResultUpdate();

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
	
	// バトルモード突入、継続チェック
	void RushContinuesCheck(){
		// バトルモード中ならリターン
		if (battle.current != Battle.NONE){
			return;
		}

		// どのカメラタイプでエリア指定するか
		bool tmpIsBattle = false;
		switch(DBG_BATTLE_CAMERA_TYPE){
			case DBGCameraType.NORMAL: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInNormalCameraFramework(); break;
			case DBGCameraType.PULL: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInPullCameraFramework(); break;
			case DBGCameraType.LOOKAT: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInNormalCameraFramework(); break;
			case DBGCameraType.STOP: tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInCameraFramework(); break;
			default: Debug.LogError("out of ragne DBG_BATTLE_CAMERA_TYPE"); break;
		}

		bool battleStart = tmpIsBattle && battleInterval < 0;

		// バトルモード条件
		if (battleStart){
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
	}

	// バトルリザルト更新
	void ResultUpdate(){
		switch(resultPahse.current){
			case ResultPhase.FIRST:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm() && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					resultPahse.Change(ResultPhase.SECOND);
					resultPahse.Start();
					return;
				}
			}
			break;
			case ResultPhase.SECOND:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm() && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					resultPahse.Change(ResultPhase.THIRD);
					resultPahse.Start();
					return;
				}
			}
			break;
			case ResultPhase.THIRD:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm() && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					resultPahse.Change(ResultPhase.FOURTH);
					resultPahse.Start();
					return;
				}
			}
			break;
			case ResultPhase.FOURTH:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm() && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					skillBattleResultEnd = true;
					return;
				}
			}
			break;
		}
		resultPahse.Update();
	}

	// バトルカード選択中コルーチン
	IEnumerator ChoiceCard(){
		// 減速スロー
		IEnumerator<float> speed = UtilityMath.FLerp(SLOW_START, SLOW_END, SLOW_START_TIME, EaseType.OUT_EXP);
		while(speed.MoveNext()){
			slowSpeed = speed.Current;
			yield return null;
		}
		
		// 最遅スロー維持
		float time = 0.0f;
		while (time < SLOW_KEEP_TIME){
			time += Time.deltaTime;
			yield return null;
		}

		// 加速スロー
		speed = UtilityMath.FLerp(SLOW_END, SLOW_END2, SLOW_END_TIME, EaseType.OUT_EXP);
		while(speed.MoveNext()){
			slowSpeed = speed.Current;
			yield return null;
		}

		skillBattleStartEnd = true;
	}

	// BATTLE_START終了処理
	void BattleEncountFin(){
		BattleBoardData.battle_Encount.gameObject.SetActive(false);
	}

	// SKILL_BATTLE_CHOICE終了処理
	void SkillBattleChoiceFin(){
		BattleBoardData.skillChoiceBoard.SetActive(false);
	}

	// SKILL_BATTLE_PLAY終了処理
	void SkillBattlePlayFin(){
		GameData.GetBattleBoard().FindChild("SkillBattleBoard").gameObject.SetActive(false);
	}

	// SKILL_BATTLE_RESULT終了処理
	void SkillBattleResultFin(){
		skillBattleResultEnd = false;
	}

	// BATTLE_END終了処理
	void BattleEndFin(){
		BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().End();
		battleInterval = BATTLE_START_INTERVAL;
	}

	// BATTLE_FORCE_END終了処理
	void BattleForceEndFin(){
		
	}
}
