//===============================================================================
// Battle preparation.
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int nextBattleBGM		{ get { return _nextBattleBGM; } set { _nextBattleBGM = value; } }			protected int _nextBattleBGM;
	public int nextBattleVictoryBGM		{ get { return _nextBattleVictoryBGM; } set { _nextBattleVictoryBGM = value; } }			protected int _nextBattleVictoryBGM;
	public int nextBattleCaptureME		{ get { return _nextBattleCaptureME; } set { _nextBattleCaptureME = value; } }			protected int _nextBattleCaptureME;
	public int nextBattleBack		{ get { return _nextBattleBack; } set { _nextBattleBack = value; } }			protected int _nextBattleBack;
}

//===============================================================================
//
//===============================================================================
public partial class Game_Temp {
	public int encounter_triggered		{ get { return _encounter_triggered; } set { _encounter_triggered = value; } }			protected int _encounter_triggered;
	public int encounter_type		{ get { return _encounter_type; } set { _encounter_type = value; } }			protected int _encounter_type;
	public int party_levels_before_battle		{ get { return _party_levels_before_battle; } set { _party_levels_before_battle = value; } }			protected int _party_levels_before_battle;

	public void battle_rules() {
		if (!@battle_rules) @battle_rules = new List<string>();
		return @battle_rules;
	}

	public void clear_battle_rules() {
		self.battle_rules.clear;
	}

	public void add_battle_rule(rule, var = null) {
		rules = self.battle_rules;
		switch (rule.ToString().downcase) {
			case "single": case "1v1": case "1v2": case "2v1": case "1v3": case "3v1":
					case "double": case "2v2": case "2v3": case "3v2": case "triple": case "3v3":
				rules["size"] = rule.ToString().downcase;
				break;
			case "canlose":                 rules["canLose"]             = true; break;
			case "cannotlose":              rules["canLose"]             = false; break;
			case "canrun":                  rules["canRun"]              = true; break;
			case "cannotrun":               rules["canRun"]              = false; break;
			case "roamerflees":             rules["roamerFlees"]         = true; break;
			case "canswitch":               rules["canSwitch"]           = true; break;
			case "cannotswitch":            rules["canSwitch"]           = false; break;
			case "noexp":                   rules["expGain"]             = false; break;
			case "nomoney":                 rules["moneyGain"]           = false; break;
			case "disablepokeballs":        rules["disablePokeBalls"]    = true; break;
			case "forcecatchintoparty":     rules["forceCatchIntoParty"] = true; break;
			case "switchstyle":             rules["switchStyle"]         = true; break;
			case "setstyle":                rules["switchStyle"]         = false; break;
			case "anims":                   rules["battleAnims"]         = true; break;
			case "noanims":                 rules["battleAnims"]         = false; break;
			case "terrain":
				rules["defaultTerrain"] = GameData.BattleTerrain.try_get(var)&.id;
				break;
			case "weather":
				rules["defaultWeather"] = GameData.BattleWeather.try_get(var)&.id;
				break;
			case "environment": case "environ":
				rules["environment"] = GameData.Environment.try_get(var)&.id;
				break;
			case "backdrop": case "battleback": rules["backdrop"]            = var; break;
			case "base":                    rules["base"]                = var; break;
			case "outcome": case "outcomevar": rules["outcomeVar"]          = var; break;
			case "nopartner":               rules["noPartner"]           = true; break;
			default:
				Debug.LogError(_INTL("Battle rule \"{1}\" does not exist.", rule));
				//throw new ArgumentException(_INTL("Battle rule \"{1}\" does not exist.", rule));
				break;
		}
	}
}

//===============================================================================
//
//===============================================================================
public void setBattleRule(*args) {
	r = null;
	foreach (var arg in args) { //'args.each' do => |arg|
		if r
			Game.GameData.game_temp.add_battle_rule(r, arg);
			r = null;
		} else {
			switch (arg.downcase) {
				case "terrain": case "weather": case "environment": case "environ": case "backdrop":
						case "battleback": case "base": case "outcome": case "outcomevar":
					r = arg;
					continue;
					break;
			}
			Game.GameData.game_temp.add_battle_rule(arg);
		}
	}
	if (r) raise _INTL("Argument {1} expected a variable after it but didn't have one.", r);
}

// Used to determine the environment in battle, and also the form of Burmy/
// Wormadam.
public void GetEnvironment() {
	ret = :None;
	map_env = Game.GameData.game_map.metadata&.battle_environment;
	if (map_env) ret = map_env;
	if (Game.GameData.game_temp.encounter_type &&
		GameData.EncounterType.get(Game.GameData.game_temp.encounter_type).type == types.fishing) {
		terrainTag = Game.GameData.game_player.FacingTerrainTag;
	} else {
		terrainTag = Game.GameData.game_player.terrain_tag;
	}
	tile_environment = terrainTag.battle_environment;
	if (ret == :Forest && new []{:Grass, :TallGrass}.Contains(tile_environment)) {
		ret = :ForestGrass;
	} else if (tile_environment) {
		ret = tile_environment;
	}
	return ret;
}

