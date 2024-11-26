//===============================================================================
//
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int roamPosition		{ get { return _roamPosition; } set { _roamPosition = value; } }			protected int _roamPosition;
	/// <summary>Whether a roamer has been encountered on current map</summary>
	public int roamedAlready		{ get { return _roamedAlready; } set { _roamedAlready = value; } }			protected int _roamedAlready;
	public int roamEncounter		{ get { return _roamEncounter; } set { _roamEncounter = value; } }			protected int _roamEncounter;
	public int roamPokemon		{ get { return _roamPokemon; } set { _roamPokemon = value; } }			protected int _roamPokemon;
	public int roamPokemonCaught		{ get { return _roamPokemonCaught; } }			protected int _roamPokemonCaught;

	public void roamPokemonCaught() {
		if (!@roamPokemonCaught) @roamPokemonCaught = new List<string>();
		return @roamPokemonCaught;
	}
}

//===============================================================================
// Making roaming Pokémon roam around.
//===============================================================================
// Resets all roaming Pokemon that were defeated without having been caught.
public void ResetAllRoamers() {
	if (!Game.GameData.PokemonGlobal.roamPokemon) return;
	for (int i = Game.GameData.PokemonGlobal.roamPokemon.length; i < Game.GameData.PokemonGlobal.roamPokemon.length; i++) { //for 'Game.GameData.PokemonGlobal.roamPokemon.length' times do => |i|
		if (Game.GameData.PokemonGlobal.roamPokemon[i] != true || !Game.GameData.PokemonGlobal.roamPokemonCaught[i]) continue;
		Game.GameData.PokemonGlobal.roamPokemon[i] = null;
	}
}

// Gets the roaming areas for a particular Pokémon.
public void RoamingAreas(idxRoamer) {
	// [species ID, level, Game Switch, encounter type, battle BGM, area maps hash]
	roamData = Settings.ROAMING_SPECIES[idxRoamer];
	if (roamData && roamData[5]) return roamData[5];
	return Settings.ROAMING_AREAS;
}

// Puts a roamer in a completely random map available to it.
public void RandomRoam(index) {
	if (!Game.GameData.PokemonGlobal.roamPosition) return;
	keys = RoamingAreas(index).keys;
	Game.GameData.PokemonGlobal.roamPosition[index] = keys[rand(keys.length)];
}

// Makes all roaming Pokémon roam to another map.
public void RoamPokemon() {
	if (!Game.GameData.PokemonGlobal.roamPokemon) Game.GameData.PokemonGlobal.roamPokemon = new List<string>();
	// Start all roamers off in random maps
	if (!Game.GameData.PokemonGlobal.roamPosition) {
		Game.GameData.PokemonGlobal.roamPosition = new List<string>();
		for (int i = Settings.ROAMING_SPECIES.length; i < Settings.ROAMING_SPECIES.length; i++) { //for 'Settings.ROAMING_SPECIES.length' times do => |i|
			if (!GameData.Species.exists(Settings.ROAMING_SPECIES[i][0])) continue;
			keys = RoamingAreas(i).keys;
			Game.GameData.PokemonGlobal.roamPosition[i] = keys[rand(keys.length)];
		}
	}
	// Roam each Pokémon in turn
	for (int i = Settings.ROAMING_SPECIES.length; i < Settings.ROAMING_SPECIES.length; i++) { //for 'Settings.ROAMING_SPECIES.length' times do => |i|
		RoamPokemonOne(i);
	}
}

