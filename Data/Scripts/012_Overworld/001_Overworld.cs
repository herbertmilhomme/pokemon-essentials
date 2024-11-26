//===============================================================================
// Constant checks.
//===============================================================================
// Pokérus check
EventHandlers.add(:on_frame_update, :pokerus_counter,
	block: () => {
		if (!Game.GameData.player || Game.GameData.player.party.none(pkmn => pkmn.pokerusStage == 1)) continue;
		last = Game.GameData.PokemonGlobal.pokerusTime;
		if (!last) continue;
		now = GetTimeNow;
		if (last.year != now.year || last.month != now.month || last.day != now.day) {
			Game.GameData.player.pokemon_party.each(pkmn => pkmn.lowerPokerusCount);
			Game.GameData.PokemonGlobal.pokerusTime = now;
		}
	}
)

// Returns whether the Poké Center should explain Pokérus to the player, if a
// healed Pokémon has it.
public bool Pokerus() {
	if (Game.GameData.game_switches[Settings.SEEN_POKERUS_SWITCH]) return false;
	foreach (var i in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |i|
		if (i.pokerusStage == 1) return true;
	}
	return false;
}

public partial class Game_Temp {
	public int warned_low_battery		{ get { return _warned_low_battery; } set { _warned_low_battery = value; } }			protected int _warned_low_battery;
	public int cue_bgm		{ get { return _cue_bgm; } set { _cue_bgm = value; } }			protected int _cue_bgm;
	public int cue_bgm_timer_start		{ get { return _cue_bgm_timer_start; } set { _cue_bgm_timer_start = value; } }			protected int _cue_bgm_timer_start;
	public int cue_bgm_delay		{ get { return _cue_bgm_delay; } set { _cue_bgm_delay = value; } }			protected int _cue_bgm_delay;
}

public bool BatteryLow() {
	pstate = System.power_state;
	// If it's not discharging, it doesn't matter if it's low
	if (!pstate.discharging) return false;
	// Check for less than 10m, priority over the percentage
	// Some laptops (Chromebooks, Macbooks) have very long lifetimes
	if (pstate.seconds && pstate.seconds <= 600) return true;
	// Check for <=15%
	if (pstate.percent && pstate.percent <= 15) return true;
	return false;
}

EventHandlers.add(:on_frame_update, :low_battery_warning,
	block: () => {
		if (Game.GameData.game_temp.warned_low_battery || !BatteryLow()) continue;
		if (Game.GameData.game_temp.in_menu || Game.GameData.game_temp.in_battle || Game.GameData.game_player.move_route_forcing ||
						Game.GameData.game_temp.message_window_showing || MapInterpreterRunning()) continue;
		Game.GameData.game_temp.warned_low_battery = true;
		Message(_INTL("The game has detected that the battery is low. You should save soon to avoid losing your progress."));
	}
)

EventHandlers.add(:on_frame_update, :cue_bgm_after_delay,
	block: () => {
		if (Game.GameData.game_temp.cue_bgm_delay.null()) continue;
		if (System.uptime - Game.GameData.game_temp.cue_bgm_timer_start < Game.GameData.game_temp.cue_bgm_delay) continue;
		Game.GameData.game_temp.cue_bgm_delay = null;
		if (Game.GameData.game_system.getPlayingBGM.null()) BGMPlay(Game.GameData.game_temp.cue_bgm);
	}
)

//===============================================================================
// Checks per step.
//===============================================================================
// Party Pokémon gain happiness from walking
EventHandlers.add(:on_player_step_taken, :gain_happiness,
	block: () => {
		if (!Game.GameData.PokemonGlobal.happinessSteps) Game.GameData.PokemonGlobal.happinessSteps = 0;
		Game.GameData.PokemonGlobal.happinessSteps += 1;
		if (Game.GameData.PokemonGlobal.happinessSteps < 128) continue;
		foreach (var pkmn in Game.GameData.player.able_party) { //'Game.GameData.player.able_party.each' do => |pkmn|
			if (rand(2) == 0) pkmn.changeHappiness("walking");
		}
		Game.GameData.PokemonGlobal.happinessSteps = 0;
	}
)

