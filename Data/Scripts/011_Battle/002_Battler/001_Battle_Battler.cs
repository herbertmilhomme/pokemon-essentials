//===============================================================================
//
//===============================================================================
public partial class Battle.Battler {
	// Fundamental to this object
	public int battle		{ get { return _battle; } }			protected int _battle;
	public int index		{ get { return _index; } set { _index = value; } }			protected int _index;
	// The Pokémon and its properties
	public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;
	public int pokemonIndex		{ get { return _pokemonIndex; } set { _pokemonIndex = value; } }			protected int _pokemonIndex;
	public int species		{ get { return _species; } set { _species = value; } }			protected int _species;
	public int types		{ get { return _types; } set { _types = value; } }			protected int _types;
	public int ability_id		{ get { return _ability_id; } set { _ability_id = value; } }			protected int _ability_id;
	public int item_id		{ get { return _item_id; } set { _item_id = value; } }			protected int _item_id;
	public int moves		{ get { return _moves; } set { _moves = value; } }			protected int _moves;
	public int attack		{ get { return _attack; } set { _attack = value; } }			protected int _attack;
	public int spatk		{ get { return _spatk; } set { _spatk = value; } }			protected int _spatk;
	public int speed		{ get { return _speed; } set { _speed = value; } }			protected int _speed;
	public int stages		{ get { return _stages; } set { _stages = value; } }			protected int _stages;
	public int totalhp		{ get { return _totalhp; } }			protected int _totalhp;
	/// <summary>Boolean to mark whether self has fainted properly</summary>
	public int fainted		{ get { return _fainted; } }			protected int _fainted;
	/// <summary>Boolean to mark whether self was captured</summary>
	public int captured		{ get { return _captured; } set { _captured = value; } }			protected int _captured;
	public int dummy		{ get { return _dummy; } }			protected int _dummy;
	public int effects		{ get { return _effects; } set { _effects = value; } }			protected int _effects;
	// Things the battler has done in battle
	public int turnCount		{ get { return _turnCount; } set { _turnCount = value; } }			protected int _turnCount;
	public int participants		{ get { return _participants; } set { _participants = value; } }			protected int _participants;
	public int lastAttacker		{ get { return _lastAttacker; } set { _lastAttacker = value; } }			protected int _lastAttacker;
	public int lastFoeAttacker		{ get { return _lastFoeAttacker; } set { _lastFoeAttacker = value; } }			protected int _lastFoeAttacker;
	public int lastHPLost		{ get { return _lastHPLost; } set { _lastHPLost = value; } }			protected int _lastHPLost;
	public int lastHPLostFromFoe		{ get { return _lastHPLostFromFoe; } set { _lastHPLostFromFoe = value; } }			protected int _lastHPLostFromFoe;
	public int lastMoveUsed		{ get { return _lastMoveUsed; } set { _lastMoveUsed = value; } }			protected int _lastMoveUsed;
	public int lastMoveUsedType		{ get { return _lastMoveUsedType; } set { _lastMoveUsedType = value; } }			protected int _lastMoveUsedType;
	public int lastRegularMoveUsed		{ get { return _lastRegularMoveUsed; } set { _lastRegularMoveUsed = value; } }			protected int _lastRegularMoveUsed;
	/// <summary>For Instruct</summary>
	public int lastRegularMoveTarget		{ get { return _lastRegularMoveTarget; } set { _lastRegularMoveTarget = value; } }			protected int _lastRegularMoveTarget;
	public int lastRoundMoved		{ get { return _lastRoundMoved; } set { _lastRoundMoved = value; } }			protected int _lastRoundMoved;
	/// <summary>For Stomping Tantrum</summary>
	public int lastMoveFailed		{ get { return _lastMoveFailed; } set { _lastMoveFailed = value; } }			protected int _lastMoveFailed;
	/// <summary>For Stomping Tantrum</summary>
	public int lastRoundMoveFailed		{ get { return _lastRoundMoveFailed; } set { _lastRoundMoveFailed = value; } }			protected int _lastRoundMoveFailed;
	public int movesUsed		{ get { return _movesUsed; } set { _movesUsed = value; } }			protected int _movesUsed;
	/// <summary>ID of multi-turn move currently being used</summary>
	public int currentMove		{ get { return _currentMove; } set { _currentMove = value; } }			protected int _currentMove;
	/// <summary>Used for Emergency Exit/Wimp Out</summary>
	public int droppedBelowHalfHP		{ get { return _droppedBelowHalfHP; } set { _droppedBelowHalfHP = value; } }			protected int _droppedBelowHalfHP;
	/// <summary>Used for Eject Pack</summary>
	public int statsDropped		{ get { return _statsDropped; } set { _statsDropped = value; } }			protected int _statsDropped;
	/// <summary>Boolean for Focus Punch</summary>
	public int tookMoveDamageThisRound		{ get { return _tookMoveDamageThisRound; } set { _tookMoveDamageThisRound = value; } }			protected int _tookMoveDamageThisRound;
	/// <summary>Boolean for whether self took damage this round</summary>
	public int tookDamageThisRound		{ get { return _tookDamageThisRound; } set { _tookDamageThisRound = value; } }			protected int _tookDamageThisRound;
	public int tookPhysicalHit		{ get { return _tookPhysicalHit; } set { _tookPhysicalHit = value; } }			protected int _tookPhysicalHit;
	/// <summary>Boolean for whether self's stat(s) raised this round</summary>
	public int statsRaisedThisRound		{ get { return _statsRaisedThisRound; } set { _statsRaisedThisRound = value; } }			protected int _statsRaisedThisRound;
	/// <summary>Boolean for whether self's stat(s) lowered this round</summary>
	public int statsLoweredThisRound		{ get { return _statsLoweredThisRound; } set { _statsLoweredThisRound = value; } }			protected int _statsLoweredThisRound;
	/// <summary>Whether Hail started in the round</summary>
	public int canRestoreIceFace		{ get { return _canRestoreIceFace; } set { _canRestoreIceFace = value; } }			protected int _canRestoreIceFace;
	public int damageState		{ get { return _damageState; } set { _damageState = value; } }			protected int _damageState;

