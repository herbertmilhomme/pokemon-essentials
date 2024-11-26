//===============================================================================
// Increases the user's Attack by 1 stage.
//===============================================================================
public partial class Battle.Move.RaiseUserAttack1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1};
	}
}

//===============================================================================
// Increases the user's Attack by 2 stages. (Swords Dance)
//===============================================================================
public partial class Battle.Move.RaiseUserAttack2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 2};
	}
}

//===============================================================================
// If this move KO's the target, increases the user's Attack by 2 stages.
// (Fell Stinger (Gen 6-))
//===============================================================================
public partial class Battle.Move.RaiseUserAttack2IfTargetFaints : Battle.Move {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 2};
	}

	public void EffectAfterAllHits(user, target) {
		if (!target.damageState.fainted) return;
		if (!user.CanRaiseStatStage(@statUp[0], user, self)) return;
		user.RaiseStatStage(@statUp[0], @statUp[1], user);
	}
}

//===============================================================================
// Increases the user's Attack by 3 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserAttack3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 3};
	}
}

//===============================================================================
// If this move KO's the target, increases the user's Attack by 3 stages.
// (Fell Stinger (Gen 7+))
//===============================================================================
public partial class Battle.Move.RaiseUserAttack3IfTargetFaints : Battle.Move {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 3};
	}

	public void EffectAfterAllHits(user, target) {
		if (!target.damageState.fainted) return;
		if (!user.CanRaiseStatStage(@statUp[0], user, self)) return;
		user.RaiseStatStage(@statUp[0], @statUp[1], user);
	}
}

//===============================================================================
// Reduces the user's HP by half of max, and sets its Attack to maximum.
// (Belly Drum)
//===============================================================================
public partial class Battle.Move.MaxUserAttackLoseHalfOfTotalHP : Battle.Move {
	public int statUp		{ get { return _statUp; } }			protected int _statUp;

	public bool canSnatch() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 12};
	}

	public bool MoveFailed(user, targets) {
		hpLoss = (int)Math.Max(user.totalhp / 2, 1);
		if (user.hp <= hpLoss) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (!user.CanRaiseStatStage(@statUp[0], user, self, true)) return true;
		return false;
	}

	public void EffectGeneral(user) {
		hpLoss = (int)Math.Max(user.totalhp / 2, 1);
		user.ReduceHP(hpLoss, false, false);
		if (user.hasActiveAbility(Abilitys.CONTRARY)) {
			user.stages[@statUp[0]] = -Battle.Battler.STAT_STAGE_MAXIMUM;
			user.statsLoweredThisRound = true;
			user.statsDropped = true;
			@battle.CommonAnimation("StatDown", user);
			@battle.Display(_INTL("{1} cut its own HP and minimized its {2}!",
				user.ToString(), GameData.Stat.get(@statUp[0]).name));
		} else {
			user.stages[@statUp[0]] = Battle.Battler.STAT_STAGE_MAXIMUM;
			user.statsRaisedThisRound = true;
			@battle.CommonAnimation("StatUp", user);
			@battle.Display(_INTL("{1} cut its own HP and maximized its {2}!",
				user.ToString(), GameData.Stat.get(@statUp[0]).name));
		}
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Reduces the user's HP by half of max, and raises its Attack, Special Attack
// and Speed by 2 stages each. (Fillet Away)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkSpAtkSpeed2LoseHalfOfTotalHP : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 2, :SPECIAL_ATTACK, 2, :SPEED, 2};
	}

	public bool MoveFailed(user, targets) {
		hpLoss = (int)Math.Max(user.totalhp / 2, 1);
		if (user.hp <= hpLoss) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return super;
	}

	public override void EffectGeneral(user) {
		base.EffectGeneral();
		hpLoss = (int)Math.Max(user.totalhp / 2, 1);
		user.ReduceHP(hpLoss, false, false);
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Increases the user's Defense by 1 stage. (Harden, Steel Wing, Withdraw)
//===============================================================================
public partial class Battle.Move.RaiseUserDefense1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 1};
	}
}

//===============================================================================
// Increases the user's Defense by 1 stage. User curls up. (Defense Curl)
//===============================================================================
public partial class Battle.Move.RaiseUserDefense1CurlUpUser : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 1};
	}

	public override void EffectGeneral(user) {
		user.effects.DefenseCurl = true;
		base.EffectGeneral();
	}
}

//===============================================================================
// Increases the user's Defense by 2 stages. (Acid Armor, Barrier, Iron Defense)
//===============================================================================
public partial class Battle.Move.RaiseUserDefense2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 2};
	}
}

//===============================================================================
// Increases the user's Defense by 3 stages. (Cotton Guard)
//===============================================================================
public partial class Battle.Move.RaiseUserDefense3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 3};
	}
}

//===============================================================================
// Increases the user's Special Attack by 1 stage. (Charge Beam, Fiery Dance)
//===============================================================================
public partial class Battle.Move.RaiseUserSpAtk1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 1};
	}
}

//===============================================================================
// Increases the user's Special Attack by 2 stages. (Nasty Plot)
//===============================================================================
public partial class Battle.Move.RaiseUserSpAtk2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 2};
	}
}

//===============================================================================
// Increases the user's Special Attack by 3 stages. (Tail Glow)
//===============================================================================
public partial class Battle.Move.RaiseUserSpAtk3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 3};
	}
}

//===============================================================================
// Increases the user's Special Defense by 1 stage.
//===============================================================================
public partial class Battle.Move.RaiseUserSpDef1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Increases the user's Special Defense by 1 stage.
// Charges up user's next attack if it is Electric-type. (Charge)
//===============================================================================
public partial class Battle.Move.RaiseUserSpDef1PowerUpElectricMove : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_DEFENSE, 1};
	}

	public void EffectGeneral(user) {
		user.effects.Charge = 2;
		@battle.Display(_INTL("{1} began charging power!", user.ToString()));
		super;
	}
}

//===============================================================================
// Increases the user's Special Defense by 2 stages. (Amnesia)
//===============================================================================
public partial class Battle.Move.RaiseUserSpDef2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_DEFENSE, 2};
	}
}

//===============================================================================
// Increases the user's Special Defense by 3 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserSpDef3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_DEFENSE, 3};
	}
}

//===============================================================================
// Increases the user's Speed by 1 stage. (Flame Charge)
//===============================================================================
public partial class Battle.Move.RaiseUserSpeed1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPEED, 1};
	}
}

//===============================================================================
// Increases the user's Speed by 2 stages. (Agility, Rock Polish)
//===============================================================================
public partial class Battle.Move.RaiseUserSpeed2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPEED, 2};
	}
}

//===============================================================================
// Increases the user's Speed by 2 stages. Lowers user's weight by 100kg.
// (Autotomize)
//===============================================================================
public partial class Battle.Move.RaiseUserSpeed2LowerUserWeight : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPEED, 2};
	}

	public void EffectGeneral(user) {
		if (user.Weight + user.effects.WeightChange > 1) {
			user.effects.WeightChange -= 1000;
			@battle.Display(_INTL("{1} became nimble!", user.ToString()));
		}
		super;
	}
}

//===============================================================================
// Increases the user's Speed by 3 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserSpeed3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPEED, 3};
	}
}

