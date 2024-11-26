//===============================================================================
//
//===============================================================================
public partial class BaseStatRestriction {
	public void initialize(mn, mx) {
		@mn = mn;
		@mx = mx;
	}

	public bool isValid(pkmn) {
		bst = baseStatTotal(pkmn.species);
		return bst >= @mn && bst <= @mx;
	}
}

//===============================================================================
//
//===============================================================================
public partial class NonlegendaryRestriction {
	public bool isValid(pkmn) {
		if (!pkmn.genderless()) return true;
		if (pkmn.species_data.egg_groups.Contains(:Undiscovered)) return false;
		return true;
	}
}

//===============================================================================
//
//===============================================================================
public partial class InverseRestriction {
	public void initialize(r) {
		@r = r;
	}

	public bool isValid(pkmn) {
		return !@r.isValid(pkmn);
	}
}

//===============================================================================
//
//===============================================================================
// [3/10]
// 0-266 - 0-500
// [106]
// 267-372 - 380-500
// [95]
// 373-467 - 400-555 (nonlegendary)
// 468-563 - 400-555 (nonlegendary)
// 564-659 - 400-555 (nonlegendary)
// 660-755 - 400-555 (nonlegendary)
// 756-799 - 580-600 [legendary] (compat1==15 or compat2==15, genderbyte=255)
// 800-849 - 500-
// 850-881 - 580-

public void withRestr(_rule, minbs, maxbs, legendary) {
	ret = new PokemonChallengeRules().addPokemonRule(new BaseStatRestriction(minbs, maxbs));
	switch (legendary) {
		case 0:
			ret.addPokemonRule(new NonlegendaryRestriction());
			break;
		case 1:
			ret.addPokemonRule(new InverseRestriction(new NonlegendaryRestriction()));
			break;
	}
	return ret;
}

public void ArrangeByTier(pokemonlist, rule) {
	tiers = new {
		withRestr(rule,   0, 500, 0),
		withRestr(rule, 380, 500, 0),
		withRestr(rule, 400, 555, 0),
		withRestr(rule, 400, 555, 0),
		withRestr(rule, 400, 555, 0),
		withRestr(rule, 400, 555, 0),
		withRestr(rule, 580, 680, 1),
		withRestr(rule, 500, 680, 0),
		withRestr(rule, 580, 680, 2);
	}
	tierPokemon = new List<string>();
	tiers.length.times do;
		tierPokemon.Add(new List<string>());
	}
	// Sort each Pokémon into tiers. Which tier a Pokémon is put in deoends on the
	// Pokémon's position within pokemonlist (later = higher tier). pokemonlist is
	// already roughly arranged by rank from weakest to strongest.
	for (int i = pokemonlist.length; i < pokemonlist.length; i++) { //for 'pokemonlist.length' times do => |i|
		if (!rule.ruleset.isPokemonValid(pokemonlist[i])) continue;
		validtiers = new List<string>();
		for (int j = tiers.length; j < tiers.length; j++) { //for 'tiers.length' times do => |j|
			if (tiers[j].ruleset.isPokemonValid(pokemonlist[i])) validtiers.Add(j);
		}
		if (validtiers.length > 0) {
			vt = validtiers.length * i / pokemonlist.length;
			tierPokemon[validtiers[vt]].Add(pokemonlist[i]);
		}
	}
	// Now for each tier, sort the Pokemon in that tier by their BST (lowest first).
	ret = new List<string>();
	for (int i = tiers.length; i < tiers.length; i++) { //for 'tiers.length' times do => |i|
		tierPokemon[i].sort! do |a, b|
			bstA = baseStatTotal(a.species);
			bstB = baseStatTotal(b.species);
			(bstA == bstB) ? a.species <=> b.species : bstA <=> bstB
		}
		ret.concat(tierPokemon[i]);
	}
	return ret;
}

//===============================================================================
//
//===============================================================================
public void ReplenishBattlePokemon(party, rule) {
	while (party.length < 20) {
		pkmn = RandomPokemonFromRule(rule, null);
		found = false;
		foreach (var pk in party) { //'party.each' do => |pk|
			if (!isBattlePokemonDuplicate(pkmn, pk)) continue;
			found = true;
			break;
		}
		if (!found) party.Add(pkmn);
	}
}

