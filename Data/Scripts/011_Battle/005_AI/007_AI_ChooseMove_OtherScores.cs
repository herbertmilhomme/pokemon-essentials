//===============================================================================
// Don't prefer hitting a wild shiny Pokémon.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:shiny_target,
	block: (score, move, user, target, ai, battle) => {
		if (target.wild() && target.battler.shiny()) {
			old_score = score;
			score -= 20;
			Debug.log_score_change(score - old_score, "avoid attacking a shiny wild Pokémon");
		}
		next score;
	}
)

//===============================================================================
// Prefer Shadow moves (for flavour).
//===============================================================================
Battle.AI.Handlers.GeneralMoveScore.add(:shadow_moves,
	block: (score, move, user, ai, battle) => {
		if (move.rough_type == types.SHADOW) {
			old_score = score;
			score += 10;
			Debug.log_score_change(score - old_score, "prefer using a Shadow move");
		}
		next score;
	}
)

//===============================================================================
// If user is frozen, prefer a move that can thaw the user.
//===============================================================================
Battle.AI.Handlers.GeneralMoveScore.add(:thawing_move_when_frozen,
	block: (score, move, user, ai, battle) => {
		if (ai.trainer.medium_skill() && user.status == statuses.FROZEN) {
			old_score = score;
			if (move.move.thawsUser()) {
				score += 20;
				Debug.log_score_change(score - old_score, "move will thaw the user");
			} else if (user.check_for_move(m => m.thawsUser())) {
				if ((user knows another move that thaws) score -= 20) {   // Don't prefer this move;
				Debug.log_score_change(score - old_score, "user knows another move will thaw it");
			}
		}
		next score;
	}
)

//===============================================================================
// Prefer using a priority move if the user is slower than the target and...
// - the user is at low HP, or
// - the target is predicted to be knocked out by the move.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:priority_move_against_faster_target,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.high_skill() && target.faster_than(user) && move.rough_priority(user) > 0) {
			// User is at risk of being knocked out
			if (ai.trainer.has_skill_flag("HPAware") && user.hp < user.totalhp / 3) {
				old_score = score;
				score += 8;
				Debug.log_score_change(score - old_score, "user at low HP and move has priority over faster target");
			}
			// Target is predicted to be knocked out by the move
			if (move.damagingMove() && move.rough_damage >= target.hp) {
				old_score = score;
				score += 8;
				Debug.log_score_change(score - old_score, "target at low HP and move has priority over faster target");
			}
			// Any foe knows Quick Guard and can protect against priority moves
			old_score = score;
			ai.each_foe_battler(user.side) do |b, i|
				if (!b.has_move_with_function("ProtectUserSideFromPriorityMoves")) continue;
				if (Settings.MECHANICS_GENERATION <= 5 && b.effects.ProtectRate > 1) continue;
				score -= 5;
			}
			if (score != old_score) {
				Debug.log_score_change(score - old_score, "a foe knows Quick Guard and may protect against priority moves");
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer a move that can be Magic Coated if the target (or any foe if the
// move doesn't have a target) knows Magic Coat/has Magic Bounce.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:target_can_Magic_Coat_or_Bounce_move,
	block: (score, move, user, target, ai, battle) => {
		if (move.statusMove() && move.move.canMagicCoat() && target.opposes(user) &&
			(target.faster_than(user) || !target.battler.semiInvulnerable())) {
			old_score = score;
			if (target.has_active_ability(abilitys.MAGICBOUNCE) && !target.being_mold_broken()) {
				score = Battle.AI.MOVE_USELESS_SCORE;
				Debug.log_score_change(score - old_score, "useless because target will Magic Bounce it");
			} else if (target.has_move_with_function("BounceBackProblemCausingStatusMoves") &&
						target.can_attack() && !target.battler.semiInvulnerable()) {
				score -= 7;
				Debug.log_score_change(score - old_score, "target knows Magic Coat and could bounce it");
			}
		}
		next score;
	}
)

Battle.AI.Handlers.GeneralMoveScore.add(:any_foe_can_Magic_Coat_or_Bounce_move,
	block: (score, move, user, ai, battle) => {
		if (move.statusMove() && move.move.canMagicCoat() && move.Target(user.battler).num_targets == 0) {
			old_score = score;
			ai.each_foe_battler(user.side) do |b, i|
				if (user.faster_than(b) && b.battler.semiInvulnerable()) continue;
				if (b.has_active_ability(abilitys.MAGICBOUNCE) && !b.being_mold_broken()) {
					score = Battle.AI.MOVE_USELESS_SCORE;
					Debug.log_score_change(score - old_score, "useless because a foe will Magic Bounce it");
					break;
				} else if (b.has_move_with_function("BounceBackProblemCausingStatusMoves") &&
							b.can_attack() && !b.battler.semiInvulnerable()) {
					score -= 7;
					Debug.log_score_change(score - old_score, "a foe knows Magic Coat and could bounce it");
					break;
				}
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer a move that can be Snatched if any other battler knows Snatch.
//===============================================================================
Battle.AI.Handlers.GeneralMoveScore.add(:any_battler_can_Snatch_move,
	block: (score, move, user, ai, battle) => {
		if (move.statusMove() && move.move.canSnatch()) {
			ai.each_battler do |b, i|
				if (b.index == user.index) continue;
				if (b.effects.SkyDrop >= 0) continue;
				if (!b.has_move_with_function("StealAndUseBeneficialStatusMove")) continue;
				old_score = score;
				score -= 7;
				Debug.log_score_change(score - old_score, "another battler could Snatch it");
				break;
			}
		}
		next score;
	}
)

//===============================================================================
// Pick a good move for the Choice items.
//===============================================================================
Battle.AI.Handlers.GeneralMoveScore.add(:good_move_for_choice_item,
	block: (score, move, user, ai, battle) => {
		if (!ai.trainer.medium_skill()) next score;
		if (!user.has_active_item(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF}) &&
									!user.has_active_ability(abilitys.GORILLATACTICS)) next score;
		old_score = score;
		// Really don't prefer status moves (except Trick)
		if (move.statusMove() && move.function_code != "UserTargetSwapItems") {
			score -= 25;
			Debug.log_score_change(score - old_score, "don't want to be Choiced into a status move");
			next score;
		}
		// Don't prefer moves which are 0x against at least one type
		move_type = move.rough_type;
		foreach (var type_data in GameData.Type) { //'GameData.Type.each' do => |type_data|
			if (type_data.immunities.Contains(move_type)) score -= 8;
		}
		// Don't prefer moves with lower accuracy
		if (move.accuracy > 0) {
			score -= (0.4 * (100 - move.accuracy)).ToInt();   // -0 (100%) to -39 (1%)
		}
		// Don't prefer moves with low PP
		if (move.move.pp <= 5) score -= 10;
		Debug.log_score_change(score - old_score, "move is less suitable to be Choiced into");
		next score;
	}
)

//===============================================================================
// Prefer damaging moves if the foe is down to their last Pokémon (opportunistic).
// Prefer damaging moves if the AI is down to its last Pokémon but the foe has
// more (desperate).
//===============================================================================
Battle.AI.Handlers.GeneralMoveScore.add(:damaging_move_and_either_side_no_reserves,
	block: (score, move, user, ai, battle) => {
		if (ai.trainer.medium_skill() && move.damagingMove()) {
			reserves = battle.AbleNonActiveCount(user.idxOwnSide);
			foes     = battle.AbleNonActiveCount(user.idxOpposingSide);
			// Don't mess with scores just because a move is damaging; need to play well
			if (ai.trainer.high_skill() && foes > reserves) next score;   // AI is outnumbered
			// Prefer damaging moves depending on remaining Pokémon
			old_score = score;
			if (foes == 0) {          // Foe is down to their last Pokémon
				score += 10;         // => Go for the kill
				Debug.log_score_change(score - old_score, "prefer damaging moves (no foe party Pokémon left)");
			} else if (reserves == 0) {   // AI is down to its last Pokémon, foe has reserves
				score += 5;          // => Go out with a bang
				Debug.log_score_change(score - old_score, "prefer damaging moves (no ally party Pokémon left)");
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer Fire-type moves if target knows Powder and is faster than the
// user.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:target_can_powder_fire_moves,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.high_skill() && move.rough_type == types.FIRE &&
			target.has_move_with_function("TargetNextFireMoveDamagesTarget") &&
			target.faster_than(user)) {
			old_score = score;
			score -= 5;   // Only 5 because we're not sure target will use Powder
			Debug.log_score_change(score - old_score, "target knows Powder and could negate Fire moves");
		}
		next score;
	}
)

//===============================================================================
// Don't prefer moves if target knows a move that can make them Electric-type,
// and if target is unaffected by Electric moves.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:target_can_make_moves_Electric_and_be_immune,
	block: (score, move, user, target, ai, battle) => {
		if (!ai.trainer.high_skill()) next score;
		if (!target.has_move_with_function("TargetMovesBecomeElectric") &&
									!(move.rough_type == types.NORMAL && target.has_move_with_function("NormalMovesBecomeElectric"))) next score;
		if (!ai.pokemon_can_absorb_move(target, move, :ELECTRIC) &&
									!Effectiveness.ineffective(target.effectiveness_of_type_against_battler(:ELECTRIC, user))) next score;
		priority = move.rough_priority(user);
		if (priority > 0 || (priority == 0 && target.faster_than(user))) {   // Target goes first
			old_score = score;
			score -= 5;   // Only 5 because we're not sure target will use Electrify/Ion Deluge
			Debug.log_score_change(score - old_score, "target knows Electrify/Ion Deluge and is immune to Electric moves");
		}
		next score;
	}
)

//===============================================================================
// Don't prefer attacking the target if they'd be semi-invulnerable.
// TODO: Don't treat the move as useless? If the user's moves are all useless
//       because of this, it will want to switch instead, which may not be
//       desirable.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:target_semi_invulnerable,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.medium_skill() && move.rough_accuracy > 0 &&
			(target.battler.semiInvulnerable() || target.effects.SkyDrop >= 0)) {
			if (user.has_active_ability(abilitys.NOGUARD) || target.has_active_ability(abilitys.NOGUARD)) next score;
			priority = move.rough_priority(user);
			if (priority > 0 || (priority == 0 && user.faster_than(target))) {   // User goes first
				miss = true;
				if (ai.trainer.high_skill()) {
					// Knows what can get past semi-invulnerability
					if (target.effects.SkyDrop >= 0 ||
						target.battler.inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
																						"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
																						"TwoTurnAttackInvulnerableInSkyTargetCannotAct")) {
						if (move.move.hitsFlyingTargets()) miss = false;
					} else if (target.battler.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderground")) {
						if (move.move.hitsDiggingTargets()) miss = false;
					} else if (target.battler.inTwoTurnAttack("TwoTurnAttackInvulnerableUnderwater")) {
						if (move.move.hitsDivingTargets()) miss = false;
					}
				}
				if (miss) {
					old_score = score;
					score = Battle.AI.MOVE_USELESS_SCORE;
					Debug.log_score_change(score - old_score, "target is semi-invulnerable");
				}
			}
		}
		next score;
	}
)

