//===============================================================================
//
//===============================================================================
public partial class BattleChallenge {
	public int currentChallenge		{ get { return _currentChallenge; } }			protected int _currentChallenge;

	public const int BATTLE_TOWER_ID   = 0;
	public const int BATTLE_PALACE_ID  = 1;
	public const int BATTLE_ARENA_ID   = 2;
	public const int BATTLE_FACTORY_ID = 3;

	public void initialize() {
		@bc = new BattleChallengeData();
		@currentChallenge = -1;
		@types = new List<string>();
	}

	public void set(id, numrounds, rules) {
		@id = id;
		@numRounds = numrounds;
		@rules = rules;
		register(id, System.Text.RegularExpressions.Regex.IsMatch(id,@"double"), 3,
						System.Text.RegularExpressions.Regex.IsMatch(id,@"^factory") ? BATTLE_FACTORY_ID : BATTLE_TOWER_ID,
						System.Text.RegularExpressions.Regex.IsMatch(id,@"open$") ? 1 : 0);
		WriteCup(id, rules);
	}

	public void register(id, doublebattle, numPokemon, battletype, mode = 1) {
		ensureType(id);
		if (battletype == BATTLE_FACTORY_ID) {
			@bc.setExtraData(new BattleFactoryData(@bc));
			numPokemon = 3;
			battletype = BATTLE_TOWER_ID;
		}
		if (!@rules) @rules = modeToRules(doublebattle, numPokemon, battletype, mode);
	}

	public void rules() {
		if (!@rules) {
			@rules = modeToRules(self.data.doublebattle, self.data.numPokemon,
													self.data.battletype, self.data.mode);
		}
		return @rules;
	}

	public void modeToRules(doublebattle, numPokemon, battletype, mode) {
		rules = new PokemonChallengeRules();
		// Set the battle type
		switch (battletype) {
			case BATTLE_PALACE_ID:
				rules.setBattleType(new BattlePalace());
				break;
			case BATTLE_ARENA_ID:
				rules.setBattleType(new BattleArena());
				doublebattle = false;
				break;
			default:   // Factory works the same as Tower
				rules.setBattleType(new BattleTower());
				break;
		}
		// Set standard rules and maximum level
		switch (mode) {
			case 1:      // Open Level
				rules.setRuleset(new StandardRules(numPokemon, GameData.GrowthRate.max_level));
				rules.setLevelAdjustment(new OpenLevelAdjustment(30));
				break;
			case 2:   // Battle Tent
				rules.setRuleset(new StandardRules(numPokemon, GameData.GrowthRate.max_level));
				rules.setLevelAdjustment(new OpenLevelAdjustment(60));
				break;
			default:
				rules.setRuleset(new StandardRules(numPokemon, 50));
				rules.setLevelAdjustment(new OpenLevelAdjustment(50));
				break;
		}
		// Set whether battles are single or double
		if (doublebattle) {
			rules.addBattleRule(new DoubleBattle());
		} else {
			rules.addBattleRule(new SingleBattle());
		}
		return rules;
	}

	public void start(*args) {
		t = ensureType(@id);
		@currentChallenge = @id;   // must appear before Start
		@bc.Start(t, @numRounds);
	}

	public void Start(challenge) {}

	public void End() {
		if (@currentChallenge != -1) {
			ensureType(@currentChallenge).saveWins(@bc);
			@currentChallenge = -1;
		}
		@bc.End;
	}

	public void Battle() {
		if (@bc.extraData) return @bc.extraData.Battle(self);   // Battle Factory
		opponent = GenerateBattleTrainer(self.nextTrainer, self.rules);
		bttrainers = GetBTTrainers(@id);
		trainerdata = bttrainers[self.nextTrainer];
		opponent.lose_text = GetMessageFromHash(MessageTypes.FRONTIER_END_SPEECHES_LOSE, trainerdata[4]);
		opponent.win_text = GetMessageFromHash(MessageTypes.FRONTIER_END_SPEECHES_WIN, trainerdata[3]);
		ret = OrganizedBattleEx(opponent, self.rules);
		return ret;
	}

