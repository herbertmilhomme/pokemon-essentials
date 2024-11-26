//===============================================================================
//
//===============================================================================
public static partial class Battle.ItemEffects {
	SpeedCalc                       = new ItemHandlerHash();
	WeightCalc                      = new ItemHandlerHash();   // Float Stone
	// Battler's HP/stat changed
	HPHeal                          = new ItemHandlerHash();
	OnStatLoss                      = new ItemHandlerHash();
	// Battler's status problem
	StatusCure                      = new ItemHandlerHash();
	// Battler's stat stages
	StatLossImmunity                = new ItemHandlerHash();
	// Priority and turn order
	PriorityBracketChange           = new ItemHandlerHash();
	PriorityBracketUse              = new ItemHandlerHash();
	// Move usage failures
	OnMissingTarget                 = new ItemHandlerHash();   // Blunder Policy
	// Accuracy calculation
	AccuracyCalcFromUser            = new ItemHandlerHash();
	AccuracyCalcFromTarget          = new ItemHandlerHash();
	// Damage calculation
	DamageCalcFromUser              = new ItemHandlerHash();
	DamageCalcFromTarget            = new ItemHandlerHash();
	CriticalCalcFromUser            = new ItemHandlerHash();
	CriticalCalcFromTarget          = new ItemHandlerHash();   // None!
	// Upon a move hitting a target
	OnBeingHit                      = new ItemHandlerHash();
	OnBeingHitPositiveBerry         = new ItemHandlerHash();
	// Items that trigger at the end of using a move
	AfterMoveUseFromTarget          = new ItemHandlerHash();
	AfterMoveUseFromUser            = new ItemHandlerHash();
	OnEndOfUsingMove                = new ItemHandlerHash();   // Leppa Berry
	OnEndOfUsingMoveStatRestore     = new ItemHandlerHash();   // White Herb
	// Experience and EV gain
	ExpGainModifier                 = new ItemHandlerHash();   // Lucky Egg
	EVGainModifier                  = new ItemHandlerHash();
	// Weather and terrin
	WeatherExtender                 = new ItemHandlerHash();
	TerrainExtender                 = new ItemHandlerHash();   // Terrain Extender
	TerrainStatBoost                = new ItemHandlerHash();
	// End Of Round
	EndOfRoundHealing               = new ItemHandlerHash();
	EndOfRoundEffect                = new ItemHandlerHash();
	// Switching and fainting
	CertainSwitching                = new ItemHandlerHash();   // Shed Shell
	TrappingByTarget                = new ItemHandlerHash();   // None!
	OnSwitchIn                      = new ItemHandlerHash();   // Air Balloon
	OnIntimidated                   = new ItemHandlerHash();   // Adrenaline Orb
	// Running from battle
	CertainEscapeFromBattle         = new ItemHandlerHash();   // Smoke Ball

	//-----------------------------------------------------------------------------

	public static void trigger(hash, *args, ret: false) {
		new_ret = hash.trigger(*args);
		return (!new_ret.null()) ? new_ret : ret;
	}

	//-----------------------------------------------------------------------------

	public static void triggerSpeedCalc(item, battler, mult) {
		return trigger(SpeedCalc, item, battler, mult, ret: mult);
	}

	public static void triggerWeightCalc(item, battler, w) {
		return trigger(WeightCalc, item, battler, w, ret: w);
	}

	//-----------------------------------------------------------------------------

	public static void triggerHPHeal(item, battler, battle, forced) {
		return trigger(HPHeal, item, battler, battle, forced);
	}

