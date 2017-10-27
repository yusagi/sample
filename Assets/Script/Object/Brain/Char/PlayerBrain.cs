using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBrain : CharBrain {

	// ビタ止め用タイマー
	private float m_PushStopTimer;

	// 初期化
	public override void BrainIni(){
		base.BrainIni();
		m_State.StateIni();

		m_PushStopTimer = GetInfo().PUSH_STOP_TIME;

		// スキルデータ管理初期化
		((DeckManager)m_State.GetSkillDataManager()).SetDeck(DeckDataBase.PLAYER_DECK);
	}

	// 更新
	public override void BrainUpdate(){
		base.BrainUpdate();
		
		// 入力情報から状態変更
		if (m_State.IsLock() == false){	
			// 球体に触れてる
			if (InputManager.IsPlanetTouch()){
				InputManager.TouchType touchType = InputManager.GetTouchType();
				// フリックした
				if (touchType == InputManager.TouchType.Frick){
					Vector3 inputVelocity = -InputManager.GetFlickVelocity();
					float flickPow = inputVelocity.magnitude;
					flickPow = Mathf.Max(flickPow, 1080.0f);
					inputVelocity = inputVelocity.normalized * flickPow;
					SetInputVelocity(inputVelocity);
					if (m_State.GetState() != CharState.State.FLICK_MOVE){
						m_State.StateChange(CharState.State.FLICK_MOVE);
					}
				}
				// フリック移動中
				if (m_State.GetState() == CharState.State.FLICK_MOVE){
					if (InputManager.GetTouchType() == InputManager.TouchType.Touch){
						m_PushStopTimer -= Time.deltaTime;
						if (m_PushStopTimer <= 0){
							m_State.StateChange(CharState.State.STOP);
							m_PushStopTimer = GetInfo().PUSH_STOP_TIME;
						}
					}
					else{
						m_PushStopTimer = GetInfo().PUSH_STOP_TIME;
					}
				}
				
			}
		}
		
		m_State.StateUpdate();
	}
}
