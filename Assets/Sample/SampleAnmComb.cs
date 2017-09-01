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
		if (Input.GetKey(KeyCode.Z)){
			Dbg.ClearConsole();
			if (m_AnmCor != null){
				StopCoroutine(m_AnmCor);
			}	
			m_AnmCor = StartCoroutine(AnmComb());
		}

		if (Input.GetKeyDown(KeyCode.X)){
			m_AnmMgr.ChangeAnimation("Jab", 0.0f, 0, 1);
		}
	}

	IEnumerator AnmComb(){
		Time.timeScale = 1.0f; 
			 
			 m_AnmMgr.ChangeAnimationLoop("Run", 0.2f, 0);
			 yield return null;
			 m_AnmMgr.ChangeAnimation("Jab", 0.2f, 0, 1.0f);

		
		while(m_AnmMgr.GetState() != AnmState.END){
			
			if (m_AnmMgr.GetAnimator().GetCurrentAnimatorClipInfo(0).Length > 0){
				foreach(var clip in m_AnmMgr.GetAnimator().GetCurrentAnimatorClipInfo(0)){
					Debug.Log(clip.clip.name);
				}
			}
			else{
				Debug.Log("NULL");
			}
			
			yield return null;
		}
Debug.Log("終了");
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
