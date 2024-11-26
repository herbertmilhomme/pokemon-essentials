//===============================================================================
//
//===============================================================================
public static partial class Battle.PokeBallEffects {
	IsUnconditional = new ItemHandlerHash();
	ModifyCatchRate = new ItemHandlerHash();
	OnCatch         = new ItemHandlerHash();
	OnFailCatch     = new ItemHandlerHash();

	public static bool isUnconditional(ball, battle, battler) {
		ret = IsUnconditional.trigger(ball, battle, battler);
		return (!ret.null()) ? ret : false;
	}

	public static void modifyCatchRate(ball, catchRate, battle, battler) {
		ret = ModifyCatchRate.trigger(ball, catchRate, battle, battler);
		return (!ret.null()) ? ret : catchRate;
	}

	public static void onCatch(ball, battle, pkmn) {
		OnCatch.trigger(ball, battle, pkmn);
	}

	public static void onFailCatch(ball, battle, battler) {
		Game.GameData.stats.failed_poke_ball_count += 1;
		OnFailCatch.trigger(ball, battle, battler);
	}
}

//===============================================================================
// IsUnconditional
//===============================================================================

Battle.PokeBallEffects.IsUnconditional.add(:MASTERBALL, block: (ball, battle, battler) => {
	next true;
});

//===============================================================================
// ModifyCatchRate
// NOTE: This code is not called if the battler is an Ultra Beast (except if the
//       Ball is a Beast Ball). In this case, all Balls' catch rates are set
//       elsewhere to 0.1x.
//===============================================================================

Battle.PokeBallEffects.ModifyCatchRate.add(:GREATBALL, block: (ball, catchRate, battle, battler) => {
	next catchRate * 1.5;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:ULTRABALL, block: (ball, catchRate, battle, battler) => {
	next catchRate * 2;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:SAFARIBALL, block: (ball, catchRate, battle, battler) => {
	next catchRate * 1.5;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:NETBALL, block: (ball, catchRate, battle, battler) => {
	multiplier = (Settings.NEW_POKE_BALL_CATCH_RATES) ? 3.5 : 3;
	if (battler.Type == Types.BUG || battler.Type == Types.WATER) catchRate *= multiplier;
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:DIVEBALL, block: (ball, catchRate, battle, battler) => {
	if (battle.environment == :Underwater) catchRate *= 3.5;
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:NESTBALL, block: (ball, catchRate, battle, battler) => {
	if (battler.level <= 30) {
		catchRate *= (int)Math.Max((41 - battler.level) / 10.0, 1);
	}
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:REPEATBALL, block: (ball, catchRate, battle, battler) => {
	multiplier = (Settings.NEW_POKE_BALL_CATCH_RATES) ? 3.5 : 3;
	if (battle.Player.owned(battler.species)) catchRate *= multiplier;
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:TIMERBALL, block: (ball, catchRate, battle, battler) => {
	multiplier = (int)Math.Min(1 + (0.3 * battle.turnCount), 4);
	catchRate *= multiplier;
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:DUSKBALL, block: (ball, catchRate, battle, battler) => {
	multiplier = (Settings.NEW_POKE_BALL_CATCH_RATES) ? 3 : 3.5;
	if (battle.time == 2) catchRate *= multiplier;   // Night or in cave
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:QUICKBALL, block: (ball, catchRate, battle, battler) => {
	if (battle.turnCount == 0) catchRate *= 5;
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:FASTBALL, block: (ball, catchRate, battle, battler) => {
	baseStats = battler.pokemon.baseStats;
	baseSpeed = baseStats[:SPEED];
	if (baseSpeed >= 100) catchRate *= 4;
	next (int)Math.Min(catchRate, 255);
});

Battle.PokeBallEffects.ModifyCatchRate.add(:LEVELBALL, block: (ball, catchRate, battle, battler) => {
	maxlevel = 0;
	battle.allSameSideBattlers.each(b => { if (b.level > maxlevel) maxlevel = b.level; });
	if (maxlevel >= battler.level * 4) {
		catchRate *= 8;
	} else if (maxlevel >= battler.level * 2) {
		catchRate *= 4;
	} else if (maxlevel > battler.level) {
		catchRate *= 2;
	}
	next (int)Math.Min(catchRate, 255);
});

Battle.PokeBallEffects.ModifyCatchRate.add(:LUREBALL, block: (ball, catchRate, battle, battler) => {
	if (Game.GameData.game_temp.encounter_type &&
		GameData.EncounterType.get(Game.GameData.game_temp.encounter_type).type == types.fishing) {
		multiplier = (Settings.NEW_POKE_BALL_CATCH_RATES) ? 5 : 3;
		catchRate *= multiplier;
	}
	next (int)Math.Min(catchRate, 255);
});

Battle.PokeBallEffects.ModifyCatchRate.add(:HEAVYBALL, block: (ball, catchRate, battle, battler) => {
	if (catchRate == 0) next 0;
	weight = battler.Weight;
	if (Settings.NEW_POKE_BALL_CATCH_RATES) {
		if (weight >= 3000) {
			catchRate += 30;
		} else if (weight >= 2000) {
			catchRate += 20;
		} else if (weight < 1000) {
			catchRate -= 20;
		}
	} else {
		if (weight >= 4096) {
			catchRate += 40;
		} else if (weight >= 3072) {
			catchRate += 30;
		} else if (weight >= 2048) {
			catchRate += 20;
		} else {
			catchRate -= 20;
		}
	}
	next catchRate.clamp(1, 255);
});

Battle.PokeBallEffects.ModifyCatchRate.add(:LOVEBALL, block: (ball, catchRate, battle, battler) => {
	foreach (var b in battle.allSameSideBattlers) { //'battle.allSameSideBattlers.each' do => |b|
		if (b.species != battler.species) continue;
		if (b.gender == battler.gender || b.gender == 2 || battler.gender == 2) continue;
		catchRate *= 8;
		break;
	}
	next (int)Math.Min(catchRate, 255);
});

Battle.PokeBallEffects.ModifyCatchRate.add(:MOONBALL, block: (ball, catchRate, battle, battler) => {
	// NOTE: Moon Ball cares about whether any species in the target's evolutionary
	//       family can evolve with the Moon Stone, not whether the target itself
	//       can immediately evolve with the Moon Stone.
	moon_stone = GameData.Item.try_get(:MOONSTONE);
	if (moon_stone && battler.pokemon.species_data.family_item_evolutions_use_item(moon_stone.id)) {
		catchRate *= 4;
	}
	next (int)Math.Min(catchRate, 255);
});

Battle.PokeBallEffects.ModifyCatchRate.add(:SPORTBALL, block: (ball, catchRate, battle, battler) => {
	next catchRate * 1.5;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:DREAMBALL, block: (ball, catchRate, battle, battler) => {
	if (battler.asleep()) catchRate *= 4;
	next catchRate;
});

Battle.PokeBallEffects.ModifyCatchRate.add(:BEASTBALL, block: (ball, catchRate, battle, battler) => {
	if (battler.pokemon.species_data.has_flag("UltraBeast")) {
		catchRate *= 5;
	} else {
		catchRate /= 10;
	}
	next catchRate;
});

//===============================================================================
// OnCatch
//===============================================================================

Battle.PokeBallEffects.OnCatch.add(:HEALBALL, block: (ball, battle, pkmn) => {
	pkmn.heal;
});

Battle.PokeBallEffects.OnCatch.add(:FRIENDBALL, block: (ball, battle, pkmn) => {
	pkmn.happiness = (Settings.APPLY_HAPPINESS_SOFT_CAP) ? 150 : 200;
});
