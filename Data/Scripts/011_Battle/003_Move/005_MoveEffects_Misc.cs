//===============================================================================
// No additional effect.
//===============================================================================
public partial class Battle.Move.None : Battle.Move {
}

//===============================================================================
// Does absolutely nothing. Shows a special message. (Celebrate)
//===============================================================================
public partial class Battle.Move.DoesNothingCongratulations : Battle.Move {
	public void EffectGeneral(user) {
		if (user.wild()) {
			@battle.Display(_INTL("Congratulations from {1}!", user.ToString(true)));
		} else {
			@battle.Display(_INTL("Congratulations, {1}!", @battle.GetOwnerName(user.index)));
		}
	}
}

//===============================================================================
// Does absolutely nothing. (Hold Hands)
//===============================================================================
public partial class Battle.Move.DoesNothingFailsIfNoAlly : Battle.Move {
	public bool ignoresSubstitute(user) {  return true; }

	public bool MoveFailed(user, targets) {
		if (user.allAllies.length == 0) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Does absolutely nothing. (Splash)
//===============================================================================
public partial class Battle.Move.DoesNothingUnusableInGravity : Battle.Move {
	public bool unusableInGravity() { return true; }

	public void EffectGeneral(user) {
		@battle.Display(_INTL("But nothing happened!"));
	}
}

//===============================================================================
// Scatters coins that the player picks up after winning the battle. (Pay Day)
// NOTE: In Gen 6+, if the user levels up after this move is used, the amount of
//       money picked up depends on the user's new level rather than its level
//       when it used the move. I think this is silly, so I haven't coded this
//       effect.
//===============================================================================
public partial class Battle.Move.AddMoneyGainedFromBattle : Battle.Move {
	public void EffectGeneral(user) {
		if (user.OwnedByPlayer()) {
			@battle.field.effects.PayDay += 5 * user.level;
		}
		@battle.Display(_INTL("Coins were scattered everywhere!"));
	}
}

//===============================================================================
// Doubles the prize money the player gets after winning the battle. (Happy Hour)
//===============================================================================
public partial class Battle.Move.DoubleMoneyGainedFromBattle : Battle.Move {
	public void EffectGeneral(user) {
		if (!user.opposes()) @battle.field.effects.HappyHour = true;
		@battle.Display(_INTL("Everyone is caught up in the happy atmosphere!"));
	}
}

//===============================================================================
// Fails if this isn't the user's first turn. (First Impression)
//===============================================================================
public partial class Battle.Move.FailsIfNotUserFirstTurn : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (user.turnCount > 1) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Fails unless user has already used all other moves it knows. (Last Resort)
//===============================================================================
public partial class Battle.Move.FailsIfUserHasUnusedMove : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		hasThisMove = false;
		hasOtherMoves = false;
		hasUnusedMoves = false;
		foreach (var m in user.Moves) { //user.eachMove do => |m|
			if (m.id == @id) hasThisMove    = true;
			if (m.id != @id) hasOtherMoves  = true;
			if (m.id != @id && !user.movesUsed.Contains(m.id)) hasUnusedMoves = true;
		}
		if (!hasThisMove || !hasOtherMoves || hasUnusedMoves) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Fails unless user has consumed a berry at some point. (Belch)
//===============================================================================
public partial class Battle.Move.FailsIfUserNotConsumedBerry : Battle.Move {
	public bool CanChooseMove(user, commandPhase, showMessages) {
		if (!user.belched()) {
			if (showMessages) {
				msg = _INTL("{1} hasn't eaten any held berry, so it can't possibly belch!", user.ToString());
				(commandPhase) ? @battle.DisplayPaused(msg) : @battle.Display(msg)
			}
			return false;
		}
		return true;
	}

	public bool MoveFailed(user, targets) {
		if (!user.belched()) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Fails if the target is not holding an item, or if the target is affected by
// Magic Room/Klutz. (Poltergeist)
//===============================================================================
public partial class Battle.Move.FailsIfTargetHasNoItem : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (!target.item || !target.itemActive()) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		@battle.Display(_INTL("{1} is about to be attacked by its {2}!", target.ToString(), target.itemName));
		return false;
	}
}

//===============================================================================
// Only damages Pokémon that share a type with the user. (Synchronoise)
//===============================================================================
public partial class Battle.Move.FailsUnlessTargetSharesTypeWithUser : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		userTypes = user.Types(true);
		targetTypes = target.Types(true);
		sharesType = false;
		foreach (var t in userTypes) { //'userTypes.each' do => |t|
			if (!targetTypes.Contains(t)) continue;
			sharesType = true;
			break;
		}
		if (!sharesType) {
			if (show_message) @battle.Display(_INTL("{1} is unaffected!", target.ToString()));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Fails if user was hit by a damaging move this round. (Focus Punch)
//===============================================================================
public partial class Battle.Move.FailsIfUserDamagedThisTurn : Battle.Move {
	public void DisplayChargeMessage(user) {
		user.effects.FocusPunch = true;
		@battle.CommonAnimation("FocusPunch", user);
		@battle.Display(_INTL("{1} is tightening its focus!", user.ToString()));
	}

	public override void DisplayUseMessage(user) {
		if (!user.effects.FocusPunch || !user.tookMoveDamageThisRound) base.DisplayUseMessage();
	}

	public bool MoveFailed(user, targets) {
		if (user.effects.FocusPunch && user.tookMoveDamageThisRound) {
			@battle.Display(_INTL("{1} lost its focus and couldn't move!", user.ToString()));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Fails if the target didn't choose a damaging move to use this round, or has
// already moved. (Sucker Punch)
//===============================================================================
public partial class Battle.Move.FailsIfTargetActed : Battle.Move {
	public bool FailsAgainstTarget(user, target, show_message) {
		if (@battle.choices[target.index].Action != :UseMove) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		oppMove = @battle.choices[target.index].Move;
		if (!oppMove ||
			(oppMove.function_code != "UseMoveTargetIsAboutToUse" &&
			(target.movedThisRound() || oppMove.statusMove()))) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// If attack misses, user takes crash damage of 1/2 of max HP. (Supercell Slam)
//===============================================================================
public partial class Battle.Move.CrashDamageIfFails : Battle.Move {
	public bool recoilMove() { return true; }

	public void CrashDamage(user) {
		if (!user.takesIndirectDamage()) return;
		@battle.Display(_INTL("{1} kept going and crashed!", user.ToString()));
		@battle.scene.DamageAnimation(user);
		user.ReduceHP(user.totalhp / 2, false);
		user.ItemHPHealCheck;
		if (user.fainted()) user.Faint;
	}
}

//===============================================================================
// If attack misses, user takes crash damage of 1/2 of max HP. Can't be used in
// gravity. (High Jump Kick, Jump Kick)
//===============================================================================
public partial class Battle.Move.CrashDamageIfFailsUnusableInGravity : Battle.Move.CrashDamageIfFails {
	public bool unusableInGravity() { return true; }
}

//===============================================================================
// Starts sunny weather. (Sunny Day)
//===============================================================================
public partial class Battle.Move.StartSunWeather : Battle.Move.WeatherMove {
	public override void initialize(battle, move) {
		base.initialize();
		@weatherType = Types.Sun;
	}
}

//===============================================================================
// Starts rainy weather. (Rain Dance)
//===============================================================================
public partial class Battle.Move.StartRainWeather : Battle.Move.WeatherMove {
	public override void initialize(battle, move) {
		base.initialize();
		@weatherType = Types.Rain;
	}
}

//===============================================================================
// Starts sandstorm weather. (Sandstorm)
//===============================================================================
public partial class Battle.Move.StartSandstormWeather : Battle.Move.WeatherMove {
	public override void initialize(battle, move) {
		base.initialize();
		@weatherType = Types.Sandstorm;
	}
}

//===============================================================================
// Starts hail weather. (Hail)
//===============================================================================
public partial class Battle.Move.StartHailWeather : Battle.Move.WeatherMove {
	public override void initialize(battle, move) {
		base.initialize();
		@weatherType = (Settings.USE_SNOWSTORM_WEATHER_INSTEAD_OF_HAIL) ? :Snowstorm : :Hail;
	}
}

//===============================================================================
// For 5 rounds, creates an electric terrain which boosts Electric-type moves and
// prevents Pokémon from falling asleep. Affects non-airborne Pokémon only.
// (Electric Terrain)
//===============================================================================
public partial class Battle.Move.StartElectricTerrain : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.field.terrain == :Electric) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.StartTerrain(user, :Electric);
	}
}

//===============================================================================
// For 5 rounds, creates a grassy terrain which boosts Grass-type moves and heals
// Pokémon at the end of each round. Affects non-airborne Pokémon only.
// (Grassy Terrain)
//===============================================================================
public partial class Battle.Move.StartGrassyTerrain : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.field.terrain == :Grassy) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.StartTerrain(user, :Grassy);
	}
}

//===============================================================================
// For 5 rounds, creates a misty terrain which weakens Dragon-type moves and
// protects Pokémon from status problems. Affects non-airborne Pokémon only.
// (Misty Terrain)
//===============================================================================
public partial class Battle.Move.StartMistyTerrain : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.field.terrain == :Misty) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.StartTerrain(user, :Misty);
	}
}

