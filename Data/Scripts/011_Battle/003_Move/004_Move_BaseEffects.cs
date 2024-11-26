// DO NOT USE ANY CLASS NAMES IN HERE AS FUNCTION CODES!
// These are base classes for other classes to build on; those other classes are
// named after function codes, so use those instead.

//===============================================================================
// Superclass that handles moves using a non-existent function code.
// Damaging moves just do damage with no additional effect.
// Status moves always fail.
//===============================================================================
public partial class Battle.Move.Unimplemented : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (statusMove()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Pseudomove for confusion damage.
//===============================================================================
public partial class Battle.Move.Confusion : Battle.Move {
	public void initialize(battle, move) {
		@battle        = battle;
		@realMove      = move;
		@id            = :CONFUSEDAMAGE;
		@name          = "";
		@function_code = "None";
		@power         = 40;
		@type          = null;
		@category      = 0;
		@accuracy      = 100;
		@pp            = -1;
		@target        = :User;
		@priority      = 0;
		@flags         = new List<string>();
		@addlEffect    = 0;
		@powerBoost    = false;
		@snatched      = false;
	}

	public bool physicalMove(thisType = null) {    return true;  }
	public bool specialMove(thisType = null) {     return false; }
	public void CritialOverride(user, target) {return -1;    }
}

//===============================================================================
// Struggle.
//===============================================================================
public partial class Battle.Move.Struggle : Battle.Move {
	public void initialize(battle, move) {
		@battle        = battle;
		@realMove      = null;                     // Not associated with a move
		@id            = :STRUGGLE;
		@name          = _INTL("Struggle");
		@function_code = "Struggle";
		@power         = 50;
		@type          = null;
		@category      = 0;
		@accuracy      = 0;
		@pp            = -1;
		@target        = :RandomNearFoe;
		@priority      = 0;
		@flags         = new {"Contact", "CanProtect"};
		@addlEffect    = 0;
		@powerBoost    = false;
		@snatched      = false;
	}

	public bool physicalMove(thisType = null) {  return true;  }
	public bool specialMove(thisType = null) {   return false; }

	public void EffectAfterAllHits(user, target) {
		if (target.damageState.unaffected) return;
		user.ReduceHP((int)Math.Round(user.totalhp / 4.0), false);
		@battle.Display(_INTL("{1} is damaged by recoil!", user.ToString()));
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Raise one of user's stats.
//===============================================================================
public partial class Battle.Move.StatUpMove : Battle.Move {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		return !user.CanRaiseStatStage(@statUp[0], user, self, true);
	}

	public void EffectGeneral(user) {
		if (damagingMove()) return;
		user.RaiseStatStage(@statUp[0], @statUp[1], user);
	}

	public void AdditionalEffect(user, target) {
		if (user.CanRaiseStatStage(@statUp[0], user, self)) {
			user.RaiseStatStage(@statUp[0], @statUp[1], user);
		}
	}
}

//===============================================================================
// Raise multiple of user's stats.
//===============================================================================
public partial class Battle.Move.MultiStatUpMove : Battle.Move {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		failed = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("{1}'s stats won't go any higher!", user.ToString()));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (damagingMove()) return;
		showAnim = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			if (user.RaiseStatStage(@statUp[i * 2], @statUp[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
	}

	public void AdditionalEffect(user, target) {
		showAnim = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			if (user.RaiseStatStage(@statUp[i * 2], @statUp[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
	}
}

//===============================================================================
// Lower multiple of user's stats.
//===============================================================================
public partial class Battle.Move.StatDownMove : Battle.Move {
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public void OnStartUse(user, targets) {
		@stats_lowered = false;
	}

	public void EffectWhenDealingDamage(user, target) {
		if (@stats_lowered) return;
		if (@battle.AllFainted(target.idxOwnSide)) return;
		@stats_lowered = true;
		showAnim = true;
		for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
			if (!user.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
			if (user.LowerStatStage(@statDown[i * 2], @statDown[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
	}
}

//===============================================================================
// Lower one of target's stats.
//===============================================================================
public partial class Battle.Move.TargetStatDownMove : Battle.Move {
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanLowerStatStage(@statDown[0], user, self, show_message);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.LowerStatStage(@statDown[0], @statDown[1], user);
	}

	public void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		if (!target.CanLowerStatStage(@statDown[0], user, self)) return;
		target.LowerStatStage(@statDown[0], @statDown[1], user);
	}
}

//===============================================================================
// Lower multiple of target's stats.
//===============================================================================
public partial class Battle.Move.TargetMultiStatDownMove : Battle.Move {
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		failed = true;
		for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
			if (!target.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			// NOTE: It's a bit of a faff to make sure the appropriate failure message
			//       is shown here, I know.
			canLower = false;
			if (target.hasActiveAbility(Abilitys.CONTRARY) && !target.beingMoldBroken()) {
				for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
					if (target.statStageAtMax(@statDown[i * 2])) continue;
					canLower = true;
					break;
				}
				if (!canLower && show_message) @battle.Display(_INTL("{1}'s stats won't go any higher!", user.ToString()));
			} else {
				for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
					if (target.statStageAtMin(@statDown[i * 2])) continue;
					canLower = true;
					break;
				}
				if (!canLower && show_message) @battle.Display(_INTL("{1}'s stats won't go any lower!", user.ToString()));
			}
			if (canLower) {
				target.CanLowerStatStage(@statDown[0], user, self, show_message);
			}
			return true;
		}
		return false;
	}

	public void CheckForMirrorArmor(user, target) {
		if (target.hasActiveAbility(Abilitys.MIRRORARMOR) && user.index != target.index) {
			failed = true;
			for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
				if (target.statStageAtMin(@statDown[i * 2])) continue;
				if (!user.CanLowerStatStage(@statDown[i * 2], target, self, false, false, true)) continue;
				failed = false;
				break;
			}
			if (failed) {
				@battle.ShowAbilitySplash(target);
				if (!Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1}'s {2} activated!", target.ToString(), target.abilityName));
				}
				user.CanLowerStatStage(@statDown[0], target, self, true, false, true);   // Show fail message
				@battle.HideAbilitySplash(target);
				return false;
			}
		}
		return true;
	}

	public void LowerTargetMultipleStats(user, target) {
		if (!CheckForMirrorArmor(user, target)) return;
		showAnim = true;
		showMirrorArmorSplash = true;
		for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
			if (!target.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
			if ((target.LowerStatStage(@statDown[i * 2], @statDown[(i * 2) + 1], user,
																showAnim, false, (showMirrorArmorSplash) ? 1 : 3)) {
				showAnim = false;
			}
			showMirrorArmorSplash = false;
		}
		@battle.HideAbilitySplash(target);   // To hide target's Mirror Armor splash
	}

	public void EffectAgainstTarget(user, target) {
		if (!damagingMove()) LowerTargetMultipleStats(user, target);
	}

	public void AdditionalEffect(user, target) {
		if (!target.damageState.substitute) LowerTargetMultipleStats(user, target);
	}
}

//===============================================================================
// Fixed damage-inflicting move.
//===============================================================================
public partial class Battle.Move.FixedDamageMove : Battle.Move {
	public void FixedDamage(user, target) {return 1; }

	public void CalcDamage(user, target, numTargets = 1) {
		target.damageState.critical   = false;
		target.damageState.calcDamage = FixedDamage(user, target);
		if (target.damageState.calcDamage < 1) target.damageState.calcDamage = 1;
	}
}

//===============================================================================
// Two turn move.
//===============================================================================
public partial class Battle.Move.TwoTurnMove : Battle.Move {
	public int chargingTurn		{ get { return _chargingTurn; } }			protected int _chargingTurn;

	public bool chargingTurnMove() { return true; }

	// user.effects.TwoTurnAttack is set to the move's ID if this
	// method returns true, or null if false.
	// Non-null means the charging turn. null means the attacking turn.
	public bool IsChargingTurn(user) {
		@powerHerb = false;
		@chargingTurn = false;   // Assume damaging turn by default
		@damagingTurn = true;
		// null at start of charging turn, move's ID at start of damaging turn
		if (!user.effects.TwoTurnAttack) {
			@powerHerb = user.hasActiveItem(Items.POWERHERB);
			@chargingTurn = true;
			@damagingTurn = @powerHerb;
		}
		return !@damagingTurn;   // Deliberately not "return @chargingTurn"
	}

	// Stops damage being dealt in the first (charging) turn.
	public override bool DamagingMove() {
		if (!@damagingTurn) return false;
		return base.DamagingMove();
	}

	// Does the charging part of this move, for when this move only takes one round
	// to use.
	public void QuickChargingMove(user, targets) {
		if (!@chargingTurn || !@damagingTurn) return;   // Move only takes one turn to use
		ChargingTurnMessage(user, targets);
		ShowAnimation(@id, user, targets, 1);   // Charging anim
		targets.each(b => ChargingTurnEffect(user, b));
		if (@powerHerb) {
			// Moves that would make the user semi-invulnerable will hide the user
			// after the charging animation, so the "UseItem" animation shouldn't show
			// for it
			if ((!new []{"TwoTurnAttackInvulnerableInSky",
					"TwoTurnAttackInvulnerableUnderground",
					"TwoTurnAttackInvulnerableUnderwater",
					"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
					"TwoTurnAttackInvulnerableRemoveProtections",
					"TwoTurnAttackInvulnerableInSkyTargetCannotAct"}.Contains(@function_code)) {
				@battle.CommonAnimation("UseItem", user);
			}
			@battle.Display(_INTL("{1} became fully charged due to its Power Herb!", user.ToString()));
			user.ConsumeItem;
		}
	}

	public override void AccuracyCheck(user, target) {
		if (!@damagingTurn) return true;
		return base.AccuracyCheck();
	}

	public void InitialEffect(user, targets, hitNum) {
		if (@damagingTurn) {
			AttackingTurnMessage(user, targets);
		} else if (@chargingTurn) {
			ChargingTurnMessage(user, targets);
		}
	}

	public void ChargingTurnMessage(user, targets) {
		@battle.Display(_INTL("{1} began charging up!", user.ToString()));
	}

	public void AttackingTurnMessage(user, targets) {}

	public void EffectAgainstTarget(user, target) {
		if (@damagingTurn) {
			AttackingTurnEffect(user, target);
		} else if (@chargingTurn) {
			ChargingTurnEffect(user, target);
		}
	}

	public void ChargingTurnEffect(user, target) {
		// Skull Bash/Sky Drop are the only two-turn moves with an effect here, and
		// the latter just records the target is being Sky Dropped
	}

	public void AttackingTurnEffect(user, target) {}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@chargingTurn && !@damagingTurn) hitNum = 1;   // Charging anim
		base.ShowAnimation();
	}
}

//===============================================================================
// Healing move.
//===============================================================================
public partial class Battle.Move.HealingMove : Battle.Move {
	public bool healingMove() {       return true; }
	public void HealAmount(user) {return 1;    }
	public bool canSnatch() {         return true; }

	public bool MoveFailed(user, targets) {
		if (user.hp == user.totalhp) {
			@battle.Display(_INTL("{1}'s HP is full!", user.ToString()));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		amt = HealAmount(user);
		user.RecoverHP(amt);
		@battle.Display(_INTL("{1}'s HP was restored.", user.ToString()));
	}
}

//===============================================================================
// Recoil move.
//===============================================================================
public partial class Battle.Move.RecoilMove : Battle.Move {
	public bool recoilMove() {                  return true; }
	public void RecoilDamage(user, target) {return 1;    }

	public void EffectAfterAllHits(user, target) {
		if (target.damageState.unaffected) return;
		if (!user.takesIndirectDamage()) return;
		if (user.hasActiveAbility(Abilitys.ROCKHEAD)) return;
		amt = RecoilDamage(user, target);
		if (amt < 1) amt = 1;
		if (user.pokemon.isSpecies(Speciess.BASCULIN) && new []{2, 3}.Contains(user.pokemon.form)) {
			user.pokemon.evolution_counter += amt;
		}
		user.ReduceHP(amt, false);
		@battle.Display(_INTL("{1} is damaged by recoil!", user.ToString()));
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Protect move.
//===============================================================================
public partial class Battle.Move.ProtectMove : Battle.Move {
	public override void initialize(battle, move) {
		base.initialize();
		@sidedEffect = false;
	}

	public override void ChangeUsageCounters(user, specialUsage) {
		oldVal = user.effects.ProtectRate;
		base.ChangeUsageCounters();
		user.effects.ProtectRate = oldVal;
	}

	public bool MoveFailed(user, targets) {
		if (@sidedEffect) {
			if (user.OwnSide.effects[@effect]) {
				user.effects.ProtectRate = 1;
				@battle.Display(_INTL("But it failed!"));
				return true;
			}
		} else if (user.effects[@effect]) {
			user.effects.ProtectRate = 1;
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if ((!@sidedEffect || Settings.MECHANICS_GENERATION <= 5) &&
			user.effects.ProtectRate > 1 &&
			@battle.Random(user.effects.ProtectRate) != 0) {
			user.effects.ProtectRate = 1;
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedLastInRound(user)) {
			user.effects.ProtectRate = 1;
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (@sidedEffect) {
			user.OwnSide.effects[@effect] = true;
		} else {
			user.effects[@effect] = true;
		}
		user.effects.ProtectRate *= (Settings.MECHANICS_GENERATION >= 6) ? 3 : 2;
		ProtectMessage(user);
	}

	public void ProtectMessage(user) {
		if (@sidedEffect) {
			@battle.Display(_INTL("{1} protected {2}!", @name, user.Team(true)));
		} else {
			@battle.Display(_INTL("{1} protected itself!", user.ToString()));
		}
	}
}

//===============================================================================
// Weather-inducing move.
//===============================================================================
public partial class Battle.Move.WeatherMove : Battle.Move {
	public int weatherType		{ get { return _weatherType; } }			protected int _weatherType;

	public override void initialize(battle, move) {
		base.initialize();
		@weatherType = Types.None;
	}

	public bool MoveFailed(user, targets) {
		switch (@battle.field.weather) {
			case :HarshSun:
				@battle.Display(_INTL("The extremely harsh sunlight was not lessened at all!"));
				return true;
			case :HeavyRain:
				@battle.Display(_INTL("There is no relief from this heavy rain!"));
				return true;
			case :StrongWinds:
				@battle.Display(_INTL("The mysterious air current blows on regardless!"));
				return true;
			case @weatherType:
				@battle.Display(_INTL("But it failed!"));
				return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.StartWeather(user, @weatherType, true, false);
	}
}

//===============================================================================
// Pledge move.
//===============================================================================
public partial class Battle.Move.PledgeMove : Battle.Move {
	public void OnStartUse(user, targets) {
		@pledgeSetup = false;
		@pledgeCombo = false;
		@pledgeOtherUser = null;
		@comboEffect = null;
		@overrideAnim = null;
		// Check whether this is the use of a combo move
		@combos.each do |i|
			if (i[0] != user.effects.FirstPledge) continue;
			@battle.Display(_INTL("The two moves have become one! It's a combined move!"));
			@pledgeCombo = true;
			@comboEffect = i[1];
			@overrideAnim = i[3];
			break;
		}
		if (@pledgeCombo) return;
		// Check whether this is the setup of a combo move
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (@battle.choices[b.index].Action != :UseMove || b.movedThisRound()) continue;
			move = @battle.choices[b.index].Move;
			if (!move) continue;
			@combos.each do |i|
				if (i[0] != move.function_code) continue;
				@pledgeSetup = true;
				@pledgeOtherUser = b;
				break;
			}
			if (@pledgeSetup) break;
		}
	}

	public override bool DamagingMove() {
		if (@pledgeSetup) return false;
		return base.DamagingMove();
	}

	public void BaseType(user) {
		// This method is called before OnStartUse, so it has to calculate the type
		// separately
		@combos.each do |i|
			if (i[0] != user.effects.FirstPledge) continue;
			if (!GameData.Type.exists(i[2])) continue;
			return i[2];
		}
		return super;
	}

	public void BaseDamage(baseDmg, user, target) {
		if (@pledgeCombo) baseDmg *= 2;
		return baseDmg;
	}

	public void EffectGeneral(user) {
		user.effects.FirstPledge = null;
		if (!@pledgeSetup) return;
		@battle.Display(_INTL("{1} is waiting for {2}'s move...",
														user.ToString(), @pledgeOtherUser.ToString(true)));
		@pledgeOtherUser.effects.FirstPledge = @function_code;
		@pledgeOtherUser.effects.MoveNext    = true;
		user.lastMoveFailed = true;   // Treated as a failure for Stomping Tantrum
	}

	public void EffectAfterAllHits(user, target) {
		if (!@pledgeCombo) return;
		msg = null;
		animName = null;
		switch (@comboEffect) {
			case :SeaOfFire:   // Grass + Fire
				if (user.OpposingSide.effects.SeaOfFire == 0) {
					user.OpposingSide.effects.SeaOfFire = 4;
					msg = _INTL("A sea of fire enveloped {1}!", user.OpposingTeam(true));
					animName = (user.opposes()) ? "SeaOfFire" : "SeaOfFireOpp";
				}
				break;
			case :Rainbow:   // Fire + Water
				if (user.OwnSide.effects.Rainbow == 0) {
					user.OwnSide.effects.Rainbow = 4;
					msg = _INTL("A rainbow appeared in the sky on {1}'s side!", user.Team(true));
					animName = (user.opposes()) ? "RainbowOpp" : "Rainbow";
				}
				break;
			case :Swamp:   // Water + Grass
				if (user.OpposingSide.effects.Swamp == 0) {
					user.OpposingSide.effects.Swamp = 4;
					msg = _INTL("A swamp enveloped {1}!", user.OpposingTeam(true));
					animName = (user.opposes()) ? "Swamp" : "SwampOpp";
				}
				break;
		}
		if (msg) @battle.Display(msg);
		if (animName) @battle.CommonAnimation(animName);
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@pledgeSetup) return;   // No animation for setting up
		if (@overrideAnim) id = @overrideAnim;
		return base.ShowAnimation();
	}
}
