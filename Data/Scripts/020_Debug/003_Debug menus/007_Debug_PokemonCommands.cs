//===============================================================================
// HP/Status options.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :hp_status_menu, {
	"name"   => _INTL("HP/status..."),
	"parent" => :main;
});

MenuHandlers.add(:pokemon_debug_menu, :set_hp, {
	"name"   => _INTL("Set HP"),
	"parent" => :hp_status_menu,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.egg()) {
			screen.Display(_INTL("{1} is an egg.", pkmn.name));
		} else {
			params = new ChooseNumberParams();
			params.setRange(0, pkmn.totalhp);
			params.setDefaultValue(pkmn.hp);
			newhp = MessageChooseNumber(
				_INTL("Set {1}'s HP (max. {2}).", pkmn.name, pkmn.totalhp), params,
				() => screen.Update))
			if (newhp != pkmn.hp) {
				pkmn.hp = newhp;
				screen.RefreshSingle(pkmnid);
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_status, {
	"name"   => _INTL("Set status"),
	"parent" => :hp_status_menu,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.egg()) {
			screen.Display(_INTL("{1} is an egg.", pkmn.name));
		} else if (pkmn.hp <= 0) {
			screen.Display(_INTL("{1} is fainted, can't change status.", pkmn.name));
		} else {
			cmd = 0;
			commands = [_INTL("[Cure]")];
			ids = [:NONE];
			foreach (var s in GameData.Status) { //'GameData.Status.each' do => |s|
				if (s.id == :NONE) continue;
				commands.Add(_INTL("Set {1}", s.name));
				ids.Add(s.id);
			}
			do { //loop; while (true);
				msg = _INTL("Current status: {1}", GameData.Status.get(pkmn.status).name);
				if (pkmn.status == statuses.SLEEP) {
					msg = _INTL("Current status: {1} (turns: {2})",
											GameData.Status.get(pkmn.status).name, pkmn.statusCount);
				}
				cmd = screen.ShowCommands(msg, commands, cmd);
				if (cmd < 0) break;
				switch (cmd) {
					case 0:   // Cure
						pkmn.heal_status;
						screen.RefreshSingle(pkmnid);
						break;
					default:   // Give status problem
						count = 0;
						cancel = false;
						if (ids[cmd] == :SLEEP) {
							params = new ChooseNumberParams();
							params.setRange(0, 9);
							params.setDefaultValue(3);
							count = MessageChooseNumber(
								_INTL("Set the Pokémon's sleep count."), params,
								() =>  screen.Update())
							if (count <= 0) cancel = true;
						}
						if (!cancel) {
							pkmn.status      = ids[cmd];
							pkmn.statusCount = count;
							screen.RefreshSingle(pkmnid);
						}
						break;
				}
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :full_heal, {
	"name"   => _INTL("Fully heal"),
	"parent" => :hp_status_menu,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.egg()) {
			screen.Display(_INTL("{1} is an egg.", pkmn.name));
		} else {
			pkmn.heal;
			screen.Display(_INTL("{1} was fully healed.", pkmn.name));
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :make_fainted, {
	"name"   => _INTL("Make fainted"),
	"parent" => :hp_status_menu,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.egg()) {
			screen.Display(_INTL("{1} is an egg.", pkmn.name));
		} else {
			pkmn.hp = 0;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_pokerus, {
	"name"   => _INTL("Set Pokérus"),
	"parent" => :hp_status_menu,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			pokerus = (pkmn.pokerus) ? pkmn.pokerus : 0;
			msg = new {_INTL("{1} doesn't have Pokérus.", pkmn.name),
						_INTL("Has strain {1}, infectious for {2} more days.", pokerus / 16, pokerus % 16),
						_INTL("Has strain {1}, not infectious.", pokerus / 16)}[pkmn.pokerusStage];
			cmd = screen.ShowCommands(msg,
																	new {_INTL("Give random strain"),
																	_INTL("Make not infectious"),
																	_INTL("Clear Pokérus")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Give random strain
					pkmn.givePokerus;
					screen.RefreshSingle(pkmnid);
					break;
				case 1:   // Make not infectious
					if (pokerus > 0) {
						strain = pokerus / 16;
						p = strain << 4;
						pkmn.pokerus = p;
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 2:   // Clear Pokérus
					pkmn.pokerus = 0;
					screen.RefreshSingle(pkmnid);
					break;
			}
		}
		next false;
	}
});

//===============================================================================
// Level/stats options.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :level_stats, {
	"name"   => _INTL("Level/stats..."),
	"parent" => :main;
});

MenuHandlers.add(:pokemon_debug_menu, :set_level, {
	"name"   => _INTL("Set level"),
	"parent" => :level_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.egg()) {
			screen.Display(_INTL("{1} is an egg.", pkmn.name));
		} else {
			params = new ChooseNumberParams();
			params.setRange(1, GameData.GrowthRate.max_level);
			params.setDefaultValue(pkmn.level);
			level = MessageChooseNumber(
				_INTL("Set the Pokémon's level (max. {1}).", params.maxNumber), params,
				() => screen.Update))
			if (level != pkmn.level) {
				pkmn.level = level;
				pkmn.calc_stats;
				screen.RefreshSingle(pkmnid);
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_exp, {
	"name"   => _INTL("Set Exp"),
	"parent" => :level_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.egg()) {
			screen.Display(_INTL("{1} is an egg.", pkmn.name));
		} else {
			minxp = pkmn.growth_rate.minimum_exp_for_level(pkmn.level);
			maxxp = pkmn.growth_rate.minimum_exp_for_level(pkmn.level + 1);
			if (minxp == maxxp) {
				screen.Display(_INTL("{1} is at the maximum level.", pkmn.name));
			} else {
				params = new ChooseNumberParams();
				params.setRange(minxp, maxxp - 1);
				params.setDefaultValue(pkmn.exp);
				newexp = MessageChooseNumber(
					_INTL("Set the Pokémon's Exp (range {1}-{2}).", minxp, maxxp - 1), params,
					() => screen.Update())
				if (newexp != pkmn.exp) {
					pkmn.exp = newexp;
					pkmn.calc_stats;
					screen.RefreshSingle(pkmnid);
				}
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :hidden_values, {
	"name"   => _INTL("EV/IV/personal ID..."),
	"parent" => :level_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			persid = string.Format("0x{0:X8}", pkmn.personalID);
			cmd = screen.ShowCommands(_INTL("Personal ID is {1}.", persid),
																	new {_INTL("Set EVs"),
																	_INTL("Set IVs"),
																	_INTL("Randomise pID")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Set EVs
					cmd2 = 0;
					do { //loop; while (true);
						totalev = 0;
						evcommands = new List<string>();
						ev_id = new List<string>();
						foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
							evcommands.Add(s.name + $" ({pkmn.ev[s.id]})");
							ev_id.Add(s.id);
							totalev += pkmn.ev[s.id];
						}
						evcommands.Add(_INTL("Randomise all"));
						evcommands.Add(_INTL("Max randomise all"));
						cmd2 = screen.ShowCommands(_INTL("Change which EV?\nTotal: {1}/{2} ({3}%)",
																							totalev, Pokemon.EV_LIMIT,
																							100 * totalev / Pokemon.EV_LIMIT), evcommands, cmd2);
						if (cmd2 < 0) break;
						if (cmd2 < ev_id.length) {
							params = new ChooseNumberParams();
							upperLimit = 0;
							GameData.Stat.each_main(s => { if (s.id != ev_id[cmd2]) upperLimit += pkmn.ev[s.id]; });
							upperLimit = Pokemon.EV_LIMIT - upperLimit;
							upperLimit = (int)Math.Min(upperLimit, Pokemon.EV_STAT_LIMIT);
							thisValue = (int)Math.Min(pkmn.ev[ev_id[cmd2]], upperLimit);
							params.setRange(0, upperLimit);
							params.setDefaultValue(thisValue);
							params.setCancelValue(thisValue);
							f = MessageChooseNumber(_INTL("Set the EV for {1} (max. {2}).",
																							GameData.Stat.get(ev_id[cmd2]).name, upperLimit), params) { screen.Update };
							if (f != pkmn.ev[ev_id[cmd2]]) {
								pkmn.ev[ev_id[cmd2]] = f;
								pkmn.calc_stats;
								screen.RefreshSingle(pkmnid);
							}
						} else {   // (Max) Randomise all
							evTotalTarget = Pokemon.EV_LIMIT;
							if (cmd2 == evcommands.length - 2) {   // Randomize all (not max)
								evTotalTarget = rand(Pokemon.EV_LIMIT);
							}
							GameData.Stat.each_main(s => pkmn.ev[s.id] = 0);
							while (evTotalTarget > 0) {
								r = rand(ev_id.length);
								if (pkmn.ev[ev_id[r]] >= Pokemon.EV_STAT_LIMIT) continue;
								addVal = 1 + rand(Pokemon.EV_STAT_LIMIT / 4);
								addVal = addVal.clamp(0, evTotalTarget);
								addVal = addVal.clamp(0, Pokemon.EV_STAT_LIMIT - pkmn.ev[ev_id[r]]);
								if (addVal == 0) continue;
								pkmn.ev[ev_id[r]] += addVal;
								evTotalTarget -= addVal;
							}
							pkmn.calc_stats;
							screen.RefreshSingle(pkmnid);
						}
					}
					break;
				case 1:   // Set IVs
					cmd2 = 0;
					do { //loop; while (true);
						hiddenpower = HiddenPower(pkmn);
						totaliv = 0;
						ivcommands = new List<string>();
						iv_id = new List<string>();
						foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
							ivcommands.Add(s.name + $" ({pkmn.iv[s.id]})");
							iv_id.Add(s.id);
							totaliv += pkmn.iv[s.id];
						}
						msg = _INTL("Change which IV?\nHidden Power:\n{1}, power {2}\nTotal: {3}/{4} ({5}%)",
												GameData.Type.get(hiddenpower[0]).name, hiddenpower[1], totaliv,
												iv_id.length * Pokemon.IV_STAT_LIMIT, 100 * totaliv / (iv_id.length * Pokemon.IV_STAT_LIMIT));
						ivcommands.Add(_INTL("Randomise all"));
						cmd2 = screen.ShowCommands(msg, ivcommands, cmd2);
						if (cmd2 < 0) break;
						if (cmd2 < iv_id.length) {
							params = new ChooseNumberParams();
							params.setRange(0, Pokemon.IV_STAT_LIMIT);
							params.setDefaultValue(pkmn.iv[iv_id[cmd2]]);
							params.setCancelValue(pkmn.iv[iv_id[cmd2]]);
							f = MessageChooseNumber(_INTL("Set the IV for {1} (max. 31).",
																							GameData.Stat.get(iv_id[cmd2]).name), params) { screen.Update };
							if (f != pkmn.iv[iv_id[cmd2]]) {
								pkmn.iv[iv_id[cmd2]] = f;
								pkmn.calc_stats;
								screen.RefreshSingle(pkmnid);
							}
						} else {   // Randomise all
							GameData.Stat.each_main(s => pkmn.iv[s.id] = rand(Pokemon.IV_STAT_LIMIT + 1));
							pkmn.calc_stats;
							screen.RefreshSingle(pkmnid);
						}
					}
					break;
				case 2:   // Randomise pID
					pkmn.personalID = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
					pkmn.calc_stats;
					screen.RefreshSingle(pkmnid);
					break;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_happiness, {
	"name"   => _INTL("Set happiness"),
	"parent" => :level_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.happiness);
		h = MessageChooseNumber(
			_INTL("Set the Pokémon's happiness (max. 255)."), params,
			() => screen.Update())
		if (h != pkmn.happiness) {
			pkmn.happiness = h;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :contest_stats, {
	"name"   => _INTL("Contest stats..."),
	"parent" => :level_stats;
});

MenuHandlers.add(:pokemon_debug_menu, :set_beauty, {
	"name"   => _INTL("Set Beauty"),
	"parent" => :contest_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.beauty);
		newval = MessageChooseNumber(
			_INTL("Set the Pokémon's Beauty (max. 255)."), params,
			() => screen.Update())
		if (newval != pkmn.beauty) {
			pkmn.beauty = newval;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_cool, {
	"name"   => _INTL("Set Cool"),
	"parent" => :contest_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.cool);
		newval = MessageChooseNumber(
			_INTL("Set the Pokémon's Cool (max. 255)."), params,
			() => screen.Update())
		if (newval != pkmn.cool) {
			pkmn.cool = newval;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_cute, {
	"name"   => _INTL("Set Cute"),
	"parent" => :contest_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.cute);
		newval = MessageChooseNumber(
			_INTL("Set the Pokémon's Cute (max. 255)."), params,
			() => screen.Update())
		if (newval != pkmn.cute) {
			pkmn.cute = newval;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_smart, {
	"name"   => _INTL("Set Smart"),
	"parent" => :contest_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.smart);
		newval = MessageChooseNumber(
			_INTL("Set the Pokémon's Smart (max. 255)."), params,
			() => screen.Update())
		if (newval != pkmn.smart) {
			pkmn.smart = newval;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_tough, {
	"name"   => _INTL("Set Tough"),
	"parent" => :contest_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.tough);
		newval = MessageChooseNumber(
			_INTL("Set the Pokémon's Tough (max. 255)."), params,
			() => screen.Update())
		if (newval != pkmn.tough) {
			pkmn.tough = newval;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_sheen, {
	"name"   => _INTL("Set Sheen"),
	"parent" => :contest_stats,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		params = new ChooseNumberParams();
		params.setRange(0, 255);
		params.setDefaultValue(pkmn.sheen);
		newval = MessageChooseNumber(
			_INTL("Set the Pokémon's Sheen (max. 255)."), params,
			() => screen.Update())
		if (newval != pkmn.sheen) {
			pkmn.sheen = newval;
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

//===============================================================================
// Moves options.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :moves, {
	"name"   => _INTL("Moves..."),
	"parent" => :main;
});

MenuHandlers.add(:pokemon_debug_menu, :teach_move, {
	"name"   => _INTL("Teach move"),
	"parent" => :moves,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		move = ChooseMoveList;
		if (move) {
			LearnMove(pkmn, move);
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :forget_move, {
	"name"   => _INTL("Forget move"),
	"parent" => :moves,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		moveindex = screen.ChooseMove(pkmn, _INTL("Choose move to forget."));
		if (moveindex >= 0) {
			movename = pkmn.moves[moveindex].name;
			pkmn.forget_move_at_index(moveindex);
			screen.Display(_INTL("{1} forgot {2}.", pkmn.name, movename));
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :reset_moves, {
	"name"   => _INTL("Reset moves"),
	"parent" => :moves,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		pkmn.reset_moves;
		screen.Display(_INTL("{1}'s moves were reset.", pkmn.name));
		screen.RefreshSingle(pkmnid);
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_move_pp, {
	"name"   => _INTL("Set move PP"),
	"parent" => :moves,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			commands = new List<string>();
			foreach (var i in pkmn.moves) { //'pkmn.moves.each' do => |i|
				if (!i.id) break;
				if (i.total_pp <= 0) {
					commands.Add(_INTL("{1} (PP: ---)", i.name));
				} else {
					commands.Add(_INTL("{1} (PP: {2}/{3})", i.name, i.pp, i.total_pp));
				}
			}
			commands.Add(_INTL("Restore all PP"));
			cmd = screen.ShowCommands(_INTL("Alter PP of which move?"), commands, cmd);
			if (cmd < 0) break;
			if (cmd >= 0 && cmd < commands.length - 1) {   // Move
				move = pkmn.moves[cmd];
				movename = move.name;
				if (move.total_pp <= 0) {
					screen.Display(_INTL("{1} has infinite PP.", movename));
				} else {
					cmd2 = 0;
					do { //loop; while (true);
						msg = _INTL("{1}: PP {2}/{3} (PP Up {4}/3)", movename, move.pp, move.total_pp, move.ppup);
						cmd2 = screen.ShowCommands(msg,
																				new {_INTL("Set PP"),
																					_INTL("Full PP"),
																					_INTL("Set PP Up")}, cmd2);
						if (cmd2 < 0) break;
						switch (cmd2) {
							case 0:   // Change PP
								params = new ChooseNumberParams();
								params.setRange(0, move.total_pp);
								params.setDefaultValue(move.pp);
								h = MessageChooseNumber(
									_INTL("Set PP of {1} (max. {2}).", movename, move.total_pp), params,
									() => screen.Update())
								move.pp = h;
								break;
							case 1:   // Full PP
								move.pp = move.total_pp;
								break;
							case 2:   // Change PP Up
								params = new ChooseNumberParams();
								params.setRange(0, 3);
								params.setDefaultValue(move.ppup);
								h = MessageChooseNumber(
									_INTL("Set PP Up of {1} (max. 3).", movename), params,
									() => screen.Update())
								move.ppup = h;
								if (move.pp > move.total_pp) move.pp = move.total_pp;
								break;
						}
					}
				}
			} else if (cmd == commands.length - 1) {   // Restore all PP
				pkmn.heal_PP;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_initial_moves, {
	"name"   => _INTL("Reset initial moves"),
	"parent" => :moves,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		pkmn.record_first_moves;
		screen.Display(_INTL("{1}'s moves were set as its first-known moves.", pkmn.name));
		screen.RefreshSingle(pkmnid);
		next false;
	}
});

//===============================================================================
// Other options.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :set_item, {
	"name"   => _INTL("Set item"),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		commands = new {
			_INTL("Change item"),
			_INTL("Remove item");
		}
		do { //loop; while (true);
			msg = (pkmn.hasItem()) ? _INTL("Item is {1}.", pkmn.item.name) : _INTL("No item.");
			cmd = screen.ShowCommands(msg, commands, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Change item
					item = ChooseItemList(pkmn.item_id);
					if (item && item != pkmn.item_id) {
						pkmn.item = item;
						if (GameData.Item.get(item).is_mail()) {
							pkmn.mail = new Mail(item, _INTL("Text"), Game.GameData.player.name);
						}
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 1:   // Remove item
					if (pkmn.hasItem()) {
						pkmn.item = null;
						pkmn.mail = null;
						screen.RefreshSingle(pkmnid);
					}
					break;
				default:
					break; //break loop here as default value
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_ability, {
	"name"   => _INTL("Set ability"),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		commands = new {
			_INTL("Set possible ability"),
			_INTL("Set any ability"),
			_INTL("Reset");
		}
		do { //loop; while (true);
			if (pkmn.ability) {
				msg = _INTL("Ability is {1} (index {2}).", pkmn.ability.name, pkmn.ability_index);
			} else {
				msg = _INTL("No ability (index {1}).", pkmn.ability_index);
			}
			cmd = screen.ShowCommands(msg, commands, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Set possible ability
					abils = pkmn.getAbilityList;
					ability_commands = new List<string>();
					abil_cmd = 0;
					foreach (var i in abils) { //'abils.each' do => |i|
						ability_commands.Add(((i[1] < 2) ? "" : "(H) ") + GameData.Ability.get(i[0]).name);
						if (pkmn.ability_id == i[0]) abil_cmd = ability_commands.length - 1;
					}
					abil_cmd = screen.ShowCommands(_INTL("Choose an ability."), ability_commands, abil_cmd);
					if (abil_cmd < 0) continue;
					pkmn.ability_index = abils[abil_cmd][1];
					pkmn.ability = null;
					screen.RefreshSingle(pkmnid);
					break;
				case 1:   // Set any ability
					new_ability = ChooseAbilityList(pkmn.ability_id);
					if (new_ability && new_ability != pkmn.ability_id) {
						pkmn.ability = new_ability;
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 2:   // Reset
					pkmn.ability_index = null;
					pkmn.ability = null;
					screen.RefreshSingle(pkmnid);
					break;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_nature, {
	"name"   => _INTL("Set nature"),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		commands = new List<string>();
		ids = new List<string>();
		foreach (var nature in GameData.Nature) { //'GameData.Nature.each' do => |nature|
			if (nature.stat_changes.length == 0) {
				commands.Add(_INTL("{1} (---)", nature.real_name));
			} else {
				plus_text = "";
				minus_text = "";
				foreach (var change in nature.stat_changes) { //'nature.stat_changes.each' do => |change|
					if (change[1] > 0) {
						if (!plus_text.empty()) plus_text += "/";
						plus_text += GameData.Stat.get(change[0]).name_brief;
					} else if (change[1] < 0) {
						if (!minus_text.empty()) minus_text += "/";
						minus_text += GameData.Stat.get(change[0]).name_brief;
					}
				}
				commands.Add(_INTL("{1} (+{2}, -{3})", nature.real_name, plus_text, minus_text));
			}
			ids.Add(nature.id);
		}
		commands.Add(_INTL("[Reset]"));
		cmd = ids.index(pkmn.nature_id || ids[0]);
		do { //loop; while (true);
			msg = _INTL("Nature is {1}.", pkmn.nature.name);
			cmd = screen.ShowCommands(msg, commands, cmd);
			if (cmd < 0) break;
			if (cmd >= 0 && cmd < commands.length - 1) {   // Set nature
				pkmn.nature = ids[cmd];
			} else if (cmd == commands.length - 1) {   // Reset
				pkmn.nature = null;
			}
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_gender, {
	"name"   => _INTL("Set gender"),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (pkmn.singleGendered()) {
			screen.Display(_INTL("{1} is single-gendered or genderless.", pkmn.speciesName));
		} else {
			cmd = 0;
			do { //loop; while (true);
				msg = new {_INTL("Gender is male."), _INTL("Gender is female.")}[pkmn.male() ? 0 : 1];
				cmd = screen.ShowCommands(msg,
																		new {_INTL("Make male"),
																		_INTL("Make female"),
																		_INTL("Reset")}, cmd);
				if (cmd < 0) break;
				switch (cmd) {
					case 0:   // Make male
						pkmn.makeMale;
						if (!pkmn.male()) {
							screen.Display(_INTL("{1}'s gender couldn't be changed.", pkmn.name));
						}
						break;
					case 1:   // Make female
						pkmn.makeFemale;
						if (!pkmn.female()) {
							screen.Display(_INTL("{1}'s gender couldn't be changed.", pkmn.name));
						}
						break;
					case 2:   // Reset
						pkmn.gender = null;
						break;
				}
				if (!settingUpBattle && !pkmn.egg()) Game.GameData.player.pokedex.register(pkmn);
				screen.RefreshSingle(pkmnid);
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :species_and_form, {
	"name"   => _INTL("Species/form..."),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			msg = new {_INTL("Species {1}, form {2}.", pkmn.speciesName, pkmn.form),
						_INTL("Species {1}, form {2} (forced).", pkmn.speciesName, pkmn.form)}[(pkmn.forced_form.null()) ? 0 : 1];
			cmd = screen.ShowCommands(msg,
																	new {_INTL("Set species"),
																	_INTL("Set form"),
																	_INTL("Remove form override")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Set species
					species = ChooseSpeciesList(pkmn.species);
					if (species && species != pkmn.species) {
						pkmn.species = species;
						pkmn.calc_stats;
						if (!settingUpBattle && !pkmn.egg()) Game.GameData.player.pokedex.register(pkmn);
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 1:   // Set form
					cmd2 = 0;
					formcmds = new {[], new List<string>()}
					foreach (var sp in GameData.Species) { //'GameData.Species.each' do => |sp|
						if (sp.species != pkmn.species) continue;
						form_name = sp.form_name
						if (!form_name || form_name.empty()) form_name = _INTL("Unnamed form");
						form_name = string.Format("{0}: {0}", sp.form, form_name)
						formcmds[0].Add(sp.form)
						formcmds[1].Add(form_name)
						if (pkmn.form == sp.form) cmd2 = formcmds[0].length - 1;
					}
					if (formcmds[0].length <= 1) {
						screen.Display(_INTL("Species {1} only has one form.", pkmn.speciesName));
						if (pkmn.form != 0 && screen.Confirm(_INTL("Do you want to reset the form to 0?"))) {
							pkmn.form = 0;
							if (!settingUpBattle && !pkmn.egg()) Game.GameData.player.pokedex.register(pkmn);
							screen.RefreshSingle(pkmnid);
						}
					} else {
						cmd2 = screen.ShowCommands(_INTL("Set the Pokémon's form."), formcmds[1], cmd2);
						if (cmd2 < 0) continue;
						f = formcmds[0][cmd2];
						if (f != pkmn.form) {
							if (MultipleForms.hasFunction(pkmn, "getForm")) {
								if (!screen.Confirm(_INTL("This species decides its own form. Override?"))) continue;
								pkmn.forced_form = f;
							}
							pkmn.form = f;
							if (!settingUpBattle && !pkmn.egg()) Game.GameData.player.pokedex.register(pkmn);
							screen.RefreshSingle(pkmnid);
						}
					}
					break;
				case 2:   // Remove form override
					pkmn.forced_form = null;
					screen.RefreshSingle(pkmnid);
					break;
			}
		}
		next false;
	}
});

//===============================================================================
// Cosmetic options.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :cosmetic, {
	"name"   => _INTL("Cosmetic info..."),
	"parent" => :main;
});

MenuHandlers.add(:pokemon_debug_menu, :set_shininess, {
	"name"   => _INTL("Set shininess"),
	"parent" => :cosmetic,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			msg_idx = pkmn.shiny() ? (pkmn.super_shiny() ? 1 : 0) : 2;
			msg = new {_INTL("Is shiny."), _INTL("Is super shiny."), _INTL("Is normal (not shiny).")}[msg_idx];
			cmd = screen.ShowCommands(msg, new {_INTL("Make shiny"), _INTL("Make super shiny"),
																				_INTL("Make normal"), _INTL("Reset")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Make shiny
					pkmn.shiny = true;
					pkmn.super_shiny = false;
					break;
				case 1:   // Make super shiny
					pkmn.super_shiny = true;
					break;
				case 2:   // Make normal
					pkmn.shiny = false;
					pkmn.super_shiny = false;
					break;
				case 3:   // Reset
					pkmn.shiny = null;
					pkmn.super_shiny = null;
					break;
			}
			if (!settingUpBattle && !pkmn.egg()) Game.GameData.player.pokedex.register(pkmn);
			screen.RefreshSingle(pkmnid);
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_pokeball, {
	"name"   => _INTL("Set Poké Ball"),
	"parent" => :cosmetic,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		commands = new List<string>();
		balls = new List<string>();
		foreach (var item_data in GameData.Item) { //'GameData.Item.each' do => |item_data|
			if (item_data.is_poke_ball()) balls.Add(new {item_data.id, item_data.name});
		}
		balls.sort! { |a, b| a[1] <=> b[1] };
		cmd = 0;
		balls.each_with_index do |ball, i|
			if (ball[0] != pkmn.poke_ball) continue;
			cmd = i;
			break;
		}
		balls.each(ball => commands.Add(ball[1]));
		do { //loop; while (true);
			oldball = GameData.Item.get(pkmn.poke_ball).name;
			cmd = screen.ShowCommands(_INTL("{1} used.", oldball), commands, cmd);
			if (cmd < 0) break;
			pkmn.poke_ball = balls[cmd][0];
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_ribbons, {
	"name"   => _INTL("Set ribbons"),
	"parent" => :cosmetic,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			commands = new List<string>();
			ids = new List<string>();
			foreach (var ribbon_data in GameData.Ribbon) { //'GameData.Ribbon.each' do => |ribbon_data|
				commands.Add(_INTL("{1} {2}",
														(pkmn.hasRibbon(ribbon_data.id)) ? "[Y]" : "[  ]", ribbon_data.name))
				ids.Add(ribbon_data.id);
			}
			commands.Add(_INTL("Give all"));
			commands.Add(_INTL("Clear all"));
			cmd = screen.ShowCommands(_INTL("{1} ribbons.", pkmn.numRibbons), commands, cmd);
			if (cmd < 0) break;
			if (cmd >= 0 && cmd < ids.length) {   // Toggle ribbon
				if (pkmn.hasRibbon(ids[cmd])) {
					pkmn.takeRibbon(ids[cmd]);
				} else {
					pkmn.giveRibbon(ids[cmd]);
				}
			} else if (cmd == commands.length - 2) {   // Give all
				foreach (var ribbon_data in GameData.Ribbon) { //'GameData.Ribbon.each' do => |ribbon_data|
					pkmn.giveRibbon(ribbon_data.id);
				}
			} else if (cmd == commands.length - 1) {   // Clear all
				pkmn.clearAllRibbons;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :set_nickname, {
	"name"   => _INTL("Set nickname"),
	"parent" => :cosmetic,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			speciesname = pkmn.speciesName;
			msg = new {_INTL("{1} has the nickname {2}.", speciesname, pkmn.name),
						_INTL("{1} has no nickname.", speciesname)}[pkmn.nicknamed() ? 0 : 1];
			cmd = screen.ShowCommands(msg, new {_INTL("Rename"), _INTL("Erase name")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Rename
					oldname = (pkmn.nicknamed()) ? pkmn.name : "";
					newname = EnterPokemonName(_INTL("{1}'s nickname?", speciesname),
																			0, Pokemon.MAX_NAME_SIZE, oldname, pkmn);
					pkmn.name = newname;
					screen.RefreshSingle(pkmnid);
					break;
				case 1:   // Erase name
					pkmn.name = null;
					screen.RefreshSingle(pkmnid);
					break;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :ownership, {
	"name"   => _INTL("Ownership..."),
	"parent" => :cosmetic,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			gender = new {_INTL("Male"), _INTL("Female"), _INTL("Unknown")}[pkmn.owner.gender];
			msg = new {_INTL("Player's Pokémon\n{1}\n{2}\n{3} ({4})",
									pkmn.owner.name, gender, pkmn.owner.public_id, pkmn.owner.id),
						_INTL("Foreign Pokémon\n{1}\n{2}\n{3} ({4})",
									pkmn.owner.name, gender, pkmn.owner.public_id, pkmn.owner.id)}[pkmn.foreign(Game.GameData.player) ? 1 : 0];
			cmd = screen.ShowCommands(msg,
																	new {_INTL("Make player's"),
																	_INTL("Set OT's name"),
																	_INTL("Set OT's gender"),
																	_INTL("Random foreign ID"),
																	_INTL("Set foreign ID")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Make player's
					pkmn.owner = Pokemon.Owner.new_from_trainer(Game.GameData.player);
					break;
				case 1:   // Set OT's name
					pkmn.owner.name = EnterPlayerName(_INTL("{1}'s OT's name?", pkmn.name), 1, Settings.MAX_PLAYER_NAME_SIZE);
					break;
				case 2:   // Set OT's gender
					cmd2 = screen.ShowCommands(_INTL("Set OT's gender."),
																			new {_INTL("Male"), _INTL("Female"), _INTL("Unknown")}, pkmn.owner.gender);
					if (cmd2 >= 0) pkmn.owner.gender = cmd2;
					break;
				case 3:   // Random foreign ID
					pkmn.owner.id = Game.GameData.player.make_foreign_ID;
					break;
				case 4:   // Set foreign ID
					params = new ChooseNumberParams();
					params.setRange(0, 65_535);
					params.setDefaultValue(pkmn.owner.public_id);
					val = MessageChooseNumber(
						_INTL("Set the new ID (max. 65535)."), params,
						() => screen.Update())
					pkmn.owner.id = val | (val << 16);
					break;
			}
		}
		next false;
	}
});

//===============================================================================
// Can store/release/trade.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :set_discardable, {
	"name"   => _INTL("Set discardable"),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			msg = _INTL("Click option to toggle.");
			cmds = new List<string>();
			cmds.Add((pkmn.cannot_store) ? _INTL("Cannot store") : _INTL("Can store"));
			cmds.Add((pkmn.cannot_release) ? _INTL("Cannot release") : _INTL("Can release"));
			cmds.Add((pkmn.cannot_trade) ? _INTL("Cannot trade") : _INTL("Can trade"));
			cmd = screen.ShowCommands(msg, cmds, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Toggle storing
					pkmn.cannot_store = !pkmn.cannot_store;
					break;
				case 1:   // Toggle releasing
					pkmn.cannot_release = !pkmn.cannot_release;
					break;
				case 2:   // Toggle trading
					pkmn.cannot_trade = !pkmn.cannot_trade;
					break;
			}
		}
		next false;
	}
});

//===============================================================================
// Other options.
//===============================================================================

MenuHandlers.add(:pokemon_debug_menu, :set_egg, {
	"name"        => _INTL("Set egg"),
	"parent"      => :main,
	"always_show" => false,
	"effect"      => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			msg = new {_INTL("Not an egg"),
						_INTL("Egg (hatches in {1} steps).", pkmn.steps_to_hatch)}[pkmn.egg() ? 1 : 0];
			cmd = screen.ShowCommands(msg,
																	new {_INTL("Make egg"),
																	_INTL("Make Pokémon"),
																	_INTL("Set steps left to 1")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Make egg
					if (!pkmn.egg() && (HasEgg(pkmn.species) ||
						screen.Confirm(_INTL("{1} cannot legally be an egg. Make egg anyway?", pkmn.speciesName)))) {
						pkmn.level          = Settings.EGG_LEVEL;
						pkmn.calc_stats;
						pkmn.name           = _INTL("Egg");
						pkmn.steps_to_hatch = pkmn.species_data.hatch_steps;
						pkmn.hatched_map    = 0;
						pkmn.obtain_method  = 1;
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 1:   // Make Pokémon
					if (pkmn.egg()) {
						pkmn.name           = null;
						pkmn.steps_to_hatch = 0;
						pkmn.hatched_map    = 0;
						pkmn.obtain_method  = 0;
						if (!settingUpBattle) Game.GameData.player.pokedex.register(pkmn);
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 2:   // Set steps left to 1
					if (pkmn.egg()) pkmn.steps_to_hatch = 1;
					break;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :shadow_pkmn, {
	"name"   => _INTL("Shadow Pkmn..."),
	"parent" => :main,
	"effect" => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		cmd = 0;
		do { //loop; while (true);
			msg = new {_INTL("Not a Shadow Pokémon."),
						_INTL("Heart gauge is {1} (stage {2}).", pkmn.heart_gauge, pkmn.heartStage)}[pkmn.shadowPokemon() ? 1 : 0];
			cmd = screen.ShowCommands(msg, new {_INTL("Make Shadow"), _INTL("Set heart gauge")}, cmd);
			if (cmd < 0) break;
			switch (cmd) {
				case 0:   // Make Shadow
					if (pkmn.shadowPokemon()) {
						screen.Display(_INTL("{1} is already a Shadow Pokémon.", pkmn.name));
					} else {
						pkmn.makeShadow;
						screen.RefreshSingle(pkmnid);
					}
					break;
				case 1:   // Set heart gauge
					if (pkmn.shadowPokemon()) {
						oldheart = pkmn.heart_gauge;
						params = new ChooseNumberParams();
						params.setRange(0, pkmn.max_gauge_size);
						params.setDefaultValue(pkmn.heart_gauge);
						val = MessageChooseNumber(
							_INTL("Set the heart gauge (max. {1}).", pkmn.max_gauge_size),
							params,
							() => screen.Update())
						if (val != oldheart) {
							pkmn.adjustHeart(val - oldheart);
							pkmn.check_ready_to_purify;
						}
					} else {
						screen.Display(_INTL("{1} is not a Shadow Pokémon.", pkmn.name));
					}
					break;
			}
		}
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :mystery_gift, {
	"name"        => _INTL("Mystery Gift"),
	"parent"      => :main,
	"always_show" => false,
	"effect"      => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		CreateMysteryGift(0, pkmn);
		next false;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :duplicate, {
	"name"        => _INTL("Duplicate"),
	"parent"      => :main,
	"always_show" => false,
	"effect"      => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (!screen.Confirm(_INTL("Are you sure you want to copy this Pokémon?"))) next false;
		clonedpkmn = pkmn.clone;
		switch (screen) {
			case PokemonPartyScreen:
				StorePokemon(clonedpkmn);
				screen.HardRefresh;
				screen.Display(_INTL("The Pokémon was duplicated."));
				break;
			case PokemonStorageScreen:
				if (screen.storage.MoveCaughtToParty(clonedpkmn)) {
					if (pkmnid[0] != -1) {
						screen.Display(_INTL("The duplicated Pokémon was moved to your party."));
					}
				} else {
					oldbox = screen.storage.currentBox;
					newbox = screen.storage.StoreCaught(clonedpkmn);
					if (newbox < 0) {
						screen.Display(_INTL("All boxes are full."));
					} else if (newbox != oldbox) {
						screen.Display(_INTL("The duplicated Pokémon was moved to box \"{1}.\"", screen.storage[newbox].name));
						screen.storage.currentBox = oldbox;
					}
				}
				screen.HardRefresh;
				break;
		}
		next true;
	}
});

MenuHandlers.add(:pokemon_debug_menu, :delete, {
	"name"        => _INTL("Delete"),
	"parent"      => :main,
	"always_show" => false,
	"effect"      => (pkmn, pkmnid, heldpoke, settingUpBattle, screen) => {
		if (!screen.Confirm(_INTL("Are you sure you want to delete this Pokémon?"))) next false;
		switch (screen) {
			case PokemonPartyScreen:
				screen.party.delete_at(pkmnid);
				screen.HardRefresh;
				break;
			case PokemonStorageScreen:
				screen.scene.Release(pkmnid, heldpoke);
				(heldpoke) ? screen.heldpkmn = null : screen.storage.Delete(pkmnid[0], pkmnid[1])
				screen.scene.Refresh;
				break;
		}
		next true;
	}
});
