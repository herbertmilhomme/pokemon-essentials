//===============================================================================
// Miniature game map drawing.
//===============================================================================
public partial class MapSprite {
	public void initialize(map, viewport = null) {
		@sprite = new Sprite(viewport);
		@sprite.bitmap = createMinimap(map);
		@sprite.x = (Graphics.width / 2) - (@sprite.bitmap.width / 2);
		@sprite.y = (Graphics.height / 2) - (@sprite.bitmap.height / 2);
	}

	public void dispose() {
		@sprite.bitmap.dispose;
		@sprite.dispose;
	}

	public int z { set {
		@sprite.z = value;
		}
	}

	public void getXY() {
		if (!Input.trigger(Input.MOUSELEFT)) return null;
		mouse = Mouse.getMousePos(true);
		if (!mouse) return null;
		if (mouse[0] < @sprite.x || mouse[0] >= @sprite.x + @sprite.bitmap.width) {
			return null;
		}
		if (mouse[1] < @sprite.y || mouse[1] >= @sprite.y + @sprite.bitmap.height) {
			return null;
		}
		x = mouse[0] - @sprite.x;
		y = mouse[1] - @sprite.y;
		return new {x / 4, y / 4};
	}
}

//===============================================================================
//
//===============================================================================
public partial class SelectionSprite : Sprite {
	public void initialize(viewport = null) {
		@sprite = new Sprite(viewport);
		@sprite.bitmap = null;
		@sprite.z = 2;
		@othersprite = null;
	}

	public bool disposed() {
		return @sprite.disposed();
	}

	public void dispose() {
		@sprite.bitmap&.dispose;
		@othersprite = null;
		@sprite.dispose;
	}

	public int othersprite { set {
		@othersprite = value;
		if (@othersprite && !@othersprite.disposed() &&
			@othersprite.bitmap && !@othersprite.bitmap.disposed()) {
			@sprite.bitmap = DoEnsureBitmap(
				@sprite.bitmap, @othersprite.bitmap.width, @othersprite.bitmap.height
			);
			red = new Color(255, 0, 0);
			@sprite.bitmap.clear;
			@sprite.bitmap.fill_rect(0, 0, @othersprite.bitmap.width, 2, red);
			@sprite.bitmap.fill_rect(0, @othersprite.bitmap.height - 2,
															@othersprite.bitmap.width, 2, red);
			@sprite.bitmap.fill_rect(0, 0, 2, @othersprite.bitmap.height, red);
			@sprite.bitmap.fill_rect(@othersprite.bitmap.width - 2, 0, 2,
															@othersprite.bitmap.height, red);
			}
	}
	}

