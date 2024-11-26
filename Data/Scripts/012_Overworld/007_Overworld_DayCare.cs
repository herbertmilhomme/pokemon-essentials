//===============================================================================
// NOTE: In Gen 7+, the Day Care is replaced by the Pokémon Nursery, which works
//       in much the same way except deposited Pokémon no longer gain Exp because
//       of the player walking around and, in Gen 8+, deposited Pokémon are able
//       to learn egg moves from each other if they are the same species. In
//       Essentials, this code can be used for both facilities, and these
//       mechanics differences are set by some Settings.
// NOTE: The Day Care has a different price than the Pokémon Nursery. For the Day
//       Care, you are charged when you withdraw a deposited Pokémon and you pay
//       an amount based on how many levels it gained. For the Nursery, you pay
//       Game.GameData.500 up-front when you deposit a Pokémon. This difference will appear in
//       the Day Care Lady's event, not in these scripts.
//===============================================================================
public partial class DayCare {
	//=============================================================================
	// Code that generates an egg based on two given Pokémon.
	//=============================================================================
	public static partial class EggGenerator {
		#region Class Functions
		#endregion

		public void generate(mother, father) {
			// Determine which Pokémon is the mother and which is the father
			// Ensure mother is female, if the pair contains a female
			// Ensure father is male, if the pair contains a male
			// Ensure father is genderless, if the pair is a genderless with Ditto
			if (mother.male() || father.female() || mother.genderless()) {
				mother, father = father, mother;
			}
			mother_data = new {mother, mother.species_data.egg_groups.Contains(:Ditto)};
			father_data = new {father, father.species_data.egg_groups.Contains(:Ditto)};
			// Determine which parent the egg's species is based from
			species_parent = (mother_data[1]) ? father : mother;
			// Determine the egg's species
			baby_species = determine_egg_species(species_parent.species, mother, father);
			mother_data.Add(mother.species_data.breeding_can_produce(baby_species));
			father_data.Add(father.species_data.breeding_can_produce(baby_species));
			// Generate egg
			egg = generate_basic_egg(baby_species);
			// Inherit properties from parent(s)
			inherit_form(egg, species_parent, mother_data, father_data);
			inherit_nature(egg, mother, father);
			inherit_ability(egg, mother_data, father_data);
			inherit_moves(egg, mother_data, father_data);
			inherit_IVs(egg, mother, father);
			inherit_poke_ball(egg, mother_data, father_data);
			// Calculate other properties of the egg
			set_shininess(egg, mother, father);   // Masuda method and Shiny Charm
			set_pokerus(egg);
			// Recalculate egg's stats
			egg.calc_stats;
			return egg;
		}

		public void determine_egg_species(parent_species, mother, father) {
			ret = GameData.Species.get(parent_species).get_baby_species(true, mother.item_id, father.item_id);
			// Check for alternate offspring (i.e. Nidoran M/F, Volbeat/Illumise, Manaphy/Phione)
			offspring = GameData.Species.get(ret).offspring;
			if (offspring.length > 0) ret = offspring.sample;
			return ret;
		}

		public void generate_basic_egg(species) {
			egg = new Pokemon(species, Settings.EGG_LEVEL);
			egg.name           = _INTL("Egg");
			egg.steps_to_hatch = egg.species_data.hatch_steps;
			egg.obtain_text    = _INTL("Day-Care Couple");
			egg.happiness      = 120;
			if (species == speciess.SINISTEA) egg.form           = 0;
			// Set regional form
			new_form = MultipleForms.call("getFormOnEggCreation", egg);
			if (new_form) egg.form = new_form;
			return egg;
		}

		public void inherit_form(egg, species_parent, mother, father) {
			// mother = new {mother, mother_ditto, mother_in_family}
			// father = new {father, father_ditto, father_in_family}
			// Inherit form from the parent that determined the egg's species
			if (species_parent.species_data.has_flag("InheritFormFromMother")) {
				egg.form = species_parent.form;
			}
			// Inherit form from a parent holding an Ever Stone
			new {mother, father}.each do |parent|
				if (!parent[2]) continue;   // Parent isn't a related species to the egg
				if (!parent[0].species_data.has_flag("InheritFormWithEverStone")) continue;
				if (!parent[0].hasItem(Items.EVERSTONE)) continue;
				egg.form = parent[0].form;
				break;
			}
		}