//===============================================================================
// Increases the user's accuracy by 1 stage.
//===============================================================================
public partial class Battle.Move.RaiseUserAccuracy1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ACCURACY, 1};
	}
}

//===============================================================================
// Increases the user's accuracy by 2 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserAccuracy2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ACCURACY, 2};
	}
}

//===============================================================================
// Increases the user's accuracy by 3 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserAccuracy3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ACCURACY, 3};
	}
}

//===============================================================================
// Increases the user's evasion by 1 stage. (Double Team)
//===============================================================================
public partial class Battle.Move.RaiseUserEvasion1 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:EVASION, 1};
	}
}

//===============================================================================
// Increases the user's evasion by 2 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserEvasion2 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:EVASION, 2};
	}
}

//===============================================================================
// Increases the user's evasion by 2 stages. Minimizes the user. (Minimize)
//===============================================================================
public partial class Battle.Move.RaiseUserEvasion2MinimizeUser : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:EVASION, 2};
	}

	public override void EffectGeneral(user) {
		user.effects.Minimize = true;
		base.EffectGeneral();
	}
}

//===============================================================================
// Increases the user's evasion by 3 stages.
//===============================================================================
public partial class Battle.Move.RaiseUserEvasion3 : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:EVASION, 3};
	}
}

//===============================================================================
// Increases the user's critical hit rate. (Focus Energy)
//===============================================================================
public partial class Battle.Move.RaiseUserCriticalHitRate2 : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.FocusEnergy >= 2) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.FocusEnergy = 2;
		@battle.Display(_INTL("{1} is getting pumped!", user.ToString()));
	}
}

//===============================================================================
// Increases the user's Attack and Defense by 1 stage each. (Bulk Up)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkDef1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :DEFENSE, 1};
	}
}

//===============================================================================
// Increases the user's Attack, Defense and Speed by 1 stage each. (Victory Dance)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkDefSpd1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :DEFENSE, 1, :SPEED, 1};
	}
}

//===============================================================================
// Increases the user's Attack, Defense and accuracy by 1 stage each. (Coil)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkDefAcc1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :DEFENSE, 1, :ACCURACY, 1};
	}
}

//===============================================================================
// Increases the user's Attack and Special Attack by 1 stage each. (Work Up)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkSpAtk1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :SPECIAL_ATTACK, 1};
	}
}

//===============================================================================
// Increases the user's Attack and Sp. Attack by 1 stage each.
// In sunny weather, increases are 2 stages each instead. (Growth)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkSpAtk1Or2InSun : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :SPECIAL_ATTACK, 1};
	}

	public void OnStartUse(user, targets) {
		increment = 1;
		if (new []{:Sun, :HarshSun}.Contains(user.effectiveWeather)) increment = 2;
		@statUp[1] = @statUp[3] = increment;
	}
}

//===============================================================================
// Decreases the user's Defense and Special Defense by 1 stage each.
// Increases the user's Attack, Speed and Special Attack by 2 stages each.
// (Shell Smash)
//===============================================================================
public partial class Battle.Move.LowerUserDefSpDef1RaiseUserAtkSpAtkSpd2 : Battle.Move {
	public int statUp		{ get { return _statUp; } set { _statUp = value; } }			protected int _statUp;
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public bool canSnatch() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statUp   = new {:ATTACK, 2, :SPECIAL_ATTACK, 2, :SPEED, 2};
		@statDown = new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1};
	}

	public bool MoveFailed(user, targets) {
		failed = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
			if (!user.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("{1}'s stats can't be changed further!", user.ToString()));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		showAnim = true;
		for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
			if (!user.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
			if (user.LowerStatStage(@statDown[i * 2], @statDown[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
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
// Increases the user's Attack and Speed by 1 stage each. (Dragon Dance)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkSpd1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :SPEED, 1};
	}
}

//===============================================================================
// Removes trapping moves, entry hazards and Leech Seed on user/user's side.
// Poisons the target. (Mortal Spin)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkSpd1RemoveEntryHazardsAndSubstitutes : Battle.Move.RaiseUserAtkSpd1 {
	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		failed = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		@battle.allBattlers.each do |b|
			if (b.effects.Substitute > 0) failed = false;
		}
		if (user.OwnSide.effects.StealthRock ||
											user.OwnSide.effects.Spikes > 0 ||
											user.OwnSide.effects.ToxicSpikes > 0 ||
											user.OwnSide.effects.StickyWeb) failed = false;
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		something_tidied = false;
		@battle.allBattlers.each do |b|
			if (b.effects.Substitute == 0) continue;
			b.effects.Substitute = 0;
			something_tidied = true;
			@battle.Display(_INTL("{1}'s substitute faded!", b.ToString()));
		}
		if (user.OwnSide.effects.StealthRock) {
			user.OwnSide.effects.StealthRock = false;
			something_tidied = true;
			@battle.Display(_INTL("The pointed stones disappeared from around {1}!", user.Team(true)));
		}
		if (user.OpposingSide.effects.StealthRock) {
			user.OpposingSide.effects.StealthRock = false;
			something_tidied = true;
			@battle.Display(_INTL("The pointed stones disappeared from around {1}!", user.OpposingTeam(true)));
		}
		if (user.OwnSide.effects.Spikes > 0) {
			user.OwnSide.effects.Spikes = 0;
			something_tidied = true;
			@battle.Display(_INTL("The spikes disappeared from the ground around {1}!", user.Team(true)));
		}
		if (user.OpposingSide.effects.Spikes > 0) {
			user.OpposingSide.effects.Spikes = 0;
			something_tidied = true;
			@battle.Display(_INTL("The spikes disappeared from the ground around {1}!", user.OpposingSideTeam(true)));
		}
		if (user.OwnSide.effects.ToxicSpikes > 0) {
			user.OwnSide.effects.ToxicSpikes = 0;
			something_tidied = true;
			@battle.Display(_INTL("The poison spikes disappeared from the ground around {1}!", user.Team(true)));
		}
		if (user.OpposingSide.effects.ToxicSpikes > 0) {
			user.OpposingSide.effects.ToxicSpikes = 0;
			something_tidied = true;
			@battle.Display(_INTL("The poison spikes disappeared from the ground around {1}!", user.OpposingSideTeam(true)));
		}
		if (user.OwnSide.effects.StickyWeb) {
			user.OwnSide.effects.StickyWeb = false;
			something_tidied = true;
			@battle.Display(_INTL("The sticky webs disappeared from the ground around {1}!", user.Team(true)));
		}
		if (user.OpposingSide.effects.StickyWeb) {
			user.OpposingSide.effects.StickyWeb = false;
			something_tidied = true;
			@battle.Display(_INTL("The sticky webs disappeared from the ground around {1}!", user.OpposingSideTeam(true)));
		}
		if (something_tidied) @battle.Display(_INTL("Tidying up complete!"));
		super;
	}
}

//===============================================================================
// Increases the user's Speed by 2 stages, and its Attack by 1 stage. (Shift Gear)
//===============================================================================
public partial class Battle.Move.RaiseUserAtk1Spd2 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPEED, 2, :ATTACK, 1};
	}
}

