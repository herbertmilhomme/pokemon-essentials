//===============================================================================
//
//===============================================================================
public partial class BattlePalaceBattle : Battle {
	// Percentage chances of choosing attack, defense, support moves
	@@BattlePalaceUsualTable = {
		HARDY   = new {61,  7, 32},
		LONELY  = new {20, 25, 55},
		BRAVE   = new {70, 15, 15},
		ADAMANT = new {38, 31, 31},
		NAUGHTY = new {20, 70, 10},
		BOLD    = new {30, 20, 50},
		DOCILE  = new {56, 22, 22},
		RELAXED = new {25, 15, 60},
		IMPISH  = new {69,  6, 25},
		LAX     = new {35, 10, 55},
		TIMID   = new {62, 10, 28},
		HASTY   = new {58, 37,  5},
		SERIOUS = new {34, 11, 55},
		JOLLY   = new {35,  5, 60},
		NAIVE   = new {56, 22, 22},
		MODEST  = new {35, 45, 20},
		MILD    = new {44, 50,  6},
		QUIET   = new {56, 22, 22},
		BASHFUL = new {30, 58, 12},
		RASH    = new {30, 13, 57},
		CALM    = new {40, 50, 10},
		GENTLE  = new {18, 70, 12},
		SASSY   = new {88,  6,  6},
		CAREFUL = new {42, 50,  8},
		QUIRKY  = new {56, 22, 22}
	}
	@@BattlePalacePinchTable = {
		HARDY   = new {61,  7, 32},
		LONELY  = new {84,  8,  8},
		BRAVE   = new {32, 60,  8},
		ADAMANT = new {70, 15, 15},
		NAUGHTY = new {70, 22,  8},
		BOLD    = new {32, 58, 10},
		DOCILE  = new {56, 22, 22},
		RELAXED = new {75, 15, 10},
		IMPISH  = new {28, 55, 17},
		LAX     = new {29,  6, 65},
		TIMID   = new {30, 20, 50},
		HASTY   = new {88,  6,  6},
		SERIOUS = new {29, 11, 60},
		JOLLY   = new {35, 60,  5},
		NAIVE   = new {56, 22, 22},
		MODEST  = new {34, 60,  6},
		MILD    = new {34,  6, 60},
		QUIET   = new {56, 22, 22},
		BASHFUL = new {30, 58, 12},
		RASH    = new {27,  6, 67},
		CALM    = new {25, 62, 13},
		GENTLE  = new {90,  5,  5},
		SASSY   = new {22, 20, 58},
		CAREFUL = new {42,  5, 53},
		QUIRKY  = new {56, 22, 22}
	}

	public override void initialize(*arg) {
		base.initialize();
		@justswitched          = new {false, false, false, false};
		@battleAI.battlePalace = true;
	}

	public void MoveCategory(move) {
		if (move.target == :User || move.function_code == "MultiTurnAttackBideThenReturnDoubleDamage") {
			return 1;
		} else if (move.statusMove() ||
					move.function_code == "CounterPhysicalDamage" || move.function_code == "CounterSpecialDamage") {
			return 2;
		} else {
			return 0;
		}
	}

	// Different implementation of CanChooseMove, ignores Imprison/Torment/Taunt/Disable/Encore
	public bool CanChooseMovePartial(idxPokemon, idxMove) {
		thispkmn = @battlers[idxPokemon];
		thismove = thispkmn.moves[idxMove];
		if (!thismove) return false;
		if (thismove.pp <= 0) return false;
		if (thispkmn.effects.ChoiceBand &&
			thismove.id != thispkmn.effects.ChoiceBand &&
			thispkmn.hasActiveItem(Items.CHOICEBAND)) {
			return false;
		}
		// though incorrect, just for convenience (actually checks Torment later)
		if (thispkmn.effects.Torment &&
			thispkmn.lastMoveUsed && thismove.id == thispkmn.lastMoveUsed) {
			return false;
		}
		return true;
	}

	public void RegisterMove(idxBattler, idxMove, _showMessages = true) {
		this_battler = @battlers[idxBattler];
		@choices[idxBattler].Action = :UseMove;
		@choices[idxBattler].Index = idxMove;   // Index of move to be used (-2="Incapable of using its power...")
		@choices[idxBattler].Move = (idxMove == -2) ? @struggle : this_battler.moves[idxMove];   // Battle.Move object
		@choices[idxBattler].Target = -1;   // No target chosen yet
	}