//===============================================================================
// Account for accuracy of move.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:predicted_accuracy,
	block: (score, move, user, target, ai, battle) => {
		acc = move.rough_accuracy.ToInt();
		if (acc < 90) {
			old_score = score;
			score -= (0.25 * (100 - acc)).ToInt();   // -2 (89%) to -24 (1%)
			Debug.log_score_change(score - old_score, $"accuracy (predicted {acc}%)");
		}
		next score;
	}
)

//===============================================================================
// Adjust score based on how much damage it can deal.
// Prefer the move even more if it's predicted to do enough damage to KO the
// target.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:predicted_damage,
	block: (score, move, user, target, ai, battle) => {
		if (move.damagingMove()) {
			dmg = move.rough_damage;
			old_score = score;
			if (target.effects.Substitute > 0) {
				target_hp = target.effects.Substitute;
				score += ((int)Math.Min(15.0 * dmg / target.effects.Substitute, 20)).ToInt();
				Debug.log_score_change(score - old_score, $"damaging move (predicted damage {dmg} = {100 * dmg / target.hp}% of target's Substitute)");
			} else {
				score += ((int)Math.Min(25.0 * dmg / target.hp, 30)).ToInt();
				Debug.log_score_change(score - old_score, $"damaging move (predicted damage {dmg} = {100 * dmg / target.hp}% of target's HP)");
				if (ai.trainer.has_skill_flag("HPAware") && dmg > target.hp * 1.1) {   // Predicted to KO the target
					old_score = score;
					score += 20;
					Debug.log_score_change(score - old_score, "predicted to KO the target");
					if (move.move.multiHitMove() && target.hp == target.totalhp &&
						(target.has_active_ability(abilitys.STURDY) || target.has_active_item(items.FOCUSSASH))) {
						old_score = score;
						score += 8;
						Debug.log_score_change(score - old_score, "predicted to overcome the target's Sturdy/Focus Sash");
					}
				}
			}
		}
		next score;
	}
)

