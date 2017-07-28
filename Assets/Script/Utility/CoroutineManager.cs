using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour {
	private static CoroutineManager _instance;

	void Awake(){
		_instance = this;
	}

	public static void LocalStartCoroutine(IEnumerator coroutine){
		_instance.localStartCoroutine(coroutine);
	}

	private void localStartCoroutine(IEnumerator coroutine){
		StartCoroutine(coroutine);
	}
}