using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	// 再生状態
	public enum AnmState{
        GET_NAME,
		CHANGE,
		START,
		PLAY,
		END,
		LOOP,
		NONE,
	}

public class AnimationManager : MonoBehaviour {

    private class ChainAnmData
    {
        public ChainAnmData(string name, string nextName)
        {
            _name = name;
            _nextName = nextName;
        }
        public string _name;
        public string _nextName;
    }

	// 定数
	public static float FRAME_RATE = 60.0f;

	// メンバ変数
	private Animator m_Animator;			// アニメーター
	private AnmState m_AnmState;			// アニメーション状態
	private Coroutine m_PlayAnimation;		// アニメーション状態遷移コルーチン
	private List<AnmData> m_AnmList = new List<AnmData>();		// 再生アニメーションリスト
	private float m_PrevTime;				// 前フレームのアニメーションのNormalize時間
    private System.Action<int> m_EndIntervention; // 終了時間変更の介入処理
    private Coroutine m_ChainAnimation;     // 連続アニメーションコルーチン
    private List<ChainAnmData> m_ChainAnmList = new List<ChainAnmData>(); // 連続アニメーションリスト
	
	
	void Awake(){
		m_Animator = GetComponent<Animator>();
		m_AnmState = AnmState.NONE;
		m_PlayAnimation = null;
		m_AnmList.Clear();
		m_PrevTime = 0.0f;
        m_EndIntervention = null;
        m_ChainAnmList.Clear();
        m_ChainAnimation = null;

    }

    // アニメーション変更(ループ)
    public void ChangeAnimationLoopInFixedTime(string name)
    {
        AnimationReset();
        m_PlayAnimation = StartCoroutine(setLoopAnimation(name));
    }

    // アニメーション変更(nextNameは再生時間の取得に使用)
    public void ChangeAnimationInFixedTime(string name, string nextName = null)
    {
        AnimationReset();
        m_PlayAnimation = StartCoroutine(setAnimation(name, nextName));
    }
    
    // 再生中のアニメーションのあとに続けてアニメーションを再生する(ループでないアニメーションに限る)
    public void ChainAnimation(string name, string nextName = null)
    {
        m_ChainAnmList.Add(new ChainAnmData(name, nextName));
        if (m_PlayAnimation == null)
        {
            return;
        }

        if (m_ChainAnimation == null)
        {
            m_ChainAnimation = StartCoroutine(setChainAnimation());
        }
    }

    // アニメーション変更(ループ)
    private void ChangeAnimationLoopInFixedTime(string name, int layer, float durationTime, float fixedTime)
    {
        AnmData data = new AnmData(name, layer, durationTime, 0, fixedTime);
        m_AnmList.Add(data);
        m_PlayAnimation = StartCoroutine(loopAnimation());
    }

    // アニメーション変更
    private void ChangeAnimationInFixedTime(string name, int layer, float durationTime, int endFrame, float fixedTime)
    {
        AnmData data = new AnmData(name, layer, durationTime, endFrame, fixedTime);
        m_AnmList.Add(data);
        m_PlayAnimation = StartCoroutine(playAnimation());
    }

    // 再生中のアニメーションのあとに続けてアニメーションを再生する(ループでないアニメーションに限る)
    private void ChainAnimation(string name, int layer, float durationTime, int endFrame, float fixedTime)
    {
        AnmData data = new AnmData(name, layer, durationTime, endFrame, fixedTime);
        m_AnmList.Add(data);
    }

    // ループアニメーションコルーチン
    private IEnumerator loopAnimation()
    {
        AnmData data = m_AnmList[0];

        m_Animator.CrossFadeInFixedTime(data._name, data._durationTime, data._layer, data._fixedTime);
        m_AnmState = AnmState.CHANGE;

        yield return new WaitForSeconds(data._durationTime);

        m_AnmList.Remove(data);

        m_AnmState = AnmState.LOOP;
        m_PlayAnimation = null;
    }

    // 単発アニメーションコルーチン
    private IEnumerator playAnimation()
    {
        AnmData data = m_AnmList[0];

        // 変更
        m_Animator.CrossFadeInFixedTime(data._name, data._durationTime, data._layer, data._fixedTime);
        m_AnmState = AnmState.CHANGE;
        yield return new WaitForSeconds(data._durationTime);

        // 開始
        m_AnmState = AnmState.PLAY;
        // アニメーション中
        int endFrame = data._endFrame;
        // 終了時間変更の介入処理設定
        m_EndIntervention = (end) => { endFrame = end; };
        while(GetFrame() < endFrame)
        {
            yield return null;
        }
        m_EndIntervention = null;

        // アニメーションリストから再生したアニメーションを削除
        m_AnmList.Remove(data);

        // アニメーションリストが空
        if (m_AnmList.Count == 0)
        {
            // 終了
            m_AnmState = AnmState.END;
            m_PlayAnimation = null;
        }
        // アニメーションが入ってたら続けて再生
        else
        {
            m_PlayAnimation = StartCoroutine(playAnimation());
        }

    }

