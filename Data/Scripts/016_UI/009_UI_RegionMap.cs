//===============================================================================
//
//===============================================================================
public partial class MapBottomSprite : Sprite {
	public int mapname		{ get { return _mapname; } set { _mapname = value; } }			protected int _mapname;
	public int maplocation		{ get { return _maplocation; } }			protected int _maplocation;

	TEXT_MAIN_COLOR   = new Color(248, 248, 248);
	TEXT_SHADOW_COLOR = new Color(0, 0, 0);

	public override void initialize(viewport = null) {
		base.initialize(viewport);
		@mapname     = "";
		@maplocation = "";
		@mapdetails  = "";
		self.bitmap = new Bitmap(Graphics.width, Graphics.height);
		SetSystemFont(self.bitmap);
		refresh;
	}

	public int mapname { set {
		if (@mapname == value) return;
		@mapname = value;
		refresh;
		}
	}

	public int maplocation { set {
		if (@maplocation == value) return;
		@maplocation = value;
		refresh;
		}
	}

	// From Wichu
	public int mapdetails { set {
		if (@mapdetails == value) return;
		@mapdetails = value;
		refresh;
		}	}

	public void refresh() {
		bitmap.clear;
		textpos = new {
			new {@mapname,                     18,   4, :left, TEXT_MAIN_COLOR, TEXT_SHADOW_COLOR},
			new {@maplocation,                 18, 360, :left, TEXT_MAIN_COLOR, TEXT_SHADOW_COLOR},
			new {@mapdetails, Graphics.width - 16, 360, :right, TEXT_MAIN_COLOR, TEXT_SHADOW_COLOR}
		}
		DrawTextPositions(bitmap, textpos);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonRegionMap_Scene {
	public const int LEFT          = 0;
	public const int TOP           = 0;
	public const int RIGHT         = 29;
	public const int BOTTOM        = 19;
	public const int SQUARE_WIDTH  = 16;
	public const int SQUARE_HEIGHT = 16;

	public void initialize(region = - 1, wallmap = true) {
		@region  = region;
		@wallmap = wallmap;
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(as_editor = false, fly_map = false) {
		@editor   = as_editor;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@fly_map = fly_map;
		@mode    = fly_map ? 1 : 0;
		map_metadata = Game.GameData.game_map.metadata;
		playerpos = (map_metadata) ? map_metadata.town_map_position : null;
		if (!playerpos) {
			mapindex = 0;
			@map     = GameData.TownMap.get(0);
			@map_x   = LEFT;
			@map_y   = TOP;
		} else if (@region >= 0 && @region != playerpos[0] && GameData.TownMap.exists(@region)) {
			mapindex = @region;
			@map     = GameData.TownMap.get(@region);
			@map_x   = LEFT;
			@map_y   = TOP;
		} else {
			mapindex = playerpos[0];
			@map     = GameData.TownMap.get(playerpos[0]);
			@map_x   = playerpos[1];
			@map_y   = playerpos[2];
			mapsize  = map_metadata.town_map_size;
			if (mapsize && mapsize[0] && mapsize[0] > 0) {
				sqwidth  = mapsize[0];
				sqheight = (mapsize[1].length.to_f / mapsize[0]).ceil;
				@map_x += (int)Math.Floor(Game.GameData.game_player.x * sqwidth / Game.GameData.game_map.width) if sqwidth > 1;
				@map_y += (int)Math.Floor(Game.GameData.game_player.y * sqheight / Game.GameData.game_map.height) if sqheight > 1;
			}
		}
		if (!@map) {
			Message(_INTL("The map data cannot be found."));
			return false;
		}
		addBackgroundOrColoredPlane(@sprites, "background", "Town Map/bg", Color.black, @viewport);
		@sprites["map"] = new IconSprite(0, 0, @viewport);
		@sprites["map"].setBitmap($"Graphics/UI/Town Map/{@map.filename}");
		@sprites["map"].x += (Graphics.width - @sprites["map"].bitmap.width) / 2;
		@sprites["map"].y += (Graphics.height - @sprites["map"].bitmap.height) / 2;
		foreach (var graphic in Settings.REGION_MAP_EXTRAS) { //'Settings.REGION_MAP_EXTRAS.each' do => |graphic|
			if (graphic[0] != mapindex || !location_shown(graphic)) continue;
			if (!@sprites["map2"]) {
				@sprites["map2"] = new BitmapSprite(480, 320, @viewport);
				@sprites["map2"].x = @sprites["map"].x;
				@sprites["map2"].y = @sprites["map"].y;
			}
			DrawImagePositions(
				@sprites["map2"].bitmap,
				new {$"Graphics/UI/Town Map/{graphic[4]}", graphic[2] * SQUARE_WIDTH, graphic[3] * SQUARE_HEIGHT}
			);
		}
		@sprites["mapbottom"] = new MapBottomSprite(@viewport);
		@sprites["mapbottom"].mapname     = @map.name;
		@sprites["mapbottom"].maplocation = GetMapLocation(@map_x, @map_y);
		@sprites["mapbottom"].mapdetails  = GetMapDetails(@map_x, @map_y);
		if (playerpos && mapindex == playerpos[0]) {
			@sprites["player"] = new IconSprite(0, 0, @viewport);
			@sprites["player"].setBitmap(GameData.TrainerType.player_map_icon_filename(Game.GameData.player.trainer_type));
			@sprites["player"].x = point_x_to_screen_x(@map_x);
			@sprites["player"].y = point_y_to_screen_y(@map_y);
		}
		k = 0;
		(LEFT..RIGHT).each do |i|
			(TOP..BOTTOM).each do |j|
				healspot = GetHealingSpot(i, j);
				if (!healspot || !Game.GameData.PokemonGlobal.visitedMaps[healspot[0]]) continue;
				@sprites[$"point{k}"] = AnimatedSprite.create("Graphics/UI/Town Map/icon_fly", 2, 16);
				@sprites[$"point{k}"].viewport = @viewport;
				@sprites[$"point{k}"].x        = point_x_to_screen_x(i);
				@sprites[$"point{k}"].y        = point_y_to_screen_y(j);
				@sprites[$"point{k}"].visible  = @mode == 1;
				@sprites[$"point{k}"].play;
				k += 1;
			}
		}
		@sprites["cursor"] = AnimatedSprite.create("Graphics/UI/Town Map/cursor", 2, 5);
		@sprites["cursor"].viewport = @viewport;
		@sprites["cursor"].x        = point_x_to_screen_x(@map_x);
		@sprites["cursor"].y        = point_y_to_screen_y(@map_y);
		@sprites["cursor"].play;
		@sprites["help"] = new BitmapSprite(Graphics.width, 32, @viewport);
		SetSystemFont(@sprites["help"].bitmap);
		refresh_fly_screen;
		@changed = false;
		FadeInAndShow(@sprites) { Update };
	}

	public void EndScene() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void point_x_to_screen_x(x) {
		return (-SQUARE_WIDTH / 2) + (x * SQUARE_WIDTH) + ((Graphics.width - @sprites["map"].bitmap.width) / 2);
	}

	public void point_y_to_screen_y(y) {
		return (-SQUARE_HEIGHT / 2) + (y * SQUARE_HEIGHT) + ((Graphics.height - @sprites["map"].bitmap.height) / 2);
	}

	public bool location_shown(point) {
		if (@wallmap) return point[5];
		return point[1] > 0 && Game.GameData.game_switches[point[1]];
	}

	public void SaveMapData() {
		GameData.TownMap.save;
		Compiler.write_town_map;
	}

	public void GetMapLocation(x, y) {
		if (!@map.point) return "";
		@map.point.each do |point|
			if (point[0] != x || point[1] != y) continue;
			if (point[7] && (@wallmap || point[7] <= 0 || !Game.GameData.game_switches[point[7]])) return "";
			name = GetMessageFromHash(MessageTypes.REGION_LOCATION_NAMES, point[2]);
			return (@editor) ? point[2] : name;
		}
		return "";
	}

