using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleCoroutine : MonoBehaviour {

	// コルーチンは同じフレーム中に複数個スタートしたら「yield return null」 をした順で更新されてくっぽい

	private Coroutine sample;

	// Use this for initialization
	void Start () {
		sample = StartCoroutine(sample1());
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log(sample);
	}

	IEnumerator sample1(){
		yield return null;

		sample = null;
	}

	IEnumerator sample2(){
		while(true){
			Debug.Log("sample2");
			yield return null;
		}
	}
}
