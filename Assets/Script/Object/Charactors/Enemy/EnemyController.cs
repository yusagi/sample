﻿using System.Collections;
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
	private Animator animator;
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
	public float hp = 100;
	// AP
	public float ap{get;set;}

#endregion

#region Unity関数

	void Awake(){
		transform.rotation = Random.rotation;
		
		skillManager = new SkillManager();
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.PUNCH]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.KICK]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.HIGH_KICK]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.DEFENSE]);
		skillManager.AddSkill(SkillDataBase.DATAS[SkillType.COUNTER]);

		rigidbody = new Rigidbody_grgr(transform);
		rigidbody.isMove = false;

		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);

		animator = GetComponent<Animator>();

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
				if (BattleManager._instance.battle.IsFirst()){
					animator.speed = BattleManager._instance.SLOW_START;
				}
				switch(BattleManager._instance.battle.current){
					// スキルバトル結果
					case BattleManager.Battle.SKILL_BATTLE_RESULT:{
						if (BattleManager._instance.battle.IsFirst()){
							animator.speed = 1.0f;
							rigidbody.velocity = Vector3.zero;
						}

						// 勝敗
						switch(skillManager.result){
							// 勝ち
							case SkillAttributeResult.WIN:{
								Vector3 toPlayer = GameData.GetPlayer().position - transform.position;
								Vector3 front = Vector3.ProjectOnPlane(toPlayer, transform.up).normalized;
								rigidbody.velocity = front * rigidbody.GetSpeed();
								rigidbody.AddForce(front * resultSpeed);
								Move(rigidbody.velocity);
							}
							break;
							// それ以外
							default:{
								Move(transform.forward * speed);
							}
							break;
						}
					}
					break;
					// バトル終了
					case BattleManager.Battle.BATTLE_END:{
						if (BattleManager._instance.battle.IsFirst()){
							animator.speed = 1.0f;
							state.Change(State.MOVE);
						}
					}	
					break;
					default:{
						float start = BattleManager._instance.SLOW_START;
						float end = BattleManager._instance.SLOW_END;
						float t = (BattleManager._instance.SLOW_TIME - BattleManager._instance.slowTimer) / BattleManager._instance.SLOW_TIME;
						animator.speed = Mathf.Lerp(start, end, t);
						Move(transform.forward * speed);
					}
					break;
				}
			}
			break;
		}
		state.Update();

		rigidbody.Update();
	}

#endregion

	// 移動
	void Move(Vector3 velocity){
		if (velocity.magnitude > UtilityMath.epsilon){
			float length = velocity.magnitude * Time.deltaTime * animator.speed;
			float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
			transform.rotation = Quaternion.LookRotation(velocity, transform.up);
			transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
			animator.SetBool("Run", true);
		}
		else{
			animator.SetBool("Run", false);
		}
		
		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
	}
	
	// 昇天
	void Ascension(){
		ascensionTimer -= Time.deltaTime;
		if (ascensionTimer < 0){
			rigidbody.isMove = false;
			transform.rotation = Random.rotation;
			ascensionTimer = ASCENSION_TIME;
			animator.SetBool("DamageDown", false);
			state.Change(State.MOVE);
		}
	}
}