//===============================================================================
// Increases the user's Attack and accuracy by 1 stage each. (Hone Claws)
//===============================================================================
public partial class Battle.Move.RaiseUserAtkAcc1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :ACCURACY, 1};
	}
}

//===============================================================================
// Increases the user's Defense and Special Defense by 1 stage each.
// (Cosmic Power, Defend Order)
//===============================================================================
public partial class Battle.Move.RaiseUserDefSpDef1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Increases the user's Sp. Attack and Sp. Defense by 1 stage each. (Calm Mind)
//===============================================================================
public partial class Battle.Move.RaiseUserSpAtkSpDef1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 1, :SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Increases the user's Sp. Attack and Sp. Defense by 1 stage each. Cures the
// user's status condition. (Take Heart)
//===============================================================================
public partial class Battle.Move.RaiseUserSpAtkSpDef1CureStatus : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 1, :SPECIAL_DEFENSE, 1};
	}

	public bool MoveFailed(user, targets) {
		failed = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!user.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			failed = false;
			break;
		}
		if (user.HasAnyStatus()) failed = false;
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public override void EffectGeneral(user) {
		base.EffectGeneral();
		user.CureStatus;
	}
}

//===============================================================================
// Increases the user's Sp. Attack, Sp. Defense and Speed by 1 stage each.
// (Quiver Dance)
//===============================================================================
public partial class Battle.Move.RaiseUserSpAtkSpDefSpd1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPECIAL_ATTACK, 1, :SPECIAL_DEFENSE, 1, :SPEED, 1};
	}
}

//===============================================================================
// Increases the user's Attack, Defense, Speed, Special Attack and Special Defense
// by 1 stage each. (Ancient Power, Ominous Wind, Silver Wind)
//===============================================================================
public partial class Battle.Move.RaiseUserMainStats1 : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:ATTACK, 1, :DEFENSE, 1, :SPECIAL_ATTACK, 1, :SPECIAL_DEFENSE, 1, :SPEED, 1};
	}
}

//===============================================================================
// Increases the user's Attack, Defense, Special Attack, Special Defense and
// Speed by 1 stage each, and reduces the user's HP by a third of its total HP.
// Fails if it can't do either effect. (Clangorous Soul)
//===============================================================================
public partial class Battle.Move.RaiseUserMainStats1LoseThirdOfTotalHP : Battle.Move.MultiStatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {
			:ATTACK, 1,
			:DEFENSE, 1,
			:SPECIAL_ATTACK, 1,
			:SPECIAL_DEFENSE, 1,
			:SPEED, 1;
		}
	}

	public bool MoveFailed(user, targets) {
		if (user.hp <= (int)Math.Max(user.totalhp / 3, 1)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return super;
	}

	public override void EffectGeneral(user) {
		base.EffectGeneral();
		user.ReduceHP((int)Math.Max(user.totalhp / 3, 1), false);
		user.ItemHPHealCheck;
	}
}

//===============================================================================
// Increases the user's Attack, Defense, Speed, Special Attack and Special
// Defense by 1 stage each. The user cannot switch out or flee. Fails if the user
// is already affected by the second effect of this move, but can be used if the
// user is prevented from switching out or fleeing by another effect (in which
// case, the second effect of this move is not applied to the user). The user may
// still switch out if holding Shed Shell or Eject Button, or if affected by a
// Red Card. (No Retreat)
//===============================================================================
public partial class Battle.Move.RaiseUserMainStats1TrapUserInBattle : Battle.Move.RaiseUserMainStats1 {
	public bool MoveFailed(user, targets) {
		if (user.effects.NoRetreat) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return super;
	}

	public override void EffectGeneral(user) {
		base.EffectGeneral();
		if (!user.trappedInBattle()) {
			user.effects.NoRetreat = true;
			@battle.Display(_INTL("{1} can no longer escape because it used {2}!", user.ToString(), @name));
		}
	}
}

//===============================================================================
// User rages until the start of a round in which they don't use this move. (Rage)
// (Handled in Battler's ProcessMoveAgainstTarget): Ups rager's Attack by 1
// stage each time it loses HP due to a move.
//===============================================================================
public partial class Battle.Move.StartRaiseUserAtk1WhenDamaged : Battle.Move {
	public void EffectGeneral(user) {
		user.effects.Rage = true;
	}
}

//===============================================================================
// Decreases the user's Attack by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerUserAttack1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1};
	}
}

//===============================================================================
// Decreases the user's Attack by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerUserAttack2 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 2};
	}
}

//===============================================================================
// Decreases the user's Defense by 1 stage. (Clanging Scales)
//===============================================================================
public partial class Battle.Move.LowerUserDefense1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the user's Defense by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerUserDefense2 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 2};
	}
}

//===============================================================================
// Decreases the user's Special Attack by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerUserSpAtk1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_ATTACK, 1};
	}
}

//===============================================================================
// Decreases the user's Special Attack by 1 stage. Scatters coins that the player
// picks up after winning the battle. (Make It Rain)
//===============================================================================
public partial class Battle.Move.LowerUserSpAtk1 : Battle.Move.LowerUserSpAtk1 {
	public void EffectWhenDealingDamage(user, target) {
		if (@stats_lowered) return;
		if (user.OwnedByPlayer()) {
			@battle.field.effects.PayDay += 5 * user.level;
		}
		@battle.Display(_INTL("Coins were scattered everywhere!"));
		super;
	}
}

//===============================================================================
// Decreases the user's Special Attack by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerUserSpAtk2 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_ATTACK, 2};
	}
}

//===============================================================================
// Decreases the user's Special Defense by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerUserSpDef1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the user's Special Defense by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerUserSpDef2 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_DEFENSE, 2};
	}
}

//===============================================================================
// Decreases the user's Speed by 1 stage. (Hammer Arm, Ice Hammer)
//===============================================================================
public partial class Battle.Move.LowerUserSpeed1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 1};
	}
}

//===============================================================================
// Decreases the user's Speed by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerUserSpeed2 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 2};
	}
}

//===============================================================================
// Decreases the user's Attack and Defense by 1 stage each. (Superpower)
//===============================================================================
public partial class Battle.Move.LowerUserAtkDef1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1, :DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the user's Defense and Special Defense by 1 stage each.
// (Armor Cannon, Close Combat, Dragon Ascent, Headlong Rush)
//===============================================================================
public partial class Battle.Move.LowerUserDefSpDef1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 1, :SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the user's Defense, Special Defense and Speed by 1 stage each.
// (V-create)
//===============================================================================
public partial class Battle.Move.LowerUserDefSpDefSpd1 : Battle.Move.StatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 1, :DEFENSE, 1, :SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Increases the user's and allies' Attack by 1 stage. (Howl (Gen 8+))
//===============================================================================
public partial class Battle.Move.RaiseTargetAttack1 : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b.CanRaiseStatStage(:ATTACK, user, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (damagingMove()) return false;
		return !target.CanRaiseStatStage(:ATTACK, user, self, show_message);
	}

	public void EffectAgainstTarget(user, target) {
		if (damagingMove()) return;
		target.RaiseStatStage(:ATTACK, 1, user);
	}

	public void AdditionalEffect(user, target) {
		if (!target.CanRaiseStatStage(:ATTACK, user, self)) return;
		target.RaiseStatStage(:ATTACK, 1, user);
	}
}

