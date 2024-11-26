//===============================================================================
// User flees from battle. (Teleport (Gen 7-))
//===============================================================================
public partial class Battle.Move.FleeFromBattle : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (!@battle.CanRun(user.index) || (user.wild() && user.allAllies.length > 0)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.Display(_INTL("{1} fled from battle!", user.ToString()));
		@battle.decision = Battle.Outcome.FLEE;
	}
}

//===============================================================================
// User switches out. If user is a wild Pokémon, ends the battle instead.
// (Teleport (Gen 8+))
//===============================================================================
public partial class Battle.Move.SwitchOutUserStatusMove : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.wild()) {
			if (!@battle.CanRun(user.index) || user.allAllies.length > 0) {
				@battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else if (!@battle.CanChooseNonActive(user.index)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		if (user.wild()) return;
		@battle.Display(_INTL("{1} went back to {2}!", user.ToString(),
														@battle.GetOwnerName(user.index)));
		@battle.Pursuit(user.index);
		if (user.fainted()) return;
		newPkmn = @battle.GetReplacementPokemonIndex(user.index);   // Owner chooses
		if (newPkmn < 0) return;
		@battle.RecallAndReplace(user.index, newPkmn);
		@battle.ClearChoice(user.index);   // Replacement Pokémon does nothing this round
		@battle.moldBreaker = false;
		@battle.OnBattlerEnteringBattle(user.index);
		switchedBattlers.Add(user.index);
	}

	public void EffectGeneral(user) {
		if (user.wild()) {
			@battle.Display(_INTL("{1} fled from battle!", user.ToString()));
			@battle.decision = Battle.Outcome.FLEE;
		}
	}
}

//===============================================================================
// After inflicting damage, user switches out. Ignores trapping moves.
// (Flip Turn, U-turn, Volt Switch)
//===============================================================================
public partial class Battle.Move.SwitchOutUserDamagingMove : Battle.Move {
	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		if (user.fainted() || numHits == 0 || @battle.AllFainted(user.idxOpposingSide)) return;
		targetSwitched = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!switchedBattlers.Contains(b.index)) targetSwitched = false;
		}
		if (targetSwitched) return;
		if (!@battle.CanChooseNonActive(user.index)) return;
		@battle.Display(_INTL("{1} went back to {2}!", user.ToString(),
														@battle.GetOwnerName(user.index)));
		@battle.Pursuit(user.index);
		if (user.fainted()) return;
		newPkmn = @battle.GetReplacementPokemonIndex(user.index);   // Owner chooses
		if (newPkmn < 0) return;
		@battle.RecallAndReplace(user.index, newPkmn);
		@battle.ClearChoice(user.index);   // Replacement Pokémon does nothing this round
		@battle.moldBreaker = false;
		@battle.OnBattlerEnteringBattle(user.index);
		switchedBattlers.Add(user.index);
	}
}

//===============================================================================
// Decreases the target's Attack and Special Attack by 1 stage each. Then, user
// switches out. Ignores trapping moves. (Parting Shot)
//===============================================================================
public partial class Battle.Move.LowerTargetAtkSpAtk1SwitchOutUser : Battle.Move.TargetMultiStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1, :SPECIAL_ATTACK, 1};
	}

	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		switcher = user;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (switchedBattlers.Contains(b.index)) continue;
			if (b.effects.MagicCoat || b.effects.MagicBounce) switcher = b;
		}
		if (switcher.fainted() || numHits == 0) return;
		if (!@battle.CanChooseNonActive(switcher.index)) return;
		@battle.Display(_INTL("{1} went back to {2}!", switcher.ToString(),
														@battle.GetOwnerName(switcher.index)));
		@battle.Pursuit(switcher.index);
		if (switcher.fainted()) return;
		newPkmn = @battle.GetReplacementPokemonIndex(switcher.index);   // Owner chooses
		if (newPkmn < 0) return;
		@battle.RecallAndReplace(switcher.index, newPkmn);
		@battle.ClearChoice(switcher.index);   // Replacement Pokémon does nothing this round
		if (switcher.index == user.index) @battle.moldBreaker = false;
		@battle.OnBattlerEnteringBattle(switcher.index);
		switchedBattlers.Add(switcher.index);
	}
}

