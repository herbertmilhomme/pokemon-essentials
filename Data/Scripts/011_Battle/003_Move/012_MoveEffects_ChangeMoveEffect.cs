//===============================================================================
// This round, user becomes the target of attacks that have single targets.
// (Follow Me, Rage Powder)
//===============================================================================
public partial class Battle.Move.RedirectAllMovesToUser : Battle.Move {
	public void EffectGeneral(user) {
		user.effects.FollowMe = 1;
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (b.effects.FollowMe < user.effects.FollowMe) continue;
			user.effects.FollowMe = b.effects.FollowMe + 1;
		}
		if (powderMove()) user.effects.RagePowder = true;
		@battle.Display(_INTL("{1} became the center of attention!", user.ToString()));
	}
}

//===============================================================================
// This round, target becomes the target of attacks that have single targets.
// (Spotlight)
//===============================================================================
public partial class Battle.Move.RedirectAllMovesToTarget : Battle.Move {
	public bool canMagicCoat() { return true; }

	public void EffectAgainstTarget(user, target) {
		target.effects.Spotlight = 1;
		foreach (var b in target.allAllies) { //'target.allAllies.each' do => |b|
			if (b.effects.Spotlight < target.effects.Spotlight) continue;
			target.effects.Spotlight = b.effects.Spotlight + 1;
		}
		@battle.Display(_INTL("{1} became the center of attention!", target.ToString()));
	}
}

//===============================================================================
// Unaffected by moves and abilities that would redirect this move. (Snipe Shot)
//===============================================================================
public partial class Battle.Move.CannotBeRedirected : Battle.Move {
	public bool cannotRedirect() { return true; }
}

//===============================================================================
// Randomly damages or heals the target. (Present)
// NOTE: Apparently a Normal Gem should be consumed even if this move will heal,
//       but I think that's silly so I've omitted that effect.
//===============================================================================
public partial class Battle.Move.RandomlyDamageOrHealTarget : Battle.Move {
	public void OnStartUse(user, targets) {
		@presentDmg = 0;   // 0 = heal, >0 = damage
		r = @battle.Random(100);
		if (r < 40) {
			@presentDmg = 40;
		} else if (r < 70) {
			@presentDmg = 80;
		} else if (r < 80) {
			@presentDmg = 120;
		}
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (@presentDmg > 0) return false;
		if (!target.canHeal()) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public override bool DamagingMove() {
		if (@presentDmg == 0) return false;
		return base.DamagingMove();
	}

	public void BaseDamage(baseDmg, user, target) {
		return @presentDmg;
	}

	public void EffectAgainstTarget(user, target) {
		if (@presentDmg > 0) return;
		target.RecoverHP(target.totalhp / 4);
		@battle.Display(_INTL("{1}'s HP was restored.", target.ToString()));
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@presentDmg == 0) hitNum = 1;   // Healing anim
		base.ShowAnimation();
	}
}

//===============================================================================
// Damages target if target is a foe, or heals target by 1/2 of its max HP if
// target is an ally. (Pollen Puff)
//===============================================================================
public partial class Battle.Move.HealAllyOrDamageFoe : Battle.Move {
	public override void Target(user) {
		if (user.effects.HealBlock > 0) return GameData.Target.get(:NearFoe);
		return base.Target();
	}

	public void OnStartUse(user, targets) {
		@healing = false;
		if (targets.length > 0) @healing = !user.opposes(targets[0]);
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!@healing) return false;
		if (target.effects.Substitute > 0 && !ignoresSubstitute(user)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (!target.canHeal()) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public override bool DamagingMove() {
		if (@healing) return false;
		return base.DamagingMove();
	}

	public void EffectAgainstTarget(user, target) {
		if (!@healing) return;
		target.RecoverHP(target.totalhp / 2);
		@battle.Display(_INTL("{1}'s HP was restored.", target.ToString()));
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (@healing) hitNum = 1;   // Healing anim
		base.ShowAnimation();
	}
}

//===============================================================================
// User is Ghost: User loses 1/2 of max HP, and curses the target.
// Cursed Pokémon lose 1/4 of their max HP at the end of each round.
// User is not Ghost: Decreases the user's Speed by 1 stage, and increases the
// user's Attack and Defense by 1 stage each. (Curse)
//===============================================================================
public partial class Battle.Move.CurseTargetOrLowerUserSpd1RaiseUserAtkDef1 : Battle.Move {
	public int statUp		{ get { return _statUp; } set { _statUp = value; } }			protected int _statUp;
	public int statDown		{ get { return _statDown; } }			protected int _statDown;

	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@statUp   = new {:ATTACK, 1, :DEFENSE, 1};
		@statDown = new {:SPEED, 1};
	}

	public void Target(user) {
		if (user.Type == Types.GHOST) {
			ghost_target = (Settings.MECHANICS_GENERATION >= 8) ? :RandomNearFoe : :NearFoe;
			return GameData.Target.get(ghost_target);
		}
		return super;
	}

	public bool MoveFailed(user, targets) {
		if (user.Type == Types.GHOST) return false;
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
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (user.Type == Types.GHOST && target.effects.Curse) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (user.Type == Types.GHOST) return;
		// Non-Ghost effect
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

	public void EffectAgainstTarget(user, target) {
		if (!user.Type == Types.GHOST) return;
		// Ghost effect
		@battle.Display(_INTL("{1} cut its own HP and laid a curse on {2}!", user.ToString(), target.ToString(true)));
		target.effects.Curse = true;
		user.ReduceHP(user.totalhp / 2, false, false);
		user.ItemHPHealCheck;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (!user.Type == Types.GHOST) hitNum = 1;   // Non-Ghost anim
		base.ShowAnimation();
	}
}

//===============================================================================
// Effect depends on the environment. (Secret Power)
//===============================================================================
public partial class Battle.Move.EffectDependsOnEnvironment : Battle.Move {
	public int secretPower		{ get { return _secretPower; } }			protected int _secretPower;

	public bool flinchingMove() { return new []{6, 10, 12}.Contains(@secretPower); }

