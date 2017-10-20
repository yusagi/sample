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

	private float m_SaveSpeed;

	public SkillBattle(BattleManager battleManager, CharBrain brain){
		m_BattleManager = battleManager;
		m_Brain = brain;
	}

	public override void BattleIni(){
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
						m_Brain.GetState().UnLock();
						m_Brain.SetNextState(CharState.State.FLICK_MOVE, false);
						m_Brain.SetInputVelocity(m_Brain.transform.forward * m_SaveSpeed);
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
							Quaternion rot = Quaternion.LookRotation(m_TargetVel, m_Brain.transform.up);
							m_Brain.SetInputRotate(rot);
							m_SaveSpeed = m_Brain.GetInfo().m_CurrentSpeed;
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
						m_Brain.GetState().UnLock();
						m_Brain.SetNextState(CharState.State.FLICK_MOVE, false);
						m_Brain.SetInputVelocity(m_Brain.transform.forward * m_SaveSpeed);
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
        int damage = 0;
		SkillBattleManager skillBattleManager = m_BattleManager.GetSkillBattleManager();
		SkillBattlePhase phase = m_BattleManager.m_ResultPhase.current;
        AnimationType anmType = skillBattleManager.GetResultData(m_Brain.gameObject, phase)._anmType;
		CharBrain target = m_TargetBrain;
        switch (anmType)
        {
            // 通常攻撃
            case AnimationType.ATTACK:
                {
                    SkillData data = skillBattleManager.GetResultData(m_Brain.gameObject, phase)._skill;

                    AnmCombs combs = AnmCombDataBase.COMBS_DATA[data._anmName];
                    int count = combs._combs.Count;
                    if (count > 1){
                        m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime(combs._combs[0], combs._combs[1]);
                        for(int i = 1; combs._combs.Count - i > 1; i++){
                            m_Brain.GetState().GetAnmMgr().ChainAnimation(combs._combs[i], combs._combs[i + 1]);
                        }
                        m_Brain.GetState().GetAnmMgr().ChainAnimation(combs._combs[count - 1]);
                    }
                    else{
                        m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime(combs._combs[0]);
                    }

                    damage = data._attack;
                }
                break;
            // 防御
            case AnimationType.GUARD:
                {
                    // ガードの時は相手のタイミングを参照してアニメーションを再生する
                    AnmData data = m_Brain.GetState().GetAnmMgr().GetAnmData(skillBattleManager.GetResultData(target.gameObject, phase)._skill._anmName, "Common");
                    m_Brain.GetState().GetAnmMgr().ChangeAnmDataInFixedTime("Guard", -1, -1, -1, data._slowEndFrame, data._slowStartFrame);

                    m_Brain.SetHP(m_Brain.GetInfo().m_HP + skillBattleManager.GetResultData(target.gameObject, phase)._skill._attack);
                }
                break;
            // ダメージ
            case AnimationType.DAMAGE:
                    {
                        // ダメージの時は相手のタイミングを参照してアニメーションを再生する
                        m_Brain.StartCoroutine(damageAnm(target));
                    }
                    break;
            // アニメーションなし
            case AnimationType.NONE:
                {
                        m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime("Idle");
                }
                break;
        }

		target.SetHP(target.GetInfo().m_HP - damage);
    }

    // ダメージモーション時
    IEnumerator damageAnm(CharBrain target){
        yield return null;  
		
        while(target.GetState().GetAnmMgr().GetAnmList().Count > 1){
            while(target.GetState().GetAnmMgr().GetState() == AnmState.CHANGE){
                yield return null;
            }
            while (target.GetState().GetAnmMgr().GetPlayAnmData()._slowEndFrame >= target.GetState().GetAnmMgr().GetFrame()){
                yield return null;
            }
            m_Brain.GetState().GetAnmMgr().ChangeAnimationInFixedTime("DAMAGED00");
            yield return null;
        }
    }
}