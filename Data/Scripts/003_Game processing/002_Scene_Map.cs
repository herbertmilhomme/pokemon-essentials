//===============================================================================
// Modified Scene_Map class for Pokémon.
//===============================================================================
public partial class Scene_Map {
	public int spritesetGlobal		{ get { return _spritesetGlobal; } }			protected int _spritesetGlobal;
	public int map_renderer		{ get { return _map_renderer; } }			protected int _map_renderer;

	public void spriteset(map_id = -1) {
		if (map_id > 0 && @spritesets[map_id]) return @spritesets[map_id];
		@spritesets.each_value do |i|
			if (i.map == Game.GameData.game_map) return i;
		}
		return @spritesets.values[0];
	}

	public void createSpritesets() {
		if (!@map_renderer || @map_renderer.disposed()) @map_renderer = new TilemapRenderer(Spriteset_Map.viewport);
		if (!@spritesetGlobal) @spritesetGlobal = new Spriteset_Global();
		@spritesets = new List<string>();
		foreach (var map in Game.GameData.map_factory.maps) { //'Game.GameData.map_factory.maps.each' do => |map|
			@spritesets[map.map_id] = new Spriteset_Map(map);
		}
		Game.GameData.map_factory.setSceneStarted(self);
		updateSpritesets(true);
	}

	public void createSingleSpriteset(map) {
		temp = Game.GameData.scene.spriteset.getAnimations;
		@spritesets[map] = new Spriteset_Map(Game.GameData.map_factory.maps[map]);
		Game.GameData.scene.spriteset.restoreAnimations(temp);
		Game.GameData.map_factory.setSceneStarted(self);
		updateSpritesets(true);
	}

	public void disposeSpritesets() {
		if (!@spritesets) return;
		@spritesets.each_key do |i|
			if (!@spritesets[i]) continue;
			@spritesets[i].dispose;
			@spritesets[i] = null;
		}
		@spritesets.clear;
		@spritesets = new List<string>();
	}

	public void dispose() {
		disposeSpritesets;
		@map_renderer.dispose;
		@map_renderer = null;
		@spritesetGlobal.dispose;
		@spritesetGlobal = null;
	}

	public void autofade(mapid) {
		playingBGM = Game.GameData.game_system.playing_bgm;
		playingBGS = Game.GameData.game_system.playing_bgs;
		if (!playingBGM && !playingBGS) return;
		map = load_data(string.Format("Data/Map{0:3}.rxdata", mapid));
		if (playingBGM && map.autoplay_bgm) {
			test_filename = map.bgm.name;
			if (DayNight.isNight() && FileTest.audio_exist("Audio/BGM/" + test_filename + "_n")) test_filename += "_n";
			if (playingBGM.name != test_filename) BGMFade(0.8);
		}
		if (playingBGS && map.autoplay_bgs && playingBGS.name != map.bgs.name) {
			BGMFade(0.8);
		}
		Graphics.frame_reset;
	}

	public void transfer_player(cancel_swimming = true) {
		Game.GameData.game_temp.player_transferring = false;
		CancelVehicles(Game.GameData.game_temp.player_new_map_id, cancel_swimming);
		autofade(Game.GameData.game_temp.player_new_map_id);
		BridgeOff;
		@spritesetGlobal.playersprite.clearShadows;
		if (Game.GameData.game_map.map_id != Game.GameData.game_temp.player_new_map_id) {
			Game.GameData.map_factory.setup(Game.GameData.game_temp.player_new_map_id);
		}
		Game.GameData.game_player.moveto(Game.GameData.game_temp.player_new_x, Game.GameData.game_temp.player_new_y);
		switch (Game.GameData.game_temp.player_new_direction) {
			case 2:  Game.GameData.game_player.turn_down; break;
			case 4:  Game.GameData.game_player.turn_left; break;
			case 6:  Game.GameData.game_player.turn_right; break;
			case 8:  Game.GameData.game_player.turn_up; break;
		}
		Game.GameData.game_player.straighten;
		Game.GameData.game_temp.followers.map_transfer_followers;
		Game.GameData.game_map.update;
		disposeSpritesets;
		RPG.Cache.clear;
		createSpritesets;
		if (Game.GameData.game_temp.transition_processing) {
			Game.GameData.game_temp.transition_processing = false;
			Graphics.transition;
		}
		Game.GameData.game_map.autoplay;
		Graphics.frame_reset;
		Input.update;
	}

	public void call_menu() {
		Game.GameData.game_temp.menu_calling = false;
		Game.GameData.game_temp.in_menu = true;
		Game.GameData.game_player.straighten;
		Game.GameData.game_map.update;
		sscene = new PokemonPauseMenu_Scene();
		sscreen = new PokemonPauseMenu(sscene);
		sscreen.StartPokemonMenu;
		Game.GameData.game_temp.in_menu = false;
	}

	public void call_debug() {
		Game.GameData.game_temp.debug_calling = false;
		PlayDecisionSE;
		Game.GameData.game_player.straighten;
		FadeOutIn { DebugMenu };
	}

	public void miniupdate() {
		Game.GameData.game_temp.in_mini_update = true;
		do { //loop; while (true);
			Game.GameData.game_player.update;
			updateMaps;
			Game.GameData.game_system.update;
			Game.GameData.game_screen.update;
			if (!Game.GameData.game_temp.player_transferring) break;
			transfer_player(false);
			if (Game.GameData.game_temp.transition_processing) break;
		}
		updateSpritesets;
		Game.GameData.game_temp.in_mini_update = false;
	}

	public void updateMaps() {
		foreach (var map in Game.GameData.map_factory.maps) { //'Game.GameData.map_factory.maps.each' do => |map|
			map.update;
		}
		Game.GameData.map_factory.updateMaps(self);
	}

	public void updateSpritesets(refresh = false) {
		if (!@spritesets) @spritesets = new List<string>();
		foreach (var map in Game.GameData.map_factory.maps) { //'Game.GameData.map_factory.maps.each' do => |map|
			if (!@spritesets[map.map_id]) @spritesets[map.map_id] = new Spriteset_Map(map);
		}
		keys = @spritesets.keys.clone;
		foreach (var i in keys) { //'keys.each' do => |i|
			if (Game.GameData.map_factory.hasMap(i)) {
				@spritesets[i].update;
			} else {
				@spritesets[i]&.dispose;
				@spritesets[i] = null;
				@spritesets.delete(i);
			}
		}
		@spritesetGlobal.update;
		DayNightTint(@map_renderer);
		if (refresh) @map_renderer.refresh;
		@map_renderer.update;
		EventHandlers.trigger(:on_frame_update);
	}

	public void update() {
		do { //loop; while (true);
			MapInterpreter.update;
			Game.GameData.game_player.update;
			updateMaps;
			Game.GameData.game_system.update;
			Game.GameData.game_screen.update;
			if (!Game.GameData.game_temp.player_transferring) break;
			transfer_player(false);
			if (Game.GameData.game_temp.transition_processing) break;
		}
		updateSpritesets;
		if (Game.GameData.game_temp.title_screen_calling) {
			SaveData.mark_values_as_unloaded;
			Game.GameData.scene = CallTitle;
			return;
		}
		if (Game.GameData.game_temp.transition_processing) {
			Game.GameData.game_temp.transition_processing = false;
			if (Game.GameData.game_temp.transition_name == "") {
				Graphics.transition;
			} else {
				Graphics.transition(40, "Graphics/Transitions/" + Game.GameData.game_temp.transition_name);
			}
		}
		if (Game.GameData.game_temp.message_window_showing) return;
		if (!MapInterpreterRunning() && !Game.GameData.PokemonGlobal.forced_movement()) {
			if (Input.trigger(Input.USE)) {
				Game.GameData.game_temp.interact_calling = true;
			} else if (Input.trigger(Input.ACTION)) {
				if (!Game.GameData.game_system.menu_disabled && !Game.GameData.game_player.moving()) {
					Game.GameData.game_temp.menu_calling = true;
					Game.GameData.game_temp.menu_beep = true;
				}
			} else if (Input.trigger(Input.SPECIAL)) {
				if (!Game.GameData.game_player.moving()) Game.GameData.game_temp.ready_menu_calling = true;
			} else if (Input.press(Input.F9)) {
				if (Core.DEBUG) Game.GameData.game_temp.debug_calling = true;
			}
		}
		if (!Game.GameData.game_player.moving()) {
			if (Game.GameData.game_temp.menu_calling) {
				call_menu;
			} else if (Game.GameData.game_temp.debug_calling) {
				call_debug;
			} else if (Game.GameData.game_temp.ready_menu_calling) {
				Game.GameData.game_temp.ready_menu_calling = false;
				Game.GameData.game_player.straighten;
				UseKeyItem;
			} else if (Game.GameData.game_temp.interact_calling) {
				Game.GameData.game_temp.interact_calling = false;
				triggered = false;
				// Try to trigger an event the player is standing on, and one in front of
				// the player
				if (!Game.GameData.game_temp.in_mini_update) {
					triggered ||= Game.GameData.game_player.check_event_trigger_here([0]);
					if (!triggered) triggered ||= Game.GameData.game_player.check_event_trigger_there(new {0, 2});
				}
				// Try to trigger an interaction with a tile
				if (!triggered) {
					Game.GameData.game_player.straighten;
					EventHandlers.trigger(:on_player_interact);
				}
			}
		}
	}

	public void main() {
		createSpritesets;
		Graphics.transition;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			update;
			if (Game.GameData.scene != self) break;
		}
		Graphics.freeze;
		dispose;
		if (Game.GameData.game_temp.title_screen_calling) {
			if (MapInterpreterRunning()) MapInterpreter.command_end;
			Game.GameData.game_temp.last_uptime_refreshed_play_time = null;
			Game.GameData.game_temp.title_screen_calling = false;
			BGMFade(1.0);
			Graphics.transition;
			Graphics.freeze;
		}
	}
}