	public bool InChallenge() {
		return InProgress();
	}

	public bool InProgress() {
		return @bc.inProgress;
	}

	public bool Resting() {
		return @bc.resting;
	}

	public int extra        { get { return @bc.extraData;    } }
	public int decision     { get { return @bc.decision;     } }
	public int wins         { get { return @bc.wins;         } }
	public int swaps        { get { return @bc.swaps;        } }
	public int battleNumber { get { return @bc.battleNumber; } }
	public int nextTrainer  { get { return @bc.nextTrainer;  } }
	public int GoOn       { get { return @bc.GoOn;       } }
	public int AddWin     { get { return @bc.AddWin;     } }
	public int Cancel     { get { return @bc.Cancel;     } }
	public int Rest       { get { return @bc.Rest;       } }
	public bool MatchOver() { @bc.MatchOver(); }
	public int GoToStart  { get { return @bc.GoToStart;  } }

	public void setDecision(value) {
		@bc.decision = value;
	}

	public void setParty(value) {
		@bc.setParty(value);
	}

	public void data() {
		if (!InProgress() || @currentChallenge < 0) return null;
		return ensureType(@currentChallenge).clone;
	}

	public void getCurrentWins(challenge) {
		return ensureType(challenge).currentWins;
	}

	public void getPreviousWins(challenge) {
		return ensureType(challenge).previousWins;
	}

	public void getMaxWins(challenge) {
		return ensureType(challenge).maxWins;
	}

	public void getCurrentSwaps(challenge) {
		return ensureType(challenge).currentSwaps;
	}

	public void getPreviousSwaps(challenge) {
		return ensureType(challenge).previousSwaps;
	}

	public void getMaxSwaps(challenge) {
		return ensureType(challenge).maxSwaps;
	}

	//-----------------------------------------------------------------------------

	private;

