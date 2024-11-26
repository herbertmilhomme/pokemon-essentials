//===============================================================================
//
//===============================================================================
public static partial class Battle.CatchAndStoreMixin {
	//-----------------------------------------------------------------------------
	// Store caught Pokémon.
	//-----------------------------------------------------------------------------

	public void StorePokemon(pkmn) {
		// Nickname the Pokémon (unless it's a Shadow Pokémon)
		if (!pkmn.shadowPokemon()) {
			if (Game.GameData.PokemonSystem.givenicknames == 0 &&
				DisplayConfirm(_INTL("Would you like to give a nickname to {1}?", pkmn.name))) {
				nickname = @scene.NameEntry(_INTL("{1}'s nickname?", pkmn.speciesName), pkmn);
				pkmn.name = nickname;
			}
		}
		// Store the Pokémon
		if (Player.party_full() && (@sendToBoxes == 0 || @sendToBoxes == 2)) {   // Ask/must add to party
			cmds = new {_INTL("Add to your party"),
							_INTL("Send to a Box"),
							_INTL("See {1}'s summary", pkmn.name),
							_INTL("Check party")};
			if (@sendToBoxes == 2) cmds.delete_at(1);   // Remove "Send to a Box" option
			do { //loop; while (true);
				cmd = ShowCommands(_INTL("Where do you want to send {1} to?", pkmn.name), cmds, 99);
				if (cmd == 99 && @sendToBoxes == 2) continue;   // Can't cancel if must add to party
				if (cmd == 99) break;   // Cancelling = send to a Box
				if (cmd >= 1 && @sendToBoxes == 2) cmd += 1;
				switch (cmd) {
					case 0:   // Add to your party
						Display(_INTL("Choose a Pokémon in your party to send to your Boxes."));
						party_index = -1;
						@scene.PartyScreen(0, (@sendToBoxes != 2), 1) do |idxParty, _partyScene|
							party_index = idxParty;
							next true;
						}
						if (party_index < 0) continue;   // Cancelled
						party_size = Player.party.length;
						// Get chosen Pokémon and clear battle-related conditions
						send_pkmn = Player.party[party_index];
						@peer.OnLeavingBattle(self, send_pkmn, @usedInBattle[0][party_index], true);
						if (send_pkmn.status == statuses.POISON) send_pkmn.statusCount = 0;   // Bad poison becomes regular
						send_pkmn.makeUnmega;
						send_pkmn.makeUnprimal;
						// Send chosen Pokémon to storage
						stored_box = @peer.StorePokemon(Player, send_pkmn);
						Player.party.delete_at(party_index);
						box_name = @peer.BoxName(stored_box);
						DisplayPaused(_INTL("{1} has been sent to Box \"{2}\".", send_pkmn.name, box_name));
						// Rearrange all remembered properties of party Pokémon
						for (int idx = party_index; idx < party_size; idx++) { //each 'party_size' do => |idx|
							if (idx < party_size - 1) {
								@initialItems[0][idx] = @initialItems[0][idx + 1];
								Game.GameData.game_temp.party_levels_before_battle[idx] = Game.GameData.game_temp.party_levels_before_battle[idx + 1];
							} else {
								@initialItems[0][idx] = null;
								Game.GameData.game_temp.party_levels_before_battle[idx] = null;
							}
						}
						break;
						break;
					case 1:   // Send to a Box
						break;
						break;
					case 2:   // See X's summary
						FadeOutIn do;
							summary_scene = new PokemonSummary_Scene();
							summary_screen = new PokemonSummaryScreen(summary_scene, true);
							summary_screen.StartScreen([pkmn], 0);
						}
						break;
					case 3:   // Check party
						@scene.PartyScreen(0, true, 2);
						break;
				}
			}
		}
		// Store as normal (add to party if there's space, or send to a Box if not)
		stored_box = @peer.StorePokemon(Player, pkmn);
		if (stored_box < 0) {
			DisplayPaused(_INTL("{1} has been added to your party.", pkmn.name));
			if (@initialItems) @initialItems[0][Player.party.length - 1] = pkmn.item_id;
			return;
		}
		// Messages saying the Pokémon was stored in a PC box
		box_name = @peer.BoxName(stored_box);
		DisplayPaused(_INTL("{1} has been sent to Box \"{2}\"!", pkmn.name, box_name));
	}

	// Register all caught Pokémon in the Pokédex, and store them.
	public void RecordAndStoreCaughtPokemon() {
		@caughtPokemon.each do |pkmn|
			SetCaught(pkmn);
			SetSeen(pkmn);   // In case the form changed upon leaving battle
			// Record the Pokémon's species as owned in the Pokédex
			if (!Player.owned(pkmn.species)) {
				Player.pokedex.set_owned(pkmn.species);
				if (Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(pkmn.species)) {
					DisplayPaused(_INTL("{1}'s data was added to the Pokédex.", pkmn.name));
					Player.pokedex.register_last_seen(pkmn);
					@scene.ShowPokedex(pkmn.species);
				}
			}
			// Record a Shadow Pokémon's species as having been caught
			if (pkmn.shadowPokemon()) Player.pokedex.set_shadow_pokemon_owned(pkmn.species);
			// Store caught Pokémon
			StorePokemon(pkmn);
		}
		@caughtPokemon.clear;
	}

	//-----------------------------------------------------------------------------
	// Throw a Poké Ball.
	//-----------------------------------------------------------------------------

	public void ThrowPokeBall(idxBattler, ball, catch_rate = null, showPlayer = false) {
		// Determine which Pokémon you're throwing the Poké Ball at
		battler = null;
		if (opposes(idxBattler)) {
			battler = @battlers[idxBattler];
		} else {
			battler = @battlers[idxBattler].DirectOpposing(true);
		}
		if (battler.fainted()) battler = battler.allAllies[0];
		// Messages
		itemName = GameData.Item.get(ball).name;
		if (battler.fainted()) {
			if (itemName.starts_with_vowel()) {
				Display(_INTL("{1} threw an {2}!", Player.name, itemName));
			} else {
				Display(_INTL("{1} threw a {2}!", Player.name, itemName));
			}
			Display(_INTL("But there was no target..."));
			return;
		}
		if (itemName.starts_with_vowel()) {
			DisplayBrief(_INTL("{1} threw an {2}!", Player.name, itemName));
		} else {
			DisplayBrief(_INTL("{1} threw a {2}!", Player.name, itemName));
		}
		// Animation of opposing trainer blocking Poké Balls (unless it's a Snag Ball
		// at a Shadow Pokémon)
		if (trainerBattle() && !(GameData.Item.get(ball).is_snag_ball() && battler.shadowPokemon())) {
			@scene.ThrowAndDeflect(ball, 1);
			Display(_INTL("The Trainer blocked your Poké Ball! Don't be a thief!"));
			return;
		}
		// Calculate the number of shakes (4=capture)
		pkmn = battler.pokemon;
		@criticalCapture = false;
		numShakes = CaptureCalc(pkmn, battler, catch_rate, ball);
		Debug.Log($"[Threw Poké Ball] {itemName}, {numShakes} shakes (4=capture)");
		// Animation of Ball throw, absorb, shake and capture/burst out
		@scene.Throw(ball, numShakes, @criticalCapture, battler.index, showPlayer);
		// Outcome message
		switch (numShakes) {
			case 0:
				Display(_INTL("Oh no! The Pokémon broke free!"));
				Battle.PokeBallEffects.onFailCatch(ball, self, battler);
				break;
			case 1:
				Display(_INTL("Aww! It appeared to be caught!"));
				Battle.PokeBallEffects.onFailCatch(ball, self, battler);
				break;
			case 2:
				Display(_INTL("Aargh! Almost had it!"));
				Battle.PokeBallEffects.onFailCatch(ball, self, battler);
				break;
			case 3:
				Display(_INTL("Gah! It was so close, too!"));
				Battle.PokeBallEffects.onFailCatch(ball, self, battler);
				break;
			case 4:
				DisplayBrief(_INTL("Gotcha! {1} was caught!", pkmn.name));
				@scene.ThrowSuccess;   // Play capture success jingle
				RemoveFromParty(battler.index, battler.pokemonIndex);
				// Gain Exp
				if (Settings.GAIN_EXP_FOR_CAPTURE) {
					battler.captured = true;
					GainExp;
					battler.captured = false;
				}
				battler.Reset;
				if (AllFainted(battler.index)) {
					@decision = (trainerBattle()) ? Battle.Outcome.WIN : Battle.Outcome.CATCH;
				}
				// Modify the Pokémon's properties because of the capture
				if (GameData.Item.get(ball).is_snag_ball()) {
					pkmn.owner = Pokemon.Owner.new_from_trainer(Player);
				}
				Battle.PokeBallEffects.onCatch(ball, self, pkmn);
				pkmn.poke_ball = ball;
				if (pkmn.mega()) pkmn.makeUnmega;
				pkmn.makeUnprimal;
				if (pkmn.shadowPokemon()) pkmn.update_shadow_moves;
				pkmn.record_first_moves;
				// Reset form
				if (MultipleForms.hasFunction(pkmn.species, "getForm")) pkmn.forced_form = null;
				@peer.OnLeavingBattle(self, pkmn, true, true);
				// Make the Poké Ball and data box disappear
				@scene.HideCaptureBall(idxBattler);
				// Save the Pokémon for storage at the end of battle
				@caughtPokemon.Add(pkmn);
				break;
		}
		if (numShakes != 4) {
			if (!@poke_ball_failed) @first_poke_ball = ball;
			@poke_ball_failed = true;
		}
	}

	//-----------------------------------------------------------------------------
	// Calculate how many shakes a thrown Poké Ball will make (4 = capture).
	//-----------------------------------------------------------------------------

	public void CaptureCalc(pkmn, battler, catch_rate, ball) {
		if (Core.DEBUG && Input.press(Input.CTRL)) return 4;
		// Get a catch rate if one wasn't provided
		if (!catch_rate) catch_rate = pkmn.species_data.catch_rate;
		// Modify catch_rate depending on the Poké Ball's effect
		if (!pkmn.species_data.has_flag("UltraBeast") || ball == Items.BEAST_BALL) {
			catch_rate = Battle.PokeBallEffects.modifyCatchRate(ball, catch_rate, self, battler);
		} else {
			catch_rate /= 10;
		}
		// First half of the shakes calculation
		a = battler.totalhp;
		b = battler.hp;
		x = (((3 * a) - (2 * b)) * catch_rate.to_f) / (3 * a);
		// Calculation modifiers
		if (battler.status == statuses.SLEEP || battler.status == statuses.FROZEN) {
			x *= 2.5;
		} else if (battler.status != statuses.NONE) {
			x *= 1.5;
		}
		x = x.floor;
		if (x < 1) x = 1;
		// Definite capture, no need to perform randomness checks
		if (x >= 255 || Battle.PokeBallEffects.isUnconditional(ball, self, battler)) return 4;
		// Second half of the shakes calculation
		y = (int)Math.Floor(65_536 / ((255.0 / x)**0.1875));
		// Critical capture check
		if (Settings.ENABLE_CRITICAL_CAPTURES) {
			dex_modifier = 0;
			numOwned = Game.GameData.player.pokedex.owned_count;
			if (numOwned > 600) {
				dex_modifier = 5;
			} else if (numOwned > 450) {
				dex_modifier = 4;
			} else if (numOwned > 300) {
				dex_modifier = 3;
			} else if (numOwned > 150) {
				dex_modifier = 2;
			} else if (numOwned > 30) {
				dex_modifier = 1;
			}
			if (Game.GameData.bag.has(:CATCHINGCHARM)) dex_modifier *= 2;
			c = x * dex_modifier / 12;
			// Calculate the number of shakes
			if (c > 0 && Random(256) < c) {
				@criticalCapture = true;
				if (Random(65_536) < y) return 4;
				return 0;
			}
		}
		// Calculate the number of shakes
		numShakes = 0;
		for (int i = 4; i < 4; i++) { //for '4' times do => |i|
			if (numShakes < i) break;
			if (Random(65_536) < y) numShakes += 1;
		}
		return numShakes;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Battle {
	include Battle.CatchAndStoreMixin;
}