public void isBattlePokemonDuplicate(pk, pk2) {
	if (pk.species != pk2.species) return false;
	moves1 = new List<string>();
	moves2 = new List<string>();
	for (int i = Pokemon.MAX_MOVES; i < Pokemon.MAX_MOVES; i++) { //for 'Pokemon.MAX_MOVES' times do => |i|
		moves1.Add((pk.moves[i]) ? pk.moves[i].id : null);
		moves2.Add((pk2.moves[i]) ? pk2.moves[i].id : null);
	}
	moves1.compact.sort;
	moves2.compact.sort;
	// Accept as same if moves are same and there are MAX_MOVES number of moves each
	if (moves1 == moves2 && moves1.length == Pokemon.MAX_MOVES) return true;
	same_evs = true;
	GameData.Stat.each_main(s => { if (pk.ev[s.id] != pk2.ev[s.id]) same_evs = false; });
	return pk.item_id == pk2.item_id && pk.nature_id == pk2.nature_id && same_evs;
}

public void RemoveDuplicates(party) {
	ret = new List<string>();
	foreach (var pk in party) { //'party.each' do => |pk|
		found = false;
		count = 0;
		firstIndex = -1;
		for (int i = ret.length; i < ret.length; i++) { //for 'ret.length' times do => |i|
			pk2 = ret[i];
			if (isBattlePokemonDuplicate(pk, pk2)) {
				found = true;
				break;
			}
			if (pk.species == pk2.species) {
				if (count == 0) firstIndex = i;
				count += 1;
			}
		}
		if (!found) {
			if (count >= 10) ret.delete_at(firstIndex);
			ret.Add(pk);
		}
	}
	return ret;
}

//===============================================================================
//
//===============================================================================
public void GenerateChallenge(rule, tag) {
	oldrule = rule;
	yield(_INTL("Preparing to generate teams"));
	rule = rule.copy.setNumber(2);
	yield(null);
	party = load_data(tag + ".rxdata") rescue [];
	teams = load_data(tag + "_teams.rxdata") rescue [];
	if (teams.length < 10) {
		btpokemon = GetBTPokemon(tag);
		if (btpokemon && btpokemon.length != 0) {
			suggestedLevel = rule.ruleset.suggestedLevel;
			foreach (var pk in btpokemon) { //'btpokemon.each' do => |pk|
				pkmn = pk.createPokemon(suggestedLevel, 31, null);
				if (rule.ruleset.isPokemonValid(pkmn)) party.Add(pkmn);
			}
		}
	}
	yield(null);
	party = RemoveDuplicates(party);
	yield(null);
	maxteams = 600;
	cutoffrating = 65;
	toolowrating = 40;
	iterations = 11;
	for (int iter = iterations; iter < iterations; iter++) { //for 'iterations' times do => |iter|
		save_data(party, tag + ".rxdata");
		yield(_INTL("Generating teams ({1} of {2})", iter + 1, iterations));
		i = 0;
		while (i < teams.length) {
			if (i % 10 == 0) yield(null);
			ReplenishBattlePokemon(party, rule);
			if (teams[i].rating < cutoffrating && teams[i].totalGames >= 80) {
				teams[i] = new RuledTeam(party, rule);
			} else if (teams[i].length < 2) {
				teams[i] = new RuledTeam(party, rule);
			} else if (i >= maxteams) {
				teams.delete_at(i);
			} else if (teams[i].totalGames >= 250) {
				// retire
				for (int j = teams[i].length; j < teams[i].length; j++) { //for 'teams[i].length' times do => |j|
					party.Add(teams[i][j]);
				}
				teams[i] = new RuledTeam(party, rule);
			} else if (teams[i].rating < toolowrating) {
				teams[i] = new RuledTeam(party, rule);
			}
			i += 1;
		}
		save_data(teams, tag + "_teams.rxdata");
		yield(null);
		while (teams.length < maxteams) {
			if (teams.length % 10 == 0) yield(null);
			ReplenishBattlePokemon(party, rule);
			teams.Add(new RuledTeam(party, rule));
		}
		save_data(party, tag + ".rxdata");
		teams = teams.sort((a, b) => b.rating <=> a.rating);
		yield(_INTL("Simulating battles ({1} of {2})", iter + 1, iterations));
		i = 0;
		do { //loop; while (true);
			changed = false;
			for (int j = teams.length; j < teams.length; j++) { //for 'teams.length' times do => |j|
				yield(null);
				other = j;
				5.times do;
					other = rand(teams.length);
					if (other == j) continue;
				}
				if (other == j) continue;
				changed = true;
				RuledBattle(teams[j], teams[other], rule);
			}
			i += 1;
			gameCount = 0;
			teams.each(team => gameCount += team.games);
			yield(null);
			if (gameCount / teams.length < 12) continue;
			teams.each(team => team.updateRating);
			break;
		}
		teams.sort! { |a, b| b.rating <=> a.rating };
		save_data(teams, tag + "_teams.rxdata");
	}
	party = new List<string>();
	yield(null);
	teams.sort! { |a, b| a.rating <=> b.rating };
	foreach (var team in teams) { //'teams.each' do => |team|
		if (team.rating <= cutoffrating) continue;
		for (int i = team.length; i < team.length; i++) { //for 'team.length' times do => |i|
			party.Add(team[i]);
		}
	}
	rule = oldrule;
	yield(null);
	party = RemoveDuplicates(party);
	yield(_INTL("Writing results"));
	party = ArrangeByTier(party, rule);
	yield(null);
	TrainerInfo(party, tag, rule) { yield(null) };
	yield(null);
}