	public void ensureType(id) {
		if (!@types[id]) @types[id] = new BattleChallengeType();
		return @types[id];
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattleChallengeData {
	public int battleNumber		{ get { return _battleNumber; } }			protected int _battleNumber;
	public int numRounds		{ get { return _numRounds; } }			protected int _numRounds;
	public int party		{ get { return _party; } }			protected int _party;
	public int inProgress		{ get { return _inProgress; } }			protected int _inProgress;
	public int resting		{ get { return _resting; } }			protected int _resting;
	public int wins		{ get { return _wins; } }			protected int _wins;
	public int swaps		{ get { return _swaps; } }			protected int _swaps;
	public int decision		{ get { return _decision; } set { _decision = value; } }			protected int _decision;
	public int extraData		{ get { return _extraData; } }			protected int _extraData;

	public void initialize() {
		reset;
	}

	public void setExtraData(value) {
		@extraData = value;
	}

	public void setParty(value) {
		if (@inProgress) Game.GameData.player.party = value;
		@party = value;
	}

	public void Start(t, numRounds) {
		@inProgress   = true;
		@resting      = false;
		@decision     = Battle.Outcome.UNDECIDED;
		@swaps        = t.currentSwaps;
		@wins         = t.currentWins;
		@battleNumber = 1;
		@trainers     = new List<string>();
		if (numRounds <= 0) raise _INTL("Number of rounds is 0 or less.");
		@numRounds = numRounds;
		// Get all the trainers for the next set of battles
		btTrainers = GetBTTrainers(BattleChallenge.currentChallenge);
		while (@trainers.length < @numRounds) {
			newtrainer = BattleChallengeTrainer(@wins + @trainers.length, btTrainers);
			found = false;
			@trainers.each do |tr|
				if (tr == newtrainer) found = true;
			}
			if (!found) @trainers.Add(newtrainer);
		}
		@start = new {Game.GameData.game_map.map_id, Game.GameData.game_player.x, Game.GameData.game_player.y};
		@oldParty = Game.GameData.player.party;
		if (@party) Game.GameData.player.party = @party;
		Game.save(safe: true);
	}

	public void GoToStart() {
		if (Game.GameData.scene.is_a(Scene_Map)) {
			Game.GameData.game_temp.player_transferring  = true;
			Game.GameData.game_temp.player_new_map_id    = @start[0];
			Game.GameData.game_temp.player_new_x         = @start[1];
			Game.GameData.game_temp.player_new_y         = @start[2];
			Game.GameData.game_temp.player_new_direction = 8;
			DismountBike;
			Game.GameData.scene.transfer_player;
		}
	}

	public void AddWin() {
		if (!@inProgress) return;
		@battleNumber += 1;
		@wins += 1;
	}

	public void AddSwap() {
		if (@inProgress) @swaps += 1;
	}

	public bool MatchOver() {
		if (!@inProgress || @decision != Battle.Outcome.UNDECIDED) return true;
		return @battleNumber > @numRounds;
	}

	public void Rest() {
		if (!@inProgress) return;
		@resting = true;
		SaveInProgress;
	}

	public void GoOn() {
		if (!@inProgress) return;
		@resting = false;
		SaveInProgress;
	}

	public void Cancel() {
		if (@oldParty) Game.GameData.player.party = @oldParty;
		reset;
	}

	public void End() {
		Game.GameData.player.party = @oldParty;
		if (!@inProgress) return;
		save = (@decision != Battle.Outcome.UNDECIDED);
		reset;
		Game.GameData.game_map.need_refresh = true;
		if (save) Game.save(safe: true);
	}

	public void nextTrainer() {
		return @trainers[@battleNumber - 1];
	}

	//-----------------------------------------------------------------------------

	private;

	public void reset() {
		@inProgress   = false;
		@resting      = false;
		@start        = null;
		@decision     = Battle.Outcome.UNDECIDED;
		@wins         = 0;
		@swaps        = 0;
		@battleNumber = 0;
		@trainers     = new List<string>();
		@oldParty     = null;
		@party        = null;
		@extraData    = null;
	}

	public void SaveInProgress() {
		oldmapid     = Game.GameData.game_map.map_id;
		oldx         = Game.GameData.game_player.x;
		oldy         = Game.GameData.game_player.y;
		olddirection = Game.GameData.game_player.direction;
		Game.GameData.game_map.map_id = @start[0];
		Game.GameData.game_player.moveto2(@start[1], @start[2]);
		Game.GameData.game_player.direction = 8;   // facing up
		Game.save(safe: true);
		Game.GameData.game_map.map_id = oldmapid;
		Game.GameData.game_player.moveto2(oldx, oldy);
		Game.GameData.game_player.direction = olddirection;
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattleChallengeType {
	public int currentWins		{ get { return _currentWins; } set { _currentWins = value; } }			protected int _currentWins;
	public int previousWins		{ get { return _previousWins; } set { _previousWins = value; } }			protected int _previousWins;
	public int maxWins		{ get { return _maxWins; } set { _maxWins = value; } }			protected int _maxWins;
	public int currentSwaps		{ get { return _currentSwaps; } set { _currentSwaps = value; } }			protected int _currentSwaps;
	public int previousSwaps		{ get { return _previousSwaps; } set { _previousSwaps = value; } }			protected int _previousSwaps;
	public int maxSwaps		{ get { return _maxSwaps; } set { _maxSwaps = value; } }			protected int _maxSwaps;
	public int doublebattle		{ get { return _doublebattle; } }			protected int _doublebattle;
	public int numPokemon		{ get { return _numPokemon; } }			protected int _numPokemon;
	public int battletype		{ get { return _battletype; } }			protected int _battletype;
	public int mode		{ get { return _mode; } }			protected int _mode;

	public void initialize() {
		@previousWins  = 0;
		@maxWins       = 0;
		@currentWins   = 0;
		@currentSwaps  = 0;
		@previousSwaps = 0;
		@maxSwaps      = 0;
	}

	public void saveWins(challenge) {
		if (challenge.decision == Battle.Outcome.UNDECIDED) {
			@currentWins  = 0;
			@currentSwaps = 0;
		} else {
			if (challenge.decision == Battle.Outcome.WIN) {
				@currentWins  = challenge.wins;
				@currentSwaps = challenge.swaps;
			} else {                       // if lost
				@currentWins  = 0;
				@currentSwaps = 0;
			}
			@maxWins       = (int)Math.Max(@maxWins, challenge.wins);
			@previousWins  = challenge.wins;
			@maxSwaps      = (int)Math.Max(@maxSwaps, challenge.swaps);
			@previousSwaps = challenge.swaps;
		}
	}
}

//===============================================================================
// Battle Factory data
//===============================================================================
public partial class BattleFactoryData {
	public void initialize(bcdata) {
		@bcdata = bcdata;
	}

	public void PrepareRentals() {
		@rentals = BattleFactoryPokemon(BattleChallenge.rules, @bcdata.wins, @bcdata.swaps, new List<string>());
		@trainerid = @bcdata.nextTrainer;
		bttrainers = GetBTTrainers(BattleChallenge.currentChallenge);
		trainerdata = bttrainers[@trainerid];
		@opponent = new NPCTrainer(
			GetMessageFromHash(MessageTypes.TRAINER_NAMES, trainerdata[1]),
			trainerdata[0]
		);
		@opponent.lose_text = GetMessageFromHash(MessageTypes.FRONTIER_END_SPEECHES_LOSE, trainerdata[4]);
		@opponent.win_text = GetMessageFromHash(MessageTypes.FRONTIER_END_SPEECHES_WIN, trainerdata[3]);
		opponentPkmn = BattleFactoryPokemon(BattleChallenge.rules, @bcdata.wins, @bcdata.swaps, @rentals);
		@opponent.party = opponentPkmn.sample(3);
	}

	public void ChooseRentals() {
		FadeOutIn do;
			scene = new BattleSwapScene();
			screen = new BattleSwapScreen(scene);
			@rentals = screen.StartRent(@rentals);
			@bcdata.AddSwap;
			@bcdata.setParty(@rentals);
		}
	}

	public void PrepareSwaps() {
		@oldopponent = @opponent.party;
		trainerid = @bcdata.nextTrainer;
		bttrainers = GetBTTrainers(BattleChallenge.currentChallenge);
		trainerdata = bttrainers[trainerid];
		@opponent = new NPCTrainer(
			GetMessageFromHash(MessageTypes.TRAINER_NAMES, trainerdata[1]),
			trainerdata[0]
		);
		@opponent.lose_text = GetMessageFromHash(MessageTypes.FRONTIER_END_SPEECHES_LOSE, trainerdata[4]);
		@opponent.win_text = GetMessageFromHash(MessageTypes.FRONTIER_END_SPEECHES_WIN, trainerdata[3]);
		opponentPkmn = BattleFactoryPokemon(BattleChallenge.rules, @bcdata.wins, @bcdata.swaps,
																					[].concat(@rentals).concat(@oldopponent));
		@opponent.party = opponentPkmn.sample(3);
	}

	public void ChooseSwaps() {
		swapMade = true;
		FadeOutIn do;
			scene = new BattleSwapScene();
			screen = new BattleSwapScreen(scene);
			swapMade = screen.StartSwap(@rentals, @oldopponent);
			if (swapMade) @bcdata.AddSwap;
			@bcdata.setParty(@rentals);
		}
		return swapMade;
	}

	public void Battle(challenge) {
		return OrganizedBattleEx(@opponent, challenge.rules);
	}
}
