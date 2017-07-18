using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillerDeadTime : MonoBehaviour {

	
	public bool isDead { get; set; }
	private int DEAD_TIME = 3;
	private float timer;

	void Awake(){
		timer = DEAD_TIME;
		isDead = false;
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (isDead){
			timer -= Time.deltaTime;
			if (timer <= 0.0f){
				GameObject.Destroy(gameObject);
			}
		}
	}
}
