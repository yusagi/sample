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
		Transform player = GameData.GetPlayer();
		transform.position = player.position;
		transform.position += player.forward * offset.z;
		arrow = GetComponentInChildren<Image>();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	}

	void LateUpdate(){
		Transform target = GameData.GetEnemy();
		Transform player = GameData.GetPlayer();

		if (target.GetComponent<FEnemyController>().planetWalk.isActive == false){
			arrow.enabled = false;
		}
		else{
			arrow.enabled = true;
		}
		Vector3 targetVelocity = (target.position - player.position).normalized;
		targetVelocity = Vector3.ProjectOnPlane(targetVelocity, player.up).normalized;
		transform.rotation = Quaternion.LookRotation(targetVelocity, player.up);

		transform.position = player.position;
		//transform.position += transform.right * offset.x;
		transform.position += transform.up * offset.y;
		transform.position += transform.forward * offset.z;
	}

#endregion

}
