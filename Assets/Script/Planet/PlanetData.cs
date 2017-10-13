using UnityEngine;

public struct PlanetData
{
	public PlanetData(string name, Vector3 position, int id, float size)
	{
		_name = name;
		_position = position;
		_id = id;
		_size = size;
	}
	
	public string _name;
	public Vector3 _position;
	public int _id;
	public float _size;
}