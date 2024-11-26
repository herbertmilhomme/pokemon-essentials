//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Species {
		public int id		{ get { return _id; } }			protected int _id;
		public int species		{ get { return _species; } }			protected int _species;
		public int form		{ get { return _form; } }			protected int _form;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int real_form_name		{ get { return _real_form_name; } }			protected int _real_form_name;
		public int real_category		{ get { return _real_category; } }			protected int _real_category;
		public int real_pokedex_entry		{ get { return _real_pokedex_entry; } }			protected int _real_pokedex_entry;
		public int pokedex_form		{ get { return _pokedex_form; } }			protected int _pokedex_form;
		public int types		{ get { return _types; } }			protected int _types;
		public int base_stats		{ get { return _base_stats; } }			protected int _base_stats;
		public int evs		{ get { return _evs; } }			protected int _evs;
		public int base_exp		{ get { return _base_exp; } }			protected int _base_exp;
		public int growth_rate		{ get { return _growth_rate; } }			protected int _growth_rate;
		public int gender_ratio		{ get { return _gender_ratio; } }			protected int _gender_ratio;
		public int catch_rate		{ get { return _catch_rate; } }			protected int _catch_rate;
		public int happiness		{ get { return _happiness; } }			protected int _happiness;
		public int moves		{ get { return _moves; } }			protected int _moves;
		public int tutor_moves		{ get { return _tutor_moves; } }			protected int _tutor_moves;
		public int egg_moves		{ get { return _egg_moves; } }			protected int _egg_moves;
		public int abilities		{ get { return _abilities; } }			protected int _abilities;
		public int hidden_abilities		{ get { return _hidden_abilities; } }			protected int _hidden_abilities;
		public int wild_item_common		{ get { return _wild_item_common; } }			protected int _wild_item_common;
		public int wild_item_uncommon		{ get { return _wild_item_uncommon; } }			protected int _wild_item_uncommon;
		public int wild_item_rare		{ get { return _wild_item_rare; } }			protected int _wild_item_rare;
		public int egg_groups		{ get { return _egg_groups; } }			protected int _egg_groups;
		public int hatch_steps		{ get { return _hatch_steps; } }			protected int _hatch_steps;
		public int incense		{ get { return _incense; } }			protected int _incense;
		public int offspring		{ get { return _offspring; } }			protected int _offspring;
		public int evolutions		{ get { return _evolutions; } }			protected int _evolutions;
		public int height		{ get { return _height; } }			protected int _height;
		public int weight		{ get { return _weight; } }			protected int _weight;
		public int color		{ get { return _color; } }			protected int _color;
		public int shape		{ get { return _shape; } }			protected int _shape;
		public int habitat		{ get { return _habitat; } }			protected int _habitat;
		public int generation		{ get { return _generation; } }			protected int _generation;
		public int flags		{ get { return _flags; } }			protected int _flags;
		public int mega_stone		{ get { return _mega_stone; } }			protected int _mega_stone;
		public int mega_move		{ get { return _mega_move; } }			protected int _mega_move;
		public int unmega_form		{ get { return _unmega_form; } }			protected int _unmega_form;
		public int mega_message		{ get { return _mega_message; } }			protected int _mega_message;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "species.dat";
		PBS_BASE_FILENAME = new {"pokemon", "pokemon_forms"};

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void schema(compiling_forms = false) {
			ret = new List<string>();
			if (compiling_forms) {
				ret["SectionName"]    = new {:id,                 "ev", :Species};
			} else {
				ret["SectionName"]    = new {:id,                 "m"};
				ret["Name"]           = new {:real_name,          "s"};
			}
			ret["FormName"]         = new {:real_form_name,     "q"};
			if (compiling_forms) {
				ret["PokedexForm"]    = new {:pokedex_form,       "u"};
				ret["MegaStone"]      = new {:mega_stone,         "e", :Item};
				ret["MegaMove"]       = new {:mega_move,          "e", :Move};
				ret["UnmegaForm"]     = new {:unmega_form,        "u"};
				ret["MegaMessage"]    = new {:mega_message,       "u"};
			}
			ret["Types"]            = new {:types,              "*e", :Type};
			ret["BaseStats"]        = new {:base_stats,         "vvvvvv"};
			if (!compiling_forms) {
				ret["GenderRatio"]    = new {:gender_ratio,       "e", :GenderRatio};
				ret["GrowthRate"]     = new {:growth_rate,        "e", :GrowthRate};
			}
			ret["BaseExp"]          = new {:base_exp,           "v"};
			ret["EVs"]              = new {:evs,                "*ev", :Stat};
			ret["CatchRate"]        = new {:catch_rate,         "u"};
			ret["Happiness"]        = new {:happiness,          "u"};
			ret["Abilities"]        = new {:abilities,          "*e", :Ability};
			ret["HiddenAbilities"]  = new {:hidden_abilities,   "*e", :Ability};
			ret["Moves"]            = new {:moves,              "*ue", null, :Move};
			ret["TutorMoves"]       = new {:tutor_moves,        "*e", :Move};
			ret["EggMoves"]         = new {:egg_moves,          "*e", :Move};
			ret["EggGroups"]        = new {:egg_groups,         "*e", :EggGroup};
			ret["HatchSteps"]       = new {:hatch_steps,        "v"};
			if (compiling_forms) {
				ret["Offspring"]      = new {:offspring,          "*e", :Species};
			} else {
				ret["Incense"]        = new {:incense,            "e", :Item};
				ret["Offspring"]      = new {:offspring,          "*s"};
			}
			ret["Height"]           = new {:height,             "f"};
			ret["Weight"]           = new {:weight,             "f"};
			ret["Color"]            = new {:color,              "e", :BodyColor};
			ret["Shape"]            = new {:shape,              "e", :BodyShape};
			ret["Habitat"]          = new {:habitat,            "e", :Habitat};
			ret["Category"]         = new {:real_category,      "s"};
			ret["Pokedex"]          = new {:real_pokedex_entry, "q"};
			ret["Generation"]       = new {:generation,         "i"};
			ret["Flags"]            = new {:flags,              "*s"};
			ret["WildItemCommon"]   = new {:wild_item_common,   "*e", :Item};
			ret["WildItemUncommon"] = new {:wild_item_uncommon, "*e", :Item};
			ret["WildItemRare"]     = new {:wild_item_rare,     "*e", :Item};
			if (compiling_forms) {
				ret["Evolutions"]     = new {:evolutions,         "*ees", :Species, :Evolution};
				ret["Evolution"]      = new {:evolutions,         "^eeS", :Species, :Evolution};
			} else {
				ret["Evolutions"]     = new {:evolutions,         "*ses", null, :Evolution};
				ret["Evolution"]      = new {:evolutions,         "^seS", null, :Evolution};
			}
			return ret;
		}

		public static void editor_properties() {
			return new {
				new {"ID",                ReadOnlyProperty,                   _INTL("The ID of the Pokémon.")},
				new {"Name",              new LimitStringProperty(Pokemon.MAX_NAME_SIZE), _INTL("Name of the Pokémon.")},
				new {"FormName",          StringProperty,                     _INTL("Name of this form of the Pokémon.")},
				new {"Types",             new GameDataPoolProperty(:Type, false), _INTL("The Pokémon's type(s).")},
				new {"BaseStats",         BaseStatsProperty,                  _INTL("Base stats of the Pokémon.")},
				new {"GenderRatio",       new GameDataProperty(:GenderRatio), _INTL("Proportion of males to females for this species.")},
				new {"GrowthRate",        new GameDataProperty(:GrowthRate),  _INTL("Pokémon's growth rate.")},
				new {"BaseExp",           new LimitProperty(9999),            _INTL("Base experience earned when this species is defeated.")},
				new {"EVs",               EffortValuesProperty,               _INTL("Effort Value points earned when this species is defeated.")},
				new {"CatchRate",         new LimitProperty(255),             _INTL("Catch rate of this species (0-255).")},
				new {"Happiness",         new LimitProperty(255),             _INTL("Base happiness of this species (0-255).")},
				new {"Abilities",         new AbilitiesProperty(),              _INTL("Abilities which the Pokémon can have (max. 2).")},
				new {"HiddenAbilities",   new AbilitiesProperty(),              _INTL("Secret abilities which the Pokémon can have.")},
				new {"Moves",             LevelUpMovesProperty,               _INTL("Moves which the Pokémon learns while levelling up.")},
				new {"TutorMoves",        new EggMovesProperty(),               _INTL("Moves which the Pokémon can be taught by TM/HM/Move Tutor.")},
				new {"EggMoves",          new EggMovesProperty(),               _INTL("Moves which the Pokémon can learn via breeding.")},
				new {"EggGroups",         new EggGroupsProperty(),              _INTL("Egg groups that the Pokémon belongs to for breeding purposes.")},
				new {"HatchSteps",        new LimitProperty(99_999),          _INTL("Number of steps until an egg of this species hatches.")},
				new {"Incense",           ItemProperty,                       _INTL("Item needed to be held by a parent to produce an egg of this species.")},
				new {"Offspring",         new GameDataPoolProperty(:Species), _INTL("All possible species that an egg can be when breeding for an egg of this species (if blank, the egg can only be this species).")},
				new {"Height",            new NonzeroLimitProperty(999),      _INTL("Height of the Pokémon in 0.1 metres (e.g. 42 = 4.2m).")},
				new {"Weight",            new NonzeroLimitProperty(9999),     _INTL("Weight of the Pokémon in 0.1 kilograms (e.g. 42 = 4.2kg).")},
				new {"Color",             new GameDataProperty(:BodyColor),   _INTL("Pokémon's body color.")},
				new {"Shape",             new GameDataProperty(:BodyShape),   _INTL("Body shape of this species.")},
				new {"Habitat",           new GameDataProperty(:Habitat),     _INTL("The habitat of this species.")},
				new {"Category",          StringProperty,                     _INTL("Kind of Pokémon species.")},
				new {"Pokedex",           StringProperty,                     _INTL("Description of the Pokémon as displayed in the Pokédex.")},
				new {"Generation",        new LimitProperty(99_999),          _INTL("The number of the generation the Pokémon debuted in.")},
				new {"Flags",             StringListProperty,                 _INTL("Words/phrases that distinguish this species from others.")},
				new {"WildItemCommon",    new GameDataPoolProperty(:Item),    _INTL("Item(s) commonly held by wild Pokémon of this species.")},
				new {"WildItemUncommon",  new GameDataPoolProperty(:Item),    _INTL("Item(s) uncommonly held by wild Pokémon of this species.")},
				new {"WildItemRare",      new GameDataPoolProperty(:Item),    _INTL("Item(s) rarely held by wild Pokémon of this species.")},
				new {"Evolutions",        new EvolutionsProperty(),             _INTL("Evolution paths of this species.")}
			}
		}

		/// <param name="species"> | Symbol, self, String</param>
		/// <param name="form"></param>
		// @return [self, null]
		public static void get_species_form(Symbol species, Integer form) {
			if (!species || !form) return null;
			validate species => new {Symbol, self, String};
			validate form => Integer;
			if (species.is_a(self)) species = species.species;
			if (species.is_a(String)) species = species.to_sym;
			trial = string.Format("{0}_{0}", species, form).to_sym;
			species_form = (DATA[trial].null()) ? species : trial;
			return (DATA.has_key(species_form)) ? DATA[species_form] : null;
		}

		public static void each_species() {
			DATA.each_value(species => { if (species.form == 0) yield species; });
		}

		public static void each_form_for_species(species) {
			DATA.each_value(species => { if (species.species == species) yield species; });
		}

		public static void species_count() {
			ret = 0;
			self.each_species(species => ret += 1);
			return ret;
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id                 = hash.id;
			@species            = hash.species            || @id;
			@form               = hash.form               || 0;
			@real_name          = hash.real_name          || "Unnamed";
			@real_form_name     = hash.real_form_name;
			@real_category      = hash.real_category      || "???";
			@real_pokedex_entry = hash.real_pokedex_entry || "???";
			@pokedex_form       = hash.pokedex_form       || @form;
			@types              = hash.types              || [:NORMAL];
			@base_stats         = hash.base_stats         || {};
			@evs                = hash.evs                || {};
			foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
				if (!@base_stats[s.id] || @base_stats[s.id] <= 0) @base_stats[s.id] = 1;
				if (!@evs[s.id] || @evs[s.id] < 0) @evs[s.id]        = 0;
			}
			@base_exp           = hash.base_exp           || 100;
			@growth_rate        = hash.growth_rate        || :Medium;
			@gender_ratio       = hash.gender_ratio       || :Female50Percent;
			@catch_rate         = hash.catch_rate         || 255;
			@happiness          = hash.happiness          || 70;
			@moves              = hash.moves              || [];
			@tutor_moves        = hash.tutor_moves        || [];
			@egg_moves          = hash.egg_moves          || [];
			@abilities          = hash.abilities          || [];
			@hidden_abilities   = hash.hidden_abilities   || [];
			@wild_item_common   = hash.wild_item_common   || [];
			@wild_item_uncommon = hash.wild_item_uncommon || [];
			@wild_item_rare     = hash.wild_item_rare     || [];
			@egg_groups         = hash.egg_groups         || [:Undiscovered];
			@hatch_steps        = hash.hatch_steps        || 1;
			@incense            = hash.incense;
			@offspring          = hash.offspring          || [];
			@evolutions         = hash.evolutions         || [];
			@height             = hash.height             || 1;
			@weight             = hash.weight             || 1;
			@color              = hash.color              || :Red;
			@shape              = hash.shape              || :Head;
			@habitat            = hash.habitat            || :None;
			@generation         = hash.generation         || 0;
			@flags              = hash.flags              || [];
			@mega_stone         = hash.mega_stone;
			@mega_move          = hash.mega_move;
			@unmega_form        = hash.unmega_form        || 0;
			@mega_message       = hash.mega_message       || 0;
			@s_file_suffix    = hash.s_file_suffix    || "";
		}

		// @return [String] the translated name of this species
		public void name() {
			return GetMessageFromHash(MessageTypes.SPECIES_NAMES, @real_name);
		}

		// @return [String] the translated name of this form of this species
		public void form_name() {
			return GetMessageFromHash(MessageTypes.SPECIES_FORM_NAMES, @real_form_name);
		}

		// @return [String] the translated Pokédex category of this species
		public void category() {
			return GetMessageFromHash(MessageTypes.SPECIES_CATEGORIES, @real_category);
		}

		// @return [String] the translated Pokédex entry of this species
		public void pokedex_entry() {
			return GetMessageFromHash(MessageTypes.POKEDEX_ENTRIES, @real_pokedex_entry);
		}

		public void default_form() {
			@flags.each do |flag|
				if (System.Text.RegularExpressions.Regex.IsMatch(flag,@"^DefaultForm_(\d+)$",RegexOptions.IgnoreCase)) return $~[1].ToInt();
			}
			return -1;
		}

		public void base_form() {
			default = default_form;
			return (default >= 0) ? default : @form;
		}

		public bool single_gendered() {
			return GameData.GenderRatio.get(@gender_ratio).single_gendered();
		}

		public void base_stat_total() {
			return @base_stats.values.sum;
		}

		public bool has_flag(flag) {
			return @flags.any(f => f.downcase == flag.downcase);
		}

		public void apply_metrics_to_sprite(sprite, index, shadow = false) {
			metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
			metrics_data.apply_metrics_to_sprite(sprite, index, shadow);
		}

		public bool shows_shadow() {
			metrics_data = GameData.SpeciesMetrics.get_species_form(@species, @form);
			return metrics_data.shows_shadow();
		}

		public void get_evolutions(exclude_invalid = false) {
			ret = new List<string>();
			@evolutions.each do |evo|
				if (evo[3]) continue;   // Is the prevolution
				if (evo[1] == :None && exclude_invalid) continue;
				ret.Add(new {evo[0], evo[1], evo[2]});   // new {Species, method, parameter}
			}
			return ret;
		}

		public void get_family_evolutions(exclude_invalid = true) {
			evos = get_evolutions(exclude_invalid);
			evos = evos.sort((a, b) => GameData.Species.keys.index(a[0]) <=> GameData.Species.keys.index(b[0]));
			ret = new List<string>();
			foreach (var evo in evos) { //'evos.each' do => |evo|
				ret.Add([@species].concat(evo));   // new {Prevo species, evo species, method, parameter}
				evo_array = GameData.Species.get(evo[0]).get_family_evolutions(exclude_invalid);
				if (evo_array && evo_array.length > 0) ret.concat(evo_array);
			}
			return ret;
		}

		public void get_previous_species() {
			if (@evolutions.length == 0) return @species;
			@evolutions.each(evo => { if (evo[3]) return evo[0]; });   // Is the prevolution
			return @species;
		}

		public void get_baby_species(check_items = false, item1 = null, item2 = null) {
			ret = @species;
			if (@evolutions.length == 0) return ret;
			@evolutions.each do |evo|
				if (!evo[3]) continue;   // Check only the prevolution
				if (check_items) {
					incense = GameData.Species.get(evo[0]).incense;
					if (!incense || item1 == incense || item2 == incense) ret = evo[0];
				} else {
					ret = evo[0];   // Species of prevolution
				}
				break;
			}
			if (ret != @species) ret = GameData.Species.get(ret).get_baby_species(check_items, item1, item2);
			return ret;
		}

		// Returns an array of all the species in this species' evolution family.
		public void get_family_species() {
			sp = get_baby_species;
			evos = GameData.Species.get(sp).get_family_evolutions(false);
			if (evos.length == 0) return [sp];
			return [sp].concat(evos.map(e => e[1])).uniq;
		}

		// This takes into account whether other_species is evolved.
		public bool breeding_can_produce(other_species) {
			other_family = GameData.Species.get(other_species).get_family_species;
			if (@offspring.length > 0) {
				return (other_family & @offspring).length > 0;
			}
			return other_family.Contains(@species);
		}

		// If this species doesn't have egg moves, looks at prevolutions one at a
		// time and returns theirs instead.
		public void get_egg_moves() {
			if (!@egg_moves.empty()) return @egg_moves;
			prevo = get_previous_species;
			if (prevo != @species) return GameData.Species.get_species_form(prevo, @form).get_egg_moves;
			return @egg_moves;
		}

		public bool family_evolutions_have_method(check_method, check_param = null) {
			sp = get_baby_species;
			evos = GameData.Species.get(sp).get_family_evolutions;
			if (evos.length == 0) return false;
			foreach (var evo in evos) { //'evos.each' do => |evo|
				if (check_method.Length > 0) {
					if (!check_method.Contains(evo[2])) continue;
				} else if (evo[2] != check_method) {
					continue;
				}
				if (check_param.null() || evo[3] == check_param) return true;
			}
			return false;
		}

		// Used by the Moon Ball when checking if a Pokémon's evolution family
		// includes an evolution that uses the Moon Stone.
		public bool family_item_evolutions_use_item(check_item = null) {
			sp = get_baby_species;
			evos = GameData.Species.get(sp).get_family_evolutions;
			if (!evos || evos.length == 0) return false;
			foreach (var evo in evos) { //'evos.each' do => |evo|
				if (GameData.Evolution.get(evo[2]).use_item_proc.null()) continue;
				if (check_item.null() || evo[3] == check_item) return true;
			}
			return false;
		}

		public void minimum_level() {
			if (@evolutions.length == 0) return 1;
			@evolutions.each do |evo|
				if (!evo[3]) continue;   // Check only the prevolution
				prevo_data = GameData.Species.get_species_form(evo[0], base_form);
				if (!prevo_data.incense.null()) return 1;
				prevo_min_level = prevo_data.minimum_level;
				evo_method_data = GameData.Evolution.get(evo[1]);
				if (evo_method_data.level_up_proc.null() &&
																	evo_method_data.battle_level_up_proc.null() &&
																	evo_method_data.id != :Shedinja) return prevo_min_level;
				any_level_up = evo_method_data.any_level_up;
				return (any_level_up) ? prevo_min_level + 1 : evo[2];
			}
			return 1;
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key, writing_form = false) {
			if (key == "ID") key = "SectionName";
			ret = null;
			if (self.class.schema(writing_form).Contains(key)) {
				ret = self.send(self.class.schema(writing_form)[key][0]);
				if (ret == false || (ret.Length > 0 && ret.length == 0)) ret = null;
			}
			switch (key) {
				case "SectionName":
					if (writing_form) ret = new {@species, @form};
					break;
				case "FormName":
					if (nil_or_empty(ret)) ret = null;
					break;
				case "PokedexForm":
					if (ret == @form) ret = null;
					break;
				case "UnmegaForm": case "MegaMessage": case "Generation":
					if (ret == 0) ret = null;
					break;
				case "BaseStats":
					new_ret = new List<string>();
					foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
						if (s.s_order >= 0) new_ret[s.s_order] = ret[s.id];
					}
					ret = new_ret;
					break;
				case "EVs":
					new_ret = new List<string>();
					foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
						if (ret[s.id] > 0 && s.s_order >= 0) new_ret.Add(new {s.id, ret[s.id]});
					}
					ret = new_ret;
					break;
				case "Height": case "Weight":
					ret = ret.to_f / 10;
					break;
				case "Habitat":
					if (ret == :None) ret = null;
					break;
				case "Evolutions":
					ret = null;   // Want to use "Evolution" instead
					break;
				case "Evolution":
					if (ret) {
						ret = ret.reject(evo => evo[3]);   // Remove prevolutions
						foreach (var evo in ret) { //'ret.each' do => |evo|
							param_type = GameData.Evolution.get(evo[1]).parameter;
							if (!param_type.null()) {
								if (param_type.is_a(Symbol) && !GameData.const_defined(param_type)) {
									evo[2] = getConstantName(param_type, evo[2]);
								} else {
									evo[2] = evo[2].ToString();
								}
							}
						}
						ret.each_with_index((evo, i) => ret[i] = evo[0, 3]);
						if (ret.length == 0) ret = null;
					}
					break;
			}
			if (writing_form && !ret.null()) {
				base_form = GameData.Species.get(@species);
				if (!new []{"WildItemCommon", "WildItemUncommon", "WildItemRare"}.Contains(key) ||
					(base_form.wild_item_common == @wild_item_common &&
					base_form.wild_item_uncommon == @wild_item_uncommon &&
					base_form.wild_item_rare == @wild_item_rare)) {
					if (base_form.get_property_for_PBS(key) == ret) ret = null;
				}
			}
			return ret;
		}
	}
}