	public static void triggerOnStatLoss(item, user, move_user, battle) {
		return trigger(OnStatLoss, item, user, move_user, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerStatusCure(item, battler, battle, forced) {
		return trigger(StatusCure, item, battler, battle, forced);
	}

	//-----------------------------------------------------------------------------

	public static void triggerStatLossImmunity(item, battler, stat, battle, show_messages) {
		return trigger(StatLossImmunity, item, battler, stat, battle, show_messages);
	}

	//-----------------------------------------------------------------------------

	public static void triggerPriorityBracketChange(item, battler, battle) {
		return trigger(PriorityBracketChange, item, battler, battle, ret: 0);
	}

	public static void triggerPriorityBracketUse(item, battler, battle) {
		PriorityBracketUse.trigger(item, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerOnMissingTarget(item, user, target, move, hit_num, battle) {
		OnMissingTarget.trigger(item, user, target, move, hit_num, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerAccuracyCalcFromUser(item, mods, user, target, move, type) {
		AccuracyCalcFromUser.trigger(item, mods, user, target, move, type);
	}

	public static void triggerAccuracyCalcFromTarget(item, mods, user, target, move, type) {
		AccuracyCalcFromTarget.trigger(item, mods, user, target, move, type);
	}

	//-----------------------------------------------------------------------------

	public static void triggerDamageCalcFromUser(item, user, target, move, mults, power, type) {
		DamageCalcFromUser.trigger(item, user, target, move, mults, power, type);
	}

	public static void triggerDamageCalcFromTarget(item, user, target, move, mults, power, type) {
		DamageCalcFromTarget.trigger(item, user, target, move, mults, power, type);
	}

	public static void triggerCriticalCalcFromUser(item, user, target, move, crit_stage) {
		return trigger(CriticalCalcFromUser, item, user, target, move, crit_stage, ret: crit_stage);
	}

	public static void triggerCriticalCalcFromTarget(item, user, target, move, crit_stage) {
		return trigger(CriticalCalcFromTarget, item, user, target, move, crit_stage, ret: crit_stage);
	}

	//-----------------------------------------------------------------------------

	public static void triggerOnBeingHit(item, user, target, move, battle) {
		OnBeingHit.trigger(item, user, target, move, battle);
	}

	public static void triggerOnBeingHitPositiveBerry(item, battler, battle, forced) {
		return trigger(OnBeingHitPositiveBerry, item, battler, battle, forced);
	}

	//-----------------------------------------------------------------------------

	public static void triggerAfterMoveUseFromTarget(item, battler, user, move, switched_battlers, battle) {
		AfterMoveUseFromTarget.trigger(item, battler, user, move, switched_battlers, battle);
	}

	public static void triggerAfterMoveUseFromUser(item, user, targets, move, num_hits, battle) {
		AfterMoveUseFromUser.trigger(item, user, targets, move, num_hits, battle);
	}

	public static void triggerOnEndOfUsingMove(item, battler, battle, forced) {
		return trigger(OnEndOfUsingMove, item, battler, battle, forced);
	}

	public static void triggerOnEndOfUsingMoveStatRestore(item, battler, battle, forced) {
		return trigger(OnEndOfUsingMoveStatRestore, item, battler, battle, forced);
	}

	//-----------------------------------------------------------------------------

	public static void triggerExpGainModifier(item, battler, exp) {
		return trigger(ExpGainModifier, item, battler, exp, ret: -1);
	}

	public static void triggerEVGainModifier(item, battler, ev_array) {
		if (!EVGainModifier[item]) return false;
		EVGainModifier.trigger(item, battler, ev_array);
		return true;
	}

	//-----------------------------------------------------------------------------

	public static void triggerWeatherExtender(item, weather, duration, battler, battle) {
		return trigger(WeatherExtender, item, weather, duration, battler, battle, ret: duration);
	}

	public static void triggerTerrainExtender(item, terrain, duration, battler, battle) {
		return trigger(TerrainExtender, item, terrain, duration, battler, battle, ret: duration);
	}

	public static void triggerTerrainStatBoost(item, battler, battle) {
		return trigger(TerrainStatBoost, item, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerEndOfRoundHealing(item, battler, battle) {
		EndOfRoundHealing.trigger(item, battler, battle);
	}

	public static void triggerEndOfRoundEffect(item, battler, battle) {
		EndOfRoundEffect.trigger(item, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerCertainSwitching(item, switcher, battle) {
		return trigger(CertainSwitching, item, switcher, battle);
	}

	public static void triggerTrappingByTarget(item, switcher, bearer, battle) {
		return trigger(TrappingByTarget, item, switcher, bearer, battle);
	}

	public static void triggerOnSwitchIn(item, battler, battle) {
		OnSwitchIn.trigger(item, battler, battle);
	}

	public static void triggerOnIntimidated(item, battler, battle) {
		return trigger(OnIntimidated, item, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerCertainEscapeFromBattle(item, battler) {
		return trigger(CertainEscapeFromBattle, item, battler);
	}
}

//===============================================================================
// SpeedCalc handlers
//===============================================================================

Battle.ItemEffects.SpeedCalc.add(:CHOICESCARF,
	block: (item, battler, mult) => {
		next mult * 1.5;
	}
)

Battle.ItemEffects.SpeedCalc.add(:IRONBALL,
	block: (item, battler, mult) => {
		next mult / 2;
	}
)

Battle.ItemEffects.SpeedCalc.add(:MACHOBRACE,
	block: (item, battler, mult) => {
		next mult / 2;
	}
)

Battle.ItemEffects.SpeedCalc.copy(:MACHOBRACE, :POWERANKLET, :POWERBAND,
																								:POWERBELT, :POWERBRACER,
																								:POWERLENS, :POWERWEIGHT);

Battle.ItemEffects.SpeedCalc.add(:QUICKPOWDER,
	block: (item, battler, mult) => {
		if (battler.isSpecies(Speciess.DITTO) && !battler.effects.Transform) next mult * 2;
	}
)

//===============================================================================
// WeightCalc handlers
//===============================================================================

Battle.ItemEffects.WeightCalc.add(:FLOATSTONE,
	block: (item, battler, w) => {
		next (int)Math.Max(w / 2, 1);
	}
)

//===============================================================================
// HPHeal handlers
//===============================================================================

Battle.ItemEffects.HPHeal.add(:AGUAVBERRY,
	block: (item, battler, battle, forced) => {
		next battler.ConfusionBerry(item, forced, :SPECIAL_DEFENSE,
			_INTL("For {1}, the {2} was too bitter!", battler.ToString(true), GameData.Item.get(item).name)
		);
	}
)

Battle.ItemEffects.HPHeal.add(:APICOTBERRY,
	block: (item, battler, battle, forced) => {
		next battler.StatIncreasingBerry(item, forced, :SPECIAL_DEFENSE);
	}
)

Battle.ItemEffects.HPHeal.add(:BERRYJUICE,
	block: (item, battler, battle, forced) => {
		if (!battler.canHeal()) next false;
		if (!forced && battler.hp > battler.totalhp / 2) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] Forced consuming of {itemName}");
		if (!forced) battle.CommonAnimation("UseItem", battler);
		battler.RecoverHP(20);
		if (forced) {
			battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} restored its health using its {2}!", battler.ToString(), itemName));
		}
		next true;
	}
)

Battle.ItemEffects.HPHeal.add(:FIGYBERRY,
	block: (item, battler, battle, forced) => {
		next battler.ConfusionBerry(item, forced, :ATTACK,
			_INTL("For {1}, the {2} was too spicy!", battler.ToString(true), GameData.Item.get(item).name)
		);
	}
)

Battle.ItemEffects.HPHeal.add(:GANLONBERRY,
	block: (item, battler, battle, forced) => {
		next battler.StatIncreasingBerry(item, forced, :DEFENSE);
	}
)

Battle.ItemEffects.HPHeal.add(:IAPAPABERRY,
	block: (item, battler, battle, forced) => {
		next battler.ConfusionBerry(item, forced, :DEFENSE,
			_INTL("For {1}, the {2} was too sour!", battler.ToString(true), GameData.Item.get(item).name)
		);
	}
)

Battle.ItemEffects.HPHeal.add(:LANSATBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumePinchBerry()) next false;
		if (battler.effects.FocusEnergy >= 2) next false;
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.effects.FocusEnergy = 2;
		itemName = GameData.Item.get(item).name;
		if (forced) {
			battle.Display(_INTL("{1} got pumped from the {2}!", battler.ToString(), itemName));
		} else {
			battle.Display(_INTL("{1} used its {2} to get pumped!", battler.ToString(), itemName));
		}
		next true;
	}
)

Battle.ItemEffects.HPHeal.add(:LIECHIBERRY,
	block: (item, battler, battle, forced) => {
		next battler.StatIncreasingBerry(item, forced, :ATTACK);
	}
)

Battle.ItemEffects.HPHeal.add(:MAGOBERRY,
	block: (item, battler, battle, forced) => {
		next battler.ConfusionBerry(item, forced, :SPEED,
			_INTL("For {1}, the {2} was too sweet!", battler.ToString(true), GameData.Item.get(item).name)
		);
	}
)

Battle.ItemEffects.HPHeal.add(:MICLEBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumePinchBerry()) next false;
		if (battler.effects.MicleBerry) next false;
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.effects.MicleBerry = true;
		itemName = GameData.Item.get(item).name;
		if (forced) {
			Debug.Log($"[Item triggered] Forced consuming of {itemName}");
			battle.Display(_INTL("{1} boosted the accuracy of its next move!", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} boosted the accuracy of its next move using its {2}!",
				battler.ToString(), itemName));
		}
		next true;
	}
)

Battle.ItemEffects.HPHeal.add(:ORANBERRY,
	block: (item, battler, battle, forced) => {
		if (!battler.canHeal()) next false;
		if (!forced && !battler.canConsumePinchBerry(false)) next false;
		amt = 10;
		ripening = false;
		if (battler.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(battler, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		if (ripening) battle.HideAbilitySplash(battler);
		battler.RecoverHP(amt);
		itemName = GameData.Item.get(item).name;
		if (forced) {
			Debug.Log($"[Item triggered] Forced consuming of {itemName}");
			battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} restored a little HP using its {2}!", battler.ToString(), itemName));
		}
		next true;
	}
)

Battle.ItemEffects.HPHeal.add(:PETAYABERRY,
	block: (item, battler, battle, forced) => {
		next battler.StatIncreasingBerry(item, forced, :SPECIAL_ATTACK);
	}
)

Battle.ItemEffects.HPHeal.add(:SALACBERRY,
	block: (item, battler, battle, forced) => {
		next battler.StatIncreasingBerry(item, forced, :SPEED);
	}
)

Battle.ItemEffects.HPHeal.add(:SITRUSBERRY,
	block: (item, battler, battle, forced) => {
		if (!battler.canHeal()) next false;
		if (!forced && !battler.canConsumePinchBerry(false)) next false;
		amt = battler.totalhp / 4;
		ripening = false;
		if (battler.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(battler, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		if (ripening) battle.HideAbilitySplash(battler);
		battler.RecoverHP(amt);
		itemName = GameData.Item.get(item).name;
		if (forced) {
			Debug.Log($"[Item triggered] Forced consuming of {itemName}");
			battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} restored its health using its {2}!", battler.ToString(), itemName));
		}
		next true;
	}
)

Battle.ItemEffects.HPHeal.add(:STARFBERRY,
	block: (item, battler, battle, forced) => {
		stats = new List<string>();
		GameData.Stat.each_main_battle(s => { if (battler.CanRaiseStatStage(s.id, battler)) stats.Add(s.id); });
		if (stats.length == 0) next false;
		stat = stats[battle.Random(stats.length)];
		next battler.StatIncreasingBerry(item, forced, stat, 2);
	}
)

Battle.ItemEffects.HPHeal.add(:WIKIBERRY,
	block: (item, battler, battle, forced) => {
		next battler.ConfusionBerry(item, forced, :SPECIAL_ATTACK,
			_INTL("For {1}, the {2} was too dry!", battler.ToString(true), GameData.Item.get(item).name)
		);
	}
)

//===============================================================================
// OnStatLoss handlers
//===============================================================================
Battle.ItemEffects.OnStatLoss.add(:EJECTPACK,
	block: (item, battler, move_user, battle) => {
		if (battler.effects.SkyDrop >= 0 ||
									battler.inTwoTurnAttack("TwoTurnAttackInvulnerableInSkyTargetCannotAct")) next false;   // Sky Drop
		if (battle.AllFainted(battler.idxOpposingSide)) next false;
		if (battler.wild()) next false;   // Wild Pokémon can't eject
		if (!battle.CanSwitchOut(battler.index)) next false;   // Battler can't switch out
		if (!battle.CanChooseNonActive(battler.index)) next false;   // No Pokémon can switch in
		battle.CommonAnimation("UseItem", battler);
		battle.Display(_INTL("{1} is switched out by the {2}!", battler.ToString(), battler.itemName));
		battler.ConsumeItem(true, false);
		if (battle.endOfRound) {   // Just switch out
			if (!battler.fainted()) battle.scene.Recall(battler.index);
			battler.AbilitiesOnSwitchOut;   // Inc. primordial weather check
			next true;
		}
		newPkmn = battle.GetReplacementPokemonIndex(battler.index);   // Owner chooses
		if (newPkmn < 0) next false;   // Shouldn't ever do this
		battle.RecallAndReplace(battler.index, newPkmn);
		battle.ClearChoice(battler.index);   // Replacement Pokémon does nothing this round
		if (move_user && battler.index == move_user.index) battle.moldBreaker = false;
		battle.OnBattlerEnteringBattle(battler.index);
		next true;
	}
)

//===============================================================================
// StatusCure handlers
//===============================================================================

Battle.ItemEffects.StatusCure.add(:ASPEARBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.status != statuses.FROZEN) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.CureStatus(forced);
		if (!forced) battle.Display(_INTL("{1}'s {2} defrosted it!", battler.ToString(), itemName));
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:CHERIBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.status != statuses.PARALYSIS) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.CureStatus(forced);
		if (!forced) battle.Display(_INTL("{1}'s {2} cured its paralysis!", battler.ToString(), itemName));
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:CHESTOBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.status != statuses.SLEEP) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.CureStatus(forced);
		if (!forced) battle.Display(_INTL("{1}'s {2} woke it up!", battler.ToString(), itemName));
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:LUMBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.status == statuses.NONE &&
									battler.effects.Confusion == 0) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		oldStatus = battler.status;
		oldConfusion = (battler.effects.Confusion > 0);
		battler.CureStatus(forced);
		battler.CureConfusion;
		if (forced) {
			if (oldConfusion) battle.Display(_INTL("{1} snapped out of its confusion.", battler.ToString()));
		} else {
			switch (oldStatus) {
				case :SLEEP:
					battle.Display(_INTL("{1}'s {2} woke it up!", battler.ToString(), itemName));
					break;
				case :POISON:
					battle.Display(_INTL("{1}'s {2} cured its poisoning!", battler.ToString(), itemName));
					break;
				case :BURN:
					battle.Display(_INTL("{1}'s {2} healed its burn!", battler.ToString(), itemName));
					break;
				case :PARALYSIS:
					battle.Display(_INTL("{1}'s {2} cured its paralysis!", battler.ToString(), itemName));
					break;
				case :FROZEN:
					battle.Display(_INTL("{1}'s {2} defrosted it!", battler.ToString(), itemName));
					break;
			}
			if (oldConfusion) {
				battle.Display(_INTL("{1}'s {2} snapped it out of its confusion!", battler.ToString(), itemName));
			}
		}
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:MENTALHERB,
	block: (item, battler, battle, forced) => {
		if (battler.effects.Attract == -1 &&
									battler.effects.Taunt == 0 &&
									battler.effects.Encore == 0 &&
									!battler.effects.Torment &&
									battler.effects.Disable == 0 &&
									battler.effects.HealBlock == 0) next false;
		itemName = GameData.Item.get(item).name;
		Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("UseItem", battler);
		if (battler.effects.Attract >= 0) {
			if (forced) {
				battle.Display(_INTL("{1} got over its infatuation.", battler.ToString()));
			} else {
				battle.Display(_INTL("{1} cured its infatuation status using its {2}!",
					battler.ToString(), itemName));
			}
			battler.CureAttract;
		}
		if (battler.effects.Taunt > 0) battle.Display(_INTL("{1}'s taunt wore off!", battler.ToString()));
		battler.effects.Taunt = 0;
		if (battler.effects.Encore > 0) battle.Display(_INTL("{1}'s encore ended!", battler.ToString()));
		battler.effects.Encore     = 0;
		battler.effects.EncoreMove = null;
		if (battler.effects.Torment) battle.Display(_INTL("{1}'s torment wore off!", battler.ToString()));
		battler.effects.Torment = false;
		if (battler.effects.Disable > 0) battle.Display(_INTL("{1} is no longer disabled!", battler.ToString()));
		battler.effects.Disable = 0;
		if (battler.effects.HealBlock > 0) battle.Display(_INTL("{1}'s Heal Block wore off!", battler.ToString()));
		battler.effects.HealBlock = 0;
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:PECHABERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.status != statuses.POISON) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.CureStatus(forced);
		if (!forced) battle.Display(_INTL("{1}'s {2} cured its poisoning!", battler.ToString(), itemName));
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:PERSIMBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.effects.Confusion == 0) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.CureConfusion;
		if (forced) {
			battle.Display(_INTL("{1} snapped out of its confusion.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1}'s {2} snapped it out of its confusion!", battler.ToString(),
				itemName));
		}
		next true;
	}
)

Battle.ItemEffects.StatusCure.add(:RAWSTBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (battler.status != statuses.BURN) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		battler.CureStatus(forced);
		if (!forced) battle.Display(_INTL("{1}'s {2} healed its burn!", battler.ToString(), itemName));
		next true;
	}
)

