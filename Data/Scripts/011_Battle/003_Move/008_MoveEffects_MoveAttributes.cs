//===============================================================================
// Inflicts a fixed 20HP damage. (Sonic Boom)
//===============================================================================
public partial class Battle.Move.FixedDamage20 : Battle.Move.FixedDamageMove {
	public void FixedDamage(user, target) {
		return 20;
	}
}

//===============================================================================
// Inflicts a fixed 40HP damage. (Dragon Rage)
//===============================================================================
public partial class Battle.Move.FixedDamage40 : Battle.Move.FixedDamageMove {
	public void FixedDamage(user, target) {
		return 40;
	}
}

//===============================================================================
// Halves the target's current HP. (Nature's Madness, Super Fang)
//===============================================================================
public partial class Battle.Move.FixedDamageHalfTargetHP : Battle.Move.FixedDamageMove {
	public void FixedDamage(user, target) {
		return (int)Math.Round(target.hp / 2.0);
	}
}

//===============================================================================
// Inflicts damage equal to the user's level. (Night Shade, Seismic Toss)
//===============================================================================
public partial class Battle.Move.FixedDamageUserLevel : Battle.Move.FixedDamageMove {
	public void FixedDamage(user, target) {
		return user.level;
	}
}

//===============================================================================
// Inflicts damage between 0.5 and 1.5 times the user's level. (Psywave)
//===============================================================================
public partial class Battle.Move.FixedDamageUserLevelRandom : Battle.Move.FixedDamageMove {
	public void FixedDamage(user, target) {
		min = (int)Math.Floor(user.level / 2);
		max = (int)Math.Floor(user.level * 3 / 2);
		return min + @battle.Random(max - min + 1);
	}
}

//===============================================================================
// Inflicts damage to bring the target's HP down to equal the user's HP. (Endeavor)
//===============================================================================
public partial class Battle.Move.LowerTargetHPToUserHP : Battle.Move.FixedDamageMove {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (user.hp >= target.hp) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void NumHits(user, targets) {return 1; }

	public void FixedDamage(user, target) {
		return target.hp - user.hp;
	}
}

//===============================================================================
// OHKO. Accuracy increases by difference between levels of user and target.
//===============================================================================
public partial class Battle.Move.OHKO : Battle.Move.FixedDamageMove {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.level > user.level) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		if (target.hasActiveAbility(Abilitys.STURDY) && !target.beingMoldBroken()) {
			if (show_message) {
				@battle.ShowAbilitySplash(target);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("But it failed to affect {1}!", target.ToString(true)));
				} else {
					@battle.Display(_INTL("But it failed to affect {1} because of its {2}!",
																	target.ToString(true), target.abilityName));
				}
				@battle.HideAbilitySplash(target);
			}
			return true;
		}
		return false;
	}

	public void AccuracyCheck(user, target) {
		acc = @accuracy + user.level - target.level;
		return @battle.Random(100) < acc;
	}

	public void FixedDamage(user, target) {
		return target.totalhp;
	}

	public override void HitEffectivenessMessages(user, target, numTargets = 1) {
		base.HitEffectivenessMessages();
		if (target.fainted()) @battle.Display(_INTL("It's a one-hit KO!"));
	}
}

//===============================================================================
// OHKO. Accuracy increases by difference between levels of user and target.
// Lower accuracy when used by a non-Ice-type Pokémon. Doesn't affect Ice-type
// Pokémon. (Sheer Cold (Gen 7+))
//===============================================================================
public partial class Battle.Move.OHKOIce : Battle.Move.OHKO {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.Type == Types.ICE) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return super;
	}

	public void AccuracyCheck(user, target) {
		acc = @accuracy + user.level - target.level;
		if (!user.Type == Types.ICE) acc -= 10;
		return @battle.Random(100) < acc;
	}
}

//===============================================================================
// OHKO. Accuracy increases by difference between levels of user and target. Hits
// targets that are semi-invulnerable underground. (Fissure)
//===============================================================================
public partial class Battle.Move.OHKOHitsUndergroundTarget : Battle.Move.OHKO {
	public bool hitsDiggingTargets() { return true; }
}

//===============================================================================
// The target's ally loses 1/16 of its max HP. (Flame Burst)
//===============================================================================
public partial class Battle.Move.DamageTargetAlly : Battle.Move {
	public void EffectWhenDealingDamage(user, target) {
		hitAlly = new List<string>();
		foreach (var b in target.allAllies) { //'target.allAllies.each' do => |b|
			if (!b.near(target.index)) continue;
			if (!b.takesIndirectDamage()) continue;
			hitAlly.Add(new {b.index, b.hp});
			b.ReduceHP(b.totalhp / 16, false);
		}
		if (hitAlly.length == 2) {
			@battle.Display(_INTL("The bursting flame hit {1} and {2}!",
															@battle.battlers[hitAlly[0][0]].ToString(true),
															@battle.battlers[hitAlly[1][0]].ToString(true)));
		} else if (hitAlly.length > 0) {
			foreach (var b in hitAlly) { //'hitAlly.each' do => |b|
				@battle.Display(_INTL("The bursting flame hit {1}!",
																@battle.battlers[b[0]].ToString(true)));
			}
		}
		hitAlly.each(b => @battle.battlers[b[0]].ItemHPHealCheck);
	}
}

//===============================================================================
// Power increases with the user's HP. (Eruption, Water Spout)
//===============================================================================
public partial class Battle.Move.PowerHigherWithUserHP : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		return (int)Math.Max(150 * user.hp / user.totalhp, 1);
	}
}

//===============================================================================
// Power increases the less HP the user has. (Flail, Reversal)
//===============================================================================
public partial class Battle.Move.PowerLowerWithUserHP : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		ret = 20;
		n = 48 * user.hp / user.totalhp;
		if (n < 2) {
			ret = 200;
		} else if (n < 5) {
			ret = 150;
		} else if (n < 10) {
			ret = 100;
		} else if (n < 17) {
			ret = 80;
		} else if (n < 33) {
			ret = 40;
		}
		return ret;
	}
}

//===============================================================================
// Power increases with the target's HP. (Hard Press)
//===============================================================================
public partial class Battle.Move.PowerHigherWithTargetHP100 : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		return (int)Math.Max(100 * target.hp / target.totalhp, 1);
	}
}

//===============================================================================
// Power increases with the target's HP. (Crush Grip, Wring Out)
//===============================================================================
public partial class Battle.Move.PowerHigherWithTargetHP120 : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		return (int)Math.Max(120 * target.hp / target.totalhp, 1);
	}
}

//===============================================================================
// Power increases with the user's happiness. (Return)
//===============================================================================
public partial class Battle.Move.PowerHigherWithUserHappiness : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		return (int)Math.Max((int)Math.Floor(user.happiness * 2 / 5), 1);
	}
}

