using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBrain : CharBrain {

	// 初期化
	public override void BrainIni(){
		base.BrainIni();
		m_State.StateIni();
	}

	// 更新
	public override void BrainUpdate(){
		base.BrainUpdate();
		
		// 入力情報から状態変更
		if (m_State.IsLock() == false){	
			if (InputManager.IsPlanetTouch() && InputManager.GetTouchType() == InputManager.TouchType.Frick){
				SetInputVelocity(-InputManager.GetFlickVelocity());
				if (m_State.GetState() != CharState.State.FLICK_MOVE){
					m_State.StateChange(CharState.State.FLICK_MOVE);
				}
			}
		}
		
		m_State.StateUpdate();
	}
}
