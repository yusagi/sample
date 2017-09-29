using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillChoiceBoardController : MonoBehaviour {

    // ユーザー識別用
    public enum USER
    {
        PLAYER,
        TARGET,
    }


    // リザルトデータクラス
	public class ResultData{
		public ResultData(SkillData skill, AnimationType anmType){
			_skill = skill;
			_anmType = anmType;
		}

		public SkillData _skill;
		public AnimationType _anmType;
	}

    // ユーザーオブジェクト
    public Transform m_Player;
    public Transform m_Target;

    // UIオブジェクト
    List<GameObject> m_PlayerCardsUI = new List<GameObject>();
    List<GameObject> m_TargetCardsUI = new List<GameObject>();

	// キャラごとの選択したカード
	Dictionary<GameObject, List<SkillData>> m_Choices = new Dictionary<GameObject, List<SkillData>>();
    // キャラクターオブジェクトごとの各フェーズのアニメーションタイプを格納
    Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, ResultData>> m_Results = new Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, ResultData>>();

    public int m_PlayerAP{get;set;}
	public int m_EnemyAP{get;set;}

	void Awake(){
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // スキルバトル初期化処理
	public void BattleIni(GameObject player, GameObject target){
		m_Choices[player] = new List<SkillData>();
		m_Choices[target] = new List<SkillData>();

		m_Results[player] = new Dictionary<BattleManager.ResultPhase, ResultData>();
		m_Results[target] = new Dictionary<BattleManager.ResultPhase, ResultData>();
	}
	
    // スキルバトル処理
	public void Battle(GameObject player, GameObject target){
		int playerCount = m_Choices[player].Count;
		int targetCount = m_Choices[target].Count;

			BattleManager.ResultPhase phase = (BattleManager.ResultPhase)(m_Results[player].Count);
			SkillData pData = (m_Choices[player].Count > 0) ? m_Choices[player][0] : null;
			SkillData tData = (m_Choices[target].Count > 0) ? m_Choices[target][0] : null;
			SkillBattleResult(phase, player, pData, target, tData);

			string name1 = (pData == null) ? "NULL" : pData._name;
			string name2 = (tData == null) ? "NULL" : tData._name;

			Debug.Log(name1 + " vs " + name2);
    }

    // 選択カードリセット
	public void ChoiceReset(){
		foreach(var choices in m_Choices){
			choices.Value.Clear();
		}
	}

    // スキルバトル終了処理
	public void BattleEnd(){
		foreach(var card in m_PlayerCardsUI)
        {
			GameObject.Destroy(card.gameObject);
		}
        foreach(var card in m_TargetCardsUI)
        {
            GameObject.Destroy(card.gameObject);
        }
        m_PlayerCardsUI.Clear();
        m_TargetCardsUI.Clear();
		m_Choices.Clear();
        m_Results.Clear();
	}

    // カードオブジェクト追加
	public void AddCardObject(GameObject card, USER user){
        switch (user)
        {
            case USER.PLAYER:
                {
                    int num = m_PlayerCardsUI.Count;
                    card.transform.SetParent(m_Player.GetChild(num), false);
                    card.GetComponentInChildren<Text>().text = card.name;
                    m_PlayerCardsUI.Add(card);
                }
                break;
            case USER.TARGET:
                {
                    int num = m_TargetCardsUI.Count;
                    card.transform.SetParent(m_Target.GetChild(num), false);
                    card.GetComponentInChildren<Text>().text = card.name;
                    m_TargetCardsUI.Add(card);
                }
                break;
        }
	}

    // カードオブジェクト消去
    public void CutCardObject(GameObject card, USER user)
    {
        switch (user)
        {
            case USER.PLAYER:
                {
                    // 削除オブジェクトのPOINT兄弟番号取得
                    int siblinNum = card.transform.parent.GetSiblingIndex();

                    // ルート
                    Transform root = m_Player;

                    // カードの親子関係の変更
                    foreach (Transform siblin in root)
                    {
                        if (siblin.GetSiblingIndex() > siblinNum)
                        {
                            // 子を取り出す
                            if (siblin.childCount > 0)
                            {
                                Transform tmpCard = siblin.GetChild(0);

                                tmpCard.SetParent(root.GetChild(siblin.GetSiblingIndex() - 1), false);
                            }
                        }
                    }

                    // オブジェクト削除
                    m_PlayerCardsUI.Remove(card);
                    GameObject.Destroy(card);
                }
                break;
            case USER.TARGET:
                {
                    // 削除オブジェクトのPOINT兄弟番号取得
                    int siblinNum = card.transform.parent.GetSiblingIndex();

                    // ルート
                    Transform root = m_Target;

                    // カードの親子関係の変更
                    foreach (Transform siblin in root)
                    {
                        if (siblin.GetSiblingIndex() > siblinNum)
                        {
                            // 子を取り出す
                            if (siblin.childCount > 0)
                            {
                                Transform tmpCard = siblin.GetChild(0);

                                tmpCard.SetParent(root.GetChild(siblin.GetSiblingIndex() - 1), false);
                            }
                        }
                    }

                    // オブジェクト削除
                    m_TargetCardsUI.Remove(card);
                    GameObject.Destroy(card);
                }
                break;
        }
    }

    // スキル選択
	public void AddChoice(SkillData card, GameObject charactor){
		m_Choices[charactor].Add(card);
	}

    // スキル選択解除
	public void CutChoice(SkillData card, GameObject charactor){
		m_Choices[charactor].Remove(card);
	}

    // カードオブジェクトリスト取得
    public List<GameObject> GetCardList(USER user)
    {
        switch (user)
        {
            case USER.PLAYER: return m_PlayerCardsUI;
            case USER.TARGET: return m_TargetCardsUI;
        }

        Debug.LogError("out of range");
        return null;
    }

    // アニメーションタイプを取得
	public AnimationType GetAnimationType(GameObject charactor, BattleManager.ResultPhase phase){

        return (m_Results[charactor])[phase]._anmType;
	}

    // スキルデータ取得
	public SkillData GetSkillData(GameObject charactor, BattleManager.ResultPhase phase){

        return (m_Results[charactor])[phase]._skill;
	}

    // リザルトデータ取得
    public bool IsResult(GameObject charactor, BattleManager.ResultPhase phase)
    {
        return m_Results[charactor].ContainsKey(phase);
    }

	// プレイヤーの選んだ枚数
	public int GetChoiceCount(GameObject charactor){
		return m_Choices[charactor].Count;
	}

    // アクションバトルごとの結果を格納
	void SkillBattleResult(BattleManager.ResultPhase phase, GameObject player, SkillData pData, GameObject target, SkillData tData){
        // プレイヤー未選択
		if (pData == null){
            // アニメーションなし
			m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
            // 相手データあり
			if (tData != null){
                switch (tData._type)
                {
                    // 通常攻撃
                    case ActionType.NORMAL_ATTACK:
                        {
                            m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                        }
                        return;
                    // カウンター攻撃
                    case ActionType.COUNTER_ATTACK:
                        {
                            m_Results[target][phase] = new ResultData(tData, AnimationType.COUNTER_MATCH);
                        }
                        return;
                    // 防御
                    case ActionType.GUARD:
                        {
                            m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD);
                        }
                        return;
                    // 防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                        {
                            m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                        }
                        return;
				}
			}
		}
        // 相手未選択
		if (tData == null){
            // アニメーションなし
			m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
            // プレイヤーデータ有り
			if (pData != null){
                switch (pData._type)
                {
                    // 通常攻撃
                    case ActionType.NORMAL_ATTACK:
                        {
                            m_Results[target][phase] = new ResultData(pData, AnimationType.ATTACK);
                        }
                        return;
                    // カウンター攻撃
                    case ActionType.COUNTER_ATTACK:
                        {
                            m_Results[target][phase] = new ResultData(pData, AnimationType.COUNTER_MATCH);
                        }
                        return;
                    // 防御
                    case ActionType.GUARD:
                        {
                            m_Results[target][phase] = new ResultData(pData, AnimationType.GUARD);
                        }
                        return;
                    // 防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                        {
                            m_Results[target][phase] = new ResultData(pData, AnimationType.ATTACK);
                        }
                        return;
                }
			}
			return;
		}

        // 両キャラクターとも選択
		switch(pData._type){
            // プレイヤー通常攻撃
            case ActionType.NORMAL_ATTACK:{
				switch(tData._type){
					// エネミー通常攻撃
					case ActionType.NORMAL_ATTACK:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
						m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
					}
					break;
					// エネミーカウンター攻撃
					case ActionType.COUNTER_ATTACK:{
                        m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
						m_Results[target][phase] = new ResultData(tData, AnimationType.COUNTER_ATTACK);
					}
					break;
					// エネミー防御
					case ActionType.GUARD:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK_REPELLED);
						m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD);
					}
					break;
                    // エネミー防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                    {
                        m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
                        //m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD_BREAK_ATTACK_REPELLED);
                        m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                    }
                    break;
				}
			}
			break;
			// プレイヤーカウンター攻撃
			case ActionType.COUNTER_ATTACK:{
				switch(tData._type){
					// エネミー通常攻撃
					case ActionType.NORMAL_ATTACK:{
                        m_Results[player][phase] = new ResultData(pData, AnimationType.COUNTER_ATTACK);
						m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
					}
					break;
					// エネミーカウンター攻撃
					case ActionType.COUNTER_ATTACK:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.COUNTER_MATCH);
						m_Results[target][phase] = new ResultData(tData, AnimationType.COUNTER_MATCH);
					}
					break;
					// エネミー防御
					case ActionType.GUARD:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.COUNTER_MATCH);
						m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD);
					}
					break;
                    // エネミー防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                    {
                        m_Results[player][phase] = new ResultData(pData, AnimationType.COUNTER_ATTACK);
                        m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                    }
                    break;
				}
			}
			break;
			// プレイヤー防御
			case ActionType.GUARD:{
                    switch (tData._type)
                    {
                        // エネミー通常攻撃
                        case ActionType.NORMAL_ATTACK:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK_REPELLED);
                            }
                            break;
                        // エネミーカウンター攻撃
                        case ActionType.COUNTER_ATTACK:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.COUNTER_MATCH);
                            }
                            break;
                        // エネミー防御
                        case ActionType.GUARD:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD);
                            }
                            break;
                        // エネミー防御破壊攻撃
                        case ActionType.GUARD_BREAK_ATTACK:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD_BREAK);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                            }
                            break;
                    }
			}
			break;
            // プレイヤー防御破壊攻撃
            case ActionType.GUARD_BREAK_ATTACK:
                {
                    switch (tData._type)
                    {
                        // エネミー通常攻撃
                        case ActionType.NORMAL_ATTACK:
                            {
                                //m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD_BREAK_ATTACK_REPELLED);
                                m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                            }
                            break;
                        // エネミーカウンター攻撃
                        case ActionType.COUNTER_ATTACK:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.COUNTER_ATTACK);
                            }
                            break;
                        // エネミー防御
                        case ActionType.GUARD:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD_BREAK);
                            }
                            break;
                        // エネミー防御破壊攻撃
                        case ActionType.GUARD_BREAK_ATTACK:
                            {
                                m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK);
                                m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK);
                            }
                            break;
                    }
                }
                break;
		}
	}
}
