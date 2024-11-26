//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Choosing Pokémon to switch.
	//-----------------------------------------------------------------------------

	// Checks whether the replacement Pokémon (at party index idxParty) can enter
	// battle.
	// NOTE: Messages are only shown while in the party screen when choosing a
	//       command for the next round.
	public bool CanSwitchIn(idxBattler, idxParty, partyScene = null) {
		if (idxParty < 0) return true;
		party = Party(idxBattler);
		if (idxParty >= party.length) return false;
		if (!party[idxParty]) return false;
		if (party[idxParty].egg()) {
			partyScene&.Display(_INTL("An Egg can't battle!"));
			return false;
		}
		if (!IsOwner(idxBattler, idxParty)) {
			if (partyScene) {
				owner = GetOwnerFromPartyIndex(idxBattler, idxParty);
				partyScene.Display(_INTL("You can't switch {1}'s Pokémon with one of yours!", owner.name));
			}
			return false;
		}
		if (party[idxParty].fainted()) {
			partyScene&.Display(_INTL("{1} has no energy left to battle!", party[idxParty].name));
			return false;
		}
		if (FindBattler(idxParty, idxBattler)) {
			partyScene&.Display(_INTL("{1} is already in battle!", party[idxParty].name));
			return false;
		}
		return true;
	}

	// Check whether the currently active Pokémon (at battler index idxBattler) can
	// switch out.
	public bool CanSwitchOut(idxBattler, partyScene = null) {
		battler = @battlers[idxBattler];
		if (battler.fainted()) return true;
		// Ability/item effects that allow switching no matter what
		if (battler.abilityActive() && Battle.AbilityEffects.triggerCertainSwitching(battler.ability, battler, self)) {
			return true;
		}
		if (battler.itemActive() && Battle.ItemEffects.triggerCertainSwitching(battler.item, battler, self)) {
			return true;
		}
		// Other certain switching effects
		if (Settings.MORE_TYPE_EFFECTS && battler.Type == Types.GHOST) return true;
		// Other certain trapping effects
		if (battler.trappedInBattle()) {
			partyScene&.Display(_INTL("{1} can't be switched out!", battler.ToString()));
			return false;
		}
		// Trapping abilities/items
		foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
			if (!b.abilityActive()) continue;
			if (Battle.AbilityEffects.triggerTrappingByTarget(b.ability, battler, b, self)) {
				partyScene&.Display(_INTL("{1}'s {2} prevents switching!", b.ToString(), b.abilityName));
				return false;
			}
		}
		foreach (var b in allOtherSideBattlers(idxBattler)) { //'allOtherSideBattlers(idxBattler).each' do => |b|
			if (!b.itemActive()) continue;
			if (Battle.ItemEffects.triggerTrappingByTarget(b.item, battler, b, self)) {
				partyScene&.Display(_INTL("{1}'s {2} prevents switching!", b.ToString(), b.itemName));
				return false;
			}
		}
		return true;
	}

	// Check whether the currently active Pokémon (at battler index idxBattler) can
	// switch out (and that its replacement at party index idxParty can switch in).
	// NOTE: Messages are only shown while in the party screen when choosing a
	//       command for the next round.
	public bool CanSwitch(idxBattler, idxParty = -1, partyScene = null) {
		// Check whether party Pokémon can switch in
		if (!CanSwitchIn(idxBattler, idxParty, partyScene)) return false;
		// Make sure another battler isn't already choosing to switch to the party
		// Pokémon
		foreach (var b in allSameSideBattlers(idxBattler)) { //'allSameSideBattlers(idxBattler).each' do => |b|
			if (choices[b.index].Action != :SwitchOut || choices[b.index].Index != idxParty) continue;
			partyScene&.Display(_INTL("{1} has already been selected.",
																	Party(idxBattler)[idxParty].name));
			return false;
		}
		// Check whether battler can switch out
		return CanSwitchOut(idxBattler, partyScene);
	}

	public bool CanChooseNonActive(idxBattler) {
		Party(idxBattler).each_with_index do |_pkmn, i|
			if (CanSwitchIn(idxBattler, i)) return true;
		}
		return false;
	}

	public void RegisterSwitch(idxBattler, idxParty) {
		if (!CanSwitch(idxBattler, idxParty)) return false;
		@choices[idxBattler].Action = :SwitchOut;
		@choices[idxBattler].Index = idxParty;   // Party index of Pokémon to switch in
		@choices[idxBattler].Move = null;
		return true;
	}

	//-----------------------------------------------------------------------------
	// Open the party screen and potentially pick a replacement Pokémon (or AI
	// chooses replacement).
	//-----------------------------------------------------------------------------

	// Open party screen and potentially choose a Pokémon to switch with. Used in
	// all instances where the party screen is opened.
	public void PartyScreen(idxBattler, checkLaxOnly = false, canCancel = false, shouldRegister = false) {
		ret = -1;
		@scene.PartyScreen(idxBattler, canCancel) do |idxParty, partyScene|
			if (checkLaxOnly) {
				if (!CanSwitchIn(idxBattler, idxParty, partyScene)) next false;
			} else if (!CanSwitch(idxBattler, idxParty, partyScene)) {
				next false;
			}
			if (shouldRegister && (idxParty < 0 || !RegisterSwitch(idxBattler, idxParty))) {
				next false;
			}
			ret = idxParty;
			next true;
		}
		return ret;
	}

	// For choosing a replacement Pokémon when prompted in the middle of other
	// things happening (U-turn, Baton Pass, in def EORSwitch).
	public void SwitchInBetween(idxBattler, checkLaxOnly = false, canCancel = false) {
		if (!@controlPlayer && OwnedByPlayer(idxBattler)) return PartyScreen(idxBattler, checkLaxOnly, canCancel);
		return @battleAI.DefaultChooseNewEnemy(idxBattler);
	}

	//-----------------------------------------------------------------------------
	// Switching Pokémon.
	//-----------------------------------------------------------------------------

	// General switching method that checks if any Pokémon need to be sent out and,
	// if (so, does. Called at the end of each round.) {
	public void EORSwitch(favorDraws = false) {
		if (decided() && !favorDraws) return;
		if (@decision == Outcome.DRAW && favorDraws) return;
		Judge;
		if (decided()) return;
		// Check through each fainted battler to see if that spot can be filled.
		switched = new List<string>();
		do { //loop; while (true);
			switched.clear;
			@battlers.each do |b|
				if (!b || !b.fainted()) continue;
				idxBattler = b.index;
				if (!CanChooseNonActive(idxBattler)) continue;
				if (!OwnedByPlayer(idxBattler)) {   // Opponent/ally is switching in
					if (b.wild()) continue;   // Wild Pokémon can't switch
					idxPartyNew = SwitchInBetween(idxBattler);
					opponent = GetOwnerFromBattlerIndex(idxBattler);
					// NOTE: The player is only offered the chance to switch their own
					//       Pokémon when an opponent replaces a fainted Pokémon in single
					//       battles. In double battles, etc. there is no such offer.
					if (@internalBattle && @switchStyle && trainerBattle() && SideSize(0) == 1 &&
						opposes(idxBattler) && !@battlers[0].fainted() && !switched.Contains(0) &&
						CanChooseNonActive(0) && @battlers[0].effects.Outrage == 0) {
						idxPartyForName = idxPartyNew;
						enemyParty = Party(idxBattler);
						if (enemyParty[idxPartyNew].ability == abilitys.ILLUSION && !CheckGlobalAbility(Abilities.NEUTRALIZINGGAS)) {
							new_index = LastInTeam(idxBattler);
							if (new_index >= 0 && new_index != idxPartyNew) idxPartyForName = new_index;
						}
						if ((DisplayConfirm(_INTL("{1} is about to send out {2}. Will you switch your Pokémon?",
																			opponent.full_name, enemyParty[idxPartyForName].name))) {
							idxPlayerPartyNew = SwitchInBetween(0, false, true);
							if (idxPlayerPartyNew >= 0) {
								MessageOnRecall(@battlers[0]);
								RecallAndReplace(0, idxPlayerPartyNew);
								switched.Add(0);
							}
						}
					}
					RecallAndReplace(idxBattler, idxPartyNew);
					switched.Add(idxBattler);
				} else if (trainerBattle()) {   // Player switches in in a trainer battle
					idxPlayerPartyNew = GetReplacementPokemonIndex(idxBattler);   // Owner chooses
					RecallAndReplace(idxBattler, idxPlayerPartyNew);
					switched.Add(idxBattler);
				} else {   // Player's Pokémon has fainted in a wild battle
					switch = false;
					if (DisplayConfirm(_INTL("Use next Pokémon?"))) {
						switch = true;
					} else {
						switch = (Run(idxBattler, true) <= 0);
					}
					if (switch) {
						idxPlayerPartyNew = GetReplacementPokemonIndex(idxBattler);   // Owner chooses
						RecallAndReplace(idxBattler, idxPlayerPartyNew);
						switched.Add(idxBattler);
					}
				}
			}
			if (switched.length == 0) break;
			OnBattlerEnteringBattle(switched);
		}
	}

	public void GetReplacementPokemonIndex(idxBattler, random = false) {
		if (random) {
			choices = new List<string>();   // Find all Pokémon that can switch in
			eachInTeamFromBattlerIndex(idxBattler) do |_pkmn, i|
				if (CanSwitchIn(idxBattler, i)) choices.Add(i);
			}
			if (choices.length == 0) return -1;
			return choices[Random(choices.length)];
		} else {
			return SwitchInBetween(idxBattler, true);
		}
	}

	// Actually performs the recalling and sending out in all situations.
	public void RecallAndReplace(idxBattler, idxParty, randomReplacement = false, batonPass = false) {
		if (!@battlers[idxBattler].fainted()) @scene.Recall(idxBattler);
		@battlers[idxBattler].AbilitiesOnSwitchOut;   // Inc. primordial weather check
		if (SideSize(idxBattler) == 1) @scene.ShowPartyLineup(idxBattler & 1);
		if (!randomReplacement) MessagesOnReplace(idxBattler, idxParty);
		Replace(idxBattler, idxParty, batonPass);
	}

	public void MessageOnRecall(battler) {
		if (battler.OwnedByPlayer()) {
			if (battler.hp <= battler.totalhp / 4) {
				DisplayBrief(_INTL("Good job, {1}! Come back!", battler.name));
			} else if (battler.hp <= battler.totalhp / 2) {
				DisplayBrief(_INTL("OK, {1}! Come back!", battler.name));
			} else if (battler.turnCount >= 5) {
				DisplayBrief(_INTL("{1}, that's enough! Come back!", battler.name));
			} else if (battler.turnCount >= 2) {
				DisplayBrief(_INTL("{1}, come back!", battler.name));
			} else {
				DisplayBrief(_INTL("{1}, switch out! Come back!", battler.name));
			}
		} else {
			owner = GetOwnerName(battler.index);
			DisplayBrief(_INTL("{1} withdrew {2}!", owner, battler.name));
		}
	}

	// Only called from def RecallAndReplace and Battle Arena's def Switch.
	public void MessagesOnReplace(idxBattler, idxParty) {
		party = Party(idxBattler);
		newPkmnName = party[idxParty].name;
		if (party[idxParty].ability == abilitys.ILLUSION && !CheckGlobalAbility(Abilities.NEUTRALIZINGGAS)) {
			new_index = LastInTeam(idxBattler);
			if (new_index >= 0 && new_index != idxParty) newPkmnName = party[new_index].name;
		}
		if (OwnedByPlayer(idxBattler)) {
			opposing = @battlers[idxBattler].DirectOpposing;
			if (opposing.fainted() || opposing.hp == opposing.totalhp) {
				DisplayBrief(_INTL("You're in charge, {1}!", newPkmnName));
			} else if (opposing.hp >= opposing.totalhp / 2) {
				DisplayBrief(_INTL("Go for it, {1}!", newPkmnName));
			} else if (opposing.hp >= opposing.totalhp / 4) {
				DisplayBrief(_INTL("Just a little more! Hang in there, {1}!", newPkmnName));
			} else {
				DisplayBrief(_INTL("Your opponent's weak! Get 'em, {1}!", newPkmnName));
			}
		} else {
			owner = GetOwnerFromBattlerIndex(idxBattler);
			DisplayBrief(_INTL("{1} sent out {2}!", owner.full_name, newPkmnName));
		}
	}

	// Only called from def RecallAndReplace above and Battle Arena's def
	// Switch.
	public void Replace(idxBattler, idxParty, batonPass = false) {
		party = Party(idxBattler);
		idxPartyOld = @battlers[idxBattler].pokemonIndex;
		// Initialise the new Pokémon
		@battlers[idxBattler].Initialize(party[idxParty], idxParty, batonPass);
		// Reorder the party for this battle
		partyOrder = PartyOrder(idxBattler);
		partyOrder[idxParty], partyOrder[idxPartyOld] = partyOrder[idxPartyOld], partyOrder[idxParty];
		// Send out the new Pokémon
		SendOut(new {idxBattler, party[idxParty]});
		if (Settings.RECALCULATE_TURN_ORDER_AFTER_SPEED_CHANGES) CalculatePriority(false, [idxBattler]);
	}

	// Called from def Replace above and at the start of battle.
	// sendOuts is an array; each element is itself an array: [idxBattler,pkmn]
	public void SendOut(sendOuts, startBattle = false) {
		sendOuts.each(b => @peer.OnEnteringBattle(self, @battlers[b[0]], b[1]));
		@scene.SendOutBattlers(sendOuts, startBattle);
		foreach (var b in sendOuts) { //'sendOuts.each' do => |b|
			@scene.ResetCommandsIndex(b[0]);
			SetSeen(@battlers[b[0]]);
			@usedInBattle[b[0] & 1][b[0] / 2] = true;
		}
	}

	//-----------------------------------------------------------------------------
	// Effects upon a Pokémon entering battle.
	//-----------------------------------------------------------------------------

	// Called at the start of battle only.
	public void OnAllBattlersEnteringBattle() {
		CalculatePriority(true);
		battler_indices = new List<string>();
		allBattlers.each(b => battler_indices.Add(b.index));
		OnBattlerEnteringBattle(battler_indices);
		CalculatePriority;
		// Check forms are correct
		allBattlers.each(b => b.CheckForm);
	}

	// Called when one or more Pokémon switch in. Does a lot of things, including
	// entry hazards, form changes and items/abilities that trigger upon switching
	// in.
	public void OnBattlerEnteringBattle(battler_index, skip_event_reset = false) {
		if (!battler_index.Length > 0) battler_index = [battler_index];
		battler_index.flatten!;
		// NOTE: This isn't done for switch commands, because they previously call
		//       RecallAndReplace, which could cause Neutralizing Gas to end, which
		//       in turn could cause Intimidate to trigger another Pokémon's Eject
		//       Pack. That Eject Pack should trigger at the end of this method, but
		//       this resetting would prevent that from happening, so it is skipped
		//       and instead done earlier in def AttackPhaseSwitch.
		if (!skip_event_reset) {
			foreach (var b in allBattlers) { //'allBattlers.each' do => |b|
				b.droppedBelowHalfHP = false;
				b.statsDropped = false;
			}
		}
		// For each battler that entered battle, in speed order
		foreach (var b in Priority(true)) { //'Priority(true).each' do => |b|
			if (!battler_index.Contains(b.index) || b.fainted()) continue;
			RecordBattlerAsParticipated(b);
			MessagesOnBattlerEnteringBattle(b);
			// Position/field effects triggered by the battler appearing
			EffectsOnBattlerEnteringPosition(b);   // Healing Wish/Lunar Dance
			EntryHazards(b);
			// Battler faints if it is knocked out because of an entry hazard above
			if (b.fainted()) {
				b.Faint;
				GainExp;
				Judge;
				continue;
			}
			b.CheckForm;
			// Primal Revert upon entering battle
			PrimalReversion(b.index);
			// Ending primordial weather, checking Trace
			b.ContinualAbilityChecks(true);
			// Abilities that trigger upon switching in
			if ((!b.fainted() && b.unstoppableAbility()) || b.abilityActive()) {
				Battle.AbilityEffects.triggerOnSwitchIn(b.ability, b, self, true);
			}
			EndPrimordialWeather;   // Checking this again just in case
			// Items that trigger upon switching in (Air Balloon message)
			if (b.itemActive()) {
				Battle.ItemEffects.triggerOnSwitchIn(b.item, b, self);
			}
			// Berry check, status-curing ability check
			b.HeldItemTriggerCheck;
			b.AbilityStatusCureCheck;
		}
		// Check for triggering of Emergency Exit/Wimp Out/Eject Pack (only one will
		// be triggered)
		foreach (var b in Priority(true)) { //'Priority(true).each' do => |b|
			if (b.ItemOnStatDropped) break;
			if (b.AbilitiesOnDamageTaken) break;
		}
		foreach (var b in allBattlers) { //'allBattlers.each' do => |b|
			b.droppedBelowHalfHP = false;
			b.statsDropped = false;
		}
	}

	public void RecordBattlerAsParticipated(battler) {
		// Record money-doubling effect of Amulet Coin/Luck Incense
		if (!battler.opposes() && new []{:AMULETCOIN, :LUCKINCENSE}.Contains(battler.item_id)) {
			@field.effects.AmuletCoin = true;
		}
		// Update battlers' participants (who will gain Exp/EVs when a battler faints)
		allBattlers.each(b => b.UpdateParticipants);
	}

	public void MessagesOnBattlerEnteringBattle(battler) {
		// Introduce Shadow Pokémon
		if (battler.shadowPokemon()) {
			CommonAnimation("Shadow", battler);
			if (battler.opposes()) Display(_INTL("Oh!\nA Shadow Pokémon!"));
		}
	}

	// Called when a Pokémon enters battle, and when Ally Switch is used.
	public void EffectsOnBattlerEnteringPosition(battler) {
		position = @positions[battler.index];
		// Healing Wish
		if (position.effects.HealingWish) {
			if (battler.canHeal() || battler.status != statuses.NONE) {
				CommonAnimation("HealingWish", battler);
				Display(_INTL("The healing wish came true for {1}!", battler.ToString(true)));
				battler.RecoverHP(battler.totalhp);
				battler.CureStatus(false);
				position.effects.HealingWish = false;
			} else if (Settings.MECHANICS_GENERATION < 8) {
				position.effects.HealingWish = false;
			}
		}
		// Lunar Dance
		if (position.effects.LunarDance) {
			full_pp = true;
			battler.eachMove(m => { if (m.pp < m.total_pp) full_pp = false; });
			if (battler.canHeal() || battler.status != statuses.NONE || !full_pp) {
				CommonAnimation("LunarDance", battler);
				Display(_INTL("{1} became cloaked in mystical moonlight!", battler.ToString()));
				battler.RecoverHP(battler.totalhp);
				battler.CureStatus(false);
				battler.eachMove(m => battler.SetPP(m, m.total_pp));
				position.effects.LunarDance = false;
			} else if (Settings.MECHANICS_GENERATION < 8) {
				position.effects.LunarDance = false;
			}
		}
	}

	public void EntryHazards(battler) {
		battler_side = battler.OwnSide;
		// Stealth Rock
		if (battler_side.effects.StealthRock && battler.takesIndirectDamage() &&
			GameData.Types.exists(Types.ROCK) && !battler.hasActiveItem(Items.HEAVYDUTYBOOTS)) {
			bTypes = battler.Types(true);
			eff = Effectiveness.calculate(:ROCK, *bTypes);
			if (!Effectiveness.ineffective(eff)) {
				battler.ReduceHP(battler.totalhp * eff / 8, false);
				Display(_INTL("Pointed stones dug into {1}!", battler.ToString()));
				battler.ItemHPHealCheck;
			}
		}
		// Spikes
		if (battler_side.effects.Spikes > 0 && battler.takesIndirectDamage() &&
			!battler.airborne() && !battler.hasActiveItem(Items.HEAVYDUTYBOOTS)) {
			spikesDiv = new {8, 6, 4}[battler_side.effects.Spikes - 1];
			battler.ReduceHP(battler.totalhp / spikesDiv, false);
			Display(_INTL("{1} is hurt by the spikes!", battler.ToString()));
			battler.ItemHPHealCheck;
		}
		// Toxic Spikes
		if (battler_side.effects.ToxicSpikes > 0 && !battler.fainted() && !battler.airborne()) {
			if (battler.Type == Types.POISON) {
				battler_side.effects.ToxicSpikes = 0;
				Display(_INTL("{1} absorbed the poison spikes!", battler.ToString()));
			} else if (battler.CanPoison(null, false) && !battler.hasActiveItem(Items.HEAVYDUTYBOOTS)) {
				if (battler_side.effects.ToxicSpikes == 2) {
					battler.Poison(null, _INTL("{1} was badly poisoned by the poison spikes!", battler.ToString()), true);
				} else {
					battler.Poison(null, _INTL("{1} was poisoned by the poison spikes!", battler.ToString()));
				}
			}
		}
		// Sticky Web
		if (battler_side.effects.StickyWeb && !battler.fainted() && !battler.airborne() &&
			!battler.hasActiveItem(Items.HEAVYDUTYBOOTS)) {
			Display(_INTL("{1} was caught in a sticky web!", battler.ToString()));
			if (battler.CanLowerStatStage(:SPEED)) {
				battler.LowerStatStage(:SPEED, 1, null);
				battler.ItemStatRestoreCheck;
			}
		}
	}
}
