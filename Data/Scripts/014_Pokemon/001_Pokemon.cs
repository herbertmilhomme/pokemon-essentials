//===============================================================================
// Instances of this class are individual Pokémon.
// The player's party Pokémon are stored in the array Game.GameData.player.party.
//===============================================================================
public partial class Pokemon {
	/// <summary>this Pokémon's species</summary>
	public Symbol species		{ get { return _species; } }			private Symbol _species;
	// If defined, this Pokémon's form will be this value even if a MultipleForms
	// handler tries to say otherwise.
	/// <summary>this Pokémon's form</summary>
	public Integer? forced_form		{ get { return _forced_form; } set { _forced_form = value; } }			protected Integer? _forced_form;
	// If defined, is the time (in Integer form) when this Pokémon's form was set.
	/// <summary>the time this Pokémon's form was set</summary>
	public Integer? time_form_set		{ get { return _time_form_set; } set { _time_form_set = value; } }			protected Integer? _time_form_set;
	/// <summary>the current experience points</summary>
	public Integer exp		{ get { return _exp; } }			private Integer _exp;
	/// <summary>the number of steps until this Pokémon hatches, 0 if this Pokémon is not an egg</summary>
	public Integer steps_to_hatch		{ get { return _steps_to_hatch; } set { _steps_to_hatch = value; } }			protected Integer _steps_to_hatch;
	/// <summary>the current HP</summary>
	public Integer hp		{ get { return _hp; } }			private Integer _hp;
	/// <summary>this Pokémon's current status (see GameData.Status)</summary>
	public Symbol status		{ get { return _status; } }			private Symbol _status;
	/// <summary>sleep count / toxic flag / 0:
	///   sleep (number of rounds before waking up), toxic (0 = regular poison, 1 = toxic)</summary>
	public Integer statusCount		{ get { return _statusCount; } set { _statusCount = value; } }			protected Integer _statusCount;
	// This Pokémon's shininess (true, false, null). Is recalculated if made null.
	/// <param name="value">whether this Pokémon is shiny | Boolean, null</param>
	public int shiny		{ get { return _shiny; } }			protected int _shiny;
	/// <summary>the moves known by this Pokémon</summary>
	public Array<Pokemon.Move> moves		{ get { return _moves; } set { _moves = value; } }			protected Array<Pokemon.Move> _moves;
	/// <summary>the IDs of moves known by this Pokémon when it was obtained</summary>
	public Array<Symbol> first_moves		{ get { return _first_moves; } set { _first_moves = value; } }			protected Array<Symbol> _first_moves;
	/// <summary>an array of ribbons owned by this Pokémon</summary>
	public Array<Symbol> ribbons		{ get { return _ribbons; } set { _ribbons = value; } }			protected Array<Symbol> _ribbons;
	/// <summary>contest stats</summary>
	public Integer cool		{ get { return _cool; } set { _cool = value; } }			protected Integer _cool;
	public Integer beauty		{ get { return _beauty } set { _beauty = value; } }			protected Integer _beauty;
	public Integer cute		{ get { return _cute } set { _cute = value; } }			public Integer _cute;
	public Integer smart		{ get { return _smart } set { _smart = value; } }			public Integer _smart;
	public Integer tough		{ get { return _tough } set { _tough = value; } }			public Integer _tough;
	public Integer sheen		{ get { return _sheen } set { _sheen = value; } }			public Integer _sheen;
	/// <summary>the Pokérus strain and infection time</summary>
	public Integer pokerus		{ get { return _pokerus; } set { _pokerus = value; } }			protected Integer _pokerus;
	/// <summary>this Pokémon's current happiness (an integer between 0 and 255)</summary>
	public Integer happiness		{ get { return _happiness; } set { _happiness = value; } }			protected Integer _happiness;
	/// <summary>the item ID of the Poké Ball this Pokémon is in</summary>
	public Symbol poke_ball		{ get { return _poke_ball; } set { _poke_ball = value; } }			protected Symbol _poke_ball;
	/// <summary>this Pokémon's markings, one value per mark</summary>
	public Array<Integer> markings		{ get { return _markings; } set { _markings = value; } }			protected Array<Integer> _markings;
	/// <summary>a hash of IV values for HP, Atk, Def, Speed, Sp. Atk and Sp. Def</summary>
	public Hash<Integer> iv		{ get { return _iv; } set { _iv = value; } }			protected Hash<Integer> _iv;
	// An array of booleans indicating whether a stat is made to have maximum IVs
	// (for Hyper Training). Set like @ivMaxed[:ATTACK] = true
	/// <summary>a hash of booleans that max each IV value</summary>
	public Hash<Boolean> ivMaxed		{ get { return _ivMaxed; } set { _ivMaxed = value; } }			protected Hash<Boolean> _ivMaxed;
	/// <summary>this Pokémon's effort values</summary>
	public Hash<Integer> ev		{ get { return _ev; } set { _ev = value; } }			protected Hash<Integer> _ev;
	/// <summary>calculated stats</summary>
	public Integer totalhp		{ get { return _totalhp; } }			private Integer _totalhp;
	public Integer attack		{ get { return _attack } set { _attack = value; } }			public Integer _attack;
	public Integer defense		{ get { return _defense } set { _defense = value; } }			public Integer _defense;
	public Integer spatk		{ get { return _spatk } set { _spatk = value; } }			public Integer _spatk;
	public Integer spdef		{ get { return _spdef } set { _spdef = value; } }			public Integer _spdef;
	public Integer speed		{ get { return _speed } set { _speed = value; } }			public Integer _speed;
	/// <summary>this Pokémon's owner</summary>
	public Owner owner		{ get { return _owner; } }			private Owner _owner;
	/// <summary>the manner this Pokémon was obtained:
	///   0 (met), 1 (as egg), 2 (traded), 4 (fateful encounter)</summary>
	public Integer obtain_method		{ get { return _obtain_method; } set { _obtain_method = value; } }			protected Integer _obtain_method;
	/// <summary>the ID of the map this Pokémon was obtained in</summary>
	public Integer obtain_map		{ get { return _obtain_map; } set { _obtain_map = value; } }			protected Integer _obtain_map;
	// Describes the manner this Pokémon was obtained. If left undefined,
	// the obtain map's name is used.
	/// <summary>the obtain text</summary>
	public String obtain_text		{ get { return _obtain_text; } set { _obtain_text = value; } }			protected String _obtain_text;
	/// <summary>the level of this Pokémon when it was obtained</summary>
	public Integer obtain_level		{ get { return _obtain_level; } set { _obtain_level = value; } }			protected Integer _obtain_level;
	// If this Pokémon hatched from an egg, returns the map ID where the hatching happened.
	// Otherwise returns 0.
	/// <summary>the map ID where egg was hatched (0 by default)</summary>
	public Integer hatched_map		{ get { return _hatched_map; } set { _hatched_map = value; } }			protected Integer _hatched_map;
	// Another Pokémon which has been fused with this Pokémon (or null if there is none).
	// Currently only used by Kyurem, to record a fused Reshiram or Zekrom.
	/// <summary>the Pokémon fused into this one (null if there is none)</summary>
	public Pokemon? fused		{ get { return _fused; } set { _fused = value; } }			protected Pokemon? _fused;
	/// <summary>this Pokémon's personal ID</summary>
	public Integer personalID		{ get { return _personalID; } set { _personalID = value; } }			protected Integer _personalID;
	// A number used by certain species to evolve.
	public int evolution_counter		{ get { return _evolution_counter; } }			protected int _evolution_counter;
	// Used by Galarian Yamask to remember that it took sufficient damage from a
	// battle and can evolve.
	public int ready_to_evolve		{ get { return _ready_to_evolve; } set { _ready_to_evolve = value; } }			protected int _ready_to_evolve;
	// Whether this Pokémon can be deposited in storage/Day Care
	public int cannot_store		{ get { return _cannot_store; } set { _cannot_store = value; } }			protected int _cannot_store;
	// Whether this Pokémon can be released
	public int cannot_release		{ get { return _cannot_release; } set { _cannot_release = value; } }			protected int _cannot_release;
	// Whether this Pokémon can be traded
	public int cannot_trade		{ get { return _cannot_trade; } set { _cannot_trade = value; } }			protected int _cannot_trade;