// Poison party Pokémon
EventHandlers.add(:on_player_step_taken_can_transfer, :poison_party,
	block: (handled) => {
		// handled is an array: [null]. If [true], a transfer has happened because of
		// this event, so don't do anything that might cause another one
		if (handled[0]) continue;
		if (!Settings.POISON_IN_FIELD || Game.GameData.PokemonGlobal.stepcount % 4 != 0) continue;
		flashed = false;
		foreach (var pkmn in Game.GameData.player.able_party) { //'Game.GameData.player.able_party.each' do => |pkmn|
			if (pkmn.status != statuses.POISON || pkmn.hasAbility(Abilitys.IMMUNITY)) continue;
			if (!flashed) {
				SEPlay("Poison step");
				Flash(new Color(255, 0, 0, 128), 8);
				flashed = true;
			}
			if (pkmn.hp > 1 || Settings.POISON_FAINT_IN_FIELD) pkmn.hp -= 1;
			if (pkmn.hp == 1 && !Settings.POISON_FAINT_IN_FIELD) {
				pkmn.status = :NONE;
				Message(_INTL("{1} survived the poisoning.\nThe poison faded away!", pkmn.name));
				continue;
			} else if (pkmn.hp == 0) {
				pkmn.changeHappiness("faint");
				pkmn.status = :NONE;
				Message(_INTL("{1} fainted...", pkmn.name));
			}
			if (Game.GameData.player.able_pokemon_count == 0) {
				handled[0] = true;
				CheckAllFainted;
			}
		}
	}
)

public void CheckAllFainted() {
	if (Game.GameData.player.able_pokemon_count == 0) {
		Message(_INTL("You have no more Pokémon that can fight!") + "\1");
		Message(_INTL("You blacked out!"));
		BGMFade(1.0);
		BGSFade(1.0);
		FadeOutIn { StartOver };
	}
}

// Gather soot from soot grass.
EventHandlers.add(:on_step_taken, :pick_up_soot,
	block: (event) => {
		thistile = Game.GameData.map_factory.getRealTilePos(event.map.map_id, event.x, event.y);
		map = Game.GameData.map_factory.getMap(thistile[0]);
		new {2, 1, 0}.each do |i|
			tile_id = map.data[thistile[1], thistile[2], i];
			if (tile_id.null()) continue;
			if (GameData.TerrainTag.try_get(map.terrain_tags[tile_id]).id != :SootGrass) continue;
			if (event == Game.GameData.game_player && Game.GameData.bag.has(:SOOTSACK)) {
				old_soot = Game.GameData.player.soot;
				Game.GameData.player.soot += 1;
				if (Game.GameData.player.soot > old_soot) Game.GameData.stats.soot_collected += Game.GameData.player.soot - old_soot;
			}
			map.erase_tile(thistile[1], thistile[2], i);
			break;
		}
	}
)

// Show grass rustle animation.
EventHandlers.add(:on_step_taken, :grass_rustling,
	block: (event) => {
		if (!Game.GameData.scene.is_a(Scene_Map)) continue;
		if (event.respond_to("name") && System.Text.RegularExpressions.Regex.IsMatch(event.name,@"airborne",RegexOptions.IgnoreCase)) continue;
		event.each_occupied_tile do |x, y|
			if (!Game.GameData.map_factory.getTerrainTagFromCoords(event.map.map_id, x, y, true).shows_grass_rustle) continue;
			spriteset = Game.GameData.scene.spriteset(event.map_id);
			spriteset&.addUserAnimation(Settings.GRASS_ANIMATION_ID, x, y, true, 1);
		}
	}
)

// Show water ripple animation.
EventHandlers.add(:on_step_taken, :still_water_ripple,
	block: (event) => {
		if (!Game.GameData.scene.is_a(Scene_Map)) continue;
		if (event.respond_to("name") && System.Text.RegularExpressions.Regex.IsMatch(event.name,@"airborne",RegexOptions.IgnoreCase)) continue;
		event.each_occupied_tile do |x, y|
			if (!Game.GameData.map_factory.getTerrainTagFromCoords(event.map.map_id, x, y, true).shows_water_ripple) continue;
			spriteset = Game.GameData.scene.spriteset(event.map_id);
			spriteset&.addUserAnimation(Settings.WATER_RIPPLE_ANIMATION_ID, x, y, true, -1);
		}
	}
)

// Auto-move the player over waterfalls and ice.
EventHandlers.add(:on_step_taken, :auto_move_player,
	block: (event) => {
		if (!Game.GameData.scene.is_a(Scene_Map)) continue;
		if (event != Game.GameData.game_player) continue;
		currentTag = Game.GameData.game_player.TerrainTag;
		if (currentTag.waterfall_crest || currentTag.waterfall ||
			Game.GameData.PokemonGlobal.descending_waterfall || Game.GameData.PokemonGlobal.ascending_waterfall) {
			TraverseWaterfall;
		} else if (currentTag.ice || Game.GameData.PokemonGlobal.ice_sliding) {
			SlideOnIce;
		}
	}
)

