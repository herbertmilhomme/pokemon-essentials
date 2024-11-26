//===============================================================================
// UseText handlers.
//===============================================================================

ItemHandlers.UseText.add(:BICYCLE, block: (item) => {
	next (Game.GameData.PokemonGlobal.bicycle) ? _INTL("Walk") : _INTL("Use");
});

ItemHandlers.UseText.copy(:BICYCLE, :MACHBIKE, :ACROBIKE);

ItemHandlers.UseText.add(:EXPALLOFF, block: (item) => {
	next _INTL("Turn on");
});

ItemHandlers.UseText.add(:EXPALL, block: (item) => {
	next _INTL("Turn off");
});

//===============================================================================
// UseFromBag handlers.
// Return values: 0 = not used
//                1 = used
//                2 = close the Bag to use
// If there is no UseFromBag handler for an item being used from the Bag (not on
// a Pokémon), calls the UseInField handler for it instead.
//===============================================================================

ItemHandlers.UseFromBag.add(:HONEY, block: (item) => {
	next 2;
});

ItemHandlers.UseFromBag.add(:ESCAPEROPE, block: (item) => {
	if (!Game.GameData.game_player.can_map_transfer_with_follower()) {
		Message(_INTL("It can't be used when you have someone with you."));
		next 0;
	}
	if ((Game.GameData.PokemonGlobal.escapePoint rescue false) && Game.GameData.PokemonGlobal.escapePoint.length > 0) {
		next 2;   // End screen and use item
	}
	Message(_INTL("Can't use that here."));
	next 0;
});

ItemHandlers.UseFromBag.add(:BICYCLE, block: (item) => {
	next (BikeCheck) ? 2 : 0;
});

ItemHandlers.UseFromBag.copy(:BICYCLE, :MACHBIKE, :ACROBIKE);

ItemHandlers.UseFromBag.add(:OLDROD, block: (item) => {
	notCliff = Game.GameData.game_map.passable(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction, Game.GameData.game_player);
	if (Game.GameData.game_player.FacingTerrainTag.can_fish && (Game.GameData.PokemonGlobal.surfing || notCliff)) next 2;
	Message(_INTL("Can't use that here."));
	next 0;
});

ItemHandlers.UseFromBag.copy(:OLDROD, :GOODROD, :SUPERROD);

ItemHandlers.UseFromBag.add(:ITEMFINDER, block: (item) => {
	next 2;
});

ItemHandlers.UseFromBag.copy(:ITEMFINDER, :DOWSINGMCHN, :DOWSINGMACHINE);

ItemHandlers.UseFromBag.add(:TOWNMAP, block: (item) => {
	FadeOutIn do;
		scene = new PokemonRegionMap_Scene(-1, false);
		screen = new PokemonRegionMapScreen(scene);
		ret = screen.StartScreen;
		if (ret) Game.GameData.game_temp.fly_destination = ret;
		if (ret) next 99999;   // Ugly hack to make Bag scene not reappear if flying
	}
	next (Game.GameData.game_temp.fly_destination) ? 2 : 0;
});

ItemHandlers.UseFromBag.addIf(:move_machines,
	block: (item) => { GameData.Item.get(item).is_machine() },
	block: (item) => {
		if (Game.GameData.player.pokemon_count == 0) {
			Message(_INTL("There is no Pokémon."));
			next 0;
		}
		item_data = GameData.Item.get(item);
		move = item_data.move;
		if (!move) next 0;
		Message("\\se[PC access]" + _INTL("You booted up the {1}.", item_data.name) + "\1");
		if ((!ConfirmMessage(_INTL("Do you want to teach {1} to a Pokémon?",
																			GameData.Move.get(move).name))) next 0;
		if (MoveTutorChoose(move, null, true, item_data.is_TR())) next 1;
		next 0;
	}
)

//===============================================================================
// ConfirmUseInField handlers.
// Return values: true/false
// Called when an item is used from the Ready Menu.
// If an item does not have this handler, it is treated as returning true.
//===============================================================================

ItemHandlers.ConfirmUseInField.add(:ESCAPEROPE, block: (item) => {
	escape = (Game.GameData.PokemonGlobal.escapePoint rescue null);
	if (!escape || escape == new List<string>()) {
		Message(_INTL("Can't use that here."));
		next false;
	}
	if (!Game.GameData.game_player.can_map_transfer_with_follower()) {
		Message(_INTL("It can't be used when you have someone with you."));
		next false;
	}
	mapname = GetMapNameFromId(escape[0]);
	next ConfirmMessage(_INTL("Want to escape from here and return to {1}?", mapname));
});

//===============================================================================
// UseInField handlers.
// Return values: false = not used
//                true = used
// Called if an item is used from the Bag (not on a Pokémon and not a TM/HM) and
// there is no UseFromBag handler above.
// If an item has this handler, it can be registered to the Ready Menu.
//===============================================================================

public void Repel(item, steps) {
	if (Game.GameData.PokemonGlobal.repel > 0) {
		Message(_INTL("But a repellent's effect still lingers from earlier."));
		return false;
	}
	SEPlay("Repel");
	Game.GameData.stats.repel_count += 1;
	UseItemMessage(item);
	Game.GameData.PokemonGlobal.repel = steps;
	return true;
}

ItemHandlers.UseInField.add(:REPEL, block: (item) => {
	next Repel(item, 100);
});

ItemHandlers.UseInField.add(:SUPERREPEL, block: (item) => {
	next Repel(item, 200);
});

ItemHandlers.UseInField.add(:MAXREPEL, block: (item) => {
	next Repel(item, 250);
});

EventHandlers.add(:on_player_step_taken, :repel_counter,
	block: () => {
		if (Game.GameData.PokemonGlobal.repel <= 0 || Game.GameData.game_player.terrain_tag.ice) continue;   // Shouldn't count down if on ice
		Game.GameData.PokemonGlobal.repel -= 1;
		if (Game.GameData.PokemonGlobal.repel > 0) continue;
		repels = new List<string>();
		GameData.Item.each(itm => { if (itm.has_flag("Repel")) repels.Add(itm.id); });
		if (repels.none(item => Game.GameData.bag.has(item))) {
			Message(_INTL("The repellent's effect wore off!"));
			continue;
		}
		if (!ConfirmMessage(_INTL("The repellent's effect wore off! Would you like to use another one?"))) continue;
		ret = null;
		FadeOutIn do;
			scene = new PokemonBag_Scene();
			screen = new PokemonBagScreen(scene, Game.GameData.bag);
			ret = screen.ChooseItemScreen(block: (item) => { repels.Contains(item) });
		}
		if (ret) UseItem(Game.GameData.bag, ret);
	}
)