	public void update() {
		if (@othersprite && !@othersprite.disposed()) {
			@sprite.visible = @othersprite.visible;
			@sprite.x = @othersprite.x;
			@sprite.y = @othersprite.y;
		} else {
			@sprite.visible = false;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class RegionMapSprite {
	public void initialize(map, viewport = null) {
		@sprite = new Sprite(viewport);
		@sprite.bitmap = createRegionMap(map);
		@sprite.x = (Graphics.width / 2) - (@sprite.bitmap.width / 2);
		@sprite.y = (Graphics.height / 2) - (@sprite.bitmap.height / 2);
	}

	public void dispose() {
		@sprite.bitmap.dispose;
		@sprite.dispose;
	}

	public int z { set {
		@sprite.z = value;
		}
	}

	public void createRegionMap(map) {
		town_map = GameData.TownMap.get(map);
		bitmap = new AnimatedBitmap($"Graphics/UI/Town Map/{town_map.filename}").deanimate;
		retbitmap = new Bitmap(bitmap.width / 2, bitmap.height / 2);
		retbitmap.stretch_blt(
			new Rect(0, 0, bitmap.width / 2, bitmap.height / 2),
			bitmap,
			new Rect(0, 0, bitmap.width, bitmap.height)
		);
		bitmap.dispose;
		return retbitmap;
	}

	public void getXY() {
		if (!Input.trigger(Input.MOUSELEFT)) return null;
		mouse = Mouse.getMousePos(true);
		if (!mouse) return null;
		if (mouse[0] < @sprite.x || mouse[0] >= @sprite.x + @sprite.bitmap.width) {
			return null;
		}
		if (mouse[1] < @sprite.y || mouse[1] >= @sprite.y + @sprite.bitmap.height) {
			return null;
		}
		x = mouse[0] - @sprite.x;
		y = mouse[1] - @sprite.y;
		return new {x / 8, y / 8};
	}
}

//===============================================================================
// Visual Editor (map connections).
//===============================================================================
public partial class MapScreenScene {
	public void getMapSprite(id) {
		if (!@mapsprites[id]) {
			@mapsprites[id] = new Sprite(@viewport);
			@mapsprites[id].z = 0;
			@mapsprites[id].bitmap = null;
		}
		if (!@mapsprites[id].bitmap || @mapsprites[id].bitmap.disposed()) {
			@mapsprites[id].bitmap = createMinimap(id);
		}
		return @mapsprites[id];
	}

	public void close() {
		DisposeSpriteHash(@sprites);
		DisposeSpriteHash(@mapsprites);
		@viewport.dispose;
	}

	public void setMapSpritePos(id, x, y) {
		sprite = getMapSprite(id);
		sprite.x = x;
		sprite.y = y;
		sprite.visible = true;
	}

	public void putNeighbors(id, sprites) {
		conns = @mapconns;
		mapsprite = getMapSprite(id);
		dispx = mapsprite.x;
		dispy = mapsprite.y;
		foreach (var conn in conns) { //'conns.each' do => |conn|
			if (conn[0] == id) {
				b = sprites.any(i => i == conn[3]);
				if (!b) {
					x = ((conn[1] - conn[4]) * 4) + dispx;
					y = ((conn[2] - conn[5]) * 4) + dispy;
					setMapSpritePos(conn[3], x, y);
					sprites.Add(conn[3]);
					putNeighbors(conn[3], sprites);
				}
			} else if (conn[3] == id) {
				b = sprites.any(i => i == conn[0]);
				if (!b) {
					x = ((conn[4] - conn[1]) * 4) + dispx;
					y = ((conn[5] - conn[2]) * 4) + dispy;
					setMapSpritePos(conn[0], x, y);
					sprites.Add(conn[3]);
					putNeighbors(conn[0], sprites);
				}
			}
		}
	}

	public bool hasConnections(conns, id) {
		foreach (var conn in conns) { //'conns.each' do => |conn|
			if (conn[0] == id || conn[3] == id) return true;
		}
		return false;
	}

	public bool connectionsSymmetric(conn1, conn2) {
		if (conn1[0] == conn2[0]) {
			// Equality
			if (conn1[1] != conn2[1]) return false;
			if (conn1[2] != conn2[2]) return false;
			if (conn1[3] != conn2[3]) return false;
			if (conn1[4] != conn2[4]) return false;
			if (conn1[5] != conn2[5]) return false;
			return true;
		} else if (conn1[0] == conn2[3]) {
			// Symmetry
			if (conn1[1] != -conn2[1]) return false;
			if (conn1[2] != -conn2[2]) return false;
			if (conn1[3] != conn2[0]) return false;
			if (conn1[4] != -conn2[4]) return false;
			if (conn1[5] != -conn2[5]) return false;
			return true;
		}
		return false;
	}

	public void removeOldConnections(ret, mapid) {
		ret.delete_if(conn => conn[0] == mapid || conn[3] == mapid);
	}

	// Returns the maps within _keys_ that are directly connected to this map, _map_.
	public void getDirectConnections(keys, map) {
		thissprite = getMapSprite(map);
		thisdims = MapFactoryHelper.getMapDims(map);
		ret = new List<string>();
		foreach (var i in keys) { //'keys.each' do => |i|
			if (i == map) continue;
			othersprite = getMapSprite(i);
			otherdims = MapFactoryHelper.getMapDims(i);
			x1 = (thissprite.x - othersprite.x) / 4;
			y1 = (thissprite.y - othersprite.y) / 4;
			if (x1 == otherdims[0] || x1 == -thisdims[0] ||
				y1 == otherdims[1] || y1 == -thisdims[1]) {
				ret.Add(i);
			}
		}
		// If no direct connections, add an indirect connection
		if (ret.length == 0) {
			key = (map == keys[0]) ? keys[1] : keys[0];
			ret.Add(key);
		}
		return ret;
	}

	public void generateConnectionData() {
		ret = new List<string>();
		// Create a clone of current map connection
		@mapconns.each do |conn|
			ret.Add(conn.clone);
		}
		keys = @mapsprites.keys;
		if (keys.length < 2) return ret;
		// Remove all connections containing any sprites on the canvas from the array
		foreach (var i in keys) { //'keys.each' do => |i|
			removeOldConnections(ret, i);
		}
		// Rebuild connections
		foreach (var i in keys) { //'keys.each' do => |i|
			refs = getDirectConnections(keys, i);
			foreach (var refmap in refs) { //'refs.each' do => |refmap|
				othersprite = getMapSprite(i);
				refsprite = getMapSprite(refmap);
				c1 = (refsprite.x - othersprite.x) / 4;
				c2 = (refsprite.y - othersprite.y) / 4;
				conn = new {refmap, 0, 0, i, c1, c2};
				j = 0;
				while (j < ret.length && !connectionsSymmetric(ret[j], conn)) {
					j += 1;
				}
				if (j == ret.length) ret.Add(conn);
			}
		}
		return ret;
	}

	public void serializeConnectionData() {
		conndata = generateConnectionData;
		save_data(conndata, "Data/map_connections.dat");
		Compiler.write_connections;
		@mapconns = conndata;
	}

	public void putSprite(id) {
		addSprite(id);
		putNeighbors(id, new List<string>());
	}

	public void addSprite(id) {
		mapsprite = getMapSprite(id);
		x = (Graphics.width - mapsprite.bitmap.width) / 2;
		y = (Graphics.height - mapsprite.bitmap.height) / 2;
		mapsprite.x = x.ToInt() & ~3;
		mapsprite.y = y.ToInt() & ~3;
	}

	public void saveMapSpritePos() {
		@mapspritepos.clear;
		@mapsprites.each_key do |i|
			s = @mapsprites[i];
			if (s && !s.disposed()) @mapspritepos[i] = new {s.x, s.y};
		}
	}

	public void mapScreen() {
		@sprites = new List<string>();
		@mapsprites = new List<string>();
		@mapspritepos = new List<string>();
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@lasthitmap = -1;
		@lastclick = -1;
		@oldmousex = null;
		@oldmousey = null;
		@dragging = false;
		@dragmapid = -1;
		@dragOffsetX = 0;
		@dragOffsetY = 0;
		@selmapid = -1;
		@sprites["background"] = new ColoredPlane(new Color(160, 208, 240), @viewport);
		@sprites["selsprite"] = new SelectionSprite(@viewport);
		@sprites["title"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("D: Help"), 0, Graphics.height - 64, Graphics.width, 64, @viewport
		);
		@sprites["title"].z = 2;
		@mapinfos = LoadMapInfos;
		conns = MapFactoryHelper.getMapConnections;
		@mapconns = new List<string>();
		foreach (var map_conns in conns) { //'conns.each' do => |map_conns|
			if (!map_conns) continue;
			foreach (var c in map_conns) { //'map_conns.each' do => |c|
				if (@mapconns.none(x => x[0] == c[0] && x[3] == c[3])) @mapconns.Add(c.clone);
			}
		}
		if (Game.GameData.game_map) {
			@currentmap = Game.GameData.game_map.map_id;
		} else {
			@currentmap = (Game.GameData.data_system) ? Game.GameData.data_system.edit_map_id : 1;
		}
		putSprite(@currentmap);
	}

	public void setTopSprite(id) {
		@mapsprites.each_key do |i|
			@mapsprites[i].z = (i == id) ? 1 : 0;
		}
	}

