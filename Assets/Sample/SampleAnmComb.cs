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
		Time.timeScale = slowTime;
		if (Input.GetKeyDown(KeyCode.Z)){
			if (m_AnmCor != null){
				StopCoroutine(m_AnmCor);
			}	
			m_AnmCor = StartCoroutine(AnmComb());
		}
	}

	IEnumerator AnmComb(){
        m_AnmMgr.ChangeAnimationLoop("Idle", 0, 0);
        yield return null;
        m_AnmMgr.ChangeAnimationLoop("Idle", 0, 0);
        yield return null;
        m_AnmMgr.ChangeAnimationLoop("Idle", 0, 0);
        yield return null;
        m_AnmMgr.ChangeAnimationLoop("Idle", 0, 0);
        yield return null;
        m_AnmMgr.ChangeAnimationLoop("Run", 0, 0);
        yield break;

        m_AnmMgr.ChangeAnimation("Jab", 0.068f, 0, 1);

		m_AnmMgr.ChainAnimation(new AnmData("Hikick", 0.068f, 0, 1));
		m_AnmMgr.ChainAnimation(new AnmData("Hikick", 0.068f, 0, 1));
		m_AnmMgr.ChainAnimation(new AnmData("Land", 0.068f, 0, 0.3f));
		m_AnmMgr.ChainAnimation(new AnmData("Rising_P", 0.1f, 0, 1));
		m_AnmMgr.ChainAnimation(new AnmData("Spinkick", 0.068f, 0, 1));

		while(m_AnmMgr.GetState() != AnmState.END){
			yield return null;
		}
		m_AnmMgr.ChangeAnimationLoop("Idle", 0.068f, 0);
		yield break;

		m_AnmMgr.ChangeAnimation("Hikick", 0.068f, 0, 1);
		while(m_AnmMgr.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmMgr.ChangeAnimation("Hikick", 0.068f, 0, 1);
		while(m_AnmMgr.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmMgr.ChangeAnimation("Land", 0.068f, 0, 0.3f);
		while(m_AnmMgr.GetState() != AnmState.END){
			yield return null;
		}
		m_AnmMgr.ChangeAnimation("Rising_P", 0.1f, 0, 1);
		while(m_AnmMgr.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmMgr.ChangeAnimation("Spinkick", 0.068f, 0, 1);
		while(m_AnmMgr.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmMgr.ChangeAnimationLoop("Idle", 0.068f, 0);
	}
}