// Record current levels of Pokémon in party, to see if they gain a level during
// battle and may need to evolve afterwards
EventHandlers.add(:on_start_battle, :record_party_status,
	block: () => {
		Game.GameData.game_temp.party_levels_before_battle = new List<string>();
		Game.GameData.player.party.each_with_index do |pkmn, i|
			Game.GameData.game_temp.party_levels_before_battle[i] = pkmn.level;
		}
	}
)

public bool CanDoubleBattle() {
	if (Game.GameData.player.able_pokemon_count >= 2) return true;
	return Game.GameData.PokemonGlobal.partner && Game.GameData.player.able_pokemon_count >= 1;
}

public bool CanTripleBattle() {
	if (Game.GameData.player.able_pokemon_count >= 3) return true;
	return Game.GameData.PokemonGlobal.partner && Game.GameData.player.able_pokemon_count >= 2;
}

//===============================================================================
// Helper methods for setting up and closing down battles.
//===============================================================================
public static partial class BattleCreationHelperMethods {
	#region Class Functions
	#endregion

	// Skip battle if the player has no able Pokémon, or if holding Ctrl in Debug mode
	public bool skip_battle() {
		if (Game.GameData.player.able_pokemon_count == 0) return true;
		if (Core.DEBUG && Input.press(Input.CTRL)) return true;
		return false;
	}

	public void skip_battle(outcome_variable, trainer_battle = false) {
		if (!trainer_battle && Game.GameData.player.pokemon_count > 0) Message(_INTL("SKIPPING BATTLE..."));
		if (trainer_battle && Core.DEBUG) Message(_INTL("SKIPPING BATTLE..."));
		if (trainer_battle && Game.GameData.player.able_pokemon_count > 0) Message(_INTL("AFTER WINNING..."));
		Game.GameData.game_temp.clear_battle_rules;
		if (Game.GameData.game_temp.memorized_bgm && Game.GameData.game_system.is_a(Game_System)) {
			Game.GameData.game_system.bgm_pause;
			Game.GameData.game_system.bgm_position = Game.GameData.game_temp.memorized_bgm_position;
			Game.GameData.game_system.bgm_resume(Game.GameData.game_temp.memorized_bgm);
		}
		Game.GameData.game_temp.memorized_bgm            = null;
		Game.GameData.game_temp.memorized_bgm_position   = 0;
		Game.GameData.PokemonGlobal.nextBattleBGM        = null;
		Game.GameData.PokemonGlobal.nextBattleVictoryBGM = null;
		Game.GameData.PokemonGlobal.nextBattleCaptureME  = null;
		Game.GameData.PokemonGlobal.nextBattleBack       = null;
		Game.GameData.PokemonEncounters.reset_step_count;
		outcome = Battle.Outcome.WIN;
		if (trainer_battle && Game.GameData.player.all_fainted()) outcome = Battle.Outcome.UNDECIDED;
		Set(outcome_variable, outcome);
		return outcome;
	}

	public bool partner_can_participate(foe_party) {
		if (!Game.GameData.PokemonGlobal.partner || Game.GameData.game_temp.battle_rules["noPartner"]) return false;
		if (foe_party.length > 1) return true;
		if (Game.GameData.game_temp.battle_rules["size"]) {
			if (Game.GameData.game_temp.battle_rules["size"] == "single" ||
											Game.GameData.game_temp.battle_rules["size"System.Text.RegularExpressions.Regex.IsMatch(],@"^1v",RegexOptions.IgnoreCase)) return false;   // "1v1", "1v2", "1v3", etc.
			return true;
		}
		return false;
	}

	// Generate information for the player and partner trainer(s)
	public void set_up_player_trainers(foe_party) {
		trainer_array = [Game.GameData.player];
		ally_items    = new List<string>();
		pokemon_array = Game.GameData.player.party;
		party_starts  = [0];
		if (partner_can_participate(foe_party)) {
			ally = new NPCTrainer(Game.GameData.PokemonGlobal.partner[1], Game.GameData.PokemonGlobal.partner[0]);
			ally.id    = Game.GameData.PokemonGlobal.partner[2];
			ally.party = Game.GameData.PokemonGlobal.partner[3];
			data = GameData.Trainer.try_get(Game.GameData.PokemonGlobal.partner[0], Game.GameData.PokemonGlobal.partner[1], Game.GameData.PokemonGlobal.partner[2]);
			ally_items[1] = data&.items.clone || [];
			trainer_array.Add(ally);
			pokemon_array = new List<string>();
			Game.GameData.player.party.each(pkmn => pokemon_array.Add(pkmn));
			party_starts.Add(pokemon_array.length);
			ally.party.each(pkmn => pokemon_array.Add(pkmn));
			if (Game.GameData.game_temp.battle_rules["size"].null()) setBattleRule("double");
		}
		return trainer_array, ally_items, pokemon_array, party_starts;
	}

