using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour {

	public Image m_Image;
    public Text m_Text;
	private bool m_IsChoice = false;
	private SkillData m_Data{get;set;}
    private SkillCardUI m_NextChase = null;

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
        m_Text.text = data._name;
        m_Image.color = GetColor(data._actionType);
    }

    // アクションタイプ別カラー取得
    Color GetColor(ActionType type)
    {
        switch (type)
        {
            case ActionType.RED: return Color.red;
            case ActionType.GREEN: return Color.green;
            case ActionType.BLUE: return Color.blue;
            case ActionType.GUARD: return Color.gray;
        }

        Debug.LogError("out of range");
        return Color.black;
    }

    // 次のコンボ先を設定
    public void SetNextChase(SkillCardUI next){
        m_NextChase = next;
    }

    // 次のコンボ先を取得
    public SkillCardUI GetNextChase(){
        return m_NextChase;
    }
}
