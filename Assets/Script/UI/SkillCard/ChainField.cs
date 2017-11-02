using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainField : MonoBehaviour {

	private static float SKILL_CARD_SIZE = 200;
	private static float CHAIN_SIZE = 100;

	List<GameObject> m_ChainList = new List<GameObject>();
	
	public void CreateChain(SkillCardUI start, SkillCardUI end){
		Vector3 velocity = end.transform.position - start.transform.position;
		Vector3 n_Velocity = velocity.normalized;

		float length = velocity.magnitude - SKILL_CARD_SIZE;

		int chainNUm = Mathf.CeilToInt(length / CHAIN_SIZE);

		Vector3 startPosition = start.transform.position + (velocity.normalized * (SKILL_CARD_SIZE + CHAIN_SIZE) * 0.5f);

		for(int i = 0; i < chainNUm; i++){
			GameObject chain = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/UI/SkillBattle/Chain"));		
			Vector3 position = startPosition +  (n_Velocity * i * CHAIN_SIZE);
			Quaternion rotation = Quaternion.LookRotation(Vector3.forward, velocity);
			chain.transform.SetParent(transform);
			chain.transform.position = position;
			chain.transform.rotation = rotation;
			chain.GetComponent<ChainUI>().SetSkillCardCanvasGroup(start.m_CanvasGroup);
			m_ChainList.Add(chain);
		}
	}

	public void DestroyChain(){
		foreach(GameObject chain in m_ChainList){
			GameObject.Destroy(chain);
		}
	}
}
