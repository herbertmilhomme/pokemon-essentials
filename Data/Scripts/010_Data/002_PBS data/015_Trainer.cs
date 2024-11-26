//===============================================================================
//
//===============================================================================
public static partial class GameData {
	public partial class Trainer {
		public int id		{ get { return _id; } }			protected int _id;
		public int trainer_type		{ get { return _trainer_type; } }			protected int _trainer_type;
		public int real_name		{ get { return _real_name; } }			protected int _real_name;
		public int version		{ get { return _version; } }			protected int _version;
		public int items		{ get { return _items; } }			protected int _items;
		public int real_lose_text		{ get { return _real_lose_text; } }			protected int _real_lose_text;
		public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;
		public int s_file_suffix		{ get { return _pbs_file_suffix; } }			protected int _pbs_file_suffix;

		DATA = new List<string>();
		public const string DATA_FILENAME = "trainers.dat";
		public const string PBS_BASE_FILENAME = "trainers";
		// "Pokemon" is specially mentioned in def compile_trainers and def
		// write_trainers, and acts as a subheading for a particular Pokémon.
		SCHEMA = {
			"SectionName" => new {:id,             "esU", :TrainerType},
			"Items"       => new {:items,          "*e", :Item},
			"LoseText"    => new {:real_lose_text, "q"},
			"Pokemon"     => new {:pokemon,        "ev", :Species}   // Species, level
		}
		// This schema is for definable properties of individual Pokémon (apart from
		// species and level which are above).
		SUB_SCHEMA = {
			"Form"         => new {:form,            "u"},
			"Name"         => new {:real_name,       "s"},
			"Moves"        => new {:moves,           "*e", :Move},
			"Ability"      => new {:ability,         "e", :Ability},
			"AbilityIndex" => new {:ability_index,   "u"},
			"Item"         => new {:item,            "e", :Item},
			"Gender"       => new {:gender,          "e", {"M" => 0, "m" => 0, "Male" => 0, "male" => 0, "0" => 0,
																								"F" => 1, "f" => 1, "Female" => 1, "female" => 1, "1" => 1}},
			"Nature"       => new {:nature,          "e", :Nature},
			"IV"           => new {:iv,              "uUUUUU"},
			"EV"           => new {:ev,              "uUUUUU"},
			"Happiness"    => new {:happiness,       "u"},
			"Shiny"        => new {:shininess,       "b"},
			"SuperShiny"   => new {:super_shininess, "b"},
			"Shadow"       => new {:shadowness,      "b"},
			"Ball"         => new {:poke_ball,       "e", :Item}
		}

		extend ClassMethodsSymbols;
		include InstanceMethods;

		public static void sub_schema() {
			return SUB_SCHEMA;
		}

		/// <param name="tr_type"> | Symbol, String</param>
		/// <param name="tr_name"></param>
		/// <param name="tr_version"> | Integer, null</param>
		// @return [Boolean] whether the given other is defined as a self
		public static bool exists(Symbol tr_type, String tr_name, Integer tr_version = 0) {
			validate tr_type => [Symbol, String];
			validate tr_name => [String];
			key = [tr_type.to_sym, tr_name, tr_version];
			return !self.DATA[key].null();
		}

		/// <param name="tr_type"> | Symbol, String</param>
		/// <param name="tr_name"></param>
		/// <param name="tr_version"> | Integer, null</param>
		// @return [self]
		public static void get(Symbol tr_type, String tr_name, Integer tr_version = 0) {
			validate tr_type => [Symbol, String];
			validate tr_name => [String];
			key = [tr_type.to_sym, tr_name, tr_version];
			Debug.LogError($"Unknown trainer {tr_type} {tr_name} {tr_version}." unless self.DATA.has_key(key));
			//throw new Exception($"Unknown trainer {tr_type} {tr_name} {tr_version}." unless self.DATA.has_key(key));
			return self.DATA[key];
		}

		/// <param name="tr_type"> | Symbol, String</param>
		/// <param name="tr_name"></param>
		/// <param name="tr_version"> | Integer, null</param>
		// @return [self, null]
		public static void try_get(Symbol tr_type, String tr_name, Integer tr_version = 0) {
			validate tr_type => [Symbol, String];
			validate tr_name => [String];
			key = [tr_type.to_sym, tr_name, tr_version];
			return (self.DATA.has_key(key)) ? self.DATA[key] : null;
		}

		//---------------------------------------------------------------------------

