using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAreaLineRenderer : MonoBehaviour {

	public int vertexNums = 360;
	public float width = 1.0f;

	private int prevVertexNums;
	private LineRenderer lineDraw;

	void Awake(){
		lineDraw = gameObject.AddComponent<LineRenderer>();
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
		Quaternion pRotate = GameData.GetPlayer().rotation;
		float vAngle = 30.0f;
		for(int i = 0; i <= vertexNums; i++){
			Vector3 p;
			Quaternion rotate = Quaternion.AngleAxis(i, pRotate * Vector3.up) * pRotate;
			rotate = Quaternion.AngleAxis(vAngle, rotate * Vector3.right) * rotate;

			p = GameData.GetPlanet().position + (rotate * Vector3.up * (GameData.GetPlanet().localScale.y * 0.5f + 0.1f));
			vertexs.Add(p);
		}
		transform.rotation = Quaternion.identity;
		lineDraw.SetPositions(vertexs.ToArray());
	}
}
