using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorChecker : MonoBehaviour {

	public Animator m_Animator{get;set;}
	public List<AnmState> m_AnmList = new List<AnmState>();
	public int m_CurrentHash{get;set;}

	public class AnmState{
		public AnmState(string name, bool start, float endTime){
			m_Name = name;
			m_IsStart = start;
			m_EndTime = endTime;
		}
		public string m_Name{get;set;}
		public bool m_IsStart{get;set;}
		public float m_EndTime{get;set;}
	}

	void Awake(){
		m_Animator = GetComponent<Animator>();
		m_AnmList.Clear();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (m_AnmList.Count >= 1){
			var anm = m_AnmList[0];

			if (!anm.m_IsStart && m_Animator.GetCurrentAnimatorStateInfo(0).IsName(anm.m_Name)){
				m_Animator.Play("Idle");
				return;
			}

			// アニメーションスタート
			if (!anm.m_IsStart){
				anm.m_IsStart = true;
				m_Animator.Play(anm.m_Name);
				return;
			}

			if (m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= anm.m_EndTime){
				m_AnmList.RemoveAt(0);
			}
		}
	}

	public void Play(string name, float endTime = 1.0f){
		m_AnmList.Add(new AnmState(name, false, endTime));
	}

	public bool IsEndAllAnm(){
		return m_AnmList.Count == 0;
	}
}
