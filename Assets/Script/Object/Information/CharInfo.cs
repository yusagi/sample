using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharInfo : BaseObjInfo {
	[Header("摩擦定数")]
	[Tooltip("最小摩擦力")] public float MIN_FRICTION;
	[Tooltip("最大摩擦力")] public float MAX_FRICTION;
	
	[Space(1)]
	[Header("ギア")]
	[Tooltip("ギア2突入条件値")] public float GEAR_SECOND_BASE;
	[Tooltip("ギア3突入条件値")] public float GEAR_THIRD_BASE;
	[Tooltip("ギア1加速度")] public float GEAR_FIRST_ACCELE;
	[Tooltip("ギア2加速度")] public float GEAR_SECOND_ACCELE;
	[Tooltip("ギア3加速度")] public float GEAR_THIRD_ACCELE;

	[Space(1)]
	[Header("速度")]
	[Tooltip("フリック移動最高速度")] public float FLICK_MAX_SPEED;
	[Tooltip("現在の進行ベクトル")] public Vector3 m_CurrentVelocity;
	[Tooltip("現在の速度")] public float m_CurrentSpeed;

	[Space(1)]
	[Header("回転")]
	[Tooltip("現在の回転")] public Quaternion m_CurrentRotate;

	[Space(1)]
	[Header("体力")]
	[Tooltip("初期体力")] public int START_HP;
	[Tooltip("残り体力")] public int m_HP;
}