	// These arrays should all have the same number of values in them
	STAT_STAGE_MULTIPLIERS    = new {2, 2, 2, 2, 2, 2, 2, 3, 4, 5, 6, 7, 8};
	STAT_STAGE_DIVISORS       = new {8, 7, 6, 5, 4, 3, 2, 2, 2, 2, 2, 2, 2};
	ACC_EVA_STAGE_MULTIPLIERS = new {3, 3, 3, 3, 3, 3, 3, 4, 5, 6, 7, 8, 9};
	ACC_EVA_STAGE_DIVISORS    = new {9, 8, 7, 6, 5, 4, 3, 3, 3, 3, 3, 3, 3};
	public const int STAT_STAGE_MAXIMUM        = 6;   // Is also the minimum (-6)

	//-----------------------------------------------------------------------------
	// Complex accessors.
	//-----------------------------------------------------------------------------

	public int level		{ get { return _level; } }			protected int _level;

	public int level { set {
		@level = value;
		if (@pokemon) @pokemon.level = value;
		}
	}

	public int form		{ get { return _form; } }			protected int _form;

	public int form { set {
		@form = value;
		if (@pokemon && !@effects.Transform) @pokemon.form = value;
		}
	}

	public void ability() {
		return GameData.Ability.try_get(@ability_id);
	}

	public int ability { set {
		new_ability = GameData.Ability.try_get(value);
		@ability_id = (new_ability) ? new_ability.id : null;
		}
	}

	public void item() {
		return GameData.Item.try_get(@item_id);
	}

	public int item { set {
		new_item = GameData.Item.try_get(value);
		@item_id = (new_item) ? new_item.id : null;
		if (@pokemon) @pokemon.item = @item_id;
		}
	}

	public void defense() {
		if (@battle.field.effects.WonderRoom > 0) return @spdef;
		return @defense;
	}

	public int defense		{ get { return _defense; } }			protected int _defense;