ItemHandlers.UseInField.add(:BLACKFLUTE, block: (item) => {
	UseItemMessage(item);
	if (Settings.FLUTES_CHANGE_WILD_ENCOUNTER_LEVELS) {
		Message(_INTL("Now you're more likely to encounter high-level Pokémon!"));
		Game.GameData.PokemonMap.higher_level_wild_pokemon = true;
		Game.GameData.PokemonMap.lower_level_wild_pokemon = false;
	} else {
		Message(_INTL("The likelihood of encountering Pokémon decreased!"));
		Game.GameData.PokemonMap.lower_encounter_rate = true;
		Game.GameData.PokemonMap.higher_encounter_rate = false;
	}
	next true;
});

ItemHandlers.UseInField.add(:WHITEFLUTE, block: (item) => {
	UseItemMessage(item);
	if (Settings.FLUTES_CHANGE_WILD_ENCOUNTER_LEVELS) {
		Message(_INTL("Now you're more likely to encounter low-level Pokémon!"));
		Game.GameData.PokemonMap.lower_level_wild_pokemon = true;
		Game.GameData.PokemonMap.higher_level_wild_pokemon = false;
	} else {
		Message(_INTL("The likelihood of encountering Pokémon increased!"));
		Game.GameData.PokemonMap.higher_encounter_rate = true;
		Game.GameData.PokemonMap.lower_encounter_rate = false;
	}
	next true;
});

ItemHandlers.UseInField.add(:HONEY, block: (item) => {
	UseItemMessage(item);
	SweetScent;
	next true;
});

ItemHandlers.UseInField.add(:ESCAPEROPE, block: (item) => {
	escape = (Game.GameData.PokemonGlobal.escapePoint rescue null);
	if (!escape || escape == new List<string>()) {
		Message(_INTL("Can't use that here."));
		next false;
	}
	if (!Game.GameData.game_player.can_map_transfer_with_follower()) {
		Message(_INTL("It can't be used when you have someone with you."));
		next false;
	}
	UseItemMessage(item);
	FadeOutIn do;
		Game.GameData.game_temp.player_new_map_id    = escape[0];
		Game.GameData.game_temp.player_new_x         = escape[1];
		Game.GameData.game_temp.player_new_y         = escape[2];
		Game.GameData.game_temp.player_new_direction = escape[3];
		CancelVehicles;
		Game.GameData.scene.transfer_player;
		Game.GameData.game_map.autoplay;
		Game.GameData.game_map.refresh;
	}
	EraseEscapePoint;
	next true;
});

ItemHandlers.UseInField.add(:SACREDASH, block: (item) => {
	if (Game.GameData.player.pokemon_count == 0) {
		Message(_INTL("There is no Pokémon."));
		next false;
	}
	canrevive = false;
	foreach (var i in Game.GameData.player.pokemon_party) { //'Game.GameData.player.pokemon_party.each' do => |i|
		if (!i.fainted()) continue;
		canrevive = true;
		break;
	}
	if (!canrevive) {
		Message(_INTL("It won't have any effect."));
		next false;
	}
	revived = 0;
	FadeOutIn do;
		scene = new PokemonParty_Scene();
		screen = new PokemonPartyScreen(scene, Game.GameData.player.party);
		screen.StartScene(_INTL("Using item..."), false);
		SEPlay("Use item in party");
		Game.GameData.player.party.each_with_index do |pkmn, i|
			if (!pkmn.fainted()) continue;
			revived += 1;
			pkmn.heal;
			screen.RefreshSingle(i);
			screen.Display(_INTL("{1}'s HP was restored.", pkmn.name));
		}
		if (revived == 0) screen.Display(_INTL("It won't have any effect."));
		screen.EndScene;
	}
	next (revived > 0);
});

ItemHandlers.UseInField.add(:BICYCLE, block: (item) => {
	if (BikeCheck) {
		if (Game.GameData.PokemonGlobal.bicycle) {
			DismountBike;
		} else {
			MountBike;
		}
		next true;
	}
	next false;
});

ItemHandlers.UseInField.copy(:BICYCLE, :MACHBIKE, :ACROBIKE);

ItemHandlers.UseInField.add(:OLDROD, block: (item) => {
	notCliff = Game.GameData.game_map.passable(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction, Game.GameData.game_player);
	if (!Game.GameData.game_player.FacingTerrainTag.can_fish || (!Game.GameData.PokemonGlobal.surfing && !notCliff)) {
		Message(_INTL("Can't use that here."));
		next false;
	}
	encounter = Game.GameData.PokemonEncounters.has_encounter_type(types.OldRod);
	if (Fishing(encounter, 1)) {
		Game.GameData.stats.fishing_battles += 1;
		Encounter(:OldRod);
	}
	next true;
});

ItemHandlers.UseInField.add(:GOODROD, block: (item) => {
	notCliff = Game.GameData.game_map.passable(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction, Game.GameData.game_player);
	if (!Game.GameData.game_player.FacingTerrainTag.can_fish || (!Game.GameData.PokemonGlobal.surfing && !notCliff)) {
		Message(_INTL("Can't use that here."));
		next false;
	}
	encounter = Game.GameData.PokemonEncounters.has_encounter_type(types.GoodRod);
	if (Fishing(encounter, 2)) {
		Game.GameData.stats.fishing_battles += 1;
		Encounter(:GoodRod);
	}
	next true;
});

ItemHandlers.UseInField.add(:SUPERROD, block: (item) => {
	notCliff = Game.GameData.game_map.passable(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction, Game.GameData.game_player);
	if (!Game.GameData.game_player.FacingTerrainTag.can_fish || (!Game.GameData.PokemonGlobal.surfing && !notCliff)) {
		Message(_INTL("Can't use that here."));
		next false;
	}
	encounter = Game.GameData.PokemonEncounters.has_encounter_type(types.SuperRod);
	if (Fishing(encounter, 3)) {
		Game.GameData.stats.fishing_battles += 1;
		Encounter(:SuperRod);
	}
	next true;
});

