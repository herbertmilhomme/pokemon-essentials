//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Attack phase actions.
	//-----------------------------------------------------------------------------

	// Quick Claw, Custap Berry's "X let it move first!" message.
	public void AttackPhasePriorityChangeMessages() {
		foreach (var b in Priority) { //'Priority.each' do => |b|
			if (b.effects.PriorityAbility && b.abilityActive()) {
				Battle.AbilityEffects.triggerPriorityBracketUse(b.ability, b, self);
			} else if (b.effects.PriorityItem && b.itemActive()) {
				Battle.ItemEffects.triggerPriorityBracketUse(b.item, b, self);
			}
		}
	}

	public void AttackPhaseCall() {
		foreach (var b in Priority) { //'Priority.each' do => |b|
			unless (@choices[b.index].Action == :Call && !b.fainted()) continue;
			b.lastMoveFailed = false;   // Counts as a successful move for Stomping Tantrum
			Call(b.index);
		}
	}

	public void Pursuit(idxSwitcher) {
		@switching = true;
		foreach (var b in Priority) { //'Priority.each' do => |b|
			if (b.fainted() || !b.opposes(idxSwitcher)) continue;   // Shouldn't hit an ally
			if (b.movedThisRound() || !ChoseMoveFunctionCode(b.index, "PursueSwitchingFoe")) continue;
			// Check whether Pursuit can be used
			unless (MoveCanTarget(b.index, idxSwitcher, @choices[b.index].Move.Target(b))) continue;
			unless (CanChooseMove(b.index, @choices[b.index].Index, false)) continue;
			if (b.status == statuses.SLEEP || b.status == statuses.FROZEN) continue;
			if (b.effects.SkyDrop >= 0) continue;
			if (b.hasActiveAbility(Abilitys.TRUANT) && b.effects.Truant) continue;
			// Mega Evolve
			if (!b.wild()) {
				owner = GetOwnerIndexFromBattlerIndex(b.index);
				if (@megaEvolution[b.idxOwnSide][owner] == b.index) MegaEvolve(b.index);
			}
			// Use Pursuit
			@choices[b.index].Target = idxSwitcher;   // Change Pursuit's target
			b.ProcessTurn(@choices[b.index], false);
			if (decided() || @battlers[idxSwitcher].fainted()) break;
		}
		@switching = false;
	}

	public void AttackPhaseSwitch() {
		foreach (var b in Priority) { //'Priority.each' do => |b|
			unless (@choices[b.index].Action == :SwitchOut && !b.fainted()) continue;
			idxNewPkmn = @choices[b.index].Index;   // Party index of Pokémon to switch to
			b.lastMoveFailed = false;   // Counts as a successful move for Stomping Tantrum
			@lastMoveUser = b.index;
			// Switching message
			MessageOnRecall(b);
			// Pursuit interrupts switching
			Pursuit(b.index);
			if (decided()) return;
			// Switch Pokémon
			foreach (var b2 in allBattlers) { //'allBattlers.each' do => |b2|
				b2.droppedBelowHalfHP = false;
				b2.statsDropped = false;
			}
			RecallAndReplace(b.index, idxNewPkmn);
			OnBattlerEnteringBattle(b.index, true);
		}
	}

	public void AttackPhaseItems() {
		foreach (var b in Priority) { //'Priority.each' do => |b|
			unless (@choices[b.index].Action == :UseItem && !b.fainted()) continue;
			b.lastMoveFailed = false;   // Counts as a successful move for Stomping Tantrum
			item = @choices[b.index].Index;
			if (!item) continue;
			switch (GameData.Item.get(item).battle_use) {
				case 1: case 2:   // Use on Pokémon/Pokémon's move
					if (@choices[b.index].Move >= 0) UseItemOnPokemon(item, @choices[b.index].Move, b);
					break;
				case 3:      // Use on battler
					UseItemOnBattler(item, @choices[b.index].Move, b);
					break;
				case 4:      // Use Poké Ball
					UsePokeBallInBattle(item, @choices[b.index].Move, b);
					break;
				case 5:      // Use directly
					UseItemInBattle(item, @choices[b.index].Move, b);
					break;
				default:
					continue;
					break;
			}
			if (decided()) return;
		}
		if (Settings.RECALCULATE_TURN_ORDER_AFTER_SPEED_CHANGES) CalculatePriority;
	}

	public void AttackPhaseMegaEvolution() {
		foreach (var b in Priority) { //'Priority.each' do => |b|
			if (b.wild()) continue;
			unless (@choices[b.index].Action == :UseMove && !b.fainted()) continue;
			owner = GetOwnerIndexFromBattlerIndex(b.index);
			if (@megaEvolution[b.idxOwnSide][owner] != b.index) continue;
			MegaEvolve(b.index);
		}
	}

	public void AttackPhaseMoves() {
		// Show charging messages (Focus Punch)
		foreach (var b in Priority) { //'Priority.each' do => |b|
			unless (@choices[b.index].Action == :UseMove && !b.fainted()) continue;
			if (b.movedThisRound()) continue;
			@choices[b.index].Move.DisplayChargeMessage(b);
		}
		// Main move processing loop
		do { //loop; while (true);
			priority = Priority;
			// Forced to go next
			advance = false;
			foreach (var b in priority) { //'priority.each' do => |b|
				unless (b.effects.MoveNext && !b.fainted()) continue;
				unless (@choices[b.index].Action == :UseMove || @choices[b.index].Action == :Shift) continue;
				if (b.movedThisRound()) continue;
				advance = b.ProcessTurn(@choices[b.index]);
				if (advance) break;
			}
			if (decided()) return;
			if (advance) continue;
			// Regular priority order
			foreach (var b in priority) { //'priority.each' do => |b|
				if (b.effects.Quash > 0 || b.fainted()) continue;
				unless (@choices[b.index].Action == :UseMove || @choices[b.index].Action == :Shift) continue;
				if (b.movedThisRound()) continue;
				advance = b.ProcessTurn(@choices[b.index]);
				if (advance) break;
			}
			if (decided()) return;
			if (advance) continue;
			// Quashed
			if (Settings.MECHANICS_GENERATION >= 8) {
				foreach (var b in priority) { //'priority.each' do => |b|
					unless (b.effects.Quash > 0 && !b.fainted()) continue;
					unless (@choices[b.index].Action == :UseMove || @choices[b.index].Action == :Shift) continue;
					if (b.movedThisRound()) continue;
					advance = b.ProcessTurn(@choices[b.index]);
					if (advance) break;
				}
			} else {
				quashLevel = 0;
				do { //loop; while (true);
					quashLevel += 1;
					moreQuash = false;
					foreach (var b in priority) { //'priority.each' do => |b|
						if (b.effects.Quash > quashLevel) moreQuash = true;
						unless (b.effects.Quash == quashLevel && !b.fainted()) continue;
						unless (@choices[b.index].Action == :UseMove || @choices[b.index].Action == :Shift) continue;
						if (b.movedThisRound()) continue;
						advance = b.ProcessTurn(@choices[b.index]);
						break;
					}
					if (advance || !moreQuash) break;
				}
			}
			if (decided()) return;
			if (advance) continue;
			// Check for all done
			foreach (var b in priority) { //'priority.each' do => |b|
				if (b.fainted()) continue;
				if (b.movedThisRound() || !new []{:UseMove, :Shift}.Contains(@choices[b.index].Action)) continue;
				advance = true;
				break;
			}
			if (advance) continue;
			// All Pokémon have moved; } the loop
			break;
		}
	}

	//-----------------------------------------------------------------------------
	// Attack phase.
	//-----------------------------------------------------------------------------

	public void AttackPhase() {
		@scene.BeginAttackPhase;
		// Reset certain effects
		@battlers.each_with_index do |b, i|
			if (!b) continue;
			if (!b.fainted()) b.turnCount += 1;
			@successStates[i].clear;
			if (@choices[i].Action != :UseMove && @choices[i].Action != :Shift && @choices[i].Action != :SwitchOut) {
				b.effects.DestinyBond = false;
				b.effects.Grudge      = false;
			}
			if (!ChoseMoveFunctionCode(i, "StartRaiseUserAtk1WhenDamaged")) b.effects.Rage = false;
		}
		// Calculate move order for this round
		CalculatePriority(true);
		Debug.Log($"");
		// Perform actions
		AttackPhasePriorityChangeMessages;
		AttackPhaseCall;
		AttackPhaseSwitch;
		if (decided()) return;
		AttackPhaseItems;
		if (decided()) return;
		AttackPhaseMegaEvolution;
		AttackPhaseMoves;
	}
}
