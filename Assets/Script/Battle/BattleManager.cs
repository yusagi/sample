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

	// スロー状態
	public enum TimePhase{
		SLOW,
		SLOW_END,
		KEEP,
		KEEP_END,
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

	private CharCore m_Player;	// プレイヤー
	private CharCore m_Target;	// 対戦相手
	private Transform m_Planet;

	private CameraController m_CameraController;
	private SkillBattleManager m_SkillBattleManager;
	private Transform m_UICanvas;

	// 状態
	public Phase<Battle> m_Battle = new Phase<Battle>();
	public Phase<SkillBattlePhase> m_ResultPhase = new Phase<SkillBattlePhase>();
    private Phase<TimePhase> m_TimePhase = new Phase<TimePhase>();

	// フラグ
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
	public float EXCLAMATION_TIME;			// びっくりマークの表示時間
	public float BATTLE_PLAY_DISTANCE;		// バトルモードで接敵した時の止まる距離
	public float BATTLE_PLAY_SPEED;			// バトルモードで接敵した時のキャラの速度
	public float BATTLE_START_DISTANCE;		// バトルモード開始の索敵距離
	public float SLOW = 0.1f; 						// 通常スローの値
	public float MORE_SLOW = 0.01f;					// よりスローの値
	public float MORE_SLOW_TIME = 0.5f; 			// よりスローである時間
	public float DELAY_UI_SLIDEIN_TIME = 0.6f; 		// スキルUIが再びスライドインするまでの遅延時間
	public float UI_OUT_TIME = 0.2f;				// UIがはけるまでの時間

	private int MAX_CHOICES = 4;	// スキルカードの選択肢の数
	private int MAX_SELECTS = 1;	// 選べるスキル数

	private float BATTLE_START_INTERVAL = 4.0f;	// 次のバトルを開始できるまでのインターバル

	#endregion

	public void SetBattleCharInfo(CharCore player, CharCore target){
		m_Player = player;
		m_Target = target;
	}

	public void SetBattleManagerInfo(Transform planet, Transform uiCanvas, CameraController camera, SkillBattleManager skillBattleManager){
		m_Planet = planet;
		m_UICanvas = uiCanvas;
		m_CameraController = camera;
		m_SkillBattleManager = skillBattleManager;
	}

	// Use this for initialization
	void Start()
	{

		//_instance = this;
		m_Battle.Change(Battle.NONE);
		m_ResultPhase.Change(SkillBattlePhase.END);
		m_TimePhase.Change(TimePhase.QUICK_END);

		m_MoreSlowTimer = 0.0f;
		m_DelayUiSlideInTimer = 0.0f;
	}

	// Update is called once per frame
	void Update()
	{
	}

	void LateUpdate()
	{
		BattleUpdate();

		RushContinuesCheck();
	}

	// スキルバトルマネージャー取得
	public SkillBattleManager GetSkillBattleManager(){
		return m_SkillBattleManager;
	}

	// 2キャラの距離を測る
	DistanceType GetObjectsDistance(){
		// 距離を求める
		Vector3 center = m_Planet.position;
		float radius = m_Planet.localScale.y * 0.5f;
		float distance = Rigidbody_grgr.Arc(center, radius, m_Player.transform.position, m_Target.transform.position);

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
		// びっくりマーク表示
		GameObject exclamation = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI/SkillBattle/ExclamationMark"));
		exclamation.transform.SetParent(m_UICanvas, false);
		float timer = EXCLAMATION_TIME;
		while(timer > 0){
			timer -= Time.unscaledDeltaTime;
			yield return null;
		}
		GameObject.Destroy(exclamation);

		// エンカウントUI表示
		GameObject encount = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI/SkillBattle/UI_Encount"));
		encount.transform.SetParent(m_UICanvas, false);

		while(m_Battle.current == Battle.BATTLE_ENCOUNT || m_Battle.current == Battle.SKILL_BATTLE_START){
			yield return null;
		}

		GameObject.Destroy(encount);
	}

	// 接敵スローポイントの生成コルーチン
	IEnumerator SlowAreaCreate(){
		GameObject area = new GameObject("SLOW_AREA");
		BattleAreaLineRenderer areaLine = area.AddComponent<BattleAreaLineRenderer>();
		areaLine.arc = BATTLE_PLAY_DISTANCE * 0.5f;
		areaLine.SetColor(Color.red);

		while(m_Battle.current != Battle.SKILL_BATTLE_START){
			Vector3 line = m_Player.transform.position - m_Target.transform.position;
			Vector3 center = m_Target.transform.position + (line*0.5f);
			Vector3 up = (center - m_Planet.position).normalized;
			Vector3 front = Vector3.ProjectOnPlane(Vector3.forward, up);

			area.transform.rotation = Quaternion.LookRotation(front, up);
			yield return null;
		}

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

		switch (m_Battle.current)
		{
			// バトルエンカウント
			case Battle.BATTLE_ENCOUNT:
				{
					// エンカウント演出
					if (m_Battle.IsFirst())
					{
						// スキルバトル初期化
						m_SkillBattleManager.BattleIni(m_Player.gameObject, m_Target.gameObject);
						// エンカウントUI表示
						StartCoroutine(EncountUI());
						// プレイヤーのAPを設定
						m_SkillBattleManager.SetFighterAP(m_Player.gameObject, (int)(m_Player.GetBrain().GetInfo().m_CurrentSpeed * 3.6f));
						// 相手のAPを設定
						m_SkillBattleManager.SetFighterAP(m_Target.gameObject, (int)(m_Target.GetBrain().GetInfo().m_CurrentSpeed * 3.6f));
					}
		
					// SKILL_BATTLE_STARTへ移行(相手との距離が一定値より近くなったら)
					//if (GetObjectsDistance() == DistanceType.NEAR)
					if (GetObjectsDistance() == DistanceType.NEAR)
					{
						Vector3 line = m_Player.transform.position - m_Target.transform.position;
						Vector3 center = m_Target.transform.position + (line * 0.5f);
						Vector3 up = (center - m_Planet.position).normalized;

						Vector3 planetCenter = m_Planet.position;
						float planetRad = m_Planet.localScale.y * 0.5f;
						
						Vector3 setPosition = Rigidbody_grgr.RotateToPosition(up, planetCenter, planetRad, 0);
						Quaternion pRotation;
						Quaternion tRotation;
						bool pIsRotate = Rigidbody_grgr.MoveToRotation(out pRotation, planetCenter, planetRad, setPosition, -m_Player.transform.forward * BATTLE_PLAY_DISTANCE * 0.5f, m_Player.transform.forward);
						bool tIsRotate = Rigidbody_grgr.MoveToRotation(out tRotation, planetCenter, planetRad, setPosition, -m_Target.transform.forward * BATTLE_PLAY_DISTANCE * 0.5f, m_Target.transform.forward);
						
						if (pIsRotate && tIsRotate){
							m_Player.GetBrain().SetTransform(pRotation);
							m_Target.GetBrain().SetTransform(tRotation);
						}
						else{
							Debug.LogError("rotation error");
						}

						m_Player.GetBrain().SetInputSpeed(BATTLE_PLAY_SPEED);
						m_Target.GetBrain().SetInputSpeed(BATTLE_PLAY_SPEED);

						m_Battle.Change(Battle.SKILL_BATTLE_START);
						m_Battle.Start();
						return;
					}
				}
				break;
			// スキルバトル開始
			case Battle.SKILL_BATTLE_START:
				{
					if (m_Battle.IsFirst())
					{
						// スロー処理(初回なのでキープタイムがカード選択時間)
						m_BattleSlow = StartCoroutine(BattleSlow(SLOW_START1, SLOW_END1, SLOW_TIME1, SLOW_KEEP_TIME, SLOW_END1, 0));
#if UNITY_EDITOR
						// ログ消去
						Dbg.ClearConsole();
#endif
					}

					// カード選択したか
					if (slowEnd == false && keepEnd == false && quickEnd == false){
						if (m_SkillBattleManager.GetChoiceCount(m_Player.gameObject) == MAX_SELECTS){
							slowEnd = keepEnd = quickEnd = true;
						}
					}

					// UI出現タイミング
					if (m_TimePhase.IsFirst()){
						if (m_TimePhase.current == TimePhase.SLOW_END){
							// スキルUIボードのアクティブTRUE
							m_SkillBattleManager.gameObject.SetActive(true);
						}
						if (m_TimePhase.current == TimePhase.KEEP){
							// プレイヤーカード生成
							int pCardNum = m_Player.GetBrain().GetState().GetSkillManager().GetSkillCards().Count;
							for (int i = 0; i < MAX_CHOICES; i++)
							{
								var card = m_Player.GetBrain().GetState().GetSkillManager().GetSkillCards()[Random.Range(0, pCardNum)];
								GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI/SkillBattle/SkillCard"));
								instance.name = card._name;
								instance.GetComponent<SkillCardUI>().AddCardData(card);
								m_SkillBattleManager.AddCardObject(instance, m_Player.gameObject);
							}

							// 敵カード生成
							int tCardNum = m_Target.GetBrain().GetState().GetSkillManager().GetSkillCards().Count;
							for (int i = 0; i < MAX_CHOICES; i++)
							{
								var card = m_Target.GetBrain().GetState().GetSkillManager().GetSkillCards()[Random.Range(0, tCardNum)];
								GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI/SkillBattle/SkillCard"));
								instance.name = card._name;
								instance.GetComponent<SkillCardUI>().AddCardData(card);
								m_SkillBattleManager.AddCardObject(instance, m_Target.gameObject);
							}
						}
					}

					// スキル選択へ
					if (m_TimePhase.current == TimePhase.QUICK_END)
					{
						// 索敵範囲に表示を戻す
						BattleAreaLineRenderer areaLine = m_Player.GetComponent<BattleAreaLineRenderer>();
						areaLine.arc = BATTLE_START_DISTANCE;
						areaLine.SetColor(Color.yellow);
						if (m_SkillBattleManager.GetChoiceCount(m_Player.gameObject) > 0)
						{
                            // 敵カード選択
                            int tCardNum = m_SkillBattleManager.GetCardList(m_Target.gameObject).Count;
							for (int i = 0; i < MAX_SELECTS; i++)
							{
                                GameObject card = m_SkillBattleManager.GetCardList(m_Target.gameObject)[Random.Range(0, tCardNum)];
								m_SkillBattleManager.AddChoice(m_Target.gameObject, card.transform);
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
						//BattleBoardData.skillChoiceBoard.Battle(m_Player.gameObject, m_Target.gameObject);
						m_ResultPhase.Change(SkillBattlePhase.START);
						m_ResultPhase.Start();
					}
					
					ResultUpdate();

					//  BATTLE_ENDへ
					if (m_ResultPhase.current == SkillBattlePhase.END)
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
		// バトルモード中ならリターン
		if (m_Battle.current != Battle.NONE)
		{
			return;
		}

		// バトルモード突入条件
		float arc = Rigidbody_grgr.Arc(m_Planet.position, m_Planet.localScale.y * 0.5f, m_Player.transform.position, m_Target.transform.position);
		bool tmpIsBattle = (arc < BATTLE_START_DISTANCE);

		// バトルモード条件
		if (tmpIsBattle)
		{
			if (m_BattleInterval > 0){
				return;
			}
				
			// バトル状態設定
			SetBattle();

			// バトルプレイエリア生成
			//StartCoroutine(SlowAreaCreate());
			
			// プレイヤー、エネミー、カメラがバトルに突入
			m_Player.GetBrain().SetNextState(CharState.State.BATTLE, true);
			m_Target.GetBrain().SetNextState(CharState.State.BATTLE, true);
			m_CameraController.phase.Change(CameraController.CameraPhase.BATTLE);
			
			// バトルエンカウント
			m_Battle.Change(Battle.BATTLE_ENCOUNT);
			m_Battle.Start();
		}
		else{
			m_BattleInterval -= Time.deltaTime;
		}
	}

	// バトル状態設定
	void SetBattle(){
			// ターゲット方向に線を引く
			Vector3 line = m_Target.transform.position - m_Player.transform.position;
			// 相手が向いてる方向をプレイヤー基準で計算
			Vector3 targetForward = Vector3.ProjectOnPlane(m_Target.transform.forward, m_Player.transform.up).normalized;
			// プレイヤーと相手の向きの角度差
			float angle = Mathf.Acos(Vector3.Dot(m_Player.transform.forward, targetForward)) * Mathf.Rad2Deg;
			
			SkillBattle pBattle = (SkillBattle)m_Player.GetBrain().GetState().GetBattle();
			SkillBattle tBattle = (SkillBattle)m_Target.GetBrain().GetState().GetBattle();
			// プレイヤーと相手がだいたい向き合ってる
			if (angle >= 45.0f){
				pBattle.SetBattleType(SkillBattle.BattleType.FRONT);
				tBattle.SetBattleType(SkillBattle.BattleType.FRONT);
			}
			// プレイヤーと相手がだいたい同じ方向
			else{
				// 相手が前方か後方か調べる
				Vector3 toTarget = Vector3.ProjectOnPlane(line, m_Player.transform.up).normalized;
				float toTargetAngle = Mathf.Acos(Vector3.Dot(m_Player.transform.forward, toTarget)) * Mathf.Rad2Deg;

				// 相手が前方
				if (toTargetAngle < 90.0f){
					pBattle.SetBattleType(SkillBattle.BattleType.FRONT);
					tBattle.SetBattleType(SkillBattle.BattleType.BACK);
				}
				// 相手が後方
				else{
					pBattle.SetBattleType(SkillBattle.BattleType.BACK);
					tBattle.SetBattleType(SkillBattle.BattleType.FRONT);
				}
			}

			pBattle.SetTargetBrain(m_Target.GetBrain());
			pBattle.SetTargetVel(Vector3.ProjectOnPlane(line, m_Player.transform.up).normalized);
			tBattle.SetTargetBrain(m_Player.GetBrain());
			tBattle.SetTargetVel(Vector3.ProjectOnPlane(-line, m_Target.transform.up).normalized);
			// デバグ用
			//player.m_Battle = player.FrontBattle(target.transform);
			//target.m_Battle = target.FrontBattle(player.transform);
	}

	// BATTLE_END終了処理
	void BattleEndFin()
	{
		Time.timeScale = 1.0f;
		m_SkillBattleManager.gameObject.SetActive(false);
		m_SkillBattleManager.BattleEnd();
		m_BattleInterval = BATTLE_START_INTERVAL;
	}

	public float m_BattleNormalTime = 0.8f;

	// バトルリザルト更新
	void ResultUpdate()
	{
		switch (m_ResultPhase.current)
		{
			// バトル開始
			case SkillBattlePhase.START:{
				m_SkillBattleManager.Battle(m_Player.gameObject, m_Target.gameObject);
                m_BattleSlow = StartCoroutine(BattleSlow(SLOW, SLOW, 0, 0, SLOW, 0));
				m_ResultPhase.Change(SkillBattlePhase.FIRST);
				m_ResultPhase.Start();
				return;
			}
			case SkillBattlePhase.FIRST:
			case SkillBattlePhase.SECOND:
			case SkillBattlePhase.THIRD:
				{
					// フェーズ開始時
					if (m_ResultPhase.IsFirst())
					{
                    }

                    CharCore winner = m_SkillBattleManager.GetWinner(m_ResultPhase.current).GetComponent<CharCore>();
					AnimationManager winnerAnmMgr = winner.GetBrain().GetState().GetAnmMgr();

                    // アニメーション切り変え中
                    if (winnerAnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }
                    
                    // 通常再生ポイント
                    if (!m_SlowEnd && winnerAnmMgr.GetPlayAnmData()._slowEndFrame <= winnerAnmMgr.GetFrame())
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
								m_SkillBattleManager.UIFadeOut(m_SkillBattleManager.GetChoice(m_Player.gameObject));
								m_SkillBattleManager.UIFadeOut(m_SkillBattleManager.GetChoice(m_Target.gameObject));
							}
							break;
						}
						m_SkillBattleManager.gameObject.SetActive(false);
                        m_SkillBattleManager.ChoiceReset();
						m_SkillBattleManager.DestoryEnforcement();
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(m_BattleNormalTime, m_BattleNormalTime, 0, 0, m_BattleNormalTime, 0));
                        m_SlowEnd = true;
                    }

					// アニメーション連続再生中
					if (winnerAnmMgr.GetAnmList().Count > 1){
						break;
					}
					if (winnerAnmMgr.GetState() == AnmState.CHAINE){
						break;
					}

                    // スローポイント
                    if (!m_SlowStart && (winnerAnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || winnerAnmMgr.GetPlayAnmData()._slowStartFrame <= winnerAnmMgr.GetFrame()))
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
							m_SkillBattleManager.gameObject.SetActive(true);
						}
						else{
							break;
						}
					}

                    // アニメーション終了
                    if (m_SkillBattleManager.GetChoiceCount(m_Player.gameObject) > 0)
                    {
						m_MoreSlowTimer = 0.0f;
						m_DelayUiSlideInTimer = 0.0f;

                        // 敵カード選択
                        int tCardNum = m_SkillBattleManager.GetCardList(m_Target.gameObject).Count;
						for (int i = 0; i < MAX_SELECTS; i++)
						{
                            GameObject card = m_SkillBattleManager.GetCardList(m_Target.gameObject)[Random.Range(0, tCardNum)];
							m_SkillBattleManager.AddChoice(m_Target.gameObject, card.transform);
						}

                        m_SkillBattleManager.Battle(m_Player.gameObject, m_Target.gameObject);

						int phase = (int)m_ResultPhase.current + 1;
                        m_ResultPhase.Change((SkillBattlePhase)phase);
						m_ResultPhase.Start();

                        m_SlowEnd = false;
                        m_SlowStart = false;
						return;
					}
				}
				break;
			case SkillBattlePhase.FOURTH:
				{
                    CharCore winner = m_SkillBattleManager.GetWinner(m_ResultPhase.current).GetComponent<CharCore>();
					AnimationManager winnerAnmMgr = winner.GetBrain().GetState().GetAnmMgr();

                    // アニメーション切り変え中
                    if (winnerAnmMgr.GetState() == AnmState.CHANGE)
                    {
                        break;
                    }

                    // 通常再生ポイント
                    if (!m_SlowEnd && winnerAnmMgr.GetPlayAnmData()._slowEndFrame <= winnerAnmMgr.GetFrame())
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
								m_SkillBattleManager.UIFadeOut(m_SkillBattleManager.GetChoice(m_Player.gameObject));
								m_SkillBattleManager.UIFadeOut(m_SkillBattleManager.GetChoice(m_Target.gameObject));
							}
							break;
						}
                        m_SkillBattleManager.ChoiceReset();
						m_SkillBattleManager.gameObject.SetActive(false);
						m_SkillBattleManager.DestoryEnforcement();
                        StopCoroutine(m_BattleSlow);
                        m_BattleSlow = StartCoroutine(BattleSlow(m_BattleNormalTime, m_BattleNormalTime, 0, 0, m_BattleNormalTime, 0));
                        m_SlowEnd = true;
                    }

					// アニメーション連続再生中
					if (winnerAnmMgr.GetAnmList().Count > 1){
						break;
					}
					if (winnerAnmMgr.GetState() == AnmState.CHAINE){
						break;
					}

                    if ( m_Player.GetBrain().GetState().GetAnmMgr().IsState(AnmState.END | AnmState.LOOP) && m_Target.GetBrain().GetState().GetAnmMgr().IsState(AnmState.END | AnmState.LOOP))
					{
						m_MoreSlowTimer = 0.0f;
						m_DelayUiSlideInTimer = 0.0f;
						Time.timeScale = 1.0f;
                        m_SlowEnd = false;
                        m_SlowStart = false;
						m_ResultPhase.Change(SkillBattlePhase.END);
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
		SkillBattleManager board = m_SkillBattleManager;
		m_TimePhase.Change(TimePhase.SLOW);
		m_TimePhase.Start();

		slowEnd = false;
		keepEnd = false;
		quickEnd = false;

		// 減速スロー
		IEnumerator<float> speed = UtilityMath.FLerp(slowStart1, slowEnd1, slowTime1, EaseType.OUT_EXP, 1, true);
		while (speed.MoveNext())
		{

			if (slowEnd == true)
			{
				break;
			}

			Time.timeScale = speed.Current;
			yield return null;
			// TimePhase.Slowの更新
			m_TimePhase.Update();
		}

		// SLOW_END
		m_TimePhase.Change(TimePhase.SLOW_END);
		m_TimePhase.Start();
		yield return null;

		// 待機
		float time = 0.0f;
		m_TimePhase.Change(TimePhase.KEEP);
		m_TimePhase.Start();
		while (time < keepTime)
		{

			if (keepEnd == true)
			{
				break;
			}
			time += Time.unscaledDeltaTime;
			yield return null;
			// TimePhase.KEEPの更新
			m_TimePhase.Update();
		}

		// KEEP_END
		m_TimePhase.Change(TimePhase.KEEP_END);
		m_TimePhase.Start();
		yield return null;


		// 加速スロー
		speed = UtilityMath.FLerp(Time.timeScale, slowEnd2, slowTime2, EaseType.OUT_EXP, 1, true);
		m_TimePhase.Change(TimePhase.QUICK);
		m_TimePhase.Start();
		while (speed.MoveNext())
		{
			if (quickEnd == true){
				break;
			}
			Time.timeScale = speed.Current;
			yield return null;
			// TimePhase.QUICKの更新
			m_TimePhase.Update();
		}

		// QUICK_END
		m_TimePhase.Change(TimePhase.QUICK_END);
		m_TimePhase.Start();
		yield return null;
		m_TimePhase.Update();
	}
}