//===============================================================================
// User switches out. Various effects affecting the user are passed to the
// replacement. (Baton Pass)
//===============================================================================
public partial class Battle.Move.SwitchOutUserPassOnEffects : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (!@battle.CanChooseNonActive(user.index)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		if (user.fainted() || numHits == 0) return;
		if (!@battle.CanChooseNonActive(user.index)) return;
		@battle.Pursuit(user.index);
		if (user.fainted()) return;
		newPkmn = @battle.GetReplacementPokemonIndex(user.index);   // Owner chooses
		if (newPkmn < 0) return;
		@battle.RecallAndReplace(user.index, newPkmn, false, true);
		@battle.ClearChoice(user.index);   // Replacement Pokémon does nothing this round
		@battle.moldBreaker = false;
		@battle.OnBattlerEnteringBattle(user.index);
		switchedBattlers.Add(user.index);
	}
}

//===============================================================================
// When used against a sole wild Pokémon, makes target flee and ends the battle;
// fails if target is a higher level than the user.
// When used against a trainer's Pokémon, target switches out.
// For status moves. (Roar, Whirlwind)
//===============================================================================
public partial class Battle.Move.SwitchOutTargetStatusMove : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.hasActiveAbility(Abilitys.SUCTIONCUPS) && !target.beingMoldBroken()) {
			if (show_message) {
				@battle.ShowAbilitySplash(target);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} anchors itself!", target.ToString()));
				} else {
					@battle.Display(_INTL("{1} anchors itself with {2}!", target.ToString(), target.abilityName));
				}
				@battle.HideAbilitySplash(target);
			}
			return true;
		}
		if (target.effects.Ingrain) {
			if (show_message) @battle.Display(_INTL("{1} anchored itself with its roots!", target.ToString()));
			return true;
		}
		if (target.wild() && target.allAllies.length == 0 && @battle.canRun) {
			// End the battle
			if (target.level > user.level) {
				if (show_message) @battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else if (!target.wild()) {
			// Switch target out
			canSwitch = false;
			@battle.eachInTeamFromBattlerIndex(target.index) do |_pkmn, i|
				canSwitch = @battle.CanSwitchIn(target.index, i);
				if (canSwitch) break;
			}
			if (!canSwitch) {
				if (show_message) @battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (target.wild()) @battle.decision = Battle.Outcome.FLEE;
	}

	public void SwitchOutTargetEffect(user, targets, numHits, switched_battlers) {
		if (!switched_battlers.empty()) return;
		if (user.fainted() || numHits == 0) return;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.fainted() || b.damageState.unaffected) continue;
			if (b.wild()) continue;
			if (b.effects.Ingrain) continue;
			if (b.hasActiveAbility(Abilitys.SUCTIONCUPS) && !b.beingMoldBroken()) continue;
			newPkmn = @battle.GetReplacementPokemonIndex(b.index, true);   // Random
			if (newPkmn < 0) continue;
			@battle.RecallAndReplace(b.index, newPkmn, true);
			@battle.Display(_INTL("{1} was dragged out!", b.ToString()));
			@battle.ClearChoice(b.index);   // Replacement Pokémon does nothing this round
			@battle.OnBattlerEnteringBattle(b.index);
			switched_battlers.Add(b.index);
			break;
		}
	}
}

//===============================================================================
// When used against a sole wild Pokémon, makes target flee and ends the battle;
// fails if target is a higher level than the user.
// When used against a trainer's Pokémon, target switches out.
// For damaging moves. (Circle Throw, Dragon Tail)
//===============================================================================
public partial class Battle.Move.SwitchOutTargetDamagingMove : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		if ((target.wild() && target.allAllies.length == 0 && @battle.canRun &&
			target.level <= user.level &&
			(target.effects.Substitute == 0 || ignoresSubstitute(user))) {
			@battle.decision = Battle.Outcome.FLEE;
		}
	}

	public void SwitchOutTargetEffect(user, targets, numHits, switched_battlers) {
		if (!switched_battlers.empty()) return;
		if (user.fainted() || numHits == 0) return;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.fainted() || b.damageState.unaffected || b.damageState.substitute) continue;
			if (b.wild()) continue;
			if (b.effects.Ingrain) continue;
			if (b.hasActiveAbility(Abilitys.SUCTIONCUPS) && !b.beingMoldBroken()) continue;
			newPkmn = @battle.GetReplacementPokemonIndex(b.index, true);   // Random
			if (newPkmn < 0) continue;
			@battle.RecallAndReplace(b.index, newPkmn, true);
			@battle.Display(_INTL("{1} was dragged out!", b.ToString()));
			@battle.ClearChoice(b.index);   // Replacement Pokémon does nothing this round
			@battle.OnBattlerEnteringBattle(b.index);
			switched_battlers.Add(b.index);
			break;
		}
	}
}

