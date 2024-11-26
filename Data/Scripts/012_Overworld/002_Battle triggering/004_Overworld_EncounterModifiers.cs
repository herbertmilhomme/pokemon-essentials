//===============================================================================
// This section was created solely for you to put various bits of code that
// modify various wild Pokémon and trainers immediately prior to battling them.
// Be sure that any code you use here ONLY applies to the Pokémon/trainers you
// want it to apply to!
//===============================================================================

// Make all wild Pokémon shiny while a certain Switch is ON (see Settings).
EventHandlers.add(:on_wild_pokemon_created, :make_shiny_switch,
	block: (pkmn) => {
		if (Game.GameData.game_switches[Settings.SHINY_WILD_POKEMON_SWITCH]) pkmn.shiny = true;
	}
)

// In the Safari Zone and Bug-Catching Contests, wild Pokémon reroll their IVs up
// to 4 times if they don't have a perfect IV.
EventHandlers.add(:on_wild_pokemon_created, :reroll_ivs_in_safari_and_bug_contest,
	block: (pkmn) => {
		if (!InSafari() && !InBugContest()) continue;
		rerolled = false;
		4.times do;
			if (pkmn.iv.any((_stat, val) => val == Pokemon.IV_STAT_LIMIT)) break;
			rerolled = true;
			foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
				pkmn.iv[s.id] = rand(Pokemon.IV_STAT_LIMIT + 1);
			}
		}
		if (rerolled) pkmn.calc_stats;
	}
)

// In Gen 6 and later, Legendary/Mythical/Ultra Beast Pokémon are guaranteed to
// have at least 3 perfect IVs.
EventHandlers.add(:on_wild_pokemon_created, :some_perfect_ivs_for_legendaries,
	block: (pkmn) => {
		if (!Settings.LEGENDARIES_HAVE_SOME_PERFECT_IVS) continue;
		data = pkmn.species_data;
		if (!data.has_flag("Legendary") && !data.has_flag("Mythical") && !data.has_flag("UltraBeast")) continue;
		stats = new List<string>();
		GameData.Stat.each_main(s => stats.Add(s.id));
		perfect_stats = stats.sample(3);
		perfect_stats.each(s => pkmn.iv[s] = Pokemon.IV_STAT_LIMIT);
		pkmn.calc_stats;
	}
)

// Used in the random dungeon map. Makes the levels of all wild Pokémon in that
// map depend on the levels of Pokémon in the player's party.
// This is a simple method, and can/should be modified to account for evolutions
// and other such details.  Of course, you don't HAVE to use this code.
EventHandlers.add(:on_wild_pokemon_created, :level_depends_on_party,
	block: (pkmn) => {
		if (!Game.GameData.game_map.metadata&.has_flag("ScaleWildEncounterLevels")) continue;
		new_level = BalancedLevel(Game.GameData.player.party) - 4 + rand(5);   // For variety
		new_level = new_level.clamp(1, GameData.GrowthRate.max_level);
		pkmn.level = new_level;
		pkmn.calc_stats;
		pkmn.reset_moves;
	}
)

// This is the basis of a trainer modifier. It works both for trainers loaded
// when you battle them, and for partner trainers when they are registered.
// Note that you can only modify a partner trainer's Pokémon, and not the trainer
// themselves nor their items this way, as those are generated from scratch
// before each battle.
// EventHandlers.add(:on_trainer_load, :put_a_name_here,
//   block: (trainer) => {
//     if (trainer) {   // An NPCTrainer object containing party/items/lose text, etc.
//       YOUR CODE HERE
//     }
//   }
// )