    // アニメーションデータを使用したデータ設定(ループ)
    IEnumerator setLoopAnimation(string name)
    {
        // Chnage中
        m_AnmState = AnmState.GET_NAME;

        // 再生中のアニメーション名取得
        string currentName = GetName();
        while (currentName == null)
        {
            yield return null;
            currentName = GetName();
        }

        // 同名の場合は中止
        if (currentName == name)
        {
            m_AnmState = AnmState.LOOP;
            yield break;
        }

        // 再生するアニメーションデータを取得
        AnmData data = GetTransData(name, currentName);

        m_PlayAnimation = null;
        ChangeAnimationLoopInFixedTime(name, data._layer, data._durationTime, data._fixedTime);
    }

    // アニメーションデータを使用したデータ設定
    IEnumerator setAnimation(string name, string nextName)
    {
        // Chnage中
        m_AnmState = AnmState.GET_NAME;

        // 再生中のアニメーション名取得
        string currentName = GetName();
        while(currentName == null)
        {
            yield return null;
            currentName = GetName();
        }

        // 再生するアニメーションデータを取得
        AnmData data = GetTransData(name, currentName);
        int endFrame = GetTransData(name, nextName)._endFrame;

        ChangeAnimationInFixedTime(name, data._layer, data._durationTime, endFrame, data._fixedTime);
        yield return null;
    }

    // アニメーション追加コルーチン
    IEnumerator setChainAnimation()
    {
        // 状態が現在NAMEを取得中は待機
        while(m_AnmState == AnmState.GET_NAME)
        {
            yield return null;
        }

        // アニメーションリストが空なら中止
        if (m_AnmList.Count == 0)
        {
            yield break;
        }

        ChainAnmData chainData = m_ChainAnmList[0];
        string name = chainData._name;
        string nextName = chainData._nextName;

        // 再生するアニメーションデータを取得
        string currentName = m_AnmList[0]._name;
        AnmData data = GetTransData(name, currentName);
        int endFrame = GetTransData(name, nextName)._endFrame;

        ChainAnimation(name, data._layer, data._durationTime, endFrame, data._fixedTime);

        m_ChainAnmList.Remove(chainData);
        if (m_ChainAnmList.Count > 0)
        {
            m_ChainAnimation = StartCoroutine(setChainAnimation());
        }
        else
        {
            m_ChainAnimation = null;
        }
    }

    // 再生中アニメーションの終了時間を介入して変更
    public void EndIntervention(int endFrame)
    {
        if (m_EndIntervention != null)
        {
            m_EndIntervention(endFrame);
        }
    }

	// アニメーションが終了状態
	public bool IsAnmEnd(){
		return m_AnmState == AnmState.END;
	}

	// アニメーションがENDかLOOP状態
	public bool IsAnmEndORLoop(){
		return (m_AnmState == AnmState.END) || (m_AnmState == AnmState.LOOP);
    }

    // フレームから時間取得
    public float FrameToTime(float frameCount)
    {
        float time = frameCount / FRAME_RATE;
        return time;
    }

    // 時間からフレーム取得
    public int TimeToFrame(float time)
    {
        int frameCount = (int)(time * FRAME_RATE);
        return frameCount;
    }

    // アニメーション状態取得
    public AnmState GetState(){
		return m_AnmState;
	}

	// アニメーター取得
	public Animator GetAnimator(){
		return m_Animator;
	}

    // 現在フレーム取得
    public int GetFrame()
    {
        return TimeToFrame(GetTime());
    }

