//===============================================================================
// User steals the target's item, if the user has none itself. (Covet, Thief)
// Items stolen from wild Pokémon are kept after the battle.
//===============================================================================
public partial class Battle.Move.UserTakesTargetItem : Battle.Move {
	public void EffectAfterAllHits(user, target) {
		if (user.wild()) return;   // Wild Pokémon can't thieve
		if (user.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (!target.item || user.item) return;
		if (target.unlosableItem(target.item)) return;
		if (user.unlosableItem(target.item)) return;
		if (target.hasActiveAbility(Abilitys.STICKYHOLD) && !target.beingMoldBroken()) return;
		itemName = target.itemName;
		user.item = target.item;
		// Permanently steal the item from wild Pokémon
		if (target.wild() && !user.initialItem && target.item == target.initialItem) {
			user.setInitialItem(target.item);
			target.RemoveItem;
		} else {
			target.RemoveItem(false);
		}
		@battle.Display(_INTL("{1} stole {2}'s {3}!", user.ToString(), target.ToString(true), itemName));
		user.HeldItemTriggerCheck;
	}
}

//===============================================================================
// User gives its item to the target. The item remains given after wild battles.
// (Bestow)
//===============================================================================
public partial class Battle.Move.TargetTakesUserItem : Battle.Move {
	public override bool ignoresSubstitute(user) {
		if (Settings.MECHANICS_GENERATION >= 6) return true;
		return base.ignoresSubstitute();
	}

	public bool MoveFailed(user, targets) {
		if (!user.item || user.unlosableItem(user.item)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.item || target.unlosableItem(user.item)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		itemName = user.itemName;
		target.item = user.item;
		// Permanently steal the item from wild Pokémon
		if (user.wild() && !target.initialItem && user.item == user.initialItem) {
			target.setInitialItem(user.item);
			user.RemoveItem;
		} else {
			user.RemoveItem(false);
		}
		@battle.Display(_INTL("{1} received {2} from {3}!", target.ToString(), itemName, user.ToString(true)));
		target.HeldItemTriggerCheck;
	}
}

//===============================================================================
// User and target swap items. They remain swapped after wild battles.
// (Switcheroo, Trick)
//===============================================================================
public partial class Battle.Move.UserTargetSwapItems : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.wild()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!user.item && !target.item) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.unlosableItem(target.item) ||
			target.unlosableItem(user.item) ||
			user.unlosableItem(user.item) ||
			user.unlosableItem(target.item)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (target.hasActiveAbility(Abilitys.STICKYHOLD) && !target.beingMoldBroken()) {
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

	public void EffectAgainstTarget(user, target) {
		oldUserItem = user.item;
		oldUserItemName = user.itemName;
		oldTargetItem = target.item;
		oldTargetItemName = target.itemName;
		user.item                             = oldTargetItem;
		if (!user.hasActiveAbility(Abilitys.GORILLATACTICS)) user.effects.ChoiceBand   = null;
		if (user.hasActiveAbility(Abilitys.UNBURDEN)) user.effects.Unburden     = (!user.item && oldUserItem);
		target.item                           = oldUserItem;
		if (!target.hasActiveAbility(Abilitys.GORILLATACTICS)) target.effects.ChoiceBand = null;
		if (target.hasActiveAbility(Abilitys.UNBURDEN)) target.effects.Unburden   = (!target.item && oldTargetItem);
		// Permanently steal the item from wild Pokémon
		if (target.wild() && !user.initialItem && oldTargetItem == target.initialItem) {
			user.setInitialItem(oldTargetItem);
		}
		@battle.Display(_INTL("{1} switched items with its opponent!", user.ToString()));
		if (oldTargetItem) @battle.Display(_INTL("{1} obtained {2}.", user.ToString(), oldTargetItemName));
		if (oldUserItem) @battle.Display(_INTL("{1} obtained {2}.", target.ToString(), oldUserItemName));
		user.HeldItemTriggerCheck;
		target.HeldItemTriggerCheck;
	}
}

//===============================================================================
// User recovers the last item it held and consumed. (Recycle)
//===============================================================================
public partial class Battle.Move.RestoreUserConsumedItem : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (!user.recycleItem || user.item) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		item = user.recycleItem;
		user.item = item;
		if (@battle.wildBattle() && !user.initialItem) user.setInitialItem(item);
		user.setRecycleItem(null);
		user.effects.PickupItem = null;
		user.effects.PickupUse  = 0;
		itemName = GameData.Item.get(item).name;
		if (itemName.starts_with_vowel()) {
			@battle.Display(_INTL("{1} found an {2}!", user.ToString(), itemName));
		} else {
			@battle.Display(_INTL("{1} found a {2}!", user.ToString(), itemName));
		}
		user.HeldItemTriggerCheck;
	}
}

//===============================================================================
// Target drops its item. It regains the item at the end of the battle. (Knock Off)
// If target has a losable item, damage is multiplied by 1.5.
//===============================================================================
public partial class Battle.Move.RemoveTargetItem : Battle.Move {
	public void BaseDamage(baseDmg, user, target) {
		if (Settings.MECHANICS_GENERATION >= 6 &&
			target.item && !target.unlosableItem(target.item)) {
			// NOTE: Damage is still boosted even if target has Sticky Hold or a
			//       substitute.
			baseDmg = (int)Math.Round(baseDmg * 1.5);
		}
		return baseDmg;
	}

