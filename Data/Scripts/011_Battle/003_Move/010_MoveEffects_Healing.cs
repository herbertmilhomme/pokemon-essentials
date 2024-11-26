//===============================================================================
// Heals user to full HP. User falls asleep for 2 more rounds. (Rest)
//===============================================================================
public partial class Battle.Move.HealUserFullyAndFallAsleep : Battle.Move.HealingMove {
	public bool MoveFailed(user, targets) {
		if (user.asleep()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (!user.CanSleep(user, true, self, true)) return true;
		if (super) return true;
		return false;
	}

	public void HealAmount(user) {
		return user.totalhp - user.hp;
	}

	public void EffectGeneral(user) {
		user.SleepSelf(_INTL("{1} slept and became healthy!", user.ToString()), 3);
		super;
	}
}

//===============================================================================
// Heals user by 1/2 of its max HP.
//===============================================================================
public partial class Battle.Move.HealUserHalfOfTotalHP : Battle.Move.HealingMove {
	public void HealAmount(user) {
		return (int)Math.Round(user.totalhp / 2.0);
	}
}

//===============================================================================
// Heals user by an amount depending on the weather. (Moonlight, Morning Sun,
// Synthesis)
//===============================================================================
public partial class Battle.Move.HealUserDependingOnWeather : Battle.Move.HealingMove {
	public void OnStartUse(user, targets) {
		switch (user.effectiveWeather) {
			case :Sun: case :HarshSun:
				@healAmount = (int)Math.Round(user.totalhp * 2 / 3.0);
				break;
			case :None: case :StrongWinds:
				@healAmount = (int)Math.Round(user.totalhp / 2.0);
				break;
			default:
				@healAmount = (int)Math.Round(user.totalhp / 4.0);
				break;
		}
	}

	public void HealAmount(user) {
		return @healAmount;
	}
}

//===============================================================================
// Heals user by 1/2 of its max HP, or 2/3 of its max HP in a sandstorm. (Shore Up)
//===============================================================================
public partial class Battle.Move.HealUserDependingOnSandstorm : Battle.Move.HealingMove {
	public void HealAmount(user) {
		if (user.effectiveWeather == Weathers.Sandstorm) return (int)Math.Round(user.totalhp * 2 / 3.0);
		return (int)Math.Round(user.totalhp / 2.0);
	}
}

//===============================================================================
// Heals user by 1/2 of its max HP. (Roost)
// User roosts, and its Flying type is ignored for attacks used against it.
//===============================================================================
public partial class Battle.Move.HealUserHalfOfTotalHPLoseFlyingTypeThisTurn : Battle.Move.HealingMove {
	public void HealAmount(user) {
		return (int)Math.Round(user.totalhp / 2.0);
	}

	public override void EffectGeneral(user) {
		base.EffectGeneral();
		user.effects.Roost = true;
	}
}

//===============================================================================
// Cures the target's permanent status problems. Heals user by 1/2 of its max HP.
// (Purify)
//===============================================================================
public partial class Battle.Move.CureTargetStatusHealUserHalfOfTotalHP : Battle.Move.HealingMove {
	public bool canSnatch() {    return false; }   // Because it affects a target
	public bool canMagicCoat() { return true;  }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.status == statuses.NONE) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void HealAmount(user) {
		return (int)Math.Round(user.totalhp / 2.0);
	}

	public override void EffectAgainstTarget(user, target) {
		target.CureStatus;
		base.EffectAgainstTarget();
	}
}

