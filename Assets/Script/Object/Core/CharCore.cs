using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrgrMove))]
public class CharCore : BaseCore {
	[SerializeField] protected GrgrMove m_Move;
	[SerializeField] protected CharBrain m_Brain;
	
	public BattleManager m_BattleManager{get;set;}

	protected override void CoreAwake(){
		if (m_Brain == null){
			m_Brain = gameObject.AddComponent<CharBrain>();
		}
	}

	protected override void CoreStart(){
		m_Brain.BrainIni();

		Transform planet = m_PlanetManager.GetPlanet(GetPlanetID()).transform;
		m_Stand.Stand(planet.position, planet.localScale.y * 0.5f, GROUND_UP);
	}

	protected override void CoreUpdate(){
		m_Brain.InfoUpdate();
		m_Brain.BrainUpdate();
		m_Brain.InfoUpdate();

		Transform planet = m_PlanetManager.GetPlanet(GetPlanetID()).transform;
		m_Move.Move(planet.position, planet.localScale.y * 0.5f, m_Brain.GetInfo().m_CurrentVelocity * Time.deltaTime, GROUND_UP + m_Brain.GetInfo().m_Jamp);
		if (m_Brain.IsRotate()){
			m_Stand.Rotate(m_Brain.GetInfo().m_CurrentFront);
		}
	}

	public CharBrain GetBrain(){
		return m_Brain;
	}

	public GrgrStand GetStand(){
		return m_Stand;
	}

	public GrgrMove GetMove(){
		return m_Move;
	}
}
