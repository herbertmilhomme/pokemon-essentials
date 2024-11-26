//===============================================================================
//
//===============================================================================
public partial class Battle.Move {
	//-----------------------------------------------------------------------------
	// Effect methods per move usage.
	//-----------------------------------------------------------------------------

	public bool CanChooseMove(user, commandPhase, showMessages) {  return true; }   // For Belch
	public void DisplayChargeMessage(user) {}   // For Focus Punch/shell Trap/Beak Blast
	public void OnStartUse(user, targets) {}
	public void AddTarget(targets, user) {}   // For Counter, etc. and Bide
	public void ModifyTargets(targets, user) {}   // For Dragon Darts

	// Reset move usage counters (child classes can increment them).
	public void ChangeUsageCounters(user, specialUsage) {
		user.effects.FuryCutter   = 0;
		user.effects.ParentalBond = 0;
		user.effects.ProtectRate  = 1;
		@battle.field.effects.FusionBolt  = false;
		@battle.field.effects.FusionFlare = false;
	}

	public void DisplayUseMessage(user) {
		@battle.DisplayBrief(_INTL("{1} used {2}!", user.ToString(), @name));
	}

	public bool ShowFailMessages(targets) {  return true; }
	public void MissMessage(user, target) {return false; }

	//-----------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------

	// Whether the move is currently in the "charging" turn of a two-turn move.
	// Is false if Power Herb or another effect lets a two-turn move charge and
	// attack in the same turn.
	// user.effects.TwoTurnAttack is set to the move's ID during the
	// charging turn, and is null during the attack turn.
	public bool IsChargingTurn(user) {  return false; }
	public bool DamagingMove() { return damagingMove(); }

	public bool ContactMove(user) {
		if (user.hasActiveAbility(Abilitys.LONGREACH)) return false;
		if (punchingMove() && user.hasActiveItem(Items.PUNCHINGGLOVE)) return false;
		return contactMove();
	}

	// The maximum number of hits in a round this move will actually perform. This
	// can be 1 for Beat Up, and can be 2 for any moves affected by Parental Bond.
	public void NumHits(user, targets) {
		if (user.hasActiveAbility(Abilitys.PARENTALBOND) && DamagingMove() &&
			!chargingTurnMove() && targets.length == 1) {
			// Record that Parental Bond applies, to weaken the second attack
			user.effects.ParentalBond = 3;
			return 2;
		}
		return 1;
	}

	// For two-turn moves when they charge and attack in the same turn.
	public void QuickChargingMove(user, targets) {}

	//-----------------------------------------------------------------------------
	// Effect methods per hit.
	//-----------------------------------------------------------------------------

	public void OverrideSuccessCheckPerHit(user, target) {return false; }
	public void CrashDamage(user) {}
	public void InitialEffect(user, targets, hitNum) {}
	public void DesignateTargetsForHit(targets, hitNum) {return targets; }   // For Dragon Darts
	public bool RepeatHit() { return false; }   // For Dragon Darts

