//===============================================================================
// ItemHandlers.
//===============================================================================
public static partial class ItemHandlers {
	UseText             = new ItemHandlerHash();
	UseFromBag          = new ItemHandlerHash();
	ConfirmUseInField   = new ItemHandlerHash();
	UseInField          = new ItemHandlerHash();
	UseOnPokemon        = new ItemHandlerHash();
	UseOnPokemonMaximum = new ItemHandlerHash();
	CanUseInBattle      = new ItemHandlerHash();
	UseInBattle         = new ItemHandlerHash();
	BattleUseOnBattler  = new ItemHandlerHash();
	BattleUseOnPokemon  = new ItemHandlerHash();

	#region Class Functions
	#endregion

	public void hasUseText(item) {
		return !UseText[item].null();
	}

	// Shows "Use" option in Bag.
	public void hasOutHandler(item) {
		return !UseFromBag[item].null() || !UseInField[item].null() || !UseOnPokemon[item].null();
	}

	// Shows "Register" option in Bag.
	public void hasUseInFieldHandler(item) {
		return !UseInField[item].null();
	}

	public void hasUseOnPokemon(item) {
		return !UseOnPokemon[item].null();
	}

	public void hasUseOnPokemonMaximum(item) {
		return !UseOnPokemonMaximum[item].null();
	}

	public void hasUseInBattle(item) {
		return !UseInBattle[item].null();
	}

	public void hasBattleUseOnBattler(item) {
		return !BattleUseOnBattler[item].null();
	}

	public void hasBattleUseOnPokemon(item) {
		return !BattleUseOnPokemon[item].null();
	}

	// Returns text to display instead of "Use".
	public void getUseText(item) {
		return UseText.trigger(item);
	}

	// Return value:
	// 0 - Item not used
	// 1 - Item used, don't end screen
	// 2 - Item used, end screen
	public void triggerUseFromBag(item) {
		if (UseFromBag[item]) return UseFromBag.trigger(item);
		// No UseFromBag handler exists; check the UseInField handler if present
		if (UseInField[item]) {
			return (UseInField.trigger(item)) ? 1 : 0;
		}
		return 0;
	}

	// Returns whether item can be used.
	public void triggerConfirmUseInField(item) {
		if (!ConfirmUseInField[item]) return true;
		return ConfirmUseInField.trigger(item);
	}

	// Return value:
	// -1 - Item effect not found
	// 0  - Item not used
	// 1  - Item used
	public void triggerUseInField(item) {
		if (!UseInField[item]) return -1;
		return (UseInField.trigger(item)) ? 1 : 0;
	}

	// Returns whether item was used.
	public void triggerUseOnPokemon(item, qty, pkmn, scene) {
		if (!UseOnPokemon[item]) return false;
		return UseOnPokemon.trigger(item, qty, pkmn, scene);
	}

	// Returns the maximum number of the item that can be used on the Pokémon at once.
	public void triggerUseOnPokemonMaximum(item, pkmn) {
		if (!UseOnPokemonMaximum[item]) return 1;
		if (!Settings.USE_MULTIPLE_STAT_ITEMS_AT_ONCE) return 1;
		return (int)Math.Max(UseOnPokemonMaximum.trigger(item, pkmn), 1);
	}

	public void triggerCanUseInBattle(item, pkmn, battler, move, firstAction, battle, scene, showMessages = true) {
		if (!CanUseInBattle[item]) return true;   // Can use the item by default
		return CanUseInBattle.trigger(item, pkmn, battler, move, firstAction, battle, scene, showMessages);
	}

	public void triggerUseInBattle(item, battler, battle) {
		UseInBattle.trigger(item, battler, battle);
	}

	// Returns whether item was used.
	public void triggerBattleUseOnBattler(item, battler, scene) {
		if (!BattleUseOnBattler[item]) return false;
		return BattleUseOnBattler.trigger(item, battler, scene);
	}

	// Returns whether item was used.
	public void triggerBattleUseOnPokemon(item, pkmn, battler, choices, scene) {
		if (!BattleUseOnPokemon[item]) return false;
		return BattleUseOnPokemon.trigger(item, pkmn, battler, choices, scene);
	}
}

//===============================================================================
//
//===============================================================================
public bool CanRegisterItem(item) {
	return ItemHandlers.hasUseInFieldHandler(item);
}

public bool CanUseOnPokemon(item) {
	return ItemHandlers.hasUseOnPokemon(item) || GameData.Item.get(item).is_machine();
}