//===============================================================================
// Prefer flinching external effects (note that move effects which cause
// flinching are dealt with in the function code part of score calculation).
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:external_flinching_effects,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.medium_skill() && move.damagingMove() && !move.move.flinchingMove() &&
			user.faster_than(target) && target.effects.Substitute == 0) {
			if (user.has_active_item(new {:KINGSROCK, :RAZORFANG}) ||
				user.has_active_ability(abilitys.STENCH)) {
				if (!target.has_active_ability(new {:INNERFOCUS, :SHIELDDUST}) || target.being_mold_broken()) {
					old_score = score;
					score += 8;
					if (move.move.multiHitMove()) score += 5;
					Debug.log_score_change(score - old_score, "added chance to cause flinching");
				}
			}
		}
		next score;
	}
)

//===============================================================================
// If target is frozen, don't prefer moves that could thaw them.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:thawing_move_against_frozen_target,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.medium_skill() && target.status == statuses.FROZEN) {
			if (move.rough_type == types.FIRE || (Settings.MECHANICS_GENERATION >= 6 && move.move.thawsUser())) {
				old_score = score;
				score -= 20;
				Debug.log_score_change(score - old_score, "thaws the target");
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer a damaging move if it will trigger the target's ability or held
// item when used, e.g. Effect Spore/Rough Skin, Pickpocket, Rocky Helmet, Red
// Card.
// TODO: These abilities/items may not be triggerable after all (e.g. they
//       require the move to make contact but it doesn't), or may have a negative
//       effect for the target (e.g. Air Balloon popping), but it's too much
//       effort to go into detail deciding all this.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:trigger_target_ability_or_item_upon_hit,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.high_skill() && move.damagingMove() && target.effects.Substitute == 0) {
			if (target.ability_active()) {
				if (Battle.AbilityEffects.OnBeingHit[target.ability] ||
					(Battle.AbilityEffects.AfterMoveUseFromTarget[target.ability] &&
					(!user.has_active_ability(abilitys.SHEERFORCE) || move.move.addlEffect == 0))) {
					old_score = score;
					score -= 8;
					Debug.log_score_change(score - old_score, "can trigger the target's ability");
				}
			}
			if (target.battler.isSpecies(Speciess.CRAMORANT) && target.ability == abilitys.GULPMISSILE &&
				target.battler.form > 0 && !target.effects.Transform) {
				old_score = score;
				score -= 8;
				Debug.log_score_change(score - old_score, "can trigger the target's ability");
			}
			if (target.item_active()) {
				if (Battle.ItemEffects.OnBeingHit[target.item] ||
					(Battle.ItemEffects.AfterMoveUseFromTarget[target.item] &&
					(!user.has_active_ability(abilitys.SHEERFORCE) || move.move.addlEffect == 0))) {
					old_score = score;
					score -= 8;
					Debug.log_score_change(score - old_score, "can trigger the target's item");
				}
			}
		}
		next score;
	}
)