// Certain species of Pokémon record the distance travelled while they were in
// the party. Those species use this information to evolve.
EventHandlers.add(:on_step_taken, :party_pokemon_distance_tracker,
	block: (event) => {
		foreach (var pkmn in Game.GameData.player.pokemon_party) { //'Game.GameData.player.pokemon_party.each' do => |pkmn|
			if (!new []{:PAWMO, :BRAMBLIN, :RELLOR}.Contains(pkmn.species)) continue;
			pkmn.evolution_counter += 1;
		}
	}
)

public void OnStepTaken(eventTriggered) {
	if (Game.GameData.game_player.move_route_forcing || MapInterpreterRunning()) {
		EventHandlers.trigger(:on_step_taken, Game.GameData.game_player);
		return;
	}
	if (!Game.GameData.PokemonGlobal.stepcount) Game.GameData.PokemonGlobal.stepcount = 0;
	Game.GameData.PokemonGlobal.stepcount += 1;
	Game.GameData.PokemonGlobal.stepcount &= 0x7FFFFFFF;
	repel_active = (Game.GameData.PokemonGlobal.repel > 0);
	EventHandlers.trigger(:on_player_step_taken);
	handled = [null];
	EventHandlers.trigger(:on_player_step_taken_can_transfer, handled);
	if (handled[0]) return;
	if (!eventTriggered && !Game.GameData.game_temp.in_menu) BattleOnStepTaken(repel_active);
	Game.GameData.game_temp.encounter_triggered = false;   // This info isn't needed here
}

// Start wild encounters while turning on the spot.
EventHandlers.add(:on_player_change_direction, :trigger_encounter,
	block: () => {
		repel_active = (Game.GameData.PokemonGlobal.repel > 0);
		if (!Game.GameData.game_temp.in_menu) BattleOnStepTaken(repel_active);
	}
)

public void BattleOnStepTaken(repel_active) {
	if (Game.GameData.player.able_pokemon_count == 0 && !InSafari()) return;
	if (!Game.GameData.PokemonEncounters.encounter_possible_here()) return;
	encounter_type = Game.GameData.PokemonEncounters.encounter_type;
	if (!encounter_type) return;
	if (!Game.GameData.PokemonEncounters.encounter_triggered(encounter_type, repel_active)) return;
	Game.GameData.game_temp.encounter_type = encounter_type;
	encounter = Game.GameData.PokemonEncounters.choose_wild_pokemon(encounter_type);
	EventHandlers.trigger(:on_wild_species_chosen, encounter);
	if (Game.GameData.PokemonEncounters.allow_encounter(encounter, repel_active)) {
		if (Game.GameData.PokemonEncounters.have_double_wild_battle()) {
			encounter2 = Game.GameData.PokemonEncounters.choose_wild_pokemon(encounter_type);
			EventHandlers.trigger(:on_wild_species_chosen, encounter2);
			WildBattle.start(encounter, encounter2, can_override: true);
		} else {
			WildBattle.start(encounter, can_override: true);
		}
		Game.GameData.game_temp.encounter_type = null;
		Game.GameData.game_temp.encounter_triggered = true;
	}
	Game.GameData.game_temp.force_single_battle = false;
}

//===============================================================================
// Checks when moving between maps.
//===============================================================================
// Set up various data related to the new map.
EventHandlers.add(:on_enter_map, :setup_new_map,
	block: (old_map_id) => {   // previous map ID, is 0 if no map ID
		// Record new Teleport destination
		new_map_metadata = Game.GameData.game_map.metadata;
		if (new_map_metadata&.teleport_destination) {
			Game.GameData.PokemonGlobal.healingSpot = new_map_metadata.teleport_destination;
		}
		// End effects that apply only while on the map they were used
		Game.GameData.PokemonMap&.clear;
		// Setup new wild encounter tables
		Game.GameData.PokemonEncounters&.setup(Game.GameData.game_map.map_id);
		// Record the new map as having been visited
		Game.GameData.PokemonGlobal.visitedMaps[Game.GameData.game_map.map_id] = true;
	}
)

// Changes the overworld weather.
EventHandlers.add(:on_enter_map, :set_weather,
	block: (old_map_id) => {   // previous map ID, is 0 if no map ID
		if (old_map_id == 0 || old_map_id == Game.GameData.game_map.map_id) continue;
		old_weather = Game.GameData.game_screen.weather_type;
		new_weather = weathers.None;
		new_map_metadata = Game.GameData.game_map.metadata;
		if (new_map_metadata&.weather) {
			if (rand(100) < new_map_metadata.weather[1]) new_weather = new_map_metadata.weather[0];
		}
		if (old_weather == new_weather) continue;
		Game.GameData.game_screen.weather(new_weather, 9, 0);
	}
)

