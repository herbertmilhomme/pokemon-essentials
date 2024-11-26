//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Change HP.
	//-----------------------------------------------------------------------------

	public void ReduceHP(amt, anim = true, registerDamage = true, anyAnim = true) {
		amt = amt.round;
		if (amt > @hp) amt = @hp;
		if (amt < 1 && !fainted()) amt = 1;
		oldHP = @hp;
		self.hp -= amt;
		if (amt > 0) Debug.Log($"[HP change] {This} lost {amt} HP ({oldHP} -> {@hp})");
		if (@hp < 0) raise _INTL("HP less than 0");
		if (@hp > @totalhp) raise _INTL("HP greater than total HP");
		if (anyAnim && amt > 0) @battle.scene.HPChanged(self, oldHP, anim);
		if (amt > 0 && registerDamage) {
			if (@hp < @totalhp / 2 && @hp + amt >= @totalhp / 2) @droppedBelowHalfHP = true;
			@tookDamageThisRound = true;
			@tookMoveDamageThisRound = true;
		}
		return amt;
	}

	public void RecoverHP(amt, anim = true, anyAnim = true) {
		amt = amt.round;
		if (amt > @totalhp - @hp) amt = @totalhp - @hp;
		if (amt < 1 && @hp < @totalhp) amt = 1;
		oldHP = @hp;
		self.hp += amt;
		if (amt > 0) Debug.Log($"[HP change] {This} gained {amt} HP ({oldHP} -> {@hp})");
		if (@hp < 0) raise _INTL("HP less than 0");
		if (@hp > @totalhp) raise _INTL("HP greater than total HP");
		if (anyAnim && amt > 0) @battle.scene.HPChanged(self, oldHP, anim);
		if (@hp >= @totalhp / 2) @droppedBelowHalfHP = false;
		return amt;
	}

	public void RecoverHPFromDrain(amt, target, msg = null) {
		if (target.hasActiveAbility(:LIQUIDOOZE, true)) {
			@battle.ShowAbilitySplash(target);
			ReduceHP(amt);
			@battle.Display(_INTL("{1} sucked up the liquid ooze!", This));
			@battle.HideAbilitySplash(target);
			ItemHPHealCheck;
		} else {
			if (nil_or_empty(msg)) msg = _INTL("{1} had its energy drained!", target.ToString());
			@battle.Display(msg);
			if (canHeal()) {
				if (hasActiveItem(Items.BIGROOT)) amt = (int)Math.Floor(amt * 1.3);
				RecoverHP(amt);
			}
		}
	}

	public void TakeEffectDamage(amt, show_anim = true) {
		@droppedBelowHalfHP = false;
		hp_lost = ReduceHP(amt, show_anim);
		if (block_given()) yield hp_lost;   // Show message
		ItemHPHealCheck;
		AbilitiesOnDamageTaken;
		if (fainted()) Faint;
		@droppedBelowHalfHP = false;
	}

	public void Faint(showMessage = true) {
		if (!fainted()) {
			Debug.Log($"!!!***Can't faint with HP greater than 0");
			return;
		}
		if (@fainted) return;   // Has already fainted properly
		if (showMessage) @battle.DisplayBrief(_INTL("{1} fainted!", This));
		if (!showMessage) Debug.Log($"[Pokémon fainted] {This} ({@index})");
		@battle.scene.FaintBattler(self);
		if (opposes()) @battle.SetDefeated(self);
		InitEffects(false);
		// Reset status
		self.status      = :NONE;
		self.statusCount = 0;
		// Lose happiness
		if (@pokemon && @battle.internalBattle) {
			badLoss = @battle.allOtherSideBattlers(@index).any(b => b.level >= self.level + 30);
			@pokemon.changeHappiness((badLoss) ? "faintbad" : "faint");
		}
		// Reset form
		@battle.peer.OnLeavingBattle(@battle, @pokemon, @battle.usedInBattle[idxOwnSide][@index / 2]);
		if (mega()) @pokemon.makeUnmega;
		if (primal()) @pokemon.makeUnprimal;
		// Do other things
		@battle.ClearChoice(@index);   // Reset choice
		OwnSide.effects.LastRoundFainted = @battle.turnCount;
		// Check other battlers' abilities that trigger upon a battler fainting
		AbilitiesOnFainting;
		// Check for end of primordial weather
		@battle.EndPrimordialWeather;
	}

	//-----------------------------------------------------------------------------
	// Move PP.
	//-----------------------------------------------------------------------------

	public void SetPP(move, pp) {
		move.pp = pp;
		// No need to care about @effects.Mimic, since Mimic can't copy
		// Mimic
		if (move.realMove && move.id == move.realMove.id && !@effects.Transform) {
			move.realMove.pp = pp;
		}
	}

	public void ReducePP(move) {
		if (usingMultiTurnAttack()) return true;
		if (move.pp < 0) return true;          // Don't reduce PP for special calls of moves
		if (move.total_pp <= 0) return true;   // Infinite PP, can always be used
		if (move.pp == 0) return false;        // Ran out of PP, couldn't reduce
		if (move.pp > 0) SetPP(move, move.pp - 1);
		return true;
	}

	public void ReducePPOther(move) {
		if (move.pp > 0) SetPP(move, move.pp - 1);
	}

	//-----------------------------------------------------------------------------
	// Change type.
	//-----------------------------------------------------------------------------

	public void ChangeTypes(newType) {
		if (newType.is_a(Battle.Battler)) {
			newTypes = newType.Types;
			if (newTypes.length == 0) newTypes.Add(:NORMAL);
			newExtraType = newType.effects.ExtraType;
			if (newTypes.Contains(newExtraType)) newExtraType = null;
			@types = newTypes.clone;
			@effects.ExtraType = newExtraType;
		} else {
			newType = GameData.Type.get(newType).id;
			@types = [newType];
			@effects.ExtraType = null;
		}
		@effects.BurnUp = false;
		@effects.DoubleShock = false;
		@effects.Roost  = false;
	}

	public void ResetTypes() {
		@types = @pokemon.types;
		@effects.ExtraType = null;
		@effects.BurnUp = false;
		@effects.DoubleShock = false;
		@effects.Roost  = false;
	}

	//-----------------------------------------------------------------------------
	// Forms.
	//-----------------------------------------------------------------------------

	public void ChangeForm(newForm, msg) {
		if (fainted() || @effects.Transform || @form == newForm) return;
		oldForm = @form;
		oldDmg = @totalhp - @hp;
		self.form = newForm;
		Update(true);
		@hp = @totalhp - oldDmg;
		if (Settings.MECHANICS_GENERATION >= 6) @effects.WeightChange = 0;
		@battle.scene.ChangePokemon(self, @pokemon);
		@battle.scene.RefreshOne(@index);
		if (msg && msg != "") @battle.Display(msg);
		Debug.Log($"[Form changed] {This} changed from form {oldForm} to form {newForm}");
		@battle.SetSeen(self);
	}

	public void CheckFormOnStatusChange() {
		if (fainted() || @effects.Transform) return;
		// Shaymin - reverts if frozen
		if (isSpecies(Speciess.SHAYMIN) && frozen()) {
			ChangeForm(0, _INTL("{1} transformed!", This));
		}
	}

	public void CheckFormOnMovesetChange() {
		if (fainted() || @effects.Transform) return;
		// Keldeo - knowing Secret Sword
		if (isSpecies(Speciess.KELDEO)) {
			newForm = 0;
			if (HasMove(Moves.SECRETSWORD)) newForm = 1;
			ChangeForm(newForm, _INTL("{1} transformed!", This));
		}
	}

	public void CheckFormOnWeatherChange(ability_changed = false) {
		if (fainted() || @effects.Transform) return;
		// Castform - Forecast
		if (isSpecies(Speciess.CASTFORM)) {
			if (hasActiveAbility(Abilitys.FORECAST)) {
				newForm = 0;
				switch (effectiveWeather) {
					case :Sun: case :HarshSun:   newForm = 1; break;
					case :Rain: case :HeavyRain: newForm = 2; break;
					case :Hail: case :Snowstorm: newForm = 3; break;
				}
				if (@form != newForm) {
					@battle.ShowAbilitySplash(self, true);
					@battle.HideAbilitySplash(self);
					ChangeForm(newForm, _INTL("{1} transformed!", This));
				}
			} else {
				ChangeForm(0, _INTL("{1} transformed!", This));
			}
		}
		// Cherrim - Flower Gift
		if (isSpecies(Speciess.CHERRIM)) {
			if (hasActiveAbility(Abilitys.FLOWERGIFT)) {
				newForm = 0;
				if (new []{:Sun, :HarshSun}.Contains(effectiveWeather)) newForm = 1;
				if (@form != newForm) {
					@battle.ShowAbilitySplash(self, true);
					@battle.HideAbilitySplash(self);
					ChangeForm(newForm, _INTL("{1} transformed!", This));
				}
			} else {
				ChangeForm(0, _INTL("{1} transformed!", This));
			}
		}
		// Eiscue - Ice Face
		if (!ability_changed && isSpecies(Speciess.EISCUE) && self.ability == abilitys.ICEFACE &&
			@form == 1 && new []{:Hail, :Snowstorm}.Contains(effectiveWeather)) {
			@canRestoreIceFace = true;   // Changed form at end of round
		}
	}

	// Checks the Pokémon's form and updates it if necessary. Used for when a
	// Pokémon enters battle (endOfRound=false) and at the end of each round
	// (endOfRound=true).
	public void CheckForm(endOfRound = false) {
		if (fainted() || @effects.Transform) return;
		// Form changes upon entering battle and when the weather changes
		if (!endOfRound) CheckFormOnWeatherChange;
		// Darmanitan - Zen Mode
		if (isSpecies(Speciess.DARMANITAN) && self.ability == abilitys.ZENMODE) {
			if (@hp <= @totalhp / 2) {
				if (@form.even()) {
					@battle.ShowAbilitySplash(self, true);
					@battle.HideAbilitySplash(self);
					ChangeForm(@form + 1, _INTL("{1} triggered!", abilityName));
				}
			} else if (@form.odd()) {
				@battle.ShowAbilitySplash(self, true);
				@battle.HideAbilitySplash(self);
				ChangeForm(@form - 1, _INTL("{1} triggered!", abilityName));
			}
		}
		// Minior - Shields Down
		if (isSpecies(Speciess.MINIOR) && self.ability == abilitys.SHIELDSDOWN) {
			if (@hp > @totalhp / 2) {   // Turn into Meteor form
				newForm = (@form >= 7) ? @form - 7 : @form;
				if (@form != newForm) {
					@battle.ShowAbilitySplash(self, true);
					@battle.HideAbilitySplash(self);
					ChangeForm(newForm, _INTL("{1} deactivated!", abilityName));
				}
			} else if (@form < 7) {   // Turn into Core form
				@battle.ShowAbilitySplash(self, true);
				@battle.HideAbilitySplash(self);
				ChangeForm(@form + 7, _INTL("{1} activated!", abilityName));
			}
		}
		// Wishiwashi - Schooling
		if (isSpecies(Speciess.WISHIWASHI) && self.ability == abilitys.SCHOOLING) {
			if (@level >= 20 && @hp > @totalhp / 4) {
				if (@form != 1) {
					@battle.ShowAbilitySplash(self, true);
					@battle.HideAbilitySplash(self);
					ChangeForm(1, _INTL("{1} formed a school!", This));
				}
			} else if (@form != 0) {
				@battle.ShowAbilitySplash(self, true);
				@battle.HideAbilitySplash(self);
				ChangeForm(0, _INTL("{1} stopped schooling!", This));
			}
		}
		// Zygarde - Power Construct
		if (isSpecies(Speciess.ZYGARDE) && self.ability == abilitys.POWERCONSTRUCT && endOfRound &&
			@hp <= @totalhp / 2 && @form < 2) {   // Turn into Complete Forme
			newForm = @form + 2;
			@battle.Display(_INTL("You sense the presence of many!"));
			@battle.ShowAbilitySplash(self, true);
			@battle.HideAbilitySplash(self);
			ChangeForm(newForm, _INTL("{1} transformed into its Complete Forme!", This));
		}
		// Morpeko - Hunger Switch
		if (isSpecies(Speciess.MORPEKO) && hasActiveAbility(Abilitys.HUNGERSWITCH) && endOfRound) {
			// Intentionally doesn't show the ability splash or a message
			newForm = (@form + 1) % 2;
			ChangeForm(newForm, null);
		}
	}

	public void Transform(target) {
		oldAbil = @ability_id;
		@effects.Transform        = true;
		@effects.TransformSpecies = target.species;
		self.form = target.form;
		ChangeTypes(target);
		self.ability = target.ability;
		@attack  = target.attack;
		@defense = target.defense;
		@spatk   = target.spatk;
		@spdef   = target.spdef;
		@speed   = target.speed;
		GameData.Stat.each_battle(s => @stages[s.id] = target.stages[s.id]);
		if (Settings.NEW_CRITICAL_HIT_RATE_MECHANICS) {
			@effects.FocusEnergy = target.effects.FocusEnergy;
			@effects.LaserFocus  = target.effects.LaserFocus;
		}
		@moves.clear;
		target.moves.each_with_index do |m, i|
			@moves[i] = Battle.Move.from_pokemon_move(@battle, new Pokemon.Move(m.id));
			@moves[i].pp       = 5;
			@moves[i].total_pp = 5;
		}
		@effects.Disable      = 0;
		@effects.DisableMove  = null;
		@effects.WeightChange = target.effects.WeightChange;
		@battle.scene.RefreshOne(@index);
		@battle.Display(_INTL("{1} transformed into {2}!", This, target.ToString(true)));
		OnLosingAbility(oldAbil);
	}

	public void HyperMode() { }
}