//===============================================================================
// Change a Pokémon's level.
//===============================================================================
public void ChangeLevel(pkmn, new_level, scene) {
	new_level = new_level.clamp(1, GameData.GrowthRate.max_level);
	if (pkmn.level == new_level) {
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1}'s level remained unchanged.", pkmn.name));
		} else {
			Message(_INTL("{1}'s level remained unchanged.", pkmn.name));
		}
		return;
	}
	old_level           = pkmn.level;
	old_total_hp        = pkmn.totalhp;
	old_attack          = pkmn.attack;
	old_defense         = pkmn.defense;
	old_special_attack  = pkmn.spatk;
	old_special_defense = pkmn.spdef;
	old_speed           = pkmn.speed;
	pkmn.level = new_level;
	pkmn.calc_stats;
	if (new_level > old_level && pkmn.species_data.base_stats[:HP] == 1) pkmn.hp = 1;
	scene.Refresh;
	if (old_level > new_level) {
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1} dropped to Lv. {2}!", pkmn.name, pkmn.level));
		} else {
			Message(_INTL("{1} dropped to Lv. {2}!", pkmn.name, pkmn.level));
		}
		total_hp_diff        = pkmn.totalhp - old_total_hp;
		attack_diff          = pkmn.attack - old_attack;
		defense_diff         = pkmn.defense - old_defense;
		special_attack_diff  = pkmn.spatk - old_special_attack;
		special_defense_diff = pkmn.spdef - old_special_defense;
		speed_diff           = pkmn.speed - old_speed;
		TopRightWindow(_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
													total_hp_diff, attack_diff, defense_diff, special_attack_diff, special_defense_diff, speed_diff), scene);
		TopRightWindow(_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
													pkmn.totalhp, pkmn.attack, pkmn.defense, pkmn.spatk, pkmn.spdef, pkmn.speed), scene);
	} else {
		pkmn.changeHappiness("vitamin");
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1} grew to Lv. {2}!", pkmn.name, pkmn.level));
		} else {
			Message(_INTL("{1} grew to Lv. {2}!", pkmn.name, pkmn.level));
		}
		total_hp_diff        = pkmn.totalhp - old_total_hp;
		attack_diff          = pkmn.attack - old_attack;
		defense_diff         = pkmn.defense - old_defense;
		special_attack_diff  = pkmn.spatk - old_special_attack;
		special_defense_diff = pkmn.spdef - old_special_defense;
		speed_diff           = pkmn.speed - old_speed;
		TopRightWindow(_INTL("Max. HP<r>+{1}\nAttack<r>+{2}\nDefense<r>+{3}\nSp. Atk<r>+{4}\nSp. Def<r>+{5}\nSpeed<r>+{6}",
													total_hp_diff, attack_diff, defense_diff, special_attack_diff, special_defense_diff, speed_diff), scene);
		TopRightWindow(_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
													pkmn.totalhp, pkmn.attack, pkmn.defense, pkmn.spatk, pkmn.spdef, pkmn.speed), scene);
		// Learn new moves upon level up
		movelist = pkmn.getMoveList;
		foreach (var i in movelist) { //'movelist.each' do => |i|
			if (i[0] <= old_level || i[0] > pkmn.level) continue;
			LearnMove(pkmn, i[1], true) { scene.Update };
		}
		// Check for evolution
		new_species = pkmn.check_evolution_on_level_up;
		if (new_species) {
			FadeOutInWithMusic do;
				evo = new PokemonEvolutionScene();
				evo.StartScreen(pkmn, new_species);
				evo.Evolution;
				evo.EndScreen;
				if (scene.is_a(PokemonPartyScreen)) scene.Refresh;
			}
		}
	}
}

public void TopRightWindow(text, scene = null) {
	window = new Window_AdvancedTextPokemon(text);
	window.width = 198;
	window.x     = Graphics.width - window.width;
	window.y     = 0;
	window.z     = 99999;
	PlayDecisionSE;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		window.update;
		scene&.Update;
		if (Input.trigger(Input.USE)) break;
	}
	window.dispose;
}