// Update trail of which maps the player has most recently visited.
EventHandlers.add(:on_enter_map, :add_to_trail,
	block: (_old_map_id) => {
		if (!Game.GameData.game_map) continue;
		if (!Game.GameData.PokemonGlobal.mapTrail) Game.GameData.PokemonGlobal.mapTrail = new List<string>();
		if (Game.GameData.PokemonGlobal.mapTrail[0] != Game.GameData.game_map.map_id && Game.GameData.PokemonGlobal.mapTrail.length >= 4) {
			Game.GameData.PokemonGlobal.mapTrail.pop;
		}
		Game.GameData.PokemonGlobal.mapTrail = [Game.GameData.game_map.map_id] + Game.GameData.PokemonGlobal.mapTrail;
	}
)

// Force cycling/walking.
EventHandlers.add(:on_enter_map, :force_cycling,
	block: (_old_map_id) => {
		if (Game.GameData.game_map.metadata&.always_bicycle) {
			MountBike;
		} else if (!CanUseBike(Game.GameData.game_map.map_id)) {
			DismountBike;
		}
	}
)

// Display darkness circle on dark maps.
EventHandlers.add(:on_map_or_spriteset_change, :show_darkness,
	block: (scene, _map_changed) => {
		if (!scene || !scene.spriteset) continue;
		map_metadata = Game.GameData.game_map.metadata;
		if (map_metadata&.dark_map) {
			Game.GameData.game_temp.darkness_sprite = new DarknessSprite();
			scene.spriteset.addUserSprite(Game.GameData.game_temp.darkness_sprite);
			if (Game.GameData.PokemonGlobal.flashUsed) {
				Game.GameData.game_temp.darkness_sprite.radius = Game.GameData.game_temp.darkness_sprite.radiusMax;
			}
		} else {
			Game.GameData.PokemonGlobal.flashUsed = false;
			Game.GameData.game_temp.darkness_sprite&.dispose;
			Game.GameData.game_temp.darkness_sprite = null;
		}
	}
)

// Show location signpost.
EventHandlers.add(:on_map_or_spriteset_change, :show_location_window,
	block: (scene, map_changed) => {
		if (!scene || !scene.spriteset) continue;
		if (!map_changed || !Game.GameData.game_map.metadata&.announce_location) continue;
		nosignpost = false;
		if (Game.GameData.PokemonGlobal.mapTrail[1]) {
			for (int i = (Settings.NO_SIGNPOSTS.length / 2); i < (Settings.NO_SIGNPOSTS.length / 2); i++) { //for '(Settings.NO_SIGNPOSTS.length / 2)' times do => |i|
				if (Settings.NO_SIGNPOSTS[2 * i] == Game.GameData.PokemonGlobal.mapTrail[1] &&
														Settings.NO_SIGNPOSTS[(2 * i) + 1] == Game.GameData.game_map.map_id) nosignpost = true;
				if (Settings.NO_SIGNPOSTS[(2 * i) + 1] == Game.GameData.PokemonGlobal.mapTrail[1] &&
														Settings.NO_SIGNPOSTS[2 * i] == Game.GameData.game_map.map_id) nosignpost = true;
				if (nosignpost) break;
			}
			if (Game.GameData.game_map.name == GetMapNameFromId(Game.GameData.PokemonGlobal.mapTrail[1])) nosignpost = true;
		}
		if (!nosignpost) scene.spriteset.addUserSprite(new LocationWindow(Game.GameData.game_map.name));
	}
)

//===============================================================================
// Event locations, terrain tags.
//===============================================================================
// NOTE: Assumes the event is 1x1 tile in size. Only returns one tile.
public void FacingTile(direction = null, event = null) {
	if (Game.GameData.map_factory) return Game.GameData.map_factory.getFacingTile(direction, event);
	return FacingTileRegular(direction, event);
}

// NOTE: Assumes the event is 1x1 tile in size. Only returns one tile.
public void FacingTileRegular(direction = null, event = null) {
	if (!event) event = Game.GameData.game_player;
	if (!event) return new {0, 0, 0};
	x = event.x;
	y = event.y;
	if (!direction) direction = event.direction;
	x_offset = new {0, -1, 0, 1, -1, 0, 1, -1, 0, 1}[direction];
	y_offset = new {0, 1, 1, 1, 0, 0, 0, -1, -1, -1}[direction];
	return new {Game.GameData.game_map.map_id, x + x_offset, y + y_offset};
}

