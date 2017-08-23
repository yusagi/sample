using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSpeedWatch : MonoBehaviour {

    private GameObject m_Player;
    private Text m_CurrentSpeed;
    private bool m_IsStart;

	// Use this for initialization
	IEnumerator Start () {
        m_Player = null;
        m_CurrentSpeed = GetComponent<Text>();
        m_IsStart = false;

        while(m_Player == null)
        {
            m_Player = GameObject.Find("Player");
            yield return null;
        }

        m_IsStart = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (!m_IsStart)
        {
            return;
        }
        m_CurrentSpeed.text = (int)(m_Player.GetComponent<PlayerController>().rigidbody.GetSpeed() * 3.6f) + "km";
	}
}
