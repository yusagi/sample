using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillChoiceBoardController : MonoBehaviour {

	public enum DataType{
		PLAYER,
		ENEMY
	}

	List<GameObject> m_Cards = new List<GameObject>();
	List<SkillData> m_PlayerChoices = new List<SkillData>();
	List<SkillData> m_EnemyChoices = new List<SkillData>();
    Dictionary<GameObject, List<SkillData>> m_ChoiceSkills = new Dictionary<GameObject, List<SkillData>>();
	Dictionary<BattleManager.ResultPhase, AnimationType> m_PlayerResult = new Dictionary<BattleManager.ResultPhase, AnimationType>();
	Dictionary<BattleManager.ResultPhase, AnimationType> m_EnemyResult = new Dictionary<BattleManager.ResultPhase, AnimationType>();
    Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, AnimationType>> m_ResultAnms = new Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, AnimationType>>();

	public int m_PlayerAP{get;set;}
	public int m_EnemyAP{get;set;}

	void Awake(){
		m_Cards.Clear();
		m_PlayerChoices.Clear();
		m_EnemyChoices.Clear();
        m_ChoiceSkills.Clear();

        m_PlayerResult.Clear();
		m_EnemyResult.Clear();
        m_ResultAnms.Clear();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Battle(){
		m_PlayerResult.Clear();
		m_EnemyResult.Clear();
		
		int pCardNum = m_PlayerChoices.Count;
		int eCardNum = m_EnemyChoices.Count;

        int num = 4; //(pCardNum >= eCardNum) ? pCardNum : eCardNum;

		for (int i = 0; i < num; i++){
			BattleManager.ResultPhase pahse = (BattleManager.ResultPhase)(i);
			SkillData pData = (i < m_PlayerChoices.Count) ? m_PlayerChoices[i] : null;
			SkillData eData = (i < m_EnemyChoices.Count) ? m_EnemyChoices[i] : null;
			SkillBattleResult(pahse, pData, eData);

			string name1 = (pData == null) ? null : pData._name;
			string name2 = (eData == null) ? null : eData._name;

			Debug.Log(name1 + " vs " + name2);
        }

        m_ChoiceSkills.Add(GameManager.m_Player, m_PlayerChoices);
        m_ChoiceSkills.Add(GameManager.m_Enemy, m_EnemyChoices);

        m_ResultAnms.Add(GameManager.m_Player, m_PlayerResult);
        m_ResultAnms.Add(GameManager.m_Enemy, m_EnemyResult);
    }

	public void AddCardObject(GameObject card){
		int num = m_Cards.Count;
		card.transform.SetParent(transform.GetChild(num), false);
		card.GetComponentInChildren<Text>().text = card.name;
		m_Cards.Add(card);
	}

	public void AddChoice(SkillData card, DataType type){
		switch(type){
			case DataType.PLAYER:{
				m_PlayerChoices.Add(card);
			}
			break;
			case DataType.ENEMY:{
				m_EnemyChoices.Add(card);
			}
			break;
			default:{
				Debug.LogError("out of range AddChoice");
			}
			break;
		}
	}

	public void CutChoice(SkillData card){
		m_PlayerChoices.Remove(card);
	}

	public bool IsFullChoice(){
		return m_PlayerChoices.Count == BattleManager.MAX_CHOICES;
	}

    public List<SkillData> GetChoices(DataType type)
    {
        switch (type)
        {
            case DataType.PLAYER:
                {
                    return m_PlayerChoices;
                }
                break;
            case DataType.ENEMY:
                {
                    return m_EnemyChoices;
                }
                break;
            default:
                {
                    Debug.Log("out of range GetChoices");
                    return null;
                }
                break;
        }
    }

	public AnimationType GetAnimationType(GameObject my, BattleManager.ResultPhase pahse){

        return (m_ResultAnms[my])[pahse];

		//switch(type){
		//	case DataType.PLAYER:{
		//		return m_PlayerResult[pahse];
		//	}
		//	case DataType.ENEMY:{
		//		return m_EnemyResult[pahse];
		//	}
		//	default:{
		//		Debug.LogError("out of range GetAnimationTyp");
		//		return AnimationType.NONE;
		//	}
		//}
	}

	public SkillData GetSkillData(GameObject my, BattleManager.ResultPhase pahse){

        return m_ChoiceSkills[my][(int)pahse];
		//switch(type){
		//	case DataType.PLAYER:{
		//		return m_PlayerChoices[(int)pahse];
		//	}
		//	case DataType.ENEMY:{
		//		return m_EnemyChoices[(int)pahse];
		//	}
		//	default:{
		//		Debug.LogError("out of range GetSkillData");
		//		return null;
		//	}
		//}
	}

	public void End(){
		foreach(var card in m_Cards){
			GameObject.Destroy(card.gameObject);
		}
		m_Cards.Clear();
		m_PlayerChoices.Clear();
		m_EnemyChoices.Clear();
        m_ChoiceSkills.Clear();
		m_PlayerResult.Clear();
		m_EnemyResult.Clear();
        m_ResultAnms.Clear();
	}

	void SkillBattleResult(BattleManager.ResultPhase pahse, SkillData pData, SkillData eData){
		if (pData == null){
			m_PlayerResult[pahse] = AnimationType.NONE;
			if (eData != null){
				if (eData._type == ActionType.ATTACK){
					m_EnemyResult[pahse] = AnimationType.NORMAL_ATTACK;
					return;
				}
				else{
					m_EnemyResult[pahse] = AnimationType.NONE;
					return;
				}
			}
		}
		if (eData == null){
			m_EnemyResult[pahse] = AnimationType.NONE;
			if (pData != null){
				if (pData._type == ActionType.ATTACK){
					m_PlayerResult[pahse] = AnimationType.NORMAL_ATTACK;
					return;
				}
				else{
					m_PlayerResult[pahse] = AnimationType.NONE;
					return;
				}
			}
			return;
		}

		switch(pData._type){
			// プレイヤー攻撃
			case ActionType.ATTACK:{
				switch(eData._type){
					// エネミー攻撃
					case ActionType.ATTACK:{
						m_PlayerResult[pahse] = AnimationType.NORMAL_ATTACK;
						m_EnemyResult[pahse] = AnimationType.NORMAL_ATTACK;
					}
					break;
					// エネミーカウンター
					case ActionType.COUNTER:{
						m_PlayerResult[pahse] = AnimationType.NORMAL_ATTACK;
						m_EnemyResult[pahse] = AnimationType.COUNTER_ATTACK;
					}
					break;
					// エネミー防御
					case ActionType.DEFENSE:{
						m_PlayerResult[pahse] = AnimationType.NONE;
						m_EnemyResult[pahse] = AnimationType.NONE;
					}
					break;
				}
			}
			break;
			// プレイヤーカウンター
			case ActionType.COUNTER:{
				switch(eData._type){
					// エネミー攻撃
					case ActionType.ATTACK:{
						m_PlayerResult[pahse] = AnimationType.COUNTER_ATTACK;
						m_EnemyResult[pahse] = AnimationType.NORMAL_ATTACK;
					}
					break;
					// エネミーカウンター
					case ActionType.COUNTER:{
						m_PlayerResult[pahse] = AnimationType.NONE;
						m_EnemyResult[pahse] = AnimationType.NONE;
					}
					break;
					// エネミー防御
					case ActionType.DEFENSE:{
						m_PlayerResult[pahse] = AnimationType.NONE;
						m_EnemyResult[pahse] = AnimationType.NONE;
					}
					break;
				}
			}
			break;
			// プレイヤー防御
			case ActionType.DEFENSE:{
				m_PlayerResult[pahse] = AnimationType.NONE;
				m_EnemyResult[pahse] = AnimationType.NONE;
			}
			break;
		}
	}
}
