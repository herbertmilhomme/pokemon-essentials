//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Ability trigger checks.
	//-----------------------------------------------------------------------------

	public void AbilitiesOnSwitchOut() {
		if (abilityActive()) {
			Battle.AbilityEffects.triggerOnSwitchOut(self.ability, self, false);
		}
		// Reset form
		@battle.peer.OnLeavingBattle(@battle, @pokemon, @battle.usedInBattle[idxOwnSide][@index / 2]);
		// Check for end of Neutralizing Gas/Unnerve
		if (hasActiveAbility(Abilitys.NEUTRALIZINGGAS)) {
			// Treat self as fainted
			@hp = 0;
			@fainted = true;
			AbilitiesOnNeutralizingGasEnding;
		} else if (hasActiveAbility(new {:UNNERVE, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH})) {
			// Treat self as fainted
			@hp = 0;
			@fainted = true;
			ItemsOnUnnerveEnding;
		}
		// Treat self as fainted
		@hp = 0;
		@fainted = true;
		// Check for end of primordial weather
		@battle.EndPrimordialWeather;
	}

	public void AbilitiesOnFainting() {
		// Self fainted; check all other battlers to see if their abilities trigger
		@battle.Priority(true).each do |b|
			if (!b || !b.abilityActive()) continue;
			Battle.AbilityEffects.triggerChangeOnBattlerFainting(b.ability, b, self, @battle);
		}
		@battle.Priority(true).each do |b|
			if (!b || !b.abilityActive()) continue;
			Battle.AbilityEffects.triggerOnBattlerFainting(b.ability, b, self, @battle);
		}
		if (hasActiveAbility(:NEUTRALIZINGGAS, true)) AbilitiesOnNeutralizingGasEnding;
		if (hasActiveAbility(new {:UNNERVE, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH}, true)) ItemsOnUnnerveEnding;
	}

	// Used for Emergency Exit/Wimp Out. Returns whether self has switched out.
	public void AbilitiesOnDamageTaken(move_user = null) {
		if (!@droppedBelowHalfHP) return false;
		if (!abilityActive()) return false;
		return Battle.AbilityEffects.triggerOnHPDroppedBelowHalf(self.ability, self, move_user, @battle);
	}

	public void AbilityOnTerrainChange(ability_changed = false) {
		if (!abilityActive()) return;
		Battle.AbilityEffects.triggerOnTerrainChange(self.ability, self, @battle, ability_changed);
	}

	// Used for Rattled's Gen 8 effect. Called when Intimidate is triggered.
	public void AbilitiesOnIntimidated() {
		if (!abilityActive()) return;
		Battle.AbilityEffects.triggerOnIntimidated(self.ability, self, @battle);
	}

	public void AbilitiesOnNeutralizingGasEnding() {
		if (@battle.CheckGlobalAbility(Abilities.NEUTRALIZINGGAS)) return;
		@battle.Display(_INTL("The effects of the neutralizing gas wore off!"));
		@battle.EndPrimordialWeather;
		@battle.Priority(true).each do |b|
			if (b.fainted()) continue;
			if (!b.unstoppableAbility() && !b.abilityActive()) continue;
			Battle.AbilityEffects.triggerOnSwitchIn(b.ability, b, @battle);
		}
	}

	// Called when a Pokémon (self) enters battle, at the end of each move used,
	// and at the end of each round.
	public void ContinualAbilityChecks(onSwitchIn = false) {
		// Check for end of primordial weather
		@battle.EndPrimordialWeather;
		// Trace
		if (hasActiveAbility(Abilitys.TRACE)) {
			// NOTE: In Gen 5 only, Trace only triggers upon the Trace bearer switching
			//       in and not at any later times, even if a traceable ability turns
			//       up later. Essentials ignores this, and allows Trace to trigger
			//       whenever it can even in Gen 5 battle mechanics.
			choices = @battle.allOtherSideBattlers(@index).select do |b|
				next !b.ungainableAbility() &&
						!new []{:POWEROFALCHEMY, :RECEIVER, :TRACE}.Contains(b.ability_id);
			}
			if (choices.length > 0) {
				choice = choices[@battle.Random(choices.length)];
				@battle.ShowAbilitySplash(self);
				self.ability = choice.ability;
				@battle.Display(_INTL("{1} traced {2}'s {3}!", This, choice.ToString(true), choice.abilityName));
				@battle.HideAbilitySplash(self);
				if (!onSwitchIn && (unstoppableAbility() || abilityActive())) {
					Battle.AbilityEffects.triggerOnSwitchIn(self.ability, self, @battle);
				}
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Ability curing.
	//-----------------------------------------------------------------------------

	// Cures status conditions, confusion and infatuation.
	public void AbilityStatusCureCheck() {
		if (abilityActive()) {
			Battle.AbilityEffects.triggerStatusCure(self.ability, self);
		}
	}

	//-----------------------------------------------------------------------------
	// Ability effects.
	//-----------------------------------------------------------------------------

	// For abilities that grant immunity to moves of a particular type, and raises
	// one of the ability's bearer's stats instead.
	public void MoveImmunityStatRaisingAbility(user, move, moveType, immuneType, stat, increment, show_message) {
		if (user.index == @index) return false;
		if (moveType != immuneType) return false;
		// NOTE: If show_message is false (Dragon Darts only), the stat will not be
		//       raised. This is not how the official games work, but I'm considering
		//       that a bug because Dragon Darts won't be fired at self in the first
		//       place if it's immune, so why would this ability be triggered by them()
		if (show_message) {
			@battle.ShowAbilitySplash(self);
			if (CanRaiseStatStage(stat, self)) {
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					RaiseStatStage(stat, increment, self);
				} else {
					RaiseStatStageByCause(stat, increment, self, abilityName);
				}
			} else if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("It doesn't affect {1}...", This(true)));
			} else {
				@battle.Display(_INTL("{1}'s {2} made {3} ineffective!", This, abilityName, move.name));
			}
			@battle.HideAbilitySplash(self);
		}
		return true;
	}

	// For abilities that grant immunity to moves of a particular type, and heals
	// the ability's bearer by 1/4 of its total HP instead.
	public void MoveImmunityHealingAbility(user, move, moveType, immuneType, show_message) {
		if (user.index == @index) return false;
		if (moveType != immuneType) return false;
		// NOTE: If show_message is false (Dragon Darts only), HP will not be healed.
		//       This is not how the official games work, but I'm considering that a
		//       bug because Dragon Darts won't be fired at self in the first place
		//       if (it's immune, so why would this ability be triggered by them()) {
		if (show_message) {
			@battle.ShowAbilitySplash(self);
			if (canHeal() && RecoverHP(@totalhp / 4) > 0) {
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1}'s HP was restored.", This));
				} else {
					@battle.Display(_INTL("{1}'s {2} restored its HP.", This, abilityName));
				}
			} else if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("It doesn't affect {1}...", This(true)));
			} else {
				@battle.Display(_INTL("{1}'s {2} made {3} ineffective!", This, abilityName, move.name));
			}
			@battle.HideAbilitySplash(self);
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// Ability change.
	//-----------------------------------------------------------------------------

	public void OnLosingAbility(oldAbil, suppressed = false) {
		if (oldAbil == :NEUTRALIZINGGAS && (suppressed || !@effects.GastroAcid)) {
			AbilitiesOnNeutralizingGasEnding;
		} else if (new []{:UNNERVE, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH}.Contains(oldAbil) &&
					(suppressed || !@effects.GastroAcid)) {
			ItemsOnUnnerveEnding;
		} else if (oldAbil == :ILLUSION && @effects.Illusion) {
			@effects.Illusion = null;
			if (!@effects.Transform) {
				@battle.scene.ChangePokemon(self, @pokemon);
				@battle.Display(_INTL("{1}'s {2} wore off!", This, GameData.Ability.get(oldAbil).name));
				@battle.SetSeen(self);
			}
		}
		if (unstoppableAbility()) @effects.GastroAcid = false;
		if (self.ability != abilitys.SLOWSTART) @effects.SlowStart  = 0;
		if (self.ability != abilitys.TRUANT) @effects.Truant     = false;
		// Check for end of primordial weather
		@battle.EndPrimordialWeather;
		// Revert form if Flower Gift/Forecast was lost
		CheckFormOnWeatherChange(true);
		// Abilities that trigger when the terrain changes
		AbilityOnTerrainChange(true);
	}

	public void TriggerAbilityOnGainingIt() {
		// Ending primordial weather, checking Trace
		ContinualAbilityChecks(true);   // Don't trigger Traced ability as it's triggered below
		// Abilities that trigger upon switching in
		if ((!fainted() && unstoppableAbility()) || abilityActive()) {
			Battle.AbilityEffects.triggerOnSwitchIn(self.ability, self, @battle);
		}
		// Status-curing ability check
		AbilityStatusCureCheck;
		// Check for end of primordial weather
		@battle.EndPrimordialWeather;
	}

	//-----------------------------------------------------------------------------
	// Held item consuming/removing.
	//-----------------------------------------------------------------------------

	public bool canConsumeBerry() {
		if (@battle.CheckOpposingAbility(new {:UNNERVE, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH}, @index)) return false;
		return true;
	}

	public bool canConsumePinchBerry(check_gluttony = true) {
		if (!canConsumeBerry()) return false;
		if (@hp <= @totalhp / 4) return true;
		if (@hp <= @totalhp / 2 && (!check_gluttony || hasActiveAbility(Abilitys.GLUTTONY))) return true;
		return false;
	}

	// permanent is whether the item is lost even after battle. Is false for Knock
	// Off.
	public void RemoveItem(permanent = true) {
		if (!hasActiveAbility(Abilitys.GORILLATACTICS)) @effects.ChoiceBand = null;
		if (self.item && hasActiveAbility(Abilitys.UNBURDEN)) @effects.Unburden   = true;
		if (permanent && self.item == self.initialItem) setInitialItem(null);
		self.item = null;
	}

	public void ConsumeItem(recoverable = true, symbiosis = true, belch = true) {
		Debug.Log($"[Item consumed] {This} consumed its held {itemName}");
		if (recoverable) {
			setRecycleItem(@item_id);
			@effects.PickupItem = @item_id;
			@effects.PickupUse  = @battle.nextPickupUse;
		}
		if (belch && self.item.is_berry()) setBelched;
		RemoveItem;
		if (symbiosis) Symbiosis;
	}

	public void Symbiosis() {
		if (fainted()) return;
		if (self.item) return;
		@battle.Priority(true).each do |b|
			if (b.opposes(self)) continue;
			if (!b.hasActiveAbility(Abilitys.SYMBIOSIS)) continue;
			if (!b.item || b.unlosableItem(b.item)) continue;
			if (unlosableItem(b.item)) continue;
			@battle.ShowAbilitySplash(b);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1} shared its {2} with {3}!",
																b.ToString(), b.itemName, This(true)));
			} else {
				@battle.Display(_INTL("{1}'s {2} let it share its {3} with {4}!",
																b.ToString(), b.abilityName, b.itemName, This(true)));
			}
			self.item = b.item;
			b.item = null;
			if (b.hasActiveAbility(Abilitys.UNBURDEN)) b.effects.Unburden = true;
			@battle.HideAbilitySplash(b);
			HeldItemTriggerCheck;
			break;
		}
	}

	// item_to_use is an item ID or GameData.Item object. own_item is whether the
	// item is held by self. fling is for Fling only.
	public void HeldItemTriggered(item_to_use, own_item = true, fling = false) {
		// Cheek Pouch
		if (hasActiveAbility(Abilitys.CHEEKPOUCH) && GameData.Item.get(item_to_use).is_berry() && canHeal()) {
			@battle.ShowAbilitySplash(self);
			RecoverHP(@totalhp / 3);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1}'s HP was restored.", This));
			} else {
				@battle.Display(_INTL("{1}'s {2} restored its HP.", This, abilityName));
			}
			@battle.HideAbilitySplash(self);
		}
		if (own_item) ConsumeItem;
		if (!own_item && !fling) Symbiosis;   // Bug Bite/Pluck users trigger Symbiosis
	}

	//-----------------------------------------------------------------------------
	// Held item trigger checks.
	//-----------------------------------------------------------------------------

	// NOTE: A Pokémon using Bug Bite/Pluck, and a Pokémon having an item thrown at
	//       it via Fling, will gain the effect of the item even if the Pokémon is
	//       affected by item-negating effects.
	// item_to_use is an item ID for Stuff Cheeks, Teatime, Bug Bite/Pluck and
	// Fling, and null otherwise.
	// fling is for Fling only.
	public void HeldItemTriggerCheck(item_to_use = null, fling = false) {
		if (fainted()) return;
		if (!item_to_use && !itemActive()) return;
		ItemHPHealCheck(item_to_use, fling);
		ItemStatusCureCheck(item_to_use, fling);
		ItemEndOfMoveCheck(item_to_use, fling);
		// For Enigma Berry, Kee Berry and Maranga Berry, which have their effects
		// when forcibly consumed by Pluck/Fling.
		if (item_to_use) {
			itm = item_to_use || self.item;
			if (Battle.ItemEffects.triggerOnBeingHitPositiveBerry(itm, self, @battle, true)) {
				HeldItemTriggered(itm, false, fling);
			}
		}
	}

	// item_to_use is an item ID for Bug Bite/Pluck and Fling, and null otherwise.
	// fling is for Fling only.
	public void ItemHPHealCheck(item_to_use = null, fling = false) {
		if (!item_to_use && !itemActive()) return;
		itm = item_to_use || self.item;
		if (Battle.ItemEffects.triggerHPHeal(itm, self, @battle, !item_to_use.null())) {
			HeldItemTriggered(itm, item_to_use.null(), fling);
		} else if (!item_to_use) {
			ItemTerrainStatBoostCheck;
		}
	}

	// Cures status conditions, confusion, infatuation and the other effects cured
	// by Mental Herb.
	// item_to_use is an item ID for Bug Bite/Pluck and Fling, and null otherwise.
	// fling is for Fling only.
	public void ItemStatusCureCheck(item_to_use = null, fling = false) {
		if (fainted()) return;
		if (!item_to_use && !itemActive()) return;
		itm = item_to_use || self.item;
		if (Battle.ItemEffects.triggerStatusCure(itm, self, @battle, !item_to_use.null())) {
			HeldItemTriggered(itm, item_to_use.null(), fling);
		}
	}

	// Called at the end of using a move.
	// item_to_use is an item ID for Bug Bite/Pluck and Fling, and null otherwise.
	// fling is for Fling only.
	public void ItemEndOfMoveCheck(item_to_use = null, fling = false) {
		if (fainted()) return;
		if (!item_to_use && !itemActive()) return;
		itm = item_to_use || self.item;
		if (Battle.ItemEffects.triggerOnEndOfUsingMove(itm, self, @battle, !item_to_use.null())) {
			HeldItemTriggered(itm, item_to_use.null(), fling);
		} else if (Battle.ItemEffects.triggerOnEndOfUsingMoveStatRestore(itm, self, @battle, !item_to_use.null())) {
			HeldItemTriggered(itm, item_to_use.null(), fling);
		}
	}

	// Used for White Herb (restore lowered stats). Only called by Moody and Sticky
	// Web, as all other stat reduction happens because of/during move usage and
	// this handler is also called at the end of each move's usage.
	// item_to_use is an item ID for Bug Bite/Pluck and Fling, and null otherwise.
	// fling is for Fling only.
	public void ItemStatRestoreCheck(item_to_use = null, fling = false) {
		if (fainted()) return;
		if (!item_to_use && !itemActive()) return;
		itm = item_to_use || self.item;
		if (Battle.ItemEffects.triggerOnEndOfUsingMoveStatRestore(itm, self, @battle, !item_to_use.null())) {
			HeldItemTriggered(itm, item_to_use.null(), fling);
		}
	}

	// Called when the battle terrain changes and when a Pokémon loses HP.
	public void ItemTerrainStatBoostCheck() {
		if (!itemActive()) return;
		if (Battle.ItemEffects.triggerTerrainStatBoost(self.item, self, @battle)) {
			HeldItemTriggered(self.item);
		}
	}

	// Used for Adrenaline Orb. Called when Intimidate is triggered (even if
	// Intimidate has no effect on the Pokémon).
	public void ItemOnIntimidatedCheck() {
		if (!itemActive()) return;
		if (Battle.ItemEffects.triggerOnIntimidated(self.item, self, @battle)) {
			HeldItemTriggered(self.item);
		}
	}

	// Used for Eject Pack. Returns whether self has switched out.
	public void ItemOnStatDropped(move_user = null) {
		if (!@statsDropped) return false;
		if (!itemActive()) return false;
		return Battle.ItemEffects.triggerOnStatLoss(self.item, self, move_user, @battle);
	}

	public void ItemsOnUnnerveEnding() {
		@battle.Priority(true).each do |b|
			if (b.item&.is_berry()) b.HeldItemTriggerCheck;
		}
	}

	//-----------------------------------------------------------------------------
	// Item effects.
	//-----------------------------------------------------------------------------

	public void ConfusionBerry(item_to_use, forced, confuse_stat, confuse_msg) {
		if (!forced && !canHeal()) return false;
		if (!forced && !canConsumePinchBerry(Settings.MECHANICS_GENERATION >= 7)) return false;
		used_item_name = GameData.Item.get(item_to_use).name;
		fraction_to_heal = 8;   // Gens 6 and lower
		if (Settings.MECHANICS_GENERATION == 7) {
			fraction_to_heal = 2;
		} else if (Settings.MECHANICS_GENERATION >= 8) {
			fraction_to_heal = 3;
		}
		amt = @totalhp / fraction_to_heal;
		ripening = false;
		if (hasActiveAbility(Abilitys.RIPEN)) {
			@battle.ShowAbilitySplash(self, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) @battle.CommonAnimation("EatBerry", self);
		if (ripening) @battle.HideAbilitySplash(self);
		amt = RecoverHP(amt);
		if (amt > 0) {
			if (forced) {
				Debug.Log($"[Item triggered] Forced consuming of {used_item_name}");
				@battle.Display(_INTL("{1}'s HP was restored.", This));
			} else {
				@battle.Display(_INTL("{1} restored its health using its {2}!", This, used_item_name));
			}
		}
		if (self.nature.stat_changes.any(val => val[0] == confuse_stat && val[1] < 0)) {
			@battle.Display(confuse_msg);
			if (CanConfuseSelf(false)) Confuse;
		}
		return true;
	}

	public void StatIncreasingBerry(item_to_use, forced, stat, increment = 1) {
		if (!forced && !canConsumePinchBerry()) return false;
		if (!CanRaiseStatStage(stat, self)) return false;
		used_item_name = GameData.Item.get(item_to_use).name;
		ripening = false;
		if (hasActiveAbility(Abilitys.RIPEN)) {
			@battle.ShowAbilitySplash(self, forced);
			increment *= 2;
			ripening = true;
		}
		if (!forced) @battle.CommonAnimation("EatBerry", self);
		if (ripening) @battle.HideAbilitySplash(self);
		if (!forced) return RaiseStatStageByCause(stat, increment, self, used_item_name);
		Debug.Log($"[Item triggered] Forced consuming of {used_item_name}");
		return RaiseStatStage(stat, increment, self);
	}

	public void MoveTypeWeakeningBerry(berry_type, move_type, mults) {
		if (!canConsumeBerry()) return false;
		if (move_type != berry_type) return;
		if (!Effectiveness.super_effective(@damageState.typeMod) && move_type != types.NORMAL) return;
		mults.final_damage_multiplier /= 2;
		@damageState.berryWeakened = true;
		ripening = false;
		if (hasActiveAbility(Abilitys.RIPEN)) {
			@battle.ShowAbilitySplash(self);
			mults.final_damage_multiplier /= 2;
			ripening = true;
		}
		@battle.CommonAnimation("EatBerry", self);
		if (ripening) @battle.HideAbilitySplash(self);
	}

	public void MoveTypePoweringUpGem(gem_type, move, move_type, mults) {
		if (move.is_a(Battle.Move.PledgeMove)) return;   // Pledge moves never consume Gems
		if (move_type != gem_type) return;
		@effects.GemConsumed = @item_id;
		if (Settings.MECHANICS_GENERATION >= 6) {
			mults.power_multiplier *= 1.3;
		} else {
			mults.power_multiplier *= 1.5;
		}
	}
}
