//===============================================================================
//
//===============================================================================
public static partial class Battle.AbilityEffects {
	SpeedCalc                        = new AbilityHandlerHash();
	WeightCalc                       = new AbilityHandlerHash();
	// Battler's HP/stat changed
	OnHPDroppedBelowHalf             = new AbilityHandlerHash();
	// Battler's status problem
	StatusCheckNonIgnorable          = new AbilityHandlerHash();   // Comatose
	StatusImmunity                   = new AbilityHandlerHash();
	StatusImmunityNonIgnorable       = new AbilityHandlerHash();
	StatusImmunityFromAlly           = new AbilityHandlerHash();
	OnStatusInflicted                = new AbilityHandlerHash();   // Synchronize
	StatusCure                       = new AbilityHandlerHash();
	// Battler's stat stages
	StatLossImmunity                 = new AbilityHandlerHash();
	StatLossImmunityNonIgnorable     = new AbilityHandlerHash();   // Full Metal Body
	StatLossImmunityFromAlly         = new AbilityHandlerHash();   // Flower Veil
	OnStatGain                       = new AbilityHandlerHash();   // None!
	OnStatLoss                       = new AbilityHandlerHash();
	// Priority and turn order
	PriorityChange                   = new AbilityHandlerHash();
	PriorityBracketChange            = new AbilityHandlerHash();   // Stall
	PriorityBracketUse               = new AbilityHandlerHash();   // None!
	// Move usage failures
	OnFlinch                         = new AbilityHandlerHash();   // Steadfast
	MoveBlocking                     = new AbilityHandlerHash();
	MoveImmunity                     = new AbilityHandlerHash();
	// Move usage
	ModifyMoveBaseType               = new AbilityHandlerHash();
	// Accuracy calculation
	AccuracyCalcFromUser             = new AbilityHandlerHash();
	AccuracyCalcFromAlly             = new AbilityHandlerHash();   // Victory Star
	AccuracyCalcFromTarget           = new AbilityHandlerHash();
	// Damage calculation
	DamageCalcFromUser               = new AbilityHandlerHash();
	DamageCalcFromAlly               = new AbilityHandlerHash();
	DamageCalcFromTarget             = new AbilityHandlerHash();
	DamageCalcFromTargetNonIgnorable = new AbilityHandlerHash();
	DamageCalcFromTargetAlly         = new AbilityHandlerHash();
	CriticalCalcFromUser             = new AbilityHandlerHash();
	CriticalCalcFromTarget           = new AbilityHandlerHash();
	// Upon a move hitting a target
	OnBeingHit                       = new AbilityHandlerHash();
	OnDealingHit                     = new AbilityHandlerHash();   // Poison Touch
	// Abilities that trigger at the end of using a move
	OnEndOfUsingMove                 = new AbilityHandlerHash();
	AfterMoveUseFromTarget           = new AbilityHandlerHash();
	// End Of Round
	EndOfRoundWeather                = new AbilityHandlerHash();
	EndOfRoundHealing                = new AbilityHandlerHash();
	EndOfRoundEffect                 = new AbilityHandlerHash();
	EndOfRoundGainItem               = new AbilityHandlerHash();
	// Switching and fainting
	CertainSwitching                 = new AbilityHandlerHash();   // None!
	TrappingByTarget                 = new AbilityHandlerHash();
	OnSwitchIn                       = new AbilityHandlerHash();
	OnSwitchOut                      = new AbilityHandlerHash();
	ChangeOnBattlerFainting          = new AbilityHandlerHash();
	OnBattlerFainting                = new AbilityHandlerHash();   // Soul-Heart
	OnTerrainChange                  = new AbilityHandlerHash();   // Mimicry
	OnIntimidated                    = new AbilityHandlerHash();   // Rattled (Gen 8)
	// Running from battle
	CertainEscapeFromBattle          = new AbilityHandlerHash();   // Run Away

	//-----------------------------------------------------------------------------

	public static void trigger(hash, *args, ret: false) {
		new_ret = hash.trigger(*args);
		return (!new_ret.null()) ? new_ret : ret;
	}

	//-----------------------------------------------------------------------------

	public static void triggerSpeedCalc(ability, battler, mult) {
		return trigger(SpeedCalc, ability, battler, mult, ret: mult);
	}

	public static void triggerWeightCalc(ability, battler, weight) {
		return trigger(WeightCalc, ability, battler, weight, ret: weight);
	}

	//-----------------------------------------------------------------------------

