//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Choosing to use an item.
	//-----------------------------------------------------------------------------

	public bool CanUseItemOnPokemon(item, pkmn, battler, scene, showMessages = true) {
		if (!pkmn || pkmn.egg()) {
			if (showMessages) scene.Display(_INTL("It won't have any effect."));
			return false;
		}
		// Embargo
		if (battler && battler.effects.Embargo > 0) {
			if (showMessages) {
				scene.Display(_INTL("Embargo's effect prevents the item's use on {1}!",
															battler.ToString(true)));
			}
			return false;
		}
		// Hyper Mode and non-Scents
		if (pkmn.hyper_mode && !GameData.Item.get(item)&.is_scent()) {
			if (showMessages) scene.Display(_INTL("It won't have any effect."));
			return false;
		}
		return true;
	}

	// NOTE: Using a Poké Ball consumes all your actions for the round. The method
	//       below is one half of making this happen; the other half is in the
	//       ItemHandlers.CanUseInBattle for Poké Balls.
	public bool ItemUsesAllActions(item) {
		if (GameData.Item.get(item).is_poke_ball()) return true;
		return false;
	}

	public void RegisterItem(idxBattler, item, idxTarget = null, idxMove = null) {
		// Register for use of item on a Pokémon in the party
		@choices[idxBattler].Action = :UseItem;
		@choices[idxBattler].Index = item;        // ID of item to be used
		@choices[idxBattler].Move = idxTarget;   // Party index of Pokémon to use item on
		@choices[idxBattler].Target = idxMove;     // Index of move to recharge (Ethers)
		// Delete the item from the Bag. If it turns out it will have no effect, it
		// will be re-added to the Bag later.
		ConsumeItemInBag(item, idxBattler);
		return true;
	}

	//-----------------------------------------------------------------------------
	// Using an item.
	//-----------------------------------------------------------------------------

	public void ConsumeItemInBag(item, idxBattler) {
		if (!item) return;
		if (!GameData.Item.get(item).consumed_after_use()) return;
		if (OwnedByPlayer(idxBattler)) {
			if (!Game.GameData.bag.remove(item)) {
				Debug.LogError(_INTL("Tried to consume item that wasn't in the Bag somehow."));
				//throw new ArgumentException(_INTL("Tried to consume item that wasn't in the Bag somehow."));
			}
		} else {
			items = GetOwnerItems(idxBattler);
			if (items) items.delete_at(items.index(item));
		}
	}

	public void ReturnUnusedItemToBag(item, idxBattler) {
		if (!item) return;
		if (!GameData.Item.get(item).consumed_after_use()) return;
		if (OwnedByPlayer(idxBattler)) {
			if (Game.GameData.bag&.can_add(item)) {
				Game.GameData.bag.add(item);
			} else {
				Debug.LogError(_INTL("Couldn't return unused item to Bag somehow."));
				//throw new ArgumentException(_INTL("Couldn't return unused item to Bag somehow."));
			}
		} else {
			items = GetOwnerItems(idxBattler);
			items&.Add(item);
		}
	}

	public void UseItemMessage(item, trainerName) {
		itemName = GameData.Item.get(item).portion_name;
		if (itemName.starts_with_vowel()) {
			DisplayBrief(_INTL("{1} used an {2}.", trainerName, itemName));
		} else {
			DisplayBrief(_INTL("{1} used a {2}.", trainerName, itemName));
		}
	}

	// Uses an item on a Pokémon in the trainer's party.
	public void UseItemOnPokemon(item, idxParty, userBattler) {
		trainerName = GetOwnerName(userBattler.index);
		UseItemMessage(item, trainerName);
		pkmn = Party(userBattler.index)[idxParty];
		battler = FindBattler(idxParty, userBattler.index);
		ch = @choices[userBattler.index];
		if (ItemHandlers.triggerCanUseInBattle(item, pkmn, battler, ch[3], true, self, @scene, false)) {
			ItemHandlers.triggerBattleUseOnPokemon(item, pkmn, battler, ch, @scene);
			ch[1] = null;   // Delete item from choice
			return;
		}
		Display(_INTL("But it had no effect!"));
		// Return unused item to Bag
		ReturnUnusedItemToBag(item, userBattler.index);
	}

	// Uses an item on a Pokémon in battle that belongs to the trainer.
	public void UseItemOnBattler(item, idxParty, userBattler) {
		trainerName = GetOwnerName(userBattler.index);
		UseItemMessage(item, trainerName);
		battler = FindBattler(idxParty, userBattler.index);
		ch = @choices[userBattler.index];
		if (battler) {
			if (ItemHandlers.triggerCanUseInBattle(item, battler.pokemon, battler, ch[3], true, self, @scene, false)) {
				ItemHandlers.triggerBattleUseOnBattler(item, battler, @scene);
				ch[1] = null;   // Delete item from choice
				battler.ItemOnStatDropped;
				return;
			} else {
				Display(_INTL("But it had no effect!"));
			}
		} else {
			Display(_INTL("But it's not where this item can be used!"));
		}
		// Return unused item to Bag
		ReturnUnusedItemToBag(item, userBattler.index);
	}

	// Uses a Poké Ball in battle directly.
	public void UsePokeBallInBattle(item, idxBattler, userBattler) {
		if (idxBattler < 0) idxBattler = userBattler.index;
		battler = @battlers[idxBattler];
		ItemHandlers.triggerUseInBattle(item, battler, self);
		@choices[userBattler.index].Index = null;   // Delete item from choice
	}

	// Uses an item in battle directly.
	public void UseItemInBattle(item, idxBattler, userBattler) {
		trainerName = GetOwnerName(userBattler.index);
		UseItemMessage(item, trainerName);
		battler = (idxBattler < 0) ? userBattler : @battlers[idxBattler];
		pkmn = battler.pokemon;
		ch = @choices[userBattler.index];
		if (ItemHandlers.triggerCanUseInBattle(item, pkmn, battler, ch[3], true, self, @scene, false)) {
			ItemHandlers.triggerUseInBattle(item, battler, self);
			ch[1] = null;   // Delete item from choice
			return;
		}
		Display(_INTL("But it had no effect!"));
		// Return unused item to Bag
		ReturnUnusedItemToBag(item, userBattler.index);
	}
}
