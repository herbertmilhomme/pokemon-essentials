//===============================================================================
//
//===============================================================================
public partial class BugContestState {
	public int ballcount		{ get { return _ballcount; } set { _ballcount = value; } }			protected int _ballcount;
	public int decision		{ get { return _decision; } set { _decision = value; } }			protected int _decision;
	public int lastPokemon		{ get { return _lastPokemon; } set { _lastPokemon = value; } }			protected int _lastPokemon;
	public int timer_start		{ get { return _timer_start; } set { _timer_start = value; } }			protected int _timer_start;

	CONTESTANT_NAMES = new {
		_INTL("Bug Catcher Ed"),
		_INTL("Bug Catcher Benny"),
		_INTL("Bug Catcher Josh"),
		_INTL("Camper Barry"),
		_INTL("Cool Trainer Nick"),
		_INTL("Lass Abby"),
		_INTL("Picnicker Cindy"),
		_INTL("Youngster Samuel");
	}
	TIME_ALLOWED = Settings.BUG_CONTEST_TIME;   // In seconds

	public void initialize() {
		clear;
		@lastContest = null;
	}

	// Returns whether the last contest ended less than 24 hours ago.
	public bool ContestHeld() {
		if (!@lastContest) return false;
		return GetTimeNow.ToInt() - @lastContest < 24 * 60 * 60;   // 24 hours
	}

	public bool expired() {
		if (!undecided()) return false;
		if (TIME_ALLOWED <= 0) return false;
		return System.uptime - timer_start >= TIME_ALLOWED;
	}

	public void clear() {
		@ballcount    = 0;
		@ended        = false;
		@inProgress   = false;
		@decision     = 0;
		@lastPokemon  = null;
		@otherparty   = new List<string>();
		@contestants  = new List<string>();
		@places       = new List<string>();
		@start        = null;
		@contestMaps  = new List<string>();
		@reception    = new List<string>();
	}

	public bool inProgress() {
		return @inProgress;
	}

	public bool undecided() {
		return (@inProgress && @decision == 0);
	}

	public bool decided() {
		return (@inProgress && @decision != 0) || @ended;
	}

	public void SetPokemon(chosenpoke) {
		@chosenPokemon = chosenpoke;
	}

	public void SetContestMap(*maps) {
		foreach (var map in maps) { //'maps.each' do => |map|
			if (map.is_a(String)) {   // Map metadata flag
				foreach (var map_data in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |map_data|
					if (map_data.has_flag(map)) @contestMaps.Add(map_data.id);
				}
			} else {
				@contestMaps.Add(map);
			}
		}
	}

	// Reception map is handled separately from contest map since the reception map
	// can be outdoors, with its own grassy patches.
	public void SetReception(*maps) {
		foreach (var map in maps) { //'maps.each' do => |map|
			if (map.is_a(String)) {   // Map metadata flag
				foreach (var map_data in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |map_data|
					if (map_data.has_flag(map)) @reception.Add(map_data.id);
				}
			} else {
				@reception.Add(map);
			}
		}
	}

	public bool OffLimits(map) {
		if (@contestMaps.Contains(map)) return false;
		if (@reception.Contains(map)) return false;
		return true;
	}

	public void SetJudgingPoint(startMap, startX, startY, dir = 8) {
		@start = new {startMap, startX, startY, dir};
	}