//===============================================================================
// Increases the target's Attack by 2 stages. Decreases the target's Defense by 2
// stages. (Spicy Extract)
//===============================================================================
public partial class Battle.Move.RaiseTargetAtk2LowerTargetDef2 : Battle.Move {
	public int statUp		{ get { return _statUp; } set { _statUp = value; } }			protected int _statUp;
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public bool canMagicCoat() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statUp   = new {:ATTACK, 2};
		@statDown = new {:DEFENSE, 2};
	}

	public bool MoveFailed(user, targets) {
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
				if (!target.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
				failed = false;
				break;
			}
			if (!failed) break;
			for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
				if (!target.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
				failed = false;
				break;
			}
			if (!failed) break;
		}
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		showAnim = true;
		for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
			if (!target.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
			if (target.LowerStatStage(@statDown[i * 2], @statDown[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
		showAnim = true;
		for (int i = (@statUp.length / 2); i < (@statUp.length / 2); i++) { //for '(@statUp.length / 2)' times do => |i|
			if (!target.CanRaiseStatStage(@statUp[i * 2], user, self)) continue;
			if (target.RaiseStatStage(@statUp[i * 2], @statUp[(i * 2) + 1], user, showAnim)) {
				showAnim = false;
			}
		}
	}
}

//===============================================================================
// Increases the target's Attack by 2 stages. Confuses the target. (Swagger)
//===============================================================================
public partial class Battle.Move.RaiseTargetAttack2ConfuseTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b.CanRaiseStatStage(:ATTACK, user, self) &&
							!b.CanConfuse(user, false, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (target.CanRaiseStatStage(:ATTACK, user, self)) {
			target.RaiseStatStage(:ATTACK, 2, user);
		}
		if (target.CanConfuse(user, false, self)) target.Confuse;
	}
}

//===============================================================================
// Increases the target's Special Attack by 1 stage. Confuses the target. (Flatter)
//===============================================================================
public partial class Battle.Move.RaiseTargetSpAtk1ConfuseTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b.CanRaiseStatStage(:SPECIAL_ATTACK, user, self) &&
							!b.CanConfuse(user, false, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (target.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) {
			target.RaiseStatStage(:SPECIAL_ATTACK, 1, user);
		}
		if (target.CanConfuse(user, false, self)) target.Confuse;
	}
}

//===============================================================================
// Increases target's Special Defense by 1 stage. (Aromatic Mist)
//===============================================================================
public partial class Battle.Move.RaiseTargetSpDef1 : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.CanRaiseStatStage(:SPECIAL_DEFENSE, user, self, show_message)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.RaiseStatStage(:SPECIAL_DEFENSE, 1, user);
	}
}

//===============================================================================
// Increases one random stat of the target by 2 stages (except HP). (Acupressure)
//===============================================================================
public partial class Battle.Move.RaiseTargetRandomStat2 : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		@statArray = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (target.CanRaiseStatStage(s.id, user, self)) @statArray.Add(s.id);
		}
		if (@statArray.length == 0) {
			if (show_message) @battle.Display(_INTL("{1}'s stats won't go any higher!", target.ToString()));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		stat = @statArray[@battle.Random(@statArray.length)];
		target.RaiseStatStage(stat, 2, user);
	}
}

//===============================================================================
// Increases the target's Attack and Special Attack by 2 stages each. (Decorate)
//===============================================================================
public partial class Battle.Move.RaiseTargetAtkSpAtk2 : Battle.Move {
	public bool MoveFailed(user, targets) {
		failed = true;
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b.CanRaiseStatStage(:ATTACK, user, self) &&
							!b.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) continue;
			failed = false;
			break;
		}
		if (failed) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		showAnim = true;
		if (target.CanRaiseStatStage(:ATTACK, user, self)) {
			if (target.RaiseStatStage(:ATTACK, 2, user, showAnim)) showAnim = false;
		}
		if (target.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) {
			target.RaiseStatStage(:SPECIAL_ATTACK, 2, user, showAnim);
		}
	}
}

//===============================================================================
// Decreases the target's Attack by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerTargetAttack1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1};
	}
}

//===============================================================================
// Decreases the target's Attack by 1 stage. Bypasses target's Substitute. (Play Nice)
//===============================================================================
public partial class Battle.Move.LowerTargetAttack1BypassSubstitute : Battle.Move.TargetStatDownMove {
	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1};
	}
}

//===============================================================================
// Decreases the target's Attack by 2 stages. (Charm, Feather Dance)
//===============================================================================
public partial class Battle.Move.LowerTargetAttack2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 2};
	}
}

//===============================================================================
// Decreases the target's Attack by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetAttack3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 3};
	}
}

//===============================================================================
// Decreases the target's Defense by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerTargetDefense1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the target's Defense by 1 stage. Power is multiplied by 1.5 if
// Gravity is in effect. (Grav Apple)
//===============================================================================
public partial class Battle.Move.LowerTargetDefense1PowersUpInGravity : Battle.Move.LowerTargetDefense1 {
	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.effects.Gravity > 0) baseDmg = baseDmg * 3 / 2;
		return baseDmg;
	}
}

//===============================================================================
// 50% chance to decreases the target's Defense by 1 stage. 30% chance to make
// the target flinch. (Triple Arrows)
//===============================================================================
public partial class Battle.Move.LowerTargetDefense1FlinchTarget : Battle.Move.TargetStatDownMove {
	public bool flinchingMove() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 1};
	}

	public override void AdditionalEffect(user, target) {
		if (target.damageState.substitute) return;
		stat_chance = AdditionalEffectChance(user, target, 50);
		if (stat_chance > 0 && @battle.Random(100) < stat_chance) base.AdditionalEffect();
		flinch_chance = AdditionalEffectChance(user, target, 30);
		if (flinch_chance > 0 && @battle.Random(100) < flinch_chance) target.Flinch(user);
	}
}

//===============================================================================
// Decreases the target's Defense by 2 stages. (Screech)
//===============================================================================
public partial class Battle.Move.LowerTargetDefense2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 2};
	}
}

//===============================================================================
// Decreases the target's Defense by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetDefense3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:DEFENSE, 3};
	}
}

//===============================================================================
// Decreases the target's Special Attack by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerTargetSpAtk1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_ATTACK, 1};
	}
}

//===============================================================================
// Decreases the target's Special Attack by 2 stages. (Eerie Impulse)
//===============================================================================
public partial class Battle.Move.LowerTargetSpAtk2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_ATTACK, 2};
	}
}

//===============================================================================
// Decreases the target's Special Attack by 2 stages. Only works on the opposite
// gender. (Captivate)
//===============================================================================
public partial class Battle.Move.LowerTargetSpAtk2IfCanAttract : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_ATTACK, 2};
	}

	public override bool FailsAgainstTarget(user, target, show_message) {
		if (base.FailsAgainstTarget()) return true;
		if (damagingMove()) return false;
		if (user.gender == 2 || target.gender == 2 || user.gender == target.gender) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		if (target.hasActiveAbility(Abilitys.OBLIVIOUS) && !target.beingMoldBroken()) {
			if (show_message) {
				@battle.ShowAbilitySplash(target);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} is unaffected!", target.ToString()));
				} else {
					@battle.Display(_INTL("{1}'s {2} prevents romance!", target.ToString(), target.abilityName));
				}
				@battle.HideAbilitySplash(target);
			}
			return true;
		}
		return false;
	}

	public override void AdditionalEffect(user, target) {
		if (user.gender == 2 || target.gender == 2 || user.gender == target.gender) return;
		if (target.hasActiveAbility(Abilitys.OBLIVIOUS) && !target.beingMoldBroken()) return;
		base.AdditionalEffect();
	}
}