	// Max total IVs
	public const int IV_STAT_LIMIT = 31;
	// Max total EVs
	public const int EV_LIMIT      = 510;
	// Max EVs that a single stat can have
	public const int EV_STAT_LIMIT = 252;
	// Maximum length a Pokémon's nickname can be
	public const int MAX_NAME_SIZE = 10;
	// Maximum number of moves a Pokémon can know at once
	public const int MAX_MOVES     = 4;

	public static void play_cry(species, form = 0, volume = 90, pitch = 100) {
		GameData.Species.play_cry_from_species(species, form, volume, pitch);
	}

	public void play_cry(volume = 90, pitch = null) {
		GameData.Species.play_cry_from_pokemon(self, volume, pitch);
	}

	public override void inspect() {
		str = base.inspect().chop;
		str << string.Format(" {0} Lv.{0}>", @species, @level.ToString() || "???");
		return str;
	}

	public void species_data() {
		return GameData.Species.get_species_form(@species, form_simple);
	}

	public void evolution_counter() {
		@evolution_counter ||= 0;
		return @evolution_counter;
	}

	//-----------------------------------------------------------------------------
	// Species and form.
	//-----------------------------------------------------------------------------

	// Changes the Pokémon's species and re-calculates its statistics.
	// @param species_id [Symbol, String, GameData.Species] ID of the species to change this Pokémon to
	public int species { set {
		new_species_data = GameData.Species.get(species_id);
		if (@species == new_species_data.species) return;
		@species     = new_species_data.species;
		default_form = new_species_data.default_form;
		if (default_form >= 0) {
			@form      = default_form;
		} else if (new_species_data.form > 0) {
			@form      = new_species_data.form;
		}
		@forced_form = null;
		if (singleGendered()) @gender      = null;
		@level       = null;   // In case growth rate is different for the new species
		@ability     = null;
		calc_stats;
		}
	}

	/// <param name="check_species">ID of the species to check for | Symbol, String, GameData.Species</param>
	// @return [Boolean] whether this Pokémon is of the specified species
	public bool isSpecies(Symbol check_species) {
		return @species == check_species || (GameData.Species.exists(check_species) &&
																				@species == GameData.Species.get(check_species).species);
	}

	public void form() {
		if (!@forced_form.null()) return @forced_form;
		if (Game.GameData.game_temp.in_battle || Game.GameData.game_temp.in_storage) return @form;
		calc_form = MultipleForms.call("getForm", self);
		if (calc_form && calc_form != @form) self.form = calc_form;
		return @form;
	}

	public void form_simple() {
		return @forced_form || @form;
	}

	public int form { set {
		oldForm = @form;
		@form = value;
		@ability = null;
		MultipleForms.call("onSetForm", self, value, oldForm);
		calc_stats;
		Game.GameData.player&.pokedex&.register(self);
		}
	}

	// The same as def form=, but yields to a given block in the middle so that a
	// message about the form changing can be shown before calling "onSetForm"
	// which may have its own messages, e.g. learning a move.
	public void setForm(value) {
		oldForm = @form;
		@form = value;
		@ability = null;
		if (block_given()) yield;
		MultipleForms.call("onSetForm", self, value, oldForm);
		calc_stats;
		Game.GameData.player&.pokedex&.register(self);
	}

	public int form_simple { set {
		@form = value;
		calc_stats;
		}
	}

	//-----------------------------------------------------------------------------
	// Level.
	//-----------------------------------------------------------------------------

	// @return [Integer] this Pokémon's level
	public void level() {
		if (!@level) @level = growth_rate.level_from_exp(@exp);
		return @level;
	}

	// Sets this Pokémon's level. The given level must be between 1 and the
	// maximum level (defined in {GameData.GrowthRate}).
	// @param value [Integer] new level (between 1 and the maximum level)
	public int level { set {
		if (value < 1 || value > GameData.GrowthRate.max_level) {
			Debug.LogError(new ArgumentError(_INTL("The level number ({1}) is invalid.", value)));
			//throw new ArgumentException(new ArgumentError(_INTL("The level number ({1}) is invalid.", value)));
		}
		@exp = growth_rate.minimum_exp_for_level(value);
		@level = value;
		}
	}

	// Sets this Pokémon's Exp. Points.
	// @param value [Integer] new experience points
	public int exp { set {
		@exp = value;
		@level = null;
		}
	}

