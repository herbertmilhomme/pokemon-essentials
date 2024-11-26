//===============================================================================
//
//===============================================================================
public partial class Spriteset_Global {
	public int playersprite		{ get { return _playersprite; } }			protected int _playersprite;

	@@viewport2 = new Viewport(0, 0, Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);
	@@viewport2.z = 200;

	public void initialize() {
		@map_id = Game.GameData.game_map&.map_id || 0;
		@follower_sprites = new FollowerSprites(Spriteset_Map.viewport);
		@playersprite = new Sprite_Character(Spriteset_Map.viewport, Game.GameData.game_player);
		@weather = new RPG.Weather(Spriteset_Map.viewport);
		@picture_sprites = new List<string>();
		(1..100).each do |i|
			@picture_sprites.Add(new Sprite_Picture(@@viewport2, Game.GameData.game_screen.pictures[i]));
		}
		@timer_sprite = new Sprite_Timer();
		update;
	}

	public void dispose() {
		@follower_sprites.dispose;
		@follower_sprites = null;
		@playersprite.dispose;
		@playersprite = null;
		@weather.dispose;
		@weather = null;
		@picture_sprites.each(sprite => sprite.dispose);
		@picture_sprites.clear;
		@timer_sprite.dispose;
		@timer_sprite = null;
	}

	public void update() {
		@follower_sprites.update;
		@playersprite.update;
		if (@weather.type != Game.GameData.game_screen.weather_type) {
			@weather.fade_in(Game.GameData.game_screen.weather_type, Game.GameData.game_screen.weather_max, Game.GameData.game_screen.weather_duration);
		}
		if (@map_id != Game.GameData.game_map.map_id) {
			offsets = Game.GameData.map_factory.getRelativePos(@map_id, 0, 0, Game.GameData.game_map.map_id, 0, 0);
			if (offsets == new {0, 0}) {
				@weather.ox_offset = 0;
				@weather.oy_offset = 0;
			} else {
				@weather.ox_offset += offsets[0] * Game_Map.TILE_WIDTH;
				@weather.oy_offset += offsets[1] * Game_Map.TILE_HEIGHT;
			}
			@map_id = Game.GameData.game_map.map_id;
		}
		@weather.ox = (int)Math.Round(Game.GameData.game_map.display_x / Game_Map.X_SUBPIXELS);
		@weather.oy = (int)Math.Round(Game.GameData.game_map.display_y / Game_Map.Y_SUBPIXELS);
		@weather.update;
		@picture_sprites.each(sprite => sprite.update);
		@timer_sprite.update;
	}
}
