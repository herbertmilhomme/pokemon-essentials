//===============================================================================
// Hidden move handlers.
//===============================================================================
public static partial class HiddenMoveHandlers {
	CanUseMove     = new MoveHandlerHash();
	ConfirmUseMove = new MoveHandlerHash();
	UseMove        = new MoveHandlerHash();

	public static void addCanUseMove(item, proc) {      CanUseMove.add(item, proc);     }
	public static void addConfirmUseMove(item, proc) {  ConfirmUseMove.add(item, proc); }
	public static void addUseMove(item, proc) {         UseMove.add(item, proc);        }

	public static void hasHandler(item) {
		return !CanUseMove[item].null() && !UseMove[item].null();
	}

	// Returns whether move can be used
	public static void triggerCanUseMove(item, pokemon, showmsg) {
		if (!CanUseMove[item]) return false;
		return CanUseMove.trigger(item, pokemon, showmsg);
	}

	// Returns whether the player confirmed that they want to use the move
	public static void triggerConfirmUseMove(item, pokemon) {
		if (!ConfirmUseMove[item]) return true;
		return ConfirmUseMove.trigger(item, pokemon);
	}

	// Returns whether move was used
	public static void triggerUseMove(item, pokemon) {
		if (!UseMove[item]) return false;
		return UseMove.trigger(item, pokemon);
	}
}

//===============================================================================
//
//===============================================================================
public bool CanUseHiddenMove(pkmn, move, showmsg = true) {
	return HiddenMoveHandlers.triggerCanUseMove(move, pkmn, showmsg);
}

public void ConfirmUseHiddenMove(pokemon, move) {
	return HiddenMoveHandlers.triggerConfirmUseMove(move, pokemon);
}

public void UseHiddenMove(pokemon, move) {
	return HiddenMoveHandlers.triggerUseMove(move, pokemon);
}

// Unused
public void HiddenMoveEvent() {
	EventHandlers.trigger(:on_player_interact);
}

public void CheckHiddenMoveBadge(badge = -1, showmsg = true) {
	if (badge < 0) return true;   // No badge requirement
	if (Core.DEBUG) return true;
	if ((Settings.FIELD_MOVES_COUNT_BADGES) ? Game.GameData.player.badge_count >= badge : Game.GameData.player.badges[badge]) {
		return true;
	}
	if (showmsg) Message(_INTL("Sorry, a new Badge is required."));
	return false;
}

