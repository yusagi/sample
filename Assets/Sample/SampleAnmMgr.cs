using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnmMgr : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	bool isTime = false;
	float timer = 0.0f;
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			GetComponent<Animator>().Play("SampleAnm");
			isTime = true;
		}

		if (isTime){
			timer += Time.deltaTime;
		}

		if (timer >= 0.5f){
			gameObject.SetActive(false);
			isTime = false;
			timer = 0.0f;
		}
	}
}
