using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Traiangle : MonoBehaviour {

	public Material _mat;
	// Use this for initialization
	void Start () {
		var mesh = new Mesh ();
        mesh.vertices = new Vector3[] {
            new Vector3 (0, 1f),
            new Vector3 (1f, -1f),
            new Vector3 (-1f, -1f),
			
        };
        mesh.triangles = new int[] {
            0, 1, 2 
        };

		GetComponent<DynamicCreateMesh>().CreateMesh(mesh, _mat);
		gameObject.AddComponent<MeshCollider>().sharedMesh = mesh;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
