//===============================================================================
//
//===============================================================================
public void BattleChallenge() {
	if (!Game.GameData.PokemonGlobal.challenge) Game.GameData.PokemonGlobal.challenge = new BattleChallenge();
	return Game.GameData.PokemonGlobal.challenge;
}

public void BattleChallengeBattle() {
	return BattleChallenge.Battle;
}

// Used in events
public bool HasEligible(*arg) {
	return BattleChallenge.rules.ruleset.hasValidTeam(Game.GameData.player.party);
}

//===============================================================================
//
//===============================================================================
public void GetBTTrainers(challengeID) {
	trlists = (load_data("Data/trainer_lists.dat") rescue []);
	trlists.each(tr => { if (!tr[5] && tr[2].Contains(challengeID)) return tr[0]; });
	trlists.each(tr => { if (tr[5]) return tr[0]; });   // is default list
	return [];
}

public void GetBTPokemon(challengeID) {
	trlists = (load_data("Data/trainer_lists.dat") rescue []);
	trlists.each(tr => { if (!tr[5] && tr[2].Contains(challengeID)) return tr[1]; });
	trlists.each(tr => { if (tr[5]) return tr[1]; });   // is default list
	return [];
}

//===============================================================================
//
//===============================================================================
public void EntryScreen(*arg) {
	retval = false;
	FadeOutIn do;
		scene = new PokemonParty_Scene();
		screen = new PokemonPartyScreen(scene, Game.GameData.player.party);
		ret = screen.PokemonMultipleEntryScreenEx(BattleChallenge.rules.ruleset);
		// Set party
		if (ret) BattleChallenge.setParty(ret);
		// Continue (return true) if Pokémon were chosen
		retval = (ret && ret.length > 0);
	}
	return retval;
}

//===============================================================================
//
//===============================================================================
public partial class Game_Player : Game_Character {
	public void moveto2(x, y) {
		@x = x;
		@y = y;
		@real_x = @x * Game_Map.REAL_RES_X;
		@real_y = @y * Game_Map.REAL_RES_Y;
		@prelock_direction = 0;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Game_Event {
	public bool InChallenge() {
		return BattleChallenge.InChallenge();
	}
}

//===============================================================================
//
//===============================================================================
public void BattleChallengeGraphic(event) {
	nextTrainer = BattleChallenge.nextTrainer;
	bttrainers = GetBTTrainers(BattleChallenge.currentChallenge);
	filename = GameData.TrainerType.charset_filename_brief((bttrainers[nextTrainer][0] rescue null));
	begin;
		if (nil_or_empty(filename)) filename = "NPC 01";
		bitmap = new AnimatedBitmap("Graphics/Characters/" + filename);
		bitmap.dispose;
		event.character_name = filename;
	rescue;
		event.character_name = "NPC 01";
	}
}

public void BattleChallengeBeginSpeech() {
	if (!BattleChallenge.InProgress()) return "...";
	bttrainers = GetBTTrainers(BattleChallenge.currentChallenge);
	tr = bttrainers[BattleChallenge.nextTrainer];
	return (tr) ? GetMessageFromHash(MessageTypes.FRONTIER_INTRO_SPEECHES, tr[2]) : "...";
}

//===============================================================================
//
//===============================================================================
public partial class Pokemon {
	public int species		{ get { return _species; } set { _species = value; } }			protected int _species;
	public int item		{ get { return _item; } set { _item = value; } }			protected int _item;
	public int nature		{ get { return _nature; } set { _nature = value; } }			protected int _nature;
	public int move1		{ get { return _move1; } set { _move1 = value; } }			protected int _move1;
	public int move2		{ get { return _move2; } set { _move2 = value; } }			protected int _move2;
	public int move3		{ get { return _move3; } set { _move3 = value; } }			protected int _move3;
	public int move4		{ get { return _move4; } set { _move4 = value; } }			protected int _move4;
	public int ev		{ get { return _ev; } set { _ev = value; } }			protected int _ev;