	public void spdef() {
		if (@battle.field.effects.WonderRoom > 0) return @defense;
		return @spdef;
	}

	public int spdef		{ get { return _spdef; } }			protected int _spdef;

	public int hp		{ get { return _hp; } }			protected int _hp;

	public int hp { set {
		@hp = value.ToInt();
		if (@pokemon) @pokemon.hp = value.ToInt();
		}
	}

	public bool fainted() { return @hp <= 0; }

	public int status		{ get { return _status; } }			protected int _status;

	public int status { set {
		if (@status == statuses.SLEEP && value != :SLEEP) @effects.Truant = false;
		if (value != :POISON || self.statusCount == 0) @effects.Toxic  = 0;
		@status = value;
		if (@pokemon) @pokemon.status = value;
		if (value != :POISON && value != :SLEEP) self.statusCount = 0;
		@battle.scene.RefreshOne(@index);
		}
	}

	public int statusCount		{ get { return _statusCount; } }			protected int _statusCount;

	public int statusCount { set {
		@statusCount = value;
		if (@pokemon) @pokemon.statusCount = value;
		@battle.scene.RefreshOne(@index);
		}
	}

	//-----------------------------------------------------------------------------
	// Properties from Pokémon.
	//-----------------------------------------------------------------------------

	public int happiness       { get { return @pokemon ? @pokemon.happiness : 0;       } }
	public int affection_level { get { return @pokemon ? @pokemon.affection_level : 2; } }
	public int gender          { get { return @pokemon ? @pokemon.gender : 0;          } }
	public int nature          { get { return @pokemon ? @pokemon.nature : null;        } }
	public int pokerusStage    { get { return @pokemon ? @pokemon.pokerusStage : 0;    } }

	//-----------------------------------------------------------------------------
	// Mega Evolution, Primal Reversion, Shadow Pokémon.
	//-----------------------------------------------------------------------------

	public bool hasMega() {
		if (@effects.Transform) return false;
		return @pokemon&.hasMegaForm();
	}

	public bool mega() { return @pokemon&.mega(); }

	public bool hasPrimal() {
		if (@effects.Transform) return false;
		return @pokemon&.hasPrimalForm();
	}

	public bool primal() { return @pokemon&.primal(); }

	public bool shadowPokemon() { return false; }

	public bool inHyperMode() { return false; }

	//-----------------------------------------------------------------------------
	// Display-only properties.
	//-----------------------------------------------------------------------------

	public void name() {
		if (@effects.Illusion) return @effects.Illusion.name;
		return @name;
	}

	public int name		{ get { return _name; } }			protected int _name;

	public void displayPokemon() {
		if (@effects.Illusion) return @effects.Illusion;
		return self.pokemon;
	}

	public void displaySpecies() {
		if (@effects.Illusion) return @effects.Illusion.species;
		return self.species;
	}

	public void displayGender() {
		if (@effects.Illusion) return @effects.Illusion.gender;
		return self.gender;
	}

	public void displayForm() {
		if (@effects.Illusion) return @effects.Illusion.form;
		return self.form;
	}

	public bool shiny() {
		if (@effects.Illusion) return @effects.Illusion.shiny();
		return @pokemon&.shiny();
	}

	public bool super_shiny() {
		return @pokemon&.super_shiny();
	}

	public bool owned() {
		if (!@battle.wildBattle()) return false;
		return Game.GameData.player.owned(displaySpecies);
	}
	alias owned owned();

	public void abilityName() {
		abil = self.ability;
		return (abil) ? abil.name : "";
	}

	public void itemName() {
		itm = self.item;
		return (itm) ? itm.name : "";
	}

	public void This(lowerCase = false) {
		if (opposes()) {
			if (@battle.trainerBattle()) {
				return lowerCase ? _INTL("the opposing {1}", name) : _INTL("The opposing {1}", name);
			} else {
				return lowerCase ? _INTL("the wild {1}", name) : _INTL("The wild {1}", name);
			}
		} else if (!OwnedByPlayer()) {
			return lowerCase ? _INTL("the ally {1}", name) : _INTL("The ally {1}", name);
		}
		return name;
	}