	public void OnStartUse(user, targets) {
		// NOTE: This is Gen 7's list plus some of Gen 6 plus a bit of my own.
		@secretPower = 0;   // Body Slam, paralysis
		switch (@battle.field.terrain) {
			case :Electric:
				@secretPower = 1;   // Thunder Shock, paralysis
				break;
			case :Grassy:
				@secretPower = 2;   // Vine Whip, sleep
				break;
			case :Misty:
				@secretPower = 3;   // Fairy Wind, lower Sp. Atk by 1
				break;
			case :Psychic:
				@secretPower = 4;   // Confusion, lower Speed by 1
				break;
			default:
				switch (@battle.environment) {
					case :Grass: case :TallGrass: case :Forest: case :ForestGrass:
						@secretPower = 2;    // (Same as Grassy Terrain)
						break;
					case :MovingWater: case :StillWater: case :Underwater:
						@secretPower = 5;    // Water Pulse, lower Attack by 1
						break;
					case :Puddle:
						@secretPower = 6;    // Mud Shot, lower Speed by 1
						break;
					case :Cave:
						@secretPower = 7;    // Rock Throw, flinch
						break;
					case :Rock: case :Sand:
						@secretPower = 8;    // Mud-Slap, lower Acc by 1
						break;
					case :Snow, :Ice:
						@secretPower = 9;    // Ice Shard, freeze
						break;
					case :Volcano:
						@secretPower = 10;   // Incinerate, burn
						break;
					case :Graveyard:
						@secretPower = 11;   // Shadow Sneak, flinch
						break;
					case :Sky:
						@secretPower = 12;   // Gust, lower Speed by 1
						break;
					case :Space:
						@secretPower = 13;   // Swift, flinch
						break;
					case :UltraSpace:
						@secretPower = 14;   // Psywave, lower Defense by 1
						break;
				}
				break;
		}
	}

	// NOTE: This intentionally doesn't use def AdditionalEffect, because that
	//       method is called per hit and this move's additional effect only occurs
	//       once per use, after all the hits have happened (two hits are possible
	//       via Parental Bond).
	public void EffectAfterAllHits(user, target) {
		if (target.fainted()) return;
		if (target.damageState.unaffected || target.damageState.substitute) return;
		if (user.hasActiveAbility(Abilitys.SHEERFORCE)) return;
		chance = AdditionalEffectChance(user, target);
		if (@battle.Random(100) >= chance) return;
		switch (@secretPower) {
			case 2:
				if (target.CanSleep(user, false, self)) target.Sleep(user);
				break;
			case 10:
				if (target.CanBurn(user, false, self)) target.Burn(user);
				break;
			case 0: case 1:
				if (target.CanParalyze(user, false, self)) target.Paralyze(user);
				break;
			case 9:
				if (target.CanFreeze(user, false, self)) target.Freeze(user);
				break;
			case 5:
				if (target.CanLowerStatStage(:ATTACK, user, self)) {
					target.LowerStatStage(:ATTACK, 1, user);
				}
				break;
			case 14:
				if (target.CanLowerStatStage(:DEFENSE, user, self)) {
					target.LowerStatStage(:DEFENSE, 1, user);
				}
				break;
			case 3:
				if (target.CanLowerStatStage(:SPECIAL_ATTACK, user, self)) {
					target.LowerStatStage(:SPECIAL_ATTACK, 1, user);
				}
				break;
			case 4: case 6: case 12:
				if (target.CanLowerStatStage(:SPEED, user, self)) {
					target.LowerStatStage(:SPEED, 1, user);
				}
				break;
			case 8:
				if (target.CanLowerStatStage(:ACCURACY, user, self)) {
					target.LowerStatStage(:ACCURACY, 1, user);
				}
				break;
			case 7: case 11: case 13:
				target.Flinch(user);
				break;
		}
	}

	public void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		id = :BODYSLAM;   // Environment-specific anim
		switch (@secretPower) {
			case 1: //if (GameData.Moves.exists(Moves.THUNDERSHOCK))
				id = :THUNDERSHOCK; break;
			case 2: //if (GameData.Moves.exists(Moves.VINEWHIP))
				id = :VINEWHIP; break;
			case 3: //if (GameData.Moves.exists(Moves.FAIRYWIND))
				id = :FAIRYWIND; break;
			case 4: //if (GameData.Moves.exists(Moves.CONFUSION))
				id = :CONFUSIO; break;
			case 5: //if (GameData.Moves.exists(Moves.WATERPULSE))
				id = :WATERPULSE; break;
			case 6: //if (GameData.Moves.exists(Moves.MUDSHOT))
				id = :MUDSHOT; break;
			case 7: //if (GameData.Moves.exists(Moves.ROCKTHROW))
				id = :ROCKTHROW; break;
			case 8: //if (GameData.Moves.exists(Moves.MUDSLAP))
				id = :MUDSLAP; break;
			case 9: //if (GameData.Moves.exists(Moves.ICESHARD))
				id = :ICESHARD; break;
			case 10: //if (GameData.Moves.exists(Moves.INCINERATE))
				id = :INCINERATE; break;
			case 11: //if (GameData.Moves.exists(Moves.SHADOWSNEAK))
				id = :SHADOWSNEAK; break;
			case 12: //if (GameData.Moves.exists(Moves.GUST))
				id = :GUST; break;
			case 13: //if (GameData.Moves.exists(Moves.SWIFT))
				id = :SWIFT; break;
			case 14: //if (GameData.Moves.exists(Moves.PSYWAVE))
				id = :PSYWAVE; break;
		}
		super;
	}
}

//===============================================================================
// If Psychic Terrain applies and the user is grounded, power is multiplied by
// 1.5 (in addition to Psychic Terrain's multiplier) and it targets all opposing
// Pokémon. (Expanding Force)
//===============================================================================
public partial class Battle.Move.HitsAllFoesAndPowersUpInPsychicTerrain : Battle.Move {
	public void Target(user) {
		if (@battle.field.terrain == :Psychic && user.affectedByTerrain()) {
			return GameData.Target.get(:AllNearFoes);
		}
		return super;
	}

	public void BaseDamage(baseDmg, user, target) {
		if (@battle.field.terrain == :Psychic && user.affectedByTerrain()) {
			baseDmg = baseDmg * 3 / 2;
		}
		return baseDmg;
	}
}

//===============================================================================
// Powders the foe. This round, if it uses a Fire move, it loses 1/4 of its max
// HP instead. (Powder)
//===============================================================================
public partial class Battle.Move.TargetNextFireMoveDamagesTarget : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool canMagicCoat() {            return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.effects.Powder) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.Powder = true;
		@battle.Display(_INTL("{1} is covered in powder!", user.ToString()));
	}
}

