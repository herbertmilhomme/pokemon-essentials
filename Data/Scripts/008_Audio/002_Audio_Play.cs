//===============================================================================
//
//===============================================================================
public void StringToAudioFile(str) {
	if (System.Text.RegularExpressions.Regex.IsMatch(str,@"^(.*)\:\s*(\d+)\s*\:\s*(\d+)\s*$")) {   // Of the format "XXX: ###: ###"
		file   = Game.GameData.1;
		volume = Game.GameData.2.ToInt();
		pitch  = Game.GameData.3.ToInt();
		return new RPG.AudioFile(file, volume, pitch);
	} else if (System.Text.RegularExpressions.Regex.IsMatch(str,@"^(.*)\:\s*(\d+)\s*$")) {             // Of the format "XXX: ###"
		file   = Game.GameData.1;
		volume = Game.GameData.2.ToInt();
		return new RPG.AudioFile(file, volume, 100);
	} else {
		return new RPG.AudioFile(str, 100, 100);
	}
}

// Converts an object to an audio file.
// str -- Either a string showing the filename or an RPG.AudioFile object.
// Possible formats for _str_:
// filename                        volume and pitch 100
// filename:volume           pitch 100
// filename:volume:pitch
// volume -- Volume of the file, up to 100
// pitch -- Pitch of the file, normally 100
public void ResolveAudioFile(str, volume = null, pitch = null) {
	if (str.is_a(String)) {
		str = StringToAudioFile(str);
		str.volume = volume || 100;
		str.pitch  = pitch || 100;
	}
	if (str.is_a(RPG.AudioFile)) {
		if (volume || pitch) {
			return new RPG.AudioFile(str.name, volume || str.volume || 100,
																pitch || str.pitch || 100);
		} else {
			return str;
		}
	}
	return str;
}

//===============================================================================
//
//===============================================================================
// Plays a BGM file.
// param -- Either a string showing the filename
// (relative to Audio/BGM/) or an RPG.AudioFile object.
// Possible formats for _param_:
// filename                        volume and pitch 100
// filename:volume           pitch 100
// filename:volume:pitch
// volume -- Volume of the file, up to 100
// pitch -- Pitch of the file, normally 100
public void BGMPlay(param, volume = null, pitch = null) {
	if (!param) return;
	param = ResolveAudioFile(param, volume, pitch);
	if (param.name && param.name != "") {
		if (Game.GameData.game_system) {
			Game.GameData.game_system.bgm_play(param);
			return;
		} else if ((RPG.const_defined(:BGM) rescue false)) {
			b = new RPG.BGM(param.name, param.volume, param.pitch);
			if (b.respond_to("play")) {
				b.play;
				return;
			}
		}
		Audio.bgm_play(canonicalize("Audio/BGM/" + param.name), param.volume, param.pitch);
	}
}

// Fades out or stops BGM playback. 'x' is the time in seconds to fade out.
public void BGMFade(x = 0.0) {BGMStop(x); };

// Fades out or stops BGM playback. 'x' is the time in seconds to fade out.
public void BGMStop(timeInSeconds = 0.0) {
	if (Game.GameData.game_system && timeInSeconds > 0.0) {
		Game.GameData.game_system.bgm_fade(timeInSeconds);
		return;
	} else if (Game.GameData.game_system) {
		Game.GameData.game_system.bgm_stop;
		return;
	} else if ((RPG.const_defined(:BGM) rescue false)) {
		begin;
			(timeInSeconds > 0.0) ? RPG.BGM.fade((int)Math.Floor(timeInSeconds * 1000)) : RPG.BGM.stop
			return;
		rescue;
		}
	}
	(timeInSeconds > 0.0) ? Audio.bgm_fade((int)Math.Floor(timeInSeconds * 1000)) : Audio.bgm_stop
}

//===============================================================================
//
//===============================================================================
// Plays an ME file.
// param -- Either a string showing the filename
// (relative to Audio/ME/) or an RPG.AudioFile object.
// Possible formats for _param_:
// filename                        volume and pitch 100
// filename:volume           pitch 100
// filename:volume:pitch
// volume -- Volume of the file, up to 100
// pitch -- Pitch of the file, normally 100
public void MEPlay(param, volume = null, pitch = null) {
	if (!param) return;
	param = ResolveAudioFile(param, volume, pitch);
	if (param.name && param.name != "") {
		if (Game.GameData.game_system) {
			Game.GameData.game_system.me_play(param);
			return;
		} else if ((RPG.const_defined(:ME) rescue false)) {
			b = new RPG.ME(param.name, param.volume, param.pitch);
			if (b.respond_to("play")) {
				b.play;
				return;
			}
		}
		Audio.me_play(canonicalize("Audio/ME/" + param.name), param.volume, param.pitch);
	}
}

// Fades out or stops ME playback. 'x' is the time in seconds to fade out.
public void MEFade(x = 0.0) {MEStop(x); };

// Fades out or stops ME playback. 'x' is the time in seconds to fade out.
public void MEStop(timeInSeconds = 0.0) {
	if (Game.GameData.game_system && timeInSeconds > 0.0 && Game.GameData.game_system.respond_to("me_fade")) {
		Game.GameData.game_system.me_fade(timeInSeconds);
		return;
	} else if (Game.GameData.game_system.respond_to("me_stop")) {
		Game.GameData.game_system.me_stop(null);
		return;
	} else if ((RPG.const_defined(:ME) rescue false)) {
		begin;
			(timeInSeconds > 0.0) ? RPG.ME.fade((int)Math.Floor(timeInSeconds * 1000)) : RPG.ME.stop
			return;
		rescue;
		}
	}
	(timeInSeconds > 0.0) ? Audio.me_fade((int)Math.Floor(timeInSeconds * 1000)) : Audio.me_stop
}

