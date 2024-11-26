//===============================================================================
// Puts the target to sleep.
//===============================================================================
public partial class Battle.Move.SleepTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanSleep(user, show_message, self);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Sleep(user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanSleep(user, false, self)) target.Sleep(user);
	}
}

//===============================================================================
// Puts the target to sleep. Fails if user is not Darkrai. (Dark Void (Gen 7+))
//===============================================================================
public partial class Battle.Move.SleepTargetIfUserDarkrai : Battle.Move.SleepTarget {
	public bool MoveFailed(user, targets) {
		if (!user.isSpecies(Speciess.DARKRAI) && user.effects.TransformSpecies != :DARKRAI) {
			@battle.Display(_INTL("But {1} can't use the move!", user.ToString()));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Puts the target to sleep. Changes the user's form if the user is Meloetta.
// (Relic Song)
//===============================================================================
public partial class Battle.Move.SleepTargetChangeUserMeloettaForm : Battle.Move.SleepTarget {
	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		if (numHits == 0) return;
		if (user.fainted() || user.effects.Transform) return;
		if (!user.isSpecies(Speciess.MELOETTA)) return;
		if (user.hasActiveAbility(Abilitys.SHEERFORCE) && @addlEffect > 0) return;
		newForm = (user.form + 1) % 2;
		user.ChangeForm(newForm, _INTL("{1} transformed!", user.ToString()));
	}
}

//===============================================================================
// Makes the target drowsy; it falls asleep at the end of the next turn. (Yawn)
//===============================================================================
public partial class Battle.Move.SleepTargetNextTurn : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Yawn > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (!target.CanSleep(user, true, self)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Yawn = 2;
		@battle.Display(_INTL("{1} made {2} drowsy!", user.ToString(), target.ToString(true)));
	}
}

//===============================================================================
// Poisons the target.
//===============================================================================
public partial class Battle.Move.PoisonTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@toxic = false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanPoison(user, show_message, self);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Poison(user, null, @toxic);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanPoison(user, false, self)) target.Poison(user, null, @toxic);
	}
}

//===============================================================================
// Poisons the target and decreases its Speed by 1 stage. (Toxic Thread)
//===============================================================================
public partial class Battle.Move.PoisonTargetLowerTargetSpeed1 : Battle.Move {
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 1};
	}

	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.CanPoison(user, false, self) &&
			!target.CanLowerStatStage(@statDown[0], user, self)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (target.CanPoison(user, false, self)) target.Poison(user);
		if (target.CanLowerStatStage(@statDown[0], user, self)) {
			target.LowerStatStage(@statDown[0], @statDown[1], user);
		}
	}
}

//===============================================================================
// Removes trapping moves, entry hazards and Leech Seed on user/user's side.
// Poisons the target. (Mortal Spin)
//===============================================================================
public partial class Battle.Move.PoisonTargetRemoveUserBindingAndEntryHazards : Battle.Move.PoisonTarget {
	public void EffectAfterAllHits(user, target) {
		if (user.fainted() || target.damageState.unaffected) return;
		if (user.effects.Trapping > 0) {
			trapMove = GameData.Move.get(user.effects.TrappingMove).name;
			trapUser = @battle.battlers[user.effects.TrappingUser];
			@battle.Display(_INTL("{1} got free of {2}'s {3}!", user.ToString(), trapUser.ToString(true), trapMove));
			user.effects.Trapping     = 0;
			user.effects.TrappingMove = null;
			user.effects.TrappingUser = -1;
		}
		if (user.effects.LeechSeed >= 0) {
			user.effects.LeechSeed = -1;
			@battle.Display(_INTL("{1} shed Leech Seed!", user.ToString()));
		}
		if (user.OwnSide.effects.StealthRock) {
			user.OwnSide.effects.StealthRock = false;
			@battle.Display(_INTL("{1} blew away stealth rocks!", user.ToString()));
		}
		if (user.OwnSide.effects.Spikes > 0) {
			user.OwnSide.effects.Spikes = 0;
			@battle.Display(_INTL("{1} blew away spikes!", user.ToString()));
		}
		if (user.OwnSide.effects.ToxicSpikes > 0) {
			user.OwnSide.effects.ToxicSpikes = 0;
			@battle.Display(_INTL("{1} blew away poison spikes!", user.ToString()));
		}
		if (user.OwnSide.effects.StickyWeb) {
			user.OwnSide.effects.StickyWeb = false;
			@battle.Display(_INTL("{1} blew away sticky webs!", user.ToString()));
		}
	}
}

//===============================================================================
// Badly poisons the target. (Poison Fang, Toxic)
//===============================================================================
public partial class Battle.Move.BadPoisonTarget : Battle.Move.PoisonTarget {
	public override void initialize(battle, move) {
		base.initialize();
		@toxic = true;
	}