	public void EffectAfterAllHits(user, target) {
		if (user.wild()) return;   // Wild Pokémon can't knock off
		if (user.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (!target.item || target.unlosableItem(target.item)) return;
		if (target.hasActiveAbility(Abilitys.STICKYHOLD) && !target.beingMoldBroken()) return;
		itemName = target.itemName;
		target.RemoveItem(false);
		@battle.Display(_INTL("{1} dropped its {2}!", target.ToString(), itemName));
	}
}

//===============================================================================
// Target's berry/Gem is destroyed. (Incinerate)
//===============================================================================
public partial class Battle.Move.DestroyTargetBerryOrGem : Battle.Move {
	public void EffectWhenDealingDamage(user, target) {
		if (target.damageState.substitute || target.damageState.berryWeakened) return;
		if (!target.item || (!target.item.is_berry() &&
							!(Settings.MECHANICS_GENERATION >= 6 && target.item.is_gem()))) return;
		if (target.unlosableItem(target.item)) return;
		if (target.hasActiveAbility(Abilitys.STICKYHOLD) && !target.beingMoldBroken()) return;
		item_name = target.itemName;
		target.RemoveItem;
		@battle.Display(_INTL("{1}'s {2} was incinerated!", target.ToString(), item_name));
	}
}

//===============================================================================
// Negates the effect and usability of the target's held item for the rest of the
// battle (even if it is switched out). Fails if the target doesn't have a held
// item, the item is unlosable, the target has Sticky Hold, or the target is
// behind a substitute. (Corrosive Gas)
//===============================================================================
public partial class Battle.Move.CorrodeTargetItem : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.item || target.unlosableItem(target.item) ||
			target.effects.Substitute > 0) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		if (target.hasActiveAbility(Abilitys.STICKYHOLD) && !target.beingMoldBroken()) {
			if (show_message) {
				@battle.ShowAbilitySplash(target);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} is unaffected!", target.ToString()));
				} else {
					@battle.Display(_INTL("{1} is unaffected because of its {2}!",
																	target.ToString(true), target.abilityName));
				}
				@battle.HideAbilitySplash(target);
			}
			return true;
		}
		if (@battle.corrosiveGas[target.index % 2][target.pokemonIndex]) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		@battle.corrosiveGas[target.index % 2][target.pokemonIndex] = true;
		@battle.Display(_INTL("{1} corroded {2}'s {3}!",
														user.ToString(), target.ToString(true), target.itemName));
	}
}

//===============================================================================
// For 5 rounds, the target cannot use its held item, its held item has no
// effect, and no items can be used on it. (Embargo)
//===============================================================================
public partial class Battle.Move.StartTargetCannotUseItem : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Embargo > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Embargo = 5;
		@battle.Display(_INTL("{1} can't use items anymore!", target.ToString()));
	}
}

//===============================================================================
// For 5 rounds, all held items cannot be used in any way and have no effect.
// Held items can still change hands, but can't be thrown. (Magic Room)
//===============================================================================
public partial class Battle.Move.StartNegateHeldItems : Battle.Move {
	public void EffectGeneral(user) {
		if (@battle.field.effects.MagicRoom > 0) {
			@battle.field.effects.MagicRoom = 0;
			@battle.Display(_INTL("The area returned to normal!"));
		} else {
			@battle.field.effects.MagicRoom = 5;
			@battle.Display(_INTL("It created a bizarre area in which Pokémon's held items lose their effects!"));
		}
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@battle.field.effects.MagicRoom > 0) return;   // No animation
		base.ShowAnimation();
	}
}

//===============================================================================
// The user consumes its held berry increases its Defense by 2 stages. It also
// gains the berry's effect if it has one. The berry can be consumed even if
// Unnerve/Magic Room apply. Fails if the user is not holding a berry. This move
// cannot be chosen to be used if the user is not holding a berry. (Stuff Cheeks)
//===============================================================================
public partial class Battle.Move.UserConsumeBerryRaiseDefense2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 2};
	}

	public bool CanChooseMove(user, commandPhase, showMessages) {
		item = user.item;
		if (!item || !item.is_berry() || !user.itemActive()) {
			if (showMessages) {
				msg = _INTL("{1} can't use that move because it doesn't have a Berry!", user.ToString());
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		return true;
	}

	public bool MoveFailed(user, targets) {
		// NOTE: Unnerve does not stop a Pokémon using this move.
		item = user.item;
		if (!item || !item.is_berry() || !user.itemActive()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return super;
	}

	public override void EffectGeneral(user) {
		base.EffectGeneral();
		@battle.Display(_INTL("{1} ate its {2}!", user.ToString(), user.itemName));
		item = user.item;
		user.ConsumeItem(true, false);   // Don't trigger Symbiosis yet
		user.HeldItemTriggerCheck(item.id, false);
	}
}

//===============================================================================
// All Pokémon (except semi-invulnerable ones) consume their held berries and
// gain their effects. Berries can be consumed even if Unnerve/Magic Room apply.
// Fails if no Pokémon have a held berry. If this move would trigger an ability
// that negates the move, e.g. Lightning Rod, the bearer of that ability will
// have their ability triggered regardless of whether they are holding a berry,
// and they will not consume their berry. (Teatime)
// TODO: This isn't quite right for the messages shown when a berry is consumed.
//===============================================================================
public partial class Battle.Move.AllBattlersConsumeBerry : Battle.Move {
	public bool MoveFailed(user, targets) {
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b.item || !b.item.is_berry()) continue;
			if (b.semiInvulnerable()) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("But nothing happened!"));
			return true;
		}
		return false;
	}

	public void OnStartUse(user, targets) {
		@battle.Display(_INTL("It's teatime! Everyone dug in to their Berries!"));
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.item || !target.item.is_berry() || target.semiInvulnerable()) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		@battle.CommonAnimation("EatBerry", target);
		item = target.item;
		target.ConsumeItem(true, false);   // Don't trigger Symbiosis yet
		target.HeldItemTriggerCheck(item.id, false);
	}
}