public void ChangeExp(pkmn, new_exp, scene) {
	new_exp = new_exp.clamp(0, pkmn.growth_rate.maximum_exp);
	if (pkmn.exp == new_exp) {
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1}'s Exp. Points remained unchanged.", pkmn.name));
		} else {
			Message(_INTL("{1}'s Exp. Points remained unchanged.", pkmn.name));
		}
		return;
	}
	old_level           = pkmn.level;
	old_total_hp        = pkmn.totalhp;
	old_attack          = pkmn.attack;
	old_defense         = pkmn.defense;
	old_special_attack  = pkmn.spatk;
	old_special_defense = pkmn.spdef;
	old_speed           = pkmn.speed;
	if (pkmn.exp > new_exp) {   // Loses Exp
		difference = pkmn.exp - new_exp;
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1} lost {2} Exp. Points!", pkmn.name, difference));
		} else {
			Message(_INTL("{1} lost {2} Exp. Points!", pkmn.name, difference));
		}
		pkmn.exp = new_exp;
		pkmn.calc_stats;
		scene.Refresh;
		if (pkmn.level == old_level) return;
		// Level changed
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1} dropped to Lv. {2}!", pkmn.name, pkmn.level));
		} else {
			Message(_INTL("{1} dropped to Lv. {2}!", pkmn.name, pkmn.level));
		}
		total_hp_diff        = pkmn.totalhp - old_total_hp;
		attack_diff          = pkmn.attack - old_attack;
		defense_diff         = pkmn.defense - old_defense;
		special_attack_diff  = pkmn.spatk - old_special_attack;
		special_defense_diff = pkmn.spdef - old_special_defense;
		speed_diff           = pkmn.speed - old_speed;
		TopRightWindow(_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
													total_hp_diff, attack_diff, defense_diff, special_attack_diff, special_defense_diff, speed_diff), scene);
		TopRightWindow(_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
													pkmn.totalhp, pkmn.attack, pkmn.defense, pkmn.spatk, pkmn.spdef, pkmn.speed), scene);
	} else {   // Gains Exp
		difference = new_exp - pkmn.exp;
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1} gained {2} Exp. Points!", pkmn.name, difference));
		} else {
			Message(_INTL("{1} gained {2} Exp. Points!", pkmn.name, difference));
		}
		pkmn.exp = new_exp;
		pkmn.changeHappiness("vitamin");
		pkmn.calc_stats;
		scene.Refresh;
		if (pkmn.level == old_level) return;
		// Level changed
		if (scene.is_a(PokemonPartyScreen)) {
			scene.Display(_INTL("{1} grew to Lv. {2}!", pkmn.name, pkmn.level));
		} else {
			Message(_INTL("{1} grew to Lv. {2}!", pkmn.name, pkmn.level));
		}
		total_hp_diff        = pkmn.totalhp - old_total_hp;
		attack_diff          = pkmn.attack - old_attack;
		defense_diff         = pkmn.defense - old_defense;
		special_attack_diff  = pkmn.spatk - old_special_attack;
		special_defense_diff = pkmn.spdef - old_special_defense;
		speed_diff           = pkmn.speed - old_speed;
		TopRightWindow(_INTL("Max. HP<r>+{1}\nAttack<r>+{2}\nDefense<r>+{3}\nSp. Atk<r>+{4}\nSp. Def<r>+{5}\nSpeed<r>+{6}",
													total_hp_diff, attack_diff, defense_diff, special_attack_diff, special_defense_diff, speed_diff), scene);
		TopRightWindow(_INTL("Max. HP<r>{1}\nAttack<r>{2}\nDefense<r>{3}\nSp. Atk<r>{4}\nSp. Def<r>{5}\nSpeed<r>{6}",
													pkmn.totalhp, pkmn.attack, pkmn.defense, pkmn.spatk, pkmn.spdef, pkmn.speed), scene);
		// Learn new moves upon level up
		movelist = pkmn.getMoveList;
		foreach (var i in movelist) { //'movelist.each' do => |i|
			if (i[0] <= old_level || i[0] > pkmn.level) continue;
			LearnMove(pkmn, i[1], true) { scene.Update };
		}
		// Check for evolution
		new_species = pkmn.check_evolution_on_level_up;
		if (new_species) {
			FadeOutInWithMusic do;
				evo = new PokemonEvolutionScene();
				evo.StartScreen(pkmn, new_species);
				evo.Evolution;
				evo.EndScreen;
				if (scene.is_a(PokemonPartyScreen)) scene.Refresh;
			}
		}
	}
}

