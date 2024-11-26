Game.GameData.baseStatTotal   = new List<string>()
Game.GameData.babySpecies     = new List<string>();
Game.GameData.minimumLevel    = new List<string>();
Game.GameData.evolutions      = new List<string>();
Game.GameData.legalMoves      = new List<string>();    // For each species, all the moves they have access to
Game.GameData.legalMovesLevel = 0;     // Level for which Game.GameData.legalMoves were calculated
Game.GameData.tmMoves         = null;   // Array of all moves teachable by a HM/TM/TR

public void BaseStatTotal(species) {
	baseStats = GameData.Species.get(species).base_stats;
	ret = 0;
	baseStats.each_value(s => ret += s);
	return ret;
}

public void baseStatTotal(species) {
	if (!Game.GameData.baseStatTotal[species]) Game.GameData.baseStatTotal[species] = BaseStatTotal(species);
	return Game.GameData.baseStatTotal[species];
}

public void babySpecies(species) {
	if (!Game.GameData.babySpecies[species]) Game.GameData.babySpecies[species] = GameData.Species.get(species).get_baby_species;
	return Game.GameData.babySpecies[species];
}

public void minimumLevel(species) {
	if (!Game.GameData.minimumLevel[species]) Game.GameData.minimumLevel[species] = GameData.Species.get(species).minimum_level;
	return Game.GameData.minimumLevel[species];
}

public void evolutions(species) {
	if (!Game.GameData.evolutions[species]) Game.GameData.evolutions[species] = GameData.Species.get(species).get_evolutions(true);
	return Game.GameData.evolutions[species];
}

//===============================================================================
//
//===============================================================================
// Used to replace Sketch with any other move.
public void RandomMove() {
	keys = GameData.Move.keys;
	do { //loop; while (true);
		move_id = keys.sample;
		move = GameData.Move.get(move_id);
		if (new []{"Struggle", "ReplaceMoveWithTargetLastMoveUsed"}.Contains(move.function_code)) continue;
		return move.id;
	}
}

public void GetLegalMoves2(species, maxlevel) {
	species_data = GameData.Species.get(species);
	moves = new List<string>();
	if (!species_data) return moves;
	// Populate available moves array (moves)
	species_data.moves.each(m => { if (m[0] <= maxlevel) addMove(moves, m[1], 2); });
	if (!Game.GameData.tmMoves) {
		Game.GameData.tmMoves = new List<string>();
		GameData.Item.each(i => { if (i.is_machine()) Game.GameData.tmMoves.Add(i.move); });
	}
	species_data.tutor_moves.each(m => { if (Game.GameData.tmMoves.Contains(m)) addMove(moves, m, 0); });
	babyspecies = babySpecies(species);
	GameData.Species.get(babyspecies).egg_moves.each(m => addMove(moves, m, 2));
	movedatas = new List<string>();
	foreach (var move in moves) { //'moves.each' do => |move|
		movedatas.Add(new {move, GameData.Move.get(move)});
	}
	// Delete less powerful moves
	deleteAll = block: (a, item) => {
		while (a.Contains(item)) {
			a.delete(item);
		}
	}
	foreach (var move in moves) { //'moves.each' do => |move|
		md = GameData.Move.get(move);
		foreach (var move2 in movedatas) { //'movedatas.each' do => |move2|
			// If we have a move that always hits, remove all other moves with no
			// effect of the same type and <= base power
			if (md.accuracy == 0 && move2[1].function_code == "None" &&
				md.type == move2[1].type && md.power >= move2[1].power) {
				deleteAll.call(moves, move2[0]);
			// If we have two status moves that have the same function code, delete the
			// one with lower accuracy (Supersonic vs. Confuse Ray, etc.)
			} else if (md.function_code == move2[1].function_code && md.power == 0 &&
						move2[1].power == 0 && md.accuracy > move2[1].accuracy) {
				deleteAll.call(moves, move2[0]);
			// Delete poison-causing moves if we have a move that causes toxic
			} else if (md.function_code == "BadPoisonTarget" && move2[1].function_code == "PoisonTarget") {
				deleteAll.call(moves, move2[0]);
			// If we have two moves with the same function code and type, and one of
			// them is damaging and has 10/15/the same PP as the other move and EITHER
			// does more damage than the other move OR does the same damage but is more
			// accurate, delete the other move (Surf, Flamethrower, Thunderbolt, etc.)
			} else if (md.function_code == move2[1].function_code && md.power != 0 &&
						md.type == move2[1].type &&
						(md.total_pp == 15 || md.total_pp == 10 || md.total_pp == move2[1].total_pp) &&
						(md.power > move2[1].power ||
						(md.power == move2[1].power && md.accuracy > move2[1].accuracy))) {
				deleteAll.call(moves, move2[0]);
			}
		}
	}
	return moves;
}