//===============================================================================
// Decreases the target's Attack by 1 stage. Heals user by an amount equal to the
// target's Attack stat (after applying stat stages, before this move decreases
// it). (Strength Sap)
//===============================================================================
public partial class Battle.Move.HealUserByTargetAttackLowerTargetAttack1 : Battle.Move {
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1};
	}

	public bool healingMove() {  return true; }
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		// NOTE: The official games appear to just check whether the target's Attack
		//       stat stage is -6 and fail if so, but I've added the "fail if target
		//       has Contrary and is at +6" check too for symmetry. This move still
		//       works even if the stat stage cannot be changed due to an ability or
		//       other effect.
		if (target.hasActiveAbility(Abilitys.CONTRARY) && !target.beingMoldBroken()) {
			if (target.statStageAtMax(@statDown[0])) {
				if (show_message) @battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else if (target.statStageAtMin(@statDown[0])) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		// Calculate target's effective attack value
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stageMul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stageDiv = Battle.Battler.STAT_STAGE_DIVISORS;
		atk      = target.attack;
		atkStage = target.stages[@statDown[0]] + max_stage;
		healAmt = (int)Math.Floor(atk.to_f * stageMul[atkStage] / stageDiv[atkStage]);
		// Reduce target's Attack stat
		if (target.CanLowerStatStage(@statDown[0], user, self)) {
			target.LowerStatStage(@statDown[0], @statDown[1], user);
		}
		// Heal user
		if (target.hasActiveAbility(:LIQUIDOOZE, true)) {
			@battle.ShowAbilitySplash(target);
			user.ReduceHP(healAmt);
			@battle.Display(_INTL("{1} sucked up the liquid ooze!", user.ToString()));
			@battle.HideAbilitySplash(target);
			user.ItemHPHealCheck;
		} else if (user.canHeal()) {
			if (user.hasActiveItem(Items.BIGROOT)) healAmt = (int)Math.Floor(healAmt * 1.3);
			user.RecoverHP(healAmt);
			@battle.Display(_INTL("{1}'s HP was restored.", user.ToString()));
		}
	}
}

//===============================================================================
// User gains half the HP it inflicts as damage.
//===============================================================================
public partial class Battle.Move.HealUserByHalfOfDamageDone : Battle.Move {
	public bool healingMove() { return Settings.MECHANICS_GENERATION >= 6; }

	public void EffectAgainstTarget(user, target) {
		if (target.damageState.hpLost <= 0) return;
		hpGain = (int)Math.Round(target.damageState.hpLost / 2.0);
		user.RecoverHPFromDrain(hpGain, target);
	}
}

//===============================================================================
// User gains half the HP it inflicts as damage. Fails if target is not asleep.
// (Dream Eater)
//===============================================================================
public partial class Battle.Move.HealUserByHalfOfDamageDoneIfTargetAsleep : Battle.Move {
	public bool healingMove() { return Settings.MECHANICS_GENERATION >= 6; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.asleep()) {
			if (show_message) @battle.Display(_INTL("{1} wasn't affected!", target.ToString()));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (target.damageState.hpLost <= 0) return;
		hpGain = (int)Math.Round(target.damageState.hpLost / 2.0);
		user.RecoverHPFromDrain(hpGain, target);
	}
}

//===============================================================================
// User gains half the HP it inflicts as damage. Burns the target. (Matcha Gotcha)
//===============================================================================
public partial class Battle.Move.HealUserByHalfOfDamageDoneBurnTarget : Battle.Move.BurnTarget {
	public bool healingMove() { return Settings.MECHANICS_GENERATION >= 6; }

	public void EffectAgainstTarget(user, target) {
		if (target.damageState.hpLost <= 0) return;
		hpGain = (int)Math.Round(target.damageState.hpLost / 2.0);
		user.RecoverHPFromDrain(hpGain, target);
	}
}

//===============================================================================
// User gains 3/4 the HP it inflicts as damage. (Draining Kiss, Oblivion Wing)
//===============================================================================
public partial class Battle.Move.HealUserByThreeQuartersOfDamageDone : Battle.Move {
	public bool healingMove() { return Settings.MECHANICS_GENERATION >= 6; }

	public void EffectAgainstTarget(user, target) {
		if (target.damageState.hpLost <= 0) return;
		hpGain = (int)Math.Round(target.damageState.hpLost * 0.75);
		user.RecoverHPFromDrain(hpGain, target);
	}
}

//===============================================================================
// The user and its allies gain 25% of their total HP. (Life Dew)
//===============================================================================
public partial class Battle.Move.HealUserAndAlliesQuarterOfTotalHP : Battle.Move {
	public bool healingMove() { return true; }

	public bool MoveFailed(user, targets) {
		if (@battle.allSameSideBattlers(user).none(b => b.canHeal())) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		return !target.canHeal();
	}

	public void EffectAgainstTarget(user, target) {
		target.RecoverHP(target.totalhp / 4);
		@battle.Display(_INTL("{1}'s HP was restored.", target.ToString()));
	}
}