ItemHandlers.UseInField.add(:ITEMFINDER, block: (item) => {
	Game.GameData.stats.itemfinder_count += 1;
	SEPlay("Itemfinder");
	event = ClosestHiddenItem;
	if (!event) {
		Message(_INTL("... \\wt[10]... \\wt[10]... \\wt[10]... \\wt[10]Nope! There's no response."));
		next true;
	}
	offsetX = event.x - Game.GameData.game_player.x;
	offsetY = event.y - Game.GameData.game_player.y;
	if (offsetX == 0 && offsetY == 0) {   // Standing on the item, spin around
		4.times do;
			Wait(0.2);
			Game.GameData.game_player.turn_right_90;
		}
		Wait(0.3);
		Message(_INTL("The {1}'s indicating something right underfoot!", GameData.Item.get(item).name));
	} else {   // Item is nearby, face towards it
		direction = Game.GameData.game_player.direction;
		if (offsetX.abs > offsetY.abs) {
			direction = (offsetX < 0) ? 4 : 6;
		} else {
			direction = (offsetY < 0) ? 8 : 2;
		}
		switch (direction) {
			case 2:  Game.GameData.game_player.turn_down; break;
			case 4:  Game.GameData.game_player.turn_left; break;
			case 6:  Game.GameData.game_player.turn_right; break;
			case 8:  Game.GameData.game_player.turn_up; break;
		}
		Wait(0.3);
		Message(_INTL("Huh? The {1}'s responding!", GameData.Item.get(item).name) + "\1");
		Message(_INTL("There's an item buried around here!"));
	}
	next true;
});

ItemHandlers.UseInField.copy(:ITEMFINDER, :DOWSINGMCHN, :DOWSINGMACHINE);

ItemHandlers.UseInField.add(:TOWNMAP, block: (item) => {
	if (Game.GameData.game_temp.fly_destination.null()) ShowMap(-1, false);
	FlyToNewLocation;
	next true;
});

ItemHandlers.UseInField.add(:COINCASE, block: (item) => {
	Message(_INTL("Coins: {1}", Game.GameData.player.coins.to_s_formatted));
	next true;
});

ItemHandlers.UseInField.add(:EXPALL, block: (item) => {
	Game.GameData.bag.replace_item(:EXPALL, :EXPALLOFF);
	Message(_INTL("The Exp Share was turned off."));
	next true;
});

ItemHandlers.UseInField.add(:EXPALLOFF, block: (item) => {
	Game.GameData.bag.replace_item(:EXPALLOFF, :EXPALL);
	Message(_INTL("The Exp Share was turned on."));
	next true;
});

//===============================================================================
// UseOnPokemon handlers.
//===============================================================================

// Applies to all items defined as an evolution stone.
// No need to add more code for new ones.
ItemHandlers.UseOnPokemon.addIf(:evolution_stones,
	block: (item) => { GameData.Item.get(item).is_evolution_stone() },
	block: (item, qty, pkmn, scene) => {
		if (pkmn.shadowPokemon()) {
			scene.Display(_INTL("It won't have any effect."));
			next false;
		}
		newspecies = pkmn.check_evolution_on_use_item(item);
		if (newspecies) {
			FadeOutInWithMusic do;
				evo = new PokemonEvolutionScene();
				evo.StartScreen(pkmn, newspecies);
				evo.Evolution(false);
				evo.EndScreen;
				if (scene.is_a(PokemonPartyScreen)) {
					scene.RefreshAnnotations(block: (p) => { !p.check_evolution_on_use_item(item).null() });
					scene.Refresh;
				}
			}
			next true;
		}
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
)

ItemHandlers.UseOnPokemon.add(:SCROLLOFWATERS, block: (item, qty, pkmn, scene) => {
	if (pkmn.shadowPokemon()) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	newspecies = pkmn.check_evolution_on_use_item(item);
	if (newspecies) {
		pkmn.form = 1;   // NOTE: This is the only difference to the generic evolution stone code.
		FadeOutInWithMusic do;
			evo = new PokemonEvolutionScene();
			evo.StartScreen(pkmn, newspecies);
			evo.Evolution(false);
			evo.EndScreen;
			if (scene.is_a(PokemonPartyScreen)) {
				scene.RefreshAnnotations(block: (p) => { !p.check_evolution_on_use_item(item).null() });
				scene.Refresh;
			}
		}
		next true;
	}
	scene.Display(_INTL("It won't have any effect."));
	next false;
});

ItemHandlers.UseOnPokemon.add(:POTION, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, 20, scene);
});

ItemHandlers.UseOnPokemon.copy(:POTION, :BERRYJUICE, :SWEETHEART);
ItemHandlers.UseOnPokemon.copy(:POTION, :RAGECANDYBAR) if !Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS;

ItemHandlers.UseOnPokemon.add(:SUPERPOTION, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 60 : 50, scene);
});

ItemHandlers.UseOnPokemon.add(:HYPERPOTION, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 120 : 200, scene);
});

ItemHandlers.UseOnPokemon.add(:MAXPOTION, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, pkmn.totalhp - pkmn.hp, scene);
});

ItemHandlers.UseOnPokemon.add(:FRESHWATER, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 30 : 50, scene);
});

ItemHandlers.UseOnPokemon.add(:SODAPOP, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 50 : 60, scene);
});

ItemHandlers.UseOnPokemon.add(:LEMONADE, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 70 : 80, scene);
});

ItemHandlers.UseOnPokemon.add(:MOOMOOMILK, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, 100, scene);
});

ItemHandlers.UseOnPokemon.add(:ORANBERRY, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, 10, scene);
});

ItemHandlers.UseOnPokemon.add(:SITRUSBERRY, block: (item, qty, pkmn, scene) => {
	next HPItem(pkmn, pkmn.totalhp / 4, scene);
});

ItemHandlers.UseOnPokemon.add(:AWAKENING, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status != statuses.SLEEP) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} woke up.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:AWAKENING, :CHESTOBERRY, :BLUEFLUTE, :POKEFLUTE);

ItemHandlers.UseOnPokemon.add(:ANTIDOTE, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status != statuses.POISON) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} was cured of its poisoning.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:ANTIDOTE, :PECHABERRY);

ItemHandlers.UseOnPokemon.add(:BURNHEAL, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status != statuses.BURN) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1}'s burn was healed.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:BURNHEAL, :RAWSTBERRY);

ItemHandlers.UseOnPokemon.add(:PARALYZEHEAL, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status != statuses.PARALYSIS) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} was cured of paralysis.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:PARALYZEHEAL, :PARLYZHEAL, :CHERIBERRY);

ItemHandlers.UseOnPokemon.add(:ICEHEAL, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status != statuses.FROZEN) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} was thawed out.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:ICEHEAL, :ASPEARBERRY);

ItemHandlers.UseOnPokemon.add(:FULLHEAL, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status == statuses.NONE) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1} became healthy.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:FULLHEAL,
																:LAVACOOKIE, :OLDGATEAU, :CASTELIACONE,
																:LUMIOSEGALETTE, :SHALOURSABLE, :BIGMALASADA,
																:PEWTERCRUNCHIES, :LUMBERRY);