//===============================================================================
// Hidden move animation.
//===============================================================================
public void HiddenMoveAnimation(pokemon) {
	if (!pokemon) return false;
	viewport = new Viewport(0, 0, Graphics.width, 0);
	viewport.z = 99999;
	// Set up sprites
	bg = new Sprite(viewport);
	bg.bitmap = RPG.Cache.ui("Field move/bg");
	sprite = new PokemonSprite(viewport);
	sprite.setOffset(PictureOrigin.CENTER);
	sprite.setPokemonBitmap(pokemon);
	sprite.x = Graphics.width + (sprite.bitmap.width / 2);
	sprite.y = bg.bitmap.height / 2;
	sprite.z = 1;
	sprite.visible = false;
	strobebitmap = new AnimatedBitmap("Graphics/UI/Field move/strobes");
	strobes = new List<string>();
	strobes_start_x = new List<string>();
	strobes_timers = new List<string>();
	for (int i = 15; i < 15; i++) { //for '15' times do => |i|
		strobe = new BitmapSprite(52, 16, viewport);
		strobe.bitmap.blt(0, 0, strobebitmap.bitmap, new Rect(0, (i % 2) * 16, 52, 16));
		strobe.z = (i.even() ? 2 : 0);
		strobe.visible = false;
		strobes.Add(strobe);
	}
	strobebitmap.dispose;
	// Do the animation
	phase = 1;
	timer_start = System.uptime;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		sprite.update;
		switch (phase) {
			case 1:   // Expand viewport height from zero to full
				viewport.rect.y = lerp(Graphics.height / 2, (Graphics.height - bg.bitmap.height) / 2,
															0.25, timer_start, System.uptime);
				viewport.rect.height = Graphics.height - (viewport.rect.y * 2);
				bg.oy = (bg.bitmap.height - viewport.rect.height) / 2;
				if (viewport.rect.y == (Graphics.height - bg.bitmap.height) / 2) {
					phase = 2;
					sprite.visible = true;
					timer_start = System.uptime;
				}
				break;
			case 2:   // Slide Pokémon sprite in from right to centre
				sprite.x = lerp(Graphics.width + (sprite.bitmap.width / 2), Graphics.width / 2,
												0.4, timer_start, System.uptime);
				if (sprite.x == Graphics.width / 2) {
					phase = 3;
					pokemon.play_cry;
					timer_start = System.uptime;
				}
				break;
			case 3:   // Wait
				if (System.uptime - timer_start >= 0.75) {
					phase = 4;
					timer_start = System.uptime;
				}
				break;
			case 4:   // Slide Pokémon sprite off from centre to left
				sprite.x = lerp(Graphics.width / 2, -(sprite.bitmap.width / 2),
												0.4, timer_start, System.uptime);
				if (sprite.x == -(sprite.bitmap.width / 2)) {
					phase = 5;
					sprite.visible = false;
					timer_start = System.uptime;
				}
				break;
			case 5:   // Shrink viewport height from full to zero
				viewport.rect.y = lerp((Graphics.height - bg.bitmap.height) / 2, Graphics.height / 2,
															0.25, timer_start, System.uptime);
				viewport.rect.height = Graphics.height - (viewport.rect.y * 2);
				bg.oy = (bg.bitmap.height - viewport.rect.height) / 2;
				if (viewport.rect.y == Graphics.height / 2) phase = 6;
				break;
		}
		// Constantly stream the strobes across the screen
		strobes.each_with_index do |strobe, i|
			strobe.ox = strobe.viewport.rect.x;
			strobe.oy = strobe.viewport.rect.y;
			if (!strobe.visible) {   // Initial placement of strobes
				randomY = 16 * (1 + rand((bg.bitmap.height / 16) - 2));
				strobe.y = randomY + ((Graphics.height - bg.bitmap.height) / 2);
				strobe.x = rand(Graphics.width);
				strobe.visible = true;
				strobes_start_x[i] = strobe.x;
				strobes_timers[i] = System.uptime;
			} else if (strobe.x < Graphics.width) {   // Move strobe right
				strobe.x = strobes_start_x[i] + lerp(0, Graphics.width * 2, 0.8, strobes_timers[i], System.uptime);
			} else {   // Strobe is off the screen, reposition it to the left of the screen
				randomY = 16 * (1 + rand((bg.bitmap.height / 16) - 2));
				strobe.y = randomY + ((Graphics.height - bg.bitmap.height) / 2);
				strobe.x = -strobe.bitmap.width - rand(Graphics.width / 4);
				strobes_start_x[i] = strobe.x;
				strobes_timers[i] = System.uptime;
			}
		}
		UpdateSceneMap;
		if (phase == 6) break;
	}
	sprite.dispose;
	strobes.each(strobe => strobe.dispose);
	strobes.clear;
	bg.dispose;
	viewport.dispose;
	return true;
}

//===============================================================================
// Cut.
//===============================================================================
public void Cut() {
	move = moves.CUT;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_CUT, false) || (!Core.DEBUG && !movefinder)) {
		Message(_INTL("This tree looks like it can be cut down."));
		return false;
	}
	if (ConfirmMessage(_INTL("This tree looks like it can be cut down!\nWould you like to cut it?"))) {
		Game.GameData.stats.cut_count += 1;
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		return true;
	}
	return false;
}

HiddenMoveHandlers.CanUseMove.Add(Moves.CUT, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_CUT, showmsg)) next false;
	facingEvent = Game.GameData.game_player.FacingEvent;
	if (!facingEvent || !System.Text.RegularExpressions.Regex.IsMatch(facingEvent.name,@"cuttree",RegexOptions.IgnoreCase)) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.CUT, block: (move, pokemon) => {
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	Game.GameData.stats.cut_count += 1;
	facingEvent = Game.GameData.game_player.FacingEvent;
	if (facingEvent) SmashEvent(facingEvent);
	next true;
});

public void SmashEvent(event) {
	if (!event) return;
	if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"cuttree",RegexOptions.IgnoreCase)) {
		SEPlay("Cut");
	} else if (System.Text.RegularExpressions.Regex.IsMatch(event.name,@"smashrock",RegexOptions.IgnoreCase)) {
		SEPlay("Rock Smash");
	}
	MoveRoute(event, new {MoveRoute.WAIT, 2,
						MoveRoute.TURN_LEFT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_RIGHT, MoveRoute.WAIT, 2,
						MoveRoute.TURN_UP, MoveRoute.WAIT, 2});
	Wait(0.4);
	event.erase;
	Game.GameData.PokemonMap&.addErasedEvent(event.id);
}

