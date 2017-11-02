using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChainUI : MonoBehaviour {

	private CanvasGroup m_SkillCardCanvasGroup = null;
	[SerializeField] private Image m_Image;

	public void SetSkillCardCanvasGroup(CanvasGroup canvasGroup){
		m_SkillCardCanvasGroup = canvasGroup;
	} 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (m_SkillCardCanvasGroup != null){
			Color color = new Color(m_Image.color.r, m_Image.color.g, m_Image.color.b, m_SkillCardCanvasGroup.alpha);

			m_Image.color = color;
		}
	}
}
