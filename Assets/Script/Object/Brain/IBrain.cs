using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IBrain : MonoBehaviour{
	// 初期化
	public abstract void BrainIni();
	// 更新
	public abstract void BrainUpdate();
}