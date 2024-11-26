//===============================================================================
// Nicknaming and storing Pokémon.
//===============================================================================
public bool BoxesFull() {
	return (Game.GameData.player.party_full() && Game.GameData.PokemonStorage.full());
}

public void Nickname(pkmn) {
	if (Game.GameData.PokemonSystem.givenicknames != 0) return;
	species_name = pkmn.speciesName;
	if (ConfirmMessage(_INTL("Would you like to give a nickname to {1}?", species_name))) {
		pkmn.name = EnterPokemonName(_INTL("{1}'s nickname?", species_name),
																	0, Pokemon.MAX_NAME_SIZE, "", pkmn);
	}
}

public void StorePokemon(pkmn) {
	if (BoxesFull()) {
		Message(_INTL("There's no more room for Pokémon!") + "\1");
		Message(_INTL("The Pokémon Boxes are full and can't accept any more!"));
		return;
	}
	pkmn.record_first_moves;
	if (Game.GameData.player.party_full()) {
		stored_box = Game.GameData.PokemonStorage.StoreCaught(pkmn);
		box_name   = Game.GameData.PokemonStorage[stored_box].name;
		Message(_INTL("{1} has been sent to Box \"{2}\"!", pkmn.name, box_name));
	} else {
		Game.GameData.player.party[Game.GameData.player.party.length] = pkmn;
	}
}

public void NicknameAndStore(pkmn) {
	if (BoxesFull()) {
		Message(_INTL("There's no more room for Pokémon!") + "\1");
		Message(_INTL("The Pokémon Boxes are full and can't accept any more!"));
		return;
	}
	Game.GameData.player.pokedex.set_seen(pkmn.species);
	Game.GameData.player.pokedex.set_owned(pkmn.species);
	Nickname(pkmn);
	StorePokemon(pkmn);
}

//===============================================================================
// Giving Pokémon to the player (will send to storage if party is full).
//===============================================================================
public void AddPokemon(pkmn, level = 1, see_form = true) {
	if (!pkmn) return false;
	if (BoxesFull()) {
		Message(_INTL("There's no more room for Pokémon!") + "\1");
		Message(_INTL("The Pokémon Boxes are full and can't accept any more!"));
		return false;
	}
	if (!pkmn.is_a(Pokemon)) pkmn = new Pokemon(pkmn, level);
	species_name = pkmn.speciesName;
	Message(_INTL("{1} obtained {2}!", Game.GameData.player.name, species_name) + "\\me[Pkmn get]\\wtnp[80]");
	was_owned = Game.GameData.player.owned(pkmn.species);
	Game.GameData.player.pokedex.set_seen(pkmn.species);
	Game.GameData.player.pokedex.set_owned(pkmn.species);
	if (see_form) Game.GameData.player.pokedex.register(pkmn);
	// Show Pokédex entry for new species if it hasn't been owned before
	if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && see_form && !was_owned &&
		Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(pkmn.species)) {
		Message(_INTL("{1}'s data was added to the Pokédex.", species_name));
		Game.GameData.player.pokedex.register_last_seen(pkmn);
		FadeOutIn do;
			scene = new PokemonPokedexInfo_Scene();
			screen = new PokemonPokedexInfoScreen(scene);
			screen.DexEntry(pkmn.species);
		}
	}
	// Nickname and add the Pokémon
	NicknameAndStore(pkmn);
	return true;
}

public void AddPokemonSilent(pkmn, level = 1, see_form = true) {
	if (!pkmn || BoxesFull()) return false;
	if (!pkmn.is_a(Pokemon)) pkmn = new Pokemon(pkmn, level);
	Game.GameData.player.pokedex.set_seen(pkmn.species);
	Game.GameData.player.pokedex.set_owned(pkmn.species);
	if (see_form) Game.GameData.player.pokedex.register(pkmn);
	pkmn.record_first_moves;
	if (Game.GameData.player.party_full()) {
		Game.GameData.PokemonStorage.StoreCaught(pkmn);
	} else {
		Game.GameData.player.party[Game.GameData.player.party.length] = pkmn;
	}
	return true;
}

