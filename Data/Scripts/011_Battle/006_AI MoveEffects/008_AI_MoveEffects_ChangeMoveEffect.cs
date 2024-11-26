//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("RedirectAllMovesToUser",
	block: (score, move, user, ai, battle) => {
		// Useless if there is no ally to redirect attacks from
		if (user.battler.allAllies.length == 0) next Battle.AI.MOVE_USELESS_SCORE;
		// Prefer if ally is at low HP and user is at high HP
		if (ai.trainer.has_skill_flag("HPAware") && user.hp > user.totalhp * 2 / 3) {
			ai.each_ally(user.index) do |b, i|
				if (b.hp <= b.totalhp / 3) score += 10;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RedirectAllMovesToTarget",
	block: (score, move, user, target, ai, battle) => {
		if (target.opposes(user)) {
			// Useless if target is a foe but there is only one foe
			if (target.battler.allAllies.length == 0) next Battle.AI.MOVE_USELESS_SCORE;
			// Useless if there is no ally to attack the spotlighted foe
			if (user.battler.allAllies.length == 0) next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Generaly don't prefer this move, as it's a waste of the user's turn
		next score - 20;
	}
)

//===============================================================================
//
//===============================================================================
// CannotBeRedirected

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("RandomlyDamageOrHealTarget",
	block: (power, move, user, target, ai, battle) => {
		next 50;   // Average power, ish
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RandomlyDamageOrHealTarget",
	block: (score, move, user, ai, battle) => {
		// Generaly don't prefer this move, as it may heal the target instead
		next score - 10;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("HealAllyOrDamageFoe",
	block: (move, user, target, ai, battle) => {
		next !target.opposes(user) && !target.battler.canHeal();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("HealAllyOrDamageFoe",
	block: (score, move, user, target, ai, battle) => {
		if (target.opposes(user)) next score;
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp >= target.totalhp * 0.5) {
				score -= 10;
			} else {
				score += 20 * (target.totalhp - target.hp) / target.totalhp;   // +10 to +20
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("CurseTargetOrLowerUserSpd1RaiseUserAtkDef1",
	block: (move, user, ai, battle) => {
		if (user.has_type(types.GHOST) ||
									(move.rough_type == types.GHOST && user.has_active_ability(new {:LIBERO, :PROTEAN}))) next false;
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
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("CurseTargetOrLowerUserSpd1RaiseUserAtkDef1",
	block: (move, user, target, ai, battle) => {
		if (!user.has_type(types.GHOST) &&
									!(move.rough_type == types.GHOST && user.has_active_ability(new {:LIBERO, :PROTEAN}))) next false;
		if (target.effects.Curse || !target.battler.takesIndirectDamage()) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("CurseTargetOrLowerUserSpd1RaiseUserAtkDef1",
	block: (score, move, user, ai, battle) => {
		if (user.has_type(types.GHOST) ||
									(move.rough_type == types.GHOST && user.has_active_ability(new {:LIBERO, :PROTEAN}))) next score;
		score = ai.get_score_for_target_stat_raise(score, user, move.move.statUp);
		if (score == Battle.AI.MOVE_USELESS_SCORE) next score;
		next ai.get_score_for_target_stat_drop(score, user, move.move.statDown, false);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("CurseTargetOrLowerUserSpd1RaiseUserAtkDef1",
	block: (score, move, user, target, ai, battle) => {
		if (!user.has_type(types.GHOST) &&
									!(move.rough_type == types.GHOST && user.has_active_ability(new {:LIBERO, :PROTEAN}))) next score;
		// Don't prefer if user will faint because of using this move
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp <= user.totalhp / 2) next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Prefer early on
		if (user.turnCount < 2) score += 10;
		if (ai.trainer.medium_skill()) {
			// Prefer if the user has no damaging moves
			if (!user.check_for_move(m => m.damagingMove())) score += 15;
			// Prefer if the target can't switch out to remove its curse
			if (!battle.CanChooseNonActive(target.index)) score += 10;
		}
		if (ai.trainer.high_skill()) {
			// Prefer if user can stall while damage is dealt
			if (user.check_for_move(m => m.is_a(Battle.Move.ProtectMove))) {
				score += 5;
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("EffectDependsOnEnvironment",
	block: (score, move, user, target, ai, battle) => {
		// Determine this move's effect
		move.move.OnStartUse(user.battler, [target.battler]);
		function_code = null;
		switch (move.move.secretPower) {
			case 2:
				function_code = "SleepTarget";
				break;
			case 10:
				function_code = "BurnTarget";
				break;
			case 0: case 1:
				function_code = "ParalyzeTarget";
				break;
			case 9:
				function_code = "FreezeTarget";
				break;
			case 7: case 11: case 13:
				function_code = "FlinchTarget";
				break;
			default:
				stat_lowered = null;
				switch (move.move.secretPower) {
					case 5:
						function_code = :ATTACK;
						break;
					case 14:
						function_code = :DEFENSE;
						break;
					case 3:
						function_code = :SPECIAL_ATTACK;
						break;
					case 4: case 6: case 12:
						function_code = :SPEED;
						break;
					case 8:
						function_code = :ACCURACY;
						break;
				}
				if (stat_lowered) next ai.get_score_for_target_stat_drop(score, target, new {stat_lowered, 1});
				break;
		}
		if (function_code) {
			next Battle.AI.Handlers.apply_move_effect_against_target_score(function_code,
				score, move, user, target, ai, battle);
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("HitsAllFoesAndPowersUpInPsychicTerrain",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TargetNextFireMoveDamagesTarget",
	block: (move, user, target, ai, battle) => {
		next target.effects.Powder;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TargetNextFireMoveDamagesTarget",
	block: (score, move, user, target, ai, battle) => {
		// Prefer if target knows any Fire moves (moreso if that's the only type they know)
		if (!target.check_for_move(m => m.CalcType(target.battler) == :FIRE)) next Battle.AI.MOVE_USELESS_SCORE;
		score += 10;
		if (!target.check_for_move(m => m.CalcType(target.battler) != :FIRE)) score += 10;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("DoublePowerAfterFusionFlare",
	block: (score, move, user, ai, battle) => {
		// Prefer if an ally knows Fusion Flare
		ai.each_ally(user.index) do |b, i|
			if (b.has_move_with_function("DoublePowerAfterFusionBolt")) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("DoublePowerAfterFusionBolt",
	block: (score, move, user, ai, battle) => {
		// Prefer if an ally knows Fusion Bolt
		ai.each_ally(user.index) do |b, i|
			if (b.has_move_with_function("DoublePowerAfterFusionFlare")) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("PowerUpAllyMove",
	block: (move, user, target, ai, battle) => {
		next target.effects.HelpingHand;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("PowerUpAllyMove",
	block: (score, move, user, target, ai, battle) => {
		if (!target.check_for_move(m => m.damagingMove())) next Battle.AI.MOVE_USELESS_SCORE;
		next score + 5;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("CounterPhysicalDamage",
	block: (power, move, user, target, ai, battle) => {
		next 60;   // Representative value
	}
)
Battle.AI.Handlers.MoveEffectScore.add("CounterPhysicalDamage",
	block: (score, move, user, ai, battle) => {
		has_physical_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move { |m| m.physicalMove(m.type) &&
																			(user.effects.Substitute == 0 ||
																			m.ignoresSubstitute(b.battler)) };
			has_physical_move = true) continue;
			// Prefer if foe has a higher Attack than Special Attack
			if (b.rough_stat(:ATTACK) > b.rough_stat(:SPECIAL_ATTACK)) score += 5;
			// Prefer if the last move the foe used was physical
			if (ai.trainer.medium_skill() && b.battler.lastMoveUsed) {
				if (GameData.Move.try_get(b.battler.lastMoveUsed)&.physical()) score += 8;
			}
			// Prefer if the foe is taunted into using a damaging move
			if (b.effects.Taunt > 0) score += 5;
		}
		// Useless if no foes have a physical move to counter
		if (!has_physical_move) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("CounterSpecialDamage",
	block: (power, move, user, target, ai, battle) => {
		next 60;   // Representative value
	}
)
Battle.AI.Handlers.MoveEffectScore.add("CounterSpecialDamage",
	block: (score, move, user, ai, battle) => {
		has_special_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move { |m| m.specialMove(m.type) &&
																			(user.effects.Substitute == 0 ||
																			m.ignoresSubstitute(b.battler)) };
			has_special_move = true) continue;
			// Prefer if foe has a higher Special Attack than Attack
			if (b.rough_stat(:SPECIAL_ATTACK) > b.rough_stat(:ATTACK)) score += 5;
			// Prefer if the last move the foe used was special
			if (ai.trainer.medium_skill() && b.battler.lastMoveUsed) {
				if (GameData.Move.try_get(b.battler.lastMoveUsed)&.special()) score += 8;
			}
			// Prefer if the foe is taunted into using a damaging move
			if (b.effects.Taunt > 0) score += 5;
		}
		// Useless if no foes have a special move to counter
		if (!has_special_move) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("CounterDamagePlusHalf",
	block: (power, move, user, target, ai, battle) => {
		next 60;   // Representative value
	}
)
Battle.AI.Handlers.MoveEffectScore.add("CounterDamagePlusHalf",
	block: (score, move, user, ai, battle) => {
		has_damaging_move = false;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack() || user.faster_than(b)) continue;
			if (!b.check_for_move { |m| m.damagingMove() &&
																			(user.effects.Substitute == 0 ||
																			m.ignoresSubstitute(b.battler)) };
			has_damaging_move = true) continue;
			// Prefer if the last move the foe used was damaging
			if (ai.trainer.medium_skill() && b.battler.lastMoveUsed) {
				if (GameData.Move.try_get(b.battler.lastMoveUsed)&.damaging()) score += 8;
			}
			// Prefer if the foe is taunted into using a damaging move
			if (b.effects.Taunt > 0) score += 5;
		}
		// Useless if no foes have a damaging move to counter
		if (!has_damaging_move) next Battle.AI.MOVE_USELESS_SCORE;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserAddStockpileRaiseDefSpDef1",
	block: (move, user, ai, battle) => {
		next user.effects.Stockpile >= 3;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("UserAddStockpileRaiseDefSpDef1",
	block: (score, move, user, ai, battle) => {
		score = ai.get_score_for_target_stat_raise(score, user, new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1}, false);
		// More preferable if user also has Spit Up/Swallow
		if ((user.battler.HasMoveFunction("PowerDependsOnUserStockpile",
																			"HealUserDependingOnUserStockpile")) {
			score += new {10, 10, 8, 5}[user.effects.Stockpile];
		}
		next score;
	}
)

//===============================================================================
// NOTE: Don't worry about the stat drops caused by losing the stockpile, because
//       if (these moves are known, they want to be used.) {
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("PowerDependsOnUserStockpile",
	block: (move, user, ai, battle) => {
		next user.effects.Stockpile == 0;
	}
)
Battle.AI.Handlers.MoveBasePower.add("PowerDependsOnUserStockpile",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("PowerDependsOnUserStockpile",
	block: (score, move, user, ai, battle) => {
		// Slightly prefer to hold out for another Stockpile to make this move stronger
		if (user.effects.Stockpile < 2) score -= 5;
		next score;
	}
)

//===============================================================================
// NOTE: Don't worry about the stat drops caused by losing the stockpile, because
//       if (these moves are known, they want to be used.) {
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("HealUserDependingOnUserStockpile",
	block: (move, user, ai, battle) => {
		if (user.effects.Stockpile == 0) next true;
		if (!user.battler.canHeal() &&
								user.effects.StockpileDef == 0 &&
								user.effects.StockpileSpDef == 0) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("HealUserDependingOnUserStockpile",
	block: (score, move, user, ai, battle) => {
		if (!user.battler.canHeal()) next Battle.AI.MOVE_USELESS_SCORE;
		// Consider how much HP will be restored
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp >= user.totalhp * 0.5) next score - 10;
			score += 20 * (user.totalhp - user.hp) / user.totalhp;   // +10 to +20
		}
		// Slightly prefer to hold out for another Stockpile to make this move stronger
		if (user.effects.Stockpile < 2) score -= 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("GrassPledge",
	block: (score, move, user, ai, battle) => {
		// Prefer if an ally knows a different Pledge move
		ai.each_ally(user.index) do |b, i|
			if (b.has_move_with_function("FirePledge", "WaterPledge")) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("FirePledge",
	block: (score, move, user, ai, battle) => {
		// Prefer if an ally knows a different Pledge move
		ai.each_ally(user.index) do |b, i|
			if (b.has_move_with_function("GrassPledge", "WaterPledge")) score += 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("WaterPledge",
	block: (score, move, user, ai, battle) => {
		// Prefer if an ally knows a different Pledge move
		ai.each_ally(user.index) do |b, i|
			if (b.has_move_with_function("GrassPledge", "FirePledge")) score += 10;
		}
		next score;
	}
)

//===============================================================================
// NOTE: The move that this move will become is determined in def
//       set_up_move_check, and the score for that move is calculated instead. If
//       this move cannot become another move and will fail, the score for this
//       move is calculated as normal (and the code below says it fails).
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UseLastMoveUsed",
	block: (move, user, ai, battle) => {
		if (!battle.lastMoveUsed || !GameData.Move.exists(battle.lastMoveUsed)) next true;
		next move.move.moveBlacklist.Contains(GameData.Move.get(battle.lastMoveUsed).function_code);
	}
)

//===============================================================================
// NOTE: The move that this move will become is determined in def
//       set_up_move_check, and the score for that move is calculated instead. If
//       this move cannot become another move and will fail, the score for this
//       move is calculated as normal (and the code below says it fails).
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("UseLastMoveUsedByTarget",
	block: (move, user, target, ai, battle) => {
		if (!target.battler.lastRegularMoveUsed) next true;
		if (!GameData.Move.exists(target.battler.lastRegularMoveUsed)) next true;
		next !GameData.Move.get(target.battler.lastRegularMoveUsed).has_flag("CanMirrorMove");
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("UseMoveTargetIsAboutToUse",
	block: (move, user, target, ai, battle) => {
		next !target.check_for_move(m => m.damagingMove() && !move.move.moveBlacklist.Contains(m.function_code));
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UseMoveTargetIsAboutToUse",
	block: (score, move, user, target, ai, battle) => {
		if (target.faster_than(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer if target knows any moves that can't be copied
		if (target.check_for_move(m => m.statusMove() || move.move.moveBlacklist.Contains(m.function_code))) {
			score -= 8;
		}
		next score;
	}
)

//===============================================================================
// NOTE: The move that this move will become is determined in def
//       set_up_move_check, and the score for that move is calculated instead.
//===============================================================================
// UseMoveDependingOnEnvironment

//===============================================================================
//
//===============================================================================
// UseRandomMove

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UseRandomMoveFromUserParty",
	block: (move, user, ai, battle) => {
		will_fail = true;
		battle.Party(user.index).each_with_index do |pkmn, i|
			if (!pkmn || i == user.party_index) continue;
			if (Settings.MECHANICS_GENERATION >= 6 && pkmn.egg()) continue;
			foreach (var pkmn_move in pkmn.moves) { //'pkmn.moves.each' do => |pkmn_move|
				if (move.move.moveBlacklist.Contains(pkmn_move.function_code)) continue;
				if (pkmn_move.type == types.SHADOW) continue;
				will_fail = false;
				break;
			}
			if (!will_fail) break;
		}
		next will_fail;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UseRandomUserMoveIfAsleep",
	block: (move, user, ai, battle) => {
		will_fail = true;
		user.battler.eachMoveWithIndex do |m, i|
			if (move.move.moveBlacklist.Contains(m.function_code)) continue;
			if (!battle.CanChooseMove(user.index, i, false, true)) continue;
			will_fail = false;
			break;
		}
		next will_fail;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("BounceBackProblemCausingStatusMoves",
	block: (score, move, user, ai, battle) => {
		if (user.has_active_ability(abilitys.MAGICBOUNCE)) next Battle.AI.MOVE_USELESS_SCORE;
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.statusMove() && m.canMagicCoat())) continue;
			score += 5;
			useless = false;
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer the lower the user's HP is (better to try something else)
		if (ai.trainer.has_skill_flag("HPAware") && user.hp < user.totalhp / 2) {
			score -= (20 * (1.0 - (user.hp.to_f / user.totalhp))).ToInt();   // -10 to -20
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("StealAndUseBeneficialStatusMove",
	block: (score, move, user, ai, battle) => {
		useless = true;
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.can_attack()) continue;
			if (!b.check_for_move(m => m.statusMove() && m.canSnatch())) continue;
			score += 5;
			useless = false;
		}
		if (useless) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't prefer the lower the user's HP is (better to try something else)
		if (ai.trainer.has_skill_flag("HPAware") && user.hp < user.totalhp / 2) {
			score -= (20 * (1.0 - (user.hp.to_f / user.totalhp))).ToInt();   // -10 to -20
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ReplaceMoveThisBattleWithTargetLastMoveUsed",
	block: (move, user, ai, battle) => {
		next user.effects.Transform || !user.battler.HasMove(move.id);
	}
)
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("ReplaceMoveThisBattleWithTargetLastMoveUsed",
	block: (move, user, target, ai, battle) => {
		if (!user.faster_than(target)) next false;
		last_move_data = GameData.Move.try_get(target.battler.lastRegularMoveUsed);
		if (!last_move_data ||
								user.battler.HasMove(target.battler.lastRegularMoveUsed) ||
								move.move.moveBlacklist.Contains(last_move_data.function_code) ||
								last_move_data.type == types.SHADOW) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ReplaceMoveThisBattleWithTargetLastMoveUsed",
	block: (score, move, user, target, ai, battle) => {
		// Generally don't prefer, as this wastes the user's turn just to gain a move
		// of unknown utility
		score -= 10;
		// Slightly prefer if this move will definitely succeed, just for the sake of
		// getting rid of this move
		if (user.faster_than(target)) score += 5;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.copy("ReplaceMoveThisBattleWithTargetLastMoveUsed",
																												"ReplaceMoveWithTargetLastMoveUsed");
Battle.AI.Handlers.MoveEffectScore.copy("ReplaceMoveThisBattleWithTargetLastMoveUsed",
																					"ReplaceMoveWithTargetLastMoveUsed");
