//===============================================================================
//
//===============================================================================
public partial class BattleType {
	public void CreateBattle(scene, trainer1, trainer2) {
		return new Battle(scene, trainer1.party, trainer2.party, trainer1, trainer2);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattleTower : BattleType {
	public void CreateBattle(scene, trainer1, trainer2) {
		return new RecordedBattle(scene, trainer1.party, trainer2.party, trainer1, trainer2);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattlePalace : BattleType {
	public void CreateBattle(scene, trainer1, trainer2) {
		return new RecordedBattle.BattlePalaceBattle(scene, trainer1.party, trainer2.party, trainer1, trainer2);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattleArena : BattleType {
	public void CreateBattle(scene, trainer1, trainer2) {
		return new RecordedBattle.BattleArenaBattle(scene, trainer1.party, trainer2.party, trainer1, trainer2);
	}
}

//===============================================================================
//
//===============================================================================
public void OrganizedBattleEx(opponent, challengedata) {
	// Skip battle if holding Ctrl in Debug mode
	if (Input.press(Input.CTRL) && Core.DEBUG) {
		Message(_INTL("SKIPPING BATTLE..."));
		Message(_INTL("AFTER WINNING..."));
		Message(opponent.lose_text || "...");
		Game.GameData.game_temp.last_battle_record = null;
		MEStop;
		return true;
	}
	Game.GameData.player.heal_party;
	// Remember original data, to be restored after battle
	if (!challengedata) challengedata = new PokemonChallengeRules();
	oldlevels = challengedata.adjustLevels(Game.GameData.player.party, opponent.party);
	olditems  = Game.GameData.player.party.transform(p => p.item_id);
	olditems2 = opponent.party.transform(p => p.item_id);
	// Create the battle scene (the visual side of it)
	scene = BattleCreationHelperMethods.create_battle_scene;
	// Create the battle class (the mechanics side of it)
	battle = challengedata.createBattle(scene, Game.GameData.player, opponent);
	battle.internalBattle = false;
	// Set various other properties in the battle class
	BattleCreationHelperMethods.prepare_battle(battle);
	// Perform the battle itself
	outcome = Battle.Outcome.UNDECIDED;
	BattleAnimation(GetTrainerBattleBGM(opponent)) do;
		SceneStandby { outcome = battle.StartBattle };
	}
	Input.update;
	// Restore both parties to their original levels
	challengedata.unadjustLevels(Game.GameData.player.party, opponent.party, oldlevels);
	// Heal both parties and restore their original items
	Game.GameData.player.party.each_with_index do |pkmn, i|
		pkmn.heal;
		pkmn.makeUnmega;
		pkmn.makeUnprimal;
		pkmn.item = olditems[i];
	}
	opponent.party.each_with_index do |pkmn, i|
		pkmn.heal;
		pkmn.makeUnmega;
		pkmn.makeUnprimal;
		pkmn.item = olditems2[i];
	}
	// Save the record of the battle
	Game.GameData.game_temp.last_battle_record = null;
	if (new []{Battle.Outcome.WIN, Battle.Outcome.LOSE, Battle.Outcome.DRAW}.Contains(outcome)) {
		Game.GameData.game_temp.last_battle_record = battle.DumpRecord;
	}
	switch (outcome) {
		case Battle.Outcome.WIN:   // Won
			Game.GameData.stats.trainer_battles_won += 1;
			break;
		case Battle.Outcome.LOSE: case Battle.Outcome.FLEE: case Battle.Outcome.DRAW:
			Game.GameData.stats.trainer_battles_lost += 1;
			break;
	}
	// Return true if the player won the battle, and false if any other result
	return (outcome == Battle.Outcome.WIN);
}

//===============================================================================
// Methods that record and play back a battle.
//===============================================================================
public void RecordLastBattle() {
	Game.GameData.PokemonGlobal.lastbattle = Game.GameData.game_temp.last_battle_record;
	Game.GameData.game_temp.last_battle_record = null;
}

public void PlayLastBattle() {
	PlayBattle(Game.GameData.PokemonGlobal.lastbattle);
}

public void PlayBattle(battledata) {
	if (!battledata) return;
	scene = BattleCreationHelperMethods.create_battle_scene;
	scene.abortable = true;
	lastbattle = Marshal.restore(battledata);
	switch (lastbattle[0]) {
		case BattleChallenge.BATTLE_TOWER_ID:
			battleplayer = new RecordedBattle.PlaybackBattle(scene, lastbattle);
			break;
		case BattleChallenge.BATTLE_PALACE_ID:
			battleplayer = new RecordedBattle.BattlePalacePlaybackBattle(scene, lastbattle);
			break;
		case BattleChallenge.BATTLE_ARENA_ID:
			battleplayer = new RecordedBattle.BattleArenaPlaybackBattle(scene, lastbattle);
			break;
	}
	bgm = RecordedBattle.PlaybackHelper.GetBattleBGM(lastbattle);
	BattleAnimation(bgm) do;
		SceneStandby { battleplayer.StartBattle };
	}
}

//===============================================================================
// Debug playback methods.
//===============================================================================
public void DebugPlayBattle() {
	params = new ChooseNumberParams();
	params.setRange(0, 500);
	params.setInitialValue(0);
	params.setCancelValue(-1);
	num = MessageChooseNumber(_INTL("Choose a battle."), params);
	if (num >= 0) PlayBattleFromFile(string.Format("Battles/Battle{0:3}.dat", num));
}

public void PlayBattleFromFile(filename) {
	RgssOpen(filename, "rb", f => { PlayBattle(f.read); });
}
