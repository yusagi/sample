using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleAnmComb : MonoBehaviour {

	private AnimationController m_AnmController;
	private Coroutine m_AnmCor = null;
	public float slowTime = 1.0f;

	// Use this for initialization
	void Start () {
		m_AnmController = GetComponent<AnimationController>();
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
		m_AnmController.ChangeAnimation("Jab", 0.068f, 0, 1);

		m_AnmController.ChainAnimation(new AnmData("Hikick", 0.068f, 0, 1));
		m_AnmController.ChainAnimation(new AnmData("Hikick", 0.068f, 0, 1));
		m_AnmController.ChainAnimation(new AnmData("Land", 0.068f, 0, 0.3f));
		m_AnmController.ChainAnimation(new AnmData("Rising_P", 0.1f, 0, 1));
		m_AnmController.ChainAnimation(new AnmData("Spinkick", 0.068f, 0, 1));

		while(m_AnmController.GetState() != AnmState.END){
			yield return null;
		}
		m_AnmController.ChangeAnimationLoop("Idle", 0.068f, 0);
		yield break;

		m_AnmController.ChangeAnimation("Hikick", 0.068f, 0, 1);
		while(m_AnmController.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmController.ChangeAnimation("Hikick", 0.068f, 0, 1);
		while(m_AnmController.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmController.ChangeAnimation("Land", 0.068f, 0, 0.3f);
		while(m_AnmController.GetState() != AnmState.END){
			yield return null;
		}
		m_AnmController.ChangeAnimation("Rising_P", 0.1f, 0, 1);
		while(m_AnmController.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmController.ChangeAnimation("Spinkick", 0.068f, 0, 1);
		while(m_AnmController.GetState() != AnmState.END){
			yield return null;
		}

		m_AnmController.ChangeAnimationLoop("Idle", 0.068f, 0);
	}
}
