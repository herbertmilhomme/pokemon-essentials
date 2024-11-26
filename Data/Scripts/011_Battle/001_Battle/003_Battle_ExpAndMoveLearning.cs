//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Gaining Experience.
	//-----------------------------------------------------------------------------

	public void GainExp() {
		// Play wild victory music if it's the end of the battle (has to be here)
		if (wildBattle() && AllFainted(1) && !AllFainted(0)) @scene.WildBattleSuccess;
		if (!@internalBattle || !@expGain) return;
		// Go through each battler in turn to find the Pokémon that participated in
		// battle against it, and award those Pokémon Exp/EVs
		expAll = Game.GameData.player.has_exp_all || Game.GameData.bag.has(:EXPALL);
		p1 = Party(0);
		@battlers.each do |b|
			unless (b&.opposes()) continue;   // Can only gain Exp from fainted foes
			if (b.participants.length == 0) continue;
			unless (b.fainted() || b.captured) continue;
			// Count the number of participants
			numPartic = 0;
			foreach (var partic in b.participants) { //'b.participants.each' do => |partic|
				unless (p1[partic]&.able() && IsOwner(0, partic)) continue;
				numPartic += 1;
			}
			// Find which Pokémon have an Exp Share
			expShare = new List<string>();
			if (!expAll) {
				eachInTeam(0, 0) do |pkmn, i|
					if (!pkmn.able()) continue;
					if (!pkmn.hasItem(Items.EXPSHARE) && GameData.Item.try_get(@initialItems[0][i]) != :EXPSHARE) continue;
					expShare.Add(i);
				}
			}
			// Calculate EV and Exp gains for the participants
			if (numPartic > 0 || expShare.length > 0 || expAll) {
				// Gain EVs and Exp for participants
				eachInTeam(0, 0) do |pkmn, i|
					if (!pkmn.able()) continue;
					unless (b.participants.Contains(i) || expShare.Contains(i)) continue;
					GainEVsOne(i, b);
					GainExpOne(i, b, numPartic, expShare, expAll, !pkmn.shadowPokemon());
				}
				// Gain EVs and Exp for all other Pokémon because of Exp All
				if (expAll) {
					showMessage = true;
					eachInTeam(0, 0) do |pkmn, i|
						if (!pkmn.able()) continue;
						if (b.participants.Contains(i) || expShare.Contains(i)) continue;
						if (showMessage) DisplayPaused(_INTL("Your other Pokémon also gained Exp. Points!"));
						showMessage = false;
						GainEVsOne(i, b);
						GainExpOne(i, b, numPartic, expShare, expAll, false);
					}
				}
			}
			// Clear the participants array
			b.participants = new List<string>();
		}
	}

	public void GainEVsOne(idxParty, defeatedBattler) {
		pkmn = Party(0)[idxParty];   // The Pokémon gaining EVs from defeatedBattler
		evYield = defeatedBattler.pokemon.evYield;
		// Num of effort points pkmn already has
		evTotal = 0;
		GameData.Stat.each_main(s => evTotal += pkmn.ev[s.id]);
		// Modify EV yield based on pkmn's held item
		if (!Battle.ItemEffects.triggerEVGainModifier(pkmn.item, pkmn, evYield)) {
			Battle.ItemEffects.triggerEVGainModifier(@initialItems[0][idxParty], pkmn, evYield);
		}
		// Double EV gain because of Pokérus
		if (pkmn.pokerusStage >= 1) {   // Infected or cured
			evYield.each_key(stat => evYield[stat] *= 2);
		}
		// Gain EVs for each stat in turn
		if (pkmn.shadowPokemon() && pkmn.heartStage <= 3 && pkmn.saved_ev) {
			pkmn.saved_ev.each_value(e => evTotal += e);
			foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
				evGain = evYield[s.id].clamp(0, Pokemon.EV_STAT_LIMIT - pkmn.ev[s.id] - pkmn.saved_ev[s.id]);
				evGain = evGain.clamp(0, Pokemon.EV_LIMIT - evTotal);
				pkmn.saved_ev[s.id] += evGain;
				evTotal += evGain;
			}
		} else {
			foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
				evGain = evYield[s.id].clamp(0, Pokemon.EV_STAT_LIMIT - pkmn.ev[s.id]);
				evGain = evGain.clamp(0, Pokemon.EV_LIMIT - evTotal);
				pkmn.ev[s.id] += evGain;
				evTotal += evGain;
			}
		}
	}

	public void GainExpOne(idxParty, defeatedBattler, numPartic, expShare, expAll, showMessages = true) {
		pkmn = Party(0)[idxParty];   // The Pokémon gaining Exp from defeatedBattler
		growth_rate = pkmn.growth_rate;
		// Don't bother calculating if gainer is already at max Exp
		if (pkmn.exp >= growth_rate.maximum_exp) {
			pkmn.calc_stats;   // To ensure new EVs still have an effect
			return;
		}
		isPartic    = defeatedBattler.participants.Contains(idxParty);
		hasExpShare = expShare.Contains(idxParty);
		level = defeatedBattler.level;
		// Main Exp calculation
		exp = 0;
		a = level * defeatedBattler.pokemon.base_exp;
		if (expShare.length > 0 && (isPartic || hasExpShare)) {
			if (numPartic == 0) {   // No participants, all Exp goes to Exp Share holders
				exp = a / (Settings.SPLIT_EXP_BETWEEN_GAINERS ? expShare.length : 1);
			} else if (Settings.SPLIT_EXP_BETWEEN_GAINERS) {   // Gain from participating and/or Exp Share
				if (isPartic) exp = a / (2 * numPartic);
				if (hasExpShare) exp += a / (2 * expShare.length);
			} else {   // Gain from participating and/or Exp Share (Exp not split)
				exp = (isPartic) ? a : a / 2;
			}
		} else if (isPartic) {   // Participated in battle, no Exp Shares held by anyone
			exp = a / (Settings.SPLIT_EXP_BETWEEN_GAINERS ? numPartic : 1);
		} else if (expAll) {   // Didn't participate in battle, gaining Exp due to Exp All
			// NOTE: Exp All works like the Exp Share from Gen 6+, not like the Exp All
			//       from Gen 1, i.e. Exp isn't split between all Pokémon gaining it.
			exp = a / 2;
		}
		if (exp <= 0) return;
		// Pokémon gain more Exp from trainer battles
		if (Settings.MORE_EXP_FROM_TRAINER_POKEMON && trainerBattle()) exp = (int)Math.Floor(exp * 1.5);
		// Scale the gained Exp based on the gainer's level (or not)
		if (Settings.SCALED_EXP_FORMULA) {
			exp /= 5;
			levelAdjust = ((2 * level) + 10.0) / (pkmn.level + level + 10.0);
			levelAdjust **= 5;
			levelAdjust = Math.sqrt(levelAdjust);
			exp *= levelAdjust;
			exp = exp.floor;
			if (isPartic || hasExpShare) exp += 1;
		} else {
			exp /= 7;
		}
		// Foreign Pokémon gain more Exp
		isOutsider = (pkmn.owner.id != Player.id ||
								(pkmn.owner.language != 0 && pkmn.owner.language != Player.language))
		if (isOutsider) {
			if (pkmn.owner.language != 0 && pkmn.owner.language != Player.language) {
				exp = (int)Math.Floor(exp * 1.7);
			} else {
				exp = (int)Math.Floor(exp * 1.5);
			}
		}
		// Exp. Charm increases Exp gained
		if (Game.GameData.bag.has(:EXPCHARM)) exp = exp * 3 / 2;
		// Modify Exp gain based on pkmn's held item
		i = Battle.ItemEffects.triggerExpGainModifier(pkmn.item, pkmn, exp);
		if (i < 0) {
			i = Battle.ItemEffects.triggerExpGainModifier(@initialItems[0][idxParty], pkmn, exp);
		}
		if (i >= 0) exp = i;
		// Boost Exp gained with high affection
		if (Settings.AFFECTION_EFFECTS && @internalBattle && pkmn.affection_level >= 4 && !pkmn.mega()) {
			exp = exp * 6 / 5;
			isOutsider = true;   // To show the "boosted Exp" message
		}
		// Make sure Exp doesn't exceed the maximum
		expFinal = growth_rate.add_exp(pkmn.exp, exp);
		expGained = expFinal - pkmn.exp;
		if (expGained <= 0) return;
		// "Exp gained" message
		if (showMessages) {
			if (isOutsider) {
				DisplayPaused(_INTL("{1} got a boosted {2} Exp. Points!", pkmn.name, expGained));
			} else {
				DisplayPaused(_INTL("{1} got {2} Exp. Points!", pkmn.name, expGained));
			}
		}
		curLevel = pkmn.level;
		newLevel = growth_rate.level_from_exp(expFinal);
		if (newLevel < curLevel) {
			debugInfo = $"Levels: {curLevel}->{newLevel} | Exp: {pkmn.exp}->{expFinal} | gain: {expGained}";
			Debug.LogError(_INTL("{1}'s new level is less than its current level, which shouldn't happen.", pkmn.name) + $"\n[{debugInfo}]");
			//throw new Exception(_INTL("{1}'s new level is less than its current level, which shouldn't happen.", pkmn.name) + $"\n[{debugInfo}]");
		}
		// Give Exp
		if (pkmn.shadowPokemon()) {
			if (pkmn.heartStage <= 3) {
				pkmn.exp += expGained;
				Game.GameData.stats.total_exp_gained += expGained;
			}
			return;
		}
		Game.GameData.stats.total_exp_gained += expGained;
		tempExp1 = pkmn.exp;
		battler = FindBattler(idxParty);
		do { //loop; while (true);   // For each level gained in turn...
			// EXP Bar animation
			levelMinExp = growth_rate.minimum_exp_for_level(curLevel);
			levelMaxExp = growth_rate.minimum_exp_for_level(curLevel + 1);
			tempExp2 = (levelMaxExp < expFinal) ? levelMaxExp : expFinal;
			pkmn.exp = tempExp2;
			@scene.EXPBar(battler, levelMinExp, levelMaxExp, tempExp1, tempExp2);
			tempExp1 = tempExp2;
			curLevel += 1;
			if (curLevel > newLevel) {
				// Gained all the Exp now, } the animation
				pkmn.calc_stats;
				battler&.Update(false);
				if (battler) @scene.RefreshOne(battler.index);
				break;
			}
			// Levelled up
			if (battler) CommonAnimation("LevelUp", battler);
			oldTotalHP = pkmn.totalhp;
			oldAttack  = pkmn.attack;
			oldDefense = pkmn.defense;
			oldSpAtk   = pkmn.spatk;
			oldSpDef   = pkmn.spdef;
			oldSpeed   = pkmn.speed;
			if (battler&.pokemon) battler.pokemon.changeHappiness("levelup");
			pkmn.calc_stats;
			battler&.Update(false);
			if (battler) @scene.RefreshOne(battler.index);
			DisplayPaused(_INTL("{1} grew to Lv. {2}!", pkmn.name, curLevel)) { SEPlay("Pkmn level up") };
			@scene.LevelUp(pkmn, battler, oldTotalHP, oldAttack, oldDefense,
											oldSpAtk, oldSpDef, oldSpeed);
			// Learn all moves learned at this level
			moveList = pkmn.getMoveList;
			moveList.each(m => { if (m[0] == curLevel) LearnMove(idxParty, m[1]); });
		}
	}

	//-----------------------------------------------------------------------------
	// Learning a move.
	//-----------------------------------------------------------------------------

	public void LearnMove(idxParty, newMove) {
		pkmn = Party(0)[idxParty];
		if (!pkmn) return;
		pkmnName = pkmn.name;
		battler = FindBattler(idxParty);
		moveName = GameData.Move.get(newMove).name;
		// Pokémon already knows the move
		if (pkmn.hasMove(newMove)) return;
		// Pokémon has space for the new move; just learn it
		if (pkmn.numMoves < Pokemon.MAX_MOVES) {
			pkmn.learn_move(newMove);
			Display(_INTL("{1} learned {2}!", pkmnName, moveName)) { SEPlay("Pkmn move learnt") };
			if (battler) {
				battler.moves.Add(Move.from_pokemon_move(self, pkmn.moves.last));
				battler.CheckFormOnMovesetChange;
			}
			return;
		}
		// Pokémon already knows the maximum number of moves; try to forget one to learn the new move
		DisplayPaused(_INTL("{1} wants to learn {2}, but it already knows {3} moves.",
													pkmnName, moveName, pkmn.numMoves.to_word));
		if (DisplayConfirm(_INTL("Should {1} forget a move to learn {2}?", pkmnName, moveName))) {
			do { //loop; while (true);
				forgetMove = @scene.ForgetMove(pkmn, newMove)
				if (forgetMove >= 0) {
					oldMoveName = pkmn.moves[forgetMove].name;
					pkmn.moves[forgetMove] = new Pokemon.Move(newMove);   // Replaces current/total PP
					if (battler) battler.moves[forgetMove] = Move.from_pokemon_move(self, pkmn.moves[forgetMove]);
					DisplayPaused(_INTL("1, 2, and... ... ... Ta-da!")) { SEPlay("Battle ball drop") };
					DisplayPaused(_INTL("{1} forgot how to use {2}. And...", pkmnName, oldMoveName));
					Display(_INTL("{1} learned {2}!", pkmnName, moveName)) { SEPlay("Pkmn move learnt") };
					battler&.CheckFormOnMovesetChange;
					break;
				} else if (DisplayConfirm(_INTL("Give up on learning {1}?", moveName))) {
					Display(_INTL("{1} did not learn {2}.", pkmnName, moveName));
					break;
				}
			}
		} else {
			Display(_INTL("{1} did not learn {2}.", pkmnName, moveName));
		}
	}
}