	public void OverrideSuccessCheckPerHit(user, target) {
		return (Settings.MORE_TYPE_EFFECTS && statusMove() && user.Type == Types.POISON);
	}
}

//===============================================================================
// Paralyzes the target.
//===============================================================================
public partial class Battle.Move.ParalyzeTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanParalyze(user, show_message, self);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Paralyze(user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanParalyze(user, false, self)) target.Paralyze(user);
	}
}

//===============================================================================
// Paralyzes the target. Doesn't affect target if move's type has no effect on
// it. (Thunder Wave)
//===============================================================================
public partial class Battle.Move.ParalyzeTargetIfNotTypeImmune : Battle.Move.ParalyzeTarget {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (Effectiveness.ineffective(target.damageState.typeMod)) {
			if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			return true;
		}
		return super;
	}
}

//===============================================================================
// Paralyzes the target. Accuracy perfect in rain. (Wildbolt Storm)
//===============================================================================
public partial class Battle.Move.ParalyzeTargetAlwaysHitsInRain : Battle.Move.ParalyzeTarget {
	public void BaseAccuracy(user, target) {
		if (new []{:Rain, :HeavyRain}.Contains(target.effectiveWeather)) return 0;
		return super;
	}
}

//===============================================================================
// Paralyzes the target. Accuracy perfect in rain, 50% in sunshine. Hits some
// semi-invulnerable targets. (Thunder)
//===============================================================================
public partial class Battle.Move.ParalyzeTargetAlwaysHitsInRainHitsTargetInSky : Battle.Move.ParalyzeTarget {
	public bool hitsFlyingTargets() { return true; }

	public void BaseAccuracy(user, target) {
		switch (target.effectiveWeather) {
			case :Sun: case :HarshSun:
				return 50;
			case :Rain: case :HeavyRain:
				return 0;
		}
		return super;
	}
}

//===============================================================================
// Paralyzes the target. May cause the target to flinch. (Thunder Fang)
//===============================================================================
public partial class Battle.Move.ParalyzeFlinchTarget : Battle.Move {
	public bool flinchingMove() { return true; }

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		chance = AdditionalEffectChance(user, target, 10);
		if (chance == 0) return;
		if (target.CanParalyze(user, false, self) && @battle.Random(100) < chance) {
			target.Paralyze(user);
		}
		if (@battle.Random(100) < chance) target.Flinch(user);
	}
}

//===============================================================================
// Burns the target.
//===============================================================================
public partial class Battle.Move.BurnTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanBurn(user, show_message, self);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Burn(user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanBurn(user, false, self)) target.Burn(user);
	}
}

//===============================================================================
// Burns the target. Accuracy perfect in rain. (Sandsear Storm)
//===============================================================================
public partial class Battle.Move.BurnTargetAlwaysHitsInRain : Battle.Move.BurnTarget {
	public void BaseAccuracy(user, target) {
		if (new []{:Rain, :HeavyRain}.Contains(target.effectiveWeather)) return 0;
		return super;
	}
}

//===============================================================================
// Burns the target if any of its stats were increased this round.
// (Burning Jealousy)
//===============================================================================
public partial class Battle.Move.BurnTargetIfTargetStatsRaisedThisTurn : Battle.Move.BurnTarget {
	public override void AdditionalEffect(user, target) {
		if (target.statsRaisedThisRound) base.AdditionalEffect();
	}
}

//===============================================================================
// Burns the target. May cause the target to flinch. (Fire Fang)
//===============================================================================
public partial class Battle.Move.BurnFlinchTarget : Battle.Move {
	public bool flinchingMove() { return true; }

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		chance = AdditionalEffectChance(user, target, 10);
		if (chance == 0) return;
		if (target.CanBurn(user, false, self) && @battle.Random(100) < chance) {
			target.Burn(user);
		}
		if (@battle.Random(100) < chance) target.Flinch(user);
	}
}

//===============================================================================
// Freezes the target.
//===============================================================================
public partial class Battle.Move.FreezeTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanFreeze(user, show_message, self);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Freeze(user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanFreeze(user, false, self)) target.Freeze(user);
	}
}

//===============================================================================
// Freezes the target. Effectiveness against Water-type is 2x. (Freeze-Dry)
//===============================================================================
public partial class Battle.Move.FreezeTargetSuperEffectiveAgainstWater : Battle.Move.FreezeTarget {
	public override void CalcTypeModSingle(moveType, defType, user, target) {
		if (defType == Types.WATER) return Effectiveness.SUPER_EFFECTIVE_MULTIPLIER;
		return base.CalcTypeModSingle();
	}
}

