//===============================================================================
//
//===============================================================================
public partial class Battle {
	//-----------------------------------------------------------------------------
	// End Of Round end weather check and weather effects.
	//-----------------------------------------------------------------------------

	public void EOREndWeather(priority) {
		// NOTE: Primordial weather doesn't need to be checked here, because if it
		//       could wear off here, it will have worn off already.
		// Count down weather duration
		if (@field.weatherDuration > 0) @field.weatherDuration -= 1;
		// Weather wears off
		if (@field.weatherDuration == 0) {
			switch (@field.weather) {
				case :Sun:        Display(_INTL("The sunlight faded.")); break;
				case :Rain:       Display(_INTL("The rain stopped.")); break;
				case :Sandstorm:  Display(_INTL("The sandstorm subsided.")); break;
				case :Hail:       Display(_INTL("The hail stopped.")); break;
				case :Snowstorm:  Display(_INTL("The snow stopped.")); break;
				case :ShadowSky:  Display(_INTL("The shadow sky faded.")); break;
			}
			@field.weather = weathers.None;
			// Check for form changes caused by the weather changing
			allBattlers.each(battler => battler.CheckFormOnWeatherChange);
			// Start up the default weather
			if (@field.defaultWeather != Weathers.None) StartWeather(null, @field.defaultWeather);
			if (@field.weather == weathers.None) return;
		}
		// Weather continues
		weather_data = GameData.BattleWeather.try_get(@field.weather);
		if (weather_data) CommonAnimation(weather_data.animation);
		switch (@field.weather) {
			case :Sun:          Display(_INTL("The sunlight is strong.")); break;
			case :Rain:         Display(_INTL("Rain continues to fall.")); break;
			case :Sandstorm:    Display(_INTL("The sandstorm is raging.")); break;
			case :Hail:         Display(_INTL("The hail is crashing down.")); break;
			case :Snowstorm:    Display(_INTL("The snow is blowing about!")); break;
			case :HarshSun:     Display(_INTL("The sunlight is extremely harsh.")); break;
			case :HeavyRain:    Display(_INTL("It is raining heavily.")); break;
			case :StrongWinds:  Display(_INTL("The wind is strong.")); break;
			case :ShadowSky:    Display(_INTL("The shadow sky continues.")); break;
		}
		// Effects due to weather
		foreach (var battler in priority) { //'priority.each' do => |battler|
			// Weather-related abilities
			if (battler.abilityActive()) {
				Battle.AbilityEffects.triggerEndOfRoundWeather(battler.ability, battler.effectiveWeather, battler, self);
				if (battler.fainted()) battler.Faint;
			}
			// Weather damage
			EORWeatherDamage(battler);
		}
	}

	public void EORWeatherDamage(battler) {
		if (battler.fainted()) return;
		amt = -1;
		switch (battler.effectiveWeather) {
			case :Sandstorm:
				if (!battler.takesSandstormDamage()) return;
				Display(_INTL("{1} is buffeted by the sandstorm!", battler.ToString()));
				amt = battler.totalhp / 16;
				break;
			case :Hail:
				if (!battler.takesHailDamage()) return;
				Display(_INTL("{1} is buffeted by the hail!", battler.ToString()));
				amt = battler.totalhp / 16;
				break;
			case :ShadowSky:
				if (!battler.takesShadowSkyDamage()) return;
				Display(_INTL("{1} is hurt by the shadow sky!", battler.ToString()));
				amt = battler.totalhp / 16;
				break;
		}
		if (amt < 0) return;
		@scene.DamageAnimation(battler);
		battler.ReduceHP(amt, false);
		battler.ItemHPHealCheck;
		if (battler.fainted()) battler.Faint;
	}

	//-----------------------------------------------------------------------------
	// End Of Round use delayed moves (Future Sight, Doom Desire).
	//-----------------------------------------------------------------------------