    // 現在時間取得
    public float GetTime()
    {
        return m_Animator.GetCurrentAnimatorStateInfo(0).length * m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    // 終了時間取得
    public float GetEndTime(string name)
    {
        RuntimeAnimatorController ac = m_Animator.runtimeAnimatorController;
        AnimationClip clip = System.Array.Find<AnimationClip>(ac.animationClips, (anmClip) => anmClip.name.Equals(name));

        if (clip != null)
        {
            return clip.length;
        }
        else
        {
            return 0.0f;
        }
    }

    // 現在再生してるアニメーション名取得
    public string GetName()
    {
        AnimatorClipInfo[] infos = m_Animator.GetCurrentAnimatorClipInfo(0);
        if (infos.Length > 0)
        {
            return infos[0].clip.name;
        }
        else
        {
            return null;
        }
    }

    // 遷移データ取得
    public AnmData GetTransData(string playKey, string transKey)
    {
        if (string.IsNullOrEmpty(playKey) || string.IsNullOrEmpty(transKey))
        {
            int endFrame = TimeToFrame(GetEndTime(playKey));
            return new AnmData("NONE", 0, 0, endFrame, 0);
        }

        if (!AnimationDataBase.TRANS_DATAS.ContainsKey(playKey) || !AnimationDataBase.TRANS_DATAS[playKey].ContainsKey(transKey))
        {
            //Debug.LogError("CONTAINS");
            //Debug.LogError("playKey " + playKey + " transKey " + transKey);
            int endFrame = TimeToFrame(GetEndTime(playKey));
            return new AnmData(playKey, 0, 0, endFrame, 0);
        }

        return AnimationDataBase.TRANS_DATAS[playKey][transKey];
    }

    // アニメーションリセット
    private void AnimationReset()
    {
        if (m_PlayAnimation != null)
        {
            StopCoroutine(m_PlayAnimation);
            m_PlayAnimation = null;
            m_AnmList.Clear();
        }
        if (m_ChainAnimation != null)
        {
            StopCoroutine(m_ChainAnimation);
            m_ChainAnimation = null;
            m_ChainAnmList.Clear();
        }
    }


    // アニメーション変更(ループ)
    //public void ChangeAnimationLoop(string name, float duration, int layer){
    //	// ループ再生してるアニメーションと同じアニメーションを再生しようとする場合
    //	if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name)){
    //		return;
    //	}

    //	if (m_PlayAnimation != null){
    //		StopCoroutine(m_PlayAnimation);
    //		m_AnmList.Clear();
    //	}

    //	m_PlayAnimation = StartCoroutine(loopAnimation(name, duration, layer));
    //}

    // アニメーション変更
    //public void ChangeAnimation(string name, float duration, int layer, float endTime){
    //	if (m_PlayAnimation != null){
    //		StopCoroutine(m_PlayAnimation);
    //		m_AnmList.Clear();
    //	}
    //	AnmData data = new AnmData(name, duration, layer, endTime);
    //	m_AnmList.Add(data);
    //	m_PlayAnimation = StartCoroutine(playAnimation());
    //}
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
    //private IEnumerator playAnimation(){
    //	// アニメーションリストの先頭を取得
    //	AnmData data = m_AnmList[0];

    //	// 前回アニメーションがまだCHANGE状態なら切り替わりを待つ
    //	if (m_AnmState == AnmState.CHANGE){

    //		//m_PrevTime = 0.0f;
    //		float tmpCurrent = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //		while(m_PrevTime < tmpCurrent){
    //			m_PrevTime = tmpCurrent;
    //			yield return null;
    //			tmpCurrent = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //		}
    //	}

    //	// アニメーション変更
    //	m_Animator.CrossFade(data._name, data._duration, data._layer, data._duration);

    //	// 変更中
    //	m_AnmState = AnmState.CHANGE;
    //	m_PrevTime = 0.0f;
    //	float current = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //	while(m_PrevTime < current){
    //		yield return null;
    //		m_PrevTime = current;
    //		current = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //	}
    //	m_PrevTime = 0.0f;

    //	// 開始
    //	m_AnmState = AnmState.START;
    //	yield return null;

    //	// アニメーション中
    //	m_AnmState = AnmState.PLAY;


    //	float currentTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //	while(currentTime < data._endTime){
    //		yield return null;
    //		currentTime = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //	}

    //	// アニメーションリストから再生したアニメーションを削除
    //	m_AnmList.Remove(data);

    //	// アニメーションリストが空
    //	if (m_AnmList.Count == 0){
    //		// 終了
    //		m_AnmState = AnmState.END;
    //		m_PlayAnimation = null;
    //	}
    //	// アニメーションが入ってたら続けて再生
    //	else{
    //		m_PlayAnimation = StartCoroutine(playAnimation());
    //	}
    //}

    //IEnumerator loopAnimation(string name, float duration, int layer){

    //	// 前回アニメーションがまだCHANGE状態なら切り替わりを待つ
    //	if (m_AnmState == AnmState.CHANGE){
    //		float tmpCurrent = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //		while(m_PrevTime < tmpCurrent){
    //			m_PrevTime = tmpCurrent;
    //			yield return null;
    //			tmpCurrent = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //		}

    //		// 切り替わったあとに同じアニメーションを再生しようとしてたら終了
    //		if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name)){
    //			m_AnmState = AnmState.LOOP;
    //			m_PlayAnimation = null;
    //			yield break;
    //		}
    //	}

    //	// 開始
    //	m_Animator.CrossFade(name, duration, layer);

    //	// 変更中
    //	m_AnmState = AnmState.CHANGE;
    //	m_PrevTime = 0.0f;
    //	float current = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //	while(m_PrevTime < current){
    //		yield return null;
    //		m_PrevTime = current;
    //		current = m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    //	}
    //	m_PrevTime = 0.0f;

    //	// ループ状態
    //	m_AnmState = AnmState.LOOP;

    //	m_PlayAnimation = null;
    //}
}