	// This method is how each Pokémon is compiled from the PBS files listing
	// Battle Tower/Cup Pokémon.
	public static void fromInspected(str) {
		insp = System.Text.RegularExpressions.Regex.Replace(str, "^\s+", ""); insp = System.Text.RegularExpressions.Regex.Replace(insp, "\s+$", "");
		pieces = insp.split(/\s*;\s*/);
		species = (GameData.Species.exists(pieces[0])) ? GameData.Species.get(pieces[0]).id : null;
		item = (GameData.Item.exists(pieces[1])) ? GameData.Item.get(pieces[1]).id : null;
		nature = (GameData.Nature.exists(pieces[2])) ? GameData.Nature.get(pieces[2]).id : null;
		ev = pieces[3].split(/\s*,\s*/);
		ev_array = new List<string>();
		foreach (var stat in ev) { //'ev.each' do => |stat|
			switch (stat.upcase) {
				case "HP":				 ev_array.Add(:HP); break;
				case "ATK":				 ev_array.Add(:ATTACK); break;
				case "DEF":				 ev_array.Add(:DEFENSE); break;
				case "SA": case "SPATK": ev_array.Add(:SPECIAL_ATTACK); break;
				case "SD": case "SPDEF": ev_array.Add(:SPECIAL_DEFENSE); break;
				case "SPD":				 ev_array.Add(:SPEED); break;
			}
		}
		moves = pieces[4].split(/\s*,\s*/);
		moveid = new List<string>();
		for (int i = Pokemon.MAX_MOVES; i < Pokemon.MAX_MOVES; i++) { //for 'Pokemon.MAX_MOVES' times do => |i|
			move_data = GameData.Move.try_get(moves[i]);
			if (move_data) moveid.Add(move_data.id);
		}
		if (moveid.length == 0) moveid.Add(GameData.Move.keys.first);   // Get any one move
		return new self(species, item, nature, moveid[0], moveid[1], moveid[2], moveid[3], ev_array);
	}

	public static void fromPokemon(pkmn) {
		mov1 = (pkmn.moves[0]) ? pkmn.moves[0].id : null;
		mov2 = (pkmn.moves[1]) ? pkmn.moves[1].id : null;
		mov3 = (pkmn.moves[2]) ? pkmn.moves[2].id : null;
		mov4 = (pkmn.moves[3]) ? pkmn.moves[3].id : null;
		ev_array = new List<string>();
		foreach (var s in GameData.Stat) { //GameData.Stat.each_main do => |s|
			if (pkmn.ev[s.id] > 60) ev_array.Add(s.id);
		}
		return new self(pkmn.species, pkmn.item_id, pkmn.nature,
										mov1, mov2, mov3, mov4, ev_array);
	}

	public void initialize(species, item, nature, move1, move2, move3, move4, ev) {
		@species = species;
		itm = GameData.Item.try_get(item);
		@item    = itm ? itm.id : null;
		@nature  = nature;
		@move1   = move1;
		@move2   = move2;
		@move3   = move3;
		@move4   = move4;
		@ev      = ev;
	}

	public void inspect() {
		c1 = GameData.Species.get(@species).id;
		c2 = (@item) ? GameData.Item.get(@item).id : "";
		c3 = (@nature) ? GameData.Nature.get(@nature).id : "";
		evlist = "";
		@ev.each do |stat|
			if (evlist != "") evlist += ",";
			evlist += stat.real_name_brief;
		}
		c4 = (@move1) ? GameData.Move.get(@move1).id : "";
		c5 = (@move2) ? GameData.Move.get(@move2).id : "";
		c6 = (@move3) ? GameData.Move.get(@move3).id : "";
		c7 = (@move4) ? GameData.Move.get(@move4).id : "";
		return $"{c1};{c2};{c3};{evlist};{c4},{c5},{c6},{c7}";
	}

	// Unused.
	public void tocompact() {
		return $"{species},{item},{nature},{move1},{move2},{move3},{move4},{ev}";
	}

//  public void _dump(depth) {
//    return [@species, @item, @nature, @move1, @move2, @move3, @move4, @ev].pack("vvCvvvvC")
//  }

//  public static void _load(str) {
//    data = str.unpack("vvCvvvvC")
//    return new self(data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7])
//  }

	public void convertMove(move) {
		if (move == moves.RETURN && GameData.Moves.exists(Moves.FRUSTRATION)) move = moves.FRUSTRATION;
		return move;
	}

	public void createPokemon(level, iv, trainer) {
		pkmn = new Pokemon(@species, level, trainer, false);
		pkmn.item = @item;
		pkmn.personalID = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
		pkmn.nature = nature;
		pkmn.happiness = 0;
		pkmn.moves.Add(new Pokemon.Move(self.convertMove(@move1)));
		if (@move2) pkmn.moves.Add(new Pokemon.Move(self.convertMove(@move2)));
		if (@move3) pkmn.moves.Add(new Pokemon.Move(self.convertMove(@move3)));
		if (@move4) pkmn.moves.Add(new Pokemon.Move(self.convertMove(@move4)));
		pkmn.moves.compact!;
		if (ev.length > 0) {
			ev.each(stat => pkmn.ev[stat] = Pokemon.EV_LIMIT / ev.length);
		}
		GameData.Stat.each_main(s => pkmn.iv[s.id] = iv);
		pkmn.calc_stats;
		return pkmn;
	}
}