//===============================================================================
//
//===============================================================================
// Plays a BGS file.
// param -- Either a string showing the filename
// (relative to Audio/BGS/) or an RPG.AudioFile object.
// Possible formats for _param_:
// filename                        volume and pitch 100
// filename:volume           pitch 100
// filename:volume:pitch
// volume -- Volume of the file, up to 100
// pitch -- Pitch of the file, normally 100
public void BGSPlay(param, volume = null, pitch = null) {
	if (!param) return;
	param = ResolveAudioFile(param, volume, pitch);
	if (param.name && param.name != "") {
		if (Game.GameData.game_system) {
			Game.GameData.game_system.bgs_play(param);
			return;
		} else if ((RPG.const_defined(:BGS) rescue false)) {
			b = new RPG.BGS(param.name, param.volume, param.pitch);
			if (b.respond_to("play")) {
				b.play;
				return;
			}
		}
		Audio.bgs_play(canonicalize("Audio/BGS/" + param.name), param.volume, param.pitch);
	}
}

// Fades out or stops BGS playback. 'x' is the time in seconds to fade out.
public void BGSFade(x = 0.0) {BGSStop(x); };

// Fades out or stops BGS playback. 'x' is the time in seconds to fade out.
public void BGSStop(timeInSeconds = 0.0) {
	if (Game.GameData.game_system && timeInSeconds > 0.0) {
		Game.GameData.game_system.bgs_fade(timeInSeconds);
		return;
	} else if (Game.GameData.game_system) {
		Game.GameData.game_system.bgs_play(null);
		return;
	} else if ((RPG.const_defined(:BGS) rescue false)) {
		begin;
			(timeInSeconds > 0.0) ? RPG.BGS.fade((int)Math.Floor(timeInSeconds * 1000)) : RPG.BGS.stop
			return;
		rescue;
		}
	}
	(timeInSeconds > 0.0) ? Audio.bgs_fade((int)Math.Floor(timeInSeconds * 1000)) : Audio.bgs_stop
}

//===============================================================================
//
//===============================================================================
// Plays an SE file.
// param -- Either a string showing the filename
// (relative to Audio/SE/) or an RPG.AudioFile object.
// Possible formats for _param_:
// filename                  volume and pitch 100
// filename:volume           pitch 100
// filename:volume:pitch
// volume -- Volume of the file, up to 100
// pitch -- Pitch of the file, normally 100
public void SEPlay(param, volume = null, pitch = null) {
	if (!param) return;
	param = ResolveAudioFile(param, volume, pitch);
	if (param.name && param.name != "") {
		if (Game.GameData.game_system) {
			Game.GameData.game_system.se_play(param);
			return;
		}
		if ((RPG.const_defined(:SE) rescue false)) {
			b = new RPG.SE(param.name, param.volume, param.pitch);
			if (b.respond_to("play")) {
				b.play;
				return;
			}
		}
		Audio.se_play(canonicalize("Audio/SE/" + param.name), param.volume, param.pitch);
	}
}

// Stops SE playback.
public void SEFade(x = 0.0) {SEStop(x); };

// Stops SE playback.
public void SEStop(_timeInSeconds = 0.0) {
	if (Game.GameData.game_system) {
		Game.GameData.game_system.se_stop;
	} else if ((RPG.const_defined(:SE) rescue false)) {
		RPG.SE.stop rescue null;
	} else {
		Audio.se_stop;
	}
}

// Plays a sound effect that plays when the player moves the cursor.
public void PlayCursorSE() {
	if (!nil_or_empty(Game.GameData.data_system&.cursor_se&.name)) {
		SEPlay(Game.GameData.data_system.cursor_se);
	} else if (FileTest.audio_exist("Audio/SE/GUI sel cursor")) {
		SEPlay("GUI sel cursor", 80);
	}
}

// Plays a sound effect that plays when a decision is confirmed or a choice is made.
public void PlayDecisionSE() {
	if (!nil_or_empty(Game.GameData.data_system&.decision_se&.name)) {
		SEPlay(Game.GameData.data_system.decision_se);
	} else if (FileTest.audio_exist("Audio/SE/GUI sel decision")) {
		SEPlay("GUI sel decision", 80);
	}
}

// Plays a sound effect that plays when a choice is canceled.
public void PlayCancelSE() {
	if (!nil_or_empty(Game.GameData.data_system&.cancel_se&.name)) {
		SEPlay(Game.GameData.data_system.cancel_se);
	} else if (FileTest.audio_exist("Audio/SE/GUI sel cancel")) {
		SEPlay("GUI sel cancel", 80);
	}
}

// Plays a buzzer sound effect.
public void PlayBuzzerSE() {
	if (!nil_or_empty(Game.GameData.data_system&.buzzer_se&.name)) {
		SEPlay(Game.GameData.data_system.buzzer_se);
	} else if (FileTest.audio_exist("Audio/SE/GUI sel buzzer")) {
		SEPlay("GUI sel buzzer", 80);
	}
}

// Plays a sound effect that plays when the player closes a menu.
public void PlayCloseMenuSE() {
	if (FileTest.audio_exist("Audio/SE/GUI menu close")) {
		SEPlay("GUI menu close", 80);
	}
}
