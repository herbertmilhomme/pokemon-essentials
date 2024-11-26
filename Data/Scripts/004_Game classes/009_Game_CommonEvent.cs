//===============================================================================
// ** Game_CommonEvent
//-------------------------------------------------------------------------------
//  This class handles common events. It includes execution of parallel process
//  event. This class is used within the Game_Map class (Game.GameData.game_map).
//===============================================================================
public partial class Game_CommonEvent {
	public void initialize(common_event_id) {
		@common_event_id = common_event_id;
		@interpreter = null;
		refresh;
	}

	public void name() {
		return Game.GameData.data_common_events[@common_event_id].name;
	}

	public void trigger() {
		return Game.GameData.data_common_events[@common_event_id].trigger;
	}

	public void switch_id() {
		return Game.GameData.data_common_events[@common_event_id].switch_id;
	}

	public void list() {
		return Game.GameData.data_common_events[@common_event_id].list;
	}

	public bool switchIsOn(id) {
		switchName = Game.GameData.data_system.switches[id];
		if (!switchName) return false;
		if (System.Text.RegularExpressions.Regex.IsMatch(switchName,@"^s\:")) {
			return eval($~.post_match);
		} else {
			return Game.GameData.game_switches[id];
		}
	}

	public void refresh() {
		// Create an interpreter for parallel process if necessary
		if (self.trigger == 2 && switchIsOn(self.switch_id)) {
			if (@interpreter.null()) @interpreter = new Interpreter();
		} else {
			@interpreter = null;
		}
	}

	public void update() {
		if (!@interpreter) return;
		// Set up event if interpreter is not running
		if (!@interpreter.running()) @interpreter.setup(self.list, 0);
		// Update interpreter
		@interpreter.update;
	}
}