//===============================================================================
// Dig.
//===============================================================================
HiddenMoveHandlers.CanUseMove.Add(Moves.DIG, block: (move, pkmn, showmsg) => {
	escape = (Game.GameData.PokemonGlobal.escapePoint rescue null);
	if (!escape || escape == new List<string>()) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	if (!Game.GameData.game_player.can_map_transfer_with_follower()) {
		if (showmsg) Message(_INTL("It can't be used when you have someone with you."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.ConfirmUseMove.Add(Moves.DIG, block: (move, pkmn) => {
	escape = (Game.GameData.PokemonGlobal.escapePoint rescue null);
	if (!escape || escape == new List<string>()) next false;
	mapname = GetMapNameFromId(escape[0]);
	next ConfirmMessage(_INTL("Want to escape from here and return to {1}?", mapname));
});

HiddenMoveHandlers.UseMove.Add(Moves.DIG, block: (move, pokemon) => {
	escape = (Game.GameData.PokemonGlobal.escapePoint rescue null);
	if (escape) {
		if (!HiddenMoveAnimation(pokemon)) {
			Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
		}
		FadeOutIn do;
			Game.GameData.game_temp.player_new_map_id    = escape[0];
			Game.GameData.game_temp.player_new_x         = escape[1];
			Game.GameData.game_temp.player_new_y         = escape[2];
			Game.GameData.game_temp.player_new_direction = escape[3];
			DismountBike;
			Game.GameData.scene.transfer_player;
			Game.GameData.game_map.autoplay;
			Game.GameData.game_map.refresh;
		}
		EraseEscapePoint;
		next true;
	}
	next false;
});

//===============================================================================
// Dive.
//===============================================================================
public void Dive() {
	map_metadata = Game.GameData.game_map.metadata;
	if (!map_metadata || !map_metadata.dive_map_id) return false;
	move = moves.DIVE;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_DIVE, false) || (!Core.DEBUG && !movefinder)) {
		Message(_INTL("The sea is deep here. A Pokémon may be able to go underwater."));
		return false;
	}
	if (ConfirmMessage(_INTL("The sea is deep here. Would you like to use Dive?"))) {
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		FadeOutIn do;
			Game.GameData.game_temp.player_new_map_id    = map_metadata.dive_map_id;
			Game.GameData.game_temp.player_new_x         = Game.GameData.game_player.x;
			Game.GameData.game_temp.player_new_y         = Game.GameData.game_player.y;
			Game.GameData.game_temp.player_new_direction = Game.GameData.game_player.direction;
			Game.GameData.PokemonGlobal.surfing = false;
			Game.GameData.PokemonGlobal.diving  = true;
			Game.GameData.stats.dive_count += 1;
			UpdateVehicle;
			Game.GameData.scene.transfer_player(false);
			Game.GameData.game_map.autoplay;
			Game.GameData.game_map.refresh;
		}
		return true;
	}
	return false;
}

public void Surfacing() {
	if (!Game.GameData.PokemonGlobal.diving) return;
	surface_map_id = null;
	foreach (var map_data in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |map_data|
		if (!map_data.dive_map_id || map_data.dive_map_id != Game.GameData.game_map.map_id) continue;
		surface_map_id = map_data.id;
		break;
	}
	if (!surface_map_id) return;
	move = moves.DIVE;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_DIVE, false) || (!Core.DEBUG && !movefinder)) {
		Message(_INTL("Light is filtering down from above. A Pokémon may be able to surface here."));
		return false;
	}
	if (ConfirmMessage(_INTL("Light is filtering down from above. Would you like to use Dive?"))) {
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		FadeOutIn do;
			Game.GameData.game_temp.player_new_map_id    = surface_map_id;
			Game.GameData.game_temp.player_new_x         = Game.GameData.game_player.x;
			Game.GameData.game_temp.player_new_y         = Game.GameData.game_player.y;
			Game.GameData.game_temp.player_new_direction = Game.GameData.game_player.direction;
			Game.GameData.PokemonGlobal.surfing = true;
			Game.GameData.PokemonGlobal.diving  = false;
			UpdateVehicle;
			Game.GameData.scene.transfer_player(false);
			surfbgm = GameData.Metadata.get.surf_BGM;
			(surfbgm) ? BGMPlay(surfbgm) : Game.GameData.game_map.autoplayAsCue
			Game.GameData.game_map.refresh;
		}
		return true;
	}
	return false;
}

