using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBrain : CharBrain {

	public float FLICK_INTERVAL = 1.0f;
	[Range(0, 1080)]
	public float m_FlickPow;
	private float m_FlickTimer;

	// 初期化
	public override void BrainIni(){
		m_State.StateIni();

		m_FlickTimer = FLICK_INTERVAL;

		Quaternion rot = Quaternion.LookRotation(Vector3.back, Vector3.down);
		transform.rotation = rot;

		// スキルデータ管理設定
		((DeckManager)m_State.GetSkillDataManager()).SetDeck(DeckDataBase.ENEMY_DECK);
	}

	// 更新
	public override void BrainUpdate(){
		base.BrainUpdate();

		// 入力情報から状態変更
		if (m_State.IsLock() == false){	
			if (m_FlickTimer < 0){
				SetInputVelocity(Vector3.forward * m_FlickPow);
				m_FlickTimer = FLICK_INTERVAL;
				if (m_State.GetState() != CharState.State.FLICK_MOVE){
					m_State.StateChange(CharState.State.FLICK_MOVE);
				}
			}
		}
		
		m_State.StateUpdate();

		m_FlickTimer -= Time.deltaTime;
	}
}