// Returns whether event is in line with the player, is facing the player and is
// within distance tiles of the player.
public bool EventFacesPlayer(event, player, distance) {
	if (!event || !player || distance <= 0) return false;
	x_min = x_max = y_min = y_max = -1;
	switch (event.direction) {
		case 2:   // Down
			x_min = event.x;
			x_max = event.x + event.width - 1;
			y_min = event.y + 1;
			y_max = event.y + distance;
			break;
		case 4:   // Left
			x_min = event.x - distance;
			x_max = event.x - 1;
			y_min = event.y - event.height + 1;
			y_max = event.y;
			break;
		case 6:   // Right
			x_min = event.x + event.width;
			x_max = event.x + event.width - 1 + distance;
			y_min = event.y - event.height + 1;
			y_max = event.y;
			break;
		case 8:   // Up
			x_min = event.x;
			x_max = event.x + event.width - 1;
			y_min = event.y - event.height + 1 - distance;
			y_max = event.y - event.height;
			break;
		default:
			return false;
	}
	return player.x >= x_min && player.x <= x_max &&
				player.y >= y_min && player.y <= y_max;
}

// Returns whether event is able to walk up to the player.
public bool EventCanReachPlayer(event, player, distance) {
	if (event.map_id != player.map_id) return false;
	if (!EventFacesPlayer(event, player, distance)) return false;
	delta_x = (event.direction == 6) ? 1 : (event.direction == 4) ? -1 : 0;
	delta_y = (event.direction == 2) ? 1 : (event.direction == 8) ? -1 : 0;
	switch (event.direction) {
		case 2:   // Down
			real_distance = player.y - event.y - 1;
			break;
		case 4:   // Left
			real_distance = event.x - player.x - 1;
			break;
		case 6:   // Right
			real_distance = player.x - event.x - event.width;
			break;
		case 8:   // Up
			real_distance = event.y - event.height - player.y;
			break;
	}
	if (real_distance > 0) {
		for (int i = real_distance; i < real_distance; i++) { //for 'real_distance' times do => |i|
			if (!event.can_move_from_coordinate(event.x + (i * delta_x), event.y + (i * delta_y), event.direction)) return false;
		}
	}
	return true;
}

// Returns whether the two events are standing next to each other and facing each
// other.
public void FacingEachOther(event1, event2) {
	if (event1.map_id != event2.map_id) return false;
	return EventFacesPlayer(event1, event2, 1) && EventFacesPlayer(event2, event1, 1);
}

//===============================================================================
// Audio playing.
//===============================================================================
public void CueBGM(bgm, seconds, volume = null, pitch = null) {
	if (!bgm) return;
	bgm = ResolveAudioFile(bgm, volume, pitch);
	playingBGM = Game.GameData.game_system.playing_bgm;
	if (!playingBGM || playingBGM.name != bgm.name || playingBGM.pitch != bgm.pitch) {
		BGMFade(seconds);
		Game.GameData.game_temp.cue_bgm = bgm;
		if (!Game.GameData.game_temp.cue_bgm_delay) {
			Game.GameData.game_temp.cue_bgm_delay = seconds * 0.6;
			Game.GameData.game_temp.cue_bgm_timer_start = System.uptime;
		}
	} else if (playingBGM) {
		BGMPlay(bgm);
	}
}

public void AutoplayOnTransition() {
	surfbgm = GameData.Metadata.get.surf_BGM;
	if (Game.GameData.PokemonGlobal.surfing && surfbgm) {
		BGMPlay(surfbgm);
	} else {
		Game.GameData.game_map.autoplayAsCue;
	}
}

public void AutoplayOnSave() {
	surfbgm = GameData.Metadata.get.surf_BGM;
	if (Game.GameData.PokemonGlobal.surfing && surfbgm) {
		BGMPlay(surfbgm);
	} else {
		Game.GameData.game_map.autoplay;
	}
}