ItemHandlers.UseOnPokemon.copy(:FULLHEAL, :RAGECANDYBAR) if Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS;

ItemHandlers.UseOnPokemon.add(:FULLRESTORE, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || (pkmn.hp == pkmn.totalhp && pkmn.status == statuses.NONE)) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	hpgain = ItemRestoreHP(pkmn, pkmn.totalhp - pkmn.hp);
	pkmn.heal_status;
	scene.Refresh;
	if (hpgain > 0) {
		scene.Display(_INTL("{1}'s HP was restored by {2} points.", pkmn.name, hpgain));
	} else {
		scene.Display(_INTL("{1} became healthy.", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:REVIVE, block: (item, qty, pkmn, scene) => {
	if (!pkmn.fainted()) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.hp = (int)Math.Floor(pkmn.totalhp / 2);
	if (pkmn.hp <= 0) pkmn.hp = 1;
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1}'s HP was restored.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.add(:MAXREVIVE, block: (item, qty, pkmn, scene) => {
	if (!pkmn.fainted()) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_HP;
	pkmn.heal_status;
	scene.Refresh;
	scene.Display(_INTL("{1}'s HP was restored.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:MAXREVIVE, :MAXHONEY);

ItemHandlers.UseOnPokemon.add(:ENERGYPOWDER, block: (item, qty, pkmn, scene) => {
	if (HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 60 : 50, scene)) {
		pkmn.changeHappiness("powder");
		next true;
	}
	next false;
});

ItemHandlers.UseOnPokemon.add(:ENERGYROOT, block: (item, qty, pkmn, scene) => {
	if (HPItem(pkmn, (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 120 : 200, scene)) {
		pkmn.changeHappiness("energyroot");
		next true;
	}
	next false;
});

ItemHandlers.UseOnPokemon.add(:HEALPOWDER, block: (item, qty, pkmn, scene) => {
	if (pkmn.fainted() || pkmn.status == statuses.NONE) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_status;
	pkmn.changeHappiness("powder");
	scene.Refresh;
	scene.Display(_INTL("{1} became healthy.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.add(:REVIVALHERB, block: (item, qty, pkmn, scene) => {
	if (!pkmn.fainted()) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.heal_HP;
	pkmn.heal_status;
	pkmn.changeHappiness("revivalherb");
	scene.Refresh;
	scene.Display(_INTL("{1}'s HP was restored.", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.add(:ETHER, block: (item, qty, pkmn, scene) => {
	move = scene.ChooseMove(pkmn, _INTL("Restore which move?"));
	if (move < 0) next false;
	if (RestorePP(pkmn, move, 10) == 0) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
	next true;
});

ItemHandlers.UseOnPokemon.copy(:ETHER, :LEPPABERRY);

ItemHandlers.UseOnPokemon.add(:MAXETHER, block: (item, qty, pkmn, scene) => {
	move = scene.ChooseMove(pkmn, _INTL("Restore which move?"));
	if (move < 0) next false;
	if (RestorePP(pkmn, move, pkmn.moves[move].total_pp - pkmn.moves[move].pp) == 0) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
	next true;
});

ItemHandlers.UseOnPokemon.add(:ELIXIR, block: (item, qty, pkmn, scene) => {
	pprestored = 0;
	for (int i = pkmn.moves.length; i < pkmn.moves.length; i++) { //for 'pkmn.moves.length' times do => |i|
		pprestored += RestorePP(pkmn, i, 10);
	}
	if (pprestored == 0) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
	next true;
});

ItemHandlers.UseOnPokemon.add(:MAXELIXIR, block: (item, qty, pkmn, scene) => {
	pprestored = 0;
	for (int i = pkmn.moves.length; i < pkmn.moves.length; i++) { //for 'pkmn.moves.length' times do => |i|
		pprestored += RestorePP(pkmn, i, pkmn.moves[i].total_pp - pkmn.moves[i].pp);
	}
	if (pprestored == 0) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	scene.Display(_INTL("PP was restored."));
	next true;
});

ItemHandlers.UseOnPokemon.add(:PPUP, block: (item, qty, pkmn, scene) => {
	move = scene.ChooseMove(pkmn, _INTL("Boost PP of which move?"));
	if (move < 0) next false;
	if (pkmn.moves[move].total_pp <= 1 || pkmn.moves[move].ppup >= 3) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.moves[move].ppup += 1;
	movename = pkmn.moves[move].name;
	scene.Display(_INTL("{1}'s PP increased.", movename));
	next true;
});

ItemHandlers.UseOnPokemon.add(:PPMAX, block: (item, qty, pkmn, scene) => {
	move = scene.ChooseMove(pkmn, _INTL("Boost PP of which move?"));
	if (move < 0) next false;
	if (pkmn.moves[move].total_pp <= 1 || pkmn.moves[move].ppup >= 3) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	SEPlay("Use item in party");
	pkmn.moves[move].ppup = 3;
	movename = pkmn.moves[move].name;
	scene.Display(_INTL("{1}'s PP increased.", movename));
	next true;
});

ItemHandlers.UseOnPokemonMaximum.add(:HPUP, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:HP, 10, pkmn, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemonMaximum.copy(:HPUP, :HEALTHMOCHI);

ItemHandlers.UseOnPokemon.add(:HPUP, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:HP, 10, qty, pkmn, "vitamin", scene, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemon.copy(:HPUP, :HEALTHMOCHI);

ItemHandlers.UseOnPokemonMaximum.add(:PROTEIN, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:ATTACK, 10, pkmn, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemonMaximum.copy(:PROTEIN, :MUSCLEMOCHI);

ItemHandlers.UseOnPokemon.add(:PROTEIN, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:ATTACK, 10, qty, pkmn, "vitamin", scene, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemon.copy(:PROTEIN, :MUSCLEMOCHI);

ItemHandlers.UseOnPokemonMaximum.add(:IRON, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:DEFENSE, 10, pkmn, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemonMaximum.copy(:IRON, :RESISTMOCHI);

ItemHandlers.UseOnPokemon.add(:IRON, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:DEFENSE, 10, qty, pkmn, "vitamin", scene, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemon.copy(:IRON, :RESISTMOCHI);

ItemHandlers.UseOnPokemonMaximum.add(:CALCIUM, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:SPECIAL_ATTACK, 10, pkmn, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemonMaximum.copy(:CALCIUM, :GENIUSMOCHI);

ItemHandlers.UseOnPokemon.add(:CALCIUM, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:SPECIAL_ATTACK, 10, qty, pkmn, "vitamin", scene, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemon.copy(:CALCIUM, :GENIUSMOCHI);

ItemHandlers.UseOnPokemonMaximum.add(:ZINC, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:SPECIAL_DEFENSE, 10, pkmn, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemonMaximum.copy(:ZINC, :CLEVERMOCHI);

ItemHandlers.UseOnPokemon.add(:ZINC, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:SPECIAL_DEFENSE, 10, qty, pkmn, "vitamin", scene, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemon.copy(:ZINC, :CLEVERMOCHI);

ItemHandlers.UseOnPokemonMaximum.add(:CARBOS, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:SPEED, 10, pkmn, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemonMaximum.copy(:CARBOS, :SWIFTMOCHI);

ItemHandlers.UseOnPokemon.add(:CARBOS, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:SPEED, 10, qty, pkmn, "vitamin", scene, Settings.NO_VITAMIN_EV_CAP);
});

ItemHandlers.UseOnPokemon.copy(:CARBOS, :SWIFTMOCHI);

ItemHandlers.UseOnPokemonMaximum.add(:HEALTHFEATHER, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:HP, 1, pkmn, true);
});

ItemHandlers.UseOnPokemonMaximum.copy(:HEALTHFEATHER, :HEALTHWING);

ItemHandlers.UseOnPokemon.add(:HEALTHFEATHER, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:HP, 1, qty, pkmn, "wing", scene, true);
});

ItemHandlers.UseOnPokemon.copy(:HEALTHFEATHER, :HEALTHWING);

ItemHandlers.UseOnPokemonMaximum.add(:MUSCLEFEATHER, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:ATTACK, 1, pkmn, true);
});

ItemHandlers.UseOnPokemonMaximum.copy(:MUSCLEFEATHER, :MUSCLEWING);

ItemHandlers.UseOnPokemon.add(:MUSCLEFEATHER, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:ATTACK, 1, qty, pkmn, "wing", scene, true);
});

ItemHandlers.UseOnPokemon.copy(:MUSCLEFEATHER, :MUSCLEWING);

ItemHandlers.UseOnPokemonMaximum.add(:RESISTFEATHER, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:DEFENSE, 1, pkmn, true);
});

ItemHandlers.UseOnPokemonMaximum.copy(:RESISTFEATHER, :RESISTWING);

ItemHandlers.UseOnPokemon.add(:RESISTFEATHER, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:DEFENSE, 1, qty, pkmn, "wing", scene, true);
});

ItemHandlers.UseOnPokemon.copy(:RESISTFEATHER, :RESISTWING);

ItemHandlers.UseOnPokemonMaximum.add(:GENIUSFEATHER, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:SPECIAL_ATTACK, 1, pkmn, true);
});

ItemHandlers.UseOnPokemonMaximum.copy(:GENIUSFEATHER, :GENIUSWING);

ItemHandlers.UseOnPokemon.add(:GENIUSFEATHER, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:SPECIAL_ATTACK, 1, qty, pkmn, "wing", scene, true);
});

ItemHandlers.UseOnPokemon.copy(:GENIUSFEATHER, :GENIUSWING);

ItemHandlers.UseOnPokemonMaximum.add(:CLEVERFEATHER, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:SPECIAL_DEFENSE, 1, pkmn, true);
});

ItemHandlers.UseOnPokemonMaximum.copy(:CLEVERFEATHER, :CLEVERWING);

ItemHandlers.UseOnPokemon.add(:CLEVERFEATHER, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:SPECIAL_DEFENSE, 1, qty, pkmn, "wing", scene, true);
});