	public void ChangeMapLocation(x, y) {
		if (!@editor || !@map.point) return "";
		point = @map.point.select(loc => loc[0] == x && loc[1] == y)[0];
		currentobj  = point;
		currentname = (point) ? point[2] : "";
		currentname = MessageFreeText(_INTL("Set the name for this point."), currentname, false, 250) { Update };
		if (currentname) {
			if (currentobj) {
				currentobj[2] = currentname;
			} else {
				newobj = new {x, y, currentname, ""};
				@map.point.Add(newobj);
			}
			@changed = true;
		}
	}

	public void GetMapDetails(x, y) {
		if (!@map.point) return "";
		@map.point.each do |point|
			if (point[0] != x || point[1] != y) continue;
			if (point[7] && (@wallmap || point[7] <= 0 || !Game.GameData.game_switches[point[7]])) return "";
			if (!point[3]) return "";
			mapdesc = GetMessageFromHash(MessageTypes.REGION_LOCATION_DESCRIPTIONS, point[3]);
			return (@editor) ? point[3] : mapdesc;
		}
		return "";
	}

	public void GetHealingSpot(x, y) {
		if (!@map.point) return null;
		@map.point.each do |point|
			if (point[0] != x || point[1] != y) continue;
			if (point[7] && (@wallmap || point[7] <= 0 || !Game.GameData.game_switches[point[7]])) return null;
			return (point[4] && point[5] && point[6]) ? new {point[4], point[5], point[6]} : null;
		}
		return null;
	}

	public void refresh_fly_screen() {
		if (@fly_map || !Settings.CAN_FLY_FROM_TOWN_MAP || !CanFly()) return;
		@sprites["help"].bitmap.clear;
		text = (@mode == 0) ? _INTL("ACTION: Fly") : _INTL("ACTION: Cancel Fly");
		DrawTextPositions(
			@sprites["help"].bitmap,
			new {text, Graphics.width - 16, 4, :right, new Color(248, 248, 248), Color.black}
		);
		@sprites.each do |key, sprite|
			if (!key.Contains("point")) continue;
			sprite.visible = (@mode == 1);
			sprite.frame   = 0;
		}
	}

	public void MapScene() {
		x_offset = 0;
		y_offset = 0;
		new_x    = 0;
		new_y    = 0;
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (x_offset != 0 || y_offset != 0) {
				if (x_offset != 0) {
					@sprites["cursor"].x = lerp(new_x - x_offset, new_x, 0.1, timer_start, System.uptime);
					if (@sprites["cursor"].x == new_x) x_offset = 0;
				}
				if (y_offset != 0) {
					@sprites["cursor"].y = lerp(new_y - y_offset, new_y, 0.1, timer_start, System.uptime);
					if (@sprites["cursor"].y == new_y) y_offset = 0;
				}
				if (x_offset != 0 || y_offset != 0) continue;
			}
			ox = 0;
			oy = 0;
			switch (Input.dir8) {
				case 1: case 2: case 3:
					if (@map_y < BOTTOM) oy = 1;
					break;
				case 7: case 8: case 9:
					if (@map_y > TOP) oy = -1;
					break;
			}
			switch (Input.dir8) {
				case 1: case 4: case 7:
					if (@map_x > LEFT) ox = -1;
					break;
				case 3: case 6: case 9:
					if (@map_x < RIGHT) ox = 1;
					break;
			}
			if (ox != 0 || oy != 0) {
				@map_x += ox;
				@map_y += oy;
				x_offset = ox * SQUARE_WIDTH;
				y_offset = oy * SQUARE_HEIGHT;
				new_x = @sprites["cursor"].x + x_offset;
				new_y = @sprites["cursor"].y + y_offset;
				timer_start = System.uptime;
			}
			@sprites["mapbottom"].maplocation = GetMapLocation(@map_x, @map_y);
			@sprites["mapbottom"].mapdetails  = GetMapDetails(@map_x, @map_y);
			if (Input.trigger(Input.BACK)) {
				if (@editor && @changed) {
					if (ConfirmMessage(_INTL("Save changes?")) { Update }) SaveMapData;
					if (ConfirmMessage(_INTL("Exit from the map?")) { Update }) break;
				} else {
					break;
				}
			} else if (Input.trigger(Input.USE) && @mode == 1) {   // Choosing an area to fly to
				healspot = GetHealingSpot(@map_x, @map_y);
				if (healspot && (Game.GameData.PokemonGlobal.visitedMaps[healspot[0]] ||
					(Core.DEBUG && Input.press(Input.CTRL)))) {
					if (@fly_map) return healspot;
					name = GetMapNameFromId(healspot[0]);
					if (ConfirmMessage(_INTL("Would you like to use Fly to go to {1}?", name)) { Update }) return healspot;
				}
			} else if (Input.trigger(Input.USE) && @editor) {   // Intentionally after other USE input check
				ChangeMapLocation(@map_x, @map_y);
			} else if (Input.trigger(Input.ACTION) && Settings.CAN_FLY_FROM_TOWN_MAP &&
						!@wallmap && !@fly_map && CanFly()) {
				PlayDecisionSE;
				@mode = (@mode == 1) ? 0 : 1;
				refresh_fly_screen;
			}
		}
		PlayCloseMenuSE;
		return null;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonRegionMapScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartFlyScreen() {
		@scene.StartScene(false, true);
		ret = @scene.MapScene;
		@scene.EndScene;
		return ret;
	}

	public void StartScreen() {
		@scene.StartScene(Core.DEBUG);
		ret = @scene.MapScene;
		@scene.EndScene;
		return ret;
	}
}

//===============================================================================
//
//===============================================================================
public void ShowMap(region = -1, wallmap = true) {
	FadeOutIn do;
		scene = new PokemonRegionMap_Scene(region, wallmap);
		screen = new PokemonRegionMapScreen(scene);
		ret = screen.StartScreen;
		if (ret && !wallmap) Game.GameData.game_temp.fly_destination = ret;
	}
}