EventHandlers.add(:on_player_interact, :diving,
	block: () => {
		if (Game.GameData.PokemonGlobal.diving) {
			surface_map_id = null;
			foreach (var map_data in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |map_data|
				if (!map_data.dive_map_id || map_data.dive_map_id != Game.GameData.game_map.map_id) continue;
				surface_map_id = map_data.id;
				break;
			}
			if (surface_map_id &&
				Game.GameData.map_factory.getTerrainTag(surface_map_id, Game.GameData.game_player.x, Game.GameData.game_player.y).can_dive) {
				Surfacing;
			}
		} else if (Game.GameData.game_player.terrain_tag.can_dive) {
			Dive;
		}
	}
)

HiddenMoveHandlers.CanUseMove.Add(Moves.DIVE, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_DIVE, showmsg)) next false;
	if (Game.GameData.PokemonGlobal.diving) {
		surface_map_id = null;
		foreach (var map_data in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |map_data|
			if (!map_data.dive_map_id || map_data.dive_map_id != Game.GameData.game_map.map_id) continue;
			surface_map_id = map_data.id;
			break;
		}
		if (!surface_map_id ||
			!Game.GameData.map_factory.getTerrainTag(surface_map_id, Game.GameData.game_player.x, Game.GameData.game_player.y).can_dive) {
			if (showmsg) Message(_INTL("You can't use that here."));
			next false;
		}
	} else {
		if (!Game.GameData.game_map.metadata&.dive_map_id) {
			if (showmsg) Message(_INTL("You can't use that here."));
			next false;
		}
		if (!Game.GameData.game_player.terrain_tag.can_dive) {
			if (showmsg) Message(_INTL("You can't use that here."));
			next false;
		}
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.DIVE, block: (move, pokemon) => {
	wasdiving = Game.GameData.PokemonGlobal.diving;
	if (Game.GameData.PokemonGlobal.diving) {
		dive_map_id = null;
		foreach (var map_data in GameData.MapMetadata) { //'GameData.MapMetadata.each' do => |map_data|
			if (!map_data.dive_map_id || map_data.dive_map_id != Game.GameData.game_map.map_id) continue;
			dive_map_id = map_data.id;
			break;
		}
	} else {
		dive_map_id = Game.GameData.game_map.metadata&.dive_map_id;
	}
	if (!dive_map_id) next false;
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	FadeOutIn do;
		Game.GameData.game_temp.player_new_map_id    = dive_map_id;
		Game.GameData.game_temp.player_new_x         = Game.GameData.game_player.x;
		Game.GameData.game_temp.player_new_y         = Game.GameData.game_player.y;
		Game.GameData.game_temp.player_new_direction = Game.GameData.game_player.direction;
		Game.GameData.PokemonGlobal.surfing = wasdiving;
		Game.GameData.PokemonGlobal.diving  = !wasdiving;
		UpdateVehicle;
		Game.GameData.scene.transfer_player(false);
		Game.GameData.game_map.autoplay;
		Game.GameData.game_map.refresh;
	}
	next true;
});

//===============================================================================
// Flash.
//===============================================================================
HiddenMoveHandlers.CanUseMove.Add(Moves.FLASH, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_FLASH, showmsg)) next false;
	if (!Game.GameData.game_map.metadata&.dark_map) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	if (Game.GameData.PokemonGlobal.flashUsed) {
		if (showmsg) Message(_INTL("Flash is already being used."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.FLASH, block: (move, pokemon) => {
	darkness = Game.GameData.game_temp.darkness_sprite;
	if (!darkness || darkness.disposed()) next false;
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	Game.GameData.PokemonGlobal.flashUsed = true;
	Game.GameData.stats.flash_count += 1;
	duration = 0.7;
	foreach (var delta_t in Wait(duration)) { //Wait(duration) do => |delta_t|
		darkness.radius = lerp(darkness.radiusMin, darkness.radiusMax, duration, delta_t);
	}
	darkness.radius = darkness.radiusMax;
	next true;
});

//===============================================================================
// Fly.
//===============================================================================
public bool CanFly(pkmn = null, show_messages = false) {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_FLY, show_messages)) return false;
	if (!Core.DEBUG && !pkmn && !Game.GameData.player.get_pokemon_with_move(Moves.FLY)) return false;
	if (!Game.GameData.game_player.can_map_transfer_with_follower()) {
		if (show_messages) Message(_INTL("It can't be used when you have someone with you."));
		return false;
	}
	if (!Game.GameData.game_map.metadata&.outdoor_map) {
		if (show_messages) Message(_INTL("You can't use that here."));
		return false;
	}
	return true;
}