	public void helpWindow() {
		helptext = _INTL("A: Add map to canvas") + "\n";
		helptext += _INTL("DEL: Delete map from canvas") + "\n";
		helptext += _INTL("S: Go to another map") + "\n";
		helptext += _INTL("Click to select a map") + "\n";
		helptext += _INTL("Double-click: Edit map's metadata") + "\n";
		helptext += _INTL("Drag map to move it") + "\n";
		helptext += _INTL("Arrow keys/drag canvas: Move around canvas");
		title = Window_UnformattedTextPokemon.newWithSize(
			helptext, 0, 0, Graphics.width * 8 / 10, Graphics.height, @viewport
		);
		title.z = 2;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) break;
		}
		Input.update;
		title.dispose;
	}

	public void getMapRect(mapid) {
		sprite = getMapSprite(mapid);
		if (!sprite) return null;
		return new {sprite.x, sprite.y,
						sprite.x + sprite.bitmap.width, sprite.y + sprite.bitmap.height};
	}

	public void onDoubleClick(map_id) {
		if (map_id > 0) EditMapMetadata(map_id);
	}

	public void onClick(mapid, x, y) {
		if (@lastclick > 0 && System.uptime - @lastclick <= 0.5) {
			onDoubleClick(mapid);
			@lastclick = -1;
		} else {
			@lastclick = System.uptime;
			if (mapid >= 0) {
				@dragging = true;
				@dragmapid = mapid;
				sprite = getMapSprite(mapid);
				@sprites["selsprite"].othersprite = sprite;
				@selmapid = mapid;
				@dragOffsetX = sprite.x - x;
				@dragOffsetY = sprite.y - y;
				setTopSprite(mapid);
			} else {
				@sprites["selsprite"].othersprite = null;
				@dragging = true;
				@dragmapid = mapid;
				@selmapid = -1;
				@dragOffsetX = x;
				@dragOffsetY = y;
				saveMapSpritePos;
			}
		}
	}

	public void onRightClick(mapid, x, y) {
//   echoln $"rightclick ({mapid})"
	}

	public void onMouseUp(mapid) {
//   echoln $"mouseup ({mapid})"
		if (@dragging) @dragging = false;
	}

	public void onRightMouseUp(mapid) {
//   echoln $"rightmouseup ({mapid})"
	}

	public void onMouseOver(mapid, x, y) {
//   echoln $"mouseover ({mapid},{x},{y})"
	}

	public void onMouseMove(mapid, x, y) {
//   echoln $"mousemove ({mapid},{x},{y})"
		if (@dragging) {
			if (@dragmapid >= 0) {
				sprite = getMapSprite(@dragmapid);
				x += @dragOffsetX;
				y += @dragOffsetY;
				sprite.x = x & ~3;
				sprite.y = y & ~3;
				@sprites["title"].text = string.Format("D: Help [{1:03d}: {2:s}]", mapid, @mapinfos[@dragmapid].name);
			} else {
				xpos = x - @dragOffsetX;
				ypos = y - @dragOffsetY;
				@mapspritepos.each_key do |i|
					sprite = getMapSprite(i);
					sprite.x = (@mapspritepos[i][0] + xpos) & ~3;
					sprite.y = (@mapspritepos[i][1] + ypos) & ~3;
				}
				@sprites["title"].text = _INTL("D: Help");
			}
		} else if (mapid >= 0) {
			@sprites["title"].text = string.Format("D: Help [{1:03d}: {2:s}]", mapid, @mapinfos[mapid].name);
		} else {
			@sprites["title"].text = _INTL("D: Help");
		}
	}

	public void hittest(x, y) {
		@mapsprites.each_key do |i|
			sx = @mapsprites[i].x;
			sy = @mapsprites[i].y;
			sr = sx + @mapsprites[i].bitmap.width;
			sb = sy + @mapsprites[i].bitmap.height;
			if (x >= sx && x < sr && y >= sy && y < sb) return i;
		}
		return -1;
	}

	public void chooseMapScreen(title, currentmap) {
		return ListScreen(title, new MapLister(currentmap));
	}

	public void update() {
		mousepos = Mouse.getMousePos;
		if (mousepos) {
			hitmap = hittest(mousepos[0], mousepos[1]);
			if (Input.trigger(Input.MOUSELEFT)) {
				onClick(hitmap, mousepos[0], mousepos[1]);
			} else if (Input.trigger(Input.MOUSERIGHT)) {
				onRightClick(hitmap, mousepos[0], mousepos[1]);
			} else if (Input.release(Input.MOUSELEFT)) {
				onMouseUp(hitmap);
			} else if (Input.release(Input.MOUSERIGHT)) {
				onRightMouseUp(hitmap);
			} else {
				if (@lasthitmap != hitmap) {
					onMouseOver(hitmap, mousepos[0], mousepos[1]);
					@lasthitmap = hitmap;
				}
				if (@oldmousex != mousepos[0] || @oldmousey != mousepos[1]) {
					onMouseMove(hitmap, mousepos[0], mousepos[1]);
					@oldmousex = mousepos[0];
					@oldmousey = mousepos[1];
				}
			}
		}
		if (Input.press(Input.UP)) {
			@mapsprites.each do |i|
				if (i) i[1].y += 4;
			}
		}
		if (Input.press(Input.DOWN)) {
			@mapsprites.each do |i|
				if (i) i[1].y -= 4;
			}
		}
		if (Input.press(Input.LEFT)) {
			@mapsprites.each do |i|
				if (i) i[1].x += 4;
			}
		}
		if (Input.press(Input.RIGHT)) {
			@mapsprites.each do |i|
				if (i) i[1].x -= 4;
			}
		}
		if (Input.triggerex(:A)) {
			id = chooseMapScreen(_INTL("Add Map"), @currentmap);
			if (id > 0) {
				addSprite(id);
				setTopSprite(id);
				@mapconns = generateConnectionData;
			}
		} else if (Input.triggerex(:S)) {
			id = chooseMapScreen(_INTL("Go to Map"), @currentmap);
			if (id > 0) {
				@mapconns = generateConnectionData;
				DisposeSpriteHash(@mapsprites);
				@mapsprites.clear;
				@sprites["selsprite"].othersprite = null;
				@selmapid = -1;
				putSprite(id);
				@currentmap = id;
			}
		} else if (Input.triggerex(:DELETE)) {
			if (@mapsprites.keys.length > 1 && @selmapid >= 0) {
				@mapsprites[@selmapid].bitmap.dispose;
				@mapsprites[@selmapid].dispose;
				@mapsprites.delete(@selmapid);
				@sprites["selsprite"].othersprite = null;
				@selmapid = -1;
			}
		} else if (Input.triggerex(:D)) {
			helpWindow;
		}
		UpdateSpriteHash(@sprites);
	}

	public void MapScreenLoop() {
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			update;
			if (Input.trigger(Input.BACK)) {
				if (ConfirmMessage(_INTL("Save changes?"))) {
					serializeConnectionData;
					MapFactoryHelper.clear;
				} else {
					GameData.Encounter.load;
				}
				if (ConfirmMessage(_INTL("Exit from the editor?"))) break;
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public void ConnectionsEditor() {
	CriticalCode do;
		Graphics.resize_screen(Settings.SCREEN_WIDTH + 288, Settings.SCREEN_HEIGHT + 288);
		SetResizeFactor(1);
		mapscreen = new MapScreenScene();
		mapscreen.mapScreen;
		mapscreen.MapScreenLoop;
		mapscreen.close;
		Graphics.resize_screen(Settings.SCREEN_WIDTH, Settings.SCREEN_HEIGHT);
		SetResizeFactor(Game.GameData.PokemonSystem.screensize);
	}
}