		public void get_moves_to_inherit(egg, mother, father) {
			// mother = new {mother, mother_ditto, mother_in_family}
			// father = new {father, father_ditto, father_in_family}
			move_father = (father[1]) ? mother[0] : father[0];
			move_mother = (father[1]) ? father[0] : mother[0];
			moves = new List<string>();
			// Get level-up moves known by both parents
			foreach (var move in egg.getMoveList) { //'egg.getMoveList.each' do => |move|
				if (move[0] <= egg.level) continue;   // Could already know this move by default
				if (!mother[0].hasMove(move[1]) || !father[0].hasMove(move[1])) continue;
				moves.Add(move[1]);
			}
			// Inherit Machine moves from father (or non-Ditto genderless parent)
			if (Settings.BREEDING_CAN_INHERIT_MACHINE_MOVES && !move_father.female()) {
				foreach (var i in GameData.Item) { //'GameData.Item.each' do => |i|
					move = i.move;
					if (!move) continue;
					if (!move_father.hasMove(move) || !egg.compatible_with_move(move)) continue;
					moves.Add(move);
				}
			}
			// Inherit egg moves from each parent
			if (!move_father.female()) {
				foreach (var move in egg.species_data.egg_moves) { //'egg.species_data.egg_moves.each' do => |move|
					if (move_father.hasMove(move)) moves.Add(move);
				}
			}
			if (Settings.BREEDING_CAN_INHERIT_EGG_MOVES_FROM_MOTHER && move_mother.female()) {
				foreach (var move in egg.species_data.egg_moves) { //'egg.species_data.egg_moves.each' do => |move|
					if (move_mother.hasMove(move)) moves.Add(move);
				}
			}
			// Learn Volt Tackle if a parent has a Light Ball and is in the Pichu family
			if (egg.species == speciess.PICHU && GameData.Moves.exists(Moves.VOLTTACKLE) &&
				((father[2] && father[0].hasItem(Items.LIGHTBALL)) ||
					(mother[2] && mother[0].hasItem(Items.LIGHTBALL)))) {
				moves.Add(:VOLTTACKLE);
			}
			return moves;
		}

		public void inherit_moves(egg, mother, father) {
			moves = get_moves_to_inherit(egg, mother, father);
			// Remove duplicates (keeping the latest ones)
			moves = moves.reverse;
			moves |= new List<string>();   // remove duplicates
			moves = moves.reverse;
			// Learn moves
			first_move_index = moves.length - Pokemon.MAX_MOVES;
			if (first_move_index < 0) first_move_index = 0;
			(first_move_index...moves.length).each(i => egg.learn_move(moves[i]))
		}

		public void inherit_nature(egg, mother, father) {
			new_natures = new List<string>();
			if (mother.hasItem(Items.EVERSTONE)) new_natures.Add(mother.nature);
			if (father.hasItem(Items.EVERSTONE)) new_natures.Add(father.nature);
			if (new_natures.empty()) return;
			egg.nature = new_natures.sample;
		}

		// The female parent (or the non-Ditto parent) can pass down its Hidden
		// Ability (60% chance) or its regular ability (80% chance).
		// NOTE: This is how ability inheritance works in Gen 6+. Gen 5 is more
		//       restrictive, and even works differently between BW and B2W2, and I
		//       don't think that is worth adding in. Gen 4 and lower don't have
		//       ability inheritance at all, and again, I'm not bothering to add that
		//       in.
		public void inherit_ability(egg, mother, father) {
			// mother = new {mother, mother_ditto, mother_in_family}
			// father = new {father, father_ditto, father_in_family}
			parent = (mother[1]) ? father[0] : mother[0];   // The female or non-Ditto parent
			if (parent.hasHiddenAbility()) {
				if (rand(100) < 60) egg.ability_index = parent.ability_index;
			} else if (rand(100) < 80) {
				egg.ability_index = parent.ability_index;
			} else {
				egg.ability_index = (parent.ability_index + 1) % 2;
			}
		}