	public void Team(lowerCase = false) {
		if (opposes()) {
			return lowerCase ? _INTL("the opposing team") : _INTL("The opposing team");
		}
		return lowerCase ? _INTL("your team") : _INTL("Your team");
	}

	public void OpposingTeam(lowerCase = false) {
		if (opposes()) {
			return lowerCase ? _INTL("your team") : _INTL("Your team");
		}
		return lowerCase ? _INTL("the opposing team") : _INTL("The opposing team");
	}

	//-----------------------------------------------------------------------------
	// Calculated properties.
	//-----------------------------------------------------------------------------

	public void Speed() {
		if (fainted()) return 1;
		stage = @stages[:SPEED] + STAT_STAGE_MAXIMUM;
		speed = @speed * STAT_STAGE_MULTIPLIERS[stage] / STAT_STAGE_DIVISORS[stage];
		speedMult = 1.0;
		// Ability effects that alter calculated Speed
		if (abilityActive()) {
			speedMult = Battle.AbilityEffects.triggerSpeedCalc(self.ability, self, speedMult);
		}
		// Item effects that alter calculated Speed
		if (itemActive()) {
			speedMult = Battle.ItemEffects.triggerSpeedCalc(self.item, self, speedMult);
		}
		// Other effects
		if (OwnSide.effects.Tailwind > 0) speedMult *= 2;
		if (OwnSide.effects.Swamp > 0) speedMult /= 2;
		// Paralysis
		if (status == statuses.PARALYSIS && !hasActiveAbility(Abilitys.QUICKFEET)) {
			speedMult /= (Settings.MECHANICS_GENERATION >= 7) ? 2 : 4;
		}
		// Badge multiplier
		if (@battle.internalBattle && OwnedByPlayer() &&
			@battle.Player.badge_count >= Settings.NUM_BADGES_BOOST_SPEED) {
			speedMult *= 1.1;
		}
		// Calculation
		return (int)Math.Max((int)Math.Round(speed * speedMult), 1);
	}

	public void Weight() {
		ret = (@pokemon) ? @pokemon.weight : 500;
		ret += @effects.WeightChange;
		if (ret < 1) ret = 1;
		if (abilityActive() && !beingMoldBroken()) {
			ret = Battle.AbilityEffects.triggerWeightCalc(self.ability, self, ret);
		}
		if (itemActive()) {
			ret = Battle.ItemEffects.triggerWeightCalc(self.item, self, ret);
		}
		return (int)Math.Max(ret, 1);
	}

	//-----------------------------------------------------------------------------
	// Queries about what the battler has.
	//-----------------------------------------------------------------------------

	public void plainStats() {
		ret = new List<string>();
		ret[:ATTACK]          = self.attack;
		ret[:DEFENSE]         = self.defense;
		ret[:SPECIAL_ATTACK]  = self.spatk;
		ret[:SPECIAL_DEFENSE] = self.spdef;
		ret[:SPEED]           = self.speed;
		return ret;
	}

	public bool isSpecies(species) {
		return @pokemon&.isSpecies(species);
	}

	// Returns the active types of this Pokémon. The array should not include the
	// same type more than once, and should not include any invalid types.
	public void Types(withExtraType = false) {
		ret = @types.uniq;
		// Burn Up erases the Fire-type
		if (@effects.BurnUp) ret.delete(:FIRE);
		// Double Shock erases the Electric-type
		if (@effects.DoubleShock) ret.delete(:ELECTRIC);
		// Roost erases the Flying-type (if there are no types left, adds the Normal-
		// type)
		if (@effects.Roost) {
			ret.delete(:FLYING);
			if (ret.length == 0) ret.Add(:NORMAL);
		}
		// Add the third type specially
		if (withExtraType && @effects.ExtraType && !ret.Contains(@effects.ExtraType)) {
			ret.Add(@effects.ExtraType);
		}
		return ret;
	}

	public bool HasType(type) {
		if (!type) return false;
		activeTypes = Types(true);
		return activeTypes.Contains(GameData.Type.get(type).id);
	}

