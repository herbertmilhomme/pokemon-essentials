//===============================================================================
//
//===============================================================================
public partial class SafariState {
	public int ballcount		{ get { return _ballcount; } set { _ballcount = value; } }			protected int _ballcount;
	public int captures		{ get { return _captures; } set { _captures = value; } }			protected int _captures;
	public int decision		{ get { return _decision; } set { _decision = value; } }			protected int _decision;
	public int steps		{ get { return _steps; } set { _steps = value; } }			protected int _steps;

	public void initialize() {
		@start      = null;
		@ballcount  = 0;
		@captures   = 0;
		@inProgress = false;
		@steps      = 0;
		@decision   = 0;
	}

	public void ReceptionMap() {
		return @inProgress ? @start[0] : 0;
	}

	public bool inProgress() {
		return @inProgress;
	}

	public void GoToStart() {
		if (Game.GameData.scene.is_a(Scene_Map)) {
			FadeOutIn do;
				Game.GameData.game_temp.player_transferring   = true;
				Game.GameData.game_temp.transition_processing = true;
				Game.GameData.game_temp.player_new_map_id    = @start[0];
				Game.GameData.game_temp.player_new_x         = @start[1];
				Game.GameData.game_temp.player_new_y         = @start[2];
				Game.GameData.game_temp.player_new_direction = 2;
				DismountBike;
				Game.GameData.scene.transfer_player;
			}
		}
	}

	public void Start(ballcount) {
		@start      = new {Game.GameData.game_map.map_id, Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction};
		@ballcount  = ballcount;
		@inProgress = true;
		@steps      = Settings.SAFARI_STEPS;
	}

	public void End() {
		@start      = null;
		@ballcount  = 0;
		@captures   = 0;
		@inProgress = false;
		@steps      = 0;
		@decision   = 0;
		Game.GameData.game_map.need_refresh = true;
	}
}

//===============================================================================
//
//===============================================================================
public bool InSafari() {
	if (SafariState.inProgress()) {
		// Reception map is handled separately from safari map since the reception
		// map can be outdoors, with its own grassy patches.
		reception = SafariState.ReceptionMap;
		if (Game.GameData.game_map.map_id == reception) return true;
		if (Game.GameData.game_map.metadata&.safari_map) return true;
	}
	return false;
}

public void SafariState() {
	if (!Game.GameData.PokemonGlobal.safariState) Game.GameData.PokemonGlobal.safariState = new SafariState();
	return Game.GameData.PokemonGlobal.safariState;
}

//===============================================================================
//
//===============================================================================

EventHandlers.add(:on_enter_map, :end_safari_game,
	block: (_old_map_id) => {
		if (!InSafari()) SafariState.End;
	}
)

EventHandlers.add(:on_player_step_taken_can_transfer, :safari_game_counter,
	block: (handled) => {
		// handled is an array: [null]. If [true], a transfer has happened because of
		// this event, so don't do anything that might cause another one
		if (handled[0]) continue;
		if (Settings.SAFARI_STEPS == 0 || !InSafari() || SafariState.decision != 0) continue;
		SafariState.steps -= 1;
		if (SafariState.steps > 0) continue;
		Message("\\se[Safari Zone end]" + _INTL("PA: Ding-dong!") + "\1");
		Message(_INTL("PA: Your safari game is over!"));
		SafariState.decision = 1;
		SafariState.GoToStart;
		handled[0] = true;
	}
)

//===============================================================================
//
//===============================================================================

EventHandlers.add(:on_calling_wild_battle, :safari_battle,
	block: (pkmn, handled) => {
		// handled is an array: [null]. If [true] or [false], the battle has already
		// been overridden (the boolean is its outcome), so don't do anything that
		// would override it again
		if (!handled[0].null()) continue;
		if (!InSafari()) continue;
		handled[0] = SafariBattle(pkmn);
	}
)

public void SafariBattle(pkmn, level = 1) {
	// Generate a wild Pokémon based on the species and level
	if (!pkmn.is_a(Pokemon)) pkmn = GenerateWildPokemon(pkmn, level);
	foeParty = [pkmn];
	// Calculate who the trainer is
	playerTrainer = Game.GameData.player;
	// Create the battle scene (the visual side of it)
	scene = BattleCreationHelperMethods.create_battle_scene;
	// Create the battle class (the mechanics side of it)
	battle = new SafariBattle(scene, playerTrainer, foeParty);
	battle.ballCount = SafariState.ballcount;
	BattleCreationHelperMethods.prepare_battle(battle);
	// Perform the battle itself
	outcome = Battle.Outcome.UNDECIDED;
	BattleAnimation(GetWildBattleBGM(foeParty), 0, foeParty) do;
		SceneStandby { outcome = battle.StartBattle };
	}
	Input.update;
	// Update Safari game data based on result of battle
	SafariState.ballcount = battle.ballCount;
	if (SafariState.ballcount <= 0) {
		if (outcome != Battle.Outcome.LOSE) {   // Last Safari Ball was used to catch the wild Pokémon
			Message(_INTL("Announcer: You're out of Safari Balls! Game over!"));
		}
		SafariState.decision = 1;
		SafariState.GoToStart;
	}
	// Save the result of the battle in Game Variable 1
	//    0 - Undecided or aborted
	//    2 - Player ran out of Safari Balls
	//    3 - Player or wild Pokémon ran from battle, or player forfeited the match
	//    4 - Wild Pokémon was caught
	if (outcome == Battle.Outcome.CATCH) {
		Game.GameData.stats.safari_pokemon_caught += 1;
		SafariState.captures += 1;
		Game.GameData.stats.most_captures_per_safari_game = (int)Math.Max(Game.GameData.stats.most_captures_per_safari_game, SafariState.captures);
	}
	Set(1, outcome);
	// Used by the Poké Radar to update/break the chain
	EventHandlers.trigger(:on_wild_battle_end, pkmn.species_data.id, pkmn.level, outcome);
	// Return the outcome of the battle
	return outcome;
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPauseMenu {
	unless (method_defined(:__safari_pbShowInfo)) alias __safari_pbShowInfo ShowInfo;

	public void ShowInfo() {
		__safari_pbShowInfo;
		if (!InSafari()) return;
		if (Settings.SAFARI_STEPS <= 0) {
			@scene.ShowInfo(_INTL("Balls: {1}", SafariState.ballcount));
		} else {
			@scene.ShowInfo(_INTL("Steps: {1}/{2}\nBalls: {3}",
															SafariState.steps, Settings.SAFARI_STEPS, SafariState.ballcount));
		}
	}
}

//===============================================================================
//
//===============================================================================

MenuHandlers.add(:pause_menu, :quit_safari_game, {
	"name"      => _INTL("Quit"),
	"order"     => 60,
	"condition" => () => { next InSafari() },
	"effect"    => menu => {
		menu.HideMenu;
		if (ConfirmMessage(_INTL("Would you like to leave the Safari Game right now?"))) {
			menu.EndScene;
			SafariState.decision = 1;
			SafariState.GoToStart;
			next true;
		}
		menu.Refresh;
		menu.ShowMenu;
		next false;
	}
});
