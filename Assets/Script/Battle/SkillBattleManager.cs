using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillBattlePhase{
	START = 0,
	FIRST,
	SECOND,
	THIRD,
	FOURTH,
	END,
}

public class SkillBattleManager : MonoBehaviour{
	// ユーザー識別用
    public enum USER
    {
		PLAYER,
		TARGET,

		NONE,
    }

    // スキル勝敗
    public enum RESULT{
        WIN,
        LOSE,
        DRAW,
    }

    // キャラクターのデータ
    public class FighterData{
        public FighterData(USER user){
            _user = user;
        }

		public void Clear(){
			_user = USER.NONE;
			_ap = 0;
			_choices.Clear();
			_results.Clear();
		}

        public USER _user;
        public int _ap;
        public List<SkillCardUI> _choices = new List<SkillCardUI>();
        public Dictionary<SkillBattlePhase, ResultData> _results = new Dictionary<SkillBattlePhase, ResultData>();
    }


    // リザルトデータクラス
	public class ResultData{
		public ResultData(SkillData skill, AnimationType anmType, RESULT result){
			_skill = skill;
			_anmType = anmType;
            _result = result;
		}

		public SkillData _skill;
		public AnimationType _anmType;
        public RESULT _result;
	}
	
	// 現在のフェーズ
	public int m_CurrentPhase;
    // UIフィールド
    public Transform m_PlayerUIField;
    public Transform m_TargetUIField;
    // UIオブジェクト
    List<GameObject> m_PlayerCardsUI = new List<GameObject>();
    List<GameObject> m_TargetCardsUI = new List<GameObject>();

    // 削除予定リスト(fighter, uiObject)
    List<KeyValuePair<GameObject, GameObject>> m_DestorySchedule = new List<KeyValuePair<GameObject, GameObject>>();
    // フェーズごとのバトルの勝者を格納(DRAWはプレイヤー優先)
    Dictionary<SkillBattlePhase, GameObject> m_ResultWinner = new Dictionary<SkillBattlePhase, GameObject>();

    // キャラオブジェごとのデータ
    Dictionary<GameObject, FighterData> m_Fighters = new Dictionary<GameObject, FighterData>();

	// プレイヤーUIフィールドにプレイヤーオブジェクト設定
	public void SetPlayerObject(GameObject player){
			m_PlayerUIField.GetComponent<PlayerPointUI>().SetPlayerObject(player);
	}

    // スキルバトル初期化処理
	public void BattleIni(GameObject player, GameObject target){
        m_Fighters[player] = new FighterData(USER.PLAYER);
        m_Fighters[target] = new FighterData(USER.TARGET);

		m_CurrentPhase = (int)SkillBattlePhase.START;
	}

    // スキル選択
    public void AddChoice(GameObject fighter, Transform uiObj){

		FighterData fighterData = m_Fighters[fighter];
		SkillCardUI skillData = uiObj.GetComponent<SkillCardUI>();
		// apが足りなかったら不dat発
        int tmp = fighterData._ap - skillData.GetSkillData()._ap;
		if (tmp < 0){
			return;
        }
		// ap消費
		fighterData._ap = tmp;
		
		// カード選択に追加
		m_Fighters[fighter]._choices.Add(skillData);
		// 削除予定に追加
		AddDestorySchedule(fighter, uiObj.gameObject);
		
		// 選択されなかったポイントを非アクティブ
		foreach(var point in uiObj.parent.parent){
			Transform tmpPoint = point as Transform;
			if (tmpPoint != uiObj.parent){
				UIController uiCtrl = tmpPoint.GetComponentInChildren<UIController>();
				if (uiCtrl != null){
					uiCtrl.ChangeAnimation("IsFeadOut", true);
				}
			}
		}
    }
	
    // スキルバトル処理
	public void Battle(GameObject player, GameObject target){
		FighterData playerData = m_Fighters[player];
		FighterData targetData = m_Fighters[target];

		int playerCount = playerData._choices.Count;
		int targetCount = targetData._choices.Count;

		// フェーズの進行
		m_CurrentPhase = Mathf.Clamp(m_CurrentPhase + 1, (int)SkillBattlePhase.FIRST, (int)SkillBattlePhase.FOURTH);
		// 選択スキル取得
		SkillCardUI pData = (playerData._choices.Count > 0) ? playerData._choices[0] : null;
		SkillCardUI tData = (targetData._choices.Count > 0) ? targetData._choices[0] : null;
		SkillBattleResult(player, pData, target, tData);

		string name1 = (pData == null) ? "NULL" : pData.GetSkillData()._name;
		string name2 = (tData == null) ? "NULL" : tData.GetSkillData()._name;

		Debug.Log(name1 + " vs " + name2);
    }

