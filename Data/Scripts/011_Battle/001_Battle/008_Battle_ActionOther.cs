//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// Shifting a battler to another position in a battle larger than double.
	//-----------------------------------------------------------------------------

	public bool CanShift(idxBattler) {
		if (SideSize(0) <= 2 && SideSize(1) <= 2) return false;   // Double battle or smaller
		idxOther = -1;
		switch (SideSize(idxBattler)) {
			case 1:
				return false;   // Only one battler on that side
			case 2:
				idxOther = (idxBattler + 2) % 4;
				break;
			case 3:
				if (new []{2, 3}.Contains(idxBattler)) return false;   // In middle spot already
				idxOther = (idxBattler.even()) ? 2 : 3;
				break;
		}
		if (GetOwnerIndexFromBattlerIndex(idxBattler) != GetOwnerIndexFromBattlerIndex(idxOther)) return false;
		return true;
	}

	public void RegisterShift(idxBattler) {
		@choices[idxBattler].Action = :Shift;
		@choices[idxBattler].Index = 0;
		@choices[idxBattler].Move = null;
		return true;
	}

	//-----------------------------------------------------------------------------
	// Calling at a battler.
	//-----------------------------------------------------------------------------

	public void RegisterCall(idxBattler) {
		@choices[idxBattler].Action = :Call;
		@choices[idxBattler].Index = 0;
		@choices[idxBattler].Move = null;
		return true;
	}

	public void Call(idxBattler) {
		// Debug ending the battle
		if (DebugRun != 0) return;
		// Call the battler
		battler = @battlers[idxBattler];
		trainerName = GetOwnerName(idxBattler);
		Display(_INTL("{1} called {2}!", trainerName, battler.ToString(true)));
		Display(_INTL("{1}!", battler.name));
		if (battler.shadowPokemon()) {
			if (battler.inHyperMode()) {
				battler.pokemon.hyper_mode = false;
				battler.pokemon.change_heart_gauge("call");
				Display(_INTL("{1} came to its senses from the Trainer's call!", battler.ToString()));
			} else {
				Display(_INTL("But nothing happened!"));
			}
		} else if (battler.status == statuses.SLEEP) {
			battler.CureStatus;
		} else if (battler.CanRaiseStatStage(:ACCURACY, battler)) {
			battler.RaiseStatStage(:ACCURACY, 1, battler);
			battler.ItemOnStatDropped;
		} else {
			Display(_INTL("But nothing happened!"));
		}
	}

	//-----------------------------------------------------------------------------
	// Choosing to Mega Evolve a battler.
	//-----------------------------------------------------------------------------

	public bool HasMegaRing(idxBattler) {
		if (OwnedByPlayer(idxBattler)) {
			@mega_rings.each(item => { if (Game.GameData.bag.has(item)) return true; });
		} else {
			trainer_items = GetOwnerItems(idxBattler);
			if (!trainer_items) return false;
			@mega_rings.each(item => { if (trainer_items.Contains(item)) return true; });
		}
		return false;
	}

	public void GetMegaRingName(idxBattler) {
		if (!@mega_rings.empty()) {
			if (OwnedByPlayer(idxBattler)) {
				@mega_rings.each(item => { if (Game.GameData.bag.has(item)) return GameData.Item.get(item).name; });
			} else {
				trainer_items = GetOwnerItems(idxBattler);
				if (trainer_items) {
					@mega_rings.each(item => { if (trainer_items.Contains(item)) return GameData.Item.get(item).name; });
				}
			}
		}
		return _INTL("Mega Ring");
	}

	public bool CanMegaEvolve(idxBattler) {
		if (Game.GameData.game_switches[Settings.NO_MEGA_EVOLUTION]) return false;
		if (!@battlers[idxBattler].hasMega()) return false;
		if (@battlers[idxBattler].wild()) return false;
		if (Core.DEBUG && Input.press(Input.CTRL)) return true;
		if (@battlers[idxBattler].effects.SkyDrop >= 0) return false;
		if (!HasMegaRing(idxBattler)) return false;
		side  = @battlers[idxBattler].idxOwnSide;
		owner = GetOwnerIndexFromBattlerIndex(idxBattler);
		return @megaEvolution[side][owner] == -1;
	}

	public void RegisterMegaEvolution(idxBattler) {
		side  = @battlers[idxBattler].idxOwnSide;
		owner = GetOwnerIndexFromBattlerIndex(idxBattler);
		@megaEvolution[side][owner] = idxBattler;
	}

	public void UnregisterMegaEvolution(idxBattler) {
		side  = @battlers[idxBattler].idxOwnSide;
		owner = GetOwnerIndexFromBattlerIndex(idxBattler);
		if (@megaEvolution[side][owner] == idxBattler) @megaEvolution[side][owner] = -1;
	}

	public void ToggleRegisteredMegaEvolution(idxBattler) {
		side  = @battlers[idxBattler].idxOwnSide;
		owner = GetOwnerIndexFromBattlerIndex(idxBattler);
		if (@megaEvolution[side][owner] == idxBattler) {
			@megaEvolution[side][owner] = -1;
		} else {
			@megaEvolution[side][owner] = idxBattler;
		}
	}

	public bool RegisteredMegaEvolution(idxBattler) {
		side  = @battlers[idxBattler].idxOwnSide;
		owner = GetOwnerIndexFromBattlerIndex(idxBattler);
		return @megaEvolution[side][owner] == idxBattler;
	}

	//-----------------------------------------------------------------------------
	// Mega Evolving a battler.
	//-----------------------------------------------------------------------------

	public void MegaEvolve(idxBattler) {
		battler = @battlers[idxBattler];
		if (!battler || !battler.pokemon) return;
		if (!battler.hasMega() || battler.mega()) return;
		if (battler.OwnedByPlayer()) Game.GameData.stats.mega_evolution_count += 1;
		trainerName = GetOwnerName(idxBattler);
		old_ability = battler.ability_id;
		// Break Illusion
		if (battler.hasActiveAbility(Abilitys.ILLUSION)) {
			Battle.AbilityEffects.triggerOnBeingHit(battler.ability, null, battler, null, self);
		}
		// Mega Evolve
		switch (battler.pokemon.megaMessage) {
			case 1:   // Rayquaza
				Display(_INTL("{1}'s fervent wish has reached {2}!", trainerName, battler.ToString()));
				break;
			default:
				Display(_INTL("{1}'s {2} is reacting to {3}'s {4}!",
												battler.ToString(), battler.itemName, trainerName, GetMegaRingName(idxBattler)));
				break;
		}
		CommonAnimation("MegaEvolution", battler);
		battler.pokemon.makeMega;
		battler.form = battler.pokemon.form;
		battler.Update(true);
		@scene.ChangePokemon(battler, battler.pokemon);
		@scene.RefreshOne(idxBattler);
		CommonAnimation("MegaEvolution2", battler);
		megaName = battler.pokemon.megaName;
		if (nil_or_empty(megaName)) megaName = _INTL("Mega {1}", battler.pokemon.speciesName);
		Display(_INTL("{1} has Mega Evolved into {2}!", battler.ToString(), megaName));
		side  = battler.idxOwnSide;
		owner = GetOwnerIndexFromBattlerIndex(idxBattler);
		@megaEvolution[side][owner] = -2;
		if (battler.isSpecies(Speciess.GENGAR) && battler.mega()) {
			battler.effects.Telekinesis = 0;
		}
		// Trigger ability
		battler.OnLosingAbility(old_ability);
		battler.TriggerAbilityOnGainingIt;
		// Recalculate turn order
		if (Settings.RECALCULATE_TURN_ORDER_AFTER_MEGA_EVOLUTION) CalculatePriority(false, [idxBattler]);
	}

	//-----------------------------------------------------------------------------
	// Primal Reverting a battler.
	//-----------------------------------------------------------------------------

	public void PrimalReversion(idxBattler) {
		battler = @battlers[idxBattler];
		if (!battler || !battler.pokemon || battler.fainted()) return;
		if (!battler.hasPrimal() || battler.primal()) return;
		if (battler.OwnedByPlayer()) Game.GameData.stats.primal_reversion_count += 1;
		if (battler.isSpecies(Speciess.KYOGRE)) {
			CommonAnimation("PrimalKyogre", battler);
		} else if (battler.isSpecies(Speciess.GROUDON)) {
			CommonAnimation("PrimalGroudon", battler);
		}
		battler.pokemon.makePrimal;
		battler.form = battler.pokemon.form;
		battler.Update(true);
		@scene.ChangePokemon(battler, battler.pokemon);
		@scene.RefreshOne(idxBattler);
		if (battler.isSpecies(Speciess.KYOGRE)) {
			CommonAnimation("PrimalKyogre2", battler);
		} else if (battler.isSpecies(Speciess.GROUDON)) {
			CommonAnimation("PrimalGroudon2", battler);
		}
		Display(_INTL("{1}'s Primal Reversion!\nIt reverted to its primal form!", battler.ToString()));
	}
}
