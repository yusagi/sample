using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowController : MonoBehaviour {
	
#region メンバ変数

	public Vector3 offset;
	private Image arrow;

#endregion

#region Unity関数
	void Awake(){
	}

	// Use this for initialization
	void Start () {
        Transform player = GameManager.m_Player.transform;
        transform.position = player.position;
        transform.position += player.forward * offset.z;
        arrow = GetComponentInChildren<Image>();
    }
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
        if (GameManager.m_Enemy == null)
        {
            return;
        }

		Transform target = GameManager.m_Enemy.transform;
		Transform player = GameManager.m_Player.transform;

		if (target.GetComponent<GrgrCharCtrl>().state.current == GrgrCharCtrl.State.ASCENSION){
			arrow.enabled = false;
			return;
		}
		else{
			arrow.enabled = true;
		}
		Vector3 targetVelocity = (target.position - player.position).normalized;
        if (targetVelocity.magnitude <= Vector3.kEpsilon)
        {
            return;
        }

		targetVelocity = Vector3.ProjectOnPlane(targetVelocity, player.up).normalized;
		transform.rotation = Quaternion.LookRotation(targetVelocity, player.up);

		transform.position = player.position;
		//transform.position += transform.right * offset.x;
		transform.position += transform.up * offset.y;
		transform.position += transform.forward * offset.z;
	}

#endregion

}