ItemHandlers.UseOnPokemon.copy(:CLEVERFEATHER, :CLEVERWING);

ItemHandlers.UseOnPokemonMaximum.add(:SWIFTFEATHER, block: (item, pkmn) => {
	next MaxUsesOfEVRaisingItem(:SPEED, 1, pkmn, true);
});

ItemHandlers.UseOnPokemonMaximum.copy(:SWIFTFEATHER, :SWIFTWING);

ItemHandlers.UseOnPokemon.add(:SWIFTFEATHER, block: (item, qty, pkmn, scene) => {
	next UseEVRaisingItem(:SPEED, 1, qty, pkmn, "wing", scene, true);
});

ItemHandlers.UseOnPokemon.copy(:SWIFTFEATHER, :SWIFTWING);

ItemHandlers.UseOnPokemon.add(:FRESHSTARTMOCHI, block: (item, qty, pkmn, scene) => {
	if (!pkmn.ev.any((stat, value) => value > 0)) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	GameData.Stat.each_main(s => pkmn.ev[s.id] = 0);
	scene.Display(_INTL("{1}'s base points were all reset to zero!", pkmn.name));
	next true;
});

ItemHandlers.UseOnPokemon.add(:LONELYMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:LONELY, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:ADAMANTMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:ADAMANT, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:NAUGHTYMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:NAUGHTY, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:BRAVEMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:BRAVE, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:BOLDMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:BOLD, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:IMPISHMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:IMPISH, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:LAXMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:LAX, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:RELAXEDMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:RELAXED, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:MODESTMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:MODEST, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:MILDMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:MILD, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:RASHMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:RASH, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:QUIETMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:QUIET, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:CALMMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:CALM, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:GENTLEMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:GENTLE, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:CAREFULMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:CAREFUL, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:SASSYMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:SASSY, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:TIMIDMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:TIMID, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:HASTYMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:HASTY, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:JOLLYMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:JOLLY, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:NAIVEMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:NAIVE, item, pkmn, scene);
});

ItemHandlers.UseOnPokemon.add(:SERIOUSMINT, block: (item, qty, pkmn, scene) => {
	NatureChangingMint(:SERIOUS, item, pkmn, scene);
});

ItemHandlers.UseOnPokemonMaximum.add(:RARECANDY, block: (item, pkmn) => {
	next GameData.GrowthRate.max_level - pkmn.level;
});

ItemHandlers.UseOnPokemon.add(:RARECANDY, block: (item, qty, pkmn, scene) => {
	if (pkmn.shadowPokemon()) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	}
	if (pkmn.level >= GameData.GrowthRate.max_level) {
		new_species = pkmn.check_evolution_on_level_up;
		if (!Settings.RARE_CANDY_USABLE_AT_MAX_LEVEL || !new_species) {
			scene.Display(_INTL("It won't have any effect."));
			next false;
		}
		// Check for evolution
		FadeOutInWithMusic do;
			evo = new PokemonEvolutionScene();
			evo.StartScreen(pkmn, new_species);
			evo.Evolution;
			evo.EndScreen;
			if (scene.is_a(PokemonPartyScreen)) scene.Refresh;
		}
		next true;
	}
	// Level up
	SEPlay("Pkmn level up");
	ChangeLevel(pkmn, pkmn.level + qty, scene);
	scene.HardRefresh;
	next true;
});