//===============================================================================
// Trapping move. Traps for 5 or 6 rounds. Trapped Pokémon lose 1/16 of max HP
// at end of each round.
//===============================================================================
public partial class Battle.Move.BindTarget : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		if (target.fainted() || target.damageState.substitute) return;
		if (target.effects.Trapping > 0) return;
		// Set trapping effect duration and info
		if (user.hasActiveItem(Items.GRIPCLAW)) {
			target.effects.Trapping = (Settings.MECHANICS_GENERATION >= 5) ? 8 : 6;
		} else {
			target.effects.Trapping = 5 + @battle.Random(2);
		}
		target.effects.TrappingMove = @id;
		target.effects.TrappingUser = user.index;
		// Message
		msg = _INTL("{1} was trapped in the vortex!", target.ToString());
		switch (@id) {
			case :BIND:
				msg = _INTL("{1} was squeezed by {2}!", target.ToString(), user.ToString(true));
				break;
			case :CLAMP:
				msg = _INTL("{1} clamped {2}!", user.ToString(), target.ToString(true));
				break;
			case :FIRESPIN:
				msg = _INTL("{1} was trapped in the fiery vortex!", target.ToString());
				break;
			case :INFESTATION:
				msg = _INTL("{1} has been afflicted with an infestation by {2}!", target.ToString(), user.ToString(true));
				break;
			case :MAGMASTORM:
				msg = _INTL("{1} became trapped by Magma Storm!", target.ToString());
				break;
			case :SANDTOMB:
				msg = _INTL("{1} became trapped by Sand Tomb!", target.ToString());
				break;
			case :WHIRLPOOL:
				msg = _INTL("{1} became trapped in the vortex!", target.ToString());
				break;
			case :WRAP:
				msg = _INTL("{1} was wrapped by {2}!", target.ToString(), user.ToString(true));
				break;
		}
		@battle.Display(msg);
	}
}

//===============================================================================
// Trapping move. Traps for 5 or 6 rounds. Trapped Pokémon lose 1/16 of max HP
// at end of each round. (Whirlpool)
// Power is doubled if target is using Dive. Hits some semi-invulnerable targets.
//===============================================================================
public partial class Battle.Move.BindTargetDoublePowerIfTargetUnderwater : Battle.Move.BindTarget {
	public bool hitsDivingTargets() { return true; }

	public void ModifyDamage(damageMult, user, target) {
		if (target.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderwater")) damageMult *= 2;
		return damageMult;
	}
}

//===============================================================================
// Target can no longer switch out or flee, as long as the user remains active.
// Trapping is considered an additional effect for damaging moves.
// (Anchor Shot, Block, Mean Look, Spider Web, Spirit Shackle)
//===============================================================================
public partial class Battle.Move.TrapTargetInBattle : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		if (target.effects.MeanLook >= 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (Settings.MORE_TYPE_EFFECTS && target.Type == Types.GHOST) {
			if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.effects.MeanLook = user.index;
		@battle.Display(_INTL("{1} can no longer escape!", target.ToString()));
	}

	public void AdditionalEffect(user, target) {
		if (target.fainted() || target.damageState.substitute) return;
		if (target.effects.MeanLook >= 0) return;
		if (Settings.MORE_TYPE_EFFECTS && target.Type == Types.GHOST) return;
		target.effects.MeanLook = user.index;
		@battle.Display(_INTL("{1} can no longer escape!", target.ToString()));
	}
}

//===============================================================================
// Target can no longer switch out or flee, as long as the user remains active.
// Trapping is not considered an additional effect. (Thousand Waves)
//===============================================================================
public partial class Battle.Move.TrapTargetInBattleMainEffect : Battle.Move {
	public bool canMagicCoat() { return true; }