//===============================================================================
// Power is doubled if Fusion Flare has already been used this round. (Fusion Bolt)
//===============================================================================
public partial class Battle.Move.DoublePowerAfterFusionFlare : Battle.Move {
	public override void ChangeUsageCounters(user, specialUsage) {
		@doublePower = @battle.field.effects.FusionFlare;
		base.ChangeUsageCounters();
	}

	public void BaseDamageMultiplier(damageMult, user, target) {
		if (@doublePower) damageMult *= 2;
		return damageMult;
	}

	public void EffectGeneral(user) {
		@battle.field.effects.FusionBolt = true;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if ((targets.length > 0 && targets[0].damageState.critical) ||
									@doublePower) hitNum = 1;   // Charged anim
		base.ShowAnimation();
	}
}

//===============================================================================
// Power is doubled if Fusion Bolt has already been used this round. (Fusion Flare)
//===============================================================================
public partial class Battle.Move.DoublePowerAfterFusionBolt : Battle.Move {
	public override void ChangeUsageCounters(user, specialUsage) {
		@doublePower = @battle.field.effects.FusionBolt;
		base.ChangeUsageCounters();
	}

	public void BaseDamageMultiplier(damageMult, user, target) {
		if (@doublePower) damageMult *= 2;
		return damageMult;
	}

	public void EffectGeneral(user) {
		@battle.field.effects.FusionFlare = true;
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if ((targets.length > 0 && targets[0].damageState.critical) ||
									@doublePower) hitNum = 1;   // Charged anim
		base.ShowAnimation();
	}
}

//===============================================================================
// Powers up the ally's attack this round by 1.5. (Helping Hand)
//===============================================================================
public partial class Battle.Move.PowerUpAllyMove : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (target.fainted() || target.effects.HelpingHand) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		if (MoveFailedTargetAlreadyMoved(target, show_message)) return true;
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		target.effects.HelpingHand = true;
		@battle.Display(_INTL("{1} is ready to help {2}!", user.ToString(), target.ToString(true)));
	}
}

//===============================================================================
// Counters a physical move used against the user this round, with 2x the power.
// (Counter)
//===============================================================================
public partial class Battle.Move.CounterPhysicalDamage : Battle.Move.FixedDamageMove {
	public void AddTarget(targets, user) {
		t = user.effects.CounterTarget;
		if (t < 0 || !user.opposes(t)) return;
		user.AddTarget(targets, user, @battle.battlers[t], self, false);
	}

	public bool MoveFailed(user, targets) {
		if (targets.length == 0) {
			@battle.Display(_INTL("But there was no target..."));
			return true;
		}
		return false;
	}

	public void FixedDamage(user, target) {
		dmg = user.effects.Counter * 2;
		if (dmg == 0) dmg = 1;
		return dmg;
	}
}

//===============================================================================
// Counters a specical move used against the user this round, with 2x the power.
// (Mirror Coat)
//===============================================================================
public partial class Battle.Move.CounterSpecialDamage : Battle.Move.FixedDamageMove {
	public void AddTarget(targets, user) {
		t = user.effects.MirrorCoatTarget;
		if (t < 0 || !user.opposes(t)) return;
		user.AddTarget(targets, user, @battle.battlers[t], self, false);
	}

	public bool MoveFailed(user, targets) {
		if (targets.length == 0) {
			@battle.Display(_INTL("But there was no target..."));
			return true;
		}
		return false;
	}

	public void FixedDamage(user, target) {
		dmg = user.effects.MirrorCoat * 2;
		if (dmg == 0) dmg = 1;
		return dmg;
	}
}

//===============================================================================
// Counters the last damaging move used against the user this round, with 1.5x
// the power. (Metal Burst)
//===============================================================================
public partial class Battle.Move.CounterDamagePlusHalf : Battle.Move.FixedDamageMove {
	public void AddTarget(targets, user) {
		if (user.lastFoeAttacker.length == 0) return;
		foreach (var party_index in user.lastFoeAttacker.reverse_each) { //user.lastFoeAttacker.reverse_each do => |party_index|
			battler = @battle.FindBattler(party_index, user.index + 1);
			if (!battler) continue;
			user.AddTarget(targets, user, battler, self, false);
			break;
		}
	}

	public bool MoveFailed(user, targets) {
		if (targets.length == 0) {
			@battle.Display(_INTL("But there was no target..."));
			return true;
		}
		return false;
	}

	public void FixedDamage(user, target) {
		dmg = (int)Math.Floor(user.lastHPLostFromFoe * 1.5);
		if (dmg == 0) dmg = 1;
		return dmg;
	}
}

//===============================================================================
// Increases the user's Defense and Special Defense by 1 stage each. Ups the
// user's stockpile by 1 (max. 3). (Stockpile)
//===============================================================================
public partial class Battle.Move.UserAddStockpileRaiseDefSpDef1 : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.Stockpile >= 3) {
			@battle.Display(_INTL("{1} can't stockpile any more!", user.ToString()));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.effects.Stockpile += 1;
		@battle.Display(_INTL("{1} stockpiled {2}!",
														user.ToString(), user.effects.Stockpile));
		showAnim = true;
		if (user.CanRaiseStatStage(:DEFENSE, user, self)) {
			if (user.RaiseStatStage(:DEFENSE, 1, user, showAnim)) {
				user.effects.StockpileDef += 1;
				showAnim = false;
			}
		}
		if (user.CanRaiseStatStage(:SPECIAL_DEFENSE, user, self)) {
			if (user.RaiseStatStage(:SPECIAL_DEFENSE, 1, user, showAnim)) {
				user.effects.StockpileSpDef += 1;
			}
		}
	}
}

