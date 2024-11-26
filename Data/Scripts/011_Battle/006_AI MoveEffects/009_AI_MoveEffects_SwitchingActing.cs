//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("FleeFromBattle",
	block: (move, user, ai, battle) => {
		next !battle.CanRun(user.index) || (user.wild() && user.battler.allAllies.length > 0);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("FleeFromBattle",
	block: (score, move, user, ai, battle) => {
		// Generally don't prefer (don't want to end the battle too easily)
		next score - 20;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("SwitchOutUserStatusMove",
	block: (move, user, ai, battle) => {
		if (user.wild()) {
			next !battle.CanRun(user.index) || user.battler.allAllies.length > 0;
		}
		next !battle.CanChooseNonActive(user.index);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("SwitchOutUserStatusMove",
	block: (score, move, user, ai, battle) => {
		// Wild Pokémon run from battle - generally don't prefer (don't want to end the battle too easily)
		if (user.wild()) next score - 20;
		// Trainer-owned Pokémon switch out
		if (ai.trainer.has_skill_flag("ReserveLastPokemon") && battle.TeamAbleNonActiveCount(user.index) == 1) {
			next Battle.AI.MOVE_USELESS_SCORE;   // Don't switch in ace
		}
		// Prefer if the user switching out will lose a negative effect
		if (user.effects.PerishSong > 0) score += 20;
		if (user.effects.Confusion > 1) score += 10;
		if (user.effects.Attract >= 0) score += 10;
		// Consider the user's stat stages
		if (user.stages.any((key, val) => val >= 2)) {
			score -= 15;
		} else if (user.stages.any((key, val) => val < 0)) {
			score += 10;
		}
		// Consider the user's end of round damage/healing
		eor_damage = user.rough_end_of_round_damage;
		if (eor_damage > 0) score += 15;
		if (eor_damage < 0) score -= 15;
		// Prefer if the user doesn't have any damaging moves
		if (!user.check_for_move(m => m.damagingMove())) score += 10;
		// Don't prefer if the user's side has entry hazards on it
		if (user.OwnSide.effects.Spikes > 0) score -= 10;
		if (user.OwnSide.effects.ToxicSpikes > 0) score -= 10;
		if (user.OwnSide.effects.StealthRock) score -= 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("SwitchOutUserDamagingMove",
	block: (score, move, user, ai, battle) => {
		if (!battle.CanChooseNonActive(user.index)) next score;
		// Don't want to switch in ace
		if (ai.trainer.has_skill_flag("ReserveLastPokemon") &&
									battle.TeamAbleNonActiveCount(user.index) == 1) score -= 20;
		// Prefer if the user switching out will lose a negative effect
		if (user.effects.PerishSong > 0) score += 20;
		if (user.effects.Confusion > 1) score += 10;
		if (user.effects.Attract >= 0) score += 10;
		// Consider the user's stat stages
		if (user.stages.any((key, val) => val >= 2)) {
			score -= 15;
		} else if (user.stages.any((key, val) => val < 0)) {
			score += 10;
		}
		// Consider the user's end of round damage/healing
		eor_damage = user.rough_end_of_round_damage;
		if (eor_damage > 0) score += 15;
		if (eor_damage < 0) score -= 15;
		// Don't prefer if the user's side has entry hazards on it
		if (user.OwnSide.effects.Spikes > 0) score -= 10;
		if (user.OwnSide.effects.ToxicSpikes > 0) score -= 10;
		if (user.OwnSide.effects.StealthRock) score -= 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetAtkSpAtk1SwitchOutUser",
	block: (move, user, target, ai, battle) => {
		will_fail = true;
		for (int i = (move.move.statDown.length / 2); i < (move.move.statDown.length / 2); i++) { //for '(move.move.statDown.length / 2)' times do => |i|
			if (!target.battler.CanLowerStatStage(move.move.statDown[i * 2], user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("LowerTargetAtkSpAtk1SwitchOutUser",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_drop(score, target, move.move.statDown, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("SwitchOutUserDamagingMove",
																												"LowerTargetAtkSpAtk1SwitchOutUser");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("SwitchOutUserPassOnEffects",
	block: (move, user, ai, battle) => {
		next !battle.CanChooseNonActive(user.index);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("SwitchOutUserPassOnEffects",
	block: (score, move, user, ai, battle) => {
		// Don't want to switch in ace
		if (ai.trainer.has_skill_flag("ReserveLastPokemon") &&
									battle.TeamAbleNonActiveCount(user.index) == 1) score -= 20;
		// Don't prefer if the user will pass on a negative effect
		if (user.effects.Confusion > 1) score -= 10;
		if (user.effects.Curse) score -= 15;
		if (user.effects.Embargo > 1) score -= 10;
		if (user.effects.GastroAcid) score -= 15;
		if (user.effects.HealBlock > 1) score -= 10;
		if (user.effects.LeechSeed >= 0) score -= 10;
		if (user.effects.PerishSong > 0) score -= 20;
		// Prefer if the user will pass on a positive effect
		if (user.effects.AquaRing) score += 10;
		if (user.effects.FocusEnergy > 0) score += 10;
		if (user.effects.Ingrain) score += 10;
		if (user.effects.MagnetRise > 1) score += 8;
		if (user.effects.Substitute > 0) score += 10;
		// Consider the user's stat stages
		if (user.stages.any((key, val) => val >= 4)) {
			score += 25;
		} else if (user.stages.any((key, val) => val >= 2)) {
			score += 15;
		} else if (user.stages.any((key, val) => val < 0)) {
			score -= 15;
		}
		// Consider the user's end of round damage/healing
		eor_damage = user.rough_end_of_round_damage;
		if (eor_damage > 0) score += 15;
		if (eor_damage < 0) score -= 15;
		// Prefer if the user doesn't have any damaging moves
		if (!user.check_for_move(m => m.damagingMove())) score += 15;
		// Don't prefer if the user's side has entry hazards on it
		if (user.OwnSide.effects.Spikes > 0) score -= 10;
		if (user.OwnSide.effects.ToxicSpikes > 0) score -= 10;
		if (user.OwnSide.effects.StealthRock) score -= 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SwitchOutTargetStatusMove",
	block: (move, user, target, ai, battle) => {
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SwitchOutTargetStatusMove",
	block: (score, move, user, target, ai, battle) => {
		// Ends the battle - generally don't prefer (don't want to end the battle too easily)
		if (target.wild()) next score - 10;
		// Switches the target out
		if (target.effects.PerishSong > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer if target is at low HP and could be knocked out instead
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp <= target.totalhp / 3) score -= 10;
		}
		// Consider the target's stat stages
		if (target.stages.any((key, val) => val >= 2)) {
			score += 15;
		} else if (target.stages.any((key, val) => val < 0)) {
			score -= 15;
		}
		// Consider the target's end of round damage/healing
		eor_damage = target.rough_end_of_round_damage;
		if (eor_damage > 0) score -= 15;
		if (eor_damage < 0) score += 15;
		// Prefer if the target's side has entry hazards on it
		if (target.OwnSide.effects.Spikes > 0) score += 10;
		if (target.OwnSide.effects.ToxicSpikes > 0) score += 10;
		if (target.OwnSide.effects.StealthRock) score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SwitchOutTargetDamagingMove",
	block: (score, move, user, target, ai, battle) => {
		if (target.wild()) next score;
		// No score modification if the target can't be made to switch out
		if (target.has_active_ability(abilitys.SUCTIONCUPS) && !target.being_mold_broken()) next score;
		if (target.effects.Ingrain) next score;
		// No score modification if the target can't be replaced
		can_switch = false;
		battle.eachInTeamFromBattlerIndex(target.index) do |_pkmn, i|
			can_switch = battle.CanSwitchIn(target.index, i);
			if (can_switch) break;
		}
		if (!can_switch) next score;
		// Not score modification if the target has a Substitute
		if (target.effects.Substitute > 0) next score;
		// Don't want to switch out the target if it will faint from Perish Song
		if (target.effects.PerishSong > 0) score -= 20;
		// Consider the target's stat stages
		if (target.stages.any((key, val) => val >= 2)) {
			score += 15;
		} else if (target.stages.any((key, val) => val < 0)) {
			score -= 15;
		}
		// Consider the target's end of round damage/healing
		eor_damage = target.rough_end_of_round_damage;
		if (eor_damage > 0) score -= 15;
		if (eor_damage < 0) score += 15;
		// Prefer if the target's side has entry hazards on it
		if (target.OwnSide.effects.Spikes > 0) score += 10;
		if (target.OwnSide.effects.ToxicSpikes > 0) score += 10;
		if (target.OwnSide.effects.StealthRock) score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("BindTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.effects.Trapping > 0) next score;
		if (target.effects.Substitute > 0) next score;
		// Prefer if the user has a Binding Band or Grip Claw (because why have it if
		// you don't want to use it())
		if (user.has_active_item(new {:BINDINGBAND, :GRIPCLAW})) score += 4;
		// Target will take damage at the end of each round from the binding
		if (target.battler.takesIndirectDamage()) score += 8;
		// Check whether the target will be trapped in battle by the binding
		if (target.can_become_trapped()) {
			score += 4;   // Prefer if the target will become trapped by this move
			eor_damage = target.rough_end_of_round_damage;
			if (eor_damage > 0) {
				// Prefer if the target will take damage at the end of each round on top
				// of binding damage
				score += 5;
			} else if (eor_damage < 0) {
				// Don't prefer if the target will heal itself at the end of each round
				score -= 5;
			}
			// Prefer if the target has been Perish Songed
			if (target.effects.PerishSong > 0) score += 10;
		}
		// Don't prefer if the target can remove the binding (and the binding has an
		// effect)
		if (target.can_become_trapped() || target.battler.takesIndirectDamage()) {
			if (ai.trainer.medium_skill() &&
				target.has_move_with_function("RemoveUserBindingAndEntryHazards")) {
				score -= 8;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("BindTargetDoublePowerIfTargetUnderwater",
	block: (power, move, user, target, ai, battle) => {
		next move.move.ModifyDamage(power, user.battler, target.battler);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("BindTarget",
																												"BindTargetDoublePowerIfTargetUnderwater");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TrapTargetInBattle",
	block: (move, user, target, ai, battle) => {
		if (move.damagingMove()) next false;
		if (target.effects.MeanLook >= 0) next true;
		if (Settings.MORE_TYPE_EFFECTS && target.has_type(types.GHOST)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TrapTargetInBattle",
	block: (score, move, user, target, ai, battle) => {
		if (!target.can_become_trapped() || !battle.CanChooseNonActive(target.index)) {
			next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
		}
		// Not worth trapping if target will faint this round anyway
		eor_damage = target.rough_end_of_round_damage;
		if (eor_damage >= target.hp) {
			next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
		}
		// Not worth trapping if target can remove the binding
		if (ai.trainer.medium_skill() &&
			target.has_move_with_function("RemoveUserBindingAndEntryHazards")) {
			next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
		}
		// Score for being an additional effect
		add_effect = move.get_score_change_for_additional_effect(user, target);
		if (add_effect == -999) next score;   // Additional effect will be negated
		score += add_effect;
		// Score for target becoming trapped in battle
		if (target.effects.PerishSong > 0 ||
			target.effects.Attract >= 0 ||
			target.effects.Confusion > 0 ||
			eor_damage > 0) {
			score += 15;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("TrapTargetInBattle",
																												"TrapTargetInBattleMainEffect");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("TrapTargetInBattle",
																												"TrapTargetInBattleMainEffect");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TrapTargetInBattleLowerTargetDefSpDef1EachTurn",
	block: (move, user, target, ai, battle) => {
		if (move.damagingMove()) next false;
		if (target.effects.Octolock >= 0) next true;
		if (Settings.MORE_TYPE_EFFECTS && target.has_type(types.GHOST)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TrapTargetInBattleLowerTargetDefSpDef1EachTurn",
	block: (score, move, user, target, ai, battle) => {
		// Score for stat drop
		score = ai.get_score_for_target_stat_drop(score, target, new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1}, false);
		// Score for target becoming trapped in battle
		if (target.can_become_trapped() && battle.CanChooseNonActive(target.index)) {
			// Not worth trapping if target will faint this round anyway
			eor_damage = target.rough_end_of_round_damage;
			if (eor_damage >= target.hp) {
				next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
			}
			// Not worth trapping if target can remove the binding
			if (target.has_move_with_function("RemoveUserBindingAndEntryHazards")) {
				next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
			}
			// Score for target becoming trapped in battle
			if (target.effects.PerishSong > 0 ||
				target.effects.Attract >= 0 ||
				target.effects.Confusion > 0 ||
				eor_damage > 0) {
				score += 15;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TrapUserAndTargetInBattle",
	block: (score, move, user, target, ai, battle) => {
		// NOTE: Don't worry about scoring for the user also becoming trapped in
		//       battle, because it knows this move and accepts what it's getting
		//       itself into.
		if (target.can_become_trapped() && battle.CanChooseNonActive(target.index)) {
			// Not worth trapping if target will faint this round anyway
			eor_damage = target.rough_end_of_round_damage;
			if (eor_damage >= target.hp) {
				next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
			}
			// Not worth trapping if target can remove the binding
			if (ai.trainer.medium_skill() &&
				target.has_move_with_function("RemoveUserBindingAndEntryHazards")) {
				next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
			}
			// Score for target becoming trapped in battle
			if (target.effects.PerishSong > 0 ||
				target.effects.Attract >= 0 ||
				target.effects.Confusion > 0 ||
				eor_damage > 0) {
				score += 15;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("TrapAllBattlersInBattleForOneTurn",
	block: (move, user, ai, battle) => {
		next battle.field.effects.FairyLock > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("TrapAllBattlersInBattleForOneTurn",
	block: (score, move, user, ai, battle) => {
		// Trapping for just one turn isn't so significant, so generally don't prefer
		next score - 10;
	}
)

//===============================================================================
//
//===============================================================================
// PursueSwitchingFoe

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UsedAfterUserTakesPhysicalDamage",
	block: (move, user, ai, battle) => {
		found_physical_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.check_for_move(m => m.physicalMove(m.type))) continue;
			found_physical_move = true;
			break;
		}
		next !found_physical_move;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("UsedAfterUserTakesPhysicalDamage",
	block: (score, move, user, ai, battle) => {
		if (user.effects.Substitute > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if foes don't know any special moves
		found_special_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.check_for_move(m => m.specialMove(m.type))) continue;
			found_special_move = true;
			break;
		}
		if (!found_special_move) score += 10;
		// Generally not worth using
		next score - 10;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("UsedAfterAllyRoundWithDoublePower",
	block: (score, move, user, ai, battle) => {
		// No score change if no allies know this move
		ally_has_move = false;
		ai.each_same_side_battler(user.side) do |b, i|
			if (!b.has_move_with_function(move.function_code)) continue;
			ally_has_move = true;
			break;
		}
		if (!ally_has_move) next score;
		// Prefer for the sake of doubling in power
		score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TargetActsNext",
	block: (score, move, user, target, ai, battle) => {
		// Useless if the target is a foe
		if (target.opposes(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Compare the speeds of all battlers
		speeds = new List<string>();
		ai.each_battler { |b, i| speeds.Add(new {i, b.rough_stat(:SPEED)}) };
		if (battle.field.effects.TrickRoom > 0) {
			speeds.sort! { |a, b| a[1] <=> b[1] };
		} else {
			speeds.sort! { |a, b| b[1] <=> a[1] };
		}
		idx_user = speeds.index(ele => ele[0] == user.index);
		idx_target = speeds.index(ele => ele[0] == target.index);
		// Useless if the target is faster than the user
		if (idx_target < idx_user) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if the target will move next anyway
		if (idx_target - idx_user <= 1) next Battle.AI.MOVE_USELESS_SCORE;
		// Generally not worth using
		// NOTE: Because this move can be used against a foe but is being used on an
		//       ally (since we're here in this code), this move's score will be
		//       inverted later. A higher score here means this move will be less
		//       preferred, which is the result we want.
		next score + 10;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TargetActsLast",
	block: (score, move, user, target, ai, battle) => {
		// Useless if the target is an ally
		if (!target.opposes(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if the user has no ally (the point of this move is to let the ally
		// get in a hit before the foe)
		has_ally = false;
		if (b.can_attack() }) ai.each_ally(user.index) { |b, i| has_ally = true;
		if (!has_ally) next Battle.AI.MOVE_USELESS_SCORE;
		// Compare the speeds of all battlers
		speeds = new List<string>();
		ai.each_battler { |b, i| speeds.Add(new {i, b.rough_stat(:SPEED)}) };
		if (battle.field.effects.TrickRoom > 0) {
			speeds.sort! { |a, b| a[1] <=> b[1] };
		} else {
			speeds.sort! { |a, b| b[1] <=> a[1] };
		}
		idx_user = speeds.index(ele => ele[0] == user.index);
		idx_target = speeds.index(ele => ele[0] == target.index);
		idx_slowest_ally = -1;
		speeds.each_with_index((ele, i) => { if (user.index.even() == ele[0].even()) idx_slowest_ally = i; });
		// Useless if the target is faster than the user
		if (idx_target < idx_user) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if the target will move last anyway
		if (idx_target == speeds.length - 1) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if the slowest ally is faster than the target
		if (idx_slowest_ally < idx_target) next Battle.AI.MOVE_USELESS_SCORE;
		// Generally not worth using
		next score - 10;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TargetUsesItsLastUsedMoveAgain",
	block: (move, user, target, ai, battle) => {
		next target.battler.usingMultiTurnAttack();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TargetUsesItsLastUsedMoveAgain",
	block: (score, move, user, target, ai, battle) => {
		// We don't ever want to make a foe act again
		if (target.opposes(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if target will act before the user, as we don't know what move
		// will be instructed
		if (target.faster_than(user)) next Battle.AI.MOVE_USELESS_SCORE;
		if (!target.battler.lastRegularMoveUsed) next Battle.AI.MOVE_USELESS_SCORE;
		mov = null;
		foreach (var m in target.battler.Moves) { //target.battler.eachMove do => |m|
			if (m.id == target.battler.lastRegularMoveUsed) mov = m;
			if (mov) break;
		}
		if (mov.null() || (mov.pp == 0 && mov.total_pp > 0)) next Battle.AI.MOVE_USELESS_SCORE;
		if (move.move.moveBlacklist.Contains(mov.function_code)) next Battle.AI.MOVE_USELESS_SCORE;
		// Without lots of code here to determine good/bad moves, using this move is
		// likely to just be a waste of a turn
		// NOTE: Because this move can be used against a foe but is being used on an
		//       ally (since we're here in this code), this move's score will be
		//       inverted later. A higher score here means this move will be less
		//       preferred, which is the result we want.
		score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("StartSlowerBattlersActFirst",
	block: (score, move, user, ai, battle) => {
		// Get the speeds of all battlers
		ally_speeds = new List<string>();
		foe_speeds = new List<string>();
		ai.each_battler do |b, i|
			if (b.opposes(user)) {
				foe_speeds.Add(b.rough_stat(:SPEED));
				if (user.OpposingSide.effects.Tailwind > 1) foe_speeds.last *= 2;
				if (user.OpposingSide.effects.Swamp > 1) foe_speeds.last /= 2;
			} else {
				ally_speeds.Add(b.rough_stat(:SPEED));
				if (user.OwnSide.effects.Tailwind > 1) ally_speeds.last *= 2;
				if (user.OwnSide.effects.Swamp > 1) ally_speeds.last /= 2;
			}
		}
		// Just in case a side has no battlers
		if (ally_speeds.length == 0 || foe_speeds.length == 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Invert the speeds if Trick Room applies (and will last longer than this round)
		if (battle.field.effects.TrickRoom > 1) {
			foe_speeds.map! { |val| 100_000 - val };    // 100_000 is higher than speed can
			ally_speeds.map! { |val| 100_000 - val };   // possibly be; only order matters
		}
		// Score based on the relative speeds
		if (ally_speeds.min > foe_speeds.max) next Battle.AI.MOVE_USELESS_SCORE;
		if (foe_speeds.min > ally_speeds.max) {
			score += 20;
		} else if (ally_speeds.sum / ally_speeds.length < foe_speeds.sum / foe_speeds.length) {
			score += 10;
		} else {
			score -= 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// HigherPriorityInGrassyTerrain

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("LowerPPOfTargetLastMoveBy3",
	block: (score, move, user, target, ai, battle) => {
		add_effect = move.get_score_change_for_additional_effect(user, target);
		if (add_effect == -999) next score;   // Additional effect will be negated
		if (user.faster_than(target)) {
			last_move = target.battler.GetMoveWithID(target.battler.lastRegularMoveUsed);
			if (last_move && last_move.total_pp > 0) {
				score += add_effect;
				if (last_move.pp <= 3) next score + 20;   // Will fully deplete the move's PP
				if (last_move.pp <= 5) next score + 10;
				if (last_move.pp > 9) next score - 10;   // Too much PP left to make a difference
			}
		}
		next score;   // Don't know which move it will affect; treat as just a damaging move
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerPPOfTargetLastMoveBy4",
	block: (move, user, target, ai, battle) => {
		next !target.check_for_move(m => m.id == target.battler.lastRegularMoveUsed);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("LowerPPOfTargetLastMoveBy4",
	block: (score, move, user, target, ai, battle) => {
		if (user.faster_than(target)) {
			last_move = target.battler.GetMoveWithID(target.battler.lastRegularMoveUsed);
			if (last_move.pp <= 4) next score + 20;   // Will fully deplete the move's PP
			if (last_move.pp <= 6) next score + 10;
			if (last_move.pp > 10) next score - 10;   // Too much PP left to make a difference
		}
		next score - 10;   // Don't know which move it will affect; don't prefer
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("DisableTargetLastMoveUsed",
	block: (move, user, target, ai, battle) => {
		if (target.effects.Disable > 0 || !target.battler.lastRegularMoveUsed) next true;
		if (move.move.MoveFailedAromaVeil(user.battler, target.battler, false)) next true;
		next !target.check_for_move(m => m.id == target.battler.lastRegularMoveUsed);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetLastMoveUsed",
	block: (score, move, user, target, ai, battle) => {
		if (target.has_active_item(items.MENTALHERB)) next Battle.AI.MOVE_USELESS_SCORE;
		// Inherent preference
		score += 5;
		// Prefer if the target is locked into using a single move, or will be
		if (target.effects.ChoiceBand ||
			target.has_active_item(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF}) ||
			target.has_active_ability(abilitys.GORILLATACTICS)) {
			score += 10;
		}
		// Prefer disabling a damaging move
		if (GameData.Move.try_get(target.battler.lastRegularMoveUsed)&.damaging()) score += 8;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("DisableTargetUsingSameMoveConsecutively",
	block: (move, user, target, ai, battle) => {
		if (target.effects.Torment) next true;
		if (move.move.MoveFailedAromaVeil(user.battler, target.battler, false)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetUsingSameMoveConsecutively",
	block: (score, move, user, target, ai, battle) => {
		if (target.has_active_item(items.MENTALHERB)) next Battle.AI.MOVE_USELESS_SCORE;
		// Inherent preference
		score += 10;
		// Prefer if the target is locked into using a single move, or will be
		if (target.effects.ChoiceBand ||
			target.has_active_item(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF}) ||
			target.has_active_ability(abilitys.GORILLATACTICS)) {
			score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("DisableTargetUsingDifferentMove",
	block: (move, user, target, ai, battle) => {
		if (target.effects.Encore > 0) next true;
		if (!target.battler.lastRegularMoveUsed ||
								!GameData.Move.exists(target.battler.lastRegularMoveUsed) ||
								move.move.moveBlacklist.Contains(GameData.Move.get(target.battler.lastRegularMoveUsed).function_code)) next true;
		if (target.effects.ShellTrap) next true;
		if (move.move.MoveFailedAromaVeil(user.battler, target.battler, false)) next true;
		will_fail = true;
		next !target.check_for_move(m => m.id == target.battler.lastRegularMoveUsed);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetUsingDifferentMove",
	block: (score, move, user, target, ai, battle) => {
		if (target.has_active_item(items.MENTALHERB)) next Battle.AI.MOVE_USELESS_SCORE;
		if (user.faster_than(target)) {
			// We know which move is going to be encored (assuming the target doesn't
			// use a high priority move)
			move_data = GameData.Move.get(target.battler.lastRegularMoveUsed);
			if (move_data.status()) {
				// Prefer encoring status moves
				if (new []{:User, :BothSides}.Contains(move_data.target)) {
					score += 10;
				} else {
					score += 8;
				}
			} else if (move_data.damaging() && new []{:NearOther, :Other}.Contains(move_data.target)) {
				// Prefer encoring damaging moves depending on their type effectiveness
				// against the user
				eff = user.effectiveness_of_type_against_battler(move_data.type, target);
				if (Effectiveness.ineffective(eff)) {
					score += 20;
				} else if (Effectiveness.not_very_effective(eff)) {
					score += 15;
				} else if (Effectiveness.super_effective(eff)) {
					score -= 8;
				} else {
					score += 8;
				}
			}
		} else {
			// We don't know which move is going to be encored; just prefer limiting
			// the target's options
			score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("DisableTargetStatusMoves",
	block: (move, user, target, ai, battle) => {
		if (target.effects.Taunt > 0) next true;
		if (move.move.MoveFailedAromaVeil(user.battler, target.battler, false)) next true;
		if (Settings.MECHANICS_GENERATION >= 6 &&
								target.has_active_ability(abilitys.OBLIVIOUS) && !target.being_mold_broken()) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetStatusMoves",
	block: (score, move, user, target, ai, battle) => {
		if (!target.check_for_move(m => m.statusMove())) next Battle.AI.MOVE_USELESS_SCORE;
		// Not worth using on a sleeping target that won't imminently wake up
		if (target.status == statuses.SLEEP && target.statusCount > ((target.faster_than(user)) ? 2 : 1)) {
			if (!target.check_for_move(m => m.statusMove() && m.usableWhenAsleep())) {
				next Battle.AI.MOVE_USELESS_SCORE;
			}
		}
		// Move is likely useless if the target will lock themselves into a move,
		// because they'll likely lock themselves into a damaging move
		if (!target.effects.ChoiceBand) {
			if (target.has_active_item(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF}) ||
				target.has_active_ability(abilitys.GORILLATACTICS)) {
				next Battle.AI.MOVE_USELESS_SCORE;
			}
		}
		// Prefer based on how many status moves the target knows
		foreach (var m in target.battler.Moves) { //target.battler.eachMove do => |m|
			if (m.statusMove() && (m.pp > 0 || m.total_pp == 0)) score += 5;
		}
		// Prefer if the target has a protection move
		protection_moves = new {
			"ProtectUser",                                       // Detect, Protect
			"ProtectUserSideFromPriorityMoves",                  // Quick Guard
			"ProtectUserSideFromMultiTargetDamagingMoves",       // Wide Guard
			"UserEnduresFaintingThisTurn",                       // Endure
			"ProtectUserSideFromDamagingMovesIfUserFirstTurn",   // Mat Block
			"ProtectUserSideFromStatusMoves",                    // Crafty Shield
			"ProtectUserFromDamagingMovesKingsShield",           // King's Shield
			"ProtectUserFromDamagingMovesObstruct",              // Obstruct
			"ProtectUserFromTargetingMovesSpikyShield",          // Spiky Shield
			"ProtectUserBanefulBunker",                          // Baneful Bunker
			"ProtectUserFromDamagingMovesSilkTrap",              // Silk Trap
			"ProtectUserFromDamagingMovesBurningBulwark";         // Burning Bulwark
		}
		if (target.check_for_move(m => m.statusMove() && protection_moves.Contains(m.function_code))) {
			score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("DisableTargetHealingMoves",
	block: (move, user, target, ai, battle) => {
		if (target.effects.HealBlock > 0) next true;
		if (move.move.MoveFailedAromaVeil(user.battler, target.battler, false)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetHealingMoves",
	block: (score, move, user, target, ai, battle) => {
		// Useless if the foe can't heal themselves with a move or some held items
		if (!target.check_for_move(m => m.healingMove())) {
			if (!target.has_active_item(items.LEFTOVERS) &&
				!(target.has_active_item(items.BLACKSLUDGE) && target.has_type(types.POISON))) {
				next Battle.AI.MOVE_USELESS_SCORE;
			}
		}
		// Inherent preference
		score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetSoundMoves",
	block: (score, move, user, target, ai, battle) => {
		if (target.effects.ThroatChop > 1) next score;
		if (!target.check_for_move(m => m.soundMove())) next score;
		// Check additional effect chance
		add_effect = move.get_score_change_for_additional_effect(user, target);
		if (add_effect == -999) next score;   // Additional effect will be negated
		score += add_effect;
		// Inherent preference
		score += 8;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("DisableTargetMovesKnownByUser",
	block: (move, user, ai, battle) => {
		next user.effects.Imprison;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DisableTargetMovesKnownByUser",
	block: (score, move, user, target, ai, battle) => {
		// Useless if the foes have no moves that the user also knows
		affected_foe_count = 0;
		user_moves = user.battler.moves.map(m => m.id);
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.check_for_move(m => user_moves.Contains(m.id))) continue;
			affected_foe_count += 1;
			break;
		}
		if (affected_foe_count == 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Inherent preference
		score += 8 * affected_foe_count;
		next score;
	}
)