	public void EffectAgainstTarget(user, target) {
		if (target.fainted() || target.damageState.substitute) return;
		if (target.effects.MeanLook >= 0) return;
		if (Settings.MORE_TYPE_EFFECTS && target.Type == Types.GHOST) return;
		target.effects.MeanLook = user.index;
		@battle.Display(_INTL("{1} can no longer escape!", target.ToString()));
	}
}

//===============================================================================
// The target can no longer switch out or flee, while the user remains in battle.
// At the end of each round, the target's Defense and Special Defense are lowered
// by 1 stage each. (Octolock)
//===============================================================================
public partial class Battle.Move.TrapTargetInBattleLowerTargetDefSpDef1EachTurn : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		if (target.effects.Octolock >= 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (Settings.MORE_TYPE_EFFECTS && target.Type == Types.GHOST) {
			if (show_message) @battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Octolock = user.index;
		@battle.Display(_INTL("{1} can no longer escape because of {2}!", target.ToString(), @name));
	}
}

//===============================================================================
// Prevents the user and the target from switching out or fleeing. This effect
// isn't applied if either Pokémon is already prevented from switching out or
// fleeing. (Jaw Lock)
//===============================================================================
public partial class Battle.Move.TrapUserAndTargetInBattle : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		if (user.fainted() || target.fainted() || target.damageState.substitute) return;
		if (Settings.MORE_TYPE_EFFECTS && target.Type == Types.GHOST) return;
		if (user.trappedInBattle() || target.trappedInBattle()) return;
		target.effects.JawLock = user.index;
		@battle.Display(_INTL("Neither Pokémon can run away!"));
	}
}

//===============================================================================
// No Pokémon can switch out or flee until the end of the next round. (Fairy Lock)
//===============================================================================
public partial class Battle.Move.TrapAllBattlersInBattleForOneTurn : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.field.effects.FairyLock > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.field.effects.FairyLock = 2;
		@battle.Display(_INTL("No one will be able to run away during the next turn!"));
	}
}

//===============================================================================
// Interrupts a foe switching out or using U-turn/Volt Switch/Parting Shot. Power
// is doubled in that case. (Pursuit)
// (Handled in Battle's AttackPhase): Makes this attack happen before switching.
//===============================================================================
public partial class Battle.Move.PursueSwitchingFoe : Battle.Move {
	public override void AccuracyCheck(user, target) {
		if (@battle.switching) return true;
		return base.AccuracyCheck();
	}

	public void BaseDamage(baseDmg, user, target) {
		if (@battle.switching) baseDmg *= 2;
		return baseDmg;
	}
}

//===============================================================================
// Fails if user has not been hit by an opponent's physical move this round.
// (Shell Trap)
//===============================================================================
public partial class Battle.Move.UsedAfterUserTakesPhysicalDamage : Battle.Move {
	public void DisplayChargeMessage(user) {
		user.effects.ShellTrap = true;
		@battle.CommonAnimation("ShellTrap", user);
		@battle.Display(_INTL("{1} set a shell trap!", user.ToString()));
	}

	public override void DisplayUseMessage(user) {
		if (user.tookPhysicalHit) base.DisplayUseMessage();
	}

	public bool MoveFailed(user, targets) {
		if (!user.effects.ShellTrap) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (!user.tookPhysicalHit) {
			@battle.Display(_INTL("{1}'s shell trap didn't work!", user.ToString()));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Power is doubled if a user's ally has already used this move this round. (Round)
// If an ally is about to use the same move, make it go next, ignoring priority.
//===============================================================================
public partial class Battle.Move.UsedAfterAllyRoundWithDoublePower : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (user.OwnSide.effects.Round) baseDmg *= 2;
		return baseDmg;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.Round = true;
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (@battle.choices[b.index].Action != :UseMove || b.movedThisRound()) continue;
			if (@battle.choices[b.index].Move.function_code != @function_code) continue;
			b.effects.MoveNext = true;
			b.effects.Quash    = 0;
			break;
		}
	}
}

//===============================================================================
// Target moves immediately after the user, ignoring priority/speed. (After You)
//===============================================================================
public partial class Battle.Move.TargetActsNext : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		// Target has already moved this round
		if (MoveFailedTargetAlreadyMoved(target, show_message)) return true;
		// Target was going to move next anyway (somehow)
		if (target.effects.MoveNext) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		// Target didn't choose to use a move this round
		oppMove = @battle.choices[target.index].Move;
		if (!oppMove) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.MoveNext = true;
		target.effects.Quash    = 0;
		@battle.Display(_INTL("{1} took the kind offer!", target.ToString()));
	}
}

