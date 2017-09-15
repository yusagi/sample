using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillChoiceBoardController : MonoBehaviour {

	public class ResultData{
		public ResultData(SkillData skill, AnimationType anmType){
			_skill = skill;
			_anmType = anmType;
		}

		public SkillData _skill;
		public AnimationType _anmType;
	}

#region スタック式バトル用変数
    List<GameObject> m_CardsUI = new List<GameObject>();      // UIオブジェクト
	
	// キャラごとの選択したカード
	Dictionary<GameObject, List<SkillData>> m_Choices = new Dictionary<GameObject, List<SkillData>>();
    // キャラクターオブジェクトごとの各フェーズのアニメーションタイプを格納
    Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, ResultData>> m_Results = new Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, ResultData>>();
#endregion

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

	public void BattleIni(GameObject player, GameObject target){
		m_Choices[player] = new List<SkillData>();
		m_Choices[target] = new List<SkillData>();

		m_Results[player] = new Dictionary<BattleManager.ResultPhase, ResultData>();
		m_Results[target] = new Dictionary<BattleManager.ResultPhase, ResultData>();
	}
	
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

	public void ChoiceReset(){
		foreach(var choices in m_Choices){
			choices.Value.Clear();
		}
	}

	public void BattleEnd(){
		foreach(var card in m_CardsUI)
        {
			GameObject.Destroy(card.gameObject);
		}
        m_CardsUI.Clear();
		m_Choices.Clear();
        m_Results.Clear();
	}

	public void AddCardObject(GameObject card){
		int num = m_CardsUI.Count;
		card.transform.SetParent(transform.GetChild(num), false);
		card.GetComponentInChildren<Text>().text = card.name;
        m_CardsUI.Add(card);
	}

	public void AddChoice(SkillData card, GameObject charactor){
		m_Choices[charactor].Add(card);
	}

	public void CutChoice(SkillData card, GameObject charactor){
		m_Choices[charactor].Remove(card);
	}

	public AnimationType GetAnimationType(GameObject charactor, BattleManager.ResultPhase phase){

        return (m_Results[charactor])[phase]._anmType;
	}

	public SkillData GetSkillData(GameObject charactor, BattleManager.ResultPhase phase){

        return (m_Results[charactor])[phase]._skill;
	}

	// プレイヤーの選んだ枚数
	public int GetChoiceCount(GameObject charactor){
		return m_Choices[charactor].Count;
	}

	void SkillBattleResult(BattleManager.ResultPhase phase, GameObject player, SkillData pData, GameObject target, SkillData tData){
		if (pData == null){
			m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
			if (tData != null){
				if (tData._type == ActionType.ATTACK){
					m_Results[target][phase] = new ResultData(tData, AnimationType.NORMAL_ATTACK);
					return;
				}
				else{
					m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
					return;
				}
			}
		}
		if (tData == null){
			m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
			if (pData != null){
				if (pData._type == ActionType.ATTACK){
					m_Results[player][phase] = new ResultData(pData, AnimationType.NORMAL_ATTACK);
					return;
				}
				else{
					m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
					return;
				}
			}
			return;
		}

		switch(pData._type){
			// プレイヤー攻撃
			case ActionType.ATTACK:{
				switch(tData._type){
					// エネミー攻撃
					case ActionType.ATTACK:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.NORMAL_ATTACK);
						m_Results[target][phase] = new ResultData(tData, AnimationType.NORMAL_ATTACK);

						
					}
					break;
					// エネミーカウンター
					case ActionType.COUNTER:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.NORMAL_ATTACK);
						m_Results[target][phase] = new ResultData(tData, AnimationType.COUNTER_ATTACK);
					}
					break;
					// エネミー防御
					case ActionType.DEFENSE:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
						m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
					}
					break;
				}
			}
			break;
			// プレイヤーカウンター
			case ActionType.COUNTER:{
				switch(tData._type){
					// エネミー攻撃
					case ActionType.ATTACK:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.COUNTER_ATTACK);
						m_Results[target][phase] = new ResultData(tData, AnimationType.NORMAL_ATTACK);
					}
					break;
					// エネミーカウンター
					case ActionType.COUNTER:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
						m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
					}
					break;
					// エネミー防御
					case ActionType.DEFENSE:{
						m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
						m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
					}
					break;
				}
			}
			break;
			// プレイヤー防御
			case ActionType.DEFENSE:{
				m_Results[player][phase] = new ResultData(pData, AnimationType.NONE);
				m_Results[target][phase] = new ResultData(tData, AnimationType.NONE);
			}
			break;
		}
	}
}
