//===============================================================================
//
//===============================================================================
// None

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("DoesNothingCongratulations",
	block: (score, move, user, ai, battle) => {
		next Battle.AI.MOVE_USELESS_SCORE;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("DoesNothingCongratulations",
																					"DoesNothingFailsIfNoAlly");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("DoesNothingCongratulations",
																					"DoesNothingUnusableInGravity");

//===============================================================================
//
//===============================================================================
// AddMoneyGainedFromBattle

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.copy("DoesNothingCongratulations",
																					"DoubleMoneyGainedFromBattle");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("FailsIfNotUserFirstTurn",
	block: (move, user, ai, battle) => {
		next user.turnCount > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("FailsIfNotUserFirstTurn",
	block: (score, move, user, ai, battle) => {
		next score + 25;   // Use it or lose it
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("FailsIfUserHasUnusedMove",
	block: (move, user, ai, battle) => {
		has_another_move = false;
		has_unused_move = false;
		foreach (var m in user.battler.Moves) { //user.battler.eachMove do => |m|
			if (m.id == move.id) continue;
			has_another_move = true;
			if (user.battler.movesUsed.Contains(m.id)) continue;
			has_unused_move = true;
			break;
		}
		next !has_another_move || has_unused_move;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("FailsIfUserNotConsumedBerry",
	block: (move, user, ai, battle) => {
		next !user.battler.belched();
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("FailsIfTargetHasNoItem",
	block: (move, user, target, ai, battle) => {
		next !target.item || !target.item_active();
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("FailsUnlessTargetSharesTypeWithUser",
	block: (move, user, target, ai, battle) => {
		user_types = user.Types(true);
		target_types = target.Types(true);
		next (user_types & target_types).empty();
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("FailsIfUserDamagedThisTurn",
	block: (score, move, user, target, ai, battle) => {
		// Check whether user is faster than its foe(s) and could use this move
		user_faster_count = 0;
		foe_faster_count = 0;
		ai.each_foe_battler(user.side) do |b, i|
			if (user.faster_than(b)) {
				user_faster_count += 1;
			} else {
				foe_faster_count += 1;
			}
		}
		if (user_faster_count == 0) next Battle.AI.MOVE_USELESS_SCORE;
		if (foe_faster_count == 0) score += 15;
		// Effects that make the target unlikely to act before the user
		if (ai.trainer.high_skill()) {
			if (!target.can_attack()) {
				score += 15;
			} else if (target.effects.Confusion > 1 ||
						target.effects.Attract == user.index) {
				score += 10;
			} else if (target.battler.paralyzed()) {
				score += 5;
			}
		}
		// Don't risk using this move if target is weak
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp <= target.totalhp / 2) score -= 10;
			if (target.hp <= target.totalhp / 4) score -= 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("FailsIfTargetActed",
	block: (score, move, user, target, ai, battle) => {
		// Check whether user is faster than the target and could use this move
		if (target.faster_than(user)) next Battle.AI.MOVE_USELESS_SCORE;
		// Check whether the target has any damaging moves it could use
		if (!target.check_for_move(m => m.damagingMove())) next Battle.AI.MOVE_USELESS_SCORE;
		// Don't risk using this move if target is weak
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (target.hp <= target.totalhp / 2) score -= 10;
			if (target.hp <= target.totalhp / 4) score -= 10;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectAgainstTargetScore.add("CrashDamageIfFailsUnusableInGravity",
	block: (score, move, user, target, ai, battle) => {
		if (user.battler.takesIndirectDamage()) {
			score -= (0.6 * (100 - move.rough_accuracy)).ToInt();   // -0 (100%) to -60 (1%)
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartSunWeather",
	block: (move, user, ai, battle) => {
		next new []{:HarshSun, :HeavyRain, :StrongWinds, move.move.weatherType}.Contains(battle.field.weather);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartSunWeather",
	block: (score, move, user, ai, battle) => {
		if (battle.CheckGlobalAbility(Abilities.AIRLOCK) ||
																					battle.CheckGlobalAbility(Abilities.CLOUDNINE)) next Battle.AI.MOVE_USELESS_SCORE;
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.weather != weathers.None) {
			score -= ai.get_score_for_weather(battle.field.weather, user);
		}
		score += ai.get_score_for_weather(:Sun, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("StartSunWeather",
																						"StartRainWeather");
Battle.AI.Handlers.MoveEffectScore.add("StartRainWeather",
	block: (score, move, user, ai, battle) => {
		if (battle.CheckGlobalAbility(Abilities.AIRLOCK) ||
																					battle.CheckGlobalAbility(Abilities.CLOUDNINE)) next Battle.AI.MOVE_USELESS_SCORE;
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.weather != weathers.None) {
			score -= ai.get_score_for_weather(battle.field.weather, user);
		}
		score += ai.get_score_for_weather(:Rain, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("StartSunWeather",
																						"StartSandstormWeather");
Battle.AI.Handlers.MoveEffectScore.add("StartSandstormWeather",
	block: (score, move, user, ai, battle) => {
		if (battle.CheckGlobalAbility(Abilities.AIRLOCK) ||
																					battle.CheckGlobalAbility(Abilities.CLOUDNINE)) next Battle.AI.MOVE_USELESS_SCORE;
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.weather != weathers.None) {
			score -= ai.get_score_for_weather(battle.field.weather, user);
		}
		score += ai.get_score_for_weather(:Sandstorm, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("StartSunWeather",
																						"StartHailWeather");
Battle.AI.Handlers.MoveEffectScore.add("StartHailWeather",
	block: (score, move, user, ai, battle) => {
		if (battle.CheckGlobalAbility(Abilities.AIRLOCK) ||
																					battle.CheckGlobalAbility(Abilities.CLOUDNINE)) next Battle.AI.MOVE_USELESS_SCORE;
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.weather != weathers.None) {
			score -= ai.get_score_for_weather(battle.field.weather, user);
		}
		score += ai.get_score_for_weather((Settings.USE_SNOWSTORM_WEATHER_INSTEAD_OF_HAIL ? :Snowstorm : :Hail), user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartElectricTerrain",
	block: (move, user, ai, battle) => {
		next battle.field.terrain == :Electric;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartElectricTerrain",
	block: (score, move, user, ai, battle) => {
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.terrain != :None) {
			score -= ai.get_score_for_terrain(battle.field.terrain, user);
		}
		score += ai.get_score_for_terrain(:Electric, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartGrassyTerrain",
	block: (move, user, ai, battle) => {
		next battle.field.terrain == :Grassy;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartGrassyTerrain",
	block: (score, move, user, ai, battle) => {
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.terrain != :None) {
			score -= ai.get_score_for_terrain(battle.field.terrain, user);
		}
		score += ai.get_score_for_terrain(:Grassy, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartMistyTerrain",
	block: (move, user, ai, battle) => {
		next battle.field.terrain == :Misty;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartMistyTerrain",
	block: (score, move, user, ai, battle) => {
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.terrain != :None) {
			score -= ai.get_score_for_terrain(battle.field.terrain, user);
		}
		score += ai.get_score_for_terrain(:Misty, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("StartPsychicTerrain",
	block: (move, user, ai, battle) => {
		next battle.field.terrain == :Psychic;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("StartPsychicTerrain",
	block: (score, move, user, ai, battle) => {
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 10;
		}
		if (ai.trainer.high_skill() && battle.field.terrain != :None) {
			score -= ai.get_score_for_terrain(battle.field.terrain, user);
		}
		score += ai.get_score_for_terrain(:Psychic, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("RemoveTerrain",
	block: (score, move, user, ai, battle) => {
		next score - ai.get_score_for_terrain(battle.field.terrain, user);
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RemoveTerrainFailsIfNoTerrain",
	block: (move, user, ai, battle) => {
		next battle.field.terrain == :None;
	}
)
Battle.AI.Handlers.MoveEffectScore.copy("RemoveTerrain",
																					"RemoveTerrainFailsIfNoTerrain");

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("AddSpikesToFoeSide",
	block: (move, user, ai, battle) => {
		next user.OpposingSide.effects.Spikes >= 3;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("AddSpikesToFoeSide",
	block: (score, move, user, ai, battle) => {
		inBattleIndices = battle.allSameSideBattlers(user.idxOpposingSide).map(b => b.pokemonIndex);
		foe_reserves = new List<string>();
		battle.Party(user.idxOpposingSide).each_with_index do |pkmn, idxParty|
			if (!pkmn || !pkmn.able() || inBattleIndices.Contains(idxParty)) continue;
			if (ai.trainer.medium_skill()) {
				if (pkmn.hasItem(Items.HEAVYDUTYBOOTS)) continue;
				if (ai.pokemon_airborne(pkmn)) continue;
				if (pkmn.hasAbility(Abilitys.MAGICGUARD)) continue;
			}
			foe_reserves.Add(pkmn);   // pkmn will be affected by Spikes
		}
		if (foe_reserves.empty()) next Battle.AI.MOVE_USELESS_SCORE;
		multiplier = new {10, 7, 5}[user.OpposingSide.effects.Spikes];
		score += (int)Math.Min(multiplier * foe_reserves.length, 30);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("AddToxicSpikesToFoeSide",
	block: (move, user, ai, battle) => {
		next user.OpposingSide.effects.ToxicSpikes >= 2;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("AddToxicSpikesToFoeSide",
	block: (score, move, user, ai, battle) => {
		inBattleIndices = battle.allSameSideBattlers(user.idxOpposingSide).map(b => b.pokemonIndex);
		foe_reserves = new List<string>();
		battle.Party(user.idxOpposingSide).each_with_index do |pkmn, idxParty|
			if (!pkmn || !pkmn.able() || inBattleIndices.Contains(idxParty)) continue;
			if (ai.trainer.medium_skill()) {
				if (pkmn.hasItem(Items.HEAVYDUTYBOOTS)) continue;
				if (ai.pokemon_airborne(pkmn)) continue;
				if (!ai.pokemon_can_be_poisoned(pkmn)) continue;
			}
			foe_reserves.Add(pkmn);   // pkmn will be affected by Toxic Spikes
		}
		if (foe_reserves.empty()) next Battle.AI.MOVE_USELESS_SCORE;
		multiplier = new {8, 5}[user.OpposingSide.effects.ToxicSpikes];
		score += (int)Math.Min(multiplier * foe_reserves.length, 30);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("AddStealthRocksToFoeSide",
	block: (move, user, ai, battle) => {
		next user.OpposingSide.effects.StealthRock;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("AddStealthRocksToFoeSide",
	block: (score, move, user, ai, battle) => {
		inBattleIndices = battle.allSameSideBattlers(user.idxOpposingSide).map(b => b.pokemonIndex);
		foe_reserves = new List<string>();
		battle.Party(user.idxOpposingSide).each_with_index do |pkmn, idxParty|
			if (!pkmn || !pkmn.able() || inBattleIndices.Contains(idxParty)) continue;
			if (ai.trainer.medium_skill()) {
				if (pkmn.hasItem(Items.HEAVYDUTYBOOTS)) continue;
				if (pkmn.hasAbility(Abilitys.MAGICGUARD)) continue;
			}
			foe_reserves.Add(pkmn);   // pkmn will be affected by Stealth Rock
		}
		if (foe_reserves.empty()) next Battle.AI.MOVE_USELESS_SCORE;
		score += (int)Math.Min(10 * foe_reserves.length, 30);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("AddStickyWebToFoeSide",
	block: (move, user, ai, battle) => {
		next user.OpposingSide.effects.StickyWeb;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("AddStickyWebToFoeSide",
	block: (score, move, user, ai, battle) => {
		inBattleIndices = battle.allSameSideBattlers(user.idxOpposingSide).map(b => b.pokemonIndex);
		foe_reserves = new List<string>();
		battle.Party(user.idxOpposingSide).each_with_index do |pkmn, idxParty|
			if (!pkmn || !pkmn.able() || inBattleIndices.Contains(idxParty)) continue;
			if (ai.trainer.medium_skill()) {
				if (pkmn.hasItem(Items.HEAVYDUTYBOOTS)) continue;
				if (ai.pokemon_airborne(pkmn)) continue;
			}
			foe_reserves.Add(pkmn);   // pkmn will be affected by Sticky Web
		}
		if (foe_reserves.empty()) next Battle.AI.MOVE_USELESS_SCORE;
		score += (int)Math.Min(8 * foe_reserves.length, 30);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("SwapSideEffects",
	block: (move, user, ai, battle) => {
		has_effect = false;
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			effects = battle.sides[side].effects;
			foreach (var e in move.move.number_effects) { //'move.move.number_effects.each' do => |e|
				if (effects[e] == 0) continue;
				has_effect = true;
				break;
			}
			if (has_effect) break;
			foreach (var e in move.move.boolean_effects) { //'move.move.boolean_effects.each' do => |e|
				if (!effects[e]) continue;
				has_effect = true;
				break;
			}
			if (has_effect) break;
		}
		next !has_effect;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("SwapSideEffects",
	block: (score, move, user, ai, battle) => {
		if (ai.trainer.medium_skill()) {
			good_effects = new {:AuroraVeil, :LightScreen, :Mist, :Rainbow, :Reflect,
											:Safeguard, :SeaOfFire, :Swamp, :Tailwind}.map! { |e| Effects.const_get(e) };
			bad_effects = new {:Spikes, :StealthRock, :StickyWeb, :ToxicSpikes}.map! { |e| Effects.const_get(e) };
			foreach (var e in bad_effects) { //'bad_effects.each' do => |e|
				if (!new []{0, false, null}.Contains(user.OwnSide.effects[e])) score += 10;
				if (!new []{0, 1, false, null}.Contains(user.OpposingSide.effects[e])) score -= 10;
			}
			if (ai.trainer.high_skill()) {
				foreach (var e in good_effects) { //'good_effects.each' do => |e|
					if (!new []{0, 1, false, null}.Contains(user.OpposingSide.effects[e])) score += 10;
					if (!new []{0, false, null}.Contains(user.OwnSide.effects[e])) score -= 10;
				}
			}
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserMakeSubstitute",
	block: (move, user, ai, battle) => {
		if (user.effects.Substitute > 0) next true;
		next user.hp <= (int)Math.Max(user.totalhp / 4, 1);
	}
)
Battle.AI.Handlers.MoveEffectScore.add("UserMakeSubstitute",
	block: (score, move, user, ai, battle) => {
		// Prefer more the higher the user's HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			score += (int)Math.Round(10 * user.hp.to_f / user.totalhp);
		}
		// Prefer if foes don't know any moves that can bypass a substitute
		ai.each_foe_battler(user.side) do |b, i|
			if (!b.check_for_move(m => m.ignoresSubstitute(b.battler))) score += 5;
		}
		// Prefer if the user lost more than a Substitute's worth of HP from the last
		// attack against it
		if (user.battler.lastHPLost >= user.totalhp / 4) score += 7;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("RemoveUserBindingAndEntryHazards",
	block: (score, move, user, ai, battle) => {
		// Score for raising user's Speed
		if (Settings.MECHANICS_GENERATION >= 8) {
			score = Battle.AI.Handlers.apply_move_effect_score("RaiseUserSpeed1",
				score, move, user, ai, battle);
		}
		// Score for removing various effects
		if (user.effects.Trapping > 0) score += 10;
		if (user.effects.LeechSeed >= 0) score += 15;
		if (battle.AbleNonActiveCount(user.idxOwnSide) > 0) {
			if (user.OwnSide.effects.Spikes > 0) score += 15;
			if (user.OwnSide.effects.ToxicSpikes > 0) score += 15;
			if (user.OwnSide.effects.StealthRock) score += 20;
			if (user.OwnSide.effects.StickyWeb) score += 15;
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureAgainstTargetCheck.add("AttackTwoTurnsLater",
	block: (move, user, target, ai, battle) => {
		next battle.positions[target.index].effects.FutureSightCounter > 0;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("AttackTwoTurnsLater",
	block: (score, move, user, ai, battle) => {
		// Future Sight tends to be wasteful if down to last Pokémon
		if (battle.AbleNonActiveCount(user.idxOwnSide) == 0) score -= 20;
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("UserSwapsPositionsWithAlly",
	block: (move, user, ai, battle) => {
		num_targets = 0;
		idxUserOwner = battle.GetOwnerIndexFromBattlerIndex(user.index);
		ai.each_ally(user.side) do |b, i|
			if (battle.GetOwnerIndexFromBattlerIndex(b.index) != idxUserOwner) continue;
			if (!b.battler.near(user.battler)) continue;
			num_targets += 1;
		}
		next num_targets != 1;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("UserSwapsPositionsWithAlly",
	block: (score, move, user, ai, battle) => {
		next score - 30;   // Usually no point in using this
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("BurnAttackerBeforeUserActs",
	block: (score, move, user, ai, battle) => {
		foreach (var b in ai.each_foe_battler(user.side)) { //ai.each_foe_battler(user.side) do => |b|
			if (!b.battler.affectedByContactEffect()) continue;
			if (!b.battler.CanBurn(user.battler, false, move.move)) continue;
			if (ai.trainer.high_skill()) {
				if (!b.check_for_move(m => m.ContactMove(b.battler))) continue;
			}
			score += 10;   // Possible to burn
		}
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("AllBattlersLoseHalfHPUserSkipsNextTurn",
	block: (score, move, user, ai, battle) => {
		// HP halving
		foe_hp_lost = 0;
		ally_hp_lost = 0;
		ai.each_battler do |b, i|
			if (b.hp == 1) continue;
			if (b.battler.opposes(user.battler)) {
				foe_hp_lost += b.hp / 2;
			} else {
				ally_hp_lost += b.hp / 2;
			}
		}
		score += 20 * foe_hp_lost / ally_hp_lost;
		score -= 20 * ally_hp_lost / foe_hp_lost;
		// Recharging
		score = Battle.AI.Handlers.apply_move_effect_score("AttackAndSkipNextTurn",
			score, move, user, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveEffectScore.add("UserLosesHalfHP",
	block: (score, move, user, ai, battle) => {
		score = Battle.AI.Handlers.apply_move_effect_score("UserLosesHalfOfTotalHP",
			score, move, user, ai, battle);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.copy("StartSunWeather",
																						"StartShadowSkyWeather");
Battle.AI.Handlers.MoveEffectScore.add("StartShadowSkyWeather",
	block: (score, move, user, ai, battle) => {
		if (battle.CheckGlobalAbility(Abilities.AIRLOCK) ||
																					battle.CheckGlobalAbility(Abilities.CLOUDNINE)) next Battle.AI.MOVE_USELESS_SCORE;
		// Not worth it at lower HP
		if (ai.trainer.has_skill_flag("HPAware")) {
			if (user.hp < user.totalhp / 2) score -= 15;
		}
		if (ai.trainer.high_skill() && battle.field.weather != weathers.None) {
			score -= ai.get_score_for_weather(battle.field.weather, user);
		}
		score += ai.get_score_for_weather(:ShadowSky, user, true);
		next score;
	}
)

//===============================================================================
//
//===============================================================================
Battle.AI.Handlers.MoveFailureCheck.add("RemoveAllScreensAndSafeguard",
	block: (move, user, ai, battle) => {
		will_fail = true;
		foreach (var side in battle.sides) { //'battle.sides.each' do => |side|
			if (side.effects.AuroraVeil > 0 ||
													side.effects.LightScreen > 0 ||
													side.effects.Reflect > 0 ||
													side.effects.Safeguard > 0) will_fail = false;
		}
		next will_fail;
	}
)
Battle.AI.Handlers.MoveEffectScore.add("RemoveAllScreensAndSafeguard",
	block: (score, move, user, ai, battle) => {
		foe_side = user.OpposingSide;
		// Useless if the foe's side has no screens/Safeguard to remove, or if
		// they'll end this round anyway
		if (foe_side.effects.AuroraVeil <= 1 &&
			foe_side.effects.LightScreen <= 1 &&
			foe_side.effects.Reflect <= 1 &&
			foe_side.effects.Safeguard <= 1) {
			next Battle.AI.MOVE_USELESS_SCORE;
		}
		// Prefer removing opposing screens
		score = Battle.AI.Handlers.apply_move_effect_score("RemoveScreens",
			score, move, user, ai, battle);
		// Don't prefer removing same side screens
		ai.each_foe_battler(user.side) do |b, i|
			score -= Battle.AI.Handlers.apply_move_effect_score("RemoveScreens",
				0, move, b, ai, battle);
			break;
		}
		// Safeguard
		if (foe_side.effects.Safeguard > 0) score += 10;
		if (user.OwnSide.effects.Safeguard > 0) score -= 10;
		next score;
	}
)