//===============================================================================
// Target moves last this round, ignoring priority/speed. (Quash)
//===============================================================================
public partial class Battle.Move.TargetActsLast : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (MoveFailedTargetAlreadyMoved(target, show_message)) return true;
		// Target isn't going to use a move
		oppMove = @battle.choices[target.index].Move;
		if (!oppMove) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		// Target is already maximally Quashed and will move last anyway
		highestQuash = 0;
		@battle.allBattlers.each do |b|
			if (b.effects.Quash <= highestQuash) continue;
			highestQuash = b.effects.Quash;
		}
		if (highestQuash > 0 && target.effects.Quash == highestQuash) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		// Target was already going to move last
		if (highestQuash == 0 && @battle.Priority.last.index == target.index) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		highestQuash = 0;
		@battle.allBattlers.each do |b|
			if (b.effects.Quash <= highestQuash) continue;
			highestQuash = b.effects.Quash;
		}
		target.effects.Quash    = highestQuash + 1;
		target.effects.MoveNext = false;
		@battle.Display(_INTL("{1}'s move was postponed!", target.ToString()));
	}
}

//===============================================================================
// The target uses its most recent move again. (Instruct)
//===============================================================================
public partial class Battle.Move.TargetUsesItsLastUsedMoveAgain : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"MultiTurnAttackBideThenReturnDoubleDamage",       // Bide
			"ProtectUserFromDamagingMovesKingsShield",         // King's Shield
			"TargetUsesItsLastUsedMoveAgain",                  // Instruct (this move)
			// Struggle
			"Struggle",                                        // Struggle
			// Moves that affect the moveset
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",     // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",               // Sketch
			"TransformUserIntoTarget",                         // Transform
			// Moves that call other moves
			"UseLastMoveUsedByTarget",                         // Mirror Move
			"UseLastMoveUsed",                                 // Copycat
			"UseMoveTargetIsAboutToUse",                       // Me First
			"UseMoveDependingOnEnvironment",                   // Nature Power
			"UseRandomUserMoveIfAsleep",                       // Sleep Talk
			"UseRandomMoveFromUserParty",                      // Assist
			"UseRandomMove",                                   // Metronome
			// Moves that require a recharge turn
			"AttackAndSkipNextTurn",                           // Hyper Beam
			// Two-turn attacks
			"TwoTurnAttack",                                   // Razor Wind
			"TwoTurnAttackOneTurnInSun",                       // Solar Beam, Solar Blade
			"TwoTurnAttackParalyzeTarget",                     // Freeze Shock
			"TwoTurnAttackBurnTarget",                         // Ice Burn
			"TwoTurnAttackFlinchTarget",                       // Sky Attack
			"TwoTurnAttackChargeRaiseUserDefense1",            // Skull Bash
			"TwoTurnAttackInvulnerableInSky",                  // Fly
			"TwoTurnAttackInvulnerableUnderground",            // Dig
			"TwoTurnAttackInvulnerableUnderwater",             // Dive
			"TwoTurnAttackInvulnerableInSkyParalyzeTarget",    // Bounce
			"TwoTurnAttackInvulnerableRemoveProtections",      // Shadow Force, Phantom Force
			"TwoTurnAttackInvulnerableInSkyTargetCannotAct",   // Sky Drop
			"AllBattlersLoseHalfHPUserSkipsNextTurn",          // Shadow Half
			"TwoTurnAttackRaiseUserSpAtkSpDefSpd2",            // Geomancy
			// Moves that start focussing at the start of the round
			"FailsIfUserDamagedThisTurn",                      // Focus Punch
			"UsedAfterUserTakesPhysicalDamage",                // Shell Trap
			"BurnAttackerBeforeUserActs";                       // Beak Blast
		}
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if ((!target.lastRegularMoveUsed || !target.HasMove(target.lastRegularMoveUsed) ||
			!GameData.Move.exists(target.lastRegularMoveUsed)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.usingMultiTurnAttack()) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		targetMove = @battle.choices[target.index].Move;
		if (targetMove && (targetMove.function_code == "FailsIfUserDamagedThisTurn" ||   // Focus Punch
											targetMove.function_code == "UsedAfterUserTakesPhysicalDamage" ||   // Shell Trap) {
											targetMove.function_code == "BurnAttackerBeforeUserActs");    // Beak Blast
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (@moveBlacklist.Contains(GameData.Move.get(target.lastRegularMoveUsed).function_code)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		idxMove = -1;
		target.eachMoveWithIndex do |m, i|
			if (m.id == target.lastRegularMoveUsed) idxMove = i;
		}
		if (target.moves[idxMove].pp == 0 && target.moves[idxMove].total_pp > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Instruct = true;
	}
}

//===============================================================================
// For 5 rounds, for each priority bracket, slow Pokémon move before fast ones.
// (Trick Room)
//===============================================================================
public partial class Battle.Move.StartSlowerBattlersActFirst : Battle.Move {
	public void EffectGeneral(user) {
		if (@battle.field.effects.TrickRoom > 0) {
			@battle.field.effects.TrickRoom = 0;
			@battle.Display(_INTL("{1} reverted the dimensions!", user.ToString()));
		} else {
			@battle.field.effects.TrickRoom = 5;
			@battle.Display(_INTL("{1} twisted the dimensions!", user.ToString()));
		}
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@battle.field.effects.TrickRoom > 0) return;   // No animation
		base.ShowAnimation();
	}
}