	public void create_battle_scene() {
		return new Battle.Scene();
	}

	// Sets up various battle parameters and applies special rules.
	public void prepare_battle(battle) {
		battleRules = Game.GameData.game_temp.battle_rules;
		// The size of the battle, i.e. how many Pokémon on each side (default: "single")
		if (!battleRules["size"].null()) battle.setBattleMode(battleRules["size"]);
		// Whether the game won't black out even if the player loses (default: false)
		if (!battleRules["canLose"].null()) battle.canLose = battleRules["canLose"];
		// Whether the player can choose to run from the battle (default: true)
		if (!battleRules["canRun"].null()) battle.canRun = battleRules["canRun"];
		// Whether the player can manually choose to switch out Pokémon (default: true)
		if (!battleRules["canSwitch"].null()) battle.canSwitch = battleRules["canSwitch"];
		// Whether wild Pokémon always try to run from battle (default: null)
		battle.rules["alwaysflee"] = battleRules["roamerFlees"];
		// Whether Pokémon gain Exp/EVs from defeating/catching a Pokémon (default: true)
		if (!battleRules["expGain"].null()) battle.expGain = battleRules["expGain"];
		// Whether the player gains/loses money at the end of the battle (default: true)
		if (!battleRules["moneyGain"].null()) battle.moneyGain = battleRules["moneyGain"];
		// Whether Poké Balls cannot be thrown at all
		if (!battleRules["disablePokeBalls"].null()) battle.disablePokeBalls = battleRules["disablePokeBalls"];
		// Whether the player is asked what to do with a new Pokémon when their party is full
		if (Settings.NEW_CAPTURE_CAN_REPLACE_PARTY_MEMBER) battle.sendToBoxes = Game.GameData.PokemonSystem.sendtoboxes;
		if (battleRules["forceCatchIntoParty"]) battle.sendToBoxes = 2;
		// Whether the player is able to switch when an opponent's Pokémon faints
		battle.switchStyle = (Game.GameData.PokemonSystem.battlestyle == 0);
		if (!battleRules["switchStyle"].null()) battle.switchStyle = battleRules["switchStyle"];
		// Whether battle animations are shown
		battle.showAnims = (Game.GameData.PokemonSystem.battlescene == 0);
		if (!battleRules["battleAnims"].null()) battle.showAnims = battleRules["battleAnims"];
		// Terrain
		if (battleRules["defaultTerrain"].null()) {
			if (Settings.OVERWORLD_WEATHER_SETS_BATTLE_TERRAIN) {
				switch (Game.GameData.game_screen.weather_type) {
					case :Storm:
						battle.defaultTerrain = :Electric;
						break;
					case :Fog:
						battle.defaultTerrain = :Misty;
						break;
				}
			}
		} else {
			battle.defaultTerrain = battleRules["defaultTerrain"];
		}
		// Weather
		if (battleRules["defaultWeather"].null()) {
			switch (GameData.Weather.get(Game.GameData.game_screen.weather_type).category) {
				case :Rain, :Storm:
					battle.defaultWeather = Weathers.Rain;
					break;
				case :Hail:
					battle.defaultWeather = (Settings.USE_SNOWSTORM_WEATHER_INSTEAD_OF_HAIL ? :Snowstorm : :Hail);
					break;
				case :Sandstorm:
					battle.defaultWeather = Weathers.Sandstorm;
					break;
				case :Sun:
					battle.defaultWeather = Weathers.Sun;
					break;
			}
		} else {
			battle.defaultWeather = battleRules["defaultWeather"];
		}
		// Environment
		if (battleRules["environment"].null()) {
			battle.environment = GetEnvironment;
		} else {
			battle.environment = battleRules["environment"];
		}
		// Backdrop graphic filename
		if (!battleRules["backdrop"].null()) {
			backdrop = battleRules["backdrop"];
		} else if (Game.GameData.PokemonGlobal.nextBattleBack) {
			backdrop = Game.GameData.PokemonGlobal.nextBattleBack;
		} else if (Game.GameData.PokemonGlobal.surfing) {
			backdrop = "water";   // This applies wherever you are, including in caves
		} else if (Game.GameData.game_map.metadata) {
			back = Game.GameData.game_map.metadata.battle_background;
			if (back && back != "") backdrop = back;
		}
		if (!backdrop) backdrop = "indoor1";
		battle.backdrop = backdrop;
		// Choose a name for bases depending on environment
		if (battleRules["base"].null()) {
			environment_data = GameData.Environment.try_get(battle.environment);
			if (environment_data) base = environment_data.battle_base;
		} else {
			base = battleRules["base"];
		}
		if (base) battle.backdropBase = base;
		// Time of day
		if (Game.GameData.game_map.metadata&.battle_environment == :Cave) {
			battle.time = 2;   // This makes Dusk Balls work properly in caves
		} else if (Settings.TIME_SHADING) {
			timeNow = GetTimeNow;
			if (DayNight.isNight(timeNow)) {
				battle.time = 2;
			} else if (DayNight.isEvening(timeNow)) {
				battle.time = 1;
			} else {
				battle.time = 0;
			}
		}
	}

