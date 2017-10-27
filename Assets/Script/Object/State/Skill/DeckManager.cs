using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : SkillDataManagerBase{

	// デッキリスト
	private List<SkillData> m_DeckList = new List<SkillData>();
	// 墓地スキルデータリスト
	private List<SkillData> m_CemeteryList = new List<SkillData>();

	public DeckManager(){
	}

	// デッキ設定
	public void SetDeck(List<SkillData> userDeckData){
		m_DeckList = userDeckData;

		DeckReset();
	}

	// カード取り出し機能(常に先頭から取り出し)
	public override SkillData GetSkillData(int id = 0){
		if (!IsDataCheck(id)){
			m_SkillDataList = new List<SkillData>(m_CemeteryList);

			// デッキ、墓地ともに0だったら
			if (m_SkillDataList.Count == 0){
				return null;
			}

			m_CemeteryList.Clear();
			Shuffle();
		}
		
		SkillData data = m_SkillDataList[id];
		m_SkillDataList.RemoveAt(id);

		return data;
	}

	// カードを墓地へ送る機能
	public void SendCemetery(SkillData data){
		m_CemeteryList.Add(data);
	}

	// デッキリセット
	public void DeckReset(){
		m_SkillDataList.Clear();
		m_CemeteryList.Clear();
		m_SkillDataList = m_DeckList;
		Shuffle();
	}

	// シャッフル機能
	private void Shuffle(){
		m_SkillDataList = m_SkillDataList.OrderBy(i => Guid.NewGuid()).ToList();
	}
}