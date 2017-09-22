using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnmComb : MonoBehaviour {

	private AnimationManager m_AnmMgr;
	private Coroutine m_AnmCor = null;
	public float slowTime = 1.0f;

	// Use this for initialization
	void Start () {
		m_AnmMgr = GetComponent<AnimationManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Z)){
            m_AnmMgr.ChangeAnimationLoopInFixedTime("Run");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
        }


        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_AnmMgr.ChangeAnimationInFixedTime("RISING_P", "AttackCommon");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_AnmMgr.ChangeAnimationInFixedTime("Jab", "AttackCommon");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_AnmMgr.ChangeAnimationInFixedTime("Hikick", "AttackCommon");
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_AnmMgr.ChangeAnimationInFixedTime("Spinkick", "AttackCommon");
        }
    }

	IEnumerator AnmComb()
    {
        Time.timeScale = slowTime;

        m_AnmMgr.ChangeAnimationInFixedTime("Jab", "Hikick");
        while (!m_AnmMgr.IsAnmEndORLoop())
        {
            yield return null;
        }

        m_AnmMgr.ChangeAnimationInFixedTime("Hikick", "Spinkick");
        while (!m_AnmMgr.IsAnmEndORLoop())
        {
            yield return null;
        }

        m_AnmMgr.ChangeAnimationInFixedTime("Spinkick", "Land");
        while (!m_AnmMgr.IsAnmEndORLoop())
        {
            yield return null;
        }

        m_AnmMgr.ChangeAnimationInFixedTime("Land", "RISING_P");
        while (!m_AnmMgr.IsAnmEndORLoop())
        {
            yield return null;
        }

        m_AnmMgr.ChangeAnimationInFixedTime("RISING_P", "DdamageDown");
        while (!m_AnmMgr.IsAnmEndORLoop())
        {
            yield return null;
        }

        m_AnmMgr.ChangeAnimationInFixedTime("DamageDown");
        while (!m_AnmMgr.IsAnmEndORLoop())
        {
            yield return null;
        }

        m_AnmMgr.ChangeAnimationLoopInFixedTime("Idle");
    }
}