//===============================================================================
// The user and its allies gain 25% of their total HP and are cured of their
// permanent status problems. (Jungle Healing)
//===============================================================================
public partial class Battle.Move.HealUserAndAlliesQuarterOfTotalHPCureStatus : Battle.Move {
	public bool healingMove() { return true; }

	public bool MoveFailed(user, targets) {
		if (@battle.allSameSideBattlers(user).none(b => b.canHeal() || b.status != statuses.NONE)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		return target.status == statuses.NONE && !target.canHeal();
	}

	public void EffectAgainstTarget(user, target) {
		if (target.canHeal()) {
			target.RecoverHP(target.totalhp / 4);
			@battle.Display(_INTL("{1}'s HP was restored.", target.ToString()));
		}
		if (target.status != statuses.NONE) {
			old_status = target.status;
			target.CureStatus(false);
			switch (old_status) {
				case :SLEEP:
					@battle.Display(_INTL("{1} was woken from sleep.", target.ToString()));
					break;
				case :POISON:
					@battle.Display(_INTL("{1} was cured of its poisoning.", target.ToString()));
					break;
				case :BURN:
					@battle.Display(_INTL("{1}'s burn was healed.", target.ToString()));
					break;
				case :PARALYSIS:
					@battle.Display(_INTL("{1} was cured of paralysis.", target.ToString()));
					break;
				case :FROZEN:
					@battle.Display(_INTL("{1} was thawed out.", target.ToString()));
					break;
			}
		}
	}
}

//===============================================================================
// Heals target by 1/2 of its max HP. (Heal Pulse)
//===============================================================================
public partial class Battle.Move.HealTargetHalfOfTotalHP : Battle.Move {
	public bool healingMove() {  return true; }
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.hp == target.totalhp) {
			if (show_message) @battle.Display(_INTL("{1}'s HP is full!", target.ToString()));
			return true;
		} else if (!target.canHeal()) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		hpGain = (int)Math.Round(target.totalhp / 2.0);
		if (pulseMove() && user.hasActiveAbility(Abilitys.MEGALAUNCHER)) {
			hpGain = (int)Math.Round(target.totalhp * 3 / 4.0);
		}
		target.RecoverHP(hpGain);
		@battle.Display(_INTL("{1}'s HP was restored.", target.ToString()));
	}
}

//===============================================================================
// Heals target by 1/2 of its max HP, or 2/3 of its max HP in Grassy Terrain.
// (Floral Healing)
//===============================================================================
public partial class Battle.Move.HealTargetDependingOnGrassyTerrain : Battle.Move {
	public bool healingMove() {  return true; }
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.hp == target.totalhp) {
			if (show_message) @battle.Display(_INTL("{1}'s HP is full!", target.ToString()));
			return true;
		} else if (!target.canHeal()) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		hpGain = (int)Math.Round(target.totalhp / 2.0);
		if (@battle.field.terrain == :Grassy) hpGain = (int)Math.Round(target.totalhp * 2 / 3.0);
		target.RecoverHP(hpGain);
		@battle.Display(_INTL("{1}'s HP was restored.", target.ToString()));
	}
}

//===============================================================================
// Battler in user's position is healed by 1/2 of its max HP, at the end of the
// next round. (Wish)
//===============================================================================
public partial class Battle.Move.HealUserPositionNextTurn : Battle.Move {
	public bool healingMove() { return true; }
	public bool canSnatch() {   return true; }

	public bool MoveFailed(user, targets) {
		if (@battle.positions[user.index].effects.Wish > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.positions[user.index].effects.Wish       = 2;
		@battle.positions[user.index].effects.WishAmount = (int)Math.Round(user.totalhp / 2.0);
		@battle.positions[user.index].effects.WishMaker  = user.pokemonIndex;
	}
}

//===============================================================================
// Rings the user. Ringed Pokémon gain 1/16 of max HP at the end of each round.
// (Aqua Ring)
//===============================================================================
public partial class Battle.Move.StartHealUserEachTurn : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.AquaRing) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.AquaRing = true;
		@battle.Display(_INTL("{1} surrounded itself with a veil of water!", user.ToString()));
	}
}

