//===============================================================================
//
//===============================================================================
public void getTypes(species) {
	species_data = GameData.Species.get(species);
	return species_data.types.clone;
}

//===============================================================================
// If no trainers are defined for the current challenge, generate a set of random
// ones for it. If pokemonlist is given, assign Pokémon from it to all trainers.
// Save the results in the appropriate PBS files.
//===============================================================================
public void TrainerInfo(pokemonlist, trfile, rules) {
	bttrainers = GetBTTrainers(trfile);
	btpokemon = GetBTPokemon(trfile);
	// No battle trainers found; fill bttrainers with 200 randomly chosen ones from
	// all that exist (with a base money < 100)
	if (bttrainers.length == 0) {
		for (int i = 200; i < 200; i++) { //for '200' times do => |i|
			if (block_given() && i % 50 == 0) yield(null);
			trainerid = null;
			if (GameData.TrainerTypes.exists(Types.YOUNGSTER) && rand(30) == 0) {
				trainerid = :YOUNGSTER;
			} else {
				tr_typekeys = GameData.TrainerType.keys;
				do { //loop; while (true);
					tr_type = tr_typekeys.sample;
					tr_type_data = GameData.TrainerType.get(tr_type);
					if (tr_type_data.base_money >= 100) continue;
					trainerid = tr_type_data.id;
				}
			}
			// Create a random name for the trainer
			gender = GameData.TrainerType.get(trainerid).gender;
			randomName = getRandomNameEx(gender, null, 0, 12);
			// Add the trainer to bttrainers
			tr = new {trainerid, randomName, _INTL("Here I come!"), _INTL("Yes, I won!"),
						_INTL("Man, I lost!"), new List<string>()};
			bttrainers.Add(tr);
		}
		// Sort all the randomly chosen trainers by their base money (smallest first)
		bttrainers.sort! do |a, b|
			money1 = GameData.TrainerType.get(a[0]).base_money;
			money2 = GameData.TrainerType.get(b[0]).base_money;
			next (money1 == money2) ? a[0].ToString() <=> b[0].ToString() : money1 <=> money2;
		}
	}
	if (block_given()) yield(null);
	// Set all Pokémon in pokemonlist to the appropriate level, and determine their
	// type(s) and whether they are valid for the given rules
	suggestedLevel = rules.ruleset.suggestedLevel;
	rulesetTeam = rules.ruleset.copy.clearPokemonRules;
	pkmntypes = new List<string>();
	validities = new List<string>();
	foreach (var pkmn in pokemonlist) { //'pokemonlist.each' do => |pkmn|
		if (pkmn.level != suggestedLevel) pkmn.level = suggestedLevel;
		pkmntypes.Add(getTypes(pkmn.species));
		validities.Add(rules.ruleset.isPokemonValid(pkmn));
	}
	// For each trainer in bttrainers, come up with a set of Pokémon taken from
	// pokemonlist for that trainer, and copy the trainer and their set of Pokémon
	// to newbttrainers
	newbttrainers = new List<string>();
	for (int btt = bttrainers.length; btt < bttrainers.length; btt++) { //for 'bttrainers.length' times do => |btt|
		if (block_given() && btt % 50 == 0) yield(null);
		trainerdata = bttrainers[btt];
		pokemonnumbers = trainerdata[5] || [];
		// Find all the Pokémon available to the trainer, and count up how often
		// those Pokémon have each type
		species = new List<string>();
		types = new List<string>();
		GameData.Type.each(t => types[t.id] = 0);
		foreach (var pn in pokemonnumbers) { //'pokemonnumbers.each' do => |pn|
			pkmn = btpokemon[pn];
			species.Add(pkmn.species);
			t = getTypes(pkmn.species);
			t.each(typ => types[typ] += 1);
		}
		species |= new List<string>();   // remove duplicates
		// Scale down the counts of each type to the range 0 -> 10
		count = 0;
		foreach (var t in GameData.Type) { //'GameData.Type.each' do => |t|
			if (types[t.id] >= 5) {
				types[t.id] /= 4;
				if (types[t.id] > 10) types[t.id] = 10;
			} else {
				types[t.id] = 0;
			}
			count += types[t.id];
		}
		if (count == 0) types[:NORMAL] = 1;   // All type counts are 0; add 1 to Normal
		// Trainer had no Pokémon available to it; make all the type counts 1
		if (pokemonnumbers.length == 0) {
			GameData.Type.each(t => types[t.id] = 1);
		}
		// Get Pokémon from pokemonlist, if there are any, and make sure enough are
		// gotten that a valid team can be made from them
		numbers = new List<string>();
		if (pokemonlist) {
			// For each valid Pokémon in pokemonlist, add its index within pokemonlist
			// to numbers, but only if that species is available to the trainer.
			// Pokémon are less likely to be added if it is positioned later in
			// pokemonlist, or if the trainer is positioned earlier in bttrainers (i.e.
			// later trainers get better Pokémon).
			numbersPokemon = new List<string>();
			for (int index = pokemonlist.length; index < pokemonlist.length; index++) { //for 'pokemonlist.length' times do => |index|
				if (!validities[index]) continue;
				pkmn = pokemonlist[index];
				absDiff = ((index * 8 / pokemonlist.length) - (btt * 8 / bttrainers.length)).abs;
				if (species.Contains(pkmn.species)) {
					weight = new {32, 12, 5, 2, 1, 0, 0, 0}new {(int)Math.Min(absDiff, 7)};
					if (rand(40) < weight) {
						numbers.Add(index);
						numbersPokemon.Add(pokemonlist[index]);
					}
				} else {
					// Pokémon's species isn't available to the trainer; try adding it
					// anyway (more likely to add it if the trainer has access to more
					// Pokémon of the same type(s) as this Pokémon)
					t = pkmntypes[index];
					foreach (var typ in t) { //'t.each' do => |typ|
						weight = new {32, 12, 5, 2, 1, 0, 0, 0}new {(int)Math.Min(absDiff, 7)};
						weight *= types[typ];
						if (rand(40) < weight) {
							numbers.Add(index);
							numbersPokemon.Add(pokemonlist[index]);
						}
					}
				}
			}
			numbers |= new List<string>();   // Remove duplicates
			// If there aren't enough Pokémon to form a full team, or a valid team
			// can't be made from them, fill up numbers with Pokémon in pokemonlist
			// that EITHER have the same species as one available to the trainer OR has
			// a type that is available to the trainer, until a valid team can be
			// formed from what's in numbers
			if (numbers.length < Settings.MAX_PARTY_SIZE ||
				!rulesetTeam.hasValidTeam(numbersPokemon)) {
				for (int index = pokemonlist.length; index < pokemonlist.length; index++) { //for 'pokemonlist.length' times do => |index|
					pkmn = pokemonlist[index];
					if (!validities[index]) continue;
					if (species.Contains(pkmn.species)) {
						numbers.Add(index);
						numbersPokemon.Add(pokemonlist[index]);
					} else {
						t = pkmntypes[index];
						foreach (var typ in t) { //'t.each' do => |typ|
							if (types[typ] <= 0 || numbers.Contains(index)) continue;
							numbers.Add(index);
							numbersPokemon.Add(pokemonlist[index]);
							break;
						}
					}
					if (numbers.length >= Settings.MAX_PARTY_SIZE && rules.ruleset.hasValidTeam(numbersPokemon)) break;
				}
				// If there STILL aren't enough Pokémon to form a full team, or a valid
				// team can't be made from them, add random Pokémon from pokemonlist
				// until a valid team can be formed from what's in numbers
				if (numbers.length < Settings.MAX_PARTY_SIZE || !rules.ruleset.hasValidTeam(numbersPokemon)) {
					while (numbers.length < pokemonlist.length &&) {
								(numbers.length < Settings.MAX_PARTY_SIZE || !rules.ruleset.hasValidTeam(numbersPokemon))
						index = rand(pokemonlist.length);
						if (!numbers.Contains(index)) {
							numbers.Add(index);
							numbersPokemon.Add(pokemonlist[index]);
						}
					}
				}
			}
			numbers.sort!;
		}
		// Add the trainer's data, including all Pokémon that should be available to
		// it (from pokemonlist), to newbttrainers
		newbttrainers.Add(new {trainerdata[0], trainerdata[1], trainerdata[2],
												trainerdata[3], trainerdata[4], numbers});
	}
	if (block_given()) yield(null);
	// Add the trainer and Pokémon data from above to trainer_lists.dat, and then
	// create all PBS files from it
	pokemonlist = new List<string>();
	foreach (var pkmn in pokemonlist) { //'pokemonlist.each' do => |pkmn|
		pokemonlist.Add(Pokemon.fromPokemon(pkmn));
	}
	trlists = (load_data("Data/trainer_lists.dat") rescue []);
	hasDefault = false;
	trIndex = -1;
	for (int i = trlists.length; i < trlists.length; i++) { //for 'trlists.length' times do => |i|
		if (!trlists[i][5]) continue;
		hasDefault = true;
		break;
	}
	for (int i = trlists.length; i < trlists.length; i++) { //for 'trlists.length' times do => |i|
		if (!trlists[i][2].Contains(trfile)) continue;
		trIndex = i;
		trlists[i][0] = newbttrainers;
		trlists[i][1] = pokemonlist;
		trlists[i][5] = !hasDefault;
	}
	if (block_given()) yield(null);
	if (trIndex < 0) {
		info = new {newbttrainers, pokemonlist, [trfile],
						trfile + "_trainers.txt", trfile + "_pkmn.txt", !hasDefault};
		trlists.Add(info);
	}
	if (block_given()) yield(null);
	save_data(trlists, "Data/trainer_lists.dat");
	if (block_given()) yield(null);
	Compiler.write_trainer_lists;
	if (block_given()) yield(null);
}
