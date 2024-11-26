//===============================================================================
//
//===============================================================================
public partial class Battle.AI.AIMove {
	public int move		{ get { return _move; } }			protected int _move;

	public void initialize(ai) {
		@ai = ai;
	}

	public void set_up(move) {
		@move = move;
		@move.calcType = rough_type;
		@ai.battle.moldBreaker ||= new []{"IgnoreTargetAbility",
																"CategoryDependsOnHigherDamageIgnoreTargetAbility"}.Contains(function_code);
	}

	//-----------------------------------------------------------------------------

	public int id                            { get { return @move.id;                      } }
	public int name                          { get { return @move.name;                    } }
	public bool physicalMove(thisType = null) {  return @move.physicalMove(thisType); }
	public bool specialMove(thisType = null) {   return @move.specialMove(thisType);  }
	public bool damagingMove() {                 return @move.damagingMove();           }
	public bool statusMove() {                   return @move.statusMove();             }
	public int function_code                 { get { return @move.function_code;           } }

	//-----------------------------------------------------------------------------

	public int type { get { return @move.type; } }

	public void rough_type() {
		if (@ai.trainer.medium_skill()) return @move.CalcType(@ai.user.battler);
		return @move.type;
	}

	//-----------------------------------------------------------------------------

	public void Target(user) {
		return @move.Target((user.is_a(Battle.AI.AIBattler)) ? user.battler : user);
	}

	// Returns whether this move targets multiple battlers.
	public bool targets_multiple_battlers() {
		user_battler = @ai.user.battler;
		target_data = Target(user_battler);
		if (target_data.num_targets <= 1) return false;
		num_targets = 0;
		switch (target_data.id) {
			case :AllAllies:
				@ai.battle.allSameSideBattlers(user_battler).each(b => { if (b.index != user_battler.index) num_targets += 1; });
				break;
			case :UserAndAllies:
				@ai.battle.allSameSideBattlers(user_battler).each(_b => num_targets += 1);
				break;
			case :AllNearFoes:
				@ai.battle.allOtherSideBattlers(user_battler).each(b => { if (b.near(user_battler)) num_targets += 1; });
				break;
			case :AllFoes:
				@ai.battle.allOtherSideBattlers(user_battler).each(_b => num_targets += 1);
				break;
			case :AllNearOthers:
				@ai.battle.allBattlers.each(b => { if (b.near(user_battler)) num_targets += 1; });
				break;
			case :AllBattlers:
				@ai.battle.allBattlers.each(_b => num_targets += 1);
				break;
		}
		return num_targets > 1;
	}

	//-----------------------------------------------------------------------------

