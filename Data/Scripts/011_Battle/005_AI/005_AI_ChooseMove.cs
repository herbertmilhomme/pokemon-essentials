//===============================================================================
//
//===============================================================================
public partial class Battle.AI {
	public const int MOVE_FAIL_SCORE    = 20;
	public const int MOVE_USELESS_SCORE = 60;   // Move predicted to do nothing or just be detrimental
	public const int MOVE_BASE_SCORE    = 100;

	// Returns a value between 0.0 and 1.0. All move scores are lowered by this
	// value multiplied by the highest-scoring move's score.
	public void move_score_threshold() {
		return 0.6 + (0.35 * (((int)Math.Min(@trainer.skill, 100) / 100.0)**0.5));   // 0.635 to 0.95
	}

	//-----------------------------------------------------------------------------

	// Get scores for the user's moves.
	// NOTE: For any move with a target type that can target a foe (or which
	//       includes a foe(s) if it has multiple targets), the score calculated
	//       for a target ally will be inverted. The MoveHandlers for those moves
	//       should therefore treat an ally as a foe when calculating a score
	//       against it.
	public void GetMoveScores() {
		choices = new List<string>();
		@user.battler.eachMoveWithIndex do |orig_move, idxMove|
			// Unchoosable moves aren't considered
			if (!@battle.CanChooseMove(@user.index, idxMove, false)) {
				if (orig_move.pp == 0 && orig_move.total_pp > 0) {
					Debug.log_ai($"{@user.name} cannot use {orig_move.name} (no PP left)");
				} else {
					Debug.log_ai($"{@user.name} cannot choose to use {orig_move.name}");
				}
				continue;
			}
			// Set up move in class variables
			set_up_move_check(orig_move);
			// Predict whether the move will fail (generally)
			if (@trainer.has_skill_flag("PredictMoveFailure") && PredictMoveFailure) {
				Debug.log_ai($"{@user.name} is considering using {orig_move.name}...");
				Debug.log_score_change(MOVE_FAIL_SCORE - MOVE_BASE_SCORE, "move will fail");
				add_move_to_choices(choices, idxMove, MOVE_FAIL_SCORE);
				continue;
			}
			// Get the move's target type
			target_data = @move.Target(@user.battler);
			if (@move.function_code == "CurseTargetOrLowerUserSpd1RaiseUserAtkDef1" &&
				@move.rough_type == types.GHOST && @user.has_active_ability(new {:LIBERO, :PROTEAN})) {
				target_data = GameData.Target.get((Settings.MECHANICS_GENERATION >= 8) ? :RandomNearFoe : :NearFoe);
			}
			switch (target_data.num_targets) {
				case 0:   // No targets, affects the user or a side or the whole field
					// Includes: BothSides, FoeSide, None, User, UserSide
					Debug.log_ai($"{@user.name} is considering using {orig_move.name}...");
					score = MOVE_BASE_SCORE;
					Debug.logonerr(() => score = GetMoveScore);
					add_move_to_choices(choices, idxMove, score);
					break;
				case 1:   // One target to be chosen by the trainer
					// Includes: Foe, NearAlly, NearFoe, NearOther, Other, RandomNearFoe, UserOrNearAlly
					redirected_target = get_redirected_target(target_data);
					num_targets = 0;
					@battle.allBattlers.each do |b|
						if (redirected_target && b.index != redirected_target) continue;
						if (!@battle.MoveCanTarget(@user.battler.index, b.index, target_data)) continue;
						if (target_data.targets_foe && !@user.battler.opposes(b)) continue;
						Debug.log_ai($"{@user.name} is considering using {orig_move.name} against {b.name} ({b.index})...");
						score = MOVE_BASE_SCORE;
						Debug.logonerr(() => score = GetMoveScore([b]));
						add_move_to_choices(choices, idxMove, score, b.index);
						num_targets += 1;
					}
					if (num_targets == 0) Debug.Log($"     no valid targets");
					break;
				default:   // Multiple targets at once
					// Includes: AllAllies, AllBattlers, AllFoes, AllNearFoes, AllNearOthers, UserAndAllies
					targets = new List<string>();
					@battle.allBattlers.each do |b|
						if (!@battle.MoveCanTarget(@user.battler.index, b.index, target_data)) continue;
						targets.Add(b);
					}
					Debug.log_ai($"{@user.name} is considering using {orig_move.name}...");
					score = MOVE_BASE_SCORE;
					Debug.logonerr(() => score = GetMoveScore(targets));
					add_move_to_choices(choices, idxMove, score);
					break;
			}
		}
		@battle.moldBreaker = false;
		return choices;
	}

	// If the target of a move can be changed by an external effect, this method
	// returns the battler index of the new target.
	public void get_redirected_target(target_data) {
		if (@move.move.cannotRedirect()) return null;
		if (!target_data.can_target_one_foe() || target_data.num_targets != 1) return null;
		if (@user.has_active_ability(new {:PROPELLERTAIL, :STALWART})) return null;
		priority = @battle.Priority(true);
		near_only = !target_data.can_choose_distant_target();
		// Spotlight, Follow Me/Rage Powder
		new_target = -1;
		strength = 100;   // Lower strength takes priority
		foreach (var b in priority) { //'priority.each' do => |b|
			if (b.fainted() || b.effects.SkyDrop >= 0) continue;
			if (!b.opposes(@user.battler)) continue;
			if (near_only && !b.near(@user.battler)) continue;
			if (b.effects.Spotlight > 0 && b.effects.Spotlight - 50 < strength) {
				new_target = b.index;
				strength = b.effects.Spotlight - 50;   // Spotlight takes priority
			} else if ((b.effects.RagePowder && @user.battler.affectedByPowder()) ||
						(b.effects.FollowMe > 0 && b.effects.FollowMe < strength)) {
				new_target = b.index;
				strength = b.effects.FollowMe;
			}
		}
		if (new_target >= 0) return new_target;
		calc_type = @move.rough_type;
		foreach (var b in priority) { //'priority.each' do => |b|
			if (b.index == @user.index) continue;
			if (near_only && !b.near(@user.battler)) continue;
			switch (calc_type) {
				case :ELECTRIC:
					if (b.hasActiveAbility(Abilitys.LIGHTNINGROD)) new_target = b.index;
					break;
				case :WATER:
					if (b.hasActiveAbility(Abilitys.STORMDRAIN)) new_target = b.index;
					break;
			}
			if (new_target >= 0) break;
		}
		return (new_target >= 0) ? new_target : null;
	}

	public void add_move_to_choices(choices, idxMove, score, idxTarget = -1) {
		choices.Add(new {idxMove, score, idxTarget});
		// If the user is a wild Pokémon, doubly prefer one of its moves (the choice
		// is random but consistent and does not correlate to any other property of
		// the user)
		if (@user.wild() && @user.pokemon.personalID % @user.battler.moves.length == idxMove) {
			choices.Add(new {idxMove, score, idxTarget});
		}
	}

	//-----------------------------------------------------------------------------

	// Set some extra class variables for the move being assessed.
	public void set_up_move_check(move) {
		switch (move.function_code) {
			case "UseLastMoveUsed":
				if (@battle.lastMoveUsed &&
					GameData.Move.exists(@battle.lastMoveUsed) &&
					!move.moveBlacklist.Contains(GameData.Move.get(@battle.lastMoveUsed).function_code)) {
					move = Battle.Move.from_pokemon_move(@battle, new Pokemon.Move(@battle.lastMoveUsed));
				}
				break;
			case "UseMoveDependingOnEnvironment":
				move.OnStartUse(@user.battler, new List<string>());   // Determine which move is used instead
				move = Battle.Move.from_pokemon_move(@battle, new Pokemon.Move(move.npMove));
				break;
		}
		@battle.moldBreaker = @user.has_mold_breaker();
		@move.set_up(move);
	}

	// Set some extra class variables for the target being assessed.
	public void set_up_move_check_target(target) {
		@target = (target) ? @battlers[target.index] : null;
		@target&.refresh_battler;
		if (@target && @move.function_code == "UseLastMoveUsedByTarget") {
			if (@target.battler.lastRegularMoveUsed &&
				GameData.Move.exists(@target.battler.lastRegularMoveUsed) &&
				GameData.Move.get(@target.battler.lastRegularMoveUsed).has_flag("CanMirrorMove")) {
				@battle.moldBreaker = @user.has_mold_breaker();
				mov = Battle.Move.from_pokemon_move(@battle, new Pokemon.Move(@target.battler.lastRegularMoveUsed));
				@move.set_up(mov);
			}
		}
	}

	//-----------------------------------------------------------------------------

	// Returns whether the move will definitely fail (assuming no battle conditions
	// change between now and using the move).
	public void PredictMoveFailure() {
		// User is asleep and will not wake up
		if (@user.battler.asleep() && @user.statusCount > 1 && !@move.move.usableWhenAsleep()) return true;
		// User is awake and can't use moves that are only usable when asleep
		if (!@user.battler.asleep() && @move.move.usableWhenAsleep()) return true;
		// NOTE: Truanting is not considered, because if it is, a Pokémon with Truant
		//       will want to switch due to terrible moves every other round (because
		//       all of its moves will fail), and this is disruptive and shouldn't be
		//       how such Pokémon behave.
		// Primal weather
		if (@battle.Weather == Weathers.HeavyRain && @move.rough_type == types.FIRE) return true;
		if (@battle.Weather == Weathers.HarshSun && @move.rough_type == types.WATER) return true;
		// Move effect-specific checks
		if (Battle.AI.Handlers.move_will_fail(@move.function_code, @move, @user, self, @battle)) return true;
		return false;
	}

	// Returns whether the move will definitely fail against the target (assuming
	// no battle conditions change between now and using the move).
	public void PredictMoveFailureAgainstTarget() {
		// Move effect-specific checks
		if (Battle.AI.Handlers.move_will_fail_against_target(@move.function_code, @move, @user, @target, self, @battle)) return true;
		// Immunity to priority moves because of Psychic Terrain
		if (@battle.field.terrain == :Psychic && @target.battler.affectedByTerrain() &&
									@target.opposes(@user) && @move.rough_priority(@user) > 0) return true;
		// Immunity because of ability
		if (@move.move.ImmunityByAbility(@user.battler, @target.battler, false)) return true;
		// Immunity because of Dazzling/Queenly Majesty
		if (@move.rough_priority(@user) > 0 && @target.opposes(@user)) {
			each_same_side_battler(@target.side) do |b, i|
				if (b.has_active_ability(new {:DAZZLING, :QUEENLYMAJESTY})) return true;
			}
		}
		// Type immunity
		calc_type = @move.rough_type;
		typeMod = @move.move.CalcTypeMod(calc_type, @user.battler, @target.battler);
		if (@move.move.DamagingMove() && Effectiveness.ineffective(typeMod)) return true;
		// Dark-type immunity to moves made faster by Prankster
		if (Settings.MECHANICS_GENERATION >= 7 && @move.statusMove() &&
									@user.has_active_ability(abilitys.PRANKSTER) && @target.has_type(types.DARK) && @target.opposes(@user)) return true;
		// Airborne-based immunity to Ground moves
		if (@move.damagingMove() && calc_type == types.GROUND &&
									@target.battler.airborne() && !@move.move.hitsFlyingTargets()) return true;
		// Immunity to powder-based moves
		if (@move.move.powderMove() && !@target.battler.affectedByPowder()) return true;
		// Substitute
		if (@target.effects.Substitute > 0 && @move.statusMove() &&
									!@move.move.ignoresSubstitute(@user.battler) && @user.index != @target.index) return true;
		return false;
	}

	//-----------------------------------------------------------------------------

	// Get a score for the given move being used against the given target.
	// Assumes def set_up_move_check has previously been called.
	public void GetMoveScore(targets = null) {
		// Get the base score for the move
		score = MOVE_BASE_SCORE;
		// Scores for each target in turn
		if (targets) {
			// Reset the base score for the move (each target will add its own score)
			score = 0;
			affected_targets = 0;
			// Get a score for the move against each target in turn
			orig_move = @move.move;   // In case move is Mirror Move and changes depending on the target
			foreach (var target in targets) { //'targets.each' do => |target|
				set_up_move_check(orig_move);
				set_up_move_check_target(target);
				t_score = GetMoveScoreAgainstTarget;
				if (t_score < 0) continue;
				score += t_score;
				affected_targets += 1;
			}
			// Set the default score if no targets were affected
			if (affected_targets == 0) {
				score = (@trainer.has_skill_flag("PredictMoveFailure")) ? MOVE_USELESS_SCORE : MOVE_BASE_SCORE;
			}
			// Score based on how many targets were affected
			if (affected_targets == 0 && @trainer.has_skill_flag("PredictMoveFailure")) {
				if (!@move.move.worksWithNoTargets()) {
					Debug.log_score_change(MOVE_FAIL_SCORE - MOVE_BASE_SCORE, "move will fail");
					return MOVE_FAIL_SCORE;
				}
			} else {
				if (affected_targets > 1) score /= affected_targets;   // Average the score against multiple targets
				// Bonus for affecting multiple targets
				if (@trainer.has_skill_flag("PreferMultiTargetMoves") && affected_targets > 1) {
					old_score = score;
					score += (affected_targets - 1) * 10;
					Debug.log_score_change(score - old_score, "affects multiple battlers");
				}
			}
		}
		// If we're here, the move either has no targets or at least one target will
		// be affected (or the move is usable even if no targets are affected, e.g.
		// Self-Destruct)
		if (@trainer.has_skill_flag("ScoreMoves")) {
			// Modify the score according to the move's effect
			old_score = score;
			score = Battle.AI.Handlers.apply_move_effect_score(@move.function_code,
				score, @move, @user, self, @battle);
			Debug.log_score_change(score - old_score, "function code modifier (generic)");
			// Modify the score according to various other effects
			score = Battle.AI.Handlers.apply_general_move_score_modifiers(
				score, @move, @user, self, @battle);
		}
		score = score.ToInt();
		if (score < 0) score = 0;
		return score;
	}

	//-----------------------------------------------------------------------------

	// Returns the score of @move being used against @target. A return value of -1
	// means the move will fail or do nothing against the target.
	// Assumes def set_up_move_check and def set_up_move_check_target have
	// previously been called.
	public void GetMoveScoreAgainstTarget() {
		// Predict whether the move will fail against the target
		if (@trainer.has_skill_flag("PredictMoveFailure") && PredictMoveFailureAgainstTarget) {
			Debug.Log($"     move will not affect {@target.name}");
			return -1;
		}
		// Score the move
		score = MOVE_BASE_SCORE;
		if (@trainer.has_skill_flag("ScoreMoves")) {
			// Modify the score according to the move's effect against the target
			old_score = score;
			score = Battle.AI.Handlers.apply_move_effect_against_target_score(@move.function_code,
				MOVE_BASE_SCORE, @move, @user, @target, self, @battle);
			Debug.log_score_change(score - old_score, "function code modifier (against target)");
			// Modify the score according to various other effects against the target
			score = Battle.AI.Handlers.apply_general_move_against_target_score_modifiers(
				score, @move, @user, @target, self, @battle);
		}
		// Add the score against the target to the overall score
		target_data = @move.Target(@user.battler);
		if (target_data.targets_foe && !@target.opposes(@user) && @target.index != @user.index) {
			if (score == MOVE_USELESS_SCORE) {
				Debug.Log($"     move is useless against {@target.name}");
				return -1;
			}
			old_score = score;
			score = ((1.85 * MOVE_BASE_SCORE) - score).ToInt();
			Debug.log_score_change(score - old_score, "score inverted (move targets ally but can target foe)");
		}
		return score;
	}

	//-----------------------------------------------------------------------------

	// Make the final choice of which move to use depending on the calculated
	// scores for each move. Moves with higher scores are more likely to be chosen.
	public void ChooseMove(choices) {
		user_battler = @user.battler;
		// If no moves can be chosen, auto-choose a move or Struggle
		if (choices.length == 0) {
			@battle.AutoChooseMove(user_battler.index);
			Debug.log_ai($"{@user.name} will auto-use a move or Struggle");
			return;
		}
		// Figure out useful information about the choices
		max_score = 0;
		choices.each(c => { if (max_score < c[1]) max_score = c[1]; });
		// Decide whether all choices are bad, and if so, try switching instead
		if (@trainer.high_skill() && @user.can_switch_lax()) {
			badMoves = false;
			if (max_score <= MOVE_USELESS_SCORE) {
				badMoves = user.can_attack();
				if (!badMoves && AIRandom(100) < 25) badMoves = true;
			} else if (max_score < MOVE_BASE_SCORE * move_score_threshold && user_battler.turnCount > 2) {
				if (AIRandom(100) < 80) badMoves = true;
			}
			if (badMoves) {
				Debug.log_ai($"{@user.name} wants to switch due to terrible moves");
				if (ChooseToSwitchOut(true)) {
					@battle.UnregisterMegaEvolution(@user.index);
					return;
				}
				Debug.log_ai($"{@user.name} won't switch after all");
			}
		}
		// Calculate a minimum score threshold and reduce all move scores by it
		threshold = (int)Math.Floor(max_score * move_score_threshold.to_f);
		choices.each(c => c[3] = (int)Math.Max(c[1] - threshold, 0));
		total_score = choices.sum(c => c[3]);
		// Log the available choices
		if (Core.INTERNAL) {
			Debug.log_ai($"Move choices for {@user.name}:");
			choices.each_with_index do |c, i|
				chance = string.Format("{0:###0.0}", (c[3] > 0) ? 100.0 * c[3] / total_score : 0);
				log_msg = $"   * {chance}% to use {user_battler.moves[c[0]].name}";
				if (c[2] >= 0) log_msg += $" (target {c[2]})";
				log_msg += $": score {c[1]}";
				Debug.Log(log_msg);
			}
		}
		// Pick a move randomly from choices weighted by their scores
		randNum = AIRandom(total_score);
		foreach (var c in choices) { //'choices.each' do => |c|
			randNum -= c[3];
			if (randNum >= 0) continue;
			@battle.RegisterMove(user_battler.index, c[0], false);
			if (c[2] >= 0) @battle.RegisterTarget(user_battler.index, c[2]);
			break;
		}
		// Log the result
		if (@battle.choices[user_battler.index].Move) {
			move_name = @battle.choices[user_battler.index].Move.name;
			if (@battle.choices[user_battler.index].Target >= 0) {
				Debug.Log($"   => will use {move_name} (target {@battle.choices[user_battler.index].Target})");
			} else {
				Debug.Log($"   => will use {move_name}");
			}
		}
	}
}
