using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillerDeadTime : MonoBehaviour {

#region メンバ変数

	public bool isDead { get; set; }
	private int DEAD_TIME = 3;
	private float timer;

#endregion

#region Unity関数

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

#endregion
}