//===============================================================================
// Freezes the target. Accuracy perfect in hail. (Blizzard)
//===============================================================================
public partial class Battle.Move.FreezeTargetAlwaysHitsInHail : Battle.Move.FreezeTarget {
	public void BaseAccuracy(user, target) {
		if (new []{:Hail, :Snowstorm}.Contains(target.effectiveWeather)) return 0;
		return super;
	}
}

//===============================================================================
// Freezes the target. May cause the target to flinch. (Ice Fang)
//===============================================================================
public partial class Battle.Move.FreezeFlinchTarget : Battle.Move {
	public bool flinchingMove() { return true; }

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		chance = AdditionalEffectChance(user, target, 10);
		if (chance == 0) return;
		if (target.CanFreeze(user, false, self) && @battle.Random(100) < chance) {
			target.Freeze(user);
		}
		if (@battle.Random(100) < chance) target.Flinch(user);
	}
}

//===============================================================================
// Burns, freezes or paralyzes the target. (Tri Attack)
//===============================================================================
public partial class Battle.Move.ParalyzeBurnOrFreezeTarget : Battle.Move {
	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		switch (@battle.Random(3)) {
		if (target.CanBurn(user, false, self)) when 0 then target.Burn(user);
		if (target.CanFreeze(user, false, self)) when 1 then target.Freeze(user);
		if (target.CanParalyze(user, false, self)) when 2 then target.Paralyze(user);
		}
	}
}

//===============================================================================
// Poisons, paralyzes or puts to sleep the target. (Dire Claw)
//===============================================================================
public partial class Battle.Move.PoisonParalyzeOrSleepTarget : Battle.Move {
	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		switch (@battle.Random(3)) {
		if (target.CanPoison(user, false, self)) when 0 then target.Poison(user);
		if (target.CanParalyze(user, false, self)) when 1 then target.Paralyze(user);
		if (target.CanSleep(user, false, self)) when 2 then target.Sleep(user);
		}
	}
}

//===============================================================================
// User passes its status problem to the target. (Psycho Shift)
//===============================================================================
public partial class Battle.Move.GiveUserStatusToTarget : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.status == statuses.NONE) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.CanInflictStatus(user.status, user, false, self)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		msg = "";
		switch (user.status) {
			case :SLEEP:
				target.Sleep(user);
				msg = _INTL("{1} woke up.", user.ToString());
				break;
			case :POISON:
				target.Poison(user, null, user.statusCount != 0);
				msg = _INTL("{1} was cured of its poisoning.", user.ToString());
				break;
			case :BURN:
				target.Burn(user);
				msg = _INTL("{1}'s burn was healed.", user.ToString());
				break;
			case :PARALYSIS:
				target.Paralyze(user);
				msg = _INTL("{1} was cured of paralysis.", user.ToString());
				break;
			case :FROZEN:
				target.Freeze(user);
				msg = _INTL("{1} was thawed out.", user.ToString());
				break;
		}
		if (msg != "") {
			user.CureStatus(false);
			@battle.Display(msg);
		}
	}
}

//===============================================================================
// Cures user of burn, poison and paralysis. (Refresh)
//===============================================================================
public partial class Battle.Move.CureUserBurnPoisonParalysis : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (!new []{:BURN, :POISON, :PARALYSIS}.Contains(user.status)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		old_status = user.status;
		user.CureStatus(false);
		switch (old_status) {
			case :BURN:
				@battle.Display(_INTL("{1} healed its burn!", user.ToString()));
				break;
			case :POISON:
				@battle.Display(_INTL("{1} cured its poisoning!", user.ToString()));
				break;
			case :PARALYSIS:
				@battle.Display(_INTL("{1} cured its paralysis!", user.ToString()));
				break;
		}
	}
}

//===============================================================================
// Cures all party Pokémon of permanent status problems. (Aromatherapy, Heal Bell)
//===============================================================================
// NOTE: In Gen 5, this move should have a target of UserSide, while in Gen 6+ it
//       should have a target of UserAndAllies. This is because, in Gen 5, this
//       move shouldn't call def SuccessCheckAgainstTarget for each Pokémon
//       currently in battle that will be affected by this move (i.e. allies
//       aren't protected by their substitute/ability/etc., but they are in Gen
//       6+). We achieve this by not targeting any battlers in Gen 5, since
//       SuccessCheckAgainstTarget is only called for targeted battlers.
public partial class Battle.Move.CureUserPartyStatus : Battle.Move {
	public bool canSnatch() {          return true; }
	public bool worksWithNoTargets() { return true; }

	public bool MoveFailed(user, targets) {
		has_effect = @battle.allSameSideBattlers(user).any(b => b.status != statuses.NONE);
		if (!has_effect) {
			has_effect = @battle.Party(user.index).any(pkmn => pkmn&.able() && pkmn.status != statuses.NONE);
		}
		if (!has_effect) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		return target.status == statuses.NONE;
	}