//===============================================================================
// Power is 100 multiplied by the user's stockpile (X). Resets the stockpile to
// 0. Decreases the user's Defense and Special Defense by X stages each. (Spit Up)
//===============================================================================
public partial class Battle.Move.PowerDependsOnUserStockpile : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.effects.Stockpile == 0) {
			@battle.Display(_INTL("But it failed to spit up a thing!"));
			return true;
		}
		return false;
	}

	public void BaseDamage(baseDmg, user, target) {
		return 100 * user.effects.Stockpile;
	}

	public void EffectAfterAllHits(user, target) {
		if (user.fainted() || user.effects.Stockpile == 0) return;
		if (target.damageState.unaffected) return;
		@battle.Display(_INTL("{1}'s stockpiled effect wore off!", user.ToString()));
		if (@battle.AllFainted(target.idxOwnSide)) return;
		showAnim = true;
		if (user.effects.StockpileDef > 0 &&
			user.CanLowerStatStage(:DEFENSE, user, self)) {
			if (user.LowerStatStage(:DEFENSE, user.effects.StockpileDef, user, showAnim)) showAnim = false;
		}
		if (user.effects.StockpileSpDef > 0 &&
			user.CanLowerStatStage(:SPECIAL_DEFENSE, user, self)) {
			user.LowerStatStage(:SPECIAL_DEFENSE, user.effects.StockpileSpDef, user, showAnim);
		}
		user.effects.Stockpile      = 0;
		user.effects.StockpileDef   = 0;
		user.effects.StockpileSpDef = 0;
	}
}

//===============================================================================
// Heals user depending on the user's stockpile (X). Resets the stockpile to 0.
// Decreases the user's Defense and Special Defense by X stages each. (Swallow)
//===============================================================================
public partial class Battle.Move.HealUserDependingOnUserStockpile : Battle.Move {
	public bool healingMove() { return true; }
	public bool canSnatch() {   return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.Stockpile == 0) {
			@battle.Display(_INTL("But it failed to swallow a thing!"));
			return true;
		}
		if (!user.canHeal() &&
			user.effects.StockpileDef == 0 &&
			user.effects.StockpileSpDef == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		hpGain = 0;
		switch ((int)Math.Max(user.effects.Stockpile, 1)) {
			case 1:  hpGain = user.totalhp / 4; break;
			case 2:  hpGain = user.totalhp / 2; break;
			case 3:  hpGain = user.totalhp; break;
		}
		if (user.RecoverHP(hpGain) > 0) {
			@battle.Display(_INTL("{1}'s HP was restored.", user.ToString()));
		}
		@battle.Display(_INTL("{1}'s stockpiled effect wore off!", user.ToString()));
		showAnim = true;
		if (user.effects.StockpileDef > 0 &&
			user.CanLowerStatStage(:DEFENSE, user, self)) {
			if (user.LowerStatStage(:DEFENSE, user.effects.StockpileDef, user, showAnim)) {
				showAnim = false;
			}
		}
		if (user.effects.StockpileSpDef > 0 &&
			user.CanLowerStatStage(:SPECIAL_DEFENSE, user, self)) {
			user.LowerStatStage(:SPECIAL_DEFENSE, user.effects.StockpileSpDef, user, showAnim);
		}
		user.effects.Stockpile      = 0;
		user.effects.StockpileDef   = 0;
		user.effects.StockpileSpDef = 0;
	}
}

//===============================================================================
// Combos with another Pledge move used by the ally. (Grass Pledge)
// If the move is a combo, power is doubled and causes either a sea of fire or a
// swamp on the opposing side.
//===============================================================================
public partial class Battle.Move.GrassPledge : Battle.Move.PledgeMove {
	public override void initialize(battle, move) {
		base.initialize();
		// [Function code to combo with, effect, override type, override animation]
		@combos = new {new {"FirePledge",  :SeaOfFire, :FIRE, :FIREPLEDGE},
							new {"WaterPledge", :Swamp,     null,   null}}
	}
}

//===============================================================================
// Combos with another Pledge move used by the ally. (Fire Pledge)
// If the move is a combo, power is doubled and causes either a rainbow on the
// user's side or a sea of fire on the opposing side.
//===============================================================================
public partial class Battle.Move.FirePledge : Battle.Move.PledgeMove {
	public override void initialize(battle, move) {
		base.initialize();
		// [Function code to combo with, effect, override type, override animation]
		@combos = new {new {"WaterPledge", :Rainbow,   :WATER, :WATERPLEDGE},
							new {"GrassPledge", :SeaOfFire, null,    null}}
	}
}

//===============================================================================
// Combos with another Pledge move used by the ally. (Water Pledge)
// If the move is a combo, power is doubled and causes either a swamp on the
// opposing side or a rainbow on the user's side.
//===============================================================================
public partial class Battle.Move.WaterPledge : Battle.Move.PledgeMove {
	public override void initialize(battle, move) {
		base.initialize();
		// new {Function code to combo with, effect, override type, override animation}
		@combos = new {new {"GrassPledge", :Swamp,   :GRASS, :GRASSPLEDGE},
							new {"FirePledge",  :Rainbow, null,    null}}
	}
}