public void FlyToNewLocation(pkmn = null, move = moves.FLY) {
	if (Game.GameData.game_temp.fly_destination.null()) return false;
	if (!pkmn) pkmn = Game.GameData.player.get_pokemon_with_move(move);
	if (!Core.DEBUG && !pkmn) {
		Game.GameData.game_temp.fly_destination = null;
		if (block_given()) yield;
		return false;
	}
	if (!pkmn || !HiddenMoveAnimation(pkmn)) {
		name = pkmn&.name || Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", name, GameData.Move.get(move).name));
	}
	Game.GameData.stats.fly_count += 1;
	FadeOutIn do;
		SEPlay("Fly");
		Game.GameData.game_temp.player_new_map_id    = Game.GameData.game_temp.fly_destination[0];
		Game.GameData.game_temp.player_new_x         = Game.GameData.game_temp.fly_destination[1];
		Game.GameData.game_temp.player_new_y         = Game.GameData.game_temp.fly_destination[2];
		Game.GameData.game_temp.player_new_direction = 2;
		DismountBike;
		Game.GameData.scene.transfer_player;
		Game.GameData.game_map.autoplay;
		Game.GameData.game_map.refresh;
		if (block_given()) yield;
		Wait(0.25);
	}
	EraseEscapePoint;
	Game.GameData.game_temp.fly_destination = null;
	return true;
}

HiddenMoveHandlers.CanUseMove.Add(Moves.FLY, block: (move, pkmn, showmsg) => {
	next CanFly(pkmn, showmsg);
});

HiddenMoveHandlers.UseMove.Add(Moves.FLY, block: (move, pkmn) => {
	if (Game.GameData.game_temp.fly_destination.null()) {
		Message(_INTL("You can't use that here."));
		next false;
	}
	FlyToNewLocation(pkmn);
	next true;
});

//===============================================================================
// Headbutt.
//===============================================================================
public void HeadbuttEffect(event = null) {
	SEPlay("Headbutt");
	Wait(1.0);
	if (!event) event = Game.GameData.game_player.FacingEvent(true);
	a = (event.x + (int)Math.Floor(event.x / 24) + 1) * (event.y + (int)Math.Floor(event.y / 24) + 1);
	a = (a * 2 / 5) % 10;   // Even 2x as likely as odd, 0 is 1.5x as likely as odd
	b = Game.GameData.player.public_ID % 10;   // Practically equal odds of each value
	chance = 1;                 // ~50%
	if (a == b) {                    // 10%
		chance = 8;
	} else if (a > b && (a - b).abs < 5) {   // ~30.3%
		chance = 5;
	} else if (a < b && (a - b).abs > 5) {   // ~9.7%
		chance = 5;
	}
	if (rand(10) >= chance) {
		Message(_INTL("Nope. Nothing..."));
	} else {
		enctype = (chance == 1) ? :HeadbuttLow : :HeadbuttHigh;
		if (Encounter(enctype)) {
			Game.GameData.stats.headbutt_battles += 1;
		} else {
			Message(_INTL("Nope. Nothing..."));
		}
	}
}

public void Headbutt(event = null) {
	move = moves.HEADBUTT;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!Core.DEBUG && !movefinder) {
		Message(_INTL("A Pokémon could be in this tree. Maybe a Pokémon could shake it."));
		return false;
	}
	if (ConfirmMessage(_INTL("A Pokémon could be in this tree. Would you like to use Headbutt?"))) {
		Game.GameData.stats.headbutt_count += 1;
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		HeadbuttEffect(event);
		return true;
	}
	return false;
}

HiddenMoveHandlers.CanUseMove.Add(Moves.HEADBUTT, block: (move, pkmn, showmsg) => {
	facingEvent = Game.GameData.game_player.FacingEvent;
	if (!facingEvent || !System.Text.RegularExpressions.Regex.IsMatch(facingEvent.name,@"headbutttree",RegexOptions.IgnoreCase)) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.HEADBUTT, block: (move, pokemon) => {
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	Game.GameData.stats.headbutt_count += 1;
	facingEvent = Game.GameData.game_player.FacingEvent;
	HeadbuttEffect(facingEvent);
});

//===============================================================================
// Rock Smash.
//===============================================================================
public void RockSmashRandomEncounter() {
	if (Game.GameData.PokemonEncounters.encounter_triggered(:RockSmash, false, false)) {
		Game.GameData.stats.rock_smash_battles += 1;
		Encounter(:RockSmash);
	}
}

public void RockSmash() {
	move = moves.ROCKSMASH;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_ROCKSMASH, false) || (!Core.DEBUG && !movefinder)) {
		Message(_INTL("It's a rugged rock, but a Pokémon may be able to smash it."));
		return false;
	}
	if (ConfirmMessage(_INTL("This rock seems breakable with a hidden move.\nWould you like to use Rock Smash?"))) {
		Game.GameData.stats.rock_smash_count += 1;
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		return true;
	}
	return false;
}

