//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SleepTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanSleep(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SleepTarget",
	block: (score, move, user, target, ai, battle) => {
		useless_score = (move.statusMove()) ? Battle.AI.MOVE_USELESS_SCORE : score;
		if (target.effects.Yawn > 0) next useless_score;   // Target is going to fall asleep anyway
		// No score modifier if the sleep will be removed immediately
		if (target.has_active_item(new {:CHESTOBERRY, :LUMBERRY})) next useless_score;
		if (target.faster_than(user) &&
													target.has_active_ability(abilitys.HYDRATION) &&
													new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) next useless_score;
		if (target.battler.CanSleep(user.battler, false, move.move)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next useless_score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference
			score += 8;
			// Prefer if the user or an ally has a move/ability that is better if the target is asleep
			ai.each_same_side_battler(user.side) do |b, i|
				if ((b.has_move_with_function("DoublePowerIfTargetAsleepCureTarget",
																								"DoublePowerIfTargetStatusProblem",
																								"HealUserByHalfOfDamageDoneIfTargetAsleep",
																								"StartDamageTargetEachTurnIfTargetAsleep")) score += 4;
				if (b.has_active_ability(abilitys.BADDREAMS)) score += 8;
			}
			// Don't prefer if target benefits from having the sleep status problem
			// NOTE: The target's Guts/Quick Feet will benefit from the target being
			//       asleep, but the target won't (usually) be able to make use of
			//       them, so they're not worth considering.
			if (target.has_active_ability(abilitys.EARLYBIRD)) score -= 5;
			if (target.has_active_ability(abilitys.MARVELSCALE)) score -= 4;
			// Don't prefer if target has a move it can use while asleep
			if (target.check_for_move(m => m.usableWhenAsleep())) score -= 4;
			// Don't prefer if the target can heal itself (or be healed by an ally)
			if (target.has_active_ability(abilitys.SHEDSKIN)) {
				score -= 5;
			} else if (target.has_active_ability(abilitys.HYDRATION) &&
						new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) {
				score -= 8;
			}
			ai.each_same_side_battler(target.side) do |b, i|
				if (i != target.index && b.has_active_ability(abilitys.HEALER)) score -= 5;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("SleepTargetIfUserDarkrai",
	block: (move, user, ai, battle) => {
		next !user.battler.isSpecies(Speciess.DARKRAI) && user.effects.TransformSpecies != :DARKRAI;
	}
)
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SleepTargetIfUserDarkrai",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanSleep(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("SleepTarget",
																												"SleepTargetIfUserDarkrai");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("SleepTarget",
																												"SleepTargetChangeUserMeloettaForm");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SleepTargetNextTurn",
	block: (move, user, target, ai, battle) => {
		if (target.effects.Yawn > 0) next true;
		if (!target.battler.CanSleep(user.battler, false, move.move)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("SleepTarget",
																												"SleepTargetNextTurn");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("PoisonTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanPoison(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("PoisonTarget",
	block: (score, move, user, target, ai, battle) => {
		useless_score = (move.statusMove()) ? Battle.AI.MOVE_USELESS_SCORE : score;
		if (target.has_active_ability(abilitys.POISONHEAL)) next useless_score;
		// No score modifier if the poisoning will be removed immediately
		if (target.has_active_item(new {:PECHABERRY, :LUMBERRY})) next useless_score;
		if (target.faster_than(user) &&
													target.has_active_ability(abilitys.HYDRATION) &&
													new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) next useless_score;
		if (target.battler.CanPoison(user.battler, false, move.move)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next useless_score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference
			if (ai.trainer.has_skill_flag("HPAware")) {
				score += 8 * target.hp / target.totalhp;
			} else {
				score += 8;
			}
			// Prefer if the user or an ally has a move/ability that is better if the target is poisoned
			ai.each_same_side_battler(user.side) do |b, i|
				if ((b.has_move_with_function("DoublePowerIfTargetPoisoned",
																								"DoublePowerIfTargetStatusProblem")) score += 4;
				if (b.has_active_ability(abilitys.MERCILESS)) score += 5;
			}
			// Don't prefer if target benefits from having the poison status problem
			if (target.has_active_ability(new {:GUTS, :MARVELSCALE, :QUICKFEET, :TOXICBOOST})) score -= 5;
			if (target.has_active_ability(abilitys.POISONHEAL)) score -= 15;
			if (target.has_active_ability(abilitys.SYNCHRONIZE) &&
										user.battler.CanPoisonSynchronize(target.battler)) score -= 15;
			if ((target.has_move_with_function("DoublePowerIfUserPoisonedBurnedParalyzed",
																									"CureUserBurnPoisonParalysis")) score -= 4;
			if (target.check_for_move { |m|
				m.function_code == "GiveUserStatusToTarget" && user.battler.CanPoison(target.battler, false, m)) score -= 8;
			}
			// Don't prefer if the target won't take damage from the poison
			if (!target.battler.takesIndirectDamage()) score -= 10;
			// Don't prefer if the target can heal itself (or be healed by an ally)
			if (target.has_active_ability(abilitys.SHEDSKIN)) {
				score -= 5;
			} else if (target.has_active_ability(abilitys.HYDRATION) &&
						new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) {
				score -= 8;
			}
			ai.each_same_side_battler(target.side) do |b, i|
				if (i != target.index && b.has_active_ability(abilitys.HEALER)) score -= 5;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("PoisonTargetLowerTargetSpeed1",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanPoison(user.battler, false, move.move) &&
				!target.battler.CanLowerStatStage(:SPEED, user.battler, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("PoisonTargetLowerTargetSpeed1",
	block: (score, move, user, target, ai, battle) => {
		poison_score = Battle.AI.Handlers.apply_move_effect_against_target_score("PoisonTarget",
			0, move, user, target, ai, battle);
		if (poison_score != Battle.AI.MOVE_USELESS_SCORE) score += poison_score;
		score = ai.get_score_for_target_stat_drop(score, target, move.move.statDown, false);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("PoisonTarget",
																												"BadPoisonTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("PoisonTarget",
																												"BadPoisonTarget");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("ParalyzeTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanParalyze(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ParalyzeTarget",
	block: (score, move, user, target, ai, battle) => {
		useless_score = (move.statusMove()) ? Battle.AI.MOVE_USELESS_SCORE : score;
		// No score modifier if the paralysis will be removed immediately
		if (target.has_active_item(new {:CHERIBERRY, :LUMBERRY})) next useless_score;
		if (target.faster_than(user) &&
													target.has_active_ability(abilitys.HYDRATION) &&
													new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) next useless_score;
		if (target.battler.CanParalyze(user.battler, false, move.move)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next useless_score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference (because of the chance of full paralysis)
			score += 8;
			// Prefer if the target is faster than the user but will become slower if
			// paralysed
			if (target.faster_than(user)) {
				user_speed = user.rough_stat(:SPEED);
				target_speed = target.rough_stat(:SPEED);
				if (target_speed < user_speed * ((Settings.MECHANICS_GENERATION >= 7) ? 2 : 4)) score += 8;
			}
			// Prefer if the target is confused or infatuated, to compound the turn skipping
			if (target.effects.Confusion > 1) score += 4;
			if (target.effects.Attract >= 0) score += 4;
			// Prefer if the user or an ally has a move/ability that is better if the target is paralysed
			ai.each_same_side_battler(user.side) do |b, i|
				if ((b.has_move_with_function("DoublePowerIfTargetParalyzedCureTarget",
																								"DoublePowerIfTargetStatusProblem")) score += 4;
			}
			// Don't prefer if target benefits from having the paralysis status problem
			if (target.has_active_ability(new {:GUTS, :MARVELSCALE, :QUICKFEET})) score -= 5;
			if (target.has_active_ability(abilitys.SYNCHRONIZE) &&
										user.battler.CanParalyzeSynchronize(target.battler)) score -= 15;
			if ((target.has_move_with_function("DoublePowerIfUserPoisonedBurnedParalyzed",
																									"CureUserBurnPoisonParalysis")) score -= 4;
			if (target.check_for_move { |m|
				m.function_code == "GiveUserStatusToTarget" && user.battler.CanParalyze(target.battler, false, m)) score -= 8;
			}
			// Don't prefer if the target can heal itself (or be healed by an ally)
			if (target.has_active_ability(abilitys.SHEDSKIN)) {
				score -= 5;
			} else if (target.has_active_ability(abilitys.HYDRATION) &&
						new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) {
				score -= 8;
			}
			ai.each_same_side_battler(target.side) do |b, i|
				if (i != target.index && b.has_active_ability(abilitys.HEALER)) score -= 5;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("ParalyzeTargetIfNotTypeImmune",
	block: (move, user, target, ai, battle) => {
		eff = target.effectiveness_of_type_against_battler(move.rough_type, user, move);
		if (Effectiveness.ineffective(eff)) next true;
		if (move.statusMove() && !target.battler.CanParalyze(user.battler, false, move.move)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("ParalyzeTarget",
																												"ParalyzeTargetIfNotTypeImmune");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("ParalyzeTarget",
																												"ParalyzeTargetAlwaysHitsInRainHitsTargetInSky");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ParalyzeFlinchTarget",
	block: (score, move, user, target, ai, battle) => {
		paralyze_score = Battle.AI.Handlers.apply_move_effect_against_target_score("ParalyzeTarget",
			0, move, user, target, ai, battle);
		flinch_score = Battle.AI.Handlers.apply_move_effect_against_target_score("FlinchTarget",
			0, move, user, target, ai, battle);
		if (paralyze_score == Battle.AI.MOVE_USELESS_SCORE &&
			flinch_score == Battle.AI.MOVE_USELESS_SCORE) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		if (paralyze_score != Battle.AI.MOVE_USELESS_SCORE) score += paralyze_score;
		if (flinch_score != Battle.AI.MOVE_USELESS_SCORE) score += flinch_score;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("BurnTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanBurn(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("BurnTarget",
	block: (score, move, user, target, ai, battle) => {
		useless_score = (move.statusMove()) ? Battle.AI.MOVE_USELESS_SCORE : score;
		// No score modifier if the burn will be removed immediately
		if (target.has_active_item(new {:RAWSTBERRY, :LUMBERRY})) next useless_score;
		if (target.faster_than(user) &&
													target.has_active_ability(abilitys.HYDRATION) &&
													new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) next useless_score;
		if (target.battler.CanBurn(user.battler, false, move.move)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next useless_score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference
			score += 8;
			// Prefer if the target knows any physical moves that will be weaked by a burn
			if (!target.has_active_ability(abilitys.GUTS) && target.check_for_move(m => m.physicalMove())) {
				score += 4;
				if (!target.check_for_move(m => m.specialMove())) score += 4;
			}
			// Prefer if the user or an ally has a move/ability that is better if the target is burned
			ai.each_same_side_battler(user.side) do |b, i|
				if (b.has_move_with_function("DoublePowerIfTargetStatusProblem")) score += 4;
			}
			// Don't prefer if target benefits from having the burn status problem
			if (target.has_active_ability(new {:FLAREBOOST, :GUTS, :MARVELSCALE, :QUICKFEET})) score -= 4;
			if (target.has_active_ability(abilitys.HEATPROOF)) score -= 4;
			if (target.has_active_ability(abilitys.SYNCHRONIZE) &&
										user.battler.CanBurnSynchronize(target.battler)) score -= 15;
			if ((target.has_move_with_function("DoublePowerIfUserPoisonedBurnedParalyzed",
																									"CureUserBurnPoisonParalysis")) score -= 4;
			if (target.check_for_move { |m|
				m.function_code == "GiveUserStatusToTarget" && user.battler.CanBurn(target.battler, false, m)) score -= 8;
			}
			// Don't prefer if the target won't take damage from the burn
			if (!target.battler.takesIndirectDamage()) score -= 10;
			// Don't prefer if the target can heal itself (or be healed by an ally)
			if (target.has_active_ability(abilitys.SHEDSKIN)) {
				score -= 5;
			} else if (target.has_active_ability(abilitys.HYDRATION) &&
						new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) {
				score -= 8;
			}
			ai.each_same_side_battler(target.side) do |b, i|
				if (i != target.index && b.has_active_ability(abilitys.HEALER)) score -= 5;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// BurnTargetIfTargetStatsRaisedThisTurn

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("BurnFlinchTarget",
	block: (score, move, user, target, ai, battle) => {
		burn_score = Battle.AI.Handlers.apply_move_effect_against_target_score("BurnTarget",
			0, move, user, target, ai, battle);
		flinch_score = Battle.AI.Handlers.apply_move_effect_against_target_score("FlinchTarget",
			0, move, user, target, ai, battle);
		if (burn_score == Battle.AI.MOVE_USELESS_SCORE &&
			flinch_score == Battle.AI.MOVE_USELESS_SCORE) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		if (burn_score != Battle.AI.MOVE_USELESS_SCORE) score += burn_score;
		if (flinch_score != Battle.AI.MOVE_USELESS_SCORE) score += flinch_score;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("FreezeTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanFreeze(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("FreezeTarget",
	block: (score, move, user, target, ai, battle) => {
		useless_score = (move.statusMove()) ? Battle.AI.MOVE_USELESS_SCORE : score;
		// No score modifier if the freeze will be removed immediately
		if (target.has_active_item(new {:ASPEARBERRY, :LUMBERRY})) next useless_score;
		if (target.faster_than(user) &&
													target.has_active_ability(abilitys.HYDRATION) &&
													new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) next useless_score;
		if (target.battler.CanFreeze(user.battler, false, move.move)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next useless_score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference
			score += 8;
			// Prefer if the user or an ally has a move/ability that is better if the target is frozen
			ai.each_same_side_battler(user.side) do |b, i|
				if (b.has_move_with_function("DoublePowerIfTargetStatusProblem")) score += 4;
			}
			// Don't prefer if target benefits from having the frozen status problem
			// NOTE: The target's Guts/Quick Feet will benefit from the target being
			//       frozen, but the target won't be able to make use of them, so
			//       they're not worth considering.
			if (target.has_active_ability(abilitys.MARVELSCALE)) score -= 5;
			// Don't prefer if the target knows a move that can thaw it
			if (target.check_for_move(m => m.thawsUser())) score -= 8;
			// Don't prefer if the target can heal itself (or be healed by an ally)
			if (target.has_active_ability(abilitys.SHEDSKIN)) {
				score -= 5;
			} else if (target.has_active_ability(abilitys.HYDRATION) &&
						new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) {
				score -= 8;
			}
			ai.each_same_side_battler(target.side) do |b, i|
				if (i != target.index && b.has_active_ability(abilitys.HEALER)) score -= 5;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("FreezeTarget",
																												"FreezeTargetSuperEffectiveAgainstWater");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("FreezeTarget",
																												"FreezeTargetAlwaysHitsInHail");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("FreezeFlinchTarget",
	block: (score, move, user, target, ai, battle) => {
		freeze_score = Battle.AI.Handlers.apply_move_effect_against_target_score("FreezeTarget",
			0, move, user, target, ai, battle);
		flinch_score = Battle.AI.Handlers.apply_move_effect_against_target_score("FlinchTarget",
			0, move, user, target, ai, battle);
		if (freeze_score == Battle.AI.MOVE_USELESS_SCORE &&
			flinch_score == Battle.AI.MOVE_USELESS_SCORE) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		if (freeze_score != Battle.AI.MOVE_USELESS_SCORE) score += freeze_score;
		if (flinch_score != Battle.AI.MOVE_USELESS_SCORE) score += flinch_score;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ParalyzeBurnOrFreezeTarget",
	block: (score, move, user, target, ai, battle) => {
		// No score modifier if the status problem will be removed immediately
		if (target.has_active_item(items.LUMBERRY)) next score;
		if (target.faster_than(user) &&
									target.has_active_ability(abilitys.HYDRATION) &&
									new []{:Rain, :HeavyRain}.Contains(target.battler.effectiveWeather)) next score;
		// Scores for the possible effects
		new {"ParalyzeTarget", "BurnTarget", "FreezeTarget"}.each do |function_code|
			effect_score = Battle.AI.Handlers.apply_move_effect_against_target_score(function_code,
				0, move, user, target, ai, battle);
			if (effect_score != Battle.AI.MOVE_USELESS_SCORE) score += effect_score / 3;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("GiveUserStatusToTarget",
	block: (move, user, ai, battle) => {
		next user.status == statuses.NONE;
	}
)
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("GiveUserStatusToTarget",
	block: (move, user, target, ai, battle) => {
		next !target.battler.CanInflictStatus(user.status, user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("GiveUserStatusToTarget",
	block: (score, move, user, target, ai, battle) => {
		// Curing the user's status problem
		if (!user.wants_status_problem(user.status)) score += 15;
		// Giving the target a status problem
		function_code = {
			SLEEP     = "SleepTarget",
			PARALYSIS = "ParalyzeTarget",
			POISON    = "PoisonTarget",
			BURN      = "BurnTarget",
			FROZEN    = "FreezeTarget";
		}[user.status];
		if (function_code) {
			new_score = Battle.AI.Handlers.apply_move_effect_against_target_score(function_code,
				score, move, user, target, ai, battle);
			if (new_score != Battle.AI.MOVE_USELESS_SCORE) next new_score;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("CureUserBurnPoisonParalysis",
	block: (move, user, ai, battle) => {
		next !new []{:BURN, :POISON, :PARALYSIS}.Contains(user.status);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("CureUserBurnPoisonParalysis",
	block: (score, move, user, ai, battle) => {
		if (user.wants_status_problem(user.status)) next Battle.AI.MOVE_USELESS_SCORE;
		next score + 20;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("CureUserPartyStatus",
	block: (move, user, ai, battle) => {
		next battle.Party(user.index).none(pkmn => pkmn&.able() && pkmn.status != statuses.NONE);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("CureUserPartyStatus",
	block: (score, move, user, ai, battle) => {
		score = Battle.AI.MOVE_BASE_SCORE;   // Ignore the scores for each targeted battler calculated earlier
		foreach (var pkmn in battle.Party(user.index)) { //'battle.Party(user.index).each' do => |pkmn|
			if (!pkmn || pkmn.status == statuses.NONE) continue;
			if (pkmn.status == statuses.SLEEP && pkmn.statusCount == 1) continue;   // About to wake up
			score += 12;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("CureTargetBurn",
	block: (score, move, user, target, ai, battle) => {
		add_effect = move.get_score_change_for_additional_effect(user, target);
		if (add_effect == -999) next score;   // Additional effect will be negated
		if (target.status == statuses.BURN) {
			score += add_effect;
			if (target.wants_status_problem(:BURN)) {
				score += 10;
			} else {
				score -= 8;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartUserSideImmunityToInflictedStatus",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.Safeguard > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartUserSideImmunityToInflictedStatus",
	block: (score, move, user, ai, battle) => {
		// Not worth it if Misty Terrain is already safeguarding all user side battlers
		if (battle.field.terrain == :Misty &&
			(battle.field.terrainDuration > 1 || battle.field.terrainDuration < 0)) {
			already_immune = true;
			ai.each_same_side_battler(user.side) do |b, i|
				if (!b.battler.affectedByTerrain()) already_immune = false;
			}
			if (already_immune) next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Tends to be wasteful if the foe just has one Pokémon left
		if (battle.AbleNonActiveCount(user.idxOpposingSide) == 0) next score - 20;
		// Prefer for each user side battler
		ai.each_same_side_battler(user.side, (b, i) => { score += 15; });
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("FlinchTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.faster_than(user) || target.effects.Substitute > 0) next score;
		if (target.has_active_ability(abilitys.INNERFOCUS) && !target.being_mold_broken()) next score;
		add_effect = move.get_score_change_for_additional_effect(user, target);
		if (add_effect == -999) next score;   // Additional effect will be negated
		score += add_effect;
		// Inherent preference
		score += 8;
		// Prefer if the target is paralysed, confused or infatuated, to compound the
		// turn skipping
		if (target.status == statuses.PARALYSIS ||
									target.effects.Confusion > 1 ||
									target.effects.Attract >= 0) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("FlinchTarget",
																												"FlinchTargetFailsIfUserNotAsleep");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("FlinchTargetFailsIfNotUserFirstTurn",
	block: (move, user, ai, battle) => {
		next user.turnCount > 0;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("FlinchTarget",
																												"FlinchTargetFailsIfNotUserFirstTurn");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("FlinchTargetDoublePowerIfTargetInSky",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("FlinchTarget",
																												"FlinchTargetDoublePowerIfTargetInSky");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("ConfuseTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanConfuse(user.battler, false, move.move);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ConfuseTarget",
	block: (score, move, user, target, ai, battle) => {
		// No score modifier if the status problem will be removed immediately
		if (target.has_active_item(items.PERSIMBERRY)) next score;
		if (target.battler.CanConfuse(user.battler, false, move.move)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference
			if (ai.trainer.has_skill_flag("HPAware")) {
				score += 8 * target.hp / target.totalhp;
			} else {
				score += 8;
			}
			// Prefer if the target is paralysed or infatuated, to compound the turn skipping
			if (target.status == statuses.PARALYSIS || target.effects.Attract >= 0) score += 5;
			// Don't prefer if target benefits from being confused
			if (target.has_active_ability(abilitys.TANGLEDFEET)) score -= 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("ConfuseTarget",
																												"ConfuseTargetAlwaysHitsInRainHitsTargetInSky");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("AttractTarget",
	block: (move, user, target, ai, battle) => {
		next move.statusMove() && !target.battler.CanAttract(user.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("AttractTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.battler.CanAttract(user.battler, false)) {
			add_effect = move.get_score_change_for_additional_effect(user, target);
			if (add_effect == -999) next score;   // Additional effect will be negated
			score += add_effect;
			// Inherent preference
			score += 8;
			// Prefer if the target is paralysed or confused, to compound the turn skipping
			if (target.status == statuses.PARALYSIS || target.effects.Confusion > 1) score += 5;
			// Don't prefer if the target can infatuate the user because of this move
			if (target.has_active_item(items.DESTINYKNOT) &&
										user.battler.CanAttract(target.battler, false)) score -= 10;
			// Don't prefer if the user has another way to infatuate the target
			if (move.statusMove() && user.has_active_ability(abilitys.CUTECHARM)) score -= 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("SetUserTypesBasedOnEnvironment",
	block: (move, user, ai, battle) => {
		if (!user.battler.canChangeType()) next true;
		new_type = null;
		terr_types = Battle.Move.SetUserTypesBasedOnEnvironment.TERRAIN_TYPES;
		terr_type = terr_types[battle.field.terrain];
		if (terr_type && GameData.Type.exists(terr_type)) {
			new_type = terr_type;
		} else {
			env_types = Battle.Move.SetUserTypesBasedOnEnvironment.ENVIRONMENT_TYPES;
			new_type = env_types[battle.environment] || :NORMAL;
			if (!GameData.Type.exists(new_type)) new_type = types.NORMAL;
		}
		next !GameData.Type.exists(new_type) || !user.battler.HasOtherType(new_type);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("SetUserTypesBasedOnEnvironment",
	block: (score, move, user, ai, battle) => {
		// Determine the new type
		new_type = null;
		terr_types = Battle.Move.SetUserTypesBasedOnEnvironment.TERRAIN_TYPES;
		terr_type = terr_types[battle.field.terrain];
		if (terr_type && GameData.Type.exists(terr_type)) {
			new_type = terr_type;
		} else {
			env_types = Battle.Move.SetUserTypesBasedOnEnvironment.ENVIRONMENT_TYPES;
			new_type = env_types[battle.environment] || :NORMAL;
			if (!GameData.Type.exists(new_type)) new_type = types.NORMAL;
		}
		// Check if any user's moves will get STAB because of the type change
		if (user.has_damaging_move_of_type(new_type)) score += 14;
		// Check if any user's moves will lose STAB because of the type change
		foreach (var type in user.Types(true)) { //'user.Types(true).each' do => |type|
			if (type == new_type) continue;
			if (user.has_damaging_move_of_type(type)) score -= 14;
		}
		// NOTE: Other things could be considered, like the foes' moves'
		//       effectivenesses against the current and new user's type(s), and
		//       which set of STAB is more beneficial. However, I'm keeping this
		//       simple because, if you know this move, you probably want to use it
		//       just because.
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetUserTypesToResistLastAttack",
	block: (move, user, target, ai, battle) => {
		if (!user.battler.canChangeType()) next true;
		if (!target.battler.lastMoveUsed || !target.battler.lastMoveUsedType ||
								GameData.Type.get(target.battler.lastMoveUsedType).pseudo_type) next true;
		has_possible_type = false;
		foreach (var t in GameData.Type) { //'GameData.Type.each' do => |t|
			if (t.pseudo_type || user.has_type(t.id) ||
							!Effectiveness.resistant_type(target.battler.lastMoveUsedType, t.id)) continue;
			has_possible_type = true;
			break;
		}
		next !has_possible_type;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetUserTypesToResistLastAttack",
	block: (score, move, user, target, ai, battle) => {
		effectiveness = user.effectiveness_of_type_against_battler(target.battler.lastMoveUsedType, target);
		if (Effectiveness.ineffective(effectiveness)) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (Effectiveness.super_effective(effectiveness)) {
			score += 15;
		} else if (Effectiveness.normal(effectiveness)) {
			score += 10;
		} else {   // Not very effective
			score += 5;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetUserTypesToTargetTypes",
	block: (move, user, target, ai, battle) => {
		if (!user.battler.canChangeType()) next true;
		if (target.Types(true).empty()) next true;
		if (user.Types == target.Types &&
								user.effects.ExtraType == target.effects.ExtraType) next true;
		next false;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("SetUserTypesToUserMoveType",
	block: (move, user, ai, battle) => {
		if (!user.battler.canChangeType()) next true;
		has_possible_type = false;
		user.battler.eachMoveWithIndex do |m, i|
			if (Settings.MECHANICS_GENERATION >= 6 && i > 0) break;
			if (GameData.Type.get(m.type).pseudo_type) continue;
			if (user.has_type(m.type)) continue;
			has_possible_type = true;
			break;
		}
		next !has_possible_type;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetUserTypesToUserMoveType",
	block: (score, move, user, target, ai, battle) => {
		possible_types = new List<string>();
		user.battler.eachMoveWithIndex do |m, i|
			if (Settings.MECHANICS_GENERATION >= 6 && i > 0) break;
			if (GameData.Type.get(m.type).pseudo_type) continue;
			if (user.has_type(m.type)) continue;
			possible_types.Add(m.type);
		}
		// Check if any user's moves will get STAB because of the type change
		foreach (var type in possible_types) { //'possible_types.each' do => |type|
			if (!user.has_damaging_move_of_type(type)) continue;
			score += 14;
			break;
		}
		// NOTE: Other things could be considered, like the foes' moves'
		//       effectivenesses against the current and new user's type(s), and
		//       whether any of the user's moves will lose STAB because of the type
		//       change (and if so, which set of STAB is more beneficial). However,
		//       I'm keeping this simple because, if you know this move, you probably
		//       want to use it just because.
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetTargetTypesToPsychic",
	block: (move, user, target, ai, battle) => {
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetTargetTypesToPsychic",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if target's foes know damaging moves that are super-effective
		// against Psychic, and don't prefer if they know damaging moves that are
		// ineffective against Psychic
		ai.each_foe_battler(target.side) do |b, i|
			foreach (var m in b.battler.Moves) { //b.battler.eachMove do => |m|
				if (!m.damagingMove()) continue;
				effectiveness = Effectiveness.calculate(m.CalcType(b.battler), :PSYCHIC);
				if (Effectiveness.super_effective(effectiveness)) {
					score += 10;
				} else if (Effectiveness.ineffective(effectiveness)) {
					score -= 10;
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("SetTargetTypesToPsychic",
																												"SetTargetTypesToWater");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetTargetTypesToWater",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if target's foes know damaging moves that are super-effective
		// against Water, and don't prefer if they know damaging moves that are
		// ineffective against Water
		ai.each_foe_battler(target.side) do |b, i|
			foreach (var m in b.battler.Moves) { //b.battler.eachMove do => |m|
				if (!m.damagingMove()) continue;
				effectiveness = Effectiveness.calculate(m.CalcType(b.battler), :WATER);
				if (Effectiveness.super_effective(effectiveness)) {
					score += 10;
				} else if (Effectiveness.ineffective(effectiveness)) {
					score -= 10;
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("SetTargetTypesToWater",
																												"AddGhostTypeToTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("AddGhostTypeToTarget",
	block: (score, move, user, target, ai, battle) => {
		// Prefer/don't prefer depending on the effectiveness of the target's foes'
		// damaging moves against the added type
		ai.each_foe_battler(target.side) do |b, i|
			foreach (var m in b.battler.Moves) { //b.battler.eachMove do => |m|
				if (!m.damagingMove()) continue;
				effectiveness = Effectiveness.calculate(m.CalcType(b.battler), :GHOST);
				if (Effectiveness.super_effective(effectiveness)) {
					score += 10;
				} else if (Effectiveness.not_very_effective(effectiveness)) {
					score -= 5;
				} else if (Effectiveness.ineffective(effectiveness)) {
					score -= 10;
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("AddGhostTypeToTarget",
																												"AddGrassTypeToTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("AddGrassTypeToTarget",
	block: (score, move, user, target, ai, battle) => {
		// Prefer/don't prefer depending on the effectiveness of the target's foes'
		// damaging moves against the added type
		ai.each_foe_battler(target.side) do |b, i|
			foreach (var m in b.battler.Moves) { //b.battler.eachMove do => |m|
				if (!m.damagingMove()) continue;
				effectiveness = Effectiveness.calculate(m.CalcType(b.battler), :GRASS);
				if (Effectiveness.super_effective(effectiveness)) {
					score += 10;
				} else if (Effectiveness.not_very_effective(effectiveness)) {
					score -= 5;
				} else if (Effectiveness.ineffective(effectiveness)) {
					score -= 10;
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserLosesFireType",
	block: (move, user, ai, battle) => {
		next !user.has_type(types.FIRE);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetTargetAbilityToSimple",
	block: (move, user, target, ai, battle) => {
		if (!GameData.Abilitys.exists(Abilitys.SIMPLE)) next true;
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetTargetAbilityToSimple",
	block: (score, move, user, target, ai, battle) => {
		if (!target.ability_active()) next Battle.AI.MOVE_USELESS_SCORE;
		old_ability_rating = target.wants_ability(target.ability_id);
		new_ability_rating = target.wants_ability(abilitys.SIMPLE);
		side_mult = (target.opposes(user)) ? 1 : -1;
		if (old_ability_rating > new_ability_rating) {
			score += 5 * side_mult * (int)Math.Max(old_ability_rating - new_ability_rating, 3);
		} else if (old_ability_rating < new_ability_rating) {
			score -= 5 * side_mult * (int)Math.Max(new_ability_rating - old_ability_rating, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetTargetAbilityToInsomnia",
	block: (move, user, target, ai, battle) => {
		if (!GameData.Abilitys.exists(Abilitys.INSOMNIA)) next true;
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetTargetAbilityToInsomnia",
	block: (score, move, user, target, ai, battle) => {
		if (!target.ability_active()) next Battle.AI.MOVE_USELESS_SCORE;
		old_ability_rating = target.wants_ability(target.ability_id);
		new_ability_rating = target.wants_ability(abilitys.INSOMNIA);
		side_mult = (target.opposes(user)) ? 1 : -1;
		if (old_ability_rating > new_ability_rating) {
			score += 5 * side_mult * (int)Math.Max(old_ability_rating - new_ability_rating, 3);
		} else if (old_ability_rating < new_ability_rating) {
			score -= 5 * side_mult * (int)Math.Max(new_ability_rating - old_ability_rating, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetUserAbilityToTargetAbility",
	block: (move, user, target, ai, battle) => {
		if (user.battler.unstoppableAbility()) next true;
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetUserAbilityToTargetAbility",
	block: (score, move, user, target, ai, battle) => {
		if (!user.ability_active()) next Battle.AI.MOVE_USELESS_SCORE;
		old_ability_rating = user.wants_ability(user.ability_id);
		new_ability_rating = user.wants_ability(target.ability_id);
		if (old_ability_rating > new_ability_rating) {
			score += 5 * (int)Math.Max(old_ability_rating - new_ability_rating, 3);
		} else if (old_ability_rating < new_ability_rating) {
			score -= 5 * (int)Math.Max(new_ability_rating - old_ability_rating, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("SetTargetAbilityToUserAbility",
	block: (move, user, target, ai, battle) => {
		if (!user.ability || user.ability_id == target.ability_id) next true;
		if (user.battler.ungainableAbility() ||
								new []{:POWEROFALCHEMY, :RECEIVER, :TRACE}.Contains(user.ability_id)) next true;
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("SetTargetAbilityToUserAbility",
	block: (score, move, user, target, ai, battle) => {
		if (!target.ability_active()) next Battle.AI.MOVE_USELESS_SCORE;
		old_ability_rating = target.wants_ability(target.ability_id);
		new_ability_rating = target.wants_ability(user.ability_id);
		side_mult = (target.opposes(user)) ? 1 : -1;
		if (old_ability_rating > new_ability_rating) {
			score += 5 * side_mult * (int)Math.Max(old_ability_rating - new_ability_rating, 3);
		} else if (old_ability_rating < new_ability_rating) {
			score -= 5 * side_mult * (int)Math.Max(new_ability_rating - old_ability_rating, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("UserTargetSwapAbilities",
	block: (move, user, target, ai, battle) => {
		if (!user.ability || user.battler.unstoppableAbility() ||
								user.battler.ungainableAbility() || user.ability_id == abilitys.WONDERGUARD) next true;
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetSwapAbilities",
	block: (score, move, user, target, ai, battle) => {
		if (!user.ability_active() && !target.ability_active()) next Battle.AI.MOVE_USELESS_SCORE;
		old_user_ability_rating = user.wants_ability(user.ability_id);
		new_user_ability_rating = user.wants_ability(target.ability_id);
		user_diff = new_user_ability_rating - old_user_ability_rating;
		if (!user.ability_active()) user_diff = 0;
		old_target_ability_rating = target.wants_ability(target.ability_id);
		new_target_ability_rating = target.wants_ability(user.ability_id);
		target_diff = new_target_ability_rating - old_target_ability_rating;
		if (!target.ability_active()) target_diff = 0;
		side_mult = (target.opposes(user)) ? 1 : -1;
		if (user_diff > target_diff) {
			score += 5 * side_mult * (int)Math.Max(user_diff - target_diff, 3);
		} else if (target_diff < user_diff) {
			score -= 5 * side_mult * (int)Math.Max(target_diff - user_diff, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("NegateTargetAbility",
	block: (move, user, target, ai, battle) => {
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("NegateTargetAbility",
	block: (score, move, user, target, ai, battle) => {
		if (!target.ability_active()) next Battle.AI.MOVE_USELESS_SCORE;
		target_ability_rating = target.wants_ability(target.ability_id);
		side_mult = (target.opposes(user)) ? 1 : -1;
		if (target_ability_rating > 0) {
			score += 5 * side_mult * (int)Math.Max(target_ability_rating, 3);
		} else if (target_ability_rating < 0) {
			score -= 5 * side_mult * (int)Math.Max(target_ability_rating.abs, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("NegateTargetAbilityIfTargetActed",
	block: (score, move, user, target, ai, battle) => {
		if (target.effects.Substitute > 0) next score;
		if (target.battler.unstoppableAbility() || !target.ability_active()) next score;
		if (user.faster_than(target)) next score;
		target_ability_rating = target.wants_ability(target.ability_id);
		side_mult = (target.opposes(user)) ? 1 : -1;
		if (target_ability_rating > 0) {
			score += 5 * side_mult * (int)Math.Max(target_ability_rating, 3);
		} else if (target_ability_rating < 0) {
			score -= 5 * side_mult * (int)Math.Max(target_ability_rating.abs, 3);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// IgnoreTargetAbility

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartUserAirborne",
	block: (move, user, ai, battle) => {
		if (user.has_active_item(items.IRONBALL)) next true;
		if (user.effects.Ingrain ||
								user.effects.SmackDown ||
								user.effects.MagnetRise > 0) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartUserAirborne",
	block: (score, move, user, ai, battle) => {
		// Move is useless if user is already airborne
		if (user.has_type(types.FLYING) ||
			user.has_active_ability(abilitys.LEVITATE) ||
			user.has_active_item(items.AIRBALLOON) ||
			user.effects.Telekinesis > 0) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Prefer if any foes have damaging Ground-type moves that do 1x or more
		// damage to the user
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.has_damaging_move_of_type(types.GROUND)) continue;
			if (Effectiveness.resistant(user.effectiveness_of_type_against_battler(:GROUND, b))) continue;
			score += 10;
		}
		// Don't prefer if terrain exists (which the user will no longer be affected by)
		if (ai.trainer.medium_skill()) {
			if (battle.field.terrain != :None) score -= 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("StartTargetAirborneAndAlwaysHitByMoves",
	block: (move, user, target, ai, battle) => {
		if (target.has_active_item(items.IRONBALL)) next true;
		next move.move.FailsAgainstTarget(user.battler, target.battler, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("StartTargetAirborneAndAlwaysHitByMoves",
	block: (score, move, user, target, ai, battle) => {
		// Move is useless if the target is already airborne
		if (target.has_type(types.FLYING) ||
			target.has_active_ability(abilitys.LEVITATE) ||
			target.has_active_item(items.AIRBALLOON)) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Prefer if any allies have moves with accuracy < 90%
		// Don't prefer if any allies have damaging Ground-type moves that do 1x or
		// more damage to the target
		ai.each_foe_battler(target.side) do |b, i|
			foreach (var m in b.battler.Moves) { //b.battler.eachMove do => |m|
				acc = m.accuracy;
				if (ai.trainer.medium_skill()) acc = m.BaseAccuracy(b.battler, target.battler);
				if (acc < 90 && acc != 0) score += 5;
				if (acc <= 50 && acc != 0) score += 5;
			}
			if (!b.has_damaging_move_of_type(types.GROUND)) continue;
			if (Effectiveness.resistant(target.effectiveness_of_type_against_battler(:GROUND, b))) continue;
			score -= 7;
		}
		// Prefer if terrain exists (which the target will no longer be affected by)
		if (ai.trainer.medium_skill()) {
			if (battle.field.terrain != :None) score += 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// HitsTargetInSky

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitsTargetInSkyGroundsTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.effects.Substitute > 0) next score;
		if (!target.battler.airborne()) {
			if (target.faster_than(user) ||
										!target.battler.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
																										"TwoTurnAttackInvulnerableInSkyParalyzeTarget")) next score;
		}
		// Prefer if the target is airborne
		score += 10;
		// Prefer if any allies have damaging Ground-type moves
		ai.each_foe_battler(target.side) do |b, i|
			if (b.has_damaging_move_of_type(types.GROUND)) score += 8;
		}
		// Don't prefer if terrain exists (which the target will become affected by)
		if (ai.trainer.medium_skill()) {
			if (battle.field.terrain != :None) score -= 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartGravity",
	block: (move, user, ai, battle) => {
		next battle.field.effects.Gravity > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartGravity",
	block: (score, move, user, ai, battle) => {
		ai.each_battler do |b, i|
			// Prefer grounding airborne foes, don't prefer grounding airborne allies
			// Prefer making allies affected by terrain, don't prefer making foes
			// affected by terrain
			if (b.battler.airborne()) {
				score_change = 10;
				if (ai.trainer.medium_skill()) {
					if (battle.field.terrain != :None) score_change -= 8;
				}
				score += (user.opposes(b)) ? score_change : -score_change;
				// Prefer if allies have any damaging Ground moves they'll be able to use
				// on a grounded foe, and vice versa
				ai.each_foe_battler(b.side) do |b2, j|
					if (!b2.has_damaging_move_of_type(types.GROUND)) continue;
					score += (user.opposes(b2)) ? -8 : 8;
				}
			}
			// Prefer ending Sky Drop being used on allies, don't prefer ending Sky
			// Drop being used on foes
			if (b.effects.SkyDrop >= 0) {
				score += (user.opposes(b)) ? -8 : 8;
			}
			// Gravity raises accuracy of all moves; prefer if the user/ally has low
			// accuracy moves, don't prefer if foes have any
			if (b.check_for_move(m => m.accuracy < 85)) {
				score += (user.opposes(b)) ? -8 : 8;
			}
			// Prefer stopping foes' sky-based attacks, don't prefer stopping allies'
			// sky-based attacks
			if (user.faster_than(b) &&
				b.battler.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
																		"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
																		"TwoTurnAttackInvulnerableInSkyTargetCannotAct")) {
				score += (user.opposes(b)) ? 10 : -10;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TransformUserIntoTarget",
	block: (move, user, target, ai, battle) => {
		if (user.effects.Transform) next true;
		if (target.effects.Transform ||
								target.effects.Illusion) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TransformUserIntoTarget",
	block: (score, move, user, target, ai, battle) => {
		next score - 5;
	}
)