	public void EORUseFutureSight(position, position_index) {
		if (!position || position.effects.FutureSightCounter == 0) return;
		position.effects.FutureSightCounter -= 1;
		if (position.effects.FutureSightCounter > 0) return;
		if (!@battlers[position_index] || @battlers[position_index].fainted()) return;   // No target
		moveUser = null;
		foreach (var battler in allBattlers) { //'allBattlers.each' do => |battler|
			if (battler.opposes(position.effects.FutureSightUserIndex)) continue;
			if (battler.pokemonIndex != position.effects.FutureSightUserPartyIndex) continue;
			moveUser = battler;
			break;
		}
		if (moveUser && moveUser.index == position_index) return;   // Target is the user
		if (!moveUser) {   // User isn't in battle, get it from the party
			party = Party(position.effects.FutureSightUserIndex);
			pkmn = party[position.effects.FutureSightUserPartyIndex];
			if (pkmn&.able()) {
				moveUser = new Battler(self, position.effects.FutureSightUserIndex);
				moveUser.InitDummyPokemon(pkmn, position.effects.FutureSightUserPartyIndex);
			}
		}
		if (!moveUser) return;   // User is fainted
		move = position.effects.FutureSightMove;
		Display(_INTL("{1} took the {2} attack!", @battlers[position_index].ToString(),
										GameData.Move.get(move).name));
		// NOTE: Future Sight failing against the target here doesn't count towards
		//       Stomping Tantrum.
		userLastMoveFailed = moveUser.lastMoveFailed;
		@futureSight = true;
		moveUser.UseMoveSimple(move, position_index);
		@futureSight = false;
		moveUser.lastMoveFailed = userLastMoveFailed;
		if (@battlers[position_index].fainted()) @battlers[position_index].Faint;
		position.effects.FutureSightCounter        = 0;
		position.effects.FutureSightMove           = null;
		position.effects.FutureSightUserIndex      = -1;
		position.effects.FutureSightUserPartyIndex = -1;
	}

	//-----------------------------------------------------------------------------
	// End Of Round healing from Wish.
	//-----------------------------------------------------------------------------

	public void EORWishHealing() {
		@positions.each_with_index do |pos, idxPos|
			if (!pos || pos.effects.Wish == 0) continue;
			pos.effects.Wish -= 1;
			if (pos.effects.Wish > 0) continue;
			if (!@battlers[idxPos] || !@battlers[idxPos].canHeal()) continue;
			wishMaker = ThisEx(idxPos, pos.effects.WishMaker);
			@battlers[idxPos].RecoverHP(pos.effects.WishAmount);
			Display(_INTL("{1}'s wish came true!", wishMaker));
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round Sea of Fire damage (Fire Pledge + Grass Pledge combination).
	//-----------------------------------------------------------------------------

	public void EORSeaOfFireDamage(priority) {
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			if (sides[side].effects.SeaOfFire == 0) continue;
			sides[side].effects.SeaOfFire -= 1;
			if (sides[side].effects.SeaOfFire == 0) continue;
			if (side == 0) CommonAnimation("SeaOfFire");
			if (side == 1) CommonAnimation("SeaOfFireOpp");
			foreach (var battler in priority) { //'priority.each' do => |battler|
				if (battler.opposes(side)) continue;
				if (!battler.takesIndirectDamage() || battler.Type == Types.FIRE) continue;
				@scene.DamageAnimation(battler);
				battler.TakeEffectDamage(battler.totalhp / 8, false) do |hp_lost|
					Display(_INTL("{1} is hurt by the sea of fire!", battler.ToString()));
				}
			}
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round healing from Grassy Terrain.
	//-----------------------------------------------------------------------------

	public void EORTerrainHealing(battler) {
		if (battler.fainted()) return;
		// Grassy Terrain (healing)
		if (@field.terrain == :Grassy && battler.affectedByTerrain() && battler.canHeal()) {
			Debug.Log($"[Lingering effect] Grassy Terrain heals {battler.ToString(true)}");
			battler.RecoverHP(battler.totalhp / 16);
			Display(_INTL("{1}'s HP was restored.", battler.ToString()));
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round various healing effects.
	//-----------------------------------------------------------------------------

	public void EORHealingEffects(priority) {
		// Aqua Ring
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (!battler.effects.AquaRing) continue;
			if (!battler.canHeal()) continue;
			CommonAnimation("AquaRing", battler);
			hpGain = battler.totalhp / 16;
			if (battler.hasActiveItem(Items.BIGROOT)) hpGain = (int)Math.Floor(hpGain * 1.3);
			battler.RecoverHP(hpGain);
			Display(_INTL("Aqua Ring restored {1}'s HP!", battler.ToString(true)));
		}
		// Ingrain
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (!battler.effects.Ingrain) continue;
			if (!battler.canHeal()) continue;
			CommonAnimation("Ingrain", battler);
			hpGain = battler.totalhp / 16;
			if (battler.hasActiveItem(Items.BIGROOT)) hpGain = (int)Math.Floor(hpGain * 1.3);
			battler.RecoverHP(hpGain);
			Display(_INTL("{1} absorbed nutrients with its roots!", battler.ToString()));
		}
		// Leech Seed
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.effects.LeechSeed < 0) continue;
			if (!battler.takesIndirectDamage()) continue;
			recipient = @battlers[battler.effects.LeechSeed];
			if (!recipient || recipient.fainted()) continue;
			CommonAnimation("LeechSeed", recipient, battler);
			battler.TakeEffectDamage(battler.totalhp / 8) do |hp_lost|
				recipient.RecoverHPFromDrain(hp_lost, battler,
																			_INTL("{1}'s health is sapped by Leech Seed!", battler.ToString()));
				recipient.AbilitiesOnDamageTaken;
			}
			if (recipient.fainted()) recipient.Faint;
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round deal damage from status problems.
	//-----------------------------------------------------------------------------

