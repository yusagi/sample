using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : EnemyControllerBase {

	// 移動速度
	public float speed;

	// カラー
	public Color color;

	// コンポーネント
	private Animator animator;
	private PlanetWalk planetWalk;
	private Rigidbody_grgr rigidbody;
	
	// Ascension
	private float ascensionTimer;
	private const float ASCENSION_TIME = 3.0f;
	

	void Awake(){
		transform.rotation = Random.rotation;

		animator = GetComponent<Animator>();
		planetWalk = GetComponent<PlanetWalk>();
		rigidbody = GetComponent<Rigidbody_grgr>();

		ascensionTimer = ASCENSION_TIME;

		state = State.Move;

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
		switch(state){
			case State.Move:{
				if (rigidbody.isMove)
					rigidbody.isMove = false;
				Move();
			}
			break;
			case State.Escape:{
				if (GameData.GetPlayer().GetComponent<PlayerController>().state != PlayerController.State.Chase){
					state = State.Move;
					return;
				}
				Escape();
			}
			break;
			case State.Ascension:{
				Ascension();
			}
			break;
			case State.HitStop:{
				
			}
			break;
		}

	}

	// 移動
	void Move(){
		Vector3 moveVelocity = transform.forward * speed;
		
		// 動いてなかったら
		if (moveVelocity.magnitude *Time.deltaTime < UtilityMath.epsilon){
			return;
		}
		else{
			float length = moveVelocity.magnitude * Time.deltaTime * animator.speed;
			float angle = length / (2.0f*Mathf.PI*GameData.GetPlanet().transform.localScale.y*0.5f) * 360.0f;
			transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;

			animator.SetFloat("Speed", moveVelocity.magnitude);
		}
	}

	// 逃走
	void Escape(){
		float speed = 20.0f;

		Vector3 moveVelocity = GameData.GetPlayer().GetComponent<Rigidbody_grgr>().velocity.normalized;
		float length = moveVelocity.magnitude * Time.deltaTime * speed;
		float angle = length / (2.0f * Mathf.PI * GameData.GetPlanet().transform.localScale.y * 0.5f) * 360.0f;
		Vector3 front = Vector3.ProjectOnPlane(moveVelocity, transform.up).normalized;
		transform.rotation = Quaternion.LookRotation(front, transform.up);
		transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
		}

	// 昇天
	void Ascension(){
		ascensionTimer -= Time.deltaTime;
		if (ascensionTimer < 0){
			transform.rotation = Random.rotation;
			planetWalk.enabled = true;
			ascensionTimer = ASCENSION_TIME;
			state = State.Move;
		}
	}
}