//===============================================================================
// StatLossImmunity handlers
//===============================================================================

Battle.ItemEffects.StatLossImmunity.add(:CLEARAMULET,
	block: (item, battler, stat, battle, showMessages) => {
		battle.Display(_INTL("The effects of {1}'s {2} prevent its stats from being lowered!",
													if (showMessages) battler.ToString(), GameData.Item.get(item).name));
		next true;
	}
)

//===============================================================================
// PriorityBracketChange handlers
//===============================================================================

Battle.ItemEffects.PriorityBracketChange.add(:CUSTAPBERRY,
	block: (item, battler, battle) => {
		if (battler.canConsumePinchBerry()) next 1;
	}
)

Battle.ItemEffects.PriorityBracketChange.add(:LAGGINGTAIL,
	block: (item, battler, battle) => {
		next -1;
	}
)

Battle.ItemEffects.PriorityBracketChange.copy(:LAGGINGTAIL, :FULLINCENSE);

Battle.ItemEffects.PriorityBracketChange.add(:QUICKCLAW,
	block: (item, battler, battle) => {
		if (battle.Random(100) < 20) next 1;
	}
)

//===============================================================================
// PriorityBracketUse handlers
//===============================================================================

Battle.ItemEffects.PriorityBracketUse.add(:CUSTAPBERRY,
	block: (item, battler, battle) => {
		battle.CommonAnimation("EatBerry", battler);
		battle.Display(_INTL("{1}'s {2} let it move first!", battler.ToString(), battler.itemName));
		battler.ConsumeItem;
	}
)

Battle.ItemEffects.PriorityBracketUse.add(:QUICKCLAW,
	block: (item, battler, battle) => {
		battle.CommonAnimation("UseItem", battler);
		battle.Display(_INTL("{1}'s {2} let it move first!", battler.ToString(), battler.itemName));
	}
)

