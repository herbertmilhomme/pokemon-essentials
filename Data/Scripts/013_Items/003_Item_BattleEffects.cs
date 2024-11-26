//===============================================================================
// CanUseInBattle handlers.
//===============================================================================

ItemHandlers.CanUseInBattle.add(:GUARDSPEC, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.OwnSide.effects.Mist > 0) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:POKEDOLL, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battle.wildBattle()) {
		if (showMessages) {
			scene.Display(_INTL("Oak's words echoed... There's a time and place for everything! But not now."));
		}
		next false;
	}
	if (!battle.canRun) {
		if (showMessages) scene.Display(_INTL("You can't escape!"));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:POKEDOLL, :FLUFFYTAIL, :POKETOY);

ItemHandlers.CanUseInBattle.addIf(:poke_balls,
	block: (item) => { GameData.Item.get(item).is_poke_ball() },
	block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
		if (battle.Player.party_full() && Game.GameData.PokemonStorage.full()) {
			if (showMessages) scene.Display(_INTL("There is no room left in the PC!"));
			next false;
		}
		if (battle.disablePokeBalls) {
			if (showMessages) scene.Display(_INTL("You can't throw a Poké Ball!"));
			next false;
		}
		// NOTE: Using a Poké Ball consumes all your actions for the round. The code
		//       below is one half of making this happen; the other half is in def
		//       ItemUsesAllActions().
		if (!firstAction) {
			if (showMessages) scene.Display(_INTL("It's impossible to aim without being focused!"));
			next false;
		}
		if (battler.semiInvulnerable()) {
			if (showMessages) scene.Display(_INTL("It's no good! It's impossible to aim at a Pokémon that's not in sight!"));
			next false;
		}
		// NOTE: The code below stops you from throwing a Poké Ball if there is more
		//       than one unfainted opposing Pokémon. (Snag Balls can be thrown in
		//       this case, but only in trainer battles, and the trainer will deflect
		//       them if they are trying to catch a non-Shadow Pokémon.)
		if (battle.OpposingBattlerCount > 1 && !(GameData.Item.get(item).is_snag_ball() && battle.trainerBattle())) {
			if (showMessages) scene.Display(_INTL("It's no good! It's impossible to aim unless there is only one Pokémon!"));
			next false;
		}
		next true;
	}
)

ItemHandlers.CanUseInBattle.add(:POTION, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.able() || pokemon.hp == pokemon.totalhp) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:POTION,
																	:SUPERPOTION, :HYPERPOTION, :MAXPOTION,
																	:BERRYJUICE, :SWEETHEART, :FRESHWATER, :SODAPOP,
																	:LEMONADE, :MOOMOOMILK, :ORANBERRY, :SITRUSBERRY,
																	:ENERGYPOWDER, :ENERGYROOT);
ItemHandlers.CanUseInBattle.copy(:POTION, :RAGECANDYBAR) if !Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS;

ItemHandlers.CanUseInBattle.add(:AWAKENING, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanCureStatus(:SLEEP, pokemon, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:AWAKENING, :CHESTOBERRY);

ItemHandlers.CanUseInBattle.add(:BLUEFLUTE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (battler&.hasActiveAbility(Abilitys.SOUNDPROOF)) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next BattleItemCanCureStatus(:SLEEP, pokemon, scene, showMessages);
});

