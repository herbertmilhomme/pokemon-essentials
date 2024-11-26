//===============================================================================
//
//===============================================================================
public partial class Battle.AI.AIBattler {
	public int index		{ get { return _index; } set { _index = value; } }			protected int _index;
	public int side		{ get { return _side; } }			protected int _side;
	public int party_index		{ get { return _party_index; } }			protected int _party_index;
	public int battler		{ get { return _battler; } }			protected int _battler;

	public void initialize(ai, index) {
		@ai = ai;
		@index = index;
		@side = (@ai.battle.opposes(@index)) ? 1 : 0;
		refresh_battler;
	}

	public void refresh_battler() {
		old_party_index = @party_index;
		@battler = @ai.battle.battlers[@index];
		@party_index = battler.pokemonIndex;
	}

	public int pokemon     { get { return battler.pokemon;     } }
	public int level       { get { return battler.level;       } }
	public int hp          { get { return battler.hp;          } }
	public int totalhp     { get { return battler.totalhp;     } }
	public bool fainted() {    return battler.fainted();    }
	public int status      { get { return battler.status;      } }
	public int statusCount { get { return battler.statusCount; } }
	public int gender      { get { return battler.gender;      } }
	public int turnCount   { get { return battler.turnCount;   } }
	public int effects     { get { return battler.effects;     } }
	public int stages      { get { return battler.stages;      } }
	public bool statStageAtMax(stat) {  return battler.statStageAtMax(stat); }
	public bool statStageAtMin(stat) {  return battler.statStageAtMin(stat); }
	public int moves       { get { return battler.moves;       } }

	public bool wild() {
		return @ai.battle.wildBattle() && opposes();
	}

	public void name() {
		return string.Format("{0} ({0})", battler.name, @index);
	}

	public bool opposes(other = null) {
		if (other.null()) return @side == 1;
		return other.side != @side;
	}

	public int idxOwnSide      { get { return battler.idxOwnSide;      } }
	public int OwnSide       { get { return battler.OwnSide;       } }
	public int idxOpposingSide { get { return battler.idxOpposingSide; } }
	public int OpposingSide  { get { return battler.OpposingSide;  } }

	//-----------------------------------------------------------------------------