//===============================================================================
// Power decreases with the user's happiness. (Frustration)
//===============================================================================
public partial class Battle.Move.PowerLowerWithUserHappiness : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		return (int)Math.Max((int)Math.Floor((255 - user.happiness) * 2 / 5), 1);
	}
}

//===============================================================================
// Power increases with the user's positive stat changes (ignores negative ones).
// (Power Trip, Stored Power)
//===============================================================================
public partial class Battle.Move.PowerHigherWithUserPositiveStatStages : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		mult = 1;
		GameData.Stat.each_battle(s => { if (user.stages[s.id] > 0) mult += user.stages[s.id]; });
		return 20 * mult;
	}
}

//===============================================================================
// Power increases with the target's positive stat changes (ignores negative ones).
// (Punishment)
//===============================================================================
public partial class Battle.Move.PowerHigherWithTargetPositiveStatStages : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		mult = 3;
		GameData.Stat.each_battle(s => { if (target.stages[s.id] > 0) mult += target.stages[s.id]; });
		return (int)Math.Min(20 * mult, 200);
	}
}

//===============================================================================
// Power increases the quicker the user is than the target. (Electro Ball)
//===============================================================================
public partial class Battle.Move.PowerHigherWithUserFasterThanTarget : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		ret = 40;
		n = user.Speed / target.Speed;
		if (n >= 4) {
			ret = 150;
		} else if (n >= 3) {
			ret = 120;
		} else if (n >= 2) {
			ret = 80;
		} else if (n >= 1) {
			ret = 60;
		}
		return ret;
	}
}

//===============================================================================
// Power increases the quicker the target is than the user. (Gyro Ball)
//===============================================================================
public partial class Battle.Move.PowerHigherWithTargetFasterThanUser : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		return (int)Math.Max((int)Math.Min((int)Math.Floor(25 * target.Speed / user.Speed), 150), 1);
	}
}

//===============================================================================
// Power increases the less PP this move has. (Trump Card)
//===============================================================================
public partial class Battle.Move.PowerHigherWithLessPP : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		dmgs = new {200, 80, 60, 50, 40};
		ppLeft = (int)Math.Min(@pp, dmgs.length - 1);   // PP is reduced before the move is used
		return dmgs[ppLeft];
	}
}

//===============================================================================
// Power increases the heavier the target is. (Grass Knot, Low Kick)
//===============================================================================
public partial class Battle.Move.PowerHigherWithTargetWeight : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		ret = 20;
		weight = target.Weight;
		if (weight >= 2000) {
			ret = 120;
		} else if (weight >= 1000) {
			ret = 100;
		} else if (weight >= 500) {
			ret = 80;
		} else if (weight >= 250) {
			ret = 60;
		} else if (weight >= 100) {
			ret = 40;
		}
		return ret;
	}
}

//===============================================================================
// Power increases the heavier the user is than the target. (Heat Crash, Heavy Slam)
//===============================================================================
public partial class Battle.Move.PowerHigherWithUserHeavierThanTarget : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		ret = 40;
		n = (int)Math.Floor(user.Weight / target.Weight);
		if (n >= 5) {
			ret = 120;
		} else if (n >= 4) {
			ret = 100;
		} else if (n >= 3) {
			ret = 80;
		} else if (n >= 2) {
			ret = 60;
		}
		return ret;
	}
}

//===============================================================================
// Power doubles for each consecutive use. (Fury Cutter)
//===============================================================================
public partial class Battle.Move.PowerHigherWithConsecutiveUse : Battle.Move {
	public override void ChangeUsageCounters(user, specialUsage) {
		oldVal = user.effects.FuryCutter;
		base.ChangeUsageCounters();
		maxMult = 1;
		while ((@power << (maxMult - 1)) < 160) {
			maxMult += 1;   // 1-4 for base damage of 20, 1-3 for base damage of 40
		}
		user.effects.FuryCutter = (oldVal >= maxMult) ? maxMult : oldVal + 1;
	}

	public void BaseDamage(baseDmg, user, target) {
		return baseDmg << (user.effects.FuryCutter - 1);
	}
}

//===============================================================================
// Power is multiplied by the number of consecutive rounds in which this move was
// used by any Pokémon on the user's side. (Echoed Voice)
//===============================================================================
public partial class Battle.Move.PowerHigherWithConsecutiveUseOnUserSide : Battle.Move {
	public override void ChangeUsageCounters(user, specialUsage) {
		oldVal = user.OwnSide.effects.EchoedVoiceCounter;
		base.ChangeUsageCounters();
		if (!user.OwnSide.effects.EchoedVoiceUsed) {
			user.OwnSide.effects.EchoedVoiceCounter = (oldVal >= 5) ? 5 : oldVal + 1;
		}
		user.OwnSide.effects.EchoedVoiceUsed = true;
	}

	public void BaseDamage(baseDmg, user, target) {
		return baseDmg * user.OwnSide.effects.EchoedVoiceCounter;   // 1-5
	}
}

//===============================================================================
// Power is chosen at random. Power is doubled if the target is using Dig. Hits
// some semi-invulnerable targets. (Magnitude)
//===============================================================================
public partial class Battle.Move.RandomPowerDoublePowerIfTargetUnderground : Battle.Move {
	public bool hitsDiggingTargets() { return true; }

	public void OnStartUse(user, targets) {
		baseDmg = new {10, 30, 50, 70, 90, 110, 150};
		magnitudes = new {
			4,
			5, 5,
			6, 6, 6, 6,
			7, 7, 7, 7, 7, 7,
			8, 8, 8, 8,
			9, 9,
			10;
		}
		magni = magnitudes[@battle.Random(magnitudes.length)];
		@magnitudeDmg = baseDmg[magni - 4];
		@battle.Display(_INTL("Magnitude {1}!", magni));
	}

	public void BaseDamage(baseDmg, user, target) {
		return @magnitudeDmg;
	}

	public void ModifyDamage(damageMult, user, target) {
		if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderground")) damageMult *= 2;
		if (@battle.field.terrain == :Grassy) damageMult /= 2;
		return damageMult;
	}
}

//===============================================================================
// Power is increased by 50% in sunny weather. (Hydro Steam)
//===============================================================================
public partial class Battle.Move.IncreasePowerInSun : Battle.Move {
	// NOTE: No code needed here. Effect is coded in def CalcDamageMultipliers.
}

//===============================================================================
// Power is increased by 50% if Electric Terrain applies. (Psyblade)
//===============================================================================
public partial class Battle.Move.IncreasePowerInElectricTerrain : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.terrain == :Electric && target.affectedByTerrain()) baseDmg = (int)Math.Floor(baseDmg * 1.5);
		return baseDmg;
	}
}

