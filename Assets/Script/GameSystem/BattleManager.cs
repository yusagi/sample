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
		SKILL_BATTLE_PLAY,
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

#endregion

#region メンバ変数

	// 状態
	public Phase<Battle> battle = new Phase<Battle>();
	public Phase<ResultPhase> resultPahse = new Phase<ResultPhase>(); 

	// フラグ
	public bool skillBattleStartEnd{get;set;}
    public bool skillBattlePlayEnd { get; set; }
	private bool battleStart{get;set;}

    public Dictionary<int, bool> battlePlayflags = new Dictionary<int, bool>();

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
	public bool DBG_IS_SKILLBATTLE_RESULT_STOP = true;
	public bool DBG_IS_CAMERA_STOP = false;
	public bool DBG_BATTLE_START = false;
	public float DBG_SKILL_ANIMATION_TIME = 3.0f;
    public float DBG_ACTION_JAMP = 10.0f;
	public float DBG_PLAY_DISTANCE = 3.0f;

	public UnityEngine.UI.Text hps;
	public UnityEngine.UI.Text currentAP;

#endregion

	public static void BattleForceEnd(){
		switch(_instance.battle.current){
			case Battle.BATTLE_ENCOUNT:{
				_instance.BattleEncountFin();
			}
			break;
			case Battle.SKILL_BATTLE_START:{
				_instance.SkillBattleChoiceFin();
			}
			break;
			case Battle.SKILL_BATTLE_PLAY:{
				_instance.SkillBattlePlayFin();
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
        skillBattlePlayEnd = false;
		battleStart = false;
        battlePlayflags.Clear();
        slowSpeed = SLOW_START;
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
		SKILLBATTLE_ANIMATION_TIME = DBG_SKILL_ANIMATION_TIME;
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
                        for (int i = 0; i < MAX_CHOICES; i++)
                        {
                            var card = player.skillManager.GetSkillCards()[Random.Range(0, pCardNum)];
                            GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
                            instance.name = card._name;
                            instance.GetComponent<SkillCardUI>().m_Data = card;
                            skillChoiceBoard.AddCardObject(instance);
                        }


                        // プレイヤーのAPを設定
                        skillChoiceBoard.m_PlayerAP = (int)(GameData.GetPlayer().GetComponent<PlayerController>().rigidbody.GetSpeed() * 3.6f);

                        // カード選択コルーチンスタート
                        StartCoroutine(BattleSlow());
				}

				// APUI表示
				currentAP.text = skillChoiceBoard.m_PlayerAP.ToString() + "AP";

				// スキル選択へ
				if (skillBattleStartEnd){

					BattleBoardData.skillChoiceBoard.SetActive(false);
					skillBattleStartEnd = false;

					if (BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().GetChoices(SkillChoiceBoardController.DataType.PLAYER).Count > 0)
					{
						// 敵カード選択
						EnemyController enemy = GameData.GetEnemy().GetComponent<EnemyController>();
						int eCardNum = enemy.skillManager.GetSkillCards().Count;
						for (int i = 0; i < MAX_CHOICES; i++)
						{
							BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().AddChoice(enemy.skillManager.GetSkillCards()[Random.Range(0, eCardNum)], SkillChoiceBoardController.DataType.ENEMY);
						}

						// バトル状態設定
						Vector3 line = GameData.GetEnemy().position - GameData.GetPlayer().position;
						Vector3 halfPos = GameData.GetPlayer().position + (line*0.5f);
						Vector3 up = (halfPos - GameData.GetPlanet().position);
						Vector3 pFront = Vector3.ProjectOnPlane(GameData.GetPlayer().forward, up).normalized;
						Vector3 eFront = Vector3.ProjectOnPlane(GameData.GetEnemy().forward, up).normalized;
						float angle = Mathf.Acos(Vector3.Dot(pFront, eFront)) * Mathf.Rad2Deg;
						if (angle <= 45.0f){
							GameData.GetEnemy().GetComponent<EnemyController>().battle = GameData.GetEnemy().GetComponent<EnemyController>().BackBattle();
						}
						else{
							GameData.GetEnemy().GetComponent<EnemyController>().battle = GameData.GetEnemy().GetComponent<EnemyController>().FrontBattle();
						}
						GameData.GetPlayer().GetComponent<PlayerController>().battle = GameData.GetPlayer().GetComponent<PlayerController>().FrontBattle();
						Debug.Log(angle);	

						battle.Change(Battle.SKILL_BATTLE_PLAY);
						battle.Start();
						return;
					}
					else{
						battle.Change(Battle.SKILL_BATTLE_END);
						battle.Start();
						return;
					}

				}
			}
			break;
			// スキルバトルプレイ
			case Battle.SKILL_BATTLE_PLAY:{
				// バトル演出

				if (battle.IsFirst()){
					BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().Battle();
                        resultPahse.Change(ResultPhase.FIRST);
                        resultPahse.Start();
                    }

				if (battlePlayflags.ContainsValue(false)){
					if (GameData.GetPlayer().GetComponent<PlayerController>().isActionStart && GameData.GetEnemy().GetComponent<EnemyController>().isActionStart)
					{
						ResultUpdate();
					}
				}
				//  SKILL_BATTLE_ENDへ移行
				else {
					SkillBattlePlayFin();
					battle.Change(Battle.BATTLE_END);
					battle.Start();
					return;
				}
			}
			break;
			// スキルバトル終了
			case Battle.SKILL_BATTLE_END:{
				 if (slowSpeed >= 1.0f){
					battle.Change(Battle.BATTLE_END);
					battle.Start();
					return;
				 }
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
		if (battleStart == true){
			return;
		}

if (Input.GetKeyDown(KeyCode.Space))
	DBG_BATTLE_START = !DBG_BATTLE_START;
		// どのカメラタイプでエリア指定するか
		bool tmpIsBattle = tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInCameraFramework();

		battleStart = tmpIsBattle && battleInterval < 0;

		// バトルモード条件
		if (battleStart){
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
			if (battle.current == Battle.BATTLE_ENCOUNT || battle.current == Battle.SKILL_BATTLE_START){
				// 強制終了
				BattleForceEnd();
			}
		}
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
        skillBattlePlayEnd = false;
		battlePlayflags.Clear();
	}
	
	// BATTLE_END終了処理
	void BattleEndFin(){
		BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().End();
		battleInterval = BATTLE_START_INTERVAL;
		battleStart = false;
	}

	// BATTLE_FORCE_END終了処理
	void BattleForceEndFin(){
		
	}

	// バトルリザルト更新
	void ResultUpdate(){
		switch(resultPahse.current){
			case ResultPhase.FIRST:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm()  && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					resultPahse.Change(ResultPhase.SECOND);
					resultPahse.Start();
					return;
				}
			}
			break;
			case ResultPhase.SECOND:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm()   && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					resultPahse.Change(ResultPhase.THIRD);
					resultPahse.Start();
					return;
				}
			}
			break;
			case ResultPhase.THIRD:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm()   && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
					resultPahse.Change(ResultPhase.FOURTH);
					resultPahse.Start();
					return;
				}
			}
			break;
			case ResultPhase.FOURTH:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().animator.IsEndAllAnm()   && GameData.GetEnemy().GetComponent<EnemyController>().animator.IsEndAllAnm()){
                        //skillBattleResultEnd = true;
                        GameData.GetPlayer().GetComponent<PlayerController>().isAction = true;
                        GameData.GetEnemy().GetComponent<EnemyController>().isAction =  true;

                    return;
				}
			}
			break;
		}
		resultPahse.Update();
	}

	// バトル中スローコルーチン
	IEnumerator BattleSlow(){
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

        skillBattleStartEnd = true;
		yield return null;

		// 加速スロー
        speed = UtilityMath.FLerp(SLOW_END, SLOW_END2, SLOW_END_TIME, EaseType.OUT_EXP);
        while (speed.MoveNext())
        {
            slowSpeed = speed.Current;
            yield return null;
        }

        slowSpeed = 1.0f;
	}

	// バトルカード選択中コルーチン
	IEnumerator Choice(){
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

	}

    // バトルプレイ中コルーチン
    IEnumerator Play(){

        // 加速スロー
        IEnumerator<float> speed = UtilityMath.FLerp(SLOW_END, SLOW_END2, SLOW_END_TIME, EaseType.OUT_EXP);
        while (speed.MoveNext())
        {
            slowSpeed = speed.Current;
            yield return null;
        }

        slowSpeed = 1.0f;
    }
}
