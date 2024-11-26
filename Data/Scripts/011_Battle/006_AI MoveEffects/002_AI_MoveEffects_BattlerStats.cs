//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaiseUserAttack1",
	block: (move, user, ai, battle) => {
		next move.statusMove() &&
				!user.battler.CanRaiseStatStage(move.move.statUp[0], user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserAttack1",
	block: (score, move, user, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAttack1",
																						"RaiseUserAttack2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAttack1",
																					"RaiseUserAttack2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseUserAttack2IfTargetFaints",
	block: (score, move, user, target, ai, battle) => {
		if (move.rough_damage >= target.hp * 0.9) {
			next ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAttack1",
																						"RaiseUserAttack3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAttack1",
																					"RaiseUserAttack3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("RaiseUserAttack2IfTargetFaints",
																												"RaiseUserAttack3IfTargetFaints");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("MaxUserAttackLoseHalfOfTotalHP",
	block: (move, user, ai, battle) => {
		if (user.hp <= (int)Math.Max(user.totalhp / 2, 1)) next true;
		next !user.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("MaxUserAttackLoseHalfOfTotalHP",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		// Don't prefer the lower the user's HP is
		if (ai.trainer.has_skill_flag("HPAware")) {
			score -= 60 * (1 - (user.hp.to_f / user.totalhp));   // -0 to -30
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAttack1",
																						"RaiseUserDefense1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAttack1",
																					"RaiseUserDefense1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserDefense1",
																						"RaiseUserDefense1CurlUpUser");
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserDefense1CurlUpUser",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (!user.effects.DefenseCurl &&
			user.has_move_with_function("MultiTurnAttackPowersUpEachTurn")) {
			score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserDefense1",
																						"RaiseUserDefense2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserDefense1",
																					"RaiseUserDefense2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserDefense1",
																						"RaiseUserDefense3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserDefense1",
																					"RaiseUserDefense3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAttack1",
																						"RaiseUserSpAtk1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAttack1",
																					"RaiseUserSpAtk1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpAtk1",
																						"RaiseUserSpAtk2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpAtk1",
																					"RaiseUserSpAtk2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpAtk1",
																						"RaiseUserSpAtk3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpAtk1",
																					"RaiseUserSpAtk3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserDefense1",
																						"RaiseUserSpDef1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserDefense1",
																					"RaiseUserSpDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpDef1",
																						"RaiseUserSpDef1PowerUpElectricMove");
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserSpDef1PowerUpElectricMove",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (user.has_damaging_move_of_type(types.ELECTRIC)) {
			score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpDef1",
																						"RaiseUserSpDef2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpDef1",
																					"RaiseUserSpDef2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpDef1",
																						"RaiseUserSpDef3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpDef1",
																					"RaiseUserSpDef3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpDef1",
																						"RaiseUserSpeed1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpDef1",
																					"RaiseUserSpeed1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpeed1",
																						"RaiseUserSpeed2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpeed1",
																					"RaiseUserSpeed2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpeed2",
																						"RaiseUserSpeed2LowerUserWeight");
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserSpeed2LowerUserWeight",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (ai.trainer.medium_skill()) {
			current_weight = user.battler.Weight;
			if (current_weight > 1) {
				if (user.has_move_with_function("PowerHigherWithUserHeavierThanTarget")) score -= 5;
				ai.each_foe_battler(user.side) do |b, i|
					if (b.has_move_with_function("PowerHigherWithUserHeavierThanTarget")) score -= 5;
					if (b.has_move_with_function("PowerHigherWithTargetWeight")) score += 5;
					// User will become susceptible to Sky Drop
					if (b.has_move_with_function("TwoTurnAttackInvulnerableInSkyTargetCannotAct") &&
						Settings.MECHANICS_GENERATION >= 6) {
						if (current_weight >= 2000 && current_weight < 3000) score -= 10;
					}
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpeed1",
																						"RaiseUserSpeed3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpeed1",
																					"RaiseUserSpeed3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserSpeed1",
																						"RaiseUserAccuracy1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpeed1",
																					"RaiseUserAccuracy1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAccuracy1",
																						"RaiseUserAccuracy2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAccuracy1",
																					"RaiseUserAccuracy2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAccuracy1",
																						"RaiseUserAccuracy3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAccuracy1",
																					"RaiseUserAccuracy3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAccuracy1",
																						"RaiseUserEvasion1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAccuracy1",
																					"RaiseUserEvasion1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserEvasion1",
																						"RaiseUserEvasion2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserEvasion1",
																					"RaiseUserEvasion2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserEvasion2",
																						"RaiseUserEvasion2MinimizeUser");
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserEvasion2MinimizeUser",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (ai.trainer.medium_skill() && !user.effects.Minimize) {
			ai.each_foe_battler(user.side) do |b, i|
				// Moves that do double damage and (in Gen 6+) have perfect accuracy
				if (b.check_for_move(m => m.tramplesMinimize())) {
					score -= (Settings.MECHANICS_GENERATION >= 6) ? 15 : 10;
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserEvasion1",
																						"RaiseUserEvasion3");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserEvasion1",
																					"RaiseUserEvasion3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaiseUserCriticalHitRate2",
	block: (move, user, ai, battle) => {
		next user.effects.FocusEnergy >= 2;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserCriticalHitRate2",
	block: (score, move, user, ai, battle) => {
		if (!user.check_for_move(m => m.damagingMove())) next Battle.AI.MOVE_USELESS_SCORE;
		score += 15;
		if (ai.trainer.medium_skill()) {
			// Other effects that raise the critical hit rate
			if (user.item_active()) {
				if (new []{:RAZORCLAW, :SCOPELENS}.Contains(user.item_id) ||
					(user.item_id == items.LUCKYPUNCH && user.battler.isSpecies(Speciess.CHANSEY)) ||
					(new []{:LEEK, :STICK}.Contains(user.item_id) &&
					(user.battler.isSpecies(Speciess.FARFETCHD) || user.battler.isSpecies(Speciess.SIRFETCHD)))) {
					score += 10;
				}
			}
			// Critical hits do more damage
			if (user.has_active_ability(abilitys.SNIPER)) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaiseUserAtkDef1",
	block: (move, user, ai, battle) => {
		if (move.damagingMove()) next false;
		will_fail = true;
		for (int i = (move.move.statUp.length / 2); i < (move.move.statUp.length / 2); i++) { //for '(move.move.statUp.length / 2)' times do => |i|
			if (!user.battler.CanRaiseStatStage(move.move.statUp[i * 2], user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAttack1",
																					"RaiseUserAtkDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkDef1",
																						"RaiseUserAtkDefAcc1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkDef1",
																					"RaiseUserAtkDefAcc1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkDef1",
																						"RaiseUserAtkSpAtk1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkDef1",
																					"RaiseUserAtkSpAtk1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserAtkSpAtk1Or2InSun");
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserAtkSpAtk1Or2InSun",
	block: (score, move, user, ai, battle) => {
		raises = move.move.statUp.clone;
		if (new []{:Sun, :HarshSun}.Contains(user.battler.effectiveWeather)) {
			raises[1] = 2;
			raises[3] = 2;
		}
		next ai.get_score_for_target_stat_raise(score, user, raises);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("LowerUserDefSpDef1RaiseUserAtkSpAtkSpd2",
	block: (move, user, ai, battle) => {
		will_fail = true;
		for (int i = (move.move.statUp.length / 2); i < (move.move.statUp.length / 2); i++) { //for '(move.move.statUp.length / 2)' times do => |i|
			if (!user.battler.CanRaiseStatStage(move.move.statUp[i * 2], user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		for (int i = (move.move.statDown.length / 2); i < (move.move.statDown.length / 2); i++) { //for '(move.move.statDown.length / 2)' times do => |i|
			if (!user.battler.CanLowerStatStage(move.move.statDown[i * 2], user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("LowerUserDefSpDef1RaiseUserAtkSpAtkSpd2",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		next ai.get_score_for_target_stat_drop(score, user, move.move.statDown, false);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserAtkSpd1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserAtkSpd1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserAtk1Spd2");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserAtk1Spd2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserAtkAcc1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserAtkAcc1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserDefSpDef1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserDefSpDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserSpAtkSpDef1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserSpAtkSpDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserSpAtkSpDefSpd1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserSpAtkSpDefSpd1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkSpAtk1",
																						"RaiseUserMainStats1");
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserAtkSpAtk1",
																					"RaiseUserMainStats1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaiseUserMainStats1LoseThirdOfTotalHP",
	block: (move, user, ai, battle) => {
		if (user.hp <= (int)Math.Max(user.totalhp / 3, 1)) next true;
		next Battle.AI.Handlers.move_will_fail("RaiseUserAtkDef1", move, user, ai, battle);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserMainStats1LoseThirdOfTotalHP",
	block: (score, move, user, ai, battle) => {
		// Score for stat increase
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for losing HP
		if (ai.trainer.has_skill_flag("HPAware") && user.hp <= user.totalhp * 0.75) {
			score -= 45 * (user.totalhp - user.hp) / user.totalhp;   // -0 to -30
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaiseUserMainStats1TrapUserInBattle",
	block: (move, user, ai, battle) => {
		if (user.effects.NoRetreat) next true;
		next Battle.AI.Handlers.move_will_fail("RaiseUserAtkDef1", move, user, ai, battle);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RaiseUserMainStats1TrapUserInBattle",
	block: (score, move, user, ai, battle) => {
		// Score for stat increase
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		// Score for user becoming trapped in battle
		if (user.can_become_trapped() && battle.CanChooseNonActive(user.index)) {
			// Not worth trapping if user will faint this round anyway
			eor_damage = user.rough_end_of_round_damage;
			if (eor_damage >= user.hp) {
				next (move.damagingMove()) ? score : Battle.AI.MOVE_USELESS_SCORE;
			}
			// Score for user becoming trapped in battle
			if (user.effects.PerishSong > 0 ||
				user.effects.Attract >= 0 ||
				eor_damage > 0) {
				score -= 15;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("StartRaiseUserAtk1WhenDamaged",
	block: (score, move, user, ai, battle) => {
		// Ignore the stat-raising effect if user is at a low HP and likely won't
		// benefit from it
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp / 3) next score;
		}
		// Prefer if user benefits from a raised Attack stat
		if (ai.stat_raise_worthwhile(user, :ATTACK)) score += 10;
		if (user.has_move_with_function("PowerHigherWithUserPositiveStatStages")) score += 7;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("LowerUserAttack1",
	block: (score, move, user, ai, battle) => {
		next ai.get_score_for_target_stat_drop(score, user, move.move.statDown);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserAttack2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserDefense1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserDefense1",
																					"LowerUserDefense2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserSpAtk1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserSpAtk1",
																					"LowerUserSpAtk2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserDefense1",
																					"LowerUserSpDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserSpDef1",
																					"LowerUserSpDef2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserSpeed1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserSpeed1",
																					"LowerUserSpeed2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserAtkDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserDefSpDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("LowerUserAttack1",
																					"LowerUserDefSpDefSpd1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseTargetAttack1",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() &&
				!target.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseTargetAttack1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:ATTACK, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseTargetAttack2ConfuseTarget",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move) &&
				!target.battler.CanConfuse(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseTargetAttack2ConfuseTarget",
	block: (score, move, user, target, ai, battle) => {
		if (!target.has_active_ability(abilitys.CONTRARY) || target.being_mold_broken()) {
			if (!target.battler.CanConfuse(user.battler, false, move.move)) next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Score for stat raise
		score = ai.get_score_for_target_stat_raise(score, target, new {:ATTACK, 2}, false);
		// Score for confusing the target
		next Battle.AI.Handlers.apply_move_effect_against_target_score(
			"ConfuseTarget", score, move, user, target, ai, battle);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseTargetSpAtk1ConfuseTarget",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanRaiseStatStage(:SPECIAL_ATTACK, user.battler, move.move) &&
				!target.battler.CanConfuse(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseTargetSpAtk1ConfuseTarget",
	block: (score, move, user, target, ai, battle) => {
		if (!target.has_active_ability(abilitys.CONTRARY) || target.being_mold_broken()) {
			if (!target.battler.CanConfuse(user.battler, false, move.move)) next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Score for stat raise
		score = ai.get_score_for_target_stat_raise(score, target, new {:SPECIAL_ATTACK, 1}, false);
		// Score for confusing the target
		next Battle.AI.Handlers.apply_move_effect_against_target_score(
			"ConfuseTarget", score, move, user, target, ai, battle);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseTargetSpDef1",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanRaiseStatStage(:SPECIAL_DEFENSE, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseTargetSpDef1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:SPECIAL_DEFENSE, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseTargetRandomStat2",
	block: (move, user, target, ai, battle) => {
		will_fail = true;
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (!target.battler.CanRaiseStatStage(s.id, user.battler, move.move)) continue;
			will_fail = false;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseTargetRandomStat2",
	block: (score, move, user, target, ai, battle) => {
		if (target.has_active_ability(abilitys.CONTRARY) && !target.being_mold_broken()) next Battle.AI.MOVE_USELESS_SCORE;
		if (target.rough_end_of_round_damage >= target.hp) next Battle.AI.MOVE_USELESS_SCORE;
		if (target.index != user.index) score -= 7;   // Less likely to use on ally
		if (target.has_active_ability(abilitys.SIMPLE)) score += 10;
		// Prefer if target is at high HP, don't prefer if target is at low HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp >= target.totalhp * 0.7) {
				score += 10;
			} else {
				score += (50 * ((target.hp.to_f / target.totalhp) - 0.6)).ToInt();   // +5 to -30
			}
		}
		// Prefer if target has Stored Power
		if (target.has_move_with_function("PowerHigherWithUserPositiveStatStages")) {
			score += 10;
		}
		// Don't prefer if any foe has Punishment
		ai.each_foe_battler(target.side) do |b, i|
			if (!b.has_move_with_function("PowerHigherWithTargetPositiveStatStages")) continue;
			score -= 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseTargetAtkSpAtk2",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move) &&
				!target.battler.CanRaiseStatStage(:SPECIAL_ATTACK, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseTargetAtkSpAtk2",
	block: (score, move, user, target, ai, battle) => {
		if (target.opposes(user)) next Battle.AI.MOVE_USELESS_SCORE;
		next ai.get_score_for_target_stat_raise(score, target, new {:ATTACK, 2, :SPECIAL_ATTACK, 2});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetAttack1",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() &&
				!target.battler.CanLowerStatStage(move.move.statDown[0], user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("LowerTargetAttack1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_drop(score, target, move.move.statDown);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAttack1",
																												"LowerTargetAttack1BypassSubstitute");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAttack1",
																												"LowerTargetAttack1BypassSubstitute");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAttack1",
																												"LowerTargetAttack2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAttack1",
																												"LowerTargetAttack2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAttack1",
																												"LowerTargetAttack3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAttack1",
																												"LowerTargetAttack3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAttack1",
																												"LowerTargetDefense1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAttack1",
																												"LowerTargetDefense1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetDefense1",
																												"LowerTargetDefense1PowersUpInGravity");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetDefense1",
																												"LowerTargetDefense1PowersUpInGravity");
Battle.AI.Handlers.MoveBasePower.add("LowerTargetDefense1PowersUpInGravity",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetDefense1",
																												"LowerTargetDefense2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetDefense1",
																												"LowerTargetDefense2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetDefense1",
																												"LowerTargetDefense3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetDefense1",
																												"LowerTargetDefense3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAttack1",
																												"LowerTargetSpAtk1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAttack1",
																												"LowerTargetSpAtk1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpAtk1",
																												"LowerTargetSpAtk2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpAtk1",
																												"LowerTargetSpAtk2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetSpAtk2IfCanAttract",
	block: (move, user, target, ai, battle) => {
		if (move.statusMove() &&
								!target.battler.CanLowerStatStage(move.move.statDown[0], user.battler, move.move)) next true;
		if (user.gender == 2 || target.gender == 2 || user.gender == target.gender) next true;
		if (target.has_active_ability(abilitys.OBLIVIOUS) && !target.being_mold_broken()) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpAtk2",
																												"LowerTargetSpAtk2IfCanAttract");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpAtk1",
																												"LowerTargetSpAtk3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpAtk1",
																												"LowerTargetSpAtk3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetDefense1",
																												"LowerTargetSpDef1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetDefense1",
																												"LowerTargetSpDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpDef1",
																												"LowerTargetSpDef2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpDef1",
																												"LowerTargetSpDef2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpDef1",
																												"LowerTargetSpDef3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpDef1",
																												"LowerTargetSpDef3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpDef1",
																												"LowerTargetSpeed1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpDef1",
																												"LowerTargetSpeed1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpeed1",
																												"LowerTargetSpeed1WeakerInGrassyTerrain");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpeed1",
																												"LowerTargetSpeed1WeakerInGrassyTerrain");
Battle.AI.Handlers.MoveBasePower.add("LowerTargetSpeed1WeakerInGrassyTerrain",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetSpeed1MakeTargetWeakerToFire",
	block: (move, user, target, ai, battle) => {
		if (!target.effects.TarShot) next false;
		next move.statusMove() &&
				!target.battler.CanLowerStatStage(move.move.statDown[0], user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("LowerTargetSpeed1MakeTargetWeakerToFire",
	block: (score, move, user, target, ai, battle) => {
		// Score for stat drop
		score = ai.get_score_for_target_stat_drop(score, target, move.move.statDown);
		// Score for adding weakness to Fire
		if (!target.effects.TarShot) {
			eff = target.effectiveness_of_type_against_battler(:FIRE);
			if (!Effectiveness.ineffective(eff)) {
				if (user.has_damaging_move_of_type(types.FIRE)) score += 10 * eff;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpeed1",
																												"LowerTargetSpeed2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpeed1",
																												"LowerTargetSpeed2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpeed1",
																												"LowerTargetSpeed3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpeed1",
																												"LowerTargetSpeed3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetSpeed1",
																												"LowerTargetAccuracy1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetSpeed1",
																												"LowerTargetAccuracy1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAccuracy1",
																												"LowerTargetAccuracy2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAccuracy1",
																												"LowerTargetAccuracy2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAccuracy1",
																												"LowerTargetAccuracy3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAccuracy1",
																												"LowerTargetAccuracy3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAccuracy1",
																												"LowerTargetEvasion1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAccuracy1",
																												"LowerTargetEvasion1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetEvasion1RemoveSideEffects",
	block: (move, user, target, ai, battle) => {
		target_side = target.OwnSide;
		target_opposing_side = target.OpposingSide;
		if (target_side.effects.AuroraVeil > 0 ||
									target_side.effects.LightScreen > 0 ||
									target_side.effects.Reflect > 0 ||
									target_side.effects.Mist > 0 ||
									target_side.effects.Safeguard > 0) next false;
		if (target_side.effects.StealthRock ||
									target_side.effects.Spikes > 0 ||
									target_side.effects.ToxicSpikes > 0 ||
									target_side.effects.StickyWeb) next false;
		if (Settings.MECHANICS_GENERATION >= 6 &&
									(target_opposing_side.effects.StealthRock ||
									target_opposing_side.effects.Spikes > 0 ||
									target_opposing_side.effects.ToxicSpikes > 0 ||
									target_opposing_side.effects.StickyWeb)) next false;
		if (Settings.MECHANICS_GENERATION >= 8 && battle.field.terrain != :None) next false;
		next move.statusMove() &&
				!target.battler.CanLowerStatStage(move.move.statDown[0], user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("LowerTargetEvasion1RemoveSideEffects",
	block: (score, move, user, target, ai, battle) => {
		if (!target.opposes(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Score for stat drop
		score = ai.get_score_for_target_stat_drop(score, target, move.move.statDown);
		// Score for removing side effects/terrain
		if (target.OwnSide.effects.AuroraVeil > 1 ||
									target.OwnSide.effects.Reflect > 1 ||
									target.OwnSide.effects.LightScreen > 1 ||
									target.OwnSide.effects.Mist > 1 ||
									target.OwnSide.effects.Safeguard > 1) score += 10;
		if (target.can_switch_lax()) {
			if (target.OwnSide.effects.Spikes > 0 ||
										target.OwnSide.effects.ToxicSpikes > 0 ||
										target.OwnSide.effects.StealthRock ||
										target.OwnSide.effects.StickyWeb) score -= 15;
		}
		if (user.can_switch_lax() && Settings.MECHANICS_GENERATION >= 6) {
			if (target.OpposingSide.effects.Spikes > 0 ||
										target.OpposingSide.effects.ToxicSpikes > 0 ||
										target.OpposingSide.effects.StealthRock ||
										target.OpposingSide.effects.StickyWeb) score += 15;
		}
		if (Settings.MECHANICS_GENERATION >= 8 && battle.field.terrain != :None) {
			score -= ai.get_score_for_terrain(battle.field.terrain, user);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetEvasion1",
																												"LowerTargetEvasion2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetEvasion1",
																												"LowerTargetEvasion2");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetEvasion1",
																												"LowerTargetEvasion3");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetEvasion1",
																												"LowerTargetEvasion3");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetAtkDef1",
	block: (move, user, target, ai, battle) => {
		if (!move.statusMove()) next false;
		will_fail = true;
		for (int i = (move.move.statDown.length / 2); i < (move.move.statDown.length / 2); i++) { //for '(move.move.statDown.length / 2)' times do => |i|
			if (!target.battler.CanLowerStatStage(move.move.statDown[i * 2], user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAttack1",
																												"LowerTargetAtkDef1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("LowerTargetAtkDef1",
																												"LowerTargetAtkSpAtk1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAtkDef1",
																												"LowerTargetAtkSpAtk1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerPoisonedTargetAtkSpAtkSpd1",
	block: (move, user, target, ai, battle) => {
		if (!target.battler.poisoned()) next true;
		next Battle.AI.Handlers.move_will_fail_against_target("LowerTargetAtkSpAtk1",
																														move, user, target, ai, battle);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("LowerTargetAtkSpAtk1",
																												"LowerPoisonedTargetAtkSpAtkSpd1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseAlliesAtkDef1",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move) &&
				!target.battler.CanRaiseStatStage(:DEFENSE, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseAlliesAtkDef1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:ATTACK, 1, :DEFENSE, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaisePlusMinusUserAndAlliesAtkSpAtk1",
	block: (move, user, ai, battle) => {
		will_fail = true;
		ai.each_same_side_battler(user.side) do |b, i|
			if (!b.has_active_ability(new {:MINUS, :PLUS})) continue;
			if (!b.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move) &&
							!b.battler.CanRaiseStatStage(:SPECIAL_ATTACK, user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaisePlusMinusUserAndAlliesAtkSpAtk1",
	block: (move, user, target, ai, battle) => {
		if (!target.has_active_ability(new {:MINUS, :PLUS})) next true;
		next !target.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move) &&
				!target.battler.CanRaiseStatStage(:SPECIAL_ATTACK, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RaisePlusMinusUserAndAlliesAtkSpAtk1",
	block: (score, move, user, ai, battle) => {
		if (move.Target(user.battler) != :UserSide) next score;
		ai.each_same_side_battler(user.side) do |b, i|
			score = ai.get_score_for_target_stat_raise(score, b, new {:ATTACK, 1, :SPECIAL_ATTACK, 1}, false);
		}
		next score;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaisePlusMinusUserAndAlliesAtkSpAtk1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:ATTACK, 1, :SPECIAL_ATTACK, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RaisePlusMinusUserAndAlliesDefSpDef1",
	block: (move, user, ai, battle) => {
		will_fail = true;
		ai.each_same_side_battler(user.side) do |b, i|
			if (!b.has_active_ability(new {:MINUS, :PLUS})) continue;
			if (!b.battler.CanRaiseStatStage(:DEFENSE, user.battler, move.move) &&
							!b.battler.CanRaiseStatStage(:SPECIAL_DEFENSE, user.battler, move.move)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaisePlusMinusUserAndAlliesDefSpDef1",
	block: (move, user, target, ai, battle) => {
		if (!target.has_active_ability(new {:MINUS, :PLUS})) next true;
		next !target.battler.CanRaiseStatStage(:DEFENSE, user.battler, move.move) &&
				!target.battler.CanRaiseStatStage(:SPECIAL_DEFENSE, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RaisePlusMinusUserAndAlliesDefSpDef1",
	block: (score, move, user, ai, battle) => {
		if (move.Target(user.battler) != :UserSide) next score;
		ai.each_same_side_battler(user.side) do |b, i|
			score = ai.get_score_for_target_stat_raise(score, b, new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1}, false);
		}
		next score;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaisePlusMinusUserAndAlliesDefSpDef1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseGroundedGrassBattlersAtkSpAtk1",
	block: (move, user, target, ai, battle) => {
		if (!target.has_type(types.GRASS) || target.battler.airborne() || target.battler.semiInvulnerable()) next true;
		next !target.battler.CanRaiseStatStage(:ATTACK, user.battler, move.move) &&
				!target.battler.CanRaiseStatStage(:SPECIAL_ATTACK, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseGroundedGrassBattlersAtkSpAtk1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:ATTACK, 1, :SPECIAL_ATTACK, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("RaiseGrassBattlersDef1",
	block: (move, user, target, ai, battle) => {
		if (!target.has_type(types.GRASS) || target.battler.semiInvulnerable()) next true;
		next !target.battler.CanRaiseStatStage(:DEFENSE, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RaiseGrassBattlersDef1",
	block: (score, move, user, target, ai, battle) => {
		next ai.get_score_for_target_stat_raise(score, target, new {:DEFENSE, 1});
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetSwapAtkSpAtkStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		drops = new List<string>();
		new {:ATTACK, :SPECIAL_ATTACK}.each do |stat|
			stage_diff = target.stages[stat] - user.stages[stat];
			if (stage_diff > 0) {
				raises.Add(stat);
				raises.Add(stage_diff);
			} else if (stage_diff < 0) {
				drops.Add(stat);
				drops.Add(stage_diff);
			}
		}
		if (raises.length == 0) next Battle.AI.MOVE_USELESS_SCORE;   // No stat raises
		if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, user, raises, false, true);
		if (raises.length > 0) score = ai.get_score_for_target_stat_drop(score, target, raises, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, user, drops, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_raise(score, target, drops, false, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetSwapDefSpDefStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		drops = new List<string>();
		new {:DEFENSE, :SPECIAL_DEFENSE}.each do |stat|
			stage_diff = target.stages[stat] - user.stages[stat];
			if (stage_diff > 0) {
				raises.Add(stat);
				raises.Add(stage_diff);
			} else if (stage_diff < 0) {
				drops.Add(stat);
				drops.Add(stage_diff);
			}
		}
		if (raises.length == 0) next Battle.AI.MOVE_USELESS_SCORE;   // No stat raises
		if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, user, raises, false, true);
		if (raises.length > 0) score = ai.get_score_for_target_stat_drop(score, target, raises, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, user, drops, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_raise(score, target, drops, false, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetSwapStatStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		drops = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			stage_diff = target.stages[s.id] - user.stages[s.id];
			if (stage_diff > 0) {
				raises.Add(s.id);
				raises.Add(stage_diff);
			} else if (stage_diff < 0) {
				drops.Add(s.id);
				drops.Add(stage_diff);
			}
		}
		if (raises.length == 0) next Battle.AI.MOVE_USELESS_SCORE;   // No stat raises
		if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, user, raises, false, true);
		if (raises.length > 0) score = ai.get_score_for_target_stat_drop(score, target, raises, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, user, drops, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_raise(score, target, drops, false, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserCopyTargetStatStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		drops = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			stage_diff = target.stages[s.id] - user.stages[s.id];
			if (stage_diff > 0) {
				raises.Add(s.id);
				raises.Add(stage_diff);
			} else if (stage_diff < 0) {
				drops.Add(s.id);
				drops.Add(stage_diff);
			}
		}
		if (raises.length == 0) next Battle.AI.MOVE_USELESS_SCORE;   // No stat raises
		if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, user, raises, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, user, drops, false, true);
		if (Settings.NEW_CRITICAL_HIT_RATE_MECHANICS) {
			if (user.effects.FocusEnergy > 0 && target.effects.FocusEnergy == 0) {
				score -= 5;
			} else if (user.effects.FocusEnergy == 0 && target.effects.FocusEnergy > 0) {
				score += 5;
			}
			if (user.effects.LaserFocus > 0 && target.effects.LaserFocus == 0) {
				score -= 5;
			} else if (user.effects.LaserFocus == 0 && target.effects.LaserFocus > 0) {
				score += 5;
			}
		}
		next score;
	}
)

//===============================================================================
// NOTE: Accounting for the stat theft before damage calculation, to calculate a
//       more accurate predicted damage, would be complex, involving
//       CanRaiseStatStage() and Contrary and Simple; I'm not bothering with
//       that.
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserStealTargetPositiveStatStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (target.stages[s.id] <= 0) continue;
			raises.Add(s.id);
			raises.Add(target.stages[s.id]);
		}
		if (raises.length > 0) {
			score = ai.get_score_for_target_stat_raise(score, user, raises, false);
			score = ai.get_score_for_target_stat_drop(score, target, raises, false, true);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("InvertTargetStatStages",
	block: (move, user, target, ai, battle) => {
		next !target.battler.hasAlteredStatStages();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("InvertTargetStatStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		drops = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (target.stages[s.id] > 0) {
				drops.Add(s.id);
				drops.Add(target.stages[s.id] * 2);
			} else if (target.stages[s.id] < 0) {
				raises.Add(s.id);
				raises.Add(target.stages[s.id] * 2);
			}
		}
		if (drops.length == 0) next Battle.AI.MOVE_USELESS_SCORE;   // No stats will drop
		if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, target, raises, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, target, drops, false, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ResetTargetStatStages",
	block: (score, move, user, target, ai, battle) => {
		raises = new List<string>();
		drops = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (target.stages[s.id] > 0) {
				drops.Add(s.id);
				drops.Add(target.stages[s.id]);
			} else if (target.stages[s.id] < 0) {
				raises.Add(s.id);
				raises.Add(target.stages[s.id]);
			}
		}
		if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, target, raises, false, true);
		if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, target, drops, false, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ResetAllBattlersStatStages",
	block: (move, user, ai, battle) => {
		next battle.allBattlers.none(b => b.hasAlteredStatStages());
	}
)
Battle.AI.Handlers.MoveEffectScore.add("ResetAllBattlersStatStages",
	block: (score, move, user, ai, battle) => {
		foreach (var b in ai) { //ai.each_battler do => |b|
			raises = new List<string>();
			drops = new List<string>();
			foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
				if (b.stages[s.id] > 0) {
					drops.Add(s.id);
					drops.Add(b.stages[s.id]);
				} else if (b.stages[s.id] < 0) {
					raises.Add(s.id);
					raises.Add(b.stages[s.id]);
				}
			}
			if (raises.length > 0) score = ai.get_score_for_target_stat_raise(score, b, raises, false, true);
			if (drops.length > 0) score = ai.get_score_for_target_stat_drop(score, b, drops, false, true);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartUserSideImmunityToStatStageLowering",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.Mist > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartUserSideImmunityToStatStageLowering",
	block: (score, move, user, ai, battle) => {
		has_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (b.check_for_move { |m| m.is_a(Battle.Move.TargetStatDownMove) ||
																m.is_a(Battle.Move.TargetMultiStatDownMove) ||
																new []{"LowerPoisonedTargetAtkSpAtkSpd1",
																"PoisonTargetLowerTargetSpeed1",
																"HealUserByTargetAttackLowerTargetAttack1"}.Contains(m.function_code) };
				score += 15) {
				has_move = true;
			}
		}
		if (!has_move) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("UserSwapBaseAtkDef",
	block: (score, move, user, ai, battle) => {
		// No flip-flopping
		if (user.effects.PowerTrick) next Battle.AI.MOVE_USELESS_SCORE;
		// Check stats
		user_atk = user.base_stat(:ATTACK);
		user_def = user.base_stat(:DEFENSE);
		if (user_atk == user_def) next Battle.AI.MOVE_USELESS_SCORE;
		// NOTE: Prefer to raise Attack regardless of the drop to Defense. Only
		//       prefer to raise Defense if Attack is useless.
		if (user_def > user_atk) {   // Attack will be raised
			if (!ai.stat_raise_worthwhile(user, :ATTACK, true)) next Battle.AI.MOVE_USELESS_SCORE;
			score += (40 * ((user_def.to_f / user_atk) - 1)).ToInt();
			if (!ai.stat_drop_worthwhile(user, :DEFENSE, true)) score += 5;   // No downside
		} else {   // Defense will be raised
			if (!ai.stat_raise_worthwhile(user, :DEFENSE, true)) next Battle.AI.MOVE_USELESS_SCORE;
			// Don't want to lower user's Attack if it can make use of it
			if (ai.stat_drop_worthwhile(user, :ATTACK, true)) next Battle.AI.MOVE_USELESS_SCORE;
			score += (40 * ((user_atk.to_f / user_def) - 1)).ToInt();
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetSwapBaseSpeed",
	block: (score, move, user, target, ai, battle) => {
		user_speed = user.base_stat(:SPEED);
		target_speed = target.base_stat(:SPEED);
		if (user_speed == target_speed) next Battle.AI.MOVE_USELESS_SCORE;
		if (battle.field.effects.TrickRoom > 1) {
			// User wants to be slower so it can move first
			if (target_speed > user_speed) next Battle.AI.MOVE_USELESS_SCORE;
			score += (40 * ((user_speed.to_f / target_speed) - 1)).ToInt();
		} else {
			// User wants to be faster so it can move first
			if (user_speed > target_speed) next Battle.AI.MOVE_USELESS_SCORE;
			score += (40 * ((target_speed.to_f / user_speed) - 1)).ToInt();
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetAverageBaseAtkSpAtk",
	block: (score, move, user, target, ai, battle) => {
		user_atk = user.base_stat(:ATTACK);
		user_spatk = user.base_stat(:SPECIAL_ATTACK);
		target_atk = target.base_stat(:ATTACK);
		target_spatk = target.base_stat(:SPECIAL_ATTACK);
		if (user_atk >= target_atk && user_spatk >= target_spatk) next Battle.AI.MOVE_USELESS_SCORE;
		change_matters = false;
		// Score based on changes to Attack
		if (target_atk > user_atk) {
			// User's Attack will be raised, target's Attack will be lowered
			if (ai.stat_raise_worthwhile(user, :ATTACK, true) ||
				ai.stat_drop_worthwhile(target, :ATTACK, true)) {
				score += (40 * ((target_atk.to_f / user_atk) - 1)).ToInt();
				change_matters = true;
			}
		} else if (target_atk < user_atk) {
			// User's Attack will be lowered, target's Attack will be raised
			if (ai.stat_drop_worthwhile(user, :ATTACK, true) ||
				ai.stat_raise_worthwhile(target, :ATTACK, true)) {
				score -= (40 * ((user_atk.to_f / target_atk) - 1)).ToInt();
				change_matters = true;
			}
		}
		// Score based on changes to Special Attack
		if (target_spatk > user_spatk) {
			// User's Special Attack will be raised, target's Special Attack will be lowered
			if (ai.stat_raise_worthwhile(user, :SPECIAL_ATTACK, true) ||
				ai.stat_drop_worthwhile(target, :SPECIAL_ATTACK, true)) {
				score += (40 * ((target_spatk.to_f / user_spatk) - 1)).ToInt();
				change_matters = true;
			}
		} else if (target_spatk < user_spatk) {
			// User's Special Attack will be lowered, target's Special Attack will be raised
			if (ai.stat_drop_worthwhile(user, :SPECIAL_ATTACK, true) ||
				ai.stat_raise_worthwhile(target, :SPECIAL_ATTACK, true)) {
				score -= (40 * ((user_spatk.to_f / target_spatk) - 1)).ToInt();
				change_matters = true;
			}
		}
		if (!change_matters) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetAverageBaseDefSpDef",
	block: (score, move, user, target, ai, battle) => {
		user_def = user.base_stat(:DEFENSE);
		user_spdef = user.base_stat(:SPECIAL_DEFENSE);
		target_def = target.base_stat(:DEFENSE);
		target_spdef = target.base_stat(:SPECIAL_DEFENSE);
		if (user_def >= target_def && user_spdef >= target_spdef) next Battle.AI.MOVE_USELESS_SCORE;
		change_matters = false;
		// Score based on changes to Defense
		if (target_def > user_def) {
			// User's Defense will be raised, target's Defense will be lowered
			if (ai.stat_raise_worthwhile(user, :DEFENSE, true) ||
				ai.stat_drop_worthwhile(target, :DEFENSE, true)) {
				score += (40 * ((target_def.to_f / user_def) - 1)).ToInt();
				change_matters = true;
			}
		} else if (target_def < user_def) {
			// User's Defense will be lowered, target's Defense will be raised
			if (ai.stat_drop_worthwhile(user, :DEFENSE, true) ||
				ai.stat_raise_worthwhile(target, :DEFENSE, true)) {
				score -= (40 * ((user_def.to_f / target_def) - 1)).ToInt();
				change_matters = true;
			}
		}
		// Score based on changes to Special Defense
		if (target_spdef > user_spdef) {
			// User's Special Defense will be raised, target's Special Defense will be lowered
			if (ai.stat_raise_worthwhile(user, :SPECIAL_DEFENSE, true) ||
				ai.stat_drop_worthwhile(target, :SPECIAL_DEFENSE, true)) {
				score += (40 * ((target_spdef.to_f / user_spdef) - 1)).ToInt();
				change_matters = true;
			}
		} else if (target_spdef < user_spdef) {
			// User's Special Defense will be lowered, target's Special Defense will be raised
			if (ai.stat_drop_worthwhile(user, :SPECIAL_DEFENSE, true) ||
				ai.stat_raise_worthwhile(target, :SPECIAL_DEFENSE, true)) {
				score -= (40 * ((user_spdef.to_f / target_spdef) - 1)).ToInt();
				change_matters = true;
			}
		}
		if (!change_matters) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetAverageHP",
	block: (score, move, user, target, ai, battle) => {
		if (user.hp >= target.hp) next Battle.AI.MOVE_USELESS_SCORE;
		mult = (user.hp + target.hp) / (2.0 * user.hp);
		if (mult >= 1.2) score += (10 * mult).ToInt();
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartUserSideDoubleSpeed",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.Tailwind > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartUserSideDoubleSpeed",
	block: (score, move, user, ai, battle) => {
		// Don't want to make allies faster if Trick Room will make them act later
		if (ai.trainer.medium_skill()) {
			if (battle.field.effects.TrickRoom > 1) next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Get the speeds of all battlers
		ally_speeds = new List<string>();
		foe_speeds = new List<string>();
		foreach (var b in ai) { //ai.each_battler do => |b|
			spd = b.rough_stat(:SPEED);
			(b.opposes(user)) ? foe_speeds.Add(spd) : ally_speeds.Add(spd)
		}
		if (ally_speeds.min > foe_speeds.max) next Battle.AI.MOVE_USELESS_SCORE;
		// Compare speeds of all battlers
		outspeeds = 0;
		foreach (var ally_speed in ally_speeds) { //'ally_speeds.each' do => |ally_speed|
			foreach (var foe_speed in foe_speeds) { //'foe_speeds.each' do => |foe_speed|
				if (foe_speed > ally_speed && foe_speed < ally_speed * 2) outspeeds += 1;
			}
		}
		if (outspeeds == 0) next Battle.AI.MOVE_USELESS_SCORE;
		// This move will achieve something
		next score + 8 + (10 * outspeeds);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("StartSwapAllBattlersBaseDefensiveStats",
	block: (score, move, user, ai, battle) => {
		change_matters = false;
		ai.each_battler do |b, i|
			b_def = b.base_stat(:DEFENSE);
			b_spdef = b.base_stat(:SPECIAL_DEFENSE);
			if (b_def == b_spdef) continue;
			score_change = 0;
			if (b_def > b_spdef) {
				// Battler's Defense will be lowered, battler's Special Defense will be raised
				if (ai.stat_drop_worthwhile(b, :DEFENSE, true)) {
					score_change -= (20 * ((b_def.to_f / b_spdef) - 1)).ToInt();
					change_matters = true;
				}
				// Battler's Special Defense will be raised
				if (ai.stat_raise_worthwhile(b, :SPECIAL_DEFENSE, true)) {
					score_change += (20 * ((b_def.to_f / b_spdef) - 1)).ToInt();
					change_matters = true;
				}
			} else {
				// Battler's Special Defense will be lowered
				if (ai.stat_drop_worthwhile(b, :SPECIAL_DEFENSE, true)) {
					score_change -= (20 * ((b_spdef.to_f / b_def) - 1)).ToInt();
					change_matters = true;
				}
				// Battler's Defense will be raised
				if (ai.stat_raise_worthwhile(b, :DEFENSE, true)) {
					score_change += (20 * ((b_spdef.to_f / b_def) - 1)).ToInt();
					change_matters = true;
				}
			}
			score += (b.opposes(user)) ? -score_change : score_change;
		}
		if (!change_matters) next Battle.AI.MOVE_USELESS_SCORE;
		if (score <= Battle.AI.MOVE_BASE_SCORE) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)
