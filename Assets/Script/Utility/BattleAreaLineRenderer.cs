using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAreaLineRenderer : MonoBehaviour {

	public int vertexNums = 361;
	public float width = 1.0f;
	public float arc{get;set;}

	private int prevVertexNums;
	private LineRenderer lineDraw;

	void Awake(){
		if ((lineDraw = gameObject.GetComponent<LineRenderer>()) == null){
			lineDraw = gameObject.AddComponent<LineRenderer>();
		}
		lineDraw.material = new Material(Shader.Find("Particles/Additive"));
		lineDraw.SetColors(Color.yellow, Color.yellow);
	}

	// Use this for initialization
	void Start () {
		DrawLine();
	}
	
	// Update is called once per frame
	void Update () {
		lineDraw.widthMultiplier = width;
		DrawLine();
	}

	void DrawLine(){
		lineDraw.numPositions = vertexNums;
		List<Vector3> vertexs = new List<Vector3>();
		Quaternion pRotate = transform.rotation;
		float vAngle = Rigidbody_grgr.Angle(arc, GameManager.m_Planet.transform.localScale.y * 0.5f);
		for(int i = 0; i <= vertexNums; i++){
			Vector3 p;
			Quaternion rotate = Quaternion.AngleAxis(i, pRotate * Vector3.up) * pRotate;
			rotate = Quaternion.AngleAxis(vAngle, rotate * Vector3.right) * rotate;

			p = GameManager.m_Planet.transform.position + (rotate * Vector3.up * (GameManager.m_Planet.transform.localScale.y * 0.5f + 0.1f));
			vertexs.Add(p);
		}
		lineDraw.SetPositions(vertexs.ToArray());
	}

	public void SetColor(Color color){
		lineDraw.SetColors(color, color);
	}
}