public void addMove(moves, move, base) {
	data = GameData.Move.get(move);
	if (moves.Contains(data.id)) return;
	if (new []{:BUBBLE, :BUBBLEBEAM}.Contains(data.id)) return;   // Never add these moves
	count = base + 1;   // Number of times to add move to moves
	if (data.function_code == "None" && data.power <= 40) count = base;
	if (data.power <= 30 || new []{:GROWL, :TAILWHIP, :LEER}.Contains(data.id)) {
		count = base;
	}
	if (data.power >= 60 ||
		new []{:REFLECT, :LIGHTSCREEN, :SAFEGUARD, :SUBSTITUTE, :FAKEOUT}.Contains(data.id)) {
		count = base + 2;
	}
	if (data.power >= 80 && data.type == types.NORMAL) count = base + 3;
	if ((new []{:PROTECT, :DETECT, :TOXIC, :AERIALACE, :WILLOWISP, :SPORE, :THUNDERWAVE,
			:HYPNOSIS, :CONFUSERAY, :ENDURE, :SWORDSDANCE}.Contains(data.id)) {
		count = base + 3;
	}
	count.times(() => moves.Add(data.id));
}

// Returns whether moves contains any move with the same type as thismove but
// with a higher base damage than it.
public void hasMorePowerfulMove(moves, thismove) {
	thisdata = GameData.Move.get(thismove);
	if (thisdata.power == 0) return false;
	foreach (var move in moves) { //'moves.each' do => |move|
		if (!move) continue;
		moveData = GameData.Move.get(move);
		if (moveData.type == thisdata.type && moveData.power > thisdata.power) {
			return true;
		}
	}
	return false;
}

//===============================================================================
// Generate a random Pokémon that adheres to the given rules.
//===============================================================================
public void RandomPokemonFromRule(rules, trainer) {
	pkmn = null;
	iteration = -1;
	do { //loop; while (true);
		iteration += 1;
		species = null;
		level = rules.ruleset.suggestedLevel;
		keys = GameData.Species.keys;
		do { //loop; while (true);
			do { //loop; while (true);
				species = keys.sample;
				if (GameData.Species.get(species).form == 0) break;
			}
			r = rand(20);
			bst = baseStatTotal(species);
			if (level < minimumLevel(species)) continue;
			if (iteration.even()) {
				if (r < 16 && bst < 400) continue;
				if (r < 13 && bst < 500) continue;
			} else {
				if (bst > 400) continue;
				if (r < 10 && babySpecies(species) != species) continue;
			}
			if (r < 10 && babySpecies(species) == species) continue;
			if (r < 7 && evolutions(species).length > 0) continue;
			break;
		}
		ev = new List<string>();
		GameData.Stat.each_main(s => { if (rand(100) < 50) ev.Add(s.id); });
		nature = null;
		keys = GameData.Nature.keys;
		do { //loop; while (true);
			nature = keys.sample;
			nature_data = GameData.Nature.get(nature);
			if (new []{:LAX, :GENTLE}.Contains(nature_data.id) || nature_data.stat_changes.length == 0) {
				if (rand(20) < 19) continue;
			} else {
				raised_emphasis = false;
				lowered_emphasis = false;
				foreach (var change in nature_data.stat_changes) { //'nature_data.stat_changes.each' do => |change|
					if (!ev.Contains(change[0])) continue;
					if (change[1] > 0) raised_emphasis = true;
					if (change[1] < 0) lowered_emphasis = true;
				}
				if (rand(10) < 6 && !raised_emphasis) continue;
				if (rand(10) < 9 && lowered_emphasis) continue;
			}
			break;
		}
		if (level != Game.GameData.legalMovesLevel) Game.GameData.legalMoves = new List<string>();
		Game.GameData.legalMovesLevel = level;
		if (!Game.GameData.legalMoves[species]) Game.GameData.legalMoves[species] = GetLegalMoves2(species, level);
		itemlist = new {
			:ORANBERRY, :SITRUSBERRY, :ADAMANTORB, :BABIRIBERRY,
			:BLACKSLUDGE, :BRIGHTPOWDER, :CHESTOBERRY, :CHOICEBAND,
			:CHOICESCARF, :CHOICESPECS, :CHOPLEBERRY, :DAMPROCK,
			:DEEPSEATOOTH, :EXPERTBELT, :FLAMEORB, :FOCUSSASH,
			:FOCUSBAND, :HEATROCK, :LEFTOVERS, :LIFEORB, :LIGHTBALL,
			:LIGHTCLAY, :LUMBERRY, :OCCABERRY, :PETAYABERRY, :SALACBERRY,
			:SCOPELENS, :SHEDSHELL, :SHELLBELL, :SHUCABERRY, :LIECHIBERRY,
			:SILKSCARF, :THICKCLUB, :TOXICORB, :WIDELENS, :YACHEBERRY,
			:HABANBERRY, :SOULDEW, :PASSHOBERRY, :QUICKCLAW, :WHITEHERB;
		}
		// Most used: Leftovers, Life Orb, Choice Band, Choice Scarf, Focus Sash
		item = null;
		do { //loop; while (true);
			if (rand(40) == 0) {
				item = items.LEFTOVERS;
				break;
			}
			item = itemlist[rand(itemlist.length)];
			if (!item) continue;
			switch (item) {
				case :LIGHTBALL:
					if (species != speciess.PIKACHU) continue;
					break;
				case :SHEDSHELL:
					if (species != speciess.FORRETRESS && species != speciess.SKARMORY) continue;
					break;
				case :SOULDEW:
					if (species != speciess.LATIOS && species != speciess.LATIAS) continue;
					break;
				case :FOCUSSASH:
					if (baseStatTotal(species) > 450 && rand(10) < 8) continue;
					break;
				case :ADAMANTORB:
					if (species != speciess.DIALGA) continue;
					break;
				case :PASSHOBERRY:
					if (species != speciess.STEELIX) continue;
					break;
				case :BABIRIBERRY:
					if (species != speciess.TYRANITAR) continue;
					break;
				case :HABANBERRY:
					if (species != speciess.GARCHOMP) continue;
					break;
				case :OCCABERRY:
					if (species != speciess.METAGROSS) continue;
					break;
				case :CHOPLEBERRY:
					if (species != speciess.UMBREON) continue;
					break;
				case :YACHEBERRY:
					if (!new []{:TORTERRA, :GLISCOR, :DRAGONAIR}.Contains(species)) continue;
					break;
				case :SHUCABERRY:
					if (species != speciess.HEATRAN) continue;
					break;
				case :DEEPSEATOOTH:
					if (species != speciess.CLAMPERL) continue;
					break;
				case :THICKCLUB:
					if (!new []{:CUBONE, :MAROWAK}.Contains(species)) continue;
					break;
				case :LIECHIBERRY:
					if (!ev.Contains(:ATTACK) && rand(100) < 50) ev.Add(:ATTACK);
					break;
				case :SALACBERRY:
					if (!ev.Contains(:SPEED) && rand(100) < 50) ev.Add(:SPEED);
					break;
				case :PETAYABERRY:
					if (!ev.Contains(:SPECIAL_ATTACK) && rand(100) < 50) ev.Add(:SPECIAL_ATTACK);
					break;
			}
			break;
		}
		if (level < 10 && GameData.Items.exists(Items.ORANBERRY)) {
			if (rand(40) == 0 || item == items.SITRUSBERRY) item = items.ORANBERRY;
		} else if (level > 20 && GameData.Items.exists(Items.SITRUSBERRY)) {
			if (rand(40) == 0 || item == items.ORANBERRY) item = items.SITRUSBERRY;
		}
		moves = Game.GameData.legalMoves[species];
		sketch = false;
		if (moves[0] == :SKETCH) {
			sketch = true;
			Pokemon.MAX_MOVES.times(m => moves[m] = RandomMove);
		}
		if (moves.length == 0) continue;
		if ((moves | []).length < Pokemon.MAX_MOVES) {
			if (moves.length == 0) moves = [:TACKLE];
			moves |= new List<string>();
		} else {
			newmoves = new List<string>();
			rest = GameData.Moves.exists(Moves.REST) ? :REST : null;
			spitup = GameData.Moves.exists(Moves.SPITUP) ? :SPITUP : null;
			swallow = GameData.Moves.exists(Moves.SWALLOW) ? :SWALLOW : null;
			stockpile = GameData.Moves.exists(Moves.STOCKPILE) ? :STOCKPILE : null;
			snore = GameData.Moves.exists(Moves.SNORE) ? :SNORE : null;
			sleeptalk = GameData.Moves.exists(Moves.SLEEPTALK) ? :SLEEPTALK : null;
			do { //loop; while (true);
				newmoves.clear;
				while (newmoves.length < (int)Math.Min(moves.length, Pokemon.MAX_MOVES)) {
					m = moves[rand(moves.length)];
					if (rand(100) < 50 && hasMorePowerfulMove(moves, m)) continue;
					if (m && !newmoves.Contains(m)) newmoves.Add(m);
				}
				if ((newmoves.Contains(spitup) || newmoves.Contains(swallow)) &&
					!newmoves.Contains(stockpile) && !sketch) {
					continue;
				}
				if ((!newmoves.Contains(spitup) && !newmoves.Contains(swallow)) &&
					newmoves.Contains(stockpile) && !sketch) {
					continue;
				}
				if (newmoves.Contains(sleeptalk) && !newmoves.Contains(rest) &&
					!((sketch || !moves.Contains(rest)) && rand(100) < 20)) {
					continue;
				}
				if (newmoves.Contains(snore) && !newmoves.Contains(rest) &&
					!((sketch || !moves.Contains(rest)) && rand(100) < 20)) {
					continue;
				}
				total_power = 0;
				hasPhysical = false;
				hasSpecial = false;
				hasNormal = false;
				foreach (var move in newmoves) { //'newmoves.each' do => |move|
					d = GameData.Move.get(move);
					if (d.power == 0) continue;
					total_power += d.power;
					if (d.type == types.NORMAL) hasNormal = true;
					if (d.category == 0) hasPhysical = true;
					if (d.category == 1) hasSpecial = true;
				}
				if (!hasPhysical && ev.Contains(:ATTACK) && rand(100) < 80) {
					// No physical attack, but emphasizes Attack
					continue;
				}
				if (!hasSpecial && ev.Contains(:SPECIAL_ATTACK) && rand(100) < 80) {
					// No special attack, but emphasizes Special Attack
					continue;
				}
				r = rand(10);
				if (r > 6 && total_power > 180) continue;
				if (r > 8 && total_power > 140) continue;
				if (total_power == 0 && rand(100) < 95) continue;
				//###########
				// Moves accepted
				if (hasPhysical && !hasSpecial) {
					if (rand(100) < 80) ev.Add(:ATTACK);
					if (rand(100) < 80) ev.delete(:SPECIAL_ATTACK);
				}
				if (!hasPhysical && hasSpecial) {
					if (rand(100) < 80) ev.delete(:ATTACK);
					if (rand(100) < 80) ev.Add(:SPECIAL_ATTACK);
				}
				if (!hasNormal && item == items.SILKSCARF) item = items.LEFTOVERS;
				moves = newmoves;
				break;
			}
		}
		if (item == items.LIGHTCLAY && moves.none(m => new []{:LIGHTSCREEN, :REFLECT}.Contains(m))) {
			item = items.LEFTOVERS;
		}
		if (item == items.BLACKSLUDGE) {
			types = GameData.Species.get(species).types;
			if (!types.Contains(:POISON)) item = items.LEFTOVERS;
		}
		if (item == items.HEATROCK && moves.none(m => m == :SUNNYDAY)) item = items.LEFTOVERS;
		if (item == items.DAMPROCK && moves.none(m => m == :RAINDANCE)) {
			item = items.LEFTOVERS;
		}
		if (moves.any(m => m == :REST)) {
			if (rand(100) < 33) item = items.LUMBERRY;
			if (rand(100) < 25) item = items.CHESTOBERRY;
		}
		pk = new Pokemon(species, item, nature, moves[0], moves[1], moves[2], moves[3], ev);
		pkmn = pk.createPokemon(level, 31, trainer);
		if (rules.ruleset.isPokemonValid(pkmn)) break;
	}
	return pkmn;
}
