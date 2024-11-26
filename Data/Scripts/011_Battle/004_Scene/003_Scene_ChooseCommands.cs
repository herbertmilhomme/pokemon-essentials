//===============================================================================
//
//===============================================================================
public partial class Battle.Scene {
	//-----------------------------------------------------------------------------
	// The player chooses a main command for a Pokémon.
	// Return values: -1=Cancel, 0=Fight, 1=Bag, 2=Pokémon, 3=Run, 4=Call
	//-----------------------------------------------------------------------------

	public void CommandMenu(idxBattler, firstAction) {
		shadowTrainer = (GameData.Types.exists(Types.SHADOW) && @battle.trainerBattle());
		cmds = new {
			_INTL("What will\n{1} do?", @battle.battlers[idxBattler].name),
			_INTL("Fight"),
			_INTL("Bag"),
			_INTL("Pokémon"),
			(shadowTrainer) ? _INTL("Call") : (firstAction) ? _INTL("Run") : _INTL("Cancel")
		}
		ret = CommandMenuEx(idxBattler, cmds, (shadowTrainer) ? 2 : (firstAction) ? 0 : 1);
		if (ret == 3 && shadowTrainer) ret = 4;   // Convert "Run" to "Call"
		if (ret == 3 && !firstAction) ret = -1;   // Convert "Run" to "Cancel"
		return ret;
	}