		public void inherit_IVs(egg, mother, father) {
			// Get all stats
			stats = new List<string>();
			GameData.Stat.each_main(s => stats.Add(s.id));
			// Get the number of stats to inherit (includes ones inherited via Power items)
			inherit_count = 3;
			if (Settings.MECHANICS_GENERATION >= 6) {
				if (mother.hasItem(Items.DESTINYKNOT) || father.hasItem(Items.DESTINYKNOT)) inherit_count = 5;
			}
			// Inherit IV because of Power items (if both parents have the same Power
			// item, then the parent that passes that Power item's stat down is chosen
			// randomly)
			power_items = new {
				new {:POWERWEIGHT, :HP},
				new {:POWERBRACER, :ATTACK},
				new {:POWERBELT,   :DEFENSE},
				new {:POWERLENS,   :SPECIAL_ATTACK},
				new {:POWERBAND,   :SPECIAL_DEFENSE},
				new {:POWERANKLET, :SPEED}
			}
			power_stats = new List<string>();
			new {mother, father}.each do |parent|
				foreach (var item in power_items) { //'power_items.each' do => |item|
					if (!parent.hasItem(item[0])) continue;
					power_stats[item[1]] ||= new List<string>();
					power_stats[item[1]].Add(parent.iv[item[1]]);
					break;
				}
			}
			power_stats.each_pair do |stat, new_stats|
				if (!new_stats || new_stats.length == 0) continue;
				new_stat = new_stats.sample;
				egg.iv[stat] = new_stat;
				stats.delete(stat);   // Don't try to inherit this stat's IV again
				inherit_count -= 1;
			}
			// Inherit the rest of the IVs
			chosen_stats = stats.sample(inherit_count);
			chosen_stats.each { |stat| egg.iv[stat] = new {mother, father}.sample.iv[stat] };
		}

		// Poké Balls can only be inherited from parents that are related to the
		// egg's species.
		// NOTE: This is how Poké Ball inheritance works in Gen 7+. Gens 5 and lower
		//       don't have Poké Ball inheritance at all. In Gen 6, only a female
		//       parent can pass down its Poké Ball. I don't think it's worth adding
		//       in these variations on the mechanic.
		// NOTE: The official games treat Nidoran M/F and Volbeat/Illumise as
		//       unrelated for the purpose of this mechanic. Essentials treats them
		//       as related and allows them to pass down their Poké Balls.
		public void inherit_poke_ball(egg, mother, father) {
			// mother = new {mother, mother_ditto, mother_in_family}
			// father = new {father, father_ditto, father_in_family}
			balls = new List<string>();
			new {mother, father}.each do |parent|
				if (parent[2]) balls.Add(parent[0].poke_ball);
			}
			balls.delete(:MASTERBALL);    // Can't inherit this Ball
			balls.delete(:CHERISHBALL);   // Can't inherit this Ball
			if (!balls.empty()) egg.poke_ball = balls.sample;
		}

		// NOTE: There is a bug in Gen 8 that skips the original generation of an
		//       egg's personal ID if the Masuda Method/Shiny Charm can cause any
		//       rerolls. Essentials doesn't have this bug, meaning eggs are slightly
		//       more likely to be shiny (in Gen 8+ mechanics) than in Gen 8 itself.
		public void set_shininess(egg, mother, father) {
			shiny_retries = 0;
			if (father.owner.language != mother.owner.language) {
				shiny_retries += (Settings.MECHANICS_GENERATION >= 8) ? 6 : 5;
			}
			if (Game.GameData.bag.has(:SHINYCHARM)) shiny_retries += 2;
			if (shiny_retries == 0) return;
			shiny_retries.times do;
				if (egg.shiny()) break;
				egg.shiny = null;   // Make it recalculate shininess
				egg.personalID = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
			}
		}

		public void set_pokerus(egg) {
			if (rand(65_536) < Settings.POKERUS_CHANCE) egg.givePokerus;
		}
	}

	//=============================================================================
	// A slot in the Day Care, which can contain a Pokémon.
	//=============================================================================
	public partial class DayCareSlot {
		public int pokemon		{ get { return _pokemon; } }			protected int _pokemon;

		public void initialize() {
			reset;
		}

		public void reset() {
			@pokemon = null;
			@initial_level = 0;
		}

		public void deposit(pkmn) {
			@pokemon = pkmn;
			@pokemon.heal;
			if (@pokemon.isSpecies(Speciess.SHAYMIN)) @pokemon.form = 0;
			@initial_level = pkmn.level;
		}

		public bool filled() {
			return !@pokemon.null();
		}

		public void pokemon_name() {
			return (filled()) ? @pokemon.name : "";
		}

		public void level_gain() {
			return (filled()) ? @pokemon.level - @initial_level : 0;
		}

		public void cost() {
			return (level_gain + 1) * 100;
		}

		public void choice_text() {
			if (!filled()) return null;
			if (@pokemon.male()) {
				return _INTL("{1} (♂, Lv.{2})", @pokemon.name, @pokemon.level);
			} else if (@pokemon.female()) {
				return _INTL("{1} (♀, Lv.{2})", @pokemon.name, @pokemon.level);
			}
			return _INTL("{1} (Lv.{2})", @pokemon.name, @pokemon.level);
		}

		public void add_exp(amount = 1) {
			if (!filled()) return;
			max_exp = @pokemon.growth_rate.maximum_exp;
			if (@pokemon.exp >= max_exp) return;
			old_level = @pokemon.level;
			@pokemon.exp += amount;
			if (@pokemon.level == old_level) return;
			@pokemon.calc_stats;
			move_list = @pokemon.getMoveList;
			move_list.each(move => { if (move[0] == @pokemon.level) @pokemon.learn_move(move[1]); });
		}
	}

	//-----------------------------------------------------------------------------

	public int slots		{ get { return _slots; } }			protected int _slots;
	public int egg_generated		{ get { return _egg_generated; } set { _egg_generated = value; } }			protected int _egg_generated;
	public int step_counter		{ get { return _step_counter; } set { _step_counter = value; } }			protected int _step_counter;
	public int gain_exp		{ get { return _gain_exp; } set { _gain_exp = value; } }			protected int _gain_exp;
	/// <summary>For deposited Pokémon of the same species</summary>
	public int share_egg_moves		{ get { return _share_egg_moves; } set { _share_egg_moves = value; } }			protected int _share_egg_moves;

	public const int MAX_SLOTS = 2;

	public void initialize() {
		@slots = new List<string>();
		MAX_SLOTS.times(() => @slots.Add(new DayCareSlot()));
		reset_egg_counters;
		@gain_exp = Settings.DAY_CARE_POKEMON_GAIN_EXP_FROM_WALKING;
		@share_egg_moves = Settings.DAY_CARE_POKEMON_CAN_SHARE_EGG_MOVES;
	}

	public int this[int index] { get {
		return @slots[index];
		}
	}

	public void reset_egg_counters() {
		@egg_generated = false;
		@step_counter = 0;
	}

	public void count() {
		return @slots.select(slot => slot.filled()).length;
	}

	public void get_compatibility() {
		return compatibility;
	}

	public void generate_egg() {
		if (self.count != 2) return null;
		pkmn1, pkmn2 = pokemon_pair;
		return EggGenerator.generate(pkmn1, pkmn2);
	}

	public void share_egg_move() {
		if (count != 2) return;
		pkmn1, pkmn2 = pokemon_pair;
		if (pkmn1.species != pkmn2.species) return;
		egg_moves1 = pkmn1.species_data.get_egg_moves;
		egg_moves2 = pkmn2.species_data.get_egg_moves;
		known_moves1 = new List<string>();
		known_moves2 = new List<string>();
		if (pkmn2.numMoves < Pokemon.MAX_MOVES) {
			pkmn1.moves.each(m => { if (egg_moves2.Contains(m.id) && !pkmn2.hasMove(m.id)) known_moves1.Add(m.id); });
		}
		if (pkmn1.numMoves < Pokemon.MAX_MOVES) {
			pkmn2.moves.each(m => { if (egg_moves1.Contains(m.id) && !pkmn1.hasMove(m.id)) known_moves2.Add(m.id); });
		}
		if (!known_moves1.empty()) {
			if (known_moves2.empty()) {
				pkmn2.learn_move(known_moves1[0]);
			} else {
				learner = new {new {pkmn1, known_moves2[0]}, new {pkmn2, known_moves1[0]}}.sample;
				learner[0].learn_move(learner[1]);
			}
		} else if (!known_moves2.empty()) {
			pkmn1.learn_move(known_moves2[0]);
		}
	}

	public void update_on_step_taken() {
		@step_counter += 1;
		if (@step_counter >= 256) {
			@step_counter = 0;
			// Make an egg available at the Day Care
			if (!@egg_generated && count == 2) {
				compat = compatibility;
				egg_chance = new {0, 20, 50, 70}[compat];
				if (Game.GameData.bag.has(:OVALCHARM)) egg_chance = new {0, 40, 80, 88}[compat];
				if (rand(100) < egg_chance) @egg_generated = true;
			}
			// Have one deposited Pokémon learn an egg move from the other
			// NOTE: I don't know what the chance of this happening is.
			if (@share_egg_moves && rand(100) < 50) share_egg_move;
		}
		// Day Care Pokémon gain Exp/moves
		if (@gain_exp) {
			@slots.each(slot => slot.add_exp);
		}
	}

	//-----------------------------------------------------------------------------

	public static void count() {
		return Game.GameData.PokemonGlobal.day_care.count;
	}

	public static bool egg_generated() {
		return Game.GameData.PokemonGlobal.day_care.egg_generated;
	}

	public static void reset_egg_counters() {
		Game.GameData.PokemonGlobal.day_care.reset_egg_counters;
	}

	public static void get_details(index, name_var, cost_var) {
		day_care = Game.GameData.PokemonGlobal.day_care;
		if (name_var > 0) Game.GameData.game_variables[name_var] = day_care[index].pokemon_name;
		if (cost_var > 0) Game.GameData.game_variables[cost_var] = day_care[index].cost;
	}

	public static void get_level_gain(index, name_var, level_var) {
		day_care = Game.GameData.PokemonGlobal.day_care;
		if (name_var > 0) Game.GameData.game_variables[name_var] = day_care[index].pokemon_name;
		if (level_var > 0) Game.GameData.game_variables[level_var] = day_care[index].level_gain;
	}

	public static void get_compatibility(compat_var) {
		if (compat_var > 0) Game.GameData.game_variables[compat_var] = Game.GameData.PokemonGlobal.day_care.get_compatibility;
	}

	public static void deposit(party_index) {
		Game.GameData.stats.day_care_deposits += 1;
		day_care = Game.GameData.PokemonGlobal.day_care;
		pkmn = Game.GameData.player.party[party_index];
		if (pkmn.null()) raise _INTL("No Pokémon at index {1} in party.", party_index);
		foreach (var slot in day_care.slots) { //'day_care.slots.each' do => |slot|
			if (slot.filled()) continue;
			slot.deposit(pkmn);
			Game.GameData.player.party.delete_at(party_index);
			day_care.reset_egg_counters;
			return;
		}
		Debug.LogError(_INTL("No room to deposit a Pokémon."));
		//throw new ArgumentException(_INTL("No room to deposit a Pokémon."));
	}

	public static void withdraw(index) {
		day_care = Game.GameData.PokemonGlobal.day_care;
		slot = day_care[index];
		if (!slot.filled()) {
			Debug.LogError(_INTL("No Pokémon found in slot {1}.", index));
			//throw new ArgumentException(_INTL("No Pokémon found in slot {1}.", index));
		} else if (Game.GameData.player.party_full()) {
			Debug.LogError(_INTL("No room in party for Pokémon."));
			//throw new ArgumentException(_INTL("No room in party for Pokémon."));
		}
		Game.GameData.stats.day_care_levels_gained += slot.level_gain;
		Game.GameData.player.party.Add(slot.pokemon);
		slot.reset;
		day_care.reset_egg_counters;
	}

	public static void choose(message, choice_var) {
		day_care = Game.GameData.PokemonGlobal.day_care;
		switch (day_care.count) {
			case 0:
				Debug.LogError(_INTL("No Pokémon found in Day Care to choose from."));
				//throw new ArgumentException(_INTL("No Pokémon found in Day Care to choose from."));
				break;
			case 1:
				day_care.slots.each_with_index((slot, i) => { if (slot.filled()) Game.GameData.game_variables[choice_var] = i; });
				break;
			default:
				commands = new List<string>();
				indices = new List<string>();
				day_care.slots.each_with_index do |slot, i|
					choice_text = slot.choice_text;
					if (!choice_text) continue;
					commands.Add(choice_text);
					indices.Add(i);
				}
				commands.Add(_INTL("CANCEL"));
				command = Message(message, commands, commands.length);
				Game.GameData.game_variables[choice_var] = (command == commands.length - 1) ? -1 : indices[command];
				break;
		}
	}

	public static void collect_egg() {
		day_care = Game.GameData.PokemonGlobal.day_care;
		egg = day_care.generate_egg;
		if (egg.null()) raise _INTL("Couldn't generate the egg.");
		if (Game.GameData.player.party_full()) raise _INTL("No room in party for egg.");
		Game.GameData.player.party.Add(egg);
		day_care.reset_egg_counters;
	}

	//-----------------------------------------------------------------------------

	private;

	public void pokemon_pair() {
		pkmn1 = null;
		pkmn2 = null;
		@slots.each do |slot|
			if (!slot.filled()) continue;
			if (pkmn1.null()) {
				pkmn1 = slot.pokemon;
			} else {
				pkmn2 = slot.pokemon;
			}
		}
		if (pkmn2.null()) raise _INTL("Couldn't find 2 deposited Pokémon.");
		return pkmn1, pkmn2;
	}

	public bool pokemon_in_ditto_egg_group(pkmn) {
		return pkmn.species_data.egg_groups.Contains(:Ditto);
	}

	public bool compatible_gender(pkmn1, pkmn2) {
		if (pkmn1.female() && pkmn2.male()) return true;
		if (pkmn1.male() && pkmn2.female()) return true;
		ditto1 = pokemon_in_ditto_egg_group(pkmn1);
		ditto2 = pokemon_in_ditto_egg_group(pkmn2);
		if (ditto1 && !ditto2) return true;
		if (ditto2 && !ditto1) return true;
		return false;
	}

	public void compatibility() {
		if (self.count != 2) return 0;
		// Find the Pokémon whose compatibility is being calculated
		pkmn1, pkmn2 = pokemon_pair;
		// Shadow Pokémon cannot breed
		if (pkmn1.shadowPokemon() || pkmn2.shadowPokemon()) return 0;
		// Pokémon in the Undiscovered egg group cannot breed
		egg_groups1 = pkmn1.species_data.egg_groups;
		egg_groups2 = pkmn2.species_data.egg_groups;
		if (egg_groups1.Contains(:Undiscovered) ||
								egg_groups2.Contains(:Undiscovered)) return 0;
		// Pokémon that don't share an egg group (and neither is in the Ditto group)
		// cannot breed
		if (!egg_groups1.Contains(:Ditto) &&
								!egg_groups2.Contains(:Ditto) &&
								(egg_groups1 & egg_groups2).length == 0) return 0;
		// Pokémon with incompatible genders cannot breed
		if (!compatible_gender(pkmn1, pkmn2)) return 0;
		// Pokémon can breed; calculate a compatibility factor
		ret = 1;
		if (pkmn1.species == pkmn2.species) ret += 1;
		if (pkmn1.owner.id != pkmn2.owner.id) ret += 1;
		return ret;
	}
}

//===============================================================================
// With each step taken, add Exp to Pokémon in the Day Care and try to generate
// an egg.
//===============================================================================
EventHandlers.add(:on_player_step_taken, :update_day_care,
	block: () => {
		Game.GameData.PokemonGlobal.day_care.update_on_step_taken;
	}
)