	public void Judge() {
		judgearray = new List<string>();
		if (@lastPokemon) {
			judgearray.Add(new {-1, @lastPokemon.species, BugContestScore(@lastPokemon)});
		}
		maps_with_encounters = new List<string>();
		@contestMaps.each do |map|
			enc_type = types.BugContest;
			if (!Game.GameData.PokemonEncounters.map_has_encounter_type(map, enc_type)) enc_type = types.Land;
			if (Game.GameData.PokemonEncounters.map_has_encounter_type(map, enc_type)) {
				maps_with_encounters.Add(new {map, enc_type});
			}
		}
		if (maps_with_encounters.empty()) raise _INTL("There are no Bug Contest/Land encounters for any Bug Contest maps.");
		@contestants.each do |cont|
			enc_data = maps_with_encounters.sample;
			enc = Game.GameData.PokemonEncounters.choose_wild_pokemon_for_map(enc_data[0], enc_data[1]);
			if (!enc) raise _INTL("No encounters for map {1} somehow, so can't judge contest.", enc_data[0]);
			pokemon = new Pokemon(enc[0], enc[1]);
			pokemon.hp = rand(1...pokemon.totalhp);
			score = BugContestScore(pokemon);
			judgearray.Add(new {cont, pokemon.species, score});
		}
		if (judgearray.length < 3) raise _INTL("Too few bug-catching contestants");
		judgearray.sort! { |a, b| b[2] <=> a[2] };   // sort by score in descending order
		@places.Add(judgearray[0]);
		@places.Add(judgearray[1]);
		@places.Add(judgearray[2]);
	}

	public void GetPlaceInfo(place) {
		cont = @places[place][0];
		if (cont < 0) {
			Game.GameData.game_variables[1] = Game.GameData.player.name;
		} else {
			Game.GameData.game_variables[1] = CONTESTANT_NAMES[cont];
		}
		Game.GameData.game_variables[2] = GameData.Species.get(@places[place][1]).name;
		Game.GameData.game_variables[3] = @places[place][2];
	}

	public void ClearIfEnded() {
		if (!@inProgress && (!@start || @start[0] != Game.GameData.game_map.map_id)) clear;
	}

	public void StartJudging() {
		@decision = 1;
		Judge;
		if (Game.GameData.scene.is_a(Scene_Map)) {
			FadeOutIn do;
				Game.GameData.game_temp.player_transferring  = true;
				Game.GameData.game_temp.player_new_map_id    = @start[0];
				Game.GameData.game_temp.player_new_x         = @start[1];
				Game.GameData.game_temp.player_new_y         = @start[2];
				Game.GameData.game_temp.player_new_direction = @start[3];
				DismountBike;
				Game.GameData.scene.transfer_player;
				Game.GameData.game_map.need_refresh = true;   // in case player moves to the same map
			}
		}
	}

	public bool IsContestant(i) {
		return @contestants.any(item => i == item);
	}

	public void Start(ballcount) {
		@ballcount = ballcount;
		@inProgress = true;
		@otherparty = new List<string>();
		@lastPokemon = null;
		@lastContest = null;
		@timer_start = System.uptime;
		@places = new List<string>();
		chosenpkmn = Game.GameData.player.party[@chosenPokemon];
		for (int i = Game.GameData.player.party.length; i < Game.GameData.player.party.length; i++) { //for 'Game.GameData.player.party.length' times do => |i|
			if (i != @chosenPokemon) @otherparty.Add(Game.GameData.player.party[i]);
		}
		@contestants = new List<string>();
		(int)Math.Min(5, CONTESTANT_NAMES.length).times do
			do { //loop; while (true);
				value = rand(CONTESTANT_NAMES.length);
				if (@contestants.Contains(value)) continue;
				@contestants.Add(value);
				break;
			}
		}
		Game.GameData.player.party = [chosenpkmn];
		@decision = 0;
		@ended = false;
		Game.GameData.stats.bug_contest_count += 1;
	}

	public void place() {
		for (int i = 3; i < 3; i++) { //for '3' times do => |i|
			if (@places[i][0] < 0) return i;
		}
		return 3;
	}

	public void End(interrupted = false) {
		if (!@inProgress) return;
		@otherparty.each(pkmn => Game.GameData.player.party.Add(pkmn));
		if (interrupted) {
			@ended = false;
		} else {
			if (@lastPokemon) NicknameAndStore(@lastPokemon);
			@ended = true;
		}
		if (place == 0) Game.GameData.stats.bug_contest_wins += 1;
		@ballcount = 0;
		@inProgress = false;
		@decision = 0;
		@lastPokemon = null;
		@otherparty = new List<string>();
		@contestMaps = new List<string>();
		@reception = new List<string>();
		@lastContest = GetTimeNow.ToInt();
		Game.GameData.game_map.need_refresh = true;
	}
}