//===============================================================================
// Uses the last move that was used. (Copycat)
//===============================================================================
public partial class Battle.Move.UseLastMoveUsed : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool callsAnotherMove() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			// Struggle, Belch
			"Struggle",                                          // Struggle
			"FailsIfUserNotConsumedBerry",                       // Belch              // Not listed on Bulbapedia
			// Moves that affect the moveset
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",       // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",                 // Sketch
			"TransformUserIntoTarget",                           // Transform
			// Counter moves
			"CounterPhysicalDamage",                             // Counter
			"CounterSpecialDamage",                              // Mirror Coat
			"CounterDamagePlusHalf",                             // Metal Burst        // Not listed on Bulbapedia
			// Helping Hand, Feint (always blacklisted together, don't know why)
			"PowerUpAllyMove",                                   // Helping Hand
			"RemoveProtections",                                 // Feint
			// Protection moves
			"ProtectUser",                                       // Detect, Protect
			"ProtectUserSideFromPriorityMoves",                  // Quick Guard        // Not listed on Bulbapedia
			"ProtectUserSideFromMultiTargetDamagingMoves",       // Wide Guard         // Not listed on Bulbapedia
			"UserEnduresFaintingThisTurn",                       // Endure
			"ProtectUserSideFromDamagingMovesIfUserFirstTurn",   // Mat Block
			"ProtectUserSideFromStatusMoves",                    // Crafty Shield      // Not listed on Bulbapedia
			"ProtectUserFromDamagingMovesKingsShield",           // King's Shield
			"ProtectUserFromDamagingMovesObstruct",              // Obstruct           // Not listed on Bulbapedia
			"ProtectUserFromTargetingMovesSpikyShield",          // Spiky Shield
			"ProtectUserBanefulBunker",                          // Baneful Bunker
			"ProtectUserFromDamagingMovesSilkTrap",              // Silk Trap
			"ProtectUserFromDamagingMovesBurningBulwark",        // Burning Bulwark
			// Moves that call other moves
			"UseLastMoveUsedByTarget",                           // Mirror Move
			"UseLastMoveUsed",                                   // Copycat (this move)
			"UseMoveTargetIsAboutToUse",                         // Me First
			"UseMoveDependingOnEnvironment",                     // Nature Power       // Not listed on Bulbapedia
			"UseRandomUserMoveIfAsleep",                         // Sleep Talk
			"UseRandomMoveFromUserParty",                        // Assist
			"UseRandomMove",                                     // Metronome
			// Move-redirecting and stealing moves
			"BounceBackProblemCausingStatusMoves",               // Magic Coat         // Not listed on Bulbapedia
			"StealAndUseBeneficialStatusMove",                   // Snatch
			"RedirectAllMovesToUser",                            // Follow Me, Rage Powder
			"RedirectAllMovesToTarget",                          // Spotlight
			// Set up effects that trigger upon KO
			"ReduceAttackerMovePPTo0IfUserFaints",               // Grudge             // Not listed on Bulbapedia
			"AttackerFaintsIfUserFaints",                        // Destiny Bond
			// Held item-moving moves
			"UserTakesTargetItem",                               // Covet, Thief
			"UserTargetSwapItems",                               // Switcheroo, Trick
			"TargetTakesUserItem",                               // Bestow
			// Moves that start focussing at the start of the round
			"FailsIfUserDamagedThisTurn",                        // Focus Punch
			"UsedAfterUserTakesPhysicalDamage",                  // Shell Trap
			"BurnAttackerBeforeUserActs",                        // Beak Blast
			// Event moves that do nothing
			"DoesNothingFailsIfNoAlly",                          // Hold Hands
			"DoesNothingCongratulations";                         // Celebrate
		}
		if (Settings.MECHANICS_GENERATION >= 6) {
			@moveBlacklist += new {
				// Target-switching moves
				"SwitchOutTargetStatusMove",                       // Roar, Whirlwind
				"SwitchOutTargetDamagingMove";                      // Circle Throw, Dragon Tail
			}
		}
	}

	public override void ChangeUsageCounters(user, specialUsage) {
		base.ChangeUsageCounters();
		@copied_move = @battle.lastMoveUsed;
	}

	public bool MoveFailed(user, targets) {
		if (!@copied_move ||
			!GameData.Move.exists(@copied_move) ||
			@moveBlacklist.Contains(GameData.Move.get(@copied_move).function_code)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.UseMoveSimple(@copied_move);
	}
}

//===============================================================================
// Uses the last move that the target used. (Mirror Move)
//===============================================================================
public partial class Battle.Move.UseLastMoveUsedByTarget : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }
	public bool callsAnotherMove() { return true; }

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.lastRegularMoveUsed ||
			!GameData.Move.exists(target.lastRegularMoveUsed) ||
			!GameData.Move.get(target.lastRegularMoveUsed).has_flag("CanMirrorMove")) {
			if (show_message) @battle.Display(_INTL("The mirror move failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		user.UseMoveSimple(target.lastRegularMoveUsed, target.index);
	}

	public void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		// No animation
	}
}

//===============================================================================
// Uses the move the target was about to use this round, with 1.5x power.
// (Me First)
//===============================================================================
public partial class Battle.Move.UseMoveTargetIsAboutToUse : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool ignoresSubstitute(user) {  return true; }
	public bool callsAnotherMove() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"UserTakesTargetItem",                // Covet, Thief
			// Struggle, Belch
			"Struggle",                           // Struggle
			"FailsIfUserNotConsumedBerry",        // Belch
			// Counter moves
			"CounterPhysicalDamage",              // Counter
			"CounterSpecialDamage",               // Mirror Coat
			"CounterDamagePlusHalf",              // Metal Burst
			// Moves that start focussing at the start of the round
			"FailsIfUserDamagedThisTurn",         // Focus Punch
			"UsedAfterUserTakesPhysicalDamage",   // Shell Trap
			"BurnAttackerBeforeUserActs";          // Beak Blast
		}
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (MoveFailedTargetAlreadyMoved(target, show_message)) return true;
		oppMove = @battle.choices[target.index].Move;
		if (!oppMove || oppMove.statusMove() || @moveBlacklist.Contains(oppMove.function_code)) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		user.effects.MeFirst = true;
		user.UseMoveSimple(@battle.choices[target.index].Move.id);
		user.effects.MeFirst = false;
	}
}

//===============================================================================
// Uses a different move depending on the environment. (Nature Power)
// NOTE: This code does not support the Gen 5 and older definition of the move
//       where it targets the user. It makes more sense for it to target another
//       Pokémon.
//===============================================================================
public partial class Battle.Move.UseMoveDependingOnEnvironment : Battle.Move {
	public int npMove		{ get { return _npMove; } }			protected int _npMove;

	public bool callsAnotherMove() { return true; }

	public void OnStartUse(user, targets) {
		// NOTE: It's possible in theory to not have the move Nature Power wants to
		//       turn into, but what self-respecting game wouldn't at least have Tri
		//       Attack in it?
		@npMove = Moves.TRIATTACK;
		switch (@battle.field.terrain) {
			case :Electric:
				if (GameData.Moves.exists(Moves.THUNDERBOLT)) @npMove = Moves.THUNDERBOLT;
				break;
			case :Grassy:
				if (GameData.Moves.exists(Moves.ENERGYBALL)) @npMove = Moves.ENERGYBALL;
				break;
			case :Misty:
				if (GameData.Moves.exists(Moves.MOONBLAST)) @npMove = Moves.MOONBLAST;
				break;
			case :Psychic:
				if (GameData.Moves.exists(Moves.PSYCHIC)) @npMove = Moves.PSYCHIC;
				break;
			default:
				try_move = null;
				switch (@battle.environment) {
					case :Grass: case :TallGrass: case :Forest: case :ForestGrass:
						try_move = (Settings.MECHANICS_GENERATION >= 6) ? :ENERGYBALL : :SEEDBOMB;
						break;
					case :MovingWater: case :StillWater: case :Underwater:
						try_move = moves.HYDROPUMP;
						break;
					case :Puddle:
						try_move = moves.MUDBOMB;
						break;
					case :Cave:
						try_move = (Settings.MECHANICS_GENERATION >= 6) ? :POWERGEM : :ROCKSLIDE;
						break;
					case :Rock: case :Sand:
						try_move = (Settings.MECHANICS_GENERATION >= 6) ? :EARTHPOWER : :EARTHQUAKE;
						break;
					case :Snow:
						try_move = moves.BLIZZARD;
						if (Settings.MECHANICS_GENERATION == 6) try_move = moves.FROSTBREATH;
						if (Settings.MECHANICS_GENERATION >= 7) try_move = moves.ICEBEAM;
						break;
					case :Ice:
						try_move = moves.ICEBEAM;
						break;
					case :Volcano:
						try_move = moves.LAVAPLUME;
						break;
					case :Graveyard:
						try_move = moves.SHADOWBALL;
						break;
					case :Sky:
						try_move = moves.AIRSLASH;
						break;
					case :Space:
						try_move = moves.DRACOMETEOR;
						break;
					case :UltraSpace:
						try_move = moves.PSYSHOCK;
						break;
				}
				if (GameData.Move.exists(try_move)) @npMove = try_move;
				break;
		}
	}