//===============================================================================
// Decreases the target's Special Attack by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetSpAtk3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_ATTACK, 3};
	}
}

//===============================================================================
// Decreases the target's Special Defense by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerTargetSpDef1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the target's Special Defense by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetSpDef2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_DEFENSE, 2};
	}
}

//===============================================================================
// Decreases the target's Special Defense by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetSpDef3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPECIAL_DEFENSE, 3};
	}
}

//===============================================================================
// Decreases the target's Speed by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerTargetSpeed1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 1};
	}
}

//===============================================================================
// Decreases the target's Speed by 1 stage. Accuracy perfect in rain.
// (Bleakwind Storm)
//===============================================================================
public partial class Battle.Move.LowerTargetSpeed1AlwaysHitsInRain : Battle.Move.LowerTargetSpeed1 {
	public void BaseAccuracy(user, target) {
		if (new []{:Rain, :HeavyRain}.Contains(target.effectiveWeather)) return 0;
		return super;
	}
}

//===============================================================================
// Decreases the target's Speed by 1 stage. Power is halved in Grassy Terrain.
// (Bulldoze)
//===============================================================================
public partial class Battle.Move.LowerTargetSpeed1WeakerInGrassyTerrain : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 1};
	}

	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.terrain == :Grassy) baseDmg = (int)Math.Round(baseDmg / 2.0);
		return baseDmg;
	}
}

//===============================================================================
// Decreases the target's Speed by 1 stage. Doubles the effectiveness of damaging
// Fire moves used against the target (this effect does not stack). Fails if
// neither of these effects can be applied. (Tar Shot)
//===============================================================================
public partial class Battle.Move.LowerTargetSpeed1MakeTargetWeakerToFire : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 1};
	}

	public override bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.TarShot) return base.FailsAgainstTarget();
		return false;
	}

	public override void EffectAgainstTarget(user, target) {
		base.EffectAgainstTarget();
		if (!target.effects.TarShot) {
			target.effects.TarShot = true;
			@battle.Display(_INTL("{1} became weaker to fire!", target.ToString()));
		}
	}
}

//===============================================================================
// Decreases the target's Speed by 2 stages. (Cotton Spore, Scary Face, String Shot)
//===============================================================================
public partial class Battle.Move.LowerTargetSpeed2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 2};
	}
}

//===============================================================================
// Decreases the target's Speed by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetSpeed3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:SPEED, 3};
	}
}

//===============================================================================
// Decreases the target's accuracy by 1 stage.
//===============================================================================
public partial class Battle.Move.LowerTargetAccuracy1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ACCURACY, 1};
	}
}

//===============================================================================
// Decreases the target's accuracy by 2 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetAccuracy2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ACCURACY, 2};
	}
}

//===============================================================================
// Decreases the target's accuracy by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetAccuracy3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ACCURACY, 3};
	}
}

//===============================================================================
// Decreases the target's evasion by 1 stage. (Sweet Scent (Gen 5-))
//===============================================================================
public partial class Battle.Move.LowerTargetEvasion1 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:EVASION, 1};
	}
}

//===============================================================================
// Decreases the target's evasion by 1 stage. Ends all barriers and entry
// hazards for the target's side OR on both sides. (Defog)
//===============================================================================
public partial class Battle.Move.LowerTargetEvasion1RemoveSideEffects : Battle.Move.TargetStatDownMove {
	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:EVASION, 1};
	}

	public override bool FailsAgainstTarget(user, target, show_message) {
		targetSide = target.OwnSide;
		targetOpposingSide = target.OpposingSide;
		if (targetSide.effects.AuroraVeil > 0 ||
										targetSide.effects.LightScreen > 0 ||
										targetSide.effects.Reflect > 0 ||
										targetSide.effects.Mist > 0 ||
										targetSide.effects.Safeguard > 0) return false;
		if (targetSide.effects.StealthRock ||
										targetSide.effects.Spikes > 0 ||
										targetSide.effects.ToxicSpikes > 0 ||
										targetSide.effects.StickyWeb) return false;
		if (Settings.MECHANICS_GENERATION >= 6 &&
										(targetOpposingSide.effects.StealthRock ||
										targetOpposingSide.effects.Spikes > 0 ||
										targetOpposingSide.effects.ToxicSpikes > 0 ||
										targetOpposingSide.effects.StickyWeb)) return false;
		if (Settings.MECHANICS_GENERATION >= 8 && @battle.field.terrain != :None) return false;
		return base.FailsAgainstTarget();
	}

	public void EffectAgainstTarget(user, target) {
		if (target.CanLowerStatStage(@statDown[0], user, self)) {
			target.LowerStatStage(@statDown[0], @statDown[1], user);
		}
		if (target.OwnSide.effects.AuroraVeil > 0) {
			target.OwnSide.effects.AuroraVeil = 0;
			@battle.Display(_INTL("{1}'s Aurora Veil wore off!", target.Team));
		}
		if (target.OwnSide.effects.LightScreen > 0) {
			target.OwnSide.effects.LightScreen = 0;
			@battle.Display(_INTL("{1}'s Light Screen wore off!", target.Team));
		}
		if (target.OwnSide.effects.Reflect > 0) {
			target.OwnSide.effects.Reflect = 0;
			@battle.Display(_INTL("{1}'s Reflect wore off!", target.Team));
		}
		if (target.OwnSide.effects.Mist > 0) {
			target.OwnSide.effects.Mist = 0;
			@battle.Display(_INTL("{1}'s Mist faded!", target.Team));
		}
		if (target.OwnSide.effects.Safeguard > 0) {
			target.OwnSide.effects.Safeguard = 0;
			@battle.Display(_INTL("{1} is no longer protected by Safeguard!!", target.Team));
		}
		if (target.OwnSide.effects.StealthRock ||
			(Settings.MECHANICS_GENERATION >= 6 &&
			target.OpposingSide.effects.StealthRock)) {
			target.OwnSide.effects.StealthRock      = false;
			if (Settings.MECHANICS_GENERATION >= 6) target.OpposingSide.effects.StealthRock = false;
			@battle.Display(_INTL("{1} blew away stealth rocks!", user.ToString()));
		}
		if (target.OwnSide.effects.Spikes > 0 ||
			(Settings.MECHANICS_GENERATION >= 6 &&
			target.OpposingSide.effects.Spikes > 0)) {
			target.OwnSide.effects.Spikes      = 0;
			if (Settings.MECHANICS_GENERATION >= 6) target.OpposingSide.effects.Spikes = 0;
			@battle.Display(_INTL("{1} blew away spikes!", user.ToString()));
		}
		if (target.OwnSide.effects.ToxicSpikes > 0 ||
			(Settings.MECHANICS_GENERATION >= 6 &&
			target.OpposingSide.effects.ToxicSpikes > 0)) {
			target.OwnSide.effects.ToxicSpikes      = 0;
			if (Settings.MECHANICS_GENERATION >= 6) target.OpposingSide.effects.ToxicSpikes = 0;
			@battle.Display(_INTL("{1} blew away poison spikes!", user.ToString()));
		}
		if (target.OwnSide.effects.StickyWeb ||
			(Settings.MECHANICS_GENERATION >= 6 &&
			target.OpposingSide.effects.StickyWeb)) {
			target.OwnSide.effects.StickyWeb      = false;
			if (Settings.MECHANICS_GENERATION >= 6) target.OpposingSide.effects.StickyWeb = false;
			@battle.Display(_INTL("{1} blew away sticky webs!", user.ToString()));
		}
		if (Settings.MECHANICS_GENERATION >= 8 && @battle.field.terrain != :None) {
			switch (@battle.field.terrain) {
				case :Electric:
					@battle.Display(_INTL("The electricity disappeared from the battlefield."));
					break;
				case :Grassy:
					@battle.Display(_INTL("The grass disappeared from the battlefield."));
					break;
				case :Misty:
					@battle.Display(_INTL("The mist disappeared from the battlefield."));
					break;
				case :Psychic:
					@battle.Display(_INTL("The weirdness disappeared from the battlefield."));
					break;
			}
			@battle.field.terrain = :None;
		}
	}
}

