//===============================================================================
// Entering/exiting cave animations
//===============================================================================
public void CaveEntranceEx(exiting) {
	// Create bitmap
	sprite = new BitmapSprite(Graphics.width, Graphics.height);
	sprite.z = 100000;
	// Define values used for the animation
	duration = 0.4;
	totalBands = 15;
	bandheight = ((Graphics.height / 2.0) - 10) / totalBands;
	bandwidth  = ((Graphics.width / 2.0) - 12) / totalBands;
	start_gray = (exiting) ? 0 : 255;
	end_gray = (exiting) ? 255 : 0;
	// Create initial array of band colors (black if exiting, white if entering)
	grays = new Array(totalBands, i => { start_gray; });
	// Animate bands changing color
	timer_start = System.uptime;
	until System.uptime - timer_start >= duration;
		x = 0;
		y = 0;
		// Calculate color of each band
		for (int k = totalBands; k < totalBands; k++) { //for 'totalBands' times do => |k|
			grays[k] = lerp(start_gray, end_gray, duration, timer_start + (k * duration / totalBands), System.uptime);
		}
		// Draw gray rectangles
		rectwidth  = Graphics.width;
		rectheight = Graphics.height;
		for (int i = totalBands; i < totalBands; i++) { //for 'totalBands' times do => |i|
			currentGray = grays[i];
			sprite.bitmap.fill_rect(new Rect(x, y, rectwidth, rectheight),
															new Color(currentGray, currentGray, currentGray));
			x += bandwidth;
			y += bandheight;
			rectwidth  -= bandwidth * 2;
			rectheight -= bandheight * 2;
		}
		Graphics.update;
		Input.update;
	}
	// Set the tone at end of band animation
	if (exiting) {
		ToneChangeAll(new Tone(255, 255, 255), 0);
	} else {
		ToneChangeAll(new Tone(-255, -255, -255), 0);
	}
	// Animate fade to white (if exiting) or black (if entering)
	timer_start = System.uptime;
	do { //loop; while (true);
		sprite.color = new Color(end_gray, end_gray, end_gray,
														lerp(0, 255, duration, timer_start, System.uptime));
		Graphics.update;
		Input.update;
		if (sprite.color.alpha >= 255) break;
	}
	// Set the tone at end of fading animation
	ToneChangeAll(new Tone(0, 0, 0), 8);
	// Pause briefly
	timer_start = System.uptime;
	until System.uptime - timer_start >= 0.1;
		Graphics.update;
		Input.update;
	}
	sprite.dispose;
}

public void CaveEntrance() {
	SetEscapePoint;
	CaveEntranceEx(false);
}

public void CaveExit() {
	EraseEscapePoint;
	CaveEntranceEx(true);
}

//===============================================================================
// Blacking out animation
//===============================================================================
public void StartOver(game_over = false) {
	if (InBugContest()) {
		BugContestStartOver;
		return;
	}
	Game.GameData.stats.blacked_out_count += 1;
	Game.GameData.player.heal_party;
	if (Game.GameData.PokemonGlobal.pokecenterMapId && Game.GameData.PokemonGlobal.pokecenterMapId >= 0) {
		if (game_over) {
			Message("\\w[]\\wm\\c[8]\\l[3]" +;
								_INTL("After the unfortunate defeat, you hurry to the Pokémon Center."));
		} else if (Game.GameData.player.all_fainted()) {
			Message("\\w[]\\wm\\c[8]\\l[3]" +;
								_INTL("You hurry to the Pokémon Center, shielding your exhausted Pokémon from any further harm..."));
		} else {   // Forfeited a trainer battle
			Message("\\w[]\\wm\\c[8]\\l[3]" +;
								_INTL("You went running to the Pokémon Center to regroup and reconsider your battle strategy..."));
		}
		CancelVehicles;
		Followers.clear;
		Game.GameData.game_switches[Settings.STARTING_OVER_SWITCH] = true;
		Game.GameData.game_temp.player_new_map_id    = Game.GameData.PokemonGlobal.pokecenterMapId;
		Game.GameData.game_temp.player_new_x         = Game.GameData.PokemonGlobal.pokecenterX;
		Game.GameData.game_temp.player_new_y         = Game.GameData.PokemonGlobal.pokecenterY;
		Game.GameData.game_temp.player_new_direction = Game.GameData.PokemonGlobal.pokecenterDirection;
		DismountBike;
		if (Game.GameData.scene.is_a(Scene_Map)) Game.GameData.scene.transfer_player;
		Game.GameData.game_map.refresh;
	} else {
		homedata = GameData.PlayerMetadata.get(Game.GameData.player.character_ID)&.home;
		if (!homedata) homedata = GameData.Metadata.get.home;
		if (homedata && !RgssExists(string.Format("Data/Map{0:3}.rxdata", homedata[0]))) {
			if (Core.DEBUG) {
				Message(string.Format("Can't find the map 'Map{1:03d}' in the Data folder. The game will resume at the player's position.", homedata[0]));
			}
			Game.GameData.player.heal_party;
			return;
		}
		if (game_over) {
			Message("\\w[]\\wm\\c[8]\\l[3]" +;
								_INTL("After the unfortunate defeat, you hurry back home."));
		} else if (Game.GameData.player.all_fainted()) {
			Message("\\w[]\\wm\\c[8]\\l[3]" +;
								_INTL("You hurry back home, shielding your exhausted Pokémon from any further harm..."));
		} else {   // Forfeited a trainer battle
			Message("\\w[]\\wm\\c[8]\\l[3]" +;
								_INTL("You went running back home to regroup and reconsider your battle strategy..."));
		}
		if (homedata) {
			CancelVehicles;
			Followers.clear;
			Game.GameData.game_switches[Settings.STARTING_OVER_SWITCH] = true;
			Game.GameData.game_temp.player_new_map_id    = homedata[0];
			Game.GameData.game_temp.player_new_x         = homedata[1];
			Game.GameData.game_temp.player_new_y         = homedata[2];
			Game.GameData.game_temp.player_new_direction = homedata[3];
			DismountBike;
			if (Game.GameData.scene.is_a(Scene_Map)) Game.GameData.scene.transfer_player;
			Game.GameData.game_map.refresh;
		} else {
			Game.GameData.player.heal_party;
		}
	}
	EraseEscapePoint;
}
