// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class SkillChoiceBoardController : MonoBehaviour {

//     // ユーザー識別用
//     public enum USER
//     {
//         PLAYER,
//         TARGET,
//     }

//     // スキル勝敗
//     public enum RESULT{
//         WIN,
//         LOSE,
//         DRAW,
//     }


//     // リザルトデータクラス
// 	public class ResultData{
// 		public ResultData(SkillData skill, AnimationType anmType, RESULT result){
// 			_skill = skill;
// 			_anmType = anmType;
//             _result = result;
// 		}

// 		public SkillData _skill;
// 		public AnimationType _anmType;
//         public RESULT _result;
// 	}

//     // ユーザーオブジェクト
//     public Transform m_Player;
//     public Transform m_Target;

//     // UIオブジェクト
//     List<GameObject> m_PlayerCardsUI = new List<GameObject>();
//     List<GameObject> m_TargetCardsUI = new List<GameObject>();

//     // 削除予定リスト
//     List<KeyValuePair<USER, GameObject>> m_DestorySchedule = new List<KeyValuePair<USER, GameObject>>();

// 	// キャラごとの選択したカード
// 	Dictionary<GameObject, List<SkillData>> m_Choices = new Dictionary<GameObject, List<SkillData>>();
//     // キャラクターオブジェクトごとの各フェーズのアニメーションタイプを格納
//     Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, ResultData>> m_Results = new Dictionary<GameObject, Dictionary<BattleManager.ResultPhase, ResultData>>();
//     // フェーズごとのバトルの勝者を格納(DRAWはプレイヤー優先)
//     Dictionary<BattleManager.ResultPhase, GameObject> m_ResultWinner = new Dictionary<BattleManager.ResultPhase, GameObject>();

//     public int m_PlayerAP{get;set;}
// 	public int m_TargetAP{get;set;}

// 	void Awake(){
// 	}

// 	// Use this for initialization
// 	void Start () {
		
// 	}
	
// 	// Update is called once per frame
// 	void Update () {
		
// 	}

//     // スキルバトル初期化処理
// 	public void BattleIni(GameObject player, GameObject target){
// 		m_Choices[player] = new List<SkillData>();
// 		m_Choices[target] = new List<SkillData>();

// 		m_Results[player] = new Dictionary<BattleManager.ResultPhase, ResultData>();
// 		m_Results[target] = new Dictionary<BattleManager.ResultPhase, ResultData>();
// 	}

//     // スキル選択
//     public void Choice(USER user, SkillData data, Transform uiObj, GameObject useObj){
//         int myAP = 0;

//         // ユーザー別でapを設定
//         switch(user){
//             case USER.PLAYER: myAP = m_PlayerAP; break;
//             case USER.TARGET: myAP = m_TargetAP; break;
//         }


//         int tmp = myAP - data._ap;
// 		if (tmp < 0){
// 			return;
//         }
//         // ユーザー別でapを消費
//         switch(user){
//             case USER.PLAYER: m_PlayerAP = tmp; break;
//             case USER.TARGET: m_TargetAP = tmp; break;
//         }
        
// 		// スキルデータに自身を登録
//         data._object = uiObj;
// 		// カード選択に追加
// 		AddChoice(data, useObj);
// 		// 削除予定に追加
// 		AddDestorySchedule(user, uiObj.gameObject);
// 		// ボタンの機能を非アクティブに
// 		uiObj.GetComponentInChildren<UnityEngine.UI.Button>().enabled = false;

// 		// 選択されなかったポイントを非アクティブ
// 		foreach(var point in uiObj.parent.parent){
// 			Transform tmpPoint = point as Transform;
// 			if (tmpPoint != uiObj.parent){
// 				UIController uiCtrl = tmpPoint.GetComponentInChildren<UIController>();
// 				if (uiCtrl != null){
// 					uiCtrl.ChangeAnimation("IsFeadOut", true);
// 				}
// 			}
// 		}
//     }
	
//     // スキルバトル処理
// 	public void Battle(GameObject player, GameObject target){
// 		int playerCount = m_Choices[player].Count;
// 		int targetCount = m_Choices[target].Count;

//             // 現在のフェーズを取得
// 			BattleManager.ResultPhase phase = (BattleManager.ResultPhase)(m_Results[player].Count);
// 			SkillData pData = (m_Choices[player].Count > 0) ? m_Choices[player][0] : null;
// 			SkillData tData = (m_Choices[target].Count > 0) ? m_Choices[target][0] : null;
// 			SkillBattleResult(phase, player, pData, target, tData);

// 			string name1 = (pData == null) ? "NULL" : pData._name;
// 			string name2 = (tData == null) ? "NULL" : tData._name;

// 			Debug.Log(name1 + " vs " + name2);
//     }

//     // 選択カードリセット
// 	public void ChoiceReset(){
// 		foreach(var choices in m_Choices){
// 			choices.Value.Clear();
// 		}
// 	}