ItemHandlers.CanUseInBattle.add(:ANTIDOTE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanCureStatus(:POISON, pokemon, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:ANTIDOTE, :PECHABERRY);

ItemHandlers.CanUseInBattle.add(:BURNHEAL, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanCureStatus(:BURN, pokemon, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:BURNHEAL, :RAWSTBERRY);

ItemHandlers.CanUseInBattle.add(:PARALYZEHEAL, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanCureStatus(:PARALYSIS, pokemon, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:PARALYZEHEAL, :PARLYZHEAL, :CHERIBERRY);

ItemHandlers.CanUseInBattle.add(:ICEHEAL, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanCureStatus(:FROZEN, pokemon, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:ICEHEAL, :ASPEARBERRY);

ItemHandlers.CanUseInBattle.add(:FULLHEAL, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.able() ||
		(pokemon.status == statuses.NONE &&
		(!battler || battler.effects.Confusion == 0))) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:FULLHEAL,
																	:LAVACOOKIE, :OLDGATEAU, :CASTELIACONE,
																	:LUMIOSEGALETTE, :SHALOURSABLE, :BIGMALASADA,
																	:PEWTERCRUNCHIES, :LUMBERRY, :HEALPOWDER);
ItemHandlers.CanUseInBattle.copy(:FULLHEAL, :RAGECANDYBAR) if Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS;

ItemHandlers.CanUseInBattle.add(:FULLRESTORE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.able() ||
		(pokemon.hp == pokemon.totalhp && pokemon.status == statuses.NONE &&
		(!battler || battler.effects.Confusion == 0))) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:REVIVE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.fainted()) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:REVIVE, :MAXREVIVE, :REVIVALHERB, :MAXHONEY);

ItemHandlers.CanUseInBattle.add(:ETHER, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.able() || move < 0 ||
		pokemon.moves[move].total_pp <= 0 ||
		pokemon.moves[move].pp == pokemon.moves[move].total_pp) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:ETHER, :MAXETHER, :LEPPABERRY);

ItemHandlers.CanUseInBattle.add(:ELIXIR, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!pokemon.able()) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	canRestore = false;
	foreach (var m in pokemon.moves) { //'pokemon.moves.each' do => |m|
		if (m.id == 0) continue;
		if (m.total_pp <= 0 || m.pp == m.total_pp) continue;
		canRestore = true;
		break;
	}
	if (!canRestore) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.copy(:ELIXIR, :MAXELIXIR);

ItemHandlers.CanUseInBattle.add(:REDFLUTE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.effects.Attract < 0 ||
		battler.hasActiveAbility(Abilitys.SOUNDPROOF)) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:PERSIMBERRY, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.effects.Confusion == 0) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:YELLOWFLUTE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.effects.Confusion == 0 ||
		battler.hasActiveAbility(Abilitys.SOUNDPROOF)) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:XATTACK, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanRaiseStat(:ATTACK, battler, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:XATTACK, :XATTACK2, :XATTACK3, :XATTACK6);

