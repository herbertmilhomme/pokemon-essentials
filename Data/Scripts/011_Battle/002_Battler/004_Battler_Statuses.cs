//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Generalised checks for whether a status problem can be inflicted.
	//-----------------------------------------------------------------------------

	// NOTE: Not all "does it have this status?" checks use this method. If the
	//       check is leading up to curing self of that status condition, then it
	//       will look at the value of @status directly instead - if it is that
	//       status condition then it is curable. This method only checks for
	//       "counts as having that status", which includes Comatose which can't be
	//       cured.
	public bool HasStatus(checkStatus) {
		if (Battle.AbilityEffects.triggerStatusCheckNonIgnorable(self.ability, self, checkStatus)) {
			return true;
		}
		return @status == checkStatus;
	}

	public bool HasAnyStatus() {
		if (Battle.AbilityEffects.triggerStatusCheckNonIgnorable(self.ability, self, null)) {
			return true;
		}
		return @status != statuses.NONE;
	}

	public bool CanInflictStatus(newStatus, user, showMessages, move = null, ignoreStatus = false) {
		if (fainted()) return false;
		self_inflicted = (user && user.index == @index);   // Rest and Flame Orb/Toxic Orb only
		// Already have that status problem
		if (self.status == newStatus && !ignoreStatus) {
			if (showMessages) {
				msg = "";
				switch (self.status) {
					case :SLEEP:      msg = _INTL("{1} is already asleep!", This); break;
					case :POISON:     msg = _INTL("{1} is already poisoned!", This); break;
					case :BURN:       msg = _INTL("{1} already has a burn!", This); break;
					case :PARALYSIS:  msg = _INTL("{1} is already paralyzed!", This); break;
					case :FROZEN:     msg = _INTL("{1} is already frozen solid!", This); break;
				}
				@battle.Display(msg);
			}
			return false;
		}
		// Trying to replace a status problem with another one
		if (self.status != statuses.NONE && !ignoreStatus && !(self_inflicted && move)) {   // Rest can replace a status problem
			if (showMessages) @battle.Display(_INTL("It doesn't affect {1}...", This(true)));
			return false;
		}
		// Trying to inflict a status problem on a Pokémon behind a substitute
		if (@effects.Substitute > 0 && !(move && move.ignoresSubstitute(user)) &&
			!self_inflicted) {
			if (showMessages) @battle.Display(_INTL("It doesn't affect {1}...", This(true)));
			return false;
		}
		// Weather immunity
		if (newStatus == Statuses.FROZEN && new []{:Sun, :HarshSun}.Contains(effectiveWeather)) {
			if (showMessages) @battle.Display(_INTL("It doesn't affect {1}...", This(true)));
			return false;
		}
		// Terrains immunity
		if (affectedByTerrain()) {
			switch (@battle.field.terrain) {
				case :Electric:
					if (newStatus == Statuses.SLEEP) {
						if (showMessages) {
							@battle.Display(_INTL("{1} surrounds itself with electrified terrain!", This(true)));
						}
						return false;
					}
					break;
				case :Misty:
					if (showMessages) @battle.Display(_INTL("{1} surrounds itself with misty terrain!", This(true)));
					return false;
			}
		}
		// Uproar immunity
		if (newStatus == Statuses.SLEEP && !(hasActiveAbility(Abilitys.SOUNDPROOF) && !beingMoldBroken())) {
			@battle.allBattlers.each do |b|
				if (b.effects.Uproar == 0) continue;
				if (showMessages) @battle.Display(_INTL("But the uproar kept {1} awake!", This(true)));
				return false;
			}
		}
		// Type immunities
		hasImmuneType = false;
		switch (newStatus) {
			case :SLEEP:
				// No type is immune to sleep
				break;
			case :POISON:
				if (!(user && user.hasActiveAbility(Abilitys.CORROSION))) {
					hasImmuneType |= Type == Types.POISON;
					hasImmuneType |= Type == Types.STEEL;
				}
				break;
			case :BURN:
				hasImmuneType |= Type == Types.FIRE;
				break;
			case :PARALYSIS:
				hasImmuneType |= Type == Types.ELECTRIC && Settings.MORE_TYPE_EFFECTS;
				break;
			case :FROZEN:
				hasImmuneType |= Type == Types.ICE;
				break;
		}
		if (hasImmuneType) {
			if (showMessages) @battle.Display(_INTL("It doesn't affect {1}...", This(true)));
			return false;
		}
		// Ability immunity
		immuneByAbility = false;
		immAlly = null;
		if (Battle.AbilityEffects.triggerStatusImmunityNonIgnorable(self.ability, self, newStatus)) {
			immuneByAbility = true;
		} else if (abilityActive() && (self_inflicted || !beingMoldBroken()) &&
			Battle.AbilityEffects.triggerStatusImmunity(self.ability, self, newStatus)) {
			immuneByAbility = true;
		} else {
			foreach (var b in allAllies) { //'allAllies.each' do => |b|
				if (!b.abilityActive() || (!self_inflicted && b.beingMoldBroken())) continue;
				if (!Battle.AbilityEffects.triggerStatusImmunityFromAlly(b.ability, self, newStatus)) continue;
				immuneByAbility = true;
				immAlly = b;
				break;
			}
		}
		if (immuneByAbility) {
			if (showMessages) {
				@battle.ShowAbilitySplash(immAlly || self);
				msg = "";
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					switch (newStatus) {
						case :SLEEP:      msg = _INTL("{1} stays awake!", This); break;
						case :POISON:     msg = _INTL("{1} cannot be poisoned!", This); break;
						case :BURN:       msg = _INTL("{1} cannot be burned!", This); break;
						case :PARALYSIS:  msg = _INTL("{1} cannot be paralyzed!", This); break;
						case :FROZEN:     msg = _INTL("{1} cannot be frozen solid!", This); break;
					}
				} else if (immAlly) {
					switch (newStatus) {
						case :SLEEP:
							msg = _INTL("{1} stays awake because of {2}'s {3}!",
													This, immAlly.ToString(true), immAlly.abilityName);
							break;
						case :POISON:
							msg = _INTL("{1} cannot be poisoned because of {2}'s {3}!",
													This, immAlly.ToString(true), immAlly.abilityName);
							break;
						case :BURN:
							msg = _INTL("{1} cannot be burned because of {2}'s {3}!",
													This, immAlly.ToString(true), immAlly.abilityName);
							break;
						case :PARALYSIS:
							msg = _INTL("{1} cannot be paralyzed because of {2}'s {3}!",
													This, immAlly.ToString(true), immAlly.abilityName);
							break;
						case :FROZEN:
							msg = _INTL("{1} cannot be frozen solid because of {2}'s {3}!",
													This, immAlly.ToString(true), immAlly.abilityName);
							break;
					}
				} else {
					switch (newStatus) {
						case :SLEEP:      msg = _INTL("{1} stays awake because of its {2}!", This, abilityName); break;
						case :POISON:     msg = _INTL("{1}'s {2} prevents poisoning!", This, abilityName); break;
						case :BURN:       msg = _INTL("{1}'s {2} prevents burns!", This, abilityName); break;
						case :PARALYSIS:  msg = _INTL("{1}'s {2} prevents paralysis!", This, abilityName); break;
						case :FROZEN:     msg = _INTL("{1}'s {2} prevents freezing!", This, abilityName); break;
					}
				}
				@battle.Display(msg);
				@battle.HideAbilitySplash(immAlly || self);
			}
			return false;
		}
		// Safeguard immunity
		if (OwnSide.effects.Safeguard > 0 && !self_inflicted && move &&
			!(user && user.hasActiveAbility(Abilitys.INFILTRATOR))) {
			if (showMessages) @battle.Display(_INTL("{1}'s team is protected by Safeguard!", This));
			return false;
		}
		return true;
	}

	public bool CanSynchronizeStatus(newStatus, user) {
		if (fainted()) return false;
		// Trying to replace a status problem with another one
		if (self.status != statuses.NONE) return false;
		// Terrain immunity
		if (@battle.field.terrain == :Misty && affectedByTerrain()) return false;
		// Type immunities
		hasImmuneType = false;
		switch (newStatus) {
			case :POISON:
				// NOTE: user will have Synchronize, so it can't have Corrosion.
				if (!(user && user.hasActiveAbility(Abilitys.CORROSION))) {
					hasImmuneType |= Type == Types.POISON;
					hasImmuneType |= Type == Types.STEEL;
				}
				break;
			case :BURN:
				hasImmuneType |= Type == Types.FIRE;
				break;
			case :PARALYSIS:
				hasImmuneType |= Type == Types.ELECTRIC && Settings.MORE_TYPE_EFFECTS;
				break;
		}
		if (hasImmuneType) return false;
		// Ability immunity
		if (Battle.AbilityEffects.triggerStatusImmunityNonIgnorable(self.ability, self, newStatus)) {
			return false;
		}
		if (abilityActive() && Battle.AbilityEffects.triggerStatusImmunity(self.ability, self, newStatus)) {
			return false;
		}
		foreach (var b in allAllies) { //'allAllies.each' do => |b|
			if (!b.abilityActive()) continue;
			if (!Battle.AbilityEffects.triggerStatusImmunityFromAlly(b.ability, self, newStatus)) continue;
			return false;
		}
		// Safeguard immunity
		// NOTE: user will have Synchronize, so it can't have Infiltrator.
		if (OwnSide.effects.Safeguard > 0 &&
			!(user && user.hasActiveAbility(Abilitys.INFILTRATOR))) {
			return false;
		}
		return true;
	}

	//-----------------------------------------------------------------------------
	// Generalised infliction of status problem.
	//-----------------------------------------------------------------------------

	public void InflictStatus(newStatus, newStatusCount = 0, msg = null, user = null) {
		// Inflict the new status
		self.status      = newStatus;
		self.statusCount = newStatusCount;
		@effects.Toxic = 0;
		// Show animation
		if (newStatus == Statuses.POISON && newStatusCount > 0) {
			@battle.CommonAnimation("Toxic", self);
		} else {
			anim_name = GameData.Status.get(newStatus).animation;
			if (anim_name) @battle.CommonAnimation(anim_name, self);
		}
		// Show message
		if (msg && !msg.empty()) {
			@battle.Display(msg);
		} else {
			switch (newStatus) {
				case :SLEEP:
					@battle.Display(_INTL("{1} fell asleep!", This));
					break;
				case :POISON:
					if (newStatusCount > 0) {
						@battle.Display(_INTL("{1} was badly poisoned!", This));
					} else {
						@battle.Display(_INTL("{1} was poisoned!", This));
					}
					break;
				case :BURN:
					@battle.Display(_INTL("{1} was burned!", This));
					break;
				case :PARALYSIS:
					@battle.Display(_INTL("{1} is paralyzed! It may be unable to move!", This));
					break;
				case :FROZEN:
					@battle.Display(_INTL("{1} was frozen solid!", This));
					break;
			}
		}
		if (newStatus == Statuses.SLEEP) Debug.Log($"[Status change] {This}'s sleep count is {newStatusCount}");
		// Form change check
		CheckFormOnStatusChange;
		// Synchronize
		if (abilityActive()) {
			Battle.AbilityEffects.triggerOnStatusInflicted(self.ability, self, user, newStatus);
		}
		// Status cures
		ItemStatusCureCheck;
		AbilityStatusCureCheck;
		// Petal Dance/Outrage/Thrash get cancelled immediately by falling asleep
		// NOTE: I don't know why this applies only to Outrage and only to falling
		//       asleep (i.e. it doesn't cancel Rollout/Uproar/other multi-turn
		//       moves, and it doesn't cancel any moves if self becomes frozen/
		//       disabled/anything else). This behaviour was tested in Gen 5.
		if (@status == statuses.SLEEP && @effects.Outrage > 0) {
			@effects.Outrage = 0;
			@currentMove = null;
		}
	}

	//-----------------------------------------------------------------------------
	// Sleep.
	//-----------------------------------------------------------------------------

	public bool asleep() {
		return HasStatus(:SLEEP);
	}

	public bool CanSleep(user, showMessages, move = null, ignoreStatus = false) {
		return CanInflictStatus(:SLEEP, user, showMessages, move, ignoreStatus);
	}

	public bool CanSleepYawn() {
		if (self.status != statuses.NONE) return false;
		if (affectedByTerrain() && new []{:Electric, :Misty}.Contains(@battle.field.terrain)) {
			return false;
		}
		if (!hasActiveAbility(Abilitys.SOUNDPROOF) && @battle.allBattlers.any(b => b.effects.Uproar > 0)) {
			return false;
		}
		if (Battle.AbilityEffects.triggerStatusImmunityNonIgnorable(self.ability, self, :SLEEP)) {
			return false;
		}
		// NOTE: Bulbapedia claims that Flower Veil shouldn't prevent sleep due to
		//       drowsiness, but I disagree because that makes no sense. Also, the
		//       comparable Sweet Veil does prevent sleep due to drowsiness.
		if (abilityActive() && Battle.AbilityEffects.triggerStatusImmunity(self.ability, self, :SLEEP)) {
			return false;
		}
		foreach (var b in allAllies) { //'allAllies.each' do => |b|
			if (!b.abilityActive()) continue;
			if (!Battle.AbilityEffects.triggerStatusImmunityFromAlly(b.ability, self, :SLEEP)) continue;
			return false;
		}
		// NOTE: Bulbapedia claims that Safeguard shouldn't prevent sleep due to
		//       drowsiness. I disagree with this too. Compare with the other sided
		//       effects Misty/Electric Terrain, which do prevent it.
		if (OwnSide.effects.Safeguard > 0) return false;
		return true;
	}

	public void Sleep(user = null, msg = null) {
		InflictStatus(:SLEEP, SleepDuration, msg, user);
	}

	public void SleepSelf(msg = null, duration = -1) {
		InflictStatus(:SLEEP, SleepDuration(duration), msg);
	}

	public void SleepDuration(duration = -1) {
		if (duration <= 0) duration = 2 + @battle.Random(3);
		if (hasActiveAbility(Abilitys.EARLYBIRD)) duration = (int)Math.Floor(duration / 2);
		return duration;
	}

	//-----------------------------------------------------------------------------
	// Poison.
	//-----------------------------------------------------------------------------

	public bool poisoned() {
		return HasStatus(:POISON);
	}

	public bool CanPoison(user, showMessages, move = null) {
		return CanInflictStatus(:POISON, user, showMessages, move);
	}

	public bool CanPoisonSynchronize(target) {
		return CanSynchronizeStatus(:POISON, target);
	}

	public void Poison(user = null, msg = null, toxic = false) {
		InflictStatus(:POISON, (toxic) ? 1 : 0, msg, user);
	}

	//-----------------------------------------------------------------------------
	// Burn.
	//-----------------------------------------------------------------------------

	public bool burned() {
		return HasStatus(:BURN);
	}

	public bool CanBurn(user, showMessages, move = null) {
		return CanInflictStatus(:BURN, user, showMessages, move);
	}

	public bool CanBurnSynchronize(target) {
		return CanSynchronizeStatus(:BURN, target);
	}

	public void Burn(user = null, msg = null) {
		InflictStatus(:BURN, 0, msg, user);
	}

	//-----------------------------------------------------------------------------
	// Paralyze.
	//-----------------------------------------------------------------------------

	public bool paralyzed() {
		return HasStatus(:PARALYSIS);
	}

	public bool CanParalyze(user, showMessages, move = null) {
		return CanInflictStatus(:PARALYSIS, user, showMessages, move);
	}

	public bool CanParalyzeSynchronize(target) {
		return CanSynchronizeStatus(:PARALYSIS, target);
	}

	public void Paralyze(user = null, msg = null) {
		InflictStatus(:PARALYSIS, 0, msg, user);
	}

	//-----------------------------------------------------------------------------
	// Freeze.
	//-----------------------------------------------------------------------------

	public bool frozen() {
		return HasStatus(:FROZEN);
	}

	public bool CanFreeze(user, showMessages, move = null) {
		return CanInflictStatus(:FROZEN, user, showMessages, move);
	}

	public void Freeze(user = null, msg = null) {
		InflictStatus(:FROZEN, 0, msg, user);
	}

	//-----------------------------------------------------------------------------
	// Generalised status displays.
	//-----------------------------------------------------------------------------

	public void ContinueStatus() {
		if (self.status == statuses.POISON && @statusCount > 0) {
			@battle.CommonAnimation("Toxic", self);
		} else {
			anim_name = GameData.Status.get(self.status).animation;
			if (anim_name) @battle.CommonAnimation(anim_name, self);
		}
		if (block_given()) yield;
		switch (self.status) {
			case :SLEEP:
				@battle.Display(_INTL("{1} is fast asleep.", This));
				break;
			case :POISON:
				@battle.Display(_INTL("{1} was hurt by poison!", This));
				break;
			case :BURN:
				@battle.Display(_INTL("{1} was hurt by its burn!", This));
				break;
			case :PARALYSIS:
				@battle.Display(_INTL("{1} is paralyzed! It can't move!", This));
				break;
			case :FROZEN:
				@battle.Display(_INTL("{1} is frozen solid!", This));
				break;
		}
		if (self.status == statuses.SLEEP) Debug.Log($"[Status continues] {This}'s sleep count is {@statusCount}");
	}

	public void CureStatus(showMessages = true) {
		oldStatus = status;
		self.status = :NONE;
		if (showMessages) {
			switch (oldStatus) {
				case :SLEEP:      @battle.Display(_INTL("{1} woke up!", This)); break;
				case :POISON:     @battle.Display(_INTL("{1} was cured of its poisoning.", This)); break;
				case :BURN:       @battle.Display(_INTL("{1}'s burn was healed.", This)); break;
				case :PARALYSIS:  @battle.Display(_INTL("{1} was cured of paralysis.", This)); break;
				case :FROZEN:     @battle.Display(_INTL("{1} thawed out!", This)); break;
			}
		}
		if (!showMessages) Debug.Log($"[Status change] {This}'s status was cured");
	}

	//-----------------------------------------------------------------------------
	// Confusion.
	//-----------------------------------------------------------------------------

	public bool CanConfuse(user = null, showMessages = true, move = null, selfInflicted = false) {
		if (fainted()) return false;
		if (@effects.Confusion > 0) {
			if (showMessages) @battle.Display(_INTL("{1} is already confused.", This));
			return false;
		}
		if (@effects.Substitute > 0 && !(move && move.ignoresSubstitute(user)) &&
			!selfInflicted) {
			if (showMessages) @battle.Display(_INTL("But it failed!"));
			return false;
		}
		// Terrains immunity
		if (affectedByTerrain() && @battle.field.terrain == :Misty && Settings.MECHANICS_GENERATION >= 7) {
			if (showMessages) @battle.Display(_INTL("{1} surrounds itself with misty terrain!", This(true)));
			return false;
		}
		if ((selfInflicted || !beingMoldBroken()) && hasActiveAbility(Abilitys.OWNTEMPO)) {
			if (showMessages) {
				@battle.ShowAbilitySplash(self);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} doesn't become confused!", This));
				} else {
					@battle.Display(_INTL("{1}'s {2} prevents confusion!", This, abilityName));
				}
				@battle.HideAbilitySplash(self);
			}
			return false;
		}
		if (OwnSide.effects.Safeguard > 0 && !selfInflicted &&
			!(user && user.hasActiveAbility(Abilitys.INFILTRATOR))) {
			if (showMessages) @battle.Display(_INTL("{1}'s team is protected by Safeguard!", This));
			return false;
		}
		return true;
	}

	public bool CanConfuseSelf(showMessages) {
		return CanConfuse(null, showMessages, null, true);
	}

	public void Confuse(msg = null) {
		@effects.Confusion = ConfusionDuration;
		@battle.CommonAnimation("Confusion", self);
		if (nil_or_empty(msg)) msg = _INTL("{1} became confused!", This);
		@battle.Display(msg);
		Debug.Log($"[Lingering effect] {This}'s confusion count is {@effects.Confusion}");
		// Confusion cures
		ItemStatusCureCheck;
		AbilityStatusCureCheck;
	}

	public void ConfusionDuration(duration = -1) {
		if (duration <= 0) duration = 2 + @battle.Random(4);
		return duration;
	}

	public void CureConfusion() {
		@effects.Confusion = 0;
	}

	//-----------------------------------------------------------------------------
	// Attraction.
	//-----------------------------------------------------------------------------

	public bool CanAttract(user, showMessages = true) {
		if (fainted()) return false;
		if (!user || user.fainted()) return false;
		if (@effects.Attract >= 0) {
			if (showMessages) @battle.Display(_INTL("{1} is unaffected!", This));
			return false;
		}
		agender = user.gender;
		ogender = gender;
		if (agender == 2 || ogender == 2 || agender == ogender) {
			if (showMessages) @battle.Display(_INTL("{1} is unaffected!", This));
			return false;
		}
		if (hasActiveAbility(new {:AROMAVEIL, :OBLIVIOUS}) && !beingMoldBroken()) {
			if (showMessages) {
				@battle.ShowAbilitySplash(self);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} is unaffected!", This));
				} else {
					@battle.Display(_INTL("{1}'s {2} prevents romance!", This, abilityName));
				}
				@battle.HideAbilitySplash(self);
			}
			return false;
		} else {
			foreach (var b in allAllies) { //'allAllies.each' do => |b|
				if (!b.hasActiveAbility(Abilitys.AROMAVEIL) || b.beingMoldBroken()) continue;
				if (showMessages) {
					@battle.ShowAbilitySplash(b);
					if (Battle.Scene.USE_ABILITY_SPLASH) {
						@battle.Display(_INTL("{1} is unaffected!", This));
					} else {
						@battle.Display(_INTL("{1}'s {2} prevents romance!", b.ToString(), b.abilityName));
					}
					@battle.HideAbilitySplash(b);
				}
				return false;
			}
		}
		return true;
	}

	public void Attract(user, msg = null) {
		@effects.Attract = user.index;
		@battle.CommonAnimation("Attract", self);
		if (nil_or_empty(msg)) msg = _INTL("{1} fell in love!", This);
		@battle.Display(msg);
		// Destiny Knot
		if (hasActiveItem(Items.DESTINYKNOT) && user.CanAttract(self, false)) {
			user.Attract(self, _INTL("{1} fell in love from the {2}!", user.ToString(true), itemName));
		}
		// Attraction cures
		ItemStatusCureCheck;
		AbilityStatusCureCheck;
	}

	public void CureAttract() {
		@effects.Attract = -1;
	}

	//-----------------------------------------------------------------------------
	// Flinching.
	//-----------------------------------------------------------------------------

	public void Flinch(_user = null) {
		if (hasActiveAbility(Abilitys.INNERFOCUS) && !beingMoldBroken()) return;
		@effects.Flinch = true;
	}
}