//===============================================================================
// Damage is increased by 33% if the move is super-effective. (Electro Drift)
//===============================================================================
public partial class Battle.Move.IncreasePowerIfSuperEffective : Battle.Move {
	public void ModifyDamage(damageMult, user, target) {
		if (Effectiveness.super_effective(target.damageState.typeMod)) damageMult = damageMult * 4 / 3;
		return damageMult;
	}
}

//===============================================================================
// Power is doubled 30% of the time. (Fickle Beam)
//===============================================================================
public partial class Battle.Move.DoublePower30PercentChance : Battle.Move {
	public void OnStartUse(user, targets) {
		@double_power = @battle.Random(100) < 30;
		if (@double_power) {
			@battle.DisplayBrief(_INTL("{1} is going all out for this attack!", user.ToString()));
		}
	}

	public void BaseDamage(baseDmg, user, target) {
		if (@double_power) baseDmg *= 2;
		return baseDmg;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@double_power) hitNum = 1;
		base.ShowAnimation();
	}
}

//===============================================================================
// Power is doubled if the target's HP is down to 1/2 or less. (Brine)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetHPLessThanHalf : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (target.hp <= target.totalhp / 2) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the user is burned, poisoned or paralyzed. (Facade)
// Burn's halving of Attack is negated (new mechanics).
//===============================================================================
public partial class Battle.Move.DoublePowerIfUserPoisonedBurnedParalyzed : Battle.Move {
	public bool damageReducedByBurn() { return Settings.MECHANICS_GENERATION <= 5; }

	public void BaseDamage(baseDmg, user, target) {
		if (user.poisoned() || user.burned() || user.paralyzed()) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the target is asleep. Wakes the target up. (Wake-Up Slap)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetAsleepCureTarget : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if ((target.asleep() &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			baseDmg *= 2;
		}
		return baseDmg;
	}

	public void EffectAfterAllHits(user, target) {
		if (target.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (target.status != statuses.SLEEP) return;
		target.CureStatus;
	}
}

//===============================================================================
// Power is doubled if the target is poisoned. (Venoshock)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetPoisoned : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if ((target.poisoned() &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			baseDmg *= 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the target is poisoned, and then poisons the target.
// (Barb Barrage)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetPoisonedPoisonTarget : Battle.Move.PoisonTarget {
	public void BaseDamage(baseDmg, user, target) {
		if ((target.poisoned() &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			baseDmg *= 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the target is paralyzed. Cures the target of paralysis.
// (Smelling Salts)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetParalyzedCureTarget : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if ((target.paralyzed() &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			baseDmg *= 2;
		}
		return baseDmg;
	}

	public void EffectAfterAllHits(user, target) {
		if (target.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (target.status != statuses.PARALYSIS) return;
		target.CureStatus;
	}
}

//===============================================================================
// Power is doubled if the target has a status problem. (Hex)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetStatusProblem : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if ((target.HasAnyStatus() &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			baseDmg *= 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the target has a status problem, and then burns the
// target. (Infernal Parade)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetStatusProblemBurnTarget : Battle.Move.BurnTarget {
	public void BaseDamage(baseDmg, user, target) {
		if ((target.HasAnyStatus() &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			baseDmg *= 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the user has no held item. (Acrobatics)
//===============================================================================
public partial class Battle.Move.DoublePowerIfUserHasNoItem : Battle.Move {
	public void BaseDamageMultiplier(damageMult, user, target) {
		if (!user.item || user.effects.GemConsumed) damageMult *= 2;
		return damageMult;
	}
}

//===============================================================================
// Power is doubled if the target is using Dive. Hits some semi-invulnerable
// targets. (Surf)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetUnderwater : Battle.Move {
	public bool hitsDivingTargets() { return true; }

	public void ModifyDamage(damageMult, user, target) {
		if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderwater")) damageMult *= 2;
		return damageMult;
	}
}

//===============================================================================
// Power is doubled if the target is using Dig. Power is halved if Grassy Terrain
// is in effect. Hits some semi-invulnerable targets. (Earthquake)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetUnderground : Battle.Move {
	public bool hitsDiggingTargets() { return true; }

	public void ModifyDamage(damageMult, user, target) {
		if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderground")) damageMult *= 2;
		if (@battle.field.terrain == :Grassy) damageMult /= 2;
		return damageMult;
	}
}

//===============================================================================
// Power is doubled if the target is using Bounce, Fly or Sky Drop. Hits some
// semi-invulnerable targets. (Gust)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetInSky : Battle.Move {
	public bool hitsFlyingTargets() { return true; }

	public void BaseDamage(baseDmg, user, target) {
		if ((target.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
																						"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
																						"TwoTurnAttackInvulnerableInSkyTargetCannotAct") ||
										target.effects.SkyDrop >= 0) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if Electric Terrain applies. (Rising Voltage)
//===============================================================================
public partial class Battle.Move.DoublePowerInElectricTerrain : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.terrain == :Electric && target.affectedByTerrain()) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the user's last move failed. (Stomping Tantrum)
//===============================================================================
public partial class Battle.Move.DoublePowerIfUserLastMoveFailed : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (user.lastRoundMoveFailed) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if a user's teammate fainted last round. (Retaliate)
//===============================================================================
public partial class Battle.Move.DoublePowerIfAllyFaintedLastTurn : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		lrf = user.OwnSide.effects.LastRoundFainted;
		if (lrf >= 0 && lrf == @battle.turnCount - 1) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the user has lost HP due to the target's move this round.
// (Avalanche, Revenge)
//===============================================================================
public partial class Battle.Move.DoublePowerIfUserLostHPThisTurn : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (user.lastAttacker.Contains(target.index)) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the target has already lost HP this round. (Assurance)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetLostHPThisTurn : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (target.tookDamageThisRound) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if any of the user's stats were lowered this round. (Lash Out)
//===============================================================================
public partial class Battle.Move.DoublePowerIfUserStatsLoweredThisTurn : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (user.statsLoweredThisRound) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the target has already moved this round. (Payback)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetActed : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if ((@battle.choices[target.index].Action != :None &&
			((@battle.choices[target.index].Action != :UseMove &&
			@battle.choices[target.index].Action != :Shift) || target.movedThisRound())) {
			baseDmg *= 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// Power is doubled if the user moves before the target, or if the target
// switched in this round. (Bolt Beak, Fishious Rend)
//===============================================================================
public partial class Battle.Move.DoublePowerIfTargetNotActed : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (@battle.choices[target.index].Action == :None ||   // Switched in
			(new []{:UseMove, :Shift}.Contains(@battle.choices[target.index].Action) && !target.movedThisRound())) {
			baseDmg *= 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// This attack is always a critical hit. (Frost Breath, Storm Throw)
//===============================================================================
public partial class Battle.Move.AlwaysCriticalHit : Battle.Move {
	public void CritialOverride(user, target) {return 1; }
}

//===============================================================================
// Until the end of the next round, the user's moves will always be critical hits.
// (Laser Focus)
//===============================================================================
public partial class Battle.Move.EnsureNextCriticalHit : Battle.Move {
	public bool canSnatch() { return true; }

	public void EffectGeneral(user) {
		user.effects.LaserFocus = 2;
		@battle.Display(_INTL("{1} concentrated intensely!", user.ToString()));
	}
}

//===============================================================================
// For 5 rounds, foes' attacks cannot become critical hits. (Lucky Chant)
//===============================================================================
public partial class Battle.Move.StartPreventCriticalHitsAgainstUserSide : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.LuckyChant > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.LuckyChant = 5;
		@battle.Display(_INTL("The Lucky Chant shielded {1} from critical hits!", user.Team(true)));
	}
}

//===============================================================================
// If target would be KO'd by this attack, it survives with 1HP instead.
// (False Swipe, Hold Back)
//===============================================================================
public partial class Battle.Move.CannotMakeTargetFaint : Battle.Move {
	public bool nonLethal(user, target) {  return true; }
}

//===============================================================================
// If user would be KO'd this round, it survives with 1HP instead. (Endure)
//===============================================================================
public partial class Battle.Move.UserEnduresFaintingThisTurn : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.Endure;
	}

	public void ProtectMessage(user) {
		@battle.Display(_INTL("{1} braced itself!", user.ToString()));
	}
}

//===============================================================================
// Weakens Electric attacks. (Mud Sport)
//===============================================================================
public partial class Battle.Move.StartWeakenElectricMoves : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (Settings.MECHANICS_GENERATION >= 6) {
			if (@battle.field.effects.MudSportField > 0) {
				@battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else if (@battle.allBattlers.any(b => b.effects.MudSport)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (Settings.MECHANICS_GENERATION >= 6) {
			@battle.field.effects.MudSportField = 5;
		} else {
			user.effects.MudSport = true;
		}
		@battle.Display(_INTL("Electricity's power was weakened!"));
	}
}

//===============================================================================
// Weakens Fire attacks. (Water Sport)
//===============================================================================
public partial class Battle.Move.StartWeakenFireMoves : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (Settings.MECHANICS_GENERATION >= 6) {
			if (@battle.field.effects.WaterSportField > 0) {
				@battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else if (@battle.allBattlers.any(b => b.effects.WaterSport)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (Settings.MECHANICS_GENERATION >= 6) {
			@battle.field.effects.WaterSportField = 5;
		} else {
			user.effects.WaterSport = true;
		}
		@battle.Display(_INTL("Fire's power was weakened!"));
	}
}

//===============================================================================
// For 5 rounds, lowers power of physical attacks against the user's side.
// (Reflect)
//===============================================================================
public partial class Battle.Move.StartWeakenPhysicalDamageAgainstUserSide : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.Reflect > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.Reflect = 5;
		if (user.hasActiveItem(Items.LIGHTCLAY)) user.OwnSide.effects.Reflect = 8;
		@battle.Display(_INTL("{1} raised {2}'s Defense!", @name, user.Team(true)));
	}
}

//===============================================================================
// For 5 rounds, lowers power of special attacks against the user's side. (Light Screen)
//===============================================================================
public partial class Battle.Move.StartWeakenSpecialDamageAgainstUserSide : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.LightScreen > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.LightScreen = 5;
		if (user.hasActiveItem(Items.LIGHTCLAY)) user.OwnSide.effects.LightScreen = 8;
		@battle.Display(_INTL("{1} raised {2}'s Special Defense!", @name, user.Team(true)));
	}
}

//===============================================================================
// For 5 rounds, lowers power of attacks against the user's side. Fails if
// weather is not hail. (Aurora Veil)
//===============================================================================
public partial class Battle.Move.StartWeakenDamageAgainstUserSideIfHail : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (!new []{:Hail, :Snowstorm}.Contains(user.effectiveWeather)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (user.OwnSide.effects.AuroraVeil > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.AuroraVeil = 5;
		if (user.hasActiveItem(Items.LIGHTCLAY)) user.OwnSide.effects.AuroraVeil = 8;
		@battle.Display(_INTL("{1} made {2} stronger against physical and special moves!",
														@name, user.Team(true)));
	}
}

//===============================================================================
// Ends the opposing side's Light Screen, Reflect and Aurora Break. (Brick Break,
// Psychic Fangs)
//===============================================================================
public partial class Battle.Move.RemoveScreens : Battle.Move {
	public bool ignoresReflect() { return true; }

