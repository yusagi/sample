using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{

	#region static変数
	public static BattleManager _instance;

	public float BATTLE_ENCOUNT_TIME = 0.0f;
	public float BATTLE_START_INTERVAL = 4.0f;
	public float SKILLBATTLE_ANIMATION_TIME = 3.0f;

	public static int MAX_CHOICES = 4;
	public static int MAX_SELECTS = 1;
	#endregion

	#region enum

	// バトルフェーズ
	public enum Battle
	{
		BATTLE_ENCOUNT,
		SKILL_BATTLE_START,
		SKILL_BATTLE_PLAY,
		BATTLE_END,
		BATTLE_FORCE_END, // 強制終了
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

	#endregion

	#region メンバ変数

	private Transform m_Player;
	private Transform m_Target;

	// 状態
	public Phase<Battle> battle = new Phase<Battle>();
	public Phase<ResultPhase> resultPahse = new Phase<ResultPhase>();

	// フラグ
	private bool battleStart { get; set; }
    private bool m_SlowStart = false;
    private bool m_SlowEnd = false;
    private bool m_IsChoice = false;

    // スロー状態
    private TimePhase m_TimePhase;

	// タイマー
	float battleInterval = -1;

	// スロー
	public float SLOW_START1 = 0.3f;
	public float SLOW_END1 = 0.01f;
	public float SLOW_END2 = 0.3f;
	public float SLOW_TIME1 = 2.0f;
	public float SLOW_KEEP_TIME = 2.0f;
	public float SLOW_TIME2 = 0.5f;

	// コルーチン
	private Coroutine m_BattleSlow;

	// デバグ
	public bool DBG_IS_CAMERA_STOP = false;
	public float DBG_ACTION_JAMP = 10.0f;
	public float DBG_PLAY_DISTANCE = 3.0f;

	public UnityEngine.UI.Text hps;
	public UnityEngine.UI.Text currentAP;

	#endregion

	public static void BattleForceEnd()
	{
		switch (_instance.battle.current)
		{
			case Battle.BATTLE_ENCOUNT:
				{
					_instance.BattleEncountFin();
				}
				break;
			case Battle.SKILL_BATTLE_START:
				{
					_instance.SkillBattleChoiceFin();
				}
				break;
			case Battle.SKILL_BATTLE_PLAY:
				{
					_instance.SkillBattlePlayFin();
				}
				break;
		}

		_instance.battle.Change(Battle.BATTLE_FORCE_END);
		_instance.battle.Start();
	}

	// Use this for initialization
	void Start()
	{
		m_Player = GameManager.m_Player.transform;
		m_Target = GameManager.m_Enemy.transform;

		_instance = this;
		battle.Change(Battle.NONE);
		battleStart = false;
		BattleBoardData.Initialize();
	}

	// Update is called once per frame
	void Update()
	{

		if (battleInterval > 0)
		{
			battleInterval -= Time.deltaTime;
		}
		hps.text = "Player " + m_Player.GetComponent<GrgrCharCtrl>().hp + "   " + "Enemy " + m_Target.transform.GetComponent<GrgrCharCtrl>().hp;
	}

	void LateUpdate()
	{

		BattleUpdate();

		RushContinuesCheck();
	}

	// Battle更新
	void BattleUpdate()
	{
		switch (battle.current)
		{
			// 強制終了
			case Battle.BATTLE_FORCE_END:
				{
					battle.Change(Battle.BATTLE_END);
					battle.Start();
					return;
				}
			// バトルエンカウント
			case Battle.BATTLE_ENCOUNT:
				{
					// エンカウント演出

					if (battle.IsFirst())
					{
						BattleBoardData.skillChoiceBoard.SetActive(true);
					}

					// SKILL_BATTLE_STARTへ移行
					if (battle.phaseTime >= BATTLE_ENCOUNT_TIME)
					{
						battle.Change(Battle.SKILL_BATTLE_START);
						battle.Start();
						return;
					}
				}
				break;
			// スキルバトル開始
			case Battle.SKILL_BATTLE_START:
				{
					// スキル表示演出
					SkillChoiceBoardController skillChoiceBoard = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
					if (battle.IsFirst())
					{
						// スキルバトル初期化
						skillChoiceBoard.BattleIni(m_Player.gameObject, m_Target.gameObject);

						// プレイヤーカード生成
						GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();
						int pCardNum = player.skillManager.GetSkillCards().Count;
						for (int i = 0; i < MAX_CHOICES; i++)
						{
							var card = player.skillManager.GetSkillCards()[Random.Range(0, pCardNum)];
							GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard 1"));
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
                            GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/SkillCard 1"));
                            instance.name = card._name;
                            instance.GetComponent<SkillCardUI>().AddCardData(card);
                            skillChoiceBoard.AddCardObject(instance, SkillChoiceBoardController.USER.TARGET);
                        }

						// プレイヤーのAPを設定
						skillChoiceBoard.m_PlayerAP = (int)(m_Player.GetComponent<GrgrCharCtrl>().rigidbody.GetSpeed() * 3.6f);

						// カード選択コルーチンスタート
						m_BattleSlow = StartCoroutine(BattleSlow(SLOW_START1, SLOW_END1, SLOW_TIME1, SLOW_KEEP_TIME, SLOW_END2, SLOW_TIME2));
#if UNITY_EDITOR
						// ログ消去
						Dbg.ClearConsole();
#endif
					}

					// APUI表示
					currentAP.text = skillChoiceBoard.m_PlayerAP.ToString() + "AP";

					// スキル選択へ
					if (m_TimePhase == TimePhase.QUICK_END)
					{
                        Time.timeScale = 1.0f;
						BattleBoardData.skillChoiceBoard.SetActive(false);

						if (skillChoiceBoard.GetChoiceCount(m_Player.gameObject) > 0)
						{
                            // 敵カード選択
                            int tCardNum = skillChoiceBoard.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
							for (int i = 0; i < MAX_SELECTS; i++)
							{
                                GameObject card = skillChoiceBoard.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
								skillChoiceBoard.AddChoice(card.GetComponent<SkillCardUI>().GetSkillData(), m_Target.gameObject);
                                skillChoiceBoard.CutCardObject(card, SkillChoiceBoardController.USER.TARGET);   
							}

                            // 敵のバトル状態設定
                            GrgrCharCtrl target = m_Target.transform.GetComponent<GrgrCharCtrl>();
                            Vector3 line = m_Target.transform.position - m_Player.position;
							Vector3 halfPos = m_Player.position + (line * 0.5f);
							Vector3 up = (halfPos - GameManager.m_Planet.transform.position);
							Vector3 pFront = Vector3.ProjectOnPlane(m_Player.forward, up).normalized;
							Vector3 eFront = Vector3.ProjectOnPlane(m_Target.transform.forward, up).normalized;
							float angle = Mathf.Acos(Vector3.Dot(pFront, eFront)) * Mathf.Rad2Deg;
							if (angle <= 45.0f)
							{
								Debug.Log("Back");
								target.battle = target.BackBattle(m_Player);
							}
							else
							{
								target.battle = target.FrontBattle(m_Player);
							}

							// プレイヤーのバトル状態設定
							m_Player.GetComponent<GrgrCharCtrl>().battle = m_Player.GetComponent<GrgrCharCtrl>().FrontBattle(m_Target);

							// プレイへ移行
							battle.Change(Battle.SKILL_BATTLE_PLAY);
							battle.Start();
							return;
						}
						else
						{
							// バトル終了へ移行
							battle.Change(Battle.BATTLE_END);
							battle.Start();
							return;
						}

					}
				}
				break;
			// スキルバトルプレイ
			case Battle.SKILL_BATTLE_PLAY:
				{
					// バトル演出

					if (battle.IsFirst())
					{
						BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().Battle(m_Player.gameObject, m_Target.gameObject);
						resultPahse.Change(ResultPhase.FIRST);
						resultPahse.Start();
					}
					GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();
					GrgrCharCtrl target = m_Target.GetComponent<GrgrCharCtrl>();

					if (player.m_IsBattleAnmStart && target.m_IsBattleAnmStart)
					{
						ResultUpdate();
					}

					//  BATTLE_ENDへ
					if ((player.m_IsBattleAnmStart && target.m_IsBattleAnmStart) && (!player.m_IsBattleAnmPlay && !target.m_IsBattleAnmPlay))
					{
						SkillBattlePlayFin();
						battle.Change(Battle.BATTLE_END);
						battle.Start();
						return;
					}
				}
				break;
			// バトル終了
			case Battle.BATTLE_END:
				{
					BattleEndFin();
					battle.Change(Battle.NONE);
					battle.Start();
					return;
				}
		}
		battle.Update();
	}

	// バトルモード突入、継続チェック
	void RushContinuesCheck()
	{
		if (battleStart == true)
		{
			return;
		}

		// どのカメラタイプでエリア指定するか
		bool tmpIsBattle = GameData.GetCamera().GetComponent<CameraController>().IsInCameraFramework();

		battleStart = tmpIsBattle && battleInterval < 0;


		// バトルモード条件
		if (battleStart)
		{
			// バトルモード中ならリターン
			if (battle.current != Battle.NONE)
			{
				return;
			}
			if (m_Target.transform.GetComponent<GrgrCharCtrl>().state.current == GrgrCharCtrl.State.ASCENSION)
				return;
			if (m_Player.GetComponent<GrgrCharCtrl>().state.current == GrgrCharCtrl.State.ASCENSION)
				return;

			// バトルエンカウント
			battle.Change(Battle.BATTLE_ENCOUNT);
			battle.Start();

			// プレイヤー、エネミー、カメラがバトルに突入
			m_Player.GetComponent<GrgrCharCtrl>().state.Change(GrgrCharCtrl.State.BATTLE);
			m_Target.transform.GetComponent<GrgrCharCtrl>().state.Change(GrgrCharCtrl.State.BATTLE);
			GameData.GetCamera().GetComponent<CameraController>().phase.Change(CameraController.CameraPhase.BATTLE);
		}
	}

	// BATTLE_START終了処理
	void BattleEncountFin()
	{
	}

	// SKILL_BATTLE_CHOICE終了処理
	void SkillBattleChoiceFin()
	{
		BattleBoardData.skillChoiceBoard.SetActive(false);
	}

	// SKILL_BATTLE_PLAY終了処理
	void SkillBattlePlayFin()
	{
	}

	// BATTLE_END終了処理
	void BattleEndFin()
	{
		BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().BattleEnd();
		battleInterval = BATTLE_START_INTERVAL;
		battleStart = false;
	}

	// BATTLE_FORCE_END終了処理
	void BattleForceEndFin()
	{

	}

	// バトルリザルト更新
	void ResultUpdate()
	{
		switch (resultPahse.current)
		{
			case ResultPhase.FIRST:
				{
					// フェーズ開始時
					if (resultPahse.IsFirst())
					{
						StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(0.1f, 0.1f, 0, 0, 0.1f, 0));
                        BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().ChoiceReset();
                        m_IsChoice = false;
                    }

                    GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (player.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }
                    
                    // 通常再生ポイント
                    if (!m_SlowEnd && player.m_AnmMgr.GetAnmData()._slowEndFrame <= player.m_AnmMgr.GetFrame())
                    {
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    // スローポイント
                    if (!m_SlowStart && (player.m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || player.m_AnmMgr.GetAnmData()._slowStartFrame <= player.m_AnmMgr.GetFrame()))
                    {
                        BattleBoardData.skillChoiceBoard.SetActive(true);
                        StopCoroutine(m_BattleSlow);
                        StartCoroutine(BattleSlow(0.1f, 0.1f, 0, 0, 0.1f, 0));
                        m_SlowStart = true;
                    }

                    // アニメーション終了
                    if (BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().GetChoiceCount(m_Player.gameObject) > 0)
                    {
                        // スキルUI消去
                        BattleBoardData.skillChoiceBoard.SetActive(false);

                        // 敵カード選択
                        SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
                        int tCardNum = controller.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
						for (int i = 0; i < MAX_SELECTS; i++)
						{
                            GameObject card = controller.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
							controller.AddChoice(card.GetComponent<SkillCardUI>().GetSkillData(), m_Target.gameObject);
                            controller.CutCardObject(card, SkillChoiceBoardController.USER.TARGET);
						}

                        BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().Battle(m_Player.gameObject, m_Target.gameObject);

                        resultPahse.Change(ResultPhase.SECOND);
						resultPahse.Start();

                        m_SlowEnd = false;
                        m_SlowStart = false;
						return;
					}
				}
				break;
			case ResultPhase.SECOND:
				{
					if (resultPahse.IsFirst())
					{
                        BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().ChoiceReset();
                    }

                    GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (player.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && player.m_AnmMgr.GetAnmData()._slowEndFrame <= player.m_AnmMgr.GetFrame())
                    {
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    // スローポイント
                    if (!m_SlowStart && (player.m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || player.m_AnmMgr.GetAnmData()._slowStartFrame <= player.m_AnmMgr.GetFrame()))
                    {
                        BattleBoardData.skillChoiceBoard.SetActive(true);
                        StopCoroutine(m_BattleSlow);
                        StartCoroutine(BattleSlow(0.1f, 0.1f, 0, 0, 0.1f, 0));
                        m_SlowStart = true;
                    }

                    // アニメーション終了
                    if (BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().GetChoiceCount(m_Player.gameObject) > 0)
                    {
                        // スキルUI消去
                        BattleBoardData.skillChoiceBoard.SetActive(false);

                        // 敵カード選択
                        SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
                        int tCardNum = controller.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
                        for (int i = 0; i < MAX_SELECTS; i++)
                        {
                            GameObject card = controller.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
                            controller.AddChoice(card.GetComponent<SkillCardUI>().GetSkillData(), m_Target.gameObject);
                            controller.CutCardObject(card, SkillChoiceBoardController.USER.TARGET);
                        }

                        BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().Battle(m_Player.gameObject, m_Target.gameObject);
                        resultPahse.Change(ResultPhase.THIRD);
						resultPahse.Start();
                        m_SlowEnd = false;
                        m_SlowStart = false;
                        return;
					}
				}
				break;
			case ResultPhase.THIRD:
                {
                    if (resultPahse.IsFirst())
                    {
                        m_TimePhase = TimePhase.SLOW;
                        BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().ChoiceReset();
                    }

                    GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (player.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && player.m_AnmMgr.GetAnmData()._slowEndFrame <= player.m_AnmMgr.GetFrame())
                    {
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    // スローポイント
                    if (!m_SlowStart && (player.m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || player.m_AnmMgr.GetAnmData()._slowStartFrame <= player.m_AnmMgr.GetFrame()))
                    {
                        BattleBoardData.skillChoiceBoard.SetActive(true);
                        StopCoroutine(m_BattleSlow);
                        StartCoroutine(BattleSlow(0.1f, 0.1f, 0, 0, 0.1f, 0));
                        m_SlowStart = true;
                    }

                    // アニメーション終了
                    if (BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().GetChoiceCount(m_Player.gameObject) > 0)
                    {
                        // スキルUI消去
                        BattleBoardData.skillChoiceBoard.SetActive(false);

                        // 敵カード選択
                        SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
                        int tCardNum = controller.GetCardList(SkillChoiceBoardController.USER.TARGET).Count;
                        for (int i = 0; i < MAX_SELECTS; i++)
                        {
                            GameObject card = controller.GetCardList(SkillChoiceBoardController.USER.TARGET)[Random.Range(0, tCardNum)];
                            controller.AddChoice(card.GetComponent<SkillCardUI>().GetSkillData(), m_Target.gameObject);
                            controller.CutCardObject(card, SkillChoiceBoardController.USER.TARGET);
                        }

                        BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>().Battle(m_Player.gameObject, m_Target.gameObject);
                        resultPahse.Change(ResultPhase.FOURTH);
						resultPahse.Start();
                        m_SlowEnd = false;
                        m_SlowStart = false;
                        return;
                    }
				}
				break;
			case ResultPhase.FOURTH:
				{
					if (resultPahse.IsFirst())
					{

                    }
                    GrgrCharCtrl player = m_Player.GetComponent<GrgrCharCtrl>();

                    // アニメーション切り変え中
                    if (player.m_AnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && player.m_AnmMgr.GetAnmData()._slowEndFrame <= player.m_AnmMgr.GetFrame())
                    {
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(1, 1, 0, 0, 1, 0));
                        m_SlowEnd = true;
                    }

                    if ( m_Player.GetComponent<GrgrCharCtrl>().m_AnmMgr.IsState(AnmState.END | AnmState.LOOP) && m_Target.transform.GetComponent<GrgrCharCtrl>().m_AnmMgr.IsState(AnmState.END | AnmState.LOOP))
					{
						m_Player.GetComponent<GrgrCharCtrl>().m_IsBattleAnmPlay = false;
						m_Target.transform.GetComponent<GrgrCharCtrl>().m_IsBattleAnmPlay = false;
						Time.timeScale = 1.0f;
                        m_SlowEnd = false;
                        m_SlowStart = false;
                        return;
					}
				}
				break;
		}
		resultPahse.Update();
	}

	// バトル中スローコルーチン
	IEnumerator BattleSlow(float slowStart1, float slowEnd1, float slowTime1, float keepTime, float slowEnd2, float slowTime2)
	{
		SkillChoiceBoardController board = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();
		m_TimePhase = TimePhase.SLOW;

		// 減速スロー
		IEnumerator<float> speed = UtilityMath.FLerp(slowStart1, slowEnd1, slowTime1, EaseType.OUT_EXP, 1, true);
		while (speed.MoveNext())
		{
			Time.timeScale = speed.Current;

			if (board.GetChoiceCount(m_Player.gameObject) == MAX_SELECTS)
			{
				m_TimePhase = TimePhase.SLOW_END;
				yield return null;
				break;
			}

			yield return null;
		}

		// 最遅スロー維持
		if (m_TimePhase != TimePhase.SLOW_END)
		{
			float time = 0.0f;
			m_TimePhase = TimePhase.KEEP;
			while (time < keepTime)
			{
				time += Time.unscaledDeltaTime;

				if (board.GetChoiceCount(m_Player.gameObject) == MAX_SELECTS)
				{
					break;
				}

				yield return null;
			}

			m_TimePhase = TimePhase.SLOW_END;
			yield return null;
		}


		// 加速スロー
		speed = UtilityMath.FLerp(Time.timeScale, slowEnd2, slowTime2, EaseType.OUT_EXP, 1, true);
		m_TimePhase = TimePhase.QUICK;
		while (speed.MoveNext())
		{
			Time.timeScale = speed.Current;
			yield return null;
		}

		m_TimePhase = TimePhase.QUICK_END;
		yield return null;
	}
}
