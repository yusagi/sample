using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

#region enum

	public enum State{
			MOVE,
			ASCENSION,
			BATTLE,
		}

#endregion

#region メンバ変数

	// 移動速度
	public float speed = 10.0f;
	public float resultSpeed = 10.0f;
	// カラー
	public Color color = new Color(255, 81, 81, 255);
	// コンポーネント
	public AnimatorChecker animator;
	// 付属クラス
	public Rigidbody_grgr rigidbody;
	public SkillManager skillManager{get;set;}
	// 状態
	public Phase<State> state = new Phase<State>();
	// ASCENSION
	private float ascensionTimer;
	private const float ASCENSION_TIME = 3.0f;
	// 地上からの高さ調整
	public float HEIGHT_FROM_GROUND = -0.6f;
	// HP
	public int hp = 100;
	// AP
	public float ap{get;set;}
	// コルーチン
	IEnumerator<Quaternion> qLerp;

#endregion

#region Unity関数

	void Awake(){
		transform.rotation = Random.rotation;
		
		skillManager = new SkillManager();
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.JAB]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.HIKICK]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.SPINKICK]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.DEFENSE]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.COUNTER]);

		rigidbody = new Rigidbody_grgr(transform);
		rigidbody.isMove = false;

		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);

		animator = GetComponent<AnimatorChecker>();

		ascensionTimer = ASCENSION_TIME;
		
		state.Change(State.MOVE);

		Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
		foreach(Renderer renderer in renderers){
			renderer.material.color = color;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		state.Start();
		switch(state.current){
			// 移動
			case State.MOVE:{
				animator.m_Animator.SetBool("Run", true);
				Move(transform.forward * speed);
			}
			break;
			// 昇天
			case State.ASCENSION:{
				Ascension();
			}
			break;
			// バトル
			case State.BATTLE:{
				BattleUpdate();
			}
			break;
		}
		state.Update();

		rigidbody.Update();
	}

#endregion

#region 移動
	// 移動
	void Move(Vector3 velocity){
		if (velocity.magnitude > UtilityMath.epsilon){
			float length = velocity.magnitude * Time.deltaTime * animator.m_Animator.speed;
			float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
			transform.rotation = Quaternion.LookRotation(velocity, transform.up);
			transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
		}
		
		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
	}
#endregion

#region 昇天
	void AscensionIni(Vector3 impact){
		rigidbody.isMove = true;
		rigidbody.velocity = impact;
		animator.m_Animator.Play("DamageDown");
	}

	// 昇天
	void Ascension(){
		ascensionTimer -= Time.deltaTime;
		if (ascensionTimer < 0){
			rigidbody.isMove = false;
			transform.rotation = Random.rotation;
			ascensionTimer = ASCENSION_TIME;
			animator.m_Animator.Play("Idle");
			hp = 100;
			state.Change(State.MOVE);
		}
	}

#endregion

#region バトルモード
	// バトルモード更新
	void BattleUpdate(){
			if (BattleManager._instance.battle.IsFirst()){
				//animator.m_Animator.speed = BattleManager._instance.SLOW_START;
			}

			switch(BattleManager._instance.battle.current){
				// スキルバトルスタート
				case BattleManager.Battle.SKILL_BATTLE_START:{
					if (BattleManager._instance.battle.IsFirst()){
						return;
					}
					animator.m_Animator.speed = BattleManager._instance.slowSpeed;
					Move(transform.forward * speed);
				}
				break;
				// スキルバトルプレイ
				case BattleManager.Battle.SKILL_BATTLE_PLAY:{
					if (BattleManager._instance.battle.IsFirst()){
						// 向きを敵がわに
						Vector3 toPlayer = (GameData.GetPlayer().position - transform.position);
						Vector3 front = Vector3.ProjectOnPlane(toPlayer, transform.up).normalized;
						transform.rotation = Quaternion.LookRotation(front, transform.up);

						float angle = Mathf.Acos(Vector3.Dot(transform.up, GameData.GetPlayer().up)) * Mathf.Rad2Deg * 0.5f - 2.0f;
						Vector3 axis = Vector3.Cross(transform.up, GameData.GetPlayer().up);
						qLerp = UtilityMath.QLerp(transform.rotation, Quaternion.AngleAxis(angle, axis) * transform.rotation, BattleManager.SKILLBATTLE_PLAY_TIME);

						animator.m_Animator.speed = 1.0f;
						animator.m_Animator.SetBool("Run", false);
					}

					qLerp.MoveNext();
					transform.rotation = qLerp.Current;
					Move(Vector3.zero);
				}
				break;
				// スキルバトル結果
				case BattleManager.Battle.SKILL_BATTLE_RESULT:{
					if (BattleManager._instance.battle.IsFirst()){
						animator.m_Animator.speed = BattleManager.SKILLBATTLE_ANIMATION_TIME;
						return;
					}
					BattleResultUpdate();
				}
				break;
				// バトル終了
				case BattleManager.Battle.BATTLE_END:{
					if (BattleManager._instance.battle.IsFirst()){
						animator.m_Animator.speed = 1.0f;

						// hpが0
						if (hp <= 0){
							Vector3 impact = transform.up;
							AscensionIni(impact);
							state.Change(State.ASCENSION);
						}
						else{
							animator.m_Animator.Play("Idle");
							state.Change(State.MOVE);
						}
					}
				}	
				break;
			}
	}

	// バトルリザルト更新
	void BattleResultUpdate(){
		BattleManager.ResultPhase phase = BattleManager._instance.resultPahse.current;
			switch(phase){
				case BattleManager.ResultPhase.FIRST:{
					if (BattleManager._instance.resultPahse.IsFirst()){
						BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
					}
				}
				break;
				case BattleManager.ResultPhase.SECOND:{
					if (BattleManager._instance.resultPahse.IsFirst()){
						BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
					}
				}
				break;
				case BattleManager.ResultPhase.THIRD:{
					if (BattleManager._instance.resultPahse.IsFirst()){
						BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
					}
				}
				break;
				case BattleManager.ResultPhase.FOURTH:{
					if (BattleManager._instance.resultPahse.IsFirst()){
						BattleResultSetAnm(BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>(), phase);
					}
				}
				break;
			}
	}

	// バトルリザルトセットアニメーション
	void BattleResultSetAnm(SkillChoiceBoardController controller, BattleManager.ResultPhase resultPhase){
		int damage = 0;
		switch(controller.GetAnimationType(resultPhase, SkillChoiceBoardController.DataType.ENEMY)){
			case AnimationType.NORMAL_ATTACK:{
				SkillData data = controller.GetSkillData(resultPhase, SkillChoiceBoardController.DataType.ENEMY);
				animator.Play(data._anmName);
				damage = data._attack;
			}
			break;
			case AnimationType.COUNTER_ATTACK:{
				SkillData data = controller.GetSkillData(resultPhase, SkillChoiceBoardController.DataType.PLAYER);
				animator.Play("Land", 0.4f);
				animator.Play("Rising_P");
				damage = data._attack;
				hp += damage;
			}
			break;
		}

		GameData.GetPlayer().GetComponent<PlayerController>().hp -= damage;
	}
#endregion
}