	public void rough_priority(user) {
		ret = @move.Priority(user.battler);
		if (user.ability_active()) {
			ret = Battle.AbilityEffects.triggerPriorityChange(user.ability, user.battler, @move, ret);
			user.battler.effects.Prankster = false;   // Untrigger this
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	// Returns this move's base power, taking into account various effects that
	// modify it.
	public void base_power() {
		ret = @move.power;
		if (ret == 1) ret = 60;
		if (!@ai.trainer.medium_skill()) return ret;
		return Battle.AI.Handlers.get_base_power(function_code,
			ret, self, @ai.user, @ai.target, @ai, @ai.battle);
	}

	// Full damage calculation.
	public void rough_damage() {
		base_dmg = base_power;
		if (@move.is_a(Battle.Move.FixedDamageMove)) return base_dmg;
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stage_mul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stage_div = Battle.Battler.STAT_STAGE_DIVISORS;
		// Get the user and target of this move
		user = @ai.user;
		user_battler = user.battler;
		target = @ai.target;
		target_battler = target.battler;
		// Get the move's type
		calc_type = rough_type;
		// Decide whether the move has 50% chance of higher of being a critical hit
		crit_stage = rough_critical_hit_stage;
		is_critical = crit_stage >= Battle.Move.CRITICAL_HIT_RATIOS.length ||
									Battle.Move.CRITICAL_HIT_RATIOS[crit_stage] <= 2;
		//#### Calculate user's attack stat #####
		if ((new []{"CategoryDependsOnHigherDamagePoisonTarget",
				"CategoryDependsOnHigherDamageIgnoreTargetAbility"}.Contains(function_code)) {
			@move.OnStartUse(user.battler, [target.battler]);   // Calculate category
		}
		atk, atk_stage = @move.GetAttackStats(user.battler, target.battler);
		if (!target.has_active_ability(abilitys.UNAWARE) || target.being_mold_broken()) {
			if (is_critical && atk_stage < max_stage) atk_stage = max_stage;
			atk = (int)Math.Floor(atk.to_f * stage_mul[atk_stage] / stage_div[atk_stage]);
		}
		//#### Calculate target's defense stat #####
		defense, def_stage = @move.GetDefenseStats(user.battler, target.battler);
		if (!user.has_active_ability(abilitys.UNAWARE) || user.being_mold_broken()) {
			if (is_critical && def_stage > max_stage) def_stage = max_stage;
			defense = (int)Math.Floor(defense.to_f * stage_mul[def_stage] / stage_div[def_stage]);
		}
		//#### Calculate all multiplier effects #####
		multipliers = {
			power_multiplier        = 1.0,
			attack_multiplier       = 1.0,
			defense_multiplier      = 1.0,
			final_damage_multiplier = 1.0;
		}
		// Global abilities
		if (@ai.trainer.medium_skill() &&
			((@ai.battle.CheckGlobalAbility(Abilities.DARKAURA) && calc_type == types.DARK) ||
				(@ai.battle.CheckGlobalAbility(Abilities.FAIRYAURA) && calc_type == types.FAIRY))) {
			if (@ai.battle.CheckGlobalAbility(Abilities.AURABREAK)) {
				multipliers.power_multiplier *= 3 / 4.0;
			} else {
				multipliers.power_multiplier *= 4 / 3.0;
			}
		}
		// Ability effects that alter damage
		if (user.ability_active()) {
			switch (user.ability_id) {
				case :AERILATE: case :GALVANIZE: case :PIXILATE: case :REFRIGERATE:
					if (type == types.NORMAL) multipliers.power_multiplier *= 1.2;   // NOTE: Not calc_type.
					break;
				case :ANALYTIC:
					if (rough_priority(user) <= 0) {
						user_faster = false;
						@ai.each_battler do |b, i|
							user_faster = (i != user.index && user.faster_than(b));
							if (user_faster) break;
						}
						if (!user_faster) multipliers.power_multiplier *= 1.3;
					}
					break;
				case :NEUROFORCE:
					if (Effectiveness.super_effective_type(calc_type, *target.Types(true))) {
						multipliers.final_damage_multiplier *= 1.25;
					}
					break;
				case :NORMALIZE:
					if (Settings.MECHANICS_GENERATION >= 7) multipliers.power_multiplier *= 1.2;
					break;
				case :SNIPER:
					if (is_critical) multipliers.final_damage_multiplier *= 1.5;
					break;
				case :STAKEOUT:
					// NOTE: Can't predict whether the target will switch out this round.
					break;
				case :TINTEDLENS:
					if (Effectiveness.resistant_type(calc_type, *target.Types(true))) {
						multipliers.final_damage_multiplier *= 2;
					}
					break;
				default:
					Battle.AbilityEffects.triggerDamageCalcFromUser(
						user.ability, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
					);
					break;
			}
		}
		foreach (var b in user_battler.allAllies) { //'user_battler.allAllies.each' do => |b|
			if (!b.abilityActive() || b.beingMoldBroken()) continue;
			Battle.AbilityEffects.triggerDamageCalcFromAlly(
				b.ability, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
			);
		}
		if (target.ability_active() && !target.being_mold_broken()) {
			switch (target.ability_id) {
				case :FILTER: case :SOLIDROCK:
					if (Effectiveness.super_effective_type(calc_type, *target.Types(true))) {
						multipliers.final_damage_multiplier *= 0.75;
					}
					break;
				default:
					Battle.AbilityEffects.triggerDamageCalcFromTarget(
						target.ability, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
					);
					break;
			}
		}
		if (target.ability_active()) {
			Battle.AbilityEffects.triggerDamageCalcFromTargetNonIgnorable(
				target.ability, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
			);
		}
		foreach (var b in target_battler.allAllies) { //'target_battler.allAllies.each' do => |b|
			if (!b.abilityActive() || b.beingMoldBroken()) continue;
			Battle.AbilityEffects.triggerDamageCalcFromTargetAlly(
				b.ability, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
			);
		}
		// Item effects that alter damage
		if (user.item_active()) {
			switch (user.item_id) {
				case :EXPERTBELT:
					if (Effectiveness.super_effective_type(calc_type, *target.Types(true))) {
						multipliers.final_damage_multiplier *= 1.2;
					}
					break;
				case :LIFEORB:
					multipliers.final_damage_multiplier *= 1.3;
					break;
				default:
					Battle.ItemEffects.triggerDamageCalcFromUser(
						user.item, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
					);
					user.effects.GemConsumed = null;   // Untrigger consuming of Gems
					break;
			}
		}
		if (target.item_active() && target.item && !target.item.is_berry()) {
			Battle.ItemEffects.triggerDamageCalcFromTarget(
				target.item, user_battler, target_battler, @move, multipliers, base_dmg, calc_type
			);
		}
		// Parental Bond
		if (user.has_active_ability(abilitys.PARENTALBOND)) {
			multipliers.power_multiplier *= (Settings.MECHANICS_GENERATION >= 7) ? 1.25 : 1.5;
		}
		// Me First - n/a because can't predict the move Me First will use
		// Helping Hand - n/a
		// Charge
		if (@ai.trainer.medium_skill() &&
			user.effects.Charge > 0 && calc_type == types.ELECTRIC) {
			multipliers.power_multiplier *= 2;
		}
		// Mud Sport and Water Sport
		if (@ai.trainer.medium_skill()) {
			switch (calc_type) {
				case :ELECTRIC:
					if (@ai.battle.allBattlers.any(b => b.effects.MudSport)) {
						multipliers.power_multiplier /= 3;
					}
					if (@ai.battle.field.effects.MudSportField > 0) {
						multipliers.power_multiplier /= 3;
					}
					break;
				case :FIRE:
					if (@ai.battle.allBattlers.any(b => b.effects.WaterSport)) {
						multipliers.power_multiplier /= 3;
					}
					if (@ai.battle.field.effects.WaterSportField > 0) {
						multipliers.power_multiplier /= 3;
					}
					break;
			}
		}
		// Terrain moves
		if (@ai.trainer.medium_skill()) {
			terrain_multiplier = (Settings.MECHANICS_GENERATION >= 8) ? 1.3 : 1.5;
			switch (@ai.battle.field.terrain) {
				case :Electric:
					if (calc_type == types.ELECTRIC && user_battler.affectedByTerrain()) multipliers.power_multiplier *= terrain_multiplier;
					break;
				case :Grassy:
					if (calc_type == types.GRASS && user_battler.affectedByTerrain()) multipliers.power_multiplier *= terrain_multiplier;
					break;
				case :Psychic:
					if (calc_type == types.PSYCHIC && user_battler.affectedByTerrain()) multipliers.power_multiplier *= terrain_multiplier;
					break;
				case :Misty:
					if (calc_type == types.DRAGON && target_battler.affectedByTerrain()) multipliers.power_multiplier /= 2;
					break;
			}
		}
		// Badge multipliers
		if (@ai.trainer.high_skill() && @ai.battle.internalBattle && target_battler.OwnedByPlayer()) {
			// Don't need to check the Atk/Sp Atk-boosting badges because the AI
			// won't control the player's Pokémon.
			if (physicalMove(calc_type) && @ai.battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_DEFENSE) {
				multipliers.defense_multiplier *= 1.1;
			} else if (specialMove(calc_type) && @ai.battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_SPDEF) {
				multipliers.defense_multiplier *= 1.1;
			}
		}
		// Multi-targeting attacks
		if (@ai.trainer.high_skill() && targets_multiple_battlers()) {
			multipliers.final_damage_multiplier *= 0.75;
		}
		// Weather
		if (@ai.trainer.medium_skill()) {
			switch (user_battler.effectiveWeather) {
				case :Sun: case :HarshSun:
					switch (calc_type) {
						case :FIRE:
							multipliers.final_damage_multiplier *= 1.5;
							break;
						case :WATER:
							multipliers.final_damage_multiplier /= 2;
							break;
					}
					break;
				case :Rain: case :HeavyRain:
					switch (calc_type) {
						case :FIRE:
							multipliers.final_damage_multiplier /= 2;
							break;
						case :WATER:
							multipliers.final_damage_multiplier *= 1.5;
							break;
					}
					break;
				case :Sandstorm:
					if (target.has_type(types.ROCK) && specialMove(calc_type) &&
						function_code != "UseTargetDefenseInsteadOfTargetSpDef") {   // Psyshock
						multipliers.defense_multiplier *= 1.5;
					}
					break;
			}
		}
		// Critical hits
		if (is_critical) {
			if (Settings.NEW_CRITICAL_HIT_RATE_MECHANICS) {
				multipliers.final_damage_multiplier *= 1.5;
			} else {
				multipliers.final_damage_multiplier *= 2;
			}
		}
		// Random variance - n/a
		// STAB
		if (calc_type && user.has_type(calc_type)) {
			if (user.has_active_ability(abilitys.ADAPTABILITY)) {
				multipliers.final_damage_multiplier *= 2;
			} else {
				multipliers.final_damage_multiplier *= 1.5;
			}
		}
		// Type effectiveness
		typemod = target.effectiveness_of_type_against_battler(calc_type, user, @move);
		multipliers.final_damage_multiplier *= typemod;
		// Burn
		if (@ai.trainer.high_skill() && user.status == statuses.BURN && physicalMove(calc_type) &&
			@move.damageReducedByBurn() && !user.has_active_ability(abilitys.GUTS)) {
			multipliers.final_damage_multiplier /= 2;
		}
		// Aurora Veil, Reflect, Light Screen
		if (@ai.trainer.medium_skill() && !@move.ignoresReflect() && !is_critical &&
			!user.has_active_ability(abilitys.INFILTRATOR)) {
			if (target.OwnSide.effects.AuroraVeil > 0) {
				if (@ai.battle.SideBattlerCount(target_battler) > 1) {
					multipliers.final_damage_multiplier *= 2 / 3.0;
				} else {
					multipliers.final_damage_multiplier /= 2;
				}
			} else if (target.OwnSide.effects.Reflect > 0 && physicalMove(calc_type)) {
				if (@ai.battle.SideBattlerCount(target_battler) > 1) {
					multipliers.final_damage_multiplier *= 2 / 3.0;
				} else {
					multipliers.final_damage_multiplier /= 2;
				}
			} else if (target.OwnSide.effects.LightScreen > 0 && specialMove(calc_type)) {
				if (@ai.battle.SideBattlerCount(target_battler) > 1) {
					multipliers.final_damage_multiplier *= 2 / 3.0;
				} else {
					multipliers.final_damage_multiplier /= 2;
				}
			}
		}
		// Minimize
		if (@ai.trainer.medium_skill() && target.effects.Minimize && @move.tramplesMinimize()) {
			multipliers.final_damage_multiplier *= 2;
		}
		// NOTE: No need to check BaseDamageMultiplier, as it's already accounted
		//       for in an AI's MoveBasePower handler or can't be checked now anyway.
		// NOTE: No need to check ModifyDamage, as it's already accounted for in an
		//       AI's MoveBasePower handler.
		//#### Main damage calculation #####
		base_dmg = (int)Math.Max((int)Math.Round(base_dmg * multipliers.power_multiplier), 1);
		atk      = (int)Math.Max((int)Math.Round(atk      * multipliers.attack_multiplier), 1);
		defense  = (int)Math.Max((int)Math.Round(defense  * multipliers.defense_multiplier), 1);
		damage   = (int)Math.Floor((int)Math.Floor((int)Math.Floor((2.0 * user.level / 5) + 2) * base_dmg * atk / defense) / 50) + 2;
		damage   = (int)Math.Max((int)Math.Round(damage * multipliers.final_damage_multiplier), 1);
		ret = damage.floor;
		if (@move.nonLethal(user_battler, target_battler) && ret >= target.hp) ret = target.hp - 1;
		return ret;
	}

	//-----------------------------------------------------------------------------

	public void accuracy() {
		if (@ai.trainer.medium_skill()) return @move.BaseAccuracy(@ai.user.battler, @ai.target.battler);
		return @move.accuracy;
	}

	// Full accuracy calculation.
	public void rough_accuracy() {
		// Determine user and target
		user = @ai.user;
		user_battler = user.battler;
		target = @ai.target;
		target_battler = target.battler;
		// OHKO move accuracy
		if (@move.is_a(Battle.Move.OHKO)) {
			ret = self.accuracy + user.level - target.level;
			if (function_code == "OHKOIce" && !user.has_type(types.ICE)) ret -= 10;
			return (int)Math.Max(ret, 0);
		}
		// "Always hit" effects and "always hit" accuracy
		if (@ai.trainer.medium_skill()) {
			if (target.effects.Telekinesis > 0) return 100;
			if (target.effects.Minimize && @move.tramplesMinimize() &&
										Settings.MECHANICS_GENERATION >= 6) return 100;
		}
		// Get base accuracy
		baseAcc = self.accuracy;
		if (baseAcc == 0) return 100;
		// Get the move's type
		type = rough_type;
		// Calculate all modifier effects
		modifiers = new List<string>();
		modifiers.base_accuracy  = baseAcc;
		modifiers.accuracy_stage = user.stages[:ACCURACY];
		modifiers.evasion_stage  = target.stages[:EVASION];
		modifiers.accuracy_multiplier = 1.0;
		modifiers.evasion_multiplier  = 1.0;
		apply_rough_accuracy_modifiers(user, target, type, modifiers);
		// Check if move certainly misses/can't miss
		if (modifiers.base_accuracy < 0) return 0;
		if (modifiers.base_accuracy == 0) return 100;
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
		return modifiers.base_accuracy * accuracy / evasion;
	}

	public void apply_rough_accuracy_modifiers(user, target, calc_type, modifiers) {
		user_battler = user.battler;
		target_battler = target.battler;
		// Ability effects that alter accuracy calculation
		if (user.ability_active()) {
			Battle.AbilityEffects.triggerAccuracyCalcFromUser(
				user.ability, modifiers, user_battler, target_battler, @move, calc_type
			);
		}
		foreach (var b in user_battler.allAllies) { //'user_battler.allAllies.each' do => |b|
			if (!b.abilityActive()) continue;
			Battle.AbilityEffects.triggerAccuracyCalcFromAlly(
				b.ability, modifiers, user_battler, target_battler, @move, calc_type
			);
		}
		if (target.ability_active() && !target.being_mold_broken()) {
			Battle.AbilityEffects.triggerAccuracyCalcFromTarget(
				target.ability, modifiers, user_battler, target_battler, @move, calc_type
			);
		}
		// Item effects that alter accuracy calculation
		if (user.item_active()) {
			if (user.item == items.ZOOMLENS) {
				if (rough_priority(user) <= 0) {
					if (target.faster_than(user)) modifiers.accuracy_multiplier *= 1.2;
				}
			} else {
				Battle.ItemEffects.triggerAccuracyCalcFromUser(
					user.item, modifiers, user_battler, target_battler, @move, calc_type
				);
			}
		}
		if (target.item_active()) {
			Battle.ItemEffects.triggerAccuracyCalcFromTarget(
				target.item, modifiers, user_battler, target_battler, @move, calc_type
			);
		}
		// Other effects, inc. ones that set accuracy_multiplier or evasion_stage to specific values
		if (@ai.battle.field.effects.Gravity > 0) {
			modifiers.accuracy_multiplier *= 5 / 3.0;
		}
		if (@ai.trainer.medium_skill()) {
			if (user.effects.MicleBerry) {
				modifiers.accuracy_multiplier *= 1.2;
			}
			if (target.effects.Foresight && modifiers.evasion_stage > 0) modifiers.evasion_stage = 0;
			if (target.effects.MiracleEye && modifiers.evasion_stage > 0) modifiers.evasion_stage = 0;
		}
		// "AI-specific calculations below"
		if (function_code == "IgnoreTargetDefSpDefEvaStatStages") modifiers.evasion_stage = 0;   // Chip Away
		if (@ai.trainer.medium_skill()) {
			if (user.effects.LockOn > 0 &&
																			user.effects.LockOnPos == target.index) modifiers.base_accuracy = 0;
		}
		if (@ai.trainer.medium_skill()) {
			switch (function_code) {
				case "BadPoisonTarget":
					if (Settings.MORE_TYPE_EFFECTS &&
																					@move.statusMove() && user.has_type(types.POISON)) modifiers.base_accuracy = 0;
					break;
			}
		}
	}

	//-----------------------------------------------------------------------------

	// Full critical hit chance calculation (returns the determined critical hit
	// stage).
	public void rough_critical_hit_stage() {
		user = @ai.user;
		user_battler = user.battler;
		target = @ai.target;
		target_battler = target.battler;
		if (target_battler.OwnSide.effects.LuckyChant > 0) return -1;
		crit_stage = 0;
		// Ability effects that alter critical hit rate
		if (user.ability_active()) {
			crit_stage = Battle.AbilityEffects.triggerCriticalCalcFromUser(user_battler.ability,
				user_battler, target_battler, @move, crit_stage);
			if (crit_stage < 0) return -1;
		}
		if (target.ability_active() && !target.being_mold_broken()) {
			crit_stage = Battle.AbilityEffects.triggerCriticalCalcFromTarget(target_battler.ability,
				user_battler, target_battler, @move, crit_stage);
			if (crit_stage < 0) return -1;
		}
		// Item effects that alter critical hit rate
		if (user.item_active()) {
			crit_stage = Battle.ItemEffects.triggerCriticalCalcFromUser(user_battler.item,
				user_battler, target_battler, @move, crit_stage);
			if (crit_stage < 0) return -1;
		}
		if (target.item_active()) {
			crit_stage = Battle.ItemEffects.triggerCriticalCalcFromTarget(user_battler.item,
				user_battler, target_battler, @move, crit_stage);
			if (crit_stage < 0) return -1;
		}
		// Other effects
		switch (@move.CritialOverride(user_battler, target_battler)) {
			case 1:   return 99;
			case -1:  return -1;
		}
		if (crit_stage > 50) return 99;   // Merciless
		if (user_battler.effects.LaserFocus > 0) return 99;
		if (@move.highCriticalRate()) crit_stage += 1;
		crit_stage += user_battler.effects.FocusEnergy;
		if (user_battler.inHyperMode() && @move.type == types.SHADOW) crit_stage += 1;
		crit_stage = (int)Math.Min(crit_stage, Battle.Move.CRITICAL_HIT_RATIOS.length - 1);
		return crit_stage;
	}

	//-----------------------------------------------------------------------------

	// Return values:
	//   0: Isn't an additional effect or always triggers
	//   -999: Additional effect will be negated
	//   Other: Amount to add to a move's score
	// TODO: This value just gets added to the score, but it should only modify the
	//       score for the additional effect and shouldn't reduce that to less than
	//       0.
	public void get_score_change_for_additional_effect(user, target = null) {
		chance = @move.addlEffect;
		// Doesn't have an additional effect
		if (chance == 0) return 0;
		// Additional effect will be negated
		if (user.has_active_ability(abilitys.SHEERFORCE)) return -999;
		if (target && user.index != target.index &&
									target.has_active_ability(abilitys.SHIELDDUST) && !target.being_mold_broken()) return -999;
		// Additional effect will always trigger
		if (chance > 100) return 0;
		// Calculate the chance
		if ((Settings.MECHANICS_GENERATION >= 6 || function_code != "EffectDependsOnEnvironment") &&
									(user.has_active_ability(abilitys.SERENEGRACE) || user.OwnSide.effects.Rainbow > 0)) chance *= 2;
		// Don't prefer if the additional effect has a low chance of happening
		ret = 0;
		if (chance <= 10) {
			ret -= 10;
		} else if (chance <= 50) {
			ret -= 5;
		} else if (chance >= 80) {
			ret += 5;
		}
		return ret;
	}
}
