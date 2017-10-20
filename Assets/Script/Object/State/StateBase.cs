using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateBase : MonoBehaviour {

	// 初期化
	public abstract void StateIni();
	// 更新
	public abstract void StateUpdate();
}
