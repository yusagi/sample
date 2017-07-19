using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FEnemyController : Charactor {

#region enum

	public enum State{
			MOVE,
			ASCENSION,
			BATTLE,
			SKILL_BATTLE_END
		}

#endregion

#region メンバ変数

	// 移動速度
	public float speed;

	// カラー
	public Color color;

	// コンポーネント
	private Animator animator;

	// 状態
	public Phase<State> state = new Phase<State>();
	
	// ASCENSION
	private float ascensionTimer;
	private const float ASCENSION_TIME = 3.0f;

#endregion

#region Unity関数

	new void Awake(){
		transform.rotation = Random.rotation;

		PLANET_HEIGHT = -0.6f;
		planetWalk = new PlanetWalk(transform);
		rigidbody = new Rigidbody_grgr(transform);
		rigidbody.isMove = false;

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
	new void Update () {
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
				if (state.IsFirst()){
					animator.speed = 0.0f;
				}
			}
			break;
			case State.SKILL_BATTLE_END:{
				if (state.IsFirst()){
					animator.speed = 1.0f;
				}

				Move();
			}
			break;
		}
		state.Update();

		rigidbody.Update();
		planetWalk.Update(PLANET_HEIGHT);
	}

#endregion

	// 移動
	void Move(){
		Vector3 moveVelocity = transform.forward * speed;
		
		float length = moveVelocity.magnitude * Time.deltaTime * animator.speed;
		float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
		transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;

		animator.SetBool("Run", true);
	}
	
	// 昇天
	void Ascension(){
		ascensionTimer -= Time.deltaTime;
		if (ascensionTimer < 0){
			transform.rotation = Random.rotation;
			planetWalk.isActive = true;
			ascensionTimer = ASCENSION_TIME;
			state.Change(State.MOVE);
		}
	}
}
