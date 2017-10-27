using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharCore))]
[RequireComponent(typeof(CharState))]
[RequireComponent(typeof(CharInfo))]
public class CharBrain : BaseBrain {

	protected Vector3 m_InputVelocity;
	protected float m_InputSpeed;
	protected Vector3 m_InputFront;
	protected int m_Damage;
	protected Transform m_LookBase;
	protected CharState.State m_NextState;

	[SerializeField] protected CharCore m_Core;
	[SerializeField] protected CharState m_State;
	[SerializeField] protected CharInfo m_Info;

	private bool m_IsMove;
	private bool m_IsFront;
	private bool m_IsChangeSpeed;
	private bool m_IsChangeState;
	private bool m_IsLock;

	void Awake()
	{
		if (m_Info == null){
			m_Info = gameObject.AddComponent<CharInfo>();
		}		
	}

	// 初期化
	public override void BrainIni(){
		m_IsMove = false;
		m_IsFront = false;
		m_IsChangeSpeed = false;
		m_IsChangeState = false;

		m_Info.m_HP = m_Info.MAX_HP;
	}
	// 更新
	public override void BrainUpdate(){
		StateUpdate();
	}

	// キャラクター情報更新
	public void InfoUpdate(){
		m_Info.m_HP += GetHP();

		// 移動情報更新
		if (IsFlag(ref m_IsMove)){
			m_Info.m_CurrentVelocity = GetInputVelocity();
		}

		// 速度情報更新
		if (IsFlag(ref m_IsChangeSpeed)){
			m_Info.m_CurrentSpeed = GetInputSpeed();
		}

		// 回転情報更新
		if (m_IsFront){
			m_Info.m_CurrentFront = GetInputFront();
		}
	}

	// 状態情報更新
	public void StateUpdate(){
		if (IsFlag(ref m_IsChangeState)){
			m_State.StateChange(GetNextState());
			if (m_IsLock){
				m_State.Lock();
			}
			else{
				m_State.UnLock();
			}
		}
	}

	// 移動量情報を設定
	public void SetInputVelocity(Vector3 velocity){
		m_InputVelocity = velocity;
		m_IsMove = true;
		SetInputSpeed(velocity.magnitude);
	}

	// 移動量情報を取得
	public Vector3 GetInputVelocity(){
		Vector3 tmp = m_InputVelocity;
		m_InputVelocity = Vector3.zero;
		return tmp;
	}

	// 回転情報を設定
	public void SetInputFront(Vector3 front){
		m_InputFront = front;
		m_IsFront = true;
	}

	// 回転情報を取得
	public Vector3 GetInputFront(){
		Vector3 tmp = m_InputFront;
		m_InputFront = Vector3.zero;
		return tmp;
	}

	// 移動速度情報を設定
	public void SetInputSpeed(float speed){
		m_InputSpeed = speed;
		m_IsChangeSpeed = true;
	}

	// 移動速度情報を取得
	public float GetInputSpeed(){
		float tmp = m_InputSpeed;
		m_InputSpeed = 0.0f;
		return tmp;
	}

	// 状態情報設定
	public void SetNextState(CharState.State state, bool isLock){
		m_NextState = state;
		m_IsChangeState = true;
		m_IsLock = isLock;
	}

	// 状態情報取得
	public CharState.State GetNextState(){
		CharState.State tmp = m_NextState;
		m_NextState = CharState.State.NONE;
		return tmp;
	}

	// ダメージ情報を設定
	public void SetDamage(int damage){
		m_Damage = damage;
	}

	// ダメージ情報取得
	public int GetHP(){
		int tmp = m_Damage;
		m_Damage = 0;
		return tmp;
	}

	// 移動基準視点を設定
	public void SetLookBase(Transform target){
		m_LookBase = target;
	}

	// 移動基準視点を取得
	public Transform GetLookBase(){
		return m_LookBase;
	}

	// コアクラスを取得
	public CharCore GetCore(){
		return m_Core;
	}

	// 状態クラスを取得
	public CharState GetState(){
		return m_State;
	}

	// 情報クラスを取得
	public CharInfo GetInfo(){
		return m_Info;
	}

	// 座標と回転を設定
	public void SetTransform(Quaternion rotation, Vector3 position){
		transform.rotation = rotation;
		transform.position = position;
	}

	// HPを設定
	public void SetHP(int hp){
		m_Info.m_HP = hp;
	}

	// 回転可能か？
	public bool IsRotate(){
		return IsFlag(ref m_IsFront);
	}

	// 渡されたフラグからtrue,falseの状態取得とフラグのリセット
	private bool IsFlag(ref bool isFlag){
		if (isFlag){
			isFlag = false;
			return true;
		}
		else return false;
	}
}
