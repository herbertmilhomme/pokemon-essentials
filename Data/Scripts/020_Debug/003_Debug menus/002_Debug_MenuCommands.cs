//===============================================================================
// Field options.
//===============================================================================

MenuHandlers.add(:debug_menu, :field_menu, {
	"name"        => _INTL("Field options..."),
	"parent"      => :main,
	"description" => _INTL("Warp to maps, edit switches/variables, use the PC, edit Day Care, etc."),
	"always_show" => false;
});

MenuHandlers.add(:debug_menu, :warp, {
	"name"        => _INTL("Warp to map"),
	"parent"      => :field_menu,
	"description" => _INTL("Instantly warp to another map of your choice."),
	"effect"      => (sprites, viewport) => {
		map = WarpToMap;
		if (!map) next false;
		FadeOutAndHide(sprites);
		DisposeMessageWindow(sprites["textbox"]);
		DisposeSpriteHash(sprites);
		viewport.dispose;
		if (Game.GameData.scene.is_a(Scene_Map)) {
			Game.GameData.game_temp.player_new_map_id    = map[0];
			Game.GameData.game_temp.player_new_x         = map[1];
			Game.GameData.game_temp.player_new_y         = map[2];
			Game.GameData.game_temp.player_new_direction = 2;
			Game.GameData.scene.transfer_player;
		} else {
			CancelVehicles;
			Game.GameData.map_factory.setup(map[0]);
			Game.GameData.game_player.moveto(map[1], map[2]);
			Game.GameData.game_player.turn_down;
			Game.GameData.game_map.update;
			Game.GameData.game_map.autoplay;
		}
		Game.GameData.game_map.refresh;
		next true;   // Closes the debug menu to allow the warp
	}
});

MenuHandlers.add(:debug_menu, :use_pc, {
	"name"        => _INTL("Use PC"),
	"parent"      => :field_menu,
	"description" => _INTL("Use a PC to access Pokémon storage and player's PC."),
	"effect"      => () => {
		PokeCenterPC;
	}
});

MenuHandlers.add(:debug_menu, :switches, {
	"name"        => _INTL("Switches"),
	"parent"      => :field_menu,
	"description" => _INTL("Edit all Game Switches (except Script Switches)."),
	"effect"      => () => {
		DebugVariables(0);
	}
});

MenuHandlers.add(:debug_menu, :variables, {
	"name"        => _INTL("Variables"),
	"parent"      => :field_menu,
	"description" => _INTL("Edit all Game Variables. Can set them to numbers or text."),
	"effect"      => () => {
		DebugVariables(1);
	}
});

MenuHandlers.add(:debug_menu, :safari_zone_and_bug_contest, {
	"name"        => _INTL("Safari Zone and Bug-Catching Contest"),
	"parent"      => :field_menu,
	"description" => _INTL("Edit steps/time remaining and number of usable Poké Balls."),
	"effect"      => () => {
		if (InSafari()) {
			safari = SafariState;
			cmd = 0;
			do { //loop; while (true);
				cmds = new {_INTL("Steps remaining: {1}", (Settings.SAFARI_STEPS > 0) ? safari.steps : _INTL("infinite")),
								GameData.Item.get(:SAFARIBALL).name_plural + ": " + safari.ballcount.ToString()};
				cmd = ShowCommands(null, cmds, -1, cmd);
				if (cmd < 0) break;
				switch (cmd) {
					case 0:   // Steps remaining
						if (Settings.SAFARI_STEPS > 0) {
							params = new ChooseNumberParams();
							params.setRange(0, 99999);
							params.setDefaultValue(safari.steps);
							safari.steps = MessageChooseNumber(_INTL("Set the steps remaining in this Safari game."), params);
						}
						break;
					case 1:   // Safari Balls
						params = new ChooseNumberParams();
						params.setRange(0, 99999);
						params.setDefaultValue(safari.ballcount);
						safari.ballcount = MessageChooseNumber(
							_INTL("Set the quantity of {1}.", GameData.Item.get(:SAFARIBALL).name_plural), params);
						break;
				}
			}
		} else if (InBugContest()) {
			contest = BugContestState;
			cmd = 0;
			do { //loop; while (true);
				cmds = new List<string>();
				if (Settings.BUG_CONTEST_TIME > 0) {
					time_left = Settings.BUG_CONTEST_TIME - (System.uptime - contest.timer_start).ToInt();
					if (time_left < 0) time_left = 0;
					min = time_left / 60;
					sec = time_left % 60;
					time_string = string.Format("{1:02d}m {2:02d}s", min, sec);
				} else {
					time_string = _INTL("infinite");
				}
				cmds.Add(_INTL("Time remaining: {1}", time_string));
				cmds.Add(GameData.Item.get(:SPORTBALL).name_plural + ": " + contest.ballcount.ToString());
				cmd = ShowCommands(null, cmds, -1, cmd);
				if (cmd < 0) break;
				switch (cmd) {
					case 0:   // Steps remaining
						if (Settings.BUG_CONTEST_TIME > 0) {
							params = new ChooseNumberParams();
							params.setRange(0, 99999);
							params.setDefaultValue(min);
							new_time = MessageChooseNumber(_INTL("Set the time remaining (in minutes) in this Bug-Catching Contest."), params);
							contest.timer_start += (new_time - min) * 60;
							foreach (var sprite in Game.GameData.scene.spriteset.usersprites) { //'Game.GameData.scene.spriteset.usersprites.each' do => |sprite|
								if (!sprite.is_a(TimerDisplay)) continue;
								sprite.start_time = contest.timer_start;
								break;
							}
						}
						break;
					case 1:   // Sport Balls
						params = new ChooseNumberParams();
						params.setRange(0, 99999);
						params.setDefaultValue(contest.ballcount);
						contest.ballcount = MessageChooseNumber(
							_INTL("Set the quantity of {1}.", GameData.Item.get(:SPORTBALL).name_plural), params);
						break;
				}
			}
		} else {
			Message(_INTL("You aren't in the Safari Zone or a Bug-Catching Contest!"));
		}
	}
});