	public void AromatherapyHeal(pkmn, battler = null) {
		oldStatus = (battler) ? battler.status : pkmn.status;
		curedName = (battler) ? battler.ToString() : pkmn.name;
		if (battler) {
			battler.CureStatus(false);
		} else {
			pkmn.status      = :NONE;
			pkmn.statusCount = 0;
		}
		switch (oldStatus) {
			case :SLEEP:
				@battle.Display(_INTL("{1} was woken from sleep.", curedName));
				break;
			case :POISON:
				@battle.Display(_INTL("{1} was cured of its poisoning.", curedName));
				break;
			case :BURN:
				@battle.Display(_INTL("{1}'s burn was healed.", curedName));
				break;
			case :PARALYSIS:
				@battle.Display(_INTL("{1} was cured of paralysis.", curedName));
				break;
			case :FROZEN:
				@battle.Display(_INTL("{1} was thawed out.", curedName));
				break;
		}
	}

	public void EffectAgainstTarget(user, target) {
		// Cure all Pokémon in battle on the user's side.
		AromatherapyHeal(target.pokemon, target);
	}

	public void EffectGeneral(user) {
		// Cure all Pokémon in battle on the user's side. For the benefit of the Gen
		// 5 version of this move, to make Pokémon out in battle get cured first.
		if (Target(user) == :UserSide) {
			@battle.allSameSideBattlers(user).each do |b|
				if (b.status != statuses.NONE) AromatherapyHeal(b.pokemon, b);
			}
		}
		// Cure all Pokémon in the user's and partner trainer's party.
		// NOTE: This intentionally affects the partner trainer's inactive Pokémon
		//       too.
		@battle.Party(user.index).each_with_index do |pkmn, i|
			if (!pkmn || !pkmn.able() || pkmn.status == statuses.NONE) continue;
			if (@battle.FindBattler(i, user)) continue;   // Skip Pokémon in battle
			AromatherapyHeal(pkmn);
		}
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		base.ShowAnimation();
		switch (@id) {
			case :AROMATHERAPY:
				@battle.Display(_INTL("A soothing aroma wafted through the area!"));
				break;
			case :HEALBELL:
				@battle.Display(_INTL("A bell chimed!"));
				break;
		}
	}
}

//===============================================================================
// Cures the target's burn. (Sparkling Aria)
//===============================================================================
public partial class Battle.Move.CureTargetBurn : Battle.Move {
	public void AdditionalEffect(user, target) {
		if (target.fainted() || target.damageState.substitute) return;
		if (target.status != statuses.BURN) return;
		target.CureStatus;
	}
}

//===============================================================================
// Safeguards the user's side from being inflicted with status problems.
// (Safeguard)
//===============================================================================
public partial class Battle.Move.StartUserSideImmunityToInflictedStatus : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.Safeguard > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.Safeguard = 5;
		@battle.Display(_INTL("{1} became cloaked in a mystical veil!", user.Team));
	}
}

//===============================================================================
// Causes the target to flinch.
//===============================================================================
public partial class Battle.Move.FlinchTarget : Battle.Move {
	public bool flinchingMove() { return true; }

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Flinch(user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		target.Flinch(user);
	}
}

//===============================================================================
// Causes the target to flinch. Fails if the user is not asleep. (Snore)
//===============================================================================
public partial class Battle.Move.FlinchTargetFailsIfUserNotAsleep : Battle.Move.FlinchTarget {
	public bool usableWhenAsleep() { return true; }