ItemHandlers.CanUseInBattle.add(:XDEFENSE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanRaiseStat(:DEFENSE, battler, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:XDEFENSE,
																	:XDEFENSE2, :XDEFENSE3, :XDEFENSE6,
																	:XDEFEND, :XDEFEND2, :XDEFEND3, :XDEFEND6);

ItemHandlers.CanUseInBattle.add(:XSPATK, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanRaiseStat(:SPECIAL_ATTACK, battler, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:XSPATK,
																	:XSPATK2, :XSPATK3, :XSPATK6,
																	:XSPECIAL, :XSPECIAL2, :XSPECIAL3, :XSPECIAL6);

ItemHandlers.CanUseInBattle.add(:XSPDEF, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanRaiseStat(:SPECIAL_DEFENSE, battler, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:XSPDEF, :XSPDEF2, :XSPDEF3, :XSPDEF6);

ItemHandlers.CanUseInBattle.add(:XSPEED, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanRaiseStat(:SPEED, battler, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:XSPEED, :XSPEED2, :XSPEED3, :XSPEED6);

ItemHandlers.CanUseInBattle.add(:XACCURACY, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	next BattleItemCanRaiseStat(:ACCURACY, battler, scene, showMessages);
});

ItemHandlers.CanUseInBattle.copy(:XACCURACY, :XACCURACY2, :XACCURACY3, :XACCURACY6);

ItemHandlers.CanUseInBattle.add(:MAXMUSHROOMS, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!BattleItemCanRaiseStat(:ATTACK, battler, scene, false) &&
		!BattleItemCanRaiseStat(:DEFENSE, battler, scene, false) &&
		!BattleItemCanRaiseStat(:SPECIAL_ATTACK, battler, scene, false) &&
		!BattleItemCanRaiseStat(:SPECIAL_DEFENSE, battler, scene, false) &&
		!BattleItemCanRaiseStat(:SPEED, battler, scene, false)) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:DIREHIT, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.effects.FocusEnergy >= 1) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:DIREHIT2, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.effects.FocusEnergy >= 2) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:DIREHIT3, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (!battler || battler.effects.FocusEnergy >= 3) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

ItemHandlers.CanUseInBattle.add(:POKEFLUTE, block: (item, pokemon, battler, move, firstAction, battle, scene, showMessages) => {
	if (battle.allBattlers.none(b => b.status == statuses.SLEEP && !b.hasActiveAbility(Abilitys.SOUNDPROOF))) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	next true;
});

//===============================================================================
// UseInBattle handlers.
// For items used directly or on an opposing battler.
//===============================================================================

ItemHandlers.UseInBattle.add(:GUARDSPEC, block: (item, battler, battle) => {
	battler.OwnSide.effects.Mist = 5;
	battle.Display(_INTL("{1} became shrouded in mist!", battler.Team));
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.UseInBattle.add(:POKEDOLL, block: (item, battler, battle) => {
	battle.decision = Battle.Outcome.FLEE;
	battle.DisplayPaused(_INTL("You got away safely!"));
});

ItemHandlers.UseInBattle.copy(:POKEDOLL, :FLUFFYTAIL, :POKETOY);

ItemHandlers.UseInBattle.add(:POKEFLUTE, block: (item, battler, battle) => {
	foreach (var b in battle.allBattlers) { //'battle.allBattlers.each' do => |b|
		if (b.status == statuses.SLEEP && !b.hasActiveAbility(Abilitys.SOUNDPROOF)) b.CureStatus(false);
	}
	battle.Display(_INTL("All Pokémon were roused by the tune!"));
});

ItemHandlers.UseInBattle.addIf(:poke_balls,
	block: (item) => { GameData.Item.get(item).is_poke_ball() },
	block: (item, battler, battle) => {
		battle.ThrowPokeBall(battler.index, item);
	}
)

//===============================================================================
// BattleUseOnPokemon handlers.
// For items used on Pokémon or on a Pokémon's move.
//===============================================================================

ItemHandlers.BattleUseOnPokemon.add(:POTION, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, 20, scene);
});

ItemHandlers.BattleUseOnPokemon.copy(:POTION, :BERRYJUICE, :SWEETHEART);
ItemHandlers.BattleUseOnPokemon.copy(:POTION, :RAGECANDYBAR) if !Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS;

ItemHandlers.BattleUseOnPokemon.add(:SUPERPOTION, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 60 : 50, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:HYPERPOTION, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 120 : 200, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:MAXPOTION, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, pokemon.totalhp - pokemon.hp, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:FRESHWATER, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 30 : 50, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:SODAPOP, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 50 : 60, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:LEMONADE, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 70 : 80, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:MOOMOOMILK, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, 100, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:ORANBERRY, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, 10, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:SITRUSBERRY, block: (item, pokemon, battler, choices, scene) => {
	BattleHPItem(pokemon, battler, pokemon.totalhp / 4, scene);
});

ItemHandlers.BattleUseOnPokemon.add(:AWAKENING, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1} woke up.", name));
});

ItemHandlers.BattleUseOnPokemon.copy(:AWAKENING, :CHESTOBERRY, :BLUEFLUTE);

ItemHandlers.BattleUseOnPokemon.add(:ANTIDOTE, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1} was cured of its poisoning.", name));
});

ItemHandlers.BattleUseOnPokemon.copy(:ANTIDOTE, :PECHABERRY);

ItemHandlers.BattleUseOnPokemon.add(:BURNHEAL, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1}'s burn was healed.", name));
});

ItemHandlers.BattleUseOnPokemon.copy(:BURNHEAL, :RAWSTBERRY);

ItemHandlers.BattleUseOnPokemon.add(:PARALYZEHEAL, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1} was cured of paralysis.", name));
});

ItemHandlers.BattleUseOnPokemon.copy(:PARALYZEHEAL, :PARLYZHEAL, :CHERIBERRY);

ItemHandlers.BattleUseOnPokemon.add(:ICEHEAL, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1} was thawed out.", name));
});

ItemHandlers.BattleUseOnPokemon.copy(:ICEHEAL, :ASPEARBERRY);

