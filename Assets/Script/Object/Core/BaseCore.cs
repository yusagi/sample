using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GrgrStand))]
public class BaseCore : MonoBehaviour {
	[SerializeField] protected GrgrStand m_Stand;

	public PlanetManager m_PlanetManager{get;set;}
	public float GROUND_UP;
	private PlanetID m_PlanetID;
	
	void Awake()
	{
		CoreAwake();
	}
	
	void Start()
	{
		CoreStart();
	}

	void Update(){
		CoreUpdate();
	}

	protected virtual void CoreAwake(){

	}

	protected virtual void CoreStart(){

	}

	protected virtual void CoreUpdate(){
		Transform planet = m_PlanetManager.GetPlanet(GetPlanetID()).transform;
		m_Stand.Stand(planet.position, planet.localScale.y * 0.5f, GROUND_UP);
	}

	public void SetPlanetID(PlanetID id){
		m_PlanetID = id;
	}

	public PlanetID GetPlanetID(){
		return m_PlanetID;
	}


}