		public void initialize(hash) {
			@id              = hash.id;
			@trainer_type    = hash.trainer_type;
			@real_name       = hash.real_name       || "";
			@version         = hash.version         || 0;
			@items           = hash.items           || [];
			@real_lose_text  = hash.real_lose_text  || "...";
			@pokemon         = hash.pokemon         || [];
			@pokemon.each do |pkmn|
				foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
					if (pkmn.iv) pkmn.iv[s.id] ||= 0;
					if (pkmn.ev) pkmn.ev[s.id] ||= 0;
				}
			}
			@s_file_suffix = hash.s_file_suffix || "";
		}

		// @return [String] the translated name of this trainer
		public void name() {
			return GetMessageFromHash(MessageTypes.TRAINER_NAMES, @real_name);
		}

		// @return [String] the translated in-battle lose message of this trainer
		public void lose_text() {
			return GetMessageFromHash(MessageTypes.TRAINER_SPEECHES_LOSE, @real_lose_text);
		}

		// Creates a battle-ready version of a trainer's data.
		// @return [Array] all information about a trainer in a usable form
		public void to_trainer() {
			// Determine trainer's name
			tr_name = self.name;
			foreach (var rival in Settings.RIVAL_NAMES) { //'Settings.RIVAL_NAMES.each' do => |rival|
				if (rival[0] != @trainer_type || !Game.GameData.game_variables[rival[1]].is_a(String)) continue;
				tr_name = Game.GameData.game_variables[rival[1]];
				break;
			}
			// Create trainer object
			trainer = new NPCTrainer(tr_name, @trainer_type, @version);
			trainer.id        = Game.GameData.player.make_foreign_ID;
			trainer.items     = @items.clone;
			trainer.lose_text = self.lose_text;
			// Create each Pokémon owned by the trainer
			@pokemon.each do |pkmn_data|
				species = GameData.Species.get(pkmn_data.species).species;
				pkmn = new Pokemon(species, pkmn_data.level, trainer, false);
				trainer.party.Add(pkmn);
				// Set Pokémon's properties if defined
				if (pkmn_data.form) pkmn.form_simple = pkmn_data.form;
				pkmn.item = pkmn_data.item;
				if (pkmn_data.moves && pkmn_data.moves.length > 0) {
					pkmn_data.moves.each(move => pkmn.learn_move(move));
				} else {
					pkmn.reset_moves;
				}
				pkmn.ability_index = pkmn_data.ability_index || 0;
				pkmn.ability = pkmn_data.ability;
				pkmn.gender = pkmn_data.gender || ((trainer.male()) ? 0 : 1);
				pkmn.shiny = (pkmn_data.shininess) ? true : false;
				pkmn.super_shiny = (pkmn_data.super_shininess) ? true : false;
				if (pkmn_data.nature) {
					pkmn.nature = pkmn_data.nature;
				} else {   // Make the nature random but consistent for the same species used by the same trainer type
					species_num = GameData.Species.keys.index(species) || 1;
					tr_type_num = GameData.TrainerType.keys.index(@trainer_type) || 1;
					idx = (species_num + tr_type_num) % GameData.Nature.count;
					pkmn.nature = GameData.Nature.get(GameData.Nature.keys[idx]).id;
				}
				foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
					if (pkmn_data.iv) {
						pkmn.iv[s.id] = pkmn_data.iv[s.id];
					} else {
						pkmn.iv[s.id] = (int)Math.Min(pkmn_data.level / 2, Pokemon.IV_STAT_LIMIT);
					}
					if (pkmn_data.ev) {
						pkmn.ev[s.id] = pkmn_data.ev[s.id];
					} else {
						pkmn.ev[s.id] = (int)Math.Min(pkmn_data.level * 3 / 2, Pokemon.EV_LIMIT / 6);
					}
				}
				if (pkmn_data.happiness) pkmn.happiness = pkmn_data.happiness;
				if (!nil_or_empty(pkmn_data.real_name)) {
					pkmn.name = GetMessageFromHash(MessageTypes.POKEMON_NICKNAMES, pkmn_data.real_name);
				}
				if (pkmn_data.shadowness) {
					pkmn.makeShadow;
					pkmn.shiny = false;
				}
				if (pkmn_data.poke_ball) {
					pkmn.poke_ball = pkmn_data.poke_ball;
				} else if (trainer.default_poke_ball) {
					pkmn.poke_ball = trainer.default_poke_ball;
				}
				pkmn.calc_stats;
			}
			return trainer;
		}

		unless (method_defined(:__orig__get_property_for_PBS)) alias __orig__get_property_for_PBS get_property_for_PBS;
		public void get_property_for_PBS(key, index = 0) {
			ret = __orig__get_property_for_PBS(key);
			switch (key) {
				case "SectionName":
					if (@version == 0) ret = new {@trainer_type, @real_name};
					break;
				case "Pokemon":
					ret = new {@pokemon[index][:species], @pokemon[index][:level]};
					break;
			}
			return ret;
		}

		public void get_pokemon_property_for_PBS(key, index = 0) {
			if (key == "Pokemon") return new {@pokemon[index][:species], @pokemon[index][:level]};
			ret = @pokemon[index][SUB_SCHEMA[key][0]];
			if (ret == false || (ret.Length > 0 && ret.length == 0) || ret == "") ret = null;
			switch (key) {
				case "Gender":
					if (ret) ret = new {"male", "female"}[ret];
					break;
				case "IV": case "EV":
					if (ret) {
						new_ret = new List<string>();
						foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
							if (s.s_order >= 0) new_ret[s.s_order] = ret[s.id];
						}
						ret = new_ret;
					}
					break;
				case "Shiny":
					if (@pokemon[index][:super_shininess]) ret = null;
					break;
			}
			return ret;
		}
	}
}