	public bool MoveFailed(user, targets) {
		if (!user.asleep()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Causes the target to flinch. Fails if this isn't the user's first turn.
// (Fake Out)
//===============================================================================
public partial class Battle.Move.FlinchTargetFailsIfNotUserFirstTurn : Battle.Move.FlinchTarget {
	public bool MoveFailed(user, targets) {
		if (user.turnCount > 1) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Power is doubled if the target is using Bounce, Fly or Sky Drop. Hits some
// semi-invulnerable targets. May make the target flinch. (Twister)
//===============================================================================
public partial class Battle.Move.FlinchTargetDoublePowerIfTargetInSky : Battle.Move.FlinchTarget {
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
// Confuses the target.
//===============================================================================
public partial class Battle.Move.ConfuseTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanConfuse(user, show_message, self);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Confuse;
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (!target.CanConfuse(user, false, self)) return;
		target.Confuse;
	}
}

//===============================================================================
// Confuses the target. Accuracy perfect in rain, 50% in sunshine. Hits some
// semi-invulnerable targets. (Hurricane)
//===============================================================================
public partial class Battle.Move.ConfuseTargetAlwaysHitsInRainHitsTargetInSky : Battle.Move.ConfuseTarget {
	public bool hitsFlyingTargets() { return true; }

	public void BaseAccuracy(user, target) {
		switch (target.effectiveWeather) {
			case :Sun: case :HarshSun:
				return 50;
			case :Rain: case :HeavyRain:
				return 0;
		}
		return super;
	}
}

//===============================================================================
// Confuses the target. If attack misses, user takes crash damage of 1/2 of max
// HP. (Axe Kick)
//===============================================================================
public partial class Battle.Move.ConfuseTargetCrashDamageIfFails : Battle.Move.ConfuseTarget {
	public bool recoilMove() { return true; }

	public void CrashDamage(user) {
		if (!user.takesIndirectDamage()) return;
		@battle.Display(_INTL("{1} kept going and crashed!", user.ToString()));
		@battle.scene.DamageAnimation(user);
		user.ReduceHP(user.totalhp / 2, false);
		user.ItemHPHealCheck;
		if (user.fainted()) user.Faint;
	}
}

//===============================================================================
// Attracts the target. (Attract)
//===============================================================================
public partial class Battle.Move.AttractTarget : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		if (!target.CanAttract(user, show_message)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.Attract(user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanAttract(user, false)) target.Attract(user);
	}
}

//===============================================================================
// Changes user's type depending on the environment. (Camouflage)
//===============================================================================
public partial class Battle.Move.SetUserTypesBasedOnEnvironment : Battle.Move {
	TERRAIN_TYPES = {
		Electric = :ELECTRIC,
		Grassy   = :GRASS,
		Misty    = :FAIRY,
		Psychic  = :PSYCHIC;
	}
	ENVIRONMENT_TYPES = {
		None        = :NORMAL,
		Grass       = :GRASS,
		TallGrass   = :GRASS,
		MovingWater = :WATER,
		StillWater  = :WATER,
		Puddle      = :WATER,
		Underwater  = :WATER,
		Cave        = :ROCK,
		Rock        = :GROUND,
		Sand        = :GROUND,
		Forest      = :BUG,
		ForestGrass = :BUG,
		Snow        = :ICE,
		Ice         = :ICE,
		Volcano     = :FIRE,
		Graveyard   = :GHOST,
		Sky         = :FLYING,
		Space       = :DRAGON,
		UltraSpace  = :PSYCHIC;
	}

	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (!user.canChangeType()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		@newType = Types.NORMAL;
		terr_type = TERRAIN_TYPES[@battle.field.terrain];
		if (terr_type && GameData.Type.exists(terr_type)) {
			@newType = terr_type;
		} else {
			@newType = ENVIRONMENT_TYPES[@battle.environment] || :NORMAL;
			if (!GameData.Type.exists(@newType)) @newType = Types.NORMAL;
		}
		if (!GameData.Type.exists(@newType) || !user.HasOtherType(@newType)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.ChangeTypes(@newType);
		typeName = GameData.Type.get(@newType).name;
		@battle.Display(_INTL("{1}'s type changed to {2}!", user.ToString(), typeName));
	}
}

//===============================================================================
// Changes user's type to a random one that resists/is immune to the last move
// used by the target. (Conversion 2)
//===============================================================================
public partial class Battle.Move.SetUserTypesToResistLastAttack : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool MoveFailed(user, targets) {
		if (!user.canChangeType()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.lastMoveUsed || !target.lastMoveUsedType ||
			GameData.Type.get(target.lastMoveUsedType).pseudo_type) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		@newTypes = new List<string>();
		foreach (var t in GameData.Type) { //'GameData.Type.each' do => |t|
			if (t.pseudo_type || user.HasType(t.id) ||
							!Effectiveness.resistant_type(target.lastMoveUsedType, t.id)) continue;
			@newTypes.Add(t.id);
		}
		if (@newTypes.length == 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		newType = @newTypes[@battle.Random(@newTypes.length)];
		user.ChangeTypes(newType);
		typeName = GameData.Type.get(newType).name;
		@battle.Display(_INTL("{1}'s type changed to {2}!", user.ToString(), typeName));
	}
}

//===============================================================================
// User copies target's types. (Reflect Type)
//===============================================================================
public partial class Battle.Move.SetUserTypesToTargetTypes : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool MoveFailed(user, targets) {
		if (!user.canChangeType()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		newTypes = target.Types(true);
		if (newTypes.length == 0) {   // Target has no type to copy
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (user.Types == target.Types &&
			user.effects.ExtraType == target.effects.ExtraType) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		user.ChangeTypes(target);
		@battle.Display(_INTL("{1}'s type changed to match {2}'s!",
														user.ToString(), target.ToString(true)));
	}
}

//===============================================================================
// Changes user's type to that of a random user's move, except a type the user
// already has (even partially), OR changes to the user's first move's type.
// (Conversion)
//===============================================================================
public partial class Battle.Move.SetUserTypesToUserMoveType : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (!user.canChangeType()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		userTypes = user.Types(true);
		@newTypes = new List<string>();
		user.eachMoveWithIndex do |m, i|
			if (Settings.MECHANICS_GENERATION >= 6 && i > 0) break;
			if (GameData.Type.get(m.type).pseudo_type) continue;
			if (userTypes.Contains(m.type)) continue;
			if (!@newTypes.Contains(m.type)) @newTypes.Add(m.type);
		}
		if (@newTypes.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		newType = @newTypes[@battle.Random(@newTypes.length)];
		user.ChangeTypes(newType);
		typeName = GameData.Type.get(newType).name;
		@battle.Display(_INTL("{1}'s type changed to {2}!", user.ToString(), typeName));
	}
}

//===============================================================================
// The target's types become Psychic. (Magic Powder)
//===============================================================================
public partial class Battle.Move.SetTargetTypesToPsychic : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.canChangeType() || !GameData.Types.exists(Types.PSYCHIC) ||
			!target.HasOtherType(Types.PSYCHIC)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.ChangeTypes(:PSYCHIC);
		typeName = GameData.Type.get(:PSYCHIC).name;
		@battle.Display(_INTL("{1}'s type changed to {2}!", target.ToString(), typeName));
	}
}

//===============================================================================
// Target becomes Water type. (Soak)
//===============================================================================
public partial class Battle.Move.SetTargetTypesToWater : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.canChangeType() || !GameData.Types.exists(Types.WATER) ||
			!target.HasOtherType(Types.WATER)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.ChangeTypes(:WATER);
		typeName = GameData.Type.get(:WATER).name;
		@battle.Display(_INTL("{1}'s type changed to {2}!", target.ToString(), typeName));
	}
}

