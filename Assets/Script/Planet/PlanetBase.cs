using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlanetBase : MonoBehaviour {
	public PlanetID m_ID{get;set;}
	// 移動コンポーネント
	public PlanetMove m_Move;
}