HiddenMoveHandlers.CanUseMove.Add(Moves.ROCKSMASH, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_ROCKSMASH, showmsg)) next false;
	facingEvent = Game.GameData.game_player.FacingEvent;
	if (!facingEvent || !System.Text.RegularExpressions.Regex.IsMatch(facingEvent.name,@"smashrock",RegexOptions.IgnoreCase)) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.ROCKSMASH, block: (move, pokemon) => {
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	Game.GameData.stats.rock_smash_count += 1;
	facingEvent = Game.GameData.game_player.FacingEvent;
	if (facingEvent) {
		SmashEvent(facingEvent);
		RockSmashRandomEncounter;
	}
	next true;
});

//===============================================================================
// Strength.
//===============================================================================
public void Strength() {
	if (Game.GameData.PokemonMap.strengthUsed) {
		Message(_INTL("Strength made it possible to move boulders around."));
		return false;
	}
	move = moves.STRENGTH;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_STRENGTH, false) || (!Core.DEBUG && !movefinder)) {
		Message(_INTL("It's a big boulder, but a Pokémon may be able to push it aside."));
		return false;
	}
	Message(_INTL("It's a big boulder, but you may be able to push it aside with a hidden move.") + "\1");
	if (ConfirmMessage(_INTL("Would you like to use Strength?"))) {
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		Message(_INTL("Strength made it possible to move boulders around!"));
		Game.GameData.PokemonMap.strengthUsed = true;
		return true;
	}
	return false;
}

EventHandlers.add(:on_player_interact, :strength_event,
	block: () => {
		facingEvent = Game.GameData.game_player.FacingEvent;
		if (facingEvent && System.Text.RegularExpressions.Regex.IsMatch(facingEvent.name,@"strengthboulder",RegexOptions.IgnoreCase)) Strength;
	}
)

HiddenMoveHandlers.CanUseMove.Add(Moves.STRENGTH, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_STRENGTH, showmsg)) next false;
	if (Game.GameData.PokemonMap.strengthUsed) {
		if (showmsg) Message(_INTL("Strength is already being used."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.STRENGTH, block: (move, pokemon) => {
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name) + "\1");
	}
	Message(_INTL("Strength made it possible to move boulders around!"));
	Game.GameData.PokemonMap.strengthUsed = true;
	next true;
});

//===============================================================================
// Surf.
//===============================================================================
public void Surf() {
	if (!Game.GameData.game_player.can_ride_vehicle_with_follower()) return false;
	move = moves.SURF;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_SURF, false) || (!Core.DEBUG && !movefinder)) {
		return false;
	}
	if (ConfirmMessage(_INTL("The water is a deep blue color... Would you like to use Surf on it?"))) {
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		CancelVehicles;
		HiddenMoveAnimation(movefinder);
		surfbgm = GameData.Metadata.get.surf_BGM;
		if (surfbgm) CueBGM(surfbgm, 0.5);
		StartSurfing;
		return true;
	}
	return false;
}

public void StartSurfing() {
	CancelVehicles;
	Game.GameData.PokemonEncounters.reset_step_count;
	Game.GameData.PokemonGlobal.surfing = true;
	Game.GameData.stats.surf_count += 1;
	UpdateVehicle;
	Game.GameData.game_temp.surf_base_coords = Game.GameData.map_factory.getFacingCoords(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction);
	Game.GameData.game_player.jumpForward;
}

public void EndSurf(_xOffset, _yOffset) {
	if (!Game.GameData.PokemonGlobal.surfing) return false;
	if (Game.GameData.game_player.FacingTerrainTag.can_surf) return false;
	base_coords = new {Game.GameData.game_player.x, Game.GameData.game_player.y};
	if (Game.GameData.game_player.jumpForward) {
		Game.GameData.game_temp.surf_base_coords = base_coords;
		Game.GameData.game_temp.ending_surf = true;
		return true;
	}
	return false;
}

EventHandlers.add(:on_player_interact, :start_surfing,
	block: () => {
		if (Game.GameData.PokemonGlobal.surfing) continue;
		if (Game.GameData.game_map.metadata&.always_bicycle) continue;
		if (!Game.GameData.game_player.FacingTerrainTag.can_surf_freely) continue;
		if (!Game.GameData.game_map.passable(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction, Game.GameData.game_player)) continue;
		Surf;
	}
)

