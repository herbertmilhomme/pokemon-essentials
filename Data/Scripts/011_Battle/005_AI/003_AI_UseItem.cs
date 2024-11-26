//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	HP_HEAL_ITEMS = {
		POTION       = 20,
		SUPERPOTION  = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 60 : 50,
		HYPERPOTION  = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 120 : 200,
		MAXPOTION    = 999,
		BERRYJUICE   = 20,
		SWEETHEART   = 20,
		FRESHWATER   = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 30 : 50,
		SODAPOP      = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 50 : 60,
		LEMONADE     = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 70 : 80,
		MOOMOOMILK   = 100,
		ORANBERRY    = 10,
		SITRUSBERRY  = 1,   // Actual amount is determined below (pkmn.totalhp / 4)
		ENERGYPOWDER = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 60 : 50,
		ENERGYROOT   = (Settings.REBALANCED_HEALING_ITEM_AMOUNTS) ? 120 : 200;
	}
	if (!Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS) HP_HEAL_ITEMS[:RAGECANDYBAR] = 20;
	FULL_RESTORE_ITEMS = [
		:FULLRESTORE;
	];
	ONE_STATUS_CURE_ITEMS = new {   // Preferred over items that heal all status problems
		:AWAKENING, :CHESTOBERRY, :BLUEFLUTE,
		:ANTIDOTE, :PECHABERRY,
		:BURNHEAL, :RAWSTBERRY,
		:PARALYZEHEAL, :PARLYZHEAL, :CHERIBERRY,
		:ICEHEAL, :ASPEARBERRY;
	}
	ALL_STATUS_CURE_ITEMS = new {
		:FULLHEAL, :LAVACOOKIE, :OLDGATEAU, :CASTELIACONE, :LUMIOSEGALETTE,
		:SHALOURSABLE, :BIGMALASADA, :PEWTERCRUNCHIES, :LUMBERRY, :HEALPOWDER;
	}
	if (Settings.RAGE_CANDY_BAR_CURES_STATUS_PROBLEMS) ALL_STATUS_CURE_ITEMS.Add(:RAGECANDYBAR);
	ONE_STAT_RAISE_ITEMS = {
		XATTACK    = new {:ATTACK, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XATTACK2   = new {:ATTACK, 2},
		XATTACK3   = new {:ATTACK, 3},
		XATTACK6   = new {:ATTACK, 6},
		XDEFENSE   = new {:DEFENSE, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XDEFENSE2  = new {:DEFENSE, 2},
		XDEFENSE3  = new {:DEFENSE, 3},
		XDEFENSE6  = new {:DEFENSE, 6},
		XDEFEND    = new {:DEFENSE, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XDEFEND2   = new {:DEFENSE, 2},
		XDEFEND3   = new {:DEFENSE, 3},
		XDEFEND6   = new {:DEFENSE, 6},
		XSPATK     = new {:SPECIAL_ATTACK, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XSPATK2    = new {:SPECIAL_ATTACK, 2},
		XSPATK3    = new {:SPECIAL_ATTACK, 3},
		XSPATK6    = new {:SPECIAL_ATTACK, 6},
		XSPECIAL   = new {:SPECIAL_ATTACK, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XSPECIAL2  = new {:SPECIAL_ATTACK, 2},
		XSPECIAL3  = new {:SPECIAL_ATTACK, 3},
		XSPECIAL6  = new {:SPECIAL_ATTACK, 6},
		XSPDEF     = new {:SPECIAL_DEFENSE, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XSPDEF2    = new {:SPECIAL_DEFENSE, 2},
		XSPDEF3    = new {:SPECIAL_DEFENSE, 3},
		XSPDEF6    = new {:SPECIAL_DEFENSE, 6},
		XSPEED     = new {:SPEED, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XSPEED2    = new {:SPEED, 2},
		XSPEED3    = new {:SPEED, 3},
		XSPEED6    = new {:SPEED, 6},
		XACCURACY  = new {:ACCURACY, (Settings.X_STAT_ITEMS_RAISE_BY_TWO_STAGES) ? 2 : 1},
		XACCURACY2 = new {:ACCURACY, 2},
		XACCURACY3 = new {:ACCURACY, 3},
		XACCURACY6 = new {:ACCURACY, 6}
	}
	ALL_STATS_RAISE_ITEMS = [
		:MAXMUSHROOMS;
	];
	REVIVE_ITEMS = {
		REVIVE      = 5,
		MAXREVIVE   = 7,
		REVIVALHERB = 7,
		MAXHONEY    = 7;
	}

	//-----------------------------------------------------------------------------

	// Decide whether the opponent should use an item on the Pokémon.
	public void ChooseToUseItem() {
		item = null;
		idxTarget = null;   // Party index (battle_use type 1/2/3) or battler index
		idxMove = null;
		item, idxTarget, idxMove = choose_item_to_use;
		if (!item) return false;
		// Register use of item
		@battle.RegisterItem(@user.index, item, idxTarget, idxMove);
		Debug.log_ai($"{@user.name} will use item {GameData.Item.get(item).name}");
		return true;
	}

	// Return values are:
	//   item ID
	//   target index (party index for items with a battle use of 1/2/3, battler
	//     index otherwise)
	//   move index (for items usable on moves only)
	public void choose_item_to_use() {
		if (!@battle.internalBattle) return null;
		items = @battle.GetOwnerItems(@user.index);
		if (!items || items.length == 0) return null;
		// Find all items usable on the Pokémon choosing this action
		pkmn = @user.battler.pokemon;
		usable_items = new List<string>();
		foreach (var item in items) { //'items.each' do => |item|
			usage = get_usability_of_item_on_pkmn(item, @user.party_index, @user.side);
			usage.each_pair do |key, vals|
				usable_items[key] ||= new List<string>();
				usable_items[key] += vals;
			}
		}
		// Prioritise using a HP restoration item
		if (usable_items.hp_heal && (pkmn.hp <= pkmn.totalhp / 4 ||
			(pkmn.hp <= pkmn.totalhp / 2 && AIRandom(100) < 30))) {
			usable_items.hp_heal.sort! { |a, b| (a[2] == b[2]) ? a[3] <=> b[3] : a[2] <=> b[2] };
			foreach (var item in usable_items.hp_heal) { //'usable_items.hp_heal.each' do => |item|
				if (item[3] >= (pkmn.totalhp - pkmn.hp) * 0.75) return item[0], item[1];
			}
			return usable_items.hp_heal.last[0], usable_items.hp_heal.last[1];
		}
		// Next prioritise using a status-curing item
		if (usable_items.status_cure &&
			(new []{:SLEEP, :FROZEN}.Contains(pkmn.status) || AIRandom(100) < 40)) {
			usable_items.status_cure.sort! { |a, b| a[2] <=> b[2] };
			return usable_items.status_cure.first[0], usable_items.status_cure.first[1];
		}
		// Next try using an item that raises all stats (Max Mushrooms)
		if (usable_items.all_stats_raise && AIRandom(100) < 30) {
			return usable_items.stat_raise.first[0], usable_items.stat_raise.first[1];
		}
		// Next try using an X item
		if (usable_items.stat_raise && AIRandom(100) < 30) {
			usable_items.stat_raise.sort! { |a, b| (a[2] == b[2]) ? a[3] <=> b[3] : a[2] <=> b[2] };
			return usable_items.stat_raise.last[0], usable_items.stat_raise.last[1];
		}
		// Find items usable on other Pokémon in the user's team
		// NOTE: Currently only checks Revives.
		usable_items = new List<string>();
		@battle.eachInTeamFromBattlerIndex(@user.index) do |team_pkmn, i|
			if (!team_pkmn.fainted()) continue;   // Remove this line to check unfainted Pokémon too
			foreach (var item in items) { //'items.each' do => |item|
				usage = get_usability_of_item_on_pkmn(item, i, @user.side);
				usage.each_pair do |key, vals|
					usable_items[key] ||= new List<string>();
					usable_items[key] += vals;
				}
			}
		}
		// Try using a Revive (prefer Max Revive-type items over Revive)
		if (usable_items.revive &&
			(@battle.AbleNonActiveCount(@user.index) == 0 || AIRandom(100) < 40)) {
			usable_items.revive.sort! { |a, b| (a[2] == b[2]) ? a[1] <=> b[1] : a[2] <=> b[2] };
			return usable_items.revive.last[0], usable_items.revive.last[1];
		}
		return null;
	}

	public void get_usability_of_item_on_pkmn(item, party_index, side) {
		pkmn = @battle.Party(side)[party_index];
		battler = @battle.FindBattler(party_index, side);
		ret = new List<string>();
		if (!@battle.CanUseItemOnPokemon(item, pkmn, battler, @battle.scene, false)) return ret;
		if ((!ItemHandlers.triggerCanUseInBattle(item, pkmn, battler, null,
																											false, self, @battle.scene, false)) return ret;
		want_to_cure_status = (pkmn.status != statuses.NONE);
		if (battler) {
			if (want_to_cure_status) {
				want_to_cure_status = @battlers[battler.index].wants_status_problem(pkmn.status);
				if (pkmn.status == statuses.SLEEP && pkmn.statusCount <= 2) want_to_cure_status = false;
			}
			want_to_cure_status ||= (battler.effects.Confusion > 1);
		}
		if (HP_HEAL_ITEMS.Contains(item)) {
			if (pkmn.hp < pkmn.totalhp) {
				heal_amount = HP_HEAL_ITEMS[item];
				if (item == items.SITURUSBERRY) heal_amount = pkmn.totalhp / 4;
				ret.hp_heal ||= new List<string>();
				ret.hp_heal.Add(new {item, party_index, 5, heal_amount});
			}
		} else if (FULL_RESTORE_ITEMS.Contains(item)) {
			prefer_full_restore = (pkmn.hp <= pkmn.totalhp * 2 / 3 && want_to_cure_status);
			if (pkmn.hp < pkmn.totalhp) {
				ret.hp_heal ||= new List<string>();
				ret.hp_heal.Add(new {item, party_index, (prefer_full_restore) ? 3 : 7, 999});
			}
			if (want_to_cure_status) {
				ret.status_cure ||= new List<string>();
				ret.status_cure.Add(new {item, party_index, (prefer_full_restore) ? 3 : 9});
			}
		} else if (ONE_STATUS_CURE_ITEMS.Contains(item)) {
			if (want_to_cure_status) {
				ret.status_cure ||= new List<string>();
				ret.status_cure.Add(new {item, party_index, 5});
			}
		} else if (ALL_STATUS_CURE_ITEMS.Contains(item)) {
			if (want_to_cure_status) {
				ret.status_cure ||= new List<string>();
				ret.status_cure.Add(new {item, party_index, 7});
			}
		} else if (ONE_STAT_RAISE_ITEMS.Contains(item)) {
			stat_data = ONE_STAT_RAISE_ITEMS[item];
			if (battler && stat_raise_worthwhile(@battlers[battler.index], stat_data[0])) {
				ret.stat_raise ||= new List<string>();
				ret.stat_raise.Add(new {item, party_index, battler.stages[stat_data[0]], stat_data[1]});
			}
		} else if (ALL_STATS_RAISE_ITEMS.Contains(item)) {
			if (battler) {
				ret.all_stats_raise ||= new List<string>();
				ret.all_stats_raise.Add(new {item, party_index});
			}
		} else if (REVIVE_ITEMS.Contains(item)) {
			ret.revive ||= new List<string>();
			ret.revive.Add(new {item, party_index, REVIVE_ITEMS[item]});
		}
		return ret;
	}
}