	public void EORStatusProblemDamage(priority) {
		// Damage from poisoning
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.fainted()) continue;
			if (battler.status != statuses.POISON) continue;
			if (battler.statusCount > 0) {
				battler.effects.Toxic += 1;
				if (battler.effects.Toxic > 16) battler.effects.Toxic = 16;
			}
			if (battler.hasActiveAbility(Abilitys.POISONHEAL)) {
				if (battler.canHeal()) {
					anim_name = GameData.Status.get(:POISON).animation;
					if (anim_name) CommonAnimation(anim_name, battler);
					ShowAbilitySplash(battler);
					battler.RecoverHP(battler.totalhp / 8);
					if (Scene.USE_ABILITY_SPLASH) {
						Display(_INTL("{1}'s HP was restored.", battler.ToString()));
					} else {
						Display(_INTL("{1}'s {2} restored its HP.", battler.ToString(), battler.abilityName));
					}
					HideAbilitySplash(battler);
				}
			} else if (battler.takesIndirectDamage()) {
				battler.droppedBelowHalfHP = false;
				dmg = battler.totalhp / 8;
				if (battler.statusCount > 0) dmg = battler.totalhp * battler.effects.Toxic / 16;
				battler.ContinueStatus(() => battler.ReduceHP(dmg, false));
				battler.ItemHPHealCheck;
				battler.AbilitiesOnDamageTaken;
				if (battler.fainted()) battler.Faint;
				battler.droppedBelowHalfHP = false;
			}
		}
		// Damage from burn
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.status != statuses.BURN || !battler.takesIndirectDamage()) continue;
			battler.droppedBelowHalfHP = false;
			dmg = (Settings.MECHANICS_GENERATION >= 7) ? battler.totalhp / 16 : battler.totalhp / 8;
			if (battler.hasActiveAbility(Abilitys.HEATPROOF)) dmg = (int)Math.Round(dmg / 2.0);
			battler.ContinueStatus(() => battler.ReduceHP(dmg, false));
			battler.ItemHPHealCheck;
			battler.AbilitiesOnDamageTaken;
			if (battler.fainted()) battler.Faint;
			battler.droppedBelowHalfHP = false;
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round deal damage from effects (except by trapping).
	//-----------------------------------------------------------------------------

	public void EOREffectDamage(priority) {
		// Damage from sleep (Nightmare)
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (!battler.asleep()) battler.effects.Nightmare = false;
			if (!battler.effects.Nightmare || !battler.takesIndirectDamage()) continue;
			CommonAnimation("Nightmare", battler);
			battler.TakeEffectDamage(battler.totalhp / 4) do |hp_lost|
				Display(_INTL("{1} is locked in a nightmare!", battler.ToString()));
			}
		}
		// Curse
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (!battler.effects.Curse || !battler.takesIndirectDamage()) continue;
			CommonAnimation("Curse", battler);
			battler.TakeEffectDamage(battler.totalhp / 4) do |hp_lost|
				Display(_INTL("{1} is afflicted by the curse!", battler.ToString()));
			}
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round deal damage to trapped battlers.
	//-----------------------------------------------------------------------------

	TRAPPING_MOVE_COMMON_ANIMATIONS = {
		BIND        = "Bind",
		CLAMP       = "Clamp",
		FIRESPIN    = "FireSpin",
		MAGMASTORM  = "MagmaStorm",
		SANDTOMB    = "SandTomb",
		WRAP        = "Wrap",
		INFESTATION = "Infestation";
	}

	public void EORTrappingDamage(battler) {
		if (battler.fainted() || battler.effects.Trapping == 0) return;
		battler.effects.Trapping -= 1;
		move_name = GameData.Move.get(battler.effects.TrappingMove).name;
		if (battler.effects.Trapping == 0) {
			Display(_INTL("{1} was freed from {2}!", battler.ToString(), move_name));
			return;
		}
		anim = TRAPPING_MOVE_COMMON_ANIMATIONS[battler.effects.TrappingMove] || "Wrap";
		CommonAnimation(anim, battler);
		if (!battler.takesIndirectDamage()) return;
		hpLoss = (Settings.MECHANICS_GENERATION >= 6) ? battler.totalhp / 8 : battler.totalhp / 16;
		if (@battlers[battler.effects.TrappingUser].hasActiveItem(Items.BINDINGBAND)) {
			hpLoss = (Settings.MECHANICS_GENERATION >= 6) ? battler.totalhp / 6 : battler.totalhp / 8;
		}
		@scene.DamageAnimation(battler);
		battler.TakeEffectDamage(hpLoss, false) do |hp_lost|
			Display(_INTL("{1} is hurt by {2}!", battler.ToString(), move_name));
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round end effects that apply to a battler.
	//-----------------------------------------------------------------------------

	public void EORCountDownBattlerEffect(priority, effect) {
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.fainted() || battler.effects[effect] == 0) continue;
			battler.effects[effect] -= 1;
			if (block_given() && battler.effects[effect] == 0) yield battler;
		}
	}

	public void EOREndBattlerEffects(priority) {
		// Taunt
		EORCountDownBattlerEffect(priority, Effects.Taunt) do |battler|
			Display(_INTL("{1}'s taunt wore off!", battler.ToString()));
		}
		// Encore
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.fainted() || battler.effects.Encore == 0) continue;
			idxEncoreMove = battler.EncoredMoveIndex;
			if (idxEncoreMove >= 0) {
				battler.effects.Encore -= 1;
				if (battler.effects.Encore == 0 || battler.moves[idxEncoreMove].pp == 0) {
					battler.effects.Encore = 0;
					Display(_INTL("{1}'s encore ended!", battler.ToString()));
				}
			} else {
				Debug.Log($"[End of effect] {battler.ToString()}'s encore ended (encored move no longer known)");
				battler.effects.Encore     = 0;
				battler.effects.EncoreMove = null;
			}
		}
		// Disable/Cursed Body
		EORCountDownBattlerEffect(priority, Effects.Disable) do |battler|
			battler.effects.DisableMove = null;
			Display(_INTL("{1} is no longer disabled!", battler.ToString()));
		}
		// Magnet Rise
		EORCountDownBattlerEffect(priority, Effects.MagnetRise) do |battler|
			Display(_INTL("{1}'s electromagnetism wore off!", battler.ToString()));
		}
		// Telekinesis
		EORCountDownBattlerEffect(priority, Effects.Telekinesis) do |battler|
			Display(_INTL("{1} was freed from the telekinesis!", battler.ToString()));
		}
		// Heal Block
		EORCountDownBattlerEffect(priority, Effects.HealBlock) do |battler|
			Display(_INTL("{1}'s Heal Block wore off!", battler.ToString()));
		}
		// Embargo
		EORCountDownBattlerEffect(priority, Effects.Embargo) do |battler|
			Display(_INTL("{1} can use items again!", battler.ToString()));
			battler.ItemTerrainStatBoostCheck;
		}
		// Yawn
		EORCountDownBattlerEffect(priority, Effects.Yawn) do |battler|
			if (battler.CanSleepYawn()) {
				Debug.Log($"[Lingering effect] {battler.ToString()} fell asleep because of Yawn");
				battler.Sleep;
			}
		}
		// Perish Song
		perishSongUsers = new List<string>();
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.fainted() || battler.effects.PerishSong == 0) continue;
			battler.effects.PerishSong -= 1;
			Display(_INTL("{1}'s perish count fell to {2}!", battler.ToString(), battler.effects.PerishSong));
			if (battler.effects.PerishSong == 0) {
				perishSongUsers.Add(battler.effects.PerishSongUser);
				battler.ReduceHP(battler.hp);
			}
			battler.ItemHPHealCheck;
			if (battler.fainted()) battler.Faint;
		}
		// Judge if all remaining Pokemon fainted by a Perish Song triggered by a single side
		if (perishSongUsers.length > 0 &&
			((perishSongUsers.find_all(idxBattler => opposes(idxBattler)).length == perishSongUsers.length) ||
			(perishSongUsers.find_all(idxBattler => !opposes(idxBattler)).length == perishSongUsers.length))) {
			JudgeCheckpoint(@battlers[perishSongUsers[0]]);
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round end effects that apply to one side of the field.
	//-----------------------------------------------------------------------------

	public void EORCountDownSideEffect(side, effect, msg) {
		if (@sides[side].effects[effect] <= 0) return;
		@sides[side].effects[effect] -= 1;
		if (@sides[side].effects[effect] == 0) Display(msg);
	}

	public void EOREndSideEffects(side, priority) {
		// Reflect
		EORCountDownSideEffect(side, Effects.Reflect,
														_INTL("{1}'s Reflect wore off!", @battlers[side].Team));
		// Light Screen
		EORCountDownSideEffect(side, Effects.LightScreen,
														_INTL("{1}'s Light Screen wore off!", @battlers[side].Team));
		// Safeguard
		EORCountDownSideEffect(side, Effects.Safeguard,
														_INTL("{1} is no longer protected by Safeguard!", @battlers[side].Team));
		// Mist
		EORCountDownSideEffect(side, Effects.Mist,
														_INTL("{1} is no longer protected by mist!", @battlers[side].Team));
		// Tailwind
		EORCountDownSideEffect(side, Effects.Tailwind,
														_INTL("{1}'s Tailwind petered out!", @battlers[side].Team));
		// Lucky Chant
		EORCountDownSideEffect(side, Effects.LuckyChant,
														_INTL("{1}'s Lucky Chant wore off!", @battlers[side].Team));
		// Pledge Rainbow
		EORCountDownSideEffect(side, Effects.Rainbow,
														_INTL("The rainbow on {1}'s side disappeared!", @battlers[side].Team(true)));
		// Pledge Sea of Fire
		EORCountDownSideEffect(side, Effects.SeaOfFire,
														_INTL("The sea of fire around {1} disappeared!", @battlers[side].Team(true)));
		// Pledge Swamp
		EORCountDownSideEffect(side, Effects.Swamp,
														_INTL("The swamp around {1} disappeared!", @battlers[side].Team(true)));
		// Aurora Veil
		EORCountDownSideEffect(side, Effects.AuroraVeil,
														_INTL("{1}'s Aurora Veil wore off!", @battlers[side].Team));
	}

	//-----------------------------------------------------------------------------
	// End Of Round end effects that apply to the whole field.
	//-----------------------------------------------------------------------------

	public void EORCountDownFieldEffect(effect, msg) {
		if (@field.effects[effect] <= 0) return;
		@field.effects[effect] -= 1;
		if (@field.effects[effect] > 0) return;
		Display(msg);
		if (effect == Effects.MagicRoom) {
			Priority(true).each(battler => battler.ItemTerrainStatBoostCheck);
		}
	}

	public void EOREndFieldEffects(priority) {
		// Trick Room
		EORCountDownFieldEffect(Effects.TrickRoom,
															_INTL("The twisted dimensions returned to normal!"));
		// Gravity
		EORCountDownFieldEffect(Effects.Gravity,
															_INTL("Gravity returned to normal!"));
		// Water Sport
		EORCountDownFieldEffect(Effects.WaterSportField,
															_INTL("The effects of Water Sport have faded."));
		// Mud Sport
		EORCountDownFieldEffect(Effects.MudSportField,
															_INTL("The effects of Mud Sport have faded."));
		// Wonder Room
		EORCountDownFieldEffect(Effects.WonderRoom,
															_INTL("Wonder Room wore off, and Defense and Sp. Def stats returned to normal!"));
		// Magic Room
		EORCountDownFieldEffect(Effects.MagicRoom,
															_INTL("Magic Room wore off, and held items' effects returned to normal!"));
	}

	//-----------------------------------------------------------------------------
	// End Of Round end terrain check.
	//-----------------------------------------------------------------------------

	public void EOREndTerrain() {
		// Count down terrain duration
		if (@field.terrainDuration > 0) @field.terrainDuration -= 1;
		// Terrain wears off
		if (@field.terrain != :None && @field.terrainDuration == 0) {
			switch (@field.terrain) {
				case :Electric:
					Display(_INTL("The electric current disappeared from the battlefield!"));
					break;
				case :Grassy:
					Display(_INTL("The grass disappeared from the battlefield!"));
					break;
				case :Misty:
					Display(_INTL("The mist disappeared from the battlefield!"));
					break;
				case :Psychic:
					Display(_INTL("The weirdness disappeared from the battlefield!"));
					break;
			}
			@field.terrain = :None;
			allBattlers.each(battler => battler.AbilityOnTerrainChange);
			// Start up the default terrain
			if (@field.defaultTerrain != :None) {
				StartTerrain(null, @field.defaultTerrain, false);
				allBattlers.each(battler => battler.AbilityOnTerrainChange);
				allBattlers.each(battler => battler.ItemTerrainStatBoostCheck);
			}
			if (@field.terrain == :None) return;
		}
		// Terrain continues
		terrain_data = GameData.BattleTerrain.try_get(@field.terrain);
		if (terrain_data) CommonAnimation(terrain_data.animation);
		switch (@field.terrain) {
			case :Electric:  Display(_INTL("An electric current is running across the battlefield.")); break;
			case :Grassy:    Display(_INTL("Grass is covering the battlefield.")); break;
			case :Misty:     Display(_INTL("Mist is swirling about the battlefield.")); break;
			case :Psychic:   Display(_INTL("The battlefield is weird.")); break;
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round end self-inflicted effects on battler.
	//-----------------------------------------------------------------------------

	public void EOREndBattlerSelfEffects(battler) {
		if (battler.fainted()) return;
		// Hyper Mode (Shadow Pokémon)
		if (battler.inHyperMode()) {
			if (Random(100) < 10) {
				battler.pokemon.hyper_mode = false;
				Display(_INTL("{1} came to its senses!", battler.ToString()));
			} else {
				Display(_INTL("{1} is in Hyper Mode!", battler.ToString()));
			}
		}
		// Uproar
		if (battler.effects.Uproar > 0) {
			battler.effects.Uproar -= 1;
			if (battler.effects.Uproar == 0) {
				Display(_INTL("{1} calmed down.", battler.ToString()));
			} else {
				Display(_INTL("{1} is making an uproar!", battler.ToString()));
			}
		}
		// Slow Start's end message
		if (battler.effects.SlowStart > 0) {
			battler.effects.SlowStart -= 1;
			if (battler.effects.SlowStart == 0) {
				Display(_INTL("{1} finally got its act together!", battler.ToString()));
			}
		}
	}

	//-----------------------------------------------------------------------------
	// End Of Round shift distant battlers to middle positions.
	//-----------------------------------------------------------------------------

	public void EORShiftDistantBattlers() {
		// Move battlers around if none are near to each other
		// NOTE: This code assumes each side has a maximum of 3 battlers on it, and
		//       is not generalised to larger side sizes.
		if (!singleBattle()) {
			swaps = new List<string>();   // Each element is an array of two battler indices to swap
			for (int side = 2; side < 2; side++) { //for '2' times do => |side|
				if (SideSize(side) == 1) continue;   // Only battlers on sides of size 2+ need to move
				// Check if any battler on this side is near any battler on the other side
				anyNear = false;
				foreach (var battler in allSameSideBattlers(side)) { //'allSameSideBattlers(side).each' do => |battler|
					anyNear = allOtherSideBattlers(battler).any(other => nearBattlers(other.index, battler.index));
					if (anyNear) break;
				}
				if (anyNear) break;
				// No battlers on this side are near any battlers on the other side; try
				// to move them
				// NOTE: If we get to here (assuming both sides are of size 3 or less),
				//       there is definitely only 1 able battler on this side, so we
				//       don't need to worry about multiple battlers trying to move into
				//       the same position. If you add support for a side of size 4+,
				//       this code will need revising to account for that, as well as to
				//       add more complex code to ensure battlers will end up near each
				//       other.
				foreach (var battler in allSameSideBattlers(side)) { //'allSameSideBattlers(side).each' do => |battler|
					// Get the position to move to
					pos = -1;
					switch (SideSize(side)) {
						case 2:  pos = new {2, 3, 0, 1}[battler.index]; break;   // The unoccupied position
						case 3:  pos = (side == 0) ? 2 : 3; break;    // The centre position
					}
					if (pos < 0) continue;
					// Can't move if the same trainer doesn't control both positions
					idxOwner = GetOwnerIndexFromBattlerIndex(battler.index);
					if (GetOwnerIndexFromBattlerIndex(pos) != idxOwner) continue;
					swaps.Add(new {battler.index, pos});
				}
			}
			// Move battlers around
			foreach (var pair in swaps) { //'swaps.each' do => |pair|
				if (SideSize(pair[0]) == 2 && swaps.length > 1) continue;
				if (!SwapBattlers(pair[0], pair[1])) continue;
				switch (SideSize(pair[1])) {
					case 2:
						Display(_INTL("{1} moved across!", @battlers[pair[1]].ToString()));
						break;
					case 3:
						Display(_INTL("{1} moved to the center!", @battlers[pair[1]].ToString()));
						break;
				}
			}
		}
	}

	//-----------------------------------------------------------------------------
	// Main End Of Round phase method.
	//-----------------------------------------------------------------------------

	public void EndOfRoundPhase() {
		Debug.Log($"");
		Debug.Log($"[End of round {@turnCount + 1}]");
		@endOfRound = true;
		@scene.BeginEndOfRoundPhase;
		CalculatePriority;           // recalculate speeds
		priority = Priority(true);   // in order of fastest -> slowest speeds only
		// Weather
		EOREndWeather(priority);
		// Future Sight/Doom Desire
		@positions.each_with_index((pos, idxPos) => EORUseFutureSight(pos, idxPos));
		// Wish
		EORWishHealing;
		// Sea of Fire damage (Fire Pledge + Grass Pledge combination)
		EORSeaOfFireDamage(priority);
		// Status-curing effects/abilities and HP-healing items
		foreach (var battler in priority) { //'priority.each' do => |battler|
			EORTerrainHealing(battler);
			// Healer, Hydration, Shed Skin
			if (battler.abilityActive()) {
				Battle.AbilityEffects.triggerEndOfRoundHealing(battler.ability, battler, self);
			}
			// Black Sludge, Leftovers
			if (battler.itemActive()) {
				Battle.ItemEffects.triggerEndOfRoundHealing(battler.item, battler, self);
			}
		}
		// Self-curing of status due to affection
		if (Settings.AFFECTION_EFFECTS && @internalBattle) {
			foreach (var battler in priority) { //'priority.each' do => |battler|
				if (battler.fainted() || battler.status == statuses.NONE) continue;
				if (!battler.OwnedByPlayer() || battler.affection_level < 4 || battler.mega()) continue;
				if (Random(100) < 80) continue;
				old_status = battler.status;
				battler.CureStatus(false);
				switch (old_status) {
					case :SLEEP:
						Display(_INTL("{1} shook itself awake so you wouldn't worry!", battler.ToString()));
						break;
					case :POISON:
						Display(_INTL("{1} managed to expel the poison so you wouldn't worry!", battler.ToString()));
						break;
					case :BURN:
						Display(_INTL("{1} healed its burn with its sheer determination so you wouldn't worry!", battler.ToString()));
						break;
					case :PARALYSIS:
						Display(_INTL("{1} gathered all its energy to break through its paralysis so you wouldn't worry!", battler.ToString()));
						break;
					case :FROZEN:
						Display(_INTL("{1} melted the ice with its fiery determination so you wouldn't worry!", battler.ToString()));
						break;
				}
			}
		}
		// Healing from Aqua Ring, Ingrain, Leech Seed
		EORHealingEffects(priority);
		// Damage from Hyper Mode (Shadow Pokémon)
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (!battler.inHyperMode() || @choices[battler.index].Action != :UseMove) continue;
			hpLoss = battler.totalhp / 24;
			@scene.DamageAnimation(battler);
			battler.ReduceHP(hpLoss, false);
			Display(_INTL("The Hyper Mode attack hurts {1}!", battler.ToString(true)));
			if (battler.fainted()) battler.Faint;
		}
		// Damage from poison/burn
		EORStatusProblemDamage(priority);
		// Damage from Nightmare and Curse
		EOREffectDamage(priority);
		// Trapping attacks (Bind/Clamp/Fire Spin/Magma Storm/Sand Tomb/Whirlpool/Wrap)
		priority.each(battler => EORTrappingDamage(battler));
		// Octolock
		foreach (var battler in priority) { //'priority.each' do => |battler|
			if (battler.fainted() || battler.effects.Octolock < 0) continue;
			CommonAnimation("Octolock", battler);
			if (battler.CanLowerStatStage(:DEFENSE)) battler.LowerStatStage(:DEFENSE, 1, null);
			if (battler.CanLowerStatStage(:SPECIAL_DEFENSE)) battler.LowerStatStage(:SPECIAL_DEFENSE, 1, null);
			battler.ItemOnStatDropped;
		}
		// Effects that apply to a battler that wear off after a number of rounds
		EOREndBattlerEffects(priority);
		// Check for end of battle (i.e. because of Perish Song)
		if (decided()) {
			GainExp;
			return;
		}
		// Effects that apply to a side that wear off after a number of rounds
		2.times(side => EOREndSideEffects(side, priority));
		// Effects that apply to the whole field that wear off after a number of rounds
		EOREndFieldEffects(priority);
		// End of terrains
		EOREndTerrain;
		foreach (var battler in priority) { //'priority.each' do => |battler|
			// Self-inflicted effects that wear off after a number of rounds
			EOREndBattlerSelfEffects(battler);
			// Bad Dreams, Moody, Speed Boost
			if (battler.abilityActive()) {
				Battle.AbilityEffects.triggerEndOfRoundEffect(battler.ability, battler, self);
			}
			// Flame Orb, Sticky Barb, Toxic Orb
			if (battler.itemActive()) {
				Battle.ItemEffects.triggerEndOfRoundEffect(battler.item, battler, self);
			}
			// Harvest, Pickup, Ball Fetch
			if (battler.abilityActive()) {
				Battle.AbilityEffects.triggerEndOfRoundGainItem(battler.ability, battler, self);
			}
		}
		GainExp;
		if (decided()) return;
		// Form checks
		priority.each(battler => battler.CheckForm(true));
		// Switch Pokémon in if possible
		EORSwitch;
		if (decided()) return;
		// In battles with at least one side of size 3+, move battlers around if none
		// are near to any foes
		EORShiftDistantBattlers;
		// Try to make Trace work, check for end of primordial weather
		priority.each(battler => battler.ContinualAbilityChecks);
		// Reset/count down battler-specific effects (no messages)
		foreach (var battler in allBattlers) { //'allBattlers.each' do => |battler|
			battler.effects.BanefulBunker    = false;
			battler.effects.BurningBulwark   = false;
			if (Settings.MECHANICS_GENERATION >= 9) {
				if (battler.effects.Charge > 1) battler.effects.Charge         -= 1;
			} else {
				if (battler.effects.Charge > 0) battler.effects.Charge         -= 1;
			}
			battler.effects.Counter          = -1;
			battler.effects.CounterTarget    = -1;
			battler.effects.Electrify        = false;
			battler.effects.Endure           = false;
			battler.effects.FirstPledge      = null;
			battler.effects.Flinch           = false;
			battler.effects.FocusPunch       = false;
			battler.effects.FollowMe         = 0;
			battler.effects.HelpingHand      = false;
			if (battler.effects.HyperBeam > 0) battler.effects.HyperBeam        -= 1;
			battler.effects.KingsShield      = false;
			if (battler.effects.LaserFocus > 0) battler.effects.LaserFocus       -= 1;
			if (battler.effects.LockOn > 0) {   // Also Mind Reader
				battler.effects.LockOn         -= 1;
				if (battler.effects.LockOn == 0) battler.effects.LockOnPos      = -1;
			}
			battler.effects.MagicBounce      = false;
			battler.effects.MagicCoat        = false;
			battler.effects.MirrorCoat       = -1;
			battler.effects.MirrorCoatTarget = -1;
			battler.effects.Obstruct         = false;
			battler.effects.Powder           = false;
			battler.effects.Prankster        = false;
			battler.effects.PriorityAbility  = false;
			battler.effects.PriorityItem     = false;
			battler.effects.Protect          = false;
			battler.effects.RagePowder       = false;
			battler.effects.Roost            = false;
			battler.effects.SilkTrap         = false;
			battler.effects.Snatch           = 0;
			battler.effects.SpikyShield      = false;
			battler.effects.Spotlight        = 0;
			if (battler.effects.ThroatChop > 0) battler.effects.ThroatChop       -= 1;
			battler.lastHPLost                           = 0;
			battler.lastHPLostFromFoe                    = 0;
			battler.droppedBelowHalfHP                   = false;
			battler.statsDropped                         = false;
			battler.tookMoveDamageThisRound              = false;
			battler.tookDamageThisRound                  = false;
			battler.tookPhysicalHit                      = false;
			battler.statsRaisedThisRound                 = false;
			battler.statsLoweredThisRound                = false;
			battler.canRestoreIceFace                    = false;
			battler.lastRoundMoveFailed                  = battler.lastMoveFailed;
			battler.lastAttacker.clear;
			battler.lastFoeAttacker.clear;
		}
		// Reset/count down side-specific effects (no messages)
		for (int side = 2; side < 2; side++) { //for '2' times do => |side|
			@sides[side].effects.CraftyShield         = false;
			if (!@sides[side].effects.EchoedVoiceUsed) {
				@sides[side].effects.EchoedVoiceCounter = 0;
			}
			@sides[side].effects.EchoedVoiceUsed      = false;
			@sides[side].effects.MatBlock             = false;
			@sides[side].effects.QuickGuard           = false;
			@sides[side].effects.Round                = false;
			@sides[side].effects.WideGuard            = false;
		}
		// Reset/count down field-specific effects (no messages)
		@field.effects.IonDeluge   = false;
		if (@field.effects.FairyLock > 0) @field.effects.FairyLock   -= 1;
		@field.effects.FusionBolt  = false;
		@field.effects.FusionFlare = false;
		@endOfRound = false;
	}
}