//===============================================================================
// Giving Pokémon/eggs to the player (can only add to party).
//===============================================================================
public void AddToParty(pkmn, level = 1, see_form = true) {
	if (!pkmn || Game.GameData.player.party_full()) return false;
	if (!pkmn.is_a(Pokemon)) pkmn = new Pokemon(pkmn, level);
	species_name = pkmn.speciesName;
	Message(_INTL("{1} obtained {2}!", Game.GameData.player.name, species_name) + "\\me[Pkmn get]\\wtnp[80]");
	was_owned = Game.GameData.player.owned(pkmn.species);
	Game.GameData.player.pokedex.set_seen(pkmn.species);
	Game.GameData.player.pokedex.set_owned(pkmn.species);
	if (see_form) Game.GameData.player.pokedex.register(pkmn);
	// Show Pokédex entry for new species if it hasn't been owned before
	if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && see_form && !was_owned &&
		Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(pkmn.species)) {
		Message(_INTL("{1}'s data was added to the Pokédex.", species_name));
		Game.GameData.player.pokedex.register_last_seen(pkmn);
		FadeOutIn do;
			scene = new PokemonPokedexInfo_Scene();
			screen = new PokemonPokedexInfoScreen(scene);
			screen.DexEntry(pkmn.species);
		}
	}
	// Nickname and add the Pokémon
	NicknameAndStore(pkmn);
	return true;
}

public void AddToPartySilent(pkmn, level = null, see_form = true) {
	if (!pkmn || Game.GameData.player.party_full()) return false;
	if (!pkmn.is_a(Pokemon)) pkmn = new Pokemon(pkmn, level);
	if (see_form) Game.GameData.player.pokedex.register(pkmn);
	Game.GameData.player.pokedex.set_owned(pkmn.species);
	pkmn.record_first_moves;
	Game.GameData.player.party[Game.GameData.player.party.length] = pkmn;
	return true;
}

public void AddForeignPokemon(pkmn, level = 1, owner_name = null, nickname = null, owner_gender = 0, see_form = true) {
	if (!pkmn || Game.GameData.player.party_full()) return false;
	if (!pkmn.is_a(Pokemon)) pkmn = new Pokemon(pkmn, level);
	pkmn.owner = Pokemon.Owner.new_foreign(owner_name || "", owner_gender);
	if (!nil_or_empty(nickname)) pkmn.name = nickname[0, Pokemon.MAX_NAME_SIZE];
	pkmn.calc_stats;
	if (owner_name) {
		Message(_INTL("{1} received a Pokémon from {2}.", Game.GameData.player.name, owner_name) + "\\me[Pkmn get]\\wtnp[80]");
	} else {
		Message(_INTL("{1} received a Pokémon.", Game.GameData.player.name) + "\\me[Pkmn get]\\wtnp[80]");
	}
	was_owned = Game.GameData.player.owned(pkmn.species);
	Game.GameData.player.pokedex.set_seen(pkmn.species);
	Game.GameData.player.pokedex.set_owned(pkmn.species);
	if (see_form) Game.GameData.player.pokedex.register(pkmn);
	// Show Pokédex entry for new species if it hasn't been owned before
	if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && see_form && !was_owned &&
		Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(pkmn.species)) {
		Message(_INTL("The Pokémon's data was added to the Pokédex."));
		Game.GameData.player.pokedex.register_last_seen(pkmn);
		FadeOutIn do;
			scene = new PokemonPokedexInfo_Scene();
			screen = new PokemonPokedexInfoScreen(scene);
			screen.DexEntry(pkmn.species);
		}
	}
	// Add the Pokémon
	StorePokemon(pkmn);
	return true;
}

public void GenerateEgg(pkmn, text = "") {
	if (!pkmn || Game.GameData.player.party_full()) return false;
	if (!pkmn.is_a(Pokemon)) pkmn = new Pokemon(pkmn, Settings.EGG_LEVEL);
	// Set egg's details
	pkmn.name           = _INTL("Egg");
	pkmn.steps_to_hatch = pkmn.species_data.hatch_steps;
	pkmn.obtain_text    = text;
	pkmn.calc_stats;
	// Add egg to party
	Game.GameData.player.party[Game.GameData.player.party.length] = pkmn;
	return true;
}
alias AddEgg GenerateEgg;
alias GenEgg GenerateEgg;

//===============================================================================
// Analyse Pokémon in the party.
//===============================================================================
// Returns the first unfainted, non-egg Pokémon in the player's party.
public void FirstAblePokemon(variable_ID) {
	Game.GameData.player.party.each_with_index do |pkmn, i|
		if (!pkmn.able()) continue;
		Set(variable_ID, i);
		return pkmn;
	}
	Set(variable_ID, -1);
	return null;
}

