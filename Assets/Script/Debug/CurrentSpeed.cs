using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentSpeed : MonoBehaviour {
	public UnityEngine.UI.Text m_Text;
	private CharInfo m_Player;

	public void SetPlayer(CharInfo player){
		m_Player = player;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void LateUpdate()
	{
		m_Text.text = ((int)(m_Player.m_CurrentSpeed * 3.6f)).ToString() + "km";
	}
}
