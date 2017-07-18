using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControllerBase : MonoBehaviour{
	public enum State{
		Move,
		Escape,
		Ascension,
		HitStop
	}

	// 状態
	public State state{get;set;}
} 