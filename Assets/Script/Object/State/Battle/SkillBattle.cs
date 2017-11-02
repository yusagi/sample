using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillBattle : BattleBase{

	public enum BattleType{
		FRONT,	// 正面バトル
		BACK,	// 背後からバトル
	}

	private BattleManager m_BattleManager;
	private CharBrain m_Brain;
	
	private BattleType m_BattleType;
	private CharBrain m_TargetBrain;
	private Vector3 m_TargetVel;

	private Coroutine m_DamageCoroutine;

	private float m_SaveSpeed;

	public SkillBattle(BattleManager battleManager, CharBrain brain){
		m_BattleManager = battleManager;
		m_Brain = brain;
	}

	public override void BattleIni(){
		m_DamageCoroutine = null;
	}

	public override void BattleUpdate(){
		
		switch(m_BattleType){
			// 正面バトル
			case BattleType.FRONT:{
				switch(m_BattleManager.m_Battle.current){
					// エンカウント
					case BattleManager.Battle.BATTLE_ENCOUNT:
					// バトルスタート
					case BattleManager.Battle.SKILL_BATTLE_START:
					{
						if (m_BattleManager.m_Battle.current == BattleManager.Battle.BATTLE_ENCOUNT && m_BattleManager.m_Battle.IsFirst()){
							m_SaveSpeed = m_Brain.GetInfo().m_CurrentSpeed;
						}
						m_TargetVel = Vector3.ProjectOnPlane(m_TargetVel, m_Brain.transform.up).normalized;
						m_Brain.SetInputVelocity(m_TargetVel * m_Brain.GetInfo().m_CurrentSpeed);
					}
					break;
					// バトルプレイ
					case BattleManager.Battle.SKILL_BATTLE_PLAY:{
						if (m_BattleManager.m_Battle.IsFirst()){
							m_Brain.SetInputVelocity(Vector3.zero);
						}
						BattleResultUpdate();
					}
					break;
					// バトル終了
					case BattleManager.Battle.BATTLE_END:{
						if (m_Brain.GetInfo().m_HP > 0){
							m_Brain.GetState().UnLock();
							m_Brain.SetNextState(CharState.State.FLICK_MOVE, false);
							m_Brain.SetInputVelocity(m_Brain.transform.forward * m_SaveSpeed);
						}
						else{
							m_Brain.GetState().UnLock();
							m_Brain.SetNextState(CharState.State.DOWN, true);
						}
					}	
					break;
				}
			}
			break;
			// 背面バトル
			case BattleType.BACK:{
				switch(m_BattleManager.m_Battle.current){
					// エンカウント
					case BattleManager.Battle.BATTLE_ENCOUNT:
					// バトルスタート
					case BattleManager.Battle.SKILL_BATTLE_START:
					{
						if (m_BattleManager.m_Battle.current == BattleManager.Battle.BATTLE_ENCOUNT && m_BattleManager.m_Battle.IsFirst()){
							m_Brain.GetState().GetAnmMgr().ChangeAnimationLoopInFixedTime("Idle");
							m_Brain.SetInputFront(m_TargetVel);
							m_SaveSpeed = m_Brain.GetInfo().m_CurrentSpeed;
							m_Brain.SetInputVelocity(Vector3.zero);
						}
					}
					break;
					// バトルプレイ
					case BattleManager.Battle.SKILL_BATTLE_PLAY:{
						BattleResultUpdate();
					}
					break;
					// バトル終了
					case BattleManager.Battle.BATTLE_END:{
						if (m_Brain.GetInfo().m_HP > 0){
							m_Brain.GetState().UnLock();
							m_Brain.SetNextState(CharState.State.FLICK_MOVE, false);
							m_Brain.SetInputVelocity(m_Brain.transform.forward * m_SaveSpeed);
						}
						else{
							m_Brain.GetState().UnLock();
							m_Brain.SetNextState(CharState.State.DOWN, true);
						}
					}
					break;
				}
			}
			break;
		}
			
	}

	public void SetBattleType(BattleType type){
		m_BattleType = type;
	}
	public void SetTargetBrain(CharBrain target){
		m_TargetBrain = target;
	}
	public void SetTargetVel(Vector3 targetVel){
		m_TargetVel = targetVel;
	}

	
	// バトル結果更新 
    void BattleResultUpdate()
    {
        SkillBattlePhase phase = m_BattleManager.m_ResultPhase.current;
		if (phase == SkillBattlePhase.START || phase == SkillBattlePhase.END){
			return;
		}

		if (m_BattleManager.m_ResultPhase.IsFirst()){
			BattleResultSetAnm();
		}
    }

    // バトルリザルトセットアニメーション
    void BattleResultSetAnm()
    {
		if (m_DamageCoroutine != null){
			m_Brain.StopCoroutine(m_DamageCoroutine);
			m_DamageCoroutine = null;
		}

		SkillBattleManager skillBattleManager = m_BattleManager.GetSkillBattleManager();
		SkillBattlePhase phase = m_BattleManager.m_ResultPhase.current;
		SkillBattleManager.ResultData resultData = skillBattleManager.GetResultData(m_Brain.gameObject, phase);

		// カードを墓地へ送る
		List<SkillCardUI> cemeteryList = resultData._skillList;
		if (cemeteryList != null){
			foreach(SkillCardUI ui in cemeteryList){
				((DeckManager)m_Brain.GetState().GetSkillDataManager()).SendCemetery(ui.GetSkillData());
			}
		}

        int damage = 0;
        AnimationType anmType = resultData._anmType;
		CharBrain target = m_TargetBrain;
        switch (anmType)
        {
            // 通常攻撃
            case AnimationType.ATTACK:
                {
                    List<SkillCardUI> skillList = resultData._skillList;
					// 初撃のダメージ
					damage += skillList[0].GetSkillData()._attack;
					// 初動技ネーム
					string firstAnmName = skillList[0].GetSkillData()._anmName;
					// コンボモーション情報
                    AnmCombos combos = AnmCombDataBase.COMBS_DATA[firstAnmName];
					// バトル結果
					SkillBattleManager.RESULT result = resultData._result;
					// チェインアニメーションリスト
					List<string> chainAnm = new List<string>();

					// コンボモーションを追加
					bool firstComboAnm = true;
					foreach(string anmName in combos._combos){
						// 先頭のアニーメションネームが初動技になる
						if (firstComboAnm){
							firstComboAnm = false;
							firstAnmName = anmName;
							continue;
						}
						chainAnm.Add(anmName);
					}
					// 追撃モーション追加
					if (result == SkillBattleManager.RESULT.WIN){
						bool firstChaseAnm = true;
						foreach(SkillCardUI anmName in skillList){
							// 先頭のアニメーションは除外
							if (firstChaseAnm){
								firstChaseAnm = false;
								continue;
							}
							chainAnm.Add(anmName.GetSkillData()._anmName);
							// ダメージ追加
							damage += anmName.GetSkillData()._attack;
						}
					}
					
					// 初動アニメーション
					m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime(firstAnmName);

					// チェインアニメーション
					foreach(string anmName in chainAnm){
						m_Brain.GetState().GetAnmMgr().ChainAnimation(anmName);
					}
                }
                break;
            // 防御
            case AnimationType.GUARD:
                {
					int endFrame = 0;
					float slowEndFrame = 0;
					float slowStartFrame = 0;
					int recovery = 0;
                    // ガードの時は相手のタイミングを参照してアニメーションを再生する
					if (skillBattleManager.GetResultData(target.gameObject, phase)._skillList.Count > 0){
                    	SkillData skillData = skillBattleManager.GetResultData(target.gameObject, phase)._skillList[0].GetSkillData();
						AnmData anmData = target.GetState().GetAnmMgr().GetPlayAnmData();
						endFrame = anmData._endFrame;
						slowEndFrame = anmData._slowEndFrame;
						slowStartFrame = anmData._slowStartFrame;
						recovery = skillData._attack;
					}
					AnmData playAnm = m_Brain.GetState().GetAnmMgr().GetTransData("Guard");
                    m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime(new AnmData("Guard", 0, playAnm._durationTime, playAnm._offsetTime, endFrame, slowEndFrame, slowStartFrame));

                    m_Brain.SetDamage(recovery);
                }
                break;
            // ダメージ
            case AnimationType.DAMAGE:
                    {
                        // ダメージの時は相手のタイミングを参照してアニメーションを再生する
                        m_DamageCoroutine = m_Brain.StartCoroutine(damageAnm(target));
                    }
                    break;
            // アニメーションなし
            case AnimationType.NONE:
                {
                        m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime("Idle");
                }
                break;
        }

		target.SetDamage(-damage);
    }

    // ダメージモーション時
    IEnumerator damageAnm(CharBrain target){
        yield return null;  
		
        while(target.GetState().GetAnmMgr().GetState() == AnmState.CHANGE){
                yield return null;
            }
            // while (target.GetState().GetAnmMgr().GetPlayAnmData()._slowEndFrame > target.GetState().GetAnmMgr().GetFrame()){
            //     yield return null;
            // }
            m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime("DAMAGED00");
    }
}