MenuHandlers.add(:debug_menu, :edit_field_effects, {
	"name"        => _INTL("Change field effects"),
	"parent"      => :field_menu,
	"description" => _INTL("Edit Repel steps, Strength and Flash usage, and Black/White Flute effects."),
	"effect"      => () => {
		cmd = 0;
		do { //loop; while (true);
			cmds = new List<string>();
			cmds.Add(_INTL("Repel steps: {1}", Game.GameData.PokemonGlobal.repel));
			cmds.Add((Game.GameData.PokemonMap.strengthUsed ? "[Y]" : "[  ]") + " " + _INTL("Strength used"));
			cmds.Add((Game.GameData.PokemonGlobal.flashUsed ? "[Y]" : "[  ]") + " " + _INTL("Flash used"));
			cmds.Add((Game.GameData.PokemonMap.lower_encounter_rate ? "[Y]" : "[  ]") + " " + _INTL("Lower encounter rate"));
			cmds.Add((Game.GameData.PokemonMap.higher_encounter_rate ? "[Y]" : "[  ]") + " " + _INTL("Higher encounter rate"));
			cmds.Add((Game.GameData.PokemonMap.lower_level_wild_pokemon ? "[Y]" : "[  ]") + " " + _INTL("Lower level wild Pokémon"));
			cmds.Add((Game.GameData.PokemonMap.higher_level_wild_pokemon ? "[Y]" : "[  ]") + " " + _INTL("Higher level wild Pokémon"));
			cmd = ShowCommands(null, cmds, -1, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Repel steps
					params = new ChooseNumberParams();
					params.setRange(0, 99999);
					params.setDefaultValue(Game.GameData.PokemonGlobal.repel);
					Game.GameData.PokemonGlobal.repel = MessageChooseNumber(_INTL("Set the number of steps remaining."), params);
					break;
				case 1:   // Strength used
					Game.GameData.PokemonMap.strengthUsed = !Game.GameData.PokemonMap.strengthUsed;
					break;
				case 2:   // Flash used
					if (Game.GameData.game_map.metadata&.dark_map && Game.GameData.scene.is_a(Scene_Map)) {
						Game.GameData.PokemonGlobal.flashUsed = !Game.GameData.PokemonGlobal.flashUsed;
						darkness = Game.GameData.game_temp.darkness_sprite;
						if (darkness && !darkness.disposed()) darkness.dispose;
						Game.GameData.game_temp.darkness_sprite = new DarknessSprite();
						Game.GameData.scene.spriteset&.addUserSprite(Game.GameData.game_temp.darkness_sprite);
						if (Game.GameData.PokemonGlobal.flashUsed) {
							Game.GameData.game_temp.darkness_sprite.radius = Game.GameData.game_temp.darkness_sprite.radiusMax;
						}
					} else {
						Message(_INTL("You're not in a dark map!"));
					}
					break;
				case 3:   // Lower encounter rate
					Game.GameData.PokemonMap.lower_encounter_rate ||= false;
					Game.GameData.PokemonMap.lower_encounter_rate = !Game.GameData.PokemonMap.lower_encounter_rate;
					break;
				case 4:   // Higher encounter rate
					Game.GameData.PokemonMap.higher_encounter_rate ||= false;
					Game.GameData.PokemonMap.higher_encounter_rate = !Game.GameData.PokemonMap.higher_encounter_rate;
					break;
				case 5:   // Lower level wild Pokémon
					Game.GameData.PokemonMap.lower_level_wild_pokemon ||= false;
					Game.GameData.PokemonMap.lower_level_wild_pokemon = !Game.GameData.PokemonMap.lower_level_wild_pokemon;
					break;
				case 6:   // Higher level wild Pokémon
					Game.GameData.PokemonMap.higher_level_wild_pokemon ||= false;
					Game.GameData.PokemonMap.higher_level_wild_pokemon = !Game.GameData.PokemonMap.higher_level_wild_pokemon;
					break;
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :refresh_map, {
	"name"        => _INTL("Refresh map"),
	"parent"      => :field_menu,
	"description" => _INTL("Make all events on this map, and common events, refresh themselves."),
	"effect"      => () => {
		Game.GameData.game_map.need_refresh = true;
		Message(_INTL("The map will refresh."));
	}
});

MenuHandlers.add(:debug_menu, :day_care, {
	"name"        => _INTL("Day Care"),
	"parent"      => :field_menu,
	"description" => _INTL("View Pokémon in the Day Care and edit them."),
	"effect"      => () => {
		DebugDayCare;
	}
});

MenuHandlers.add(:debug_menu, :storage_wallpapers, {
	"name"        => _INTL("Toggle storage wallpapers"),
	"parent"      => :field_menu,
	"description" => _INTL("Unlock and lock special wallpapers used in Pokémon storage."),
	"effect"      => () => {
		w = Game.GameData.PokemonStorage.allWallpapers;
		if (w.length <= PokemonStorage.BASICWALLPAPERQTY) {
			Message(_INTL("There are no special wallpapers defined."));
		} else {
			paperscmd = 0;
			unlockarray = Game.GameData.PokemonStorage.unlockedWallpapers;
			do { //loop; while (true);
				paperscmds = new List<string>();
				paperscmds.Add(_INTL("Unlock all"));
				paperscmds.Add(_INTL("Lock all"));
				(PokemonStorage.BASICWALLPAPERQTY...w.length).each do |i|
					paperscmds.Add((unlockarray[i] ? "[Y]" : "[  ]") + " " + w[i]);
				}
				paperscmd = ShowCommands(null, paperscmds, -1, paperscmd);
				if (paperscmd < 0) break;
				switch (paperscmd) {
					case 0:   // Unlock all
						(PokemonStorage.BASICWALLPAPERQTY...w.length).each do |i|
							unlockarray[i] = true;
						}
						break;
					case 1:   // Lock all
						(PokemonStorage.BASICWALLPAPERQTY...w.length).each do |i|
							unlockarray[i] = false;
						}
						break;
					default:
						paperindex = paperscmd - 2 + PokemonStorage.BASICWALLPAPERQTY;
						unlockarray[paperindex] = !Game.GameData.PokemonStorage.unlockedWallpapers[paperindex];
						break;
				}
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :skip_credits, {
	"name"        => _INTL("Skip credits"),
	"parent"      => :field_menu,
	"description" => _INTL("Toggle whether credits can be ended early by pressing the Use input."),
	"effect"      => () => {
		Game.GameData.PokemonGlobal.creditsPlayed = !Game.GameData.PokemonGlobal.creditsPlayed;
		if (Game.GameData.PokemonGlobal.creditsPlayed) Message(_INTL("Credits can be skipped when played in future."));
		if (!Game.GameData.PokemonGlobal.creditsPlayed) Message(_INTL("Credits cannot be skipped when next played."));
	}
});

//===============================================================================
// Battle options.
//===============================================================================

MenuHandlers.add(:debug_menu, :battle_menu, {
	"name"        => _INTL("Battle options..."),
	"parent"      => :main,
	"description" => _INTL("Start battles, reset this map's trainers, ready rematches, edit roamers, etc."),
	"always_show" => false;
});

MenuHandlers.add(:debug_menu, :test_wild_battle, {
	"name"        => _INTL("Test wild battle"),
	"parent"      => :battle_menu,
	"description" => _INTL("Start a single battle against a wild Pokémon. You choose the species/level."),
	"effect"      => () => {
		species = ChooseSpeciesList;
		if (species) {
			params = new ChooseNumberParams();
			params.setRange(1, GameData.GrowthRate.max_level);
			params.setInitialValue(5);
			params.setCancelValue(0);
			level = MessageChooseNumber(_INTL("Set the wild {1}'s level.",
																					GameData.Species.get(species).name), params);
			if (level > 0) {
				Game.GameData.game_temp.encounter_type = null;
				setBattleRule("canLose");
				WildBattle.start(species, level);
			}
		}
		next false;
	}
});

MenuHandlers.add(:debug_menu, :test_wild_battle_advanced, {
	"name"        => _INTL("Test wild battle advanced"),
	"parent"      => :battle_menu,
	"description" => _INTL("Start a battle against 1 or more wild Pokémon. Battle size is your choice."),
	"effect"      => () => {
		pkmn = new List<string>();
		size0 = 1;
		pkmnCmd = 0;
		do { //loop; while (true);
			pkmnCmds = new List<string>();
			pkmn.each(p => pkmnCmds.Add(string.Format("{0} Lv.{0}", p.name, p.level)));
			pkmnCmds.Add(_INTL("[Add Pokémon]"));
			pkmnCmds.Add(_INTL("[Set player side size]"));
			pkmnCmds.Add(_INTL("[Start {1}v{2} battle]", size0, pkmn.length));
			pkmnCmd = ShowCommands(null, pkmnCmds, -1, pkmnCmd);
			if (pkmnCmd < 0) break;
			if (pkmnCmd == pkmnCmds.length - 1) {      // Start battle
				if (pkmn.length == 0) {
					Message(_INTL("No Pokémon were chosen, cannot start battle."));
					continue;
				}
				setBattleRule(string.Format("{0}v{0}", size0, pkmn.length));
				setBattleRule("canLose");
				Game.GameData.game_temp.encounter_type = null;
				WildBattle.start(*pkmn);
				break;
			} else if (pkmnCmd == pkmnCmds.length - 2) {   // Set player side size
				if (!CanDoubleBattle()) {
					Message(_INTL("You only have one Pokémon."));
					continue;
				}
				maxVal = (CanTripleBattle()) ? 3 : 2;
				params = new ChooseNumberParams();
				params.setRange(1, maxVal);
				params.setInitialValue(size0);
				params.setCancelValue(0);
				newSize = MessageChooseNumber(
					_INTL("Choose the number of battlers on the player's side (max. {1}).", maxVal), params
				);
				if (newSize > 0) size0 = newSize;
			} else if (pkmnCmd == pkmnCmds.length - 3) {   // Add Pokémon
				species = ChooseSpeciesList;
				if (species) {
					params = new ChooseNumberParams();
					params.setRange(1, GameData.GrowthRate.max_level);
					params.setInitialValue(5);
					params.setCancelValue(0);
					level = MessageChooseNumber(_INTL("Set the wild {1}'s level.",
																							GameData.Species.get(species).name), params);
					if (level > 0) {
						pkmn.Add(GenerateWildPokemon(species, level));
						size0 = pkmn.length;
					}
				}
			} else {                                   // Edit a Pokémon
				if (ConfirmMessage(_INTL("Change this Pokémon?"))) {
					scr = new PokemonDebugPartyScreen();
					scr.PokemonDebug(pkmn[pkmnCmd], -1, null, true);
					scr.EndScreen;
				} else if (ConfirmMessage(_INTL("Delete this Pokémon?"))) {
					pkmn.delete_at(pkmnCmd);
					size0 = (int)Math.Max(pkmn.length, 1);
				}
			}
		}
		next false;
	}
});

MenuHandlers.add(:debug_menu, :test_trainer_battle, {
	"name"        => _INTL("Test trainer battle"),
	"parent"      => :battle_menu,
	"description" => _INTL("Start a single battle against a trainer of your choice."),
	"effect"      => () => {
		trainerdata = ListScreen(_INTL("SINGLE TRAINER"), new TrainerBattleLister(0, false));
		if (trainerdata) {
			setBattleRule("canLose");
			TrainerBattle.start(trainerdata[0], trainerdata[1], trainerdata[2]);
		}
		next false;
	}
});

MenuHandlers.add(:debug_menu, :test_trainer_battle_advanced, {
	"name"        => _INTL("Test trainer battle advanced"),
	"parent"      => :battle_menu,
	"description" => _INTL("Start a battle against 1 or more trainers with a battle size of your choice."),
	"effect"      => () => {
		trainers = new List<string>();
		size0 = 1;
		size1 = 1;
		trainerCmd = 0;
		do { //loop; while (true);
			trainerCmds = new List<string>();
			trainers.each(t => trainerCmds.Add(string.Format("{0} x{0}", t[1].full_name, t[1].party_count)));
			trainerCmds.Add(_INTL("[Add trainer]"));
			trainerCmds.Add(_INTL("[Set player side size]"));
			trainerCmds.Add(_INTL("[Set opponent side size]"));
			trainerCmds.Add(_INTL("[Start {1}v{2} battle]", size0, size1));
			trainerCmd = ShowCommands(null, trainerCmds, -1, trainerCmd);
			if (trainerCmd < 0) break;
			if (trainerCmd == trainerCmds.length - 1) {      // Start battle
				if (trainers.length == 0) {
					Message(_INTL("No trainers were chosen, cannot start battle."));
					continue;
				} else if (size1 < trainers.length) {
					Message(_INTL("Opposing side size is invalid. It should be at least {1}.", trainers.length));
					continue;
				} else if (size1 > trainers.length && trainers[0][1].party_count == 1) {
					Message(
						_INTL("Opposing side size cannot be {1}, as that requires the first trainer to have 2 or more Pokémon, which they don't.",
									size1)
					);
					continue;
				}
				setBattleRule(string.Format("{0}v{0}", size0, size1));
				setBattleRule("canLose");
				battleArgs = new List<string>();
				trainers.each(t => battleArgs.Add(t[1]));
				TrainerBattle.start(*battleArgs);
				break;
			} else if (trainerCmd == trainerCmds.length - 2) {   // Set opponent side size
				if (trainers.length == 0 || (trainers.length == 1 && trainers[0][1].party_count == 1)) {
					Message(_INTL("No trainers were chosen or trainer only has one Pokémon."));
					continue;
				}
				maxVal = 2;
				if (trainers.length >= 3 ||
											(trainers.length == 2 && trainers[0][1].party_count >= 2) ||
											trainers[0][1].party_count >= 3) maxVal = 3;
				params = new ChooseNumberParams();
				params.setRange(1, maxVal);
				params.setInitialValue(size1);
				params.setCancelValue(0);
				newSize = MessageChooseNumber(
					_INTL("Choose the number of battlers on the opponent's side (max. {1}).", maxVal), params
				);
				if (newSize > 0) size1 = newSize;
			} else if (trainerCmd == trainerCmds.length - 3) {   // Set player side size
				if (!CanDoubleBattle()) {
					Message(_INTL("You only have one Pokémon."));
					continue;
				}
				maxVal = (CanTripleBattle()) ? 3 : 2;
				params = new ChooseNumberParams();
				params.setRange(1, maxVal);
				params.setInitialValue(size0);
				params.setCancelValue(0);
				newSize = MessageChooseNumber(
					_INTL("Choose the number of battlers on the player's side (max. {1}).", maxVal), params
				);
				if (newSize > 0) size0 = newSize;
			} else if (trainerCmd == trainerCmds.length - 4) {   // Add trainer
				trainerdata = ListScreen(_INTL("CHOOSE A TRAINER"), new TrainerBattleLister(0, false));
				if (trainerdata) {
					tr = LoadTrainer(trainerdata[0], trainerdata[1], trainerdata[2]);
					EventHandlers.trigger(:on_trainer_load, tr);
					trainers.Add(new {0, tr});
					size0 = trainers.length;
					size1 = trainers.length;
				}
			} else {                                         // Edit a trainer
				if (ConfirmMessage(_INTL("Change this trainer?"))) {
					trainerdata = ListScreen(_INTL("CHOOSE A TRAINER"),
																		new TrainerBattleLister(trainers[trainerCmd][0], false));
					if (trainerdata) {
						tr = LoadTrainer(trainerdata[0], trainerdata[1], trainerdata[2]);
						EventHandlers.trigger(:on_trainer_load, tr);
						trainers[trainerCmd] = new {0, tr};
					}
				} else if (ConfirmMessage(_INTL("Delete this trainer?"))) {
					trainers.delete_at(trainerCmd);
					size0 = (int)Math.Max(trainers.length, 1);
					size1 = (int)Math.Max(trainers.length, 1);
				}
			}
		}
		next false;
	}
});

MenuHandlers.add(:debug_menu, :encounter_version, {
	"name"        => _INTL("Set wild encounters version"),
	"parent"      => :battle_menu,
	"description" => _INTL("Choose which version of wild encounters should be used."),
	"effect"      => () => {
		params = new ChooseNumberParams();
		params.setRange(0, 99);
		params.setInitialValue(Game.GameData.PokemonGlobal.encounter_version);
		params.setCancelValue(-1);
		value = MessageChooseNumber(_INTL("Set encounters version to which value?"), params);
		if (value >= 0) Game.GameData.PokemonGlobal.encounter_version = value;
	}
});

MenuHandlers.add(:debug_menu, :roamers, {
	"name"        => _INTL("Roaming Pokémon"),
	"parent"      => :battle_menu,
	"description" => _INTL("Toggle and edit all roaming Pokémon."),
	"effect"      => () => {
		DebugRoamers;
	}
});

MenuHandlers.add(:debug_menu, :reset_trainers, {
	"name"        => _INTL("Reset map's trainers"),
	"parent"      => :battle_menu,
	"description" => _INTL("Turn off Self Switches A and B for all events with \"Trainer\" in their name."),
	"effect"      => () => {
		if (Game.GameData.game_map) {
			foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
				if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"trainer",RegexOptions.IgnoreCase)) {
					Game.GameData.game_self_switches[new {Game.GameData.game_map.map_id, event.id, "A"}] = false;
					Game.GameData.game_self_switches[new {Game.GameData.game_map.map_id, event.id, "B"}] = false;
				}
			}
			Game.GameData.game_map.need_refresh = true;
			Message(_INTL("All Trainers on this map were reset."));
		} else {
			Message(_INTL("This command can't be used here."));
		}
	}
});

MenuHandlers.add(:debug_menu, :toggle_exp_all, {
	"name"        => _INTL("Toggle Exp. All's effect"),
	"parent"      => :battle_menu,
	"description" => _INTL("Toggle Exp. All's effect of giving Exp. to non-participants."),
	"effect"      => () => {
		Game.GameData.player.has_exp_all = !Game.GameData.player.has_exp_all;
		if (Game.GameData.player.has_exp_all) Message(_INTL("Enabled Exp. All's effect."));
		if (!Game.GameData.player.has_exp_all) Message(_INTL("Disabled Exp. All's effect."));
	}
});

MenuHandlers.add(:debug_menu, :toggle_logging, {
	"name"        => _INTL("Toggle logging of battle messages"),
	"parent"      => :battle_menu,
	"description" => _INTL("Record debug logs for battles in Data/debuglog.txt."),
	"effect"      => () => {
		Core.INTERNAL = !Core.INTERNAL;
		if (Core.INTERNAL) Message(_INTL("Debug logs for battles will be made in the Data folder."));
		if (!Core.INTERNAL) Message(_INTL("Debug logs for battles will not be made."));
	}
});

//===============================================================================
// Pokémon options.
//===============================================================================

MenuHandlers.add(:debug_menu, :pokemon_menu, {
	"name"        => _INTL("Pokémon options..."),
	"parent"      => :main,
	"description" => _INTL("Heal the party, give Pokémon, fill/empty PC storage, etc."),
	"always_show" => false;
});

MenuHandlers.add(:debug_menu, :heal_party, {
	"name"        => _INTL("Heal party"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Fully heal the HP/status/PP of all Pokémon in the party."),
	"effect"      => () => {
		Game.GameData.player.party.each(pkmn => pkmn.heal);
		Message(_INTL("Your Pokémon were fully healed."));
	}
});

MenuHandlers.add(:debug_menu, :add_pokemon, {
	"name"        => _INTL("Add Pokémon"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Give yourself a Pokémon of a chosen species/level. Goes to PC if party is full."),
	"effect"      => () => {
		species = ChooseSpeciesList;
		if (species) {
			params = new ChooseNumberParams();
			params.setRange(1, GameData.GrowthRate.max_level);
			params.setInitialValue(5);
			params.setCancelValue(0);
			level = MessageChooseNumber(_INTL("Set the Pokémon's level."), params);
			if (level > 0) {
				goes_to_party = !Game.GameData.player.party_full();
				if (AddPokemonSilent(species, level)) {
					if (goes_to_party) {
						Message(_INTL("Added {1} to party.", GameData.Species.get(species).name));
					} else {
						Message(_INTL("Added {1} to Pokémon storage.", GameData.Species.get(species).name));
					}
				} else {
					Message(_INTL("Couldn't add Pokémon because party and storage are full."));
				}
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :fill_boxes, {
	"name"        => _INTL("Fill storage boxes"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Puts one Pokémon of each species (at Level 50) in storage."),
	"effect"      => () => {
		added = 0;
		box_qty = Game.GameData.PokemonStorage.maxPokemon(0);
		completed = true;
		foreach (var species_data in GameData.Species) { //'GameData.Species.each' do => |species_data|
			sp = species_data.species;
			f = species_data.form;
			// Record each form of each species as seen and owned
			if (f == 0) {
				if (species_data.single_gendered()) {
					g = (species_data.gender_ratio == :AlwaysFemale) ? 1 : 0;
					Game.GameData.player.pokedex.register(sp, g, f, 0, false);
					Game.GameData.player.pokedex.register(sp, g, f, 1, false);
				} else {   // Both male and female
					Game.GameData.player.pokedex.register(sp, 0, f, 0, false);
					Game.GameData.player.pokedex.register(sp, 0, f, 1, false);
					Game.GameData.player.pokedex.register(sp, 1, f, 0, false);
					Game.GameData.player.pokedex.register(sp, 1, f, 1, false);
				}
				Game.GameData.player.pokedex.set_owned(sp, false);
			} else if (species_data.real_form_name && !species_data.real_form_name.empty()) {
				g = (species_data.gender_ratio == :AlwaysFemale) ? 1 : 0;
				Game.GameData.player.pokedex.register(sp, g, f, 0, false);
				Game.GameData.player.pokedex.register(sp, g, f, 1, false);
			}
			// Add Pokémon (if form 0, i.e. one of each species)
			if (f != 0) continue;
			if (added >= Settings.NUM_STORAGE_BOXES * box_qty) {
				completed = false;
				continue;
			}
			added += 1;
			Game.GameData.PokemonStorage[(added - 1) / box_qty, (added - 1) % box_qty] = new Pokemon(sp, 50);
		}
		Game.GameData.player.pokedex.refresh_accessible_dexes;
		Message(_INTL("Storage boxes were filled with one Pokémon of each species."));
		if (!completed) {
			Message(_INTL("Note: The number of storage spaces ({1} boxes of {2}) is less than the number of species.",
											Settings.NUM_STORAGE_BOXES, box_qty));
		}
	}
});

MenuHandlers.add(:debug_menu, :clear_boxes, {
	"name"        => _INTL("Clear storage boxes"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Remove all Pokémon in storage."),
	"effect"      => () => {
		for (int i = Game.GameData.PokemonStorage.maxBoxes; i < Game.GameData.PokemonStorage.maxBoxes; i++) { //for 'Game.GameData.PokemonStorage.maxBoxes' times do => |i|
			for (int j = Game.GameData.PokemonStorage.maxPokemon(i); j < Game.GameData.PokemonStorage.maxPokemon(i); j++) { //for 'Game.GameData.PokemonStorage.maxPokemon(i)' times do => |j|
				Game.GameData.PokemonStorage[i, j] = null;
			}
		}
		Message(_INTL("The storage boxes were cleared."));
	}
});

MenuHandlers.add(:debug_menu, :give_demo_party, {
	"name"        => _INTL("Give demo party"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Give yourself 6 preset Pokémon. They overwrite the current party."),
	"effect"      => () => {
		party = new List<string>();
		species = new {:PIKACHU, :PIDGEOTTO, :KADABRA, :GYARADOS, :DIGLETT, :CHANSEY};
		species.each(id => { if (GameData.Species.exists(id)) party.Add(id); });
		Game.GameData.player.party.clear;
		// Generate Pokémon of each species at level 20
		foreach (var spec in party) { //'party.each' do => |spec|
			pkmn = new Pokemon(spec, 20);
			Game.GameData.player.party.Add(pkmn);
			Game.GameData.player.pokedex.register(pkmn);
			Game.GameData.player.pokedex.set_owned(spec);
			switch (spec) {
				case Pokemons.PIDGEOTTO:
					pkmn.learn_move(Moves.FLY);
					break;
				case Pokemons.KADABRA:
					pkmn.learn_move(Moves.FLASH);
					pkmn.learn_move(Moves.TELEPORT);
					break;
				case Pokemons.GYARADOS:
					pkmn.learn_move(Moves.SURF);
					pkmn.learn_move(Moves.DIVE);
					pkmn.learn_move(Moves.WATERFALL);
					break;
				case Pokemons.DIGLETT:
					pkmn.learn_move(Moves.DIG);
					pkmn.learn_move(Moves.CUT);
					pkmn.learn_move(Moves.HEADBUTT);
					pkmn.learn_move(Moves.ROCKSMASH);
					break;
				case Pokemons.CHANSEY:
					pkmn.learn_move(Moves.SOFTBOILED);
					pkmn.learn_move(Moves.STRENGTH);
					pkmn.learn_move(Moves.SWEETSCENT);
					break;
			}
			pkmn.record_first_moves;
		}
		Message(_INTL("Filled party with demo Pokémon."));
	}
});

MenuHandlers.add(:debug_menu, :quick_hatch_party_eggs, {
	"name"        => _INTL("Quick hatch all party eggs"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Make all eggs in the party require just one more step to hatch."),
	"effect"      => () => {
		Game.GameData.player.party.each(pkmn => { if (pkmn.egg()) pkmn.steps_to_hatch = 1; });
		Message(_INTL("All eggs in your party now require one step to hatch."));
	}
});

MenuHandlers.add(:debug_menu, :open_storage, {
	"name"        => _INTL("Access Pokémon storage"),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Opens the Pokémon storage boxes in Organize Boxes mode."),
	"effect"      => () => {
		FadeOutIn do;
			scene = new PokemonStorageScene();
			screen = new PokemonStorageScreen(scene, Game.GameData.PokemonStorage);
			screen.StartScreen(0);
		}
	}
});

//===============================================================================
// Shadow Pokémon options.
//===============================================================================

MenuHandlers.add(:debug_menu, :shadow_pokemon_menu, {
	"name"        => _INTL("Shadow Pokémon options..."),
	"parent"      => :pokemon_menu,
	"description" => _INTL("Snag Machine and purification."),
	"always_show" => false;
});

MenuHandlers.add(:debug_menu, :toggle_snag_machine, {
	"name"        => _INTL("Toggle Snag Machine"),
	"parent"      => :shadow_pokemon_menu,
	"description" => _INTL("Toggle all Poké Balls being able to catch Shadow Pokémon."),
	"effect"      => () => {
		Game.GameData.player.has_snag_machine = !Game.GameData.player.has_snag_machine;
		if (Game.GameData.player.has_snag_machine) Message(_INTL("Gave the Snag Machine."));
		if (!Game.GameData.player.has_snag_machine) Message(_INTL("Lost the Snag Machine."));
	}
});

MenuHandlers.add(:debug_menu, :toggle_purify_chamber_access, {
	"name"        => _INTL("Toggle Purify Chamber access"),
	"parent"      => :shadow_pokemon_menu,
	"description" => _INTL("Toggle access to the Purify Chamber via the PC."),
	"effect"      => () => {
		Game.GameData.player.seen_purify_chamber = !Game.GameData.player.seen_purify_chamber;
		if (Game.GameData.player.seen_purify_chamber) Message(_INTL("The Purify Chamber is accessible."));
		if (!Game.GameData.player.seen_purify_chamber) Message(_INTL("The Purify Chamber is not accessible."));
	}
});

MenuHandlers.add(:debug_menu, :purify_chamber, {
	"name"        => _INTL("Use Purify Chamber"),
	"parent"      => :shadow_pokemon_menu,
	"description" => _INTL("Open the Purify Chamber for Shadow Pokémon purification."),
	"effect"      => () => {
		PurifyChamber;
	}
});

MenuHandlers.add(:debug_menu, :relic_stone, {
	"name"        => _INTL("Use Relic Stone"),
	"parent"      => :shadow_pokemon_menu,
	"description" => _INTL("Choose a Shadow Pokémon to show to the Relic Stone for purification."),
	"effect"      => () => {
		RelicStone;
	}
});

//===============================================================================
// Item options.
//===============================================================================

MenuHandlers.add(:debug_menu, :items_menu, {
	"name"        => _INTL("Item options..."),
	"parent"      => :main,
	"description" => _INTL("Give and take items."),
	"always_show" => false;
});

MenuHandlers.add(:debug_menu, :add_item, {
	"name"        => _INTL("Add item"),
	"parent"      => :items_menu,
	"description" => _INTL("Choose an item and a quantity of it to add to the Bag."),
	"effect"      => () => {
		ListScreenBlock(_INTL("ADD ITEM"), new ItemLister()) do |button, item|
			if (button == Input.USE && item) {
				params = new ChooseNumberParams();
				params.setRange(1, Settings.BAG_MAX_PER_SLOT);
				params.setInitialValue(1);
				params.setCancelValue(0);
				qty = MessageChooseNumber(_INTL("Add how many {1}?",
																					GameData.Item.get(item).name_plural), params);
				if (qty > 0) {
					Game.GameData.bag.add(item, qty);
					Message(_INTL("Gave {1}x {2}.", qty, GameData.Item.get(item).name));
				}
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :fill_bag, {
	"name"        => _INTL("Fill Bag"),
	"parent"      => :items_menu,
	"description" => _INTL("Empties the Bag and then fills it with a certain number of every item."),
	"effect"      => () => {
		params = new ChooseNumberParams();
		params.setRange(1, Settings.BAG_MAX_PER_SLOT);
		params.setInitialValue(1);
		params.setCancelValue(0);
		qty = MessageChooseNumber(_INTL("Choose the number of items."), params);
		if (qty > 0) {
			Game.GameData.bag.clear;
			// NOTE: This doesn't simply use Game.GameData.bag.add for every item in turn, because
			//       that's really slow when done in bulk.
			pocket_sizes = Settings.BAG_MAX_POCKET_SIZE;
			bag = Game.GameData.bag.pockets;   // Called here so that it only rearranges itself once
			foreach (var i in GameData.Item) { //'GameData.Item.each' do => |i|
				if (!pocket_sizes[i.pocket - 1] || pocket_sizes[i.pocket - 1] == 0) continue;
				if (pocket_sizes[i.pocket - 1] > 0 && bag[i.pocket].length >= pocket_sizes[i.pocket - 1]) continue;
				item_qty = (i.is_important()) ? 1 : qty;
				bag[i.pocket].Add(new {i.id, item_qty});
			}
			// NOTE: Auto-sorting pockets don't need to be sorted afterwards, because
			//       items are added in the same order they would be sorted into.
			Message(_INTL("The Bag was filled with {1} of each item.", qty));
		}
	}
});

MenuHandlers.add(:debug_menu, :empty_bag, {
	"name"        => _INTL("Empty Bag"),
	"parent"      => :items_menu,
	"description" => _INTL("Remove all items from the Bag."),
	"effect"      => () => {
		Game.GameData.bag.clear;
		Message(_INTL("The Bag was cleared."));
	}
});

//===============================================================================
// Player options.
//===============================================================================

MenuHandlers.add(:debug_menu, :player_menu, {
	"name"        => _INTL("Player options..."),
	"parent"      => :main,
	"description" => _INTL("Set money, badges, Pokédexes, player's appearance and name, etc."),
	"always_show" => false;
});

MenuHandlers.add(:debug_menu, :set_money, {
	"name"        => _INTL("Set money"),
	"parent"      => :player_menu,
	"description" => _INTL("Edit how much money, Game Corner Coins and Battle Points you have."),
	"effect"      => () => {
		cmd = 0;
		do { //loop; while (true);
			cmds = new {_INTL("Money: ${1}", Game.GameData.player.money.to_s_formatted),
							_INTL("Coins: {1}", Game.GameData.player.coins.to_s_formatted),
							_INTL("Battle Points: {1}", Game.GameData.player.battle_points.to_s_formatted)};
			cmd = ShowCommands(null, cmds, -1, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Money
					params = new ChooseNumberParams();
					params.setRange(0, Settings.MAX_MONEY);
					params.setDefaultValue(Game.GameData.player.money);
					Game.GameData.player.money = MessageChooseNumber("\\ts[]" + _INTL("Set the player's money."), params);
					break;
				case 1:   // Coins
					params = new ChooseNumberParams();
					params.setRange(0, Settings.MAX_COINS);
					params.setDefaultValue(Game.GameData.player.coins);
					Game.GameData.player.coins = MessageChooseNumber("\\ts[]" + _INTL("Set the player's Coin amount."), params);
					break;
				case 2:   // Battle Points
					params = new ChooseNumberParams();
					params.setRange(0, Settings.MAX_BATTLE_POINTS);
					params.setDefaultValue(Game.GameData.player.battle_points);
					Game.GameData.player.battle_points = MessageChooseNumber("\\ts[]" + _INTL("Set the player's BP amount."), params);
					break;
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :set_badges, {
	"name"        => _INTL("Set Gym Badges"),
	"parent"      => :player_menu,
	"description" => _INTL("Toggle possession of each Gym Badge."),
	"effect"      => () => {
		badgecmd = 0;
		do { //loop; while (true);
			badgecmds = new List<string>();
			badgecmds.Add(_INTL("Give all"));
			badgecmds.Add(_INTL("Remove all"));
			for (int i = 24; i < 24; i++) { //for '24' times do => |i|
				badgecmds.Add((Game.GameData.player.badges[i] ? "[Y]" : "[  ]") + " " + _INTL("Badge {1}", i + 1));
			}
			badgecmd = ShowCommands(null, badgecmds, -1, badgecmd);
			if (badgecmd < 0) break;
			switch (badgecmd) {
				case 0:   // Give all
					24.times(i => Game.GameData.player.badges[i] = true);
					break;
				case 1:   // Remove all
					24.times(i => Game.GameData.player.badges[i] = false);
					break;
				default:
					Game.GameData.player.badges[badgecmd - 2] = !Game.GameData.player.badges[badgecmd - 2];
					break;
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :toggle_running_shoes, {
	"name"        => _INTL("Toggle running shoes"),
	"parent"      => :player_menu,
	"description" => _INTL("Toggle possession of running shoes."),
	"effect"      => () => {
		Game.GameData.player.has_running_shoes = !Game.GameData.player.has_running_shoes;
		if (Game.GameData.player.has_running_shoes) Message(_INTL("Gave Running Shoes."));
		if (!Game.GameData.player.has_running_shoes) Message(_INTL("Lost Running Shoes."));
	}
});

MenuHandlers.add(:debug_menu, :toggle_pokedex, {
	"name"        => _INTL("Toggle Pokédex and Regional Dexes"),
	"parent"      => :player_menu,
	"description" => _INTL("Toggle possession of the Pokédex, and edit Regional Dex accessibility."),
	"effect"      => () => {
		dexescmd = 0;
		do { //loop; while (true);
			dexescmds = new List<string>();
			dexescmds.Add(_INTL("Have Pokédex: {1}", Game.GameData.player.has_pokedex ? "[YES]" : "[NO]"));
			dex_names = Settings.pokedex_names;
			for (int i = dex_names.length; i < dex_names.length; i++) { //for 'dex_names.length' times do => |i|
				name = (dex_names[i].Length > 0) ? dex_names[i][0] : dex_names[i];
				unlocked = Game.GameData.player.pokedex.unlocked(i);
				dexescmds.Add((unlocked ? "[Y]" : "[  ]") + " " + name);
			}
			dexescmd = ShowCommands(null, dexescmds, -1, dexescmd);
			if (dexescmd < 0) break;
			dexindex = dexescmd - 1;
			if (dexindex < 0) {   // Toggle Pokédex ownership
				Game.GameData.player.has_pokedex = !Game.GameData.player.has_pokedex;
			} else if (Game.GameData.player.pokedex.unlocked(dexindex)) {   // Toggle Regional Dex accessibility
				Game.GameData.player.pokedex.lock(dexindex);
			} else {
				Game.GameData.player.pokedex.unlock(dexindex);
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :toggle_pokegear, {
	"name"        => _INTL("Toggle Pokégear"),
	"parent"      => :player_menu,
	"description" => _INTL("Toggle possession of the Pokégear."),
	"effect"      => () => {
		Game.GameData.player.has_pokegear = !Game.GameData.player.has_pokegear;
		if (Game.GameData.player.has_pokegear) Message(_INTL("Gave Pokégear."));
		if (!Game.GameData.player.has_pokegear) Message(_INTL("Lost Pokégear."));
	}
});

MenuHandlers.add(:debug_menu, :edit_phone_contacts, {
	"name"        => _INTL("Edit phone and contacts"),
	"parent"      => :player_menu,
	"description" => _INTL("Edit properties of the phone and of contacts registered in it."),
	"effect"      => () => {
		if (!Game.GameData.PokemonGlobal.phone) {
			Message(_INTL("The phone is not defined."));
			continue;
		}
		cmd = 0;
		do { //loop; while (true);
			cmds = new List<string>();
			time = Game.GameData.PokemonGlobal.phone.time_to_next_call.ToInt();   // time is in seconds
			min = time / 60;
			sec = time % 60;
			cmds.Add(_INTL("Time until next call: {1}m {2}s", min, sec));
			cmds.Add((Phone.rematches_enabled ? "[Y]" : "[  ]") + " " + _INTL("Rematches possible"));
			cmds.Add(_INTL("Maximum rematch version : {1}", Phone.rematch_variant));
			if (Game.GameData.PokemonGlobal.phone.contacts.length > 0) {
				cmds.Add(_INTL("Make all contacts ready for a rematch"));
				cmds.Add(_INTL("Edit individual contacts: {1}", Game.GameData.PokemonGlobal.phone.contacts.length));
			}
			cmd = ShowCommands(null, cmds, -1, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Time until next call
					params = new ChooseNumberParams();
					params.setRange(0, 99999);
					params.setDefaultValue(min);
					params.setCancelValue(-1);
					new_time = MessageChooseNumber(_INTL("Set the time (in minutes) until the next phone call."), params);
					if (new_time >= 0) Game.GameData.PokemonGlobal.phone.time_to_next_call = new_time * 60;
					break;
				case 1:   // Rematches possible
					Phone.rematches_enabled = !Phone.rematches_enabled;
					break;
				case 2:   // Maximum rematch version
					params = new ChooseNumberParams();
					params.setRange(0, 99);
					params.setDefaultValue(Phone.rematch_variant);
					new_version = MessageChooseNumber(_INTL("Set the maximum version number a trainer contact can reach."), params);
					Phone.rematch_variant = new_version;
					break;
				case 3:   // Make all contacts ready for a rematch
					foreach (var contact in Game.GameData.PokemonGlobal.phone.contacts) { //'Game.GameData.PokemonGlobal.phone.contacts.each' do => |contact|
						if (!contact.trainer()) continue;
						contact.rematch_flag = 1;
						contact.set_trainer_event_ready_for_rematch;
					}
					Message(_INTL("All trainers in the phone are now ready to rebattle."));
					break;
				case 4:   // Edit individual contacts
					contact_cmd = 0;
					do { //loop; while (true);
						contact_cmds = new List<string>();
						foreach (var contact in Game.GameData.PokemonGlobal.phone.contacts) { //'Game.GameData.PokemonGlobal.phone.contacts.each' do => |contact|
							visible_string = (contact.visible()) ? "[Y]" : "[  ]";
							if (contact.trainer()) {
								battle_string = (contact.can_rematch()) ? "(can battle)" : "";
								contact_cmds.Add(string.Format("{0} {0} ({0}) {0}", visible_string, contact.display_name, contact.variant, battle_string));
							} else {
								contact_cmds.Add(string.Format("{0} {0}", visible_string, contact.display_name));
							}
						}
						contact_cmd = ShowCommands(null, contact_cmds, -1, contact_cmd);
						if (contact_cmd < 0) break;
						contact = Game.GameData.PokemonGlobal.phone.contacts[contact_cmd];
						edit_cmd = 0;
						do { //loop; while (true);
							edit_cmds = new List<string>();
							edit_cmds.Add((contact.visible() ? "[Y]" : "[  ]") + " " + _INTL("Contact visible"));
							if (contact.trainer()) {
								edit_cmds.Add((contact.can_rematch() ? "[Y]" : "[  ]") + " " + _INTL("Can battle"));
								ready_time = contact.time_to_ready;   // time is in seconds
								ready_min = ready_time / 60;
								ready_sec = ready_time % 60;
								edit_cmds.Add(_INTL("Time until ready to battle: {1}m {2}s", ready_min, ready_sec));
								edit_cmds.Add(_INTL("Last defeated version: {1}", contact.variant));
							}
							if (edit_cmds.length == 0) break;
							edit_cmd = ShowCommands(null, edit_cmds, -1, edit_cmd);
							if (edit_cmd < 0) break;
							switch (edit_cmd) {
								case 0:   // Visibility
									if (contact.can_hide()) contact.visible = !contact.visible;
									break;
								case 1:   // Can battle
									contact.rematch_flag = (contact.can_rematch()) ? 0 : 1;
									if (contact.can_rematch()) contact.time_to_ready = 0;
									break;
								case 2:   // Time until ready to battle
									params = new ChooseNumberParams();
									params.setRange(0, 99999);
									params.setDefaultValue(ready_min);
									params.setCancelValue(-1);
									new_time = MessageChooseNumber(_INTL("Set the time (in minutes) until this trainer is ready to battle."), params);
									if (new_time >= 0) contact.time_to_ready = new_time * 60;
									break;
								case 3:   // Last defeated version
									params = new ChooseNumberParams();
									params.setRange(0, 99);
									params.setDefaultValue(contact.variant);
									new_version = MessageChooseNumber(_INTL("Set the last defeated version number of this trainer."), params);
									contact.version = contact.start_version + new_version;
									break;
							}
						}
					}
					break;
			}
		}
	}
});

MenuHandlers.add(:debug_menu, :toggle_box_link, {
	"name"        => _INTL("Toggle access to storage from party screen"),
	"parent"      => :player_menu,
	"description" => _INTL("Toggle Box Link's effect of accessing Pokémon storage via the party screen."),
	"effect"      => () => {
		Game.GameData.player.has_box_link = !Game.GameData.player.has_box_link;
		if (Game.GameData.player.has_box_link) Message(_INTL("Enabled access to storage from the party screen."));
		if (!Game.GameData.player.has_box_link) Message(_INTL("Disabled access to storage from the party screen."));
	}
});

MenuHandlers.add(:debug_menu, :set_player_character, {
	"name"        => _INTL("Set player character"),
	"parent"      => :player_menu,
	"description" => _INTL("Edit the player's character, as defined in \"metadata.txt\"."),
	"effect"      => () => {
		index = 0;
		cmds = new List<string>();
		ids = new List<string>();
		foreach (var player in GameData.PlayerMetadata) { //'GameData.PlayerMetadata.each' do => |player|
			if (player.id == Game.GameData.player.character_ID) index = cmds.length;
			cmds.Add(player.id.ToString());
			ids.Add(player.id);
		}
		if (cmds.length == 1) {
			Message(_INTL("There is only one player character defined."));
			break;
		}
		cmd = ShowCommands(null, cmds, -1, index);
		if (cmd >= 0 && cmd != index) {
			ChangePlayer(ids[cmd]);
			Message(_INTL("The player character was changed."));
		}
	}
});

MenuHandlers.add(:debug_menu, :change_outfit, {
	"name"        => _INTL("Set player outfit"),
	"parent"      => :player_menu,
	"description" => _INTL("Edit the player's outfit number."),
	"effect"      => () => {
		oldoutfit = Game.GameData.player.outfit;
		params = new ChooseNumberParams();
		params.setRange(0, 99);
		params.setDefaultValue(oldoutfit);
		Game.GameData.player.outfit = MessageChooseNumber(_INTL("Set the player's outfit."), params);
		if (Game.GameData.player.outfit != oldoutfit) Message(_INTL("Player's outfit was changed."));
	}
});

MenuHandlers.add(:debug_menu, :rename_player, {
	"name"        => _INTL("Set player name"),
	"parent"      => :player_menu,
	"description" => _INTL("Rename the player."),
	"effect"      => () => {
		trname = EnterPlayerName("Your name?", 0, Settings.MAX_PLAYER_NAME_SIZE, Game.GameData.player.name);
		if (nil_or_empty(trname) && ConfirmMessage(_INTL("Give yourself a default name?"))) {
			trainertype = Game.GameData.player.trainer_type;
			gender      = GetTrainerTypeGender(trainertype);
			trname      = SuggestTrainerName(gender);
		}
		if (nil_or_empty(trname)) {
			Message(_INTL("The player's name remained {1}.", Game.GameData.player.name));
		} else {
			Game.GameData.player.name = trname;
			Message(_INTL("The player's name was changed to {1}.", Game.GameData.player.name));
		}
	}
});

MenuHandlers.add(:debug_menu, :random_id, {
	"name"        => _INTL("Randomize player ID"),
	"parent"      => :player_menu,
	"description" => _INTL("Generate a random new ID for the player."),
	"effect"      => () => {
		Game.GameData.player.id = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
		Message(_INTL("The player's ID was changed to {1} (full ID: {2}).", Game.GameData.player.public_ID, Game.GameData.player.id));
	}
});

//===============================================================================
// PBS file editors.
//===============================================================================

MenuHandlers.add(:debug_menu, :s_editors_menu, {
	"name"        => _INTL("PBS file editors..."),
	"parent"      => :main,
	"description" => _INTL("Edit information in the PBS files.");
});

MenuHandlers.add(:debug_menu, :set_map_connections, {
	"name"        => _INTL("Edit map_connections.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Connect maps using a visual interface. Can also edit map encounters/metadata."),
	"effect"      => () => {
		FadeOutIn { ConnectionsEditor };
	}
});

MenuHandlers.add(:debug_menu, :set_encounters, {
	"name"        => _INTL("Edit encounters.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit the wild Pokémon that can be found on maps, and how they are encountered."),
	"effect"      => () => {
		FadeOutIn { EncountersEditor };
	}
});

MenuHandlers.add(:debug_menu, :set_trainers, {
	"name"        => _INTL("Edit trainers.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit individual trainers, their Pokémon and items."),
	"effect"      => () => {
		FadeOutIn { TrainerBattleEditor };
	}
});

MenuHandlers.add(:debug_menu, :set_trainer_types, {
	"name"        => _INTL("Edit trainer_types.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit the properties of trainer types."),
	"effect"      => () => {
		FadeOutIn { TrainerTypeEditor };
	}
});

MenuHandlers.add(:debug_menu, :set_map_metadata, {
	"name"        => _INTL("Edit map_metadata.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit map metadata."),
	"effect"      => () => {
		MapMetadataScreen(DefaultMap);
	}
});

MenuHandlers.add(:debug_menu, :set_metadata, {
	"name"        => _INTL("Edit metadata.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit global metadata and player character metadata."),
	"effect"      => () => {
		MetadataScreen;
	}
});

MenuHandlers.add(:debug_menu, :set_items, {
	"name"        => _INTL("Edit items.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit item data."),
	"effect"      => () => {
		FadeOutIn { ItemEditor };
	}
});

MenuHandlers.add(:debug_menu, :set_species, {
	"name"        => _INTL("Edit pokemon.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Edit Pokémon species data."),
	"effect"      => () => {
		FadeOutIn { PokemonEditor };
	}
});

MenuHandlers.add(:debug_menu, :position_sprites, {
	"name"        => _INTL("Edit pokemon_metrics.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Reposition Pokémon sprites in battle."),
	"effect"      => () => {
		FadeOutIn do;
			sp = new SpritePositioner();
			sps = new SpritePositionerScreen(sp);
			sps.Start;
		}
	}
});

MenuHandlers.add(:debug_menu, :auto_position_sprites, {
	"name"        => _INTL("Auto-set pokemon_metrics.txts"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Automatically reposition all Pokémon sprites in battle. Don't use lightly."),
	"effect"      => () => {
		if (ConfirmMessage(_INTL("Are you sure you want to reposition all sprites?"))) {
			msgwindow = CreateMessageWindow;
			MessageDisplay(msgwindow, _INTL("Repositioning all sprites. Please wait."), false);
			Graphics.update;
			AutoPositionAll;
			DisposeMessageWindow(msgwindow);
		}
	}
});

MenuHandlers.add(:debug_menu, :set_pokedex_lists, {
	"name"        => _INTL("Edit regional_dexes.txt"),
	"parent"      => :s_editors_menu,
	"description" => _INTL("Create, rearrange and delete Regional Pokédex lists."),
	"effect"      => () => {
		FadeOutIn { RegionalDexEditorMain };
	}
});

//===============================================================================
// Other editors.
//===============================================================================

MenuHandlers.add(:debug_menu, :editors_menu, {
	"name"        => _INTL("Other editors..."),
	"parent"      => :main,
	"description" => _INTL("Edit battle animations, terrain tags, map data, etc.");
});

MenuHandlers.add(:debug_menu, :animation_editor, {
	"name"        => _INTL("Battle animation editor"),
	"parent"      => :editors_menu,
	"description" => _INTL("Edit the battle animations."),
	"effect"      => () => {
		FadeOutIn { AnimationEditor };
	}
});

MenuHandlers.add(:debug_menu, :animation_organiser, {
	"name"        => _INTL("Battle animation organiser"),
	"parent"      => :editors_menu,
	"description" => _INTL("Rearrange/add/delete battle animations."),
	"effect"      => () => {
		FadeOutIn { AnimationsOrganiser };
	}
});

MenuHandlers.add(:debug_menu, :import_animations, {
	"name"        => _INTL("Import all battle animations"),
	"parent"      => :editors_menu,
	"description" => _INTL("Import all battle animations from the \"Animations\" folder."),
	"effect"      => () => {
		ImportAllAnimations;
	}
});

MenuHandlers.add(:debug_menu, :export_animations, {
	"name"        => _INTL("Export all battle animations"),
	"parent"      => :editors_menu,
	"description" => _INTL("Export all battle animations individually to the \"Animations\" folder."),
	"effect"      => () => {
		ExportAllAnimations;
	}
});

MenuHandlers.add(:debug_menu, :set_terrain_tags, {
	"name"        => _INTL("Edit terrain tags"),
	"parent"      => :editors_menu,
	"description" => _INTL("Edit the terrain tags of tiles in tilesets. Required for tags 8+."),
	"effect"      => () => {
		FadeOutIn { TilesetScreen };
	}
});

MenuHandlers.add(:debug_menu, :fix_invalid_tiles, {
	"name"        => _INTL("Fix invalid tiles"),
	"parent"      => :editors_menu,
	"description" => _INTL("Scans all maps and erases non-existent tiles."),
	"effect"      => () => {
		DebugFixInvalidTiles;
	}
});

//===============================================================================
// Other options.
//===============================================================================

MenuHandlers.add(:debug_menu, :files_menu, {
	"name"        => _INTL("Files options..."),
	"parent"      => :main,
	"description" => _INTL("Compile, generate PBS files, translations, Mystery Gifts, etc.");
});

MenuHandlers.add(:debug_menu, :compile_data, {
	"name"        => _INTL("Compile data"),
	"parent"      => :files_menu,
	"description" => _INTL("Fully compile all data."),
	"effect"      => () => {
		msgwindow = CreateMessageWindow;
		Compiler.compile_all(true);
		MessageDisplay(msgwindow, _INTL("All game data was compiled."));
		DisposeMessageWindow(msgwindow);
	}
});

MenuHandlers.add(:debug_menu, :create_pbs_files, {
	"name"        => _INTL("Create PBS file(s)"),
	"parent"      => :files_menu,
	"description" => _INTL("Choose one or all PBS files and create it."),
	"effect"      => () => {
		cmd = 0;
		cmds = new {
			_INTL("[Create all]"),
			"abilities.txt",
			"battle_facility_lists.txt",
			"berry_plants.txt",
			"dungeon_parameters.txt",
			"dungeon_tilesets.txt",
			"encounters.txt",
			"items.txt",
			"map_connections.txt",
			"map_metadata.txt",
			"metadata.txt",
			"moves.txt",
			"phone.txt",
			"pokemon.txt",
			"pokemon_forms.txt",
			"pokemon_metrics.txt",
			"regional_dexes.txt",
			"ribbons.txt",
			"shadow_pokemon.txt",
			"town_map.txt",
			"trainer_types.txt",
			"trainers.txt",
			"types.txt";
		}
		do { //loop; while (true);
			cmd = ShowCommands(null, cmds, -1, cmd);
			switch (cmd) {
				case 0:   Compiler.write_all_pbs_files; break;
				case 1:   Compiler.write_abilities; break;
				case 2:   Compiler.write_trainer_lists; break;
				case 3:   Compiler.write_berry_plants; break;
				case 4:   Compiler.write_dungeon_parameters; break;
				case 5:   Compiler.write_dungeon_tilesets; break;
				case 6:   Compiler.write_encounters; break;
				case 7:   Compiler.write_items; break;
				case 8:   Compiler.write_connections; break;
				case 9:   Compiler.write_map_metadata; break;
				case 10:  Compiler.write_metadata; break;
				case 11:  Compiler.write_moves; break;
				case 12:  Compiler.write_phone; break;
				case 13:  Compiler.write_pokemon; break;
				case 14:  Compiler.write_pokemon_forms; break;
				case 15:  Compiler.write_pokemon_metrics; break;
				case 16:  Compiler.write_regional_dexes; break;
				case 17:  Compiler.write_ribbons; break;
				case 18:  Compiler.write_shadow_pokemon; break;
				case 19:  Compiler.write_town_map; break;
				case 20:  Compiler.write_trainer_types; break;
				case 21:  Compiler.write_trainers; break;
				case 22:  Compiler.write_types; break;
				default: break; break;
			}
			Message(_INTL("File written."));
		}
	}
});

MenuHandlers.add(:debug_menu, :rename_files, {
	"name"        => _INTL("Rename outdated files"),
	"parent"      => :files_menu,
	"description" => _INTL("Check for files with outdated names and rename/move them. Can alter map data."),
	"effect"      => () => {
		if (ConfirmMessage(_INTL("Are you sure you want to automatically rename outdated files?"))) {
			FilenameUpdater.rename_files;
			Message(_INTL("Done."));
		}
	}
});

MenuHandlers.add(:debug_menu, :extract_text, {
	"name"        => _INTL("Extract text for translation"),
	"parent"      => :files_menu,
	"description" => _INTL("Extract all text in the game to text files for translating."),
	"effect"      => () => {
		if (Settings.LANGUAGES.length == 0) {
			Message(_INTL("No languages are defined in the LANGUAGES array in Settings."));
			Message(_INTL("You need to add at least one language to LANGUAGES first, to choose which one to extract text for."));
			continue;
		}
		// Choose a language from Settings to name the extraction folder after
		cmds = new List<string>();
		Settings.LANGUAGES.each(val => cmds.Add(val[0]));
		cmds.Add(_INTL("Cancel"));
		language_index = Message(_INTL("Choose a language to extract text for."), cmds, cmds.length);
		if (language_index == cmds.length - 1) continue;
		language_name = Settings.LANGUAGES[language_index][1];
		// Choose whether to extract core text or game text
		text_type = Message(_INTL("Choose a language to extract text for."),
													new {_INTL("Game-specific text"), _INTL("Core text"), _INTL("Cancel")}, 3);
		if (text_type == 2) continue;
		// If game text, choose whether to extract map texts to map-specific files or
		// to one big file
		map_files = 0;
		if (text_type == 0) {
			map_files = Message(_INTL("How many text files should map event texts be extracted to?"),
														new {_INTL("One big file"), _INTL("One file per map"), _INTL("Cancel")}, 3);
			if (map_files == 2) continue;
		}
		// Extract the chosen set of text for the chosen language
		Translator.extract_text(language_name, text_type == 1, map_files == 1);
	}
});

MenuHandlers.add(:debug_menu, :compile_text, {
	"name"        => _INTL("Compile translated text"),
	"parent"      => :files_menu,
	"description" => _INTL("Import text files and convert them into a language file."),
	"effect"      => () => {
		// Find all folders with a particular naming convention
		cmds = Dir.glob("Text_*_*");
		if (cmds.length == 0) {
			Message(_INTL("No language folders found to compile."));
			Message(_INTL("Language folders must be named \"Text_SOMETHING_core\" or \"Text_SOMETHING_game\" and be in the root folder."));
			continue;
		}
		cmds.Add(_INTL("Cancel"));
		// Ask which folder to compile into a .dat file
		folder_index = Message(_INTL("Choose a language folder to compile."), cmds, cmds.length);
		if (folder_index == cmds.length - 1) continue;
		// Compile the text files in the chosen folder
		dat_filename = cmds[folder_index] = System.Text.RegularExpressions.Regex.Replace(cmds[folder_index], "^Text_", "");
		Translator.compile_text(cmds[folder_index], dat_filename);
	}
});

MenuHandlers.add(:debug_menu, :mystery_gift, {
	"name"        => _INTL("Manage Mystery Gifts"),
	"parent"      => :files_menu,
	"description" => _INTL("Edit and enable/disable Mystery Gifts."),
	"effect"      => () => {
		ManageMysteryGifts;
	}
});

MenuHandlers.add(:debug_menu, :reload_system_cache, {
	"name"        => _INTL("Reload system cache"),
	"parent"      => :files_menu,
	"description" => _INTL("Refreshes the system's file cache. Use if you change a file while playing."),
	"effect"      => () => {
		System.reload_cache;
		Message(_INTL("Done."));
	}
});