public void GainExpFromExpCandy(pkmn, base_amt, qty, scene) {
	if (pkmn.level >= GameData.GrowthRate.max_level || pkmn.shadowPokemon()) {
		scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	SEPlay("Pkmn level up");
	if (scene.is_a(PokemonPartyScreen)) scene.scene.SetHelpText("");
	if (qty > 1) {
		(qty - 1).times(() => pkmn.changeHappiness("vitamin"))
	}
	ChangeExp(pkmn, pkmn.exp + (base_amt * qty), scene);
	scene.HardRefresh;
	return true;
}

//===============================================================================
// Restore HP.
//===============================================================================
public void ItemRestoreHP(pkmn, restoreHP) {
	newHP = pkmn.hp + restoreHP;
	if (newHP > pkmn.totalhp) newHP = pkmn.totalhp;
	hpGain = newHP - pkmn.hp;
	pkmn.hp = newHP;
	return hpGain;
}

public void HPItem(pkmn, restoreHP, scene) {
	if (!pkmn.able() || pkmn.hp == pkmn.totalhp) {
		scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	SEPlay("Use item in party");
	hpGain = ItemRestoreHP(pkmn, restoreHP);
	scene.Refresh;
	scene.Display(_INTL("{1}'s HP was restored by {2} points.", pkmn.name, hpGain));
	return true;
}

public void BattleHPItem(pkmn, battler, restoreHP, scene) {
	if (battler) {
		if (battler.RecoverHP(restoreHP) > 0) {
			scene.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		}
	} else if (ItemRestoreHP(pkmn, restoreHP) > 0) {
		scene.Display(_INTL("{1}'s HP was restored.", pkmn.name));
	}
	return true;
}

//===============================================================================
// Restore PP.
//===============================================================================
public void RestorePP(pkmn, idxMove, pp) {
	if (!pkmn.moves[idxMove] || !pkmn.moves[idxMove].id) return 0;
	if (pkmn.moves[idxMove].total_pp <= 0) return 0;
	oldpp = pkmn.moves[idxMove].pp;
	newpp = pkmn.moves[idxMove].pp + pp;
	if (newpp > pkmn.moves[idxMove].total_pp) newpp = pkmn.moves[idxMove].total_pp;
	pkmn.moves[idxMove].pp = newpp;
	return newpp - oldpp;
}

public void BattleRestorePP(pkmn, battler, idxMove, pp) {
	if (RestorePP(pkmn, idxMove, pp) == 0) return;
	if (battler && !battler.effects.Transform &&
		battler.moves[idxMove] && battler.moves[idxMove].id == pkmn.moves[idxMove].id) {
		battler.SetPP(battler.moves[idxMove], pkmn.moves[idxMove].pp);
	}
}

//===============================================================================
// Change EVs.
//===============================================================================
public void JustRaiseEffortValues(pkmn, stat, evGain) {
	stat = GameData.Stat.get(stat).id;
	evTotal = 0;
	GameData.Stat.each_main(s => evTotal += pkmn.ev[s.id]);
	evGain = evGain.clamp(0, Pokemon.EV_STAT_LIMIT - pkmn.ev[stat]);
	evGain = evGain.clamp(0, Pokemon.EV_LIMIT - evTotal);
	if (evGain > 0) {
		pkmn.ev[stat] += evGain;
		pkmn.calc_stats;
	}
	return evGain;
}

public void RaiseEffortValues(pkmn, stat, evGain = 10, no_ev_cap = false) {
	stat = GameData.Stat.get(stat).id;
	if (!no_ev_cap && pkmn.ev[stat] >= 100) return 0;
	evTotal = 0;
	GameData.Stat.each_main(s => evTotal += pkmn.ev[s.id]);
	evGain = evGain.clamp(0, Pokemon.EV_STAT_LIMIT - pkmn.ev[stat]);
	if (!no_ev_cap) evGain = evGain.clamp(0, 100 - pkmn.ev[stat]);
	evGain = evGain.clamp(0, Pokemon.EV_LIMIT - evTotal);
	if (evGain > 0) {
		pkmn.ev[stat] += evGain;
		pkmn.calc_stats;
	}
	return evGain;
}

public void MaxUsesOfEVRaisingItem(stat, amt_per_use, pkmn, no_ev_cap = false) {
	max_per_stat = (no_ev_cap) ? Pokemon.EV_STAT_LIMIT : 100;
	amt_can_gain = max_per_stat - pkmn.ev[stat];
	ev_total = 0;
	GameData.Stat.each_main(s => ev_total += pkmn.ev[s.id]);
	amt_can_gain = (int)Math.Min(amt_can_gain, Pokemon.EV_LIMIT - ev_total);
	return (int)Math.Max((amt_can_gain.to_f / amt_per_use).ceil, 1);
}

public void UseEVRaisingItem(stat, amt_per_use, qty, pkmn, happiness_type, scene, no_ev_cap = false) {
	ret = true;
	for (int i = qty; i < qty; i++) { //for 'qty' times do => |i|
		if (RaiseEffortValues(pkmn, stat, amt_per_use, no_ev_cap) > 0) {
			pkmn.changeHappiness(happiness_type);
		} else {
			if (i == 0) ret = false;
			break;
		}
	}
	if (!ret) {
		scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	SEPlay("Use item in party");
	scene.Refresh;
	scene.Display(_INTL("{1}'s {2} increased.", pkmn.name, GameData.Stat.get(stat).name));
	return true;
}

public void MaxUsesOfEVLoweringBerry(stat, pkmn) {
	ret = (pkmn.ev[stat].to_f / 10).ceil;
	happiness = pkmn.happiness;
	uses = 0;
	if (happiness < 255) {
		bonus_per_use = 0;
		if (pkmn.obtain_map == Game.GameData.game_map.map_id) bonus_per_use += 1;
		if (pkmn.poke_ball == Items.LUXURY_BALL) bonus_per_use += 1;
		has_soothe_bell = pkmn.hasItem(Items.SOOTHEBELL);
		do { //loop; while (true);
			uses += 1;
			gain = new {10, 5, 2}[happiness / 100];
			gain += bonus_per_use;
			if (has_soothe_bell) gain = (int)Math.Floor(gain * 1.5);
			happiness += gain;
			if (happiness >= 255) break;
		}
	}
	return (int)Math.Max(ret, uses);
}

public void RaiseHappinessAndLowerEV(pkmn, scene, stat, qty, messages) {
	h = pkmn.happiness < 255;
	e = pkmn.ev[stat] > 0;
	if (!h && !e) {
		scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	if h
		qty.times(i => pkmn.changeHappiness("evberry"));
	}
	if e
		pkmn.ev[stat] -= 10 * qty;
		if (pkmn.ev[stat] < 0) pkmn.ev[stat] = 0;
		pkmn.calc_stats;
	}
	scene.Refresh;
	scene.Display(messages[2 - (h ? 0 : 2) - (e ? 0 : 1)]);
	return true;
}

//===============================================================================
// Change nature.
//===============================================================================
public void NatureChangingMint(new_nature, item, pkmn, scene) {
	if (pkmn.nature_for_stats == new_nature) {
		scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	if (!scene.Confirm(_INTL("It might affect {1}'s stats. Are you sure you want to use it?", pkmn.name))) {
		return false;
	}
	pkmn.nature_for_stats = new_nature;
	pkmn.calc_stats;
	scene.Refresh;
	scene.Display(_INTL("{1}'s stats may have changed due to the effects of the {2}!",
												pkmn.name, GameData.Item.get(item).name));
	return true;
}

//===============================================================================
// Battle items.
//===============================================================================
public bool BattleItemCanCureStatus(status, pkmn, scene, showMessages) {
	if (!pkmn.able() || pkmn.status != status) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	return true;
}

public bool BattleItemCanRaiseStat(stat, battler, scene, showMessages) {
	if (!battler || !battler.CanRaiseStatStage(stat, battler)) {
		if (showMessages) scene.Display(_INTL("It won't have any effect."));
		return false;
	}
	return true;
}

//===============================================================================
// Decide whether the player is able to ride/dismount their Bicycle.
//===============================================================================
public void BikeCheck() {
	if (Game.GameData.PokemonGlobal.surfing || Game.GameData.PokemonGlobal.diving ||
		(!Game.GameData.PokemonGlobal.bicycle &&
		(Game.GameData.game_player.TerrainTag.must_walk || Game.GameData.game_player.TerrainTag.must_walk_or_run))) {
		Message(_INTL("Can't use that here."));
		return false;
	}
	if (!Game.GameData.game_player.can_ride_vehicle_with_follower()) {
		Message(_INTL("It can't be used when you have someone with you."));
		return false;
	}
	map_metadata = Game.GameData.game_map.metadata;
	if (Game.GameData.PokemonGlobal.bicycle) {
		if (map_metadata&.always_bicycle) {
			Message(_INTL("You can't dismount your Bike here."));
			return false;
		}
		return true;
	}
	if (!map_metadata || (!map_metadata.can_bicycle && !map_metadata.outdoor_map)) {
		Message(_INTL("Can't use that here."));
		return false;
	}
	return true;
}

//===============================================================================
// Find the closest hidden item (for Itemfinder).
//===============================================================================
public void ClosestHiddenItem() {
	result = new List<string>();
	playerX = Game.GameData.game_player.x;
	playerY = Game.GameData.game_player.y;
	foreach (var event in Game.GameData.game_map.events) { //Game.GameData.game_map.events.each_value do => |event|
		if (!System.Text.RegularExpressions.Regex.IsMatch(event.name,@"hiddenitem",RegexOptions.IgnoreCase)) continue;
		if ((playerX - event.x).abs >= 8) continue;
		if ((playerY - event.y).abs >= 6) continue;
		if (Game.GameData.game_self_switches[new {Game.GameData.game_map.map_id, event.id, "A"}]) continue;
		result.Add(event);
	}
	if (result.length == 0) return null;
	ret = null;
	retmin = 0;
	foreach (var event in result) { //'result.each' do => |event|
		dist = (playerX - event.x).abs + (playerY - event.y).abs;
		if (ret && retmin <= dist) continue;
		ret = event;
		retmin = dist;
	}
	return ret;
}

//===============================================================================
// Teach and forget a move.
//===============================================================================
public void LearnMove(pkmn, move, ignore_if_known = false, by_machine = false, &block) {
	if (!pkmn) return false;
	move = GameData.Move.get(move).id;
	if (pkmn.egg() && !Core.DEBUG) {
		Message(_INTL("Eggs can't be taught any moves."), &block);
		return false;
	} else if (pkmn.shadowPokemon()) {
		Message(_INTL("Shadow Pokémon can't be taught any moves."), &block);
		return false;
	}
	pkmn_name = pkmn.name;
	move_name = GameData.Move.get(move).name;
	if (pkmn.hasMove(move)) {
		if (!ignore_if_known) Message(_INTL("{1} already knows {2}.", pkmn_name, move_name), &block);
		return false;
	} else if (pkmn.numMoves < Pokemon.MAX_MOVES) {
		pkmn.learn_move(move);
		Message("\\se[]" + _INTL("{1} learned {2}!", pkmn_name, move_name) + "\\se[Pkmn move learnt]", &block);
		return true;
	}
	Message(_INTL("{1} wants to learn {2}, but it already knows {3} moves.",
									pkmn_name, move_name, pkmn.numMoves.to_word) + "\1", &block);
	if (ConfirmMessage(_INTL("Should {1} forget a move to learn {2}?", pkmn_name, move_name), &block)) {
		do { //loop; while (true);
			move_index = ForgetMove(pkmn, move);
			if (move_index >= 0) {
				old_move_name = pkmn.moves[move_index].name;
				oldmovepp = pkmn.moves[move_index].pp;
				pkmn.moves[move_index] = new Pokemon.Move(move);   // Replaces current/total PP
				if (by_machine && Settings.TAUGHT_MACHINES_KEEP_OLD_PP) {
					pkmn.moves[move_index].pp = (int)Math.Min(oldmovepp, pkmn.moves[move_index].total_pp);
				}
				Message(_INTL("1, 2, and...\\wt[16] ...\\wt[16] ...\\wt[16] Ta-da!") + "\\se[Battle ball drop]\1", &block);
				Message(_INTL("{1} forgot how to use {2}.\nAnd..." + "\1", pkmn_name, old_move_name), &block);
				Message("\\se[]" + _INTL("{1} learned {2}!", pkmn_name, move_name) + "\\se[Pkmn move learnt]", &block);
				if (by_machine) pkmn.changeHappiness("machine");
				return true;
			} else if (ConfirmMessage(_INTL("Give up on learning {1}?", move_name), &block)) {
				Message(_INTL("{1} did not learn {2}.", pkmn_name, move_name), &block);
				return false;
			}
		}
	} else {
		Message(_INTL("{1} did not learn {2}.", pkmn_name, move_name), &block);
	}
	return false;
}

public void ForgetMove(pkmn, moveToLearn) {
	ret = -1;
	FadeOutIn do;
		scene = new PokemonSummary_Scene();
		screen = new PokemonSummaryScreen(scene);
		ret = screen.StartForgetScreen([pkmn], 0, moveToLearn);
	}
	return ret;
}

//===============================================================================
// Use an item from the Bag and/or on a Pokémon.
//===============================================================================
// @return [Integer] 0 = item wasn't used; 1 = item used; 2 = close Bag to use in field
public void UseItem(bag, item, bagscene = null) {
	itm = GameData.Item.get(item);
	useType = itm.field_use;
	if (useType == 1) {   // Item is usable on a Pokémon
		if (Game.GameData.player.pokemon_count == 0) {
			Message(_INTL("There is no Pokémon."));
			return 0;
		}
		ret = false;
		annot = null;
		if (itm.is_evolution_stone()) {
			annot = new List<string>();
			foreach (var pkmn in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |pkmn|
				elig = pkmn.check_evolution_on_use_item(item);
				annot.Add((elig) ? _INTL("ABLE") : _INTL("NOT ABLE"));
			}
		}
		FadeOutIn do;
			scene = new PokemonParty_Scene();
			screen = new PokemonPartyScreen(scene, Game.GameData.player.party);
			screen.StartScene(_INTL("Use on which Pokémon?"), false, annot);
			do { //loop; while (true);
				scene.SetHelpText(_INTL("Use on which Pokémon?"));
				chosen = screen.ChoosePokemon;
				if (chosen < 0) {
					ret = false;
					break;
				}
				pkmn = Game.GameData.player.party[chosen];
				if (!CheckUseOnPokemon(item, pkmn, screen)) continue;
				qty = 1;
				max_at_once = ItemHandlers.triggerUseOnPokemonMaximum(item, pkmn);
				max_at_once = (int)Math.Min(max_at_once, Game.GameData.bag.quantity(item));
				if (max_at_once > 1) {
					qty = screen.scene.ChooseNumber(
						_INTL("How many {1} do you want to use?", GameData.Item.get(item).portion_name_plural), max_at_once
					);
					if (screen.is_a(PokemonPartyScreen)) screen.scene.SetHelpText("");
				}
				if (qty <= 0) continue;
				ret = ItemHandlers.triggerUseOnPokemon(item, qty, pkmn, screen);
				unless (ret && itm.consumed_after_use()) continue;
				bag.remove(item, qty);
				if (bag.has(item)) continue;
				Message(_INTL("You used your last {1}.", itm.portion_name)) { screen.Update };
				break;
			}
			screen.EndScene;
			bagscene&.Refresh;
		}
		return (ret) ? 1 : 0;
	} else if (useType == 2 || itm.is_machine()) {   // Item is usable from Bag or teaches a move
		intret = ItemHandlers.triggerUseFromBag(item);
		if (intret >= 0) {
			if (intret == 1 && itm.consumed_after_use()) bag.remove(item);
			return intret;
		}
		Message(_INTL("Can't use that here."));
		return 0;
	}
	Message(_INTL("Can't use that here."));
	return 0;
}

// Only called when in the party screen and having chosen an item to be used on
// the selected Pokémon.
public void UseItemOnPokemon(item, pkmn, scene) {
	itm = GameData.Item.get(item);
	// TM or HM
	if (itm.is_machine()) {
		machine = itm.move;
		if (!machine) return false;
		movename = GameData.Move.get(machine).name;
		if (pkmn.shadowPokemon()) {
			Message(_INTL("Shadow Pokémon can't be taught any moves.")) { scene.Update };
		} else if (!pkmn.compatible_with_move(machine)) {
			Message(_INTL("{1} can't learn {2}.", pkmn.name, movename)) { scene.Update };
		} else {
			Message("\\se[PC access]" + _INTL("You booted up the {1}.", itm.portion_name) + "\1") { scene.Update };
			if (ConfirmMessage(_INTL("Do you want to teach {1} to {2}?", movename, pkmn.name)) { scene.Update }) {
				if (LearnMove(pkmn, machine, false, true) { scene.Update }) {
					if (itm.consumed_after_use()) Game.GameData.bag.remove(item);
					return true;
				}
			}
		}
		return false;
	}
	// Other item
	qty = 1;
	max_at_once = ItemHandlers.triggerUseOnPokemonMaximum(item, pkmn);
	max_at_once = (int)Math.Min(max_at_once, Game.GameData.bag.quantity(item));
	if (max_at_once > 1) {
		qty = scene.scene.ChooseNumber(
			_INTL("How many {1} do you want to use?", itm.portion_name_plural), max_at_once
		);
		if (scene.is_a(PokemonPartyScreen)) scene.scene.SetHelpText("");
	}
	if (qty <= 0) return false;
	ret = ItemHandlers.triggerUseOnPokemon(item, qty, pkmn, scene);
	scene.ClearAnnotations;
	scene.HardRefresh;
	if (ret && itm.consumed_after_use()) {
		Game.GameData.bag.remove(item, qty);
		if (!Game.GameData.bag.has(item)) {
			Message(_INTL("You used your last {1}.", itm.portion_name)) { scene.Update };
		}
	}
	return ret;
}

public void UseKeyItemInField(item) {
	ret = ItemHandlers.triggerUseInField(item);
	if (ret == -1) {   // Item effect not found
		Message(_INTL("Can't use that here."));
	} else if (ret > 0 && GameData.Item.get(item).consumed_after_use()) {
		Game.GameData.bag.remove(item);
	}
	return ret > 0;
}

public void UseItemMessage(item) {
	itemname = GameData.Item.get(item).portion_name;
	if (itemname.starts_with_vowel()) {
		Message(_INTL("You used an {1}.", itemname));
	} else {
		Message(_INTL("You used a {1}.", itemname));
	}
}

public void CheckUseOnPokemon(item, pkmn, _screen) {
	return pkmn && !pkmn.egg() && (!pkmn.hyper_mode || GameData.Item.get(item)&.is_scent());
}

//===============================================================================
// Give an item to a Pokémon to hold, and take a held item from a Pokémon.
//===============================================================================
public void GiveItemToPokemon(item, pkmn, scene, pkmnid = 0) {
	newitemname = GameData.Item.get(item).portion_name;
	if (pkmn.egg()) {
		scene.Display(_INTL("Eggs can't hold items."));
		return false;
	} else if (pkmn.mail) {
		scene.Display(_INTL("{1}'s mail must be removed before giving it an item.", pkmn.name));
		if (!TakeItemFromPokemon(pkmn, scene)) return false;
	}
	if (pkmn.hasItem()) {
		olditemname = pkmn.item.portion_name;
		if (newitemname.starts_with_vowel()) {
			scene.Display(_INTL("{1} is already holding an {2}.", pkmn.name, olditemname) + "\1");
		} else {
			scene.Display(_INTL("{1} is already holding a {2}.", pkmn.name, olditemname) + "\1");
		}
		if (scene.Confirm(_INTL("Would you like to switch the two items?"))) {
			Game.GameData.bag.remove(item);
			if (!Game.GameData.bag.add(pkmn.item)) {
				if (!Game.GameData.bag.add(item)) raise _INTL("Couldn't re-store deleted item in Bag somehow");
				scene.Display(_INTL("The Bag is full. The Pokémon's item could not be removed."));
			} else if (GameData.Item.get(item).is_mail()) {
				if (WriteMail(item, pkmn, pkmnid, scene)) {
					pkmn.item = item;
					scene.Display(_INTL("Took the {1} from {2} and gave it the {3}.", olditemname, pkmn.name, newitemname));
					return true;
				} else if (!Game.GameData.bag.add(item)) {
					Debug.LogError(_INTL("Couldn't re-store deleted item in Bag somehow"));
					//throw new ArgumentException(_INTL("Couldn't re-store deleted item in Bag somehow"));
				}
			} else {
				pkmn.item = item;
				scene.Display(_INTL("Took the {1} from {2} and gave it the {3}.", olditemname, pkmn.name, newitemname));
				return true;
			}
		}
	} else if (!GameData.Item.get(item).is_mail() || WriteMail(item, pkmn, pkmnid, scene)) {
		Game.GameData.bag.remove(item);
		pkmn.item = item;
		scene.Display(_INTL("{1} is now holding the {2}.", pkmn.name, newitemname));
		return true;
	}
	return false;
}

public void TakeItemFromPokemon(pkmn, scene) {
	ret = false;
	if (!pkmn.hasItem()) {
		scene.Display(_INTL("{1} isn't holding anything.", pkmn.name));
	} else if (!Game.GameData.bag.can_add(pkmn.item)) {
		scene.Display(_INTL("The Bag is full. The Pokémon's item could not be removed."));
	} else if (pkmn.mail) {
		if (scene.Confirm(_INTL("Save the removed mail in your PC?"))) {
			if (MoveToMailbox(pkmn)) {
				scene.Display(_INTL("The mail was saved in your PC."));
				pkmn.item = null;
				ret = true;
			} else {
				scene.Display(_INTL("Your PC's Mailbox is full."));
			}
		} else if (scene.Confirm(_INTL("If the mail is removed, its message will be lost. OK?"))) {
			Game.GameData.bag.add(pkmn.item);
			scene.Display(_INTL("Received the {1} from {2}.", pkmn.item.portion_name, pkmn.name));
			pkmn.item = null;
			pkmn.mail = null;
			ret = true;
		}
	} else {
		Game.GameData.bag.add(pkmn.item);
		scene.Display(_INTL("Received the {1} from {2}.", pkmn.item.portion_name, pkmn.name));
		pkmn.item = null;
		ret = true;
	}
	return ret;
}

//===============================================================================
// Choose an item from the Bag.
//===============================================================================
public void ChooseItem(var = 0, *args) {
	ret = null;
	FadeOutIn do;
		scene = new PokemonBag_Scene();
		screen = new PokemonBagScreen(scene, Game.GameData.bag);
		ret = screen.ChooseItemScreen;
	}
	if (var > 0) Game.GameData.game_variables[var] = ret || :NONE;
	return ret;
}

