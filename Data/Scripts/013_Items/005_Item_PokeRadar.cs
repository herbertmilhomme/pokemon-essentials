//===============================================================================
//
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int pokeradarBattery		{ get { return _pokeradarBattery; } set { _pokeradarBattery = value; } }			protected int _pokeradarBattery;
}

//===============================================================================
//
//===============================================================================
public partial class Game_Temp {
	/// <summary>[species, level, chain count, grasses (x,y,ring,rarity)]</summary>
	public int poke_radar_data		{ get { return _poke_radar_data; } set { _poke_radar_data = value; } }			protected int _poke_radar_data;
}

//===============================================================================
// Using the Poke Radar.
//===============================================================================
public bool CanUsePokeRadar() {
	// Can't use Radar if not in tall grass
	terrain = Game.GameData.game_map.terrain_tag(Game.GameData.game_player.x, Game.GameData.game_player.y);
	if (!terrain.land_wild_encounters || !terrain.shows_grass_rustle) {
		Message(_INTL("Can't use that here."));
		return false;
	}
	// Can't use Radar if map has no grass-based encounters (ignoring Bug Contest)
	if (!Game.GameData.PokemonEncounters.has_normal_land_encounters()) {
		Message(_INTL("Can't use that here."));
		return false;
	}
	// Can't use Radar while cycling
	if (Game.GameData.PokemonGlobal.bicycle) {
		Message(_INTL("Can't use that while on a bicycle."));
		return false;
	}
	// Debug
	if (Core.DEBUG && Input.press(Input.CTRL)) return true;
	// Can't use Radar if it isn't fully charged
	if (Game.GameData.PokemonGlobal.pokeradarBattery && Game.GameData.PokemonGlobal.pokeradarBattery > 0) {
		Message(_INTL("The battery has run dry!\nFor it to recharge, you need to walk another {1} steps.",
										Game.GameData.PokemonGlobal.pokeradarBattery));
		return false;
	}
	return true;
}

public void UsePokeRadar() {
	if (!CanUsePokeRadar()) return false;
	Game.GameData.stats.poke_radar_count += 1;
	if (!Game.GameData.game_temp.poke_radar_data) Game.GameData.game_temp.poke_radar_data = new {null, 0, 0, new List<string>(), false};
	Game.GameData.game_temp.poke_radar_data[4] = false;
	Game.GameData.PokemonGlobal.pokeradarBattery = 50;
	PokeRadarHighlightGrass;
	return true;
}

public void PokeRadarCancel() {
	Game.GameData.game_temp.poke_radar_data = null;
}

public void PokeRadarHighlightGrass(showmessage = true) {
	grasses = new List<string>();   // x, y, ring (0-3 inner to outer), rarity
	// Choose 1 random tile from each ring around the player
	for (int i = 4; i < 4; i++) { //for '4' times do => |i|
		r = rand((i + 1) * 8);
		// Get coordinates of randomly chosen tile
		x = Game.GameData.game_player.x;
		y = Game.GameData.game_player.y;
		if (r <= (i + 1) * 2) {
			x = Game.GameData.game_player.x - i - 1 + r;
			y = Game.GameData.game_player.y - i - 1;
		} else if (r <= ((i + 1) * 6) - 2) {
			x = new {Game.GameData.game_player.x + i + 1, Game.GameData.game_player.x - i - 1}[r % 2];
			y = Game.GameData.game_player.y - i + (int)Math.Floor((r - 1 - ((i + 1) * 2)) / 2);
		} else {
			x = Game.GameData.game_player.x - i + r - ((i + 1) * 6);
			y = Game.GameData.game_player.y + i + 1;
		}
		// Add tile to grasses array if it's a valid grass tile
		if (x < 0 || x >= Game.GameData.game_map.width ||
						y < 0 || y >= Game.GameData.game_map.height) continue;
		terrain = Game.GameData.game_map.terrain_tag(x, y);
		if (!terrain.land_wild_encounters || !terrain.shows_grass_rustle) continue;
		// Choose a rarity for the grass (0=normal, 1=rare, 2=shiny)
		s = (rand(100) < 25) ? 1 : 0;
		if (Game.GameData.game_temp.poke_radar_data && Game.GameData.game_temp.poke_radar_data[2] > 0) {
			v = (int)Math.Max((65_536 / Settings.SHINY_POKEMON_CHANCE) - ((int)Math.Min(Game.GameData.game_temp.poke_radar_data[2], 40) * 200), 200);
			v = (65_536 / v.to_f).ceil;
			if (rand(65_536) < v) s = 2;
		}
		grasses.Add(new {x, y, i, s});
	}
	if (grasses.length == 0) {
		// No shaking grass found, break the chain
		if (showmessage) Message(_INTL("The grassy patch remained quiet..."));
		PokeRadarCancel;
	} else {
		// Show grass rustling animations
		foreach (var grass in grasses) { //'grasses.each' do => |grass|
			switch (grass[3]) {
				case 0:   // Normal rustle
					Game.GameData.scene.spriteset.addUserAnimation(Settings.RUSTLE_NORMAL_ANIMATION_ID, grass[0], grass[1], true, 1);
					break;
				case 1:   // Vigorous rustle
					Game.GameData.scene.spriteset.addUserAnimation(Settings.RUSTLE_VIGOROUS_ANIMATION_ID, grass[0], grass[1], true, 1);
					break;
				case 2:   // Shiny rustle
					Game.GameData.scene.spriteset.addUserAnimation(Settings.RUSTLE_SHINY_ANIMATION_ID, grass[0], grass[1], true, 1);
					break;
			}
		}
		if (Game.GameData.game_temp.poke_radar_data) Game.GameData.game_temp.poke_radar_data[3] = grasses;
		Wait(0.5);
	}
}

