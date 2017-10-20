using System.Collections;
using System.Collections.Generic;

public class AnmCombs{
	public List<string> _combs;

	public AnmCombs(List<string> combs){
		_combs = combs;
	}
}

public class AnmCombDataBase{

	public static Dictionary<string, AnmCombs> COMBS_DATA = new Dictionary<string, AnmCombs>(){
		{"Jab", new AnmCombs(new List<string>(){"Jab", "Jab", "Jab"})},
		{"Hikick", new AnmCombs(new List<string>(){"Jab", "Jab", "Hikick"})},
		{"Spinkick", new AnmCombs(new List<string>(){"Jab", "Jab", "Spinkick"})},
	};
}