//===============================================================================
// Gives target the Ghost type. (Trick-or-Treat)
//===============================================================================
public partial class Battle.Move.AddGhostTypeToTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.canChangeType() || !GameData.Types.exists(Types.GHOST) || target.Type == Types.GHOST) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.ExtraType = :GHOST;
		typeName = GameData.Type.get(:GHOST).name;
		@battle.Display(_INTL("{1} transformed into the {2} type!", target.ToString(), typeName));
	}
}

//===============================================================================
// Gives target the Grass type. (Forest's Curse)
//===============================================================================
public partial class Battle.Move.AddGrassTypeToTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.canChangeType() || !GameData.Types.exists(Types.GRASS) || target.Type == Types.GRASS) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.ExtraType = :GRASS;
		typeName = GameData.Type.get(:GRASS).name;
		@battle.Display(_INTL("{1} transformed into the {2} type!", target.ToString(), typeName));
	}
}

//===============================================================================
// User loses their Fire type. Fails if user is not Fire-type. (Burn Up)
//===============================================================================
public partial class Battle.Move.UserLosesFireType : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (!user.Type == Types.FIRE) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAfterAllHits(user, target) {
		if (!user.effects.BurnUp) {
			user.effects.BurnUp = true;
			@battle.Display(_INTL("{1} burned itself out!", user.ToString()));
		}
	}
}

//===============================================================================
// User loses their Electric type. Fails if user is not Electric-type.
// (Double Shock)
//===============================================================================
public partial class Battle.Move.UserLosesElectricType : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (!user.Type == Types.ELECTRIC) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAfterAllHits(user, target) {
		if (!user.effects.DoubleShock) {
			user.effects.DoubleShock = true;
			@battle.Display(_INTL("{1} used up all its electricity!", user.ToString()));
		}
	}
}

//===============================================================================
// Target's ability becomes Simple. (Simple Beam)
//===============================================================================
public partial class Battle.Move.SetTargetAbilityToSimple : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (!GameData.Abilitys.exists(Abilitys.SIMPLE)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.unstoppableAbility() || new []{:TRUANT, :SIMPLE}.Contains(target.ability_id)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		@battle.ShowAbilitySplash(target, true, false);
		oldAbil = target.ability;
		target.ability = abilitys.SIMPLE;
		@battle.ReplaceAbilitySplash(target);
		@battle.Display(_INTL("{1} acquired {2}!", target.ToString(), target.abilityName));
		@battle.HideAbilitySplash(target);
		target.OnLosingAbility(oldAbil);
		target.TriggerAbilityOnGainingIt;
	}
}

//===============================================================================
// Target's ability becomes Insomnia. (Worry Seed)
//===============================================================================
public partial class Battle.Move.SetTargetAbilityToInsomnia : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (!GameData.Abilitys.exists(Abilitys.INSOMNIA)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.unstoppableAbility() || new []{:TRUANT, :INSOMNIA}.Contains(target.ability_id)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		@battle.ShowAbilitySplash(target, true, false);
		oldAbil = target.ability;
		target.ability = abilitys.INSOMNIA;
		@battle.ReplaceAbilitySplash(target);
		@battle.Display(_INTL("{1} acquired {2}!", target.ToString(), target.abilityName));
		@battle.HideAbilitySplash(target);
		target.OnLosingAbility(oldAbil);
		target.TriggerAbilityOnGainingIt;
	}
}