ItemHandlers.UseOnPokemonMaximum.add(:EXPCANDYXS, block: (item, pkmn) => {
	gain_amount = 100;
	next ((pkmn.growth_rate.maximum_exp - pkmn.exp) / gain_amount.to_f).ceil;
});

ItemHandlers.UseOnPokemon.add(:EXPCANDYXS, block: (item, qty, pkmn, scene) => {
	next GainExpFromExpCandy(pkmn, 100, qty, scene);
});

ItemHandlers.UseOnPokemonMaximum.add(:EXPCANDYS, block: (item, pkmn) => {
	gain_amount = 800;
	next ((pkmn.growth_rate.maximum_exp - pkmn.exp) / gain_amount.to_f).ceil;
});

ItemHandlers.UseOnPokemon.add(:EXPCANDYS, block: (item, qty, pkmn, scene) => {
	next GainExpFromExpCandy(pkmn, 800, qty, scene);
});

ItemHandlers.UseOnPokemonMaximum.add(:EXPCANDYM, block: (item, pkmn) => {
	gain_amount = 3_000;
	next ((pkmn.growth_rate.maximum_exp - pkmn.exp) / gain_amount.to_f).ceil;
});

ItemHandlers.UseOnPokemon.add(:EXPCANDYM, block: (item, qty, pkmn, scene) => {
	next GainExpFromExpCandy(pkmn, 3_000, qty, scene);
});

ItemHandlers.UseOnPokemonMaximum.add(:EXPCANDYL, block: (item, pkmn) => {
	gain_amount = 10_000;
	next ((pkmn.growth_rate.maximum_exp - pkmn.exp) / gain_amount.to_f).ceil;
});

ItemHandlers.UseOnPokemon.add(:EXPCANDYL, block: (item, qty, pkmn, scene) => {
	next GainExpFromExpCandy(pkmn, 10_000, qty, scene);
});

ItemHandlers.UseOnPokemonMaximum.add(:EXPCANDYXL, block: (item, pkmn) => {
	gain_amount = 30_000;
	next ((pkmn.growth_rate.maximum_exp - pkmn.exp) / gain_amount.to_f).ceil;
});

ItemHandlers.UseOnPokemon.add(:EXPCANDYXL, block: (item, qty, pkmn, scene) => {
	next GainExpFromExpCandy(pkmn, 30_000, qty, scene);
});

ItemHandlers.UseOnPokemonMaximum.add(:POMEGBERRY, block: (item, pkmn) => {
	next MaxUsesOfEVLoweringBerry(:HP, pkmn);
});

ItemHandlers.UseOnPokemon.add(:POMEGBERRY, block: (item, qty, pkmn, scene) => {
	next RaiseHappinessAndLowerEV(
		pkmn, scene, :HP, qty, new {
			_INTL("{1} adores you! Its base HP fell!", pkmn.name),
			_INTL("{1} became more friendly. Its base HP can't go lower.", pkmn.name),
			_INTL("{1} became more friendly. However, its base HP fell!", pkmn.name);
		}
	);
});

ItemHandlers.UseOnPokemonMaximum.add(:KELPSYBERRY, block: (item, pkmn) => {
	next MaxUsesOfEVLoweringBerry(:ATTACK, pkmn);
});

ItemHandlers.UseOnPokemon.add(:KELPSYBERRY, block: (item, qty, pkmn, scene) => {
	next RaiseHappinessAndLowerEV(
		pkmn, scene, :ATTACK, qty, new {
			_INTL("{1} adores you! Its base Attack fell!", pkmn.name),
			_INTL("{1} became more friendly. Its base Attack can't go lower.", pkmn.name),
			_INTL("{1} became more friendly. However, its base Attack fell!", pkmn.name);
		}
	);
});

ItemHandlers.UseOnPokemonMaximum.add(:QUALOTBERRY, block: (item, pkmn) => {
	next MaxUsesOfEVLoweringBerry(:DEFENSE, pkmn);
});

ItemHandlers.UseOnPokemon.add(:QUALOTBERRY, block: (item, qty, pkmn, scene) => {
	next RaiseHappinessAndLowerEV(
		pkmn, scene, :DEFENSE, qty, new {
			_INTL("{1} adores you! Its base Defense fell!", pkmn.name),
			_INTL("{1} became more friendly. Its base Defense can't go lower.", pkmn.name),
			_INTL("{1} became more friendly. However, its base Defense fell!", pkmn.name);
		}
	);
});

ItemHandlers.UseOnPokemonMaximum.add(:HONDEWBERRY, block: (item, pkmn) => {
	next MaxUsesOfEVLoweringBerry(:SPECIAL_ATTACK, pkmn);
});

ItemHandlers.UseOnPokemon.add(:HONDEWBERRY, block: (item, qty, pkmn, scene) => {
	next RaiseHappinessAndLowerEV(
		pkmn, scene, :SPECIAL_ATTACK, qty, new {
			_INTL("{1} adores you! Its base Special Attack fell!", pkmn.name),
			_INTL("{1} became more friendly. Its base Special Attack can't go lower.", pkmn.name),
			_INTL("{1} became more friendly. However, its base Special Attack fell!", pkmn.name);
		}
	);
});

ItemHandlers.UseOnPokemonMaximum.add(:GREPABERRY, block: (item, pkmn) => {
	next MaxUsesOfEVLoweringBerry(:SPECIAL_DEFENSE, pkmn);
});

ItemHandlers.UseOnPokemon.add(:GREPABERRY, block: (item, qty, pkmn, scene) => {
	next RaiseHappinessAndLowerEV(
		pkmn, scene, :SPECIAL_DEFENSE, qty, new {
			_INTL("{1} adores you! Its base Special Defense fell!", pkmn.name),
			_INTL("{1} became more friendly. Its base Special Defense can't go lower.", pkmn.name),
			_INTL("{1} became more friendly. However, its base Special Defense fell!", pkmn.name);
		}
	);
});

ItemHandlers.UseOnPokemonMaximum.add(:TAMATOBERRY, block: (item, pkmn) => {
	next MaxUsesOfEVLoweringBerry(:SPEED, pkmn);
});

ItemHandlers.UseOnPokemon.add(:TAMATOBERRY, block: (item, qty, pkmn, scene) => {
	next RaiseHappinessAndLowerEV(
		pkmn, scene, :SPEED, qty, new {
			_INTL("{1} adores you! Its base Speed fell!", pkmn.name),
			_INTL("{1} became more friendly. Its base Speed can't go lower.", pkmn.name),
			_INTL("{1} became more friendly. However, its base Speed fell!", pkmn.name);
		}
	);
});

