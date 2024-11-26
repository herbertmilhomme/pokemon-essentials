//===============================================================================
// Load various wild battle music.
//===============================================================================
// wildParty is an array of Pokémon objects.
public void GetWildBattleBGM(_wildParty) {
	if (Game.GameData.PokemonGlobal.nextBattleBGM) return Game.GameData.PokemonGlobal.nextBattleBGM.clone;
	ret = null;
	if (!ret) {
		// Check map metadata
		music = Game.GameData.game_map.metadata&.wild_battle_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) {
		// Check global metadata
		music = GameData.Metadata.get.wild_battle_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) ret = StringToAudioFile("Battle wild");
	return ret;
}

public void GetWildVictoryBGM() {
	if (Game.GameData.PokemonGlobal.nextBattleVictoryBGM) {
		return Game.GameData.PokemonGlobal.nextBattleVictoryBGM.clone;
	}
	ret = null;
	// Check map metadata
	music = Game.GameData.game_map.metadata&.wild_victory_BGM;
	if (music && music != "") ret = StringToAudioFile(music);
	if (!ret) {
		// Check global metadata
		music = GameData.Metadata.get.wild_victory_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) ret = StringToAudioFile("Battle victory");
	ret.name = "../../Audio/BGM/" + ret.name;
	return ret;
}

public void GetWildCaptureME() {
	if (Game.GameData.PokemonGlobal.nextBattleCaptureME) {
		return Game.GameData.PokemonGlobal.nextBattleCaptureME.clone;
	}
	ret = null;
	if (!ret) {
		// Check map metadata
		music = Game.GameData.game_map.metadata&.wild_capture_ME;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) {
		// Check global metadata
		music = GameData.Metadata.get.wild_capture_ME;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) ret = StringToAudioFile("Battle capture success");
	ret.name = "../../Audio/ME/" + ret.name;
	return ret;
}

//===============================================================================
// Load/play various trainer battle music.
//===============================================================================
public void PlayTrainerIntroBGM(trainer_type) {
	trainer_type_data = GameData.TrainerType.get(trainer_type);
	if (nil_or_empty(trainer_type_data.intro_BGM)) return;
	bgm = StringToAudioFile(trainer_type_data.intro_BGM);
	if (!Game.GameData.game_temp.memorized_bgm) {
		if (Game.GameData.game_temp.cue_bgm_delay) {
			Game.GameData.game_temp.cue_bgm_delay = null;
			Game.GameData.game_temp.memorized_bgm = Game.GameData.game_temp.cue_bgm;
			Game.GameData.game_temp.memorized_bgm_position = 0;
		} else {
			Game.GameData.game_temp.memorized_bgm = Game.GameData.game_system.getPlayingBGM;
			Game.GameData.game_temp.memorized_bgm_position = (Audio.bgm_pos rescue 0);
		}
	}
	BGMPlay(bgm);
}

// Can be a Player, NPCTrainer or an array of them.
public void GetTrainerBattleBGM(trainer) {
	if (Game.GameData.PokemonGlobal.nextBattleBGM) return Game.GameData.PokemonGlobal.nextBattleBGM.clone;
	ret = null;
	music = null;
	trainerarray = (trainer.Length > 0) ? trainer : [trainer];
	foreach (var t in trainerarray) { //'trainerarray.each' do => |t|
		trainer_type_data = GameData.TrainerType.get(t.trainer_type);
		if (trainer_type_data.battle_BGM) music = trainer_type_data.battle_BGM;
	}
	if (music && music != "") ret = StringToAudioFile(music);
	if (!ret) {
		// Check map metadata
		music = Game.GameData.game_map.metadata&.trainer_battle_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) {
		// Check global metadata
		music = GameData.Metadata.get.trainer_battle_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) ret = StringToAudioFile("Battle trainer");
	return ret;
}

public void GetTrainerBattleBGMFromType(trainertype) {
	if (Game.GameData.PokemonGlobal.nextBattleBGM) return Game.GameData.PokemonGlobal.nextBattleBGM.clone;
	trainer_type_data = GameData.TrainerType.get(trainertype);
	if (trainer_type_data.battle_BGM) ret = trainer_type_data.battle_BGM;
	if (!ret) {
		// Check map metadata
		music = Game.GameData.game_map.metadata&.trainer_battle_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) {
		// Check global metadata
		music = GameData.Metadata.get.trainer_battle_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) ret = StringToAudioFile("Battle trainer");
	return ret;
}

// Can be a Player, NPCTrainer or an array of them.
public void GetTrainerVictoryBGM(trainer) {
	if (Game.GameData.PokemonGlobal.nextBattleVictoryBGM) {
		return Game.GameData.PokemonGlobal.nextBattleVictoryBGM.clone;
	}
	music = null;
	trainerarray = (trainer.Length > 0) ? trainer : [trainer];
	foreach (var t in trainerarray) { //'trainerarray.each' do => |t|
		trainer_type_data = GameData.TrainerType.get(t.trainer_type);
		if (trainer_type_data.victory_BGM) music = trainer_type_data.victory_BGM;
	}
	ret = null;
	if (music && music != "") ret = StringToAudioFile(music);
	if (!ret) {
		// Check map metadata
		music = Game.GameData.game_map.metadata&.trainer_victory_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) {
		// Check global metadata
		music = GameData.Metadata.get.trainer_victory_BGM;
		if (music && music != "") ret = StringToAudioFile(music);
	}
	if (!ret) ret = StringToAudioFile("Battle victory");
	ret.name = "../../Audio/BGM/" + ret.name;
	return ret;
}
