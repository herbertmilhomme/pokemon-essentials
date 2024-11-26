//===============================================================================
// Fishing.
//===============================================================================
public void FishingBegin() {
	Game.GameData.PokemonGlobal.fishing = true;
	if (!CommonEvent(Settings.FISHING_BEGIN_COMMON_EVENT)) {
		Game.GameData.game_player.set_movement_type((Game.GameData.PokemonGlobal.surfing) ? :surf_fishing : :fishing);
		Game.GameData.game_player.lock_pattern = true;
		for (int pattern = 4; pattern < 4; pattern++) { //for '4' times do => |pattern|
			Game.GameData.game_player.pattern = 3 - pattern;
			Wait(0.05);
		}
	}
}

public void FishingEnd() {
	if (!CommonEvent(Settings.FISHING_END_COMMON_EVENT)) {
		for (int pattern = 4; pattern < 4; pattern++) { //for '4' times do => |pattern|
			Game.GameData.game_player.pattern = pattern;
			Wait(0.05);
		}
	}
	if (block_given()) yield;
	Game.GameData.game_player.set_movement_type((Game.GameData.PokemonGlobal.surfing) ? :surfing_stopped : :walking_stopped);
	Game.GameData.game_player.lock_pattern = false;
	Game.GameData.game_player.straighten;
	Game.GameData.PokemonGlobal.fishing = false;
}

public void Fishing(hasEncounter, rodType = 1) {
	Game.GameData.stats.fishing_count += 1;
	speedup = (Game.GameData.player.first_pokemon && new []{:STICKYHOLD, :SUCTIONCUPS}.Contains(Game.GameData.player.first_pokemon.ability_id));
	biteChance = 20 + (25 * rodType);   // 45, 70, 95
	if (speedup) biteChance *= 1.5;   // 67.5, 100, 100
	hookChance = 100;
	FishingBegin;
	msgWindow = CreateMessageWindow;
	ret = false;
	do { //loop; while (true);
		time = rand(5..10);
		if (speedup) time = (int)Math.Min(time, rand(5..10));
		message = "";
		time.times(() => message += ".   ");
		if (WaitMessage(msgWindow, time)) {
			FishingEnd { MessageDisplay(msgWindow, _INTL("Not even a nibble...")) };
			break;
		}
		if (hasEncounter && rand(100) < biteChance) {
			Game.GameData.game_player.animation_id = Settings.EXCLAMATION_ANIMATION_ID;
			Game.GameData.game_player.animation_height = 3;
			Game.GameData.game_player.animation_regular_tone = true;
			duration = rand(5..10) / 10.0;   // 0.5-1 seconds
			if (!WaitForInput(msgWindow, message + "\n" + _INTL("Oh! A bite!"), duration)) {
				FishingEnd { MessageDisplay(msgWindow, _INTL("The Pokémon got away...")) };
				break;
			}
			if (Settings.FISHING_AUTO_HOOK || rand(100) < hookChance) {
				FishingEnd do;
					if (!Settings.FISHING_AUTO_HOOK) MessageDisplay(msgWindow, _INTL("Landed a Pokémon!"));
				}
				ret = true;
				break;
			}
//      biteChance += 15
//      hookChance += 15
		} else {
			FishingEnd { MessageDisplay(msgWindow, _INTL("Not even a nibble...")) };
			break;
		}
	}
	DisposeMessageWindow(msgWindow);
	return ret;
}

// Show waiting dots before a Pokémon bites
public void WaitMessage(msgWindow, time) {
	message = "";
	for (int i = (time + 1); i < (time + 1); i++) { //for '(time + 1)' times do => |i|
		if (i > 0) message += ".   ";
		MessageDisplay(msgWindow, message, false);
		foreach (var delta_t in Wait(0.4)) { //Wait(0.4) do => |delta_t|
			if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) return true;
		}
	}
	return false;
}

// A Pokémon is biting, reflex test to reel it in
public void WaitForInput(msgWindow, message, duration) {
	MessageDisplay(msgWindow, message, false);
	twitch_frame_duration = 0.2;   // 0.2 seconds
	timer_start = System.uptime;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		UpdateSceneMap;
		// Twitch cycle: 1,0,1,0,0,0,0,0
		twitch_frame = ((System.uptime - timer_start) / twitch_frame_duration).ToInt() % 8;
		switch (twitch_frame) {
			case 0: case 2:
				Game.GameData.game_player.pattern = 1;
				break;
			default:
				Game.GameData.game_player.pattern = 0;
				break;
		}
		if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
			Game.GameData.game_player.pattern = 0;
			return true;
		}
		if (!Settings.FISHING_AUTO_HOOK && System.uptime - timer_start > duration) break;
	}
	return false;
}
