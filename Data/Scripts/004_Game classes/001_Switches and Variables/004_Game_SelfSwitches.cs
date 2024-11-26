//===============================================================================
// ** Game_SelfSwitches
//-------------------------------------------------------------------------------
//  This class handles self switches. It's a wrapper for the built-in class
//  "Hash." Refer to "Game.GameData.game_self_switches" for the instance of this class.
//===============================================================================
public partial class Game_SelfSwitches {
	public void initialize() {
		@data = new List<string>();
	}

	// Get Self Switch
	//     key : key
	public int this[int key] { get {
		return @data[key] == true;
		}
	}

	// Set Self Switch
	//     key   : key
	//     value : ON (true) / OFF (false)
	public int this[(key, value)] { get {
		@data[key] = value;
		}
	}
}