//===============================================================================
// Event movement.
//===============================================================================
public static partial class MoveRoute {
	public const int DOWN                  = 1;
	public const int LEFT                  = 2;
	public const int RIGHT                 = 3;
	public const int UP                    = 4;
	public const int LOWER_LEFT            = 5;
	public const int LOWER_RIGHT           = 6;
	public const int UPPER_LEFT            = 7;
	public const int UPPER_RIGHT           = 8;
	public const int RANDOM                = 9;
	public const int TOWARD_PLAYER         = 10;
	public const int AWAY_FROM_PLAYER      = 11;
	public const int FORWARD               = 12;
	public const int BACKWARD              = 13;
	public const int JUMP                  = 14;   // xoffset, yoffset
	public const int WAIT                  = 15;   // frames
	public const int TURN_DOWN             = 16;
	public const int TURN_LEFT             = 17;
	public const int TURN_RIGHT            = 18;
	public const int TURN_UP               = 19;
	public const int TURN_RIGHT90          = 20;
	public const int TURN_LEFT90           = 21;
	public const int TURN180               = 22;
	public const int TURN_RIGHT_OR_LEFT90  = 23;
	public const int TURN_RANDOM           = 24;
	public const int TURN_TOWARD_PLAYER    = 25;
	public const int TURN_AWAY_FROM_PLAYER = 26;
	public const int SWITCH_ON             = 27;   // 1 param
	public const int SWITCH_OFF            = 28;   // 1 param
	public const int CHANGE_SPEED          = 29;   // 1 param
	public const int CHANGE_FREQUENCY      = 30;   // 1 param
	public const int WALK_ANIME_ON         = 31;
	public const int WALK_ANIME_OFF        = 32;
	public const int STEP_ANIME_ON         = 33;
	public const int STEP_ANIME_OFF        = 34;
	public const int DIRECTION_FIX_ON      = 35;
	public const int DIRECTION_FIX_OFF     = 36;
	public const int THROUGH_ON            = 37;
	public const int THROUGH_OFF           = 38;
	public const int ALWAYS_ON_TOP_ON      = 39;
	public const int ALWAYS_ON_TOP_OFF     = 40;
	public const int GRAPHIC               = 41;   // Name, hue, direction, pattern
	public const int OPACITY               = 42;   // 1 param
	public const int BLENDING              = 43;   // 1 param
	public const int PLAY_SE               = 44;   // 1 param
	public const int SCRIPT                = 45;   // 1 param
	public const int SCRIPT_ASYNC          = 101;   // 1 param
}

public void MoveRoute(event, commands, waitComplete = false) {
	route = new RPG.MoveRoute();
	route.repeat    = false;
	route.skippable = true;
	route.list.clear;
	route.list.Add(new RPG.MoveCommand(MoveRoute.THROUGH_ON));
	i = 0;
	while (i < commands.length) {
		switch (commands[i]) {
			case MoveRoute.WAIT: case MoveRoute.SWITCH_ON: case MoveRoute.SWITCH_OFF:
				case MoveRoute.CHANGE_SPEED: case MoveRoute.CHANGE_FREQUENCY: case MoveRoute.OPACITY:
				case MoveRoute.BLENDING: case MoveRoute.PLAY_SE: case MoveRoute.SCRIPT:
				route.list.Add(new RPG.MoveCommand(commands[i], [commands[i + 1]]));
				i += 1;
				break;
			case MoveRoute.SCRIPT_ASYNC:
				route.list.Add(new RPG.MoveCommand(MoveRoute.SCRIPT, [commands[i + 1]]));
				route.list.Add(new RPG.MoveCommand(MoveRoute.WAIT, [0]));
				i += 1;
				break;
			case MoveRoute.JUMP:
				route.list.Add(new RPG.MoveCommand(commands[i], new {commands[i + 1], commands[i + 2]}));
				i += 2;
				break;
			case MoveRoute.GRAPHIC:
				route.list.Add(new RPG.MoveCommand(commands[i],
																						new {commands[i + 1], commands[i + 2],
																							commands[i + 3], commands[i + 4]}));
				i += 4;
				break;
			default:
				route.list.Add(new RPG.MoveCommand(commands[i]));
				break;
		}
		i += 1;
	}
	route.list.Add(new RPG.MoveCommand(MoveRoute.THROUGH_OFF));
	route.list.Add(new RPG.MoveCommand(0));
	event&.force_move_route(route);
	return route;
}

// duration is in seconds.
public void Wait(duration) {
	timer_start = System.uptime;
	until System.uptime - timer_start >= duration;
		if (block_given()) yield System.uptime - timer_start;
		Graphics.update;
		Input.update;
		UpdateSceneMap;
	}
}

//===============================================================================
// Player/event movement in the field.
//===============================================================================
public void SlideOnIce() {
	if (!Core.DEBUG || !Input.press(Input.CTRL)) {
		if (Game.GameData.game_player.TerrainTag.ice && Game.GameData.game_player.can_move_in_direction(Game.GameData.game_player.direction)) {
			Game.GameData.PokemonGlobal.ice_sliding = true;
			Game.GameData.game_player.straighten;
			Game.GameData.game_player.walk_anime = false;
			return;
		}
	}
	Game.GameData.PokemonGlobal.ice_sliding = false;
	Game.GameData.game_player.walk_anime = true;
}

