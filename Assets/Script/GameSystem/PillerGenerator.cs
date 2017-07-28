﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillerGenerator : MonoBehaviour {

	public Transform pillers;
	public float INTERVAL = 5.0f;
	public int GENERATE_NUM = 10;
	public int MAX_PILLER = 500;

	private float timer = 0.0f;

	// Use this for initialization
	void Start () {
		transform.rotation = Quaternion.identity;
		int count = 100;
		while(count != 0){
			if (!PillerGenerate())
				continue;
			count--;
		}
		timer = INTERVAL;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate(){
		timer -= Time.deltaTime;
		if (timer <= 0.0f){
			int count = GENERATE_NUM;
			while(count != 0){
				if (IsMaxPiller())
					break;
				if (!PillerGenerate())
					continue;
				count--;
			}
			timer = INTERVAL;
		}
	}

	bool PillerGenerate(){
			transform.rotation = Random.rotation;
			RaycastHit hitInfo;
			Physics.Raycast(transform.position, transform.up, out hitInfo, Mathf.Infinity, (int)Layer.PLAYER);
			if (hitInfo.collider != null){
				return false;
			}
			GameObject clone = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Piller"), Vector3.zero, transform.rotation);
			clone.transform.SetParent(pillers, false);
			return true;
	}

	bool IsMaxPiller(){
		return (pillers.childCount == MAX_PILLER);
	}
}