	// Mode: 0 = regular battle with "Run" (first choosable action in the round only)
	//       1 = regular battle with "Cancel"
	//       2 = regular battle with "Call" (for Shadow Pokémon battles)
	//       3 = Safari Zone
	//       4 = Bug-Catching Contest
	public void CommandMenuEx(idxBattler, texts, mode = 0) {
		ShowWindow(COMMAND_BOX);
		cw = @sprites["commandWindow"];
		cw.setTexts(texts);
		cw.setIndexAndMode(@lastCmd[idxBattler], mode);
		SelectBattler(idxBattler);
		ret = -1;
		do { //loop; while (true);
			oldIndex = cw.index;
			Update(cw);
			// Update selected command
			if (Input.trigger(Input.LEFT)) {
				if ((cw.index & 1) == 1) cw.index -= 1;
			} else if (Input.trigger(Input.RIGHT)) {
				if ((cw.index & 1) == 0) cw.index += 1;
			} else if (Input.trigger(Input.UP)) {
				if ((cw.index & 2) == 2) cw.index -= 2;
			} else if (Input.trigger(Input.DOWN)) {
				if ((cw.index & 2) == 0) cw.index += 2;
			}
			if (cw.index != oldIndex) PlayCursorSE;
			// Actions
			if (Input.trigger(Input.USE)) {                 // Confirm choice
				PlayDecisionSE;
				ret = cw.index;
				@lastCmd[idxBattler] = ret;
				break;
			} else if (Input.trigger(Input.BACK) && mode == 1) {   // Cancel
				PlayCancelSE
				break;
			} else if (Input.trigger(Input.F9) && Core.DEBUG) {    // Debug menu
				PlayDecisionSE;
				ret = -2;
				break;
			}
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// The player chooses a move for a Pokémon to use.
	//-----------------------------------------------------------------------------

	public void FightMenu(idxBattler, megaEvoPossible = false) {
		battler = @battle.battlers[idxBattler];
		cw = @sprites["fightWindow"];
		cw.battler = battler;
		moveIndex = 0;
		if (battler.moves[@lastMove[idxBattler]]&.id) {
			moveIndex = @lastMove[idxBattler];
		}
		cw.shiftMode = (@battle.CanShift(idxBattler)) ? 1 : 0;
		cw.setIndexAndMode(moveIndex, (megaEvoPossible) ? 1 : 0);
		needFullRefresh = true;
		needRefresh = false;
		do { //loop; while (true);
			// Refresh view if necessary
			if (needFullRefresh) {
				ShowWindow(FIGHT_BOX);
				SelectBattler(idxBattler);
				needFullRefresh = false;
			}
			if (needRefresh) {
				if (megaEvoPossible) {
					newMode = (@battle.RegisteredMegaEvolution(idxBattler)) ? 2 : 1;
					if (newMode != cw.mode) cw.mode = newMode;
				}
				needRefresh = false;
			}
			oldIndex = cw.index;
			// General update
			Update(cw);
			// Update selected command
			if (Input.trigger(Input.LEFT)) {
				if ((cw.index & 1) == 1) cw.index -= 1;
			} else if (Input.trigger(Input.RIGHT)) {
				if (battler.moves[cw.index + 1]&.id && (cw.index & 1) == 0) cw.index += 1;
			} else if (Input.trigger(Input.UP)) {
				if ((cw.index & 2) == 2) cw.index -= 2;
			} else if (Input.trigger(Input.DOWN)) {
				if (battler.moves[cw.index + 2]&.id && (cw.index & 2) == 0) cw.index += 2;
			}
			if (cw.index != oldIndex) PlayCursorSE;
			// Actions
			if (Input.trigger(Input.USE)) {      // Confirm choice
				PlayDecisionSE;
				if (yield cw.index) break;
				needFullRefresh = true;
				needRefresh = true;
			} else if (Input.trigger(Input.BACK)) {   // Cancel fight menu
				PlayCancelSE
				if (yield -1) break;
				needRefresh = true;
			} else if (Input.trigger(Input.ACTION)) {   // Toggle Mega Evolution
				if (megaEvoPossible) {
					PlayDecisionSE;
					if (yield -2) break;
					needRefresh = true;
				}
			} else if (Input.trigger(Input.SPECIAL)) {   // Shift
				if (cw.shiftMode > 0) {
					PlayDecisionSE;
					if (yield -3) break;
					needRefresh = true;
				}
			}
		}
		@lastMove[idxBattler] = cw.index;
	}

	//-----------------------------------------------------------------------------
	// Opens the party screen to choose a Pokémon to switch in (or just view its
	// summary screens).
	// mode: 0=Pokémon command, 1=choose a Pokémon to send to the Boxes, 2=view
	//       summaries only
	//-----------------------------------------------------------------------------

	public void PartyScreen(idxBattler, canCancel = false, mode = 0) {
		// Fade out and hide all sprites
		visibleSprites = FadeOutAndHide(@sprites);
		// Get player's party
		partyPos = @battle.PartyOrder(idxBattler);
		partyStart, _partyEnd = @battle.TeamIndexRangeFromBattlerIndex(idxBattler);
		modParty = @battle.PlayerDisplayParty(idxBattler);
		// Start party screen
		scene = new PokemonParty_Scene();
		switchScreen = new PokemonPartyScreen(scene, modParty);
		msg = _INTL("Choose a Pokémon.");
		if (mode == 1) msg = _INTL("Send which Pokémon to Boxes?");
		switchScreen.StartScene(msg, @battle.NumPositions(0, 0));
		// Loop while in party screen
		do { //loop; while (true);
			// Select a Pokémon
			scene.SetHelpText(msg);
			idxParty = switchScreen.ChoosePokemon;
			if (idxParty < 0) {
				if (!canCancel) continue;
				break;
			}
			// Choose a command for the selected Pokémon
			cmdSwitch  = -1;
			cmdBoxes   = -1;
			cmdSummary = -1;
			commands = new List<string>();
			if ((mode == 0 && modParty[idxParty].able() &&
																																		(@battle.canSwitch || !canCancel)) commands[cmdSwitch  = commands.length] = _INTL("Switch In");
			if (mode == 1) commands[cmdBoxes   = commands.length] = _INTL("Send to Boxes");
			commands[cmdSummary = commands.length] = _INTL("Summary");
			commands[commands.length]              = _INTL("Cancel");
			command = scene.ShowCommands(_INTL("Do what with {1}?", modParty[idxParty].name), commands);
			if ((cmdSwitch >= 0 && command == cmdSwitch) ||   // Switch In
				(cmdBoxes >= 0 && command == cmdBoxes)) {        // Send to Boxes
				idxPartyRet = -1;
				partyPos.each_with_index do |pos, i|
					if (pos != idxParty + partyStart) continue;
					idxPartyRet = i;
					break;
				}
				if (yield idxPartyRet, switchScreen) break;
			} else if (cmdSummary >= 0 && command == cmdSummary) {   // Summary
				scene.Summary(idxParty, true);
			}
		}
		// Close party screen
		switchScreen.EndScene;
		// Fade back into battle screen
		FadeInAndShow(@sprites, visibleSprites);
	}

	//-----------------------------------------------------------------------------
	// Opens the Bag screen and chooses an item to use.
	//-----------------------------------------------------------------------------

	public void ItemMenu(idxBattler, _firstAction) {
		// Fade out and hide all sprites
		visibleSprites = FadeOutAndHide(@sprites);
		// Set Bag starting positions
		oldLastPocket = Game.GameData.bag.last_viewed_pocket;
		oldChoices    = Game.GameData.bag.last_pocket_selections.clone;
		if (@bagLastPocket) {
			Game.GameData.bag.last_viewed_pocket     = @bagLastPocket;
			Game.GameData.bag.last_pocket_selections = @bagChoices;
		} else {
			Game.GameData.bag.reset_last_selections;
		}
		// Start Bag screen
		itemScene = new PokemonBag_Scene();
		itemScene.StartScene(Game.GameData.bag, true,
													block: (item) => {
														useType = GameData.Item.get(item).battle_use;
														next useType && useType > 0;
													}, false);
		// Loop while in Bag screen
		wasTargeting = false;
		do { //loop; while (true);
			// Select an item
			item = itemScene.ChooseItem;
			if (!item) break;
			// Choose a command for the selected item
			item = GameData.Item.get(item);
			itemName = item.name;
			useType = item.battle_use;
			cmdUse = -1;
			commands = new List<string>();
			if (useType && useType != 0) commands[cmdUse = commands.length] = _INTL("Use");
			commands[commands.length]          = _INTL("Cancel");
			command = itemScene.ShowCommands(_INTL("{1} is selected.", itemName), commands);
			unless (cmdUse >= 0 && command == cmdUse) continue;   // Use
			// Use types:
			// 0 = not usable in battle
			// 1 = use on Pokémon (lots of items, Blue Flute)
			// 2 = use on Pokémon's move (Ethers)
			// 3 = use on battler (X items, Persim Berry, Red/Yellow Flutes)
			// 4 = use on opposing battler (Poké Balls)
			// 5 = use no target (Poké Doll, Guard Spec., Poké Flute, Launcher items)
			switch (useType) {
				case 1: case 2: case 3:   // Use on Pokémon/Pokémon's move/battler
					// Auto-choose the Pokémon/battler whose action is being decided if they
					// are the only available Pokémon/battler to use the item on
					switch (useType) {
						case 1:   // Use on Pokémon
							if (@battle.TeamLengthFromBattlerIndex(idxBattler) == 1) {
								if (yield item.id, useType, @battle.battlers[idxBattler].pokemonIndex, -1, itemScene) {
									break;
								} else {
									continue;
								}
							}
							break;
						case 3:   // Use on battler
							if (@battle.PlayerBattlerCount == 1) {
								if (yield item.id, useType, @battle.battlers[idxBattler].pokemonIndex, -1, itemScene) {
									break;
								} else {
									continue;
								}
							}
							break;
					}
					// Fade out and hide Bag screen
					itemScene.FadeOutScene;
					// Get player's party
					party    = @battle.Party(idxBattler);
					partyPos = @battle.PartyOrder(idxBattler);
					partyStart, _partyEnd = @battle.TeamIndexRangeFromBattlerIndex(idxBattler);
					modParty = @battle.PlayerDisplayParty(idxBattler);
					// Start party screen
					pkmnScene = new PokemonParty_Scene();
					pkmnScreen = new PokemonPartyScreen(pkmnScene, modParty);
					pkmnScreen.StartScene(_INTL("Use on which Pokémon?"), @battle.NumPositions(0, 0));
					idxParty = -1;
					// Loop while in party screen
					do { //loop; while (true);
						// Select a Pokémon
						pkmnScene.SetHelpText(_INTL("Use on which Pokémon?"));
						idxParty = pkmnScreen.ChoosePokemon;
						if (idxParty < 0) break;
						idxPartyRet = -1;
						partyPos.each_with_index do |pos, i|
							if (pos != idxParty + partyStart) continue;
							idxPartyRet = i;
							break;
						}
						if (idxPartyRet < 0) continue;
						pkmn = party[idxPartyRet];
						if (!pkmn || pkmn.egg()) continue;
						idxMove = -1;
						if (useType == 2) {   // Use on Pokémon's move
							idxMove = pkmnScreen.ChooseMove(pkmn, _INTL("Restore which move?"));
							if (idxMove < 0) continue;
						}
						if (yield item.id, useType, idxPartyRet, idxMove, pkmnScene) break;
					}
					pkmnScene.EndScene;
					if (idxParty >= 0) break;
					// Cancelled choosing a Pokémon; show the Bag screen again
					itemScene.FadeInScene;
					break;
				case 4:   // Use on opposing battler (Poké Balls)
					idxTarget = -1;
					if (@battle.OpposingBattlerCount(idxBattler) == 1) {
						@battle.allOtherSideBattlers(idxBattler).each(b => idxTarget = b.index);
						if (yield item.id, useType, idxTarget, -1, itemScene) break;
					} else {
						wasTargeting = true;
						// Fade out and hide Bag screen
						itemScene.FadeOutScene;
						// Fade in and show the battle screen, choosing a target
						tempVisibleSprites = visibleSprites.clone;
						tempVisibleSprites["commandWindow"] = false;
						tempVisibleSprites["targetWindow"]  = true;
						idxTarget = ChooseTarget(idxBattler, GameData.Target.get(:Foe), tempVisibleSprites);
						if (idxTarget >= 0) {
							if (yield item.id, useType, idxTarget, -1, self) break;
						}
						// Target invalid/cancelled choosing a target; show the Bag screen again
						wasTargeting = false;
						FadeOutAndHide(@sprites);
						itemScene.FadeInScene;
					}
					break;
				case 5:   // Use with no target
					if (yield item.id, useType, idxBattler, -1, itemScene) break;
					break;
			}
		}
		@bagLastPocket = Game.GameData.bag.last_viewed_pocket;
		@bagChoices    = Game.GameData.bag.last_pocket_selections.clone;
		Game.GameData.bag.last_viewed_pocket     = oldLastPocket;
		Game.GameData.bag.last_pocket_selections = oldChoices;
		// Close Bag screen
		itemScene.EndScene;
		// Fade back into battle screen (if not already showing it)
		if (!wasTargeting) FadeInAndShow(@sprites, visibleSprites);
	}

	//-----------------------------------------------------------------------------
	// The player chooses a target battler for a move/item (non-single battles
	// only).
	//-----------------------------------------------------------------------------

	// Returns an array containing battler names to display when choosing a move's
	// target.
	// null means can't select that position, "" means can select that position but
	// there is no battler there, otherwise is a battler's name.
	public void CreateTargetTexts(idxBattler, target_data) {
		texts = new Array(@battle.battlers.length) do |i|
			if (!@battle.battlers[i]) next null;
			showName = false;
			// NOTE: Targets listed here are ones with num_targets of 0, plus
			//       RandomNearFoe which should look like it targets the user. All
			//       other targets are handled by the "else" part.
			switch (target_data.id) {
				case :None: case :User: case :RandomNearFoe:
					showName = (i == idxBattler);
					break;
				case :UserSide:
					showName = !@battle.opposes(i, idxBattler);
					break;
				case :FoeSide:
					showName = @battle.opposes(i, idxBattler);
					break;
				case :BothSides:
					showName = true;
					break;
				default:
					showName = @battle.MoveCanTarget(idxBattler, i, target_data);
					break;
			}
			if (!showName) next null;
			next (@battle.battlers[i].fainted()) ? "" : @battle.battlers[i].name;
		}
		return texts;
	}