// Do things after a jump to start/end surfing.
EventHandlers.add(:on_step_taken, :surf_jump,
	block: (event) => {
		if (!Game.GameData.scene.is_a(Scene_Map) || !event.is_a(Game_Player)) continue;
		if (!Game.GameData.game_temp.surf_base_coords) continue;
		// Hide the temporary surf base graphic after jumping onto/off it
		Game.GameData.game_temp.surf_base_coords = null;
		// Finish up dismounting from surfing
		if (Game.GameData.game_temp.ending_surf) {
			CancelVehicles;
			Game.GameData.PokemonEncounters.reset_step_count;
			Game.GameData.game_map.autoplayAsCue;   // Play regular map BGM
			Game.GameData.game_temp.ending_surf = false;
		}
	}
)

HiddenMoveHandlers.CanUseMove.Add(Moves.SURF, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_SURF, showmsg)) next false;
	if (Game.GameData.PokemonGlobal.surfing) {
		if (showmsg) Message(_INTL("You're already surfing."));
		next false;
	}
	if (!Game.GameData.game_player.can_ride_vehicle_with_follower()) {
		if (showmsg) Message(_INTL("It can't be used when you have someone with you."));
		next false;
	}
	if (Game.GameData.game_map.metadata&.always_bicycle) {
		if (showmsg) Message(_INTL("Let's enjoy cycling!"));
		next false;
	}
	if (!Game.GameData.game_player.FacingTerrainTag.can_surf_freely ||
		!Game.GameData.game_map.passable(Game.GameData.game_player.x, Game.GameData.game_player.y, Game.GameData.game_player.direction, Game.GameData.game_player)) {
		if (showmsg) Message(_INTL("No surfing here!"));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.SURF, block: (move, pokemon) => {
	Game.GameData.game_temp.in_menu = false;
	CancelVehicles;
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	surfbgm = GameData.Metadata.get.surf_BGM;
	if (surfbgm) CueBGM(surfbgm, 0.5);
	StartSurfing;
	next true;
});

//===============================================================================
// Sweet Scent.
//===============================================================================
public void SweetScent() {
	if (Game.GameData.game_screen.weather_type != types.None) {
		Message(_INTL("The sweet scent faded for some reason..."));
		return;
	}
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	viewport.color.red   = 255;
	viewport.color.green = 32;
	viewport.color.blue  = 32;
	viewport.color.alpha -= 10;
	SEPlay("Sweet Scent");
	start_alpha = viewport.color.alpha;
	duration = 2.0;
	fade_time = 0.4;
	foreach (var delta_t in Wait(duration)) { //Wait(duration) do => |delta_t|
		if (delta_t < duration / 2) {
			viewport.color.alpha = lerp(start_alpha, start_alpha + 128, fade_time, delta_t);
		} else {
			viewport.color.alpha = lerp(start_alpha + 128, start_alpha, fade_time, delta_t - duration + fade_time);
		}
	}
	viewport.dispose;
	SEStop(0.5);
	enctype = Game.GameData.PokemonEncounters.encounter_type;
	if (!enctype || !Game.GameData.PokemonEncounters.encounter_possible_here() ||
		!Encounter(enctype, false)) {
		Message(_INTL("There appears to be nothing here..."));
	}
}