public void ChooseApricorn(var = 0) {
	ret = null;
	FadeOutIn do;
		scene = new PokemonBag_Scene();
		screen = new PokemonBagScreen(scene, Game.GameData.bag);
		ret = screen.ChooseItemScreen(block: (item) => { GameData.Item.get(item).is_apricorn() });
	}
	if (var > 0) Game.GameData.game_variables[var] = ret || :NONE;
	return ret;
}

public void ChooseFossil(var = 0) {
	ret = null;
	FadeOutIn do;
		scene = new PokemonBag_Scene();
		screen = new PokemonBagScreen(scene, Game.GameData.bag);
		ret = screen.ChooseItemScreen(block: (item) => { GameData.Item.get(item).is_fossil() });
	}
	if (var > 0) Game.GameData.game_variables[var] = ret || :NONE;
	return ret;
}

// Shows a list of items to choose from, with the chosen item's ID being stored
// in the given Game Variable. Only items which the player has are listed.
public void ChooseItemFromList(message, variable, *args) {
	commands = new List<string>();
	itemid   = new List<string>();
	foreach (var item in args) { //'args.each' do => |item|
		if (!GameData.Item.exists(item)) continue;
		itm = GameData.Item.get(item);
		if (!Game.GameData.bag.has(itm)) continue;
		commands.Add(itm.name);
		itemid.Add(itm.id);
	}
	if (commands.length == 0) {
		Game.GameData.game_variables[variable] = :NONE;
		return null;
	}
	commands.Add(_INTL("Cancel"));
	itemid.Add(null);
	ret = Message(message, commands, -1);
	if (ret < 0 || ret >= commands.length - 1) {
		Game.GameData.game_variables[variable] = :NONE;
		return null;
	}
	Game.GameData.game_variables[variable] = itemid[ret] || :NONE;
	return itemid[ret];
}