// Makes a single roaming Pokémon roam to another map. Doesn't roam if it isn't
// currently possible to encounter it (i.e. its Game Switch is off).
public void RoamPokemonOne(idxRoamer) {
	// [species ID, level, Game Switch, encounter type, battle BGM, area maps hash]
	roamData = Settings.ROAMING_SPECIES[idxRoamer];
	if (roamData[2] > 0 && !Game.GameData.game_switches[roamData[2]]) return;   // Game Switch is off
	if (!GameData.Species.exists(roamData[0])) return;
	// Get hash of area patrolled by the roaming Pokémon
	mapIDs = RoamingAreas(idxRoamer).keys;
	if (!mapIDs || mapIDs.length == 0) return;   // No roaming area defined somehow
	// Get the roaming Pokémon's current map
	currentMap = Game.GameData.PokemonGlobal.roamPosition[idxRoamer];
	if (!currentMap) {
		currentMap = mapIDs[rand(mapIDs.length)];
		Game.GameData.PokemonGlobal.roamPosition[idxRoamer] = currentMap;
	}
	// Make an array of all possible maps the roaming Pokémon could roam to
	newMapChoices = new List<string>();
	nextMaps = RoamingAreas(idxRoamer)[currentMap];
	if (!nextMaps) return;
	nextMaps.each(map => newMapChoices.Add(map));
	// Rarely, add a random possible map into the mix
	if (rand(32) == 0) newMapChoices.Add(mapIDs[rand(mapIDs.length)]);
	// Choose a random new map to roam to
	if (newMapChoices.length > 0) {
		Game.GameData.PokemonGlobal.roamPosition[idxRoamer] = newMapChoices[rand(newMapChoices.length)];
	}
}

// When the player moves to a new map (with a different name), make all roaming
// Pokémon roam.
EventHandlers.add(:on_enter_map, :move_roaming_pokemon,
	block: (old_map_id) => {
		// Get and compare map names
		mapInfos = LoadMapInfos;
		if (mapInfos && old_map_id > 0 && mapInfos[old_map_id] &&
						mapInfos[old_map_id].name && Game.GameData.game_map.name == mapInfos[old_map_id].name) continue;
		// Make roaming Pokémon roam
		RoamPokemon;
		Game.GameData.PokemonGlobal.roamedAlready = false;
	}
)

//===============================================================================
// Encountering a roaming Pokémon in a wild battle.
//===============================================================================
public partial class Game_Temp {
	/// <summary>Index of roaming Pokémon to encounter next</summary>
	public int roamer_index_for_encounter		{ get { return _roamer_index_for_encounter; } set { _roamer_index_for_encounter = value; } }			protected int _roamer_index_for_encounter;
}

//===============================================================================
//
//===============================================================================
// Returns whether the given category of encounter contains the actual encounter
// method that will occur in the player's current position.
public void RoamingMethodAllowed(roamer_method) {
	enc_type = Game.GameData.PokemonEncounters.encounter_type;
	type = GameData.EncounterType.get(enc_type).type;
	switch (roamer_method) {
		case 0:   // Any step-triggered method (except Bug Contest)
			return new []{:land, :cave, :water}.Contains(type);
		case 1:   // Walking (except Bug Contest)
			return new []{:land, :cave}.Contains(type);
		case 2:   // Surfing
			return type == types.water;
		case 3:   // Fishing
			return type == types.fishing;
		case 4:   // Water-based
			return new []{:water, :fishing}.Contains(type);
	}
	return false;
}

