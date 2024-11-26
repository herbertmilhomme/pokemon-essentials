//===============================================================================
// Hits twice.
//===============================================================================
public partial class Battle.Move.HitTwoTimes : Battle.Move {
	public bool multiHitMove() {            return true; }
	public void NumHits(user, targets) {return 2;    }
}

//===============================================================================
// Hits twice. May poison the target on each hit. (Twineedle)
//===============================================================================
public partial class Battle.Move.HitTwoTimesPoisonTarget : Battle.Move.PoisonTarget {
	public bool multiHitMove() {            return true; }
	public void NumHits(user, targets) {return 2;    }
}

//===============================================================================
// Hits twice. Causes the target to flinch. (Double Iron Bash)
//===============================================================================
public partial class Battle.Move.HitTwoTimesFlinchTarget : Battle.Move.FlinchTarget {
	public bool multiHitMove() {            return true; }
	public void NumHits(user, targets) {return 2;    }
}

//===============================================================================
// Hits in 2 volleys. The second volley targets the original target's ally if it
// has one (that can be targeted), or the original target if not. A battler
// cannot be targeted if it is is immune to or protected from this move somehow,
// or if this move will miss it. (Dragon Darts)
// NOTE: This move sometimes shows a different failure message compared to the
//       official games. This is because of the order in which failure checks are
//       done (all checks for each target in turn, versus all targets for each
//       check in turn). This is considered unimportant, and since correcting it
//       would involve extensive code rewrites, it is being ignored.
//===============================================================================
public partial class Battle.Move.HitTwoTimesTargetThenTargetAlly : Battle.Move {
	public void NumHits(user, targets) {return 1;    }
	public bool RepeatHit() {             return true; }

	public void ModifyTargets(targets, user) {
		if (targets.length != 1) return;
		choices = new List<string>();
		targets[0].allAllies.each(b => user.AddTarget(choices, user, b, self));
		if (choices.length == 0) return;
		idxChoice = (choices.length > 1) ? @battle.Random(choices.length) : 0;
		user.AddTarget(targets, user, choices[idxChoice], self, !Target(user).can_choose_distant_target());
	}

	public bool ShowFailMessages(targets) {
		if (targets.length > 1) {
			valid_targets = targets.select(b => !b.fainted() && !b.damageState.unaffected);
			return valid_targets.length <= 1;
		}
		return super;
	}

	public void DesignateTargetsForHit(targets, hitNum) {
		valid_targets = new List<string>();
		targets.each(b => { if (!b.damageState.unaffected) valid_targets.Add(b); });
		if (valid_targets[1] && hitNum == 1) return [valid_targets[1]];
		return [valid_targets[0]];
	}
}

//===============================================================================
// Hits 3 times. Power is multiplied by the hit number. An accuracy check is
// performed for each hit. (Triple Kick)
//===============================================================================
public partial class Battle.Move.HitThreeTimesPowersUpWithEachHit : Battle.Move {
	public bool multiHitMove() {            return true; }
	public void NumHits(user, targets) {return 3;    }

	public bool successCheckPerHit() {
		return @accCheckPerHit;
	}

	public void OnStartUse(user, targets) {
		@calcBaseDmg = 0;
		@accCheckPerHit = !user.hasActiveAbility(Abilitys.SKILLLINK) && !user.hasActiveItem(Items.LOADEDDICE);
	}

	public void BaseDamage(baseDmg, user, target) {
		@calcBaseDmg += baseDmg;
		return @calcBaseDmg;
	}
}

//===============================================================================
// Hits 3 times in a row. If each hit could be a critical hit, it will definitely
// be a critical hit. (Surging Strikes)
//===============================================================================
public partial class Battle.Move.HitThreeTimesAlwaysCriticalHit : Battle.Move {
	public bool multiHitMove() {                   return true; }
	public void NumHits(user, targets)        {return 3;    }
	public void CritialOverride(user, target) {return 1;    }
}

//===============================================================================
// Hits 10 times in a row. An accuracy check is performed for each hit.
// (Population Bomb)
//===============================================================================
public partial class Battle.Move.HitThreeTimesAlwaysCriticalHit : Battle.Move {
	public bool multiHitMove() { return true; }

