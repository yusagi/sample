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
	// 最大手札枚数
	public static int MAX_CHOICES = 4;

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

	// バトル結果のタイプ
	private enum RESULTE_TYPE{
		NORMAL,
		DOUBLE_WIN,
		DOUBLE_LOSE,
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
		public ResultData(List<SkillCardUI> skillList, AnimationType anmType, RESULT result){
			_skillList = skillList;
			_anmType = anmType;
            _result = result;
		}

		public List<SkillCardUI> _skillList = new List<SkillCardUI>();
		public AnimationType _anmType;
        public RESULT _result;
	}
	
    // UIフィールド
    public Transform m_PlayerUIField;
    public Transform m_TargetUIField;
    // UIオブジェクト
    List<SkillCardUI> m_PlayerCardsUI = new List<SkillCardUI>();
    List<SkillCardUI> m_TargetCardsUI = new List<SkillCardUI>();

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

	// プレイヤーUIフィールドのボタンを全てオフ
	public void PlayerFieldButtonDisable(){
		m_PlayerUIField.GetComponent<PlayerPointUI>().AllButtonDisable();
	}
	// プレイヤーUIフィールドのボタンを全てオン
	public void PlayerFieldButtonEnable(){
		m_PlayerUIField.GetComponent<PlayerPointUI>().AllButtonEnable();
	}

    // スキルバトル初期化処理
	public void BattleIni(GameObject player, GameObject target){
        m_Fighters[player] = new FighterData(USER.PLAYER);
        m_Fighters[target] = new FighterData(USER.TARGET);

		PlayerFieldButtonEnable();
	}

    // カードオブジェクト追加
	public void AddCardObject(GameObject fighter, int num){
		FighterData fighterData = m_Fighters[fighter];

        switch (fighterData._user)
        {
            case USER.PLAYER:
                {
					SetCardObject(fighter, m_PlayerUIField, m_PlayerCardsUI, num);
					HandAlignment(m_PlayerUIField, m_PlayerCardsUI);
					SetChase(m_PlayerCardsUI);
                }
                break;
            case USER.TARGET:
                {
					SetCardObject(fighter, m_TargetUIField, m_TargetCardsUI, num);
					HandAlignment(m_TargetUIField, m_TargetCardsUI);
					SetChase(m_TargetCardsUI);
                }
                break;
        }


	}

	// スキルカードUIの追加
	private void SetCardObject(GameObject fighter, Transform uiField, List<SkillCardUI> cardsUI, int num){
		if (num <= 0){
			return;
		}

		int currentNum = cardsUI.Count;
		// 手札制限
		if (currentNum == MAX_CHOICES){
			return;
		}
		
		DeckManager deckMgr = (DeckManager)fighter.GetComponent<CharCore>().GetBrain().GetState().GetSkillDataManager();
		SkillData card = deckMgr.GetSkillData(0);
		// デッキ0
		if (card == null){
			return;
		}
		GameObject instance = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI/SkillBattle/SkillCard"));
		instance.name = card._name;
		instance.GetComponent<SkillCardUI>().AddCardData(card);

		//instance.transform.SetParent(uiField.GetChild(currentNum), false);
		cardsUI.Add(instance.GetComponent<SkillCardUI>());

		num -= 1;
		SetCardObject(fighter, uiField, cardsUI, num);
	}

	// カードUIオブジェクトリストの先頭から手札を左詰めにする
	private void HandAlignment(Transform uiField, List<SkillCardUI> cardsUI){
		int num = 0;
		foreach(SkillCardUI cardObj in cardsUI){
			if (num >= uiField.childCount){
				return;
			}
			cardObj.transform.SetParent(uiField.GetChild(num), false);
			num++;
		}
	}

	// 手札内で追撃カードが揃ってる場合にSkillCardUIに追撃先を設定
	private void SetChase(List<SkillCardUI> cardUIs){
		// 選択された追撃カードを格納
		List<SkillCardUI> selectedChase = new List<SkillCardUI>();
		
		// 一番左のベースを検索
		foreach (SkillCardUI baseUI in cardUIs){
			// ベースカードか？
			if (baseUI.GetSkillData()._chaseType == ChaseType.BASE){
				ArtsType baseArts = baseUI.GetSkillData()._artsType;
				ChaseType next = (ChaseType)((int)ChaseType.BASE + 1);
				setChase(baseUI, cardUIs, selectedChase, next, baseArts);
			}
		}
	}
	// 追撃設定のループ処理
	private void setChase(SkillCardUI cardUI, List<SkillCardUI> cardUIs, List<SkillCardUI> selectedChase, ChaseType chaseType, ArtsType baseArts){
		// 直前に発見した追撃カード
		SkillCardUI prevChase = null;

		// 1番左端のカードから検索してく
		foreach(SkillCardUI chase in cardUIs){
			// チェイスがつながるか？
			if (chase.GetSkillData()._chaseType == chaseType){
				// アーツが一致するか？
				if (chase.GetSkillData()._artsType == baseArts){
					// すでに追撃に設定されてるか確認
					bool isSelected = false;
					foreach(SkillCardUI selected in selectedChase){
						if (selected == chase){
							isSelected = true;
							break;
						}
					}
					// 設定済みなら次の追撃を検索
					if (isSelected == true){
						prevChase = chase;
						continue;
					}
					// 選択済みに追加
					selectedChase.Add(chase);

					// 追撃に設定
					cardUI.SetNextChase(chase);

					// 次の追撃タイプを設定
					int chaseTypeNum = (int)chaseType + 1;
					ChaseType nextChase = (ChaseType)chaseTypeNum;
					// 追撃タイプに最後でないか確認
					if (nextChase != ChaseType.CHASE_END){
						setChase(chase, cardUIs, selectedChase, nextChase, baseArts);
					}
					break;
				}
			}
		}
		// 追撃が設定されてないなら
		if (cardUI.GetNextChase() == null){
			cardUI.SetNextChase(prevChase);
		}
	}

    // スキル選択
    public void AddChoice(GameObject fighter, Transform uiObj){

		FighterData fighterData = m_Fighters[fighter];
		SkillCardUI skillData = uiObj.GetComponent<SkillCardUI>();

		// apが足りなかったら不発
        int tmpAP;
		tmpAP = fighterData._ap - skillData.GetSkillData()._ap;
		if (tmpAP < 0){
			return;
        }
		// ap消費
		fighterData._ap = tmpAP;
		// カード選択リストに追加
		addChoice(fighter, skillData);
		
		// 選択したカードが基点だったらコンボ追加
		if (skillData.GetSkillData()._chaseType == ChaseType.BASE){
			SkillCardUI nextData = skillData.GetNextChase();
			while(nextData != null){
				// apが足りなかったら終了
				tmpAP = fighterData._ap - nextData.GetSkillData()._ap;
				if (tmpAP < 0){
					break;
				}
				fighterData._ap = tmpAP;
				addChoice(fighter, nextData);
				nextData = nextData.GetNextChase();
			}
		}
		
		// 選択されなかったポイントをフェードアウト
		Transform uiField = uiObj.parent.parent;
		foreach(Transform point in uiField){
			bool isFaedOut = true;
			foreach(SkillCardUI selectedCard in m_Fighters[fighter]._choices){
				if (point == selectedCard.transform.parent){
					isFaedOut = false;
					break;
				}
			}
			// スキルUIが入ってるか確認
			if (point.childCount > 0 && isFaedOut){
				SkillCardUI uiCard = point.GetComponentInChildren<SkillCardUI>();
				UIFadeOut(uiCard);
			}		
		}
    }

	private void addChoice(GameObject fighter, SkillCardUI skillData){
		// カード選択に追加
		m_Fighters[fighter]._choices.Add(skillData);
		// 削除予定に追加
		AddDestorySchedule(fighter, skillData.gameObject);
	}
	
    // スキルバトル処理
	public void Battle(GameObject player, GameObject target, SkillBattlePhase phase){
		SkillBattleResult(player, target, phase);
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

    // カードオブジェクト消去
    private void CutCardObject(GameObject fighter, GameObject uiObject)
    {
        Transform root;
        List<SkillCardUI> uis;
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
                uis = new List<SkillCardUI>();
            }
            break;
        }

        // オブジェクト削除
        uis.Remove(uiObject.GetComponent<SkillCardUI>());
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
    public List<SkillCardUI> GetCardList(GameObject fighter)
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

	// 選択したカードリスト取得
	public List<SkillCardUI> GetChoices(GameObject fighter){
		return m_Fighters[fighter]._choices;
	}

	// fighterの選んだ枚数
	public int GetChoiceCount(GameObject fighter){
		return m_Fighters[fighter]._choices.Count;
	}

	// fighterのap設定
	public void SetFighterAP(GameObject fighter, int ap){
		m_Fighters[fighter]._ap = ap;
	}

    // アクションバトルごとの結果を格納
	void SkillBattleResult(GameObject player, GameObject target, SkillBattlePhase phase){
		FighterData pFighter = m_Fighters[player];
		FighterData tFighter = m_Fighters[target];
		
		// プレイヤーと相手の初手のスキル
		SkillCardUI pFirstSkill = null;
		if (pFighter._choices.Count > 0){
			pFirstSkill = pFighter._choices[0];
		}
		SkillCardUI tFirstSkill = null;
		if (tFighter._choices.Count > 0){
			tFirstSkill = tFighter._choices[0];
		}

		// プレイヤーか相手のスキルが未選択
		if (pFirstSkill == null || tFirstSkill == null){
			// 相手データあり
			if (tFirstSkill != null){
				AnimationType pAnmType = AnimationType.NONE;
				switch (tFirstSkill.GetSkillData()._actionType)
				{
					// グー, チョキ, パー
					case ActionType.RED:
					case ActionType.GREEN:
					case ActionType.BLUE:{
						tFighter._results[phase] = new ResultData(tFighter._choices, AnimationType.ATTACK, RESULT.WIN);
						foreach(SkillCardUI ui in tFighter._choices){
							UIWin(ui);
						}

						pAnmType = AnimationType.DAMAGE;

						m_ResultWinner[phase] = target;
					}
					break;
					// ガード
					case ActionType.GUARD:{
						tFighter._results[phase] = new ResultData(tFighter._choices, AnimationType.GUARD, RESULT.WIN);
						foreach(SkillCardUI ui in tFighter._choices){
							UIWin(ui);
						}

						m_ResultWinner[phase] = target;
					}
					break;
				}

				pFighter._results[phase] = new ResultData(null, pAnmType, RESULT.LOSE);
				return;
			}
			// プレイヤーデータ有り
			if (pFirstSkill != null){
				AnimationType tAnmType = AnimationType.NONE;
				switch (pFirstSkill.GetSkillData()._actionType)
				{
					// グー, チョキ, パー, ガード
					case ActionType.RED:
					case ActionType.GREEN:
					case ActionType.BLUE:{
						pFighter._results[phase] = new ResultData(pFighter._choices, AnimationType.ATTACK, RESULT.WIN);
						foreach (SkillCardUI ui in pFighter._choices){
							UIWin(ui);
						}

						tAnmType = AnimationType.DAMAGE;

						m_ResultWinner[phase] = player;
					}
					break;
					case ActionType.GUARD:{
							pFighter._results[phase] = new ResultData(pFighter._choices, AnimationType.GUARD, RESULT.WIN);
							foreach (SkillCardUI ui in pFighter._choices){
								UIWin(ui);
							}

							m_ResultWinner[phase] = player;
					}
					break;
				}
				
				tFighter._results[phase] = new ResultData(null, tAnmType, RESULT.LOSE);
				return;
			}

			pFighter._results[phase] = new ResultData(null, AnimationType.NONE, RESULT.LOSE);
			tFighter._results[phase] = new ResultData(null, AnimationType.NONE, RESULT.LOSE);
			m_ResultWinner[phase] = player;

			return;
		}

        // 両キャラクターとも選択
		switch(pFirstSkill.GetSkillData()._actionType){
            // グー
            case ActionType.RED:{
				switch(tFirstSkill.GetSkillData()._actionType){
					// グー
					case ActionType.RED:{
						SetBattleResult(player, pFighter._choices, AnimationType.ATTACK,
										target, tFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.DOUBLE_WIN, phase);
					}
					break;
					// チョキ
					case ActionType.GREEN:{
						SetBattleResult(player, pFighter._choices, AnimationType.ATTACK,
										target, tFighter._choices, AnimationType.DAMAGE,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
					// パー
                    case ActionType.BLUE:
                    {
						SetBattleResult(target, tFighter._choices, AnimationType.ATTACK,
										player, pFighter._choices, AnimationType.DAMAGE,
										RESULTE_TYPE.NORMAL, phase);
                    }
                    break;
					// 防御
					case ActionType.GUARD:{
						SetBattleResult(target, tFighter._choices, AnimationType.GUARD,
										player, pFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
				}
			}
			break;
			// チョキ
			case ActionType.GREEN:{
				switch(tFirstSkill.GetSkillData()._actionType){
					// グー
					case ActionType.RED:{
						SetBattleResult(target, tFighter._choices, AnimationType.ATTACK,
										player, pFighter._choices, AnimationType.DAMAGE,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
					// チョキ
					case ActionType.GREEN:{
						SetBattleResult(player, pFighter._choices, AnimationType.ATTACK,
										target, tFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.DOUBLE_WIN, phase);
					}
					break;
					// パー
					case ActionType.BLUE:{
						SetBattleResult(player, pFighter._choices, AnimationType.ATTACK,
										target, tFighter._choices, AnimationType.DAMAGE,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
					// 防御
					case ActionType.GUARD:{
						SetBattleResult(target, tFighter._choices, AnimationType.GUARD,
										player, pFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
				}
			}
			break;
			// パー
			case ActionType.BLUE:{
				switch(tFirstSkill.GetSkillData()._actionType){
					// グー
					case ActionType.RED:{
						SetBattleResult(player, pFighter._choices, AnimationType.ATTACK,
										target, tFighter._choices, AnimationType.DAMAGE,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
					// チョキ
					case ActionType.GREEN:{
						SetBattleResult(target, tFighter._choices, AnimationType.ATTACK,
										player, pFighter._choices, AnimationType.DAMAGE,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
					// パー
					case ActionType.BLUE:{
						SetBattleResult(player, pFighter._choices, AnimationType.ATTACK,
										target, tFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.DOUBLE_WIN, phase);
					}
					break;
					// 防御
					case ActionType.GUARD:{
						SetBattleResult(target, tFighter._choices, AnimationType.GUARD,
										player, pFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
				}
			}
			break;
			// ガード
			case ActionType.GUARD:{
				switch(tFirstSkill.GetSkillData()._actionType){
					// グー, チョキ, パー
					case ActionType.RED:
					case ActionType.GREEN:
					case ActionType.BLUE:{
						SetBattleResult(player, pFighter._choices, AnimationType.GUARD,
										target, tFighter._choices, AnimationType.ATTACK,
										RESULTE_TYPE.NORMAL, phase);
					}
					break;
					// 防御
					case ActionType.GUARD:{
						SetBattleResult(player, pFighter._choices, AnimationType.GUARD,
										target, tFighter._choices, AnimationType.GUARD,
										RESULTE_TYPE.DOUBLE_LOSE, phase);
					}
					break;
				}
			}
			break;
		}
	}

	private void SetBattleResult(GameObject fighterObj1, List<SkillCardUI> fighter1Data, AnimationType fighter1AnimationType,  
								 GameObject fighterObj2, List<SkillCardUI> fighter2Data, AnimationType fighter2AnimationType,
								 RESULTE_TYPE resulteType, SkillBattlePhase phase){
		
		FighterData fighter1 = m_Fighters[fighterObj1];
		FighterData fighter2 = m_Fighters[fighterObj2];

		switch(resulteType){
			// 勝者と敗者がでた(fighterObj1が勝者)
			case RESULTE_TYPE.NORMAL:{
				fighter1._results[phase] = new ResultData(fighter1Data, fighter1AnimationType, RESULT.WIN);
				fighter2._results[phase] = new ResultData(fighter2Data, fighter2AnimationType, RESULT.LOSE);

				foreach(SkillCardUI ui in fighter1Data){
					UIWin(ui);
				}

				m_ResultWinner[phase] = fighterObj1;
			}
			break;
			// 引き分け
			case RESULTE_TYPE.DOUBLE_WIN:{
				fighter1._results[phase] = new ResultData(fighter1Data, fighter1AnimationType, RESULT.DRAW);
				fighter2._results[phase] = new ResultData(fighter2Data, fighter2AnimationType, RESULT.DRAW);

				foreach(SkillCardUI ui in fighter1Data){
					UIWin(ui);
				}
				foreach(SkillCardUI ui in fighter2Data){
					UIWin(ui);
				}

				m_ResultWinner[phase] = fighterObj1;
			}
			break;
			// 両方敗者
			case RESULTE_TYPE.DOUBLE_LOSE:{
				fighter1._results[phase] = new ResultData(fighter1Data, fighter1AnimationType, RESULT.LOSE);
				fighter2._results[phase] = new ResultData(fighter2Data, fighter2AnimationType, RESULT.LOSE);

				m_ResultWinner[phase] = fighterObj1;

			}
			break;
		}
	}

	// UIフェードアウト
	public void UIFadeOut(SkillCardUI card){
		card.GetComponentInChildren<Animator>().Play("FeadOut");
	}

	// UI勝利アニメーション
	public void UIWin(SkillCardUI card){
		card.GetComponentInChildren<Animator>().Play("LightStart");
	}
}