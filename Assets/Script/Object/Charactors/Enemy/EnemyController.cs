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
	// カラー
	public Color color = new Color(255, 81, 81, 255);
	// コンポーネント
	private Animator animator;
	// 付属クラス
	public Rigidbody_grgr rigidbody;
	// 状態
	public Phase<State> state = new Phase<State>();
	// ASCENSION
	private float ascensionTimer;
	private const float ASCENSION_TIME = 3.0f;
	// 地上からの高さ調整
	public float HEIGHT_FROM_GROUND = -0.6f;

#endregion

#region Unity関数

	void Awake(){
		transform.rotation = Random.rotation;
		
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
			case State.MOVE:{
				Move();
			}
			break;
			case State.ASCENSION:{
				Ascension();
			}
			break;
			case State.BATTLE:{
				switch(BattleManager.battle.current){
					case BattleManager.Battle.BATTLE_START:{
						if (BattleManager.battle.IsFirst()){
							animator.speed = 0.0f;
						}
					}
					break;
					case BattleManager.Battle.SKILL_BATTLE_END:{
						if (BattleManager.battle.IsFirst()){
							animator.speed = 1.0f;
						}
						Move();
					}
					break;
					default:{
						animator.speed = 0.1f;
						Move();
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
	void Move(){
		Vector3 moveVelocity = transform.forward * speed;
		
		float length = moveVelocity.magnitude * Time.deltaTime * animator.speed;
		float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
		transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
		transform.position = Rigidbody_grgr.RotateToPosition(transform.up, GameData.GetPlanet().position, GameData.GetPlanet().localScale.y * 0.5f, HEIGHT_FROM_GROUND);
		animator.SetBool("Run", true);
	}
	
	// 昇天
	void Ascension(){
		ascensionTimer -= Time.deltaTime;
		if (ascensionTimer < 0){
			rigidbody.isMove = false;
			transform.rotation = Random.rotation;
			ascensionTimer = ASCENSION_TIME;
			state.Change(State.MOVE);
		}
	}
}