	public void EffectGeneral(user) {
		if (user.OpposingSide.effects.LightScreen > 0) {
			user.OpposingSide.effects.LightScreen = 0;
			@battle.Display(_INTL("{1}'s Light Screen wore off!", user.OpposingTeam));
		}
		if (user.OpposingSide.effects.Reflect > 0) {
			user.OpposingSide.effects.Reflect = 0;
			@battle.Display(_INTL("{1}'s Reflect wore off!", user.OpposingTeam));
		}
		if (user.OpposingSide.effects.AuroraVeil > 0) {
			user.OpposingSide.effects.AuroraVeil = 0;
			@battle.Display(_INTL("{1}'s Aurora Veil wore off!", user.OpposingTeam));
		}
	}

	public void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if ((user.OpposingSide.effects.AuroraVeil > 0 ||
			user.OpposingSide.effects.LightScreen > 0 ||
			user.OpposingSide.effects.Reflect > 0) {
			hitNum = 1;   // Wall-breaking anim
		}
		super;
	}
}

//===============================================================================
// User is protected against moves with the "CanProtect" flag this round.
// (Detect, Protect)
//===============================================================================
public partial class Battle.Move.ProtectUser : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.Protect;
	}
}

//===============================================================================
// User is protected against moves with the "CanProtect" flag this round. If a
// Pokémon makes contact with the user while this effect applies, that Pokémon is
// poisoned. (Baneful Bunker)
//===============================================================================
public partial class Battle.Move.ProtectUserBanefulBunker : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.BanefulBunker;
	}
}

//===============================================================================
// User is protected against damaging moves this round. If a Pokémon makes
// contact with the user while this effect applies, that Pokémon is burned.
// (Burning Bulwark)
//===============================================================================
public partial class Battle.Move.ProtectUserFromDamagingMovesBurningBulwark : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.BurningBulwark;
	}
}

//===============================================================================
// User is protected against damaging moves this round. Decreases the Attack of
// the user of a stopped contact move by 2 stages. (King's Shield)
//===============================================================================
public partial class Battle.Move.ProtectUserFromDamagingMovesKingsShield : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.KingsShield;
	}
}

//===============================================================================
// For the rest of this round, the user avoids all damaging moves that would hit
// it. If a move that makes contact is stopped by this effect, decreases the
// Defense of the Pokémon using that move by 2 stages. Contributes to Protect's
// counter. (Obstruct)
//===============================================================================
public partial class Battle.Move.ProtectUserFromDamagingMovesObstruct : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.Obstruct;
	}
}