public void PokeRadarGetShakingGrass() {
	if (!Game.GameData.game_temp.poke_radar_data) return -1;
	grasses = Game.GameData.game_temp.poke_radar_data[3];
	if (grasses.length == 0) return -1;
	foreach (var i in grasses) { //'grasses.each' do => |i|
		if (Game.GameData.game_player.x == i[0] && Game.GameData.game_player.y == i[1]) return i[2];
	}
	return -1;
}

public void PokeRadarOnShakingGrass() {
	return PokeRadarGetShakingGrass >= 0;
}

public void PokeRadarGetEncounter(rarity = 0) {
	// Poké Radar-exclusive encounters can only be found in vigorously-shaking grass
	if (rarity > 0) {
		// Get all Poké Radar-exclusive encounters for this map
		map_id = Game.GameData.game_map.map_id;
		enc_list = null;
		encounter_data = GameData.Encounter.get(map_id, Game.GameData.PokemonGlobal.encounter_version);
		if (encounter_data && encounter_data.types[:PokeRadar] &&
			rand(100) < encounter_data.step_chances[:PokeRadar]) {
			enc_list = encounter_data.types[:PokeRadar];
		}
		// If there are any exclusives, first have a chance of encountering those
		if (enc_list && enc_list.length > 0) {
			chance_total = 0;
			enc_list.each(a => chance_total += a[0]);
			rnd = rand(chance_total);
			encounter = null;
			foreach (var enc in enc_list) { //'enc_list.each' do => |enc|
				rnd -= enc[0];
				if (rnd >= 0) continue;
				encounter = enc;
				break;
			}
			level = rand(encounter[2]..encounter[3]);
			return new {encounter[1], level};
		}
	}
	// Didn't choose a Poké Radar-exclusive species, choose a regular encounter instead
	return Game.GameData.PokemonEncounters.choose_wild_pokemon(Game.GameData.PokemonEncounters.encounter_type, rarity + 1);
}

//===============================================================================
// Event handlers.
//===============================================================================