//===============================================================================
// Ingrains the user. Ingrained Pokémon gain 1/16 of max HP at the end of each
// round, and cannot flee or switch out. (Ingrain)
//===============================================================================
public partial class Battle.Move.StartHealUserEachTurnTrapUserInBattle : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.Ingrain) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.Ingrain = true;
		@battle.Display(_INTL("{1} planted its roots!", user.ToString()));
	}
}

//===============================================================================
// Target will lose 1/4 of max HP at end of each round, while asleep. (Nightmare)
//===============================================================================
public partial class Battle.Move.StartDamageTargetEachTurnIfTargetAsleep : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.asleep() || target.effects.Nightmare) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Nightmare = true;
		@battle.Display(_INTL("{1} began having a nightmare!", target.ToString()));
	}
}

//===============================================================================
// Seeds the target. Seeded Pokémon lose 1/8 of max HP at the end of each round,
// and the Pokémon in the user's position gains the same amount. (Leech Seed)
//===============================================================================
public partial class Battle.Move.StartLeechSeedTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.LeechSeed >= 0) {
			if (show_message) @battle.Display(_INTL("{1} evaded the attack!", target.ToString()));
			return true;
		}
		if (target.Type == Types.GRASS) {
			if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			return true;
		}
		return false;
	}

	public void MissMessage(user, target) {
		@battle.Display(_INTL("{1} evaded the attack!", target.ToString()));
		return true;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.LeechSeed = user.index;
		@battle.Display(_INTL("{1} was seeded!", target.ToString()));
	}
}

//===============================================================================
// The user takes damage equal to 1/2 of its total HP, even if the target is
// unaffected (this is not recoil damage). (Steel Beam)
//===============================================================================
public partial class Battle.Move.UserLosesHalfOfTotalHP : Battle.Move {
	public void EffectAfterAllHits(user, target) {
		if (!user.takesIndirectDamage()) return;
		amt = (user.totalhp / 2.0).ceil;
		if (amt < 1) amt = 1;
		user.ReduceHP(amt, false);
		@battle.Display(_INTL("{1} is damaged by recoil!", user.ToString()));
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Damages user by 1/2 of its max HP, even if this move misses. (Mind Blown)
//===============================================================================
public partial class Battle.Move.UserLosesHalfOfTotalHPExplosive : Battle.Move {
	public bool worksWithNoTargets() { return true; }

	public bool MoveFailed(user, targets) {
		bearer = @battle.CheckGlobalAbility(:DAMP, true);
		if (bearer) {
			@battle.ShowAbilitySplash(bearer);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1} cannot use {2}!", user.ToString(), @name));
			} else {
				@battle.Display(_INTL("{1} cannot use {2} because of {3}'s {4}!",
																user.ToString(), @name, bearer.ToString(true), bearer.abilityName));
			}
			@battle.HideAbilitySplash(bearer);
			return true;
		}
		return false;
	}

	public void SelfKO(user) {
		if (!user.takesIndirectDamage()) return;
		user.ReduceHP((int)Math.Round(user.totalhp / 2.0), false);
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// User faints, even if the move does nothing else. (Explosion, Self-Destruct)
//===============================================================================
public partial class Battle.Move.UserFaintsExplosive : Battle.Move {
	public bool worksWithNoTargets() {      return true; }
	public void NumHits(user, targets) {return 1;    }

	public bool MoveFailed(user, targets) {
		bearer = @battle.CheckGlobalAbility(:DAMP, true);
		if (bearer) {
			@battle.ShowAbilitySplash(bearer);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1} cannot use {2}!", user.ToString(), @name));
			} else {
				@battle.Display(_INTL("{1} cannot use {2} because of {3}'s {4}!",
																user.ToString(), @name, bearer.ToString(true), bearer.abilityName));
			}
			@battle.HideAbilitySplash(bearer);
			return true;
		}
		return false;
	}

	public void SelfKO(user) {
		if (user.fainted()) return;
		user.ReduceHP(user.hp, false);
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// User faints. If Misty Terrain applies, base power is multiplied by 1.5.
// (Misty Explosion)
//===============================================================================
public partial class Battle.Move.UserFaintsPowersUpInMistyTerrainExplosive : Battle.Move.UserFaintsExplosive {
	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.terrain == :Misty) baseDmg = baseDmg * 3 / 2;
		return baseDmg;
	}
}

