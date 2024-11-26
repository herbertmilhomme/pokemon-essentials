//===============================================================================
// ** Game_Switches
//-------------------------------------------------------------------------------
//  This class handles switches. It's a wrapper for the built-in class "Array."
//  Refer to "Game.GameData.game_switches" for the instance of this class.
//===============================================================================
public partial class Game_Switches {
	public void initialize() {
		@data = new List<string>();
	}

	// Get Switch
	//     switch_id : switch ID
	public int this[int switch_id] { get {
		if (switch_id <= 5000 && @data[switch_id]) return @data[switch_id];
		return false;
		}
	}

	// Set Switch
	//     switch_id : switch ID
	//     value     : ON (true) / OFF (false)
	public int this[(switch_id, value)] { get {
		if (switch_id <= 5000) @data[switch_id] = value;
		}
	}
}
