//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("HitTwoTimes",
	block: (power, move, user, target, ai, battle) => {
		next power * move.move.NumHits(user.battler, [target.battler]);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitTwoTimes",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if the target has a Substitute and this move can break it before
		// the last hit
		if (target.effects.Substitute > 0 && !move.move.ignoresSubstitute(user.battler)) {
			dmg = move.rough_damage;
			num_hits = move.move.NumHits(user.battler, [target.battler]);
			if (target.effects.Substitute < dmg * (num_hits - 1) / num_hits) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("HitTwoTimes",
																				"HitTwoTimesPoisonTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitTwoTimesPoisonTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for hitting multiple times
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("HitTwoTimes",
			score, move, user, target, ai, battle);
		// Score for poisoning
		poison_score = Battle.AI.Handlers.apply_move_effect_against_target_score("PoisonTarget",
			0, move, user, target, ai, battle);
		if (poison_score != Battle.AI.MOVE_USELESS_SCORE) score += poison_score;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("HitTwoTimes",
																				"HitTwoTimesFlinchTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitTwoTimesFlinchTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for hitting multiple times
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("HitTwoTimes",
			score, move, user, target, ai, battle);
		// Score for flinching
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("FlinchTarget",
			score, move, user, target, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("HitTwoTimesTargetThenTargetAlly",
	block: (power, move, user, target, ai, battle) => {
		next power * 2;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("HitThreeTimesPowersUpWithEachHit",
	block: (power, move, user, target, ai, battle) => {
		next power * 6;   // Hits do x1, x2, x3 ret in turn, for x6 in total
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitThreeTimesPowersUpWithEachHit",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if the target has a Substitute and this move can break it before
		// the last hit
		if (target.effects.Substitute > 0 && !move.move.ignoresSubstitute(user.battler)) {
			dmg = move.rough_damage;
			if (target.effects.Substitute < dmg / 2) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("HitTwoTimes",
																				"HitThreeTimesAlwaysCriticalHit");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("HitTwoTimes",
																												"HitThreeTimesAlwaysCriticalHit");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("HitTwoToFiveTimes",
	block: (power, move, user, target, ai, battle) => {
		if (user.has_active_ability(abilitys.SKILLLINK)) next power * 5;
		next power * 31 / 10;   // Average damage dealt
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitTwoToFiveTimes",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if the target has a Substitute and this move can break it before
		// the last/third hit
		if (target.effects.Substitute > 0 && !move.move.ignoresSubstitute(user.battler)) {
			dmg = move.rough_damage;
			num_hits = (user.has_active_ability(abilitys.SKILLLINK)) ? 5 : 3;   // 3 is about average
			if (target.effects.Substitute < dmg * (num_hits - 1) / num_hits) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("HitTwoToFiveTimesOrThreeForAshGreninja",
	block: (power, move, user, target, ai, battle) => {
		if (user.battler.isSpecies(Speciess.GRENINJA) && user.battler.form == 2) {
			next move.move.BaseDamage(power, user.battler, target.battler) * move.move.NumHits(user.battler, [target.battler]);
		}
		if (user.has_active_ability(abilitys.SKILLLINK)) next power * 5;
		next power * 31 / 10;   // Average damage dealt
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("HitTwoToFiveTimes",
																												"HitTwoToFiveTimesOrThreeForAshGreninja");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("HitTwoToFiveTimes",
																				"HitTwoToFiveTimesRaiseUserSpd1LowerUserDef1");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitTwoToFiveTimesRaiseUserSpd1LowerUserDef1",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a multi-hit attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("HitTwoToFiveTimes",
			score, move, user, target, ai, battle);
		// Score for user's stat changes
		score = ai.get_score_for_target_stat_raise(score, user, new {:SPEED, 1}, false);
		score = ai.get_score_for_target_stat_drop(score, user, new {:DEFENSE, 1}, false);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("HitOncePerUserTeamMember",
	block: (move, user, ai, battle) => {
		will_fail = true;
		battle.eachInTeamFromBattlerIndex(user.index) do |pkmn, i|
			if (!pkmn.able() || pkmn.status != statuses.NONE) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveBasePower.add("HitOncePerUserTeamMember",
	block: (power, move, user, target, ai, battle) => {
		ret = 0;
		battle.eachInTeamFromBattlerIndex(user.index) do |pkmn, _i|
			if (pkmn.able() && pkmn.status == statuses.NONE) ret += 5 + (pkmn.baseStats[:ATTACK] / 10);
		}
		next ret;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HitOncePerUserTeamMember",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if the target has a Substitute and this move can break it before
		// the last hit
		if (target.effects.Substitute > 0 && !move.move.ignoresSubstitute(user.battler)) {
			dmg = move.rough_damage;
			num_hits = 0;
			battle.eachInTeamFromBattlerIndex(user.index) do |pkmn, _i|
				if (pkmn.able() && pkmn.status == statuses.NONE) num_hits += 1;
			}
			if (target.effects.Substitute < dmg * (num_hits - 1) / num_hits) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("AttackAndSkipNextTurn",
	block: (score, move, user, ai, battle) => {
		// Don't prefer if user is at a high HP (treat this move as a last resort)
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp / 2) score -= 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttack",
	block: (score, move, user, target, ai, battle) => {
		// Power Herb makes this a 1 turn move, the same as a move with no effect
		if (user.has_active_item(items.POWERHERB)) next score;
		// Treat as a failure if user has Truant (the charging turn has no effect)
		if (user.has_active_ability(abilitys.TRUANT)) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if user will faint from EoR damage before finishing this attack
		if (user.rough_end_of_round_damage >= user.hp) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer because it uses up two turns
		score -= 10;
		// Don't prefer if user is at a low HP (time is better spent on quicker moves)
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		// Don't prefer if target has a protecting move
		if (ai.trainer.high_skill() && !(user.has_active_ability(abilitys.UNSEENFIST) && move.move.contactMove())) {
			has_protect_move = false;
			if (move.Target(user).num_targets > 1 &&
				(Settings.MECHANICS_GENERATION >= 7 || move.damagingMove())) {
				if (target.has_move_with_function("ProtectUserSideFromMultiTargetDamagingMoves")) {
					has_protect_move = true;
				}
			}
			if (move.move.canProtectAgainst()) {
				if ((target.has_move_with_function("ProtectUser",
																					"ProtectUserFromTargetingMovesSpikyShield",
																					"ProtectUserBanefulBunker")) {
					has_protect_move = true;
				}
				if (move.damagingMove()) {
					// NOTE: Doesn't check for Mat Block because it only works on its
					//       user's first turn in battle, so it can't be used in response
					//       to this move charging up.
					if ((target.has_move_with_function("ProtectUserFromDamagingMovesKingsShield",
																						"ProtectUserFromDamagingMovesObstruct",
																						"ProtectUserFromDamagingMovesSilkTrap",
																						"ProtectUserFromDamagingMovesBurningBulwark")) {
						has_protect_move = true;
					}
				}
				if (move.rough_priority(user) > 0) {
					if (target.has_move_with_function("ProtectUserSideFromPriorityMoves")) {
						has_protect_move = true;
					}
				}
			}
			if (has_protect_move) score -= 20;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("TwoTurnAttackOneTurnInSun",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamageMultiplier(power, user.battler, target.battler);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackOneTurnInSun",
	block: (score, move, user, target, ai, battle) => {
		// In sunny weather this a 1 turn move, the same as a move with no effect
		if (new []{:Sun, :HarshSun}.Contains(user.battler.effectiveWeather)) next score;
		// Score for being a two turn attack
		next Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackParalyzeTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for paralysing
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("ParalyzeTarget",
			score, move, user, target, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackBurnTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for burning
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("BurnTarget",
			score, move, user, target, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackFlinchTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for flinching
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("FlinchTarget",
			score, move, user, target, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("RaiseUserAtkDef1",
																						"TwoTurnAttackRaiseUserSpAtkSpDefSpd2");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackRaiseUserSpAtkSpDefSpd2",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for raising user's stats
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackChargeRaiseUserDefense1",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for raising the user's stat
		score = Battle.AI.Handlers.apply_move_effect_score("RaiseUserDefense1",
			score, move, user, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackChargeRaiseUserSpAtk1",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for raising the user's stat
		score = Battle.AI.Handlers.apply_move_effect_score("RaiseUserSpAtk1",
			score, move, user, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackInvulnerableUnderground",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for being semi-invulnerable underground
		ai.each_foe_battler(user.side) do |b, i|
			if (b.check_for_move(m => m.hitsDiggingTargets())) {
				score -= 10;
			} else {
				score += 8;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackInvulnerableUnderwater",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for being semi-invulnerable underwater
		ai.each_foe_battler(user.side) do |b, i|
			if (b.check_for_move(m => m.hitsDivingTargets())) {
				score -= 10;
			} else {
				score += 8;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackInvulnerableInSky",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for being semi-invulnerable in the sky
		ai.each_foe_battler(user.side) do |b, i|
			if (b.check_for_move(m => m.hitsFlyingTargets())) {
				score -= 10;
			} else {
				score += 8;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackInvulnerableInSkyParalyzeTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack and semi-invulnerable in the sky
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttackInvulnerableInSky",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for paralyzing the target
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("ParalyzeTarget",
			score, move, user, target, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TwoTurnAttackInvulnerableInSkyTargetCannotAct",
	block: (move, user, target, ai, battle) => {
		if (!target.opposes(user)) next true;
		if (target.effects.Substitute > 0 && !move.move.ignoresSubstitute(user.battler)) next true;
		if (target.has_type(types.FLYING)) next true;
		if (Settings.MECHANICS_GENERATION >= 6 && target.battler.Weight >= 2000) next true;   // 200.0kg
		if (target.battler.semiInvulnerable() || target.effects.SkyDrop >= 0) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("TwoTurnAttackInvulnerableInSky",
																												"TwoTurnAttackInvulnerableInSkyTargetCannotAct");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TwoTurnAttackInvulnerableRemoveProtections",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a two turn attack
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("TwoTurnAttack",
			score, move, user, target, ai, battle);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		// Score for being invulnerable
		score += 8;
		// Score for removing protections
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("RemoveProtections",
			score, move, user, target, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// MultiTurnAttackPreventSleeping

//===============================================================================
//
//===============================================================================
// MultiTurnAttackConfuseUserAtEnd

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("MultiTurnAttackPowersUpEachTurn",
	block: (power, move, user, target, ai, battle) => {
		// NOTE: The * 2 (roughly) incorporates the higher damage done in subsequent
		//       rounds. It is nearly the average damage this move will do per round,
		//       assuming it hits for 3 rounds (hoping for hits in all 5 rounds is
		//       optimistic).
		next move.move.BaseDamage(power, user.battler, target.battler) * 2;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("MultiTurnAttackBideThenReturnDoubleDamage",
	block: (power, move, user, target, ai, battle) => {
		next 40;   // Representative value
	}
)
Battle.AI.Handlers.MoveEffectScore.add("MultiTurnAttackBideThenReturnDoubleDamage",
	block: (score, move, user, ai, battle) => {
		// Useless if no foe has any damaging moves
		has_damaging_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (b.status == statuses.SLEEP && b.statusCount > 2) continue;
			if (b.status == statuses.FROZEN) continue;
			if (b.check_for_move(m => m.damagingMove())) has_damaging_move = true;
			if (has_damaging_move) break;
		}
		if (!has_damaging_move) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer if the user isn't at high HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp / 4) next Battle.AI.MOVE_USELESS_SCORE;
			if (user.hp <= user.totalhp / 2) score -= 15;
			if (user.hp <= user.totalhp * 3 / 4) score -= 8;
		}
		next score;
	}
)