//===============================================================================
// User copies target's ability. (Role Play)
//===============================================================================
public partial class Battle.Move.SetUserAbilityToTargetAbility : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool MoveFailed(user, targets) {
		if (user.unstoppableAbility()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.ability || user.ability == target.ability) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.ungainableAbility() ||
			new []{:POWEROFALCHEMY, :RECEIVER, :TRACE, :WONDERGUARD}.Contains(target.ability_id)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		@battle.ShowAbilitySplash(user, true, false);
		oldAbil = user.ability;
		user.ability = target.ability;
		@battle.ReplaceAbilitySplash(user);
		@battle.Display(_INTL("{1} copied {2}'s {3}!",
														user.ToString(), target.ToString(true), target.abilityName));
		@battle.HideAbilitySplash(user);
		user.OnLosingAbility(oldAbil);
		user.TriggerAbilityOnGainingIt;
	}
}

//===============================================================================
// Target copies user's ability. (Entrainment)
//===============================================================================
public partial class Battle.Move.SetTargetAbilityToUserAbility : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (!user.ability) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (user.ungainableAbility() ||
			new []{:POWEROFALCHEMY, :RECEIVER, :TRACE}.Contains(user.ability_id)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.unstoppableAbility() || target.ability == abilitys.TRUANT) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		@battle.ShowAbilitySplash(target, true, false);
		oldAbil = target.ability;
		target.ability = user.ability;
		@battle.ReplaceAbilitySplash(target);
		@battle.Display(_INTL("{1} acquired {2}!", target.ToString(), target.abilityName));
		@battle.HideAbilitySplash(target);
		target.OnLosingAbility(oldAbil);
		target.TriggerAbilityOnGainingIt;
	}
}

//===============================================================================
// User and target swap abilities. (Skill Swap)
//===============================================================================
public partial class Battle.Move.UserTargetSwapAbilities : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool MoveFailed(user, targets) {
		if (!user.ability) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (user.unstoppableAbility()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (user.ungainableAbility() || user.ability == abilitys.WONDERGUARD) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.ability ||
			(user.ability == target.ability && Settings.MECHANICS_GENERATION <= 5)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.unstoppableAbility()) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.ungainableAbility() || target.ability == abilitys.WONDERGUARD) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (user.opposes(target)) {
			@battle.ShowAbilitySplash(user, false, false);
			@battle.ShowAbilitySplash(target, true, false);
		}
		oldUserAbil   = user.ability;
		oldTargetAbil = target.ability;
		user.ability   = oldTargetAbil;
		target.ability = oldUserAbil;
		if (user.opposes(target)) {
			@battle.ReplaceAbilitySplash(user);
			@battle.ReplaceAbilitySplash(target);
		}
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			@battle.Display(_INTL("{1} swapped Abilities with its target!", user.ToString()));
		} else {
			@battle.Display(_INTL("{1} swapped its {2} Ability with its target's {3} Ability!",
															user.ToString(), target.abilityName, user.abilityName));
		}
		if (user.opposes(target)) {
			@battle.HideAbilitySplash(user);
			@battle.HideAbilitySplash(target);
		}
		user.OnLosingAbility(oldUserAbil);
		target.OnLosingAbility(oldTargetAbil);
		user.TriggerAbilityOnGainingIt;
		target.TriggerAbilityOnGainingIt;
	}
}

//===============================================================================
// Target's ability is negated. (Gastro Acid)
//===============================================================================
public partial class Battle.Move.NegateTargetAbility : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.unstoppableAbility() || target.effects.GastroAcid) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.GastroAcid = true;
		target.effects.Truant     = false;
		@battle.Display(_INTL("{1}'s Ability was suppressed!", target.ToString()));
		target.OnLosingAbility(target.ability, true);
	}
}

//===============================================================================
// Negates the target's ability while it remains on the field, if it has already
// performed its action this round. (Core Enforcer)
//===============================================================================
public partial class Battle.Move.NegateTargetAbilityIfTargetActed : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		if (target.damageState.substitute || target.effects.GastroAcid) return;
		if (target.unstoppableAbility()) return;
		if (@battle.choices[target.index].Action != :UseItem &&
							!((@battle.choices[target.index].Action == :UseMove ||
							@battle.choices[target.index].Action == :Shift) && target.movedThisRound())) return;
		target.effects.GastroAcid = true;
		target.effects.Truant     = false;
		@battle.Display(_INTL("{1}'s Ability was suppressed!", target.ToString()));
		target.OnLosingAbility(target.ability, true);
	}
}