	public void after_battle(outcome, can_lose) {
		foreach (var pkmn in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |pkmn|
			if (pkmn.status == statuses.POISON) pkmn.statusCount = 0;   // Bad poison becomes regular
			pkmn.makeUnmega;
			pkmn.makeUnprimal;
		}
		if (Game.GameData.PokemonGlobal.partner) {
			Game.GameData.player.heal_party;
			foreach (var pkmn in Game.GameData.PokemonGlobal.partner[3]) { //'Game.GameData.PokemonGlobal.partner[3].each' do => |pkmn|
				pkmn.heal;
				pkmn.makeUnmega;
				pkmn.makeUnprimal;
			}
		}
		if (Battle.Outcome.should_black_out(outcome) && can_lose) {
			Game.GameData.player.party.each(pkmn => pkmn.heal);
			timer_start = System.uptime;
			until System.uptime - timer_start >= 0.25;
				Graphics.update;
			}
		}
		EventHandlers.trigger(:on_end_battle, outcome, can_lose);
		Game.GameData.game_player.straighten;
	}

	// Save the result of the battle in a Game Variable (1 by default)
	//    0 - Undecided or aborted
	//    1 - Player won
	//    2 - Player lost
	//    3 - Player or wild Pokémon ran from battle, or player forfeited the match
	//    4 - Wild Pokémon was caught
	//    5 - Draw
	public void set_outcome(outcome, outcome_variable = 1, trainer_battle = false) {
		switch (outcome) {
			case Battle.Outcome.WIN: case Battle.Outcome.CATCH:
				if (!trainer_battle) Game.GameData.stats.wild_battles_won += 1;
				if (trainer_battle) Game.GameData.stats.trainer_battles_won += 1;
				break;
			case Battle.Outcome.LOSE: case Battle.Outcome.FLEE: case Battle.Outcome.DRAW:
				if (!trainer_battle) Game.GameData.stats.wild_battles_lost += 1;
				if (trainer_battle) Game.GameData.stats.trainer_battles_lost += 1;
				break;
		}
		Set(outcome_variable, outcome);
	}
}

//===============================================================================
// Wild battles.
//===============================================================================
public partial class WildBattle {
	// Used when walking in tall grass, hence the additional code.
	public static void start(*args, can_override: false) {
		foe_party = WildBattle.generate_foes(*args);
		// Potentially call a different WildBattle.start-type method instead (for
		// roaming Pokémon, Safari battles, Bug Contest battles)
		if (foe_party.length == 1 && can_override) {
			handled = [null];
			EventHandlers.trigger(:on_calling_wild_battle, foe_party[0], handled);
			if (!handled[0].null()) return handled[0];
		}
		// Perform the battle
		outcome = WildBattle.start_core(*foe_party);
		// Used by the Poké Radar to update/break the chain
		if (foe_party.length == 1 && can_override) {
			EventHandlers.trigger(:on_wild_battle_end, foe_party[0].species, foe_party[0].level, outcome);
		}
		// Return false if the player lost or drew the battle, and true if any other result
		return !Battle.Outcome.should_black_out(outcome);
	}