//===============================================================================
// OnMissingTarget handlers
//===============================================================================

Battle.ItemEffects.OnMissingTarget.add(:BLUNDERPOLICY,
	block: (item, user, target, move, hit_num, battle) => {
		if (hit_num > 0 || target.damageState.invulnerable) continue;
		if (new []{"OHKO", "OHKOIce", "OHKOHitsUndergroundTarget"}.Contains(move.function_code)) continue;
		if (!user.CanRaiseStatStage(:SPEED, user)) continue;
		battle.CommonAnimation("UseItem", user);
		user.RaiseStatStageByCause(:SPEED, 2, user, user.itemName);
		battle.Display(_INTL("The {1} was used up...", user.itemName));
		user.HeldItemTriggered(item);
	}
)

//===============================================================================
// AccuracyCalcFromUser handlers
//===============================================================================

Battle.ItemEffects.AccuracyCalcFromUser.add(:WIDELENS,
	block: (item, mods, user, target, move, type) => {
		mods.accuracy_multiplier *= 1.1;
	}
)

Battle.ItemEffects.AccuracyCalcFromUser.add(:ZOOMLENS,
	block: (item, mods, user, target, move, type) => {
		if ((target.battle.choices[target.index].Action != :UseMove &&
			target.battle.choices[target.index].Action != :Shift) ||
			target.movedThisRound()) {
			mods.accuracy_multiplier *= 1.2;
		}
	}
)

//===============================================================================
// AccuracyCalcFromTarget handlers
//===============================================================================

Battle.ItemEffects.AccuracyCalcFromTarget.add(:BRIGHTPOWDER,
	block: (item, mods, user, target, move, type) => {
		mods.accuracy_multiplier *= 0.9;
	}
)

Battle.ItemEffects.AccuracyCalcFromTarget.copy(:BRIGHTPOWDER, :LAXINCENSE);

//===============================================================================
// DamageCalcFromUser handlers
//===============================================================================