    // 選択カードリセット
	public void ChoiceReset(){
		foreach(var fighters in m_Fighters){
			fighters.Value._choices.Clear();
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
		m_Fighters.Clear();
        m_ResultWinner.Clear();
	}

    // カードオブジェクト追加
	public void AddCardObject(GameObject card, GameObject fighter){
		FighterData fighterData = m_Fighters[fighter];

        switch (fighterData._user)
        {
            case USER.PLAYER:
                {
                    int num = m_PlayerCardsUI.Count;
                    card.transform.SetParent(m_PlayerUIField.GetChild(num), false);
                    m_PlayerCardsUI.Add(card);
                }
                break;
            case USER.TARGET:
                {
                    int num = m_TargetCardsUI.Count;
                    card.transform.SetParent(m_TargetUIField.GetChild(num), false);
                    m_TargetCardsUI.Add(card);
                }
                break;
        }
	}

    // カードオブジェクト消去
    private void CutCardObject(GameObject fighter, GameObject uiObject)
    {
        Transform root;
        List<GameObject> uis;
		USER user = m_Fighters[fighter]._user;
        switch(user){
            case USER.PLAYER:{
                root = m_PlayerUIField;
                uis = m_PlayerCardsUI;
            }
            break;
            case USER.TARGET:{
                root = m_TargetUIField;
                uis = m_TargetCardsUI;
            }
            break;
            default:{
                root = null;
                uis = new List<GameObject>();
            }
            break;
        }

        // 削除オブジェクトのPOINT兄弟番号取得
        int siblinNum = uiObject.transform.parent.GetSiblingIndex();

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
        uis.Remove(uiObject);
        GameObject.Destroy(uiObject);
    }

    // スキル選択解除
	private void CutChoice(SkillCardUI card, GameObject fighter){
		m_Fighters[fighter]._choices.Remove(card);
	}

    // 削除予定追加
    private void AddDestorySchedule(GameObject fighter, GameObject uiObj){
        m_DestorySchedule.Add(new KeyValuePair<GameObject, GameObject>(fighter, uiObj));
    }

    // 削除予定執行
    public void DestoryEnforcement(){
        foreach(var obj in m_DestorySchedule){
            CutCardObject(obj.Key, obj.Value);
        }
        m_DestorySchedule.Clear();
    }

    // フェーズごとのバトルの勝者を取得(DRAWはプレイヤー優先)
    public GameObject GetWinner(SkillBattlePhase phase){
        return m_ResultWinner[phase];
    }

    // カードオブジェクトリスト取得
    public List<GameObject> GetCardList(GameObject fighter)
    {
		USER user = m_Fighters[fighter]._user;

        switch (user)
        {
            case USER.PLAYER: return m_PlayerCardsUI;
            case USER.TARGET: return m_TargetCardsUI;
        }

        Debug.LogError("out of range");
        return null;
    }

    // フェーズのリザルトを取得
    public ResultData GetResultData(GameObject fighter, SkillBattlePhase phase){
		return m_Fighters[fighter]._results[phase];
    }

	// 選択したカード取得
	public SkillCardUI GetChoice(GameObject fighter){
		return m_Fighters[fighter]._choices[0];
	}

	// fighterの選んだ枚数
	public int GetChoiceCount(GameObject fighter){
		return m_Fighters[fighter]._choices.Count;
	}

	// fighterのap設定
	public void SetFighterAP(GameObject fighter, int ap){
		m_Fighters[fighter]._ap = ap;
	}

	// UIフェードアウト
	public void UIFadeOut(SkillCardUI card){
		card.GetComponentInChildren<Animator>().Play("FeadOut");
	}


    // アクションバトルごとの結果を格納
	void SkillBattleResult(GameObject player, SkillCardUI pData, GameObject target, SkillCardUI tData){
		FighterData pFighter = m_Fighters[player];
		FighterData tFighter = m_Fighters[target];
		SkillBattlePhase phase = (SkillBattlePhase)m_CurrentPhase;
        // プレイヤー未選択
		if (pData == null){
            // アニメーションなし
			pFighter._results[(SkillBattlePhase)m_CurrentPhase] = new ResultData(pData.GetSkillData(), AnimationType.NONE, RESULT.LOSE);
            // 相手データあり
			if (tData != null){
                switch (tData.GetSkillData()._type)
                {
                    // 通常攻撃
                    case ActionType.NORMAL_ATTACK:
                        {
							tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
                            tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

							m_ResultWinner[phase] = target;
                        }
                        return;
                    // 防御
                    case ActionType.GUARD:
                        {
							tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.GUARD, RESULT.WIN);
                            tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

							m_ResultWinner[phase] = target;
                        }
                        return;
                    // 防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                        {
							tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
                            tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

							m_ResultWinner[phase] = target;
                        }
                        return;
				}
			}
		}
        // 相手未選択
		if (tData == null){
            // アニメーションなし
			tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.NONE, RESULT.LOSE);
            // プレイヤーデータ有り
			if (pData != null){
                switch (pData.GetSkillData()._type)
                {
                    // 通常攻撃
                    case ActionType.NORMAL_ATTACK:
                        {
							pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
                            pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

							m_ResultWinner[phase] = player;
                        }
                        return;
                    // 防御
                    case ActionType.GUARD:
                        {
							pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.GUARD, RESULT.WIN);
                            pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

							m_ResultWinner[phase] = player;
                        }
                        return;
                    // 防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                        {
							pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
                            pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

							m_ResultWinner[phase] = player;
                        }
                        return;
                }
			}
			return;
		}

        // 両キャラクターとも選択
		switch(pData.GetSkillData()._type){
            // プレイヤー通常攻撃
            case ActionType.NORMAL_ATTACK:{
				switch(tData.GetSkillData()._type){
					// エネミー通常攻撃
					case ActionType.NORMAL_ATTACK:{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN); 
                        pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
						
						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
                        tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						m_ResultWinner[phase] = player;
					}
					break;
					// エネミー防御
					case ActionType.GUARD:{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.LOSE);

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.GUARD, RESULT.WIN);
                        tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						m_ResultWinner[phase] = target;
					}
					break;
                    // エネミー防御破壊攻撃
                    case ActionType.GUARD_BREAK_ATTACK:
                    {
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
                        pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.DAMAGE, RESULT.LOSE);

						m_ResultWinner[phase] = player;
                    }
                    break;
				}
			}
			break;
			// プレイヤー防御
			case ActionType.GUARD:{
				switch (tData.GetSkillData()._type)
				{
					// エネミー通常攻撃
					case ActionType.NORMAL_ATTACK:
					{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.GUARD, RESULT.WIN); 
						pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
						
						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.LOSE);

						m_ResultWinner[phase] = player;
					}
					break;
					// エネミー防御
					case ActionType.GUARD:
					{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.GUARD, RESULT.LOSE);

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.GUARD, RESULT.LOSE);

						m_ResultWinner[phase] = player;
					}
					break;
					// エネミー防御破壊攻撃
					case ActionType.GUARD_BREAK_ATTACK:
					{
						tFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.DAMAGE, RESULT.LOSE); 

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
						tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						m_ResultWinner[phase] = target;
					}
					break;
				}
			}
			break;
            // プレイヤー防御破壊攻撃
            case ActionType.GUARD_BREAK_ATTACK:
			{
				switch (tData.GetSkillData()._type)
				{
					// エネミー通常攻撃
					case ActionType.NORMAL_ATTACK:
					{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.DAMAGE, RESULT.LOSE);

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
						tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						m_ResultWinner[phase] = target;
					}
					break;
					// エネミー防御
					case ActionType.GUARD:
					{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
						pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.DAMAGE, RESULT.LOSE);

						m_ResultWinner[phase] = player;
					}
					break;
					// エネミー防御破壊攻撃
					case ActionType.GUARD_BREAK_ATTACK:
					{
						pFighter._results[phase] = new ResultData(pData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
						pData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						tFighter._results[phase] = new ResultData(tData.GetSkillData(), AnimationType.ATTACK, RESULT.WIN);
						tData.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);

						m_ResultWinner[phase] = player;
					}
					break;
				}
			}
			break;
		}
	}
}