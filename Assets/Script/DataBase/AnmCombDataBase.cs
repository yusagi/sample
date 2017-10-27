using System.Collections;
using System.Collections.Generic;

public class AnmCombos{
	public List<string> _combos;

	public AnmCombos(List<string> combos){
		_combos = combos;
	}
}

public class AnmCombDataBase{

	public static Dictionary<string, AnmCombos> COMBS_DATA = new Dictionary<string, AnmCombos>(){
		{"Jab", new AnmCombos(new List<string>(){"Jab", "Jab"})},
		{"Hikick", new AnmCombos(new List<string>(){"Jab", "Hikick"})},
		{"Spinkick", new AnmCombos(new List<string>(){})},
	};
}