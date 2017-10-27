using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderBase : MonoBehaviour {
	
	
	void OnCollisionEnter(Collision other)
	{
		
	}

	protected virtual void CollisionEnterUpdate(Collision other){

	}
}