	public static void start_core(*args) {
		outcome_variable = Game.GameData.game_temp.battle_rules["outcomeVar"] || 1;
		can_lose         = Game.GameData.game_temp.battle_rules["canLose"] || false;
		// Skip battle if the player has no able Pokémon, or if holding Ctrl in Debug mode
		if (BattleCreationHelperMethods.skip_battle()) {
			return BattleCreationHelperMethods.skip_battle(outcome_variable);
		}
		// Record information about party Pokémon to be used at the end of battle
		// (e.g. comparing levels for an evolution check)
		EventHandlers.trigger(:on_start_battle);
		// Generate array of foes
		foe_party = WildBattle.generate_foes(*args);
		// Generate information for the player and partner trainer(s)
		player_trainers, ally_items, player_party, player_party_starts = BattleCreationHelperMethods.set_up_player_trainers(foe_party);
		// Create the battle scene (the visual side of it)
		scene = BattleCreationHelperMethods.create_battle_scene;
		// Create the battle class (the mechanics side of it)
		battle = new Battle(scene, player_party, foe_party, player_trainers, null);
		battle.party1starts = player_party_starts;
		battle.ally_items   = ally_items;
		// Set various other properties in the battle class
		if (Game.GameData.game_temp.battle_rules["size"].null()) setBattleRule($"{foe_party.length}v{foe_party.length}");
		BattleCreationHelperMethods.prepare_battle(battle);
		Game.GameData.game_temp.clear_battle_rules;
		// Perform the battle itself
		outcome = Battle.Outcome.UNDECIDED;
		BattleAnimation(GetWildBattleBGM(foe_party), (foe_party.length == 1) ? 0 : 2, foe_party) do;
			SceneStandby { outcome = battle.StartBattle };
			BattleCreationHelperMethods.after_battle(outcome, can_lose);
		}
		Input.update;
		// Save the result of the battle in a Game Variable (1 by default)
		BattleCreationHelperMethods.set_outcome(outcome, outcome_variable);
		return outcome;
	}

	public static void generate_foes(*args) {
		ret = new List<string>();
		species_id = null;
		foreach (var arg in args) { //'args.each' do => |arg|
			switch (arg) {
				case Pokemon:
					if (species_id) raise _INTL("Species {1} was given but not a level.", species_id);
					ret.Add(arg);
					break;
				case Array:
					if (species_id) raise _INTL("Species {1} was given but not a level.", species_id);
					species = GameData.Species.get(arg[0]).id;
					pkmn = GenerateWildPokemon(species, arg[1]);
					ret.Add(pkmn);
					break;
				default:
					if (species_id) {   // Expecting level
						if (!arg.is_a(Integer) || !(1..GameData.GrowthRate.max_level).Contains(arg)) {
							Debug.LogError(_INTL("Expected a level (1..{1}) but {2} is not a number or not a valid level.", GameData.GrowthRate.max_level, arg));
							//throw new ArgumentException(_INTL("Expected a level (1..{1}) but {2} is not a number or not a valid level.", GameData.GrowthRate.max_level, arg));
						}
						ret.Add(GenerateWildPokemon(species_id, arg));
						species_id = null;
					} else {   // Expecting species ID
						if (!GameData.Species.exists(arg)) {
							Debug.LogError(_INTL("Species {1} does not exist.", arg));
							//throw new ArgumentException(_INTL("Species {1} does not exist.", arg));
						}
						species_id = arg;
					}
					break;
			}
		}
		if (species_id) raise _INTL("Species {1} was given but not a level.", species_id);
		return ret;
	}
}

//===============================================================================
// Trainer battles.
//===============================================================================
public partial class TrainerBattle {
	// Used by most trainer events, which can be positioned in such a way that
	// multiple trainer events spot the player at once. The extra code in this
	// method deals with that case and can cause a double trainer battle instead.
	public static void start(*args) {
		// If there is another NPC trainer who spotted the player at the same time,
		// and it is possible to have a double battle (the player has 2+ able Pokémon
		// or has a partner trainer), then record this first NPC trainer into
		// Game.GameData.game_temp.waiting_trainer and end this method. That second NPC event will
		// then trigger and cause the battle to happen against this first trainer and
		// themselves.
		if (!Game.GameData.game_temp.waiting_trainer && MapInterpreterRunning() && CanDoubleBattle()) {
			thisEvent = MapInterpreter.get_self;
			// Find all other triggered trainer events
			triggeredEvents = Game.GameData.game_player.TriggeredTrainerEvents([2], false, true);
			otherEvent = new List<string>();
			foreach (var i in triggeredEvents) { //'triggeredEvents.each' do => |i|
				if (i.id == thisEvent.id) continue;
				if (Game.GameData.game_self_switches[new {Game.GameData.game_map.map_id, i.id, "A"}]) continue;
				otherEvent.Add(i);
			}
			// If there is exactly 1 other triggered trainer event, this trainer can be
			// stored up to battle with that one
			if (otherEvent.length == 1) {
				trainers, _items, _end_speeches, _party, _party_starts = TrainerBattle.generate_foes(*args);
				// If this is just 1 trainer with 6 or fewer Pokémon, it can be stored up
				// to battle alongside the other trainer
				if (trainers.length == 1 && trainers[0].party.length <= Settings.MAX_PARTY_SIZE) {
					Game.GameData.game_temp.waiting_trainer = new {trainers[0], thisEvent.id};
					return false;
				}
			}
		}
		// Perform the battle
		if (Game.GameData.game_temp.waiting_trainer) {
			new_args = args + [Game.GameData.game_temp.waiting_trainer[0]];
			outcome = TrainerBattle.start_core(*new_args);
			if (outcome == 1) MapInterpreter.SetSelfSwitch(Game.GameData.game_temp.waiting_trainer[1], "A", true);
			Game.GameData.game_temp.waiting_trainer = null;
		} else {
			outcome = TrainerBattle.start_core(*args);
		}
		// Return true if the player won the battle, and false if any other result
		return outcome == Battle.Outcome.WIN;
	}