	public bool HasOtherType(type) {
		if (!type) return false;
		activeTypes = Types(true);
		activeTypes.delete(GameData.Type.get(type).id);
		return activeTypes.length > 0;
	}

	// NOTE: Do not create any held item which affects whether a Pokémon's ability
	//       is active. The ability Klutz affects whether a Pokémon's item is
	//       active, and the code for the two combined would cause an infinite loop
	//       (regardless of whether any Pokémon actually has either the ability or
	//       the item - the code existing is enough to cause the loop).
	public bool abilityActive(ignore_fainted = false, check_ability = null) {
		if (fainted() && !ignore_fainted) return false;
		if (@effects.GastroAcid) return false;
		if (check_ability != abilitys.NEUTRALIZINGGAS && self.ability != abilitys.NEUTRALIZINGGAS &&
										@battle.CheckGlobalAbility(Abilities.NEUTRALIZINGGAS)) return false;
		return true;
	}

	public bool hasActiveAbility(check_ability, ignore_fainted = false) {
		if (!abilityActive(ignore_fainted, check_ability)) return false;
		if (check_ability.Length > 0) return check_ability.Contains(@ability_id);
		return self.ability == check_ability;
	}
	alias hasWorkingAbility hasActiveAbility();

	// Applies to both losing self's ability (i.e. being replaced by another) and
	// having self's ability be negated.
	public bool unstoppableAbility(abil = null) {
		if (!abil) abil = @ability_id;
		abil = GameData.Ability.try_get(abil);
		if (!abil) return false;
		ability_blacklist = new {
			// Form-changing abilities
			:BATTLEBOND,
			:DISGUISE,
//      :FLOWERGIFT,                                        // This can be stopped
//      :FORECAST,                                          // This can be stopped
			:GULPMISSILE,
			:ICEFACE,
			:MULTITYPE,
			:POWERCONSTRUCT,
			:SCHOOLING,
			:SHIELDSDOWN,
			:STANCECHANGE,
			:ZENMODE,
			// Abilities intended to be inherent properties of a certain species
			:ASONECHILLINGNEIGH,
			:ASONEGRIMNEIGH,
			:COMATOSE,
			:RKSSYSTEM;
		}
		if (ability_blacklist.Contains(abil.id)) return true;
		if (hasActiveItem(Items.ABILITYSHIELD)) return true;
		return false;
	}

	// Applies to gaining the ability.
	public bool ungainableAbility(abil = null) {
		if (!abil) abil = @ability_id;
		abil = GameData.Ability.try_get(abil);
		if (!abil) return false;
		ability_blacklist = new {
			// Form-changing abilities
			:BATTLEBOND,
			:DISGUISE,
			:FLOWERGIFT,
			:FORECAST,
			:GULPMISSILE,
			:ICEFACE,
			:MULTITYPE,
			:POWERCONSTRUCT,
			:SCHOOLING,
			:SHIELDSDOWN,
			:STANCECHANGE,
			:ZENMODE,
			// Appearance-changing abilities
			:ILLUSION,
			:IMPOSTER,
			// Abilities intended to be inherent properties of a certain species
			:ASONECHILLINGNEIGH,
			:ASONEGRIMNEIGH,
			:COMATOSE,
			:RKSSYSTEM,
			// Abilities that can't be negated
			:NEUTRALIZINGGAS;
		}
		return ability_blacklist.Contains(abil.id);
	}

	public bool itemActive(ignoreFainted = false) {
		if (fainted() && !ignoreFainted) return false;
		if (@effects.Embargo > 0) return false;
		if (@battle.field.effects.MagicRoom > 0) return false;
		if (@battle.corrosiveGas[@index % 2][@pokemonIndex]) return false;
		if (hasActiveAbility(:KLUTZ, ignoreFainted)) return false;
		return true;
	}

	public bool hasActiveItem(check_item, ignore_fainted = false) {
		if (!itemActive(ignore_fainted)) return false;
		if (check_item.Length > 0) return check_item.Contains(@item_id);
		return self.item == check_item;
	}
	alias hasWorkingItem hasActiveItem();