//===============================================================================
// For the rest of this round, the user avoids all damaging moves that would hit
// it. If a move that makes contact is stopped by this effect, decreases the
// Speed of the Pokémon using that move by 1 stage. Contributes to Protect's
// counter. (Silk Trap)
//===============================================================================
public partial class Battle.Move.ProtectUserFromDamagingMovesSilkTrap : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.SilkTrap;
	}
}

//===============================================================================
// User is protected against moves that target it this round. Damages the user of
// a stopped contact move by 1/8 of its max HP. (Spiky Shield)
//===============================================================================
public partial class Battle.Move.ProtectUserFromTargetingMovesSpikyShield : Battle.Move.ProtectMove {
	public override void initialize(battle, move) {
		base.initialize();
		@effect = Effects.SpikyShield;
	}
}

//===============================================================================
// This round, the user's side is unaffected by damaging moves. (Mat Block)
//===============================================================================
public partial class Battle.Move.ProtectUserSideFromDamagingMovesIfUserFirstTurn : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.turnCount > 1) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedLastInRound(user)) return true;
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.MatBlock = true;
		@battle.Display(_INTL("{1} intends to flip up a mat and block incoming attacks!", user.ToString()));
	}
}

//===============================================================================
// User's side is protected against status moves this round. (Crafty Shield)
//===============================================================================
public partial class Battle.Move.ProtectUserSideFromStatusMoves : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.CraftyShield) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedLastInRound(user)) return true;
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.CraftyShield = true;
		@battle.Display(_INTL("Crafty Shield protected {1}!", user.Team(true)));
	}
}

//===============================================================================
// User's side is protected against moves with priority greater than 0 this round.
// (Quick Guard)
//===============================================================================
public partial class Battle.Move.ProtectUserSideFromPriorityMoves : Battle.Move.ProtectMove {
	public bool canSnatch() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@effect      = Effects.QuickGuard;
		@sidedEffect = true;
	}
}

//===============================================================================
// User's side is protected against moves that target multiple battlers this round.
// (Wide Guard)
//===============================================================================
public partial class Battle.Move.ProtectUserSideFromMultiTargetDamagingMoves : Battle.Move.ProtectMove {
	public bool canSnatch() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@effect      = Effects.WideGuard;
		@sidedEffect = true;
	}
}

//===============================================================================
// Ends target's protections immediately. (Feint)
//===============================================================================
public partial class Battle.Move.RemoveProtections : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		target.effects.BanefulBunker          = false;
		target.effects.BurningBulwark         = false;
		target.effects.KingsShield            = false;
		target.effects.Obstruct               = false;
		target.effects.Protect                = false;
		target.effects.SilkTrap               = false;
		target.effects.SpikyShield            = false;
		target.OwnSide.effects.CraftyShield = false;
		target.OwnSide.effects.MatBlock     = false;
		target.OwnSide.effects.QuickGuard   = false;
		target.OwnSide.effects.WideGuard    = false;
	}
}

//===============================================================================
// Ends target's protections immediately. (Hyperspace Hole)
//===============================================================================
public partial class Battle.Move.RemoveProtectionsBypassSubstitute : Battle.Move.RemoveProtections {
	public bool ignoresSubstitute(user) {  return true; }
}

//===============================================================================
// Decreases the user's Defense by 1 stage. Ends target's protections
// immediately. (Hyperspace Fury)
//===============================================================================
public partial class Battle.Move.HoopaRemoveProtectionsBypassSubstituteLowerUserDef1 : Battle.Move.StatDownMove {
	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 1};
	}

	public bool MoveFailed(user, targets) {
		if (!user.isSpecies(Speciess.HOOPA)) {
			@battle.Display(_INTL("But {1} can't use the move!", user.ToString(true)));
			return true;
		} else if (user.form != 1) {
			@battle.Display(_INTL("But {1} can't use it the way it is now!", user.ToString(true)));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.BanefulBunker          = false;
		target.effects.BurningBulwark         = false;
		target.effects.KingsShield            = false;
		target.effects.Obstruct               = false;
		target.effects.Protect                = false;
		target.effects.SilkTrap               = false;
		target.effects.SpikyShield            = false;
		target.OwnSide.effects.CraftyShield = false;
		target.OwnSide.effects.MatBlock     = false;
		target.OwnSide.effects.QuickGuard   = false;
		target.OwnSide.effects.WideGuard    = false;
	}
}

//===============================================================================
// User takes recoil damage equal to 1/4 of the damage this move dealt.
//===============================================================================
public partial class Battle.Move.RecoilQuarterOfDamageDealt : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(target.damageState.totalHPLost / 4.0);
	}
}

//===============================================================================
// User takes recoil damage equal to 1/3 of the damage this move dealt.
//===============================================================================
public partial class Battle.Move.RecoilThirdOfDamageDealt : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(target.damageState.totalHPLost / 3.0);
	}
}

//===============================================================================
// User takes recoil damage equal to 1/3 of the damage this move dealt.
// May paralyze the target. (Volt Tackle)
//===============================================================================
public partial class Battle.Move.RecoilThirdOfDamageDealtParalyzeTarget : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(target.damageState.totalHPLost / 3.0);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanParalyze(user, false, self)) target.Paralyze(user);
	}
}

//===============================================================================
// User takes recoil damage equal to 1/3 of the damage this move dealt.
// May burn the target. (Flare Blitz)
//===============================================================================
public partial class Battle.Move.RecoilThirdOfDamageDealtBurnTarget : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(target.damageState.totalHPLost / 3.0);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanBurn(user, false, self)) target.Burn(user);
	}
}

//===============================================================================
// User takes recoil damage equal to 1/2 of the damage this move dealt.
// (Head Smash, Light of Ruin)
//===============================================================================
public partial class Battle.Move.RecoilHalfOfDamageDealt : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(target.damageState.totalHPLost / 2.0);
	}
}

//===============================================================================
// User takes recoil damage equal to 1/2 of is maximum HP. (Chloroblast)
//===============================================================================
public partial class Battle.Move.RecoilHalfOfTotalHP : Battle.Move.RecoilMove {
	public void RecoilDamage(user, target) {
		return (int)Math.Round(user.totalhp / 2.0);
	}
}

//===============================================================================
// Type effectiveness is multiplied by the Flying-type's effectiveness against
// the target. (Flying Press)
//===============================================================================
public partial class Battle.Move.EffectivenessIncludesFlyingType : Battle.Move {
	public override void CalcTypeModSingle(moveType, defType, user, target) {
		ret = base.CalcTypeModSingle();
		if (GameData.Types.exists(Types.FLYING)) {
			ret *= Effectiveness.calculate(:FLYING, defType);
		}
		return ret;
	}
}