EventHandlers.add(:on_wild_species_chosen, :poke_radar_chain,
	block: (encounter) => {
		if (GameData.EncounterType.get(Game.GameData.game_temp.encounter_type).type != types.land ||
			Game.GameData.PokemonGlobal.bicycle || Game.GameData.PokemonGlobal.partner) {
			PokeRadarCancel;
			continue;
		}
		ring = PokeRadarGetShakingGrass;
		if (ring >= 0) {   // Encounter triggered by stepping into rustling grass
			// Get rarity of shaking grass
			rarity = 0;   // 0 = rustle, 1 = vigorous rustle, 2 = shiny rustle
			Game.GameData.game_temp.poke_radar_data[3].each(g => { if (g[2] == ring) rarity = g[3]; });
			if (Game.GameData.game_temp.poke_radar_data[2] > 0) {   // Chain count, i.e. is chaining
				chain_chance = 58 + (ring * 10);
				chain_chance += (int)Math.Min(Game.GameData.game_temp.poke_radar_data[2], 40) / 4;   // Chain length
				if (Game.GameData.game_temp.poke_radar_data[4]) chain_chance += 10;   // Previous in chain was caught
				if (rarity == 2 || rand(100) < chain_chance) {
					// Continue the chain
					encounter[0] = Game.GameData.game_temp.poke_radar_data[0];   // Species
					encounter[1] = Game.GameData.game_temp.poke_radar_data[1];   // Level
					Game.GameData.game_temp.force_single_battle = true;
				} else {
					// Break the chain, force an encounter with a different species
					100.times do;
						if (encounter && encounter[0] != Game.GameData.game_temp.poke_radar_data[0]) break;
						new_encounter = Game.GameData.PokemonEncounters.choose_wild_pokemon(Game.GameData.PokemonEncounters.encounter_type);
						encounter[0] = new_encounter[0];
						encounter[1] = new_encounter[1];
					}
					if (encounter[0] == Game.GameData.game_temp.poke_radar_data[0] && encounter[1] == Game.GameData.game_temp.poke_radar_data[1]) {
						// Chain couldn't be broken somehow; continue it after all
						Game.GameData.game_temp.force_single_battle = true;
					} else {
						PokeRadarCancel;
					}
				}
			} else {   // Not chaining; will start one
				// Force random wild encounter, vigorous shaking means rarer species
				new_encounter = PokeRadarGetEncounter(rarity);
				encounter[0] = new_encounter[0];
				encounter[1] = new_encounter[1];
				Game.GameData.game_temp.force_single_battle = true;
			}
		} else if (encounter) {   // Encounter triggered by stepping in non-rustling grass
			PokeRadarCancel;
		}
	}
)

EventHandlers.add(:on_wild_pokemon_created, :poke_radar_shiny,
	block: (pkmn) => {
		if (!Game.GameData.game_temp.poke_radar_data) continue;
		grasses = Game.GameData.game_temp.poke_radar_data[3];
		if (!grasses) continue;
		foreach (var grass in grasses) { //'grasses.each' do => |grass|
			if (Game.GameData.game_player.x != grass[0] || Game.GameData.game_player.y != grass[1]) continue;
			if (grass[3] == 2) pkmn.shiny = true;
			break;
		}
	}
)

EventHandlers.add(:on_wild_battle_end, :poke_radar_continue_chain,
	block: (species, level, outcome) => {
		if (Game.GameData.game_temp.poke_radar_data && new []{Battle.Outcome.WIN, Battle.Outcome.CATCH}.Contains(outcome)) {
			Game.GameData.game_temp.poke_radar_data[0] = species;
			Game.GameData.game_temp.poke_radar_data[1] = level;
			Game.GameData.game_temp.poke_radar_data[2] += 1;
			Game.GameData.stats.poke_radar_longest_chain = (int)Math.Max(Game.GameData.game_temp.poke_radar_data[2], Game.GameData.stats.poke_radar_longest_chain);
			// Catching makes the next Radar encounter more likely to continue the chain
			Game.GameData.game_temp.poke_radar_data[4] = (outcome == Battle.Outcome.CATCH);
			PokeRadarHighlightGrass(false);
		} else {
			PokeRadarCancel;
		}
	}
)

EventHandlers.add(:on_player_step_taken, :poke_radar,
	block: () => {
		if (Game.GameData.PokemonGlobal.pokeradarBattery && Game.GameData.PokemonGlobal.pokeradarBattery > 0 &&
			!Game.GameData.game_temp.poke_radar_data) {
			Game.GameData.PokemonGlobal.pokeradarBattery -= 1;
		}
		terrain = Game.GameData.game_map.terrain_tag(Game.GameData.game_player.x, Game.GameData.game_player.y);
		if (!terrain.land_wild_encounters || !terrain.shows_grass_rustle) {
			PokeRadarCancel;
		}
	}
)

EventHandlers.add(:on_enter_map, :cancel_poke_radar,
	block: (_old_map_id) => {
		PokeRadarCancel;
	}
)

//===============================================================================
// Item handlers.
//===============================================================================

ItemHandlers.UseInField.add(:POKERADAR, block: (item) => {
	next UsePokeRadar;
});

ItemHandlers.UseFromBag.add(:POKERADAR, block: (item) => {
	next (CanUsePokeRadar()) ? 2 : 0;
});
