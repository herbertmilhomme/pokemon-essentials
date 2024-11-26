//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("HealUserFullyAndFallAsleep",
	block: (move, user, ai, battle) => {
		if (!user.battler.canHeal()) next true;
		if (user.battler.asleep()) next true;
		if (!user.battler.CanSleep(user.battler, false, move.move, true)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("HealUserFullyAndFallAsleep",
	block: (score, move, user, ai, battle) => {
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) {
				score -= 10;
			} else {
				score += 30 * (user.totalhp - user.hp) / user.totalhp;   // +15 to +30
			}
		}
		// Check whether an existing status problem will be removed
		if (user.status != statuses.NONE) {
			score += (user.wants_status_problem(user.status)) ? -10 : 8;
		}
		// Check if user is happy to be asleep, e.g. can use moves while asleep
		if (ai.trainer.medium_skill()) {
			score += (user.wants_status_problem(:SLEEP)) ? 10 : -8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("HealUserHalfOfTotalHP",
	block: (move, user, ai, battle) => {
		next !user.battler.canHeal();
	}
)
Battle.AI.Handlers.MoveEffectScore.add("HealUserHalfOfTotalHP",
	block: (score, move, user, ai, battle) => {
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) next score - 10;
			score += 30 * (user.totalhp - user.hp) / user.totalhp;   // +15 to +30
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("HealUserHalfOfTotalHP",
																						"HealUserDependingOnWeather");
Battle.AI.Handlers.MoveEffectScore.add("HealUserDependingOnWeather",
	block: (score, move, user, ai, battle) => {
		// Consider how much HP will be restored
		score = Battle.AI.Handlers.apply_move_effect_score("HealUserHalfOfTotalHP",
			score, move, user, ai, battle);
		switch (user.battler.effectiveWeather) {
			case :Sun: case :HarshSun:
				score += 5;
				break;
			case :None: case :StrongWinds:
				break;
			default:
				score -= 10;
				break;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("HealUserHalfOfTotalHP",
																						"HealUserDependingOnSandstorm");
Battle.AI.Handlers.MoveEffectScore.add("HealUserDependingOnSandstorm",
	block: (score, move, user, ai, battle) => {
		// Consider how much HP will be restored
		score = Battle.AI.Handlers.apply_move_effect_score("HealUserHalfOfTotalHP",
			score, move, user, ai, battle);
		if (user.battler.effectiveWeather == Weathers.Sandstorm) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("HealUserHalfOfTotalHP",
																						"HealUserHalfOfTotalHPLoseFlyingTypeThisTurn");
Battle.AI.Handlers.MoveEffectScore.add("HealUserHalfOfTotalHPLoseFlyingTypeThisTurn",
	block: (score, move, user, ai, battle) => {
		// Consider how much HP will be restored
		score = Battle.AI.Handlers.apply_move_effect_score("HealUserHalfOfTotalHP",
			score, move, user, ai, battle);
		// User loses the Flying type this round
		// NOTE: Not worth considering and scoring for.
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("CureTargetStatusHealUserHalfOfTotalHP",
	block: (move, user, target, ai, battle) => {
		next !user.battler.canHeal() || target.status == statuses.NONE;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("CureTargetStatusHealUserHalfOfTotalHP",
	block: (score, move, user, target, ai, battle) => {
		// Consider how much HP will be restored
		score = Battle.AI.Handlers.apply_move_effect_score("HealUserHalfOfTotalHP",
			score, move, user, ai, battle);
		// Will cure target's status
		score += (target.wants_status_problem(target.status)) ? 10 : -8;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("HealUserByTargetAttackLowerTargetAttack1",
	block: (move, user, target, ai, battle) => {
		if (target.has_active_ability(abilitys.CONTRARY) && !target.being_mold_broken()) {
			next target.statStageAtMax(:ATTACK);
		}
		next target.statStageAtMin(:ATTACK);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealUserByTargetAttackLowerTargetAttack1",
	block: (score, move, user, target, ai, battle) => {
		// Check whether lowering the target's Attack will have any impact
		if (ai.trainer.medium_skill()) {
			score = ai.get_score_for_target_stat_drop(score, target, move.move.statDown);
		}
		// Healing the user
		if (target.has_active_ability(abilitys.LIQUIDOOZE)) {
			score -= 20;
		} else if (user.battler.canHeal()) {
			if (user.has_active_item(items.BIGROOT)) score += 5;
			if (ai.trainer.has_skill_flag("HPAware")) {
				// Consider how much HP will be restored
				heal_amt = target.rough_stat(:ATTACK);
				if (user.has_active_item(items.BIGROOT)) heal_amt *= 1.3;
				heal_amt = (int)Math.Min(heal_amt, user.totalhp - user.hp);
				if (heal_amt > user.totalhp * 0.2) {   // Only modify the score if it'll heal a decent amount
					if (user.hp < user.totalhp * 0.5) {
						score += 20 * (user.totalhp - user.hp) / user.totalhp;   // +10 to +20
					}
					score += 20 * heal_amt / user.totalhp;   // +4 to +20
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealUserByHalfOfDamageDone",
	block: (score, move, user, target, ai, battle) => {
		rough_dmg = move.rough_damage;
		if (target.has_active_ability(abilitys.LIQUIDOOZE)) {
			if (rough_dmg < target.hp) score -= 20;
		} else if (user.battler.canHeal()) {
			if (user.has_active_item(items.BIGROOT)) score += 5;
			if (ai.trainer.has_skill_flag("HPAware")) {
				// Consider how much HP will be restored
				heal_amt = rough_dmg / 2;
				if (user.has_active_item(items.BIGROOT)) heal_amt *= 1.3;
				heal_amt = (int)Math.Min(heal_amt, user.totalhp - user.hp);
				if (heal_amt > user.totalhp * 0.2) {   // Only modify the score if it'll heal a decent amount
					if (user.hp < user.totalhp * 0.5) {
						score += 20 * (user.totalhp - user.hp) / user.totalhp;   // +10 to +20
					}
					score += 20 * heal_amt / user.totalhp;   // +4 to +20
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("HealUserByHalfOfDamageDoneIfTargetAsleep",
	block: (move, user, target, ai, battle) => {
		next !target.battler.asleep();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("HealUserByHalfOfDamageDone",
																												"HealUserByHalfOfDamageDoneIfTargetAsleep");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealUserByThreeQuartersOfDamageDone",
	block: (score, move, user, target, ai, battle) => {
		rough_dmg = move.rough_damage;
		if (target.has_active_ability(abilitys.LIQUIDOOZE)) {
			if (rough_dmg < target.hp) score -= 20;
		} else if (user.battler.canHeal()) {
			if (user.has_active_item(items.BIGROOT)) score += 5;
			if (ai.trainer.has_skill_flag("HPAware")) {
				// Consider how much HP will be restored
				heal_amt = rough_dmg * 0.75;
				if (user.has_active_item(items.BIGROOT)) heal_amt *= 1.3;
				heal_amt = (int)Math.Min(heal_amt, user.totalhp - user.hp);
				if (heal_amt > user.totalhp * 0.2) {   // Only modify the score if it'll heal a decent amount
					if (user.hp < user.totalhp * 0.5) {
						score += 20 * (user.totalhp - user.hp) / user.totalhp;   // +10 to +20
					}
					score += 20 * heal_amt / user.totalhp;   // +4 to +20
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("HealUserAndAlliesQuarterOfTotalHP",
	block: (move, user, target, ai, battle) => {
		next !target.battler.canHeal();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealUserAndAlliesQuarterOfTotalHP",
	block: (score, move, user, target, ai, battle) => {
		if (!target.battler.canHeal()) next score;
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp >= target.totalhp * 0.75) {
				score -= 5;
			} else {
				score += 15 * (target.totalhp - target.hp) / target.totalhp;   // +3 to +15
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("HealUserAndAlliesQuarterOfTotalHPCureStatus",
	block: (move, user, target, ai, battle) => {
		next !target.battler.canHeal() && target.status == statuses.NONE;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealUserAndAlliesQuarterOfTotalHPCureStatus",
	block: (score, move, user, target, ai, battle) => {
		// Consider how much HP will be restored
		score = Battle.AI.Handlers.apply_move_effect_score("HealUserAndAlliesQuarterOfTotalHP",
			score, move, user, ai, battle);
		// Check whether an existing status problem will be removed
		if (target.status != statuses.NONE) {
			score += (target.wants_status_problem(target.status)) ? -10 : 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("HealTargetHalfOfTotalHP",
	block: (move, user, target, ai, battle) => {
		next !target.battler.canHeal();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealTargetHalfOfTotalHP",
	block: (score, move, user, target, ai, battle) => {
		if (target.opposes(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp >= target.totalhp * 0.5) {
				score -= 10;
			} else {
				heal_amt = target.totalhp * 0.5;
				if (move.move.pulseMove() &&
																						user.has_active_ability(abilitys.MEGALAUNCHER)) heal_amt = target.totalhp * 0.75;
				heal_amt = (int)Math.Min(heal_amt, target.totalhp - target.hp);
				score += 20 * (target.totalhp - target.hp) / target.totalhp;   // +10 to +20
				score += 20 * heal_amt / target.totalhp;   // +10 or +15
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("HealTargetHalfOfTotalHP",
																												"HealTargetDependingOnGrassyTerrain");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealTargetDependingOnGrassyTerrain",
	block: (score, move, user, target, ai, battle) => {
		if (user.opposes(target)) next Battle.AI.MOVE_USELESS_SCORE;
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp >= target.totalhp * 0.5) {
				score -= 10;
			} else {
				heal_amt = target.totalhp * 0.5;
				if (battle.field.terrain == :Grassy) heal_amt = (int)Math.Round(target.totalhp * 2 / 3.0);
				heal_amt = (int)Math.Min(heal_amt, target.totalhp - target.hp);
				score += 20 * (target.totalhp - target.hp) / target.totalhp;   // +10 to +20
				score += 20 * heal_amt / target.totalhp;   // +10 or +13
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("HealUserPositionNextTurn",
	block: (move, user, ai, battle) => {
		next battle.positions[user.index].effects.Wish > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("HealUserPositionNextTurn",
	block: (score, move, user, ai, battle) => {
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) {
				score -= 10;
			} else {
				score += 20 * (user.totalhp - user.hp) / user.totalhp;   // +10 to +20
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartHealUserEachTurn",
	block: (move, user, ai, battle) => {
		next user.effects.AquaRing;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartHealUserEachTurn",
	block: (score, move, user, ai, battle) => {
		score += 15;
		if (user.has_active_item(items.BIGROOT)) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartHealUserEachTurnTrapUserInBattle",
	block: (move, user, ai, battle) => {
		next user.effects.Ingrain;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartHealUserEachTurnTrapUserInBattle",
	block: (score, move, user, ai, battle) => {
		score += 8;
		if (user.turnCount < 2) score += 15;
		if (user.has_active_item(items.BIGROOT)) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("StartDamageTargetEachTurnIfTargetAsleep",
	block: (move, user, target, ai, battle) => {
		next !target.battler.asleep() || target.effects.Nightmare;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("StartDamageTargetEachTurnIfTargetAsleep",
	block: (score, move, user, target, ai, battle) => {
		if (target.statusCount <= 1) next Battle.AI.MOVE_USELESS_SCORE;
		next score + (8 * target.statusCount);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("StartLeechSeedTarget",
	block: (move, user, target, ai, battle) => {
		if (target.effects.LeechSeed >= 0) next true;
		if (target.has_type(types.GRASS) || !target.battler.takesIndirectDamage()) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("StartLeechSeedTarget",
	block: (score, move, user, target, ai, battle) => {
		score += 15;
		// Prefer early on
		if (user.turnCount < 2) score += 10;
		if (ai.trainer.medium_skill()) {
			// Prefer if the user has no damaging moves
			if (!user.check_for_move(m => m.damagingMove())) score += 10;
			// Prefer if the target can't switch out to remove its seeding
			if (!battle.CanChooseNonActive(target.index)) score += 8;
			// Don't prefer if the leeched HP will hurt the user
			if (target.has_active_ability([:LIQUIDOOZE])) score -= 20;
		}
		if (ai.trainer.high_skill()) {
			// Prefer if user can stall while damage is dealt
			if (user.check_for_move(m => m.is_a(Battle.Move.ProtectMove))) {
				score += 10;
			}
			// Don't prefer if target can remove the seed
			if (target.has_move_with_function("RemoveUserBindingAndEntryHazards")) {
				score -= 15;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("UserLosesHalfOfTotalHP",
	block: (score, move, user, ai, battle) => {
		score -= 15;   // User will lose 50% HP, don't prefer this move
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.75) score += 15;   // User has HP to spare
			if (user.hp <= user.totalhp * 0.25) score += 15;   // User is near fainting anyway; suicide
		}
		if (ai.trainer.high_skill()) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (reserves == 0) {   // AI is down to its last Pokémon
				score += 30;      // => Go out with a bang
			} else if (foes == 0) {    // Foe is down to their last Pokémon, AI has reserves
				score += 20;      // => Go for the kill
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserLosesHalfOfTotalHPExplosive",
	block: (move, user, ai, battle) => {
		next !battle.CheckGlobalAbility(:DAMP, true).null();
	}
)
Battle.AI.Handlers.MoveEffectScore.copy("UserLosesHalfOfTotalHP",
																					"UserLosesHalfOfTotalHPExplosive");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("UserLosesHalfOfTotalHPExplosive",
																						"UserFaintsExplosive");
Battle.AI.Handlers.MoveEffectScore.add("UserFaintsExplosive",
	block: (score, move, user, ai, battle) => {
		score -= 20;   // User will faint, don't prefer this move
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) score -= 10;
			if (user.hp <= user.totalhp * 0.25) score += 20;   // User is near fainting anyway; suicide
		}
		if (ai.trainer.high_skill()) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (reserves == 0) {   // AI is down to its last Pokémon
				score += 30;      // => Go out with a bang
			} else if (foes == 0) {    // Foe is down to their last Pokémon, AI has reserves
				score += 20;      // => Go for the kill
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("UserFaintsExplosive",
																						"UserFaintsPowersUpInMistyTerrainExplosive");
Battle.AI.Handlers.MoveBasePower.add("UserFaintsPowersUpInMistyTerrainExplosive",
	block: (power, move, user, target, ai, battle) => {
		if (battle.field.terrain == :Misty) power = power * 3 / 2;
		next power;
	}
)
Battle.AI.Handlers.MoveEffectScore.copy("UserFaintsExplosive",
																					"UserFaintsPowersUpInMistyTerrainExplosive");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("UserFaintsFixedDamageUserHP",
	block: (power, move, user, target, ai, battle) => {
		next user.hp;
	}
)
Battle.AI.Handlers.MoveEffectScore.copy("UserFaintsExplosive",
																					"UserFaintsFixedDamageUserHP");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserFaintsLowerTargetAtkSpAtk2",
	block: (score, move, user, target, ai, battle) => {
		score -= 20;   // User will faint, don't prefer this move
		// Check the impact of lowering the target's stats
		score = ai.get_score_for_target_stat_drop(score, target, move.move.statDown);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for the user fainting
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) score -= 10;
			if (user.hp <= user.totalhp * 0.25) score += 20;   // User is near fainting anyway; suicide
		}
		if (ai.trainer.high_skill()) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (reserves > 0 && foes == 0) {    // Foe is down to their last Pokémon, AI has reserves
				score += 20;                   // => Can afford to lose this Pokémon
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserFaintsHealAndCureReplacement",
	block: (move, user, ai, battle) => {
		next !battle.CanChooseNonActive(user.index);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("UserFaintsHealAndCureReplacement",
	block: (score, move, user, ai, battle) => {
		score -= 20;   // User will faint, don't prefer this move
		// Check whether the replacement user needs healing, and don't make the below
		// calculations if not
		if (ai.trainer.medium_skill()) {
			need_healing = false;
			battle.eachInTeamFromBattlerIndex(user.index) do |pkmn, party_index|
				if (pkmn.hp >= pkmn.totalhp * 0.75 && pkmn.status == statuses.NONE) continue;
				need_healing = true;
				break;
			}
			if (!need_healing) next Battle.AI.MOVE_USELESS_SCORE;
			score += 10;
		}
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) score -= 10;
			if (user.hp <= user.totalhp * 0.25) score += 20;   // User is near fainting anyway; suicide
		}
		if (ai.trainer.high_skill()) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (reserves > 0 && foes == 0) {    // Foe is down to their last Pokémon, AI has reserves
				score += 20;                   // => Can afford to lose this Pokémon
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("UserFaintsHealAndCureReplacement",
																						"UserFaintsHealAndCureReplacementRestorePP");
Battle.AI.Handlers.MoveEffectScore.copy("UserFaintsHealAndCureReplacement",
																					"UserFaintsHealAndCureReplacementRestorePP");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("StartPerishCountsForAllBattlers",
	block: (move, user, target, ai, battle) => {
		if (target.effects.PerishSong > 0) next true;
		if (!target.ability_active()) next false;
		next Battle.AbilityEffects.triggerMoveImmunity(target.ability, user.battler, target.battler,
																										move.move, move.rough_type, battle, false);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartPerishCountsForAllBattlers",
	block: (score, move, user, ai, battle) => {
		score -= 15;
		// Check which battlers will be affected by this move
		if (ai.trainer.medium_skill()) {
			allies_affected = 0;
			foes_affected = 0;
			foes_with_high_hp = 0;
			foreach (var b in ai) { //ai.each_battler do => |b|
				if ((Battle.AI.Handlers.move_will_fail_against_target("StartPerishCountsForAllBattlers",
																																		move, user, b, ai, battle)) continue;
				if (b.opposes(user)) {
					foes_affected += 1;
					if (b.hp >= b.totalhp * 0.75) foes_with_high_hp += 1;
				} else {
					allies_affected += 1;
				}
			}
			if (foes_affected == 0) next Battle.AI.MOVE_USELESS_SCORE;
			if (allies_affected == 0) score += 15;   // No downside for user; cancel out inherent negative score
			score -= 15 * allies_affected;
			score += 20 * foes_affected;
			if (ai.trainer.has_skill_flag("HPAware")) score += 10 * foes_with_high_hp;
		}
		if (ai.trainer.high_skill()) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (foes == 0) {          // Foe is down to their last Pokémon, can't lose Perish count
				score += 25;         // => Want to auto-win in 3 turns
			} else if (reserves == 0) {   // AI is down to its last Pokémon, can't lose Perish count
				score -= 15;         // => Don't want to auto-lose in 3 turns
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("AttackerFaintsIfUserFaints",
	block: (move, user, ai, battle) => {
		next Settings.MECHANICS_GENERATION >= 7 && user.effects.DestinyBondPrevious;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("AttackerFaintsIfUserFaints",
	block: (score, move, user, ai, battle) => {
		score -= 25;
		// Check whether user is faster than its foe(s) and could use this move
		user_faster_count = 0;
		ai.each_foe_battler(user.side) do |b, i|
			if (user.faster_than(b)) user_faster_count += 1;
		}
		if (user_faster_count == 0) next score;   // Move will almost certainly have no effect
		score += 7 * user_faster_count;
		// Prefer this move at lower user HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp * 0.4) score += 20;
			if (user.hp <= user.totalhp * 0.25) score += 10;
			if (user.hp <= user.totalhp * 0.1) score += 15;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("SetAttackerMovePPTo0IfUserFaints",
	block: (score, move, user, ai, battle) => {
		score -= 25;
		// Check whether user is faster than its foe(s) and could use this move
		user_faster_count = 0;
		ai.each_foe_battler(user.side) do |b, i|
			if (user.faster_than(b)) user_faster_count += 1;
		}
		if (user_faster_count == 0) next score;   // Move will almost certainly have no effect
		score += 7 * user_faster_count;
		// Prefer this move at lower user HP (not as preferred as Destiny Bond, though)
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp * 0.4) score += 20;
			if (user.hp <= user.totalhp * 0.25) score += 10;
			if (user.hp <= user.totalhp * 0.1) score += 15;
		}
		next score;
	}
)