//===============================================================================
// Poisons the target. This move becomes physical or special, whichever will deal
// more damage (only considers stats, stat stages and Wonder Room). Makes contact
// if (it is a physical move. Has a different animation depending on the move's) {
// category. (Shell Side Arm)
//===============================================================================
public partial class Battle.Move.CategoryDependsOnHigherDamagePoisonTarget : Battle.Move.PoisonTarget {
	public override void initialize(battle, move) {
		base.initialize();
		@calcCategory = 1;
	}

	public bool physicalMove(thisType = null) {  return (@calcCategory == 0); }
	public bool specialMove(thisType = null) {   return (@calcCategory == 1); }
	public bool contactMove() {                  return physicalMove();        }

	public void OnStartUse(user, targets) {
		target = targets[0];
		if (!target) return;
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stageMul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stageDiv = Battle.Battler.STAT_STAGE_DIVISORS;
		// Calculate user's effective attacking values
		attack_stage         = user.stages[:ATTACK] + max_stage;
		real_attack          = (int)Math.Floor(user.attack.to_f * stageMul[attack_stage] / stageDiv[attack_stage]);
		special_attack_stage = user.stages[:SPECIAL_ATTACK] + max_stage;
		real_special_attack  = (int)Math.Floor(user.spatk.to_f * stageMul[special_attack_stage] / stageDiv[special_attack_stage]);
		// Calculate target's effective defending values
		defense_stage         = target.stages[:DEFENSE] + max_stage;
		real_defense          = (int)Math.Floor(target.defense.to_f * stageMul[defense_stage] / stageDiv[defense_stage]);
		special_defense_stage = target.stages[:SPECIAL_DEFENSE] + max_stage;
		real_special_defense  = (int)Math.Floor(target.spdef.to_f * stageMul[special_defense_stage] / stageDiv[special_defense_stage]);
		// Perform simple damage calculation
		physical_damage = real_attack.to_f / real_defense;
		special_damage = real_special_attack.to_f / real_special_defense;
		// Determine move's category
		if (physical_damage == special_damage) {
			@calcCategory = (@battle.command_phase) ? rand(2) : @battle.Random(2);
		} else {
			@calcCategory = (physical_damage > special_damage) ? 0 : 1;
		}
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (physicalMove()) hitNum = 1;
		base.ShowAnimation();
	}
}

//===============================================================================
// Ignores all abilities that alter this move's success or damage. This move is
// physical if user's Attack is higher than its Special Attack (after applying
// stat stages), and special otherwise. (Photon Geyser)
//===============================================================================
public partial class Battle.Move.CategoryDependsOnHigherDamageIgnoreTargetAbility : Battle.Move.IgnoreTargetAbility {
	public override void initialize(battle, move) {
		base.initialize();
		@calcCategory = 1;
	}

	public bool physicalMove(thisType = null) {  return (@calcCategory == 0); }
	public bool specialMove(thisType = null) {   return (@calcCategory == 1); }

	public void OnStartUse(user, targets) {
		// Calculate user's effective attacking value
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stageMul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stageDiv = Battle.Battler.STAT_STAGE_DIVISORS;
		atk        = user.attack;
		atkStage   = user.stages[:ATTACK] + max_stage;
		realAtk    = (int)Math.Floor(atk.to_f * stageMul[atkStage] / stageDiv[atkStage]);
		spAtk      = user.spatk;
		spAtkStage = user.stages[:SPECIAL_ATTACK] + max_stage;
		realSpAtk  = (int)Math.Floor(spAtk.to_f * stageMul[spAtkStage] / stageDiv[spAtkStage]);
		// Determine move's category
		@calcCategory = (realAtk > realSpAtk) ? 0 : 1;
	}
}

//===============================================================================
// The user's Defense (and its Defense stat stages) are used instead of the
// user's Attack (and Attack stat stages) to calculate damage. All other effects
// are applied normally, applying the user's Attack modifiers and not the user's
// Defence modifiers. (Body Press)
//===============================================================================
public partial class Battle.Move.UseUserDefenseInsteadOfUserAttack : Battle.Move {
	public void GetAttackStats(user, target) {
		return user.defense, user.stages[:DEFENSE] + Battle.Battler.STAT_STAGE_MAXIMUM;
	}
}

//===============================================================================
// Target's Attack is used instead of user's Attack for this move's calculations.
// (Foul Play)
//===============================================================================
public partial class Battle.Move.UseTargetAttackInsteadOfUserAttack : Battle.Move {
	public void GetAttackStats(user, target) {
		if (specialMove()) return target.spatk, target.stages[:SPECIAL_ATTACK] + Battle.Battler.STAT_STAGE_MAXIMUM;
		return target.attack, target.stages[:ATTACK] + Battle.Battler.STAT_STAGE_MAXIMUM;
	}
}

//===============================================================================
// Target's Defense is used instead of its Special Defense for this move's
// calculations. (Psyshock, Psystrike, Secret Sword)
//===============================================================================
public partial class Battle.Move.UseTargetDefenseInsteadOfTargetSpDef : Battle.Move {
	public void GetDefenseStats(user, target) {
		return target.defense, target.stages[:DEFENSE] + Battle.Battler.STAT_STAGE_MAXIMUM;
	}
}

//===============================================================================
// User's attack next round against the target will definitely hit.
// (Lock-On, Mind Reader)
//===============================================================================
public partial class Battle.Move.EnsureNextMoveAlwaysHits : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		user.effects.LockOn    = 2;
		user.effects.LockOnPos = target.index;
		@battle.Display(_INTL("{1} took aim at {2}!", user.ToString(), target.ToString(true)));
	}
}

//===============================================================================
// Target's evasion stat changes are ignored from now on. (Foresight, Odor Sleuth)
// Normal and Fighting moves have normal effectiveness against the Ghost-type target.
//===============================================================================
public partial class Battle.Move.StartNegateTargetEvasionStatStageAndGhostImmunity : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public void EffectAgainstTarget(user, target) {
		target.effects.Foresight = true;
		@battle.Display(_INTL("{1} was identified!", target.ToString()));
	}
}

//===============================================================================
// Target's evasion stat changes are ignored from now on. (Miracle Eye)
// Psychic moves have normal effectiveness against the Dark-type target.
//===============================================================================
public partial class Battle.Move.StartNegateTargetEvasionStatStageAndDarkImmunity : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public void EffectAgainstTarget(user, target) {
		target.effects.MiracleEye = true;
		@battle.Display(_INTL("{1} was identified!", target.ToString()));
	}
}

//===============================================================================
// This move ignores target's Defense, Special Defense and evasion stat changes.
// (Chip Away, Darkest Lariat, Sacred Sword)
//===============================================================================
public partial class Battle.Move.IgnoreTargetDefSpDefEvaStatStages : Battle.Move {
	public override void CalcAccuracyModifiers(user, target, modifiers) {
		base.CalcAccuracyModifiers();
		modifiers.evasion_stage = 0;
	}

