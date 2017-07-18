using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// フェーズ管理
public class Phase<T>{

	public Dictionary<T, Action> onChangeEvent = new Dictionary<T, Action>();

	public T current{get;set;}

	public float phaseTime;
	public float globalTime;
	public float oldPhaseTime = -1;

	// フェーズ更新後の初回アップデートか
	public bool IsFirst(){
		return phaseTime == 0.0f && oldPhaseTime == -1.0f;
	}

	// フェーズ変更イベントの設定
	public void AddEvent(T t, Action fp){
		onChangeEvent.Add(t, fp);
	}

	// フェーズの時間をチェック
	public bool CheckPhaseTime(float t){
		if (phaseTime >= t && oldPhaseTime < t){
			return true;
		}
		return false;
	}

	// フェーズの変更
	public void Change(T t){
		if (onChangeEvent.ContainsKey(t)){
			onChangeEvent[t]();
		}
		phaseTime = -1;
		oldPhaseTime = -1;
		current = t;
	}

	// フェーズ更新スタート
	public void Start(){
		if (phaseTime < 0){
			phaseTime = 0;
		}
	}

	// フェーズ時間の更新
	public void Update(){
		oldPhaseTime = phaseTime;
		phaseTime += Time.deltaTime;
		globalTime += Time.deltaTime;
	}
}