//===============================================================================
// For 5 rounds, creates a psychic terrain which boosts Psychic-type moves and
// prevents Pokémon from being hit by >0 priority moves. Affects non-airborne
// Pokémon only. (Psychic Terrain)
//===============================================================================
public partial class Battle.Move.StartPsychicTerrain : Battle.Move {
	public bool MoveFailed(user, targets) {
		if (@battle.field.terrain == :Psychic) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		@battle.StartTerrain(user, :Psychic);
	}
}

//===============================================================================
// Removes the current terrain. (Ice Spinner)
//===============================================================================
public partial class Battle.Move.RemoveTerrain : Battle.Move {
	// NOTE: Bulbapedia claims that Ice Spinner shouldn't remove terrain if the
	//       user faints because of its Life Orb or is switched out by Red Card.
	//       I can't find any evidence of this. Also, those items trigger at the
	//       very end of a move's use, way after move effects usually happen. I'm
	//       treating Bulbapedia's claim as a mistake and ignoring it.
	public void EffectGeneral(user) {
		if (user.fainted()) return;
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

//===============================================================================
// Removes the current terrain. Fails if there is no terrain in effect.
// (Steel Roller)
//===============================================================================
public partial class Battle.Move.RemoveTerrainFailsIfNoTerrain : Battle.Move.RemoveTerrain {
	public bool MoveFailed(user, targets) {
		if (@battle.field.terrain == :None) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}
}

//===============================================================================
// Entry hazard. Lays spikes on the opposing side (max. 3 layers). (Spikes)
//===============================================================================
public partial class Battle.Move.AddSpikesToFoeSide : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		if (user.OpposingSide.effects.Spikes >= 3) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (damagingMove()) return;
		user.OpposingSide.effects.Spikes += 1;
		@battle.Display(_INTL("Spikes were scattered all around {1}'s feet!",
														user.OpposingTeam(true)));
	}

