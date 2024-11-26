//===============================================================================
// The Game module contains methods for saving and loading the game.
//===============================================================================
public static partial class Game {
	#region Class Functions
	#endregion

	// Initializes various global variables and loads the game data.
	public void initialize() {
		Game.GameData.game_temp          = new Game_Temp();
		Game.GameData.game_system        = new Game_System();
		Game.GameData.data_animations    = load_data("Data/Animations.rxdata");
		Game.GameData.data_tilesets      = load_data("Data/Tilesets.rxdata");
		Game.GameData.data_common_events = load_data("Data/CommonEvents.rxdata");
		Game.GameData.data_system        = load_data("Data/System.rxdata");
		LoadBattleAnimations;
		GameData.load_all;
		map_file = string.Format("Data/Map{0:3}.rxdata", Game.GameData.data_system.start_map_id);
		if (Game.GameData.data_system.start_map_id == 0 || !RgssExists(map_file)) {
			Debug.LogError(_INTL("No starting position was set in the map editor."));
			//throw new ArgumentException(_INTL("No starting position was set in the map editor."));
		}
	}

	// Loads bootup data from save file (if it exists) or creates bootup data (if
	// it doesn't).
	public void set_up_system() {
		save_data = (SaveData.exists()) ? SaveData.read_from_file(SaveData.FILE_PATH) : {};
		if (save_data.empty()) {
			SaveData.initialize_bootup_values;
		} else {
			SaveData.load_bootup_values(save_data);
		}
		// Set resize factor
		SetResizeFactor((int)Math.Min(Game.GameData.PokemonSystem.screensize, 4));
		// Set language (and choose language if there is no save file)
		if (!Settings.LANGUAGES.empty()) {
			if (save_data.empty() && Settings.LANGUAGES.length >= 2) Game.GameData.PokemonSystem.language = ChooseLanguage;
			MessageTypes.load_message_files(Settings.LANGUAGES[Game.GameData.PokemonSystem.language][1]);
		}
	}

	// Called when starting a new game. Initializes global variables
	// and transfers the player into the map scene.
	public void start_new() {
		if (Game.GameData.game_map&.events) {
			Game.GameData.game_map.events.each_value(event => event.clear_starting);
		}
		if (Game.GameData.game_temp) Game.GameData.game_temp.common_event_id = 0;
		Game.GameData.game_temp.begun_new_game = true;
		MapInterpreter&.clear;
		MapInterpreter&.setup(null, 0, 0);
		Game.GameData.scene = new Scene_Map();
		SaveData.load_new_game_values;
		Game.GameData.game_temp.last_uptime_refreshed_play_time = System.uptime;
		Game.GameData.stats.play_sessions += 1;
		Game.GameData.map_factory = new PokemonMapFactory(Game.GameData.data_system.start_map_id);
		Game.GameData.game_player.moveto(Game.GameData.data_system.start_x, Game.GameData.data_system.start_y);
		Game.GameData.game_player.refresh;
		Game.GameData.PokemonEncounters = new PokemonEncounters();
		Game.GameData.PokemonEncounters.setup(Game.GameData.game_map.map_id);
		Game.GameData.game_map.autoplay;
		Game.GameData.game_map.update;
	}

	// Loads the game from the given save data and starts the map scene.
	/// <param name="save_data">hash containing the save data</param>
	// @raise [SaveData.InvalidValueError] if an invalid value is being loaded
	public void load(Hash save_data) {
		validate save_data => Hash;
		SaveData.load_all_values(save_data);
		Game.GameData.game_temp.last_uptime_refreshed_play_time = System.uptime;
		Game.GameData.stats.play_sessions += 1;
		load_map;
		AutoplayOnSave;
		Game.GameData.game_map.update;
		Game.GameData.PokemonMap.updateMap;
		Game.GameData.scene = new Scene_Map();
	}

	// Loads and validates the map. Called when loading a saved game.
	public void load_map() {
		Game.GameData.game_map = Game.GameData.map_factory.map;
		magic_number_matches = (Game.GameData.game_system.magic_number == Game.GameData.data_system.magic_number);
		if (!magic_number_matches || Game.GameData.PokemonGlobal.safesave) {
			if (MapInterpreterRunning()) MapInterpreter.setup(null, 0);
			begin;
				Game.GameData.map_factory.setup(Game.GameData.game_map.map_id);
			rescue Errno.ENOENT;
				if (Core.DEBUG) {
					Message(_INTL("Map {1} was not found.", Game.GameData.game_map.map_id));
					map = WarpToMap;
					unless (map) exit;
					Game.GameData.map_factory.setup(map[0]);
					Game.GameData.game_player.moveto(map[1], map[2]);
				} else {
					Debug.LogError(_INTL("The map was not found. The game cannot continue."));
					//throw new ArgumentException(_INTL("The map was not found. The game cannot continue."));
				}
			}
			Game.GameData.game_player.center(Game.GameData.game_player.x, Game.GameData.game_player.y);
		} else {
			Game.GameData.map_factory.setMapChanged(Game.GameData.game_map.map_id);
		}
		if (Game.GameData.game_map.events.null()) {
			Debug.LogError(_INTL("The map is corrupt. The game cannot continue."));
			//throw new ArgumentException(_INTL("The map is corrupt. The game cannot continue."));
		}
		Game.GameData.PokemonEncounters = new PokemonEncounters();
		Game.GameData.PokemonEncounters.setup(Game.GameData.game_map.map_id);
		UpdateVehicle;
	}

	// Saves the game. Returns whether the operation was successful.
	/// <param name="save_file">the save file path</param>
	/// <param name="safe">whether Game.GameData.PokemonGlobal.safesave should be set to true</param>
	// @return [Boolean] whether the operation was successful
	// @raise [SaveData.InvalidValueError] if an invalid value is being saved
	public void save(String save_file = SaveData.FILE_PATH, Boolean safe: false) {
		validate save_file => String, safe => [TrueClass, FalseClass];
		Game.GameData.PokemonGlobal.safesave = safe;
		Game.GameData.game_system.save_count += 1;
		Game.GameData.game_system.magic_number = Game.GameData.data_system.magic_number;
		Game.GameData.stats.set_time_last_saved;
		begin;
			SaveData.save_to_file(save_file);
			Graphics.frame_reset;
		rescue IOError, SystemCallError;
			Game.GameData.game_system.save_count -= 1;
			return false;
		}
		return true;
	}
}