//===============================================================================
// Prefer a damaging move if it will trigger the user's ability when used, e.g.
// Poison Touch, Magician.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:trigger_user_ability_upon_hit,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.high_skill() && user.ability_active() && move.damagingMove() &&
			target.effects.Substitute == 0) {
			// NOTE: The only ability with an OnDealingHit effect also requires the
			//       move to make contact. The only abilities with an OnEndOfUsingMove
			//       effect revolve around damaging moves.
			if ((Battle.AbilityEffects.OnDealingHit[user.ability] && move.move.contactMove()) ||
				Battle.AbilityEffects.OnEndOfUsingMove[user.ability]) {
				old_score = score;
				score += 8;
				Debug.log_score_change(score - old_score, "can trigger the user's ability");
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer damaging moves that will knock out the target if they are using
// Destiny Bond or Grudge.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:knocking_out_a_destiny_bonder_or_grudger,
	block: (score, move, user, target, ai, battle) => {
		if ((ai.trainer.has_skill_flag("HPAware") || ai.trainer.high_skill()) && move.damagingMove() &&
			(target.effects.DestinyBond || target.effects.Grudge)) {
			priority = move.rough_priority(user);
			if (priority > 0 || (priority == 0 && user.faster_than(target))) {   // User goes first
				if (move.rough_damage > target.hp * 1.1) {   // Predicted to KO the target
					old_score = score;
					if (target.effects.DestinyBond) {
						score -= 20;
						if (battle.AbleNonActiveCount(user.idxOwnSide) == 0) score -= 10;
						Debug.log_score_change(score - old_score, "don't want to KO the Destiny Bonding target");
					} else if (target.effects.Grudge) {
						score -= 15;
						if (battle.AbleNonActiveCount(user.idxOwnSide) == 0) score -= 7;
						Debug.log_score_change(score - old_score, "don't want to KO the Grudge-using target");
					}
				}
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer damaging moves if the target is using Rage, unless the move will
// deal enough damage to KO the target within two rounds.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:damaging_a_raging_target,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.medium_skill() && target.effects.Rage && move.damagingMove()) {
			// Worth damaging the target if it can be knocked out within two rounds
			if (ai.trainer.has_skill_flag("HPAware")) {
				if ((move.rough_damage + target.rough_end_of_round_damage) * 2 > target.hp * 1.1) next score;
			}
			old_score = score;
			score -= 10;
			Debug.log_score_change(score - old_score, "don't want to damage a Raging target");
		}
		next score;
	}
)

//===============================================================================
// Don't prefer damaging moves if the target is Biding, unless the move will deal
// enough damage to KO the target before it retaliates (assuming the move is used
// repeatedly until the target retaliates). Doesn't do a score change if the user
// will be immune to Bide's damage.
//===============================================================================
Battle.AI.Handlers.GeneralMoveAgainstTargetScore.add(:damaging_a_biding_target,
	block: (score, move, user, target, ai, battle) => {
		if (ai.trainer.medium_skill() && target.effects.Bide > 0 && move.damagingMove()) {
			eff = user.effectiveness_of_type_against_battler(:NORMAL, target);   // Bide is Normal type
			if (!Effectiveness.ineffective(eff)) {
				// Worth damaging the target if it can be knocked out before Bide ends
				if (ai.trainer.has_skill_flag("HPAware")) {
					dmg = move.rough_damage;
					eor_dmg = target.rough_end_of_round_damage;
					hits_possible = target.effects.Bide - 1;
					eor_dmg *= hits_possible;
					if (user.faster_than(target)) hits_possible += 1;
					if ((dmg * hits_possible) + eor_dmg > target.hp * 1.1) next score;
				}
				old_score = score;
				score -= 20;
				Debug.log_score_change(score - old_score, "don't want to damage the Biding target");
			}
		}
		next score;
	}
)

//===============================================================================
// Don't prefer a dancing move if the target has the Dancer ability.
//===============================================================================
Battle.AI.Handlers.GeneralMoveScore.add(:dance_move_against_dancer,
	block: (score, move, user, ai, battle) => {
		if (move.move.danceMove()) {
			old_score = score;
			ai.each_foe_battler(user.side) do |b, i|
				if (b.has_active_ability(abilitys.DANCER)) score -= 10;
			}
			Debug.log_score_change(score - old_score, "don't want to use a dance move because a foe has Dancer");
		}
		next score;
	}
)