//===============================================================================
// Decreases the target's evasion by 2 stages. (Sweet Scent (Gen 6+))
//===============================================================================
public partial class Battle.Move.LowerTargetEvasion2 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:EVASION, 2};
	}
}

//===============================================================================
// Decreases the target's evasion by 3 stages.
//===============================================================================
public partial class Battle.Move.LowerTargetEvasion3 : Battle.Move.TargetStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:EVASION, 3};
	}
}

//===============================================================================
// Decreases the target's Attack and Defense by 1 stage each. (Tickle)
//===============================================================================
public partial class Battle.Move.LowerTargetAtkDef1 : Battle.Move.TargetMultiStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1, :DEFENSE, 1};
	}
}

//===============================================================================
// Decreases the target's Attack and Special Attack by 1 stage each. (Noble Roar)
//===============================================================================
public partial class Battle.Move.LowerTargetAtkSpAtk1 : Battle.Move.TargetMultiStatDownMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1, :SPECIAL_ATTACK, 1};
	}
}

//===============================================================================
// Decreases the Attack, Special Attack and Speed of all poisoned targets by 1
// stage each. (Venom Drench)
//===============================================================================
public partial class Battle.Move.LowerPoisonedTargetAtkSpAtkSpd1 : Battle.Move {
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public bool canMagicCoat() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statDown = new {:ATTACK, 1, :SPECIAL_ATTACK, 1, :SPEED, 1};
	}

	public bool MoveFailed(user, targets) {
		@validTargets = new List<string>();
		foreach (var b in targets) { //'targets.each' do => |b|
			if (!b || b.fainted()) continue;
			if (!b.poisoned()) continue;
			failed = true;
			for (int i = (@statDown.length / 2); i < (@statDown.length / 2); i++) { //for '(@statDown.length / 2)' times do => |i|
				if (!b.CanLowerStatStage(@statDown[i * 2], user, self)) continue;
				failed = false;
				break;
			}
			if (!failed) @validTargets.Add(b.index);
		}
		if (@validTargets.length == 0) {
			@battle.Display(_INTL("But it failed!"));
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

	public void EffectAgainstTarget(user, target) {
		if (!@validTargets.Contains(target.index)) return;
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
}

//===============================================================================
// Raises the Attack and Defense of all user's allies by 1 stage each. Bypasses
// protections, including Crafty Shield. Fails if there is no ally. (Coaching)
//===============================================================================
public partial class Battle.Move.RaiseAlliesAtkDef1 : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		@validTargets = new List<string>();
		@battle.allSameSideBattlers(user).each do |b|
			if (b.index == user.index) continue;
			if (!b.CanRaiseStatStage(:ATTACK, user, self) &&
							!b.CanRaiseStatStage(:DEFENSE, user, self)) continue;
			@validTargets.Add(b);
		}
		if (@validTargets.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@validTargets.any(b => b.index == target.index)) return false;
		if (show_message) @battle.Display(_INTL("{1}'s stats can't be raised further!", target.ToString()));
		return true;
	}

	public void EffectAgainstTarget(user, target) {
		showAnim = true;
		if (target.CanRaiseStatStage(:ATTACK, user, self)) {
			if (target.RaiseStatStage(:ATTACK, 1, user, showAnim)) showAnim = false;
		}
		if (target.CanRaiseStatStage(:DEFENSE, user, self)) {
			target.RaiseStatStage(:DEFENSE, 1, user, showAnim);
		}
	}
}

//===============================================================================
// Increases the user's and its ally's Attack and Special Attack by 1 stage each,
// if (they have Plus or Minus. (Gear Up)) {
//===============================================================================
// NOTE: In Gen 5, this move should have a target of UserSide, while in Gen 6+ it
//       should have a target of UserAndAllies. This is because, in Gen 5, this
//       move shouldn't call def SuccessCheckAgainstTarget for each Pokémon
//       currently in battle that will be affected by this move (i.e. allies
//       aren't protected by their substitute/ability/etc., but they are in Gen
//       6+). We achieve this by not targeting any battlers in Gen 5, since
//       SuccessCheckAgainstTarget is only called for targeted battlers.
public partial class Battle.Move.RaisePlusMinusUserAndAlliesAtkSpAtk1 : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canSnatch() {               return true; }

	public bool MoveFailed(user, targets) {
		@validTargets = new List<string>();
		@battle.allSameSideBattlers(user).each do |b|
			if (!b.hasActiveAbility(new {:MINUS, :PLUS})) continue;
			if (!b.CanRaiseStatStage(:ATTACK, user, self) &&
							!b.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) continue;
			@validTargets.Add(b);
		}
		if (@validTargets.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@validTargets.any(b => b.index == target.index)) return false;
		if (!target.hasActiveAbility(new {:MINUS, :PLUS})) return true;
		if (show_message) @battle.Display(_INTL("{1}'s stats can't be raised further!", target.ToString()));
		return true;
	}

	public void EffectAgainstTarget(user, target) {
		showAnim = true;
		if (target.CanRaiseStatStage(:ATTACK, user, self)) {
			if (target.RaiseStatStage(:ATTACK, 1, user, showAnim)) showAnim = false;
		}
		if (target.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) {
			target.RaiseStatStage(:SPECIAL_ATTACK, 1, user, showAnim);
		}
	}

	public void EffectGeneral(user) {
		if (Target(user) != :UserSide) return;
		@validTargets.each(b => EffectAgainstTarget(user, b));
	}
}

