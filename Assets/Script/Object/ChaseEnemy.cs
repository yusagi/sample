using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseEnemy : MonoBehaviour {

	public Transform player;
	public Transform enemy;
	public Transform arrow;

	public float CHASE_SUCCESS_ANGLE = 15.0f;

	public bool dbg_FlickVelChase;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
		PlayerController pController = player.GetComponent<PlayerController>();
		EnemyController eController = enemy.GetComponent<EnemyController>();

		if (eController.state != EnemyController.State.Move)
			return;

		Vector3 pVel = player.GetComponent<PlayerController>().currentFlickVelocity.normalized;
		if (!dbg_FlickVelChase)
			pVel = player.GetComponent<Rigidbody_grgr>().velocity.normalized;

		Vector3 aVel = arrow.forward;
		float angle = Mathf.Acos(Vector3.Dot(pVel, aVel)) * Mathf.Rad2Deg;
		if (angle < CHASE_SUCCESS_ANGLE){
			pController.state = PlayerController.State.Chase;
			eController.state = EnemyController.State.Escape;
		}
	}
}
