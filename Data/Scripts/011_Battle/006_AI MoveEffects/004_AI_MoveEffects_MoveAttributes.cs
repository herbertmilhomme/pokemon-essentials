//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("FixedDamage20",
	block: (power, move, user, target, ai, battle) => {
		next move.move.FixedDamage(user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("FixedDamage40",
	block: (power, move, user, target, ai, battle) => {
		next move.move.FixedDamage(user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("FixedDamageHalfTargetHP",
	block: (power, move, user, target, ai, battle) => {
		next move.move.FixedDamage(user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("FixedDamageUserLevel",
	block: (power, move, user, target, ai, battle) => {
		next move.move.FixedDamage(user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("FixedDamageUserLevelRandom",
	block: (power, move, user, target, ai, battle) => {
		next user.level;   // Average power
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("LowerTargetHPToUserHP",
	block: (move, user, target, ai, battle) => {
		next user.hp >= target.hp;
	}
)
Battle.AI.Handlers.MoveBasePower.add("LowerTargetHPToUserHP",
	block: (power, move, user, target, ai, battle) => {
		next move.move.FixedDamage(user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("OHKO",
	block: (move, user, target, ai, battle) => {
		if (target.level > user.level) next true;
		if (target.has_active_ability(abilitys.STURDY) && !target.being_mold_broken()) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveBasePower.add("OHKO",
	block: (power, move, user, target, ai, battle) => {
		next target.hp;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("OHKO",
	block: (score, move, user, target, ai, battle) => {
		// Don't prefer if the target has less HP and user has a non-OHKO damaging move
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.check_for_move(m => m.damagingMove() && !m.is_a(Battle.Move.OHKO))) {
				if (target.hp <= target.totalhp / 2) score -= 12;
				if (target.hp <= target.totalhp / 4) score -= 8;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("OHKOIce",
	block: (move, user, target, ai, battle) => {
		if (target.has_type(types.ICE)) next true;
		next Battle.AI.Handlers.move_will_fail_against_target("OHKO", move, user, target, ai, battle);
	}
)
Battle.AI.Handlers.MoveBasePower.copy("OHKO",
																				"OHKOIce");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("OHKO",
																												"OHKOIce");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("OHKO",
																												"OHKOHitsUndergroundTarget");
Battle.AI.Handlers.MoveBasePower.copy("OHKO",
																				"OHKOHitsUndergroundTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("OHKO",
																												"OHKOHitsUndergroundTarget");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DamageTargetAlly",
	block: (score, move, user, target, ai, battle) => {
		foreach (var b in target.battler.allAllies) { //'target.battler.allAllies.each' do => |b|
			if (!b.near(target.battler) || !b.takesIndirectDamage()) continue;
			score += 10;
			if (ai.trainer.has_skill_flag("HPAware")) {
				if (b.hp <= b.totalhp / 16) score += 10;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("PowerHigherWithUserHP",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerLowerWithUserHP");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerHigherWithTargetHP100",
																				"PowerHigherWithTargetHP120");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerHigherWithUserHappiness");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerLowerWithUserHappiness");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerHigherWithUserPositiveStatStages");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerHigherWithTargetPositiveStatStages");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerHigherWithUserFasterThanTarget");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithUserHP",
																				"PowerHigherWithTargetFasterThanUser");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("PowerHigherWithLessPP",
	block: (power, move, user, target, ai, battle) => {
		if (move.move.pp == 0 && move.move.totalpp > 0) next 0;
		dmgs = new {200, 80, 60, 50, 40};
		ppLeft = (int)Math.Min(move.move.pp - 1, dmgs.length - 1);
		next dmgs[ppLeft];
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("PowerHigherWithTargetWeight",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("PowerHigherWithTargetWeight",
																				"PowerHigherWithUserHeavierThanTarget");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("PowerHigherWithConsecutiveUse",
	block: (power, move, user, target, ai, battle) => {
		next power << user.effects.FuryCutter;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("PowerHigherWithConsecutiveUse",
	block: (score, move, user, ai, battle) => {
		// Prefer continuing to use this move
		if (user.effects.FuryCutter > 0) score += 10;
		// Prefer if holding the Metronome
		if (user.has_active_item(items.METRONOME)) score += 7;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("PowerHigherWithConsecutiveUseOnUserSide",
	block: (power, move, user, target, ai, battle) => {
		next power * (user.OwnSide.effects.EchoedVoiceCounter + 1);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("PowerHigherWithConsecutiveUse",
	block: (score, move, user, ai, battle) => {
		// Prefer continuing to use this move
		if (user.OwnSide.effects.EchoedVoiceCounter > 0) score += 10;
		// Prefer if holding the Metronome
		if (user.has_active_item(items.METRONOME)) score += 7;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("RandomPowerDoublePowerIfTargetUnderground",
	block: (power, move, user, target, ai, battle) => {
		power = 71;   // Average damage
		next move.move.ModifyDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("DoublePowerIfTargetHPLessThanHalf",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetHPLessThanHalf",
																				"DoublePowerIfUserPoisonedBurnedParalyzed");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetHPLessThanHalf",
																				"DoublePowerIfTargetAsleepCureTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DoublePowerIfTargetAsleepCureTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.status == statuses.SLEEP && target.statusCount > 1) {   // Will cure status
			if (target.wants_status_problem(:SLEEP)) {
				score += 15;
			} else {
				score -= 10;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("DoublePowerIfTargetPoisoned",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetPoisoned",
																				"DoublePowerIfTargetParalyzedCureTarget");
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DoublePowerIfTargetParalyzedCureTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.status == statuses.PARALYSIS) {   // Will cure status
			if (target.wants_status_problem(:PARALYSIS)) {
				score += 15;
			} else {
				score -= 10;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("DoublePowerIfTargetStatusProblem",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("DoublePowerIfUserHasNoItem",
	block: (power, move, user, target, ai, battle) => {
		if (!user.item || user.has_active_item(items.FLYINGGEM)) power *= 2;
		next power;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("DoublePowerIfTargetUnderwater",
	block: (power, move, user, target, ai, battle) => {
		next move.move.ModifyDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetUnderwater",
																				"DoublePowerIfTargetUnderground");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("DoublePowerIfTargetInSky",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetInSky",
																				"DoublePowerInElectricTerrain");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetInSky",
																				"DoublePowerIfUserLastMoveFailed");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("DoublePowerIfTargetInSky",
																				"DoublePowerIfAllyFaintedLastTurn");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("DoublePowerIfUserLostHPThisTurn",
	block: (score, move, user, ai, battle) => {
		// Prefer if user is slower than its foe(s) and the foe(s) can attack
		ai.each_foe_battler(user.side) do |b, i|
			if (user.faster_than(b) || !b.can_attack()) continue;
			score += 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DoublePowerIfTargetLostHPThisTurn",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if a user's ally is faster than the user and that ally can attack
		ai.each_foe_battler(target.side) do |b, i|
			if (i == user.index) continue;
			if (user.faster_than(b) || !b.can_attack()) continue;
			score += 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// DoublePowerIfUserStatsLoweredThisTurn

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DoublePowerIfTargetActed",
	block: (score, move, user, target, ai, battle) => {
		if (target.faster_than(user)) score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DoublePowerIfTargetNotActed",
	block: (score, move, user, target, ai, battle) => {
		if (user.faster_than(target)) score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// AlwaysCriticalHit

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("EnsureNextCriticalHit",
	block: (score, move, user, ai, battle) => {
		if (user.effects.LaserFocus > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Useless if the user's critical hit stage ensures critical hits already, or
		// critical hits are impossible (e.g. via Lucky Chant)
		crit_stage = 0;
		if (user.battler.OwnSide.effects.LuckyChant > 0) crit_stage = -1;
		if (crit_stage >= 0 && user.ability_active() && ![:MERCILESS].Contains(user.ability_id)) {
			crit_stage = Battle.AbilityEffects.triggerCriticalCalcFromUser(user.battler.ability,
				user.battler, user.battler, move.move, crit_stage);
		}
		if (crit_stage >= 0 && user.item_active()) {
			crit_stage = Battle.ItemEffects.triggerCriticalCalcFromUser(user.battler.item,
				user.battler, user.battler, move.move, crit_stage);
		}
		if (crit_stage >= 0 && crit_stage < 50) {
			crit_stage += user.effects.FocusEnergy;
			crit_stage = (int)Math.Min(crit_stage, Battle.Move.CRITICAL_HIT_RATIOS.length - 1);
		}
		if (crit_stage < 0 ||
			crit_stage >= Battle.Move.CRITICAL_HIT_RATIOS.length ||
			Battle.Move.CRITICAL_HIT_RATIOS[crit_stage] == 1) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Prefer if user knows a damaging move which won't definitely critical hit
		if (user.check_for_move(m => m.damagingMove() && m.function_code != "AlwaysCriticalHit")) {
			score += 15;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartPreventCriticalHitsAgainstUserSide",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.LuckyChant > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartPreventCriticalHitsAgainstUserSide",
	block: (score, move, user, ai, battle) => {
		// Useless if Pokémon on the user's side are immune to critical hits
		user_side_immune = true;
		ai.each_same_side_battler(user.side) do |b, i|
			crit_stage = 0;
			if (b.ability_active()) {
				crit_stage = Battle.AbilityEffects.triggerCriticalCalcFromTarget(b.battler.ability,
					b.battler, b.battler, move.move, crit_stage);
				if (crit_stage < 0) continue;
			}
			if (b.item_active()) {
				crit_stage = Battle.ItemEffects.triggerCriticalCalcFromTarget(b.battler.item,
					b.battler, b.battler, move.move, crit_stage);
				if (crit_stage < 0) continue;
			}
			user_side_immune = false;
			break;
		}
		if (user_side_immune) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if any foe has an increased critical hit rate or moves/effects that
		// make critical hits more likely
		ai.each_foe_battler(user.side) do |b, i|
			crit_stage = 0;
			if (crit_stage >= 0 && b.ability_active()) {
				crit_stage = Battle.AbilityEffects.triggerCriticalCalcFromUser(b.battler.ability,
					b.battler, user.battler, move.move, crit_stage);
				if (crit_stage < 0) continue;
			}
			if (crit_stage >= 0 && b.item_active()) {
				crit_stage = Battle.ItemEffects.triggerCriticalCalcFromUser(b.battler.item,
					b.battler, user.battler, move.move, crit_stage);
				if (crit_stage < 0) continue;
			}
			if (crit_stage >= 0 && crit_stage < 50) {
				crit_stage += b.effects.FocusEnergy;
				if (b.check_for_move(m => m.highCriticalRate())) crit_stage += 1;
				if (b.check_for_move(m => m.CritialOverride(b.battler, user.battler) > 0)) crit_stage = 99;
				crit_stage = (int)Math.Min(crit_stage, Battle.Move.CRITICAL_HIT_RATIOS.length - 1);
			}
			if (crit_stage > 0) score += 8 * crit_stage;
			if (b.effects.LaserFocus > 0) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("CannotMakeTargetFaint",
	block: (move, user, target, ai, battle) => {
		next target.hp == 1;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("UserEnduresFaintingThisTurn",
	block: (score, move, user, ai, battle) => {
		if (user.rough_end_of_round_damage > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer for each foe that can attack
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			score += 5;
			useless = false;
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer if user has high HP, prefer if user has lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp / 2) {
				score -= 15;
			} else if (user.hp <= user.totalhp / 4) {
				score += 8;
			}
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * 8;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartWeakenElectricMoves",
	block: (move, user, ai, battle) => {
		if (Settings.MECHANICS_GENERATION >= 6) next battle.field.effects.MudSportField > 0;
		next battle.allBattlers.any(b => b.effects.MudSport);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartWeakenElectricMoves",
	block: (score, move, user, ai, battle) => {
		// Don't prefer the lower the user's HP is
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp / 2) {
				score -= (20 * (0.75 - (user.hp.to_f / user.totalhp))).ToInt();   // -5 to -15
			}
		}
		// Prefer if foes have Electric moves
		any_foe_electric_moves = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.has_damaging_move_of_type(types.ELECTRIC)) continue;
			score += 15;
			if (!b.check_for_move(m => m.damagingMove() && m.CalcType(b.battler) != :ELECTRIC)) score += 7;
			any_foe_electric_moves = true;
		}
		if (!any_foe_electric_moves) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer if any allies have Electric moves
		ai.each_same_side_battler(user.side) do |b, i|
			if (!b.has_damaging_move_of_type(types.ELECTRIC)) continue;
			score -= 10;
			if (!b.check_for_move(m => m.damagingMove() && m.CalcType(b.battler) != :ELECTRIC)) score -= 5;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartWeakenFireMoves",
	block: (move, user, ai, battle) => {
		if (Settings.MECHANICS_GENERATION >= 6) next battle.field.effects.WaterSportField > 0;
		next battle.allBattlers.any(b => b.effects.WaterSport);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartWeakenFireMoves",
	block: (score, move, user, ai, battle) => {
		// Don't prefer the lower the user's HP is
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp / 2) {
				score -= (20 * (0.75 - (user.hp.to_f / user.totalhp))).ToInt();   // -5 to -15
			}
		}
		// Prefer if foes have Fire moves
		any_foe_fire_moves = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.has_damaging_move_of_type(types.FIRE)) continue;
			score += 15;
			if (!b.check_for_move(m => m.damagingMove() && m.CalcType(b.battler) != :FIRE)) score += 7;
			any_foe_fire_moves = true;
		}
		if (!any_foe_fire_moves) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer if any allies have Fire moves
		ai.each_same_side_battler(user.side) do |b, i|
			if (!b.has_damaging_move_of_type(types.FIRE)) continue;
			score -= 10;
			if (!b.check_for_move(m => m.damagingMove() && m.CalcType(b.battler) != :FIRE)) score -= 5;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartWeakenPhysicalDamageAgainstUserSide",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.Reflect > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartWeakenPhysicalDamageAgainstUserSide",
	block: (score, move, user, ai, battle) => {
		// Doesn't stack with Aurora Veil
		if (user.OwnSide.effects.AuroraVeil > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user is newly in battle
		if (user.turnCount < 2) score += 15;
		// Don't prefer the lower the user's HP is
		if (ai.trainer.has_skill_flag("HPAware") && battle.AbleNonActiveCount(user.idxOwnSide) == 0) {
			if (user.hp <= user.totalhp / 2) {
				score -= (20 * (0.75 - (user.hp.to_f / user.totalhp))).ToInt();   // -5 to -15
			}
		}
		// Prefer if foes have physical moves (moreso if they don't have special moves)
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.check_for_move(m => m.physicalMove(m.type))) continue;
			score += 10;
			if (!b.check_for_move(m => m.specialMove(m.type))) score += 8;
		}
		// Prefer if user has Light Clay
		if (user.has_active_item(items.LIGHTCLAY)) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartWeakenSpecialDamageAgainstUserSide",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.LightScreen > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartWeakenSpecialDamageAgainstUserSide",
	block: (score, move, user, ai, battle) => {
		// Doesn't stack with Aurora Veil
		if (user.OwnSide.effects.AuroraVeil > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user is newly in battle
		if (user.turnCount < 2) score += 15;
		// Don't prefer the lower the user's HP is
		if (ai.trainer.has_skill_flag("HPAware") && battle.AbleNonActiveCount(user.idxOwnSide) == 0) {
			if (user.hp <= user.totalhp / 2) {
				score -= (20 * (0.75 - (user.hp.to_f / user.totalhp))).ToInt();   // -5 to -15
			}
		}
		// Prefer if foes have special moves (moreso if they don't have physical moves)
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.check_for_move(m => m.specialMove(m.type))) continue;
			score += 10;
			if (!b.check_for_move(m => m.physicalMove(m.type))) score += 8;
		}
		// Prefer if user has Light Clay
		if (user.has_active_item(items.LIGHTCLAY)) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartWeakenDamageAgainstUserSideIfHail",
	block: (move, user, ai, battle) => {
		if (user.OwnSide.effects.AuroraVeil > 0) next true;
		if (!new []{:Hail, :Snowstorm}.Contains(user.battler.effectiveWeather)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartWeakenDamageAgainstUserSideIfHail",
	block: (score, move, user, ai, battle) => {
		// Doesn't stack with Reflect/Light Screen
		if (user.OwnSide.effects.Reflect > 0 &&
																					user.OwnSide.effects.LightScreen > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user is newly in battle
		if (user.turnCount < 2) score += 15;
		// Don't prefer the lower the user's HP is
		if (ai.trainer.has_skill_flag("HPAware") && battle.AbleNonActiveCount(user.idxOwnSide) == 0) {
			if (user.hp <= user.totalhp / 2) {
				score -= (20 * (0.75 - (user.hp.to_f / user.totalhp))).ToInt();   // -5 to -15
			}
		}
		// Prefer if user has Light Clay
		if (user.has_active_item(items.LIGHTCLAY)) score += 5;
		next score + 15;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("RemoveScreens",
	block: (score, move, user, ai, battle) => {
		// Prefer if allies have physical moves that are being weakened
		if (user.OpposingSide.effects.Reflect > 1 ||
			user.OpposingSide.effects.AuroraVeil > 1) {
			ai.each_same_side_battler(user.side) do |b, i|
				if (b.check_for_move(m => m.physicalMove(m.type))) score += 10;
			}
		}
		// Prefer if allies have special moves that are being weakened
		if (user.OpposingSide.effects.LightScreen > 1 ||
			user.OpposingSide.effects.AuroraVeil > 1) {
			ai.each_same_side_battler(user.side) do |b, i|
				if (b.check_for_move(m => m.specialMove(m.type))) score += 10;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("ProtectUser",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.canProtectAgainst())) continue;
			if (b.has_active_ability(abilitys.UNSEENFIST) && b.check_for_move(m => m.contactMove())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if the foe is in the middle of using a two turn attack
			if (b.effects.TwoTurnAttack &&
										GameData.Move.get(b.effects.TwoTurnAttack).flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase))) score += 15;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserBanefulBunker",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.canProtectAgainst())) continue;
			if (b.has_active_ability(abilitys.UNSEENFIST) && b.check_for_move(m => m.contactMove())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if the foe is likely to be poisoned by this move
			if (b.check_for_move(m => m.contactMove())) {
				poison_score = Battle.AI.Handlers.apply_move_effect_against_target_score("PoisonTarget",
					0, move, user, b, ai, battle);
				if (poison_score != Battle.AI.MOVE_USELESS_SCORE) {
					score += poison_score / 2;   // Halved because we don't know what move b will use
				}
			}
			// Prefer if the foe is in the middle of using a two turn attack
			if (b.effects.TwoTurnAttack &&
										GameData.Move.get(b.effects.TwoTurnAttack).flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase))) score += 15;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserFromDamagingMovesKingsShield",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.damagingMove() && m.canProtectAgainst())) continue;
			if (b.has_active_ability(abilitys.UNSEENFIST) && b.check_for_move(m => m.contactMove())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if the foe's Attack can be lowered by this move
			if (b.battler.affectedByContactEffect() && b.check_for_move(m => m.contactMove())) {
				drop_score = ai.get_score_for_target_stat_drop(
					0, b, new {:ATTACK, (Settings.MECHANICS_GENERATION >= 8) ? 1 : 2}, false);
				score += drop_score / 2;   // Halved because we don't know what move b will use
			}
			// Prefer if the foe is in the middle of using a two turn attack
			if (b.effects.TwoTurnAttack &&
										GameData.Move.get(b.effects.TwoTurnAttack).flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase))) score += 15;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		// Aegislash
		if (user.battler.isSpecies(Speciess.AEGISLASH) && user.battler.form == 1 &&
									user.ability == abilitys.STANCECHANGE) score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserFromDamagingMovesObstruct",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.damagingMove() && m.canProtectAgainst())) continue;
			if (b.has_active_ability(abilitys.UNSEENFIST) && b.check_for_move(m => m.contactMove())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if the foe's Attack can be lowered by this move
			if (b.battler.affectedByContactEffect() && b.check_for_move(m => m.contactMove())) {
				drop_score = ai.get_score_for_target_stat_drop(0, b, new {:DEFENSE, 2}, false);
				score += drop_score / 2;   // Halved because we don't know what move b will use
			}
			// Prefer if the foe is in the middle of using a two turn attack
			if (b.effects.TwoTurnAttack &&
										GameData.Move.get(b.effects.TwoTurnAttack).flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase))) score += 15;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserFromTargetingMovesSpikyShield",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.canProtectAgainst())) continue;
			if (b.has_active_ability(abilitys.UNSEENFIST) && b.check_for_move(m => m.contactMove())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if this move will deal damage
			if (b.battler.affectedByContactEffect() && b.check_for_move(m => m.contactMove())) {
				score += 5;
			}
			// Prefer if the foe is in the middle of using a two turn attack
			if (b.effects.TwoTurnAttack &&
										GameData.Move.get(b.effects.TwoTurnAttack).flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase))) score += 15;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ProtectUserSideFromDamagingMovesIfUserFirstTurn",
	block: (move, user, ai, battle) => {
		next user.turnCount > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserSideFromDamagingMovesIfUserFirstTurn",
	block: (score, move, user, ai, battle) => {
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.damagingMove() && m.canProtectAgainst())) continue;
			if (b.has_active_ability(abilitys.UNSEENFIST) && b.check_for_move(m => m.contactMove())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if the foe is in the middle of using a two turn attack
			if (b.effects.TwoTurnAttack &&
										GameData.Move.get(b.effects.TwoTurnAttack).flags.any(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^CanProtect$",RegexOptions.IgnoreCase))) score += 15;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Use it or lose it
		score += 25;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ProtectUserSideFromStatusMoves",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.CraftyShield;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserSideFromStatusMoves",
	block: (score, move, user, ai, battle) => {
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move { |m| m.statusMove() && m.Target(b.battler).targets_foe &&
																			!m.Target(b.battler).targets_all };
			useless = false) continue;
			// General preference
			score += 5;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ProtectUserSideFromPriorityMoves",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.QuickGuard;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserSideFromPriorityMoves",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.Priority(b.battler) > 0 && m.canProtectAgainst())) continue;
			useless = false;
			// General preference
			score += 7;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += 8;
			} else if (b_eor_damage < 0) {
				score -= 8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ProtectUserSideFromMultiTargetDamagingMoves",
	block: (move, user, ai, battle) => {
		next user.OwnSide.effects.WideGuard;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("ProtectUserSideFromMultiTargetDamagingMoves",
	block: (score, move, user, ai, battle) => {
		// Useless if the success chance is 25% or lower
		if (user.effects.ProtectRate >= 4) next Battle.AI.MOVE_USELESS_SCORE;
		// Score changes for each foe
		useless = true;
		ai.each_battler do |b, i|
			if (b.index == user.index || !b.can_attack()) continue;
			if (!b.check_for_move { |m| (Settings.MECHANICS_GENERATION >= 7 || move.damagingMove()) &&
																			m.Target(b.battler).num_targets > 1 };
			useless = false) continue;
			// General preference
			score += 7;
			// Prefer if foe takes EOR damage, don't prefer if they have EOR healing
			b_eor_damage = b.rough_end_of_round_damage;
			if (b_eor_damage > 0) {
				score += (b.opposes(user)) ? 8 : -8;
			} else if (b_eor_damage < 0) {
				score -= (b.opposes(user)) ? 8 : -8;
			}
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user has EOR healing, don't prefer if they take EOR damage
		user_eor_damage = user.rough_end_of_round_damage;
		if (user_eor_damage >= user.hp) {
			next Battle.AI.MOVE_USELESS_SCORE;
		} else if (user_eor_damage > 0) {
			score -= 8;
		} else if (user_eor_damage < 0) {
			score += 8;
		}
		// Don't prefer if the user used a protection move last turn, making this one
		// less likely to work
		score -= (user.effects.ProtectRate - 1) * ((Settings.MECHANICS_GENERATION >= 6) ? 15 : 10);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RemoveProtections",
	block: (score, move, user, target, ai, battle) => {
		if (target.check_for_move { |m| (m.is_a(Battle.Move.ProtectMove) ||
																		m.is_a(Battle.Move.ProtectUserSideFromStatusMoves) ||
																		m.is_a(Battle.Move.ProtectUserSideFromDamagingMovesIfUserFirstTurn)) &&
																	!m.is_a(Battle.Move.UserEnduresFaintingThisTurn) };
			score += 7) {
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("RemoveProtectionsBypassSubstitute",
																												"RemoveProtections");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("HoopaRemoveProtectionsBypassSubstituteLowerUserDef1",
	block: (move, user, ai, battle) => {
		next !user.battler.isSpecies(Speciess.HOOPA) || user.battler.form != 1;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HoopaRemoveProtectionsBypassSubstituteLowerUserDef1",
	block: (score, move, user, target, ai, battle) => {
		score = Battle.AI.Handlers.apply_move_effect_against_target_score("RemoveProtections",
			score, move, user, target, ai, battle);
		next ai.get_score_for_target_stat_drop(score, user, move.move.statDown, false);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RecoilQuarterOfDamageDealt",
	block: (score, move, user, target, ai, battle) => {
		if (!user.battler.takesIndirectDamage() || user.has_active_ability(abilitys.ROCKHEAD)) next score;
		dmg = move.rough_damage / 4;
		if (dmg >= user.hp) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (reserves <= foes) next Battle.AI.MOVE_USELESS_SCORE;
		}
		score -= 25 * (int)Math.Min(dmg, user.hp) / user.hp;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RecoilThirdOfDamageDealtParalyzeTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a recoil move
		if (user.battler.takesIndirectDamage() && !user.has_active_ability(abilitys.ROCKHEAD)) {
			dmg = move.rough_damage / 3;
			if (dmg >= user.hp) {
				reserves = battle.AbleNonActiveCount(user.idxOwnSide);
				foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
				if (reserves <= foes) next Battle.AI.MOVE_USELESS_SCORE;
			}
			score -= 25 * (int)Math.Min(dmg, user.hp) / user.hp;
		}
		// Score for paralysing
		paralyze_score = Battle.AI.Handlers.apply_move_effect_against_target_score("ParalyzeTarget",
			0, move, user, target, ai, battle);
		if (paralyze_score != Battle.AI.MOVE_USELESS_SCORE) score += paralyze_score;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RecoilThirdOfDamageDealtBurnTarget",
	block: (score, move, user, target, ai, battle) => {
		// Score for being a recoil move
		if (user.battler.takesIndirectDamage() && !user.has_active_ability(abilitys.ROCKHEAD)) {
			dmg = move.rough_damage / 3;
			if (dmg >= user.hp) {
				reserves = battle.AbleNonActiveCount(user.idxOwnSide);
				foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
				if (reserves <= foes) next Battle.AI.MOVE_USELESS_SCORE;
			}
			score -= 25 * (int)Math.Min(dmg, user.hp) / user.hp;
		}
		// Score for burning
		burn_score = Battle.AI.Handlers.apply_move_effect_against_target_score("BurnTarget",
			0, move, user, target, ai, battle);
		if (burn_score != Battle.AI.MOVE_USELESS_SCORE) score += burn_score;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RecoilHalfOfDamageDealt",
	block: (score, move, user, target, ai, battle) => {
		if (!user.battler.takesIndirectDamage() || user.has_active_ability(abilitys.ROCKHEAD)) next score;
		dmg = move.rough_damage / 2;
		if (dmg >= user.hp) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			if (reserves <= foes) next Battle.AI.MOVE_USELESS_SCORE;
		}
		score -= 25 * (int)Math.Min(dmg, user.hp) / user.hp;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("EffectivenessIncludesFlyingType",
	block: (power, move, user, target, ai, battle) => {
		if (GameData.Types.exists(Types.FLYING)) {
			targetTypes = target.Types(true);
			mult = Effectiveness.calculate(:FLYING, *targetTypes);
			power = (int)Math.Round(power * mult);
		}
		next power;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.copy("PoisonTarget",
																												"CategoryDependsOnHigherDamagePoisonTarget");

//===============================================================================
//
//===============================================================================
// CategoryDependsOnHigherDamageIgnoreTargetAbility

//===============================================================================
//
//===============================================================================
// UseUserDefenseInsteadOfUserAttack

//===============================================================================
//
//===============================================================================
// UseTargetAttackInsteadOfUserAttack

//===============================================================================
//
//===============================================================================
// UseTargetDefenseInsteadOfTargetSpDef

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("EnsureNextMoveAlwaysHits",
	block: (move, user, ai, battle) => {
		next user.effects.LockOn > 0;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("EnsureNextMoveAlwaysHits",
	block: (score, move, user, target, ai, battle) => {
		if (user.has_active_ability(abilitys.NOGUARD) || target.has_active_ability(abilitys.NOGUARD)) next Battle.AI.MOVE_USELESS_SCORE;
		if (target.effects.Telekinesis > 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if the user knows moves with low accuracy
		foreach (var m in user.battler.Moves) { //user.battler.eachMove do => |m|
			if (target.effects.Minimize && m.tramplesMinimize() && Settings.MECHANICS_GENERATION >= 6) continue;
			acc = m.accuracy;
			if (ai.trainer.medium_skill()) acc = m.BaseAccuracy(user.battler, target.battler);
			if (acc < 90 && acc != 0) score += 5;
			if (acc <= 50 && acc != 0) score += 8;
			if (m.is_a(Battle.Move.OHKO)) score += 8;
		}
		// Prefer if the target has increased evasion
		if (target.stages[:EVASION] > 0) score += 5 * target.stages[:EVASION];
		// Not worth it if the user or the target is at low HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
			if (target.hp < target.totalhp / 2) score -= 8;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("StartNegateTargetEvasionStatStageAndGhostImmunity",
	block: (score, move, user, target, ai, battle) => {
		if (target.effects.Foresight || user.has_active_ability(abilitys.SCRAPPY)) next Battle.AI.MOVE_USELESS_SCORE;
		// Check if the user knows any moves that would benefit from negating the
		// target's Ghost type immunity
		if (target.has_type(types.GHOST)) {
			foreach (var m in user.battler.Moves) { //user.battler.eachMove do => |m|
				if (!m.damagingMove()) continue;
				if (Effectiveness.ineffective_type(m.CalcType(user.battler), :GHOST)) score += 10;
			}
		}
		// Prefer if the target has increased evasion
		if (target.stages[:EVASION] > 0) score += 10 * target.stages[:EVASION];
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("StartNegateTargetEvasionStatStageAndDarkImmunity",
	block: (score, move, user, target, ai, battle) => {
		if (target.effects.MiracleEye) next Battle.AI.MOVE_USELESS_SCORE;
		// Check if the user knows any moves that would benefit from negating the
		// target's Dark type immunity
		if (target.has_type(types.DARK)) {
			foreach (var m in user.battler.Moves) { //user.battler.eachMove do => |m|
				if (!m.damagingMove()) continue;
				if (Effectiveness.ineffective_type(m.CalcType(user.battler), :DARK)) score += 10;
			}
		}
		// Prefer if the target has increased evasion
		if (target.stages[:EVASION] > 0) score += 10 * target.stages[:EVASION];
		next score;
	}
)

//===============================================================================
//
//===============================================================================
// IgnoreTargetDefSpDefEvaStatStages

//===============================================================================
//
//===============================================================================
// TypeIsUserFirstType

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("TypeDependsOnUserIVs",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("TypeAndPowerDependOnUserBerry",
	block: (move, user, ai, battle) => {
		item = user.item;
		if (!item || !item.is_berry() || !user.item_active()) next true;
		if (item.flags.none(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^NaturalGift_",RegexOptions.IgnoreCase))) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveBasePower.add("TypeAndPowerDependOnUserBerry",
	block: (power, move, user, target, ai, battle) => {
		ret = move.move.BaseDamage(1, user.battler, target.battler);
		next (ret == 1) ? 0 : ret;
	}
)

//===============================================================================
//
//===============================================================================
// TypeDependsOnUserPlate

//===============================================================================
//
//===============================================================================
// TypeDependsOnUserMemory

//===============================================================================
//
//===============================================================================
// TypeDependsOnUserDrive

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("TypeDependsOnUserMorpekoFormRaiseUserSpeed1",
	block: (move, user, ai, battle) => {
		next !user.battler.isSpecies(Speciess.MORPEKO) && user.effects.TransformSpecies != :MORPEKO;
	}
)
Battle.AI.Handlers.MoveEffectScore.copy("RaiseUserSpeed1",
																					"TypeDependsOnUserMorpekoFormRaiseUserSpeed1");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("TypeAndPowerDependOnWeather",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.copy("TypeAndPowerDependOnWeather",
																				"TypeAndPowerDependOnTerrain");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TargetMovesBecomeElectric",
	block: (move, user, target, ai, battle) => {
		next !user.faster_than(target);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TargetMovesBecomeElectric",
	block: (score, move, user, target, ai, battle) => {
		// Get Electric's effectiveness against the user
		electric_eff = user.effectiveness_of_type_against_battler(:ELECTRIC, target);
		if (target.has_type(types.ELECTRIC)) electric_eff *= 1.5;   // STAB
		if (user.has_active_ability(new {:LIGHTNINGROD, :MOTORDRIVE, :VOLTABSORB})) electric_eff = 0;
		// For each of target's moves, get its effectiveness against the user and
		// decide whether it is better or worse than Electric's effectiveness
		old_type_better = 0;
		electric_type_better = 0;
		foreach (var m in target.battler.Moves) { //target.battler.eachMove do => |m|
			if (!m.damagingMove()) continue;
			m_type = m.CalcType(target.battler);
			if (m_type == types.ELECTRIC) continue;
			eff = user.effectiveness_of_type_against_battler(m_type, target, m);
			if (target.has_type(m_type)) eff *= 1.5;   // STAB
			switch (m_type) {
				case :FIRE:
					if (user.has_active_ability(abilitys.FLASHFIRE)) eff = 0;
					break;
				case :GRASS:
					if (user.has_active_ability(abilitys.SAPSIPPER)) eff = 0;
					break;
				case :WATER:
					if (user.has_active_ability(new {:STORMDRAIN, :WATERABSORB})) eff = 0;
					break;
			}
			if (eff > electric_eff) {
				electric_type_better += 1;
			} else if (eff < electric_eff) {
				old_type_better += 1;
			}
		}
		if (electric_type_better == 0) next Battle.AI.MOVE_USELESS_SCORE;
		if (electric_type_better < old_type_better) next Battle.AI.MOVE_USELESS_SCORE;
		score += 10 * (electric_type_better - old_type_better);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("NormalMovesBecomeElectric",
	block: (score, move, user, ai, battle) => {
		base_electric_eff = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		if (user.has_active_ability(new {:LIGHTNINGROD, :MOTORDRIVE, :VOLTABSORB})) base_electric_eff = 0;
		// Check all affected foe battlers for Normal moves, get their effectiveness
		// against the user and decide whether it is better or worse than Electric's
		// effectiveness
		normal_type_better = 0;
		electric_type_better = 0;
		ai.each_foe_battler(user.side) do |b, i|
			if (move.rough_priority(b) <= 0 && b.faster_than(user)) continue;
			if (!b.has_damaging_move_of_type(types.NORMAL)) continue;
			// Normal's effectiveness
			eff = user.effectiveness_of_type_against_battler(:NORMAL, b);
			if (b.has_type(types.NORMAL)) eff *= 1.5;   // STAB
			// Electric's effectiveness
			elec_eff = user.effectiveness_of_type_against_battler(:ELECTRIC, b);
			if (b.has_type(types.ELECTRIC)) elec_eff *= 1.5;   // STAB
			elec_eff *= base_electric_eff;
			// Compare the two
			if (eff > elec_eff) {
				electric_type_better += 1;
			} else if (eff < elec_eff) {
				normal_type_better += 1;
			}
		}
		if (electric_type_better == 0 || electric_type_better < normal_type_better) {
			next (move.statusMove()) ? Battle.AI.MOVE_USELESS_SCORE : score;
		}
		score += 10 * (electric_type_better - normal_type_better);
		next score;
	}
)
