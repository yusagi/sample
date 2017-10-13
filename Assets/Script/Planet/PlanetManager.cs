using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetManager : MonoBehaviour {

	Dictionary<PlanetID, GameObject> m_Planets = new Dictionary<PlanetID, GameObject>();

	// 球体作成
	public void CreatePlanet(PlanetData data){
		// idチェック
		if (data._id < 0 && data._id >= (int)PlanetID.END){
			Debug.LogError("out of range CreatePlanet");
		}
		// 生成
        GameObject obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/" + data._name));
		obj.transform.SetParent(transform);
		obj.transform.localScale = new Vector3(data._size, data._size, data._size);
		PlanetBase pBase = obj.GetComponent<PlanetBase>();
		pBase.m_Move.SetPosition(data._position);

		m_Planets.Add((PlanetID)data._id, obj);
	}

	// 球体取得
	public GameObject GetPlanet(PlanetID id){
		// idチェック
		if ((int)id < 0 || (int)id > m_Planets.Count){
			Debug.LogError("out of range GetPlanet");
		}
		return m_Planets[id];
	}
}
