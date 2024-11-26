//===============================================================================
//
//===============================================================================
public partial class Battle.Move {
	//-----------------------------------------------------------------------------
	// Move's type calculation.
	//-----------------------------------------------------------------------------

	public void BaseType(user) {
		ret = @type;
		if (ret && user.abilityActive()) {
			ret = Battle.AbilityEffects.triggerModifyMoveBaseType(user.ability, user, self, ret);
		}
		return ret;
	}

	public void CalcType(user) {
		@powerBoost = false;
		ret = BaseType(user);
		if (ret && GameData.Types.exists(Types.ELECTRIC)) {
			if (@battle.field.effects.IonDeluge && ret == :NORMAL) {
				ret = :ELECTRIC;
				@powerBoost = false;
			}
			if (user.effects.Electrify) {
				ret = :ELECTRIC;
				@powerBoost = false;
			}
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Type effectiveness calculation.
	//-----------------------------------------------------------------------------

	public void CalcTypeModSingle(moveType, defType, user, target) {
		ret = Effectiveness.calculate(moveType, defType);
		if (Effectiveness.ineffective_type(moveType, defType)) {
			// Ring Target
			if (target.hasActiveItem(Items.RINGTARGET)) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
			// Foresight
			if ((user.hasActiveAbility(Abilitys.SCRAPPY) || user.hasActiveAbility(Abilitys.MINDSEYE) ||
					target.effects.Foresight) && defType == Types.GHOST) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
			// Miracle Eye
			if (target.effects.MiracleEye && defType == Types.DARK) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
		} else if (Effectiveness.super_effective_type(moveType, defType)) {
			// Delta Stream's weather
			if (target.effectiveWeather == Weathers.StrongWinds && defType == Types.FLYING) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
		}
		// Grounded Flying-type Pokémon become susceptible to Ground moves
		if (!target.airborne() && defType == Types.FLYING && moveType == Types.GROUND) {
			ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		}
		return ret;
	}

	public void CalcTypeMod(moveType, user, target) {
		ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		if (!moveType) return ret;
		if (moveType == Types.GROUND && target.Type == Types.FLYING && target.hasActiveItem(Items.IRONBALL)) return ret;
		// Get effectivenesses
		if (moveType == Types.SHADOW) {
			if (target.shadowPokemon()) {
				ret = Effectiveness.NOT_VERY_EFFECTIVE_MULTIPLIER;
			} else {
				ret = Effectiveness.SUPER_EFFECTIVE_MULTIPLIER;
			}
		} else {
			foreach (var type in target.Types(true)) { //'target.Types(true).each' do => |type|
				ret *= CalcTypeModSingle(moveType, type, user, target);
			}
			if (target.effects.TarShot && moveType == Types.FIRE) ret *= 2;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Accuracy check.
	//-----------------------------------------------------------------------------

	public void BaseAccuracy(user, target) {return @accuracy; }

	// Accuracy calculations for one-hit KO moves are handled elsewhere.
	public void AccuracyCheck(user, target) {
		// "Always hit" effects and "always hit" accuracy
		if (target.effects.Telekinesis > 0) return true;
		if (target.effects.Minimize && tramplesMinimize() && Settings.MECHANICS_GENERATION >= 6) return true;
		baseAcc = BaseAccuracy(user, target);
		if (baseAcc == 0) return true;
		// Calculate all multiplier effects
		modifiers = new List<string>();
		modifiers.base_accuracy  = baseAcc;
		modifiers.accuracy_stage = user.stages[:ACCURACY];
		modifiers.evasion_stage  = target.stages[:EVASION];
		modifiers.accuracy_multiplier = 1.0;
		modifiers.evasion_multiplier  = 1.0;
		CalcAccuracyModifiers(user, target, modifiers);
		// Check if move can't miss
		if (modifiers.base_accuracy == 0) return true;
		// Calculation
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		accStage = (int)Math.Min((int)Math.Max(modifiers.accuracy_stage, -max_stage), max_stage) + max_stage;
		evaStage = (int)Math.Min((int)Math.Max(modifiers.evasion_stage, -max_stage), max_stage) + max_stage;
		stageMul = Battle.Battler.ACC_EVA_STAGE_MULTIPLIERS;
		stageDiv = Battle.Battler.ACC_EVA_STAGE_DIVISORS;
		accuracy = 100.0 * stageMul[accStage] / stageDiv[accStage];
		evasion  = 100.0 * stageMul[evaStage] / stageDiv[evaStage];
		accuracy = (int)Math.Round(accuracy * modifiers.accuracy_multiplier);
		evasion  = (int)Math.Round(evasion  * modifiers.evasion_multiplier);
		if (evasion < 1) evasion = 1;
		threshold = modifiers.base_accuracy * accuracy / evasion;
		// Calculation
		r = @battle.Random(100);
		if (Settings.AFFECTION_EFFECTS && @battle.internalBattle &&
			target.OwnedByPlayer() && target.affection_level == 5 && !target.mega()) {
			if (r < threshold - 10) return true;
			if (r < threshold) target.damageState.affection_missed = true;
			return false;
		}
		return r < threshold;
	}

	public void CalcAccuracyModifiers(user, target, modifiers) {
		// Ability effects that alter accuracy calculation
		if (user.abilityActive()) {
			Battle.AbilityEffects.triggerAccuracyCalcFromUser(
				user.ability, modifiers, user, target, self, @calcType
			);
		}
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (!b.abilityActive()) continue;
			Battle.AbilityEffects.triggerAccuracyCalcFromAlly(
				b.ability, modifiers, user, target, self, @calcType
			);
		}
		if (target.abilityActive() && !target.beingMoldBroken()) {
			Battle.AbilityEffects.triggerAccuracyCalcFromTarget(
				target.ability, modifiers, user, target, self, @calcType
			);
		}
		// Item effects that alter accuracy calculation
		if (user.itemActive()) {
			Battle.ItemEffects.triggerAccuracyCalcFromUser(
				user.item, modifiers, user, target, self, @calcType
			);
		}
		if (target.itemActive()) {
			Battle.ItemEffects.triggerAccuracyCalcFromTarget(
				target.item, modifiers, user, target, self, @calcType
			);
		}
		// Other effects, inc. ones that set accuracy_multiplier or evasion_stage to
		// specific values
		if (@battle.field.effects.Gravity > 0) {
			modifiers.accuracy_multiplier *= 5 / 3.0;
		}
		if (user.effects.MicleBerry) {
			user.effects.MicleBerry = false;
			modifiers.accuracy_multiplier *= 1.2;
		}
		if (target.effects.Foresight && modifiers.evasion_stage > 0) modifiers.evasion_stage = 0;
		if (target.effects.MiracleEye && modifiers.evasion_stage > 0) modifiers.evasion_stage = 0;
		if (user.hasActiveAbility(Abilitys.MINDSEYE)) modifiers.evasion_stage = 0;
	}

	//-----------------------------------------------------------------------------
	// Critical hit check.
	//-----------------------------------------------------------------------------

	// Return values:
	//   -1: Never a critical hit.
	//    0: Calculate normally.
	//    1: Always a critical hit.
	public void CritialOverride(user, target) {return 0; }

	// Returns whether the move will be a critical hit.
	public bool IsCritical(user, target) {
		if (target.OwnSide.effects.LuckyChant > 0) return false;
		c = 0;
		// Ability effects that alter critical hit rate
		if (c >= 0 && user.abilityActive()) {
			c = Battle.AbilityEffects.triggerCriticalCalcFromUser(user.ability, user, target, self, c);
		}
		if (c >= 0 && target.abilityActive() && !target.beingMoldBroken()) {
			c = Battle.AbilityEffects.triggerCriticalCalcFromTarget(target.ability, user, target, self, c);
		}
		// Item effects that alter critical hit rate
		if (c >= 0 && user.itemActive()) {
			c = Battle.ItemEffects.triggerCriticalCalcFromUser(user.item, user, target, self, c);
		}
		if (c >= 0 && target.itemActive()) {
			c = Battle.ItemEffects.triggerCriticalCalcFromTarget(target.item, user, target, self, c);
		}
		if (c < 0) return false;
		// Move-specific "always/never a critical hit" effects
		switch (CritialOverride(user, target)) {
			case 1:   return true;
			case -1:  return false;
		}
		// Other effects
		if (c > 50) return true;   // Merciless
		if (user.effects.LaserFocus > 0) return true;
		if (highCriticalRate()) c += 1;
		c += user.effects.FocusEnergy;
		if (user.inHyperMode() && @type == types.SHADOW) c += 1;
		// Set up the critical hit ratios
		ratios = CRITICAL_HIT_RATIOS;
		if (c >= ratios.length) c = ratios.length - 1;
		// Calculation
		if (ratios[c] == 1) return true;
		r = @battle.Random(ratios[c]);
		if (r == 0) return true;
		if (r == 1 && Settings.AFFECTION_EFFECTS && @battle.internalBattle &&
			user.OwnedByPlayer() && user.affection_level == 5 && !target.mega()) {
			target.damageState.affection_critical = true;
			return true;
		}
		return false;
	}

	//-----------------------------------------------------------------------------
	// Damage calculation.
	//-----------------------------------------------------------------------------

	public void BaseDamage(baseDmg, user, target)              {return baseDmg;    }
	public void BaseDamageMultiplier(damageMult, user, target) {return damageMult; }
	public void ModifyDamage(damageMult, user, target)         {return damageMult; }

	public void GetAttackStats(user, target) {
		if (specialMove()) return user.spatk, user.stages[:SPECIAL_ATTACK] + Battle.Battler.STAT_STAGE_MAXIMUM;
		return user.attack, user.stages[:ATTACK] + Battle.Battler.STAT_STAGE_MAXIMUM;
	}

	public void GetDefenseStats(user, target) {
		if (specialMove()) return target.spdef, target.stages[:SPECIAL_DEFENSE] + Battle.Battler.STAT_STAGE_MAXIMUM;
		return target.defense, target.stages[:DEFENSE] + Battle.Battler.STAT_STAGE_MAXIMUM;
	}

	public void CalcDamage(user, target, numTargets = 1) {
		if (statusMove()) return;
		if (target.damageState.disguise || target.damageState.iceFace) {
			target.damageState.calcDamage = 1;
			return;
		}
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stageMul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stageDiv = Battle.Battler.STAT_STAGE_DIVISORS;
		// Get the move's type
		type = @calcType;   // null is treated as physical
		// Calculate whether this hit deals critical damage
		target.damageState.critical = IsCritical(user, target);
		// Calcuate base power of move
		baseDmg = BaseDamage(@power, user, target);
		// Calculate user's attack stat
		atk, atkStage = GetAttackStats(user, target);
		if (!target.hasActiveAbility(Abilitys.UNAWARE) || target.beingMoldBroken()) {
			if (target.damageState.critical && atkStage < max_stage) atkStage = max_stage;
			atk = (int)Math.Floor(atk.to_f * stageMul[atkStage] / stageDiv[atkStage]);
		}
		// Calculate target's defense stat
		defense, defStage = GetDefenseStats(user, target);
		if (!user.hasActiveAbility(Abilitys.UNAWARE)) {
			if (target.damageState.critical && defStage > max_stage) defStage = max_stage;
			defense = (int)Math.Floor(defense.to_f * stageMul[defStage] / stageDiv[defStage]);
		}
		// Calculate all multiplier effects
		multipliers = {
			power_multiplier        = 1.0,
			attack_multiplier       = 1.0,
			defense_multiplier      = 1.0,
			final_damage_multiplier = 1.0;
		}
		CalcDamageMultipliers(user, target, numTargets, type, baseDmg, multipliers);
		// Main damage calculation
		baseDmg = (int)Math.Max((int)Math.Round(baseDmg * multipliers.power_multiplier), 1);
		atk     = (int)Math.Max((int)Math.Round(atk     * multipliers.attack_multiplier), 1);
		defense = (int)Math.Max((int)Math.Round(defense * multipliers.defense_multiplier), 1);
		damage  = (int)Math.Floor((int)Math.Floor((int)Math.Floor((2.0 * user.level / 5) + 2) * baseDmg * atk / defense) / 50) + 2;
		damage  = (int)Math.Max((int)Math.Round(damage * multipliers.final_damage_multiplier), 1);
		target.damageState.calcDamage = damage;
	}

	public void CalcDamageMultipliers(user, target, numTargets, type, baseDmg, multipliers) {
		// Global abilities
		all_abilities = @battle.AllActiveAbilities;
		if ((all_abilities.Contains(:DARKAURA) && type == types.DARK) ||
			(all_abilities.Contains(:FAIRYAURA) && type == types.FAIRY)) {
			if (all_abilities.Contains(:AURABREAK)) {
				multipliers.power_multiplier *= 3 / 4.0;
			} else {
				multipliers.power_multiplier *= 4 / 3.0;
			}
		}
		if (all_abilities.Contains(:TABLETSOFRUIN) && user.ability_id != abilitys.TABLETSOFRUIN) {
			if (physicalMove()) multipliers.power_multiplier *= 3 / 4.0;
		}
		if (all_abilities.Contains(:VESSELOFRUIN) && user.ability_id != abilitys.VESSELOFRUIN) {
			if (specialMove()) multipliers.power_multiplier *= 3 / 4.0;
		}
		if (all_abilities.Contains(:SWORDOFRUIN) && user.ability_id != abilitys.SWORDOFRUIN) {
			if (@battle.field.effects.WonderRoom > 0) {
				if (specialMove()) multipliers.defense_multiplier *= 3 / 4.0;
			} else {
				if (physicalMove()) multipliers.defense_multiplier *= 3 / 4.0;
			}
		}
		if (all_abilities.Contains(:BEADSOFRUIN) && user.ability_id != abilitys.BEADSOFRUIN) {
			if (@battle.field.effects.WonderRoom > 0) {
				if (physicalMove()) multipliers.defense_multiplier *= 3 / 4.0;
			} else {
				if (specialMove()) multipliers.defense_multiplier *= 3 / 4.0;
			}
		}
		// Ability effects that alter damage
		if (user.abilityActive()) {
			Battle.AbilityEffects.triggerDamageCalcFromUser(
				user.ability, user, target, self, multipliers, baseDmg, type
			);
		}
		// NOTE: It's odd that the user's Mold Breaker prevents its partner's
		//       beneficial abilities (i.e. Flower Gift boosting Atk), but that's
		//       how it works.
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (!b.abilityActive() || b.beingMoldBroken()) continue;
			Battle.AbilityEffects.triggerDamageCalcFromAlly(
				b.ability, user, target, self, multipliers, baseDmg, type
			);
		}
		if (target.abilityActive()) {
			if (!target.beingMoldBroken()) {
				Battle.AbilityEffects.triggerDamageCalcFromTarget(
					target.ability, user, target, self, multipliers, baseDmg, type
				);
			}
			Battle.AbilityEffects.triggerDamageCalcFromTargetNonIgnorable(
				target.ability, user, target, self, multipliers, baseDmg, type
			);
		}
		foreach (var b in target.allAllies) { //'target.allAllies.each' do => |b|
			if (!b.abilityActive() || b.beingMoldBroken()) continue;
			Battle.AbilityEffects.triggerDamageCalcFromTargetAlly(
				b.ability, user, target, self, multipliers, baseDmg, type
			);
		}
		// Item effects that alter damage
		if (user.itemActive()) {
			Battle.ItemEffects.triggerDamageCalcFromUser(
				user.item, user, target, self, multipliers, baseDmg, type
			);
		}
		if (target.itemActive()) {
			Battle.ItemEffects.triggerDamageCalcFromTarget(
				target.item, user, target, self, multipliers, baseDmg, type
			);
		}
		// Parental Bond's second attack
		if (user.effects.ParentalBond == 1) {
			multipliers.power_multiplier /= (Settings.MECHANICS_GENERATION >= 7) ? 4 : 2;
		}
		// Other
		if (user.effects.MeFirst) {
			multipliers.power_multiplier *= 1.5;
		}
		if (user.effects.HelpingHand && !self.is_a(Battle.Move.Confusion)) {
			multipliers.power_multiplier *= 1.5;
		}
		if (user.effects.Charge > 0 && type == types.ELECTRIC) {
			multipliers.power_multiplier *= 2;
		}
		// Mud Sport
		if (type == types.ELECTRIC) {
			if (@battle.allBattlers.any(b => b.effects.MudSport)) {
				multipliers.power_multiplier /= 3;
			}
			if (@battle.field.effects.MudSportField > 0) {
				multipliers.power_multiplier /= 3;
			}
		}
		// Water Sport
		if (type == types.FIRE) {
			if (@battle.allBattlers.any(b => b.effects.WaterSport)) {
				multipliers.power_multiplier /= 3;
			}
			if (@battle.field.effects.WaterSportField > 0) {
				multipliers.power_multiplier /= 3;
			}
		}
		// Terrain moves
		terrain_multiplier = (Settings.MECHANICS_GENERATION >= 8) ? 1.3 : 1.5;
		switch (@battle.field.terrain) {
			case :Electric:
				if (type == types.ELECTRIC && user.affectedByTerrain()) multipliers.power_multiplier *= terrain_multiplier;
				break;
			case :Grassy:
				if (type == types.GRASS && user.affectedByTerrain()) multipliers.power_multiplier *= terrain_multiplier;
				break;
			case :Psychic:
				if (type == types.PSYCHIC && user.affectedByTerrain()) multipliers.power_multiplier *= terrain_multiplier;
				break;
			case :Misty:
				if (type == types.DRAGON && target.affectedByTerrain()) multipliers.power_multiplier /= 2;
				break;
		}
		// Badge multipliers
		if (@battle.internalBattle) {
			if (user.OwnedByPlayer()) {
				if (physicalMove() && @battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_ATTACK) {
					multipliers.attack_multiplier *= 1.1;
				} else if (specialMove() && @battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_SPATK) {
					multipliers.attack_multiplier *= 1.1;
				}
			}
			if (target.OwnedByPlayer()) {
				if (physicalMove() && @battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_DEFENSE) {
					multipliers.defense_multiplier *= 1.1;
				} else if (specialMove() && @battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_SPDEF) {
					multipliers.defense_multiplier *= 1.1;
				}
			}
		}
		// Multi-targeting attacks
		if (numTargets > 1) multipliers.final_damage_multiplier *= 0.75;
		// Weather
		switch (target.effectiveWeather) {
			case :Sun, :HarshSun:
				switch (type) {
					case :FIRE:
						multipliers.final_damage_multiplier *= 1.5;
						break;
					case :WATER:
						if (@function_code == "IncreasePowerInSun" && new []{:Sun, :HarshSun}.Contains(user.effectiveWeather)) {
							multipliers.final_damage_multiplier *= 1.5;
						} else {
							multipliers.final_damage_multiplier /= 2;
						}
						break;
				}
				break;
			case :Rain, :HeavyRain:
				switch (type) {
					case :FIRE:
						multipliers.final_damage_multiplier /= 2;
						break;
					case :WATER:
						multipliers.final_damage_multiplier *= 1.5;
						break;
				}
				break;
			case :Sandstorm:
				if (target.Type == Types.ROCK && specialMove() && @function_code != "UseTargetDefenseInsteadOfTargetSpDef") {
					multipliers.defense_multiplier *= 1.5;
				}
				break;
			case :ShadowSky:
				if (type == types.SHADOW) multipliers.final_damage_multiplier *= 1.5;
				break;
		}
		// Critical hits
		if (target.damageState.critical) {
			if (Settings.NEW_CRITICAL_HIT_RATE_MECHANICS) {
				multipliers.final_damage_multiplier *= 1.5;
			} else {
				multipliers.final_damage_multiplier *= 2;
			}
		}
		// Random variance
		if (!self.is_a(Battle.Move.Confusion)) {
			random = 85 + @battle.Random(16);
			multipliers.final_damage_multiplier *= random / 100.0;
		}
		// STAB
		if (type && user.HasType(type)) {
			if (user.hasActiveAbility(Abilitys.ADAPTABILITY)) {
				multipliers.final_damage_multiplier *= 2;
			} else {
				multipliers.final_damage_multiplier *= 1.5;
			}
		}
		// Type effectiveness
		multipliers.final_damage_multiplier *= target.damageState.typeMod;
		// Burn
		if (user.status == statuses.BURN && physicalMove() && damageReducedByBurn() &&
			!user.hasActiveAbility(Abilitys.GUTS)) {
			multipliers.final_damage_multiplier /= 2;
		}
		// Aurora Veil, Reflect, Light Screen
		if (!ignoresReflect() && !target.damageState.critical &&
			!user.hasActiveAbility(Abilitys.INFILTRATOR)) {
			if (target.OwnSide.effects.AuroraVeil > 0) {
				if (@battle.SideBattlerCount(target) > 1) {
					multipliers.final_damage_multiplier *= 2 / 3.0;
				} else {
					multipliers.final_damage_multiplier /= 2;
				}
			} else if (target.OwnSide.effects.Reflect > 0 && physicalMove()) {
				if (@battle.SideBattlerCount(target) > 1) {
					multipliers.final_damage_multiplier *= 2 / 3.0;
				} else {
					multipliers.final_damage_multiplier /= 2;
				}
			} else if (target.OwnSide.effects.LightScreen > 0 && specialMove()) {
				if (@battle.SideBattlerCount(target) > 1) {
					multipliers.final_damage_multiplier *= 2 / 3.0;
				} else {
					multipliers.final_damage_multiplier /= 2;
				}
			}
		}
		// Minimize
		if (target.effects.Minimize && tramplesMinimize()) {
			multipliers.final_damage_multiplier *= 2;
		}
		// Move-specific base damage modifiers
		multipliers.power_multiplier = BaseDamageMultiplier(multipliers.power_multiplier, user, target);
		// Move-specific final damage modifiers
		multipliers.final_damage_multiplier = ModifyDamage(multipliers.final_damage_multiplier, user, target);
	}

	//-----------------------------------------------------------------------------
	// Additional effect chance.
	//-----------------------------------------------------------------------------

	public void AdditionalEffectChance(user, target, effectChance = 0) {
		if (target.hasActiveAbility(Abilitys.SHIELDDUST) && !target.beingMoldBroken()) return 0;
		ret = (effectChance > 0) ? effectChance : @addlEffect;
		if (ret > 100) return ret;
		if ((Settings.MECHANICS_GENERATION >= 6 || @function_code != "EffectDependsOnEnvironment") &&
			(user.hasActiveAbility(Abilitys.SERENEGRACE) || user.OwnSide.effects.Rainbow > 0)) {
			ret *= 2;
		}
		if (Core.DEBUG && Input.press(Input.CTRL)) ret = 100;
		return ret;
	}

	// NOTE: Flinching caused by a move's effect is applied in that move's code,
	//       not here.
	public void FlinchChance(user, target) {
		if (flinchingMove()) return 0;
		if (target.hasActiveAbility(Abilitys.SHIELDDUST) && !target.beingMoldBroken()) return 0;
		ret = 0;
		if (user.hasActiveAbility(:STENCH, true) ||
			user.hasActiveItem(new {:KINGSROCK, :RAZORFANG}, true)) {
			ret = 10;
		}
		if (user.hasActiveAbility(Abilitys.SERENEGRACE) ||
								user.OwnSide.effects.Rainbow > 0) ret *= 2;
		return ret;
	}
}
