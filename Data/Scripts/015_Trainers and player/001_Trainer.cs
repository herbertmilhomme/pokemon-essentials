//===============================================================================
// Basic trainer class (use a child class rather than this one)
//===============================================================================
public partial class Trainer {
	public int trainer_type		{ get { return _trainer_type; } set { _trainer_type = value; } }			protected int _trainer_type;
	public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
	public int id		{ get { return _id; } set { _id = value; } }			protected int _id;
	public int language		{ get { return _language; } set { _language = value; } }			protected int _language;
	public int party		{ get { return _party; } set { _party = value; } }			protected int _party;

	public override void inspect() {
		str = base.inspect().chop;
		party_str = @party.map(p => p.species_data.species).inspect;
		str << string.Format(" {0} @party={0}>", self.full_name, party_str);
		return str;
	}

	public void full_name() {
		if (has_flag("NoName")) return @name;
		return $"{trainer_type_name} {@name}";
	}

	//-----------------------------------------------------------------------------

	// Portion of the ID which is visible on the Trainer Card
	public void public_ID(id = null) {
		return id ? id & 0xFFFF : @id & 0xFFFF;
	}

	// Other portion of the ID
	public void secret_ID(id = null) {
		return id ? id >> 16 : @id >> 16;
	}

	// Random ID other than this Trainer's ID
	public void make_foreign_ID() {
		do { //loop; while (true);
			ret = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
			if (ret != @id) return ret;
		}
		return 0;
	}

	//-----------------------------------------------------------------------------

	public int trainer_type_name { get { return GameData.TrainerType.get(self.trainer_type).name;            } }
	public int base_money        { get { return GameData.TrainerType.get(self.trainer_type).base_money;      } }
	public int gender            { get { return GameData.TrainerType.get(self.trainer_type).gender;          } }
	public bool male() {             return GameData.TrainerType.get(self.trainer_type).male();           }
	public bool female() {           return GameData.TrainerType.get(self.trainer_type).female();         }
	public int skill_level       { get { return GameData.TrainerType.get(self.trainer_type).skill_level;     } }
	public int default_poke_ball { get { return GameData.TrainerType.get(self.trainer_type).poke_ball;       } }
	public int flags             { get { return GameData.TrainerType.get(self.trainer_type).flags;           } }
	public bool has_flag(flag) {    return GameData.TrainerType.get(self.trainer_type).has_flag(flag); }

	//-----------------------------------------------------------------------------

	public void pokemon_party() {
		return @party.find_all(p => p && !p.egg());
	}

	public void able_party() {
		return @party.find_all(p => p && !p.egg() && !p.fainted());
	}

	public void party_count() {
		return @party.length;
	}

	public void pokemon_count() {
		ret = 0;
		@party.each(p => { if (p && !p.egg()) ret += 1; });
		return ret;
	}

	public void able_pokemon_count() {
		ret = 0;
		@party.each(p => { if (p && !p.egg() && !p.fainted()) ret += 1; });
		return ret;
	}

	public bool party_full() {
		return party_count >= Settings.MAX_PARTY_SIZE;
	}

	// Returns true if there are no usable Pokémon in the player's party.
	public bool all_fainted() {
		return able_pokemon_count == 0;
	}

	public void first_party() {
		return @party[0];
	}

	public void first_pokemon() {
		return pokemon_party[0];
	}

	public void first_able_pokemon() {
		return able_party[0];
	}

	public void last_party() {
		return (@party.length > 0) ? @party[@party.length - 1] : null;
	}

	public void last_pokemon() {
		p = pokemon_party;
		return (p.length > 0) ? p[p.length - 1] : null;
	}

	public void last_able_pokemon() {
		p = able_party;
		return (p.length > 0) ? p[p.length - 1] : null;
	}

	public void remove_pokemon_at_index(index) {
		if (index < 0 || index >= party_count) return false;
		have_able = false;
		@party.each_with_index do |pkmn, i|
			if (i != index && pkmn.able()) have_able = true;
			if (have_able) break;
		}
		if (!have_able) return false;
		@party.delete_at(index);
		return true;
	}

	// Checks whether the trainer would still have an unfainted Pokémon if the
	// Pokémon given by _index_ were removed from the party.
	public bool has_other_able_pokemon(index) {
		@party.each_with_index((pkmn, i) => { if (i != index && pkmn.able()) return true; });
		return false;
	}

	// Returns true if there is a Pokémon of the given species in the trainer's
	// party. You may also specify a particular form it should be.
	public bool has_species(species, form = -1) {
		return pokemon_party.any(p => p&.isSpecies(species) && (form < 0 || p.form == form));
	}

	// Returns whether there is a fatefully met Pokémon of the given species in the
	// trainer's party.
	public bool has_fateful_species(species) {
		return pokemon_party.any(p => p&.isSpecies(species) && p.obtain_method == 4);
	}

	// Returns whether there is a Pokémon with the given type in the trainer's
	// party.
	public bool has_pokemon_of_type(type) {
		if (!GameData.Type.exists(type)) return false;
		type = GameData.Type.get(type).id;
		return pokemon_party.any(p => p&.hasType(type));
	}

	// Checks whether any Pokémon in the party knows the given move, and returns
	// the first Pokémon it finds with that move, or null if no Pokémon has that move.
	public void get_pokemon_with_move(move) {
		pokemon_party.each(pkmn => { if (pkmn.hasMove(move)) return pkmn; });
		return null;
	}

	// Fully heal all Pokémon in the party.
	public void heal_party() {
		@party.each(pkmn => pkmn.heal);
	}

	//-----------------------------------------------------------------------------

	public void initialize(name, trainer_type) {
		@trainer_type = GameData.TrainerType.get(trainer_type).id;
		@name         = name;
		@id           = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
		@language     = GetLanguage;
		@party        = new List<string>();
	}
}

//===============================================================================
// Trainer class for NPC trainers
//===============================================================================
public partial class NPCTrainer : Trainer {
	public int version		{ get { return _version; } set { _version = value; } }			protected int _version;
	public int items		{ get { return _items; } set { _items = value; } }			protected int _items;
	public int lose_text		{ get { return _lose_text; } set { _lose_text = value; } }			protected int _lose_text;
	public int win_text		{ get { return _win_text; } set { _win_text = value; } }			protected int _win_text;

	public override void initialize(name, trainer_type, version = 0) {
		base.initialize(name, trainer_type);
		@version   = version;
		@items     = new List<string>();
		@lose_text = null;
		@win_text  = null;
	}
}