	public static void start_core(*args) {
		outcome_variable = Game.GameData.game_temp.battle_rules["outcomeVar"] || 1;
		can_lose         = Game.GameData.game_temp.battle_rules["canLose"] || false;
		// Skip battle if the player has no able Pokémon, or if holding Ctrl in Debug mode
		if (BattleCreationHelperMethods.skip_battle()) {
			return BattleCreationHelperMethods.skip_battle(outcome_variable, true);
		}
		// Record information about party Pokémon to be used at the end of battle (e.g.
		// comparing levels for an evolution check)
		EventHandlers.trigger(:on_start_battle);
		// Generate information for the foes
		foe_trainers, foe_items, foe_party, foe_party_starts = TrainerBattle.generate_foes(*args);
		// Generate information for the player and partner trainer(s)
		player_trainers, ally_items, player_party, player_party_starts = BattleCreationHelperMethods.set_up_player_trainers(foe_party);
		// Create the battle scene (the visual side of it)
		scene = BattleCreationHelperMethods.create_battle_scene;
		// Create the battle class (the mechanics side of it)
		battle = new Battle(scene, player_party, foe_party, player_trainers, foe_trainers);
		battle.party1starts = player_party_starts;
		battle.party2starts = foe_party_starts;
		battle.ally_items   = ally_items;
		battle.items        = foe_items;
		// Set various other properties in the battle class
		if (Game.GameData.game_temp.battle_rules["size"].null()) setBattleRule($"{foe_trainers.length}v{foe_trainers.length}");
		BattleCreationHelperMethods.prepare_battle(battle);
		Game.GameData.game_temp.clear_battle_rules;
		// Perform the battle itself
		outcome = Battle.Outcome.UNDECIDED;
		BattleAnimation(GetTrainerBattleBGM(foe_trainers), (battle.singleBattle()) ? 1 : 3, foe_trainers) do;
			SceneStandby { outcome = battle.StartBattle };
			BattleCreationHelperMethods.after_battle(outcome, can_lose);
		}
		Input.update;
		// Save the result of the battle in a Game Variable (1 by default)
		BattleCreationHelperMethods.set_outcome(outcome, outcome_variable, true);
		return outcome;
	}

	public static void generate_foes(*args) {
		trainer_array = new List<string>();
		foe_items     = new List<string>();
		pokemon_array = new List<string>();
		party_starts  = new List<string>();
		trainer_type = null;
		trainer_name = null;
		args.each_with_index do |arg, i|
			switch (arg) {
				case NPCTrainer:
					if (trainer_type) raise _INTL("Trainer type {1} was given but not a trainer name.", trainer_type);
					trainer_array.Add(arg);
					foe_items.Add(arg.items);
					party_starts.Add(pokemon_array.length);
					arg.party.each(pkmn => pokemon_array.Add(pkmn));
					break;
				case Array:   // [trainer type, trainer name, version number, speech (optional)]
					if (trainer_type) raise _INTL("Trainer type {1} was given but not a trainer name.", trainer_type);
					trainer = LoadTrainer(arg[0], arg[1], arg[2]);
					if (!trainer) MissingTrainer(arg[0], arg[1], arg[2]);
					if (!trainer) trainer = LoadTrainer(arg[0], arg[1], arg[2]);   // Try again
					if (!trainer) raise _INTL("Trainer for data '{1}' is not defined.", arg);
					EventHandlers.trigger(:on_trainer_load, trainer);
					if (arg[3] && !arg[3].empty()) trainer.lose_text = arg[3];
					trainer_array.Add(trainer);
					foe_items.Add(trainer.items);
					party_starts.Add(pokemon_array.length);
					trainer.party.each(pkmn => pokemon_array.Add(pkmn));
					break;
				default:
					if (trainer_name) {   // Expecting version number
						if (!arg.is_a(Integer) || arg < 0) {
							Debug.LogError(_INTL("Expected a trainer version number (0 or higher) but {1} is not a number or not a valid value.", arg));
							//throw new ArgumentException(_INTL("Expected a trainer version number (0 or higher) but {1} is not a number or not a valid value.", arg));
						}
						trainer = LoadTrainer(trainer_type, trainer_name, arg);
						if (!trainer) MissingTrainer(trainer_type, trainer_name, arg);
						if (!trainer) trainer = LoadTrainer(trainer_type, trainer_name, arg);   // Try again
						if (!trainer) raise _INTL("Trainer for data '{1}, {2}, {3}' is not defined.", trainer_type, trainer_name, arg);
						EventHandlers.trigger(:on_trainer_load, trainer);
						trainer_array.Add(trainer);
						foe_items.Add(trainer.items);
						party_starts.Add(pokemon_array.length);
						trainer.party.each(pkmn => pokemon_array.Add(pkmn));
						trainer_type = null;
						trainer_name = null;
					} else if (trainer_type) {   // Expecting trainer name
						if (!arg.is_a(String) || arg.strip.empty()) {
							Debug.LogError(_INTL("Expected a trainer name but '{1}' is not a valid name.", arg));
							//throw new ArgumentException(_INTL("Expected a trainer name but '{1}' is not a valid name.", arg));
						}
						if (args[i + 1].is_a(Integer)) {   // Version number is next
							trainer_name = arg.strip;
						} else {
							trainer = LoadTrainer(trainer_type, arg);
							if (!trainer) MissingTrainer(trainer_type, arg, 0);
							if (!trainer) trainer = LoadTrainer(trainer_type, arg);   // Try again
							if (!trainer) raise _INTL("Trainer for data '{1}, {2}' is not defined.", trainer_type, arg);
							EventHandlers.trigger(:on_trainer_load, trainer);
							trainer_array.Add(trainer);
							foe_items.Add(trainer.items);
							party_starts.Add(pokemon_array.length);
							trainer.party.each(pkmn => pokemon_array.Add(pkmn));
							trainer_type = null;
						}
					} else {   // Expecting trainer type
						if (!GameData.TrainerType.exists(arg)) {
							Debug.LogError(_INTL("Trainer type {1} does not exist.", arg));
							//throw new ArgumentException(_INTL("Trainer type {1} does not exist.", arg));
						}
						trainer_type = arg;
					}
					break;
			}
		}
		if (trainer_type) raise _INTL("Trainer type {1} was given but not a trainer name.", trainer_type);
		return trainer_array, foe_items, pokemon_array, party_starts;
	}
}