//===============================================================================
// Return a level value based on Pokémon in a party.
//===============================================================================
public void BalancedLevel(party) {
	if (party.length == 0) return 1;
	// Calculate the mean of all levels
	sum = 0;
	party.each(p => sum += p.level);
	if (sum == 0) return 1;
	mLevel = GameData.GrowthRate.max_level;
	average = sum.to_f / party.length;
	// Calculate the standard deviation
	varianceTimesN = 0;
	foreach (var pkmn in party) { //'party.each' do => |pkmn|
		deviation = pkmn.level - average;
		varianceTimesN += deviation * deviation;
	}
	// NOTE: This is the "population" standard deviation calculation, since no
	// sample is being taken.
	stdev = Math.sqrt(varianceTimesN / party.length);
	mean = 0;
	weights = new List<string>();
	// Skew weights according to standard deviation
	foreach (var pkmn in party) { //'party.each' do => |pkmn|
		weight = pkmn.level.to_f / sum;
		if (weight < 0.5) {
			weight -= (stdev / mLevel.to_f);
			if (weight <= 0.001) weight = 0.001;
		} else {
			weight += (stdev / mLevel.to_f);
			if (weight >= 0.999) weight = 0.999;
		}
		weights.Add(weight);
	}
	weightSum = 0;
	weights.each(w => weightSum += w);
	// Calculate the weighted mean, assigning each weight to each level's
	// contribution to the sum
	party.each_with_index((pkmn, i) => mean += pkmn.level * weights[i]);
	mean /= weightSum;
	mean = mean.round;
	if (mean < 1) mean = 1;
	// Add 2 to the mean to challenge the player
	mean += 2;
	// Adjust level to maximum
	if (mean > mLevel) mean = mLevel;
	return mean;
}

//===============================================================================
// Calculates a Pokémon's size (in millimeters).
//===============================================================================
public void Size(pkmn) {
	baseheight = pkmn.height;
	hpiv = pkmn.iv[:HP] & 15;
	ativ = pkmn.iv[:ATTACK] & 15;
	dfiv = pkmn.iv[:DEFENSE] & 15;
	saiv = pkmn.iv[:SPECIAL_ATTACK] & 15;
	sdiv = pkmn.iv[:SPECIAL_DEFENSE] & 15;
	spiv = pkmn.iv[:SPEED] & 15;
	m = pkmn.personalID & 0xFF;
	n = (pkmn.personalID >> 8) & 0xFF;
	s = ((((ativ ^ dfiv) * hpiv) ^ m) * 256) + (((saiv ^ sdiv) * spiv) ^ n);
	xyz = new {1700, 1, 65_510};
	switch (s) {
		case 0...10:           xyz = new { 290,   1,      0}; break;
		case 10...110:         xyz = new { 300,   1,     10}; break;
		case 110...310:        xyz = new { 400,   2,    110}; break;
		case 310...710:        xyz = new { 500,   4,    310}; break;
		case 710...2710:       xyz = new { 600,  20,    710}; break;
		case 2710...7710:      xyz = new { 700,  50,   2710}; break;
		case 7710...17_710:    xyz = new { 800, 100,   7710}; break;
		case 17_710...32_710:  xyz = new { 900, 150, 17_710}; break;
		case 32_710...47_710:  xyz = new {1000, 150, 32_710}; break;
		case 47_710...57_710:  xyz = new {1100, 100, 47_710}; break;
		case 57_710...62_710:  xyz = new {1200,  50, 57_710}; break;
		case 62_710...64_710:  xyz = new {1300,  20, 62_710}; break;
		case 64_710...65_210:  xyz = new {1400,   5, 64_710}; break;
		case 65_210...65_410:  xyz = new {1500,   2, 65_210}; break;
	}
	return (int)Math.Floor((int)Math.Floor(((s - xyz[2]) / xyz[1]) + xyz[0]) * baseheight / 10);
}

//===============================================================================
// Returns true if the given species can be legitimately obtained as an egg.
//===============================================================================
public bool HasEgg(species) {
	species_data = GameData.Species.try_get(species);
	if (!species_data) return false;
	species = species_data.species;
	// species may be unbreedable, so check its evolution's compatibilities
	evoSpecies = species_data.get_evolutions(true);
	compatSpecies = (evoSpecies && evoSpecies[0]) ? evoSpecies[0][0] : species;
	species_data = GameData.Species.try_get(compatSpecies);
	compat = species_data.egg_groups;
	if (compat.Contains(:Undiscovered) || compat.Contains(:Ditto)) return false;
	baby = GameData.Species.get(species).get_baby_species;
	if (species == baby) return true;   // Is a basic species
	baby = GameData.Species.get(species).get_baby_species(true);
	if (species == baby) return true;   // Is an egg species without incense
	return false;
}
