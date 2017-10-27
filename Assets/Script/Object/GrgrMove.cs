using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrgrStand))]
public class GrgrMove : MonoBehaviour {

	[SerializeField] private GrgrStand m_Stand;
	
	public void Move(Vector3 center, float radius, Vector3 velocity, float jamp = 0.0f){
		float tmpSpeed = velocity.magnitude;
		if (tmpSpeed > Vector3.kEpsilon){
			float arc = tmpSpeed;
			float angle = (arc * 360.0f) / (2 * Mathf.PI * radius);

			Quaternion baseRot = Quaternion.LookRotation(velocity, transform.up);
			Quaternion rot = Quaternion.AngleAxis(angle, baseRot * Vector3.right) * baseRot;

			transform.rotation = rot;

			m_Stand.Stand(center, radius, jamp);
		}
	}
}
