using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillCardUI : MonoBehaviour {

	private Image m_Image;
	private bool m_IsChoice = false;
	public SkillData m_Data{get;set;}

	void Awake(){
		m_Image = GetComponent<Image>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void Choice(){
		m_IsChoice = !m_IsChoice;

		SkillChoiceBoardController controller = transform.parent.parent.GetComponent<SkillChoiceBoardController>();

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
            controller.CutCardObject(gameObject);
		}
		else{
			controller.m_PlayerAP += m_Data._ap;

			m_Image.color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, 1.0f);
			controller.CutChoice(m_Data, GameManager.m_Player);
		}
	}
}