	public void NumHits(user, targets) {
		if (user.hasActiveItem(Items.LOADEDDICE)) return 4 + @battle.Random(7);
		return 10;
	}

	public bool successCheckPerHit() {
		return @accCheckPerHit;
	}

	public void OnStartUse(user, targets) {
		@accCheckPerHit = !user.hasActiveAbility(Abilitys.SKILLLINK) && !user.hasActiveItem(Items.LOADEDDICE);
	}
}

//===============================================================================
// Hits 2-5 times.
//===============================================================================
public partial class Battle.Move.HitTwoToFiveTimes : Battle.Move {
	public bool multiHitMove() { return true; }

	public void NumHits(user, targets) {
		hitChances = new {
			2, 2, 2, 2, 2, 2, 2,
			3, 3, 3, 3, 3, 3, 3,
			4, 4, 4,
			5, 5, 5;
		}
		r = @battle.Random(hitChances.length);
		if (user.hasActiveAbility(Abilitys.SKILLLINK)) r = hitChances.length - 1;
		if (r < 4 && user.hasActiveItem(Items.LOADEDDICE)) r = 4;
		return hitChances[r];
	}
}

//===============================================================================
// Hits 2-5 times. If the user is Ash Greninja, powers up and hits 3 times.
// (Water Shuriken)
//===============================================================================
public partial class Battle.Move.HitTwoToFiveTimesOrThreeForAshGreninja : Battle.Move.HitTwoToFiveTimes {
	public override void NumHits(user, targets) {
		if (user.isSpecies(Speciess.GRENINJA) && user.form == 2) return 3;
		return base.NumHits();
	}

	public override void BaseDamage(baseDmg, user, target) {
		if (user.isSpecies(Speciess.GRENINJA) && user.form == 2) return 20;
		return base.BaseDamage();
	}
}

//===============================================================================
// Hits 2-5 times in a row. If the move does not fail, increases the user's Speed
// by 1 stage and decreases the user's Defense by 1 stage. (Scale Shot)
//===============================================================================
public partial class Battle.Move.HitTwoToFiveTimesRaiseUserSpd1LowerUserDef1 : Battle.Move.HitTwoToFiveTimes {
	public void EffectAfterAllHits(user, target) {
		if (target.damageState.unaffected) return;
		if (user.CanLowerStatStage(:DEFENSE, user, self)) {
			user.LowerStatStage(:DEFENSE, 1, user);
		}
		if (user.CanRaiseStatStage(:SPEED, user, self)) {
			user.RaiseStatStage(:SPEED, 1, user);
		}
	}
}

//===============================================================================
// Hits X times, where X is the number of non-user unfainted status-free Pokémon
// in the user's party (not including partner trainers). Fails if X is 0.
// Base power of each hit depends on the base Attack stat for the species of that
// hit's participant. (Beat Up)
//===============================================================================
public partial class Battle.Move.HitOncePerUserTeamMember : Battle.Move {
	public bool multiHitMove() { return true; }

