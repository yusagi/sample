﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour {

	public Image m_Image;
	private bool m_IsChoice = false;
	private SkillData m_Data{get;set;}

	void Awake(){
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // スキルデータ取得
    public SkillData GetSkillData()
    {
        return m_Data;
    }

    // スキルデータ追加
    public void AddCardData(SkillData data)
    {
        m_Data = data;
        m_Image.color = GetColor(data._type);
    }

    // カード選択
	public void Choice(){
		BattleBoardData.skillChoiceBoard.Choice(SkillChoiceBoardController.USER.PLAYER, m_Data, transform, GameManager.m_Player);
	}

    // アクションタイプ別カラー取得
    Color GetColor(ActionType type)
    {
        switch (type)
        {
            case ActionType.NORMAL_ATTACK: return Color.red;
            case ActionType.GUARD_BREAK_ATTACK: return Color.green;
            case ActionType.GUARD: return Color.blue;
        }

        Debug.LogError("out of range");
        return Color.black;
    }
}
