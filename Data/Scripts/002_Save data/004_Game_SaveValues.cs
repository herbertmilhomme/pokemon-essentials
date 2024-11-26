//===============================================================================
// Contains the save values defined in Essentials by default.
//===============================================================================

SaveData.register(:player) do;
	ensure_class :Player;
	save_value { Game.GameData.player };
	load_value { |value| Game.GameData.player = value };
	new_game_value { new Player("Unnamed", GameData.TrainerType.keys.first) };
}

SaveData.register(:game_system) do;
	load_in_bootup;
	ensure_class :Game_System;
	save_value { Game.GameData.game_system };
	load_value { |value| Game.GameData.game_system = value };
	new_game_value { new Game_System() };
	reset_on_new_game;
}

SaveData.register(:pokemon_system) do;
	load_in_bootup;
	ensure_class :PokemonSystem;
	save_value { Game.GameData.PokemonSystem };
	load_value { |value| Game.GameData.PokemonSystem = value };
	new_game_value { new PokemonSystem() };
}

SaveData.register(:switches) do;
	ensure_class :Game_Switches;
	save_value { Game.GameData.game_switches };
	load_value { |value| Game.GameData.game_switches = value };
	new_game_value { new Game_Switches() };
}

SaveData.register(:variables) do;
	ensure_class :Game_Variables;
	save_value { Game.GameData.game_variables };
	load_value { |value| Game.GameData.game_variables = value };
	new_game_value { new Game_Variables() };
}

SaveData.register(:self_switches) do;
	ensure_class :Game_SelfSwitches;
	save_value { Game.GameData.game_self_switches };
	load_value { |value| Game.GameData.game_self_switches = value };
	new_game_value { new Game_SelfSwitches() };
}

SaveData.register(:game_screen) do;
	ensure_class :Game_Screen;
	save_value { Game.GameData.game_screen };
	load_value { |value| Game.GameData.game_screen = value };
	new_game_value { new Game_Screen() };
}

SaveData.register(:map_factory) do;
	ensure_class :PokemonMapFactory;
	save_value { Game.GameData.map_factory };
	load_value { |value| Game.GameData.map_factory = value };
}

SaveData.register(:game_player) do;
	ensure_class :Game_Player;
	save_value { Game.GameData.game_player };
	load_value { |value| Game.GameData.game_player = value };
	new_game_value { new Game_Player() };
}

SaveData.register(:global_metadata) do;
	ensure_class :PokemonGlobalMetadata;
	save_value { Game.GameData.PokemonGlobal };
	load_value { |value| Game.GameData.PokemonGlobal = value };
	new_game_value { new PokemonGlobalMetadata() };
}

SaveData.register(:map_metadata) do;
	ensure_class :PokemonMapMetadata;
	save_value { Game.GameData.PokemonMap };
	load_value { |value| Game.GameData.PokemonMap = value };
	new_game_value { new PokemonMapMetadata() };
}

SaveData.register(:bag) do;
	ensure_class :PokemonBag;
	save_value { Game.GameData.bag };
	load_value { |value| Game.GameData.bag = value };
	new_game_value { new PokemonBag() };
}

SaveData.register(:storage_system) do;
	ensure_class :PokemonStorage;
	save_value { Game.GameData.PokemonStorage };
	load_value { |value| Game.GameData.PokemonStorage = value };
	new_game_value { new PokemonStorage() };
}

SaveData.register(:essentials_version) do;
	load_in_bootup;
	ensure_class :String;
	save_value { Essentials.VERSION };
	load_value { |value| Game.GameData.save_engine_version = value };
	new_game_value { Essentials.VERSION };
}

SaveData.register(:game_version) do;
	load_in_bootup;
	ensure_class :String;
	save_value { Settings.GAME_VERSION };
	load_value { |value| Game.GameData.save_game_version = value };
	new_game_value { Settings.GAME_VERSION };
}

SaveData.register(:stats) do;
	load_in_bootup;
	ensure_class :GameStats;
	save_value { Game.GameData.stats };
	load_value { |value| Game.GameData.stats = value };
	new_game_value { new GameStats() };
	reset_on_new_game;
}