//===============================================================================
// Ignores all abilities that alter this move's success or damage.
// (Moongeist Beam, Sunsteel Strike)
//===============================================================================
public partial class Battle.Move.IgnoreTargetAbility : Battle.Move {
	public override void ChangeUsageCounters(user, specialUsage) {
		base.ChangeUsageCounters();
		if (!specialUsage) @battle.moldBreaker = true;
	}
}

//===============================================================================
// For 5 rounds, user becomes airborne. (Magnet Rise)
//===============================================================================
public partial class Battle.Move.StartUserAirborne : Battle.Move {
	public bool unusableInGravity() { return true; }
	public bool canSnatch() {         return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.Ingrain ||
			user.effects.SmackDown ||
			user.effects.MagnetRise > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.MagnetRise = 5;
		@battle.Display(_INTL("{1} levitated with electromagnetism!", user.ToString()));
	}
}

//===============================================================================
// For 3 rounds, target becomes airborne and can always be hit. (Telekinesis)
//===============================================================================
public partial class Battle.Move.StartTargetAirborneAndAlwaysHitByMoves : Battle.Move {
	public bool unusableInGravity() { return true; }
	public bool canMagicCoat() {      return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Ingrain ||
			target.effects.SmackDown ||
			target.effects.Telekinesis > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.isSpecies(Speciess.DIGLETT) ||
			target.isSpecies(Speciess.DUGTRIO) ||
			target.isSpecies(Speciess.SANDYGAST) ||
			target.isSpecies(Speciess.PALOSSAND) ||
			(target.isSpecies(Speciess.GENGAR) && target.mega())) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Telekinesis = 3;
		@battle.Display(_INTL("{1} was hurled into the air!", target.ToString()));
	}
}

//===============================================================================
// Hits airborne semi-invulnerable targets. (Sky Uppercut)
//===============================================================================
public partial class Battle.Move.HitsTargetInSky : Battle.Move {
	public bool hitsFlyingTargets() { return true; }
}

//===============================================================================
// Grounds the target while it remains active. Hits some semi-invulnerable
// targets. (Smack Down, Thousand Arrows)
//===============================================================================
public partial class Battle.Move.HitsTargetInSkyGroundsTarget : Battle.Move {
	public bool hitsFlyingTargets() { return true; }

	public override void CalcTypeModSingle(moveType, defType, user, target) {
		if (moveType == Types.GROUND && defType == Types.FLYING) return Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		return base.CalcTypeModSingle();
	}

	public void EffectAfterAllHits(user, target) {
		if (target.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableInSkyTargetCannotAct") ||
							target.effects.SkyDrop >= 0) return;   // Sky Drop
		if ((!target.airborne() && !target.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
																														"TwoTurnAttackInvulnerableInSkyParalyzeTarget")) return;
		target.effects.SmackDown = true;
		if ((target.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
															"TwoTurnAttackInvulnerableInSkyParalyzeTarget")) {   // NOTE: Not Sky Drop.
			target.effects.TwoTurnAttack = null;
			if (!target.movedThisRound()) @battle.ClearChoice(target.index);
		}
		target.effects.MagnetRise  = 0;
		target.effects.Telekinesis = 0;
		@battle.Display(_INTL("{1} fell straight down!", target.ToString()));
	}
}

//===============================================================================
// For 5 rounds, increases gravity on the field. Pokémon cannot become airborne.
// (Gravity)
//===============================================================================
public partial class Battle.Move.StartGravity : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.field.effects.Gravity > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.field.effects.Gravity = 5;
		@battle.Display(_INTL("Gravity intensified!"));
		@battle.allBattlers.each do |b|
			showMessage = false;
			if ((b.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
														"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
														"TwoTurnAttackInvulnerableInSkyTargetCannotAct")) {
				b.effects.TwoTurnAttack = null;
				if (!b.movedThisRound()) @battle.ClearChoice(b.index);
				showMessage = true;
			}
			if (b.effects.MagnetRise > 0 ||
				b.effects.Telekinesis > 0 ||
				b.effects.SkyDrop >= 0) {
				b.effects.MagnetRise  = 0;
				b.effects.Telekinesis = 0;
				b.effects.SkyDrop     = -1;
				showMessage = true;
			}
			if (showMessage) {
				@battle.Display(_INTL("{1} couldn't stay airborne because of gravity!", b.ToString()));
			}
		}
	}
}

//===============================================================================
// User transforms into the target. (Transform)
//===============================================================================
public partial class Battle.Move.TransformUserIntoTarget : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.effects.Transform) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Transform ||
			target.effects.Illusion) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		user.Transform(target);
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		base.ShowAnimation();
		@battle.scene.ChangePokemon(user, targets[0].pokemon);
	}
}
