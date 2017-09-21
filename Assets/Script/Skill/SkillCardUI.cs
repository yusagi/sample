using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour {

	private Image m_Image;
	private bool m_IsChoice = false;
	private SkillData m_Data{get;set;}

	void Awake(){
		m_Image = GetComponent<Image>();
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
		m_IsChoice = !m_IsChoice;

		SkillChoiceBoardController controller = BattleBoardData.skillChoiceBoard.GetComponent<SkillChoiceBoardController>();

		if (m_IsChoice){
			int tmp = controller.m_PlayerAP - m_Data._ap;
			if (tmp < 0){
				m_IsChoice = false;
				return;
			}
			controller.m_PlayerAP = tmp;

			m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 0.5f);
			controller.AddChoice(m_Data, GameManager.m_Player);

            // カードオブジェクト消去
            controller.CutCardObject(gameObject, SkillChoiceBoardController.USER.PLAYER);
		}
		else{
			controller.m_PlayerAP += m_Data._ap;

			m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 1.0f);
			controller.CutChoice(m_Data, GameManager.m_Player);
		}
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
