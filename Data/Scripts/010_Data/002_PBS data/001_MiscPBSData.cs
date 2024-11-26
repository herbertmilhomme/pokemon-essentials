//===============================================================================
// Data caches.
//===============================================================================
public partial class Game_Temp {
	public int regional_dexes_data		{ get { return _regional_dexes_data; } set { _regional_dexes_data = value; } }			protected int _regional_dexes_data;
	public int battle_animations_data		{ get { return _battle_animations_data; } set { _battle_animations_data = value; } }			protected int _battle_animations_data;
	public int move_to_battle_animation_data		{ get { return _move_to_battle_animation_data; } set { _move_to_battle_animation_data = value; } }			protected int _move_to_battle_animation_data;
	public int map_infos		{ get { return _map_infos; } set { _map_infos = value; } }			protected int _map_infos;
}

public void ClearData() {
	if (Game.GameData.game_temp) {
		Game.GameData.game_temp.regional_dexes_data           = null;
		Game.GameData.game_temp.battle_animations_data        = null;
		Game.GameData.game_temp.move_to_battle_animation_data = null;
		Game.GameData.game_temp.map_infos                     = null;
	}
	MapFactoryHelper.clear;
	if (Game.GameData.game_map && Game.GameData.PokemonEncounters) Game.GameData.PokemonEncounters.setup(Game.GameData.game_map.map_id);
	if (RgssExists("Data/Tilesets.rxdata")) {
		Game.GameData.data_tilesets = load_data("Data/Tilesets.rxdata");
	}
}

//===============================================================================
// Method to get Regional Dexes data.
//===============================================================================
public void LoadRegionalDexes() {
	if (!Game.GameData.game_temp) Game.GameData.game_temp = new Game_Temp();
	if (!Game.GameData.game_temp.regional_dexes_data) {
		Game.GameData.game_temp.regional_dexes_data = load_data("Data/regional_dexes.dat");
	}
	return Game.GameData.game_temp.regional_dexes_data;
}

//===============================================================================
// Methods relating to battle animations data.
//===============================================================================
public void LoadBattleAnimations() {
	if (!Game.GameData.game_temp) Game.GameData.game_temp = new Game_Temp();
	if (!Game.GameData.game_temp.battle_animations_data && RgssExists("Data/PkmnAnimations.rxdata")) {
		Game.GameData.game_temp.battle_animations_data = load_data("Data/PkmnAnimations.rxdata");
	}
	return Game.GameData.game_temp.battle_animations_data;
}

public void LoadMoveToAnim() {
	if (!Game.GameData.game_temp) Game.GameData.game_temp = new Game_Temp();
	if (!Game.GameData.game_temp.move_to_battle_animation_data) {
		Game.GameData.game_temp.move_to_battle_animation_data = load_data("Data/move2anim.dat") || [];
	}
	return Game.GameData.game_temp.move_to_battle_animation_data;
}

//===============================================================================
// Method relating to map infos data.
//===============================================================================
public void LoadMapInfos() {
	if (!Game.GameData.game_temp) Game.GameData.game_temp = new Game_Temp();
	if (!Game.GameData.game_temp.map_infos) {
		Game.GameData.game_temp.map_infos = load_data("Data/MapInfos.rxdata");
	}
	return Game.GameData.game_temp.map_infos;
}