//===============================================================================
//
//===============================================================================
public partial class TimerDisplay  {// :nodoc:
	public int start_time		{ get { return _start_time; } set { _start_time = value; } }			protected int _start_time;

	public void initialize(start_time, max_time) {
		@timer = Window_AdvancedTextPokemon.newWithSize("", Graphics.width - 120, 0, 120, 64);
		@timer.z = 99999;
		@start_time = start_time;
		@max_time = max_time;
		@display_time = null;
	}

	public void dispose() {
		@timer.dispose;
	}

	public bool disposed() {
		@timer.disposed();
	}

	public void update() {
		time_left = @max_time - (System.uptime - @start_time).ToInt();
		if (time_left < 0) time_left = 0;
		if (@display_time != time_left) {
			@display_time = time_left;
			min = @display_time / 60;
			sec = @display_time % 60;
			@timer.text = string.Format("<ac>{1:02d}:{2:02d}", min, sec);
		}
	}
}

//===============================================================================
//
//===============================================================================
// Returns a score for this Pokemon in the Bug-Catching Contest.
// Not exactly the HGSS calculation, but it should be decent enough.
public void BugContestScore(pkmn) {
	levelscore = pkmn.level * 4;
	ivscore = 0;
	pkmn.iv.each_value(iv => ivscore += iv.to_f / Pokemon.IV_STAT_LIMIT);
	ivscore = (int)Math.Floor(ivscore * 100);
	hpscore = (int)Math.Floor(100.0 * pkmn.hp / pkmn.totalhp);
	catch_rate = pkmn.species_data.catch_rate;
	rarescore = 60;
	if (catch_rate <= 120) rarescore += 20;
	if (catch_rate <= 60) rarescore += 20;
	return levelscore + ivscore + hpscore + rarescore;
}

public void BugContestState() {
	if (!Game.GameData.PokemonGlobal.bugContestState) {
		Game.GameData.PokemonGlobal.bugContestState = new BugContestState();
	}
	return Game.GameData.PokemonGlobal.bugContestState;
}

// Returns true if the Bug-Catching Contest in progress
public bool InBugContest() {
	return BugContestState.inProgress();
}

// Returns true if the Bug-Catching Contest in progress and has not yet been judged
public bool BugContestUndecided() {
	return BugContestState.undecided();
}

// Returns true if the Bug-Catching Contest in progress and is being judged
public bool BugContestDecided() {
	return BugContestState.decided();
}

public void BugContestStartOver() {
	foreach (var pkmn in Game.GameData.player.party) { //'Game.GameData.player.party.each' do => |pkmn|
		pkmn.heal;
		pkmn.makeUnmega;
		pkmn.makeUnprimal;
	}
	BugContestState.StartJudging;
}

//===============================================================================
//
//===============================================================================

EventHandlers.add(:on_map_or_spriteset_change, :show_bug_contest_timer,
	block: (scene, _map_changed) => {
		if (!InBugContest() || BugContestState.decision != 0 || BugContestState.TIME_ALLOWED == 0) continue;
		scene.spriteset.addUserSprite(
			new TimerDisplay(BugContestState.timer_start, BugContestState.TIME_ALLOWED)
		);
	}
)

EventHandlers.add(:on_frame_update, :bug_contest_counter,
	block: () => {
		if (!BugContestState.expired()) continue;
		if (Game.GameData.game_player.move_route_forcing || MapInterpreterRunning() ||
						Game.GameData.game_temp.message_window_showing) continue;
		Message(_INTL("ANNOUNCER: BEEEEEP!"));
		Message(_INTL("Time's up!"));
		BugContestState.StartJudging;
	}
)

EventHandlers.add(:on_enter_map, :end_bug_contest,
	block: (_old_map_id) => {
		BugContestState.ClearIfEnded;
	}
)

EventHandlers.add(:on_leave_map, :end_bug_contest,
	block: (new_map_id, new_map) => {
		if (!InBugContest() || !BugContestState.OffLimits(new_map_id)) continue;
		// Clear bug contest if player flies/warps/teleports out of the contest
		BugContestState.End(true);
	}
)

