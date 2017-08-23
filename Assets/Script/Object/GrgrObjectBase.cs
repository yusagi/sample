using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrgrObjectBase : MonoBehaviour {

    #region メンバ変数
    public float HEIGHT_FROM_GROUND = -0.6f;
    #endregion

    public float m_Speed = 10.0f;
    public Vector3 m_Velocity = new Vector3(0,0,1);
    public float m_Jamp = 0.0f;

    protected virtual void Awake()
    {
        GrgrSetPosition(Vector3.zero, m_Jamp);
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	protected virtual void Update () {
	}

   
    // 球体を与えられた移動量で移動
    protected void GrgrMove(Vector3 velocity, float jamp)
    {
        Transform planet = GameManager.m_Planet.transform;

        if (velocity.magnitude > Vector3.kEpsilon)
        {
            float arc = velocity.magnitude;
            float angle = arc / (2.0f * Mathf.PI * planet.localScale.y * 0.5f) * 360.0f;
            transform.rotation = Quaternion.LookRotation(velocity.normalized, transform.up);
            transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
        }
        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, planet.position, planet.localScale.y * 0.5f, HEIGHT_FROM_GROUND + jamp);
    }

    // 球体を与えられた角度で移動
    protected void GrgrMove(float angle, float jamp)
    {
        Transform planet = GameManager.m_Planet.transform;

        if (angle > Vector3.kEpsilon)
        {
            transform.rotation = Quaternion.AngleAxis(angle, transform.right) * transform.rotation;
        }
        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, planet.position, planet.localScale.y * 0.5f, HEIGHT_FROM_GROUND + jamp);
    }

    // 与えられた座標に移動
    protected void GrgrSetPosition(Vector3 position, float jamp)
    {
        Transform planet = GameManager.m_Planet.transform;

        Vector3 up = (position - planet.position);
        if (up.magnitude > Vector3.kEpsilon)
        {
            Vector3 front = Vector3.ProjectOnPlane(transform.forward, up);
            if (front.magnitude <= Vector3.kEpsilon)
            {
                front = Vector3.ProjectOnPlane(transform.right, up);
            }

            transform.rotation = Quaternion.LookRotation(front, up);
        }

        transform.position = Rigidbody_grgr.RotateToPosition(transform.up, planet.position, planet.localScale.y * 0.5f, HEIGHT_FROM_GROUND + jamp);
    }
}
