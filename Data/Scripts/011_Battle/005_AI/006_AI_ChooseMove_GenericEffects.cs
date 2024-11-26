//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	// Main method for calculating the score for moves that raise a battler's
	// stat(s).
	// By default, assumes that a stat raise is a good thing. However, this score
	// is inverted (by desire_mult) if the target opposes the user. If the move
	// could target a foe but is targeting an ally, the score is also inverted, but
	// only because it is inverted again in def GetMoveScoreAgainstTarget.
	public void get_score_for_target_stat_raise(score, target, stat_changes, whole_effect = true,
																			fixed_change = false, ignore_contrary = false) {
		if (@move.damagingMove()) whole_effect = false;
		// Decide whether the target raising its stat(s) is a good thing
		desire_mult = 1;
		if (target.opposes(@user) ||
			(@move.Target(@user.battler).targets_foe && target.index != @user.index)) {
			desire_mult = -1;
		}
		// If target has Contrary, use different calculations to score the stat change
		if (!ignore_contrary && !fixed_change && target.has_active_ability(abilitys.CONTRARY) && !target.being_mold_broken()) {
			if (desire_mult > 0 && whole_effect) {
				Debug.log_score_change(MOVE_USELESS_SCORE - score, "don't prefer raising target's stats (it has Contrary)");
				return MOVE_USELESS_SCORE;
			}
			return get_score_for_target_stat_drop(score, target, stat_changes, whole_effect, fixed_change, true);
		}
		// Don't make score changes if the move is a damaging move and its additional
		// effect (the stat raise(s)) will be negated
		add_effect = @move.get_score_change_for_additional_effect(@user, target);
		if (add_effect == -999) return score;   // Additional effect will be negated
		// Don't make score changes if target will faint from EOR damage
		if (target.rough_end_of_round_damage >= target.hp) {
			ret = (whole_effect) ? MOVE_USELESS_SCORE : score;
			Debug.Log($"     ignore stat change (target predicted to faint this round)");
			return ret;
		}
		// Don't make score changes if foes have Unaware and target can't make use of
		// extra stat stages
		if (!target.has_move_with_function("PowerHigherWithUserPositiveStatStages")) {
			foe_is_aware = false;
			each_foe_battler(target.side) do |b, i|
				if (!b.has_active_ability(abilitys.UNAWARE)) foe_is_aware = true;
			}
			if (!foe_is_aware) {
				ret = (whole_effect) ? MOVE_USELESS_SCORE : score;
				Debug.Log($"     ignore stat change (target's foes have Unaware)");
				return ret;
			}
		}
		// Figure out which stat raises can happen
		real_stat_changes = new List<string>();
		stat_changes.each_with_index do |stat, idx|
			if (idx.odd()) continue;
			if (!stat_raise_worthwhile(target, stat, fixed_change)) {
				if (target.index == @user.index) {
					Debug.Log($"     raising the user's {GameData.Stat.get(stat).name} isn't worthwhile");
				} else {
					Debug.Log($"     raising the target's {GameData.Stat.get(stat).name} isn't worthwhile");
				}
				continue;
			}
			// Calculate amount that stat will be raised by
			increment = stat_changes[idx + 1];
			if (!fixed_change && target.has_active_ability(abilitys.SIMPLE) && !target.being_mold_broken()) increment *= 2;
			increment = (int)Math.Min(increment, Battle.Battler.STAT_STAGE_MAXIMUM - target.stages[stat]);   // The actual stages gained
			// Count this as a valid stat raise
			if (increment > 0) real_stat_changes.Add(new {stat, increment});
		}
		// Discard move if it can't raise any stats
		if (real_stat_changes.length == 0) {
			Debug.Log($"     ignore stat raising (it can't be changed)");
			return (whole_effect) ? MOVE_USELESS_SCORE : score;
		}
		// Make score change based on the additional effect chance
		if (add_effect != 0) {
			old_score = score;
			score += add_effect;
			Debug.log_score_change(score - old_score, "stat raising is an additional effect");
		}
		// Make score changes based on the general concept of raising stats at all
		old_score = score;
		score = get_target_stat_raise_score_generic(score, target, real_stat_changes, desire_mult);
		Debug.log_score_change(score - old_score, "generic calculations for raising any stat");
		// Make score changes based on the specific changes to each stat that will be
		// raised
		foreach (var change in real_stat_changes) { //'real_stat_changes.each' do => |change|
			old_score = score;
			score = get_target_stat_raise_score_one(score, target, change[0], change[1], desire_mult);
			if (target.index == @user.index) {
				Debug.log_score_change(score - old_score, $"raising the user's {GameData.Stat.get(change[0]).name} by {change[1]}");
			} else {
				Debug.log_score_change(score - old_score, $"raising the target's {GameData.Stat.get(change[0]).name} by {change[1]}");
			}
		}
		return score;
	}

	//-----------------------------------------------------------------------------

	// Returns whether the target raising the given stat will have any impact.
	public bool stat_raise_worthwhile(target, stat, fixed_change = false) {
		if (!fixed_change) {
			if (!target.battler.CanRaiseStatStage(stat, @user.battler, @move.move)) return false;
		}
		// Check if target won't benefit from the stat being raised
		if ((target.has_move_with_function("SwitchOutUserPassOnEffects",
																									"PowerHigherWithUserPositiveStatStages")) return true;
		switch (stat) {
			case :ATTACK:
				if (!target.check_for_move { |m| m.physicalMove(m.type) &&
																										m.function_code != "UseUserDefenseInsteadOfUserAttack" &&
																										m.function_code != "UseTargetAttackInsteadOfUserAttack" }) return false;
				break;
			case :DEFENSE:
				each_foe_battler(target.side) do |b, i|
					if (b.check_for_move { |m| m.physicalMove(m.type) ||
																								m.function_code == "UseTargetDefenseInsteadOfTargetSpDef" }) return true;
				}
				return false;
			case :SPECIAL_ATTACK:
				if (!target.check_for_move(m => m.specialMove(m.type))) return false;
				break;
			case :SPECIAL_DEFENSE:
				each_foe_battler(target.side) do |b, i|
					if (b.check_for_move { |m| m.specialMove(m.type) &&
																								m.function_code != "UseTargetDefenseInsteadOfTargetSpDef" }) return true;
				}
				return false;
			case :SPEED:
				moves_that_prefer_high_speed = new {
					"PowerHigherWithUserFasterThanTarget",
					"PowerHigherWithUserPositiveStatStages";
				}
				if (!target.has_move_with_function(*moves_that_prefer_high_speed)) {
					meaningful = false;
					target_speed = target.rough_stat(:SPEED);
					each_foe_battler(target.side) do |b, i|
						b_speed = b.rough_stat(:SPEED);
						if (target_speed < b_speed && target_speed * 2.5 > b_speed) meaningful = true;
						if (meaningful) break;
					}
					if (!meaningful) return false;
				}
				break;
			case :ACCURACY:
				min_accuracy = 100;
				foreach (var m in target.battler.moves) { //'target.battler.moves.each' do => |m|
					if (m.accuracy == 0 || m.is_a(Battle.Move.OHKO)) continue;
					if (m.accuracy < min_accuracy) min_accuracy = m.accuracy;
				}
				if (min_accuracy >= 90 && target.stages[:ACCURACY] >= 0) {
					meaningful = false;
					each_foe_battler(target.side) do |b, i|
						if (b.stages[:EVASION] > 0) meaningful = true;
						if (meaningful) break;
					}
					if (!meaningful) return false;
				}
				break;
			case :EVASION:
				break;
		}
		return true;
	}

	//-----------------------------------------------------------------------------

	// Make score changes based on the general concept of raising stats at all.
	public void get_target_stat_raise_score_generic(score, target, stat_changes, desire_mult = 1) {
		total_increment = stat_changes.sum(change => change[1]);
		// Prefer if move is a status move and it's the user's first/second turn
		if (@user.turnCount < 2 && @move.statusMove()) {
			score += total_increment * desire_mult * 4;
		}
		if (@trainer.has_skill_flag("HPAware")) {
			// Prefer if user is at high HP, don't prefer if user is at low HP
			if (target.index != @user.index) {
				score += total_increment * desire_mult * ((100 * @user.hp / @user.totalhp) - 50) / 12;   // +4 to -4 per stage
			}
			// Prefer if target is at high HP, don't prefer if target is at low HP
			score += total_increment * desire_mult * ((100 * target.hp / target.totalhp) - 50) / 12;   // +4 to -4 per stage
		}
		// NOTE: There are no abilities that trigger upon stat raise, but this is
		//       where they would be accounted for if they existed.
		return score;
	}

	// Make score changes based on the raising of a specific stat.
	public void get_target_stat_raise_score_one(score, target, stat, increment, desire_mult = 1) {
		// Figure out how much the stat will actually change by
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stage_mul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stage_div = Battle.Battler.STAT_STAGE_DIVISORS;
		if (new []{:ACCURACY, :EVASION}.Contains(stat)) {
			stage_mul = Battle.Battler.ACC_EVA_STAGE_MULTIPLIERS;
			stage_div = Battle.Battler.ACC_EVA_STAGE_DIVISORS;
		}
		old_stage = target.stages[stat];
		new_stage = old_stage + increment;
		inc_mult = (stage_mul[new_stage + max_stage].to_f * stage_div[old_stage + max_stage]) / (stage_div[new_stage + max_stage] * stage_mul[old_stage + max_stage]);
		inc_mult -= 1;
		inc_mult *= desire_mult;
		// Stat-based score changes
		switch (stat) {
			case :ATTACK:
				// Modify score depending on current stat stage
				// More strongly prefer if the target has no special moves
				if (old_stage >= 2 && increment == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					has_special_moves = target.check_for_move(m => m.specialMove(m.type));
					inc = (has_special_moves) ? 8 : 12;
					score += inc * inc_mult;
				}
				break;
			case :DEFENSE:
				// Modify score depending on current stat stage
				if (old_stage >= 2 && increment == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * inc_mult;
				}
				break;
			case :SPECIAL_ATTACK:
				// Modify score depending on current stat stage
				// More strongly prefer if the target has no physical moves
				if (old_stage >= 2 && increment == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					has_physical_moves = target.check_for_move { |m| m.physicalMove(m.type) &&
																													m.function_code != "UseUserDefenseInsteadOfUserAttack" &&
																													m.function_code != "UseTargetAttackInsteadOfUserAttack" };
					inc = (has_physical_moves) ? 8 : 12;
					score += inc * inc_mult;
				}
				break;
			case :SPECIAL_DEFENSE:
				// Modify score depending on current stat stage
				if (old_stage >= 2 && increment == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * inc_mult;
				}
				break;
			case :SPEED:
				// Prefer if target is slower than a foe
				target_speed = target.rough_stat(:SPEED);
				each_foe_battler(target.side) do |b, i|
					b_speed = b.rough_stat(:SPEED);
					if (b_speed <= target_speed) continue;   // Target already outspeeds the foe b
					if (b_speed > target_speed * 2.5) continue;   // Much too slow to reasonably catch up
					if (b_speed < target_speed * (increment + 2) / 2) {
						score += 15 * inc_mult;   // Target will become faster than the foe b
					} else {
						score += 8 * inc_mult;
					}
					break;
				}
				// Prefer if the target has Electro Ball or Power Trip/Stored Power
				moves_that_prefer_high_speed = new {
					"PowerHigherWithUserFasterThanTarget",
					"PowerHigherWithUserPositiveStatStages";
				}
				if (target.has_move_with_function(*moves_that_prefer_high_speed)) {
					score += 5 * inc_mult;
				}
				// Don't prefer if any foe has Gyro Ball
				each_foe_battler(target.side) do |b, i|
					if (!b.has_move_with_function("PowerHigherWithTargetFasterThanUser")) continue;
					score -= 5 * inc_mult;
				}
				// Don't prefer if target has Speed Boost (will be gaining Speed anyway)
				if (target.has_active_ability(abilitys.SPEEDBOOST)) {
					score -= 15 * ((target.opposes(@user)) ? 1 : desire_mult);
				}
				break;
			case :ACCURACY:
				// Modify score depending on current stat stage
				if (old_stage >= 2 && increment == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					min_accuracy = 100;
					foreach (var m in target.battler.moves) { //'target.battler.moves.each' do => |m|
						if (m.accuracy == 0 || m.is_a(Battle.Move.OHKO)) continue;
						if (m.accuracy < min_accuracy) min_accuracy = m.accuracy;
					}
					min_accuracy = min_accuracy * stage_mul[old_stage] / stage_div[old_stage];
					if (min_accuracy < 90) {
						score += 10 * inc_mult;
					}
				}
				break;
			case :EVASION:
				// Prefer if a foe of the target will take damage at the end of the round
				each_foe_battler(target.side) do |b, i|
					eor_damage = b.rough_end_of_round_damage;
					if (eor_damage > 0) score += 5 * inc_mult;
				}
				// Modify score depending on current stat stage
				if (old_stage >= 2 && increment == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * inc_mult;
				}
				break;
		}
		// Prefer if target has Stored Power
		if (target.has_move_with_function("PowerHigherWithUserPositiveStatStages")) {
			score += 5 * increment * desire_mult;
		}
		// Don't prefer if any foe has Punishment
		each_foe_battler(target.side) do |b, i|
			if (!b.has_move_with_function("PowerHigherWithTargetPositiveStatStages")) continue;
			score -= 5 * increment * desire_mult;
		}
		return score;
	}

	//-----------------------------------------------------------------------------

	// Main method for calculating the score for moves that lower a battler's
	// stat(s).
	// By default, assumes that a stat drop is a good thing. However, this score
	// is inverted (by desire_mult) if the target is the user or an ally. This
	// inversion does not happen if the move could target a foe but is targeting an
	// ally, but only because it is inverted in def GetMoveScoreAgainstTarget
	// instead.
	public void get_score_for_target_stat_drop(score, target, stat_changes, whole_effect = true,
																		fixed_change = false, ignore_contrary = false) {
		if (@move.damagingMove()) whole_effect = false;
		// Decide whether the target lowering its stat(s) is a good thing
		desire_mult = -1;
		if (target.opposes(@user) ||
			(@move.Target(@user.battler).targets_foe && target.index != @user.index)) {
			desire_mult = 1;
		}
		// If target has Contrary, use different calculations to score the stat change
		if (!ignore_contrary && !fixed_change && target.has_active_ability(abilitys.CONTRARY) && !target.being_mold_broken()) {
			if (desire_mult > 0 && whole_effect) {
				Debug.log_score_change(MOVE_USELESS_SCORE - score, "don't prefer lowering target's stats (it has Contrary)");
				return MOVE_USELESS_SCORE;
			}
			return get_score_for_target_stat_raise(score, target, stat_changes, whole_effect, fixed_change, true);
		}
		// Don't make score changes if the move is a damaging move and its additional
		// effect (the stat drop(s)) will be negated
		add_effect = @move.get_score_change_for_additional_effect(@user, target);
		if (add_effect == -999) return score;   // Additional effect will be negated
		// Don't make score changes if target will faint from EOR damage
		if (target.rough_end_of_round_damage >= target.hp) {
			ret = (whole_effect) ? MOVE_USELESS_SCORE : score;
			Debug.Log($"     ignore stat change (target predicted to faint this round)");
			return ret;
		}
		// Don't make score changes if foes have Unaware and target can't make use of
		// its lowered stat stages
		foe_is_aware = false;
		each_foe_battler(target.side) do |b, i|
			if (!b.has_active_ability(abilitys.UNAWARE)) foe_is_aware = true;
		}
		if (!foe_is_aware) {
			ret = (whole_effect) ? MOVE_USELESS_SCORE : score;
			Debug.Log($"     ignore stat change (target's foes have Unaware)");
			return ret;
		}
		// Figure out which stat drops can happen
		real_stat_changes = new List<string>();
		stat_changes.each_with_index do |stat, idx|
			if (idx.odd()) continue;
			if (!stat_drop_worthwhile(target, stat, fixed_change)) {
				if (target.index == @user.index) {
					Debug.Log($"     lowering the user's {GameData.Stat.get(stat).name} isn't worthwhile");
				} else {
					Debug.Log($"     lowering the target's {GameData.Stat.get(stat).name} isn't worthwhile");
				}
				continue;
			}
			// Calculate amount that stat will be lowered by
			decrement = stat_changes[idx + 1];
			if (!fixed_change && target.has_active_ability(abilitys.SIMPLE) && !target.being_mold_broken()) decrement *= 2;
			decrement = (int)Math.Min(decrement, Battle.Battler.STAT_STAGE_MAXIMUM + target.stages[stat]);   // The actual stages lost
			// Count this as a valid stat drop
			if (decrement > 0) real_stat_changes.Add(new {stat, decrement});
		}
		// Discard move if it can't lower any stats
		if (real_stat_changes.length == 0) {
			Debug.Log($"     ignore stat lowering (it can't be changed)");
			return (whole_effect) ? MOVE_USELESS_SCORE : score;
		}
		// Make score change based on the additional effect chance
		if (add_effect != 0) {
			old_score = score;
			score += add_effect;
			Debug.log_score_change(score - old_score, "stat lowering is an additional effect");
		}
		// Make score changes based on the general concept of lowering stats at all
		old_score = score;
		score = get_target_stat_drop_score_generic(score, target, real_stat_changes, desire_mult);
		Debug.log_score_change(score - old_score, "generic calculations for lowering any stat");
		// Make score changes based on the specific changes to each stat that will be
		// lowered
		foreach (var change in real_stat_changes) { //'real_stat_changes.each' do => |change|
			old_score = score;
			score = get_target_stat_drop_score_one(score, target, change[0], change[1], desire_mult);
			if (target.index == @user.index) {
				Debug.log_score_change(score - old_score, $"lowering the user's {GameData.Stat.get(change[0]).name} by {change[1]}");
			} else {
				Debug.log_score_change(score - old_score, $"lowering the target's {GameData.Stat.get(change[0]).name} by {change[1]}");
			}
		}
		return score;
	}

	//-----------------------------------------------------------------------------

	// Returns whether the target lowering the given stat will have any impact.
	public bool stat_drop_worthwhile(target, stat, fixed_change = false) {
		if (!fixed_change) {
			if (!target.battler.CanLowerStatStage(stat, @user.battler, @move.move)) return false;
		}
		// Check if target won't benefit from the stat being lowered
		switch (stat) {
			case :ATTACK:
				if (!target.check_for_move { |m| m.physicalMove(m.type) &&
																										m.function_code != "UseUserDefenseInsteadOfUserAttack" &&
																										m.function_code != "UseTargetAttackInsteadOfUserAttack" }) return false;
				return false;
			case :DEFENSE:
				each_foe_battler(target.side) do |b, i|
					if (b.check_for_move { |m| m.physicalMove(m.type) ||
																								m.function_code == "UseTargetDefenseInsteadOfTargetSpDef" }) return true;
				}
				return false;
			case :SPECIAL_ATTACK:
				if (!target.check_for_move(m => m.specialMove(m.type))) return false;
				break;
			case :SPECIAL_DEFENSE:
				each_foe_battler(target.side) do |b, i|
					if (b.check_for_move { |m| m.specialMove(m.type) &&
																								m.function_code != "UseTargetDefenseInsteadOfTargetSpDef" }) return true;
				}
				return false;
			case :SPEED:
				moves_that_prefer_high_speed = new {
					"PowerHigherWithUserFasterThanTarget",
					"PowerHigherWithUserPositiveStatStages";
				}
				if (!target.has_move_with_function(*moves_that_prefer_high_speed)) {
					meaningful = false;
					target_speed = target.rough_stat(:SPEED);
					each_foe_battler(target.side) do |b, i|
						b_speed = b.rough_stat(:SPEED);
						if (target_speed > b_speed && target_speed < b_speed * 2.5) meaningful = true;
						if (meaningful) break;
					}
					if (!meaningful) return false;
				}
				break;
			case :ACCURACY:
				meaningful = false;
				foreach (var m in target.battler.moves) { //'target.battler.moves.each' do => |m|
					if (m.accuracy > 0 && !m.is_a(Battle.Move.OHKO)) meaningful = true;
					if (meaningful) break;
				}
				if (!meaningful) return false;
				break;
			case :EVASION:
				break;
		}
		return true;
	}

	//-----------------------------------------------------------------------------

	// Make score changes based on the general concept of lowering stats at all.
	public void get_target_stat_drop_score_generic(score, target, stat_changes, desire_mult = 1) {
		total_decrement = stat_changes.sum(change => change[1]);
		// Prefer if move is a status move and it's the user's first/second turn
		if (@user.turnCount < 2 && @move.statusMove()) {
			score += total_decrement * desire_mult * 4;
		}
		if (@trainer.has_skill_flag("HPAware")) {
			// Prefer if user is at high HP, don't prefer if user is at low HP
			if (target.index != @user.index) {
				score += total_decrement * desire_mult * ((100 * @user.hp / @user.totalhp) - 50) / 12;   // +4 to -4 per stage
			}
			// Prefer if target is at high HP, don't prefer if target is at low HP
			score += total_decrement * desire_mult * ((100 * target.hp / target.totalhp) - 50) / 12;   // +4 to -4 per stage
		}
		// Don't prefer if target has an ability that triggers upon stat loss
		// (Competitive, Defiant)
		if (target.opposes(@user) && Battle.AbilityEffects.OnStatLoss[target.ability]) {
			score -= 10;
		}
		return score;
	}

	// Make score changes based on the lowering of a specific stat.
	public void get_target_stat_drop_score_one(score, target, stat, decrement, desire_mult = 1) {
		// Figure out how much the stat will actually change by
		max_stage = Battle.Battler.STAT_STAGE_MAXIMUM;
		stage_mul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stage_div = Battle.Battler.STAT_STAGE_DIVISORS;
		if (new []{:ACCURACY, :EVASION}.Contains(stat)) {
			stage_mul = Battle.Battler.ACC_EVA_STAGE_MULTIPLIERS;
			stage_div = Battle.Battler.ACC_EVA_STAGE_DIVISORS;
		}
		old_stage = target.stages[stat];
		new_stage = old_stage - decrement;
		dec_mult = (stage_mul[old_stage + max_stage].to_f * stage_div[new_stage + max_stage]) / (stage_div[old_stage + max_stage] * stage_mul[new_stage + max_stage]);
		dec_mult -= 1;
		dec_mult *= desire_mult;
		// Stat-based score changes
		switch (stat) {
			case :ATTACK:
				// Modify score depending on current stat stage
				// More strongly prefer if the target has no special moves
				if (old_stage <= -2 && decrement == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					has_special_moves = target.check_for_move(m => m.specialMove(m.type));
					dec = (has_special_moves) ? 8 : 12;
					score += dec * dec_mult;
				}
				break;
			case :DEFENSE:
				// Modify score depending on current stat stage
				if (old_stage <= -2 && decrement == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * dec_mult;
				}
				break;
			case :SPECIAL_ATTACK:
				// Modify score depending on current stat stage
				// More strongly prefer if the target has no physical moves
				if (old_stage <= -2 && decrement == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					has_physical_moves = target.check_for_move { |m| m.physicalMove(m.type) &&
																													m.function_code != "UseUserDefenseInsteadOfUserAttack" &&
																													m.function_code != "UseTargetAttackInsteadOfUserAttack" };
					dec = (has_physical_moves) ? 8 : 12;
					score += dec * dec_mult;
				}
				break;
			case :SPECIAL_DEFENSE:
				// Modify score depending on current stat stage
				if (old_stage <= -2 && decrement == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * dec_mult;
				}
				break;
			case :SPEED:
				// Prefer if target is faster than an ally
				target_speed = target.rough_stat(:SPEED);
				each_foe_battler(target.side) do |b, i|
					b_speed = b.rough_stat(:SPEED);
					if (target_speed < b_speed) continue;   // Target is already slower than foe b
					if (target_speed > b_speed * 2.5) continue;   // Much too fast to reasonably be overtaken
					if (target_speed < b_speed * 2 / (decrement + 2)) {
						score += 15 * dec_mult;   // Target will become slower than foe b
					} else {
						score += 8 * dec_mult;
					}
					break;
				}
				// Prefer if any ally has Electro Ball
				each_foe_battler(target.side) do |b, i|
					if (!b.has_move_with_function("PowerHigherWithUserFasterThanTarget")) continue;
					score += 5 * dec_mult;
				}
				// Don't prefer if target has Speed Boost (will be gaining Speed anyway)
				if (target.has_active_ability(abilitys.SPEEDBOOST)) {
					score -= 15 * ((target.opposes(@user)) ? 1 : desire_mult);
				}
				break;
			case :ACCURACY:
				// Modify score depending on current stat stage
				if (old_stage <= -2 && decrement == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * dec_mult;
				}
				break;
			case :EVASION:
				// Modify score depending on current stat stage
				if (old_stage <= -2 && decrement == 1) {
					score -= 10 * ((target.opposes(@user)) ? 1 : desire_mult);
				} else {
					score += 10 * dec_mult;
				}
				break;
		}
		// Prefer if target has Stored Power
		if (target.has_move_with_function("PowerHigherWithUserPositiveStatStages")) {
			score += 5 * decrement * desire_mult;
		}
		// Don't prefer if any foe has Punishment
		each_foe_battler(target.side) do |b, i|
			if (!b.has_move_with_function("PowerHigherWithTargetPositiveStatStages")) continue;
			score -= 5 * decrement * desire_mult;
		}
		return score;
	}

	//-----------------------------------------------------------------------------

	public void get_score_for_weather(weather, move_user, starting = false) {
		if (@battle.CheckGlobalAbility(Abilities.AIRLOCK) ||
								@battle.CheckGlobalAbility(Abilities.CLOUDNINE)) return 0;
		ret = 0;
		if (starting) {
			weather_extender = {
				Sun       = :HEATROCK,
				Rain      = :DAMPROCK,
				Sandstorm = :SMOOTHROCK,
				Hail      = :ICYROCK,
				Snowstorm = :ICYROCK;
			}[weather];
			if (weather_extender && move_user.has_active_item(weather_extender)) ret += 4;
		}
		each_battler do |b, i|
			// Check each battler for weather-specific effects
			switch (weather) {
				case :Sun:
					// Check for Fire/Water moves
					if (b.has_damaging_move_of_type(types.FIRE)) {
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					if (b.has_damaging_move_of_type(types.WATER)) {
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					// Check for moves that freeze
					if (b.has_move_with_function("FreezeTarget", "FreezeFlinchTarget") ||
						(b.has_move_with_function("EffectDependsOnEnvironment") &&
						new []{:Snow, :Ice}.Contains(@battle.environment))) {
						ret += (b.opposes(move_user)) ? 5 : -5;
					}
					break;
				case :Rain:
					// Check for Fire/Water moves
					if (b.has_damaging_move_of_type(types.WATER)) {
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					if (b.has_damaging_move_of_type(types.FIRE)) {
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					break;
				case :Sandstorm:
					// Check for battlers affected by sandstorm's effects
					if (b.battler.takesSandstormDamage()) {   // End of round damage
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					if (b.has_type(types.ROCK)) {   // +SpDef for Rock types
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					break;
				case :Hail:
					// Check for battlers affected by hail's effects
					if (b.battler.takesHailDamage()) {   // End of round damage
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					break;
				case :Snowstorm:
					// TODO: Snowstorm AI.
					break;
				case :ShadowSky:
					// Check for battlers affected by Shadow Sky's effects
					if (b.has_damaging_move_of_type(types.SHADOW)) {
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					if (b.battler.takesShadowSkyDamage()) {   // End of round damage
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					break;
			}
			// Check each battler's abilities/other moves affected by the new weather
			if (@trainer.medium_skill() && !b.has_active_item(items.UTILITYUMBRELLA)) {
				beneficial_abilities = {
					Sun       = new {:CHLOROPHYLL, :FLOWERGIFT, :FORECAST, :HARVEST, :LEAFGUARD, :SOLARPOWER},
					Rain      = new {:DRYSKIN, :FORECAST, :HYDRATION, :RAINDISH, :SWIFTSWIM},
					Sandstorm = new {:SANDFORCE, :SANDRUSH, :SANDVEIL},
					Hail      = new {:FORECAST, :ICEBODY, :SLUSHRUSH, :SNOWCLOAK},
					Snowstorm = new {:FORECAST, :ICEBODY, :SLUSHRUSH, :SNOWCLOAK}
				}[weather];
				if (beneficial_abilities && beneficial_abilities.length > 0 &&
					b.has_active_ability(beneficial_abilities)) {
					ret += (b.opposes(move_user)) ? -5 : 5;
				}
				if (new []{:Hail, :Snowstorm}.Contains(weather) && b.ability == abilitys.ICEFACE) {
					ret += (b.opposes(move_user)) ? -5 : 5;
				}
				negative_abilities = {
					Sun = [:DRYSKIN];
				}[weather];
				if (negative_abilities && negative_abilities.length > 0 &&
					b.has_active_ability(negative_abilities)) {
					ret += (b.opposes(move_user)) ? 5 : -5;
				}
				beneficial_moves = {
					Sun       = new {"HealUserDependingOnWeather",
												"RaiseUserAtkSpAtk1Or2InSun",
												"TwoTurnAttackOneTurnInSun",
												"TypeAndPowerDependOnWeather"},
					Rain      = new {"ConfuseTargetAlwaysHitsInRainHitsTargetInSky",
												"ParalyzeTargetAlwaysHitsInRainHitsTargetInSky",
												"TypeAndPowerDependOnWeather"},
					Sandstorm = new {"HealUserDependingOnSandstorm",
												"TypeAndPowerDependOnWeather"},
					Hail      = new {"FreezeTargetAlwaysHitsInHail",
												"StartWeakenDamageAgainstUserSideIfHail",
												"TypeAndPowerDependOnWeather"},
					Snowstorm = new {"FreezeTargetAlwaysHitsInHail",
												"StartWeakenDamageAgainstUserSideIfHail",
												"TypeAndPowerDependOnWeather"},
					ShadowSky = ["TypeAndPowerDependOnWeather"];
				}[weather];
				if (beneficial_moves && beneficial_moves.length > 0 &&
					b.has_move_with_function(*beneficial_moves)) {
					ret += (b.opposes(move_user)) ? -5 : 5;
				}
				negative_moves = {
					Sun       = new {"ConfuseTargetAlwaysHitsInRainHitsTargetInSky",
												"ParalyzeTargetAlwaysHitsInRainHitsTargetInSky"},
					Rain      = new {"HealUserDependingOnWeather",
												"TwoTurnAttackOneTurnInSun"},
					Sandstorm = new {"HealUserDependingOnWeather",
												"TwoTurnAttackOneTurnInSun"},
					Hail      = new {"HealUserDependingOnWeather",
												"TwoTurnAttackOneTurnInSun"},
					Snowstorm = new {"HealUserDependingOnWeather",
												"TwoTurnAttackOneTurnInSun"};
				}[weather];
				if (negative_moves && negative_moves.length > 0 &&
					b.has_move_with_function(*negative_moves)) {
					ret += (b.opposes(move_user)) ? 5 : -5;
				}
			}
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	public void get_score_for_terrain(terrain, move_user, starting = false) {
		ret = 0;
		if (starting && terrain != :None && move_user.has_active_item(items.TERRAINEXTENDER)) ret += 4;
		// Inherent effects of terrain
		each_battler do |b, i|
			if (!b.battler.affectedByTerrain()) continue;
			switch (terrain) {
				case :Electric:
					// Immunity to sleep
					if (b.status == statuses.NONE) {
						ret += (b.opposes(move_user)) ? -8 : 8;
					}
					if (b.effects.Yawn > 0) {
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					// Check for Electric moves
					if (b.has_damaging_move_of_type(types.ELECTRIC)) {
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					break;
				case :Grassy:
					// End of round healing
					ret += (b.opposes(move_user)) ? -8 : 8;
					// Check for Grass moves
					if (b.has_damaging_move_of_type(types.GRASS)) {
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					break;
				case :Misty:
					// Immunity to status problems/confusion
					if (b.status == statuses.NONE || b.effects.Confusion == 0) {
						ret += (b.opposes(move_user)) ? -8 : 8;
					}
					// Check for Dragon moves
					if (b.has_damaging_move_of_type(types.DRAGON)) {
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					break;
				case :Psychic:
					// Check for priority moves
					if (b.check_for_move(m => m.priority > 0 && m.Target(b.battler)&.can_target_one_foe())) {
						ret += (b.opposes(move_user)) ? 10 : -10;
					}
					// Check for Psychic moves
					if (b.has_damaging_move_of_type(types.PSYCHIC)) {
						ret += (b.opposes(move_user)) ? -10 : 10;
					}
					break;
			}
		}
		// Held items relating to terrain
		seed = {
			Electric = :ELECTRICSEED,
			Grassy   = :GRASSYSEED,
			Misty    = :MISTYSEED,
			Psychic  = :PSYCHICSEED;
		}[terrain];
		each_battler do |b, i|
			if (seed && b.has_active_item(seed)) {
				ret += (b.opposes(move_user)) ? -8 : 8;
			}
		}
		// Check for abilities/moves affected by the terrain
		if (@trainer.medium_skill()) {
			abils = {
				Electric = :SURGESURFER,
				Grassy   = :GRASSPELT;
			}[terrain];
			good_moves = {
				Electric = ["DoublePowerInElectricTerrain"],
				Grassy   = new {"HealTargetDependingOnGrassyTerrain",
											"HigherPriorityInGrassyTerrain"},
				Misty    = ["UserFaintsPowersUpInMistyTerrainExplosive"],
				Psychic  = ["HitsAllFoesAndPowersUpInPsychicTerrain"];
			}[terrain];
			bad_moves = {
				Grassy = new {"DoublePowerIfTargetUnderground",
										"LowerTargetSpeed1WeakerInGrassyTerrain",
										"RandomPowerDoublePowerIfTargetUnderground"};
			}[terrain];
			each_battler do |b, i|
				if (!b.battler.affectedByTerrain()) continue;
				// Abilities
				if (b.has_active_ability(abilitys.MIMICRY)) {
					ret += (b.opposes(move_user)) ? -5 : 5;
				}
				if (abils && b.has_active_ability(abils)) {
					ret += (b.opposes(move_user)) ? -8 : 8;
				}
				// Moves
				if ((b.has_move_with_function("EffectDependsOnEnvironment",
																		"SetUserTypesBasedOnEnvironment",
																		"TypeAndPowerDependOnTerrain",
																		"UseMoveDependingOnEnvironment")) {
					ret += (b.opposes(move_user)) ? -5 : 5;
				}
				if (good_moves && b.has_move_with_function(*good_moves)) {
					ret += (b.opposes(move_user)) ? -5 : 5;
				}
				if (bad_moves && b.has_move_with_function(*bad_moves)) {
					ret += (b.opposes(move_user)) ? 5 : -5;
				}
			}
		}
		return ret;
	}
}