//===============================================================================
// Inflicts fixed damage equal to user's current HP. (Final Gambit)
// User faints (if successful).
//===============================================================================
public partial class Battle.Move.UserFaintsFixedDamageUserHP : Battle.Move.FixedDamageMove {
	public void NumHits(user, targets) {return 1; }

	public void OnStartUse(user, targets) {
		@finalGambitDamage = user.hp;
	}

	public void FixedDamage(user, target) {
		return @finalGambitDamage;
	}

	public void SelfKO(user) {
		if (user.fainted()) return;
		user.ReduceHP(user.hp, false);
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Decreases the target's Attack and Special Attack by 2 stages each. (Memento)
// User faints (if successful).
//===============================================================================
public partial class Battle.Move.UserFaintsLowerTargetAtkSpAtk2 : Battle.Move.TargetMultiStatDownMove {
	public bool canMagicCoat() { return false; }

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 2, :SPECIAL_ATTACK, 2};
	}

	// NOTE: The user faints even if the target's stats cannot be changed, so this
	//       method must always return false to allow the move's usage to continue.
	public bool FailsAgainstTarget(user, target, show_message) {
		return false;
	}

	public void SelfKO(user) {
		if (user.fainted()) return;
		user.ReduceHP(user.hp, false);
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// User faints. The Pokémon that replaces the user is fully healed (HP and
// status). Fails if user won't be replaced. (Healing Wish)
//===============================================================================
public partial class Battle.Move.UserFaintsHealAndCureReplacement : Battle.Move {
	public bool healingMove() { return true; }
	public bool canSnatch() {   return true; }

	public bool MoveFailed(user, targets) {
		if (!@battle.CanChooseNonActive(user.index)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void SelfKO(user) {
		if (user.fainted()) return;
		user.ReduceHP(user.hp, false);
		user.ItemHPHealCheck;
		@battle.positions[user.index].effects.HealingWish = true;
	}
}

//===============================================================================
// User faints. The Pokémon that replaces the user is fully healed (HP, PP and
// status). Fails if user won't be replaced. (Lunar Dance)
//===============================================================================
public partial class Battle.Move.UserFaintsHealAndCureReplacementRestorePP : Battle.Move {
	public bool healingMove() { return true; }
	public bool canSnatch() {   return true; }

	public bool MoveFailed(user, targets) {
		if (!@battle.CanChooseNonActive(user.index)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void SelfKO(user) {
		if (user.fainted()) return;
		user.ReduceHP(user.hp, false);
		user.ItemHPHealCheck;
		@battle.positions[user.index].effects.LunarDance = true;
	}
}

//===============================================================================
// All current battlers will perish after 3 more rounds. (Perish Song)
//===============================================================================
public partial class Battle.Move.StartPerishCountsForAllBattlers : Battle.Move {
	public bool MoveFailed(user, targets) {
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.effects.PerishSong > 0) continue;   // Heard it before
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		return target.effects.PerishSong > 0;   // Heard it before
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.PerishSong     = 4;
		target.effects.PerishSongUser = user.index;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		base.ShowAnimation();
		@battle.Display(_INTL("All Pokémon that hear the song will faint in three turns!"));
	}
}

//===============================================================================
// If user is KO'd before it next moves, the battler that caused it also faints.
// (Destiny Bond)
//===============================================================================
public partial class Battle.Move.AttackerFaintsIfUserFaints : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (Settings.MECHANICS_GENERATION >= 7 && user.effects.DestinyBondPrevious) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.DestinyBond = true;
		@battle.Display(_INTL("{1} is hoping to take its attacker down with it!", user.ToString()));
	}
}

//===============================================================================
// If user is KO'd before it next moves, the attack that caused it loses all PP.
// (Grudge)
//===============================================================================
public partial class Battle.Move.SetAttackerMovePPTo0IfUserFaints : Battle.Move {
	public void EffectGeneral(user) {
		user.effects.Grudge = true;
		@battle.Display(_INTL("{1} wants its target to bear a grudge!", user.ToString()));
	}
}
