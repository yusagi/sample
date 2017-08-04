using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		RaycastHit hit;
		if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)){
			return;
		}

		MeshCollider meshCollider = hit.collider as MeshCollider;
		if (meshCollider == null || meshCollider.sharedMesh == null){
			return;
		}
		
		Mesh mesh = meshCollider.sharedMesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.triangles;

        Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
		Transform hitTransform = hit.collider.transform;
		p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);

		Mesh new_mesh = new Mesh();
		new_mesh.vertices = new Vector3[]{
			p0,
			p1,
			p2
		};

		new_mesh.triangles = new int[]{
			0, 1, 2
		};

		string name = "mesh" + hit.triangleIndex.ToString();
		if (GameObject.Find(name) != null){
			return;
		}
		GameObject obj = new GameObject(name);
		obj.AddComponent<DynamicCreateMesh>().CreateMesh(new_mesh);
		obj.transform.position += hit.normal * 0.01f;
	}
}