HiddenMoveHandlers.CanUseMove.Add(Moves.SWEETSCENT, block: (move, pkmn, showmsg) => {
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.SWEETSCENT, block: (move, pokemon) => {
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	SweetScent;
	next true;
});

//===============================================================================
// Teleport.
//===============================================================================
HiddenMoveHandlers.CanUseMove.Add(Moves.TELEPORT, block: (move, pkmn, showmsg) => {
	if (!Game.GameData.game_map.metadata&.outdoor_map) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	healing = Game.GameData.PokemonGlobal.healingSpot;
	if (!healing) healing = GameData.PlayerMetadata.get(Game.GameData.player.character_ID)&.home;
	if (!healing) healing = GameData.Metadata.get.home;   // Home
	if (!healing) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	if (!Game.GameData.game_player.can_map_transfer_with_follower()) {
		if (showmsg) Message(_INTL("It can't be used when you have someone with you."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.ConfirmUseMove.Add(Moves.TELEPORT, block: (move, pkmn) => {
	healing = Game.GameData.PokemonGlobal.healingSpot;
	if (!healing) healing = GameData.PlayerMetadata.get(Game.GameData.player.character_ID)&.home;
	if (!healing) healing = GameData.Metadata.get.home;   // Home
	if (!healing) next false;
	mapname = GetMapNameFromId(healing[0]);
	next ConfirmMessage(_INTL("Want to return to the healing spot used last in {1}?", mapname));
});

HiddenMoveHandlers.UseMove.Add(Moves.TELEPORT, block: (move, pokemon) => {
	healing = Game.GameData.PokemonGlobal.healingSpot;
	if (!healing) healing = GameData.PlayerMetadata.get(Game.GameData.player.character_ID)&.home;
	if (!healing) healing = GameData.Metadata.get.home;   // Home
	if (!healing) next false;
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	FadeOutIn do;
		Game.GameData.game_temp.player_new_map_id    = healing[0];
		Game.GameData.game_temp.player_new_x         = healing[1];
		Game.GameData.game_temp.player_new_y         = healing[2];
		Game.GameData.game_temp.player_new_direction = 2;
		DismountBike;
		Game.GameData.scene.transfer_player;
		Game.GameData.game_map.autoplay;
		Game.GameData.game_map.refresh;
	}
	EraseEscapePoint;
	next true;
});

//===============================================================================
// Waterfall.
//===============================================================================
// Starts the ascending of a waterfall.
public void AscendWaterfall() {
	if (Game.GameData.game_player.direction != 8) return;   // Can't ascend if not facing up
	terrain = Game.GameData.game_player.FacingTerrainTag;
	if (!terrain.waterfall && !terrain.waterfall_crest) return;
	Game.GameData.stats.waterfall_count += 1;
	Game.GameData.PokemonGlobal.ascending_waterfall = true;
	Game.GameData.game_player.through = true;
}

// Triggers after finishing each step while ascending/descending a waterfall.
public void TraverseWaterfall() {
	if (Game.GameData.game_player.direction == 2) {   // Facing down; descending
		terrain = Game.GameData.game_player.TerrainTag;
		if ((Core.DEBUG && Input.press(Input.CTRL)) ||
			(!terrain.waterfall && !terrain.waterfall_crest)) {
			Game.GameData.PokemonGlobal.descending_waterfall = false;
			Game.GameData.game_player.through = false;
			return;
		}
		if (!Game.GameData.PokemonGlobal.descending_waterfall) Game.GameData.stats.waterfalls_descended += 1;
		Game.GameData.PokemonGlobal.descending_waterfall = true;
		Game.GameData.game_player.through = true;
	} else if (Game.GameData.PokemonGlobal.ascending_waterfall) {
		terrain = Game.GameData.game_player.TerrainTag;
		if ((Core.DEBUG && Input.press(Input.CTRL)) ||
			(!terrain.waterfall && !terrain.waterfall_crest)) {
			Game.GameData.PokemonGlobal.ascending_waterfall = false;
			Game.GameData.game_player.through = false;
			return;
		}
		Game.GameData.PokemonGlobal.ascending_waterfall = true;
		Game.GameData.game_player.through = true;
	}
}

public void Waterfall() {
	move = moves.WATERFALL;
	movefinder = Game.GameData.player.get_pokemon_with_move(move);
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_WATERFALL, false) || (!Core.DEBUG && !movefinder)) {
		Message(_INTL("A wall of water is crashing down with a mighty roar."));
		return false;
	}
	if (ConfirmMessage(_INTL("It's a large waterfall. Would you like to use Waterfall?"))) {
		speciesname = (movefinder) ? movefinder.name : Game.GameData.player.name;
		Message(_INTL("{1} used {2}!", speciesname, GameData.Move.get(move).name));
		HiddenMoveAnimation(movefinder);
		AscendWaterfall;
		return true;
	}
	return false;
}

EventHandlers.add(:on_player_interact, :waterfall,
	block: () => {
		terrain = Game.GameData.game_player.FacingTerrainTag;
		if (terrain.waterfall) {
			Waterfall;
		} else if (terrain.waterfall_crest) {
			Message(_INTL("A wall of water is crashing down with a mighty roar."));
		}
	}
)

HiddenMoveHandlers.CanUseMove.Add(Moves.WATERFALL, block: (move, pkmn, showmsg) => {
	if (!CheckHiddenMoveBadge(Settings.BADGE_FOR_WATERFALL, showmsg)) next false;
	if (!Game.GameData.game_player.FacingTerrainTag.waterfall) {
		if (showmsg) Message(_INTL("You can't use that here."));
		next false;
	}
	next true;
});

HiddenMoveHandlers.UseMove.Add(Moves.WATERFALL, block: (move, pokemon) => {
	if (!HiddenMoveAnimation(pokemon)) {
		Message(_INTL("{1} used {2}!", pokemon.name, GameData.Move.get(move).name));
	}
	AscendWaterfall;
	next true;
});
