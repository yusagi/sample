using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
	#region enum
	// バトルフェーズ
	public enum Battle
	{
		BATTLE_ENCOUNT,
		SKILL_BATTLE_START,
		SKILL_BATTLE_PLAY,
		BATTLE_END,
		NONE,
	}

	// リザルトフェーズ
	public enum ResultPhase
	{
		FIRST = 0,
		SECOND = 1,
		THIRD = 2,
		FOURTH = 3,
		END,
	}

	// スロー状態
	public enum TimePhase{
		SLOW,
		SLOW_END,
		KEEP,
		QUICK,
		QUICK_END
	}

	// プレイヤーと相手との距離
	public enum DistanceType{
		FAR,
		NEAR,
	}
	#endregion

	#region メンバ変数

	private Transform m_Player;	// プレイヤー
	private Transform m_Target;	// 対戦相手

	// 状態
	public Phase<Battle> m_Battle = new Phase<Battle>();
	public Phase<ResultPhase> m_ResultPhase = new Phase<ResultPhase>();
    private TimePhase m_TimePhase;

	// フラグ
	private bool m_BattleStart { get; set; }
    private bool m_SlowStart = false;
    private bool m_SlowEnd = false;

	// タイマー
	float m_BattleInterval = -1;	// インターバルのタイマー
	float m_MoreSlowTimer;			// よりスロータイマー
	float m_DelayUiSlideInTimer;	// スキルUIが再びスライドインするまでの遅延タイマー

	// スロー
	public float SLOW_START1 = 0.3f;
	public float SLOW_END1 = 0.01f;
	public float SLOW_END2 = 0.3f;
	public float SLOW_TIME1 = 1.0f;
	public float SLOW_KEEP_TIME = 2.0f;
	public float SLOW_TIME2 = 0.5f;

	// コルーチン
	private Coroutine m_BattleSlow;
	
	// 各種定数
	public float BATTLE_PLAY_DISTANCE;		// バトルモードで接敵した時の止まる距離
	public float BATTLE_START_DISTANCE;		// バトルモード開始の索敵距離
	public float SLOW = 0.1f; 						// 通常スローの値
	public float MORE_SLOW = 0.01f;					// よりスローの値
	public float MORE_SLOW_TIME = 0.5f; 			// よりスローである時間
	public float DELAY_UI_SLIDEIN_TIME = 0.6f; 		// スキルUIが再びスライドインするまでの遅延時間
	public float UI_OUT_TIME = 0.2f;				// UIがはけるまでの時間

	private int MAX_CHOICES = 4;	// スキルカードの選択肢の数
	private int MAX_SELECTS = 1;	// 選べるスキル数

	private float BATTLE_START_INTERVAL = 4.0f;	// 次のバトルを開始できるまでのインターバル

	// テキスト
	public UnityEngine.UI.Text hps;
	public UnityEngine.UI.Text currentAP;

	#endregion

	// Use this for initialization
	void Start()
	{
		m_Player = GameManager.m_Player.transform;
		m_Target = GameManager.m_Enemy.transform;

		//_instance = this;
		m_Battle.Change(Battle.NONE);
		m_ResultPhase.Change(ResultPhase.END);
		m_BattleStart = false;
		BattleBoardData.Initialize();

		m_MoreSlowTimer = 0.0f;
		m_DelayUiSlideInTimer = 0.0f;
	}

	// Update is called once per frame
	void Update()
	{
		if (m_BattleInterval > 0)
		{
			m_BattleInterval -= Time.deltaTime;
		}
		hps.text = "Player " + m_Player.GetComponent<GrgrCharCtrl>().hp + "   " + "Enemy " + m_Target.transform.GetComponent<GrgrCharCtrl>().hp;
	}

	void LateUpdate()
	{
		BattleUpdate();

		RushContinuesCheck();
	}

	// 2キャラの距離を測る
	public DistanceType GetObjectsDistance(){
		// 距離を求める
		Vector3 center = GameManager.m_Planet.transform.position;
		float radius = GameManager.m_Planet.transform.localScale.y * 0.5f;
		float distance = Rigidbody_grgr.Arc(m_Player.position, m_Target.position, center, radius);

		// 距離タイプ
		DistanceType tmpDistanceType;
		if (distance <= BATTLE_PLAY_DISTANCE){
			tmpDistanceType = DistanceType.NEAR;
		}
		else{
			tmpDistanceType = DistanceType.FAR;
		}

		return tmpDistanceType;
	}

	// エンカウントUIコルーチン
	IEnumerator EncountUI(){
		// エンカウントUI表示
		GameObject encount = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI_Encount"));
		encount.transform.SetParent(GameData.GetCanvas(), false);

		while(m_Battle.current == Battle.BATTLE_ENCOUNT){
			yield return null;
		}

		GameObject.Destroy(encount);
	}

	// 接敵スローポイントの生成コルーチン
	IEnumerator SlowAreaCreate(Quaternion rot){
		// スロー領域の表示
		GameObject area = new GameObject("SLOW_AREA");
		area.transform.rotation = rot;
		BattleAreaLineRenderer areaLine = area.AddComponent<BattleAreaLineRenderer>();
		areaLine.arc = BATTLE_PLAY_DISTANCE;
		areaLine.SetColor(Color.red);
		while(m_Battle.current != Battle.BATTLE_END){
					yield return null;
		}
		GameObject.Destroy(area);
	}

	// Battle更新
	void BattleUpdate()
	{
		if (m_Battle.current == Battle.NONE){
			return;
		}

		// APUI表示
		currentAP.text = BattleBoardData.skillChoiceBoard.m_PlayerAP.ToString() + "AP";

		switch (m_Battle.current)
		{
			// バトルエンカウント
			case Battle.BATTLE_ENCOUNT:
				{
					// エンカウント演出

					if (m_Battle.IsFirst())
					{
						// エンカウントUI表示
						StartCoroutine(EncountUI());
						// プレイヤーのAPを設定
						BattleBoardData.skillChoiceBoard.m_PlayerAP = (int)(m_Player.GetComponent<GrgrCharCtrl>().rigidbody.GetSpeed() * 3.6f);
						// 相手のAPを設定
						BattleBoardData.skillChoiceBoard.m_TargetAP = (int)(m_Target.GetComponent<GrgrCharCtrl>().rigidbody.GetSpeed() * 3.6f);

					}
		
					// SKILL_BATTLE_STARTへ移行(相手との距離が一定値より近くなったら)
					if (GetObjectsDistance() == DistanceType.NEAR)
					{
						// プレイヤー位置を調整
						Vector3 toPlayer = (m_Player.position - m_Target.position).normalized;
						m_Player.position = m_Target.position;
						m_Player.GetComponent<GrgrCharCtrl>().GrgrMove(toPlayer * (BATTLE_PLAY_DISTANCE * 0.5f), 0, true);
						// スキルUIボードのアクティブTRUE
						BattleBoardData.skillChoiceBoard.gameObject.SetActive(true);

						m_Battle.Change(Battle.SKILL_BATTLE_START);
						m_Battle.Start();
						return;
					}
				}
				break;
			// スキルバトル開始
			case Battle.SKILL_BATTLE_START:
				{
					// スキルチョイスボードコントローラー取得
					SkillChoiceBoardController skillChoiceBoard = BattleBoardData.skillChoiceBoard;
					if (m_Battle.IsFirst())
					{
						// スキルバトル初期化
						skillChoiceBoard.BattleIni(m_Player.gameObject, m_Target.gameObject);

						// プレイヤーカード生成
						GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();
						int pCardNum = player.skillManager.GetSkillCards().Count;
						for (int i = 0; i < MAX_CHOICES; i++)
						{
							var card = player.skillManager.GetSkillCards()[Random.Range(0, pCardNum)];
							GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
							instance.name = card._name;
							instance.GetComponent<SkillCardUI>().AddCardData(card);
							skillChoiceBoard.AddCardObject(instance, SkillChoiceBoardController.USER.PLAYER);
						}

                        // 敵カード生成
                        GrgrCharCtrl target = m_Target.GetComponent<GrgrCharCtrl>();
                        int tCardNum = target.skillManager.GetSkillCards().Count;
                        for (int i = 0; i < MAX_CHOICES; i++)
                        {
                            var card = target.skillManager.GetSkillCards()[Random.Range(0, tCardNum)];
                            GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard"));
                            instance.name = card._name;
                            instance.GetComponent<SkillCardUI>().AddCardData(card);
                            skillChoiceBoard.AddCardObject(instance, SkillChoiceBoardController.USER.TARGET);
                        }

						// スロー処理(初回なのでキープタイムがカード選択時間)
						m_BattleSlow = StartCoroutine(BattleSlow(SLOW_START1, SLOW_END1, SLOW_TIME1, SLOW_KEEP_TIME, SLOW_END1, 0));
#if UNITY_EDITOR
						// ログ消去
						Dbg.ClearConsole();
#endif
					}

					// カード選択したか
					if (slowEnd == false && keepEnd == false && quickEnd){
						if (skillChoiceBoard.GetChoiceCount(m_Player.gameObject) == MAX_SELECTS){
							slowEnd = keepEnd = quickEnd = true;
						}
					}

					// スキル選択へ
					if (m_TimePhase == TimePhase.QUICK_END)
					{
						// 索敵範囲に表示を戻す
						BattleAreaLineRenderer areaLine = m_Player.GetComponent<BattleAreaLineRenderer>();
						areaLine.arc = BATTLE_START_DISTANCE;
						areaLine.SetColor(Color.yellow);
						if (skillChoiceBoard.GetChoiceCount(m_Player.gameObject) > 0)
						{
                            // 敵カード選択
                            int tCardNum = skillChoiceBoard.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
							for (int i = 0; i < MAX_SELECTS; i++)
							{
                                GameObject card = skillChoiceBoard.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
								SkillData skillData = card.GetComponent<SkillCardUI>().GetSkillData();
								skillChoiceBoard.Choice(SkillChoiceBoardController.USER.TARGET, skillData, card.transform, m_Target.gameObject);
							}

							// プレイへ移行
							m_Battle.Change(Battle.SKILL_BATTLE_PLAY);
							m_Battle.Start();
							return;
						}
						else
						{
							// バトル終了へ移行
							m_Battle.Change(Battle.BATTLE_END);
							m_Battle.Start();
							return;
						}

					}
				}
				break;
			// スキルバトルプレイ
			case Battle.SKILL_BATTLE_PLAY:
				{
					// バトル演出

					if (m_Battle.IsFirst())
					{
						BattleBoardData.skillChoiceBoard.Battle(m_Player.gameObject, m_Target.gameObject);
						m_ResultPhase.Change(ResultPhase.FIRST);
						m_ResultPhase.Start();
						break;
					}
					//GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();
					//GrgrCharCtrl target = m_Target.GetComponent<GrgrCharCtrl>();
					
					ResultUpdate();

					//  BATTLE_ENDへ
					if (m_ResultPhase.current == ResultPhase.END)
					{
						m_Battle.Change(Battle.BATTLE_END);
						m_Battle.Start();
						return;
					}
				}
				break;
			// バトル終了
			case Battle.BATTLE_END:
				{
					BattleEndFin();
					m_Battle.Change(Battle.NONE);
					m_Battle.Start();
					return;
				}
		}
		m_Battle.Update();
	}

	// バトルモード突入、継続チェック
	void RushContinuesCheck()
	{
		if (m_BattleStart == true)
		{
			return;
		}

		// どのカメラタイプでエリア指定するか
		float arc = Rigidbody_grgr.Arc(m_Player.position, m_Target.position, GameManager.m_Planet.transform.position, GameManager.m_Planet.transform.localScale.y * 0.5f);
		bool tmpIsBattle = (arc < BATTLE_START_DISTANCE);//GameData.GetCamera().GetComponent<CameraController>().IsInCameraFramework();


		// バトルモード条件
		if (tmpIsBattle && m_BattleInterval < 0)
		{
			// バトルモード中ならリターン
			if (m_Battle.current != Battle.NONE)
			{
				return;
			}
			if (m_Target.transform.GetComponent<GrgrCharCtrl>().state.current == GrgrCharCtrl.State.ASCENSION)
				return;
			if (m_Player.GetComponent<GrgrCharCtrl>().state.current == GrgrCharCtrl.State.ASCENSION)
				return;
				
			// バトル状態設定
			// ターゲット方向に線を引く
			Vector3 line = m_Target.position - m_Player.position;
			// 相手が向いてる方向をプレイヤー基準で計算
			Vector3 targetForward = Vector3.ProjectOnPlane(m_Target.forward, m_Player.up).normalized;
			// プレイヤーと相手の向きの角度差
			float angle = Mathf.Acos(Vector3.Dot(m_Player.forward, targetForward)) * Mathf.Rad2Deg;
			// GrgrCharCtrl取得
			GrgrCharCtrl target = m_Target.transform.GetComponent<GrgrCharCtrl>();
			GrgrCharCtrl player = m_Player.transform.GetComponent<GrgrCharCtrl>();
			// プレイヤーと相手がだいたい向き合ってる
			if (angle >= 45.0f){
				player.m_Battle = player.FrontBattle(target.transform);
				target.m_Battle = target.FrontBattle(player.transform);
			}
			// プレイヤーと相手がだいたい同じ方向
			else{
				// 相手が前方か後方か調べる
				Vector3 toTarget = Vector3.ProjectOnPlane(line, m_Player.up).normalized;
				float toTargetAngle = Mathf.Acos(Vector3.Dot(m_Player.forward, toTarget)) * Mathf.Rad2Deg;

				// 相手が前方
				if (toTargetAngle < 90.0f){
					player.m_Battle = player.FrontBattle(target.transform);
					target.m_Battle = target.BackBattle(player.transform);
				}
				// 相手が後方
				else{
					player.m_Battle = player.BackBattle(target.transform);
					target.m_Battle = target.FrontBattle(player.transform);
				}
			}

			// デバグ用
			player.m_Battle = player.FrontBattle(target.transform);
			target.m_Battle = target.BackBattle(player.transform);

			// スロー開始エリア生成
			StartCoroutine(SlowAreaCreate(m_Target.transform.rotation));

			// バトルエンカウント
			m_Battle.Change(Battle.BATTLE_ENCOUNT);
			m_Battle.Start();

			// プレイヤー、エネミー、カメラがバトルに突入
			m_Player.GetComponent<GrgrCharCtrl>().state.Change(GrgrCharCtrl.State.BATTLE);
			m_Target.transform.GetComponent<GrgrCharCtrl>().state.Change(GrgrCharCtrl.State.BATTLE);
			GameData.GetCamera().GetComponent<CameraController>().phase.Change(CameraController.CameraPhase.BATTLE);

			// バトルスタート
			m_BattleStart = true;
		}
	}

	// BATTLE_END終了処理
	void BattleEndFin()
	{
		Time.timeScale = 1.0f;
		BattleBoardData.skillChoiceBoard.gameObject.SetActive(false);
		BattleBoardData.skillChoiceBoard.BattleEnd();
		m_BattleInterval = BATTLE_START_INTERVAL;
		m_BattleStart = false;
	}

	// バトルリザルト更新
	void ResultUpdate()
	{
		switch (m_ResultPhase.current)
		{
			case ResultPhase.FIRST:
				{
					// フェーズ開始時
					if (m_ResultPhase.IsFirst())
					{
                        m_BattleSlow = StartCoroutine(BattleSlow(SLOW, SLOW, 0, 0, SLOW, 0));
                        BattleBoardData.skillChoiceBoard.ChoiceReset();
                    }

                    GrgrCharCtrl winner = BattleBoardData.skillChoiceBoard.GetWinner(m_ResultPhase.current).GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (winner.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }
                    
                    // 通常再生ポイント
                    if (!m_SlowEnd && winner.m_AnmMgr.GetPlayAnmData()._slowEndFrame <= winner.m_AnmMgr.GetFrame())
                    {
						// よりスロー維持状態
						if (m_MoreSlowTimer == 0){
							Time.timeScale = MORE_SLOW;
						}
						m_MoreSlowTimer += Time.unscaledDeltaTime;
						// UIが途中ではける処理
						if (m_MoreSlowTimer < MORE_SLOW_TIME){
							// 途中でUIがはける
							if (m_MoreSlowTimer > UI_OUT_TIME){
								BattleBoardData.skillChoiceBoard.GetResultData(m_Player.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
								BattleBoardData.skillChoiceBoard.GetResultData(m_Target.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
							}
							break;
						}
						BattleBoardData.skillChoiceBoard.gameObject.SetActive(false);
						BattleBoardData.skillChoiceBoard.DestoryEnforcement();
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    // スローポイント
                    if (!m_SlowStart && (winner.m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || winner.m_AnmMgr.GetPlayAnmData()._slowStartFrame <= winner.m_AnmMgr.GetFrame()))
                    {
                        StopCoroutine(m_BattleSlow);
                        StartCoroutine(BattleSlow(SLOW, SLOW, 0, 0, SLOW, 0));
                        m_SlowStart = true;
                    }

					// UI復活までの遅延処理
					if (m_SlowEnd && m_DelayUiSlideInTimer < DELAY_UI_SLIDEIN_TIME){
						m_DelayUiSlideInTimer += Time.unscaledDeltaTime;
						// UI復活
						if (m_DelayUiSlideInTimer >= DELAY_UI_SLIDEIN_TIME){
							BattleBoardData.skillChoiceBoard.gameObject.SetActive(true);
						}
						else{
							break;
						}
					}

                    // アニメーション終了
                    if (BattleBoardData.skillChoiceBoard.GetChoiceCount(m_Player.gameObject) > 0)
                    {
						m_MoreSlowTimer = 0.0f;
						m_DelayUiSlideInTimer = 0.0f;

                        // 敵カード選択
                        SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard;
                        int tCardNum = controller.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
						for (int i = 0; i < MAX_SELECTS; i++)
						{
                            GameObject card = controller.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
							SkillData skillData = card.GetComponent<SkillCardUI>().GetSkillData();
							controller.Choice(SkillChoiceBoardController.USER.TARGET, skillData, card.transform, m_Target.gameObject);
						}

                        BattleBoardData.skillChoiceBoard.Battle(m_Player.gameObject, m_Target.gameObject);

                        m_ResultPhase.Change(ResultPhase.SECOND);
						m_ResultPhase.Start();

                        m_SlowEnd = false;
                        m_SlowStart = false;
						return;
					}
				}
				break;
			case ResultPhase.SECOND:
				{
					if (m_ResultPhase.IsFirst())
					{
                        BattleBoardData.skillChoiceBoard.ChoiceReset();
                    }
					
                    GrgrCharCtrl winner = BattleBoardData.skillChoiceBoard.GetWinner(m_ResultPhase.current).GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (winner.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && winner.m_AnmMgr.GetPlayAnmData()._slowEndFrame <= winner.m_AnmMgr.GetFrame())
                    {
						// よりスロー維持状態
						if (m_MoreSlowTimer == 0){
							Time.timeScale = MORE_SLOW;
						}
						m_MoreSlowTimer += Time.unscaledDeltaTime;
						// UIが途中ではける処理
						if (m_MoreSlowTimer < MORE_SLOW_TIME){
							// 途中でUIがはける
							if (m_MoreSlowTimer > UI_OUT_TIME){
								BattleBoardData.skillChoiceBoard.GetResultData(m_Player.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
								BattleBoardData.skillChoiceBoard.GetResultData(m_Target.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
							}
							break;
						}
						BattleBoardData.skillChoiceBoard.gameObject.SetActive(false);
						BattleBoardData.skillChoiceBoard.DestoryEnforcement();
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    // スローポイント
                    if (!m_SlowStart && (winner.m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || winner.m_AnmMgr.GetPlayAnmData()._slowStartFrame <= winner.m_AnmMgr.GetFrame()))
                    {
                        StopCoroutine(m_BattleSlow);
                        StartCoroutine(BattleSlow(SLOW, SLOW, 0, 0, SLOW, 0));
                        m_SlowStart = true;
                    }

					// UI復活までの遅延処理
					if (m_SlowEnd && m_DelayUiSlideInTimer < DELAY_UI_SLIDEIN_TIME){
						m_DelayUiSlideInTimer += Time.unscaledDeltaTime;
						// UI復活
						if (m_DelayUiSlideInTimer >= DELAY_UI_SLIDEIN_TIME){
							BattleBoardData.skillChoiceBoard.gameObject.SetActive(true);
						}
						else{
							break;
						}
					}

                    // アニメーション終了
                    if (BattleBoardData.skillChoiceBoard.GetChoiceCount(m_Player.gameObject) > 0)
                    {
						m_MoreSlowTimer = 0.0f;
						m_DelayUiSlideInTimer = 0.0f;

                        // 敵カード選択
                        SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard;
                        int tCardNum = controller.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
                        for (int i = 0; i < MAX_SELECTS; i++)
                        {
                            GameObject card = controller.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
							SkillData skillData = card.GetComponent<SkillCardUI>().GetSkillData();
							controller.Choice(SkillChoiceBoardController.USER.TARGET, skillData, card.transform, m_Target.gameObject);
                        }

                        BattleBoardData.skillChoiceBoard.Battle(m_Player.gameObject, m_Target.gameObject);
                        m_ResultPhase.Change(ResultPhase.THIRD);
						m_ResultPhase.Start();
                        m_SlowEnd = false;
                        m_SlowStart = false;
                        return;
					}
				}
				break;
			case ResultPhase.THIRD:
                {
                    if (m_ResultPhase.IsFirst())
                    {
                        BattleBoardData.skillChoiceBoard.ChoiceReset();
                    }

                    GrgrCharCtrl winner = BattleBoardData.skillChoiceBoard.GetWinner(m_ResultPhase.current).GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (winner.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && winner.m_AnmMgr.GetPlayAnmData()._slowEndFrame <= winner.m_AnmMgr.GetFrame())
                    {
						// よりスロー維持状態
						if (m_MoreSlowTimer == 0){
							Time.timeScale = MORE_SLOW;
						}
						m_MoreSlowTimer += Time.unscaledDeltaTime;
						// Uiが途中ではける処理
						if (m_MoreSlowTimer < MORE_SLOW_TIME){
							// 途中でUIがはける
							if (m_MoreSlowTimer > UI_OUT_TIME){
								BattleBoardData.skillChoiceBoard.GetResultData(m_Player.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
								BattleBoardData.skillChoiceBoard.GetResultData(m_Target.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
							}
							break;
						}
						BattleBoardData.skillChoiceBoard.gameObject.SetActive(false);
						BattleBoardData.skillChoiceBoard.DestoryEnforcement();
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    // スローポイント
                    if (!m_SlowStart && (winner.m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || winner.m_AnmMgr.GetPlayAnmData()._slowStartFrame <= winner.m_AnmMgr.GetFrame()))
                    {
                        StopCoroutine(m_BattleSlow);
                        StartCoroutine(BattleSlow(SLOW, SLOW, 0, 0, SLOW, 0));
                        m_SlowStart = true;
                    }

					// UI復活までの遅延処理
					if (m_SlowEnd && m_DelayUiSlideInTimer < DELAY_UI_SLIDEIN_TIME){
						m_DelayUiSlideInTimer += Time.unscaledDeltaTime;
						// UI復活
						if (m_DelayUiSlideInTimer >= DELAY_UI_SLIDEIN_TIME){
							BattleBoardData.skillChoiceBoard.gameObject.SetActive(true);
						}
						else{
							break;
						}
					}

                    // アニメーション終了
                    if (BattleBoardData.skillChoiceBoard.GetChoiceCount(m_Player.gameObject) > 0)
                    {
						m_MoreSlowTimer = 0.0f;

                        // 敵カード選択
                        SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard;
                        int tCardNum = controller.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
                        for (int i = 0; i < MAX_SELECTS; i++)
                        {
                            GameObject card = controller.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
							SkillData skillData = card.GetComponent<SkillCardUI>().GetSkillData();
							controller.Choice(SkillChoiceBoardController.USER.TARGET, skillData, card.transform, m_Target.gameObject);
                        }

                        BattleBoardData.skillChoiceBoard.Battle(m_Player.gameObject, m_Target.gameObject);
                        m_ResultPhase.Change(ResultPhase.FOURTH);
						m_ResultPhase.Start();
                        m_SlowEnd = false;
                        m_SlowStart = false;
                        return;
                    }
				}
				break;
			case ResultPhase.FOURTH:
				{
                    GrgrCharCtrl winner = BattleBoardData.skillChoiceBoard.GetWinner(m_ResultPhase.current).GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (winner.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && winner.m_AnmMgr.GetPlayAnmData()._slowEndFrame <= winner.m_AnmMgr.GetFrame())
                    {
						// よりスロー維持状態
						if (m_MoreSlowTimer == 0){
							Time.timeScale = MORE_SLOW;
						}
						m_MoreSlowTimer += Time.unscaledDeltaTime;
						// UIが途中ではける処理
						if (m_MoreSlowTimer < MORE_SLOW_TIME){
							// 途中でUIがはける
							if (m_MoreSlowTimer > UI_OUT_TIME){
								BattleBoardData.skillChoiceBoard.GetResultData(m_Player.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
								BattleBoardData.skillChoiceBoard.GetResultData(m_Target.gameObject, m_ResultPhase.current)._skill._object.GetComponentInChildren<Animator>().Play("FeadOut");
							}
							break;
						}
						BattleBoardData.skillChoiceBoard.gameObject.SetActive(false);
						BattleBoardData.skillChoiceBoard.DestoryEnforcement();
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    if ( m_Player.GetComponent<GrgrCharCtrl>().m_AnmMgr.IsState(AnmState.END | AnmState.LOOP) && m_Target.transform.GetComponent<GrgrCharCtrl>().m_AnmMgr.IsState(AnmState.END | AnmState.LOOP))
					{
						m_MoreSlowTimer = 0.0f;
						m_DelayUiSlideInTimer = 0.0f;
						Time.timeScale = 1.0f;
                        m_SlowEnd = false;
                        m_SlowStart = false;
						m_ResultPhase.Change(ResultPhase.END);
						m_ResultPhase.Start();
                        return;
					}
				}
				break;
		}
		m_ResultPhase.Update();
	}


	private bool slowEnd;
	private bool keepEnd;
	private bool quickEnd;
	// バトル中スローコルーチン
	IEnumerator BattleSlow(float slowStart1, float slowEnd1, float slowTime1, float keepTime, float slowEnd2, float slowTime2)
	{
		SkillChoiceBoardController board = BattleBoardData.skillChoiceBoard;
		m_TimePhase = TimePhase.SLOW;

		slowEnd = false;
		keepEnd = false;
		quickEnd = false;

		// 減速スロー
		IEnumerator<float> speed = UtilityMath.FLerp(slowStart1, slowEnd1, slowTime1, EaseType.OUT_EXP, 1, true);
		while (speed.MoveNext())
		{

			if (slowEnd == true)
			{
				m_TimePhase = TimePhase.SLOW_END;
				yield return null;
				break;
			}
			Time.timeScale = speed.Current;
			yield return null;
		}

		// 最遅スロー維持
		float time = 0.0f;
		m_TimePhase = TimePhase.KEEP;
		while (time < keepTime)
		{

			if (keepEnd == true)
			{
				break;
			}
			time += Time.unscaledDeltaTime;
			yield return null;
		}

		m_TimePhase = TimePhase.SLOW_END;
		yield return null;


		// 加速スロー
		speed = UtilityMath.FLerp(Time.timeScale, slowEnd2, slowTime2, EaseType.OUT_EXP, 1, true);
		m_TimePhase = TimePhase.QUICK;
		while (speed.MoveNext())
		{
			if (quickEnd == true){
				break;
			}
			Time.timeScale = speed.Current;
			yield return null;
		}

		m_TimePhase = TimePhase.QUICK_END;
		yield return null;
	}
}