//     // スキルバトル終了処理
// 	public void BattleEnd(){
// 		foreach(var card in m_PlayerCardsUI)
//         {
// 			GameObject.Destroy(card.gameObject);
// 		}
//         foreach(var card in m_TargetCardsUI)
//         {
//             GameObject.Destroy(card.gameObject);
//         }
//         m_PlayerCardsUI.Clear();
//         m_TargetCardsUI.Clear();
// 		m_Choices.Clear();
//         m_Results.Clear();
//         m_ResultWinner.Clear();
// 	}

//     // カードオブジェクト追加
// 	public void AddCardObject(GameObject card, USER user){
//         switch (user)
//         {
//             case USER.PLAYER:
//                 {
//                     int num = m_PlayerCardsUI.Count;
//                     card.transform.SetParent(m_Player.GetChild(num), false);
//                     card.GetComponentInChildren<Text>().text = card.name;
//                     m_PlayerCardsUI.Add(card);
//                 }
//                 break;
//             case USER.TARGET:
//                 {
//                     int num = m_TargetCardsUI.Count;
//                     card.transform.SetParent(m_Target.GetChild(num), false);
//                     card.GetComponentInChildren<Text>().text = card.name;
//                     m_TargetCardsUI.Add(card);
//                 }
//                 break;
//         }
// 	}

//     // カードオブジェクト消去
//     private void CutCardObject(GameObject card, USER user)
//     {
//         Transform root;
//         List<GameObject> uis;
//         switch(user){
//             case USER.PLAYER:{
//                 root = m_Player;
//                 uis = m_PlayerCardsUI;
//             }
//             break;
//             case USER.TARGET:{
//                 root = m_Target;
//                 uis = m_TargetCardsUI;
//             }
//             break;
//             default:{
//                 root = null;
//                 uis = new List<GameObject>();
//             }
//             break;
//         }

//         // 削除オブジェクトのPOINT兄弟番号取得
//         int siblinNum = card.transform.parent.GetSiblingIndex();

//         // カードの親子関係の変更
//         foreach (Transform siblin in root)
//         {
//             if (siblin.GetSiblingIndex() > siblinNum)
//             {
//                 // 子を取り出す
//                 if (siblin.childCount > 0)
//                 {
//                     Transform tmpCard = siblin.GetChild(0);

//                     tmpCard.SetParent(root.GetChild(siblin.GetSiblingIndex() - 1), false);
//                 }
//             }
//         }

//         // オブジェクト削除
//         uis.Remove(card);
//         GameObject.Destroy(card);
//     }

//     // スキル選択
// 	private void AddChoice(SkillData card, GameObject charactor){
// 		m_Choices[charactor].Add(card);
// 	}

//     // スキル選択解除
// 	private void CutChoice(SkillData card, GameObject charactor){
// 		m_Choices[charactor].Remove(card);
// 	}

//     // 削除予定追加
//     private void AddDestorySchedule(USER user, GameObject uiObj){
//         m_DestorySchedule.Add(new KeyValuePair<USER, GameObject>(user, uiObj));
//     }

//     // 削除予定執行
//     public void DestoryEnforcement(){
//         foreach(var obj in m_DestorySchedule){
//             CutCardObject(obj.Value, obj.Key);
//         }
//         m_DestorySchedule.Clear();
//     }

//     // フェーズごとのバトルの勝者を取得(DRAWはプレイヤー優先)
//     public GameObject GetWinner(BattleManager.ResultPhase phase){
//         return m_ResultWinner[phase];
//     }

//     // カードオブジェクトリスト取得
//     public List<GameObject> GetCardList(USER user)
//     {
//         switch (user)
//         {
//             case USER.PLAYER: return m_PlayerCardsUI;
//             case USER.TARGET: return m_TargetCardsUI;
//         }

//         Debug.LogError("out of range");
//         return null;
//     }

//     // フェーズのリザルトを取得
//     public ResultData GetResultData(GameObject charactor, BattleManager.ResultPhase phase){
//         return m_Results[charactor][phase];
//     }

// 	// プレイヤーの選んだ枚数
// 	public int GetChoiceCount(GameObject charactor){
// 		return m_Choices[charactor].Count;
// 	}