	public bool MoveFailed(user, targets) {
		@beatUpList = new List<string>();
		@battle.eachInTeamFromBattlerIndex(user.index) do |pkmn, i|
			if (!pkmn.able() || pkmn.status != statuses.NONE) continue;
			@beatUpList.Add(i);
		}
		if (@beatUpList.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void NumHits(user, targets) {
		return @beatUpList.length;
	}

	public void BaseDamage(baseDmg, user, target) {
		i = @beatUpList.shift;   // First element in array, and removes it from array
		atk = @battle.Party(user.index)[i].baseStats[:ATTACK];
		return 5 + (atk / 10);
	}
}

//===============================================================================
// Attacks first turn, skips second turn (if successful).
//===============================================================================
public partial class Battle.Move.AttackAndSkipNextTurn : Battle.Move {
	public void EffectGeneral(user) {
		user.effects.HyperBeam = 2;
		user.currentMove = @id;
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Razor Wind)
//===============================================================================
public partial class Battle.Move.TwoTurnAttack : Battle.Move.TwoTurnMove {
	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} whipped up a whirlwind!", user.ToString()));
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Solar Beam, Solar Blade)
// Power halved in all weather except sunshine. In sunshine, takes 1 turn instead.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackOneTurnInSun : Battle.Move.TwoTurnMove {
	public override bool IsChargingTurn(user) {
		ret = base.IsChargingTurn();
		if (!user.effects.TwoTurnAttack &&
			new []{:Sun, :HarshSun}.Contains(user.effectiveWeather)) {
			@powerHerb = false;
			@chargingTurn = true;
			@damagingTurn = true;
			return false;
		}
		return ret;
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} took in sunlight!", user.ToString()));
	}

	public void BaseDamageMultiplier(damageMult, user, target) {
		if (!new []{:None, :Sun, :HarshSun}.Contains(user.effectiveWeather)) damageMult /= 2;
		return damageMult;
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Freeze Shock)
// May paralyze the target.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackParalyzeTarget : Battle.Move.TwoTurnMove {
	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} became cloaked in a freezing light!", user.ToString()));
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanParalyze(user, false, self)) target.Paralyze(user);
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Ice Burn)
// May burn the target.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackBurnTarget : Battle.Move.TwoTurnMove {
	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} became cloaked in freezing air!", user.ToString()));
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanBurn(user, false, self)) target.Burn(user);
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Sky Attack)
// May make the target flinch.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackFlinchTarget : Battle.Move.TwoTurnMove {
	public bool flinchingMove() { return true; }

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} became cloaked in a harsh light!", user.ToString()));
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		target.Flinch(user);
	}
}

//===============================================================================
// Two turn attack. Skips first turn, and increases the user's Special Attack,
// Special Defense and Speed by 2 stages each in the second turn. (Geomancy)
//===============================================================================
public partial class Battle.Move.TwoTurnAttackRaiseUserSpAtkSpDefSpd2 : Battle.Move.TwoTurnMove {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 2, :SPECIAL_DEFENSE, 2, :SPEED, 2};
	}

	public bool MoveFailed(user, targets) {
		if (user.effects.TwoTurnAttack) return false;   // Charging turn
		failed = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("{1}'s stats won't go any higher!", user.ToString()));
			return true;
		}
		return false;
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} is absorbing power!", user.ToString()));
	}

	public void EffectGeneral(user) {
		if (!@damagingTurn) return;
		showAnim = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			if (user.RaiseStatStage(@statUp[i * 2], @statUp[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
	}
}

//===============================================================================
// Two turn attack. On the first turn, increases the user's Defense by 1 stage.
// On the second turn, does damage. (Skull Bash)
//===============================================================================
public partial class Battle.Move.TwoTurnAttackChargeRaiseUserDefense1 : Battle.Move.TwoTurnMove {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 1};
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} tucked in its head!", user.ToString()));
	}

	public void ChargingTurnEffect(user, target) {
		if (user.CanRaiseStatStage(@statUp[0], user, self)) {
			user.RaiseStatStage(@statUp[0], @statUp[1], user);
		}
	}
}

//===============================================================================
// Two-turn attack. On the first turn, increases the user's Special Attack by 1
// stage. On the second turn, does damage. (Meteor Beam)
//===============================================================================
public partial class Battle.Move.TwoTurnAttackChargeRaiseUserSpAtk1 : Battle.Move.TwoTurnMove {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 1};
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} is overflowing with space power!", user.ToString()));
	}

	public void ChargingTurnEffect(user, target) {
		if (user.CanRaiseStatStage(@statUp[0], user, self)) {
			user.RaiseStatStage(@statUp[0], @statUp[1], user);
		}
	}
}