	public override void GetDefenseStats(user, target) {
		ret1, _ret2 = base.GetDefenseStats();
		return ret1, Battle.Battler.STAT_STAGE_MAXIMUM;   // Def/SpDef stat stage
	}
}

//===============================================================================
// This move's type is the same as the user's first type. (Revelation Dance)
//===============================================================================
public partial class Battle.Move.TypeIsUserFirstType : Battle.Move {
	public void BaseType(user) {
		userTypes = user.Types(true);
		return userTypes[0] || @type;
	}
}

//===============================================================================
// This move's type is the same as the user's second type, only if the user is
// Ogerpon. (Ivy Cudgel)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserOgerponForm : Battle.Move {
	public void BaseType(user) {
		if (user.isSpecies(Speciess.OGERPON)) {
			switch (user.form) {
				case 1:
					if (GameData.Types.exists(Types.WATER)) return :WATER;
					break;
				case 2:
					if (GameData.Types.exists(Types.FIRE)) return :FIRE;
					break;
				case 3:
					if (GameData.Types.exists(Types.ROCK)) return :ROCK;
					break;
			}
		}
		return @type;
	}
}

//===============================================================================
// This move's type is the same as the user's second type, only if the user is
// Ogerpon. (Ivy Cudgel)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserTaurosFormRemoveScreens : Battle.Move.RemoveScreens {
	public void BaseType(user) {
		if (user.isSpecies(Speciess.TAUROS)) {
			switch (user.form) {
				case 1:
					if (GameData.Types.exists(Types.WATER)) return :FIGHTING;
					break;
				case 2:
					if (GameData.Types.exists(Types.FIRE)) return :FIRE;
					break;
				case 3:
					if (GameData.Types.exists(Types.ROCK)) return :WATER;
					break;
			}
		}
		return @type;
	}
}

//===============================================================================
// Power and type depends on the user's IVs. (Hidden Power)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserIVs : Battle.Move {
	public void BaseType(user) {
		hp = HiddenPower(user.pokemon);
		return hp[0];
	}

	public override void BaseDamage(baseDmg, user, target) {
		if (Settings.MECHANICS_GENERATION >= 6) return base.BaseDamage();
		hp = HiddenPower(user.pokemon);
		return hp[1];
	}
}

// NOTE: This allows Hidden Power to be Fairy-type (if you have that type in your
//       game). I don't care that the official games don't work like that.
public void HiddenPower(pkmn) {
	iv = pkmn.iv;
	idxType = 0;
	power = 60;
	types = new List<string>();
	foreach (var t in GameData.Type) { //'GameData.Type.each' do => |t|
		types[t.icon_position] ||= new List<string>();
		if (!t.pseudo_type && !new []{:NORMAL, :SHADOW}.Contains(t.id)) types[t.icon_position].Add(t.id);
	}
	types.flatten!.compact!;
	idxType |= (iv[:HP] & 1);
	idxType |= (iv[:ATTACK] & 1) << 1;
	idxType |= (iv[:DEFENSE] & 1) << 2;
	idxType |= (iv[:SPEED] & 1) << 3;
	idxType |= (iv[:SPECIAL_ATTACK] & 1) << 4;
	idxType |= (iv[:SPECIAL_DEFENSE] & 1) << 5;
	idxType = (types.length - 1) * idxType / 63;
	type = types[idxType];
	if (Settings.MECHANICS_GENERATION <= 5) {
		powerMin = 30;
		powerMax = 70;
		power |= (iv[:HP] & 2) >> 1;
		power |= (iv[:ATTACK] & 2);
		power |= (iv[:DEFENSE] & 2) << 1;
		power |= (iv[:SPEED] & 2) << 2;
		power |= (iv[:SPECIAL_ATTACK] & 2) << 3;
		power |= (iv[:SPECIAL_DEFENSE] & 2) << 4;
		power = powerMin + ((powerMax - powerMin) * power / 63);
	}
	return new {type, power};
}

//===============================================================================
// Power and type depend on the user's held berry. Destroys the berry.
// (Natural Gift)
//===============================================================================
public partial class Battle.Move.TypeAndPowerDependOnUserBerry : Battle.Move {
	public bool MoveFailed(user, targets) {
		// NOTE: Unnerve does not stop a Pokémon using this move.
		item = user.item;
		if ((!item || !item.is_berry() || !user.itemActive() ||
			item.flags.none(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^NaturalGift_",RegexOptions.IgnoreCase))) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void BaseType(user) {
		ret = :NORMAL;
		item = user.item;
		if (item) {
			foreach (var flag in item.flags) { //'item.flags.each' do => |flag|
				if (!System.Text.RegularExpressions.Regex.IsMatch(flag,@"^NaturalGift_(\w+)_(?:\d+)$",RegexOptions.IgnoreCase)) continue;
				typ = $~[1].to_sym;
				if (GameData.Type.exists(typ)) ret = typ;
				break;
			}
		}
		return ret;
	}

	public void BaseDamage(baseDmg, user, target) {
		if (user.item.id) {
			foreach (var flag in GameData.Item.get(user.item.id).flags) { //'GameData.Item.get(user.item.id).flags.each' do => |flag|
				if (System.Text.RegularExpressions.Regex.IsMatch(flag,@"^NaturalGift_(?:\w+)_(\d+)$",RegexOptions.IgnoreCase)) return (int)Math.Max($~[1].ToInt(), 10);
			}
		}
		return 1;
	}

	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		// NOTE: The item is consumed even if this move was Protected against or it
		//       missed. The item is not consumed if the target was switched out by
		//       an effect like a target's Red Card.
		// NOTE: There is no item consumption animation.
		if (user.item) user.ConsumeItem(true, true, false);
	}
}

//===============================================================================
// Type depends on the user's held Plate. (Judgment)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserPlate : Battle.Move {
	public override void initialize(battle, move) {
		base.initialize();
		@itemTypes = {
			FISTPLATE   = :FIGHTING,
			SKYPLATE    = :FLYING,
			TOXICPLATE  = :POISON,
			EARTHPLATE  = :GROUND,
			STONEPLATE  = :ROCK,
			INSECTPLATE = :BUG,
			SPOOKYPLATE = :GHOST,
			IRONPLATE   = :STEEL,
			FLAMEPLATE  = :FIRE,
			SPLASHPLATE = :WATER,
			MEADOWPLATE = :GRASS,
			ZAPPLATE    = :ELECTRIC,
			MINDPLATE   = :PSYCHIC,
			ICICLEPLATE = :ICE,
			DRACOPLATE  = :DRAGON,
			DREADPLATE  = :DARK,
			PIXIEPLATE  = :FAIRY;
		}
	}

	public void BaseType(user) {
		ret = :NORMAL;
		if (user.item_id && user.itemActive()) {
			typ = @itemTypes[user.item_id];
			if (typ && GameData.Type.exists(typ)) ret = typ;
		}
		return ret;
	}
}

//===============================================================================
// Type depends on the user's held Memory. (Multi-Attack)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserMemory : Battle.Move {
	public override void initialize(battle, move) {
		base.initialize();
		@itemTypes = {
			FIGHTINGMEMORY = :FIGHTING,
			FLYINGMEMORY   = :FLYING,
			POISONMEMORY   = :POISON,
			GROUNDMEMORY   = :GROUND,
			ROCKMEMORY     = :ROCK,
			BUGMEMORY      = :BUG,
			GHOSTMEMORY    = :GHOST,
			STEELMEMORY    = :STEEL,
			FIREMEMORY     = :FIRE,
			WATERMEMORY    = :WATER,
			GRASSMEMORY    = :GRASS,
			ELECTRICMEMORY = :ELECTRIC,
			PSYCHICMEMORY  = :PSYCHIC,
			ICEMEMORY      = :ICE,
			DRAGONMEMORY   = :DRAGON,
			DARKMEMORY     = :DARK,
			FAIRYMEMORY    = :FAIRY;
		}
	}