	// @return [Boolean] whether this Pokémon is an egg
	public bool egg() {
		return @steps_to_hatch > 0;
	}

	// @return [GameData.GrowthRate] this Pokémon's growth rate
	public void growth_rate() {
		return GameData.GrowthRate.get(species_data.growth_rate);
	}

	// @return [Integer] this Pokémon's base Experience value
	public void base_exp() {
		return species_data.base_exp;
	}

	// @return [Float] a number between 0 and 1 indicating how much of the current level's
	//   Exp this Pokémon has
	public void exp_fraction() {
		lvl = self.level;
		if (lvl >= GameData.GrowthRate.max_level) return 0.0;
		g_rate = growth_rate;
		start_exp = g_rate.minimum_exp_for_level(lvl);
		end_exp   = g_rate.minimum_exp_for_level(lvl + 1);
		return (@exp - start_exp).to_f / (end_exp - start_exp);
	}

	//-----------------------------------------------------------------------------
	// Status.
	//-----------------------------------------------------------------------------

	// Sets the Pokémon's health.
	// @param value [Integer] new HP value
	public int hp { set {
		@hp = value.clamp(0, @totalhp);
		if (@hp > 0) return;
		heal_status;
		@ready_to_evolve = false;
		if (isSpecies(Speciess.BASCULIN) || isSpecies(Speciess.YAMASK)) @evolution_counter = 0;
		}
	}

	// Sets this Pokémon's status. See {GameData.Status} for all possible status effects.
	// @param value [Symbol, String, GameData.Status] status to set
	public int status { set {
		if (!able()) return;
		new_status = GameData.Status.try_get(value);
		if (!new_status) {
			Debug.LogError(_INTL("Attempted to set {1} as Pokémon status", value.class.name));
			//throw new Exception(_INTL("Attempted to set {1} as Pokémon status", value.class.name));
		}
		@status = new_status.id;
		}
	}

	// @return [Boolean] whether the Pokémon is not fainted and not an egg
	public bool able() {
		return !egg() && @hp > 0;
	}

	// @return [Boolean] whether the Pokémon is fainted
	public bool fainted() {
		return !egg() && @hp <= 0;
	}

	// Heals all HP of this Pokémon.
	public void heal_HP() {
		if (egg()) return;
		@hp = @totalhp;
	}

	// Heals the status problem of this Pokémon.
	public void heal_status() {
		if (egg()) return;
		@status      = :NONE;
		@statusCount = 0;
	}

	// Restores all PP of this Pokémon. If a move index is given, restores the PP
	// of the move in that index.
	/// <param name="move_index">index of the move to heal (-1 if all moves</param>
	//   should be healed)
	public void heal_PP(Integer move_index = -1) {
		if (egg()) return;
		if (move_index >= 0) {
			@moves[move_index].pp = @moves[move_index].total_pp;
		} else {
			@moves.each(m => m.pp = m.total_pp);
		}
	}

	// Heals all HP, PP, and status problems of this Pokémon.
	public void heal() {
		if (egg()) return;
		heal_HP;
		heal_status;
		heal_PP;
		@ready_to_evolve = false;
	}

	//-----------------------------------------------------------------------------
	// Types.
	//-----------------------------------------------------------------------------

	// @return [Array<Symbol>] an array of this Pokémon's types
	public void types() {
		return species_data.types.clone;
	}

	/// <param name="type">type to check | Symbol, String, GameData.Type</param>
	// @return [Boolean] whether this Pokémon has the specified type
	public bool hasType(Symbol type) {
		type = GameData.Type.get(type).id;
		return self.types.Contains(type);
	}

	//-----------------------------------------------------------------------------
	// Gender.
	//-----------------------------------------------------------------------------

	// @return [0, 1, 2] this Pokémon's gender (0 = male, 1 = female, 2 = genderless)
	public void gender() {
		if (!@gender) {
			if (species_data.single_gendered()) {
				switch (species_data.gender_ratio) {
					case :AlwaysMale:    @gender = 0; break;
					case :AlwaysFemale:  @gender = 1; break;
					default:                    @gender = 2; break;
				}
			} else {
				female_chance = GameData.GenderRatio.get(species_data.gender_ratio).female_chance;
				@gender = ((@personalID & 0xFF) < female_chance) ? 1 : 0;
			}
		}
		return @gender;
	}

	// Sets this Pokémon's gender to a particular gender (if possible).
	// @param value [0, 1] new gender (0 = male, 1 = female)
	public int gender { set {
		if (singleGendered()) return;
		if (value.null() || value == 0 || value == 1) @gender = value;
		}
	}

	// Makes this Pokémon male.
	public int makeMale { get { return self.gender = 0; } }

	// Makes this Pokémon female.
	public int makeFemale { get { return self.gender = 1; } }

	// @return [Boolean] whether this Pokémon is male
	public bool male() { return self.gender == 0; }

	// @return [Boolean] whether this Pokémon is female
	public bool female() { return self.gender == 1; }

	// @return [Boolean] whether this Pokémon is genderless
	public bool genderless() { return self.gender == 2; }

	// @return [Boolean] whether this Pokémon species is restricted to only ever being one
	//   gender (or genderless)
	public bool singleGendered() {
		return species_data.single_gendered();
	}

	//-----------------------------------------------------------------------------
	// Shininess.
	//-----------------------------------------------------------------------------

	// @return [Boolean] whether this Pokémon is shiny (differently colored)
	public bool shiny() {
		if (@shiny.null()) {
			a = @personalID ^ @owner.id;
			b = a & 0xFFFF;
			c = (a >> 16) & 0xFFFF;
			d = b ^ c;
			@shiny = d < Settings.SHINY_POKEMON_CHANCE;
		}
		return @shiny;
	}

	// @return [Boolean] whether this Pokémon is super shiny (differently colored,
	//   square sparkles)
	public bool super_shiny() {
		if (@super_shiny.null()) {
			a = @personalID ^ @owner.id;
			b = a & 0xFFFF;
			c = (a >> 16) & 0xFFFF;
			d = b ^ c;
			@super_shiny = (d == 0);
		}
		return @super_shiny;
	}