	public void EffectAgainstTarget(user, target) {
		@battle.Display(_INTL("{1} turned into {2}!", @name, GameData.Move.get(@npMove).name));
		user.UseMoveSimple(@npMove, target.index);
	}
}

//===============================================================================
// Uses a random move that exists. (Metronome)
//===============================================================================
public partial class Battle.Move.UseRandomMove : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool callsAnotherMove() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"FlinchTargetFailsIfUserNotAsleep",                  // Snore
			"TargetActsNext",                                    // After You
			"TargetActsLast",                                    // Quash
			"TargetUsesItsLastUsedMoveAgain",                    // Instruct
			// Struggle, Belch
			"Struggle",                                          // Struggle
			"FailsIfUserNotConsumedBerry",                       // Belch
			// Moves that affect the moveset
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",       // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",                 // Sketch
			"TransformUserIntoTarget",                           // Transform
			// Counter moves
			"CounterPhysicalDamage",                             // Counter
			"CounterSpecialDamage",                              // Mirror Coat
			"CounterDamagePlusHalf",                             // Metal Burst        // Not listed on Bulbapedia
			// Helping Hand, Feint (always blacklisted together, don't know why)
			"PowerUpAllyMove",                                   // Helping Hand
			"RemoveProtections",                                 // Feint
			// Protection moves
			"ProtectUser",                                       // Detect, Protect
			"ProtectUserSideFromPriorityMoves",                  // Quick Guard
			"ProtectUserSideFromMultiTargetDamagingMoves",       // Wide Guard
			"UserEnduresFaintingThisTurn",                       // Endure
			"ProtectUserSideFromDamagingMovesIfUserFirstTurn",   // Mat Block
			"ProtectUserSideFromStatusMoves",                    // Crafty Shield
			"ProtectUserFromDamagingMovesKingsShield",           // King's Shield
			"ProtectUserFromDamagingMovesObstruct",              // Obstruct
			"ProtectUserFromTargetingMovesSpikyShield",          // Spiky Shield
			"ProtectUserBanefulBunker",                          // Baneful Bunker
			"ProtectUserFromDamagingMovesSilkTrap",              // Silk Trap
			"ProtectUserFromDamagingMovesBurningBulwark",        // Burning Bulwark
			// Moves that call other moves
			"UseLastMoveUsedByTarget",                           // Mirror Move
			"UseLastMoveUsed",                                   // Copycat
			"UseMoveTargetIsAboutToUse",                         // Me First
			"UseMoveDependingOnEnvironment",                     // Nature Power
			"UseRandomUserMoveIfAsleep",                         // Sleep Talk
			"UseRandomMoveFromUserParty",                        // Assist
			"UseRandomMove",                                     // Metronome
			// Move-redirecting and stealing moves
			"BounceBackProblemCausingStatusMoves",               // Magic Coat         // Not listed on Bulbapedia
			"StealAndUseBeneficialStatusMove",                   // Snatch
			"RedirectAllMovesToUser",                            // Follow Me, Rage Powder
			"RedirectAllMovesToTarget",                          // Spotlight
			// Set up effects that trigger upon KO
			"ReduceAttackerMovePPTo0IfUserFaints",               // Grudge             // Not listed on Bulbapedia
			"AttackerFaintsIfUserFaints",                        // Destiny Bond
			// Held item-moving moves
			"UserTakesTargetItem",                               // Covet, Thief
			"UserTargetSwapItems",                               // Switcheroo, Trick
			"TargetTakesUserItem",                               // Bestow
			// Moves that start focussing at the start of the round
			"FailsIfUserDamagedThisTurn",                        // Focus Punch
			"UsedAfterUserTakesPhysicalDamage",                  // Shell Trap
			"BurnAttackerBeforeUserActs",                        // Beak Blast
			// Event moves that do nothing
			"DoesNothingFailsIfNoAlly",                          // Hold Hands
			"DoesNothingCongratulations";                         // Celebrate
		}
	}

	public bool MoveFailed(user, targets) {
		@metronomeMove = null;
		move_keys = GameData.Move.keys;
		// NOTE: You could be really unlucky and roll blacklisted moves 1000 times in
		//       a row. This is too unlikely to care about, though.
		1000.times do;
			move_id = move_keys[@battle.Random(move_keys.length)];
			move_data = GameData.Move.get(move_id);
			if (@moveBlacklist.Contains(move_data.function_code)) continue;
			if (move_data.has_flag("CannotMetronome")) continue;
			if (move_data.type == types.SHADOW) continue;
			@metronomeMove = move_data.id;
			break;
		}
		if (!@metronomeMove) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		user.UseMoveSimple(@metronomeMove);
	}
}