	public void AdditionalEffect(user, target) {
		if (user.fainted()) return;
		if (user.OpposingSide.effects.Spikes >= 3) return;
		user.OpposingSide.effects.Spikes += 1;
		@battle.Display(_INTL("Spikes were scattered all around {1}'s feet!",
														user.OpposingTeam(true)));
	}
}

//===============================================================================
// Entry hazard. Lays poison spikes on the opposing side (max. 2 layers).
// (Toxic Spikes)
//===============================================================================
public partial class Battle.Move.AddToxicSpikesToFoeSide : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		if (user.OpposingSide.effects.ToxicSpikes >= 2) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (damagingMove()) return;
		user.OpposingSide.effects.ToxicSpikes += 1;
		@battle.Display(_INTL("Poison spikes were scattered all around {1}'s feet!",
														user.OpposingTeam(true)));
	}

	public void AdditionalEffect(user, target) {
		if (user.fainted()) return;
		if (user.OpposingSide.effects.ToxicSpikes >= 2) return;
		user.OpposingSide.effects.ToxicSpikes += 1;
		@battle.Display(_INTL("Poison spikes were scattered all around {1}'s feet!",
														user.OpposingTeam(true)));
	}
}

//===============================================================================
// Entry hazard. Lays stealth rocks on the opposing side. (Stealth Rock)
//===============================================================================
public partial class Battle.Move.AddStealthRocksToFoeSide : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		if (user.OpposingSide.effects.StealthRock) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (damagingMove()) return;
		user.OpposingSide.effects.StealthRock = true;
		@battle.Display(_INTL("Pointed stones float in the air around {1}!",
														user.OpposingTeam(true)));
	}

	public void AdditionalEffect(user, target) {
		if (user.fainted()) return;
		if (user.OpposingSide.effects.StealthRock) return;
		user.OpposingSide.effects.StealthRock = true;
		@battle.Display(_INTL("Pointed stones float in the air around {1}!",
														user.OpposingTeam(true)));
	}
}

//===============================================================================
// Entry hazard. Lays stealth rocks on the opposing side. (Sticky Web)
//===============================================================================
public partial class Battle.Move.AddStickyWebToFoeSide : Battle.Move {
	public bool canMagicCoat() { return true; }

