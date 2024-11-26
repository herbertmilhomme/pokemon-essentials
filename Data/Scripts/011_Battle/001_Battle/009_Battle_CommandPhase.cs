//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Clear commands.
	//-----------------------------------------------------------------------------

	public void ClearChoice(idxBattler) {
		if (!@choices[idxBattler]) @choices[idxBattler] = new List<string>();
		@choices[idxBattler].Action = :None;
		@choices[idxBattler].Index = 0;
		@choices[idxBattler].Move = null;
		@choices[idxBattler].Target = -1;
	}

	public void CancelChoice(idxBattler) {
		// If idxBattler's choice was to use an item, return that item to the Bag
		if (@choices[idxBattler].Action == :UseItem) {
			item = @choices[idxBattler].Index;
			if (item) ReturnUnusedItemToBag(item, idxBattler);
		}
		// If idxBattler chose to Mega Evolve, cancel it
		UnregisterMegaEvolution(idxBattler);
		// Clear idxBattler's choice
		ClearChoice(idxBattler);
	}

	//-----------------------------------------------------------------------------
	// Use main command menu (Fight/Pokémon/Bag/Run).
	//-----------------------------------------------------------------------------

	public void CommandMenu(idxBattler, firstAction) {
		return @scene.CommandMenu(idxBattler, firstAction);
	}

	//-----------------------------------------------------------------------------
	// Check whether actions can be taken.
	//-----------------------------------------------------------------------------

	public bool CanShowCommands(idxBattler) {
		battler = @battlers[idxBattler];
		if (!battler || battler.fainted()) return false;
		if (battler.usingMultiTurnAttack()) return false;
		return true;
	}

	public bool CanShowFightMenu(idxBattler) {
		battler = @battlers[idxBattler];
		// Encore
		if (battler.effects.Encore > 0) return false;
		// No moves that can be chosen (will Struggle instead)
		usable = false;
		battler.eachMoveWithIndex do |_m, i|
			if (!CanChooseMove(idxBattler, i, false)) continue;
			usable = true;
			break;
		}
		return usable;
	}

	//-----------------------------------------------------------------------------
	// Use sub-menus to choose an action, and register it if is allowed.
	//-----------------------------------------------------------------------------

	// Returns true if a choice was made, false if cancelled.
	public void FightMenu(idxBattler) {
		// Auto-use Encored move or no moves choosable, so auto-use Struggle
		if (!CanShowFightMenu(idxBattler)) return AutoChooseMove(idxBattler);
		// Battle Palace only
		if (AutoFightMenu(idxBattler)) return true;
		// Regular move selection
		ret = false;
		@scene.FightMenu(idxBattler, CanMegaEvolve(idxBattler)) do |cmd|
			switch (cmd) {
				case -1:   // Cancel
					break;
				case -2:   // Toggle Mega Evolution
					ToggleRegisteredMegaEvolution(idxBattler);
					next false;
					break;
				case -3:   // Shift
					UnregisterMegaEvolution(idxBattler);
					RegisterShift(idxBattler);
					ret = true;
					break;
				default:      // Chose a move to use
					if (cmd < 0 || !@battlers[idxBattler].moves[cmd] ||
												!@battlers[idxBattler].moves[cmd].id) next false;
					if (!RegisterMove(idxBattler, cmd)) next false;
					if (!singleBattle() &&
												!ChooseTarget(@battlers[idxBattler], @battlers[idxBattler].moves[cmd])) next false;
					ret = true;
					break;
			}
			next true;
		}
		return ret;
	}

	public void AutoFightMenu(idxBattler) {return false; }

	public void ChooseTarget(battler, move) {
		target_data = move.Target(battler);
		idxTarget = @scene.ChooseTarget(battler.index, target_data);
		if (idxTarget < 0) return false;
		RegisterTarget(battler.index, idxTarget);
		return true;
	}

	public void ItemMenu(idxBattler, firstAction) {
		if (!@internalBattle) {
			Display(_INTL("Items can't be used here."));
			return false;
		}
		ret = false;
		@scene.ItemMenu(idxBattler, firstAction) do |item, useType, idxPkmn, idxMove, itemScene|
			if (!item) next false;
			battler = pkmn = null;
			switch (useType) {
				case 1: case 2:   // Use on Pokémon/Pokémon's move
					if (!ItemHandlers.hasBattleUseOnPokemon(item)) next false;
					battler = FindBattler(idxPkmn, idxBattler);
					pkmn    = Party(idxBattler)[idxPkmn];
					if (!CanUseItemOnPokemon(item, pkmn, battler, itemScene)) next false;
					break;
				case 3:   // Use on battler
					if (!ItemHandlers.hasBattleUseOnBattler(item)) next false;
					battler = FindBattler(idxPkmn, idxBattler);
					if (battler) pkmn    = battler.pokemon;
					if (!CanUseItemOnPokemon(item, pkmn, battler, itemScene)) next false;
					break;
				case 4:   // Poké Balls
					if (idxPkmn < 0) next false;
					battler = @battlers[idxPkmn];
					if (battler) pkmn    = battler.pokemon;
					break;
				case 5:   // No target (Poké Doll, Guard Spec., Launcher items)
					battler = @battlers[idxBattler];
					if (battler) pkmn    = battler.pokemon;
					break;
				default:
					next false;
					break;
			}
			if (!pkmn) next false;
			if ((!ItemHandlers.triggerCanUseInBattle(item, pkmn, battler, idxMove,
																												firstAction, self, itemScene)) next false;
			if (!RegisterItem(idxBattler, item, idxPkmn, idxMove)) next false;
			ret = true;
			next true;
		}
		return ret;
	}

	public void PartyMenu(idxBattler) {
		ret = -1;
		if (@debug) {
			ret = @battleAI.DefaultChooseNewEnemy(idxBattler);
		} else {
			ret = PartyScreen(idxBattler, false, true, true);
		}
		return ret >= 0;
	}

	public void RunMenu(idxBattler) {
		// Regardless of succeeding or failing to run, stop choosing actions
		return Run(idxBattler) != 0;
	}

	public void CallMenu(idxBattler) {
		return RegisterCall(idxBattler);
	}

	public void DebugMenu() {
		BattleDebug(self);
		@scene.RefreshEverything;
		allBattlers.each(b => b.CheckFormOnWeatherChange);
		EndPrimordialWeather;
		allBattlers.each(b => b.AbilityOnTerrainChange);
		foreach (var b in allBattlers) { //'allBattlers.each' do => |b|
			b.CheckFormOnMovesetChange;
			b.CheckFormOnStatusChange;
		}
	}

	//-----------------------------------------------------------------------------
	// Command phase.
	//-----------------------------------------------------------------------------

	public void CommandPhase() {
		@command_phase = true;
		@scene.BeginCommandPhase;
		// Reset choices if commands can be shown
		@battlers.each_with_index do |b, i|
			if (!b) continue;
			if (CanShowCommands(i)) ClearChoice(i);
		}
		// Reset choices to perform Mega Evolution if it wasn't done somehow
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			@megaEvolution[side].each_with_index do |megaEvo, i|
				if (megaEvo >= 0) @megaEvolution[side][i] = -1;
			}
		}
		// Choose actions for the round (player first, then AI)
		CommandPhaseLoop(true);    // Player chooses their actions
		if (decided()) {   // Battle ended, stop choosing actions
			@command_phase = false;
			return;
		}
		CommandPhaseLoop(false);   // AI chooses their actions
		@command_phase = false;
	}

	public void CommandPhaseLoop(isPlayer) {
		// NOTE: Doing some things (e.g. running, throwing a Poké Ball) takes up all
		//       your actions in a round.
		actioned = new List<string>();
		idxBattler = -1;
		do { //loop; while (true);
			if (decided()) break;   // Battle ended, stop choosing actions
			idxBattler += 1;
			if (idxBattler >= @battlers.length) break;
			if (!@battlers[idxBattler] || OwnedByPlayer(idxBattler) != isPlayer) continue;
			if (@choices[idxBattler].Action != :None || !CanShowCommands(idxBattler)) {
				// Action is forced, can't choose one
				Debug.Log($"[Command phase] {@battlers[idxBattler].ToString()} ({idxBattler}) is forced to use a multi-turn move");
				continue;
			}
			// AI controls this battler
			if (@controlPlayer || !OwnedByPlayer(idxBattler)) {
				@battleAI.DefaultChooseEnemyCommand(idxBattler);
				continue;
			}
			// Player chooses an action
			actioned.Add(idxBattler);
			commandsEnd = false;   // Whether to cancel choosing all other actions this round
			do { //loop; while (true);
				cmd = CommandMenu(idxBattler, actioned.length == 1);
				// If being Sky Dropped, can't do anything except use a move
				if (cmd > 0 && @battlers[idxBattler].effects.SkyDrop >= 0) {
					Display(_INTL("Sky Drop won't let {1} go!", @battlers[idxBattler].ToString(true)));
					continue;
				}
				switch (cmd) {
					case 0:    // Fight
						if (FightMenu(idxBattler)) break;
						break;
					case 1:    // Bag
						if (ItemMenu(idxBattler, actioned.length == 1)) {
							if (ItemUsesAllActions(@choices[idxBattler].Index)) commandsEnd = true;
							break;
						}
						break;
					case 2:    // Pokémon
						if (PartyMenu(idxBattler)) break;
						break;
					case 3:    // Run
						// NOTE: "Run" is only an available option for the first battler the
						//       player chooses an action for in a round. Attempting to run
						//       from battle prevents you from choosing any other actions in
						//       that round.
						if (RunMenu(idxBattler)) {
							commandsEnd = true;
							break;
						}
						break;
					case 4:    // Call
						if (CallMenu(idxBattler)) break;
						break;
					case -2:   // Debug
						DebugMenu;
						continue;
						break;
					case -1:   // Go back to previous battler's action choice
						if (actioned.length <= 1) continue;
						actioned.pop;   // Forget this battler was done
						idxBattler = actioned.last - 1;
						CancelChoice(idxBattler + 1);   // Clear the previous battler's choice
						actioned.pop;   // Forget the previous battler was done
						break;
						break;
				}
				CancelChoice(idxBattler);
			}
			if (commandsEnd) break;
		}
	}
}