//===============================================================================
// Uses a random move known by any non-user Pokémon in the user's party. (Assist)
//===============================================================================
public partial class Battle.Move.UseRandomMoveFromUserParty : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool callsAnotherMove() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			// Struggle, Belch
			"Struggle",                                          // Struggle
			"FailsIfUserNotConsumedBerry",                       // Belch
			// Moves that affect the moveset
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",       // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",                 // Sketch
			"TransformUserIntoTarget",                           // Transform
			// Counter moves
			"CounterPhysicalDamage",                             // Counter
			"CounterSpecialDamage",                              // Mirror Coat
			"CounterDamagePlusHalf",                             // Metal Burst        // Not listed on Bulbapedia
			// Helping Hand, Feint (always blacklisted together, don't know why)
			"PowerUpAllyMove",                                   // Helping Hand
			"RemoveProtections",                                 // Feint
			// Protection moves
			"ProtectUser",                                       // Detect, Protect
			"ProtectUserSideFromPriorityMoves",                  // Quick Guard        // Not listed on Bulbapedia
			"ProtectUserSideFromMultiTargetDamagingMoves",       // Wide Guard         // Not listed on Bulbapedia
			"UserEnduresFaintingThisTurn",                       // Endure
			"ProtectUserSideFromDamagingMovesIfUserFirstTurn",   // Mat Block
			"ProtectUserSideFromStatusMoves",                    // Crafty Shield      // Not listed on Bulbapedia
			"ProtectUserFromDamagingMovesKingsShield",           // King's Shield
			"ProtectUserFromDamagingMovesObstruct",              // Obstruct           // Not listed on Bulbapedia
			"ProtectUserFromTargetingMovesSpikyShield",          // Spiky Shield
			"ProtectUserBanefulBunker",                          // Baneful Bunker
			"ProtectUserFromDamagingMovesSilkTrap",              // Silk Trap
			"ProtectUserFromDamagingMovesBurningBulwark",        // Burning Bulwark
			// Moves that call other moves
			"UseLastMoveUsedByTarget",                           // Mirror Move
			"UseLastMoveUsed",                                   // Copycat
			"UseMoveTargetIsAboutToUse",                         // Me First
//      "UseMoveDependingOnEnvironment",                    // Nature Power       // See below
			"UseRandomUserMoveIfAsleep",                         // Sleep Talk
			"UseRandomMoveFromUserParty",                        // Assist
			"UseRandomMove",                                     // Metronome
			// Move-redirecting and stealing moves
			"BounceBackProblemCausingStatusMoves",               // Magic Coat         // Not listed on Bulbapedia
			"StealAndUseBeneficialStatusMove",                   // Snatch
			"RedirectAllMovesToUser",                            // Follow Me, Rage Powder
			"RedirectAllMovesToTarget",                          // Spotlight
			// Set up effects that trigger upon KO
			"ReduceAttackerMovePPTo0IfUserFaints",               // Grudge             // Not listed on Bulbapedia
			"AttackerFaintsIfUserFaints",                        // Destiny Bond
			// Target-switching moves
//      "SwitchOutTargetStatusMove",                        // Roar, Whirlwind    // See below
			"SwitchOutTargetDamagingMove",                       // Circle Throw, Dragon Tail
			// Held item-moving moves
			"UserTakesTargetItem",                               // Covet, Thief
			"UserTargetSwapItems",                               // Switcheroo, Trick
			"TargetTakesUserItem",                               // Bestow
			// Moves that start focussing at the start of the round
			"FailsIfUserDamagedThisTurn",                        // Focus Punch
			"UsedAfterUserTakesPhysicalDamage",                  // Shell Trap
			"BurnAttackerBeforeUserActs",                        // Beak Blast
			// Event moves that do nothing
			"DoesNothingFailsIfNoAlly",                          // Hold Hands
			"DoesNothingCongratulations";                         // Celebrate
		}
		if (Settings.MECHANICS_GENERATION >= 6) {
			@moveBlacklist += new {
				// Moves that call other moves
				"UseMoveDependingOnEnvironment",                   // Nature Power
				// Two-turn attacks
				"TwoTurnAttack",                                   // Razor Wind                // Not listed on Bulbapedia
				"TwoTurnAttackOneTurnInSun",                       // Solar Beam, Solar Blade   // Not listed on Bulbapedia
				"TwoTurnAttackParalyzeTarget",                     // Freeze Shock              // Not listed on Bulbapedia
				"TwoTurnAttackBurnTarget",                         // Ice Burn                  // Not listed on Bulbapedia
				"TwoTurnAttackFlinchTarget",                       // Sky Attack                // Not listed on Bulbapedia
				"TwoTurnAttackChargeRaiseUserDefense1",            // Skull Bash                // Not listed on Bulbapedia
				"TwoTurnAttackInvulnerableInSky",                  // Fly
				"TwoTurnAttackInvulnerableUnderground",            // Dig
				"TwoTurnAttackInvulnerableUnderwater",             // Dive
				"TwoTurnAttackInvulnerableInSkyParalyzeTarget",    // Bounce
				"TwoTurnAttackInvulnerableRemoveProtections",      // Shadow Force/Phantom Force
				"TwoTurnAttackInvulnerableInSkyTargetCannotAct",   // Sky Drop
				"AllBattlersLoseHalfHPUserSkipsNextTurn",          // Shadow Half
				"TwoTurnAttackRaiseUserSpAtkSpDefSpd2",            // Geomancy                  // Not listed on Bulbapedia
				// Target-switching moves
				"SwitchOutTargetStatusMove";                        // Roar, Whirlwind
			}
		}
	}

	public bool MoveFailed(user, targets) {
		@assistMoves = new List<string>();
		// NOTE: This includes the Pokémon of ally trainers in multi battles.
		@battle.Party(user.index).each_with_index do |pkmn, i|
			if (!pkmn || i == user.pokemonIndex) continue;
			if (Settings.MECHANICS_GENERATION >= 6 && pkmn.egg()) continue;
			foreach (var move in pkmn.moves) { //'pkmn.moves.each' do => |move|
				if (@moveBlacklist.Contains(move.function_code)) continue;
				if (move.type == types.SHADOW) continue;
				@assistMoves.Add(move.id);
			}
		}
		if (@assistMoves.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		move = @assistMoves[@battle.Random(@assistMoves.length)];
		user.UseMoveSimple(move);
	}
}

