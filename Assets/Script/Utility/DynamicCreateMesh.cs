using UnityEngine;
using System.Collections;

[RequireComponent (typeof(MeshRenderer))]
[RequireComponent (typeof(MeshFilter))]
public class DynamicCreateMesh : MonoBehaviour
{
	public void CreateMesh(Mesh mesh){
        mesh.RecalculateNormals ();
        var filter = GetComponent<MeshFilter> ();
        filter.sharedMesh = mesh;
	}

	public void CreateMesh(Mesh mesh, Material mat){
        mesh.RecalculateNormals ();
        var filter = GetComponent<MeshFilter> ();
        filter.sharedMesh = mesh;

		var renderer = GetComponent<MeshRenderer> ();
        renderer.material = mat;
	}
}
