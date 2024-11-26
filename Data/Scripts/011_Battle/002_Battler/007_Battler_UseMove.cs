//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Turn processing.
	//-----------------------------------------------------------------------------

	public void ProcessTurn(choice, tryFlee = true) {
		if (fainted()) return false;
		// Wild roaming Pokémon always flee if possible
		if (tryFlee && wild() &&
			@battle.rules["alwaysflee"] && @battle.CanRun(@index)) {
			BeginTurn(choice);
			SEPlay("Battle flee");
			@battle.Display(_INTL("{1} fled from battle!", This));
			@battle.decision = Battle.Outcome.FLEE;
			EndTurn(choice);
			return true;
		}
		// Shift with the battler next to this one
		if (choice[0] == :Shift) {
			idxOther = -1;
			switch (@battle.SideSize(@index)) {
				case 2:
					idxOther = (@index + 2) % 4;
					break;
				case 3:
					if (@index != 2 && @index != 3) {   // If not in middle spot already
						idxOther = (@index.even()) ? 2 : 3;
					}
					break;
			}
			if (idxOther >= 0) {
				@battle.SwapBattlers(@index, idxOther);
				switch (@battle.SideSize(@index)) {
					case 2:
						@battle.Display(_INTL("{1} moved across!", This));
						break;
					case 3:
						@battle.Display(_INTL("{1} moved to the center!", This));
						break;
				}
			}
			BeginTurn(choice);
			CancelMoves;
			@lastRoundMoved = @battle.turnCount;   // Done something this round
			return true;
		}
		// If this battler's action for this round wasn't "use a move"
		if (choice[0] != :UseMove) {
			// Clean up effects that end at battler's turn
			BeginTurn(choice);
			EndTurn(choice);
			return false;
		}
		// Use the move
		Debug.Log($"[Use move] {This} ({@index}) used {choice[2].name}");
		Debug.logonerr(() => UseMove(choice, choice[2] == @battle.struggle));
		@battle.Judge;
		// Update priority order
		if (Settings.RECALCULATE_TURN_ORDER_AFTER_SPEED_CHANGES) @battle.CalculatePriority;
		return true;
	}

	//-----------------------------------------------------------------------------
	//
	//-----------------------------------------------------------------------------

	public void BeginTurn(_choice) {
		// Cancel some lingering effects which only apply until the user next moves
		@effects.DestinyBondPrevious = @effects.DestinyBond;
		@effects.DestinyBond         = false;
		@effects.Grudge              = false;
		@effects.MoveNext            = false;
		@effects.Quash               = 0;
		// Encore's effect ends if the encored move is no longer available
		if (@effects.Encore > 0 && EncoredMoveIndex < 0) {
			@effects.Encore     = 0;
			@effects.EncoreMove = null;
		}
	}

	// Called when the usage of various multi-turn moves is disrupted due to
	// failing TryUseMove, being ineffective against all targets, or because
	// Pursuit was used specially to intercept a switching foe.
	// Cancels the use of multi-turn moves and counters thereof. Note that Hyper
	// Beam's effect is NOT cancelled.
	public void CancelMoves(full_cancel = false) {
		// Outragers get confused anyway if they are disrupted during their final
		// turn of using the move
		if (@effects.Outrage == 1 && CanConfuseSelf(false) && !full_cancel) {
			Confuse(_INTL("{1} became confused due to fatigue!", This));
		}
		// Cancel usage of most multi-turn moves
		@effects.TwoTurnAttack = null;
		@effects.Rollout       = 0;
		@effects.Outrage       = 0;
		@effects.Uproar        = 0;
		@effects.Bide          = 0;
		if (@effects.HyperBeam == 0) @currentMove = null;
		// Reset counters for moves which increase them when used in succession
		@effects.FuryCutter = 0;
	}

	public void EndTurn(_choice) {
		@lastRoundMoved = @battle.turnCount;   // Done something this round
		if (!@effects.ChoiceBand &&
			(hasActiveItem(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF}) ||
			hasActiveAbility(Abilitys.GORILLATACTICS))) {
			if (@lastMoveUsed && HasMove(@lastMoveUsed)) {
				@effects.ChoiceBand = @lastMoveUsed;
			} else if (@lastRegularMoveUsed && HasMove(@lastRegularMoveUsed)) {
				@effects.ChoiceBand = @lastRegularMoveUsed;
			}
		}
		@effects.BeakBlast   = false;
		if (Settings.MECHANICS_GENERATION < 9 || @lastMoveUsedType == Types.ELECTRIC) {
			if (@effects.Charge == 1) @effects.Charge    = 0;
		}
		@effects.GemConsumed = null;
		@effects.ShellTrap   = false;
		@battle.allBattlers.each(b => b.ContinualAbilityChecks);   // Trace, end primordial weathers
	}

	public void ConfusionDamage(msg) {
		@damageState.reset;
		confusionMove = new Battle.Move.Confusion(@battle, null);
		confusionMove.calcType = confusionMove.CalcType(self);   // null
		@damageState.typeMod = confusionMove.CalcTypeMod(confusionMove.calcType, self, self);   // 8
		confusionMove.CheckDamageAbsorption(self, self);
		confusionMove.CalcDamage(self, self);
		confusionMove.ReduceDamage(self, self);
		self.hp -= @damageState.hpLost;
		confusionMove.AnimateHitAndHPLost(self, [self]);
		@battle.Display(msg);   // "It hurt itself in its confusion!"
		confusionMove.RecordDamageLost(self, self);
		confusionMove.EndureKOMessage(self);
		if (fainted()) Faint;
		ItemHPHealCheck;
	}

	//-----------------------------------------------------------------------------
	// Simple "use move" method, used when a move calls another move and for Future
	// Sight's attack.
	//-----------------------------------------------------------------------------

	public void UseMoveSimple(moveID, target = -1, idxMove = -1, specialUsage = true) {
		choice = new List<string>();
		choice[0] = :UseMove;   // "Use move"
		choice[1] = idxMove;    // Index of move to be used in user's moveset
		if (idxMove >= 0) {
			choice[2] = @moves[idxMove];
		} else {
			choice[2] = Battle.Move.from_pokemon_move(@battle, new Pokemon.Move(moveID));
			choice[2].pp = -1;
		}
		choice[3] = target;     // Target (-1 means no target yet)
		Debug.Log($"[Use move] {This} used the called/simple move {choice[2].name}");
		UseMove(choice, specialUsage);
	}

	//-----------------------------------------------------------------------------
	// Master "use move" method.
	//-----------------------------------------------------------------------------

	public void UseMove(choice, specialUsage = false) {
		// NOTE: This is intentionally determined before a multi-turn attack can
		//       set specialUsage to true.
		skipAccuracyCheck = (specialUsage && choice[2] != @battle.struggle);
		// Start using the move
		BeginTurn(choice);
		// Force the use of certain moves if they're already being used
		if (!@battle.futureSight) {
			if (usingMultiTurnAttack()) {
				choice[2] = Battle.Move.from_pokemon_move(@battle, new Pokemon.Move(@currentMove));
				specialUsage = true;
			} else if (@effects.Encore > 0 && choice[1] >= 0 &&
						@battle.CanShowCommands(@index)) {
				idxEncoredMove = EncoredMoveIndex;
				if (idxEncoredMove >= 0 && choice[1] != idxEncoredMove &&
					@battle.CanChooseMove(@index, idxEncoredMove, false)) {   // Change move if battler was Encored mid-round
					choice[1] = idxEncoredMove;
					choice[2] = @moves[idxEncoredMove];
					choice[3] = -1;   // No target chosen
				}
			}
		}
		// Labels the move being used as "move"
		move = choice[2];
		if (!move) return;   // if move was not chosen somehow
		// Try to use the move (inc. disobedience)
		@lastMoveFailed = false;
		if (!TryUseMove(choice, move, specialUsage, skipAccuracyCheck)) {
			@lastMoveUsed     = null;
			@lastMoveUsedType = null;
			if (!specialUsage) {
				@lastRegularMoveUsed   = null;
				@lastRegularMoveTarget = -1;
			}
			@battle.GainExp;   // In case self is KO'd due to confusion
			CancelMoves;
			EndTurn(choice);
			return;
		}
		move = choice[2];   // In case disobedience changed the move to be used
		if (!move) return;   // if move was not chosen somehow
		// Subtract PP
		if (!specialUsage && !ReducePP(move)) {
			@battle.Display(_INTL("{1} used {2}!", This, move.name));
			@battle.Display(_INTL("But there was no PP left for the move!"));
			@lastMoveUsed          = null;
			@lastMoveUsedType      = null;
			@lastRegularMoveUsed   = null;
			@lastRegularMoveTarget = -1;
			@lastMoveFailed        = true;
			CancelMoves;
			EndTurn(choice);
			return;
		}
		// Stance Change
		if (isSpecies(Speciess.AEGISLASH) && self.ability == abilitys.STANCECHANGE) {
			if (move.damagingMove()) {
				ChangeForm(1, _INTL("{1} changed to Blade Forme!", This));
			} else if (move.id == moves.KINGSSHIELD) {
				ChangeForm(0, _INTL("{1} changed to Shield Forme!", This));
			}
		}
		// Calculate the move's type during this usage
		move.calcType = move.CalcType(self);
		// Start effect of Mold Breaker
		@battle.moldBreaker = hasMoldBreaker();
		// Remember that user chose a two-turn move
		if (move.IsChargingTurn(self)) {
			// Beginning the use of a two-turn attack
			@effects.TwoTurnAttack = move.id;
			@currentMove = move.id;
		} else {
			@effects.TwoTurnAttack = null;   // Cancel use of two-turn attack
		}
		// Add to counters for moves which increase them when used in succession
		move.ChangeUsageCounters(self, specialUsage);
		// Charge up Metronome item
		if (hasActiveItem(Items.METRONOME) && !move.callsAnotherMove()) {
			if (@lastMoveUsed && @lastMoveUsed == move.id && !@lastMoveFailed) {
				@effects.Metronome += 1;
			} else {
				@effects.Metronome = 0;
			}
		}
		// Record move as having been used
		@lastMoveUsed     = move.id;
		@lastMoveUsedType = move.calcType;   // For Conversion 2
		if (@pokemon.isSpecies(Speciess.PRIMEAPE) && @lastMoveUsed == :RAGEFIST) {
			@pokemon.evolution_counter += 1;
		}
		if (!specialUsage) {
			@lastRegularMoveUsed   = move.id;   // For Disable, Encore, Instruct, Mimic, Mirror Move, Sketch, Spite
			@lastRegularMoveTarget = choice[3];   // For Instruct (remembering original target is fine)
			if (!@movesUsed.Contains(move.id)) @movesUsed.Add(move.id);   // For Last Resort
		}
		@battle.lastMoveUsed = move.id;   // For Copycat
		@battle.lastMoveUser = @index;   // For "self KO" battle clause to avoid draws
		@battle.successStates[@index].useState = 1;   // Battle Arena - assume failure
		// Find the default user (self or Snatcher) and target(s)
		user = FindUser(choice, move);
		user = ChangeUser(choice, move, user);
		targets = FindTargets(choice, move, user);
		targets = ChangeTargets(move, user, targets);
		// Pressure
		if (!specialUsage) {
			foreach (var b in targets) { //'targets.each' do => |b|
				unless (b.opposes(user) && b.hasActiveAbility(Abilitys.PRESSURE)) continue;
				Debug.Log($"[Ability triggered] {b.ToString()}'s {b.abilityName}");
				user.ReducePP(move);
			}
			if (move.Target(user).affects_foe_side) {
				@battle.allOtherSideBattlers(user).each do |b|
					unless (b.hasActiveAbility(Abilitys.PRESSURE)) continue;
					Debug.Log($"[Ability triggered] {b.ToString()}'s {b.abilityName}");
					user.ReducePP(move);
				}
			}
		}
		// Dazzling/Queenly Majesty make the move fail here
		@battle.Priority(true).each do |b|
			if (!b || !b.abilityActive()) continue;
			if (Battle.AbilityEffects.triggerMoveBlocking(b.ability, b, user, targets, move, @battle)) {
				@battle.DisplayBrief(_INTL("{1} used {2}!", user.ToString(), move.name));
				@battle.ShowAbilitySplash(b);
				@battle.Display(_INTL("{1} cannot use {2}!", user.ToString(), move.name));
				@battle.HideAbilitySplash(b);
				user.lastMoveFailed = true;
				CancelMoves;
				EndTurn(choice);
				return;
			}
		}
		// "X used Y!" message
		// Can be different for Bide, Fling, Focus Punch and Future Sight
		// NOTE: This intentionally passes self rather than user. The user is always
		//       self except if Snatched, but this message should state the original
		//       user (self) even if the move is Snatched.
		move.DisplayUseMessage(self);
		// Snatch's message (user is the new user, self is the original user)
		if (move.snatched) {
			@lastMoveFailed = true;   // Intentionally applies to self, not user
			@battle.Display(_INTL("{1} snatched {2}'s move!", user.ToString(), This(true)));
		}
		// "But it failed!" checks
		if (move.MoveFailed(user, targets)) {
			Debug.Log(string.Format("[Move failed] In function code {0}'s def MoveFailed?", move.function_code));
			user.lastMoveFailed = true;
			CancelMoves;
			EndTurn(choice);
			return;
		}
		// Perform set-up actions and display messages
		// Messages include Magnitude's number and Pledge moves' "it's a combo!"
		move.OnStartUse(user, targets);
		// Self-thawing due to the move
		if (user.status == statuses.FROZEN && move.thawsUser()) {
			user.CureStatus(false);
			@battle.Display(_INTL("{1} melted the ice!", user.ToString()));
		}
		// Powder
		if (user.effects.Powder && move.calcType == Types.FIRE) {
			@battle.CommonAnimation("Powder", user);
			@battle.Display(_INTL("When the flame touched the powder on the Pokémon, it exploded!"));
			user.lastMoveFailed = true;
			if (!new []{:Rain, :HeavyRain}.Contains(user.effectiveWeather) && user.takesIndirectDamage()) {
				user.TakeEffectDamage((int)Math.Round(user.totalhp / 4.0), false) do |hp_lost|
					@battle.Display(_INTL("{1} is hurt by Powder!", user.ToString()));
				}
				@battle.GainExp;   // In case user is KO'd by this
			}
			CancelMoves;
			EndTurn(choice);
			return;
		}
		// Primordial Sea, Desolate Land
		if (move.damagingMove()) {
			switch (@battle.Weather) {
				case :HeavyRain:
					if (move.calcType == Types.FIRE) {
						@battle.Display(_INTL("The Fire-type attack fizzled out in the heavy rain!"));
						user.lastMoveFailed = true;
						CancelMoves;
						EndTurn(choice);
						return;
					}
					break;
				case :HarshSun:
					if (move.calcType == Types.WATER) {
						@battle.Display(_INTL("The Water-type attack evaporated in the harsh sunlight!"));
						user.lastMoveFailed = true;
						CancelMoves;
						EndTurn(choice);
						return;
					}
					break;
			}
		}
		// Protean
		if (user.hasActiveAbility(new {:LIBERO, :PROTEAN}) &&
			!move.callsAnotherMove() && !move.snatched &&
			user.HasOtherType(move.calcType) && !GameData.Type.get(move.calcType).pseudo_type) {
			@battle.ShowAbilitySplash(user);
			user.ChangeTypes(move.calcType);
			typeName = GameData.Type.get(move.calcType).name;
			@battle.Display(_INTL("{1}'s type changed to {2}!", user.ToString(), typeName));
			@battle.HideAbilitySplash(user);
			// NOTE: The GF games say that if Curse is used by a non-Ghost-type
			//       Pokémon which becomes Ghost-type because of Protean, it should
			//       target and curse itself. I think this is silly, so I'm making it
			//       choose a random opponent to curse instead.
			if (move.function_code == "CurseTargetOrLowerUserSpd1RaiseUserAtkDef1" && targets.length == 0) {
				choice[3] = -1;
				targets = FindTargets(choice, move, user);
			}
		}
		// For two-turn moves when they charge and attack in the same turn
		move.QuickChargingMove(user, targets);
		//---------------------------------------------------------------------------
		magicCoater  = -1;
		magicBouncer = -1;
		if (targets.length == 0 && move.Target(user).num_targets > 0 && !move.worksWithNoTargets()) {
			// def FindTargets should have found a target(s), but it didn't because
			// they were all fainted
			// All target types except: None, User, UserSide, FoeSide, BothSides
			@battle.Display(_INTL("But there was no target..."));
			user.lastMoveFailed = true;
		} else {   // We have targets, or move doesn't use targets
			// Reset whole damage state, perform various success checks (not accuracy)
			@battle.allBattlers.each do |b|
				b.droppedBelowHalfHP = false;
				b.statsDropped = false;
			}
			foreach (var b in targets) { //'targets.each' do => |b|
				b.damageState.reset;
				if (SuccessCheckAgainstTarget(move, user, b, targets)) continue;
				b.damageState.unaffected = true;
			}
			// Magic Coat/Magic Bounce checks (for moves which don't target Pokémon)
			if (targets.length == 0 && move.statusMove() && move.canMagicCoat()) {
				@battle.Priority(true).each do |b|
					if (b.fainted() || !b.opposes(user)) continue;
					if (b.semiInvulnerable()) continue;
					if (b.effects.MagicCoat) {
						magicCoater = b.index;
						b.effects.MagicCoat = false;
						break;
					} else if (b.hasActiveAbility(Abilitys.MAGICBOUNCE) && !b.beingMoldBroken() &&
								!b.effects.MagicBounce) {
						magicBouncer = b.index;
						b.effects.MagicBounce = true;
						break;
					}
				}
			}
			// Get the number of hits
			numHits = move.NumHits(user, targets);
			// Process each hit in turn
			realNumHits = 0;
			for (int i = numHits; i < numHits; i++) { //for 'numHits' times do => |i|
				if (magicCoater >= 0 || magicBouncer >= 0) break;
				success = ProcessMoveHit(move, user, targets, i, skipAccuracyCheck);
				if (!success) {
					if (i == 0 && targets.length > 0) {
						hasFailed = false;
						foreach (var t in targets) { //'targets.each' do => |t|
							if (t.damageState.protected) continue;
							hasFailed = t.damageState.unaffected;
							if (!t.damageState.unaffected) break;
						}
						user.lastMoveFailed = hasFailed;
					}
					break;
				}
				realNumHits += 1;
				if (user.fainted()) break;
				if (new []{:SLEEP, :FROZEN}.Contains(user.status)) break;
				// NOTE: If a multi-hit move becomes disabled partway through doing those
				//       hits (e.g. by Cursed Body), the rest of the hits continue as
				//       normal.
				if (targets.none(t => !t.fainted())) break;   // All targets are fainted
			}
			// Battle Arena only - attack is successful
			@battle.successStates[user.index].useState = 2;
			if (targets.length > 0) {
				@battle.successStates[user.index].typeMod = 0;
				foreach (var b in targets) { //'targets.each' do => |b|
					if (b.damageState.unaffected) continue;
					@battle.successStates[user.index].typeMod += b.damageState.typeMod;
				}
			}
			// Effectiveness message for multi-hit moves
			// NOTE: No move is both multi-hit and multi-target, and the messages below
			//       aren't quite right for such a hypothetical move.
			if (numHits > 1) {
				if (move.damagingMove()) {
					foreach (var b in targets) { //'targets.each' do => |b|
						if (b.damageState.unaffected || b.damageState.substitute) continue;
						move.EffectivenessMessage(user, b, targets.length);
					}
				}
				if (realNumHits == 1) {
					@battle.Display(_INTL("Hit 1 time!"));
				} else if (realNumHits > 1) {
					@battle.Display(_INTL("Hit {1} times!", realNumHits));
				}
			}
			// Magic Coat's bouncing back (move has targets)
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.fainted()) continue;
				if (!b.damageState.magicCoat && !b.damageState.magicBounce) continue;
				if (b.damageState.magicBounce) @battle.ShowAbilitySplash(b);
				@battle.Display(_INTL("{1} bounced the {2} back!", b.ToString(), move.name));
				if (b.damageState.magicBounce) @battle.HideAbilitySplash(b);
				newChoice = choice.clone;
				newChoice[3] = user.index;
				newTargets = FindTargets(newChoice, move, b);
				newTargets = ChangeTargets(move, b, newTargets);
				success = false;
				if (!move.MoveFailed(b, newTargets)) {
					newTargets.each_with_index do |newTarget, idx|
						if (SuccessCheckAgainstTarget(move, b, newTarget, newTargets)) {
							success = true;
							continue;
						}
						newTargets[idx] = null;
					}
					newTargets.compact!;
				}
				if (success) ProcessMoveHit(move, b, newTargets, 0, false);
				if (!success) b.lastMoveFailed = true;
				targets.each(otherB => { if (otherB&.fainted()) otherB.Faint; });
				if (user.fainted()) user.Faint;
			}
			// Magic Coat's bouncing back (move has no targets)
			if (magicCoater >= 0 || magicBouncer >= 0) {
				mc = @battle.battlers[(magicCoater >= 0) ? magicCoater : magicBouncer];
				if (!mc.fainted()) {
					user.lastMoveFailed = true;
					if (magicBouncer >= 0) @battle.ShowAbilitySplash(mc);
					@battle.Display(_INTL("{1} bounced the {2} back!", mc.ToString(), move.name));
					if (magicBouncer >= 0) @battle.HideAbilitySplash(mc);
					success = false;
					if (!move.MoveFailed(mc, new List<string>())) {
						success = ProcessMoveHit(move, mc, new List<string>(), 0, false);
					}
					if (!success) mc.lastMoveFailed = true;
					targets.each(b => { if (b&.fainted()) b.Faint; });
					if (user.fainted()) user.Faint;
				}
			}
			// Move-specific effects after all hits
			targets.each(b => move.EffectAfterAllHits(user, b));
			// Faint if 0 HP
			targets.each(b => { if (b&.fainted()) b.Faint; });
			if (user.fainted()) user.Faint;
			// External/general effects after all hits. Eject Button, Shell Bell, etc.
			EffectsAfterMove(user, targets, move, realNumHits);
			@battle.allBattlers.each do |b|
				b.droppedBelowHalfHP = false;
				b.statsDropped = false;
			}
		}
		// End effect of Mold Breaker
		@battle.moldBreaker = false;
		// Gain Exp
		@battle.GainExp;
		// Battle Arena only - update skills
		@battle.allBattlers.each(b => @battle.successStates[b.index].updateSkill);
		// Shadow Pokémon triggering Hyper Mode
		if (@battle.choices[@index][0] != :None) HyperMode;   // Not if self is replaced
		// End of move usage
		EndTurn(choice);
		// Instruct
		@battle.allBattlers.each do |b|
			if (!b.effects.Instruct || !b.lastMoveUsed) continue;
			b.effects.Instruct = false;
			idxMove = -1;
			b.eachMoveWithIndex((m, i) => { if (m.id == b.lastMoveUsed) idxMove = i; });
			if (idxMove < 0) continue;
			oldLastRoundMoved = b.lastRoundMoved;
			@battle.Display(_INTL("{1} used the move instructed by {2}!", b.ToString(), user.ToString(true)));
			b.effects.Instructed = true;
			if (b.CanChooseMove(b.moves[idxMove], false)) {
				Debug.logonerr do;
					b.UseMoveSimple(b.lastMoveUsed, b.lastRegularMoveTarget, idxMove, false);
				}
				b.lastRoundMoved = oldLastRoundMoved;
				@battle.Judge;
				if (@battle.decided()) return;
			}
			b.effects.Instructed = false;
		}
		// Dancer
		if (!@effects.Dancer && !user.lastMoveFailed && realNumHits > 0 &&
			!move.snatched && magicCoater < 0 && @battle.CheckGlobalAbility(Abilities.DANCER) &&
			move.danceMove()) {
			dancers = new List<string>();
			@battle.Priority(true).each do |b|
				if (b.index != user.index && b.hasActiveAbility(Abilitys.DANCER)) dancers.Add(b);
			}
			while (dancers.length > 0) {
				nextUser = dancers.pop;
				oldLastRoundMoved = nextUser.lastRoundMoved;
				// NOTE: Petal Dance being used because of Dancer shouldn't lock the
				//       Dancer into using that move, and shouldn't contribute to its
				//       turn counter if it's already locked into Petal Dance.
				oldOutrage = nextUser.effects.Outrage;
				if (nextUser.effects.Outrage > 0) nextUser.effects.Outrage += 1;
				oldCurrentMove = nextUser.currentMove;
				preTarget = choice[3];
				if (nextUser.opposes(user) || !nextUser.opposes(preTarget)) preTarget = user.index;
				@battle.ShowAbilitySplash(nextUser, true);
				@battle.HideAbilitySplash(nextUser);
				if (!Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} kept the dance going with {2}!",
																	nextUser.ToString(), nextUser.abilityName));
				}
				nextUser.effects.Dancer = true;
				if (nextUser.CanChooseMove(move, false)) {
					Debug.logonerr(() => nextUser.UseMoveSimple(move.id, preTarget));
					nextUser.lastRoundMoved = oldLastRoundMoved;
					nextUser.effects.Outrage = oldOutrage;
					nextUser.currentMove = oldCurrentMove;
					@battle.Judge;
					if (@battle.decided()) return;
				}
				nextUser.effects.Dancer = false;
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Attack a single target.
	//-----------------------------------------------------------------------------

	public void ProcessMoveHit(move, user, targets, hitNum, skipAccuracyCheck) {
		if (user.fainted()) return false;
		// For two-turn attacks being used in a single turn
		move.InitialEffect(user, targets, hitNum);
		numTargets = 0;   // Number of targets that are affected by this hit
		// Count a hit for Parental Bond (if it applies)
		if (user.effects.ParentalBond > 0) user.effects.ParentalBond -= 1;
		// Accuracy check (accuracy/evasion calc)
		if (hitNum == 0 || move.successCheckPerHit()) {
			foreach (var b in targets) { //'targets.each' do => |b|
				b.damageState.missed = false;
				if (b.damageState.unaffected) continue;
				if (SuccessCheckPerHit(move, user, b, skipAccuracyCheck)) {
					numTargets += 1;
				} else {
					b.damageState.missed     = true;
					b.damageState.unaffected = true;
				}
			}
			// If failed against all targets
			if (targets.length > 0 && numTargets == 0 && !move.worksWithNoTargets()) {
				foreach (var b in targets) { //'targets.each' do => |b|
					if (!b.damageState.missed || b.damageState.magicCoat) continue;
					MissMessage(move, user, b);
					if (user.itemActive()) {
						Battle.ItemEffects.triggerOnMissingTarget(user.item, user, b, move, hitNum, @battle);
					}
					if (move.RepeatHit()) break;   // Dragon Darts only shows one failure message
				}
				move.CrashDamage(user);
				user.ItemHPHealCheck;
				CancelMoves;
				return false;
			}
		}
		// If we get here, this hit will happen and do something
		all_targets = targets;
		targets = move.DesignateTargetsForHit(targets, hitNum);   // For Dragon Darts
		targets.each(b => b.damageState.resetPerHit);
		//---------------------------------------------------------------------------
		// Calculate damage to deal
		if (move.DamagingMove()) {
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected) continue;
				// Check whether Substitute/Disguise will absorb the damage
				move.CheckDamageAbsorption(user, b);
				// Calculate the damage against b
				// CalcDamage shows the "eat berry" animation for SE-weakening
				// berries, although the message about it comes after the additional
				// effect below
				move.CalcDamage(user, b, targets.length);   // Stored in damageState.calcDamage
				// Lessen damage dealt because of False Swipe/Endure/etc.
				move.ReduceDamage(user, b);   // Stored in damageState.hpLost
			}
		}
		// Show move animation (for this hit)
		move.ShowAnimation(move.id, user, targets, hitNum);
		// Type-boosting Gem consume animation/message
		if (user.effects.GemConsumed && hitNum == 0) {
			// NOTE: The consume animation and message for Gems are shown now, but the
			//       actual removal of the item happens in def EffectsAfterMove.
			@battle.CommonAnimation("UseItem", user);
			@battle.Display(_INTL("The {1} strengthened {2}'s power!",
															GameData.Item.get(user.effects.GemConsumed).name, move.name));
		}
		// Messages about missed target(s) (relevant for multi-target moves only)
		if (!move.RepeatHit()) {
			foreach (var b in targets) { //'targets.each' do => |b|
				if (!b.damageState.missed) continue;
				MissMessage(move, user, b);
				if (user.itemActive()) {
					Battle.ItemEffects.triggerOnMissingTarget(user.item, user, b, move, hitNum, @battle);
				}
			}
		}
		// Deal the damage (to all allies first simultaneously, then all foes
		// simultaneously)
		if (move.DamagingMove()) {
			// This just changes the HP amounts and does nothing else
			targets.each(b => { if (!b.damageState.unaffected) move.InflictHPDamage(b); });
			// Animate the hit flashing and HP bar changes
			move.AnimateHitAndHPLost(user, targets);
		}
		// Self-Destruct/Explosion's damaging and fainting of user
		if (hitNum == 0) move.SelfKO(user);
		if (user.fainted()) user.Faint;
		if (move.DamagingMove()) {
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected) continue;
				// NOTE: This method is also used for the OHKO special message.
				move.HitEffectivenessMessages(user, b, targets.length);
				// Record data about the hit for various effects' purposes
				move.RecordDamageLost(user, b);
			}
			// Close Combat/Superpower's stat-lowering, Flame Burst's splash damage,
			// and Incinerate's berry destruction
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected) continue;
				move.EffectWhenDealingDamage(user, b);
			}
			// Ability/item effects such as Static/Rocky Helmet, and Grudge, etc.
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected) continue;
				EffectsOnMakingHit(move, user, b);
			}
			// Disguise/Endure/Sturdy/Focus Sash/Focus Band messages
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected) continue;
				move.EndureKOMessage(b);
			}
			// HP-healing held items (checks all battlers rather than just targets
			// because Flame Burst's splash damage affects non-targets)
			@battle.Priority(true).each do |b|
				if (move.preventsBattlerConsumingHealingBerry(b, targets)) continue;
				b.ItemHPHealCheck;
			}
			// Animate battlers fainting (checks all battlers rather than just targets
			// because Flame Burst's splash damage affects non-targets)
			@battle.Priority(true).each(b => { if (b&.fainted()) b.Faint; });
		}
		@battle.JudgeCheckpoint(user, move);
		// Main effect (recoil/drain, etc.)
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.damageState.unaffected) continue;
			move.EffectAgainstTarget(user, b);
		}
		move.EffectGeneral(user);
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b&.fainted()) continue;
			b.Faint;
			if (user.pokemon.isSpecies(Speciess.BISHARP) && b.isSpecies(Speciess.BISHARP) && b.item == items.LEADERSCREST) {
				user.pokemon.evolution_counter += 1;
			}
		}
		if (user.fainted()) user.Faint;
		// Additional effect
		if (!user.hasActiveAbility(Abilitys.SHEERFORCE)) {
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.calcDamage == 0) continue;
				chance = move.AdditionalEffectChance(user, b);
				if (chance <= 0) continue;
				if (@battle.Random(100) < chance) move.AdditionalEffect(user, b);
			}
		}
		// Make the target flinch (because of an item/ability)
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.fainted()) continue;
			if (b.damageState.calcDamage == 0 || b.damageState.substitute) continue;
			chance = move.FlinchChance(user, b);
			if (chance <= 0) continue;
			if (@battle.Random(100) < chance) {
				Debug.Log($"[Item/ability triggered] {user.ToString()}'s King's Rock/Razor Fang or Stench");
				b.Flinch(user);
			}
		}
		// Message for and consuming of type-weakening berries
		// NOTE: The "consume held item" animation for type-weakening berries occurs
		//       during CalcDamage above (before the move's animation), but the
		//       message about it only shows here.
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.damageState.unaffected) continue;
			if (!b.damageState.berryWeakened) continue;
			b.damageState.berryWeakened = false;   // Weakening only applies for one hit
			@battle.Display(_INTL("The {1} weakened the damage to {2}!", b.itemName, b.ToString(true)));
			b.ConsumeItem;
		}
		// Steam Engine (goes here because it should be after stat changes caused by
		// the move)
		if (new []{:FIRE, :WATER}.Contains(move.calcType)) {
			foreach (var b in targets) { //'targets.each' do => |b|
				if (b.damageState.unaffected) continue;
				if (b.damageState.calcDamage == 0 || b.damageState.substitute) continue;
				if (!b.hasActiveAbility(Abilitys.STEAMENGINE)) continue;
				if (b.CanRaiseStatStage(:SPEED, b)) b.RaiseStatStageByAbility(:SPEED, 6, b);
			}
		}
		// Fainting
		targets.each(b => { if (b&.fainted()) b.Faint; });
		if (user.fainted()) user.Faint;
		// Dragon Darts' second half of attack
		if (move.RepeatHit() && hitNum == 0 &&
			targets.any(b => !b.fainted() && !b.damageState.unaffected)) {
			ProcessMoveHit(move, user, all_targets, 1, skipAccuracyCheck);
		}
		return true;
	}
}
