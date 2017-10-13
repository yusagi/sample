using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMove : MonoBehaviour {
	
	public void Move(Vector3 velocity){
		transform.position += velocity;
	}

	public void SetPosition(Vector3 position){
		transform.position = position;
	}
}