//===============================================================================
// Increases the user's and its ally's Defense and Special Defense by 1 stage
// each, if they have Plus or Minus. (Magnetic Flux)
//===============================================================================
// NOTE: In Gen 5, this move should have a target of UserSide, while in Gen 6+ it
//       should have a target of UserAndAllies. This is because, in Gen 5, this
//       move shouldn't call def SuccessCheckAgainstTarget for each Pokémon
//       currently in battle that will be affected by this move (i.e. allies
//       aren't protected by their substitute/ability/etc., but they are in Gen
//       6+). We achieve this by not targeting any battlers in Gen 5, since
//       SuccessCheckAgainstTarget is only called for targeted battlers.
public partial class Battle.Move.RaisePlusMinusUserAndAlliesDefSpDef1 : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		@validTargets = new List<string>();
		@battle.allSameSideBattlers(user).each do |b|
			if (!b.hasActiveAbility(new {:MINUS, :PLUS})) continue;
			if (!b.CanRaiseStatStage(:DEFENSE, user, self) &&
							!b.CanRaiseStatStage(:SPECIAL_DEFENSE, user, self)) continue;
			@validTargets.Add(b);
		}
		if (@validTargets.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@validTargets.any(b => b.index == target.index)) return false;
		if (!target.hasActiveAbility(new {:MINUS, :PLUS})) return true;
		if (show_message) @battle.Display(_INTL("{1}'s stats can't be raised further!", target.ToString()));
		return true;
	}

	public void EffectAgainstTarget(user, target) {
		showAnim = true;
		if (target.CanRaiseStatStage(:DEFENSE, user, self)) {
			if (target.RaiseStatStage(:DEFENSE, 1, user, showAnim)) showAnim = false;
		}
		if (target.CanRaiseStatStage(:SPECIAL_DEFENSE, user, self)) {
			target.RaiseStatStage(:SPECIAL_DEFENSE, 1, user, showAnim);
		}
	}

	public void EffectGeneral(user) {
		if (Target(user) != :UserSide) return;
		@validTargets.each(b => EffectAgainstTarget(user, b));
	}
}

//===============================================================================
// Increases the Attack and Special Attack of all Grass-type Pokémon in battle by
// 1 stage each. Doesn't affect airborne Pokémon. (Rototiller)
//===============================================================================
public partial class Battle.Move.RaiseGroundedGrassBattlersAtkSpAtk1 : Battle.Move {
	public bool MoveFailed(user, targets) {
		@validTargets = new List<string>();
		@battle.allBattlers.each do |b|
			if (!b.Type == Types.GRASS) continue;
			if (b.airborne() || b.semiInvulnerable()) continue;
			if (!b.CanRaiseStatStage(:ATTACK, user, self) &&
							!b.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) continue;
			@validTargets.Add(b.index);
		}
		if (@validTargets.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@validTargets.Contains(target.index)) return false;
		if (!target.Type == Types.GRASS) return true;
		if (target.airborne() || target.semiInvulnerable()) return true;
		if (show_message) @battle.Display(_INTL("{1}'s stats can't be raised further!", target.ToString()));
		return true;
	}

	public void EffectAgainstTarget(user, target) {
		showAnim = true;
		if (target.CanRaiseStatStage(:ATTACK, user, self)) {
			if (target.RaiseStatStage(:ATTACK, 1, user, showAnim)) showAnim = false;
		}
		if (target.CanRaiseStatStage(:SPECIAL_ATTACK, user, self)) {
			target.RaiseStatStage(:SPECIAL_ATTACK, 1, user, showAnim);
		}
	}
}

//===============================================================================
// Increases the Defense of all Grass-type Pokémon on the field by 1 stage each.
// (Flower Shield)
//===============================================================================
public partial class Battle.Move.RaiseGrassBattlersDef1 : Battle.Move {
	public bool MoveFailed(user, targets) {
		@validTargets = new List<string>();
		@battle.allBattlers.each do |b|
			if (!b.Type == Types.GRASS) continue;
			if (b.semiInvulnerable()) continue;
			if (!b.CanRaiseStatStage(:DEFENSE, user, self)) continue;
			@validTargets.Add(b.index);
		}
		if (@validTargets.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@validTargets.Contains(target.index)) return false;
		if (!target.Type == Types.GRASS || target.semiInvulnerable()) return true;
		return !target.CanRaiseStatStage(:DEFENSE, user, self, show_message);
	}

	public void EffectAgainstTarget(user, target) {
		target.RaiseStatStage(:DEFENSE, 1, user);
	}
}

//===============================================================================
// User and target swap their Attack and Special Attack stat stages. (Power Swap)
//===============================================================================
public partial class Battle.Move.UserTargetSwapAtkSpAtkStages : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public void EffectAgainstTarget(user, target) {
		new {:ATTACK, :SPECIAL_ATTACK}.each do |s|
			if (user.stages[s] > target.stages[s]) {
				user.statsLoweredThisRound = true;
				user.statsDropped = true;
				target.statsRaisedThisRound = true;
			} else if (user.stages[s] < target.stages[s]) {
				user.statsRaisedThisRound = true;
				target.statsLoweredThisRound = true;
				target.statsDropped = true;
			}
			user.stages[s], target.stages[s] = target.stages[s], user.stages[s];
		}
		@battle.Display(_INTL("{1} switched all changes to its Attack and Sp. Atk with the target!", user.ToString()));
	}
}

//===============================================================================
// User and target swap their Defense and Special Defense stat stages. (Guard Swap)
//===============================================================================
public partial class Battle.Move.UserTargetSwapDefSpDefStages : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public void EffectAgainstTarget(user, target) {
		new {:DEFENSE, :SPECIAL_DEFENSE}.each do |s|
			if (user.stages[s] > target.stages[s]) {
				user.statsLoweredThisRound = true;
				user.statsDropped = true;
				target.statsRaisedThisRound = true;
			} else if (user.stages[s] < target.stages[s]) {
				user.statsRaisedThisRound = true;
				target.statsLoweredThisRound = true;
				target.statsDropped = true;
			}
			user.stages[s], target.stages[s] = target.stages[s], user.stages[s];
		}
		@battle.Display(_INTL("{1} switched all changes to its Defense and Sp. Def with the target!", user.ToString()));
	}
}

//===============================================================================
// User and target swap all their stat stages. (Heart Swap)
//===============================================================================
public partial class Battle.Move.UserTargetSwapStatStages : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public void EffectAgainstTarget(user, target) {
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (user.stages[s.id] > target.stages[s.id]) {
				user.statsLoweredThisRound = true;
				user.statsDropped = true;
				target.statsRaisedThisRound = true;
			} else if (user.stages[s.id] < target.stages[s.id]) {
				user.statsRaisedThisRound = true;
				target.statsLoweredThisRound = true;
				target.statsDropped = true;
			}
			user.stages[s.id], target.stages[s.id] = target.stages[s.id], user.stages[s.id];
		}
		@battle.Display(_INTL("{1} switched stat changes with the target!", user.ToString()));
	}
}

//===============================================================================
// User copies the target's stat stages. (Psych Up)
//===============================================================================
public partial class Battle.Move.UserCopyTargetStatStages : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public void EffectAgainstTarget(user, target) {
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (user.stages[s.id] > target.stages[s.id]) {
				user.statsLoweredThisRound = true;
				user.statsDropped = true;
			} else if (user.stages[s.id] < target.stages[s.id]) {
				user.statsRaisedThisRound = true;
			}
			user.stages[s.id] = target.stages[s.id];
		}
		if (Settings.NEW_CRITICAL_HIT_RATE_MECHANICS) {
			user.effects.FocusEnergy = target.effects.FocusEnergy;
			user.effects.LaserFocus  = target.effects.LaserFocus;
		}
		@battle.Display(_INTL("{1} copied {2}'s stat changes!", user.ToString(), target.ToString(true)));
	}
}

