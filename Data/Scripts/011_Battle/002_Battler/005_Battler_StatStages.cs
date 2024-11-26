//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	//-----------------------------------------------------------------------------
	// Increase stat stages.
	//-----------------------------------------------------------------------------

	public bool statStageAtMax(stat) {
		return @stages[stat] >= STAT_STAGE_MAXIMUM;
	}

	public bool CanRaiseStatStage(stat, user = null, move = null, showFailMsg = false, ignoreContrary = false) {
		if (fainted()) return false;
		// Contrary
		if (hasActiveAbility(Abilitys.CONTRARY) && !ignoreContrary && !beingMoldBroken()) {
			return CanLowerStatStage(stat, user, move, showFailMsg, true);
		}
		// Check the stat stage
		if (statStageAtMax(stat)) {
			if (showFailMsg) {
				@battle.Display(_INTL("{1}'s {2} won't go any higher!",
																This, GameData.Stat.get(stat).name));
			}
			return false;
		}
		return true;
	}

	public void RaiseStatStageBasic(stat, increment, ignoreContrary = false) {
		if (!beingMoldBroken()) {
			// Contrary
			if (hasActiveAbility(Abilitys.CONTRARY) && !ignoreContrary) {
				return LowerStatStageBasic(stat, increment, true);
			}
			// Simple
			if (hasActiveAbility(Abilitys.SIMPLE)) increment *= 2;
		}
		// Change the stat stage
		increment = (int)Math.Min(increment, STAT_STAGE_MAXIMUM - @stages[stat]);
		if (increment > 0) {
			stat_name = GameData.Stat.get(stat).name;
			new = @stages[stat] + increment;
			Debug.Log($"[Stat change] {This}'s {stat_name} changed by +{increment} ({@stages[stat]} -> {new})");
			@stages[stat] += increment;
			@statsRaisedThisRound = true;
		}
		return increment;
	}

	public void RaiseStatStage(stat, increment, user, showAnim = true, ignoreContrary = false) {
		// Contrary
		if (hasActiveAbility(Abilitys.CONTRARY) && !beingMoldBroken() && !ignoreContrary) {
			return LowerStatStage(stat, increment, user, showAnim, true);
		}
		// Perform the stat stage change
		increment = RaiseStatStageBasic(stat, increment, ignoreContrary);
		if (increment <= 0) return false;
		// Stat up animation and message
		if (showAnim) @battle.CommonAnimation("StatUp", self);
		arrStatTexts = new {
			_INTL("{1}'s {2} rose!", This, GameData.Stat.get(stat).name),
			_INTL("{1}'s {2} rose sharply!", This, GameData.Stat.get(stat).name),
			_INTL("{1}'s {2} rose drastically!", This, GameData.Stat.get(stat).name);
		}
		@battle.Display(arrStatTexts[(int)Math.Min(increment - 1, 2)]);
		// Trigger abilities upon stat gain
		if (abilityActive()) {
			Battle.AbilityEffects.triggerOnStatGain(self.ability, self, stat, user);
		}
		return true;
	}

	public void RaiseStatStageByCause(stat, increment, user, cause, showAnim = true, ignoreContrary = false) {
		// Contrary
		if (hasActiveAbility(Abilitys.CONTRARY) && !beingMoldBroken() && !ignoreContrary) {
			return LowerStatStageByCause(stat, increment, user, cause, showAnim, true);
		}
		// Perform the stat stage change
		increment = RaiseStatStageBasic(stat, increment, ignoreContrary);
		if (increment <= 0) return false;
		// Stat up animation and message
		if (showAnim) @battle.CommonAnimation("StatUp", self);
		if (user.index == @index) {
			arrStatTexts = new {
				_INTL("{1}'s {2} raised its {3}!", This, cause, GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} sharply raised its {3}!", This, cause, GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} drastically raised its {3}!", This, cause, GameData.Stat.get(stat).name);
			}
		} else {
			arrStatTexts = new {
				_INTL("{1}'s {2} raised {3}'s {4}!", user.ToString(), cause, This(true), GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} sharply raised {3}'s {4}!", user.ToString(), cause, This(true), GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} drastically raised {3}'s {4}!", user.ToString(), cause, This(true), GameData.Stat.get(stat).name);
			}
		}
		@battle.Display(arrStatTexts[(int)Math.Min(increment - 1, 2)]);
		// Trigger abilities upon stat gain
		if (abilityActive()) {
			Battle.AbilityEffects.triggerOnStatGain(self.ability, self, stat, user);
		}
		return true;
	}

	public void RaiseStatStageByAbility(stat, increment, user, splashAnim = true) {
		if (fainted()) return false;
		ret = false;
		if (splashAnim) @battle.ShowAbilitySplash(user);
		if (CanRaiseStatStage(stat, user, null, Battle.Scene.USE_ABILITY_SPLASH)) {
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				ret = RaiseStatStage(stat, increment, user);
			} else {
				ret = RaiseStatStageByCause(stat, increment, user, user.abilityName);
			}
		}
		if (splashAnim) @battle.HideAbilitySplash(user);
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Decrease stat stages.
	//-----------------------------------------------------------------------------

	public bool statStageAtMin(stat) {
		return @stages[stat] <= -STAT_STAGE_MAXIMUM;
	}

	public bool CanLowerStatStage(stat, user = null, move = null, showFailMsg = false,
													ignoreContrary = false, ignoreMirrorArmor = false) {
		if (fainted()) return false;
		if (!beingMoldBroken()) {
			// Contrary
			if (hasActiveAbility(Abilitys.CONTRARY) && !ignoreContrary) {
				return CanRaiseStatStage(stat, user, move, showFailMsg, true);
			}
			// Mirror Armor
			if (hasActiveAbility(Abilitys.MIRRORARMOR) && !ignoreMirrorArmor &&
				user && user.index != @index && !statStageAtMin(stat)) {
				return true;
			}
		}
		if (!user || user.index != @index) {   // Not self-inflicted
			if (@effects.Substitute > 0 &&
				(ignoreMirrorArmor || !(move && move.ignoresSubstitute(user)))) {
				if (showFailMsg) @battle.Display(_INTL("{1} is protected by its substitute!", This));
				return false;
			}
			if (OwnSide.effects.Mist > 0 &&
				!(user && user.hasActiveAbility(Abilitys.INFILTRATOR))) {
				if (showFailMsg) @battle.Display(_INTL("{1} is protected by Mist!", This));
				return false;
			}
			if (abilityActive()) {
				if (!beingMoldBroken() && Battle.AbilityEffects.triggerStatLossImmunity() return false;
					self.ability, self, stat, @battle, showFailMsg
				);
				if (Battle.AbilityEffects.triggerStatLossImmunityNonIgnorable() return false;
					self.ability, self, stat, @battle, showFailMsg
				);
			}
			foreach (var b in allAllies) { //'allAllies.each' do => |b|
				if (!b.abilityActive() || b.beingMoldBroken()) continue;
				if (Battle.AbilityEffects.triggerStatLossImmunityFromAlly() return false;
					b.ability, b, self, stat, @battle, showFailMsg
				);
			}
		}
		if (user && user.index != @index) {   // Only protects against moves/abilities of non-self
			if (itemActive() && Battle.ItemEffects.triggerStatLossImmunity() return false;
				self.item, self, stat, @battle, showFailMsg
			);
		}
		// Check the stat stage
		if (statStageAtMin(stat)) {
			if (showFailMsg) {
				@battle.Display(_INTL("{1}'s {2} won't go any lower!",
																This, GameData.Stat.get(stat).name));
			}
			return false;
		}
		return true;
	}

	public void LowerStatStageBasic(stat, increment, ignoreContrary = false) {
		if (!beingMoldBroken()) {
			// Contrary
			if (hasActiveAbility(Abilitys.CONTRARY) && !ignoreContrary) {
				return RaiseStatStageBasic(stat, increment, true);
			}
			// Simple
			if (hasActiveAbility(Abilitys.SIMPLE)) increment *= 2;
		}
		// Change the stat stage
		increment = (int)Math.Min(increment, STAT_STAGE_MAXIMUM + @stages[stat]);
		if (increment > 0) {
			stat_name = GameData.Stat.get(stat).name;
			new = @stages[stat] - increment;
			Debug.Log($"[Stat change] {This}'s {stat_name} changed by -{increment} ({@stages[stat]} -> {new})");
			@stages[stat] -= increment;
			@statsLoweredThisRound = true;
			@statsDropped = true;
		}
		return increment;
	}

	public void LowerStatStage(stat, increment, user, showAnim = true, ignoreContrary = false,
											mirrorArmorSplash = 0, ignoreMirrorArmor = false) {
		if (!beingMoldBroken()) {
			// Contrary
			if (hasActiveAbility(Abilitys.CONTRARY) && !ignoreContrary) {
				return RaiseStatStage(stat, increment, user, showAnim, true);
			}
			// Mirror Armor
			if (hasActiveAbility(Abilitys.MIRRORARMOR) && !ignoreMirrorArmor &&
				user && user.index != @index && !statStageAtMin(stat)) {
				if (mirrorArmorSplash < 2) {
					@battle.ShowAbilitySplash(self);
					if (!Battle.Scene.USE_ABILITY_SPLASH) {
						@battle.Display(_INTL("{1}'s {2} activated!", This, abilityName));
					}
				}
				ret = false;
				if (user.CanLowerStatStage(stat, self, null, true, ignoreContrary, true)) {
					ret = user.LowerStatStage(stat, increment, self, showAnim, ignoreContrary, mirrorArmorSplash, true);
				}
				if (mirrorArmorSplash.even()) @battle.HideAbilitySplash(self);   // i.e. not 1 or 3
				return ret;
			}
		}
		// Perform the stat stage change
		increment = LowerStatStageBasic(stat, increment, ignoreContrary);
		if (increment <= 0) return false;
		// Stat down animation and message
		if (showAnim) @battle.CommonAnimation("StatDown", self);
		arrStatTexts = new {
			_INTL("{1}'s {2} fell!", This, GameData.Stat.get(stat).name),
			_INTL("{1}'s {2} harshly fell!", This, GameData.Stat.get(stat).name),
			_INTL("{1}'s {2} severely fell!", This, GameData.Stat.get(stat).name);
		}
		@battle.Display(arrStatTexts[(int)Math.Min(increment - 1, 2)]);
		// Trigger abilities upon stat loss
		if (abilityActive()) {
			Battle.AbilityEffects.triggerOnStatLoss(self.ability, self, stat, user);
		}
		return true;
	}

	public void LowerStatStageByCause(stat, increment, user, cause, showAnim = true,
															ignoreContrary = false, ignoreMirrorArmor = false) {
		if (!beingMoldBroken()) {
			// Contrary
			if (hasActiveAbility(Abilitys.CONTRARY) && !ignoreContrary) {
				return RaiseStatStageByCause(stat, increment, user, cause, showAnim, true);
			}
			// Mirror Armor
			if (hasActiveAbility(Abilitys.MIRRORARMOR) && !ignoreMirrorArmor &&
				user && user.index != @index && !statStageAtMin(stat)) {
				@battle.ShowAbilitySplash(self);
				if (!Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1}'s {2} activated!", This, abilityName));
				}
				ret = false;
				if (user.CanLowerStatStage(stat, self, null, true, ignoreContrary, true)) {
					ret = user.LowerStatStageByCause(stat, increment, self, abilityName, showAnim, ignoreContrary, true);
				}
				@battle.HideAbilitySplash(self);
				return ret;
			}
		}
		// Perform the stat stage change
		increment = LowerStatStageBasic(stat, increment, ignoreContrary);
		if (increment <= 0) return false;
		// Stat down animation and message
		if (showAnim) @battle.CommonAnimation("StatDown", self);
		if (user.index == @index) {
			arrStatTexts = new {
				_INTL("{1}'s {2} lowered its {3}!", This, cause, GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} harshly lowered its {3}!", This, cause, GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} severely lowered its {3}!", This, cause, GameData.Stat.get(stat).name);
			}
		} else {
			arrStatTexts = new {
				_INTL("{1}'s {2} lowered {3}'s {4}!", user.ToString(), cause, This(true), GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} harshly lowered {3}'s {4}!", user.ToString(), cause, This(true), GameData.Stat.get(stat).name),
				_INTL("{1}'s {2} severely lowered {3}'s {4}!", user.ToString(), cause, This(true), GameData.Stat.get(stat).name);
			}
		}
		@battle.Display(arrStatTexts[(int)Math.Min(increment - 1, 2)]);
		// Trigger abilities upon stat loss
		if (abilityActive()) {
			Battle.AbilityEffects.triggerOnStatLoss(self.ability, self, stat, user);
		}
		return true;
	}

	public void LowerStatStageByAbility(stat, increment, user, splashAnim = true, checkContact = false) {
		ret = false;
		if (splashAnim) @battle.ShowAbilitySplash(user);
		if (CanLowerStatStage(stat, user, null, Battle.Scene.USE_ABILITY_SPLASH) &&
			(!checkContact || affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH))) {
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				ret = LowerStatStage(stat, increment, user);
			} else {
				ret = LowerStatStageByCause(stat, increment, user, user.abilityName);
			}
		}
		if (splashAnim) @battle.HideAbilitySplash(user);
		return ret;
	}

	public void LowerAttackStatStageIntimidate(user) {
		if (fainted()) return false;
		// NOTE: Substitute intentionally blocks Intimidate even if self has Contrary.
		if (@effects.Substitute > 0) {
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1} is protected by its substitute!", This));
			} else {
				@battle.Display(_INTL("{1}'s substitute protected it from {2}'s {3}!",
																This, user.ToString(true), user.abilityName));
			}
			return false;
		}
		if (Settings.MECHANICS_GENERATION >= 8 && hasActiveAbility(new {:OBLIVIOUS, :OWNTEMPO, :INNERFOCUS, :SCRAPPY})) {
			@battle.ShowAbilitySplash(self);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				@battle.Display(_INTL("{1}'s {2} cannot be lowered!", This, GameData.Stat.get(:ATTACK).name));
			} else {
				@battle.Display(_INTL("{1}'s {2} prevents {3} loss!", This, abilityName,
																GameData.Stat.get(:ATTACK).name));
			}
			@battle.HideAbilitySplash(self);
			return false;
		}
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			return LowerStatStageByAbility(:ATTACK, 1, user, false);
		}
		// NOTE: These checks exist to ensure appropriate messages are shown if
		//       Intimidate is blocked somehow (i.e. the messages should mention the
		//       Intimidate ability by name).
		if (!hasActiveAbility(Abilitys.CONTRARY)) {
			if (OwnSide.effects.Mist > 0) {
				@battle.Display(_INTL("{1} is protected from {2}'s {3} by Mist!",
																This, user.ToString(true), user.abilityName));
				return false;
			}
			if (abilityActive() &&
				(Battle.AbilityEffects.triggerStatLossImmunity(self.ability, self, :ATTACK, @battle, false) ||
					Battle.AbilityEffects.triggerStatLossImmunityNonIgnorable(self.ability, self, :ATTACK, @battle, false))) {
				@battle.Display(_INTL("{1}'s {2} prevented {3}'s {4} from working!",
																This, abilityName, user.ToString(true), user.abilityName));
				return false;
			}
			foreach (var b in allAllies) { //'allAllies.each' do => |b|
				if (!b.abilityActive()) continue;
				if (Battle.AbilityEffects.triggerStatLossImmunityFromAlly(b.ability, b, self, :ATTACK, @battle, false)) {
					@battle.Display(_INTL("{1} is protected from {2}'s {3} by {4}'s {5}!",
																	This, user.ToString(true), user.abilityName, b.ToString(true), b.abilityName));
					return false;
				}
			}
			if (itemActive() &&
				Battle.ItemEffects.triggerStatLossImmunity(self.item, self, :ATTACK, @battle, false)) {
				@battle.Display(_INTL("{1}'s {2} prevented {3}'s {4} from working!",
																This, itemName, user.ToString(true), user.abilityName));
				return false;
			}
		}
		if (!CanLowerStatStage(:ATTACK, user)) return false;
		return LowerStatStageByCause(:ATTACK, 1, user, user.abilityName);
	}

	//-----------------------------------------------------------------------------
	// Reset stat stages.
	//-----------------------------------------------------------------------------

	public bool hasAlteredStatStages() {
		GameData.Stat.each_battle(s => { if (@stages[s.id] != 0) return true; });
		return false;
	}

	public bool hasRaisedStatStages() {
		GameData.Stat.each_battle(s => { if (@stages[s.id] > 0) return true; });
		return false;
	}

	public bool hasLoweredStatStages() {
		GameData.Stat.each_battle(s => { if (@stages[s.id] < 0) return true; });
		return false;
	}

	public void ResetStatStages() {
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (@stages[s.id] > 0) {
				@statsLoweredThisRound = true;
				@statsDropped = true;
			} else if (@stages[s.id] < 0) {
				@statsRaisedThisRound = true;
			}
			@stages[s.id] = 0;
		}
	}
}
