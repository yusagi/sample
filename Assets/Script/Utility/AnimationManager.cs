using System.Collections;
using System.Collections.Generic;
using UnityEngine;


	// 再生状態
	public enum AnmState{
		CHANGE,
		START,
		PLAY,
		END,
		LOOP,
		NONE,
	}

	// アニメーションデータ
	public class AnmData{

		public AnmData(string name, float duration, int layer, float endTime){
			_name = name;
			_duration = duration;
			_layer = layer;
			_endTime = endTime;
		}

		public string _name;
		public float _duration;
		public int _layer;
		public float _endTime;
	}

public class AnimationManager : MonoBehaviour {
	// メンバ変数
	private Animator m_Animator;			// アニメーター
	private AnmState m_AnmState;			// アニメーション状態
	private Coroutine m_PlayAnimation;		// アニメーション状態遷移コルーチン
	private List<AnmData> m_AnmList = new List<AnmData>();		// 再生アニメーションリスト
	
	
	void Awake(){
		m_Animator = GetComponent<Animator>();
		m_AnmState = AnmState.NONE;
		m_PlayAnimation = null;
		m_AnmList.Clear();
	}

	// アニメーション変更(ループ)
	public void ChangeAnimationLoop(string name, float duration, int layer){
		// ループ再生してるアニメーションと同じアニメーションを再生しようとする場合
		if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name)){
			return;
		}
		
		if (m_PlayAnimation != null){
			StopCoroutine(m_PlayAnimation);
			m_AnmList.Clear();
		}

		m_Animator.CrossFade(name, duration, layer, duration);
		m_AnmState = AnmState.LOOP;
	}

	// アニメーション変更
	public void ChangeAnimation(string name, float duration, int layer, float endTime){
		if (m_PlayAnimation != null){
			StopCoroutine(m_PlayAnimation);
			m_AnmList.Clear();
		}
		AnmData data = new AnmData(name, duration, layer, endTime);
		m_AnmList.Add(data);
		m_PlayAnimation = StartCoroutine(playAnimation());
	}

	// 再生中のアニメーションのあとに続けてアニメーションを再生する(ループでないアニメーションに限る)
	public void ChainAnimation(AnmData data){
		if (m_PlayAnimation == null){
			return;
		}
		m_AnmList.Add(data);
	}

	// アニメーション状態を設定
	public void SetAnmState(AnmState state){
		m_AnmState = state;
	}

	// アニメーションが終了状態
	public bool IsAnmEnd(){
		return m_AnmState == AnmState.END;
	}

	// アニメーション状態取得
	public AnmState GetState(){
		return m_AnmState;
	}

	// アニメーター取得
	public Animator GetAnimator(){
		return m_Animator;
	}

    // フレームカウントを取得
    //public int[] GetFrameCount()
    //{
    //    AnimatorClipInfo[] ac = m_Animator.GetCurrentAnimatorClipInfo(0);
    //    float[] length = new float[ac.Length];
    //    for(int i = 0; i < ac.Length; i++)
    //    {
    //        length[i] = ac[i].clip.length;
    //    }

    //    ac[0].clip.
    //    float[] currentTime = new float[ac.Length];
    //    for (int i = 0; i < ac.Length; i++)
    //    {
    //        currentTime[i] = 
    //    }
    //}

	// アニメーション状態遷移コルーチン
	private IEnumerator playAnimation(){
		// アニメーションリストの先頭を取得
		AnmData data = m_AnmList[0];

		// アニメーション変更
		m_Animator.CrossFade(data._name, data._duration, data._layer, data._duration);

		// 変更中
		m_AnmState = AnmState.CHANGE;
		
		float prev = 0.0f;
		float current = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

		while(prev < current){
			yield return null;
			prev = current;
			current = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}

		// 開始
		m_AnmState = AnmState.START;
		yield return null;

		// アニメーション中
		m_AnmState = AnmState.PLAY;


		float currentTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		while(currentTime < data._endTime){
			yield return null;
			currentTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
		}

		// アニメーションリストから再生したアニメーションを削除
		m_AnmList.Remove(data);

		// アニメーションリストが空
		if (m_AnmList.Count == 0){
			// 終了
			m_AnmState = AnmState.END;
			m_PlayAnimation = null;
		}
		// アニメーションが入ってたら続けて再生
		else{
			m_PlayAnimation = StartCoroutine(playAnimation());
		}
	}
}