//===============================================================================
// Two turn attack. On the first turn, increases the user's Special Attack by 1
// stage. On the second turn, does damage. In rain, takes 1 turn instead.
// (Electro Shot)
//===============================================================================
public partial class Battle.Move.TwoTurnAttackOneTurnInRainChargeRaiseUserSpAtk1 : Battle.Move.TwoTurnAttackChargeRaiseUserSpAtk1 {
	public override bool IsChargingTurn(user) {
		ret = base.IsChargingTurn();
		if (!user.effects.TwoTurnAttack &&
			new []{:Rain, :HeavyRain}.Contains(user.effectiveWeather)) {
			@powerHerb = false;
			@chargingTurn = true;
			@damagingTurn = true;
			return false;
		}
		return ret;
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} absorbed electricity!", user.ToString()));
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Dig)
// (Handled in Battler's SuccessCheckPerHit): Is semi-invulnerable during use.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackInvulnerableUnderground : Battle.Move.TwoTurnMove {
	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} burrowed its way under the ground!", user.ToString()));
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Dive)
// (Handled in Battler's SuccessCheckPerHit): Is semi-invulnerable during use.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackInvulnerableUnderwater : Battle.Move.TwoTurnMove {
	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} hid underwater!", user.ToString()));
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Fly)
// (Handled in Battler's SuccessCheckPerHit): Is semi-invulnerable during use.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackInvulnerableInSky : Battle.Move.TwoTurnMove {
	public bool unusableInGravity() { return true; }

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} flew up high!", user.ToString()));
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Bounce)
// May paralyze the target.
// (Handled in Battler's SuccessCheckPerHit): Is semi-invulnerable during use.
//===============================================================================
public partial class Battle.Move.TwoTurnAttackInvulnerableInSkyParalyzeTarget : Battle.Move.TwoTurnMove {
	public bool unusableInGravity() { return true; }

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} sprang up!", user.ToString()));
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (target.CanParalyze(user, false, self)) target.Paralyze(user);
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. (Sky Drop)
// (Handled in Battler's SuccessCheckPerHit): Is semi-invulnerable during use.
// Target is also semi-invulnerable during use, and can't take any action.
// Doesn't damage airborne Pokémon (but still makes them unable to move during).
//===============================================================================
public partial class Battle.Move.TwoTurnAttackInvulnerableInSkyTargetCannotAct : Battle.Move.TwoTurnMove {
	public bool unusableInGravity() { return true; }

	public bool IsChargingTurn(user) {
		// NOTE: Sky Drop doesn't benefit from Power Herb, probably because it works
		//       differently (i.e. immobilises the target during use too).
		@powerHerb = false;
		@chargingTurn = (user.effects.TwoTurnAttack.null());
		@damagingTurn = (!user.effects.TwoTurnAttack.null());
		return !@damagingTurn;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.opposes(user)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.effects.Substitute > 0 && !ignoresSubstitute(user)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (Settings.MECHANICS_GENERATION >= 6 && target.Weight >= 2000) {   // 200.0kg
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.semiInvulnerable() ||
			(target.effects.SkyDrop >= 0 && @chargingTurn)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.effects.SkyDrop != user.index && @damagingTurn) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public override void CalcTypeMod(movetype, user, target) {
		if (target.Type == Types.FLYING) return Effectiveness.INEFFECTIVE_MULTIPLIER;
		return base.CalcTypeMod();
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} took {2} into the sky!", user.ToString(), targets[0].ToString(true)));
	}

	public void AttackingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} was freed from the Sky Drop!", targets[0].ToString()));
	}

	public void ChargingTurnEffect(user, target) {
		target.effects.SkyDrop = user.index;
	}

	public void EffectAfterAllHits(user, target) {
		if (@damagingTurn) target.effects.SkyDrop = -1;
	}
}

//===============================================================================
// Two turn attack. Skips first turn, attacks second turn. Is invulnerable during
// use. Ends target's protections upon hit. (Shadow Force, Phantom Force)
//===============================================================================
public partial class Battle.Move.TwoTurnAttackInvulnerableRemoveProtections : Battle.Move.TwoTurnMove {
	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} vanished instantly!", user.ToString()));
	}

	public void AttackingTurnEffect(user, target) {
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
// User must use this move for 2 more rounds. No battlers can sleep. (Uproar)
// NOTE: Bulbapedia claims that an uproar will wake up Pokémon even if they have
//       Soundproof, and will not allow Pokémon to fall asleep even if they have
//       Soundproof. I think this is an oversight, so I've let Soundproof Pokémon
//       be unaffected by Uproar waking/non-sleeping effects.
//===============================================================================
public partial class Battle.Move.MultiTurnAttackPreventSleeping : Battle.Move {
	public void EffectGeneral(user) {
		if (user.effects.Uproar > 0) return;
		user.effects.Uproar = 3;
		user.currentMove = @id;
		@battle.Display(_INTL("{1} caused an uproar!", user.ToString()));
		@battle.Priority(true).each do |b|
			if (b.fainted() || b.status != statuses.SLEEP) continue;
			if (b.hasActiveAbility(Abilitys.SOUNDPROOF)) continue;
			b.CureStatus;
		}
	}
}