	public static void triggerOnHPDroppedBelowHalf(ability, user, move_user, battle) {
		return trigger(OnHPDroppedBelowHalf, ability, user, move_user, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerStatusCheckNonIgnorable(ability, battler, status) {
		return trigger(StatusCheckNonIgnorable, ability, battler, status);
	}

	public static void triggerStatusImmunity(ability, battler, status) {
		return trigger(StatusImmunity, ability, battler, status);
	}

	public static void triggerStatusImmunityNonIgnorable(ability, battler, status) {
		return trigger(StatusImmunityNonIgnorable, ability, battler, status);
	}

	public static void triggerStatusImmunityFromAlly(ability, battler, status) {
		return trigger(StatusImmunityFromAlly, ability, battler, status);
	}

	public static void triggerOnStatusInflicted(ability, battler, user, status) {
		OnStatusInflicted.trigger(ability, battler, user, status);
	}

	public static void triggerStatusCure(ability, battler) {
		return trigger(StatusCure, ability, battler);
	}

	//-----------------------------------------------------------------------------

	public static void triggerStatLossImmunity(ability, battler, stat, battle, show_messages) {
		return trigger(StatLossImmunity, ability, battler, stat, battle, show_messages);
	}

	public static void triggerStatLossImmunityNonIgnorable(ability, battler, stat, battle, show_messages) {
		return trigger(StatLossImmunityNonIgnorable, ability, battler, stat, battle, show_messages);
	}

	public static void triggerStatLossImmunityFromAlly(ability, bearer, battler, stat, battle, show_messages) {
		return trigger(StatLossImmunityFromAlly, ability, bearer, battler, stat, battle, show_messages);
	}

	public static void triggerOnStatGain(ability, battler, stat, user) {
		OnStatGain.trigger(ability, battler, stat, user);
	}

	public static void triggerOnStatLoss(ability, battler, stat, user) {
		OnStatLoss.trigger(ability, battler, stat, user);
	}

	//-----------------------------------------------------------------------------

	public static void triggerPriorityChange(ability, battler, move, priority) {
		return trigger(PriorityChange, ability, battler, move, priority, ret: priority);
	}

	public static void triggerPriorityBracketChange(ability, battler, battle) {
		return trigger(PriorityBracketChange, ability, battler, battle, ret: 0);
	}

	public static void triggerPriorityBracketUse(ability, battler, battle) {
		PriorityBracketUse.trigger(ability, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerOnFlinch(ability, battler, battle) {
		OnFlinch.trigger(ability, battler, battle);
	}

	public static void triggerMoveBlocking(ability, bearer, user, targets, move, battle) {
		return trigger(MoveBlocking, ability, bearer, user, targets, move, battle);
	}

	public static void triggerMoveImmunity(ability, user, target, move, type, battle, show_message) {
		return trigger(MoveImmunity, ability, user, target, move, type, battle, show_message);
	}

	//-----------------------------------------------------------------------------

	public static void triggerModifyMoveBaseType(ability, user, move, type) {
		return trigger(ModifyMoveBaseType, ability, user, move, type, ret: type);
	}

	//-----------------------------------------------------------------------------

	public static void triggerAccuracyCalcFromUser(ability, mods, user, target, move, type) {
		AccuracyCalcFromUser.trigger(ability, mods, user, target, move, type);
	}

	public static void triggerAccuracyCalcFromAlly(ability, mods, user, target, move, type) {
		AccuracyCalcFromAlly.trigger(ability, mods, user, target, move, type);
	}

	public static void triggerAccuracyCalcFromTarget(ability, mods, user, target, move, type) {
		AccuracyCalcFromTarget.trigger(ability, mods, user, target, move, type);
	}

	//-----------------------------------------------------------------------------

	public static void triggerDamageCalcFromUser(ability, user, target, move, mults, power, type) {
		DamageCalcFromUser.trigger(ability, user, target, move, mults, power, type);
	}

	public static void triggerDamageCalcFromAlly(ability, user, target, move, mults, power, type) {
		DamageCalcFromAlly.trigger(ability, user, target, move, mults, power, type);
	}

	public static void triggerDamageCalcFromTarget(ability, user, target, move, mults, power, type) {
		DamageCalcFromTarget.trigger(ability, user, target, move, mults, power, type);
	}

	public static void triggerDamageCalcFromTargetNonIgnorable(ability, user, target, move, mults, power, type) {
		DamageCalcFromTargetNonIgnorable.trigger(ability, user, target, move, mults, power, type);
	}

	public static void triggerDamageCalcFromTargetAlly(ability, user, target, move, mults, power, type) {
		DamageCalcFromTargetAlly.trigger(ability, user, target, move, mults, power, type);
	}

	public static void triggerCriticalCalcFromUser(ability, user, target, move, crit_stage) {
		return trigger(CriticalCalcFromUser, ability, user, target, move, crit_stage, ret: crit_stage);
	}

	public static void triggerCriticalCalcFromTarget(ability, user, target, move, crit_stage) {
		return trigger(CriticalCalcFromTarget, ability, user, target, move, crit_stage, ret: crit_stage);
	}

	//-----------------------------------------------------------------------------

	public static void triggerOnBeingHit(ability, user, target, move, battle) {
		OnBeingHit.trigger(ability, user, target, move, battle);
	}

	public static void triggerOnDealingHit(ability, user, target, move, battle) {
		OnDealingHit.trigger(ability, user, target, move, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerOnEndOfUsingMove(ability, user, targets, move, battle) {
		OnEndOfUsingMove.trigger(ability, user, targets, move, battle);
	}

	public static void triggerAfterMoveUseFromTarget(ability, target, user, move, switched_battlers, battle) {
		AfterMoveUseFromTarget.trigger(ability, target, user, move, switched_battlers, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerEndOfRoundWeather(ability, weather, battler, battle) {
		EndOfRoundWeather.trigger(ability, weather, battler, battle);
	}

	public static void triggerEndOfRoundHealing(ability, battler, battle) {
		EndOfRoundHealing.trigger(ability, battler, battle);
	}

	public static void triggerEndOfRoundEffect(ability, battler, battle) {
		EndOfRoundEffect.trigger(ability, battler, battle);
	}

	public static void triggerEndOfRoundGainItem(ability, battler, battle) {
		EndOfRoundGainItem.trigger(ability, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerCertainSwitching(ability, switcher, battle) {
		return trigger(CertainSwitching, ability, switcher, battle);
	}

	public static void triggerTrappingByTarget(ability, switcher, bearer, battle) {
		return trigger(TrappingByTarget, ability, switcher, bearer, battle);
	}

	public static void triggerOnSwitchIn(ability, battler, battle, switch_in = false) {
		OnSwitchIn.trigger(ability, battler, battle, switch_in);
	}

	public static void triggerOnSwitchOut(ability, battler, end_of_battle) {
		OnSwitchOut.trigger(ability, battler, end_of_battle);
	}

	public static void triggerChangeOnBattlerFainting(ability, battler, fainted, battle) {
		ChangeOnBattlerFainting.trigger(ability, battler, fainted, battle);
	}

	public static void triggerOnBattlerFainting(ability, battler, fainted, battle) {
		OnBattlerFainting.trigger(ability, battler, fainted, battle);
	}

	public static void triggerOnTerrainChange(ability, battler, battle, ability_changed) {
		OnTerrainChange.trigger(ability, battler, battle, ability_changed);
	}

	public static void triggerOnIntimidated(ability, battler, battle) {
		OnIntimidated.trigger(ability, battler, battle);
	}

	//-----------------------------------------------------------------------------

	public static void triggerCertainEscapeFromBattle(ability, battler) {
		return trigger(CertainEscapeFromBattle, ability, battler);
	}
}

//===============================================================================
// SpeedCalc handlers
//===============================================================================

Battle.AbilityEffects.SpeedCalc.add(:CHLOROPHYLL,
	block: (ability, battler, mult) => {
		if (new []{:Sun, :HarshSun}.Contains(battler.effectiveWeather)) next mult * 2;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:QUICKFEET,
	block: (ability, battler, mult) => {
		if (battler.HasAnyStatus()) next mult * 1.5;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:SANDRUSH,
	block: (ability, battler, mult) => {
		if ([:Sandstorm].Contains(battler.effectiveWeather)) next mult * 2;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:SLOWSTART,
	block: (ability, battler, mult) => {
		if (battler.effects.SlowStart > 0) next mult / 2;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:SLUSHRUSH,
	block: (ability, battler, mult) => {
		if (new []{:Hail, :Snowstorm}.Contains(battler.effectiveWeather)) next mult * 2;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:SURGESURFER,
	block: (ability, battler, mult) => {
		if (battler.battle.field.terrain == :Electric) next mult * 2;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:SWIFTSWIM,
	block: (ability, battler, mult) => {
		if (new []{:Rain, :HeavyRain}.Contains(battler.effectiveWeather)) next mult * 2;
	}
)

Battle.AbilityEffects.SpeedCalc.add(:UNBURDEN,
	block: (ability, battler, mult) => {
		if (battler.effects.Unburden && !battler.item) next mult * 2;
	}
)

//===============================================================================
// WeightCalcy handlers
//===============================================================================

Battle.AbilityEffects.WeightCalc.add(:HEAVYMETAL,
	block: (ability, battler, w) => {
		next w * 2;
	}
)

Battle.AbilityEffects.WeightCalc.add(:LIGHTMETAL,
	block: (ability, battler, w) => {
		next (int)Math.Max(w / 2, 1);
	}
)

//===============================================================================
// OnHPDroppedBelowHalf handlers
//===============================================================================

Battle.AbilityEffects.OnHPDroppedBelowHalf.add(:EMERGENCYEXIT,
	block: (ability, battler, move_user, battle) => {
		if (battler.effects.SkyDrop >= 0 ||
									battler.inTwoTurnAttack("TwoTurnAttackInvulnerableInSkyTargetCannotAct")) next false;   // Sky Drop
		// In wild battles
		if (battle.wildBattle()) {
			if (battler.opposes() && battle.SideBattlerCount(battler.index) > 1) next false;
			if (!battle.CanRun(battler.index)) next false;
			battle.ShowAbilitySplash(battler, true);
			battle.HideAbilitySplash(battler);
			SEPlay("Battle flee");
			battle.Display(_INTL("{1} fled from battle!", battler.ToString()));
			battle.decision = Battle.Outcome.FLEE;
			next true;
		}
		// In trainer battles
		if (battle.AllFainted(battler.idxOpposingSide)) next false;
		if (!battle.CanSwitchOut(battler.index)) next false;   // Battler can't switch out
		if (!battle.CanChooseNonActive(battler.index)) next false;   // No Pokémon can switch in
		battle.ShowAbilitySplash(battler, true);
		battle.HideAbilitySplash(battler);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1}'s {2} activated!", battler.ToString(), battler.abilityName));
		}
		battle.Display(_INTL("{1} went back to {2}!",
			battler.ToString(), battle.GetOwnerName(battler.index)));
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

Battle.AbilityEffects.OnHPDroppedBelowHalf.copy(:EMERGENCYEXIT, :WIMPOUT);

//===============================================================================
// StatusCheckNonIgnorable handlers
//===============================================================================

Battle.AbilityEffects.StatusCheckNonIgnorable.add(:COMATOSE,
	block: (ability, battler, status) => {
		if (!battler.isSpecies(Speciess.KOMALA)) next false;
		if (status.null() || status == statuses.SLEEP) next true;
	}
)

//===============================================================================
// StatusImmunity handlers
//===============================================================================

Battle.AbilityEffects.StatusImmunity.add(:FLOWERVEIL,
	block: (ability, battler, status) => {
		if (battler.Type == Types.GRASS) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.add(:IMMUNITY,
	block: (ability, battler, status) => {
		if (status == statuses.POISON) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.copy(:IMMUNITY, :PASTELVEIL);

Battle.AbilityEffects.StatusImmunity.add(:INSOMNIA,
	block: (ability, battler, status) => {
		if (status == statuses.SLEEP) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.copy(:INSOMNIA, :SWEETVEIL, :VITALSPIRIT);

Battle.AbilityEffects.StatusImmunity.add(:LEAFGUARD,
	block: (ability, battler, status) => {
		if (new []{:Sun, :HarshSun}.Contains(battler.effectiveWeather)) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.add(:LIMBER,
	block: (ability, battler, status) => {
		if (status == statuses.PARALYSIS) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.add(:MAGMAARMOR,
	block: (ability, battler, status) => {
		if (status == statuses.FROZEN) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.add(:PURIFYINGSALT,
	block: (ability, battler, status) => {
		next true;
	}
)

Battle.AbilityEffects.StatusImmunity.add(:WATERVEIL,
	block: (ability, battler, status) => {
		if (status == statuses.BURN) next true;
	}
)

Battle.AbilityEffects.StatusImmunity.copy(:WATERVEIL, :THERMALEXCHANGE, :WATERBUBBLE);

//===============================================================================
// StatusImmunityNonIgnorable handlers
//===============================================================================

Battle.AbilityEffects.StatusImmunityNonIgnorable.add(:COMATOSE,
	block: (ability, battler, status) => {
		if (battler.isSpecies(Speciess.KOMALA)) next true;
	}
)

Battle.AbilityEffects.StatusImmunityNonIgnorable.add(:SHIELDSDOWN,
	block: (ability, battler, status) => {
		if (battler.isSpecies(Speciess.MINIOR) && battler.form < 7) next true;
	}
)

//===============================================================================
// StatusImmunityFromAlly handlers
//===============================================================================

Battle.AbilityEffects.StatusImmunityFromAlly.add(:FLOWERVEIL,
	block: (ability, battler, status) => {
		if (battler.Type == Types.GRASS) next true;
	}
)

Battle.AbilityEffects.StatusImmunityFromAlly.add(:PASTELVEIL,
	block: (ability, battler, status) => {
		if (status == statuses.POISON) next true;
	}
)

Battle.AbilityEffects.StatusImmunityFromAlly.add(:SWEETVEIL,
	block: (ability, battler, status) => {
		if (status == statuses.SLEEP) next true;
	}
)

//===============================================================================
// OnStatusInflicted handlers
//===============================================================================

Battle.AbilityEffects.OnStatusInflicted.add(:SYNCHRONIZE,
	block: (ability, battler, user, status) => {
		if (!user || user.index == battler.index) continue;
		switch (status) {
			case :POISON:
				if (user.CanPoisonSynchronize(battler)) {
					battler.battle.ShowAbilitySplash(battler);
					msg = null;
					if (!Battle.Scene.USE_ABILITY_SPLASH) {
						msg = _INTL("{1}'s {2} poisoned {3}!", battler.ToString(), battler.abilityName, user.ToString(true));
					}
					user.Poison(null, msg, (battler.statusCount > 0));
					battler.battle.HideAbilitySplash(battler);
				}
				break;
			case :BURN:
				if (user.CanBurnSynchronize(battler)) {
					battler.battle.ShowAbilitySplash(battler);
					msg = null;
					if (!Battle.Scene.USE_ABILITY_SPLASH) {
						msg = _INTL("{1}'s {2} burned {3}!", battler.ToString(), battler.abilityName, user.ToString(true));
					}
					user.Burn(null, msg);
					battler.battle.HideAbilitySplash(battler);
				}
				break;
			case :PARALYSIS:
				if (user.CanParalyzeSynchronize(battler)) {
					battler.battle.ShowAbilitySplash(battler);
					msg = null;
					if (!Battle.Scene.USE_ABILITY_SPLASH) {
						msg = _INTL("{1}'s {2} paralyzed {3}! It may be unable to move!",
							battler.ToString(), battler.abilityName, user.ToString(true));
					}
					user.Paralyze(null, msg);
					battler.battle.HideAbilitySplash(battler);
				}
				break;
		}
	}
)

//===============================================================================
// StatusCure handlers
//===============================================================================

Battle.AbilityEffects.StatusCure.add(:IMMUNITY,
	block: (ability, battler) => {
		if (battler.status != statuses.POISON) continue;
		battler.battle.ShowAbilitySplash(battler);
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battler.battle.Display(_INTL("{1}'s {2} cured its poisoning!", battler.ToString(), battler.abilityName));
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.copy(:IMMUNITY, :PASTELVEIL);

Battle.AbilityEffects.StatusCure.add(:INSOMNIA,
	block: (ability, battler) => {
		if (battler.status != statuses.SLEEP) continue;
		battler.battle.ShowAbilitySplash(battler);
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battler.battle.Display(_INTL("{1}'s {2} woke it up!", battler.ToString(), battler.abilityName));
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.copy(:INSOMNIA, :VITALSPIRIT);

Battle.AbilityEffects.StatusCure.add(:LIMBER,
	block: (ability, battler) => {
		if (battler.status != statuses.PARALYSIS) continue;
		battler.battle.ShowAbilitySplash(battler);
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battler.battle.Display(_INTL("{1}'s {2} cured its paralysis!", battler.ToString(), battler.abilityName));
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.add(:MAGMAARMOR,
	block: (ability, battler) => {
		if (battler.status != statuses.FROZEN) continue;
		battler.battle.ShowAbilitySplash(battler);
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battler.battle.Display(_INTL("{1}'s {2} defrosted it!", battler.ToString(), battler.abilityName));
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.add(:OBLIVIOUS,
	block: (ability, battler) => {
		if (battler.effects.Attract < 0 &&
						(battler.effects.Taunt == 0 || Settings.MECHANICS_GENERATION <= 5)) continue;
		battler.battle.ShowAbilitySplash(battler);
		if (battler.effects.Attract >= 0) {
			battler.CureAttract;
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battler.battle.Display(_INTL("{1} got over its infatuation.", battler.ToString()));
			} else {
				battler.battle.Display(_INTL("{1}'s {2} cured its infatuation status!",
					battler.ToString(), battler.abilityName));
			}
		}
		if (battler.effects.Taunt > 0 && Settings.MECHANICS_GENERATION >= 6) {
			battler.effects.Taunt = 0;
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battler.battle.Display(_INTL("{1}'s Taunt wore off!", battler.ToString()));
			} else {
				battler.battle.Display(_INTL("{1}'s {2} made its taunt wear off!",
					battler.ToString(), battler.abilityName));
			}
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.add(:OWNTEMPO,
	block: (ability, battler) => {
		if (battler.effects.Confusion == 0) continue;
		battler.battle.ShowAbilitySplash(battler);
		battler.CureConfusion;
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			battler.battle.Display(_INTL("{1} snapped out of its confusion.", battler.ToString()));
		} else {
			battler.battle.Display(_INTL("{1}'s {2} snapped it out of its confusion!",
				battler.ToString(), battler.abilityName));
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.add(:WATERVEIL,
	block: (ability, battler) => {
		if (battler.status != statuses.BURN) continue;
		battler.battle.ShowAbilitySplash(battler);
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battler.battle.Display(_INTL("{1}'s {2} healed its burn!", battler.ToString(), battler.abilityName));
		}
		battler.battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.StatusCure.copy(:WATERVEIL, :THERMALEXCHANGE, :WATERBUBBLE);

//===============================================================================
// StatLossImmunity handlers
//===============================================================================

Battle.AbilityEffects.StatLossImmunity.add(:BIGPECKS,
	block: (ability, battler, stat, battle, showMessages) => {
		if (stat != :DEFENSE) next false;
		if (showMessages) {
			battle.ShowAbilitySplash(battler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s {2} cannot be lowered!", battler.ToString(), GameData.Stat.get(stat).name));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents {3} loss!", battler.ToString(),
					battler.abilityName, GameData.Stat.get(stat).name));
			}
			battle.HideAbilitySplash(battler);
		}
		next true;
	}
)

Battle.AbilityEffects.StatLossImmunity.add(:CLEARBODY,
	block: (ability, battler, stat, battle, showMessages) => {
		if (showMessages) {
			battle.ShowAbilitySplash(battler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s stats cannot be lowered!", battler.ToString()));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents stat loss!", battler.ToString(), battler.abilityName));
			}
			battle.HideAbilitySplash(battler);
		}
		next true;
	}
)

Battle.AbilityEffects.StatLossImmunity.copy(:CLEARBODY, :WHITESMOKE);

Battle.AbilityEffects.StatLossImmunity.add(:FLOWERVEIL,
	block: (ability, battler, stat, battle, showMessages) => {
		if (!battler.Type == Types.GRASS) next false;
		if (showMessages) {
			battle.ShowAbilitySplash(battler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s stats cannot be lowered!", battler.ToString()));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents stat loss!", battler.ToString(), battler.abilityName));
			}
			battle.HideAbilitySplash(battler);
		}
		next true;
	}
)

Battle.AbilityEffects.StatLossImmunity.add(:HYPERCUTTER,
	block: (ability, battler, stat, battle, showMessages) => {
		if (stat != :ATTACK) next false;
		if (showMessages) {
			battle.ShowAbilitySplash(battler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s {2} cannot be lowered!", battler.ToString(), GameData.Stat.get(stat).name));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents {3} loss!", battler.ToString(),
					battler.abilityName, GameData.Stat.get(stat).name));
			}
			battle.HideAbilitySplash(battler);
		}
		next true;
	}
)

Battle.AbilityEffects.StatLossImmunity.add(:KEENEYE,
	block: (ability, battler, stat, battle, showMessages) => {
		if (stat != :ACCURACY) next false;
		if (showMessages) {
			battle.ShowAbilitySplash(battler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s {2} cannot be lowered!", battler.ToString(), GameData.Stat.get(stat).name));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents {3} loss!", battler.ToString(),
					battler.abilityName, GameData.Stat.get(stat).name));
			}
			battle.HideAbilitySplash(battler);
		}
		next true;
	}
)

Battle.AbilityEffects.StatLossImmunity.copy(:KEENEYE, :MINDSEYE);

//===============================================================================
// StatLossImmunityNonIgnorable handlers
//===============================================================================

Battle.AbilityEffects.StatLossImmunityNonIgnorable.add(:FULLMETALBODY,
	block: (ability, battler, stat, battle, showMessages) => {
		if (showMessages) {
			battle.ShowAbilitySplash(battler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s stats cannot be lowered!", battler.ToString()));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents stat loss!", battler.ToString(), battler.abilityName));
			}
			battle.HideAbilitySplash(battler);
		}
		next true;
	}
)

//===============================================================================
// StatLossImmunityFromAlly handlers
//===============================================================================

Battle.AbilityEffects.StatLossImmunityFromAlly.add(:FLOWERVEIL,
	block: (ability, bearer, battler, stat, battle, showMessages) => {
		if (!battler.Type == Types.GRASS) next false;
		if (showMessages) {
			battle.ShowAbilitySplash(bearer);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s stats cannot be lowered!", battler.ToString()));
			} else {
				battle.Display(_INTL("{1}'s {2} prevents {3}'s stat loss!",
					bearer.ToString(), bearer.abilityName, battler.ToString(true)));
			}
			battle.HideAbilitySplash(bearer);
		}
		next true;
	}
)

//===============================================================================
// OnStatGain handlers
//===============================================================================

// There aren't any!

//===============================================================================
// OnStatLoss handlers
//===============================================================================

Battle.AbilityEffects.OnStatLoss.add(:COMPETITIVE,
	block: (ability, battler, stat, user) => {
		if (user && !user.opposes(battler)) continue;
		battler.RaiseStatStageByAbility(:SPECIAL_ATTACK, 2, battler);
	}
)

Battle.AbilityEffects.OnStatLoss.add(:DEFIANT,
	block: (ability, battler, stat, user) => {
		if (user && !user.opposes(battler)) continue;
		battler.RaiseStatStageByAbility(:ATTACK, 2, battler);
	}
)

//===============================================================================
// PriorityChange handlers
//===============================================================================

Battle.AbilityEffects.PriorityChange.add(:GALEWINGS,
	block: (ability, battler, move, pri) => {
		if ((Settings.MECHANICS_GENERATION <= 6 || battler.hp == battler.totalhp) &&
										move.type == types.FLYING) next pri + 1;
	}
)

Battle.AbilityEffects.PriorityChange.add(:PRANKSTER,
	block: (ability, battler, move, pri) => {
		if (move.statusMove()) {
			battler.effects.Prankster = true;
			next pri + 1;
		}
	}
)

Battle.AbilityEffects.PriorityChange.add(:TRIAGE,
	block: (ability, battler, move, pri) => {
		if (move.healingMove()) next pri + 3;
	}
)

//===============================================================================
// PriorityBracketChange handlers
//===============================================================================

Battle.AbilityEffects.PriorityBracketChange.add(:QUICKDRAW,
	block: (ability, battler, battle) => {
		if (battle.Random(100) < 30) next 1;
	}
)

Battle.AbilityEffects.PriorityBracketChange.add(:STALL,
	block: (ability, battler, battle) => {
		next -1;
	}
)

//===============================================================================
// PriorityBracketUse handlers
//===============================================================================

Battle.AbilityEffects.PriorityBracketUse.add(:QUICKDRAW,
	block: (ability, battler, battle) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} made {2} move faster!", battler.abilityName, battler.ToString(true)));
		battle.HideAbilitySplash(battler);
	}
)

//===============================================================================
// OnFlinch handlers
//===============================================================================

Battle.AbilityEffects.OnFlinch.add(:STEADFAST,
	block: (ability, battler, battle) => {
		battler.RaiseStatStageByAbility(:SPEED, 1, battler);
	}
)

//===============================================================================
// MoveBlocking handlers
//===============================================================================

Battle.AbilityEffects.MoveBlocking.add(:DAZZLING,
	block: (ability, bearer, user, targets, move, battle) => {
		if (battle.choices[user.index][4] <= 0) next false;
		if (!bearer.opposes(user)) next false;
		ret = false;
		targets.each(b => { if (b.opposes(user)) ret = true; });
		next ret;
	}
)

Battle.AbilityEffects.MoveBlocking.copy(:DAZZLING, :QUEENLYMAJESTY);

//===============================================================================
// MoveImmunity handlers
//===============================================================================

Battle.AbilityEffects.MoveImmunity.add(:BULLETPROOF,
	block: (ability, user, target, move, type, battle, show_message) => {
		if (!move.bombMove()) next false;
		if (show_message) {
			battle.ShowAbilitySplash(target);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			} else {
				battle.Display(_INTL("{1}'s {2} made {3} ineffective!",
					target.ToString(), target.abilityName, move.name));
			}
			battle.HideAbilitySplash(target);
		}
		next true;
	}
)

Battle.AbilityEffects.MoveImmunity.add(:EARTHEATER,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityHealingAbility(user, move, type, :GROUND, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:FLASHFIRE,
	block: (ability, user, target, move, type, battle, show_message) => {
		if (user.index == target.index) next false;
		if (type != types.FIRE) next false;
		if (show_message) {
			battle.ShowAbilitySplash(target);
			if (!target.effects.FlashFire) {
				target.effects.FlashFire = true;
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					battle.Display(_INTL("The power of {1}'s Fire-type moves rose!", target.ToString(true)));
				} else {
					battle.Display(_INTL("The power of {1}'s Fire-type moves rose because of its {2}!",
						target.ToString(true), target.abilityName));
				}
			} else if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			} else {
				battle.Display(_INTL("{1}'s {2} made {3} ineffective!",
															target.ToString(), target.abilityName, move.name));
			}
			battle.HideAbilitySplash(target);
		}
		next true;
	}
)

Battle.AbilityEffects.MoveImmunity.add(:LIGHTNINGROD,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityStatRaisingAbility(user, move, type,
			:ELECTRIC, :SPECIAL_ATTACK, 1, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:MOTORDRIVE,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityStatRaisingAbility(user, move, type,
			:ELECTRIC, :SPEED, 1, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:SAPSIPPER,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityStatRaisingAbility(user, move, type,
			:GRASS, :ATTACK, 1, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:SOUNDPROOF,
	block: (ability, user, target, move, type, battle, show_message) => {
		if (!move.soundMove()) next false;
		if (Settings.MECHANICS_GENERATION >= 8 && user.index == target.index) next false;
		if (show_message) {
			battle.ShowAbilitySplash(target);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			} else {
				battle.Display(_INTL("{1}'s {2} blocks {3}!", target.ToString(), target.abilityName, move.name));
			}
			battle.HideAbilitySplash(target);
		}
		next true;
	}
)

Battle.AbilityEffects.MoveImmunity.add(:STORMDRAIN,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityStatRaisingAbility(user, move, type,
			:WATER, :SPECIAL_ATTACK, 1, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:TELEPATHY,
	block: (ability, user, target, move, type, battle, show_message) => {
		if (move.statusMove()) next false;
		if (user.index == target.index || target.opposes(user)) next false;
		if (show_message) {
			battle.ShowAbilitySplash(target);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} avoids attacks by its ally Pokémon!", target.ToString(true)));
			} else {
				battle.Display(_INTL("{1} avoids attacks by its ally Pokémon with {2}!",
					target.ToString(), target.abilityName));
			}
			battle.HideAbilitySplash(target);
		}
		next true;
	}
)

Battle.AbilityEffects.MoveImmunity.add(:VOLTABSORB,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityHealingAbility(user, move, type, :ELECTRIC, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:WATERABSORB,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityHealingAbility(user, move, type, :WATER, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.copy(:WATERABSORB, :DRYSKIN);

Battle.AbilityEffects.MoveImmunity.add(:WELLBAKEDBODY,
	block: (ability, user, target, move, type, battle, show_message) => {
		next target.MoveImmunityStatRaisingAbility(user, move, type,
			:FIRE, :DEFENSE, 2, show_message);
	}
)

Battle.AbilityEffects.MoveImmunity.add(:WINDRIDER,
	block: (ability, user, target, move, type, battle, show_message) => {
		if (user.index == target.index) next false;
		if (!move.windMove()) next false;
		if (show_message) {
			battle.ShowAbilitySplash(target);
			if (target.CanRaiseStatStage(:ATTACK, target)) {
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					target.RaiseStatStage(:ATTACK, 1, target);
				} else {
					target.RaiseStatStageByCause(:ATTACK, 1, target, target.abilityName);
				}
			} else if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			} else {
				battle.Display(_INTL("{1}'s {2} made {3} ineffective!", target.ToString(), target.abilityName, move.name));
			}
			battle.HideAbilitySplash(target);
		}
		next true;
	}
)

Battle.AbilityEffects.MoveImmunity.add(:WONDERGUARD,
	block: (ability, user, target, move, type, battle, show_message) => {
		if (move.statusMove()) next false;
		if (!type || Effectiveness.super_effective(target.damageState.typeMod)) next false;
		if (show_message) {
			battle.ShowAbilitySplash(target);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("It doesn't affect {1}...", target.ToString(true)));
			} else {
				battle.Display(_INTL("{1} avoided damage with {2}!", target.ToString(), target.abilityName));
			}
			battle.HideAbilitySplash(target);
		}
		next true;
	}
)

//===============================================================================
// ModifyMoveBaseType handlers
//===============================================================================

Battle.AbilityEffects.ModifyMoveBaseType.Add(Types.AERILATE,
	block: (ability, user, move, type) => {
		if (type != types.NORMAL || !GameData.Types.exists(Types.FLYING)) continue;
		move.powerBoost = true;
		next :FLYING;
	}
)

Battle.AbilityEffects.ModifyMoveBaseType.Add(Types.GALVANIZE,
	block: (ability, user, move, type) => {
		if (type != types.NORMAL || !GameData.Types.exists(Types.ELECTRIC)) continue;
		move.powerBoost = true;
		next :ELECTRIC;
	}
)

Battle.AbilityEffects.ModifyMoveBaseType.Add(Types.LIQUIDVOICE,
	block: (ability, user, move, type) => {
		if (GameData.Types.exists(Types.WATER) && move.soundMove()) next :WATER;
	}
)

Battle.AbilityEffects.ModifyMoveBaseType.Add(Types.NORMALIZE,
	block: (ability, user, move, type) => {
		if (!GameData.Types.exists(Types.NORMAL)) continue;
		if (Settings.MECHANICS_GENERATION >= 7) move.powerBoost = true;
		next :NORMAL;
	}
)

Battle.AbilityEffects.ModifyMoveBaseType.Add(Types.PIXILATE,
	block: (ability, user, move, type) => {
		if (type != types.NORMAL || !GameData.Types.exists(Types.FAIRY)) continue;
		move.powerBoost = true;
		next :FAIRY;
	}
)

Battle.AbilityEffects.ModifyMoveBaseType.Add(Types.REFRIGERATE,
	block: (ability, user, move, type) => {
		if (type != types.NORMAL || !GameData.Types.exists(Types.ICE)) continue;
		move.powerBoost = true;
		next :ICE;
	}
)

//===============================================================================
// AccuracyCalcFromUser handlers
//===============================================================================

Battle.AbilityEffects.AccuracyCalcFromUser.add(:COMPOUNDEYES,
	block: (ability, mods, user, target, move, type) => {
		mods.accuracy_multiplier *= 1.3;
	}
)

Battle.AbilityEffects.AccuracyCalcFromUser.add(:HUSTLE,
	block: (ability, mods, user, target, move, type) => {
		if (move.physicalMove()) mods.accuracy_multiplier *= 0.8;
	}
)

Battle.AbilityEffects.AccuracyCalcFromUser.add(:KEENEYE,
	block: (ability, mods, user, target, move, type) => {
		if (mods.evasion_stage > 0 && Settings.MECHANICS_GENERATION >= 6) mods.evasion_stage = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromUser.add(:NOGUARD,
	block: (ability, mods, user, target, move, type) => {
		mods.base_accuracy = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromUser.add(:UNAWARE,
	block: (ability, mods, user, target, move, type) => {
		if (move.damagingMove()) mods.evasion_stage = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromUser.add(:VICTORYSTAR,
	block: (ability, mods, user, target, move, type) => {
		mods.accuracy_multiplier *= 1.1;
	}
)

//===============================================================================
// AccuracyCalcFromAlly handlers
//===============================================================================

Battle.AbilityEffects.AccuracyCalcFromAlly.add(:VICTORYSTAR,
	block: (ability, mods, user, target, move, type) => {
		mods.accuracy_multiplier *= 1.1;
	}
)

//===============================================================================
// AccuracyCalcFromTarget handlers
//===============================================================================

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:LIGHTNINGROD,
	block: (ability, mods, user, target, move, type) => {
		if (type == types.ELECTRIC) mods.base_accuracy = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:NOGUARD,
	block: (ability, mods, user, target, move, type) => {
		mods.base_accuracy = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:SANDVEIL,
	block: (ability, mods, user, target, move, type) => {
		if (target.effectiveWeather == Weathers.Sandstorm) mods.evasion_multiplier *= 1.25;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:SNOWCLOAK,
	block: (ability, mods, user, target, move, type) => {
		if (new []{:Hail, :Snowstorm}.Contains(target.effectiveWeather)) mods.evasion_multiplier *= 1.25;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:STORMDRAIN,
	block: (ability, mods, user, target, move, type) => {
		if (type == types.WATER) mods.base_accuracy = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:TANGLEDFEET,
	block: (ability, mods, user, target, move, type) => {
		if (target.effects.Confusion > 0) mods.accuracy_multiplier /= 2;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:UNAWARE,
	block: (ability, mods, user, target, move, type) => {
		if (move.damagingMove()) mods.accuracy_stage = 0;
	}
)

Battle.AbilityEffects.AccuracyCalcFromTarget.add(:WONDERSKIN,
	block: (ability, mods, user, target, move, type) => {
		if (move.statusMove() && user.opposes(target) && mods.base_accuracy > 50) {
			mods.base_accuracy = 50;
		}
	}
)

//===============================================================================
// DamageCalcFromUser handlers
//===============================================================================

Battle.AbilityEffects.DamageCalcFromUser.add(:AERILATE,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.powerBoost) mults.power_multiplier *= 1.2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.copy(:AERILATE, :GALVANIZE, :NORMALIZE, :PIXILATE, :REFRIGERATE);

Battle.AbilityEffects.DamageCalcFromUser.add(:ANALYTIC,
	block: (ability, user, target, move, mults, power, type) => {
		// NOTE: In the official games, if another battler faints earlier in the
		//       round but it would have moved after the user, then Analytic does not
		//       power up the move. However, this makes the determination so much
		//       more complicated (involving Priority and counting or not counting
		//       speed/priority modifiers depending on which Generation's mechanics
		//       are being used), so I'm choosing to ignore it. The effect is thus:
		//       "power up the move if all other battlers on the field right now have
		//       already moved".
		if (move.MoveFailedLastInRound(user, false)) {
			mults.power_multiplier *= 1.3;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:BLAZE,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.hp <= user.totalhp / 3 && type == types.FIRE) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:DEFEATIST,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.hp <= user.totalhp / 2) mults.attack_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:DRAGONSMAW,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.DRAGON) mults.attack_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:FLAREBOOST,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.burned() && move.specialMove()) mults.power_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:FLASHFIRE,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.effects.FlashFire && type == types.FIRE) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:FLOWERGIFT,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.physicalMove() && new []{:Sun, :HarshSun}.Contains(user.effectiveWeather)) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:GORILLATACTICS,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.physicalMove()) mults.attack_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:GUTS,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.HasAnyStatus() && move.physicalMove()) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:HUGEPOWER,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.physicalMove()) mults.attack_multiplier *= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.copy(:HUGEPOWER, :PUREPOWER);

Battle.AbilityEffects.DamageCalcFromUser.add(:HUSTLE,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.physicalMove()) mults.attack_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:IRONFIST,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.punchingMove()) mults.power_multiplier *= 1.2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:MEGALAUNCHER,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.pulseMove()) mults.power_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:MINUS,
	block: (ability, user, target, move, mults, power, type) => {
		if (!move.specialMove()) continue;
		if (user.allAllies.any(b => b.hasActiveAbility(new {:MINUS, :PLUS}))) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.copy(:MINUS, :PLUS);

Battle.AbilityEffects.DamageCalcFromUser.add(:NEUROFORCE,
	block: (ability, user, target, move, mults, power, type) => {
		if (Effectiveness.super_effective(target.damageState.typeMod)) {
			mults.final_damage_multiplier *= 1.25;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:OVERGROW,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.hp <= user.totalhp / 3 && type == types.GRASS) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:PUNKROCK,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.soundMove()) mults.attack_multiplier *= 1.3;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:RECKLESS,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.recoilMove()) mults.power_multiplier *= 1.2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:RIVALRY,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.gender != 2 && target.gender != 2) {
			if (user.gender == target.gender) {
				mults.power_multiplier *= 1.25;
			} else {
				mults.power_multiplier *= 0.75;
			}
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:ROCKYPAYLOAD,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.ROCK) mults.attack_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SANDFORCE,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.effectiveWeather == Weathers.Sandstorm &&
			new []{:ROCK, :GROUND, :STEEL}.Contains(type)) {
			mults.power_multiplier *= 1.3;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SHARPNESS,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.slicingMove()) mults.power_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SHEERFORCE,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.addlEffect > 0) mults.power_multiplier *= 1.3;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SLOWSTART,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.effects.SlowStart > 0 && move.physicalMove()) mults.attack_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SNIPER,
	block: (ability, user, target, move, mults, power, type) => {
		if (target.damageState.critical) mults.final_damage_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SOLARPOWER,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.specialMove() && new []{:Sun, :HarshSun}.Contains(user.effectiveWeather)) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:STAKEOUT,
	block: (ability, user, target, move, mults, power, type) => {
		if (target.battle.choices[target.index].Action == :SwitchOut) mults.attack_multiplier *= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:STEELWORKER,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.STEEL) mults.attack_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:STEELYSPIRIT,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.STEEL) mults.final_damage_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:STRONGJAW,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.bitingMove()) mults.power_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:SWARM,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.hp <= user.totalhp / 3 && type == types.BUG) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:TECHNICIAN,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.index != target.index && move && move.function_code != "Struggle" &&
			power * mults.power_multiplier <= 60) {
			mults.power_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:TINTEDLENS,
	block: (ability, user, target, move, mults, power, type) => {
		if (Effectiveness.resistant(target.damageState.typeMod)) mults.final_damage_multiplier *= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:TORRENT,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.hp <= user.totalhp / 3 && type == types.WATER) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:TOUGHCLAWS,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.ContactMove(user)) mults.power_multiplier *= 4 / 3.0;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:TOXICBOOST,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.poisoned() && move.physicalMove()) {
			mults.power_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:TRANSISTOR,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.ELECTRIC) mults.attack_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromUser.add(:WATERBUBBLE,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.WATER) mults.attack_multiplier *= 2;
	}
)

//===============================================================================
// DamageCalcFromAlly handlers
//===============================================================================

Battle.AbilityEffects.DamageCalcFromAlly.add(:BATTERY,
	block: (ability, user, target, move, mults, power, type) => {
		if (!move.specialMove()) continue;
		mults.final_damage_multiplier *= 1.3;
	}
)

Battle.AbilityEffects.DamageCalcFromAlly.add(:FLOWERGIFT,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.physicalMove() && new []{:Sun, :HarshSun}.Contains(user.effectiveWeather)) {
			mults.attack_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromAlly.add(:POWERSPOT,
	block: (ability, user, target, move, mults, power, type) => {
		mults.final_damage_multiplier *= 1.3;
	}
)

Battle.AbilityEffects.DamageCalcFromAlly.add(:STEELYSPIRIT,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.STEEL) mults.final_damage_multiplier *= 1.5;
	}
)

//===============================================================================
// DamageCalcFromTarget handlers
//===============================================================================

Battle.AbilityEffects.DamageCalcFromTarget.add(:DRYSKIN,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.FIRE) mults.power_multiplier *= 1.25;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:FILTER,
	block: (ability, user, target, move, mults, power, type) => {
		if (Effectiveness.super_effective(target.damageState.typeMod)) {
			mults.final_damage_multiplier *= 0.75;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.copy(:FILTER, :SOLIDROCK);

Battle.AbilityEffects.DamageCalcFromTarget.add(:FLOWERGIFT,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.specialMove() && new []{:Sun, :HarshSun}.Contains(target.effectiveWeather)) {
			mults.defense_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:FLUFFY,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.calcType == Types.FIRE) mults.final_damage_multiplier *= 2;
		if (move.ContactMove(user)) mults.final_damage_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:FURCOAT,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.physicalMove() ||
																			move.function_code == "UseTargetDefenseInsteadOfTargetSpDef") mults.defense_multiplier *= 2;   // Psyshock
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:GRASSPELT,
	block: (ability, user, target, move, mults, power, type) => {
		if (user.battle.field.terrain == :Grassy) mults.defense_multiplier *= 1.5;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:HEATPROOF,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.FIRE) mults.power_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:ICESCALES,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.specialMove()) mults.final_damage_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:MARVELSCALE,
	block: (ability, user, target, move, mults, power, type) => {
		if (target.HasAnyStatus() && move.physicalMove()) {
			mults.defense_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:MULTISCALE,
	block: (ability, user, target, move, mults, power, type) => {
		if (target.hp == target.totalhp) mults.final_damage_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:PUNKROCK,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.soundMove()) mults.final_damage_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:PURIFYINGSALT,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.calcType == Types.GHOST) mults.attack_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:THICKFAT,
	block: (ability, user, target, move, mults, power, type) => {
		if (new []{:FIRE, :ICE}.Contains(type)) mults.power_multiplier /= 2;
	}
)

Battle.AbilityEffects.DamageCalcFromTarget.add(:WATERBUBBLE,
	block: (ability, user, target, move, mults, power, type) => {
		if (type == types.FIRE) mults.final_damage_multiplier /= 2;
	}
)

//===============================================================================
// DamageCalcFromTargetNonIgnorable handlers
//===============================================================================

Battle.AbilityEffects.DamageCalcFromTargetNonIgnorable.add(:PRISMARMOR,
	block: (ability, user, target, move, mults, power, type) => {
		if (Effectiveness.super_effective(target.damageState.typeMod)) {
			mults.final_damage_multiplier *= 0.75;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromTargetNonIgnorable.add(:SHADOWSHIELD,
	block: (ability, user, target, move, mults, power, type) => {
		if (target.hp == target.totalhp) mults.final_damage_multiplier /= 2;
	}
)

//===============================================================================
// DamageCalcFromTargetAlly handlers
//===============================================================================

Battle.AbilityEffects.DamageCalcFromTargetAlly.add(:FLOWERGIFT,
	block: (ability, user, target, move, mults, power, type) => {
		if (move.specialMove() && new []{:Sun, :HarshSun}.Contains(target.effectiveWeather)) {
			mults.defense_multiplier *= 1.5;
		}
	}
)

Battle.AbilityEffects.DamageCalcFromTargetAlly.add(:FRIENDGUARD,
	block: (ability, user, target, move, mults, power, type) => {
		mults.final_damage_multiplier *= 0.75;
	}
)

//===============================================================================
// CriticalCalcFromUser handlers
//===============================================================================

Battle.AbilityEffects.CriticalCalcFromUser.add(:MERCILESS,
	block: (ability, user, target, move, c) => {
		if (target.poisoned()) next 99;
	}
)

Battle.AbilityEffects.CriticalCalcFromUser.add(:SUPERLUCK,
	block: (ability, user, target, move, c) => {
		next c + 1;
	}
)

//===============================================================================
// CriticalCalcFromTarget handlers
//===============================================================================

Battle.AbilityEffects.CriticalCalcFromTarget.add(:BATTLEARMOR,
	block: (ability, user, target, move, c) => {
		next -1;
	}
)

Battle.AbilityEffects.CriticalCalcFromTarget.copy(:BATTLEARMOR, :SHELLARMOR);

//===============================================================================
// OnBeingHit handlers
//===============================================================================

Battle.AbilityEffects.OnBeingHit.add(:AFTERMATH,
	block: (ability, user, target, move, battle) => {
		if (!target.fainted()) continue;
		if (!move.ContactMove(user)) continue;
		battle.ShowAbilitySplash(target);
		dampBattler = battle.CheckGlobalAbility(:DAMP, true);
		if (dampBattler) {
			battle.ShowAbilitySplash(dampBattler);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} cannot use {2}!", target.ToString(), target.abilityName));
			} else {
				battle.Display(_INTL("{1} cannot use {2} because of {3}'s {4}!",
					target.ToString(), target.abilityName, dampBattler.ToString(true), dampBattler.abilityName));
			}
			battle.HideAbilitySplash(dampBattler);
			battle.HideAbilitySplash(target);
			continue;
		}
		if (user.takesIndirectDamage(Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			battle.scene.DamageAnimation(user);
			user.ReduceHP(user.totalhp / 4, false);
			battle.Display(_INTL("{1} was caught in the aftermath!", user.ToString()));
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:ANGERPOINT,
	block: (ability, user, target, move, battle) => {
		if (!target.damageState.critical) continue;
		if (!target.CanRaiseStatStage(:ATTACK, target)) continue;
		battle.ShowAbilitySplash(target);
		target.stages[:ATTACK] = Battle.Battler.STAT_STAGE_MAXIMUM;
		target.statsRaisedThisRound = true;
		battle.CommonAnimation("StatUp", target);
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1} maxed its {2}!", target.ToString(), GameData.Stat.get(:ATTACK).name));
		} else {
			battle.Display(_INTL("{1}'s {2} maxed its {3}!",
				target.ToString(), target.abilityName, GameData.Stat.get(:ATTACK).name));
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:COTTONDOWN,
	block: (ability, user, target, move, battle) => {
		if (battle.allBattlers.none(b => b.index != target.index && b.CanLowerStatStage(:SPEED, target))) continue;
		battle.ShowAbilitySplash(target);
		foreach (var b in battle.allBattlers) { //'battle.allBattlers.each' do => |b|
			if (b.index != target.index) b.LowerStatStageByAbility(:SPEED, 1, target, false);
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:CURSEDBODY,
	block: (ability, user, target, move, battle) => {
		if (user.fainted()) continue;
		if (user.effects.Disable > 0) continue;
		regularMove = null;
		foreach (var m in user.Moves) { //user.eachMove do => |m|
			if (m.id != user.lastRegularMoveUsed) continue;
			regularMove = m;
			break;
		}
		if (!regularMove || (regularMove.pp == 0 && regularMove.total_pp > 0)) continue;
		if (battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(target);
		if (!move.MoveFailedAromaVeil(target, user, Battle.Scene.USE_ABILITY_SPLASH)) {
			user.effects.Disable     = 3;
			user.effects.DisableMove = regularMove.id;
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s {2} was disabled!", user.ToString(), regularMove.name));
			} else {
				battle.Display(_INTL("{1}'s {2} was disabled by {3}'s {4}!",
					user.ToString(), regularMove.name, target.ToString(true), target.abilityName));
			}
			battle.HideAbilitySplash(target);
			user.ItemStatusCureCheck;
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:CUTECHARM,
	block: (ability, user, target, move, battle) => {
		if (target.fainted()) continue;
		if (!move.ContactMove(user)) continue;
		if (battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(target);
		if (user.CanAttract(target, Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			msg = null;
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				msg = _INTL("{1}'s {2} made {3} fall in love!", target.ToString(),
					target.abilityName, user.ToString(true));
			}
			user.Attract(target, msg);
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:EFFECTSPORE,
	block: (ability, user, target, move, battle) => {
		// NOTE: This ability has a 30% chance of triggering, not a 30% chance of
		//       inflicting a status condition. It can try (and fail) to inflict a
		//       status condition that the user is immune to.
		if (!move.ContactMove(user)) continue;
		if (battle.Random(100) >= 30) continue;
		r = battle.Random(3);
		if (r == 0 && user.asleep()) continue;
		if (r == 1 && user.poisoned()) continue;
		if (r == 2 && user.paralyzed()) continue;
		battle.ShowAbilitySplash(target);
		if (user.affectedByPowder(Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			switch (r) {
				case 0:
					if (user.CanSleep(target, Battle.Scene.USE_ABILITY_SPLASH)) {
						msg = null;
						if (!Battle.Scene.USE_ABILITY_SPLASH) {
							msg = _INTL("{1}'s {2} made {3} fall asleep!", target.ToString(),
								target.abilityName, user.ToString(true));
						}
						user.Sleep(target, msg);
					}
					break;
				case 1:
					if (user.CanPoison(target, Battle.Scene.USE_ABILITY_SPLASH)) {
						msg = null;
						if (!Battle.Scene.USE_ABILITY_SPLASH) {
							msg = _INTL("{1}'s {2} poisoned {3}!", target.ToString(),
								target.abilityName, user.ToString(true));
						}
						user.Poison(target, msg);
					}
					break;
				case 2:
					if (user.CanParalyze(target, Battle.Scene.USE_ABILITY_SPLASH)) {
						msg = null;
						if (!Battle.Scene.USE_ABILITY_SPLASH) {
							msg = _INTL("{1}'s {2} paralyzed {3}! It may be unable to move!",
								target.ToString(), target.abilityName, user.ToString(true));
						}
						user.Paralyze(target, msg);
					}
					break;
			}
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:FLAMEBODY,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (user.burned() || battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(target);
		if (user.CanBurn(target, Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			msg = null;
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				msg = _INTL("{1}'s {2} burned {3}!", target.ToString(), target.abilityName, user.ToString(true));
			}
			user.Burn(target, msg);
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:GOOEY,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		user.LowerStatStageByAbility(:SPEED, 1, target, true, true);
	}
)

Battle.AbilityEffects.OnBeingHit.copy(:GOOEY, :TANGLINGHAIR);

Battle.AbilityEffects.OnBeingHit.add(:ILLUSION,
	block: (ability, user, target, move, battle) => {
		// NOTE: This intentionally doesn't show the ability splash.
		if (!target.effects.Illusion) continue;
		target.effects.Illusion = null;
		battle.scene.ChangePokemon(target, target.pokemon);
		battle.Display(_INTL("{1}'s illusion wore off!", target.ToString()));
		battle.SetSeen(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:INNARDSOUT,
	block: (ability, user, target, move, battle) => {
		if (!target.fainted() || user.dummy) continue;
		battle.ShowAbilitySplash(target);
		if (user.takesIndirectDamage(Battle.Scene.USE_ABILITY_SPLASH)) {
			battle.scene.DamageAnimation(user);
			user.ReduceHP(target.damageState.hpLost, false);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} is hurt!", user.ToString()));
			} else {
				battle.Display(_INTL("{1} is hurt by {2}'s {3}!", user.ToString(),
					target.ToString(true), target.abilityName));
			}
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:IRONBARBS,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		battle.ShowAbilitySplash(target);
		if (user.takesIndirectDamage(Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			battle.scene.DamageAnimation(user);
			user.ReduceHP(user.totalhp / 8, false);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} is hurt!", user.ToString()));
			} else {
				battle.Display(_INTL("{1} is hurt by {2}'s {3}!", user.ToString(),
					target.ToString(true), target.abilityName));
			}
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.copy(:IRONBARBS, :ROUGHSKIN);

Battle.AbilityEffects.OnBeingHit.add(:JUSTIFIED,
	block: (ability, user, target, move, battle) => {
		if (move.calcType != Types.DARK) continue;
		target.RaiseStatStageByAbility(:ATTACK, 1, target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:MUMMY,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (user.fainted()) continue;
		if (user.unstoppableAbility() || user.ability == ability) continue;
		oldAbil = null;
		if (user.opposes(target)) battle.ShowAbilitySplash(target);
		if (user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			oldAbil = user.ability;
			if (user.opposes(target)) battle.ShowAbilitySplash(user, true, false);
			user.ability = ability;
			if (user.opposes(target)) battle.ReplaceAbilitySplash(user);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s Ability became {2}!", user.ToString(), user.abilityName));
			} else {
				battle.Display(_INTL("{1}'s Ability became {2} because of {3}!",
					user.ToString(), user.abilityName, target.ToString(true)));
			}
			if (user.opposes(target)) battle.HideAbilitySplash(user);
		}
		if (user.opposes(target)) battle.HideAbilitySplash(target);
		user.OnLosingAbility(oldAbil);
		user.TriggerAbilityOnGainingIt;
	}
)

Battle.AbilityEffects.OnBeingHit.copy(:MUMMY, :LINGERINGAROMA);

Battle.AbilityEffects.OnBeingHit.add(:PERISHBODY,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (user.fainted()) continue;
		if (user.effects.PerishSong > 0 || target.effects.PerishSong > 0) continue;
		battle.ShowAbilitySplash(target);
		if (user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			user.effects.PerishSong = 4;
			user.effects.PerishSongUser = target.index;
			target.effects.PerishSong = 4;
			target.effects.PerishSongUser = target.index;
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("Both Pokémon will faint in three turns!"));
			} else {
				battle.Display(_INTL("Both Pokémon will faint in three turns because of {1}'s {2}!",
					target.ToString(true), target.abilityName));
			}
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:POISONPOINT,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (user.poisoned() || battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(target);
		if (user.CanPoison(target, Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			msg = null;
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				msg = _INTL("{1}'s {2} poisoned {3}!", target.ToString(), target.abilityName, user.ToString(true));
			}
			user.Poison(target, msg);
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:RATTLED,
	block: (ability, user, target, move, battle) => {
		if (!new []{:BUG, :DARK, :GHOST}.Contains(move.calcType)) continue;
		target.RaiseStatStageByAbility(:SPEED, 1, target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:SANDSPIT,
	block: (ability, user, target, move, battle) => {
		battle.StartWeatherAbility(:Sandstorm, target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:STAMINA,
	block: (ability, user, target, move, battle) => {
		target.RaiseStatStageByAbility(:DEFENSE, 1, target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:SEEDSOWER,
	block: (ability, user, target, move, battle) => {
		if (battle.field.terrain == :Grassy) continue;
		battle.ShowAbilitySplash(target);
		battle.StartTerrain(target, :Grassy);
		// NOTE: The ability splash is hidden again in def StartTerrain.
	}
)

Battle.AbilityEffects.OnBeingHit.add(:STATIC,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (user.paralyzed() || battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(target);
		if (user.CanParalyze(target, Battle.Scene.USE_ABILITY_SPLASH) &&
			user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			msg = null;
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				msg = _INTL("{1}'s {2} paralyzed {3}! It may be unable to move!",
					target.ToString(), target.abilityName, user.ToString(true));
			}
			user.Paralyze(target, msg);
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:THERMALEXCHANGE,
	block: (ability, user, target, move, battle) => {
		if (move.calcType != Types.FIRE) continue;
		target.RaiseStatStageByAbility(:ATTACK, 1, target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:TOXICDEBRIS,
	block: (ability, user, target, move, battle) => {
		if (!move.physicalMove()) continue;
		if (target.OpposingSide.effects.ToxicSpikes >= 2) continue;
		battle.ShowAbilitySplash(target);
		target.OpposingSide.effects.ToxicSpikes += 1;
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("Poison spikes were scattered all around {1}'s feet!",
														target.OpposingTeam(true)));
		} else {
			battle.Display(_INTL("{1}'s {2} scattered poison spikes all around {3}'s feet!",
														target.ToString(), target.abilityName, target.OpposingTeam(true)));
		}
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:WANDERINGSPIRIT,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (user.ungainableAbility() || new []{:RECEIVER, :WONDERGUARD}.Contains(user.ability_id)) continue;
		oldUserAbil   = null;
		oldTargetAbil = null;
		if (user.opposes(target)) battle.ShowAbilitySplash(target);
		if (user.affectedByContactEffect(Battle.Scene.USE_ABILITY_SPLASH)) {
			if (user.opposes(target)) battle.ShowAbilitySplash(user, true, false);
			oldUserAbil   = user.ability;
			oldTargetAbil = target.ability;
			user.ability   = oldTargetAbil;
			target.ability = oldUserAbil;
			if (user.opposes(target)) {
				battle.ReplaceAbilitySplash(user);
				battle.ReplaceAbilitySplash(target);
			}
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} swapped Abilities with {2}!", target.ToString(), user.ToString(true)));
			} else {
				battle.Display(_INTL("{1} swapped its {2} Ability with {3}'s {4} Ability!",
					target.ToString(), user.abilityName, user.ToString(true), target.abilityName));
			}
			if (user.opposes(target)) {
				battle.HideAbilitySplash(user);
				battle.HideAbilitySplash(target);
			}
		}
		if (user.opposes(target)) battle.HideAbilitySplash(target);
		user.OnLosingAbility(oldUserAbil);
		target.OnLosingAbility(oldTargetAbil);
		user.TriggerAbilityOnGainingIt;
		target.TriggerAbilityOnGainingIt;
	}
)

Battle.AbilityEffects.OnBeingHit.add(:WATERCOMPACTION,
	block: (ability, user, target, move, battle) => {
		if (move.calcType != Types.WATER) continue;
		target.RaiseStatStageByAbility(:DEFENSE, 2, target);
	}
)

Battle.AbilityEffects.OnBeingHit.add(:WEAKARMOR,
	block: (ability, user, target, move, battle) => {
		if (!move.physicalMove()) continue;
		if (!target.CanLowerStatStage(:DEFENSE, target) &&
						!target.CanRaiseStatStage(:SPEED, target)) continue;
		battle.ShowAbilitySplash(target);
		target.LowerStatStageByAbility(:DEFENSE, 1, target, false);
		target.RaiseStatStageByAbility(:SPEED,
			(Settings.MECHANICS_GENERATION >= 7) ? 2 : 1, target, false)
		battle.HideAbilitySplash(target);
	}
)

//===============================================================================
// OnDealingHit handlers
//===============================================================================

Battle.AbilityEffects.OnDealingHit.add(:POISONTOUCH,
	block: (ability, user, target, move, battle) => {
		if (!move.ContactMove(user)) continue;
		if (battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(user);
		if (target.hasActiveAbility(Abilitys.SHIELDDUST) && !target.being_mold_broken()) {
			battle.ShowAbilitySplash(target);
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			}
			battle.HideAbilitySplash(target);
		} else if (target.CanPoison(user, Battle.Scene.USE_ABILITY_SPLASH)) {
			msg = null;
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				msg = _INTL("{1}'s {2} poisoned {3}!", user.ToString(), user.abilityName, target.ToString(true));
			}
			target.Poison(user, msg);
		}
		battle.HideAbilitySplash(user);
	}
)

Battle.AbilityEffects.OnDealingHit.add(:TOXICCHAIN,
	block: (ability, user, target, move, battle) => {
		if (battle.Random(100) >= 30) continue;
		battle.ShowAbilitySplash(user);
		if (target.hasActiveAbility(Abilitys.SHIELDDUST) && !target.being_mold_broken()) {
			battle.ShowAbilitySplash(target);
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			}
			battle.HideAbilitySplash(target);
		} else if (target.CanPoison(user, Battle.Scene.USE_ABILITY_SPLASH)) {
			msg = null;
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				msg = _INTL("{1}'s {2} badly poisoned {3}!", user.ToString(), user.abilityName, target.ToString(true));
			}
			target.Poison(user, msg, true);
		}
		battle.HideAbilitySplash(user);
	}
)

//===============================================================================
// OnEndOfUsingMove handlers
//===============================================================================

Battle.AbilityEffects.OnEndOfUsingMove.Add(Moves.BEASTBOOST,
	block: (ability, user, targets, move, battle) => {
		if (battle.AllFainted(user.idxOpposingSide)) continue;
		numFainted = 0;
		targets.each(b => { if (b.damageState.fainted) numFainted += 1; });
		if (numFainted == 0) continue;
		userStats = user.plainStats;
		highestStatValue = 0;
		userStats.each_value(value => { if (highestStatValue < value) highestStatValue = value; });
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main_battle do => |s|
			if (userStats[s.id] < highestStatValue) continue;
			if (user.CanRaiseStatStage(s.id, user)) {
				user.RaiseStatStageByAbility(s.id, numFainted, user);
			}
			break;
		}
	}
)

Battle.AbilityEffects.OnEndOfUsingMove.Add(Moves.CHILLINGNEIGH,
	block: (ability, user, targets, move, battle) => {
		if (battle.AllFainted(user.idxOpposingSide)) continue;
		numFainted = 0;
		targets.each(b => { if (b.damageState.fainted) numFainted += 1; });
		if (numFainted == 0 || !user.CanRaiseStatStage(:ATTACK, user)) continue;
		user.ability_id = :CHILLINGNEIGH;   // So the As One abilities can just copy this
		user.RaiseStatStageByAbility(:ATTACK, 1, user);
		user.ability_id = ability;
	}
)

Battle.AbilityEffects.OnEndOfUsingMove.copy(:CHILLINGNEIGH, :ASONECHILLINGNEIGH);

Battle.AbilityEffects.OnEndOfUsingMove.Add(Moves.GRIMNEIGH,
	block: (ability, user, targets, move, battle) => {
		if (battle.AllFainted(user.idxOpposingSide)) continue;
		numFainted = 0;
		targets.each(b => { if (b.damageState.fainted) numFainted += 1; });
		if (numFainted == 0 || !user.CanRaiseStatStage(:SPECIAL_ATTACK, user)) continue;
		user.ability_id = :GRIMNEIGH;   // So the As One abilities can just copy this
		user.RaiseStatStageByAbility(:SPECIAL_ATTACK, 1, user);
		user.ability_id = ability;
	}
)

Battle.AbilityEffects.OnEndOfUsingMove.copy(:GRIMNEIGH, :ASONEGRIMNEIGH);

Battle.AbilityEffects.OnEndOfUsingMove.Add(Moves.MAGICIAN,
	block: (ability, user, targets, move, battle) => {
		if (battle.futureSight) continue;
		if (!move.DamagingMove()) continue;
		if (user.item) continue;
		if (user.wild()) continue;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (b.damageState.unaffected || b.damageState.substitute) continue;
			if (!b.item) continue;
			if (b.unlosableItem(b.item) || user.unlosableItem(b.item)) continue;
			battle.ShowAbilitySplash(user);
			if (b.hasActiveAbility(Abilitys.STICKYHOLD)) {
				if (user.opposes(b)) battle.ShowAbilitySplash(b);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					battle.Display(_INTL("{1}'s item cannot be stolen!", b.ToString()));
				}
				if (user.opposes(b)) battle.HideAbilitySplash(b);
				continue;
			}
			user.item = b.item;
			b.item = null;
			if (b.hasActiveAbility(Abilitys.UNBURDEN)) b.effects.Unburden = true;
			if (battle.wildBattle() && !user.initialItem && user.item == b.initialItem) {
				user.setInitialItem(user.item);
				b.setInitialItem(null);
			}
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} stole {2}'s {3}!", user.ToString(),
					b.ToString(true), user.itemName));
			} else {
				battle.Display(_INTL("{1} stole {2}'s {3} with {4}!", user.ToString(),
					b.ToString(true), user.itemName, user.abilityName));
			}
			battle.HideAbilitySplash(user);
			user.HeldItemTriggerCheck;
			break;
		}
	}
)

Battle.AbilityEffects.OnEndOfUsingMove.Add(Moves.MOXIE,
	block: (ability, user, targets, move, battle) => {
		if (battle.AllFainted(user.idxOpposingSide)) continue;
		numFainted = 0;
		targets.each(b => { if (b.damageState.fainted) numFainted += 1; });
		if (numFainted == 0 || !user.CanRaiseStatStage(:ATTACK, user)) continue;
		user.RaiseStatStageByAbility(:ATTACK, numFainted, user);
	}
)

//===============================================================================
// AfterMoveUseFromTarget handlers
//===============================================================================

Battle.AbilityEffects.AfterMoveUseFromTarget.add(:BERSERK,
	block: (ability, target, user, move, switched_battlers, battle) => {
		if (!move.damagingMove()) continue;
		if (!target.droppedBelowHalfHP) continue;
		if (!target.CanRaiseStatStage(:SPECIAL_ATTACK, target)) continue;
		target.RaiseStatStageByAbility(:SPECIAL_ATTACK, 1, target);
	}
)

Battle.AbilityEffects.AfterMoveUseFromTarget.add(:COLORCHANGE,
	block: (ability, target, user, move, switched_battlers, battle) => {
		if (target.damageState.calcDamage == 0 || target.damageState.substitute) continue;
		if (!move.calcType || GameData.Type.get(move.calcType).pseudo_type) continue;
		if (target.HasType(move.calcType) && !target.HasOtherType(move.calcType)) continue;
		typeName = GameData.Type.get(move.calcType).name;
		battle.ShowAbilitySplash(target);
		target.ChangeTypes(move.calcType);
		battle.Display(_INTL("{1}'s type changed to {2} because of its {3}!",
			target.ToString(), typeName, target.abilityName));
		battle.HideAbilitySplash(target);
	}
)

Battle.AbilityEffects.AfterMoveUseFromTarget.add(:PICKPOCKET,
	block: (ability, target, user, move, switched_battlers, battle) => {
		// NOTE: According to Bulbapedia, this can still trigger to steal the user's
		//       item even if it was switched out by a Red Card. That doesn't make
		//       sense, so this code doesn't do it.
		if (target.wild()) continue;
		if (switched_battlers.Contains(user.index)) continue;   // User was switched out
		if (!move.ContactMove(user)) continue;
		if (user.effects.Substitute > 0 || target.damageState.substitute) continue;
		if (target.item || !user.item) continue;
		if (user.unlosableItem(user.item) || target.unlosableItem(user.item)) continue;
		battle.ShowAbilitySplash(target);
		if (user.hasActiveAbility(Abilitys.STICKYHOLD)) {
			if (target.opposes(user)) battle.ShowAbilitySplash(user);
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s item cannot be stolen!", user.ToString()));
			}
			if (target.opposes(user)) battle.HideAbilitySplash(user);
			battle.HideAbilitySplash(target);
			continue;
		}
		target.item = user.item;
		user.item = null;
		if (user.hasActiveAbility(Abilitys.UNBURDEN)) user.effects.Unburden = true;
		if (battle.wildBattle() && !target.initialItem && target.item == user.initialItem) {
			target.setInitialItem(target.item);
			user.setInitialItem(null);
		}
		battle.Display(_INTL("{1} pickpocketed {2}'s {3}!", target.ToString(),
			user.ToString(true), target.itemName));
		battle.HideAbilitySplash(target);
		target.HeldItemTriggerCheck;
	}
)

//===============================================================================
// EndOfRoundWeather handlers
//===============================================================================

Battle.AbilityEffects.EndOfRoundWeather.Add(Weathers.DRYSKIN,
	block: (ability, weather, battler, battle) => {
		switch (weather) {
			case :Sun: case :HarshSun:
				if (battler.takesIndirectDamage()) {
					battle.ShowAbilitySplash(battler);
					battle.scene.DamageAnimation(battler);
					battler.ReduceHP(battler.totalhp / 8, false);
					battle.Display(_INTL("{1} was hurt by the sunlight!", battler.ToString()));
					battle.HideAbilitySplash(battler);
					battler.ItemHPHealCheck;
				}
				break;
			case :Rain: case :HeavyRain:
				if (!battler.canHeal()) continue;
				battle.ShowAbilitySplash(battler);
				battler.RecoverHP(battler.totalhp / 8);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
				} else {
					battle.Display(_INTL("{1}'s {2} restored its HP.", battler.ToString(), battler.abilityName));
				}
				battle.HideAbilitySplash(battler);
				break;
		}
	}
)

Battle.AbilityEffects.EndOfRoundWeather.Add(Weathers.ICEBODY,
	block: (ability, weather, battler, battle) => {
		if (!new []{:Hail, :Snowstorm}.Contains(weather)) continue;
		if (!battler.canHeal()) continue;
		battle.ShowAbilitySplash(battler);
		battler.RecoverHP(battler.totalhp / 16);
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1}'s {2} restored its HP.", battler.ToString(), battler.abilityName));
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.EndOfRoundWeather.Add(Weathers.ICEFACE,
	block: (ability, weather, battler, battle) => {
		if (!new []{:Hail, :Snowstorm}.Contains(weather)) continue;
		if (!battler.canRestoreIceFace || battler.form != 1) continue;
		battle.ShowAbilitySplash(battler);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1}'s {2} activated!", battler.ToString(), battler.abilityName));
		}
		battler.ChangeForm(0, _INTL("{1} transformed!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.EndOfRoundWeather.Add(Weathers.RAINDISH,
	block: (ability, weather, battler, battle) => {
		if (!new []{:Rain, :HeavyRain}.Contains(weather)) continue;
		if (!battler.canHeal()) continue;
		battle.ShowAbilitySplash(battler);
		battler.RecoverHP(battler.totalhp / 16);
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		} else {
			battle.Display(_INTL("{1}'s {2} restored its HP.", battler.ToString(), battler.abilityName));
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.EndOfRoundWeather.Add(Weathers.SOLARPOWER,
	block: (ability, weather, battler, battle) => {
		if (!new []{:Sun, :HarshSun}.Contains(weather)) continue;
		if (!battler.takesIndirectDamage()) continue;
		battle.ShowAbilitySplash(battler);
		battle.scene.DamageAnimation(battler);
		battler.ReduceHP(battler.totalhp / 8, false);
		battle.Display(_INTL("{1} was hurt by the sunlight!", battler.ToString()));
		battle.HideAbilitySplash(battler);
		battler.ItemHPHealCheck;
	}
)

//===============================================================================
// EndOfRoundHealing handlers
//===============================================================================

Battle.AbilityEffects.EndOfRoundHealing.add(:HEALER,
	block: (ability, battler, battle) => {
		if (battle.Random(100) >= 30) continue;
		foreach (var b in battler.allAllies) { //'battler.allAllies.each' do => |b|
			if (b.status == statuses.NONE) continue;
			battle.ShowAbilitySplash(battler);
			oldStatus = b.status;
			b.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				switch (oldStatus) {
					case :SLEEP:
						battle.Display(_INTL("{1}'s {2} woke its partner up!", battler.ToString(), battler.abilityName));
						break;
					case :POISON:
						battle.Display(_INTL("{1}'s {2} cured its partner's poison!", battler.ToString(), battler.abilityName));
						break;
					case :BURN:
						battle.Display(_INTL("{1}'s {2} healed its partner's burn!", battler.ToString(), battler.abilityName));
						break;
					case :PARALYSIS:
						battle.Display(_INTL("{1}'s {2} cured its partner's paralysis!", battler.ToString(), battler.abilityName));
						break;
					case :FROZEN:
						battle.Display(_INTL("{1}'s {2} defrosted its partner!", battler.ToString(), battler.abilityName));
						break;
				}
			}
			battle.HideAbilitySplash(battler);
		}
	}
)

Battle.AbilityEffects.EndOfRoundHealing.add(:HYDRATION,
	block: (ability, battler, battle) => {
		if (battler.status == statuses.NONE) continue;
		if (!new []{:Rain, :HeavyRain}.Contains(battler.effectiveWeather)) continue;
		battle.ShowAbilitySplash(battler);
		oldStatus = battler.status;
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			switch (oldStatus) {
				case :SLEEP:
					battle.Display(_INTL("{1}'s {2} woke it up!", battler.ToString(), battler.abilityName));
					break;
				case :POISON:
					battle.Display(_INTL("{1}'s {2} cured its poison!", battler.ToString(), battler.abilityName));
					break;
				case :BURN:
					battle.Display(_INTL("{1}'s {2} healed its burn!", battler.ToString(), battler.abilityName));
					break;
				case :PARALYSIS:
					battle.Display(_INTL("{1}'s {2} cured its paralysis!", battler.ToString(), battler.abilityName));
					break;
				case :FROZEN:
					battle.Display(_INTL("{1}'s {2} defrosted it!", battler.ToString(), battler.abilityName));
					break;
			}
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.EndOfRoundHealing.add(:SHEDSKIN,
	block: (ability, battler, battle) => {
		if (battler.status == statuses.NONE) continue;
		unless (battle.Random(100) < 30) continue;
		battle.ShowAbilitySplash(battler);
		oldStatus = battler.status;
		battler.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			switch (oldStatus) {
				case :SLEEP:
					battle.Display(_INTL("{1}'s {2} woke it up!", battler.ToString(), battler.abilityName));
					break;
				case :POISON:
					battle.Display(_INTL("{1}'s {2} cured its poison!", battler.ToString(), battler.abilityName));
					break;
				case :BURN:
					battle.Display(_INTL("{1}'s {2} healed its burn!", battler.ToString(), battler.abilityName));
					break;
				case :PARALYSIS:
					battle.Display(_INTL("{1}'s {2} cured its paralysis!", battler.ToString(), battler.abilityName));
					break;
				case :FROZEN:
					battle.Display(_INTL("{1}'s {2} defrosted it!", battler.ToString(), battler.abilityName));
					break;
			}
		}
		battle.HideAbilitySplash(battler);
	}
)

//===============================================================================
// EndOfRoundEffect handlers
//===============================================================================

Battle.AbilityEffects.EndOfRoundEffect.add(:BADDREAMS,
	block: (ability, battler, battle) => {
		foreach (var b in battle.allOtherSideBattlers(battler.index)) { //'battle.allOtherSideBattlers(battler.index).each' do => |b|
			if (!b.near(battler) || !b.asleep()) continue;
			battle.ShowAbilitySplash(battler);
			if (!b.takesIndirectDamage(Battle.Scene.USE_ABILITY_SPLASH)) continue;
			b.TakeEffectDamage(b.totalhp / 8) do |hp_lost|
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					battle.Display(_INTL("{1} is tormented!", b.ToString()));
				} else {
					battle.Display(_INTL("{1} is tormented by {2}'s {3}!",
						b.ToString(), battler.ToString(true), battler.abilityName));
				}
				battle.HideAbilitySplash(battler);
			}
		}
	}
)

Battle.AbilityEffects.EndOfRoundEffect.add(:MOODY,
	block: (ability, battler, battle) => {
		randomUp = new List<string>();
		randomDown = new List<string>();
		if (Settings.MECHANICS_GENERATION >= 8) {
			foreach (var s in GameData.Stat) { //GameData.Stat.each_main_battle do => |s|
				if (battler.CanRaiseStatStage(s.id, battler)) randomUp.Add(s.id);
				if (battler.CanLowerStatStage(s.id, battler)) randomDown.Add(s.id);
			}
		} else {
			foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
				if (battler.CanRaiseStatStage(s.id, battler)) randomUp.Add(s.id);
				if (battler.CanLowerStatStage(s.id, battler)) randomDown.Add(s.id);
			}
		}
		if (randomUp.length == 0 && randomDown.length == 0) continue;
		battle.ShowAbilitySplash(battler);
		if (randomUp.length > 0) {
			r = battle.Random(randomUp.length);
			battler.RaiseStatStageByAbility(randomUp[r], 2, battler, false);
			randomDown.delete(randomUp[r]);
		}
		if (randomDown.length > 0) {
			r = battle.Random(randomDown.length);
			battler.LowerStatStageByAbility(randomDown[r], 1, battler, false);
		}
		battle.HideAbilitySplash(battler);
		if (randomDown.length > 0) battler.ItemStatRestoreCheck;
		battler.ItemOnStatDropped;
	}
)

Battle.AbilityEffects.EndOfRoundEffect.add(:SPEEDBOOST,
	block: (ability, battler, battle) => {
		// A Pokémon's turnCount is 0 if it became active after the beginning of a
		// round
		if (battler.turnCount > 0 && battle.choices[battler.index].Action != :Run &&
			battler.CanRaiseStatStage(:SPEED, battler)) {
			battler.RaiseStatStageByAbility(:SPEED, 1, battler);
		}
	}
)

//===============================================================================
// EndOfRoundGainItem handlers
//===============================================================================

Battle.AbilityEffects.EndOfRoundGainItem.Add(Items.BALLFETCH,
	block: (ability, battler, battle) => {
		if (battler.item) continue;
		if (battle.first_poke_ball.null()) continue;
		battle.ShowAbilitySplash(battler);
		battler.item = battle.first_poke_ball;
		if (!battler.initialItem) battler.setInitialItem(battler.item);
		battle.first_poke_ball = null;
		battle.Display(_INTL("{1} retrieved the thrown {2}!", battler.ToString(), battler.itemName));
		battle.HideAbilitySplash(battler);
		battler.HeldItemTriggerCheck;
	}
)

Battle.AbilityEffects.EndOfRoundGainItem.Add(Items.HARVEST,
	block: (ability, battler, battle) => {
		if (battler.item) continue;
		if (!battler.recycleItem || !GameData.Item.get(battler.recycleItem).is_berry()) continue;
		if (!new []{:Sun, :HarshSun}.Contains(battler.effectiveWeather)) {
			unless (battle.Random(100) < 50) continue;
		}
		battle.ShowAbilitySplash(battler);
		battler.item = battler.recycleItem;
		battler.setRecycleItem(null);
		if (!battler.initialItem) battler.setInitialItem(battler.item);
		battle.Display(_INTL("{1} harvested one {2}!", battler.ToString(), battler.itemName));
		battle.HideAbilitySplash(battler);
		battler.HeldItemTriggerCheck;
	}
)

Battle.AbilityEffects.EndOfRoundGainItem.Add(Items.PICKUP,
	block: (ability, battler, battle) => {
		if (battler.item) continue;
		foundItem = null;
		fromBattler = null;
		use = 0;
		foreach (var b in battle.allBattlers) { //'battle.allBattlers.each' do => |b|
			if (b.index == battler.index) continue;
			if (b.effects.PickupUse <= use) continue;
			foundItem   = b.effects.PickupItem;
			fromBattler = b;
			use         = b.effects.PickupUse;
		}
		if (!foundItem) continue;
		battle.ShowAbilitySplash(battler);
		battler.item = foundItem;
		fromBattler.effects.PickupItem = null;
		fromBattler.effects.PickupUse  = 0;
		if (fromBattler.recycleItem == foundItem) fromBattler.setRecycleItem(null);
		if (battle.wildBattle() && !battler.initialItem && fromBattler.initialItem == foundItem) {
			battler.setInitialItem(foundItem);
			fromBattler.setInitialItem(null);
		}
		battle.Display(_INTL("{1} found one {2}!", battler.ToString(), battler.itemName));
		battle.HideAbilitySplash(battler);
		battler.HeldItemTriggerCheck;
	}
)

//===============================================================================
// CertainSwitching handlers
//===============================================================================

// There aren't any!

//===============================================================================
// TrappingByTarget handlers
//===============================================================================

Battle.AbilityEffects.TrappingByTarget.add(:ARENATRAP,
	block: (ability, switcher, bearer, battle) => {
		if (!switcher.airborne()) next true;
	}
)

Battle.AbilityEffects.TrappingByTarget.add(:MAGNETPULL,
	block: (ability, switcher, bearer, battle) => {
		if (switcher.Type == Types.STEEL) next true;
	}
)

Battle.AbilityEffects.TrappingByTarget.add(:SHADOWTAG,
	block: (ability, switcher, bearer, battle) => {
		if (!switcher.hasActiveAbility(Abilitys.SHADOWTAG)) next true;
	}
)

//===============================================================================
// OnSwitchIn handlers
//===============================================================================

Battle.AbilityEffects.OnSwitchIn.add(:AIRLOCK,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1} has {2}!", battler.ToString(), battler.abilityName));
		}
		battle.Display(_INTL("The effects of the weather disappeared."));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.copy(:AIRLOCK, :CLOUDNINE);

Battle.AbilityEffects.OnSwitchIn.add(:ANTICIPATION,
	block: (ability, battler, battle, switch_in) => {
		if (!battler.OwnedByPlayer()) continue;
		battlerTypes = battler.Types(true);
		types = battlerTypes;
		found = false;
		foreach (var b in battle.allOtherSideBattlers(battler.index)) { //'battle.allOtherSideBattlers(battler.index).each' do => |b|
			foreach (var m in b.Moves) { //b.eachMove do => |m|
				if (m.statusMove()) continue;
				if (types.length > 0) {
					moveType = m.type;
					if (Settings.MECHANICS_GENERATION >= 6 && m.function_code == "TypeDependsOnUserIVs") {   // Hidden Power
						moveType = HiddenPower(b.pokemon)[0];
					}
					eff = Effectiveness.calculate(moveType, *types);
					if (Effectiveness.ineffective(eff)) continue;
					if (!Effectiveness.super_effective(eff) &&
									!new []{"OHKO", "OHKOIce", "OHKOHitsUndergroundTarget"}.Contains(m.function_code)) continue;
				} else if (!new []{"OHKO", "OHKOIce", "OHKOHitsUndergroundTarget"}.Contains(m.function_code)) {
					continue;
				}
				found = true;
				break;
			}
			if (found) break;
		}
		if (found) {
			battle.ShowAbilitySplash(battler);
			battle.Display(_INTL("{1} shuddered with anticipation!", battler.ToString()));
			battle.HideAbilitySplash(battler);
		}
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:ASONECHILLINGNEIGH,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} has two Abilities!", battler.ToString()));
		battle.HideAbilitySplash(battler);
		battler.ability_id = :UNNERVE;
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is too nervous to eat Berries!", battler.OpposingTeam));
		battle.HideAbilitySplash(battler);
		battler.ability_id = ability;
	}
)

Battle.AbilityEffects.OnSwitchIn.copy(:ASONECHILLINGNEIGH, :ASONEGRIMNEIGH);

Battle.AbilityEffects.OnSwitchIn.add(:AURABREAK,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} reversed all other Pokémon's auras!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:COMATOSE,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is drowsing!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:CURIOUSMEDICINE,
	block: (ability, battler, battle, switch_in) => {
		if (battler.allAllies.none(b => b.hasAlteredStatStages())) continue;
		battle.ShowAbilitySplash(battler);
		foreach (var b in battler.allAllies) { //'battler.allAllies.each' do => |b|
			if (!b.hasAlteredStatStages()) continue;
			b.ResetStatStages;
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s stat changes were removed!", b.ToString()));
			} else {
				battle.Display(_INTL("{1}'s stat changes were removed by {2}'s {3}!",
					b.ToString(), battler.ToString(true), battler.abilityName));
			}
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DARKAURA,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is radiating a dark aura!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DAUNTLESSSHIELD,
	block: (ability, battler, battle, switch_in) => {
		battler.RaiseStatStageByAbility(:DEFENSE, 1, battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DELTASTREAM,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility(:StrongWinds, battler, true);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DESOLATELAND,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility(:HarshSun, battler, true);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DOWNLOAD,
	block: (ability, battler, battle, switch_in) => {
		oDef = oSpDef = 0;
		foreach (var b in battle.allOtherSideBattlers(battler.index)) { //'battle.allOtherSideBattlers(battler.index).each' do => |b|
			oDef   += b.defense;
			oSpDef += b.spdef;
		}
		stat = (oDef < oSpDef) ? :ATTACK : :SPECIAL_ATTACK;
		battler.RaiseStatStageByAbility(stat, 1, battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DRIZZLE,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility(:Rain, battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:DROUGHT,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility(:Sun, battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:ELECTRICSURGE,
	block: (ability, battler, battle, switch_in) => {
		if (battle.field.terrain == :Electric) continue;
		battle.ShowAbilitySplash(battler);
		battle.StartTerrain(battler, :Electric);
		// NOTE: The ability splash is hidden again in def StartTerrain.
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:FAIRYAURA,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is radiating a fairy aura!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:FOREWARN,
	block: (ability, battler, battle, switch_in) => {
		if (!battler.OwnedByPlayer()) continue;
		highestPower = 0;
		forewarnMoves = new List<string>()
		foreach (var b in battle.allOtherSideBattlers(battler.index)) { //'battle.allOtherSideBattlers(battler.index).each' do => |b|
			foreach (var m in b.Moves) { //b.eachMove do => |m|
				power = m.power;
				if (new []{"OHKO", "OHKOIce", "OHKOHitsUndergroundTarget"}.Contains(m.function_code)) power = 160;
				if (["PowerHigherWithUserHP"].Contains(m.function_code)) power = 150;    // Eruption
				// Counter, Mirror Coat, Metal Burst
				if ((new []{"CounterPhysicalDamage",
												"CounterSpecialDamage",
												"CounterDamagePlusHalf"}.Contains(m.function_code)) power = 120;
				// Sonic Boom, Dragon Rage, Night Shade, Endeavor, Psywave,
				// Return, Frustration, Crush Grip, Gyro Ball, Hidden Power,
				// Natural Gift, Trump Card, Flail, Grass Knot
				if ((new []{"FixedDamage20",
											"FixedDamage40",
											"FixedDamageUserLevel",
											"LowerTargetHPToUserHP",
											"FixedDamageUserLevelRandom",
											"PowerHigherWithUserHappiness",
											"PowerLowerWithUserHappiness",
											"PowerHigherWithUserHP",
											"PowerHigherWithTargetFasterThanUser",
											"TypeAndPowerDependOnUserBerry",
											"PowerHigherWithLessPP",
											"PowerLowerWithUserHP",
											"PowerHigherWithTargetWeight"}.Contains(m.function_code)) power = 80;
				if (Settings.MECHANICS_GENERATION <= 5 && m.function_code == "TypeDependsOnUserIVs") power = 80;
				if (power < highestPower) continue;
				if (power > highestPower) forewarnMoves = new List<string>();
				forewarnMoves.Add(m.name)
				highestPower = power;
			}
		}
		if (forewarnMoves.length > 0) {
			battle.ShowAbilitySplash(battler);
			forewarnMoveName = forewarnMoves[battle.Random(forewarnMoves.length)]
			if (Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1} was alerted to {2}!",
					battler.ToString(), forewarnMoveName));
			} else {
				battle.Display(_INTL("{1}'s Forewarn alerted it to {2}!",
					battler.ToString(), forewarnMoveName));
			}
			battle.HideAbilitySplash(battler);
		}
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:FRISK,
	block: (ability, battler, battle, switch_in) => {
		if (!battler.OwnedByPlayer()) continue;
		foes = battle.allOtherSideBattlers(battler.index).select(b => b.item);
		if (foes.length > 0) {
			battle.ShowAbilitySplash(battler);
			if (Settings.MECHANICS_GENERATION >= 6) {
				foreach (var b in foes) { //'foes.each' do => |b|
					battle.Display(_INTL("{1} frisked {2} and found its {3}!",
						battler.ToString(), b.ToString(true), b.itemName));
				}
			} else {
				foe = foes[battle.Random(foes.length)];
				battle.Display(_INTL("{1} frisked the foe and found one {2}!",
					battler.ToString(), foe.itemName));
			}
			battle.HideAbilitySplash(battler);
		}
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:GRASSYSURGE,
	block: (ability, battler, battle, switch_in) => {
		if (battle.field.terrain == :Grassy) continue;
		battle.ShowAbilitySplash(battler);
		battle.StartTerrain(battler, :Grassy);
		// NOTE: The ability splash is hidden again in def StartTerrain.
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:ICEFACE,
	block: (ability, battler, battle, switch_in) => {
		if (!battler.isSpecies(Speciess.EISCUE) || battler.form != 1) continue;
		if (!new []{:Hail, :Snowstorm}.Contains(battler.effectiveWeather)) continue;
		battle.ShowAbilitySplash(battler);
		if (!Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1}'s {2} activated!", battler.ToString(), battler.abilityName));
		}
		battler.ChangeForm(0, _INTL("{1} transformed!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:IMPOSTER,
	block: (ability, battler, battle, switch_in) => {
		if (!switch_in || battler.effects.Transform) continue;
		choice = battler.DirectOpposing;
		if (choice.fainted()) continue;
		if (choice.effects.Transform ||
						choice.effects.Illusion ||
						choice.effects.Substitute > 0 ||
						choice.effects.SkyDrop >= 0 ||
						choice.semiInvulnerable()) continue;
		battle.ShowAbilitySplash(battler, true);
		battle.HideAbilitySplash(battler);
		battle.Animation(:TRANSFORM, battler, choice);
		battle.scene.ChangePokemon(battler, choice.pokemon);
		battler.Transform(choice);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:INTIMIDATE,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		foreach (var b in battle.allOtherSideBattlers(battler.index)) { //'battle.allOtherSideBattlers(battler.index).each' do => |b|
			if (!b.near(battler)) continue;
			check_item = true;
			if (b.hasActiveAbility(Abilitys.CONTRARY)) {
				if (b.statStageAtMax(:ATTACK)) check_item = false;
			} else if (b.statStageAtMin(:ATTACK)) {
				check_item = false;
			}
			check_ability = b.LowerAttackStatStageIntimidate(battler);
			if (check_ability) b.AbilitiesOnIntimidated;
			if (check_item) b.ItemOnIntimidatedCheck;
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:INTREPIDSWORD,
	block: (ability, battler, battle, switch_in) => {
		battler.RaiseStatStageByAbility(:ATTACK, 1, battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:MIMICRY,
	block: (ability, battler, battle, switch_in) => {
		if (battle.field.terrain == :None) continue;
		Battle.AbilityEffects.triggerOnTerrainChange(ability, battler, battle, false);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:MISTYSURGE,
	block: (ability, battler, battle, switch_in) => {
		if (battle.field.terrain == :Misty) continue;
		battle.ShowAbilitySplash(battler);
		battle.StartTerrain(battler, :Misty);
		// NOTE: The ability splash is hidden again in def StartTerrain.
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:MOLDBREAKER,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} breaks the mold!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:NEUTRALIZINGGAS,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler, true);
		battle.HideAbilitySplash(battler);
		battle.Display(_INTL("Neutralizing gas filled the area!"));
		foreach (var b in battle.allBattlers) { //'battle.allBattlers.each' do => |b|
			// Slow Start - end all turn counts
			b.effects.SlowStart = 0;
			// Truant - let b move on its first turn after Neutralizing Gas disappears
			b.effects.Truant = false;
			// Gorilla Tactics - end choice lock
			if (!b.hasActiveItem(new {:CHOICEBAND, :CHOICESPECS, :CHOICESCARF})) {
				b.effects.ChoiceBand = null;
			}
			// Illusion - end illusions
			if (b.effects.Illusion) {
				b.effects.Illusion = null;
				if (!b.effects.Transform) {
					battle.scene.ChangePokemon(b, b.pokemon);
					battle.Display(_INTL("{1}'s {2} wore off!", b.ToString(), b.abilityName));
					battle.SetSeen(b);
				}
			}
		}
		// Trigger items upon Unnerve being negated
		battler.ability_id = null;   // Allows checking if Unnerve was active before
		had_unnerve = battle.CheckGlobalAbility(new {:UNNERVE, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH});
		battler.ability_id = :NEUTRALIZINGGAS;
		if (had_unnerve && !battle.CheckGlobalAbility(new {:UNNERVE, :ASONECHILLINGNEIGH, :ASONEGRIMNEIGH})) {
			battle.allBattlers.each(b => b.ItemsOnUnnerveEnding);
		}
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:PASTELVEIL,
	block: (ability, battler, battle, switch_in) => {
		if (battler.allAllies.none(b => b.status == statuses.POISON)) continue;
		battle.ShowAbilitySplash(battler);
		foreach (var b in battler.allAllies) { //'battler.allAllies.each' do => |b|
			if (b.status != statuses.POISON) continue;
			b.CureStatus(Battle.Scene.USE_ABILITY_SPLASH);
			if (!Battle.Scene.USE_ABILITY_SPLASH) {
				battle.Display(_INTL("{1}'s {2} cured {3}'s poisoning!",
					battler.ToString(), battler.abilityName, b.ToString(true)));
			}
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:PRESSURE,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is exerting its pressure!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:PRIMORDIALSEA,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility(:HeavyRain, battler, true);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:PSYCHICSURGE,
	block: (ability, battler, battle, switch_in) => {
		if (battle.field.terrain == :Psychic) continue;
		battle.ShowAbilitySplash(battler);
		battle.StartTerrain(battler, :Psychic);
		// NOTE: The ability splash is hidden again in def StartTerrain.
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:SANDSTREAM,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility(:Sandstorm, battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:SCREENCLEANER,
	block: (ability, battler, battle, switch_in) => {
		if (battler.OwnSide.effects.AuroraVeil == 0 &&
						battler.OwnSide.effects.LightScreen == 0 &&
						battler.OwnSide.effects.Reflect == 0 &&
						battler.OpposingSide.effects.AuroraVeil == 0 &&
						battler.OpposingSide.effects.LightScreen == 0 &&
						battler.OpposingSide.effects.Reflect == 0) continue;
		battle.ShowAbilitySplash(battler);
		if (battler.OpposingSide.effects.AuroraVeil > 0) {
			battler.OpposingSide.effects.AuroraVeil = 0;
			battle.Display(_INTL("{1}'s Aurora Veil wore off!", battler.OpposingTeam));
		}
		if (battler.OpposingSide.effects.LightScreen > 0) {
			battler.OpposingSide.effects.LightScreen = 0;
			battle.Display(_INTL("{1}'s Light Screen wore off!", battler.OpposingTeam));
		}
		if (battler.OpposingSide.effects.Reflect > 0) {
			battler.OpposingSide.effects.Reflect = 0;
			battle.Display(_INTL("{1}'s Reflect wore off!", battler.OpposingTeam));
		}
		if (battler.OwnSide.effects.AuroraVeil > 0) {
			battler.OwnSide.effects.AuroraVeil = 0;
			battle.Display(_INTL("{1}'s Aurora Veil wore off!", battler.Team));
		}
		if (battler.OwnSide.effects.LightScreen > 0) {
			battler.OwnSide.effects.LightScreen = 0;
			battle.Display(_INTL("{1}'s Light Screen wore off!", battler.Team));
		}
		if (battler.OwnSide.effects.Reflect > 0) {
			battler.OwnSide.effects.Reflect = 0;
			battle.Display(_INTL("{1}'s Reflect wore off!", battler.Team));
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:SLOWSTART,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battler.effects.SlowStart = 5;
		if (Battle.Scene.USE_ABILITY_SPLASH) {
			battle.Display(_INTL("{1} can't get it going!", battler.ToString()));
		} else {
			battle.Display(_INTL("{1} can't get it going because of its {2}!",
				battler.ToString(), battler.abilityName));
		}
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:SNOWWARNING,
	block: (ability, battler, battle, switch_in) => {
		battle.StartWeatherAbility((Settings.USE_SNOWSTORM_WEATHER_INSTEAD_OF_HAIL ? :Snowstorm : :Hail), battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:TERAVOLT,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is radiating a bursting aura!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:TURBOBLAZE,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is radiating a blazing aura!", battler.ToString()));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:UNNERVE,
	block: (ability, battler, battle, switch_in) => {
		battle.ShowAbilitySplash(battler);
		battle.Display(_INTL("{1} is too nervous to eat Berries!", battler.OpposingTeam));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.OnSwitchIn.add(:WINDRIDER,
	block: (ability, battler, battle, switch_in) => {
		if (battler.OwnSide.effects.Tailwind == 0) continue;
		battler.RaiseStatStageByAbility(:ATTACK, 1, battler);
	}
)

//===============================================================================
// OnSwitchOut handlers
//===============================================================================

Battle.AbilityEffects.OnSwitchOut.add(:IMMUNITY,
	block: (ability, battler, endOfBattle) => {
		if (battler.status != statuses.POISON) continue;
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.status = :NONE;
	}
)

Battle.AbilityEffects.OnSwitchOut.add(:INSOMNIA,
	block: (ability, battler, endOfBattle) => {
		if (battler.status != statuses.SLEEP) continue;
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.status = :NONE;
	}
)

Battle.AbilityEffects.OnSwitchOut.copy(:INSOMNIA, :VITALSPIRIT);

Battle.AbilityEffects.OnSwitchOut.add(:LIMBER,
	block: (ability, battler, endOfBattle) => {
		if (battler.status != statuses.PARALYSIS) continue;
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.status = :NONE;
	}
)

Battle.AbilityEffects.OnSwitchOut.add(:MAGMAARMOR,
	block: (ability, battler, endOfBattle) => {
		if (battler.status != statuses.FROZEN) continue;
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.status = :NONE;
	}
)

Battle.AbilityEffects.OnSwitchOut.add(:NATURALCURE,
	block: (ability, battler, endOfBattle) => {
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.status = :NONE;
	}
)

Battle.AbilityEffects.OnSwitchOut.add(:REGENERATOR,
	block: (ability, battler, endOfBattle) => {
		if (endOfBattle) continue;
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.RecoverHP(battler.totalhp / 3, false, false);
	}
)

Battle.AbilityEffects.OnSwitchOut.add(:WATERVEIL,
	block: (ability, battler, endOfBattle) => {
		if (battler.status != statuses.BURN) continue;
		Debug.Log($"[Ability triggered] {battler.ToString()}'s {battler.abilityName}");
		battler.status = :NONE;
	}
)

Battle.AbilityEffects.OnSwitchOut.copy(:WATERVEIL, :WATERBUBBLE);

//===============================================================================
// ChangeOnBattlerFainting handlers
//===============================================================================

Battle.AbilityEffects.ChangeOnBattlerFainting.add(:POWEROFALCHEMY,
	block: (ability, battler, fainted, battle) => {
		if (battler.opposes(fainted)) continue;
		if (fainted.ungainableAbility() ||
			new []{:POWEROFALCHEMY, :RECEIVER, :TRACE, :WONDERGUARD}.Contains(fainted.ability_id)) continue;
		battle.ShowAbilitySplash(battler, true);
		battler.ability = fainted.ability;
		battle.ReplaceAbilitySplash(battler);
		battle.Display(_INTL("{1}'s {2} was taken over!", fainted.ToString(), fainted.abilityName));
		battle.HideAbilitySplash(battler);
	}
)

Battle.AbilityEffects.ChangeOnBattlerFainting.copy(:POWEROFALCHEMY, :RECEIVER);

//===============================================================================
// OnBattlerFainting handlers
//===============================================================================

Battle.AbilityEffects.OnBattlerFainting.add(:SOULHEART,
	block: (ability, battler, fainted, battle) => {
		battler.RaiseStatStageByAbility(:SPECIAL_ATTACK, 1, battler);
	}
)

//===============================================================================
// OnTerrainChange handlers
//===============================================================================

Battle.AbilityEffects.OnTerrainChange.add(:MIMICRY,
	block: (ability, battler, battle, ability_changed) => {
		if (battle.field.terrain == :None) {
			// Revert to original typing
			battle.ShowAbilitySplash(battler);
			battler.ResetTypes;
			battle.Display(_INTL("{1} changed back to its regular type!", battler.ToString()));
			battle.HideAbilitySplash(battler);
		} else {
			// Change to new typing
			terrain_hash = {
				Electric = :ELECTRIC,
				Grassy   = :GRASS,
				Misty    = :FAIRY,
				Psychic  = :PSYCHIC;
			}
			new_type = terrain_hash[battle.field.terrain];
			new_type_name = null;
			if (new_type) {
				type_data = GameData.Type.try_get(new_type);
				if (!type_data) new_type = null;
				if (type_data) new_type_name = type_data.name;
			}
			if (new_type) {
				battle.ShowAbilitySplash(battler);
				battler.ChangeTypes(new_type);
				battle.Display(_INTL("{1}'s type changed to {2}!", battler.ToString(), new_type_name));
				battle.HideAbilitySplash(battler);
			}
		}
	}
)

//===============================================================================
// OnIntimidated handlers
//===============================================================================

Battle.AbilityEffects.OnIntimidated.add(:RATTLED,
	block: (ability, battler, battle) => {
		if (Settings.MECHANICS_GENERATION < 8) continue;
		battler.RaiseStatStageByAbility(:SPEED, 1, battler);
	}
)

//===============================================================================
// CertainEscapeFromBattle handlers
//===============================================================================

Battle.AbilityEffects.CertainEscapeFromBattle.add(:RUNAWAY,
	block: (ability, battler) => {
		next true;
	}
)