	// Returns whether the specified item will be unlosable for this Pokémon.
	public bool unlosableItem(check_item) {
		if (!check_item) return false;
		item_data = GameData.Item.get(check_item);
		if (item_data.is_mail()) return true;
		if (@effects.Transform) return false;
		// Items that change a Pokémon's form
		if (mega()) {   // Check if item was needed for this Mega Evolution
			if (@pokemon.species_data.mega_stone == item_data.id) return true;
		} else {   // Check if item could cause a Mega Evolution
			foreach (var data in GameData.Species) { //'GameData.Species.each' do => |data|
				if (data.species != @species || data.unmega_form != @form) continue;
				if (data.mega_stone == item_data.id) return true;
			}
		}
		// Other unlosable items
		return item_data.unlosable(@species, self.ability);
	}

	public void eachMove() {
		@moves.each(m => yield m);
	}

	public void eachMoveWithIndex() {
		@moves.each_with_index((m, i) => yield m, i);
	}

	public bool HasMove(move_id) {
		if (!move_id) return false;
		if (m.id == move_id }) eachMove { |m| return true;
		return false;
	}

	public bool HasMoveType(check_type) {
		if (!check_type) return false;
		check_type = GameData.Type.get(check_type).id;
		if (m.type == check_type }) eachMove { |m| return true;
		return false;
	}

	public bool HasMoveFunction(*arg) {
		if (!arg) return false;
		foreach (var m in eachMove) { //eachMove do => |m|
			arg.each(code => { if (m.function_code == code) return true; });
		}
		return false;
	}

	public void GetMoveWithID(move_id) {
		if (!move_id) return null;
		if (m.id == move_id }) eachMove { |m| return m;
		return null;
	}

	public bool hasMoldBreaker() {
		return hasActiveAbility(new {:MOLDBREAKER, :TERAVOLT, :TURBOBLAZE});
	}

	public bool beingMoldBroken() {
		if (hasActiveItem(Items.ABILITYSHIELD)) return false;
		return @battle.moldBreaker;
	}

	public bool canChangeType() {
		return !new []{:MULTITYPE, :RKSSYSTEM}.Contains(@ability_id);
	}

	public bool airborne() {
		if (hasActiveItem(Items.IRONBALL)) return false;
		if (@effects.Ingrain) return false;
		if (@effects.SmackDown) return false;
		if (@battle.field.effects.Gravity > 0) return false;
		if (Type == Types.FLYING) return true;
		if (hasActiveAbility(Abilitys.LEVITATE) && !beingMoldBroken()) return true;
		if (hasActiveItem(Items.AIRBALLOON)) return true;
		if (@effects.MagnetRise > 0) return true;
		if (@effects.Telekinesis > 0) return true;
		return false;
	}

	public bool affectedByTerrain() {
		if (airborne()) return false;
		if (semiInvulnerable()) return false;
		return true;
	}

	public bool takesIndirectDamage(showMsg = false) {
		if (fainted()) return false;
		if (hasActiveAbility(Abilitys.MAGICGUARD)) {
			if (showMsg) {
				@battle.ShowAbilitySplash(self);
				if (Battle.Scene.USE_ABILITY_SPLASH) {
					@battle.Display(_INTL("{1} is unaffected!", This));
				} else {
					@battle.Display(_INTL("{1} is unaffected because of its {2}!", This, abilityName));
				}
				@battle.HideAbilitySplash(self);
			}
			return false;
		}
		return true;
	}

