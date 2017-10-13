using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrgrMove))]
public class CharCore : BaseCore {
	GrgrStand m_Stand;
	GrgrMove m_Move;
	
	public PlanetManager m_PlanetManager{get;set;}

	// Use this for initialization
	void Start () {
		m_Stand = GetComponent<GrgrStand>();
		m_Move = GetComponent<GrgrMove>();
		
		Transform planet = m_PlanetManager.GetPlanet(m_Brain.GetPlanetID()).transform;
		m_Stand.Stand(planet.position, planet.localScale.y * 0.5f, 0.0f);
	}
	
	// Update is called once per frame
	void Update () {
		m_Brain.BrainUpdate();

		Transform planet = m_PlanetManager.GetPlanet(m_Brain.GetPlanetID()).transform;
		m_Move.Move(planet.position, planet.localScale.y * 0.5f, m_Brain.GetVelocity(), 0.0f);
	}
}
