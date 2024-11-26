//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTakesTargetItem",
	block: (score, move, user, target, ai, battle) => {
		if (user.wild() || user.item) next score;
		if (!target.item || target.battler.unlosableItem(target.item)) next score;
		if (user.battler.unlosableItem(target.item)) next score;
		if (target.effects.Substitute > 0) next score;
		if (target.has_active_ability(abilitys.STICKYHOLD) && !target.being_mold_broken()) next score;
		// User can steal the target's item; score it
		user_item_preference = user.wants_item(target.item_id);
		user_no_item_preference = user.wants_item(items.NONE);
		user_diff = user_item_preference - user_no_item_preference;
		if (!user.item_active()) user_diff = 0;
		target_item_preference = target.wants_item(target.item_id);
		target_no_item_preference = target.wants_item(items.NONE);
		target_diff = target_no_item_preference - target_item_preference;
		if (!target.item_active()) target_diff = 0;
		score += user_diff * 4;
		score -= target_diff * 4;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("TargetTakesUserItem",
	block: (move, user, target, ai, battle) => {
		if (!user.item || user.battler.unlosableItem(user.item)) next true;
		if (target.item || target.battler.unlosableItem(user.item)) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("TargetTakesUserItem",
	block: (score, move, user, target, ai, battle) => {
		user_item_preference = user.wants_item(user.item_id);
		user_no_item_preference = user.wants_item(items.NONE);
		user_diff = user_no_item_preference - user_item_preference;
		if (!user.item_active()) user_diff = 0;
		target_item_preference = target.wants_item(user.item_id);
		target_no_item_preference = target.wants_item(items.NONE);
		target_diff = target_item_preference - target_no_item_preference;
		if (!target.item_active()) target_diff = 0;
		score += user_diff * 4;
		score -= target_diff * 4;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("UserTargetSwapItems",
	block: (move, user, target, ai, battle) => {
		if (user.wild()) next true;
		if (!user.item && !target.item) next true;
		if (user.battler.unlosableItem(user.item) || user.battler.unlosableItem(target.item)) next true;
		if (target.battler.unlosableItem(target.item) || target.battler.unlosableItem(user.item)) next true;
		if (target.has_active_ability(abilitys.STICKYHOLD) && !target.being_mold_broken()) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserTargetSwapItems",
	block: (score, move, user, target, ai, battle) => {
		user_new_item_preference = user.wants_item(target.item_id);
		user_old_item_preference = user.wants_item(user.item_id);
		user_diff = user_new_item_preference - user_old_item_preference;
		if (!user.item_active()) user_diff = 0;
		target_new_item_preference = target.wants_item(user.item_id);
		target_old_item_preference = target.wants_item(target.item_id);
		target_diff = target_new_item_preference - target_old_item_preference;
		if (!target.item_active()) target_diff = 0;
		score += user_diff * 4;
		score -= target_diff * 4;
		// Don't prefer if user used this move in the last round
		if (user.battler.lastMoveUsed &&
									GameData.Move.exists(user.battler.lastMoveUsed) &&
									GameData.Move.get(user.battler.lastMoveUsed).function_code == move.function_code) score -= 15;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RestoreUserConsumedItem",
	block: (move, user, ai, battle) => {
		next !user.battler.recycleItem || user.item;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RestoreUserConsumedItem",
	block: (score, move, user, ai, battle) => {
		if (!user.item_active()) next Battle.AI.MOVE_USELESS_SCORE;
		item_preference = user.wants_item(user.battler.recycleItem);
		no_item_preference = user.wants_item(items.NONE);
		score += (item_preference - no_item_preference) * 4;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveBasePower.add("RemoveTargetItem",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("RemoveTargetItem",
	block: (score, move, user, target, ai, battle) => {
		if (user.wild()) next score;
		if (!target.item || target.battler.unlosableItem(target.item)) next score;
		if (target.effects.Substitute > 0) next score;
		if (target.has_active_ability(abilitys.STICKYHOLD) && !target.being_mold_broken()) next score;
		if (!target.item_active()) next score;
		// User can knock off the target's item; score it
		item_preference = target.wants_item(target.item_id);
		no_item_preference = target.wants_item(items.NONE);
		score -= (no_item_preference - item_preference) * 4;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("DestroyTargetBerryOrGem",
	block: (score, move, user, target, ai, battle) => {
		if (!target.item || (!target.item.is_berry() &&
									!(Settings.MECHANICS_GENERATION >= 6 && target.item.is_gem()))) next score;
		if (user.battler.unlosableItem(target.item)) next score;
		if (target.effects.Substitute > 0) next score;
		if (target.has_active_ability(abilitys.STICKYHOLD) && !target.being_mold_broken()) next score;
		if (!target.item_active()) next score;
		// User can incinerate the target's item; score it
		item_preference = target.wants_item(target.item_id);
		no_item_preference = target.wants_item(items.NONE);
		score -= (no_item_preference - item_preference) * 4;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("CorrodeTargetItem",
	block: (move, user, target, ai, battle) => {
		if (!target.item || target.battler.unlosableItem(target.item) ||
								target.effects.Substitute > 0) next true;
		if (target.has_active_ability(abilitys.STICKYHOLD)) next true;
		if (battle.corrosiveGas[target.index % 2][target.party_index]) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("CorrodeTargetItem",
	block: (score, move, user, target, ai, battle) => {
		item_preference = target.wants_item(target.item_id);
		no_item_preference = target.wants_item(items.NONE);
		target_diff = no_item_preference - item_preference;
		if (!target.item_active()) target_diff = 0;
		score += target_diff * 4;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("StartTargetCannotUseItem",
	block: (move, user, target, ai, battle) => {
		next target.effects.Embargo > 0;
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("StartTargetCannotUseItem",
	block: (score, move, user, target, ai, battle) => {
		if (!target.item || !target.item_active()) next Battle.AI.MOVE_USELESS_SCORE;
		// NOTE: We won't check if the item has an effect, because if a Pokémon is
		//       holding an item, it probably does.
		item_score = target.wants_item(target.item_id);
		if (item_score <= 0) next Battle.AI.MOVE_USELESS_SCORE;   // Item has no effect or is bad
		score += item_score * 2;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("StartNegateHeldItems",
	block: (score, move, user, ai, battle) => {
		if (battle.field.effects.MagicRoom == 1) continue;   // About to expire anyway
		any_held_items = false;
		total_want = 0;   // Positive means foes want their items more than allies do
		ai.each_battler do |b, i|
			if (!b.item) continue;
			// Skip b if its item is disabled
			if (ai.trainer.medium_skill()) {
				// NOTE: We won't check if the item has an effect, because if a Pokémon
				//       is holding an item, it probably does.
				if (battle.field.effects.MagicRoom > 0) {
					// NOTE: Same as b.item_active() but ignoring the Magic Room part.
					if (b.effects.Embargo > 0) continue;
					if (battle.corrosiveGas[b.index % 2][b.party_index]) continue;
					if (b.has_active_ability(abilitys.KLUTZ)) continue;
				} else {
					if (!b.item_active()) continue;
				}
			}
			// Rate b's held item and add it to total_want
			any_held_items = true;
			want = b.wants_item(b.item_id);
			total_want += (b.opposes(user)) ? want : -want;
		}
		// Alter score
		if (!any_held_items) next Battle.AI.MOVE_USELESS_SCORE;
		if (battle.field.effects.MagicRoom > 0) {
			if (total_want >= 0) next Battle.AI.MOVE_USELESS_SCORE;
			score -= (int)Math.Max(total_want, -5) * 4;   // Will enable items, prefer if allies affected more
		} else {
			if (total_want <= 0) next Battle.AI.MOVE_USELESS_SCORE;
			score += (int)Math.Min(total_want, 5) * 4;   // Will disable items, prefer if foes affected more
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserConsumeBerryRaiseDefense2",
	block: (move, user, ai, battle) => {
		item = user.item;
		next !item || !item.is_berry() || !user.item_active();
	}
)
Battle.AI.Handlers.MoveEffectScore.add("UserConsumeBerryRaiseDefense2",
	block: (score, move, user, ai, battle) => {
		// Score for raising the user's stat
		stat_raise_score = Battle.AI.Handlers.apply_move_effect_score("RaiseUserDefense2",
			0, move, user, ai, battle);
		if (stat_raise_score != Battle.AI.MOVE_USELESS_SCORE) score += stat_raise_score;
		// Score for the consumed berry's effect
		score += user.get_score_change_for_consuming_item(user.item_id, true);
		// Score for other results of consuming the berry
		if (ai.trainer.medium_skill()) {
			// Prefer if user will heal itself with Cheek Pouch
			if (user.battler.canHeal() && user.hp < user.totalhp / 2 &&) score += 8;
										user.has_active_ability(abilitys.CHEEKPOUCH);
			// Prefer if target can recover the consumed berry
			if (user.has_active_ability(abilitys.HARVEST) ||
										user.has_move_with_function("RestoreUserConsumedItem")) score += 8;
			// Prefer if user couldn't normally consume the berry
			if (!user.battler.canConsumeBerry()) score += 5;
			// Prefer if user will become able to use Belch
			if (!user.battler.belched() && user.has_move_with_function("FailsIfUserNotConsumedBerry")) score += 5;
			// Prefer if user will benefit from not having an item
			if (user.has_active_ability(abilitys.UNBURDEN)) score += 5;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("AllBattlersConsumeBerry",
	block: (move, user, target, ai, battle) => {
		next !target.item || !target.item.is_berry() || target.battler.semiInvulnerable();
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("AllBattlersConsumeBerry",
	block: (score, move, user, target, ai, battle) => {
		// Score for the consumed berry's effect
		score_change = target.get_score_change_for_consuming_item(target.item_id, !target.opposes(user));
		// Score for other results of consuming the berry
		if (ai.trainer.medium_skill()) {
			// Prefer if target will heal itself with Cheek Pouch
			if (target.battler.canHeal() && target.hp < target.totalhp / 2 &&) score_change += 8;
													target.has_active_ability(abilitys.CHEEKPOUCH);
			// Prefer if target can recover the consumed berry
			if (target.has_active_ability(abilitys.HARVEST) ||
													target.has_move_with_function("RestoreUserConsumedItem")) score_change += 8;
			// Prefer if target couldn't normally consume the berry
			if (!target.battler.canConsumeBerry()) score_change += 5;
			// Prefer if target will become able to use Belch
			if (!target.battler.belched() && target.has_move_with_function("FailsIfUserNotConsumedBerry")) score_change += 5;
			// Prefer if target will benefit from not having an item
			if (target.has_active_ability(abilitys.UNBURDEN)) score_change += 5;
		}
		score += (target.opposes(user)) ? -score_change : score_change;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("UserConsumeTargetBerry",
	block: (score, move, user, target, ai, battle) => {
		if (!target.item || !target.item.is_berry()) next score;
		if (user.battler.unlosableItem(target.item)) next score;
		if (target.effects.Substitute > 0) next score;
		if (target.has_active_ability(abilitys.STICKYHOLD) && !target.being_mold_broken()) next score;
		// Score the user gaining the item's effect
		score += user.get_score_change_for_consuming_item(target.item_id);
		// Score for other results of consuming the berry
		if (ai.trainer.medium_skill()) {
			// Prefer if user will heal itself with Cheek Pouch
			if (user.battler.canHeal() && user.hp < user.totalhp / 2 &&) score += 8;
										user.has_active_ability(abilitys.CHEEKPOUCH);
			// Prefer if user will become able to use Belch
			if (!user.battler.belched() && user.has_move_with_function("FailsIfUserNotConsumedBerry")) score += 5;
			// Don't prefer if target will benefit from not having an item
			if (target.has_active_ability(abilitys.UNBURDEN)) score -= 5;
		}
		// Score the target no longer having the item
		item_preference = target.wants_item(target.item_id);
		no_item_preference = target.wants_item(items.NONE);
		score -= (no_item_preference - item_preference) * 3;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("ThrowUserItemAtTarget",
	block: (move, user, ai, battle) => {
		item = user.item;
		if (!item || !user.item_active() || user.battler.unlosableItem(item)) next true;
		if (item.is_berry() && !user.battler.canConsumeBerry()) next true;
		if (item.flags.none(f => System.Text.RegularExpressions.Regex.IsMatch(f,@"^Fling_",RegexOptions.IgnoreCase))) next true;
		next false;
	}
)
Battle.AI.Handlers.MoveBasePower.add("ThrowUserItemAtTarget",
	block: (power, move, user, target, ai, battle) => {
		next move.move.BaseDamage(power, user.battler, target.battler);
	}
)
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("ThrowUserItemAtTarget",
	block: (score, move, user, target, ai, battle) => {
		switch (user.item_id) {
			case :POISONBARB: case :TOXICORB:
				score = Battle.AI.Handlers.apply_move_effect_against_target_score("PoisonTarget",
					score, move, user, target, ai, battle);
				break;
			case :FLAMEORB:
				score = Battle.AI.Handlers.apply_move_effect_against_target_score("BurnTarget",
					score, move, user, target, ai, battle);
				break;
			case :LIGHTBALL:
				score = Battle.AI.Handlers.apply_move_effect_against_target_score("ParalyzeTarget",
					score, move, user, target, ai, battle);
				break;
			case :KINGSROCK: case :RAZORFANG:
				score = Battle.AI.Handlers.apply_move_effect_against_target_score("FlinchTarget",
					score, move, user, target, ai, battle);
				break;
			default:
				score -= target.get_score_change_for_consuming_item(user.item_id);
				break;
		}
		// Score for other results of consuming the berry
		if (ai.trainer.medium_skill()) {
			// Don't prefer if target will become able to use Belch
			if (user.item.is_berry() && !target.battler.belched() &&
										target.has_move_with_function("FailsIfUserNotConsumedBerry")) score -= 5;
			// Prefer if user will benefit from not having an item
			if (user.has_active_ability(abilitys.UNBURDEN)) score += 5;
		}
		// Prefer if the user doesn't want its held item/don't prefer if it wants to
		// keep its held item
		item_preference = user.wants_item(user.item_id);
		no_item_preference = user.wants_item(items.NONE);
		score += (no_item_preference - item_preference) * 2;
		next score;
	}
)
