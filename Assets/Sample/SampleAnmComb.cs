using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnmComb : MonoBehaviour {

    public float m_Slow = 0.3f;

	private AnimationManager m_AnmMgr;
    enum BATTLE_FHASE
    {
        FIRST,
        SECOND,
        THIRD,
        FOURTH,
        NONE,
    }
    Phase<BATTLE_FHASE> fhase = new Phase<BATTLE_FHASE>();

	// Use this for initialization
	void Start () {
		m_AnmMgr = GetComponent<AnimationManager>();
        fhase.Change(BATTLE_FHASE.NONE);
	}

    bool slowEnd = false;
    bool slowStart = false;

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            fhase.Change(BATTLE_FHASE.FIRST);
            fhase.Start();
        }

        switch (fhase.current) {
            case BATTLE_FHASE.FIRST:
                {
                    if (fhase.IsFirst())
                    {
                        m_AnmMgr.ChangeAnimationInFixedTime("Jab", "AttackCommon");
                        Time.timeScale = m_Slow;
                    }

                    if (m_AnmMgr.GetState() == AnmState.CHANGE){
                        slowEnd = false;
                        slowStart = false;
                        break;
                    }
                    
                    if (!slowEnd && m_AnmMgr.GetAnmData()._slowEndFrame <= m_AnmMgr.GetFrame()){
                        Time.timeScale = 1.0f;
                        slowEnd = true;
                    }

                    if (!slowStart && (m_AnmMgr.IsState(AnmState.CHAINE | AnmState.END | AnmState.LOOP) || m_AnmMgr.GetAnmData()._slowStartFrame <= m_AnmMgr.GetFrame())){
                        Time.timeScale = m_Slow;
                        slowStart = true;
                    }

                    if (m_AnmMgr.IsState(AnmState.END | AnmState.LOOP))
                    {
                        slowEnd = false;
                        slowStart = false;
                        fhase.Change(BATTLE_FHASE.SECOND);
                        fhase.Start();
                        return;
                    }
                }
                break;
            case BATTLE_FHASE.SECOND:
                {
                    if (fhase.IsFirst())
                    {
                        m_AnmMgr.ChangeAnimationInFixedTime("Hikick", "AttackCommon");
                    }

                    if (m_AnmMgr.GetState() == AnmState.CHANGE){
                        break;
                    }
                    if (!slowEnd && m_AnmMgr.GetAnmData()._slowEndFrame <= m_AnmMgr.GetFrame()){
                        Time.timeScale = 1.0f;
                        slowEnd = true;
                    }

                    if (!slowStart && (m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || m_AnmMgr.GetAnmData()._slowStartFrame <= m_AnmMgr.GetFrame())){
                        Time.timeScale = m_Slow;
                        slowStart = true;
                    }

                    if (m_AnmMgr.IsState(AnmState.END | AnmState.LOOP))
                    {
                        slowEnd = false;
                        slowStart = false;
                        fhase.Change(BATTLE_FHASE.THIRD);
                        fhase.Start();
                        return;
                    }
                }
                break;
            case BATTLE_FHASE.THIRD:
                {
                    if (fhase.IsFirst())
                    {
                        m_AnmMgr.ChangeAnimationInFixedTime("Spinkick", "AttackCommon");
                    }

                    if (m_AnmMgr.GetState() == AnmState.CHANGE){
                        break;
                    }
                    if (!slowEnd && m_AnmMgr.GetAnmData()._slowEndFrame <= m_AnmMgr.GetFrame()){
                        Time.timeScale = 1.0f;
                        slowEnd = true;
                    }

                    if (!slowStart && (m_AnmMgr.IsState(AnmState.END | AnmState.LOOP | AnmState.CHAINE) || m_AnmMgr.GetAnmData()._slowStartFrame <= m_AnmMgr.GetFrame())){
                        Time.timeScale = m_Slow;
                        slowStart = true;
                    }

                    if (m_AnmMgr.IsState(AnmState.END | AnmState.LOOP))
                    {
                        slowEnd = false;
                        slowStart = false;
                        fhase.Change(BATTLE_FHASE.FOURTH);
                        fhase.Start();
                        return;
                    }
                }
                break;
            case BATTLE_FHASE.FOURTH:
                {
                    if (fhase.IsFirst())
                    {
                        m_AnmMgr.ChangeAnimationInFixedTime("Hikick");
                    }

                    if (m_AnmMgr.GetState() == AnmState.CHANGE){
                        break;
                    }
                    if (!slowEnd && m_AnmMgr.GetAnmData()._slowEndFrame <= m_AnmMgr.GetFrame()){
                        Time.timeScale = 1.0f;
                        slowEnd = true;
                    }

                    // if (!slowStart && (m_AnmMgr.IsAnmEndORLoop() || m_AnmMgr.GetAnmData()._slowStartFrame <= m_AnmMgr.GetFrame())){
                    //     Time.timeScale = 0.1f;
                    //     slowStart = true;
                    // }

                    if (m_AnmMgr.IsState(AnmState.END | AnmState.LOOP))
                    {
                        slowEnd = false;
                        slowStart = false;
                        fhase.Change(BATTLE_FHASE.NONE);
                        fhase.Start();
                        m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
                        return;
                    }
                }
                break;
        }
        if (fhase.current != BATTLE_FHASE.NONE)
        {
            fhase.Update();
        }
    }
}
