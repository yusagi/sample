using System.Collections;
using System.Collections.Generic;
using UnityEngine;

	// 再生状態
	public enum AnmState{
		NONE    = 0,
		CHANGE  = 1,
		START   = 1 << 1,
		PLAY    = 1 << 2,
        CHAINE  = 1 << 3,
		END     = 1 << 4,
		LOOP    = 1 << 5,
	}

public class AnimationManager : MonoBehaviour {
    

	// 定数
	public static float FRAME_RATE = 60.0f;

	// メンバ変数
	private Animator m_Animator;			// アニメーター
	private AnmState m_AnmState;			// アニメーション状態
	private Coroutine m_PlayAnimation;		// アニメーション状態遷移コルーチン
    private List<AnmData> m_AnmList = new List<AnmData>();		// 再生アニメーションリスト
    private System.Action<int> m_EndIntervention; // 終了時間変更の介入処理
    private string m_CurrentName;           // 現在のアニメーション名
	
	void Awake(){
		m_Animator = GetComponent<Animator>();
		m_AnmState = AnmState.NONE;
		m_PlayAnimation = null;
		m_AnmList.Clear();
        m_EndIntervention = null;

        m_CurrentName = getName();
        
    }

    // アニメーション変更(ループ)
    public void ChangeAnimationLoopInFixedTime(string name)
    {
        if (m_CurrentName == name)
        {
            return;
        }
        AnimationReset();

        AnmData data = GetTransData(name, m_CurrentName);
        ChangeAnimationLoopInFixedTime(name, data._layer, data._durationTime, data._offsetTime);
        
        m_CurrentName = name;
    }

    // アニメーション変更(nextNameは再生時間の取得に使用)
    public void ChangeAnimationInFixedTime(string name, string nextName = null)
    {
        AnimationReset();

        // 再生するアニメーションデータを取得
        AnmData data = GetTransData(name, m_CurrentName);
        int endFrame = GetTransData(name, nextName)._endFrame;

        ChangeAnimationInFixedTime(name, data._layer, data._durationTime, endFrame, data._offsetTime, data._slowEndFrame, data._slowStartFrame);
    }
    
    // 再生中のアニメーションのあとに続けてアニメーションを再生する(ループでないアニメーションに限る)
    public void ChainAnimation(string name, string nextName = null)
    {
        if (m_PlayAnimation == null)
        {
            return;
        }

        // 再生するアニメーションデータを取得
        string currentName = m_AnmList[0]._name;
        AnmData data = GetTransData(name, currentName);
        int endFrame = GetTransData(name, nextName)._endFrame;

        ChainAnimation(name, data._layer, data._durationTime, endFrame, data._offsetTime, data._slowEndFrame, data._slowStartFrame);
    }

    // アニメーション変更(ループ)
    private void ChangeAnimationLoopInFixedTime(string name, int layer, float durationTime, float fixedTime)
    {
        AnmData data = new AnmData(name, layer, durationTime, 0, fixedTime);
        m_AnmList.Add(data);
        m_PlayAnimation = StartCoroutine(loopAnimation());
    }

    // アニメーション変更
    private void ChangeAnimationInFixedTime(string name, int layer, float durationTime, int endFrame, float fixedTime, float slowEndFrame, float slowStartFrame)
    {
        AnmData data = new AnmData(name, layer, durationTime, endFrame, fixedTime, slowEndFrame, slowStartFrame);
        m_AnmList.Add(data);
        m_PlayAnimation = StartCoroutine(playAnimation());
    }

    // 再生中のアニメーションのあとに続けてアニメーションを再生する(ループでないアニメーションに限る)
    private void ChainAnimation(string name, int layer, float durationTime, int endFrame, float fixedTime, float slowEndFrame, float slowStartFrame)
    {
        AnmData data = new AnmData(name, layer, durationTime, endFrame, fixedTime, slowEndFrame, slowStartFrame);
        m_AnmList.Add(data);
    }

    // ループアニメーションコルーチン
    private IEnumerator loopAnimation()
    {
        AnmData data = m_AnmList[0];

        m_Animator.CrossFadeInFixedTime(data._name, data._durationTime, data._layer, data._offsetTime);
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

        m_CurrentName = data._name;

        // 変更
        m_Animator.CrossFadeInFixedTime(data._name, data._durationTime, data._layer, data._offsetTime);
        m_AnmState = AnmState.CHANGE;

        yield return new WaitForSeconds(data._durationTime);

        // 開始
        m_AnmState = AnmState.PLAY;
        yield return null;

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
            m_AnmState = AnmState.CHAINE;
            yield return null;
            m_PlayAnimation = StartCoroutine(playAnimation());
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

    // アニメーションデータ取得
    public AnmData GetAnmData(int num = 0){
        int anmNum = m_AnmList.Count;
        if (anmNum > num){
            return m_AnmList[num];
        }
        else{
            return null;
        }
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

    // ステート診断
    public bool IsState(AnmState state)
    {
        int result = (int)(m_AnmState & state);

        return (result != 0) ? true : false;
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
    }

    // アニメーターから名前を取得
    private string getName()
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
}