//     // アクションバトルごとの結果を格納
// 	void SkillBattleResult(BattleManager.ResultPhase phase, GameObject player, SkillData pData, GameObject target, SkillData tData){
//         // プレイヤー未選択
// 		if (pData == null){
//             // アニメーションなし
// 			m_Results[player][phase] = new ResultData(pData, AnimationType.NONE, RESULT.LOSE);
//             // 相手データあり
// 			if (tData != null){
//                 switch (tData._type)
//                 {
//                     // 通常攻撃
//                     case ActionType.NORMAL_ATTACK:
//                         {
//                             m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.WIN);
//                             m_ResultWinner[phase] = target;
//                             tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         }
//                         return;
//                     // 防御
//                     case ActionType.GUARD:
//                         {
//                             m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD, RESULT.WIN);
//                             m_ResultWinner[phase] = target;
//                             tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         }
//                         return;
//                     // 防御破壊攻撃
//                     case ActionType.GUARD_BREAK_ATTACK:
//                         {
//                             m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.WIN);
//                             m_ResultWinner[phase] = target;
//                             tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         }
//                         return;
// 				}
// 			}
// 		}
//         // 相手未選択
// 		if (tData == null){
//             // アニメーションなし
// 			m_Results[target][phase] = new ResultData(tData, AnimationType.NONE, RESULT.LOSE);
//             // プレイヤーデータ有り
// 			if (pData != null){
//                 switch (pData._type)
//                 {
//                     // 通常攻撃
//                     case ActionType.NORMAL_ATTACK:
//                         {
//                             m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.WIN);
//                             m_ResultWinner[phase] = player;
//                             pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         }
//                         return;
//                     // 防御
//                     case ActionType.GUARD:
//                         {
//                             m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD, RESULT.WIN);
//                             m_ResultWinner[phase] = player;
//                             pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         }
//                         return;
//                     // 防御破壊攻撃
//                     case ActionType.GUARD_BREAK_ATTACK:
//                         {
//                             m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.WIN);
//                             m_ResultWinner[phase] = player;
//                             pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         }
//                         return;
//                 }
// 			}
// 			return;
// 		}

//         // 両キャラクターとも選択
// 		switch(pData._type){
//             // プレイヤー通常攻撃
//             case ActionType.NORMAL_ATTACK:{
// 				switch(tData._type){
// 					// エネミー通常攻撃
// 					case ActionType.NORMAL_ATTACK:{
// 						m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.WIN);
//                         pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
// 						m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.WIN);
//                         tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         m_ResultWinner[phase] = player;
// 					}
// 					break;
// 					// エネミー防御
// 					case ActionType.GUARD:{
// 						m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.LOSE);
// 						m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD, RESULT.WIN);
//                         tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         m_ResultWinner[phase] = target;
// 					}
// 					break;
//                     // エネミー防御破壊攻撃
//                     case ActionType.GUARD_BREAK_ATTACK:
//                     {
//                         m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.WIN);
//                         pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                         m_Results[target][phase] = new ResultData(tData, AnimationType.DAMAGE, RESULT.LOSE);
//                         m_ResultWinner[phase] = player;
//                     }
//                     break;
// 				}
// 			}
// 			break;
// 			// プレイヤー防御
// 			case ActionType.GUARD:{
//                     switch (tData._type)
//                     {
//                         // エネミー通常攻撃
//                         case ActionType.NORMAL_ATTACK:
//                             {
//                                 m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD, RESULT.WIN);
//                                 pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                                 m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.LOSE);
//                                 m_ResultWinner[phase] = player;
//                             }
//                             break;
//                         // エネミー防御
//                         case ActionType.GUARD:
//                             {
//                                 m_Results[player][phase] = new ResultData(pData, AnimationType.GUARD, RESULT.LOSE);
//                                 m_Results[target][phase] = new ResultData(tData, AnimationType.GUARD, RESULT.LOSE);
//                                 m_ResultWinner[phase] = player;
//                             }
//                             break;
//                         // エネミー防御破壊攻撃
//                         case ActionType.GUARD_BREAK_ATTACK:
//                             {
//                                 m_Results[player][phase] = new ResultData(pData, AnimationType.DAMAGE, RESULT.LOSE);
//                                 m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.WIN);
//                                 tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                                 m_ResultWinner[phase] = target;
//                             }
//                             break;
//                     }
// 			}
// 			break;
//             // プレイヤー防御破壊攻撃
//             case ActionType.GUARD_BREAK_ATTACK:
//                 {
//                     switch (tData._type)
//                     {
//                         // エネミー通常攻撃
//                         case ActionType.NORMAL_ATTACK:
//                             {
//                                 m_Results[player][phase] = new ResultData(pData, AnimationType.DAMAGE, RESULT.LOSE);
//                                 m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.WIN);
//                                 tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                                 m_ResultWinner[phase] = target;
//                             }
//                             break;
//                         // エネミー防御
//                         case ActionType.GUARD:
//                             {
//                                 m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.WIN);
//                                 pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                                 m_Results[target][phase] = new ResultData(tData, AnimationType.DAMAGE, RESULT.LOSE);
//                                 m_ResultWinner[phase] = player;
//                             }
//                             break;
//                         // エネミー防御破壊攻撃
//                         case ActionType.GUARD_BREAK_ATTACK:
//                             {
//                                 m_Results[player][phase] = new ResultData(pData, AnimationType.ATTACK, RESULT.WIN);
//                                 pData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                                 m_Results[target][phase] = new ResultData(tData, AnimationType.ATTACK, RESULT.WIN);
//                                 tData._object.GetComponentInChildren<UIController>().ChangeAnimation("IsWin", true);
//                                 m_ResultWinner[phase] = player;
//                             }
//                             break;
//                     }
//                 }
//                 break;
// 		}
// 	}
// }