//===============================================================================
// If Grassy Terrain applies, priority is increased by 1. (Grassy Glide)
//===============================================================================
public partial class Battle.Move.HigherPriorityInGrassyTerrain : Battle.Move {
	public override void Priority(user) {
		ret = base.Priority();
		if (@battle.field.terrain == :Grassy && user.affectedByTerrain()) ret += 1;
		return ret;
	}
}

//===============================================================================
// Target's last move used loses 3 PP. Damaging move. (Eerie Spell)
//===============================================================================
public partial class Battle.Move.LowerPPOfTargetLastMoveBy3 : Battle.Move {
	public void AdditionalEffect(user, target) {
		if (target.fainted() || target.damageState.substitute) return;
		last_move = target.GetMoveWithID(target.lastRegularMoveUsed);
		if (!last_move || last_move.pp == 0 || last_move.total_pp <= 0) return;
		reduction = (int)Math.Min(3, last_move.pp);
		target.SetPP(last_move, last_move.pp - reduction);
		@battle.Display(_INTL("It reduced the PP of {1}'s {2} by {3}!",
														target.ToString(true), last_move.name, reduction));
	}
}

//===============================================================================
// Target's last move used loses 4 PP. Status move. (Spite)
//===============================================================================
public partial class Battle.Move.LowerPPOfTargetLastMoveBy4 : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		last_move = target.GetMoveWithID(target.lastRegularMoveUsed);
		if (!last_move || last_move.pp == 0 || last_move.total_pp <= 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		last_move = target.GetMoveWithID(target.lastRegularMoveUsed);
		reduction = (int)Math.Min(4, last_move.pp);
		target.SetPP(last_move, last_move.pp - reduction);
		@battle.Display(_INTL("It reduced the PP of {1}'s {2} by {3}!",
														target.ToString(true), last_move.name, reduction));
	}
}

//===============================================================================
// For 5 rounds, disables the last move the target used. (Disable)
//===============================================================================
public partial class Battle.Move.DisableTargetLastMoveUsed : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Disable > 0 || !target.lastRegularMoveUsed) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedAromaVeil(user, target, show_message)) return true;
		canDisable = false;
		foreach (var m in target.Moves) { //target.eachMove do => |m|
			if (m.id != target.lastRegularMoveUsed) continue;
			if (m.pp == 0 && m.total_pp > 0) continue;
			canDisable = true;
			break;
		}
		if (!canDisable) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Disable     = 5;
		target.effects.DisableMove = target.lastRegularMoveUsed;
		@battle.Display(_INTL("{1}'s {2} was disabled!", target.ToString(),
														GameData.Move.get(target.lastRegularMoveUsed).name));
		target.ItemStatusCureCheck;
	}
}

//===============================================================================
// The target can no longer use the same move twice in a row. (Torment)
// NOTE: Torment is only supposed to start applying at the end of the round in
//       which it is used, unlike Taunt which starts applying immediately. I've
//       decided to make Torment apply immediately.
//===============================================================================
public partial class Battle.Move.DisableTargetUsingSameMoveConsecutively : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Torment) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedAromaVeil(user, target, show_message)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Torment = true;
		@battle.Display(_INTL("{1} was subjected to torment!", target.ToString()));
		target.ItemStatusCureCheck;
	}
}

