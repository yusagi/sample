using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerBase : MonoBehaviour {

	public enum State{
		Stop,
		Move,
		Chase,
		HitStop,
	}

	// 状態
	public State state{get;set;}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