ItemHandlers.BattleUseOnPokemon.add(:FULLHEAL, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	battler&.CureConfusion;
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1} became healthy.", name));
});

ItemHandlers.BattleUseOnPokemon.copy(:FULLHEAL,
																			:LAVACOOKIE, :OLDGATEAU, :CASTELIACONE,
																			:LUMIOSEGALETTE, :SHALOURSABLE, :BIGMALASADA,
																			:PEWTERCRUNCHIES, :LUMBERRY);
ItemHandlers.BattleUseOnPokemon.copy(:FULLHEAL, :RAGECANDYBAR) if Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS;

ItemHandlers.BattleUseOnPokemon.add(:FULLRESTORE, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	battler&.CureConfusion;
	name = (battler) ? battler.ToString() : pokemon.name;
	if (pokemon.hp < pokemon.totalhp) {
		BattleHPItem(pokemon, battler, pokemon.totalhp, scene);
	} else {
		scene.Refresh;
		scene.Display(_INTL("{1} became healthy.", name));
	}
});

ItemHandlers.BattleUseOnPokemon.add(:REVIVE, block: (item, pokemon, battler, choices, scene) => {
	pokemon.hp = pokemon.totalhp / 2;
	if (pokemon.hp <= 0) pokemon.hp = 1;
	pokemon.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} recovered from fainting!", pokemon.name));
});

ItemHandlers.BattleUseOnPokemon.add(:MAXREVIVE, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_HP;
	pokemon.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} recovered from fainting!", pokemon.name));
});

ItemHandlers.BattleUseOnPokemon.copy(:MAXREVIVE, :MAXHONEY);

ItemHandlers.BattleUseOnPokemon.add(:ENERGYPOWDER, block: (item, pokemon, battler, choices, scene) => {
	if (BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 60 : 50, scene)) {
		pokemon.changeHappiness("powder");
	}
});

ItemHandlers.BattleUseOnPokemon.add(:ENERGYROOT, block: (item, pokemon, battler, choices, scene) => {
	if (BattleHPItem(pokemon, battler, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 120 : 200, scene)) {
		pokemon.changeHappiness("energyroot");
	}
});

ItemHandlers.BattleUseOnPokemon.add(:HEALPOWDER, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_status;
	battler&.CureStatus(false);
	battler&.CureConfusion;
	pokemon.changeHappiness("powder");
	name = (battler) ? battler.ToString() : pokemon.name;
	scene.Refresh;
	scene.Display(_INTL("{1} became healthy.", name));
});

ItemHandlers.BattleUseOnPokemon.add(:REVIVALHERB, block: (item, pokemon, battler, choices, scene) => {
	pokemon.heal_HP;
	pokemon.heal_status;
	pokemon.changeHappiness("revivalherb");
	scene.Refresh;
	scene.Display(_INTL("{1} recovered from fainting!", pokemon.name));
});

ItemHandlers.BattleUseOnPokemon.add(:ETHER, block: (item, pokemon, battler, choices, scene) => {
	idxMove = choices[3];
	BattleRestorePP(pokemon, battler, idxMove, 10);
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
});

ItemHandlers.BattleUseOnPokemon.copy(:ETHER, :LEPPABERRY);

ItemHandlers.BattleUseOnPokemon.add(:MAXETHER, block: (item, pokemon, battler, choices, scene) => {
	idxMove = choices[3];
	BattleRestorePP(pokemon, battler, idxMove, pokemon.moves[idxMove].total_pp);
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
});

ItemHandlers.BattleUseOnPokemon.add(:ELIXIR, block: (item, pokemon, battler, choices, scene) => {
	for (int i = pokemon.moves.length; i < pokemon.moves.length; i++) { //for 'pokemon.moves.length' times do => |i|
		BattleRestorePP(pokemon, battler, i, 10);
	}
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
});

ItemHandlers.BattleUseOnPokemon.add(:MAXELIXIR, block: (item, pokemon, battler, choices, scene) => {
	for (int i = pokemon.moves.length; i < pokemon.moves.length; i++) { //for 'pokemon.moves.length' times do => |i|
		BattleRestorePP(pokemon, battler, i, pokemon.moves[i].total_pp);
	}
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
});