	// Returns how much damage this battler will take at the end of this round.
	public void rough_end_of_round_damage() {
		ret = 0;
		// Weather
		weather = battler.effectiveWeather;
		if (@ai.battle.field.weatherDuration == 1) {
			weather = @ai.battle.field.defaultWeather;
			if (@ai.battle.allBattlers.any(b => b.hasActiveAbility(new {:CLOUDNINE, :AIRLOCK}))) weather = weathers.None;
			if (new []{:Sun, :Rain, :HarshSun, :HeavyRain}.Contains(weather) && has_active_item(items.UTILITYUMBRELLA)) weather = weathers.None;
		}
		switch (weather) {
			case :Sandstorm:
				if (battler.takesSandstormDamage()) ret += (int)Math.Max(self.totalhp / 16, 1);
				break;
			case :Hail:
				if (battler.takesHailDamage()) ret += (int)Math.Max(self.totalhp / 16, 1);
				break;
			case :ShadowSky:
				if (battler.takesShadowSkyDamage()) ret += (int)Math.Max(self.totalhp / 16, 1);
				break;
		}
		switch (ability_id) {
			case :DRYSKIN:
				if (new []{:Sun, :HarshSun}.Contains(weather) && battler.takesIndirectDamage()) ret += (int)Math.Max(self.totalhp / 8, 1);
				if (new []{:Rain, :HeavyRain}.Contains(weather) && battler.canHeal()) ret -= (int)Math.Max(self.totalhp / 8, 1);
				break;
			case :ICEBODY:
				if (new []{:Hail, :Snowstorm}.Contains(weather) && battler.canHeal()) ret -= (int)Math.Max(self.totalhp / 16, 1);
				break;
			case :RAINDISH:
				if (new []{:Rain, :HeavyRain}.Contains(weather) && battler.canHeal()) ret -= (int)Math.Max(self.totalhp / 16, 1);
				break;
			case :SOLARPOWER:
				if (new []{:Sun, :HarshSun}.Contains(weather) && battler.takesIndirectDamage()) ret += (int)Math.Max(self.totalhp / 8, 1);
				break;
		}
		// Future Sight/Doom Desire
		// NOTE: Not worth estimating the damage from this.
		// Wish
		if (@ai.battle.positions[@index].effects.Wish == 1 && battler.canHeal()) {
			ret -= @ai.battle.positions[@index].effects.WishAmount;
		}
		// Sea of Fire
		if (@ai.battle.sides[@side].effects.SeaOfFire > 1 &&
			battler.takesIndirectDamage() && !has_type(types.FIRE)) {
			ret += (int)Math.Max(self.totalhp / 8, 1);
		}
		// Grassy Terrain (healing)
		if (@ai.battle.field.terrain == :Grassy && battler.affectedByTerrain() && battler.canHeal()) {
			ret -= (int)Math.Max(self.totalhp / 16, 1);
		}
		// Leftovers/Black Sludge
		if (has_active_item(items.BLACKSLUDGE)) {
			if (has_type(types.POISON)) {
				if (battler.canHeal()) ret -= (int)Math.Max(self.totalhp / 16, 1);
			} else {
				if (battler.takesIndirectDamage()) ret += (int)Math.Max(self.totalhp / 8, 1);
			}
		} else if (has_active_item(items.LEFTOVERS)) {
			if (battler.canHeal()) ret -= (int)Math.Max(self.totalhp / 16, 1);
		}
		// Aqua Ring
		if (self.effects.AquaRing && battler.canHeal()) {
			amt = self.totalhp / 16;
			if (has_active_item(items.BIGROOT)) amt = (int)Math.Floor(amt * 1.3);
			ret -= (int)Math.Max(amt, 1);
		}
		// Ingrain
		if (self.effects.Ingrain && battler.canHeal()) {
			amt = self.totalhp / 16;
			if (has_active_item(items.BIGROOT)) amt = (int)Math.Floor(amt * 1.3);
			ret -= (int)Math.Max(amt, 1);
		}
		// Leech Seed
		if (self.effects.LeechSeed >= 0) {
			if (battler.takesIndirectDamage()) {
				if (battler.takesIndirectDamage()) ret += (int)Math.Max(self.totalhp / 8, 1);
			}
		} else {
			@ai.each_battler do |b, i|
				if (i == @index || b.effects.LeechSeed != @index) continue;
				amt = (int)Math.Max((int)Math.Min(b.totalhp / 8, b.hp), 1);
				if (has_active_item(items.BIGROOT)) amt = (int)Math.Floor(amt * 1.3);
				ret -= (int)Math.Max(amt, 1);
			}
		}
		// Hyper Mode (Shadow Pokémon)
		if (battler.inHyperMode()) {
			ret += (int)Math.Max(self.totalhp / 24, 1);
		}
		// Poison/burn/Nightmare
		if (self.status == statuses.POISON) {
			if (has_active_ability(abilitys.POISONHEAL)) {
				if (battler.canHeal()) ret -= (int)Math.Max(self.totalhp / 8, 1);
			} else if (battler.takesIndirectDamage()) {
				mult = 2;
				if (self.statusCount > 0) mult = (int)Math.Min(self.effects.Toxic + 1, 16);   // Toxic
				ret += (int)Math.Max(mult * self.totalhp / 16, 1);
			}
		} else if (self.status == statuses.BURN) {
			if (battler.takesIndirectDamage()) {
				amt = (Settings.MECHANICS_GENERATION >= 7) ? self.totalhp / 16 : self.totalhp / 8;
				if (has_active_ability(abilitys.HEATPROOF)) amt = (int)Math.Round(amt / 2.0);
				ret += (int)Math.Max(amt, 1);
			}
		} else if (battler.asleep() && self.statusCount > 1 && self.effects.Nightmare) {
			if (battler.takesIndirectDamage()) ret += (int)Math.Max(self.totalhp / 4, 1);
		}
		// Curse
		if (self.effects.Curse) {
			if (battler.takesIndirectDamage()) ret += (int)Math.Max(self.totalhp / 4, 1);
		}
		// Trapping damage
		if (self.effects.Trapping > 1 && battler.takesIndirectDamage()) {
			amt = (Settings.MECHANICS_GENERATION >= 6) ? self.totalhp / 8 : self.totalhp / 16;
			if (@ai.battlers[self.effects.TrappingUser].has_active_item(items.BINDINGBAND)) {
				amt = (Settings.MECHANICS_GENERATION >= 6) ? self.totalhp / 6 : self.totalhp / 8;
			}
			ret += (int)Math.Max(amt, 1);
		}
		// Perish Song
		if (self.effects.PerishSong == 1) return 999_999;
		// Bad Dreams
		if (battler.asleep() && self.statusCount > 1 && battler.takesIndirectDamage()) {
			@ai.each_battler do |b, i|
				if (i == @index || !b.battler.near(battler) || !b.has_active_ability(abilitys.BADDREAMS)) continue;
				ret += (int)Math.Max(self.totalhp / 8, 1);
			}
		}
		// Sticky Barb
		if (has_active_item(items.STICKYBARB) && battler.takesIndirectDamage()) {
			ret += (int)Math.Max(self.totalhp / 8, 1);
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	public void base_stat(stat) {
		ret = 0;
		switch (stat) {
			case :ATTACK:           ret = battler.attack; break;
			case :DEFENSE:          ret = battler.defense; break;
			case :SPECIAL_ATTACK:   ret = battler.spatk; break;
			case :SPECIAL_DEFENSE:  ret = battler.spdef; break;
			case :SPEED:            ret = battler.speed; break;
		}
		return ret;
	}

	public void rough_stat(stat) {
		if (stat == :SPEED && @ai.trainer.high_skill()) return battler.Speed;
		stage_mul = Battle.Battler.STAT_STAGE_MULTIPLIERS;
		stage_div = Battle.Battler.STAT_STAGE_DIVISORS;
		if (new []{:ACCURACY, :EVASION}.Contains(stat)) {
			stage_mul = Battle.Battler.ACC_EVA_STAGE_MULTIPLIERS;
			stage_div = Battle.Battler.ACC_EVA_STAGE_DIVISORS;
		}
		stage = battler.stages[stat] + Battle.Battler.STAT_STAGE_MAXIMUM;
		value = base_stat(stat);
		return (int)Math.Floor(value.to_f * stage_mul[stage] / stage_div[stage]);
	}

	public bool faster_than(other) {
		if (other.null()) return false;
		this_speed  = rough_stat(:SPEED);
		other_speed = other.rough_stat(:SPEED);
		return (this_speed > other_speed) ^ (@ai.battle.field.effects.TrickRoom > 0);
	}

	//-----------------------------------------------------------------------------

	public int types { get { return battler.types; } }
	public void Types(withExtraType = false) {return battler.Types(withExtraType); }

	public bool has_type(type) {
		if (!type) return false;
		active_types = Types(true);
		return active_types.Contains(GameData.Type.get(type).id);
	}
	alias HasType() has_type();

	public void effectiveness_of_type_against_battler(type, user = null, move = null) {
		ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		if (!type) return ret;
		if (type == types.GROUND && has_type(types.FLYING) && has_active_item(items.IRONBALL)) return ret;
		// Get effectivenesses
		if (type == types.SHADOW) {
			if (battler.shadowPokemon()) {
				ret = Effectiveness.NOT_VERY_EFFECTIVE_MULTIPLIER;
			} else {
				ret = Effectiveness.SUPER_EFFECTIVE_MULTIPLIER;
			}
		} else {
			foreach (var defend_type in battler.Types(true)) { //'battler.Types(true).each' do => |defend_type|
				mult = effectiveness_of_type_against_single_battler_type(type, defend_type, user);
				if (move) {
					switch (move.function_code) {
						case "HitsTargetInSkyGroundsTarget":
							if (type == types.GROUND && defend_type == types.FLYING) mult = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
							break;
						case "FreezeTargetSuperEffectiveAgainstWater":
							if (defend_type == types.WATER) mult = Effectiveness.SUPER_EFFECTIVE_MULTIPLIER;
							break;
					}
				}
				ret *= mult;
			}
			if (self.effects.TarShot && type == types.FIRE) ret *= 2;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	public int ability_id { get { return battler.ability_id; } }
	public int ability    { get { return battler.ability;    } }

	public bool ability_active() {
		return battler.abilityActive();
	}

	public bool has_active_ability(ability, ignore_fainted = false) {
		return battler.hasActiveAbility(ability, ignore_fainted);
	}

	public bool has_mold_breaker() {
		return battler.hasMoldBreaker();
	}

	public bool being_mold_broken() {
		return battler.beingMoldBroken();
	}

	//-----------------------------------------------------------------------------

	public int item_id { get { return battler.item_id; } }
	public int item    { get { return battler.item;    } }

	public bool item_active() {
		return battler.itemActive();
	}

	public bool has_active_item(item) {
		return battler.hasActiveItem(item);
	}

	//-----------------------------------------------------------------------------

	public void check_for_move() {
		ret = false;
		foreach (var move in battler.Moves) { //battler.eachMove do => |move|
			if (move.pp == 0 && move.total_pp > 0) continue;
			unless (yield move) continue;
			ret = true;
			break;
		}
		return ret;
	}

	public bool has_damaging_move_of_type(*types) {
		foreach (var m in check_for_move) { //check_for_move do => |m|
			if (m.damagingMove() && types.Contains(m.CalcType(battler))) return true;
		}
		return false;
	}

	public bool has_move_with_function(*functions) {
		if (functions.Contains(m.function_code) }) check_for_move { |m| return true;
		return false;
	}

	//-----------------------------------------------------------------------------

	public bool can_attack() {
		if (self.effects.HyperBeam > 0) return false;
		if (status == statuses.SLEEP && statusCount > 1) return false;
		if (status == statuses.FROZEN) return false;   // Only 20% chance of unthawing; assune it won't
		if (self.effects.Truant && has_active_ability(abilitys.TRUANT)) return false;
		if (self.effects.Flinch) return false;
		// NOTE: Confusion/infatuation/paralysis have higher chances of allowing the
		//       attack, so the battler is treated as able to attack in those cases.
		return true;
	}

	public bool can_switch_lax() {
		if (wild()) return false;
		@ai.battle.eachInTeamFromBattlerIndex(@index) do |pkmn, i|
			if (@ai.battle.CanSwitchIn(@index, i)) return true;
		}
		return false;
	}

	// NOTE: This specifically means "is not currently trapped but can become
	//       trapped by an effect". Similar to def CanSwitchOut() but this returns
	//       false if any certain switching OR certain trapping applies.
	public bool can_become_trapped() {
		if (fainted()) return false;
		// Ability/item effects that allow switching no matter what
		if (ability_active() && Battle.AbilityEffects.triggerCertainSwitching(ability, battler, @ai.battle)) {
			return false;
		}
		if (item_active() && Battle.ItemEffects.triggerCertainSwitching(item, battler, @ai.battle)) {
			return false;
		}
		// Other certain switching effects
		if (Settings.MORE_TYPE_EFFECTS && has_type(types.GHOST)) return false;
		// Other certain trapping effects
		if (battler.trappedInBattle()) return false;
		// Trapping abilities/items
		@ai.each_foe_battler(side) do |b, i|
			if (b.ability_active() &&
				Battle.AbilityEffects.triggerTrappingByTarget(b.ability, battler, b.battler, @ai.battle)) {
				return false;
			}
			if (b.item_active() &&
				Battle.ItemEffects.triggerTrappingByTarget(b.item, battler, b.battler, @ai.battle)) {
				return false;
			}
		}
		return true;
	}

	//-----------------------------------------------------------------------------

	public bool wants_status_problem(new_status) {
		if (new_status == statuses.NONE) return true;
		if (ability_active()) {
			switch (ability_id) {
				case :GUTS:
					if (!new []{:SLEEP, :FROZEN}.Contains(new_status) &&
												@ai.stat_raise_worthwhile(self, :ATTACK, true)) return true;
					break;
				case :MARVELSCALE:
					if (@ai.stat_raise_worthwhile(self, :DEFENSE, true)) return true;
					break;
				case :QUICKFEET:
					if (!new []{:SLEEP, :FROZEN}.Contains(new_status) &&
												@ai.stat_raise_worthwhile(self, :SPEED, true)) return true;
					break;
				case :FLAREBOOST:
					if (new_status == statuses.BURN && @ai.stat_raise_worthwhile(self, :SPECIAL_ATTACK, true)) return true;
					break;
				case :TOXICBOOST:
					if (new_status == statuses.POISON && @ai.stat_raise_worthwhile(self, :ATTACK, true)) return true;
					break;
				case :POISONHEAL:
					if (new_status == statuses.POISON) return true;
					break;
				case :MAGICGUARD:   // Want a harmless status problem to prevent getting a harmful one
					if (new_status == statuses.POISON ||
												(new_status == statuses.BURN && !@ai.stat_raise_worthwhile(self, :ATTACK, true))) return true;
					break;
			}
		}
		if (new_status == statuses.SLEEP && check_for_move { |m| m.usableWhenAsleep() }) return true;
		if (has_move_with_function("DoublePowerIfUserPoisonedBurnedParalyzed")) {
			if (new []{:POISON, :BURN, :PARALYSIS}.Contains(new_status)) return true;
		}
		return false;
	}

	//-----------------------------------------------------------------------------

	// Returns a value indicating how beneficial the given ability will be to this
	// battler if it has it.
	// Return values are typically between -10 and +10. 0 is indifferent, positive
	// values mean this battler benefits, negative values mean this battler suffers.
	// NOTE: This method assumes the ability isn't being negated. The calculations
	//       that call this method separately check for it being negated, because
	//       they need to do something special in that case.
	public bool wants_ability(ability = abilitys.NONE) {
		if (!ability.is_a(Symbol) && ability.respond_to("id")) ability = ability.id;
		// Get the base ability rating
		ret = 0;
		Battle.AI.BASE_ABILITY_RATINGS.each_pair do |val, abilities|
			if (!abilities.Contains(ability)) continue;
			ret = val;
			break;
		}
		// Modify the rating based on ability-specific contexts
		if (@ai.trainer.medium_skill()) {
			ret = Battle.AI.Handlers.modify_ability_ranking(ability, ret, self, @ai);
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	// Returns a value indicating how beneficial the given item will be to this
	// battler if it is holding it.
	// Return values are typically between -10 and +10. 0 is indifferent, positive
	// values mean this battler benefits, negative values mean this battler suffers.
	// NOTE: This method assumes the item isn't being negated. The calculations
	//       that call this method separately check for it being negated, because
	//       they need to do something special in that case.
	public bool wants_item(item) {
		if (!item) item = items.NONE;
		if (!item.is_a(Symbol) && item.respond_to("id")) item = item.id;
		// Get the base item rating
		ret = 0;
		Battle.AI.BASE_ITEM_RATINGS.each_pair do |val, items|
			if (!items.Contains(item)) continue;
			ret = val;
			break;
		}
		// Modify the rating based on item-specific contexts
		if (@ai.trainer.medium_skill()) {
			ret = Battle.AI.Handlers.modify_item_ranking(item, ret, self, @ai);
		}
		// Prefer if this battler knows Fling and it will do a lot of damage/have an
		// additional (negative) effect when flung
		if (item != items.NONE && has_move_with_function("ThrowUserItemAtTarget")) {
			foreach (var flag in GameData.Item.get(item).flags) { //'GameData.Item.get(item).flags.each' do => |flag|
				if (!System.Text.RegularExpressions.Regex.IsMatch(flag,@"^Fling_(\d+)$",RegexOptions.IgnoreCase)) continue;
				amt = $~[1].ToInt();
				if (amt >= 80) ret += 1;
				if (amt >= 100) ret += 1;
				break;
			}
			if (new []{:FLAMEORB, :KINGSROCK, :LIGHTBALL, :POISONBARB, :RAZORFANG, :TOXICORB}.Contains(item)) {
				ret += 1;
			}
		}
		// Don't prefer if this battler knows Acrobatics
		if (has_move_with_function("DoublePowerIfUserHasNoItem")) {
			ret += (item == items.NONE) ? 1 : -1;
		}
		return ret;
	}

	//-----------------------------------------------------------------------------

	// Items can be consumed by Stuff Cheeks, Teatime, Bug Bite/Pluck and Fling.
	public void get_score_change_for_consuming_item(item, try_preserving_item = false) {
		ret = 0;
		switch (item) {
			case :ORANBERRY: case :BERRYJUICE: case :ENIGMABERRY: case :SITRUSBERRY:
				// Healing
				ret += (hp > totalhp * 0.75) ? -6 : 6;
				if (GameData.Item.get(item).is_berry() && has_active_ability(abilitys.RIPEN)) ret = ret * 3 / 2;
				break;
			case :AGUAVBERRY: case :FIGYBERRY: case :IAPAPABERRY: case :MAGOBERRY: case :WIKIBERRY:
				// Healing with confusion
				fraction_to_heal = 8;   // Gens 6 and lower
				if (Settings.MECHANICS_GENERATION == 7) {
					fraction_to_heal = 2;
				} else if (Settings.MECHANICS_GENERATION >= 8) {
					fraction_to_heal = 3;
				}
				ret += (hp > totalhp * (1 - (1.0 / fraction_to_heal))) ? -6 : 6;
				if (GameData.Item.get(item).is_berry() && has_active_ability(abilitys.RIPEN)) ret = ret * 3 / 2;
				if (@ai.trainer.high_skill()) {
					flavor_stat = {
						AGUAVBERRY  = :SPECIAL_DEFENSE,
						FIGYBERRY   = :ATTACK,
						IAPAPABERRY = :DEFENSE,
						MAGOBERRY   = :SPEED,
						WIKIBERRY   = :SPECIAL_ATTACK;
					}[item];
					if (@battler.nature.stat_changes.any(val => val[0] == flavor_stat && val[1] < 0)) {
						if (@battler.CanConfuseSelf(false)) ret -= 3;
					}
				}
				break;
			case :ASPEARBERRY: case :CHERIBERRY: case :CHESTOBERRY: case :PECHABERRY: case :RAWSTBERRY:
				// Status cure
				cured_status = {
					ASPEARBERRY = :FROZEN,
					CHERIBERRY  = :PARALYSIS,
					CHESTOBERRY = :SLEEP,
					PECHABERRY  = :POISON,
					RAWSTBERRY  = :BURN;
				}[item];
				ret += (cured_status && status == cured_status) ? 6 : -6;
				break;
			case :PERSIMBERRY:
				// Confusion cure
				ret += (self.effects.Confusion > 1) ? 6 : -6;
				break;
			case :LUMBERRY:
				// Any status/confusion cure
				ret += (status != statuses.NONE || self.effects.Confusion > 1) ? 6 : -6;
				break;
			case :MENTALHERB:
				// Cure mental effects
				if (self.effects.Attract >= 0 ||
					self.effects.Taunt > 1 ||
					self.effects.Encore > 1 ||
					self.effects.Torment ||
					self.effects.Disable > 1 ||
					self.effects.HealBlock > 1) {
					ret += 6;
				} else {
					ret -= 6;
				}
				break;
			case :APICOTBERRY: case :GANLONBERRY: case :LIECHIBERRY: case :PETAYABERRY: case :SALACBERRY:
					case :KEEBERRY: case :MARANGABERRY;
				// Stat raise
				stat = {
					APICOTBERRY  = :SPECIAL_DEFENSE,
					GANLONBERRY  = :DEFENSE,
					LIECHIBERRY  = :ATTACK,
					PETAYABERRY  = :SPECIAL_ATTACK,
					SALACBERRY   = :SPEED,
					KEEBERRY     = :DEFENSE,
					MARANGABERRY = :SPECIAL_DEFENSE;
				}[item];
				ret += (stat && @ai.stat_raise_worthwhile(self, stat)) ? 8 : -8;
				if (GameData.Item.get(item).is_berry() && has_active_ability(abilitys.RIPEN)) ret = ret * 3 / 2;
				break;
			case :STARFBERRY:
				// Random stat raise
				ret += 8;
				if (GameData.Item.get(item).is_berry() && has_active_ability(abilitys.RIPEN)) ret = ret * 3 / 2;
				break;
			case :WHITEHERB:
				// Resets lowered stats
				ret += (battler.hasLoweredStatStages()) ? 8 : -8;
				break;
			case :MICLEBERRY:
				// Raises accuracy of next move
				ret += (@ai.stat_raise_worthwhile(self, :ACCURACY, true)) ? 6 : -6;
				break;
			case :LANSATBERRY:
				// Focus energy
				ret += (self.effects.FocusEnergy < 2) ? 6 : -6;
				break;
			case :LEPPABERRY:
				// Restore PP
				ret += 6;
				if (GameData.Item.get(item).is_berry() && has_active_ability(abilitys.RIPEN)) ret = ret * 3 / 2;
				break;
		}
		if (ret < 0 && !try_preserving_item) ret = 0;
		return ret;
	}

	//-----------------------------------------------------------------------------

	private;

	public void effectiveness_of_type_against_single_battler_type(type, defend_type, user = null) {
		ret = Effectiveness.calculate(type, defend_type);
		if (Effectiveness.ineffective_type(type, defend_type)) {
			// Ring Target
			if (has_active_item(items.RINGTARGET)) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
			// Foresight
			if ((user&.has_active_ability(abilitys.SCRAPPY) || self.effects.Foresight) &&
				defend_type == types.GHOST) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
			// Miracle Eye
			if (self.effects.MiracleEye && defend_type == types.DARK) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
		} else if (Effectiveness.super_effective_type(type, defend_type)) {
			// Delta Stream's weather
			if (battler.effectiveWeather == Weathers.StrongWinds && defend_type == types.FLYING) {
				ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
			}
		}
		// Grounded Flying-type Pokémon become susceptible to Ground moves
		if (!battler.airborne() && defend_type == types.FLYING && type == types.GROUND) {
			ret = Effectiveness.NORMAL_EFFECTIVE_MULTIPLIER;
		}
		return ret;
	}
}