Battle.ItemEffects.DamageCalcFromUser.add(:ADAMANTORB,
	block: (item, user, target, move, mults, power, type) => {
		if (user.isSpecies(Speciess.DIALGA) && new []{:DRAGON, :STEEL}.Contains(type)) {
			mults.power_multiplier *= 1.2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:ADAMANTORB, :ADAMANTCRYSTAL);

Battle.ItemEffects.DamageCalcFromUser.add(:BLACKBELT,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.FIGHTING) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:BLACKBELT, :FISTPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:BLACKGLASSES,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.DARK) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:BLACKGLASSES, :DREADPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:BUGGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:BUG, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:CHARCOAL,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.FIRE) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:CHARCOAL, :FLAMEPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:CHOICEBAND,
	block: (item, user, target, move, mults, power, type) => {
		if (move.physicalMove()) mults.power_multiplier *= 1.5;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:CHOICESPECS,
	block: (item, user, target, move, mults, power, type) => {
		if (move.specialMove()) mults.power_multiplier *= 1.5;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:CORNERSTONEMASK,
	block: (item, user, target, move, mults, power, type) => {
		if (user.isSpecies(Speciess.OGERPON)) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:CORNERSTONEMASK, :HEARTHFLAMEEMASK, :WELLSPRINGMASK);

Battle.ItemEffects.DamageCalcFromUser.add(:DARKGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:DARK, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:DEEPSEATOOTH,
	block: (item, user, target, move, mults, power, type) => {
		if (user.isSpecies(Speciess.CLAMPERL) && move.specialMove()) {
			mults.attack_multiplier *= 2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:DRAGONFANG,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.DRAGON) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:DRAGONFANG, :DRACOPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:DRAGONGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:DRAGON, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:ELECTRICGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:ELECTRIC, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:EXPERTBELT,
	block: (item, user, target, move, mults, power, type) => {
		if (Effectiveness.super_effective(target.damageState.typeMod)) {
			mults.final_damage_multiplier *= 1.2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:FAIRYGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:FAIRY, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:FIGHTINGGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:FIGHTING, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:FIREGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:FIRE, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:FLYINGGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:FLYING, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:GHOSTGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:GHOST, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:GRASSGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:GRASS, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:GRISEOUSORB,
	block: (item, user, target, move, mults, power, type) => {
		if (user.isSpecies(Speciess.GIRATINA) && new []{:DRAGON, :GHOST}.Contains(type)) {
			mults.power_multiplier *= 1.2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:GRISEOUSORB, :GRISEOUSCORE);

Battle.ItemEffects.DamageCalcFromUser.add(:GROUNDGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:GROUND, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:HARDSTONE,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.ROCK) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:HARDSTONE, :STONEPLATE, :ROCKINCENSE);

Battle.ItemEffects.DamageCalcFromUser.add(:ICEGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:ICE, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:LIFEORB,
	block: (item, user, target, move, mults, power, type) => {
		if (!move.is_a(Battle.Move.Confusion)) {
			mults.final_damage_multiplier *= 1.3;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:LIGHTBALL,
	block: (item, user, target, move, mults, power, type) => {
		if (user.isSpecies(Speciess.PIKACHU)) mults.attack_multiplier *= 2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:LUSTROUSORB,
	block: (item, user, target, move, mults, power, type) => {
		if (user.isSpecies(Speciess.PALKIA) && new []{:DRAGON, :WATER}.Contains(type)) {
			mults.power_multiplier *= 1.2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:LUSTROUSORB, :LUSTROUSGLOBE);

Battle.ItemEffects.DamageCalcFromUser.add(:MAGNET,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.ELECTRIC) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:MAGNET, :ZAPPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:METALCOAT,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.STEEL) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:METALCOAT, :IRONPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:METRONOME,
	block: (item, user, target, move, mults, power, type) => {
		met = 1 + (0.2 * (int)Math.Min(user.effects.Metronome, 5));
		mults.final_damage_multiplier *= met;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:MIRACLESEED,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.GRASS) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:MIRACLESEED, :MEADOWPLATE, :ROSEINCENSE);

Battle.ItemEffects.DamageCalcFromUser.add(:MUSCLEBAND,
	block: (item, user, target, move, mults, power, type) => {
		if (move.physicalMove()) mults.power_multiplier *= 1.1;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:MYSTICWATER,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.WATER) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:MYSTICWATER, :SPLASHPLATE, :SEAINCENSE, :WAVEINCENSE);

Battle.ItemEffects.DamageCalcFromUser.add(:NEVERMELTICE,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.ICE) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:NEVERMELTICE, :ICICLEPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:NORMALGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:NORMAL, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:FAIRYFEATHER,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.FAIRY) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:FAIRYFEATHER, :PIXIEPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:POISONBARB,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.POISON) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:POISONBARB, :TOXICPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:POISONGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:POISON, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:PSYCHICGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:PSYCHIC, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:PUNCHINGGLOVE,
	block: (item, user, target, move, mults, power, type) => {
		if (move.punchingMove()) mults.power_multiplier *= 1.1;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:ROCKGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:ROCK, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:SHARPBEAK,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.FLYING) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:SHARPBEAK, :SKYPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:SILKSCARF,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.NORMAL) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:SILVERPOWDER,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.BUG) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:SILVERPOWDER, :INSECTPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:SOFTSAND,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.GROUND) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:SOFTSAND, :EARTHPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:SOULDEW,
	block: (item, user, target, move, mults, power, type) => {
		if (!user.isSpecies(Speciess.LATIAS) && !user.isSpecies(Speciess.LATIOS)) continue;
		if (Settings.SOUL_DEW_POWERS_UP_TYPES) {
			if (new []{:DRAGON, :PSYCHIC}.Contains(type)) mults.final_damage_multiplier *= 1.2;
		} else if (move.specialMove() && !user.battle.rules["souldewclause"]) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:SPELLTAG,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.GHOST) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:SPELLTAG, :SPOOKYPLATE);

Battle.ItemEffects.DamageCalcFromUser.add(:STEELGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:STEEL, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:THICKCLUB,
	block: (item, user, target, move, mults, power, type) => {
		if ((user.isSpecies(Speciess.CUBONE) || user.isSpecies(Speciess.MAROWAK)) && move.physicalMove()) {
			mults.attack_multiplier *= 2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:TWISTEDSPOON,
	block: (item, user, target, move, mults, power, type) => {
		if (type == types.PSYCHIC) mults.power_multiplier *= 1.2;
	}
)

Battle.ItemEffects.DamageCalcFromUser.copy(:TWISTEDSPOON, :MINDPLATE, :ODDINCENSE);

Battle.ItemEffects.DamageCalcFromUser.add(:WATERGEM,
	block: (item, user, target, move, mults, power, type) => {
		user.MoveTypePoweringUpGem(:WATER, move, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromUser.add(:WISEGLASSES,
	block: (item, user, target, move, mults, power, type) => {
		if (move.specialMove()) mults.power_multiplier *= 1.1;
	}
)

//===============================================================================
// DamageCalcFromTarget handlers
// NOTE: Species-specific held items consider the original species, not the
//       transformed species, and still work while transformed. The exceptions
//       are Metal/Quick Powder, which don't work if the holder is transformed.
//===============================================================================

Battle.ItemEffects.DamageCalcFromTarget.add(:ASSAULTVEST,
	block: (item, user, target, move, mults, power, type) => {
		if (move.specialMove()) mults.defense_multiplier *= 1.5;
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:BABIRIBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:STEEL, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:CHARTIBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:ROCK, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:CHILANBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:NORMAL, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:CHOPLEBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:FIGHTING, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:COBABERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:FLYING, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:COLBURBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:DARK, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:DEEPSEASCALE,
	block: (item, user, target, move, mults, power, type) => {
		if (target.isSpecies(Speciess.CLAMPERL) && move.specialMove()) {
			mults.defense_multiplier *= 2;
		}
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:EVIOLITE,
	block: (item, user, target, move, mults, power, type) => {
		// NOTE: Eviolite cares about whether the Pokémon itself can evolve, which
		//       means it also cares about the Pokémon's form. Some forms cannot
		//       evolve even if the species generally can, and such forms are not
		//       affected by Eviolite.
		if (target.pokemon.species_data.get_evolutions(true).length > 0) {
			mults.defense_multiplier *= 1.5;
		}
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:HABANBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:DRAGON, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:KASIBBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:GHOST, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:KEBIABERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:POISON, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:METALPOWDER,
	block: (item, user, target, move, mults, power, type) => {
		if (target.isSpecies(Speciess.DITTO) && !target.effects.Transform) {
			mults.defense_multiplier *= 1.5;
		}
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:OCCABERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:FIRE, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:PASSHOBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:WATER, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:PAYAPABERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:PSYCHIC, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:RINDOBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:GRASS, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:ROSELIBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:FAIRY, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:SHUCABERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:GROUND, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:SOULDEW,
	block: (item, user, target, move, mults, power, type) => {
		if (Settings.SOUL_DEW_POWERS_UP_TYPES) continue;
		if (!target.isSpecies(Speciess.LATIAS) && !target.isSpecies(Speciess.LATIOS)) continue;
		if (move.specialMove() && !user.battle.rules["souldewclause"]) {
			mults.defense_multiplier *= 1.5;
		}
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:TANGABERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:BUG, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:WACANBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:ELECTRIC, type, mults);
	}
)

Battle.ItemEffects.DamageCalcFromTarget.add(:YACHEBERRY,
	block: (item, user, target, move, mults, power, type) => {
		target.MoveTypeWeakeningBerry(:ICE, type, mults);
	}
)

//===============================================================================
// CriticalCalcFromUser handlers
//===============================================================================

Battle.ItemEffects.CriticalCalcFromUser.add(:LUCKYPUNCH,
	block: (item, user, target, move, c) => {
		if (user.isSpecies(Speciess.CHANSEY)) next c + 2;
	}
)

Battle.ItemEffects.CriticalCalcFromUser.add(:RAZORCLAW,
	block: (item, user, target, move, c) => {
		next c + 1;
	}
)

Battle.ItemEffects.CriticalCalcFromUser.copy(:RAZORCLAW, :SCOPELENS);

Battle.ItemEffects.CriticalCalcFromUser.add(:LEEK,
	block: (item, user, target, move, c) => {
		if (user.isSpecies(Speciess.FARFETCHD) || user.isSpecies(Speciess.SIRFETCHD)) next c + 2;
	}
)

Battle.ItemEffects.CriticalCalcFromUser.copy(:LEEK, :STICK);

//===============================================================================
// CriticalCalcFromTarget handlers
//===============================================================================

// There aren't any!

//===============================================================================
// OnBeingHit handlers
//===============================================================================

Battle.ItemEffects.OnBeingHit.add(:ABSORBBULB,
	block: (item, user, target, move, battle) => {
		if (move.calcType != Types.WATER) continue;
		if (!target.CanRaiseStatStage(:SPECIAL_ATTACK, target)) continue;
		battle.CommonAnimation("UseItem", target);
		target.RaiseStatStageByCause(:SPECIAL_ATTACK, 1, target, target.itemName);
		target.HeldItemTriggered(item);
	}
)

Battle.ItemEffects.OnBeingHit.add(:AIRBALLOON,
	block: (item, user, target, move, battle) => {
		battle.Display(_INTL("{1}'s {2} popped!", target.ToString(), target.itemName));
		target.ConsumeItem(false, true);
		target.Symbiosis;
	}
)

Battle.ItemEffects.OnBeingHit.add(:CELLBATTERY,
	block: (item, user, target, move, battle) => {
		if (move.calcType != Types.ELECTRIC) continue;
		if (!target.CanRaiseStatStage(:ATTACK, target)) continue;
		battle.CommonAnimation("UseItem", target);
		target.RaiseStatStageByCause(:ATTACK, 1, target, target.itemName);
		target.HeldItemTriggered(item);
	}
)

Battle.ItemEffects.OnBeingHit.add(:ENIGMABERRY,
	block: (item, user, target, move, battle) => {
		if (target.damageState.substitute ||
						target.damageState.disguise || target.damageState.iceFace) continue;
		if (!Effectiveness.super_effective(target.damageState.typeMod)) continue;
		if (Battle.ItemEffects.triggerOnBeingHitPositiveBerry(item, target, battle, false)) {
			target.HeldItemTriggered(item);
		}
	}
)

Battle.ItemEffects.OnBeingHit.add(:JABOCABERRY,
	block: (item, user, target, move, battle) => {
		if (!target.canConsumeBerry()) continue;
		if (!move.physicalMove()) continue;
		if (!user.takesIndirectDamage()) continue;
		amt = user.totalhp / 8;
		ripening = false;
		if (target.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(target);
			amt *= 2;
			ripening = true;
		}
		battle.CommonAnimation("EatBerry", target);
		if (ripening) battle.HideAbilitySplash(target);
		battle.scene.DamageAnimation(user);
		user.ReduceHP(amt, false);
		battle.Display(_INTL("{1} consumed its {2} and hurt {3}!", target.ToString(),
			target.itemName, user.ToString(true)));
		target.HeldItemTriggered(item);
	}
)

// NOTE: Kee Berry supposedly shouldn't trigger if the user has Sheer Force, but
//       I'm ignoring this. Weakness Policy has the same kind of effect and
//       nowhere says it should be stopped by Sheer Force. I suspect this
//       stoppage is either a false report that no one ever corrected, or an
//       effect that later changed and wasn't noticed.
Battle.ItemEffects.OnBeingHit.add(:KEEBERRY,
	block: (item, user, target, move, battle) => {
		if (!move.physicalMove()) continue;
		if (Battle.ItemEffects.triggerOnBeingHitPositiveBerry(item, target, battle, false)) {
			target.HeldItemTriggered(item);
		}
	}
)

Battle.ItemEffects.OnBeingHit.add(:LUMINOUSMOSS,
	block: (item, user, target, move, battle) => {
		if (move.calcType != Types.WATER) continue;
		if (!target.CanRaiseStatStage(:SPECIAL_DEFENSE, target)) continue;
		battle.CommonAnimation("UseItem", target);
		target.RaiseStatStageByCause(:SPECIAL_DEFENSE, 1, target, target.itemName);
		target.HeldItemTriggered(item);
	}
)

// NOTE: Maranga Berry supposedly shouldn't trigger if the user has Sheer Force,
//       but I'm ignoring this. Weakness Policy has the same kind of effect and
//       nowhere says it should be stopped by Sheer Force. I suspect this
//       stoppage is either a false report that no one ever corrected, or an
//       effect that later changed and wasn't noticed.
Battle.ItemEffects.OnBeingHit.add(:MARANGABERRY,
	block: (item, user, target, move, battle) => {
		if (!move.specialMove()) continue;
		if (Battle.ItemEffects.triggerOnBeingHitPositiveBerry(item, target, battle, false)) {
			target.HeldItemTriggered(item);
		}
	}
)

Battle.ItemEffects.OnBeingHit.add(:ROCKYHELMET,
	block: (item, user, target, move, battle) => {
		if (!move.ContactMove(user) || !user.affectedByContactEffect()) continue;
		if (!user.takesIndirectDamage()) continue;
		battle.scene.DamageAnimation(user);
		user.ReduceHP(user.totalhp / 6, false);
		battle.Display(_INTL("{1} was hurt by the {2}!", user.ToString(), target.itemName));
	}
)

Battle.ItemEffects.OnBeingHit.add(:ROWAPBERRY,
	block: (item, user, target, move, battle) => {
		if (!target.canConsumeBerry()) continue;
		if (!move.specialMove()) continue;
		if (!user.takesIndirectDamage()) continue;
		amt = user.totalhp / 8;
		ripening = false;
		if (target.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(target);
			amt *= 2;
			ripening = true;
		}
		battle.CommonAnimation("EatBerry", target);
		if (ripening) battle.HideAbilitySplash(target);
		battle.scene.DamageAnimation(user);
		user.ReduceHP(amt, false);
		battle.Display(_INTL("{1} consumed its {2} and hurt {3}!", target.ToString(),
			target.itemName, user.ToString(true)));
		target.HeldItemTriggered(item);
	}
)

Battle.ItemEffects.OnBeingHit.add(:SNOWBALL,
	block: (item, user, target, move, battle) => {
		if (move.calcType != Types.ICE) continue;
		if (!target.CanRaiseStatStage(:ATTACK, target)) continue;
		battle.CommonAnimation("UseItem", target);
		target.RaiseStatStageByCause(:ATTACK, 1, target, target.itemName);
		target.HeldItemTriggered(item);
	}
)

Battle.ItemEffects.OnBeingHit.add(:STICKYBARB,
	block: (item, user, target, move, battle) => {
		if (!move.ContactMove(user) || !user.affectedByContactEffect()) continue;
		if (user.fainted() || user.item) continue;
		user.item = target.item;
		target.item = null;
		if (target.hasActiveAbility(Abilitys.UNBURDEN)) target.effects.Unburden = true;
		if (battle.wildBattle() && !user.opposes() &&
			!user.initialItem && user.item == target.initialItem) {
			user.setInitialItem(user.item);
			target.setInitialItem(null);
		}
		battle.Display(_INTL("{1}'s {2} was transferred to {3}!",
			target.ToString(), user.itemName, user.ToString(true)));
	}
)

Battle.ItemEffects.OnBeingHit.add(:WEAKNESSPOLICY,
	block: (item, user, target, move, battle) => {
		if (target.damageState.disguise || target.damageState.iceFace) continue;
		if (!Effectiveness.super_effective(target.damageState.typeMod)) continue;
		if (!target.CanRaiseStatStage(:ATTACK, target) &&
						!target.CanRaiseStatStage(:SPECIAL_ATTACK, target)) continue;
		battle.CommonAnimation("UseItem", target);
		showAnim = true;
		if (target.CanRaiseStatStage(:ATTACK, target)) {
			target.RaiseStatStageByCause(:ATTACK, 2, target, target.itemName, showAnim);
			showAnim = false;
		}
		if (target.CanRaiseStatStage(:SPECIAL_ATTACK, target)) {
			target.RaiseStatStageByCause(:SPECIAL_ATTACK, 2, target, target.itemName, showAnim);
		}
		battle.Display(_INTL("The {1} was used up...", target.itemName));
		target.HeldItemTriggered(item);
	}
)

//===============================================================================
// OnBeingHitPositiveBerry handlers
// NOTE: This is for berries that have an effect when Pluck/Bug Bite/Fling
//       forces their use.
//===============================================================================

Battle.ItemEffects.OnBeingHitPositiveBerry.add(:ENIGMABERRY,
	block: (item, battler, battle, forced) => {
		if (!battler.canHeal()) next false;
		if (!forced && !battler.canConsumeBerry()) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		amt = battler.totalhp / 4;
		ripening = false;
		if (battler.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(battler, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		if (ripening) battle.HideAbilitySplash(battler);
		battler.RecoverHP(amt);
		if (forced) {
			battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} restored its health using its {2}!", battler.ToString(), itemName));
		}
		next true;
	}
)

Battle.ItemEffects.OnBeingHitPositiveBerry.add(:KEEBERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (!battler.CanRaiseStatStage(:DEFENSE, battler)) next false;
		itemName = GameData.Item.get(item).name;
		amt = 1;
		ripening = false;
		if (battler.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(battler, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		if (ripening) battle.HideAbilitySplash(battler);
		if (!forced) next battler.RaiseStatStageByCause(:DEFENSE, amt, battler, itemName);
		Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		next battler.RaiseStatStage(:DEFENSE, amt, battler);
	}
)

Battle.ItemEffects.OnBeingHitPositiveBerry.add(:MARANGABERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		if (!battler.CanRaiseStatStage(:SPECIAL_DEFENSE, battler)) next false;
		itemName = GameData.Item.get(item).name;
		amt = 1;
		ripening = false;
		if (battler.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(battler, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		if (ripening) battle.HideAbilitySplash(battler);
		if (!forced) next battler.RaiseStatStageByCause(:SPECIAL_DEFENSE, amt, battler, itemName);
		Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		next battler.RaiseStatStage(:SPECIAL_DEFENSE, amt, battler);
	}
)

//===============================================================================
// AfterMoveUseFromTarget handlers
//===============================================================================

Battle.ItemEffects.AfterMoveUseFromTarget.add(:EJECTBUTTON,
	block: (item, battler, user, move, switched_battlers, battle) => {
		if (!switched_battlers.empty()) continue;
		if (battle.AllFainted(battler.idxOpposingSide)) continue;
		if (!battle.CanChooseNonActive(battler.index)) continue;
		battle.CommonAnimation("UseItem", battler);
		battle.Display(_INTL("{1} is switched out with the {2}!", battler.ToString(), battler.itemName));
		battler.ConsumeItem(true, false);
		newPkmn = battle.GetReplacementPokemonIndex(battler.index);   // Owner chooses
		if (newPkmn < 0) continue;
		battle.RecallAndReplace(battler.index, newPkmn);
		battle.ClearChoice(battler.index);   // Replacement Pokémon does nothing this round
		switched_battlers.Add(battler.index);
		if (battler.index == user.index) battle.moldBreaker = false;
		battle.OnBattlerEnteringBattle(battler.index);
	}
)

Battle.ItemEffects.AfterMoveUseFromTarget.add(:REDCARD,
	block: (item, battler, user, move, switched_battlers, battle) => {
		if (!switched_battlers.empty() || user.fainted()) continue;
		newPkmn = battle.GetReplacementPokemonIndex(user.index, true);   // Random
		if (newPkmn < 0) continue;
		battle.CommonAnimation("UseItem", battler);
		battle.Display(_INTL("{1} held up its {2} against {3}!",
			battler.ToString(), battler.itemName, user.ToString(true)));
		battler.ConsumeItem;
		if (user.hasActiveAbility(Abilitys.SUCTIONCUPS) && !user.being_mold_broken()) {
			battle.ShowAbilitySplash(user);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} anchors itself!", user.ToString()));
			} else {
				battle.Display(_INTL("{1} anchors itself with {2}!", user.ToString(), user.abilityName));
			}
			battle.HideAbilitySplash(user);
			continue;
		}
		if (user.effects.Ingrain) {
			battle.Display(_INTL("{1} anchored itself with its roots!", user.ToString()));
			continue;
		}
		battle.RecallAndReplace(user.index, newPkmn, true);
		battle.Display(_INTL("{1} was dragged out!", user.ToString()));
		battle.ClearChoice(user.index);   // Replacement Pokémon does nothing this round
		switched_battlers.Add(user.index);
		battle.moldBreaker = false;
		battle.OnBattlerEnteringBattle(user.index);
	}
)

//===============================================================================
// AfterMoveUseFromUser handlers
//===============================================================================

Battle.ItemEffects.AfterMoveUseFromUser.add(:LIFEORB,
	block: (item, user, targets, move, numHits, battle) => {
		if (!user.takesIndirectDamage()) continue;
		if (!move.DamagingMove() || numHits == 0) continue;
		hitBattler = false;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b.damageState.unaffected && !b.damageState.substitute) hitBattler = true;
			if (hitBattler) break;
		}
		if (!hitBattler) continue;
		Debug.Log($"[Item triggered] {user.ToString()}'s {user.itemName} (recoil)");
		user.ReduceHP(user.totalhp / 10);
		battle.Display(_INTL("{1} lost some of its HP!", user.ToString()));
		user.ItemHPHealCheck;
		if (user.fainted()) user.Faint;
	}
)

// NOTE: In the official games, Shell Bell does not prevent Emergency Exit/Wimp
//       Out triggering even if Shell Bell heals the holder back to 50% HP or
//       more. Essentials ignores this exception.
Battle.ItemEffects.AfterMoveUseFromUser.add(:SHELLBELL,
	block: (item, user, targets, move, numHits, battle) => {
		if (!user.canHeal()) continue;
		totalDamage = 0;
		targets.each(b => totalDamage += b.damageState.totalHPLost);
		if (totalDamage <= 0) continue;
		user.RecoverHP(totalDamage / 8);
		battle.Display(_INTL("{1} restored a little HP using its {2}!",
			user.ToString(), user.itemName));
	}
)

Battle.ItemEffects.AfterMoveUseFromUser.add(:THROATSPRAY,
	block: (item, user, targets, move, numHits, battle) => {
		if (battle.AllFainted(user.idxOwnSide) ||
						battle.AllFainted(user.idxOpposingSide)) continue;
		if (!move.soundMove() || numHits == 0) continue;
		if (!user.CanRaiseStatStage(:SPECIAL_ATTACK, user)) continue;
		battle.CommonAnimation("UseItem", user);
		user.RaiseStatStage(:SPECIAL_ATTACK, 1, user);
		user.ConsumeItem;
	}
)

//===============================================================================
// OnEndOfUsingMove handlers
//===============================================================================

Battle.ItemEffects.OnEndOfUsingMove.Add(Moves.LEPPABERRY,
	block: (item, battler, battle, forced) => {
		if (!forced && !battler.canConsumeBerry()) next false;
		found_empty_moves = new List<string>();
		found_partial_moves = new List<string>();
		battler.pokemon.moves.each_with_index do |move, i|
			if (move.total_pp <= 0 || move.pp == move.total_pp) continue;
			(move.pp == 0) ? found_empty_moves.Add(i) : found_partial_moves.Add(i)
		}
		if (found_empty_moves.empty() && (!forced || found_partial_moves.empty())) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		amt = 10;
		ripening = false;
		if (battler.hasActiveAbility(Abilitys.RIPEN)) {
			battle.ShowAbilitySplash(battler, forced);
			amt *= 2;
			ripening = true;
		}
		if (!forced) battle.CommonAnimation("EatBerry", battler);
		if (ripening) battle.HideAbilitySplash(battler);
		choice = found_empty_moves.first;
		if (forced && choice.null()) choice = found_partial_moves.first;
		pkmnMove = battler.pokemon.moves[choice];
		pkmnMove.pp += amt;
		if (pkmnMove.pp > pkmnMove.total_pp) pkmnMove.pp = pkmnMove.total_pp;
		battler.moves[choice].pp = pkmnMove.pp;
		moveName = pkmnMove.name;
		if (forced) {
			battle.Display(_INTL("{1} restored its {2}'s PP.", battler.ToString(), moveName));
		} else {
			battle.Display(_INTL("{1}'s {2} restored its {3}'s PP!", battler.ToString(), itemName, moveName));
		}
		next true;
	}
)

//===============================================================================
// OnEndOfUsingMoveStatRestore handlers
//===============================================================================

Battle.ItemEffects.OnEndOfUsingMoveStatRestore.add(:WHITEHERB,
	block: (item, battler, battle, forced) => {
		reducedStats = false;
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (battler.stages[s.id] >= 0) continue;
			battler.stages[s.id] = 0;
			battler.statsRaisedThisRound = true;
			reducedStats = true;
		}
		if (!reducedStats) next false;
		itemName = GameData.Item.get(item).name;
		if (forced) Debug.Log($"[Item triggered] {battler.ToString()}'s {itemName}");
		if (!forced) battle.CommonAnimation("UseItem", battler);
		if (forced) {
			battle.Display(_INTL("{1}'s status returned to normal!", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} returned its status to normal using its {2}!",
				battler.ToString(), itemName));
		}
		next true;
	}
)

//===============================================================================
// ExpGainModifier handlers
//===============================================================================

Battle.ItemEffects.ExpGainModifier.add(:LUCKYEGG,
	block: (item, battler, exp) => {
		next exp * 3 / 2;
	}
)

//===============================================================================
// EVGainModifier handlers
//===============================================================================

Battle.ItemEffects.EVGainModifier.add(:MACHOBRACE,
	block: (item, battler, evYield) => {
		evYield.each_key(stat => evYield[stat] *= 2);
	}
)

Battle.ItemEffects.EVGainModifier.add(:POWERANKLET,
	block: (item, battler, evYield) => {
		evYield[:SPEED] += (Settings.MORE_EVS_FROM_POWER_ITEMS) ? 8 : 4;
	}
)

Battle.ItemEffects.EVGainModifier.add(:POWERBAND,
	block: (item, battler, evYield) => {
		evYield[:SPECIAL_DEFENSE] += (Settings.MORE_EVS_FROM_POWER_ITEMS) ? 8 : 4;
	}
)

Battle.ItemEffects.EVGainModifier.add(:POWERBELT,
	block: (item, battler, evYield) => {
		evYield[:DEFENSE] += (Settings.MORE_EVS_FROM_POWER_ITEMS) ? 8 : 4;
	}
)

Battle.ItemEffects.EVGainModifier.add(:POWERBRACER,
	block: (item, battler, evYield) => {
		evYield[:ATTACK] += (Settings.MORE_EVS_FROM_POWER_ITEMS) ? 8 : 4;
	}
)

Battle.ItemEffects.EVGainModifier.add(:POWERLENS,
	block: (item, battler, evYield) => {
		evYield[:SPECIAL_ATTACK] += (Settings.MORE_EVS_FROM_POWER_ITEMS) ? 8 : 4;
	}
)

Battle.ItemEffects.EVGainModifier.add(:POWERWEIGHT,
	block: (item, battler, evYield) => {
		evYield[:HP] += (Settings.MORE_EVS_FROM_POWER_ITEMS) ? 8 : 4;
	}
)

//===============================================================================
// WeatherExtender handlers
//===============================================================================

Battle.ItemEffects.WeatherExtender.add(:DAMPROCK,
	block: (item, weather, duration, battler, battle) => {
		if (weather == weathers.Rain) next 8;
	}
)

Battle.ItemEffects.WeatherExtender.add(:HEATROCK,
	block: (item, weather, duration, battler, battle) => {
		if (weather == weathers.Sun) next 8;
	}
)

Battle.ItemEffects.WeatherExtender.add(:ICYROCK,
	block: (item, weather, duration, battler, battle) => {
		if (new []{:Hail, :Snowstorm}.Contains(weather)) next 8;
	}
)

Battle.ItemEffects.WeatherExtender.add(:SMOOTHROCK,
	block: (item, weather, duration, battler, battle) => {
		if (weather == weathers.Sandstorm) next 8;
	}
)

//===============================================================================
// TerrainExtender handlers
//===============================================================================

Battle.ItemEffects.TerrainExtender.add(:TERRAINEXTENDER,
	block: (item, terrain, duration, battler, battle) => {
		next 8;
	}
)

//===============================================================================
// TerrainStatBoost handlers
//===============================================================================

Battle.ItemEffects.TerrainStatBoost.add(:ELECTRICSEED,
	block: (item, battler, battle) => {
		if (battle.field.terrain != :Electric) next false;
		if (!battler.CanRaiseStatStage(:DEFENSE, battler)) next false;
		itemName = GameData.Item.get(item).name;
		battle.CommonAnimation("UseItem", battler);
		next battler.RaiseStatStageByCause(:DEFENSE, 1, battler, itemName);
	}
)

Battle.ItemEffects.TerrainStatBoost.add(:GRASSYSEED,
	block: (item, battler, battle) => {
		if (battle.field.terrain != :Grassy) next false;
		if (!battler.CanRaiseStatStage(:DEFENSE, battler)) next false;
		itemName = GameData.Item.get(item).name;
		battle.CommonAnimation("UseItem", battler);
		next battler.RaiseStatStageByCause(:DEFENSE, 1, battler, itemName);
	}
)

Battle.ItemEffects.TerrainStatBoost.add(:MISTYSEED,
	block: (item, battler, battle) => {
		if (battle.field.terrain != :Misty) next false;
		if (!battler.CanRaiseStatStage(:SPECIAL_DEFENSE, battler)) next false;
		itemName = GameData.Item.get(item).name;
		battle.CommonAnimation("UseItem", battler);
		next battler.RaiseStatStageByCause(:SPECIAL_DEFENSE, 1, battler, itemName);
	}
)

Battle.ItemEffects.TerrainStatBoost.add(:PSYCHICSEED,
	block: (item, battler, battle) => {
		if (battle.field.terrain != :Psychic) next false;
		if (!battler.CanRaiseStatStage(:SPECIAL_DEFENSE, battler)) next false;
		itemName = GameData.Item.get(item).name;
		battle.CommonAnimation("UseItem", battler);
		next battler.RaiseStatStageByCause(:SPECIAL_DEFENSE, 1, battler, itemName);
	}
)

//===============================================================================
// EndOfRoundHealing handlers
//===============================================================================

Battle.ItemEffects.EndOfRoundHealing.add(:BLACKSLUDGE,
	block: (item, battler, battle) => {
		if (battler.Type == Types.POISON) {
			if (!battler.canHeal()) continue;
			battle.CommonAnimation("UseItem", battler);
			battler.RecoverHP(battler.totalhp / 16);
			battle.Display(_INTL("{1} restored a little HP using its {2}!",
				battler.ToString(), battler.itemName));
		} else if (battler.takesIndirectDamage()) {
			battle.CommonAnimation("UseItem", battler);
			battler.TakeEffectDamage(battler.totalhp / 8) do |hp_lost|
				battle.Display(_INTL("{1} is hurt by its {2}!", battler.ToString(), battler.itemName));
			}
		}
	}
)

Battle.ItemEffects.EndOfRoundHealing.add(:LEFTOVERS,
	block: (item, battler, battle) => {
		if (!battler.canHeal()) continue;
		battle.CommonAnimation("UseItem", battler);
		battler.RecoverHP(battler.totalhp / 16);
		battle.Display(_INTL("{1} restored a little HP using its {2}!",
			battler.ToString(), battler.itemName));
	}
)

//===============================================================================
// EndOfRoundEffect handlers
//===============================================================================

Battle.ItemEffects.EndOfRoundEffect.add(:FLAMEORB,
	block: (item, battler, battle) => {
		if (!battler.CanBurn(battler, false)) continue;
		battler.Burn(null, _INTL("{1} was burned by the {2}!", battler.ToString(), battler.itemName));
	}
)

Battle.ItemEffects.EndOfRoundEffect.add(:STICKYBARB,
	block: (item, battler, battle) => {
		if (!battler.takesIndirectDamage()) continue;
		battle.scene.DamageAnimation(battler);
		battler.TakeEffectDamage(battler.totalhp / 8, false) do |hp_lost|
			battle.Display(_INTL("{1} is hurt by its {2}!", battler.ToString(), battler.itemName));
		}
	}
)

Battle.ItemEffects.EndOfRoundEffect.add(:TOXICORB,
	block: (item, battler, battle) => {
		if (!battler.CanPoison(battler, false)) continue;
		battler.Poison(null, _INTL("{1} was badly poisoned by the {2}!",
			battler.ToString(), battler.itemName), true);
	}
)

//===============================================================================
// CertainSwitching handlers
//===============================================================================

Battle.ItemEffects.CertainSwitching.add(:SHEDSHELL,
	block: (item, battler, battle) => {
		next true;
	}
)

//===============================================================================
// TrappingByTarget handlers
//===============================================================================

// There aren't any!

//===============================================================================
// OnSwitchIn handlers
//===============================================================================

Battle.ItemEffects.OnSwitchIn.add(:AIRBALLOON,
	block: (item, battler, battle) => {
		battle.Display(_INTL("{1} floats in the air with its {2}!",
			battler.ToString(), battler.itemName));
	}
)

Battle.ItemEffects.OnSwitchIn.add(:ROOMSERVICE,
	block: (item, battler, battle) => {
		if (battle.field.effects.TrickRoom == 0) continue;
		if (!battler.CanLowerStatStage(:SPEED)) continue;
		battle.CommonAnimation("UseItem", battler);
		battler.LowerStatStage(:SPEED, 1, null);
		battler.ConsumeItem;
	}
)

//===============================================================================
// OnIntimidated handlers
//===============================================================================

Battle.ItemEffects.OnIntimidated.add(:ADRENALINEORB,
	block: (item, battler, battle) => {
		if (!battler.CanRaiseStatStage(:SPEED, battler)) next false;
		itemName = GameData.Item.get(item).name;
		battle.CommonAnimation("UseItem", battler);
		next battler.RaiseStatStageByCause(:SPEED, 1, battler, itemName);
	}
)

//===============================================================================
// CertainEscapeFromBattle handlers
//===============================================================================

Battle.ItemEffects.CertainEscapeFromBattle.add(:SMOKEBALL,
	block: (item, battler) => {
		next true;
	}
)