	public bool MoveFailed(user, targets) {
		if (damagingMove()) return false;
		if (user.OpposingSide.effects.StickyWeb) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		if (damagingMove()) return false;
		user.OpposingSide.effects.StickyWeb = true;
		@battle.Display(_INTL("A sticky web has been laid out beneath {1}'s feet!",
														user.OpposingTeam(true)));
	}

	public void AdditionalEffect(user, target) {
		if (user.fainted()) return;
		if (user.OpposingSide.effects.StickyWeb) return;
		user.OpposingSide.effects.StickyWeb = true;
		@battle.Display(_INTL("A sticky web has been laid out beneath {1}'s feet!",
														user.OpposingTeam(true)));
	}
}

//===============================================================================
// All effects that apply to one side of the field are swapped to the opposite
// side. (Court Change)
//===============================================================================
public partial class Battle.Move.SwapSideEffects : Battle.Move {
	public int number_effects		{ get { return _number_effects; } set { _number_effects = value; } }			protected int _number_effects;
	public int boolean_effects		{ get { return _boolean_effects; } }			protected int _boolean_effects;

	public override void initialize(battle, move) {
		base.initialize();
		@number_effects = new {
			Effects.AuroraVeil,
			Effects.LightScreen,
			Effects.Mist,
			Effects.Rainbow,
			Effects.Reflect,
			Effects.Safeguard,
			Effects.SeaOfFire,
			Effects.Spikes,
			Effects.Swamp,
			Effects.Tailwind,
			Effects.ToxicSpikes;
		}
		@boolean_effects = new {
			Effects.StealthRock,
			Effects.StickyWeb;
		}
	}

	public bool MoveFailed(user, targets) {
		has_effect = false;
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			effects = @battle.sides[side].effects;
			@number_effects.each do |e|
				if (effects[e] == 0) continue;
				has_effect = true;
				break;
			}
			if (has_effect) break;
			@boolean_effects.each do |e|
				if (!effects[e]) continue;
				has_effect = true;
				break;
			}
			if (has_effect) break;
		}
		if (!has_effect) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		side0 = @battle.sides[0];
		side1 = @battle.sides[1];
		@number_effects.each do |e|
			side0.effects[e], side1.effects[e] = side1.effects[e], side0.effects[e];
		}
		@boolean_effects.each do |e|
			side0.effects[e], side1.effects[e] = side1.effects[e], side0.effects[e];
		}
		@battle.Display(_INTL("{1} swapped the battle effects affecting each side of the field!", user.ToString()));
	}
}

//===============================================================================
// User turns 1/4 of max HP into a substitute. (Substitute)
//===============================================================================
public partial class Battle.Move.UserMakeSubstitute : Battle.Move {
	public bool canSnatch() { return true; }

	public bool MoveFailed(user, targets) {
		if (user.effects.Substitute > 0) {
			@battle.Display(_INTL("{1} already has a substitute!", user.ToString()));
			return true;
		}
		@subLife = (int)Math.Max(user.totalhp / 4, 1);
		if (user.hp <= @subLife) {
			@battle.Display(_INTL("But it does not have enough HP left to make a substitute!"));
			return true;
		}
		return false;
	}

	public void OnStartUse(user, targets) {
		user.ReduceHP(@subLife, false, false);
		user.ItemHPHealCheck;
	}

	public void EffectGeneral(user) {
		user.effects.Trapping     = 0;
		user.effects.TrappingMove = null;
		user.effects.Substitute   = @subLife;
		@battle.Display(_INTL("{1} put in a substitute!", user.ToString()));
	}
}

//===============================================================================
// Removes trapping moves, entry hazards and Leech Seed on user/user's side.
// Raises user's Speed by 1 stage (Gen 8+). (Rapid Spin)
//===============================================================================
public partial class Battle.Move.RemoveUserBindingAndEntryHazards : Battle.Move.StatUpMove {
	public override void initialize(battle, move) {
		base.initialize();
		@statUp = new {:SPEED, 1};
	}