//===============================================================================
// After battles.
//===============================================================================
EventHandlers.add(:on_end_battle, :evolve_and_black_out,
	block: (outcome, canLose) => {
		// Check for evolutions
		if (Settings.CHECK_EVOLUTION_AFTER_ALL_BATTLES ||
												!Battle.Outcome.should_black_out(outcome)) EvolutionCheck;
		Game.GameData.game_temp.party_levels_before_battle = null;
		// Check for blacking out or gaining Pickup/Huney Gather items
		switch (outcome) {
			case Battle.Outcome.WIN: case Battle.Outcome.CATCH:
				foreach (var pkmn in Game.GameData.player.pokemon_party) { //'Game.GameData.player.pokemon_party.each' do => |pkmn|
					Pickup(pkmn);
					HoneyGather(pkmn);
				}
				break;
			default:
				if (Battle.Outcome.should_black_out(outcome) && !canLose) {
					Game.GameData.game_system.bgm_unpause;
					Game.GameData.game_system.bgs_unpause;
					StartOver;
				}
				break;
		}
	}
)

public void EvolutionCheck() {
	Game.GameData.player.party.each_with_index do |pkmn, i|
		if (!pkmn || pkmn.egg()) continue;
		if (pkmn.fainted() && !Settings.CHECK_EVOLUTION_FOR_FAINTED_POKEMON) continue;
		// Find an evolution
		new_species = null;
		if (new_species.null() && Game.GameData.game_temp.party_levels_before_battle &&
			Game.GameData.game_temp.party_levels_before_battle[i] &&
			Game.GameData.game_temp.party_levels_before_battle[i] < pkmn.level) {
			new_species = pkmn.check_evolution_on_battle_level_up;
		}
		if (new_species.null()) new_species = pkmn.check_evolution_after_battle(i);
		if (new_species.null()) continue;
		// Evolve Pokémon if possible
		evo = new PokemonEvolutionScene();
		evo.StartScreen(pkmn, new_species);
		evo.Evolution;
		evo.EndScreen;
	}
}

public void DynamicItemList(*args) {
	ret = new List<string>();
	args.each(arg => { if (GameData.Item.exists(arg)) ret.Add(arg); });
	return ret;
}