ItemHandlers.UseOnPokemon.add(:ABILITYCAPSULE, block: (item, qty, pkmn, scene) => {
	if (scene.Confirm(_INTL("Do you want to change {1}'s Ability?", pkmn.name))) {
		abils = pkmn.getAbilityList;
		abil1 = null;
		abil2 = null;
		foreach (var i in abils) { //'abils.each' do => |i|
			if (i[1] == 0) abil1 = i[0];
			if (i[1] == 1) abil2 = i[0];
		}
		if (abil1.null() || abil2.null() || pkmn.hasHiddenAbility() || pkmn.isSpecies(Speciess.ZYGARDE)) {
			scene.Display(_INTL("It won't have any effect."));
			next false;
		}
		newabil = (pkmn.ability_index + 1) % 2;
		newabilname = GameData.Ability.get((newabil == 0) ? abil1 : abil2).name;
		pkmn.ability_index = newabil;
		pkmn.ability = null;
		scene.Refresh;
		scene.Display(_INTL("{1}'s Ability changed! Its Ability is now {2}!", pkmn.name, newabilname));
		next true;
	}
	next false;
});

ItemHandlers.UseOnPokemon.add(:ABILITYPATCH, block: (item, qty, pkmn, scene) => {
	if (scene.Confirm(_INTL("Do you want to change {1}'s Ability?", pkmn.name))) {
		abils = pkmn.getAbilityList;
		new_ability_id = null;
		if (pkmn.hasHiddenAbility()) {
			if (Settings.MECHANICS_GENERATION >= 9) new_ability_id = 0;   // First regular ability
		} else {
			abils.each(a => { if (a[1] == 2) new_ability_id = a[0]; });   // Hidden ability
		}
		if (!new_ability_id || pkmn.isSpecies(Speciess.ZYGARDE)) {
			scene.Display(_INTL("It won't have any effect."));
			next false;
		}
		new_ability_name = GameData.Ability.get(new_ability_id).name;
		pkmn.ability_index = 2;
		pkmn.ability = null;
		scene.Refresh;
		scene.Display(_INTL("{1}'s Ability changed! Its Ability is now {2}!", pkmn.name, new_ability_name));
		next true;
	}
	next false;
});

ItemHandlers.UseOnPokemon.add(:METEORITE, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.DEOXYS)) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	new_form = (pkmn.form + 1) % 4;   // Normal, Attack, Defense, Speed
	pkmn.setForm(new_form) do;
		scene.Refresh;
		scene.Display(_INTL("{1} transformed!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:GRACIDEA, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.SHAYMIN) || pkmn.form != 0 ||
		pkmn.status == statuses.FROZEN || DayNight.isNight()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	pkmn.setForm(1) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:REDNECTAR, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.ORICORIO) || pkmn.form == 0) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	pkmn.setForm(0) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed form!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:YELLOWNECTAR, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.ORICORIO) || pkmn.form == 1) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	pkmn.setForm(1) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed form!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:PINKNECTAR, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.ORICORIO) || pkmn.form == 2) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	pkmn.setForm(2) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed form!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:PURPLENECTAR, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.ORICORIO) || pkmn.form == 3) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	pkmn.setForm(3) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed form!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:REVEALGLASS, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.TORNADUS) &&
		!pkmn.isSpecies(Speciess.THUNDURUS) &&
		!pkmn.isSpecies(Speciess.LANDORUS) &&
		!pkmn.isSpecies(Speciess.ENAMORUS)) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	newForm = (pkmn.form == 0) ? 1 : 0;
	pkmn.setForm(newForm) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:PRISONBOTTLE, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.HOOPA)) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	newForm = (pkmn.form == 0) ? 1 : 0;
	pkmn.setForm(newForm) do;
		scene.Refresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	next true;
});

ItemHandlers.UseOnPokemon.add(:ROTOMCATALOG, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.ROTOM)) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	choices = new {
		_INTL("Light bulb"),
		_INTL("Microwave oven"),
		_INTL("Washing machine"),
		_INTL("Refrigerator"),
		_INTL("Electric fan"),
		_INTL("Lawn mower"),
		_INTL("Cancel");
	}
	new_form = scene.ShowCommands(_INTL("Which appliance would you like to order?"), choices, pkmn.form);
	if (new_form == pkmn.form) {
		scene.Display(_INTL("It won't have any effect."));
		next false;
	} else if (new_form >= 0 && new_form < choices.length - 1) {
		pkmn.setForm(new_form) do;
			scene.Refresh;
			scene.Display(_INTL("{1} transformed!", pkmn.name));
		}
		next true;
	}
	next false;
});

ItemHandlers.UseOnPokemon.add(:ZYGARDECUBE, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.ZYGARDE)) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	switch (scene.ShowCommands(_INTL("What will you do with {1}?", pkmn.name),) {
														new {_INTL("Change form"), _INTL("Change Ability"), _INTL("Cancel")});
	break;
	case 0:   // Change form
		newForm = (pkmn.form == 0) ? 1 : 0;
		pkmn.setForm(newForm) do;
			scene.Refresh;
			scene.Display(_INTL("{1} transformed!", pkmn.name));
		}
		next true;
	break;
	case 1:   // Change ability
		new_abil = (pkmn.ability_index + 1) % 2;
		pkmn.ability_index = new_abil;
		pkmn.ability = null;
		scene.Refresh;
		scene.Display(_INTL("{1}'s Ability changed! Its Ability is now {2}!", pkmn.name, pkmn.ability.name));
		next true;
	}
	next false;
});

ItemHandlers.UseOnPokemon.add(:DNASPLICERS, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.KYUREM) || !pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	// Fusing
	chosen = scene.ChoosePokemon(_INTL("Fuse with which Pokémon?"));
	if (chosen < 0) next false;
	other_pkmn = Game.GameData.player.party[chosen];
	if (pkmn == other_pkmn) {
		scene.Display(_INTL("It cannot be fused with itself."));
		next false;
	} else if (other_pkmn.egg()) {
		scene.Display(_INTL("It cannot be fused with an Egg."));
		next false;
	} else if (other_pkmn.fainted()) {
		scene.Display(_INTL("It cannot be fused with that fainted Pokémon."));
		next false;
	} else if (!other_pkmn.isSpecies(Speciess.RESHIRAM) && !other_pkmn.isSpecies(Speciess.ZEKROM)) {
		scene.Display(_INTL("It cannot be fused with that Pokémon."));
		next false;
	}
	newForm = 0;
	if (other_pkmn.isSpecies(Speciess.RESHIRAM)) newForm = 1;
	if (other_pkmn.isSpecies(Speciess.ZEKROM)) newForm = 2;
	pkmn.setForm(newForm) do;
		pkmn.fused = other_pkmn;
		Game.GameData.player.remove_pokemon_at_index(chosen);
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:DNASPLICERS, :DNASPLICERSUSED);
	next true;
});