	public void AutoFightMenu(idxBattler) {
		this_battler = @battlers[idxBattler];
		nature = this_battler.nature.id;
		randnum = @battleAI.AIRandom(100);
		category = 0;
		atkpercent = 0;
		defpercent = 0;
		if (this_battler.effects.Pinch) {
			atkpercent = @@BattlePalacePinchTable[nature][0];
			defpercent = atkpercent + @@BattlePalacePinchTable[nature][1];
		} else {
			atkpercent = @@BattlePalaceUsualTable[nature][0];
			defpercent = atkpercent + @@BattlePalaceUsualTable[nature][1];
		}
		if (randnum < atkpercent) {
			category = 0;
		} else if (randnum < atkpercent + defpercent) {
			category = 1;
		} else {
			category = 2;
		}
		moves = new List<string>();
		for (int i = this_battler.moves.length; i < this_battler.moves.length; i++) { //for 'this_battler.moves.length' times do => |i|
			if (!CanChooseMovePartial(idxBattler, i)) continue;
			if (MoveCategory(this_battler.moves[i]) != category) continue;
			moves[moves.length] = i;
		}
		if (moves.length == 0) {
			// No moves of selected category
			RegisterMove(idxBattler, -2);
		} else {
			chosenmove = moves[@battleAI.AIRandom(moves.length)];
			RegisterMove(idxBattler, chosenmove);
		}
		return true;
	}

	public void PinchChange(battler) {
		if (!battler || battler.fainted()) return;
		if (battler.effects.Pinch || battler.status == statuses.SLEEP) return;
		if (battler.hp > battler.totalhp / 2) return;
		nature = battler.nature.id;
		battler.effects.Pinch = true;
		switch (nature) {
			case :QUIET: case :BASHFUL: case :NAIVE: case :QUIRKY: case :HARDY: case :DOCILE: case :SERIOUS:
				Display(_INTL("{1} is eager for more!", battler.ToString()));
				break;
			case :CAREFUL: case :RASH: case :LAX: case :SASSY: case :MILD: case :TIMID:
				Display(_INTL("{1} began growling deeply!", battler.ToString()));
				break;
			case :GENTLE: case :ADAMANT: case :HASTY: case :LONELY: case :RELAXED: case :NAUGHTY:
				Display(_INTL("A glint appears in {1}'s eyes!", battler.ToString(true)));
				break;
			case :JOLLY: case :BOLD: case :BRAVE: case :CALM: case :IMPISH: case :MODEST:
				Display(_INTL("{1} is getting into position!", battler.ToString()));
				break;
		}
	}

	public override void EndOfRoundPhase() {
		base.EndOfRoundPhase();
		if (decided()) return;
		allBattlers.each(b => PinchChange(b));
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	public int battlePalace		{ get { return _battlePalace; } set { _battlePalace = value; } }			protected int _battlePalace;

	unless (private_method_defined(:_battlePalace_initialize)) alias _battlePalace_initialize initialize;

	public void initialize(*arg) {
		_battlePalace_initialize(*arg);
		@justswitched = new {false, false, false, false};
	}

	unless (method_defined(:_battlePalace_pbChooseToSwitchOut)) {
		alias _battlePalace_pbChooseToSwitchOut ChooseToSwitchOut;
	}

	public void ChooseToSwitchOut(force_switch = false) {
		if (!@battlePalace) return _battlePalace_pbChooseToSwitchOut(force_switch);
		thispkmn = @user;
		idxBattler = @user.index;
		shouldswitch = false;
		if (thispkmn.effects.PerishSong == 1) {
			shouldswitch = true;
		} else if (!@battle.CanChooseAnyMove(idxBattler) &&
					thispkmn.turnCount && thispkmn.turnCount > 5) {
			shouldswitch = true;
		} else {
			hppercent = thispkmn.hp * 100 / thispkmn.totalhp;
			percents = new List<string>();
			maxindex = -1;
			maxpercent = 0;
			factor = 0;
			@battle.Party(idxBattler).each_with_index do |pkmn, i|
				if (@battle.CanSwitch(idxBattler, i)) {
					percents[i] = 100 * pkmn.hp / pkmn.totalhp;
					if (percents[i] > maxpercent) {
						maxindex = i;
						maxpercent = percents[i];
					}
				} else {
					percents[i] = 0;
				}
			}
			if (hppercent < 50) {
				factor = (maxpercent < hppercent) ? 20 : 40;
			}
			if (hppercent < 25) {
				factor = (maxpercent < hppercent) ? 30 : 50;
			}
			switch (thispkmn.status) {
				case :SLEEP: case :FROZEN:
					factor += 20;
					break;
				case :POISON: case :BURN:
					factor += 10;
					break;
				case :PARALYSIS:
					factor += 15;
					break;
			}
			if (@justswitched[idxBattler]) {
				factor -= 60;
				if (factor < 0) factor = 0;
			}
			shouldswitch = (AIRandom(100) < factor);
			if (shouldswitch && maxindex >= 0) {
				@battle.RegisterSwitch(idxBattler, maxindex);
				return true;
			}
		}
		@justswitched[idxBattler] = shouldswitch;
		if (shouldswitch) {
			@battle.Party(idxBattler).each_with_index do |_pkmn, i|
				if (!@battle.CanSwitch(idxBattler, i)) continue;
				@battle.RegisterSwitch(idxBattler, i);
				return true;
			}
		}
		return false;
	}
}