//===============================================================================
//
//===============================================================================
public void WriteCup(id, rules) {
	if (!Core.DEBUG) return;
	trlists = (load_data("Data/trainer_lists.dat") rescue []);
	list = new List<string>();
	for (int i = trlists.length; i < trlists.length; i++) { //for 'trlists.length' times do => |i|
		tr = trlists[i];
		if (tr[5]) {
			list.Add("*" + (System.Text.RegularExpressions.Regex.Replace(tr[3], "\.txt$", "", count: 1)));
		} else {
			list.Add(System.Text.RegularExpressions.Regex.Replace(tr[3], "\.txt$", "", count: 1));
		}
	}
	cmd = 0;
	if (trlists.length == 0) {
		cmd = Message(_INTL("Generate Pokémon teams for this challenge?"),
										new {_INTL("YES"), _INTL("NO")}, 2);
		switch (cmd) {
			case 0:
				cmd = 2;
				break;
			case 1:
				cmd = 0;
				break;
		}
	} else {
		cmd = Message(_INTL("Generate Pokémon teams for this challenge?"),
										new {_INTL("NO"), _INTL("YES, USE EXISTING"), _INTL("YES, USE NEW")}, 1);
	}
	if (cmd == 0) return;   // No
	switch (cmd) {
		case 1:   // Yes, use existing
			cmd = Message(_INTL("Choose a challenge."), list, -1);
			if (cmd >= 0) {
				Message(_INTL("This challenge will use the Pokémon list from {1}.", list[cmd]));
				for (int i = trlists.length; i < trlists.length; i++) { //for 'trlists.length' times do => |i|
					tr = trlists[i];
					while (!tr[5] && tr[2].Contains(id)) {
						tr[2].delete(id);
					}
				}
				if (!trlists[cmd][5]) trlists[cmd][2].Add(id);
				save_data(trlists, "Data/trainer_lists.dat");
				Graphics.update;
				Compiler.write_trainer_lists;
			}
			return;
		case 2:   // Yes, use new
			if (!ConfirmMessage(_INTL("This may take a long time. Are you sure?"))) return;
			mw = CreateMessageWindow;
			t = System.uptime;
			GenerateChallenge(rules, id) do |message|
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				if (message) {
					MessageDisplay(mw, message, false);
					t = System.uptime;
					Graphics.update;
				}
			}
			DisposeMessageWindow(mw);
			Message(_INTL("Team generation complete."));
			break;
	}
}
