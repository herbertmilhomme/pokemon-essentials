//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	// Effect per hit.
	public void EffectsOnMakingHit(move, user, target) {
		if (target.damageState.calcDamage > 0 && !target.damageState.substitute) {
			// Target's ability
			if (target.abilityActive(true)) {
				oldHP = user.hp;
				Battle.AbilityEffects.triggerOnBeingHit(target.ability, user, target, move, @battle);
				if (user.hp < oldHP) user.ItemHPHealCheck;
			}
			// Cramorant - Gulp Missile
			if (target.isSpecies(Speciess.CRAMORANT) && target.ability == abilitys.GULPMISSILE &&
				target.form > 0 && !target.effects.Transform) {
				oldHP = user.hp;
				// NOTE: Strictly speaking, an attack animation should be shown (the
				//       target Cramorant attacking the user) and the ability splash
				//       shouldn't be shown.
				@battle.ShowAbilitySplash(target);
				target_form = target.form;
				target.ChangeForm(0, null);
				if (user.takesIndirectDamage(Battle.Scene.USE_ABILITY_SPLASH)) {
					@battle.scene.DamageAnimation(user);
					user.ReduceHP(user.totalhp / 4, false);
				}
				switch (target_form) {
					case 1:   // Gulping Form
						user.LowerStatStageByAbility(:DEFENSE, 1, target, false);
						break;
					case 2:   // Gorging Form
						if (user.CanParalyze(target, false)) user.Paralyze(target);
						break;
				}
				@battle.HideAbilitySplash(target);
				if (user.hp < oldHP) user.ItemHPHealCheck;
			}
			// User's ability
			if (user.abilityActive(true)) {
				Battle.AbilityEffects.triggerOnDealingHit(user.ability, user, target, move, @battle);
				user.ItemHPHealCheck;
			}
			// Target's item
			if (target.itemActive(true)) {
				oldHP = user.hp;
				Battle.ItemEffects.triggerOnBeingHit(target.item, user, target, move, @battle);
				if (user.hp < oldHP) user.ItemHPHealCheck;
			}
		}
		if (target.opposes(user)) {
			// Rage
			if (target.effects.Rage && !target.fainted() &&
				target.CanRaiseStatStage(:ATTACK, target)) {
				@battle.Display(_INTL("{1}'s rage is building!", target.ToString()));
				target.RaiseStatStage(:ATTACK, 1, target);
			}
			// Beak Blast
			if (target.effects.BeakBlast) {
				Debug.Log($"[Lingering effect] {target.ToString()}'s Beak Blast");
				if (move.ContactMove(user) && user.affectedByContactEffect() &&
					user.CanBurn(target, false, self)) {
					user.Burn(target);
				}
			}
			// Shell Trap (make the trapper move next if the trap was triggered)
			if (target.effects.ShellTrap && move.physicalMove() &&
				@battle.choices[target.index].Action == :UseMove && !target.movedThisRound() &&
				target.damageState.hpLost > 0 && !target.damageState.substitute) {
				target.tookPhysicalHit              = true;
				target.effects.MoveNext = true;
				target.effects.Quash    = 0;
			}
			// Grudge
			if (target.effects.Grudge && target.fainted()) {
				user.SetPP(move, 0);
				@battle.Display(_INTL("{1}'s {2} lost all of its PP due to the grudge!",
																user.ToString(), move.name));
			}
			// Destiny Bond (recording that it should apply)
			if (target.effects.DestinyBond && target.fainted() &&
				user.effects.DestinyBondTarget < 0) {
				user.effects.DestinyBondTarget = target.index;
			}
		}
	}

	// Effects after all hits (i.e. at end of move usage).
	public void EffectsAfterMove(user, targets, move, numHits) {
		// Defrost
		if (move.damagingMove()) {
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected || b.damageState.substitute) continue;
				if (b.status != statuses.FROZEN) continue;
				// NOTE: Non-Fire-type moves that thaw the user will also thaw the
				//       target (in Gen 6+).
				if (move.calcType == Types.FIRE || (Settings.MECHANICS_GENERATION >= 6 && move.thawsUser())) {
					b.CureStatus;
				}
			}
		}
		// Destiny Bond
		// NOTE: Although Destiny Bond is similar to Grudge, they don't apply at
		//       the same time (however, Destiny Bond does check whether it's going
		//       to trigger at the same time as Grudge).
		if (user.effects.DestinyBondTarget >= 0 && !user.fainted()) {
			dbName = @battle.battlers[user.effects.DestinyBondTarget].ToString();
			@battle.Display(_INTL("{1} took its attacker down with it!", dbName));
			user.ReduceHP(user.hp, false);
			user.ItemHPHealCheck;
			user.Faint;
			@battle.JudgeCheckpoint(user);
		}
		// User's ability
		if (user.abilityActive()) {
			Battle.AbilityEffects.triggerOnEndOfUsingMove(user.ability, user, targets, move, @battle);
		}
		if (!user.fainted() && !user.effects.Transform &&
			!@battle.AllFainted(user.idxOpposingSide)) {
			// Greninja - Battle Bond
			if (user.isSpecies(Speciess.GRENINJA) && user.ability == abilitys.BATTLEBOND &&
				!@battle.battleBond[user.index & 1][user.pokemonIndex]) {
				numFainted = 0;
				targets.each(b => { if (b.damageState.fainted) numFainted += 1; });
				if (numFainted > 0 && user.form == 1) {
					@battle.battleBond[user.index & 1][user.pokemonIndex] = true;
					@battle.Display(_INTL("{1} became fully charged due to its bond with its Trainer!", user.ToString()));
					@battle.ShowAbilitySplash(user, true);
					@battle.HideAbilitySplash(user);
					user.ChangeForm(2, _INTL("{1} became Ash-Greninja!", user.ToString()));
				}
			}
			// Cramorant = Gulp Missile
			if (user.isSpecies(Speciess.CRAMORANT) && user.ability == abilitys.GULPMISSILE && user.form == 0 &&
				((move.id == moves.SURF && numHits > 0) || (move.id == moves.DIVE && move.chargingTurn))) {
				// NOTE: Intentionally no ability splash or message here.
				user.ChangeForm((user.hp > user.totalhp / 2) ? 1 : 2, null);
			}
		}
		// Room Service
		if (move.function_code == "StartSlowerBattlersActFirst" && @battle.field.effects.TrickRoom > 0) {
			@battle.allBattlers.each do |b|
				if (!b.hasActiveItem(Items.ROOMSERVICE)) continue;
				if (!b.CanLowerStatStage(:SPEED)) continue;
				@battle.CommonAnimation("UseItem", b);
				b.LowerStatStage(:SPEED, 1, null);
				b.ConsumeItem;
			}
		}
		// Consume user's Gem
		if (user.effects.GemConsumed) {
			// NOTE: The consume animation and message for Gems are shown immediately
			//       after the move's animation, but the item is only consumed now.
			user.ConsumeItem;
		}
		switched_battlers = new List<string>();   // Indices of battlers that were switched out somehow
		// Target switching caused by Roar, Whirlwind, Circle Throw, Dragon Tail
		move.SwitchOutTargetEffect(user, targets, numHits, switched_battlers);
		// Target's item, user's item, target's ability (all negated by Sheer Force)
		if (!(user.hasActiveAbility(Abilitys.SHEERFORCE) && move.addlEffect > 0)) {
			EffectsAfterMove2(user, targets, move, numHits, switched_battlers);
		}
		// Some move effects that need to happen here, i.e. user switching caused by
		// U-turn/Volt Switch/Baton Pass/Parting Shot, Relic Song's form changing,
		// Fling/Natural Gift consuming item.
		if (!switched_battlers.Contains(user.index)) {
			move.EndOfMoveUsageEffect(user, targets, numHits, switched_battlers);
		}
		// User's ability/item that switches the user out (all negated by Sheer Force)
		if (!(user.hasActiveAbility(Abilitys.SHEERFORCE) && move.addlEffect > 0)) {
			EffectsAfterMove3(user, targets, move, numHits, switched_battlers);
		}
		if (numHits > 0) {
			@battle.allBattlers.each(b => b.ItemEndOfMoveCheck);
		}
	}

	// Everything in this method is negated by Sheer Force.
	public void EffectsAfterMove2(user, targets, move, numHits, switched_battlers) {
		// Target's held item (Eject Button, Red Card, Eject Pack)
		@battle.Priority(true).each do |b|
			if (targets.any(targetB => targetB.index == b.index) &&
				!b.damageState.unaffected && b.damageState.calcDamage > 0 && b.itemActive()) {
				Battle.ItemEffects.triggerAfterMoveUseFromTarget(b.item, b, user, move, switched_battlers, @battle);
			}
			// Target's Eject Pack
			if (switched_battlers.empty() && b.index != user.index && b.ItemOnStatDropped(user)) {
				switched_battlers.Add(b.index);
			}
		}
		// User's held item (Life Orb, Shell Bell, Throat Spray, Eject Pack)
		if (!switched_battlers.Contains(user.index) && user.itemActive()) {   // Only if user hasn't switched out
			Battle.ItemEffects.triggerAfterMoveUseFromUser(user.item, user, targets, move, numHits, @battle);
		}
		// Target's ability (Berserk, Color Change, Emergency Exit, Pickpocket, Wimp Out)
		@battle.Priority(true).each do |b|
			if (targets.any(targetB => targetB.index == b.index) &&
				!b.damageState.unaffected && !switched_battlers.Contains(b.index) && b.abilityActive()) {
				Battle.AbilityEffects.triggerAfterMoveUseFromTarget(b.ability, b, user, move, switched_battlers, @battle);
			}
			// Target's Emergency Exit, Wimp Out (including for Pokémon hurt by Flame Burst)
			if (switched_battlers.empty() && move.damagingMove() &&
				b.index != user.index && b.AbilitiesOnDamageTaken(user)) {
				switched_battlers.Add(b.index);
			}
		}
	}

	// Everything in this method is negated by Sheer Force.
	public void EffectsAfterMove3(user, targets, move, numHits, switched_battlers) {
		// User's held item that switches it out (Eject Pack)
		if (switched_battlers.empty() && user.ItemOnStatDropped(user)) {
			switched_battlers.Add(user.index);
		}
		// User's ability (Emergency Exit, Wimp Out)
		if (switched_battlers.empty() && move.damagingMove() && user.AbilitiesOnDamageTaken(user)) {
			switched_battlers.Add(user.index);
		}
	}
}