//===============================================================================
// Uses a random move the user knows. Fails if user is not asleep. (Sleep Talk)
//===============================================================================
public partial class Battle.Move.UseRandomUserMoveIfAsleep : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool usableWhenAsleep() { return true; }
	public bool callsAnotherMove() { return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"MultiTurnAttackPreventSleeping",                  // Uproar
			"MultiTurnAttackBideThenReturnDoubleDamage",       // Bide
			// Struggle, Belch
			"Struggle",                                        // Struggle             // Not listed on Bulbapedia
			"FailsIfUserNotConsumedBerry",                     // Belch
			// Moves that affect the moveset (except Transform)
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",     // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",               // Sketch
			// Moves that call other moves
			"UseLastMoveUsedByTarget",                         // Mirror Move
			"UseLastMoveUsed",                                 // Copycat
			"UseMoveTargetIsAboutToUse",                       // Me First
			"UseMoveDependingOnEnvironment",                   // Nature Power         // Not listed on Bulbapedia
			"UseRandomUserMoveIfAsleep",                       // Sleep Talk
			"UseRandomMoveFromUserParty",                      // Assist
			"UseRandomMove",                                   // Metronome
			// Two-turn attacks
			"TwoTurnAttack",                                   // Razor Wind
			"TwoTurnAttackOneTurnInSun",                       // Solar Beam, Solar Blade
			"TwoTurnAttackParalyzeTarget",                     // Freeze Shock
			"TwoTurnAttackBurnTarget",                         // Ice Burn
			"TwoTurnAttackFlinchTarget",                       // Sky Attack
			"TwoTurnAttackChargeRaiseUserDefense1",            // Skull Bash
			"TwoTurnAttackInvulnerableInSky",                  // Fly
			"TwoTurnAttackInvulnerableUnderground",            // Dig
			"TwoTurnAttackInvulnerableUnderwater",             // Dive
			"TwoTurnAttackInvulnerableInSkyParalyzeTarget",    // Bounce
			"TwoTurnAttackInvulnerableRemoveProtections",      // Shadow Force/Phantom Force
			"TwoTurnAttackInvulnerableInSkyTargetCannotAct",   // Sky Drop
			"AllBattlersLoseHalfHPUserSkipsNextTurn",          // Shadow Half
			"TwoTurnAttackRaiseUserSpAtkSpDefSpd2",            // Geomancy
			// Moves that start focussing at the start of the round
			"FailsIfUserDamagedThisTurn",                      // Focus Punch
			"UsedAfterUserTakesPhysicalDamage",                // Shell Trap
			"BurnAttackerBeforeUserActs";                       // Beak Blast
		}
	}

	public bool MoveFailed(user, targets) {
		@sleepTalkMoves = new List<string>();
		user.eachMoveWithIndex do |m, i|
			if (@moveBlacklist.Contains(m.function_code)) continue;
			if (!@battle.CanChooseMove(user.index, i, false, true)) continue;
			@sleepTalkMoves.Add(i);
		}
		if (!user.asleep() || @sleepTalkMoves.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		choice = @sleepTalkMoves[@battle.Random(@sleepTalkMoves.length)];
		user.UseMoveSimple(user.moves[choice].id, user.DirectOpposing.index);
	}
}

//===============================================================================
// This round, reflects all moves that can be Magic Coated which target the user
// or which have no target back at their origin. (Magic Coat)
//===============================================================================
public partial class Battle.Move.BounceBackProblemCausingStatusMoves : Battle.Move {
	public void EffectGeneral(user) {
		user.effects.MagicCoat = true;
		@battle.Display(_INTL("{1} shrouded itself with Magic Coat!", user.ToString()));
	}
}

//===============================================================================
// This round, snatches all used moves that can be Snatched. (Snatch)
//===============================================================================
public partial class Battle.Move.StealAndUseBeneficialStatusMove : Battle.Move {
	public void EffectGeneral(user) {
		user.effects.Snatch = 1;
		@battle.allBattlers.each do |b|
			if (b.effects.Snatch < user.effects.Snatch) continue;
			user.effects.Snatch = b.effects.Snatch + 1;
		}
		@battle.Display(_INTL("{1} waits for a target to make a move!", user.ToString()));
	}
}

//===============================================================================
// This move turns into the last move used by the target, until user switches
// out. (Mimic)
//===============================================================================
public partial class Battle.Move.ReplaceMoveThisBattleWithTargetLastMoveUsed : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"UseRandomMove",                                 // Metronome
			// Struggle
			"Struggle",                                      // Struggle
			// Moves that affect the moveset
			"ReplaceMoveThisBattleWithTargetLastMoveUsed",   // Mimic
			"ReplaceMoveWithTargetLastMoveUsed",             // Sketch
			"TransformUserIntoTarget";                        // Transform
		}
	}

	public bool MoveFailed(user, targets) {
		if (user.effects.Transform || !user.HasMove(@id)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		lastMoveData = GameData.Move.try_get(target.lastRegularMoveUsed);
		if (!lastMoveData ||
			user.HasMove(target.lastRegularMoveUsed) ||
			@moveBlacklist.Contains(lastMoveData.function_code) ||
			lastMoveData.type == types.SHADOW) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		user.eachMoveWithIndex do |m, i|
			if (m.id != @id) continue;
			newMove = new Pokemon.Move(target.lastRegularMoveUsed);
			user.moves[i] = Battle.Move.from_pokemon_move(@battle, newMove);
			@battle.Display(_INTL("{1} learned {2}!", user.ToString(), newMove.name));
			user.CheckFormOnMovesetChange;
			break;
		}
	}
}

//===============================================================================
// This move permanently turns into the last move used by the target. (Sketch)
//===============================================================================
public partial class Battle.Move.ReplaceMoveWithTargetLastMoveUsed : Battle.Move {
	public int moveBlacklist		{ get { return _moveBlacklist; } }			protected int _moveBlacklist;

	public bool ignoresSubstitute(user) {  return true; }

	public override void initialize(battle, move) {
		base.initialize();
		@moveBlacklist = new {
			"ReplaceMoveWithTargetLastMoveUsed",   // Sketch (this move)
			// Struggle
			"Struggle";                             // Struggle
		}
	}

	public bool MoveFailed(user, targets) {
		if (user.effects.Transform || !user.HasMove(@id)) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		lastMoveData = GameData.Move.try_get(target.lastRegularMoveUsed);
		if (!lastMoveData ||
			user.HasMove(target.lastRegularMoveUsed) ||
			@moveBlacklist.Contains(lastMoveData.function_code) ||
			lastMoveData.type == types.SHADOW) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		user.eachMoveWithIndex do |m, i|
			if (m.id != @id) continue;
			newMove = new Pokemon.Move(target.lastRegularMoveUsed);
			user.pokemon.moves[i] = newMove;
			user.moves[i] = Battle.Move.from_pokemon_move(@battle, newMove);
			@battle.Display(_INTL("{1} learned {2}!", user.ToString(), newMove.name));
			user.CheckFormOnMovesetChange;
			break;
		}
	}
}