//===============================================================================
//
//===============================================================================

EventHandlers.add(:on_calling_wild_battle, :bug_contest_battle,
	block: (pkmn, handled) => {
		// handled is an array: [null]. If [true] or [false], the battle has already
		// been overridden (the boolean is its outcome), so don't do anything that
		// would override it again
		if (!handled[0].null()) continue;
		if (!InBugContest()) continue;
		handled[0] = BugContestBattle(pkmn);
	}
)

public void BugContestBattle(pkmn, level = 1) {
	// Record information about party Pokémon to be used at the end of battle (e.g.
	// comparing levels for an evolution check)
	EventHandlers.trigger(:on_start_battle);
	// Generate a wild Pokémon based on the species and level
	if (!pkmn.is_a(Pokemon)) pkmn = GenerateWildPokemon(pkmn, level);
	foeParty = [pkmn];
	// Calculate who the trainers and their party are
	playerTrainer     = [Game.GameData.player];
	playerParty       = Game.GameData.player.party;
	playerPartyStarts = [0];
	// Create the battle scene (the visual side of it)
	scene = BattleCreationHelperMethods.create_battle_scene;
	// Create the battle class (the mechanics side of it)
	battle = new BugContestBattle(scene, playerParty, foeParty, playerTrainer, null);
	battle.party1starts = playerPartyStarts;
	battle.ballCount    = BugContestState.ballcount;
	setBattleRule("single");
	BattleCreationHelperMethods.prepare_battle(battle);
	// Perform the battle itself
	outcome = Battle.Outcome.UNDECIDED;
	BattleAnimation(GetWildBattleBGM(foeParty), 0, foeParty) do;
		outcome = battle.StartBattle;
		BattleCreationHelperMethods.after_battle(outcome, true);
		if (Battle.Outcome.should_black_out(outcome)) {
			Game.GameData.game_system.bgm_unpause;
			Game.GameData.game_system.bgs_unpause;
			BugContestStartOver;
		}
	}
	Input.update;
	// Update Bug Contest game data based on result of battle
	BugContestState.ballcount = battle.ballCount;
	if (BugContestState.ballcount == 0) {
		Message(_INTL("ANNOUNCER: The Bug-Catching Contest is over!"));
		BugContestState.StartJudging;
	}
	// Save the result of the battle in Game Variable 1
	BattleCreationHelperMethods.set_outcome(outcome, 1);
	// Used by the Poké Radar to update/break the chain
	EventHandlers.trigger(:on_wild_battle_end, pkmn.species_data.id, pkmn.level, outcome);
	// Return false if the player lost or drew the battle, and true if any other result
	return !Battle.Outcome.should_black_out(outcome);
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPauseMenu {
	unless (method_defined(:__bug_contest_pbShowInfo)) alias __bug_contest_pbShowInfo ShowInfo;

	public void ShowInfo() {
		__bug_contest_pbShowInfo;
		if (!InBugContest()) return;
		if (BugContestState.lastPokemon) {
			@scene.ShowInfo(_INTL("Caught: {1}\nLevel: {2}\nBalls: {3}",
															BugContestState.lastPokemon.speciesName,
															BugContestState.lastPokemon.level,
															BugContestState.ballcount));
		} else {
			@scene.ShowInfo(_INTL("Caught: None\nBalls: {1}", BugContestState.ballcount));
		}
	}
}

//===============================================================================
//
//===============================================================================

MenuHandlers.add(:pause_menu, :quit_bug_contest, {
	"name"      => _INTL("Quit Contest"),
	"order"     => 60,
	"condition" => () => { next InBugContest() },
	"effect"    => menu => {
		menu.HideMenu;
		if (ConfirmMessage(_INTL("Would you like to end the Contest now?"))) {
			menu.EndScene;
			BugContestState.StartJudging;
			next true;
		}
		menu.Refresh;
		menu.ShowMenu;
		next false;
	}
});