	// Returns the initial position of the cursor when choosing a target for a move
	// in a non-single battle.
	public void FirstTarget(idxBattler, target_data) {
		switch (target_data.id) {
			case :NearAlly:
				@battle.allSameSideBattlers(idxBattler).each do |b|
					if (b.index == idxBattler || !@battle.nearBattlers(b, idxBattler)) continue;
					if (b.fainted()) continue;
					return b.index;
				}
				@battle.allSameSideBattlers(idxBattler).each do |b|
					if (b.index == idxBattler || !@battle.nearBattlers(b, idxBattler)) continue;
					return b.index;
				}
				break;
			case :NearFoe: case :NearOther:
				indices = @battle.GetOpposingIndicesInOrder(idxBattler);
				indices.each(i => { if (@battle.nearBattlers(i, idxBattler) && !@battle.battlers[i].fainted()) return i; });
				indices.each(i => { if (@battle.nearBattlers(i, idxBattler)) return i; });
				break;
			case :Foe: case :Other:
				indices = @battle.GetOpposingIndicesInOrder(idxBattler);
				indices.each(i => { if (!@battle.battlers[i].fainted()) return i; });
				if (!indices.empty()) return indices.first;
				break;
		}
		return idxBattler;   // Target the user initially
	}

	public void ChooseTarget(idxBattler, target_data, visibleSprites = null) {
		ShowWindow(TARGET_BOX);
		cw = @sprites["targetWindow"];
		// Create an array of battler names (only valid targets are named)
		texts = CreateTargetTexts(idxBattler, target_data);
		// Determine mode based on target_data
		mode = (target_data.num_targets == 1) ? 0 : 1;
		cw.setDetails(texts, mode);
		cw.index = FirstTarget(idxBattler, target_data);
		SelectBattler((mode == 0) ? cw.index : texts, 2);   // Select initial battler/data box
		if (visibleSprites) FadeInAndShow(@sprites, visibleSprites);
		ret = -1;
		do { //loop; while (true);
			oldIndex = cw.index;
			Update(cw);
			// Update selected command
			if (mode == 0) {   // Choosing just one target, can change index
				if (Input.trigger(Input.LEFT) || Input.trigger(Input.RIGHT)) {
					inc = (cw.index.even()) ? -2 : 2;
					if (Input.trigger(Input.RIGHT)) inc *= -1;
					indexLength = @battle.sideSizes[cw.index % 2] * 2;
					newIndex = cw.index;
					do { //loop; while (true);
						newIndex += inc;
						if (newIndex < 0 || newIndex >= indexLength) break;
						if (texts[newIndex].null()) continue;
						cw.index = newIndex;
						break;
					}
				} else if (((Input.trigger(Input.UP) && cw.index.even()) ||
							(Input.trigger(Input.DOWN) && cw.index.odd())) {
					tryIndex = @battle.GetOpposingIndicesInOrder(cw.index);
					foreach (var idxBattlerTry in tryIndex) { //'tryIndex.each' do => |idxBattlerTry|
						if (texts[idxBattlerTry].null()) continue;
						cw.index = idxBattlerTry;
						break;
					}
				}
				if (cw.index != oldIndex) {
					PlayCursorSE;
					SelectBattler(cw.index, 2);   // Select the new battler/data box
				}
			}
			if (Input.trigger(Input.USE)) {   // Confirm
				ret = cw.index;
				PlayDecisionSE;
				break;
			} else if (Input.trigger(Input.BACK)) {   // Cancel
				ret = -1;
				PlayCancelSE
				break;
			}
		}
		SelectBattler(-1);   // Deselect all battlers/data boxes
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Opens a Pokémon's summary screen to try to learn a new move.
	//-----------------------------------------------------------------------------

	// Called whenever a Pokémon should forget a move. It should return -1 if the
	// selection is canceled, or 0 to 3 to indicate the move to forget. It should
	// not allow HM moves to be forgotten.
	public void ForgetMove(pkmn, moveToLearn) {
		ret = -1;
		FadeOutIn do;
			scene = new PokemonSummary_Scene();
			screen = new PokemonSummaryScreen(scene);
			ret = screen.StartForgetScreen([pkmn], 0, moveToLearn);
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Opens the nicknaming screen for a newly caught Pokémon.
	//-----------------------------------------------------------------------------

	public void NameEntry(helpText, pkmn) {
		return EnterPokemonName(helpText, 0, Pokemon.MAX_NAME_SIZE, "", pkmn);
	}

	//-----------------------------------------------------------------------------
	// Shows the Pokédex entry screen for a newly caught Pokémon.
	//-----------------------------------------------------------------------------

	public void ShowPokedex(species) {
		FadeOutIn do;
			scene = new PokemonPokedexInfo_Scene();
			screen = new PokemonPokedexInfoScreen(scene);
			screen.DexEntry(species);
		}
	}
}
