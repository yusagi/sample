using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleAreaLineRenderer : MonoBehaviour {

	public int vertexNums = 361;
	public float width = 1.0f;
	public float arc{get;set;}
	public Transform m_Planet;
	public CharCore m_Core;

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
		m_Planet = m_Core.m_PlanetManager.GetPlanet(m_Core.GetPlanetID()).transform;
		arc = m_Core.m_BattleManager.BATTLE_START_DISTANCE;
		//DrawLine();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		lineDraw.widthMultiplier = width;
		DrawLine();
	}

	void DrawLine(){
		lineDraw.numPositions = vertexNums;
		List<Vector3> vertexs = new List<Vector3>();
		Quaternion pRotate = transform.rotation;
		float vAngle = Rigidbody_grgr.Angle(arc, m_Planet.localScale.y * 0.5f);
		for(int i = 0; i <= vertexNums; i++){
			Vector3 p;
			Quaternion rotate = Quaternion.AngleAxis(i, pRotate * Vector3.up) * pRotate;
			rotate = Quaternion.AngleAxis(vAngle, rotate * Vector3.right) * rotate;

			p = m_Planet.position + (rotate * Vector3.up * (m_Planet.localScale.y * 0.5f + 0.1f));
			vertexs.Add(p);
		}
		lineDraw.SetPositions(vertexs.ToArray());
	}

	public void SetColor(Color color){
		lineDraw.SetColors(color, color);
	}
}