	public void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (!showAnimation) return;
		if (user.effects.ParentalBond == 1) {
			@battle.CommonAnimation("ParentalBond", user, targets);
		} else {
			@battle.Animation(id, user, targets, hitNum);
		}
	}

	public void SelfKO(user) {}
	public void EffectWhenDealingDamage(user, target) {}
	public void EffectAgainstTarget(user, target) {}
	public void EffectGeneral(user) {}
	public void AdditionalEffect(user, target) {}
	public void EffectAfterAllHits(user, target) {}   // Move effects that occur after all hits
	public void SwitchOutTargetEffect(user, targets, numHits, switched_battlers) {}
	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {}

	//-----------------------------------------------------------------------------
	// Check if target is immune to the move because of its ability.
	//-----------------------------------------------------------------------------

	public void ImmunityByAbility(user, target, show_message) {
		ret = false;
		if (target.abilityActive() && !target.beingMoldBroken()) {
			ret = Battle.AbilityEffects.triggerMoveImmunity(target.ability, user, target,
																											self, @calcType, @battle, show_message);
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Move failure checks.
	//-----------------------------------------------------------------------------

	// Check whether the move fails completely due to move-specific requirements.
	public bool MoveFailed(user, targets) {  return false; }
	// Checks whether the move will be ineffective against the target.
	public bool FailsAgainstTarget(user, target, show_message) {  return false; }

	public bool MoveFailedLastInRound(user, showMessage = true) {
		unmoved = @battle.allBattlers.any() do |b|
			next b.index != user.index &&
					new []{:UseMove, :Shift}.Contains(@battle.choices[b.index].Action) &&
					!b.movedThisRound();
		}
		if (!unmoved) {
			if (showMessage) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool MoveFailedTargetAlreadyMoved(target, showMessage = true) {
		if ((@battle.choices[target.index].Action != :UseMove &&
			@battle.choices[target.index].Action != :Shift) || target.movedThisRound()) {
			if (showMessage) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool MoveFailedAromaVeil(user, target, showMessage = true) {
		if (target.hasActiveAbility(Abilitys.AROMAVEIL) && !target.beingMoldBroken()) {
			if (showMessage) {
				@battle.ShowAbilitySplash(target);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} is unaffected!", target.ToString()));
				} else {
					@battle.Display(_INTL("{1} is unaffected because of its {2}!",
																	target.ToString(), target.abilityName));
				}
				@battle.HideAbilitySplash(target);
			}
			return true;
		}
		foreach (var b in target.allAllies) { //'target.allAllies.each' do => |b|
			if (!b.hasActiveAbility(Abilitys.AROMAVEIL) || b.beingMoldBroken()) continue;
			if (showMessage) {
				@battle.ShowAbilitySplash(b);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} is unaffected!", target.ToString()));
				} else {
					@battle.Display(_INTL("{1} is unaffected because of {2}'s {3}!",
																	target.ToString(), b.ToString(true), b.abilityName));
				}
				@battle.HideAbilitySplash(b);
			}
			return true;
		}
		return false;
	}

	//-----------------------------------------------------------------------------
	// Weaken the damage dealt (doesn't actually change a battler's HP).
	//-----------------------------------------------------------------------------

	public void CheckDamageAbsorption(user, target) {
		// Substitute will take the damage
		if (target.effects.Substitute > 0 && !ignoresSubstitute(user) &&
			(!user || user.index != target.index)) {
			target.damageState.substitute = true;
			return;
		}
		// Ice Face will take the damage
		if (!target.beingMoldBroken() && target.isSpecies(Speciess.EISCUE) &&
			target.form == 0 && target.ability == abilitys.ICEFACE && physicalMove()) {
			target.damageState.iceFace = true;
			return;
		}
		// Disguise will take the damage
		if (!target.beingMoldBroken() && target.isSpecies(Speciess.MIMIKYU) &&
			target.form == 0 && target.ability == abilitys.DISGUISE) {
			target.damageState.disguise = true;
			return;
		}
	}

	public void ReduceDamage(user, target) {
		damage = target.damageState.calcDamage;
		// Substitute takes the damage
		if (target.damageState.substitute) {
			if (damage > target.effects.Substitute) damage = target.effects.Substitute;
			target.damageState.hpLost       = damage;
			target.damageState.totalHPLost += damage;
			return;
		}
		// Disguise/Ice Face takes the damage
		if (target.damageState.disguise || target.damageState.iceFace) return;
		// Target takes the damage
		if (damage >= target.hp) {
			damage = target.hp;
			// Survive a lethal hit with 1 HP effects
			if (nonLethal(user, target)) {
				damage -= 1;
			} else if (target.effects.Endure) {
				target.damageState.endured = true;
				damage -= 1;
			} else if (damage == target.totalhp) {
				if (target.hasActiveAbility(Abilitys.STURDY) && !target.beingMoldBroken()) {
					target.damageState.sturdy = true;
					damage -= 1;
				} else if (target.hasActiveItem(Items.FOCUSSASH) && target.hp == target.totalhp) {
					target.damageState.focusSash = true;
					damage -= 1;
				} else if (target.hasActiveItem(Items.FOCUSBAND) && @battle.Random(100) < 10) {
					target.damageState.focusBand = true;
					damage -= 1;
				} else if (Settings.AFFECTION_EFFECTS && @battle.internalBattle &&
							target.OwnedByPlayer() && !target.mega()) {
					chance = new {0, 0, 0, 10, 15, 25}[target.affection_level];
					if (chance > 0 && @battle.Random(100) < chance) {
						target.damageState.affection_endured = true;
						damage -= 1;
					}
				}
			}
		}
		if (damage < 0) damage = 0;
		target.damageState.hpLost       = damage;
		target.damageState.totalHPLost += damage;
	}

	//-----------------------------------------------------------------------------
	// Change the target's HP by the amount calculated above.
	//-----------------------------------------------------------------------------

	public void InflictHPDamage(target) {
		if (target.damageState.substitute) {
			target.effects.Substitute -= target.damageState.hpLost;
		} else if (target.damageState.hpLost > 0) {
			target.ReduceHP(target.damageState.hpLost, false, true, false);
		}
	}

	//-----------------------------------------------------------------------------
	// Animate the damage dealt, including lowering the HP.
	//-----------------------------------------------------------------------------

	// Animate being damaged and losing HP (by a move)
	public void AnimateHitAndHPLost(user, targets) {
		// Animate allies first, then foes
		animArray = new List<string>();
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|   // side here means "allies first, then foes"
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected || b.damageState.hpLost == 0) continue;
				if ((side == 0 && b.opposes(user)) || (side == 1 && !b.opposes(user))) continue;
				oldHP = b.hp;
				if (b.damageState.substitute) {
					old_sub_hp = b.effects.Substitute + b.damageState.hpLost;
					Debug.Log($"[Substitute HP change] {b.ToString()}'s substitute lost {b.damageState.hpLost} HP ({old_sub_hp} -> {b.effects.Substitute})");
				} else {
					oldHP += b.damageState.hpLost;
				}
				effectiveness = 0;
				if (Effectiveness.resistant(b.damageState.typeMod)) {
					effectiveness = 1;
				} else if (Effectiveness.super_effective(b.damageState.typeMod)) {
					effectiveness = 2;
				}
				animArray.Add(new {b, oldHP, effectiveness});
			}
			if (animArray.length > 0) {
				@battle.scene.HitAndHPLossAnimation(animArray);
				animArray.clear;
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Messages upon being hit.
	//-----------------------------------------------------------------------------

	public void EffectivenessMessage(user, target, numTargets = 1) {
		if (self.is_a(Battle.Move.FixedDamageMove)) return;
		if (target.damageState.disguise || target.damageState.iceFace) return;
		if (Effectiveness.super_effective(target.damageState.typeMod)) {
			if (numTargets > 1) {
				@battle.Display(_INTL("It's super effective on {1}!", target.ToString(true)));
			} else {
				@battle.Display(_INTL("It's super effective!"));
			}
		} else if (Effectiveness.not_very_effective(target.damageState.typeMod)) {
			if (numTargets > 1) {
				@battle.Display(_INTL("It's not very effective on {1}...", target.ToString(true)));
			} else {
				@battle.Display(_INTL("It's not very effective..."));
			}
		}
	}

	public void HitEffectivenessMessages(user, target, numTargets = 1) {
		if (target.damageState.disguise || target.damageState.iceFace) return;
		if (target.damageState.substitute) {
			@battle.Display(_INTL("The substitute took damage for {1}!", target.ToString(true)));
		}
		if (target.damageState.critical) {
			if (user.pokemon.isSpecies(Speciess.FARFETCHD) && user.pokemon.form == 1) {
				user.pokemon.evolution_counter += 1;
			}
			if (target.damageState.affection_critical) {
				if (numTargets > 1) {
					@battle.Display(_INTL("{1} landed a critical hit on {2}, wishing to be praised!",
																	user.ToString(), target.ToString(true)));
				} else {
					@battle.Display(_INTL("{1} landed a critical hit, wishing to be praised!", user.ToString()));
				}
			} else if (numTargets > 1) {
				@battle.Display(_INTL("A critical hit on {1}!", target.ToString(true)));
			} else {
				@battle.Display(_INTL("A critical hit!"));
			}
		}
		// Effectiveness message, for moves with 1 hit
		if (!multiHitMove() && user.effects.ParentalBond == 0) {
			EffectivenessMessage(user, target, numTargets);
		}
		if (target.damageState.substitute && target.effects.Substitute == 0) {
			target.effects.Substitute = 0;
			@battle.Display(_INTL("{1}'s substitute faded!", target.ToString()));
		}
	}

	public void EndureKOMessage(target) {
		if (target.damageState.disguise) {
			@battle.ShowAbilitySplash(target);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("Its disguise served it as a decoy!"));
			} else {
				@battle.Display(_INTL("{1}'s disguise served it as a decoy!", target.ToString()));
			}
			@battle.HideAbilitySplash(target);
			target.ChangeForm(1, _INTL("{1}'s disguise was busted!", target.ToString()));
			if (Settings.MECHANICS_GENERATION >= 8) target.ReduceHP(target.totalhp / 8, false);
		} else if (target.damageState.iceFace) {
			@battle.ShowAbilitySplash(target);
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1}'s {2} activated!", target.ToString(), target.abilityName));
			}
			target.ChangeForm(1, _INTL("{1} transformed!", target.ToString()));
			@battle.HideAbilitySplash(target);
		} else if (target.damageState.endured) {
			@battle.Display(_INTL("{1} endured the hit!", target.ToString()));
		} else if (target.damageState.sturdy) {
			@battle.ShowAbilitySplash(target);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1} endured the hit!", target.ToString()));
			} else {
				@battle.Display(_INTL("{1} hung on with Sturdy!", target.ToString()));
			}
			@battle.HideAbilitySplash(target);
		} else if (target.damageState.focusSash) {
			@battle.CommonAnimation("UseItem", target);
			@battle.Display(_INTL("{1} hung on using its Focus Sash!", target.ToString()));
			target.ConsumeItem;
		} else if (target.damageState.focusBand) {
			@battle.CommonAnimation("UseItem", target);
			@battle.Display(_INTL("{1} hung on using its Focus Band!", target.ToString()));
		} else if (target.damageState.affection_endured) {
			@battle.Display(_INTL("{1} toughed it out so you wouldn't feel sad!", target.ToString()));
		}
	}

	// Used by Counter/Mirror Coat/Metal Burst/Revenge/Focus Punch/Bide/Assurance.
	public void RecordDamageLost(user, target) {
		damage = target.damageState.hpLost;
		// NOTE: In Gen 3 where a move's category depends on its type, Hidden Power
		//       is for some reason countered by Counter rather than Mirror Coat,
		//       regardless of its calculated type. Hence the following two lines of
		//       code.
		moveType = null;
		if (@function_code == "TypeDependsOnUserIVs") moveType = Types.NORMAL;   // Hidden Power
		if (!target.damageState.substitute) {
			if (physicalMove(moveType)) {
				target.effects.Counter       = damage;
				target.effects.CounterTarget = user.index;
			} else if (specialMove(moveType)) {
				target.effects.MirrorCoat       = damage;
				target.effects.MirrorCoatTarget = user.index;
			}
			if (target.opposes(user)) {
				target.lastHPLostFromFoe = damage;                // For Metal Burst
				target.lastFoeAttacker.Add(user.pokemonIndex);   // For Metal Burst
			}
		}
		if (target.effects.Bide > 0) {
			target.effects.BideDamage += damage;
			target.effects.BideTarget = user.index;
		}
		if (target.fainted()) target.damageState.fainted = true;
		target.lastHPLost = damage;
		if (damage > 0 && !target.damageState.substitute) target.tookMoveDamageThisRound = true;   // For Focus Punch
		if (damage > 0) target.tookDamageThisRound = true;   // For Assurance
		target.lastAttacker.Add(user.index);              // For Revenge
		if (target.pokemon.isSpecies(Speciess.YAMASK) && target.pokemon.form == 1) {
			target.pokemon.evolution_counter += damage;
		}
	}
}