	// @param value [Boolean] whether this Pokémon is super shiny
	public int super_shiny { set {
		@super_shiny = value;
		if (@super_shiny) @shiny = true;
		}
	}

	//-----------------------------------------------------------------------------
	// Ability.
	//-----------------------------------------------------------------------------

	// The index of this Pokémon's ability (0, 1 are natural abilities, 2+ are
	// hidden abilities) as defined for its species/form. An ability may not be
	// defined at this index. Is recalculated (as 0 or 1) if made null.
	// @return [Integer] the index of this Pokémon's ability
	public void ability_index() {
		if (!@ability_index) @ability_index = (@personalID & 1);
		return @ability_index;
	}

	// @param value [Integer, null] forced ability index (null if none is set)
	public int ability_index { set {
		@ability_index = value;
		@ability = null;
		}
	}

	// @return [GameData.Ability, null] an Ability object corresponding to this Pokémon's ability
	public void ability() {
		return GameData.Ability.try_get(ability_id);
	}

	// @return [Symbol, null] the ability symbol of this Pokémon's ability
	public void ability_id() {
		if (!@ability) {
			sp_data = species_data;
			abil_index = ability_index;
			if (abil_index >= 2) {   // Hidden ability
				@ability = sp_data.hidden_abilities[abil_index - 2];
				if (!@ability) abil_index = (@personalID & 1);
			}
			if (!@ability) {   // Natural ability or no hidden ability defined
				@ability = sp_data.abilities[abil_index] || sp_data.abilities[0];
			}
		}
		return @ability;
	}

	// @param value [Symbol, String, GameData.Ability, null] ability to set
	public int ability { set {
		if (value && !GameData.Ability.exists(value)) return;
		@ability = (value) ? GameData.Ability.get(value).id : value;
		}
	}

	// Returns whether this Pokémon has a particular ability. If no value
	// is given, returns whether this Pokémon has an ability set.
	/// <param name="check_ability">ability ID to check | Symbol, String, GameData.Ability, null</param>
	// @return [Boolean] whether this Pokémon has a particular ability or
	//   an ability at all
	public bool hasAbility(Symbol check_ability = null) {
		current_ability = self.ability;
		if (check_ability.null()) return !current_ability.null();
		return current_ability == check_ability;
	}

	// @return [Boolean] whether this Pokémon has a hidden ability
	public bool hasHiddenAbility() {
		return ability_index >= 2;
	}

	// @return [Array<Array<Symbol,Integer>>] the abilities this Pokémon can have,
	//   where every element is [ability ID, ability index]
	public void getAbilityList() {
		ret = new List<string>();
		sp_data = species_data;
		if (a }) sp_data.abilities.each_with_index { |a, i| ret.Add(new {a, i});
		if (a }) sp_data.hidden_abilities.each_with_index { |a, i| ret.Add(new {a, i + 2});
		return ret;
	}

	//-----------------------------------------------------------------------------
	// Nature.
	//-----------------------------------------------------------------------------

	// @return [GameData.Nature, null] a Nature object corresponding to this Pokémon's nature
	public void nature() {
		if (!@nature) {
			idx = @personalID % GameData.Nature.count;
			@nature = GameData.Nature.get(GameData.Nature.keys[idx]).id;
		}
		return GameData.Nature.try_get(@nature);
	}

	public void nature_id() {
		return @nature;
	}

	// Sets this Pokémon's nature to a particular nature.
	// @param value [Symbol, String, GameData.Nature, null] nature to change to
	public int nature { set {
		if (value && !GameData.Nature.exists(value)) return;
		@nature = (value) ? GameData.Nature.get(value).id : value;
		if (!@nature_for_stats) calc_stats;
		}
	}

	// Returns the calculated nature, taking into account things that change its
	// stat-altering effect (i.e. Gen 8 mints). Only used for calculating stats.
	// @return [GameData.Nature, null] this Pokémon's calculated nature
	public void nature_for_stats() {
		if (@nature_for_stats) return GameData.Nature.try_get(@nature_for_stats);
		return self.nature;
	}

	public void nature_for_stats_id() {
		return @nature_for_stats;
	}

	// If defined, this Pokémon's nature is considered to be this when calculating stats.
	// @param value [Symbol, String, GameData.Nature, null] ID of the nature to use for calculating stats
	public int nature_for_stats { set {
		if (value && !GameData.Nature.exists(value)) return;
		@nature_for_stats = (value) ? GameData.Nature.get(value).id : value;
		calc_stats;
		}
	}

	// Returns whether this Pokémon has a particular nature. If no value is given,
	// returns whether this Pokémon has a nature set.
	/// <param name="check_nature">nature ID to check | Symbol, String, GameData.Nature, null</param>
	// @return [Boolean] whether this Pokémon has a particular nature or a nature
	//   at all
	public bool hasNature(Symbol check_nature = null) {
		if (check_nature.null()) return !@nature.null();
		return self.nature == check_nature;
	}

	//-----------------------------------------------------------------------------
	// Items.
	//-----------------------------------------------------------------------------

	// @return [GameData.Item, null] an Item object corresponding to this Pokémon's item
	public void item() {
		return GameData.Item.try_get(@item);
	}

	public void item_id() {
		return @item;
	}

	// Gives an item to this Pokémon to hold.
	// @param value [Symbol, String, GameData.Item, null] ID of the item to give
	//   to this Pokémon
	public int item { set {
		if (value && !GameData.Item.exists(value)) return;
		@item = (value) ? GameData.Item.get(value).id : value;
		}
	}

	// Returns whether this Pokémon is holding an item. If an item id is passed,
	// returns whether the Pokémon is holding that item.
	/// <param name="check_item">item ID to check | Symbol, String, GameData.Item, null</param>
	// @return [Boolean] whether the Pokémon is holding the specified item or
	//   an item at all
	public bool hasItem(Symbol check_item = null) {
		if (check_item.null()) return !@item.null();
		held_item = self.item;
		return held_item && held_item == check_item;
	}

	// @return [Array<Array<Symbol>>] the items this species can be found holding in the wild
	public void wildHoldItems() {
		sp_data = species_data;
		return new {sp_data.wild_item_common, sp_data.wild_item_uncommon, sp_data.wild_item_rare};
	}

	// @return [Mail, null] mail held by this Pokémon (null if there is none)
	public void mail() {
		if (@mail && (!@mail.item || !hasItem(@mail.item))) @mail = null;
		return @mail;
	}