//===============================================================================
// For 4 rounds, the target must use the same move each round. (Encore)
//===============================================================================
public partial class Battle.Move.DisableTargetUsingDifferentMove : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"DisableTargetUsingDifferentMove",               // Encore
			// Struggle
			"Struggle",                                      // Struggle
			// Moves that affect the moveset
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",   // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",             // Sketch
			"TransformUserIntoTarget",                       // Transform
			// Moves that call other moves (see also below)
			"UseLastMoveUsedByTarget";                        // Mirror Move
		}
		if (Settings.MECHANICS_GENERATION >= 7) {
			@moveBlacklist += new {
				// Moves that call other moves
//        "UseLastMoveUsedByTarget",                    // Mirror Move   // See above
				"UseLastMoveUsed",                             // Copycat
				"UseMoveTargetIsAboutToUse",                   // Me First
				"UseMoveDependingOnEnvironment",               // Nature Power
				"UseRandomUserMoveIfAsleep",                   // Sleep Talk
				"UseRandomMoveFromUserParty",                  // Assist
				"UseRandomMove";                                // Metronome
			}
		}
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Encore > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if ((!target.lastRegularMoveUsed ||
			!GameData.Move.exists(target.lastRegularMoveUsed) ||
			@moveBlacklist.Contains(GameData.Move.get(target.lastRegularMoveUsed).function_code)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.effects.ShellTrap) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedAromaVeil(user, target, show_message)) return true;
		canEncore = false;
		foreach (var m in target.Moves) { //target.eachMove do => |m|
			if (m.id != target.lastRegularMoveUsed) continue;
			if (m.pp == 0 && m.total_pp > 0) continue;
			canEncore = true;
			break;
		}
		if (!canEncore) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Encore     = 4;
		target.effects.EncoreMove = target.lastRegularMoveUsed;
		@battle.Display(_INTL("{1} received an encore!", target.ToString()));
		target.ItemStatusCureCheck;
	}
}

//===============================================================================
// For 4 rounds, disables the target's non-damaging moves. (Taunt)
//===============================================================================
public partial class Battle.Move.DisableTargetStatusMoves : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Taunt > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedAromaVeil(user, target, show_message)) return true;
		if ((Settings.MECHANICS_GENERATION >= 6 && target.hasActiveAbility(Abilitys.OBLIVIOUS) &&
			!target.beingMoldBroken()) {
			if (show_message) {
				@battle.ShowAbilitySplash(target);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("But it failed!"));
				} else {
					@battle.Display(_INTL("But it failed because of {1}'s {2}!",
																	target.ToString(true), target.abilityName));
				}
				@battle.HideAbilitySplash(target);
			}
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Taunt = 4;
		@battle.Display(_INTL("{1} fell for the taunt!", target.ToString()));
		target.ItemStatusCureCheck;
	}
}

//===============================================================================
// For 5 rounds, disables the target's healing moves. (Heal Block)
//===============================================================================
public partial class Battle.Move.DisableTargetHealingMoves : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.HealBlock > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedAromaVeil(user, target, show_message)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.HealBlock = 5;
		@battle.Display(_INTL("{1} was prevented from healing!", target.ToString()));
		target.ItemStatusCureCheck;
	}
}

//===============================================================================
// Target cannot use sound-based moves for 2 more rounds. (Throat Chop)
//===============================================================================
public partial class Battle.Move.DisableTargetSoundMoves : Battle.Move {
	public void AdditionalEffect(user, target) {
		if (target.fainted() || target.damageState.substitute) return;
		if (target.effects.ThroatChop == 0) {
			@battle.Display(_INTL("The effects of {1} prevent {2} from using certain moves!",
															@name, target.ToString(true)));
		}
		target.effects.ThroatChop = 3;
	}
}

//===============================================================================
// Disables all target's moves that the user also knows. (Imprison)
//===============================================================================
public partial class Battle.Move.DisableTargetMovesKnownByUser : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.Imprison) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.Imprison = true;
		@battle.Display(_INTL("{1} sealed any moves its target shares with it!", user.ToString()));
	}
}
