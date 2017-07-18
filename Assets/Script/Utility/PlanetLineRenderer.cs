using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetLineRenderer : MonoBehaviour {

	public int vertexNums = 360;
	public float width = 1.0f;

	private int prevVertexNums;
	private LineRenderer lineDraw;

	void Awake(){
		lineDraw = gameObject.AddComponent<LineRenderer>();
		lineDraw.material = new Material(Shader.Find("Particles/Additive"));
		prevVertexNums = vertexNums;
	}

	// Use this for initialization
	void Start () {
		DrawLine();
	}
	
	// Update is called once per frame
	void Update () {
		lineDraw.widthMultiplier = width;
		if  (prevVertexNums != vertexNums){
			DrawLine();
			prevVertexNums = vertexNums;
		}
	}

	void DrawLine(){
		lineDraw.numPositions = vertexNums*2 + 1;
		List<Vector3> vertexs = new List<Vector3>();

		float[] angles = new float[vertexNums + 1];

		for(int i = 0; i <= vertexNums * 2; i++){
			Vector3 p;

			if (i <= vertexNums)
			{
				angles[i] = i * (360.0f / vertexNums);
				p = transform.position + (Quaternion.AngleAxis(angles[i], Vector3.forward) * transform.up * (transform.localScale.y*0.5f + 0.2f));
			}
			else{
				p = transform.position + (Quaternion.AngleAxis(angles[i - vertexNums], Vector3.right) * transform.up * (transform.localScale.y*0.5f + 0.2f));
			}
			vertexs.Add(p);
		}
		transform.rotation = Quaternion.identity;
		lineDraw.SetPositions(vertexs.ToArray());
	}
}