	// If mail is a Mail object, gives that mail to this Pokémon. If null is given,
	// removes the held mail.
	/// <param name="mail">mail to be held by this Pokémon | Mail, null</param>
	public int Mail mail { set {
		if (!mail.null() && !mail.is_a(Mail)) {
			Debug.LogError(_INTL("Invalid value {1} given", mail.inspect));
			//throw new Exception(_INTL("Invalid value {1} given", mail.inspect));
		}
		@mail = mail;
		}
	}

	//-----------------------------------------------------------------------------
	// Moves.
	//-----------------------------------------------------------------------------

	// @return [Integer] the number of moves known by the Pokémon
	public void numMoves() {
		return @moves.length;
	}

	/// <param name="move_id">ID of the move to check | Symbol, String, GameData.Move</param>
	// @return [Boolean] whether the Pokémon knows the given move
	public bool hasMove(Symbol move_id) {
		move_data = GameData.Move.try_get(move_id);
		if (!move_data) return false;
		return @moves.any(m => m.id == move_data.id);
	}

	// Returns the list of moves this Pokémon can learn by levelling up.
	// @return [Array<Array<Integer,Symbol>>] this Pokémon's move list, where every element is [level, move ID]
	public void getMoveList() {
		return species_data.moves;
	}

	// Sets this Pokémon's movelist to the default movelist it originally had.
	public void reset_moves() {
		this_level = self.level;
		// Find all level-up moves that self could have learned
		moveset = self.getMoveList;
		knowable_moves = new List<string>();
		moveset.each(m => { if (m[0] <= this_level) knowable_moves.Add(m[1]); });
		// Remove duplicates (retaining the latest copy of each move)
		knowable_moves = knowable_moves.reverse;
		knowable_moves |= new List<string>();
		knowable_moves = knowable_moves.reverse;
		// Add all moves
		@moves.clear;
		first_move_index = knowable_moves.length - MAX_MOVES;
		if (first_move_index < 0) first_move_index = 0;
		for (int i = first_move_index; i < knowable_moves.length; i++) { //each 'knowable_moves.length' do => |i|
			@moves.Add(new Pokemon.Move(knowable_moves[i]));
		}
	}

