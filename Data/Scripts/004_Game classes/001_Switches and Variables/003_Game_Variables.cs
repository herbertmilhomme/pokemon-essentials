//===============================================================================
// ** Game_Variables
//-------------------------------------------------------------------------------
//  This class handles variables. It's a wrapper for the built-in class "Array."
//  Refer to "Game.GameData.game_variables" for the instance of this class.
//===============================================================================
public partial class Game_Variables {
	public void initialize() {
		@data = new List<string>();
	}

	// Get Variable
	//     variable_id : variable ID
	public int this[int variable_id] { get {
		if (variable_id <= 5000 && !@data[variable_id].null()) return @data[variable_id];
		return 0;
		}
	}

	// Set Variable
	//     variable_id : variable ID
	//     value       : the variable's value
	public int this[(variable_id, value)] { get {
		if (variable_id <= 5000) @data[variable_id] = value;
		}
	}
}