public void TurnTowardEvent(event, otherEvent) {
	sx = 0;
	sy = 0;
	if (Game.GameData.map_factory) {
		relativePos = Game.GameData.map_factory.getThisAndOtherEventRelativePos(otherEvent, event);
		sx = relativePos[0];
		sy = relativePos[1];
	} else {
		sx = event.x - otherEvent.x;
		sy = event.y - otherEvent.y;
	}
	sx += (event.width - otherEvent.width) / 2.0;
	sy -= (event.height - otherEvent.height) / 2.0;
	if (sx == 0 && sy == 0) return;
	if (sx.abs > sy.abs) {
		(sx > 0) ? event.turn_left : event.turn_right
	} else {
		(sy > 0) ? event.turn_up : event.turn_down
	}
}

public void MoveTowardPlayer(event) {
	maxsize = (int)Math.Max(Game.GameData.game_map.width, Game.GameData.game_map.height);
	if (!EventCanReachPlayer(event, Game.GameData.game_player, maxsize)) return;
	do { //loop; while (true);
		x = event.x;
		y = event.y;
		event.move_toward_player;
		if (event.x == x && event.y == y) break;
		while (event.moving()) {
			Graphics.update;
			Input.update;
			UpdateSceneMap;
		}
	}
	Game.GameData.PokemonMap&.addMovedEvent(event.id);
}

//===============================================================================
// Bridges, cave escape points, and setting the heal point.
//===============================================================================
public void BridgeOn(height = 2) {
	Game.GameData.PokemonGlobal.bridge = height;
}

public void BridgeOff() {
	Game.GameData.PokemonGlobal.bridge = 0;
}

public void SetEscapePoint() {
	if (!Game.GameData.PokemonGlobal.escapePoint) Game.GameData.PokemonGlobal.escapePoint = new List<string>();
	xco = Game.GameData.game_player.x;
	yco = Game.GameData.game_player.y;
	switch (Game.GameData.game_player.direction) {
		case 2:   // Down
			yco -= 1;
			dir = 8;
			break;
		case 4:   // Left
			xco += 1;
			dir = 6;
			break;
		case 6:   // Right
			xco -= 1;
			dir = 4;
			break;
		case 8:   // Up
			yco += 1;
			dir = 2;
			break;
	}
	Game.GameData.PokemonGlobal.escapePoint = new {Game.GameData.game_map.map_id, xco, yco, dir};
}

public void EraseEscapePoint() {
	Game.GameData.PokemonGlobal.escapePoint = new List<string>();
}

public void SetPokemonCenter() {
	Game.GameData.PokemonGlobal.pokecenterMapId     = Game.GameData.game_map.map_id;
	Game.GameData.PokemonGlobal.pokecenterX         = Game.GameData.game_player.x;
	Game.GameData.PokemonGlobal.pokecenterY         = Game.GameData.game_player.y;
	Game.GameData.PokemonGlobal.pokecenterDirection = Game.GameData.game_player.direction;
}

//===============================================================================
// Partner trainer.
//===============================================================================
public void RegisterPartner(tr_type, tr_name, tr_id = 0) {
	tr_type = GameData.TrainerType.get(tr_type).id;
	CancelVehicles;
	trainer = LoadTrainer(tr_type, tr_name, tr_id);
	EventHandlers.trigger(:on_trainer_load, trainer);
	foreach (var i in trainer.party) { //'trainer.party.each' do => |i|
		i.owner = Pokemon.Owner.new_from_trainer(trainer);
		i.calc_stats;
	}
	Game.GameData.PokemonGlobal.partner = new {tr_type, tr_name, trainer.id, trainer.party};
}

public void DeregisterPartner() {
	Game.GameData.PokemonGlobal.partner = null;
}