//===============================================================================
// User must use this move for 1 or 2 more rounds. At end, user becomes confused.
// (Outrage, Petal Dange, Thrash)
//===============================================================================
public partial class Battle.Move.MultiTurnAttackConfuseUserAtEnd : Battle.Move {
	public void EffectAfterAllHits(user, target) {
		if (!target.damageState.unaffected && user.effects.Outrage == 0) {
			user.effects.Outrage = 2 + @battle.Random(2);
			user.currentMove = @id;
		}
		if (user.effects.Outrage > 0) {
			user.effects.Outrage -= 1;
			if (user.effects.Outrage == 0 && user.CanConfuseSelf(false)) {
				user.Confuse(_INTL("{1} became confused due to fatigue!", user.ToString()));
			}
		}
	}
}

//===============================================================================
// User must use this move for 4 more rounds. Power doubles each round.
// Power is also doubled if user has curled up. (Ice Ball, Rollout)
//===============================================================================
public partial class Battle.Move.MultiTurnAttackPowersUpEachTurn : Battle.Move {
	public void NumHits(user, targets) {return 1; }

	public void BaseDamage(baseDmg, user, target) {
		shift = (5 - user.effects.Rollout);   // 0-4, where 0 is most powerful
		if (user.effects.Rollout == 0) shift = 0;   // For first turn
		if (user.effects.DefenseCurl) shift += 1;
		baseDmg *= 2**shift;
		return baseDmg;
	}

	public void EffectAfterAllHits(user, target) {
		if (!target.damageState.unaffected && user.effects.Rollout == 0) {
			user.effects.Rollout = 5;
			user.currentMove = @id;
		}
		if (user.effects.Rollout > 0) user.effects.Rollout -= 1;
	}
}

//===============================================================================
// User bides its time this round and next round. The round after, deals 2x the
// total direct damage it took while biding to the last battler that damaged it.
// (Bide)
//===============================================================================
public partial class Battle.Move.MultiTurnAttackBideThenReturnDoubleDamage : Battle.Move.FixedDamageMove {
	public void AddTarget(targets, user) {
		if (user.effects.Bide != 1) return;   // Not the attack turn
		idxTarget = user.effects.BideTarget;
		t = (idxTarget >= 0) ? @battle.battlers[idxTarget] : null;
		if (!user.AddTarget(targets, user, t, self, false)) {
			user.AddTargetRandomFoe(targets, user, self, false);
		}
	}

	public bool MoveFailed(user, targets) {
		if (user.effects.Bide != 1) return false;   // Not the attack turn
		if (user.effects.BideDamage == 0) {
			@battle.Display(_INTL("But it failed!"));
			user.effects.Bide = 0;   // No need to reset other Bide variables
			return true;
		}
		if (targets.length == 0) {
			@battle.Display(_INTL("But there was no target..."));
			user.effects.Bide = 0;   // No need to reset other Bide variables
			return true;
		}
		return false;
	}

	public void OnStartUse(user, targets) {
		@damagingTurn = (user.effects.Bide == 1);   // If attack turn
	}

	public void DisplayUseMessage(user) {
		if (@damagingTurn) {   // Attack turn
			@battle.DisplayBrief(_INTL("{1} unleashed energy!", user.ToString()));
		} else if (user.effects.Bide > 1) {   // Charging turns
			@battle.DisplayBrief(_INTL("{1} is storing energy!", user.ToString()));
		} else {
			super;   // Start using Bide
		}
	}

	// Stops damage being dealt in the charging turns.
	public override bool DamagingMove() {
		if (!@damagingTurn) return false;
		return base.DamagingMove();
	}

	public void FixedDamage(user, target) {
		return user.effects.BideDamage * 2;
	}

	public void EffectGeneral(user) {
		if (user.effects.Bide == 0) {   // Starting using Bide
			user.effects.Bide       = 3;
			user.effects.BideDamage = 0;
			user.effects.BideTarget = -1;
			user.currentMove = @id;
		}
		user.effects.Bide -= 1;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (!@damagingTurn) hitNum = 1;   // Charging anim
		base.ShowAnimation();
	}
}