//===============================================================================
// User gains stat stages equal to each of the target's positive stat stages,
// and target's positive stat stages become 0, before damage calculation.
// (Spectral Thief)
//===============================================================================
public partial class Battle.Move.UserStealTargetPositiveStatStages : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public void CalcDamage(user, target, numTargets = 1) {
		if (target.hasRaisedStatStages()) {
			ShowAnimation(@id, user, target, 1);   // Stat stage-draining animation
			@battle.Display(_INTL("{1} stole the target's boosted stats!", user.ToString()));
			showAnim = true;
			foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
				if (target.stages[s.id] <= 0) continue;
				if (user.CanRaiseStatStage(s.id, user, self)) {
					if (user.RaiseStatStage(s.id, target.stages[s.id], user, showAnim)) showAnim = false;
				}
				target.statsLoweredThisRound = true;
				target.statsDropped = true;
				target.stages[s.id] = 0;
			}
		}
		super;
	}
}

//===============================================================================
// Reverses all stat changes of the target. (Topsy-Turvy)
//===============================================================================
public partial class Battle.Move.InvertTargetStatStages : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.hasAlteredStatStages()) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		foreach (var s in GameData.Stat) { //GameData.Stat.each_battle do => |s|
			if (target.stages[s.id] > 0) {
				target.statsLoweredThisRound = true;
				target.statsDropped = true;
			} else if (target.stages[s.id] < 0) {
				target.statsRaisedThisRound = true;
			}
			target.stages[s.id] *= -1;
		}
		@battle.Display(_INTL("{1}'s stats were reversed!", target.ToString()));
	}
}

//===============================================================================
// Resets all target's stat stages to 0. (Clear Smog)
//===============================================================================
public partial class Battle.Move.ResetTargetStatStages : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		if (target.damageState.calcDamage > 0 && !target.damageState.substitute &&
			target.hasAlteredStatStages()) {
			target.ResetStatStages;
			@battle.Display(_INTL("{1}'s stat changes were removed!", target.ToString()));
		}
	}
}

//===============================================================================
// Resets all stat stages for all battlers to 0. (Haze)
//===============================================================================
public partial class Battle.Move.ResetAllBattlersStatStages : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.allBattlers.none(b => b.hasAlteredStatStages())) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.allBattlers.each(b => b.ResetStatStages);
		@battle.Display(_INTL("All stat changes were eliminated!"));
	}
}

//===============================================================================
// For 5 rounds, user's and ally's stat stages cannot be lowered by foes. (Mist)
//===============================================================================
public partial class Battle.Move.StartUserSideImmunityToStatStageLowering : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.Mist > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.Mist = 5;
		@battle.Display(_INTL("{1} became shrouded in mist!", user.Team));
	}
}

//===============================================================================
// Swaps the user's Attack and Defense stats. (Power Trick)
//===============================================================================
public partial class Battle.Move.UserSwapBaseAtkDef : Battle.Move {
	public bool canSnatch() { return true; }

	public void EffectGeneral(user) {
		user.attack, user.defense = user.defense, user.attack;
		user.effects.PowerTrick = !user.effects.PowerTrick;
		@battle.Display(_INTL("{1} switched its Attack and Defense!", user.ToString()));
	}
}

//===============================================================================
// User and target swap their Speed stats (not their stat stages). (Speed Swap)
//===============================================================================
public partial class Battle.Move.UserTargetSwapBaseSpeed : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public void EffectAgainstTarget(user, target) {
		user.speed, target.speed = target.speed, user.speed;
		@battle.Display(_INTL("{1} switched Speed with its target!", user.ToString()));
	}
}

//===============================================================================
// Averages the user's and target's Attack.
// Averages the user's and target's Special Attack. (Power Split)
//===============================================================================
public partial class Battle.Move.UserTargetAverageBaseAtkSpAtk : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		newatk   = (int)Math.Floor((user.attack + target.attack) / 2);
		newspatk = (int)Math.Floor((user.spatk + target.spatk) / 2);
		user.attack = target.attack = newatk;
		user.spatk  = target.spatk  = newspatk;
		@battle.Display(_INTL("{1} shared its power with the target!", user.ToString()));
	}
}

//===============================================================================
// Averages the user's and target's Defense.
// Averages the user's and target's Special Defense. (Guard Split)
//===============================================================================
public partial class Battle.Move.UserTargetAverageBaseDefSpDef : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		newdef   = (int)Math.Floor((user.defense + target.defense) / 2);
		newspdef = (int)Math.Floor((user.spdef + target.spdef) / 2);
		user.defense = target.defense = newdef;
		user.spdef   = target.spdef   = newspdef;
		@battle.Display(_INTL("{1} shared its guard with the target!", user.ToString()));
	}
}

//===============================================================================
// Averages the user's and target's current HP. (Pain Split)
//===============================================================================
public partial class Battle.Move.UserTargetAverageHP : Battle.Move {
	public void EffectAgainstTarget(user, target) {
		newHP = (user.hp + target.hp) / 2;
		if (user.hp > newHP) {
			user.ReduceHP(user.hp - newHP, false, false);
		} else if (user.hp < newHP) {
			user.RecoverHP(newHP - user.hp, false);
		}
		if (target.hp > newHP) {
			target.ReduceHP(target.hp - newHP, false, false);
		} else if (target.hp < newHP) {
			target.RecoverHP(newHP - target.hp, false);
		}
		@battle.Display(_INTL("The battlers shared their pain!"));
		user.ItemHPHealCheck;
		target.ItemHPHealCheck;
	}
}

//===============================================================================
// For 4 rounds, doubles the Speed of all battlers on the user's side. (Tailwind)
//===============================================================================
public partial class Battle.Move.StartUserSideDoubleSpeed : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.OwnSide.effects.Tailwind > 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.OwnSide.effects.Tailwind = 4;
		@battle.Display(_INTL("The Tailwind blew from behind {1}!", user.Team(true)));
		@battle.allSameSideBattlers(user).each do |b|
			if (b.hasActiveAbility(Abilitys.WINDRIDER)) RaiseStatStageByAbility(:ATTACK, 1, b);
		}
	}
}

//===============================================================================
// For 5 rounds, swaps all battlers' base Defense with base Special Defense.
// (Wonder Room)
//===============================================================================
public partial class Battle.Move.StartSwapAllBattlersBaseDefensiveStats : Battle.Move {
	public void EffectGeneral(user) {
		if (@battle.field.effects.WonderRoom > 0) {
			@battle.field.effects.WonderRoom = 0;
			@battle.Display(_INTL("Wonder Room wore off, and the Defense and Sp. Def stats returned to normal!"));
		} else {
			@battle.field.effects.WonderRoom = 5;
			@battle.Display(_INTL("It created a bizarre area in which the Defense and Sp. Def stats are swapped!"));
		}
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@battle.field.effects.WonderRoom > 0) return;   // No animation
		base.ShowAnimation();
	}
}