//===============================================================================
// User consumes target's berry and gains its effect. (Bug Bite, Pluck)
//===============================================================================
public partial class Battle.Move.UserConsumeTargetBerry : Battle.Move {
	public bool preventsBattlerConsumingHealingBerry(battler, targets) {
		return targets.any(b => b.index == battler.index) &&
					battler.item&.is_berry() && Battle.ItemEffects.HPHeal[battler.item];
	}

	public void EffectAfterAllHits(user, target) {
		if (user.fainted() || target.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (!target.item || !target.item.is_berry() || target.unlosableItem(target.item)) return;
		if (target.hasActiveAbility(Abilitys.STICKYHOLD) && !target.beingMoldBroken()) return;
		item = target.item;
		itemName = target.itemName;
		user.setBelched;
		target.RemoveItem;
		@battle.Display(_INTL("{1} stole and ate its target's {2}!", user.ToString(), itemName));
		user.HeldItemTriggerCheck(item.id, false);
		user.Symbiosis;
	}
}

//===============================================================================
// User flings its item at the target. Power/effect depend on the item. (Fling)
//===============================================================================
public partial class Battle.Move.ThrowUserItemAtTarget : Battle.Move {
	public void CheckFlingSuccess(user) {
		@willFail = false;
		if (!user.item || !user.itemActive() || user.unlosableItem(user.item)) @willFail = true;
		if (@willFail) return;
		if (user.item.is_berry() && !user.canConsumeBerry()) @willFail = true;
		if (@willFail) return;
		@willFail = user.item.flags.none(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Fling_",RegexOptions.IgnoreCase));
	}

	public bool MoveFailed(user, targets) {
		if (@willFail) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public override void DisplayUseMessage(user) {
		base.DisplayUseMessage();
		CheckFlingSuccess(user);
		if (!@willFail) {
			@battle.Display(_INTL("{1} flung its {2}!", user.ToString(), user.itemName));
		}
	}

	public void NumHits(user, targets) {return 1; }

	public void BaseDamage(baseDmg, user, target) {
		if (!user.item) return 0;
		foreach (var flag in user.item.flags) { //'user.item.flags.each' do => |flag|
			if (System.Text.RegularExpressions.Regex.IsMatch(flag,@"^Fling_(\d+)$",RegexOptions.IgnoreCase)) return (int)Math.Max($~[1].ToInt(), 10);
		}
		return 10;
	}

	public void EffectAgainstTarget(user, target) {
		if (target.damageState.substitute) return;
		if (target.hasActiveAbility(Abilitys.SHIELDDUST) && !target.beingMoldBroken()) return;
		switch (user.item_id) {
			case :POISONBARB:
				if (target.CanPoison(user, false, self)) target.Poison(user);
				break;
			case :TOXICORB:
				if (target.CanPoison(user, false, self)) target.Poison(user, null, true);
				break;
			case :FLAMEORB:
				if (target.CanBurn(user, false, self)) target.Burn(user);
				break;
			case :LIGHTBALL:
				if (target.CanParalyze(user, false, self)) target.Paralyze(user);
				break;
			case :KINGSROCK: case :RAZORFANG:
				target.Flinch(user);
				break;
			default:
				target.HeldItemTriggerCheck(user.item_id, true);
				break;
		}
		// NOTE: The official games only let the target use Belch if the berry flung
		//       at it has some kind of effect (i.e. it isn't an effectless berry). I
		//       think this isn't in the spirit of "consuming a berry", so I've said
		//       that Belch is usable after having any kind of berry flung at you.
		if (user.item.is_berry()) target.setBelched;
	}

	public void EndOfMoveUsageEffect(user, targets, numHits, switchedBattlers) {
		// NOTE: The item is consumed even if this move was Protected against or it
		//       missed. The item is not consumed if the target was switched out by
		//       an effect like a target's Red Card.
		// NOTE: There is no item consumption animation.
		if (user.item) user.ConsumeItem(true, true, false);
	}
}
