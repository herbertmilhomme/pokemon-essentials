//===============================================================================
//
//===============================================================================
public partial class Battle {
	public partial class BattleAbortedException : Exception { }

	//-----------------------------------------------------------------------------

	public void Abort() {
		Debug.LogError(new BattleAbortedException("Battle aborted"));
		//throw new Exception(new BattleAbortedException("Battle aborted"));
	}

	// Makes sure all Pokémon exist that need to. Alter the type of battle if
	// necessary. Will never try to create battler positions, only delete them
	// (except for wild Pokémon whose number of positions are fixed). Reduces the
	// size of each side by 1 and tries again. If the side sizes are uneven, only
	// the larger side's size will be reduced by 1 each time, until both sides are
	// an equal size (then both sides will be reduced equally).
	public void EnsureParticipants() {
		// Prevent battles larger than 2v2 if both sides have multiple trainers
		// NOTE: This is necessary to ensure that battlers can never become unable to
		//       hit each other due to being too far away. In such situations,
		//       battlers will move to the centre position at the end of a round, but
		//       because they cannot move into a position owned by a different
		//       trainer, it's possible that battlers will be unable to move close
		//       enough to hit each other if there are multiple trainers on both
		//       sides.
		if (trainerBattle() && (@sideSizes[0] > 2 || @sideSizes[1] > 2) &&
			@player.length > 1 && @opponent.length > 1) {
			Debug.LogError(_INTL("Can't have battles larger than 2v2 where both sides have multiple trainers"));
			//throw new ArgumentException(_INTL("Can't have battles larger than 2v2 where both sides have multiple trainers"));
		}
		// Find out how many Pokémon each trainer has
		side1counts = AbleTeamCounts(0);
		side2counts = AbleTeamCounts(1);
		// Change the size of the battle depending on how many wild Pokémon there are
		if (wildBattle() && side2counts[0] != @sideSizes[1]) {
			if (@sideSizes[0] == @sideSizes[1]) {
				// Even number of battlers per side, change both equally
				@sideSizes = new {side2counts[0], side2counts[0]};
			} else {
				// Uneven number of battlers per side, just change wild side's size
				@sideSizes[1] = side2counts[0];
			}
		}
		// Check if battle is possible, including changing the number of battlers per
		// side if necessary
		do { //loop; while (true);
			needsChanging = false;
			for (int side = 2; side < 2; side++) { //for '2' times do => |side|   // Each side in turn
				if (side == 1 && wildBattle()) continue;   // Wild side's size already checked above
				sideCounts = (side == 0) ? side1counts : side2counts;
				requireds = new List<string>();
				// Find out how many Pokémon each trainer on side needs to have
				for (int i = @sideSizes[side]; i < @sideSizes[side]; i++) { //for '@sideSizes[side]' times do => |i|
					idxTrainer = GetOwnerIndexFromBattlerIndex((i * 2) + side);
					if (requireds[idxTrainer].null()) requireds[idxTrainer] = 0;
					requireds[idxTrainer] += 1;
				}
				// Compare the have values with the need values
				if (requireds.length > sideCounts.length) {
					Debug.LogError(_INTL("Error: def GetOwnerIndexFromBattlerIndex gives invalid owner index ({1} for battle type {2}v{3}, trainers {4}v{5})",);
					//throw new Exception(_INTL("Error: def GetOwnerIndexFromBattlerIndex gives invalid owner index ({1} for battle type {2}v{3}, trainers {4}v{5})",);
											requireds.length - 1, @sideSizes[0], @sideSizes[1], side1counts.length, side2counts.length);
				}
				sideCounts.each_with_index do |_count, i|
					if (!requireds[i] || requireds[i] == 0) {
						switch (side) {
							case 0:
								Debug.LogError(_INTL("Player-side trainer {1} has no battler position for their Pokémon to go (trying {2}v{3} battle)",
								//throw new Exception(_INTL("Player-side trainer {1} has no battler position for their Pokémon to go (trying {2}v{3} battle)",
														i + 1, @sideSizes[0], @sideSizes[1]));
								break;
							case 1:
								Debug.LogError(_INTL($"Opposing trainer {1} has no battler position for their Pokémon to go (trying {2}v{3} battle)",
								//throw new Exception(_INTL("Opposing trainer {1} has no battler position for their Pokémon to go (trying {2}v{3} battle)",
														i + 1, @sideSizes[0], @sideSizes[1]));
								break;
						}
					}
					if (requireds[i] <= sideCounts[i]) continue;   // Trainer has enough Pokémon to fill their positions
					if (requireds[i] == 1) {
						if (side == 0) raise _INTL("Player-side trainer {1} has no able Pokémon", i + 1);
						if (side == 1) raise _INTL("Opposing trainer {1} has no able Pokémon", i + 1);
					}
					// Not enough Pokémon, try lowering the number of battler positions
					needsChanging = true;
					break;
				}
				if (needsChanging) break;
			}
			if (!needsChanging) break;
			// Reduce one or both side's sizes by 1 and try again
			if (wildBattle()) {
				Debug.Log($"{@sideSizes[0]}v{@sideSizes[1]} battle isn't possible " +;
										$"({side1counts} player-side teams versus {side2counts[0]} wild Pokémon)");
				newSize = @sideSizes[0] - 1;
			} else {
				Debug.Log($"{@sideSizes[0]}v{@sideSizes[1]} battle isn't possible " +;
										$"({side1counts} player-side teams versus {side2counts} opposing teams)");
				newSize = @sideSizes.max - 1;
			}
			if (newSize == 0) {
				Debug.LogError(_INTL("Couldn't lower either side's size any further, battle isn't possible"));
				//throw new ArgumentException(_INTL("Couldn't lower either side's size any further, battle isn't possible"));
			}
			for (int side = 2; side < 2; side++) { //for '2' times do => |side|
				if (side == 1 && wildBattle()) continue;   // Wild Pokémon's side size is fixed
				if (@sideSizes[side] == 1 || newSize > @sideSizes[side]) continue;
				@sideSizes[side] = newSize;
			}
			Debug.Log($"Trying {@sideSizes[0]}v{@sideSizes[1]} battle instead");
		}
	}

	//-----------------------------------------------------------------------------
	// Set up all battlers
	//-----------------------------------------------------------------------------

	public void CreateBattler(idxBattler, pkmn, idxParty) {
		if (!@battlers[idxBattler].null()) {
			Debug.LogError(_INTL("Battler index {1} already exists", idxBattler));
			//throw new ArgumentException(_INTL("Battler index {1} already exists", idxBattler));
		}
		@battlers[idxBattler] = new Battler(self, idxBattler);
		@positions[idxBattler] = new ActivePosition();
		ClearChoice(idxBattler);
		@successStates[idxBattler] = new SuccessState();
		@battlers[idxBattler].Initialize(pkmn, idxParty);
	}

	public void SetUpSides() {
		ret = new {[], new List<string>()};
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			// Set up wild Pokémon
			if (side == 1 && wildBattle()) {
				Party(1).each_with_index do |pkmn, idxPkmn|
					CreateBattler((2 * idxPkmn) + side, pkmn, idxPkmn);
					// Changes the Pokémon's form upon entering battle (if it should)
					@peer.OnEnteringBattle(self, @battlers[(2 * idxPkmn) + side], pkmn, true);
					SetSeen(@battlers[(2 * idxPkmn) + side]);
					@usedInBattle[side][idxPkmn] = true;
				}
				continue;
			}
			// Set up player's Pokémon and trainers' Pokémon
			trainer = (side == 0) ? @player : @opponent;
			requireds = new List<string>();
			// Find out how many Pokémon each trainer on side needs to have
			for (int i = @sideSizes[side]; i < @sideSizes[side]; i++) { //for '@sideSizes[side]' times do => |i|
				idxTrainer = GetOwnerIndexFromBattlerIndex((i * 2) + side);
				if (requireds[idxTrainer].null()) requireds[idxTrainer] = 0;
				requireds[idxTrainer] += 1;
			}
			// For each trainer in turn, find the needed number of Pokémon for them to
			// send out, and initialize them
			battlerNumber = 0;
			partyOrder = PartyOrder(side);
			starts = PartyStarts(side);
			trainer.each_with_index do |_t, idxTrainer|
				ret[side][idxTrainer] = new List<string>();
				eachInTeam(side, idxTrainer) do |pkmn, idxPkmn|
					if (!pkmn.able()) continue;
					idxBattler = (2 * battlerNumber) + side;
					CreateBattler(idxBattler, pkmn, idxPkmn);
					ret[side][idxTrainer].Add(idxBattler);
					if (idxPkmn != starts[idxTrainer] + battlerNumber) {
						idxOther = starts[idxTrainer] + battlerNumber;
						partyOrder[idxPkmn], partyOrder[idxOther] = partyOrder[idxOther], partyOrder[idxPkmn];
					}
					battlerNumber += 1;
					if (ret[side][idxTrainer].length >= requireds[idxTrainer]) break;
				}
			}
		}
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Send out all battlers at the start of battle.
	//-----------------------------------------------------------------------------

	public void StartBattleSendOut(sendOuts) {
		// "Want to battle" messages
		if (wildBattle()) {
			foeParty = Party(1);
			switch (foeParty.length) {
				case 1:
					DisplayPaused(_INTL("Oh! A wild {1} appeared!", foeParty[0].name));
					break;
				case 2:
					DisplayPaused(_INTL("Oh! A wild {1} and {2} appeared!", foeParty[0].name,
																foeParty[1].name));
					break;
				case 3:
					DisplayPaused(_INTL("Oh! A wild {1}, {2} and {3} appeared!", foeParty[0].name,
																foeParty[1].name, foeParty[2].name));
					break;
			}
		} else {   // Trainer battle
			switch (@opponent.length) {
				case 1:
					DisplayPaused(_INTL("You are challenged by {1}!", @opponent[0].full_name));
					break;
				case 2:
					DisplayPaused(_INTL("You are challenged by {1} and {2}!", @opponent[0].full_name,
																@opponent[1].full_name));
					break;
				case 3:
					DisplayPaused(_INTL("You are challenged by {1}, {2} and {3}!",
																@opponent[0].full_name, @opponent[1].full_name, @opponent[2].full_name));
					break;
			}
		}
		// Send out Pokémon (opposing trainers first)
		new {1, 0}.each do |side|
			if (side == 1 && wildBattle()) continue;
			msg = "";
			toSendOut = new List<string>();
			trainers = (side == 0) ? @player : @opponent;
			// Opposing trainers and partner trainers's messages about sending out Pokémon
			trainers.each_with_index do |t, i|
				if (side == 0 && i == 0) continue;   // The player's message is shown last
				if (msg.length > 0) msg += "\n";
				sent = sendOuts[side][i];
				switch (sent.length) {
					case 1:
						msg += _INTL("{1} sent out {2}!", t.full_name, @battlers[sent[0]].name);
						break;
					case 2:
						msg += _INTL("{1} sent out {2} and {3}!", t.full_name,
												@battlers[sent[0]].name, @battlers[sent[1]].name);
						break;
					case 3:
						msg += _INTL("{1} sent out {2}, {3} and {4}!", t.full_name,
												@battlers[sent[0]].name, @battlers[sent[1]].name, @battlers[sent[2]].name);
						break;
				}
				toSendOut.concat(sent);
			}
			// The player's message about sending out Pokémon
			if (side == 0) {
				if (msg.length > 0) msg += "\n";
				sent = sendOuts[side][0];
				switch (sent.length) {
					case 1:
						msg += _INTL("Go! {1}!", @battlers[sent[0]].name);
						break;
					case 2:
						msg += _INTL("Go! {1} and {2}!", @battlers[sent[0]].name, @battlers[sent[1]].name);
						break;
					case 3:
						msg += _INTL("Go! {1}, {2} and {3}!", @battlers[sent[0]].name,
												@battlers[sent[1]].name, @battlers[sent[2]].name);
						break;
				}
				toSendOut.concat(sent);
			}
			if (msg.length > 0) DisplayBrief(msg);
			// The actual sending out of Pokémon
			animSendOuts = new List<string>();
			foreach (var idxBattler in toSendOut) { //'toSendOut.each' do => |idxBattler|
				animSendOuts.Add(new {idxBattler, @battlers[idxBattler].pokemon});
			}
			SendOut(animSendOuts, true);
		}
	}

	//-----------------------------------------------------------------------------
	// Start a battle.
	//-----------------------------------------------------------------------------

	public void StartBattle() {
		Debug.Log($"");
		Debug.Log($"================================================================");
		Debug.Log($"");
		logMsg = "[Started battle] ";
		if (@sideSizes[0] == 1 && @sideSizes[1] == 1) {
			logMsg += "Single ";
		} else if (@sideSizes[0] == 2 && @sideSizes[1] == 2) {
			logMsg += "Double ";
		} else if (@sideSizes[0] == 3 && @sideSizes[1] == 3) {
			logMsg += "Triple ";
		} else {
			logMsg += $"{@sideSizes[0]}v{@sideSizes[1]} ";
		}
		if (wildBattle()) logMsg += "wild ";
		if (trainerBattle()) logMsg += "trainer ";
		logMsg += $"battle ({@player.length} trainer(s) vs. ";
		if (wildBattle()) logMsg += $"{Party(1).length} wild Pokémon)";
		if (trainerBattle()) logMsg += $"{@opponent.length} trainer(s))";
		Debug.Log(logMsg);
		EnsureParticipants;
		Party(0).each(pkmn => { if (pkmn) @peer.OnStartingBattle(self, pkmn, wildBattle()); });
		Party(1).each(pkmn => { if (pkmn) @peer.OnStartingBattle(self, pkmn, wildBattle()); });
		begin;
			StartBattleCore;
		rescue BattleAbortedException;
			@decision = Outcome.UNDECIDED;
			@scene.EndBattle(@decision);
		}
		return @decision;
	}

	public void StartBattleCore() {
		// Set up the battlers on each side
		sendOuts = SetUpSides;
		@battleAI.create_ai_objects;
		// Create all the sprites and play the battle intro animation
		@scene.StartBattle(self);
		// Show trainers on both sides sending out Pokémon
		StartBattleSendOut(sendOuts);
		// Weather announcement
		weather_data = GameData.BattleWeather.try_get(@field.weather);
		if (weather_data) CommonAnimation(weather_data.animation);
		switch (@field.weather) {
			case :Sun:          Display(_INTL("The sunlight is strong.")); break;
			case :Rain:         Display(_INTL("It is raining.")); break;
			case :Sandstorm:    Display(_INTL("A sandstorm is raging.")); break;
			case :Hail:         Display(_INTL("Hail is falling.")); break;
			case :Snowstorm:    Display(_INTL("It is snowing.")); break;
			case :HarshSun:     Display(_INTL("The sunlight is extremely harsh.")); break;
			case :HeavyRain:    Display(_INTL("It is raining heavily.")); break;
			case :StrongWinds:  Display(_INTL("The wind is strong.")); break;
			case :ShadowSky:    Display(_INTL("The sky is shadowy.")); break;
		}
		// Terrain announcement
		terrain_data = GameData.BattleTerrain.try_get(@field.terrain);
		if (terrain_data) CommonAnimation(terrain_data.animation);
		switch (@field.terrain) {
			case :Electric:
				Display(_INTL("An electric current runs across the battlefield!"));
				break;
			case :Grassy:
				Display(_INTL("Grass is covering the battlefield!"));
				break;
			case :Misty:
				Display(_INTL("Mist swirls about the battlefield!"));
				break;
			case :Psychic:
				Display(_INTL("The battlefield is weird!"));
				break;
		}
		// Abilities upon entering battle
		OnAllBattlersEnteringBattle;
		// Main battle loop
		BattleLoop;
	}

	//-----------------------------------------------------------------------------
	// Main battle loop.
	//-----------------------------------------------------------------------------

	public void BattleLoop() {
		@turnCount = 0;
		do { //loop; while (true);   // Now begin the battle loop
			Debug.Log($"");
			Debug.log_header($"===== Round {@turnCount + 1} =====");
			if (@debug && @turnCount >= 100) {
				@decision = DecisionOnTime;
				Debug.Log($"");
				Debug.Log($"***Undecided after 100 rounds, aborting***");
				Abort;
				break;
			}
			Debug.Log($"");
			// Command phase
			Debug.logonerr(() => CommandPhase);
			if (decided()) break;
			// Attack phase
			Debug.logonerr(() => AttackPhase);
			if (decided()) break;
			// End of round phase
			Debug.logonerr(() => EndOfRoundPhase);
			if (decided()) break;
			@turnCount += 1;
		}
		EndOfBattle;
	}

	//-----------------------------------------------------------------------------
	// End of battle.
	//-----------------------------------------------------------------------------

	public void GainMoney() {
		if (!@internalBattle || !@moneyGain) return;
		// Money rewarded from opposing trainers
		if (trainerBattle()) {
			tMoney = 0;
			@opponent.each_with_index do |t, i|
				tMoney += MaxLevelInTeam(1, i) * t.base_money;
			}
			if (@field.effects.AmuletCoin) tMoney *= 2;
			if (@field.effects.HappyHour) tMoney *= 2;
			oldMoney = Player.money;
			Player.money += tMoney;
			moneyGained = Player.money - oldMoney;
			if (moneyGained > 0) {
				Game.GameData.stats.battle_money_gained += moneyGained;
				DisplayPaused(_INTL("You got ${1} for winning!", moneyGained.to_s_formatted));
			}
		}
		// Pick up money scattered by Pay Day
		if (@field.effects.PayDay > 0) {
			if (@field.effects.AmuletCoin) @field.effects.PayDay *= 2;
			if (@field.effects.HappyHour) @field.effects.PayDay *= 2;
			oldMoney = Player.money;
			Player.money += @field.effects.PayDay;
			moneyGained = Player.money - oldMoney;
			if (moneyGained > 0) {
				Game.GameData.stats.battle_money_gained += moneyGained;
				DisplayPaused(_INTL("You picked up ${1}!", moneyGained.to_s_formatted));
			}
		}
	}

	public void LoseMoney() {
		if (!@internalBattle || !@moneyGain) return;
		if (Game.GameData.game_switches[Settings.NO_MONEY_LOSS]) return;
		maxLevel = MaxLevelInTeam(0, 0);   // Player's Pokémon only, not partner's
		multiplier = new {8, 16, 24, 36, 48, 64, 80, 100, 120};
		idxMultiplier = (int)Math.Min(Player.badge_count, multiplier.length - 1);
		tMoney = maxLevel * multiplier[idxMultiplier];
		if (tMoney > Player.money) tMoney = Player.money;
		oldMoney = Player.money;
		Player.money -= tMoney;
		moneyLost = oldMoney - Player.money;
		if (moneyLost > 0) {
			Game.GameData.stats.battle_money_lost += moneyLost;
			if (trainerBattle()) {
				DisplayPaused(_INTL("You gave ${1} to the winner...", moneyLost.to_s_formatted));
			} else {
				DisplayPaused(_INTL("You panicked and dropped ${1}...", moneyLost.to_s_formatted));
			}
		}
	}

	public void EndOfBattle() {
		oldDecision = @decision;
		if (@decision == Outcome.WIN && wildBattle() && @caughtPokemon.length > 0) @decision = Outcome.CATCH;
		switch (oldDecision) {
			case Outcome.WIN:
				Debug.Log($"");
				Debug.log_header("===== Player won =====");
				Debug.Log($"");
				if (trainerBattle()) {
					@scene.TrainerBattleSuccess;
					switch (@opponent.length) {
						case 1:
							DisplayPaused(_INTL("You defeated {1}!", @opponent[0].full_name));
							break;
						case 2:
							DisplayPaused(_INTL("You defeated {1} and {2}!", @opponent[0].full_name,
																		@opponent[1].full_name));
							break;
						case 3:
							DisplayPaused(_INTL("You defeated {1}, {2} and {3}!", @opponent[0].full_name,
																		@opponent[1].full_name, @opponent[2].full_name));
							break;
					}
					@opponent.each_with_index do |trainer, i|
						@scene.ShowOpponent(i);
						msg = trainer.lose_text;
						if (!msg || msg.empty()) msg = "...";
						DisplayPaused(System.Text.RegularExpressions.Regex.Replace(msg, "\\[Pp][Nn]", Player.name));
					}
					Debug.Log($"");
				}
				// Gain money from winning a trainer battle, and from Pay Day
				if (@decision != Outcome.CATCH) GainMoney;
				// Hide remaining trainer
				if (trainerBattle() && @caughtPokemon.length > 0) @scene.ShowOpponent(@opponent.length);
				break;
			case Outcome.LOSE: case Outcome.DRAW:
				Debug.Log($"");
				if (@decision == Outcome.LOSE) Debug.log_header("===== Player lost =====");
				if (@decision == Outcome.DRAW) Debug.log_header("===== Player drew with opponent =====");
				Debug.Log($"");
				if (@internalBattle) {
					if (PlayerBattlerCount == 0) {
						DisplayPaused(_INTL("You have no more Pokémon that can fight!"));
						if (trainerBattle()) {
							switch (@opponent.length) {
								case 1:
									DisplayPaused(_INTL("You lost against {1}!", @opponent[0].full_name));
									break;
								case 2:
									DisplayPaused(_INTL("You lost against {1} and {2}!",
																				@opponent[0].full_name, @opponent[1].full_name));
									break;
								case 3:
									DisplayPaused(_INTL("You lost against {1}, {2} and {3}!",
																				@opponent[0].full_name, @opponent[1].full_name, @opponent[2].full_name));
									break;
							}
						}
					}
					// Lose money from losing a battle
					LoseMoney;
					if (!@canLose && PlayerBattlerCount == 0) DisplayPaused(_INTL("You blacked out!"));
				} else if (@decision == Outcome.LOSE) {   // Lost in a Battle Frontier battle
					if (@opponent) {
						@opponent.each_with_index do |trainer, i|
							@scene.ShowOpponent(i);
							msg = trainer.win_text;
							if (!msg || msg.empty()) msg = "...";
							DisplayPaused(System.Text.RegularExpressions.Regex.Replace(msg, "\\[Pp][Nn]", Player.name));
						}
						Debug.Log($"");
					}
				}
				break;
			case Outcome.CATCH:
				Debug.Log($"");
				Debug.log_header("===== Pokémon caught =====");
				Debug.Log($"");
				if (!Settings.GAIN_EXP_FOR_CAPTURE) @scene.WildBattleSuccess;
				break;
		}
		// Register captured Pokémon in the Pokédex, and store them
		RecordAndStoreCaughtPokemon;
		// Collect Pay Day money in a wild battle that ended in a capture
		if (@decision == Outcome.CATCH) GainMoney;
		// Pass on Pokérus within the party
		if (@internalBattle) {
			infected = new List<string>();
			Game.GameData.player.party.each_with_index do |pkmn, i|
				if (pkmn.pokerusStage == 1) infected.Add(i);
			}
			foreach (var idxParty in infected) { //'infected.each' do => |idxParty|
				strain = Game.GameData.player.party[idxParty].pokerusStrain;
				if (idxParty > 0 && Game.GameData.player.party[idxParty - 1].pokerusStage == 0 && rand(3) == 0) {   // 33%
					Game.GameData.player.party[idxParty - 1].givePokerus(strain);
				}
				if (idxParty < Game.GameData.player.party.length - 1 && Game.GameData.player.party[idxParty + 1].pokerusStage == 0 && rand(3) == 0) {   // 33%
					Game.GameData.player.party[idxParty + 1].givePokerus(strain);
				}
			}
		}
		// Clean up battle stuff
		@scene.EndBattle(@decision);
		@battlers.each do |b|
			if (!b) continue;
			CancelChoice(b.index);   // Restore unused items to Bag
			if (b.abilityActive()) Battle.AbilityEffects.triggerOnSwitchOut(b.ability, b, true);
		}
		Party(0).each_with_index do |pkmn, i|
			if (!pkmn) continue;
			@peer.OnLeavingBattle(self, pkmn, @usedInBattle[0][i], true);   // Reset form
			pkmn.item = @initialItems[0][i];
		}
		return @decision;
	}

	//-----------------------------------------------------------------------------
	// Judging.
	//-----------------------------------------------------------------------------

	public void JudgeCheckpoint(user, move = null) {}

	public void DecisionOnTime() {
		counts   = new {0, 0};
		hpTotals = new {0, 0};
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			foreach (var pkmn in Party(side)) { //'Party(side).each' do => |pkmn|
				if (!pkmn || !pkmn.able()) continue;
				counts[side]   += 1;
				hpTotals[side] += pkmn.hp;
			}
		}
		if (counts[0] > counts[1]) return Outcome.WIN;        // Win (player has more able Pokémon)
		if (counts[0] < counts[1]) return Outcome.LOSE;       // Loss (foe has more able Pokémon)
		if (hpTotals[0] > hpTotals[1]) return Outcome.WIN;    // Win (player has more HP in total)
		if (hpTotals[0] < hpTotals[1]) return Outcome.LOSE;   // Loss (foe has more HP in total)
		return Outcome.DRAW;
	}

	// Unused
	public void DecisionOnTime2() {
		counts   = new {0, 0};
		hpTotals = new {0, 0};
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			foreach (var pkmn in Party(side)) { //'Party(side).each' do => |pkmn|
				if (!pkmn || !pkmn.able()) continue;
				counts[side]   += 1;
				hpTotals[side] += 100 * pkmn.hp / pkmn.totalhp;
			}
			if (counts[side] > 1) hpTotals[side] /= counts[side];
		}
		if (counts[0] > counts[1]) return Outcome.WIN;        // Win (player has more able Pokémon)
		if (counts[0] < counts[1]) return Outcome.LOSE;       // Loss (foe has more able Pokémon)
		if (hpTotals[0] > hpTotals[1]) return Outcome.WIN;    // Win (player has a bigger average HP %)
		if (hpTotals[0] < hpTotals[1]) return Outcome.LOSE;   // Loss (foe has a bigger average HP %)
		return Outcome.DRAW;
	}

	public int DecisionOnDraw { get { return Outcome.DRAW; } }   // Draw

	public void Judge() {
		fainted1 = AllFainted(0);
		fainted2 = AllFainted(1);
		if (fainted1 && fainted2) {
			@decision = DecisionOnDraw;
		} else if (fainted1) {
			@decision = Outcome.LOSE;
		} else if (fainted2) {
			@decision = Outcome.WIN;
		}
	}
}
