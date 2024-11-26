//===============================================================================
//
//===============================================================================
public partial class Battle.Peer {
	public void StorePokemon(player, pkmn) {
		if (!player.party_full()) {
			player.party[player.party.length] = pkmn;
			return -1;
		}
		if (Settings.HEAL_STORED_POKEMON) {
			old_ready_evo = pkmn.ready_to_evolve;
			pkmn.heal;
			pkmn.ready_to_evolve = old_ready_evo;
		}
		oldCurBox = CurrentBox;
		storedBox = Game.GameData.PokemonStorage.StoreCaught(pkmn);
		if (storedBox < 0) {
			// NOTE: Poké Balls can't be used if storage is full, so you shouldn't ever
			//       see this message.
			DisplayPaused(_INTL("Can't catch any more..."));
			return oldCurBox;
		}
		return storedBox;
	}

	public void GetStorageCreatorName() {
		if (Game.GameData.player.seen_storage_creator) return GetStorageCreator;
		return null;
	}

	public void CurrentBox() {
		return Game.GameData.PokemonStorage.currentBox;
	}

	public void BoxName(box) {
		return (box < 0) ? "" : Game.GameData.PokemonStorage[box].name;
	}

	public void OnStartingBattle(battle, pkmn, wild = false) {
		f = MultipleForms.call("getFormOnStartingBattle", pkmn, wild);
		if (f) pkmn.form = f;
		MultipleForms.call("changePokemonOnStartingBattle", pkmn, battle);
	}

	public void OnEnteringBattle(battle, battler, pkmn, wild = false) {
		f = MultipleForms.call("getFormOnEnteringBattle", pkmn, wild);
		if (f) pkmn.form = f;
		if (battler.form != pkmn.form) battler.form = pkmn.form;
		MultipleForms.call("changePokemonOnEnteringBattle", battler, pkmn, battle);
	}

	// For switching out, including due to fainting, and for the end of battle
	public void OnLeavingBattle(battle, pkmn, usedInBattle, endBattle = false) {
		if (!pkmn) return;
		f = MultipleForms.call("getFormOnLeavingBattle", pkmn, battle, usedInBattle, endBattle);
		if (f && pkmn.form != f) pkmn.form = f;
		if (pkmn.hp > pkmn.totalhp) pkmn.hp = pkmn.totalhp;
		MultipleForms.call("changePokemonOnLeavingBattle", pkmn, battle, usedInBattle, endBattle);
	}
}

//===============================================================================
// Unused class.
//===============================================================================
public partial class Battle.NullPeer {
	public void OnEnteringBattle(battle, battler, pkmn, wild = false) {}
	public void OnLeavingBattle(battle, pkmn, usedInBattle, endBattle = false) {}

	public void StorePokemon(player, pkmn) {
		if (!player.party_full()) player.party[player.party.length] = pkmn;
		return -1;
	}

	public int GetStorageCreatorName { get { return null; } }
	public int CurrentBox            { get { return -1;  } }
	public void BoxName(box)          {return "";  }
}