	public bool takesSandstormDamage() {
		if (!takesIndirectDamage()) return false;
		if (Type == Types.GROUND || Type == Types.ROCK || Type == Types.STEEL) return false;
		if ((inTwoTurnAttack("TwoTurnAttackInvulnerableUnderground",
																		"TwoTurnAttackInvulnerableUnderwater")) return false;
		if (hasActiveAbility(new {:OVERCOAT, :SANDFORCE, :SANDRUSH, :SANDVEIL})) return false;
		if (hasActiveItem(Items.SAFETYGOGGLES)) return false;
		return true;
	}

	public bool takesHailDamage() {
		if (!takesIndirectDamage()) return false;
		if (Type == Types.ICE) return false;
		if ((inTwoTurnAttack("TwoTurnAttackInvulnerableUnderground",
																		"TwoTurnAttackInvulnerableUnderwater")) return false;
		if (hasActiveAbility(new {:OVERCOAT, :ICEBODY, :SNOWCLOAK})) return false;
		if (hasActiveItem(Items.SAFETYGOGGLES)) return false;
		return true;
	}

	public bool takesShadowSkyDamage() {
		if (!takesIndirectDamage()) return false;
		if (shadowPokemon()) return false;
		return true;
	}

	public void effectiveWeather() {
		ret = @battle.Weather;
		if (new []{:Sun, :Rain, :HarshSun, :HeavyRain}.Contains(ret) && hasActiveItem(Items.UTILITYUMBRELLA)) ret = :None;
		return ret;
	}

	public bool affectedByPowder(showMsg = false) {
		if (fainted()) return false;
		if (Type == Types.GRASS && Settings.MORE_TYPE_EFFECTS) {
			if (showMsg) @battle.Display(_INTL("{1} is unaffected!", This));
			return false;
		}
		if (Settings.MECHANICS_GENERATION >= 6) {
			if (hasActiveAbility(Abilitys.OVERCOAT) && !beingMoldBroken()) {
				if (showMsg) {
					@battle.ShowAbilitySplash(self);
					if (Battle.Scene.USE_ABILITY_SPLASH) {
						@battle.Display(_INTL("{1} is unaffected!", This));
					} else {
						@battle.Display(_INTL("{1} is unaffected because of its {2}!", This, abilityName));
					}
					@battle.HideAbilitySplash(self);
				}
				return false;
			}
			if (hasActiveItem(Items.SAFETYGOGGLES)) {
				if (showMsg) {
					@battle.Display(_INTL("{1} is unaffected because of its {2}!", This, itemName));
				}
				return false;
			}
		}
		return true;
	}

	public bool canHeal() {
		if (fainted() || @hp >= @totalhp) return false;
		if (@effects.HealBlock > 0) return false;
		return true;
	}

	public bool affectedByContactEffect(showMsg = false) {
		if (fainted()) return false;
		if (hasActiveItem(Items.PROTECTIVEPADS)) {
			if (showMsg) @battle.Display(_INTL("{1} protected itself with the {2}!", This, itemName));
			return false;
		}
		return true;
	}

	public bool trappedInBattle() {
		if (@effects.Trapping > 0) return true;
		if (@effects.MeanLook >= 0) return true;
		if (@effects.JawLock >= 0) return true;
		if (@battle.allBattlers.any(b => b.effects.JawLock == @index)) return true;
		if (@effects.Octolock >= 0) return true;
		if (@effects.Ingrain) return true;
		if (@effects.NoRetreat) return true;
		if (@battle.field.effects.FairyLock > 0) return true;
		return false;
	}

	public bool movedThisRound() {
		return @lastRoundMoved && @lastRoundMoved == @battle.turnCount;
	}

	public bool usingMultiTurnAttack() {
		if (@effects.TwoTurnAttack) return true;
		if (@effects.HyperBeam > 0) return true;
		if (@effects.Rollout > 0) return true;
		if (@effects.Outrage > 0) return true;
		if (@effects.Uproar > 0) return true;
		if (@effects.Bide > 0) return true;
		return false;
	}

	public bool inTwoTurnAttack(*arg) {
		if (!@effects.TwoTurnAttack) return false;
		ttaFunction = GameData.Move.get(@effects.TwoTurnAttack).function_code;
		arg.each(a => { if (a == ttaFunction) return true; });
		return false;
	}

	public bool semiInvulnerable() {
		return inTwoTurnAttack("TwoTurnAttackInvulnerableInSky",
														"TwoTurnAttackInvulnerableUnderground",
														"TwoTurnAttackInvulnerableUnderwater",
														"TwoTurnAttackInvulnerableInSkyParalyzeTarget",
														"TwoTurnAttackInvulnerableRemoveProtections",
														"TwoTurnAttackInvulnerableInSkyTargetCannotAct");
	}

	public void EncoredMoveIndex() {
		if (@effects.Encore == 0 || !@effects.EncoreMove) return -1;
		ret = -1;
		eachMoveWithIndex do |m, i|
			if (m.id != @effects.EncoreMove) continue;
			ret = i;
			break;
		}
		return ret;
	}

	public void initialItem() {
		return @battle.initialItems[@index & 1][@pokemonIndex];
	}

	public void setInitialItem(value) {
		item_data = GameData.Item.try_get(value);
		new_item = (item_data) ? item_data.id : null;
		@battle.initialItems[@index & 1][@pokemonIndex] = new_item;
	}

	public void recycleItem() {
		return @battle.recycleItems[@index & 1][@pokemonIndex];
	}

	public void setRecycleItem(value) {
		item_data = GameData.Item.try_get(value);
		new_item = (item_data) ? item_data.id : null;
		@battle.recycleItems[@index & 1][@pokemonIndex] = new_item;
	}

	public bool belched() {
		return @battle.belch[@index & 1][@pokemonIndex];
	}

	public void setBelched() {
		@battle.belch[@index & 1][@pokemonIndex] = true;
	}

	//-----------------------------------------------------------------------------
	// Methods relating to this battler's position on the battlefield.
	//-----------------------------------------------------------------------------

	// Returns whether the given position belongs to the opposing Pokémon's side.
	public bool opposes(i = 0) {
		if (i.respond_to("index")) i = i.index;
		return (@index & 1) != (i & 1);
	}

	// Returns whether the given position/battler is near to self.
	public bool near(i) {
		if (i.respond_to("index")) i = i.index;
		return @battle.nearBattlers(@index, i);
	}

	// Returns whether self is owned by the player.
	public bool OwnedByPlayer() {
		return @battle.OwnedByPlayer(@index);
	}

	public bool wild() {
		return @battle.wildBattle() && opposes();
	}

	// Returns 0 if self is on the player's side, or 1 if self is on the opposing
	// side.
	public void idxOwnSide() {
		return @index & 1;
	}

	// Returns 1 if self is on the player's side, or 0 if self is on the opposing
	// side.
	public void idxOpposingSide() {
		return (@index & 1) ^ 1;
	}

	// Returns the data structure for this battler's side.
	public void OwnSide() {
		return @battle.sides[idxOwnSide];
	}

	// Returns the data structure for the opposing Pokémon's side.
	public void OpposingSide() {
		return @battle.sides[idxOpposingSide];
	}

	// Yields each unfainted ally Pokémon.
	// Unused
	public void eachAlly() {
		@battle.battlers.each do |b|
			if (b && !b.fainted() && !b.opposes(@index) && b.index != @index) yield b;
		}
	}

	// Returns an array containing all unfainted ally Pokémon.
	public void allAllies() {
		return @battle.allSameSideBattlers(@index).reject(b => b.index == @index);
	}

	// Yields each unfainted opposing Pokémon.
	// Unused
	public void eachOpposing() {
		@battle.battlers.each(b => { if (b && !b.fainted() && b.opposes(@index)) yield b; });
	}

	// Returns an array containing all unfainted opposing Pokémon.
	public void allOpposing() {
		return @battle.allOtherSideBattlers(@index);
	}

	// Returns the battler that is most directly opposite to self. unfaintedOnly is
	// whether it should prefer to return a non-fainted battler.
	public void DirectOpposing(unfaintedOnly = false) {
		@battle.GetOpposingIndicesInOrder(@index).each do |i|
			if (!@battle.battlers[i]) continue;
			if (unfaintedOnly && @battle.battlers[i].fainted()) break;
			return @battle.battlers[i];
		}
		// Wanted an unfainted battler but couldn't find one; make do with a fainted
		// battler
		@battle.GetOpposingIndicesInOrder(@index).each do |i|
			if (@battle.battlers[i]) return @battle.battlers[i];
		}
		return @battle.battlers[(@index ^ 1)];
	}
}