// Common items to find via Pickup. Items from this list are added to the pool in
// order, starting from a point depending on the Pokémon's level. The number of
// items added is how many probabilities are in the PICKUP_COMMON_ITEM_CHANCES
// array below.
// There must be 9 + PICKUP_COMMON_ITEM_CHANCES.length number of items in this
// array (18 by default). The 9 is actually (100 / num_rarity_levels) - 1, where
// num_rarity_levels is in def Pickup below.
PICKUP_COMMON_ITEMS = new {
	:POTION,        // Levels 1-10
	:ANTIDOTE,      // Levels 1-10, 11-20
	:SUPERPOTION,   // Levels 1-10, 11-20, 21-30
	:GREATBALL,     // Levels 1-10, 11-20, 21-30, 31-40
	:REPEL,         // Levels 1-10, 11-20, 21-30, 31-40, 41-50
	:ESCAPEROPE,    // Levels 1-10, 11-20, 21-30, 31-40, 41-50, 51-60
	:FULLHEAL,      // Levels 1-10, 11-20, 21-30, 31-40, 41-50, 51-60, 61-70
	:HYPERPOTION,   // Levels 1-10, 11-20, 21-30, 31-40, 41-50, 51-60, 61-70, 71-80
	:ULTRABALL,     // Levels 1-10, 11-20, 21-30, 31-40, 41-50, 51-60, 61-70, 71-80, 81-90
	:REVIVE,        // Levels       11-20, 21-30, 31-40, 41-50, 51-60, 61-70, 71-80, 81-90, 91-100
	:RARECANDY,     // Levels              21-30, 31-40, 41-50, 51-60, 61-70, 71-80, 81-90, 91-100
	:SUNSTONE,      // Levels                     31-40, 41-50, 51-60, 61-70, 71-80, 81-90, 91-100
	:MOONSTONE,     // Levels                            41-50, 51-60, 61-70, 71-80, 81-90, 91-100
	:HEARTSCALE,    // Levels                                   51-60, 61-70, 71-80, 81-90, 91-100
	:FULLRESTORE,   // Levels                                          61-70, 71-80, 81-90, 91-100
	:MAXREVIVE,     // Levels                                                 71-80, 81-90, 91-100
	:PPUP,          // Levels                                                        81-90, 91-100
	:MAXELIXIR;      // Levels                                                               91-100
}
// Chances to get each item added to the pool from the array above.
PICKUP_COMMON_ITEM_CHANCES = new {30, 10, 10, 10, 10, 10, 10, 4, 4};
// Rare items to find via Pickup. Items from this list are added to the pool in
// order, starting from a point depending on the Pokémon's level. The number of
// items added is how many probabilities are in the PICKUP_RARE_ITEM_CHANCES
// array below.
// There must be 9 + PICKUP_RARE_ITEM_CHANCES.length number of items in this
// array (11 by default). The 9 is actually (100 / num_rarity_levels) - 1, where
// num_rarity_levels is in def Pickup below.
PICKUP_RARE_ITEMS = new {
	:HYPERPOTION,   // Levels 1-10
	:NUGGET,        // Levels 1-10, 11-20
	:KINGSROCK,     // Levels       11-20, 21-30
	:FULLRESTORE,   // Levels              21-30, 31-40
	:ETHER,         // Levels                     31-40, 41-50
	:IRONBALL,      // Levels                            41-50, 51-60
	:DESTINYKNOT,   // Levels                                   51-60, 61-70
	:ELIXIR,        // Levels                                          61-70, 71-80
	:DESTINYKNOT,   // Levels                                                 71-80, 81-90
	:LEFTOVERS,     // Levels                                                        81-90, 91-100
	:DESTINYKNOT;    // Levels                                                               91-100
}
// Chances to get each item added to the pool from the array above.
PICKUP_RARE_ITEM_CHANCES = new {1, 1};

// Try to gain an item after a battle if a Pokemon has the ability Pickup.
public void Pickup(pkmn) {
	if (pkmn.egg() || !pkmn.hasAbility(Abilitys.PICKUP)) return;
	if (pkmn.hasItem()) return;
	unless (rand(100) < 10) return;   // 10% chance for Pickup to trigger
	num_rarity_levels = 10;
	// Ensure common and rare item lists contain defined items
	common_items = DynamicItemList(*PICKUP_COMMON_ITEMS);
	rare_items = DynamicItemList(*PICKUP_RARE_ITEMS);
	if (common_items.length < num_rarity_levels - 1 + PICKUP_COMMON_ITEM_CHANCES.length) return;
	if (rare_items.length < num_rarity_levels - 1 + PICKUP_RARE_ITEM_CHANCES.length) return;
	// Determine the starting point for adding items from the above arrays into the
	// pool
	start_index = (int)Math.Max(((int)Math.Min(100, pkmn.level) - 1) * num_rarity_levels / 100, 0);
	// Generate a pool of items depending on the Pokémon's level
	items = new List<string>();
	PICKUP_COMMON_ITEM_CHANCES.length.times(i => items.Add(common_items[start_index + i]));
	PICKUP_RARE_ITEM_CHANCES.length.times(i => items.Add(rare_items[start_index + i]));
	// Randomly choose an item from the pool to give to the Pokémon
	all_chances = PICKUP_COMMON_ITEM_CHANCES + PICKUP_RARE_ITEM_CHANCES;
	rnd = rand(all_chances.sum);
	cumul = 0;
	all_chances.each_with_index do |c, i|
		cumul += c;
		if (rnd >= cumul) continue;
		pkmn.item = items[i];
		break;
	}
}

// Try to gain a Honey item after a battle if a Pokemon has the ability Honey Gather.
public void HoneyGather(pkmn) {
	if (!GameData.Items.exists(Items.HONEY)) return;
	if (pkmn.egg() || !pkmn.hasAbility(Abilitys.HONEYGATHER) || pkmn.hasItem()) return;
	chance = 5 + (((pkmn.level - 1) / 10) * 5);
	unless (rand(100) < chance) return;
	pkmn.item = items.HONEY;
}