EventHandlers.add(:on_wild_species_chosen, :roaming_pokemon,
	block: (encounter) => {
		Game.GameData.game_temp.roamer_index_for_encounter = null;
		if (!encounter) continue;
		// Give the regular encounter if encountering a roaming Pokémon isn't possible
		if (Game.GameData.PokemonGlobal.roamedAlready) continue;
		if (Game.GameData.PokemonGlobal.partner) continue;
		if (Game.GameData.game_temp.poke_radar_data) continue;
		if (rand(100) < 75) continue;   // 25% chance of encountering a roaming Pokémon
		// Look at each roaming Pokémon in turn and decide whether it's possible to
		// encounter it
		currentRegion = GetCurrentRegion;
		currentMapName = Game.GameData.game_map.name;
		possible_roamers = new List<string>();
		Settings.ROAMING_SPECIES.each_with_index do |data, i|
			// data = [species, level, Game Switch, roamer method, battle BGM, area maps hash]
			if (!GameData.Species.exists(data[0])) continue;
			if (data[2] > 0 && !Game.GameData.game_switches[data[2]]) continue;   // Isn't roaming
			if (Game.GameData.PokemonGlobal.roamPokemon[i] == true) continue;   // Roaming Pokémon has been caught
			// Get the roamer's current map
			roamerMap = Game.GameData.PokemonGlobal.roamPosition[i];
			if (!roamerMap) {
				mapIDs = RoamingAreas(i).keys;   // Hash of area patrolled by the roaming Pokémon
				if (!mapIDs || mapIDs.length == 0) continue;   // No roaming area defined somehow
				roamerMap = mapIDs[rand(mapIDs.length)];
				Game.GameData.PokemonGlobal.roamPosition[i] = roamerMap;
			}
			// If roamer isn't on the current map, check if it's on a map with the same
			// name and in the same region
			if (roamerMap != Game.GameData.game_map.map_id) {
				map_metadata = GameData.MapMetadata.try_get(roamerMap);
				if (!map_metadata || !map_metadata.town_map_position ||
								map_metadata.town_map_position[0] != currentRegion) continue;
				if (GetMapNameFromId(roamerMap) != currentMapName) continue;
			}
			// Check whether the roamer's roamer method is currently possible
			if (!RoamingMethodAllowed(data[3])) continue;
			// Add this roaming Pokémon to the list of possible roaming Pokémon to encounter
			possible_roamers.Add(new {i, data[0], data[1], data[4]});   // new {i, species, level, BGM}
		}
		// No encounterable roaming Pokémon were found, just have the regular encounter
		if (possible_roamers.length == 0) continue;
		// Pick a roaming Pokémon to encounter out of those available
		roamer = possible_roamers.sample;
		Game.GameData.PokemonGlobal.roamEncounter = roamer;
		Game.GameData.game_temp.roamer_index_for_encounter = roamer[0];
		if (roamer[3] && !roamer[3].empty()) Game.GameData.PokemonGlobal.nextBattleBGM = roamer[3];
		Game.GameData.game_temp.force_single_battle = true;
		encounter[0] = roamer[1];   // Species
		encounter[1] = roamer[2];   // Level
	}
)

EventHandlers.add(:on_calling_wild_battle, :roaming_pokemon,
	block: (pkmn, handled) => {
		// handled is an array: [null]. If [true] or [false], the battle has already
		// been overridden (the boolean is its outcome), so don't do anything that
		// would override it again
		if (!handled[0].null()) continue;
		if (!Game.GameData.PokemonGlobal.roamEncounter || Game.GameData.game_temp.roamer_index_for_encounter.null()) continue;
		handled[0] = RoamingPokemonBattle(pkmn);
	}
)

public void RoamingPokemonBattle(pkmn, level = 1) {
	// Get the roaming Pokémon to encounter; generate it based on the species and
	// level if it doesn't already exist
	idxRoamer = Game.GameData.game_temp.roamer_index_for_encounter;
	if (!Game.GameData.PokemonGlobal.roamPokemon[idxRoamer] ||
		!Game.GameData.PokemonGlobal.roamPokemon[idxRoamer].is_a(Pokemon)) {
		if (pkmn.is_a(Pokemon)) {
			Game.GameData.PokemonGlobal.roamPokemon[idxRoamer] = GenerateWildPokemon(pkmn.species_data.id, pkmn.level, true);
		} else {
			Game.GameData.PokemonGlobal.roamPokemon[idxRoamer] = GenerateWildPokemon(pkmn, level, true);
		}
	}
	// Set some battle rules
	setBattleRule("single");
	setBattleRule("roamerFlees");
	// Perform the battle
	outcome = WildBattle.start_core(Game.GameData.PokemonGlobal.roamPokemon[idxRoamer]);
	// Update Roaming Pokémon data based on result of battle
	if (new []{Battle.Outcome.WIN, Battle.Outcome.CATCH}.Contains(outcome)) {   // Defeated or caught
		Game.GameData.PokemonGlobal.roamPokemon[idxRoamer]       = true;
		Game.GameData.PokemonGlobal.roamPokemonCaught[idxRoamer] = (outcome == Battle.Outcome.CATCH);
	}
	Game.GameData.PokemonGlobal.roamEncounter = null;
	Game.GameData.PokemonGlobal.roamedAlready = true;
	Game.GameData.game_temp.roamer_index_for_encounter = null;
	// Used by the Poké Radar to update/break the chain
	EventHandlers.trigger(:on_wild_battle_end, pkmn.species_data.id, pkmn.level, outcome);
	// Return false if the player lost or drew the battle, and true if any other result
	return !Battle.Outcome.should_black_out(outcome);
}
