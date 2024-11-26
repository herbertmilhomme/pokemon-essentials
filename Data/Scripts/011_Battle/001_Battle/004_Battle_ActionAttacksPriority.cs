//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Choosing a move/target.
	//-----------------------------------------------------------------------------

	public bool CanChooseMove(idxBattler, idxMove, showMessages, sleepTalk = false) {
		battler = @battlers[idxBattler];
		move = battler.moves[idxMove];
		unless (move) return false;
		if (move.pp == 0 && move.total_pp > 0 && !sleepTalk) {
			if (showMessages) DisplayPaused(_INTL("There's no PP left for this move!"));
			return false;
		}
		if (battler.effects.Encore > 0) {
			idxEncoredMove = battler.EncoredMoveIndex;
			if (idxEncoredMove >= 0 && idxMove != idxEncoredMove) return false;
		}
		return battler.CanChooseMove(move, true, showMessages, sleepTalk);
	}

	public bool CanChooseAnyMove(idxBattler, sleepTalk = false) {
		battler = @battlers[idxBattler];
		battler.eachMoveWithIndex do |m, i|
			if (m.pp == 0 && m.total_pp > 0 && !sleepTalk) continue;
			if (battler.effects.Encore > 0) {
				idxEncoredMove = battler.EncoredMoveIndex;
				if (idxEncoredMove >= 0 && i != idxEncoredMove) continue;
			}
			if (!battler.CanChooseMove(m, true, false, sleepTalk)) continue;
			return true;
		}
		return false;
	}

	// Called when the Pokémon is Encored, or if it can't use any of its moves.
	// Makes the Pokémon use the Encored move (if Encored), or Struggle.
	public void AutoChooseMove(idxBattler, showMessages = true) {
		battler = @battlers[idxBattler];
		if (battler.fainted()) {
			ClearChoice(idxBattler);
			return true;
		}
		// Encore
		idxEncoredMove = battler.EncoredMoveIndex;
		if (idxEncoredMove >= 0 && CanChooseMove(idxBattler, idxEncoredMove, false)) {
			encoreMove = battler.moves[idxEncoredMove];
			@choices[idxBattler].Action = :UseMove;         // "Use move"
			@choices[idxBattler].Index = idxEncoredMove;   // Index of move to be used
			@choices[idxBattler].Move = encoreMove;       // Battle.Move object
			@choices[idxBattler].Target = -1;               // No target chosen yet
			if (singleBattle()) return true;
			if (OwnedByPlayer(idxBattler)) {
				if (showMessages) {
					DisplayPaused(_INTL("{1} has to use {2}!", battler.name, encoreMove.name));
				}
				return ChooseTarget(battler, encoreMove);
			}
			return true;
		}
		// Struggle
		if (OwnedByPlayer(idxBattler) && showMessages) {
			DisplayPaused(_INTL("{1} has no moves left!", battler.name));
		}
		@choices[idxBattler].Action = :UseMove;    // "Use move"
		@choices[idxBattler].Index = -1;          // Index of move to be used
		@choices[idxBattler].Move = @struggle;   // Struggle Battle.Move object
		@choices[idxBattler].Target = -1;          // No target chosen yet
		return true;
	}

	public void RegisterMove(idxBattler, idxMove, showMessages = true) {
		battler = @battlers[idxBattler];
		move = battler.moves[idxMove];
		if (!CanChooseMove(idxBattler, idxMove, showMessages)) return false;
		@choices[idxBattler].Action = :UseMove;   // "Use move"
		@choices[idxBattler].Index = idxMove;    // Index of move to be used
		@choices[idxBattler].Move = move;       // Battle.Move object
		@choices[idxBattler].Target = -1;         // No target chosen yet
		return true;
	}

	public bool ChoseMove(idxBattler, moveID) {
		if (!@battlers[idxBattler] || @battlers[idxBattler].fainted()) return false;
		if (@choices[idxBattler].Action == :UseMove && @choices[idxBattler].Index) {
			return @choices[idxBattler].Move.id == moveID;
		}
		return false;
	}

	public bool ChoseMoveFunctionCode(idxBattler, code) {
		if (@battlers[idxBattler].fainted()) return false;
		if (@choices[idxBattler].Action == :UseMove && @choices[idxBattler].Index) {
			return @choices[idxBattler].Move.function_code == code;
		}
		return false;
	}

	public void RegisterTarget(idxBattler, idxTarget) {
		@choices[idxBattler].Target = idxTarget;   // Set target of move
	}

	// Returns whether the idxTarget will be targeted by a move with target_data
	// used by a battler in idxUser.
	public bool MoveCanTarget(idxUser, idxTarget, target_data) {
		if (target_data.num_targets == 0) return false;
		switch (target_data.id) {
			case :NearAlly:
				if (opposes(idxUser, idxTarget)) return false;
				if (!nearBattlers(idxUser, idxTarget)) return false;
			case :UserOrNearAlly:
				if (idxUser == idxTarget) return true;
				if (opposes(idxUser, idxTarget)) return false;
				if (!nearBattlers(idxUser, idxTarget)) return false;
			case :AllAllies:
				if (idxUser == idxTarget) return false;
				if (opposes(idxUser, idxTarget)) return false;
			case :UserAndAllies:
				if (opposes(idxUser, idxTarget)) return false;
			case :NearFoe: case :RandomNearFoe: case :AllNearFoes:
				if (!opposes(idxUser, idxTarget)) return false;
				if (!nearBattlers(idxUser, idxTarget)) return false;
			case :Foe:
				if (!opposes(idxUser, idxTarget)) return false;
			case :AllFoes:
				if (!opposes(idxUser, idxTarget)) return false;
			case :NearOther: case :AllNearOthers:
				if (!nearBattlers(idxUser, idxTarget)) return false;
			case :Other:
				if (idxUser == idxTarget) return false;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// Turn order calculation (priority).
	//-----------------------------------------------------------------------------
	public void CalculatePriority(fullCalc = false, indexArray = null) {
		needRearranging = false;
		if (fullCalc) {
			@priorityTrickRoom = (@field.effects.TrickRoom > 0);
			// Recalculate everything from scratch
			randomOrder = new Array(maxBattlerIndex + 1, i => { i; });
			for (int i = (randomOrder.length - 1); i < (randomOrder.length - 1); i++) { //for '(randomOrder.length - 1)' times do => |i|   // Can't use shuffle! here
				r = i + Random(randomOrder.length - i);
				randomOrder[i], randomOrder[r] = randomOrder[r], randomOrder[i];
			}
			@priority.clear;
			(0..maxBattlerIndex).each do |i|
				b = @battlers[i];
				if (!b) continue;
				entry = [b, b.Speed, 0, 0, 0, 0, randomOrder[i]];
				// new {battler, speed, sub-priority from ability, sub-priority from item,
				//  final sub-priority, priority, tie-breaker order}
				if (@choices[b.index].Action == :UseMove || @choices[b.index].Action == :Shift) {
					// Calculate move's priority
					if (@choices[b.index].Action == :UseMove) {
						move = @choices[b.index].Move;
						pri = move.Priority(b);
						if (b.abilityActive()) {
							pri = Battle.AbilityEffects.triggerPriorityChange(b.ability, b, move, pri);
						}
						entry[5] = pri;
						@choices[b.index][4] = pri;
					}
					// Calculate sub-priority changes (first/last within priority bracket)
					// Abilities (Stall)
					if (b.abilityActive()) {
						entry[2] = Battle.AbilityEffects.triggerPriorityBracketChange(b.ability, b, self);
					}
					// Items (Quick Claw, Custap Berry, Lagging Tail, Full Incense)
					if (b.itemActive()) {
						entry[3] = Battle.ItemEffects.triggerPriorityBracketChange(b.item, b, self);
					}
				}
				@priority.Add(entry);
			}
			needRearranging = true;
		} else {
			if ((@field.effects.TrickRoom > 0) != @priorityTrickRoom) {
				needRearranging = true;
				@priorityTrickRoom = (@field.effects.TrickRoom > 0);
			}
			// Recheck all battler speeds and changes to priority caused by abilities
			@priority.each do |entry|
				if (!entry) continue;
				if (indexArray && !indexArray.Contains(entry[0].index)) continue;
				// Recalculate speed of battler
				newSpeed = entry[0].Speed;
				if (newSpeed != entry[1]) needRearranging = true;
				entry[1] = newSpeed;
				// Recalculate move's priority in case ability has changed
				choice = @choices[entry[0].index];
				if (choice[0] == :UseMove) {
					move = choice[2];
					pri = move.Priority(entry[0]);
					if (entry[0].abilityActive()) {
						pri = Battle.AbilityEffects.triggerPriorityChange(entry[0].ability, entry[0], move, pri);
					}
					if (pri != entry[5]) needRearranging = true;
					entry[5] = pri;
					choice[4] = pri;
				}
				// NOTE: If the battler's ability at the start of this round was one with
				//       a PriorityBracketChange handler (i.e. Quick Draw), but it Mega
				//       Evolved and now doesn't have that ability, that old ability's
				//       priority bracket modifier will still apply. Similarly, if its
				//       old ability did not have a PriorityBracketChange handler but it
				//       Mega Evolved and now does have it, it will not apply this round.
				//       This is because the message saying that it had an effect appears
				//       before Mega Evolution happens, and recalculating it now would
				//       make that message inaccurate because Quick Draw only has a
				//       chance of triggering. However, since Quick Draw is exclusive to
				//       a species that doesn't Mega Evolve, these circumstances should
				//       never arise and no one will notice that the priority bracket
				//       change isn't recalculated when technically it should be.
			}
		}
		// Calculate each battler's overall sub-priority, and whether its ability or
		// item is responsible
		// NOTE: Going fast beats going slow. A Pokémon with Stall and Quick Claw
		//       will go first in its priority bracket if Quick Claw triggers,
		//       regardless of Stall.
		@priority.each do |entry|
			entry[0].effects.PriorityAbility = false;
			entry[0].effects.PriorityItem = false;
			subpri = entry[2];   // Sub-priority from ability
			if ((subpri == 0 && entry[3] != 0) ||   // Ability has no effect, item has effect
				(subpri < 0 && entry[3] >= 1)) {   // Ability makes it slower, item makes it faster
				subpri = entry[3];   // Sub-priority from item
				entry[0].effects.PriorityItem = true;
			} else if (subpri != 0) {   // Ability has effect, item had superfluous/no effect
				entry[0].effects.PriorityAbility = true;
			}
			entry[4] = subpri;   // Final sub-priority
		}
		// Reorder the priority array
		if (needRearranging) {
			@priority.sort! do |a, b|
				if (a[5] != b[5]) {
					// Sort by priority (highest value first)
					b[5] <=> a[5];
				} else if (a[4] != b[4]) {
					// Sort by sub-priority (highest value first)
					b[4] <=> a[4];
				} else if (@priorityTrickRoom) {
					// Sort by speed (lowest first), and use tie-breaker if necessary
					(a[1] == b[1]) ? b[6] <=> a[6] : a[1] <=> b[1]
				} else {
					// Sort by speed (highest first), and use tie-breaker if necessary
					(a[1] == b[1]) ? b[6] <=> a[6] : b[1] <=> a[1]
				}
			}
			// Write the priority order to the debug log
			if (fullCalc && Core.INTERNAL) {
				logMsg = "[Round order] ";
				@priority.each_with_index do |entry, i|
					if (i > 0) logMsg += ", ";
					logMsg += $"{entry[0].ToString(i > 0)} ({entry[0].index})";
				}
				Debug.Log(logMsg);
			}
		}
	}

	public void Priority(onlySpeedSort = false) {
		ret = new List<string>();
		if (onlySpeedSort) {
			// Sort battlers by their speed stats and tie-breaker order only.
			tempArray = new List<string>();
			@priority.each { |pArray| tempArray.Add(new {pArray[0], pArray[1], pArray[6]}) };
			tempArray.sort! { |a, b| (a[1] == b[1]) ? b[2] <=> a[2] : b[1] <=> a[1] };
			tempArray.each(tArray => ret.Add(tArray[0]));
		} else {
			// Sort battlers by priority, sub-priority and their speed. Ties are
			// resolved in the same way each time this method is called in a round.
			@priority.each(pArray => { if (!pArray[0].fainted()) ret.Add(pArray[0]); });
		}
		return ret;
	}
}