//===============================================================================
// BattleUseOnBattler handlers.
// For items used on a Pokémon in battle.
//===============================================================================

ItemHandlers.BattleUseOnBattler.add(:REDFLUTE, block: (item, battler, scene) => {
	battler.CureAttract;
	scene.Display(_INTL("{1} got over its infatuation.", battler.ToString()));
});

ItemHandlers.BattleUseOnBattler.add(:YELLOWFLUTE, block: (item, battler, scene) => {
	battler.CureConfusion;
	scene.Display(_INTL("{1} snapped out of its confusion.", battler.ToString()));
});

ItemHandlers.BattleUseOnBattler.copy(:YELLOWFLUTE, :PERSIMBERRY);

ItemHandlers.BattleUseOnBattler.add(:XATTACK, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ATTACK, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XATTACK2, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ATTACK, 2, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XATTACK3, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ATTACK, 3, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XATTACK6, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ATTACK, 6, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XDEFENSE, block: (item, battler, scene) => {
	battler.RaiseStatStage(:DEFENSE, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XDEFENSE, :XDEFEND);

ItemHandlers.BattleUseOnBattler.add(:XDEFENSE2, block: (item, battler, scene) => {
	battler.RaiseStatStage(:DEFENSE, 2, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XDEFENSE2, :XDEFEND2);

ItemHandlers.BattleUseOnBattler.add(:XDEFENSE3, block: (item, battler, scene) => {
	battler.RaiseStatStage(:DEFENSE, 3, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XDEFENSE3, :XDEFEND3);

ItemHandlers.BattleUseOnBattler.add(:XDEFENSE6, block: (item, battler, scene) => {
	battler.RaiseStatStage(:DEFENSE, 6, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XDEFENSE6, :XDEFEND6);

ItemHandlers.BattleUseOnBattler.add(:XSPATK, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_ATTACK, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XSPATK, :XSPECIAL);

ItemHandlers.BattleUseOnBattler.add(:XSPATK2, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_ATTACK, 2, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XSPATK2, :XSPECIAL2);

ItemHandlers.BattleUseOnBattler.add(:XSPATK3, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_ATTACK, 3, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XSPATK3, :XSPECIAL3);

ItemHandlers.BattleUseOnBattler.add(:XSPATK6, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_ATTACK, 6, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.copy(:XSPATK6, :XSPECIAL6);

ItemHandlers.BattleUseOnBattler.add(:XSPDEF, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_DEFENSE, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPDEF2, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_DEFENSE, 2, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPDEF3, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_DEFENSE, 3, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPDEF6, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPECIAL_DEFENSE, 6, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPEED, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPEED, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPEED2, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPEED, 2, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPEED3, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPEED, 3, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XSPEED6, block: (item, battler, scene) => {
	battler.RaiseStatStage(:SPEED, 6, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XACCURACY, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ACCURACY, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XACCURACY2, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ACCURACY, 2, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XACCURACY3, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ACCURACY, 3, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:XACCURACY6, block: (item, battler, scene) => {
	battler.RaiseStatStage(:ACCURACY, 6, battler);
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:MAXMUSHROOMS, block: (item, battler, scene) => {
	show_anim = true;
	foreach (var stat in GameData.Stat) { //GameData.Stat.each_main_battle do => |stat|
		if (!battler.CanRaiseStatStage(stat.id, battler)) continue;
		battler.RaiseStatStage(stat.id, 1, battler, show_anim);
		show_anim = false;
	}
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:DIREHIT, block: (item, battler, scene) => {
	battler.effects.FocusEnergy = 2;
	scene.Display(_INTL("{1} is getting pumped!", battler.ToString()));
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:DIREHIT2, block: (item, battler, scene) => {
	battler.effects.FocusEnergy = 2;
	scene.Display(_INTL("{1} is getting pumped!", battler.ToString()));
	battler.pokemon.changeHappiness("battleitem");
});

ItemHandlers.BattleUseOnBattler.add(:DIREHIT3, block: (item, battler, scene) => {
	battler.effects.FocusEnergy = 3;
	scene.Display(_INTL("{1} is getting pumped!", battler.ToString()));
	battler.pokemon.changeHappiness("battleitem");
});