	public void EffectAfterAllHits(user, target) {
		if (user.fainted() || target.damageState.unaffected) return;
		if (user.effects.Trapping > 0) {
			trapMove = GameData.Move.get(user.effects.TrappingMove).name;
			trapUser = @battle.battlers[user.effects.TrappingUser];
			@battle.Display(_INTL("{1} got free of {2}'s {3}!", user.ToString(), trapUser.ToString(true), trapMove));
			user.effects.Trapping     = 0;
			user.effects.TrappingMove = null;
			user.effects.TrappingUser = -1;
		}
		if (user.effects.LeechSeed >= 0) {
			user.effects.LeechSeed = -1;
			@battle.Display(_INTL("{1} shed Leech Seed!", user.ToString()));
		}
		if (user.OwnSide.effects.StealthRock) {
			user.OwnSide.effects.StealthRock = false;
			@battle.Display(_INTL("{1} blew away stealth rocks!", user.ToString()));
		}
		if (user.OwnSide.effects.Spikes > 0) {
			user.OwnSide.effects.Spikes = 0;
			@battle.Display(_INTL("{1} blew away spikes!", user.ToString()));
		}
		if (user.OwnSide.effects.ToxicSpikes > 0) {
			user.OwnSide.effects.ToxicSpikes = 0;
			@battle.Display(_INTL("{1} blew away poison spikes!", user.ToString()));
		}
		if (user.OwnSide.effects.StickyWeb) {
			user.OwnSide.effects.StickyWeb = false;
			@battle.Display(_INTL("{1} blew away sticky webs!", user.ToString()));
		}
	}

	public override void AdditionalEffect(user, target) {
		if (Settings.MECHANICS_GENERATION >= 8) base.AdditionalEffect();
	}
}

//===============================================================================
// Attacks 2 rounds in the future. (Doom Desire, Future Sight)
//===============================================================================
public partial class Battle.Move.AttackTwoTurnsLater : Battle.Move {
	public bool targetsPosition() { return true; }

	// Stops damage being dealt in the setting-up turn.
	public override bool DamagingMove() {
		if (!@battle.futureSight) return false;
		return base.DamagingMove();
	}

	public override void AccuracyCheck(user, target) {
		if (!@battle.futureSight) return true;
		return base.AccuracyCheck();
	}

	public override void DisplayUseMessage(user) {
		if (!@battle.futureSight) base.DisplayUseMessage();
	}

	public bool FailsAgainstTarget(user, target, show_message) {
		if (!@battle.futureSight &&
			@battle.positions[target.index].effects.FutureSightCounter > 0) {
			if (show_message) @battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectAgainstTarget(user, target) {
		if (@battle.futureSight) return;   // Attack is hitting
		effects = @battle.positions[target.index].effects;
		effects.FutureSightCounter        = 3;
		effects.FutureSightMove           = @id;
		effects.FutureSightUserIndex      = user.index;
		effects.FutureSightUserPartyIndex = user.pokemonIndex;
		if (@id == :DOOMDESIRE) {
			@battle.Display(_INTL("{1} chose Doom Desire as its destiny!", user.ToString()));
		} else {
			@battle.Display(_INTL("{1} foresaw an attack!", user.ToString()));
		}
	}

	public override void ShowAnimation(id, user, targets, hitNum = 0, showAnimation = true) {
		if (!@battle.futureSight) hitNum = 1;   // Charging anim
		base.ShowAnimation();
	}
}

//===============================================================================
// User switches places with its ally. (Ally Switch)
//===============================================================================
public partial class Battle.Move.UserSwapsPositionsWithAlly : Battle.Move {
	public bool MoveFailed(user, targets) {
		numTargets = 0;
		@idxAlly = -1;
		idxUserOwner = @battle.GetOwnerIndexFromBattlerIndex(user.index);
		foreach (var b in user.allAllies) { //'user.allAllies.each' do => |b|
			if (@battle.GetOwnerIndexFromBattlerIndex(b.index) != idxUserOwner) continue;
			if (!b.near(user)) continue;
			numTargets += 1;
			@idxAlly = b.index;
		}
		if (numTargets != 1) {
			@battle.Display(_INTL("But it failed!"));
			return true;
		}
		return false;
	}

	public void EffectGeneral(user) {
		idxA = user.index;
		idxB = @idxAlly;
		if (@battle.SwapBattlers(idxA, idxB)) {
			@battle.Display(_INTL("{1} and {2} switched places!",
															@battle.battlers[idxB].ToString(), @battle.battlers[idxA].ToString(true)));
			new {idxA, idxB}.each(idx => @battle.EffectsOnBattlerEnteringPosition(@battle.battlers[idx]));
		}
	}
}

//===============================================================================
// If a Pokémon makes contact with the user before it uses this move, the
// attacker is burned. (Beak Blast)
//===============================================================================
public partial class Battle.Move.BurnAttackerBeforeUserActs : Battle.Move {
	public void DisplayChargeMessage(user) {
		user.effects.BeakBlast = true;
		@battle.CommonAnimation("BeakBlast", user);
		@battle.Display(_INTL("{1} started heating up its beak!", user.ToString()));
	}
}
