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
	public float FIRST_SLOW_START1 = 0.3f;
	public float FIRST_SLOW_END1 = 0.005f;
	public float FIRST_SLOW_TIME1 = 1.0f;
	public float UI_SELECT_TIME = 2.0f;				// UI選択時間
	public float WAIT_TIME = 0.0f;					// 通常再生までの待ち時間

	// コルーチン
	private Coroutine m_BattleSlow;
	private Coroutine m_BattlePhaseProcess;
	
	// 各種定数
	public float EXCLAMATION_TIME;			// びっくりマークの表示時間
	public float BATTLE_PLAY_FRONT_DISTANCE; // お互いに走ってる方向が同じときのスロー開始距離
	public float BATTLE_PLAY_BACK_DISTANCE;  // 片方が後ろから接敵したときのスロー開始距離
	public float BATTLE_PLAY_SPEED;			// バトルモードで接敵した時のキャラの速度
	public float BATTLE_START_DISTANCE;		// バトルモード開始の索敵距離
	public float MORE_SLOW = 0.01f;					// よりスローの値
	public float MORE_SLOW_TIME = 0.5f; 			// よりスローである時間
	public float BATTLE_NORMAL_TIME = 0.8f;			// 通常再生時速度
	public float DELAY_UI_SLIDEIN_TIME = 0.6f; 		// スキルUIが再びスライドインするまでの遅延時間
	public float UI_OUT_TIME = 0.2f;				// UIがはけるまでの時間

	private int MAX_CHOICES = 4;	// スキルカードの選択肢の数
	private int MAX_SELECTS = 1;	// 選べるスキル数

	private float BATTLE_START_INTERVAL = 4.0f;	// 次のバトルを開始できるまでのインターバル

	// 接敵時スロー開始距離
	private float m_PlayDistance;

	// デバグ用
	public bool dbg_TargetUIFiedlActive;
	public bool dbg_TargetSelectUIActive;

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
		m_ResultPhase.Change(SkillBattlePhase.START);
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
						bool pIsRotate = Rigidbody_grgr.MoveToRotation(out pRotation, planetCenter, planetRad, setPosition, -m_Player.transform.forward * m_PlayDistance * 0.48f, m_Player.transform.forward);
						bool tIsRotate = Rigidbody_grgr.MoveToRotation(out tRotation, planetCenter, planetRad, setPosition, -m_Target.transform.forward * m_PlayDistance * 0.48f, m_Target.transform.forward);
						Vector3 pPosition = Rigidbody_grgr.RotateToPosition(pRotation * Vector3.up, planetCenter, planetRad, m_Player.GROUND_UP + m_Player.GetBrain().GetInfo().m_Jamp);
						Vector3 tPosition = Rigidbody_grgr.RotateToPosition(tRotation * Vector3.up, planetCenter, planetRad, m_Target.GROUND_UP + m_Target.GetBrain().GetInfo().m_Jamp);
						if (pIsRotate && tIsRotate){
							m_Player.GetBrain().SetTransform(pRotation, pPosition);
							m_Target.GetBrain().SetTransform(tRotation, tPosition);
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
						m_BattleSlow = StartCoroutine(BattleSlow(FIRST_SLOW_START1, FIRST_SLOW_END1, FIRST_SLOW_TIME1, UI_SELECT_TIME, FIRST_SLOW_START1, 0));
#if UNITY_EDITOR
						// ログ消去
						Dbg.ClearConsole();
#endif
					}

					switch(m_TimePhase.current){
						// SLOW_END
						case TimePhase.SLOW_END:{
							if (m_TimePhase.IsFirst()){
								// UIバトルマネージャーアクティブ
								m_SkillBattleManager.gameObject.SetActive(true);
							}
						}
						break;
						//  KEEP
						case TimePhase.KEEP:{
							if (m_TimePhase.IsFirst()){
								// プレイヤーカード生成
								m_SkillBattleManager.AddCardObject(m_Player.gameObject, SkillBattleManager.MAX_CHOICES);

								// 敵カード生成
								m_SkillBattleManager.AddCardObject(m_Target.gameObject, SkillBattleManager.MAX_CHOICES);

								// スライドインアニメーション開始
								m_SkillBattleManager.UIFieldSlideIn();
								m_SkillBattleManager.PlayerFieldSelectEnable();
								break;
							}
							// カード選択したか
							if (m_SkillBattleManager.GetChoiceCount(m_Player.gameObject) > 0){
								// ボタンオフ
								m_SkillBattleManager.PlayerFieldSelectDisable();

								// 敵カード選択
								int tCardNum = m_SkillBattleManager.GetCardList(m_Target.gameObject).Count;
								if (tCardNum != 0){
									GameObject card = m_SkillBattleManager.GetCardList(m_Target.gameObject)[Random.Range(0, tCardNum)].gameObject;
									m_SkillBattleManager.AddChoice(m_Target.gameObject, card.transform);
								}

								// 未選択カードをフェードアウト
								m_SkillBattleManager.UnselectedFadeOut(m_Player.gameObject, m_Target.gameObject);

								// プレイへ移行
								m_Battle.Change(Battle.SKILL_BATTLE_PLAY);
								m_Battle.Start();
								return;
							}
						}
						break;
						// QUICK
						case TimePhase.KEEP_END:{
						}
						break;
						// QUICK_END
						case TimePhase.QUICK_END:{
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

		// 敵カード表示/非表示
		bool isDisplayActive = dbg_TargetUIFiedlActive;
		if (m_SkillBattleManager.GetChoiceCount(m_Target.gameObject) > 0){
			foreach(SkillCardUI ui in m_SkillBattleManager.GetChoices(m_Target.gameObject)){
				ui.m_CanvasGroup.ignoreParentGroups = dbg_TargetSelectUIActive;
			}
		}
		m_SkillBattleManager.UIFieldDisplayActive(SkillBattleManager.USER.TARGET, isDisplayActive);

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
			m_BattleInterval -= Time.deltaTime;
			if (m_BattleInterval > 0){
				return;
			}

		// バトルモード突入条件
		float arc = Rigidbody_grgr.Arc(m_Planet.position, m_Planet.localScale.y * 0.5f, m_Player.transform.position, m_Target.transform.position);
		bool tmpIsBattle = (arc < BATTLE_START_DISTANCE);

		// バトルモード条件
		if (tmpIsBattle)
		{
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
			if (angle >= 135.0f){
				pBattle.SetBattleType(SkillBattle.BattleType.FRONT);
				tBattle.SetBattleType(SkillBattle.BattleType.FRONT);

				m_PlayDistance = BATTLE_PLAY_FRONT_DISTANCE;
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

				m_PlayDistance = BATTLE_PLAY_BACK_DISTANCE;
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
	
	// バトルリザルト更新
	void ResultUpdate()
	{
		switch (m_ResultPhase.current)
		{
			case SkillBattlePhase.START:{
				SkillBattlePhase nextPhase = (SkillBattlePhase)((int)SkillBattlePhase.START + 1);
                m_SkillBattleManager.Battle(m_Player.gameObject, m_Target.gameObject, nextPhase);
                StopCoroutine(m_BattleSlow);
				Time.timeScale = FIRST_SLOW_START1;
				m_ResultPhase.Change(nextPhase);
				m_ResultPhase.Start();
				return;
			}
			case SkillBattlePhase.END:{ break; }
			default:
				{
					// フェーズ開始時
					if (m_ResultPhase.IsFirst())
					{
						m_BattlePhaseProcess = StartCoroutine(BattlePhaseProcess());
					}

					
					// フェーズ処理終了
					if (m_BattlePhaseProcess == null){
						int nextPhase = (int)m_ResultPhase.current + 1;
						m_ResultPhase.Change((SkillBattlePhase)nextPhase);
						m_ResultPhase.Start();
						return;
					}
				}
				break;
		}
		m_ResultPhase.Update();
	}
	
	// フェーズ処理コルーチン
	IEnumerator BattlePhaseProcess(){
		// リザルトフェーズ取得
		SkillBattlePhase phase = m_ResultPhase.current;

		// フェーズの勝者取得
		CharCore winner = m_SkillBattleManager.GetWinner(phase).GetComponent<CharCore>();
		AnimationManager winnerAnmMgr = winner.GetBrain().GetState().GetAnmMgr();

		// アニメーション切り替え中はリターン
		// while(winnerAnmMgr.GetState() == AnmState.CHANGE){
		// 	yield return null;
		// }

		// // よりスローポイント到達までリターン
		// while(winnerAnmMgr.GetPlayAnmData()._slowEndFrame >= winnerAnmMgr.GetFrame()){
		// 	yield return null;
		// }

		// よりスロー中
		// Time.timeScale = MORE_SLOW;
		// bool isUIFaedOut = false;
		// bool isMoreSlow = true;
		// float moreSlowTimer = 0;
		// while(isMoreSlow){
		// 	moreSlowTimer += Time.unscaledDeltaTime;
		// 	// 途中でUIがはける
		// 	if (isUIFaedOut == false && moreSlowTimer >= UI_OUT_TIME){
		// 		isUIFaedOut = true;
		// 		// スキルバトルのリザルトタイプ格納
		// 		SkillBattleManager.RESULT result;
		// 		result = m_SkillBattleManager.GetResultData(m_Player.gameObject, phase)._result;
		// 		foreach(SkillCardUI ui in m_SkillBattleManager.GetChoices(m_Player.gameObject)){
		// 			m_SkillBattleManager.UIFadeOut(ui);
		// 			// バトルに勝ってるなら1枚目だけフェードアウト
		// 			if (result == SkillBattleManager.RESULT.WIN){
		// 				break;
		// 			}
		// 		}
		// 		result = m_SkillBattleManager.GetResultData(m_Target.gameObject, phase)._result;
		// 		foreach(SkillCardUI ui in m_SkillBattleManager.GetChoices(m_Target.gameObject)){
		// 			m_SkillBattleManager.UIFadeOut(ui);
		// 			if (result == SkillBattleManager.RESULT.WIN){
		// 				break;
		// 			}
		// 		}
		// 	}

		// 	if (moreSlowTimer >= MORE_SLOW_TIME){
		// 		isMoreSlow = false;
		// 		// 再生速度を早回しにする
		// 		Time.timeScale = BATTLE_NORMAL_TIME;
		// 	}
		// 	else{
		// 		yield return null;
		// 	}
		// }

		// スロー解除待ち時間
		float timer = 0.0f;
		while(timer < WAIT_TIME){
			timer += Time.unscaledDeltaTime;
			yield return null;
		}

		// スキルバトルのリザルトタイプ格納
		SkillBattleManager.RESULT result;
		result = m_SkillBattleManager.GetResultData(m_Player.gameObject, phase)._result;
		foreach(SkillCardUI ui in m_SkillBattleManager.GetChoices(m_Player.gameObject)){
			m_SkillBattleManager.UIFadeOut(ui);
			// バトルに勝ってるなら1枚目だけフェードアウト
			if (result == SkillBattleManager.RESULT.WIN){
				break;
			}
		}
		result = m_SkillBattleManager.GetResultData(m_Target.gameObject, phase)._result;
		foreach(SkillCardUI ui in m_SkillBattleManager.GetChoices(m_Target.gameObject)){
			m_SkillBattleManager.UIFadeOut(ui);
			if (result == SkillBattleManager.RESULT.WIN){
				break;
			}
		}
		// 通常再生
		Time.timeScale = BATTLE_NORMAL_TIME;


		// アニメーション連続再生中
		while(winnerAnmMgr.GetAnmList().Count > 1 || winnerAnmMgr.GetState() == AnmState.CHAINE){
			// 追撃UIフェードアウト
			if (winnerAnmMgr.GetState() == AnmState.CHAINE){
				foreach(SkillCardUI ui in m_SkillBattleManager.GetChoices(winner.gameObject)){
					if (!ui.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("FeadOut")){
						m_SkillBattleManager.UIFadeOut(ui);
						break;
					}
				}
			}
			yield return null;
		}
		while(winnerAnmMgr.GetState() == AnmState.CHANGE){
			yield return null;
		}
		while(winnerAnmMgr.GetPlayAnmData()._slowEndFrame > winnerAnmMgr.GetFrame()){
			yield return null;
		}

		// スキルUIをリセット
		m_SkillBattleManager.UIFieldIdle();
		m_SkillBattleManager.AllUIIdle();
		// 選択カードリセット
		m_SkillBattleManager.ChoiceReset();
		// 削除予定執行
		m_SkillBattleManager.DestoryEnforcement();

		// どちらかのHPが0以下になってる または 最終フェーズならバトル終了
		bool isFin = (m_Player.GetBrain().GetInfo().m_HP <= 0 || m_Target.GetBrain().GetInfo().m_HP <= 0) || m_ResultPhase.current == (SkillBattlePhase)((int)SkillBattlePhase.END - 1);
		if (isFin == true){
			m_ResultPhase.Change(SkillBattlePhase.END);
			m_ResultPhase.Start();
			yield break;
		}

		// カード補充
		m_SkillBattleManager.AddCardObject(m_Player.gameObject, 4);
		m_SkillBattleManager.AddCardObject(m_Target.gameObject, 4);

		// スロー再生開始ポイントまで待機
		while(winnerAnmMgr.GetPlayAnmData()._slowStartFrame > winnerAnmMgr.GetFrame()){
			yield return null;
		}
		
		int endFrame = winnerAnmMgr.GetPlayAnmData()._endFrame;
		int currentFrame = winnerAnmMgr.GetFrame();
		int totalFrame = endFrame - currentFrame;
		float totalTime = (float)totalFrame / AnimationManager.FRAME_RATE; // fpsで秒数を求める

		// totalTime = Time.timeScale * (UI_SELECT_TIME + DELAY_UI_SLIDEIN_TIME);

		float slow = totalTime / (UI_SELECT_TIME + DELAY_UI_SLIDEIN_TIME);
		slow = Mathf.Max(slow, Vector3.kEpsilon);

		// スロー開始
		m_BattleSlow = StartCoroutine(BattleSlow(slow, slow, 0, UI_SELECT_TIME + DELAY_UI_SLIDEIN_TIME, FIRST_SLOW_START1, 0));
		
		
		// UI復活時間まで待機
		float delayUISlideInTimer = 0;
		while(delayUISlideInTimer < DELAY_UI_SLIDEIN_TIME){
			delayUISlideInTimer += Time.unscaledDeltaTime;
			yield return null;
		}

		// UI復活
		m_SkillBattleManager.PlayerFieldSelectEnable();
		m_SkillBattleManager.UIFieldSlideIn();


		// プレイヤーがカードを選択するかスロータイプがキープエンドに到達するまで待機
		while(m_SkillBattleManager.GetChoiceCount(m_Player.gameObject) == 0 && m_TimePhase.current != TimePhase.KEEP_END){
			yield return null;
		}

		m_TimePhase.Change(TimePhase.QUICK);
		m_TimePhase.Start();
		yield return null;

		// ボタンをオフ
		m_SkillBattleManager.PlayerFieldSelectDisable();

		// 敵カード選択	
		int tCardNum = m_SkillBattleManager.GetCardList(m_Target.gameObject).Count;
		if (tCardNum != 0){
			GameObject card = m_SkillBattleManager.GetCardList(m_Target.gameObject)[Random.Range(0, tCardNum)].gameObject;
			m_SkillBattleManager.AddChoice(m_Target.gameObject, card.transform);
		}
		// 未選択UIフェードアウト
		m_SkillBattleManager.UnselectedFadeOut(m_Player.gameObject, m_Target.gameObject);

		SkillBattlePhase nextPhase = (SkillBattlePhase)((int)phase  +1);
		m_SkillBattleManager.Battle(m_Player.gameObject, m_Target.gameObject, nextPhase);
		StopCoroutine(m_BattleSlow);
		m_BattlePhaseProcess = null;
		// 終了
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
		if (distance <= m_PlayDistance){
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
		areaLine.arc = m_PlayDistance * 0.5f;
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

	// バトル中スローコルーチン
	IEnumerator BattleSlow(float slowStart1, float slowEnd1, float slowTime1, float keepTime, float slowEnd2, float slowTime2)
	{
		SkillBattleManager board = m_SkillBattleManager;
		m_TimePhase.Change(TimePhase.SLOW);
		m_TimePhase.Start();
		
		IEnumerator<float> timeScale = null;
		float keepTimer = 0.0f;

		bool loopEnd = false;
		
		while(loopEnd == false){
			switch(m_TimePhase.current){
				// SLOW
				case TimePhase.SLOW:{
					if (m_TimePhase.IsFirst()){
						timeScale = UtilityMath.FLerp(slowStart1, slowEnd1, slowTime1, EaseType.OUT_EXP, 1, true);
					}

					if (timeScale.MoveNext()){
						Time.timeScale = timeScale.Current;
						m_TimePhase.Update();
					}
					else{
						m_TimePhase.Change(TimePhase.SLOW_END);
						m_TimePhase.Start();
					}
				}
				break;
				// SLOW_END
				case TimePhase.SLOW_END:{
					m_TimePhase.Change(TimePhase.KEEP);
					m_TimePhase.Start();
				}
				break;
				// KEEP
				case TimePhase.KEEP:{
					if (m_TimePhase.IsFirst()){
						keepTimer = 0.0f;
					}

					if (keepTimer < keepTime){
						keepTimer += Time.unscaledDeltaTime;
						m_TimePhase.Update();
					}
					else{
						m_TimePhase.Change(TimePhase.KEEP_END);
						m_TimePhase.Start();
					}
				}
				break;
				// KEEP_END
				case TimePhase.KEEP_END:{
					m_TimePhase.Change(TimePhase.QUICK);
					m_TimePhase.Start();
				}
				break;
				// QUICK
				case TimePhase.QUICK:{
					if (m_TimePhase.IsFirst()){
						timeScale = UtilityMath.FLerp(Time.timeScale, slowEnd2, slowTime2, EaseType.OUT_EXP, 1, true);
					}

					if (timeScale.MoveNext()){
						Time.timeScale = timeScale.Current;
						m_TimePhase.Update();
					}
					else{
						m_TimePhase.Change(TimePhase.QUICK_END);
						m_TimePhase.Start();
					}
				}
				break;
				// QUICK_END
				case TimePhase.QUICK_END:{
					m_TimePhase.Update();
					loopEnd = true;
				}
				break;
			}

			yield return null;
		}
	}
}