//===============================================================================
// Picking up an item found on the ground.
//===============================================================================
public void ItemBall(item, quantity = 1) {
	item = GameData.Item.get(item);
	if (!item || quantity < 1) return false;
	itemname = (quantity > 1) ? item.portion_name_plural : item.portion_name;
	pocket = item.pocket;
	move = item.move;
	if (Game.GameData.bag.add(item, quantity)) {   // If item can be picked up
		meName = (item.is_key_item()) ? "Key item get" : "Item get";
		if (item == items.DNASPLICERS) {
			Message($"\\me[{meName}]" + _INTL("You found \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
		} else if (item.is_machine()) {   // TM or HM
			if (quantity > 1) {
				Message("\\me[Machine get]" + _INTL("You found {1} \\c[1]{2} {3}\\c[0]!",
																							quantity, itemname, GameData.Move.get(move).name) + "\\wtnp[70]");
			} else {
				Message("\\me[Machine get]" + _INTL("You found \\c[1]{1} {2}\\c[0]!",
																							itemname, GameData.Move.get(move).name) + "\\wtnp[70]");
			}
		} else if (quantity > 1) {
			Message($"\\me[{meName}]" + _INTL("You found {1} \\c[1]{2}\\c[0]!", quantity, itemname) + "\\wtnp[40]");
		} else if (itemname.starts_with_vowel()) {
			Message($"\\me[{meName}]" + _INTL("You found an \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
		} else {
			Message($"\\me[{meName}]" + _INTL("You found a \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
		}
		Message(_INTL("You put the {1} in\nyour Bag's <icon=bagPocket{2}>\\c[1]{3}\\c[0] pocket.",
										itemname, pocket, PokemonBag.pocket_names[pocket - 1]));
		return true;
	}
	// Can't add the item
	if (item.is_machine()) {   // TM or HM
		if (quantity > 1) {
			Message(_INTL("You found {1} \\c[1]{2} {3}\\c[0]!", quantity, itemname, GameData.Move.get(move).name));
		} else {
			Message(_INTL("You found \\c[1]{1} {2}\\c[0]!", itemname, GameData.Move.get(move).name));
		}
	} else if (quantity > 1) {
		Message(_INTL("You found {1} \\c[1]{2}\\c[0]!", quantity, itemname));
	} else if (itemname.starts_with_vowel()) {
		Message(_INTL("You found an \\c[1]{1}\\c[0]!", itemname));
	} else {
		Message(_INTL("You found a \\c[1]{1}\\c[0]!", itemname));
	}
	Message(_INTL("But your Bag is full..."));
	return false;
}

//===============================================================================
// Being given an item.
//===============================================================================
public void ReceiveItem(item, quantity = 1) {
	item = GameData.Item.get(item);
	if (!item || quantity < 1) return false;
	itemname = (quantity > 1) ? item.portion_name_plural : item.portion_name;
	pocket = item.pocket;
	move = item.move;
	meName = (item.is_key_item()) ? "Key item get" : "Item get";
	if (item == items.DNASPLICERS) {
		Message($"\\me[{meName}]" + _INTL("You obtained \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
	} else if (item.is_machine()) {   // TM or HM
		if (quantity > 1) {
			Message("\\me[Machine get]" + _INTL("You obtained {1} \\c[1]{2} {3}\\c[0]!",
																						quantity, itemname, GameData.Move.get(move).name) + "\\wtnp[70]");
		} else {
			Message("\\me[Machine get]" + _INTL("You obtained \\c[1]{1} {2}\\c[0]!",
																						itemname, GameData.Move.get(move).name) + "\\wtnp[70]");
		}
	} else if (quantity > 1) {
		Message($"\\me[{meName}]" + _INTL("You obtained {1} \\c[1]{2}\\c[0]!", quantity, itemname) + "\\wtnp[40]");
	} else if (itemname.starts_with_vowel()) {
		Message($"\\me[{meName}]" + _INTL("You obtained an \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
	} else {
		Message($"\\me[{meName}]" + _INTL("You obtained a \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
	}
	if (Game.GameData.bag.add(item, quantity)) {   // If item can be added
		Message(_INTL("You put the {1} in\nyour Bag's <icon=bagPocket{2}>\\c[1]{3}\\c[0] pocket.",
										itemname, pocket, PokemonBag.pocket_names[pocket - 1]));
		return true;
	}
	return false;   // Can't add the item
}

//===============================================================================
// Buying a prize item from the Game Corner.
//===============================================================================
public void BuyPrize(item, quantity = 1) {
	item = GameData.Item.get(item);
	if (!item || quantity < 1) return false;
	item_name = (quantity > 1) ? item.portion_name_plural : item.portion_name;
	pocket = item.pocket;
	if (!Game.GameData.bag.add(item, quantity)) return false;
	Message("\\CN" + _INTL("You put the {1} in\nyour Bag's <icon=bagPocket{2}>\\c[1]{3}\\c[0] pocket.",
													item_name, pocket, PokemonBag.pocket_names[pocket - 1]));
	return true;
}