ItemHandlers.UseOnPokemon.add(:DNASPLICERSUSED, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.KYUREM) || pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	} else if (Game.GameData.player.party_full()) {
		scene.Display(_INTL("You have no room to separate the Pokémon."));
		next false;
	}
	// Unfusing
	pkmn.setForm(0) do;
		Game.GameData.player.party[Game.GameData.player.party.length] = pkmn.fused;
		pkmn.fused = null;
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:DNASPLICERSUSED, :DNASPLICERS);
	next true;
});

ItemHandlers.UseOnPokemon.add(:NSOLARIZER, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.NECROZMA) || !pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	// Fusing
	chosen = scene.ChoosePokemon(_INTL("Fuse with which Pokémon?"));
	if (chosen < 0) next false;
	other_pkmn = Game.GameData.player.party[chosen];
	if (pkmn == other_pkmn) {
		scene.Display(_INTL("It cannot be fused with itself."));
		next false;
	} else if (other_pkmn.egg()) {
		scene.Display(_INTL("It cannot be fused with an Egg."));
		next false;
	} else if (other_pkmn.fainted()) {
		scene.Display(_INTL("It cannot be fused with that fainted Pokémon."));
		next false;
	} else if (!other_pkmn.isSpecies(Speciess.SOLGALEO)) {
		scene.Display(_INTL("It cannot be fused with that Pokémon."));
		next false;
	}
	pkmn.setForm(1) do;
		pkmn.fused = other_pkmn;
		Game.GameData.player.remove_pokemon_at_index(chosen);
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:NSOLARIZER, :NSOLARIZERUSED);
	next true;
});

ItemHandlers.UseOnPokemon.add(:NSOLARIZERUSED, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.NECROZMA) || pkmn.form != 1 || pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	} else if (Game.GameData.player.party_full()) {
		scene.Display(_INTL("You have no room to separate the Pokémon."));
		next false;
	}
	// Unfusing
	pkmn.setForm(0) do;
		Game.GameData.player.party[Game.GameData.player.party.length] = pkmn.fused;
		pkmn.fused = null;
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:NSOLARIZERUSED, :NSOLARIZER);
	next true;
});

ItemHandlers.UseOnPokemon.add(:NLUNARIZER, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.NECROZMA) || !pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	// Fusing
	chosen = scene.ChoosePokemon(_INTL("Fuse with which Pokémon?"));
	if (chosen < 0) next false;
	other_pkmn = Game.GameData.player.party[chosen];
	if (pkmn == other_pkmn) {
		scene.Display(_INTL("It cannot be fused with itself."));
		next false;
	} else if (other_pkmn.egg()) {
		scene.Display(_INTL("It cannot be fused with an Egg."));
		next false;
	} else if (other_pkmn.fainted()) {
		scene.Display(_INTL("It cannot be fused with that fainted Pokémon."));
		next false;
	} else if (!other_pkmn.isSpecies(Speciess.LUNALA)) {
		scene.Display(_INTL("It cannot be fused with that Pokémon."));
		next false;
	}
	pkmn.setForm(2) do;
		pkmn.fused = other_pkmn;
		Game.GameData.player.remove_pokemon_at_index(chosen);
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:NLUNARIZER, :NLUNARIZERUSED);
	next true;
});

ItemHandlers.UseOnPokemon.add(:NLUNARIZERUSED, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.NECROZMA) || pkmn.form != 2 || pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	} else if (Game.GameData.player.party_full()) {
		scene.Display(_INTL("You have no room to separate the Pokémon."));
		next false;
	}
	// Unfusing
	pkmn.setForm(0) do;
		Game.GameData.player.party[Game.GameData.player.party.length] = pkmn.fused;
		pkmn.fused = null;
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:NLUNARIZERUSED, :NLUNARIZER);
	next true;
});

ItemHandlers.UseOnPokemon.add(:REINSOFUNITY, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.CALYREX) || !pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	}
	// Fusing
	chosen = scene.ChoosePokemon(_INTL("Fuse with which Pokémon?"));
	if (chosen < 0) next false;
	other_pkmn = Game.GameData.player.party[chosen];
	if (pkmn == other_pkmn) {
		scene.Display(_INTL("It cannot be fused with itself."));
		next false;
	} else if (other_pkmn.egg()) {
		scene.Display(_INTL("It cannot be fused with an Egg."));
		next false;
	} else if (other_pkmn.fainted()) {
		scene.Display(_INTL("It cannot be fused with that fainted Pokémon."));
		next false;
	} else if (!other_pkmn.isSpecies(Speciess.GLASTRIER) &&
				!other_pkmn.isSpecies(Speciess.SPECTRIER)) {
		scene.Display(_INTL("It cannot be fused with that Pokémon."));
		next false;
	}
	newForm = 0;
	if (other_pkmn.isSpecies(Speciess.GLASTRIER)) newForm = 1;
	if (other_pkmn.isSpecies(Speciess.SPECTRIER)) newForm = 2;
	pkmn.setForm(newForm) do;
		pkmn.fused = other_pkmn;
		Game.GameData.player.remove_pokemon_at_index(chosen);
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:REINSOFUNITY, :REINSOFUNITYUSED);
	next true;
});

ItemHandlers.UseOnPokemon.add(:REINSOFUNITYUSED, block: (item, qty, pkmn, scene) => {
	if (!pkmn.isSpecies(Speciess.CALYREX) || pkmn.fused.null()) {
		scene.Display(_INTL("It had no effect."));
		next false;
	} else if (pkmn.fainted()) {
		scene.Display(_INTL("This can't be used on the fainted Pokémon."));
		next false;
	} else if (Game.GameData.player.party_full()) {
		scene.Display(_INTL("You have no room to separate the Pokémon."));
		next false;
	}
	// Unfusing
	pkmn.setForm(0) do;
		Game.GameData.player.party[Game.GameData.player.party.length] = pkmn.fused;
		pkmn.fused = null;
		scene.HardRefresh;
		scene.Display(_INTL("{1} changed Forme!", pkmn.name));
	}
	Game.GameData.bag.replace_item(:REINSOFUNITYUSED, :REINSOFUNITY);
	next true;
});