	public void BaseType(user) {
		ret = :NORMAL;
		if (user.item_id && user.itemActive()) {
			typ = @itemTypes[user.item_id];
			if (typ && GameData.Type.exists(typ)) ret = typ;
		}
		return ret;
	}
}

//===============================================================================
// Type depends on the user's held Drive. (Techno Blast)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserDrive : Battle.Move {
	public override void initialize(battle, move) {
		base.initialize();
		@itemTypes = {
			SHOCKDRIVE = :ELECTRIC,
			BURNDRIVE  = :FIRE,
			CHILLDRIVE = :ICE,
			DOUSEDRIVE = :WATER;
		}
	}

	public void BaseType(user) {
		ret = :NORMAL;
		if (user.item_id && user.itemActive()) {
			typ = @itemTypes[user.item_id];
			if (typ && GameData.Type.exists(typ)) ret = typ;
		}
		return ret;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		t = BaseType(user);
		hitNum = 0;
		if (t == :ELECTRIC) hitNum = 1;
		if (t == :FIRE) hitNum = 2;
		if (t == :ICE) hitNum = 3;
		if (t == :WATER) hitNum = 4;
		base.ShowAnimation();
	}
}

//===============================================================================
// Increases the user's Speed by 1 stage. This move's type depends on the user's
// form (Electric if Full Belly, Dark if Hangry). Fails if the user is not
// Morpeko (works if transformed into Morpeko). (Aura Wheel)
//===============================================================================
public partial class Battle.Move.TypeDependsOnUserMorpekoFormRaiseUserSpeed1 : Battle.Move.RaiseUserSpeed1 {
	public bool MoveFailed(user, targets) {
		if (!user.isSpecies(Speciess.MORPEKO) && user.effects.TransformSpecies != :MORPEKO) {
			@battle.Display(_INTL("But {1} can't use the move!", user.ToString()));
			return true;
		}
		return false;
	}

	public void BaseType(user) {
		if (user.form == 1 && GameData.Types.exists(Types.DARK)) return :DARK;
		return @type;
	}
}

//===============================================================================
// Power is doubled in weather. Type changes depending on the weather. (Weather Ball)
//===============================================================================
public partial class Battle.Move.TypeAndPowerDependOnWeather : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (user.effectiveWeather != Weathers.None) baseDmg *= 2;
		return baseDmg;
	}

	public void BaseType(user) {
		ret = :NORMAL;
		switch (user.effectiveWeather) {
			case :Sun: case :HarshSun:
				if (GameData.Types.exists(Types.FIRE)) ret = :FIRE;
				break;
			case :Rain: case :HeavyRain:
				if (GameData.Types.exists(Types.WATER)) ret = :WATER;
				break;
			case :Sandstorm:
				if (GameData.Types.exists(Types.ROCK)) ret = :ROCK;
				break;
			case :Hail: case :Snowstorm:
				if (GameData.Types.exists(Types.ICE)) ret = :ICE;
				break;
			case :ShadowSky:
				ret = :NONE;
				break;
		}
		return ret;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		t = BaseType(user);
		if (t == :FIRE) hitNum = 1;   // Type-specific anims
		if (t == :WATER) hitNum = 2;
		if (t == :ROCK) hitNum = 3;
		if (t == :ICE) hitNum = 4;
		base.ShowAnimation();
	}
}

//===============================================================================
// Power is doubled if a terrain applies and user is grounded; also, this move's
// type and animation depends on the terrain. (Terrain Pulse)
//===============================================================================
public partial class Battle.Move.TypeAndPowerDependOnTerrain : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.terrain != :None && user.affectedByTerrain()) baseDmg *= 2;
		return baseDmg;
	}

	public void BaseType(user) {
		ret = :NORMAL;
		switch (@battle.field.terrain) {
			case :Electric:
				if (GameData.Types.exists(Types.ELECTRIC)) ret = :ELECTRIC;
				break;
			case :Grassy:
				if (GameData.Types.exists(Types.GRASS)) ret = :GRASS;
				break;
			case :Misty:
				if (GameData.Types.exists(Types.FAIRY)) ret = :FAIRY;
				break;
			case :Psychic:
				if (GameData.Types.exists(Types.PSYCHIC)) ret = :PSYCHIC;
				break;
		}
		return ret;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		t = BaseType(user);
		if (t == :ELECTRIC) hitNum = 1;   // Type-specific anims
		if (t == :GRASS) hitNum = 2;
		if (t == :FAIRY) hitNum = 3;
		if (t == :PSYCHIC) hitNum = 4;
		base.ShowAnimation();
	}
}

//===============================================================================
// Target's moves become Electric-type for the rest of the round. (Electrify)
//===============================================================================
public partial class Battle.Move.TargetMovesBecomeElectric : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Electrify) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedTargetAlreadyMoved(target, show_message)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Electrify = true;
		@battle.Display(_INTL("{1}'s moves have been electrified!", target.ToString()));
	}
}

//===============================================================================
// All Normal-type moves become Electric-type for the rest of the round.
// (Ion Deluge, Plasma Fists)
//===============================================================================
public partial class Battle.Move.NormalMovesBecomeElectric : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		if (@battle.field.effects.IonDeluge) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedLastInRound(user)) return true;
		return false;
	}

	public void EffectGeneral(user) {
		if (@battle.field.effects.IonDeluge) return;
		@battle.field.effects.IonDeluge = true;
		@battle.Display(_INTL("A deluge of ions showers the battlefield!"));
	}
}