	// Silently learns the given move. Will erase the first known move if it has to.
	/// <param name="move_id">ID of the move to learn | Symbol, String, GameData.Move</param>
	public void learn_move(Symbol move_id) {
		move_data = GameData.Move.try_get(move_id);
		if (!move_data) return;
		// Check if self already knows the move; if so, move it to the end of the array
		@moves.each_with_index do |m, i|
			if (m.id != move_data.id) continue;
			@moves.Add(m);
			@moves.delete_at(i);
			return;
		}
		// Move is not already known; learn it
		@moves.Add(new Pokemon.Move(move_data.id));
		// Delete the first known move if self now knows more moves than it should
		if (numMoves > MAX_MOVES) @moves.shift;
	}

	// Deletes the given move from the Pokémon.
	/// <param name="move_id">ID of the move to delete | Symbol, String, GameData.Move</param>
	public void forget_move(Symbol move_id) {
		move_data = GameData.Move.try_get(move_id);
		if (!move_data) return;
		@moves.delete_if(m => m.id == move_data.id);
	}

	// Deletes the move at the given index from the Pokémon.
	/// <param name="index">index of the move to be deleted</param>
	public void forget_move_at_index(Integer index) {
		@moves.delete_at(index);
	}

	// Deletes all moves from the Pokémon.
	public void forget_all_moves() {
		@moves.clear;
	}

	// Copies currently known moves into a separate array, for Move Relearner.
	public void record_first_moves() {
		clear_first_moves;
		@moves.each(m => @first_moves.Add(m.id));
	}

	// Adds a move to this Pokémon's first moves.
	/// <param name="move_id">ID of the move to add | Symbol, String, GameData.Move</param>
	public void add_first_move(Symbol move_id) {
		move_data = GameData.Move.try_get(move_id);
		if (move_data && !@first_moves.Contains(move_data.id)) @first_moves.Add(move_data.id);
	}

	// Removes a move from this Pokémon's first moves.
	/// <param name="move_id">ID of the move to remove | Symbol, String, GameData.Move</param>
	public void remove_first_move(Symbol move_id) {
		move_data = GameData.Move.try_get(move_id);
		if (move_data) @first_moves.delete(move_data.id);
	}

	// Clears this Pokémon's first moves.
	public void clear_first_moves() {
		@first_moves.clear;
	}

	/// <param name="move_id">ID of the move to check | Symbol, String, GameData.Move</param>
	// @return [Boolean] whether the Pokémon is compatible with the given move
	public bool compatible_with_move(Symbol move_id) {
		move_data = GameData.Move.try_get(move_id);
		if (!move_data) return false;
		if (species_data.tutor_moves.Contains(move_data.id)) return true;
		if (getMoveList.any(m => m[1] == move_data.id)) return true;
		if (species_data.get_egg_moves.Contains(move_data.id)) return true;
		return false;
	}

	public bool can_relearn_move() {
		if (egg() || shadowPokemon()) return false;
		this_level = self.level;
		getMoveList.each(m => { if (m[0] <= this_level && !hasMove(m[1])) return true; });
		@first_moves.each(m => { if (!hasMove(m)) return true; });
		return false;
	}

	//-----------------------------------------------------------------------------
	// Ribbons.
	//-----------------------------------------------------------------------------

	// @return [Integer] the number of ribbons this Pokémon has
	public void numRibbons() {
		return @ribbons.length;
	}

	/// <param name="ribbon">ribbon ID to check for | Symbol, String, GameData.Ribbon</param>
	// @return [Boolean] whether this Pokémon has the specified ribbon
	public bool hasRibbon(Symbol ribbon) {
		ribbon_data = GameData.Ribbon.try_get(ribbon);
		return ribbon_data && @ribbons.Contains(ribbon_data.id);
	}

	// Gives a ribbon to this Pokémon.
	/// <param name="ribbon">ID of the ribbon to give | Symbol, String, GameData.Ribbon</param>
	public void giveRibbon(Symbol ribbon) {
		ribbon_data = GameData.Ribbon.try_get(ribbon);
		if (!ribbon_data || @ribbons.Contains(ribbon_data.id)) return;
		@ribbons.Add(ribbon_data.id);
	}

	// Replaces one ribbon with the next one along, if possible. If none of the
	// given ribbons are owned, give the first one.
	// @return [Symbol, String, GameData.Ribbon] ID of the ribbon that was gained
	public void upgradeRibbon(*args) {
		args.each_with_index do |ribbon, i|
			this_ribbon_data = GameData.Ribbon.try_get(ribbon);
			if (!this_ribbon_data) continue;
			for (int j = @ribbons.length; j < @ribbons.length; j++) { //for '@ribbons.length' times do => |j|
				if (@ribbons[j] != this_ribbon_data.id) continue;
				next_ribbon_data = GameData.Ribbon.try_get(args[i + 1]);
				if (!next_ribbon_data) continue;
				@ribbons[j] = next_ribbon_data.id;
				return @ribbons[j];
			}
		}
		first_ribbon_data = GameData.Ribbon.try_get(args.first);
		last_ribbon_data = GameData.Ribbon.try_get(args.last);
		if (first_ribbon_data && last_ribbon_data && !hasRibbon(last_ribbon_data.id)) {
			giveRibbon(first_ribbon_data.id);
			return first_ribbon_data.id;
		}
		return null;
	}

	// Removes the specified ribbon from this Pokémon.
	/// <param name="ribbon">ID of the ribbon to remove | Symbol, String, GameData.Ribbon</param>
	public void takeRibbon(Symbol ribbon) {
		ribbon_data = GameData.Ribbon.try_get(ribbon);
		if (!ribbon_data) return;
		@ribbons.delete_at(@ribbons.index(ribbon_data.id));
	}

	// Removes all ribbons from this Pokémon.
	public void clearAllRibbons() {
		@ribbons.clear;
	}

	//-----------------------------------------------------------------------------
	// Pokérus.
	//-----------------------------------------------------------------------------

	// @return [Integer] the Pokérus infection stage for this Pokémon
	public void pokerusStrain() {
		return @pokerus / 16;
	}

	// Returns the Pokérus infection stage for this Pokémon. The possible stages are
	// 0 (not infected), 1 (infected) and 2 (cured).
	// @return [0, 1, 2] current Pokérus infection stage
	public void pokerusStage() {
		if (@pokerus == 0) return 0;
		return ((@pokerus % 16) == 0) ? 2 : 1;
	}

	// Gives this Pokémon Pokérus (either the specified strain or a random one).
	/// <param name="strain">Pokérus strain to give (1-15 inclusive, or 0 for random)</param>
	public void givePokerus(Integer strain = 0) {
		if (self.pokerusStage == 2) return;   // Can't re-infect a cured Pokémon
		Game.GameData.stats.pokerus_infections += 1;
		if (strain <= 0 || strain >= 16) strain = rand(1...16);
		time = 1 + (strain % 4);
		@pokerus = time;
		@pokerus |= strain << 4;
	}

	// Resets the infection time for this Pokémon's Pokérus (even if cured).
	public void resetPokerusTime() {
		if (@pokerus == 0) return;
		strain = @pokerus / 16;
		time = 1 + (strain % 4);
		@pokerus = time;
		@pokerus |= strain << 4;
	}

	// Reduces the time remaining for this Pokémon's Pokérus (if infected).
	public void lowerPokerusCount() {
		if (self.pokerusStage != 1) return;
		@pokerus -= 1;
	}

	// Cures this Pokémon's Pokérus (if infected).
	public void curePokerus() {
		@pokerus -= @pokerus % 16;
	}

	//-----------------------------------------------------------------------------
	// Ownership, obtained information.
	//-----------------------------------------------------------------------------

	// Changes this Pokémon's owner.
	// @param new_owner [Owner] the owner to change to
	public int owner { set {
		validate new_owner => Owner;
		@owner = new_owner;
		}
	}

	/// <param name="trainer">the trainer to compare to the original trainer | Player, NPCTrainer, null</param>
	// @return [Boolean] whether the given trainer is not this Pokémon's original trainer
	public bool foreign(Player trainer = Game.GameData.player) {
		return @owner.id != trainer.id || @owner.name != trainer.name;
	}

	// @return [Time] the time when this Pokémon was obtained
	public void timeReceived() {
		return Time.at(@timeReceived);
	}

	// Sets the time when this Pokémon was obtained.
	// @param value [Integer, Time, #to_i] time in seconds since Unix epoch
	public int timeReceived { set {
		@timeReceived = value.ToInt();
		}	}

	// @return [Time] the time when this Pokémon hatched
	public void timeEggHatched() {
		return (obtain_method == 1) ? Time.at(@timeEggHatched) : null;
	}

	// Sets the time when this Pokémon hatched.
	// @param value [Integer, Time, #to_i] time in seconds since Unix epoch
	public int timeEggHatched { set {
		@timeEggHatched = value.ToInt();
		}	}

	//-----------------------------------------------------------------------------
	// Other.
	//-----------------------------------------------------------------------------

	// @return [String] the name of this Pokémon
	public void name() {
		return (nicknamed()) ? @name : speciesName;
	}

	// @param value [String] the nickname of this Pokémon
	public int name { set {
		if (!value || value.empty() || value == speciesName) value = null;
		@name = value;
		}	}

	// @return [Boolean] whether this Pokémon has been nicknamed
	public bool nicknamed() {
		return @name && !@name.empty();
	}

	// @return [String] the species name of this Pokémon
	public void speciesName() {
		return species_data.name;
	}

	// @return [Integer] the height of this Pokémon in decimetres (0.1 metres)
	public void height() {
		return species_data.height;
	}

	// @return [Integer] the weight of this Pokémon in hectograms (0.1 kilograms)
	public void weight() {
		return species_data.weight;
	}

	// @return [Hash<Integer>] the EV yield of this Pokémon (a hash with six key/value pairs)
	public void evYield() {
		this_evs = species_data.evs;
		ret = new List<string>();
		GameData.Stat.each_main(s => ret[s.id] = this_evs[s.id]);
		return ret;
	}

	public void affection_level() {
		switch (@happiness) {
			case 0...100:    return 0; break;
			case 100...150:  return 1; break;
			case 150...200:  return 2; break;
			case 200...230:  return 3; break;
			case 230...255:  return 4; break;
		}
		return 5;   // 255
	}

	// Changes the happiness of this Pokémon depending on what happened to change it.
	/// <param name="method">the happiness changing method (e.g. 'walking')</param>
	public void changeHappiness(String method) {
		gain = 0;
		happiness_range = @happiness / 100;
		switch (method) {
			case "walking":
				gain = new {2, 2, 1}[happiness_range];
				break;
			case "levelup":
				gain = new {5, 4, 3}[happiness_range];
				break;
			case "groom":
				gain = new {10, 10, 4}[happiness_range];
				break;
			case "evberry":
				gain = new {10, 5, 2}[happiness_range];
				break;
			case "vitamin":
				gain = new {5, 3, 2}[happiness_range];
				break;
			case "wing":
				gain = new {3, 2, 1}[happiness_range];
				break;
			case "machine": case "battleitem":
				gain = new {1, 1, 0}[happiness_range];
				break;
			case "faint":
				gain = -1;
				break;
			case "faintbad":   // Fainted against an opponent that is 30+ levels higher
				gain = new {-5, -5, -10}[happiness_range];
				break;
			case "powder":
				gain = new {-5, -5, -10}[happiness_range];
				break;
			case "energyroot":
				gain = new {-10, -10, -15}[happiness_range];
				break;
			case "revivalherb":
				gain = new {-15, -15, -20}[happiness_range];
				break;
			default:
				Debug.LogError(_INTL("Unknown happiness-changing method: {1}", method.ToString()));
				//throw new ArgumentException(_INTL("Unknown happiness-changing method: {1}", method.ToString()));
				break;
		}
		if (gain > 0) {
			if (@obtain_map == Game.GameData.game_map.map_id) gain += 1;
			if (@poke_ball == Items.LUXURY_BALL) gain += 1;
			if (hasItem(Items.SOOTHEBELL)) gain = (int)Math.Floor(gain * 1.5);
			if (Settings.APPLY_HAPPINESS_SOFT_CAP && method != "evberry") {
				gain = (@happiness >= 179) ? 0 : gain.clamp(0, 179 - @happiness);
			}
		}
		@happiness = (@happiness + gain).clamp(0, 255);
	}

	//-----------------------------------------------------------------------------
	// Evolution checks.
	//-----------------------------------------------------------------------------

	// Checks whether this Pokemon can evolve because of levelling up.
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_on_level_up() {
		return check_evolution_internal do |pkmn, new_species, method, parameter|
			success = GameData.Evolution.get(method).call_level_up(pkmn, parameter);
			next (success) ? new_species : null;
		}
	}

	// Checks whether this Pokemon can evolve because of levelling up in battle.
	// This also checks call_level_up as above.
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_on_battle_level_up() {
		return check_evolution_internal do |pkmn, new_species, method, parameter|
			success = GameData.Evolution.get(method).call_battle_level_up(pkmn, parameter);
			next (success) ? new_species : null;
		}
	}

	// Checks whether this Pokemon can evolve because of using an item on it.
	/// <param name="item_used">the item being used | Symbol, GameData.Item, null</param>
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_on_use_item(Symbol item_used) {
		return check_evolution_internal do |pkmn, new_species, method, parameter|
			success = GameData.Evolution.get(method).call_use_item(pkmn, parameter, item_used);
			next (success) ? new_species : null;
		}
	}

	// Checks whether this Pokemon can evolve because of being traded.
	/// <param name="other_pkmn">the other Pokémon involved in the trade</param>
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_on_trade(Pokemon other_pkmn) {
		return check_evolution_internal do |pkmn, new_species, method, parameter|
			success = GameData.Evolution.get(method).call_on_trade(pkmn, parameter, other_pkmn);
			next (success) ? new_species : null;
		}
	}

	// Checks whether this Pokemon can evolve after a battle.
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_after_battle(party_index) {
		return check_evolution_internal do |pkmn, new_species, method, parameter|
			success = GameData.Evolution.get(method).call_after_battle(pkmn, party_index, parameter);
			next (success) ? new_species : null;
		}
	}

	// Checks whether this Pokemon can evolve by a triggered event.
	/// <param name="value">a value that may be used by the evolution method</param>
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_by_event(Integer value = 0) {
		return check_evolution_internal do |pkmn, new_species, method, parameter|
			success = GameData.Evolution.get(method).call_event(pkmn, parameter, value);
			next (success) ? new_species : null;
		}
	}

	// Called after this Pokémon evolves, to remove its held item (if the evolution
	// required it to have a held item) or duplicate this Pokémon (Shedinja only).
	/// <param name="new_species">the species that this Pokémon evolved into</param>
	public void action_after_evolution(Symbol new_species) {
		foreach (var evo in species_data.get_evolutions(true)) { //'species_data.get_evolutions(true).each' do => |evo|   // [new_species, method, parameter]
			if (GameData.Evolution.get(evo[1]).call_after_evolution(self, evo[0], evo[2], new_species)) break;
		}
	}

	// The core method that performs evolution checks. Needs a block given to it,
	// which will provide either a GameData.Species ID (the species to evolve
	// into) or null (keep checking).
	// @return [Symbol, null] the ID of the species to evolve into
	public void check_evolution_internal() {
		if (egg() || shadowPokemon()) return null;
		if (hasItem(Items.EVERSTONE)) return null;
		if (hasAbility(Abilitys.BATTLEBOND)) return null;
		foreach (var evo in species_data.get_evolutions(true)) { //'species_data.get_evolutions(true).each' do => |evo|   // [new_species, method, parameter, boolean]
			if (evo[3]) continue;   // Prevolution
			ret = yield self, evo[0], evo[1], evo[2];   // pkmn, new_species, method, parameter
			if (ret) return ret;
		}
		return null;
	}

	public void trigger_event_evolution(number) {
		new_species = check_evolution_by_event(number);
		if (new_species) {
			FadeOutInWithMusic do;
				evo = new PokemonEvolutionScene();
				evo.StartScreen(self, new_species);
				evo.Evolution;
				evo.EndScreen;
			}
			return true;
		}
		return false;
	}

	//-----------------------------------------------------------------------------
	// Stat calculations.
	//-----------------------------------------------------------------------------

	// @return [Hash<Integer>] this Pokémon's base stats, a hash with six key/value pairs
	public void baseStats() {
		this_base_stats = species_data.base_stats;
		ret = new List<string>();
		GameData.Stat.each_main(s => ret[s.id] = this_base_stats[s.id]);
		return ret;
	}

	// Returns this Pokémon's effective IVs, taking into account Hyper Training.
	// Only used for calculating stats.
	// @return [Hash<Integer>] hash containing this Pokémon's effective IVs
	public void calcIV() {
		this_ivs = self.iv;
		ret = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
			ret[s.id] = (@ivMaxed[s.id]) ? IV_STAT_LIMIT : this_ivs[s.id];
		}
		return ret;
	}

	// @return [Integer] the maximum HP of this Pokémon
	public void calcHP(base, level, iv, ev) {
		if (base == 1) return 1;   // For Shedinja
		if (Settings.DISABLE_IVS_AND_EVS) iv = ev = 0;
		return (int)Math.Floor(((base * 2) + iv + (ev / 4)) * level / 100) + level + 10;
	}

	// @return [Integer] the specified stat of this Pokémon (not used for total HP)
	public void calcStat(base, level, iv, ev, nat) {
		if (Settings.DISABLE_IVS_AND_EVS) iv = ev = 0;
		return (int)Math.Floor(((int)Math.Floor(((base * 2) + iv + (ev / 4)) * level / 100) + 5) * nat / 100);
	}

	// Recalculates this Pokémon's stats.
	public void calc_stats() {
		base_stats = self.baseStats;
		this_level = self.level;
		this_IV    = self.calcIV;
		// Format stat multipliers due to nature
		nature_mod = new List<string>();
		GameData.Stat.each_main(s => nature_mod[s.id] = 100);
		this_nature = self.nature_for_stats;
		if (this_nature) {
			this_nature.stat_changes.each(change => nature_mod[change[0]] += change[1]);
		}
		// Calculate stats
		stats = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
			if (s.id == :HP) {
				stats[s.id] = calcHP(base_stats[s.id], this_level, this_IV[s.id], @ev[s.id]);
			} else {
				stats[s.id] = calcStat(base_stats[s.id], this_level, this_IV[s.id], @ev[s.id], nature_mod[s.id]);
			}
		}
		hp_difference = stats[:HP] - @totalhp;
		@totalhp = stats[:HP];
		if (@hp > 0 || hp_difference > 0) self.hp = (int)Math.Max(@hp + hp_difference, 1);
		@attack  = stats[:ATTACK];
		@defense = stats[:DEFENSE];
		@spatk   = stats[:SPECIAL_ATTACK];
		@spdef   = stats[:SPECIAL_DEFENSE];
		@speed   = stats[:SPEED];
	}

	//-----------------------------------------------------------------------------
	// Pokémon creation.
	//-----------------------------------------------------------------------------

	// Creates a copy of this Pokémon and returns it.
	// @return [Pokemon] a copy of this Pokémon
	public override void clone() {
		ret = base.clone();
		ret.iv          = new List<string>();
		ret.ivMaxed     = new List<string>();
		ret.ev          = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
			ret.iv[s.id]      = @iv[s.id];
			ret.ivMaxed[s.id] = @ivMaxed[s.id];
			ret.ev[s.id]      = @ev[s.id];
		}
		ret.moves       = new List<string>();
		@moves.each_with_index((m, i) => ret.moves[i] = m.clone);
		ret.first_moves = @first_moves.clone;
		ret.owner       = @owner.clone;
		ret.ribbons     = @ribbons.clone;
		return ret;
	}

	// Creates a new Pokémon object.
	/// <param name="species">Pokémon species | Symbol, String, GameData.Species</param>
	/// <param name="level">Pokémon level</param>
	/// <param name="owner">Pokémon owner (the player by default) | Owner, Player, NPCTrainer</param>
	/// <param name="withMoves">whether the Pokémon should have moves</param>
	/// <param name="recheck_form">whether to auto-check the form</param>
	public void initialize(Symbol species, Integer level, Owner owner = Game.GameData.player, Boolean withMoves = true, Boolean recheck_form = true) {
		species_data = GameData.Species.get(species);
		@species          = species_data.species;
		@form             = species_data.base_form;
		@forced_form      = null;
		@time_form_set    = null;
		self.level        = level;
		@steps_to_hatch   = 0;
		heal_status;
		@gender           = null;
		@shiny            = null;
		@ability_index    = null;
		@ability          = null;
		@nature           = null;
		@nature_for_stats = null;
		@item             = null;
		@mail             = null;
		@moves            = new List<string>();
		if (withMoves) reset_moves;
		@first_moves      = new List<string>();
		@ribbons          = new List<string>();
		@cool             = 0;
		@beauty           = 0;
		@cute             = 0;
		@smart            = 0;
		@tough            = 0;
		@sheen            = 0;
		@pokerus          = 0;
		@name             = null;
		@happiness        = species_data.happiness;
		@poke_ball        = :POKEBALL;
		@markings         = new List<string>();
		@iv               = new List<string>();
		@ivMaxed          = new List<string>();
		@ev               = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
			@iv[s.id]       = rand(IV_STAT_LIMIT + 1);
			@ev[s.id]       = 0;
		}
		switch (owner) {
			case Owner:
				@owner = owner;
				break;
			case Player: case NPCTrainer:
				@owner = Owner.new_from_trainer(owner);
				break;
			default:
				@owner = new Owner(0, "", 2, 2);
				break;
		}
		@obtain_method    = 0;   // Met
		if (Game.GameData.game_switches && Game.GameData.game_switches[Settings.FATEFUL_ENCOUNTER_SWITCH]) @obtain_method    = 4;
		@obtain_map       = (Game.GameData.game_map) ? Game.GameData.game_map.map_id : 0;
		@obtain_text      = null;
		@obtain_level     = level;
		@hatched_map      = 0;
		@timeReceived     = Time.now.ToInt();
		@timeEggHatched   = null;
		@fused            = null;
		@personalID       = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
		@hp               = 1;
		@totalhp          = 1;
		calc_stats;
		if (@form == 0 && recheck_form) {
			f = MultipleForms.call("getFormOnCreation", self);
			if f
				self.form = f;
				if (withMoves) reset_moves;
			}
		}
	}
}
