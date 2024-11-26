//===============================================================================
// Map Factory (allows multiple maps to be loaded at once and connected).
//===============================================================================
public partial class PokemonMapFactory {
	public int maps		{ get { return _maps; } }			protected int _maps;

	public void initialize(id) {
		@maps       = new List<string>();
		@fixup      = false;
		@mapChanged = false;   // transient instance variable
		setup(id);
	}

	// Clears all maps and sets up the current map with id. This function also sets
	// the positions of neighboring maps and notifies the game system of a map
	// change.
	public void setup(id) {
		@maps.clear;
		@maps[0] = new Game_Map();
		@mapIndex = 0;
		oldID = (Game.GameData.game_map) ? Game.GameData.game_map.map_id : 0;
		if (oldID != 0 && oldID != @maps[0].map_id) setMapChanging(id, @maps[0]);
		Game.GameData.game_map = @maps[0];
		@maps[0].setup(id);
		setMapsInRange;
		setMapChanged(oldID);
	}

	public void map() {
		if (!@mapIndex || @mapIndex < 0) @mapIndex = 0;
		if (@maps[@mapIndex]) return @maps[@mapIndex];
		if (@maps.length == 0) raise $"No maps in save file... (mapIndex={@mapIndex})";
		if (@maps[0]) {
			echoln $"Using next map, may be incorrect (mapIndex={@mapIndex}, length={@maps.length})";
			return @maps[0];
		}
		Debug.LogError($"No maps in save file... (all maps empty; mapIndex={@mapIndex})");
		//throw new ArgumentException($"No maps in save file... (all maps empty; mapIndex={@mapIndex})");
	}

	public bool hasMap(id) {
		@maps.each do |map|
			if (map.map_id == id) return true;
		}
		return false;
	}

	public void getMapIndex(id) {
		for (int i = @maps.length; i < @maps.length; i++) { //for '@maps.length' times do => |i|
			if (@maps[i].map_id == id) return i;
		}
		return -1;
	}

	public void getMap(id, add = true) {
		@maps.each do |map|
			if (map.map_id == id) return map;
		}
		map = new Game_Map();
		map.setup(id);
		if (add) @maps.Add(map);
		return map;
	}

	public void getMapNoAdd(id) {
		return getMap(id, false);
	}

	public void getNewMap(playerX, playerY, map_id = null) {
		id = map_id || Game.GameData.game_map.map_id;
		foreach (var conn in MapFactoryHelper.eachConnectionForMap(id)) { //MapFactoryHelper.eachConnectionForMap(id) do => |conn|
			mapidB = null;
			newx = 0;
			newy = 0;
			if (conn[0] == id) {
				mapidB = conn[3];
				mapB = MapFactoryHelper.getMapDims(conn[3]);
				newx = conn[4] - conn[1] + playerX;
				newy = conn[5] - conn[2] + playerY;
			} else {
				mapidB = conn[0];
				mapB = MapFactoryHelper.getMapDims(conn[0]);
				newx = conn[1] - conn[4] + playerX;
				newy = conn[2] - conn[5] + playerY;
			}
			if (newx >= 0 && newx < mapB[0] && newy >= 0 && newy < mapB[1]) {
				if (map_id) return new {getMapNoAdd(mapidB), newx, newy};
				return new {getMap(mapidB), newx, newy};
			}
		}
		return null;
	}

	// Detects whether the player has moved onto a connected map, and if so, causes
	// their transfer to that map.
	public void setCurrentMap() {
		if (Game.GameData.game_player.moving() || Game.GameData.game_player.jumping()) return;
		if (Game.GameData.game_map.valid(Game.GameData.game_player.x, Game.GameData.game_player.y)) return;
		newmap = getNewMap(Game.GameData.game_player.x, Game.GameData.game_player.y);
		if (!newmap) return;
		oldmap = Game.GameData.game_map.map_id;
		if (oldmap != 0 && oldmap != newmap[0].map_id) {
			setMapChanging(newmap[0].map_id, newmap[0]);
		}
		Game.GameData.game_map = newmap[0];
		@mapIndex = getMapIndex(Game.GameData.game_map.map_id);
		Game.GameData.game_player.moveto(newmap[1], newmap[2]);
		Game.GameData.game_map.update;
		AutoplayOnTransition;
		Game.GameData.game_map.refresh;
		setMapChanged(oldmap);
		Game.GameData.game_screen.weather_duration = 20;
	}

	public void setMapsInRange() {
		if (@fixup) return;
		@fixup = true;
		id = Game.GameData.game_map.map_id;
		foreach (var conn in MapFactoryHelper.eachConnectionForMap(id)) { //MapFactoryHelper.eachConnectionForMap(id) do => |conn|
			if (conn[0] == id) {
				mapA = getMap(conn[0]);
				newdispx = ((conn[4] - conn[1]) * Game_Map.REAL_RES_X) + mapA.display_x;
				newdispy = ((conn[5] - conn[2]) * Game_Map.REAL_RES_Y) + mapA.display_y;
				if (hasMap(conn[3]) || MapFactoryHelper.mapInRangeById(conn[3], newdispx, newdispy)) {
					mapB = getMap(conn[3]);
					if (mapB.display_x != newdispx) mapB.display_x = newdispx;
					if (mapB.display_y != newdispy) mapB.display_y = newdispy;
				}
			} else {
				mapA = getMap(conn[3]);
				newdispx = ((conn[1] - conn[4]) * Game_Map.REAL_RES_X) + mapA.display_x;
				newdispy = ((conn[2] - conn[5]) * Game_Map.REAL_RES_Y) + mapA.display_y;
				if (hasMap(conn[0]) || MapFactoryHelper.mapInRangeById(conn[0], newdispx, newdispy)) {
					mapB = getMap(conn[0]);
					if (mapB.display_x != newdispx) mapB.display_x = newdispx;
					if (mapB.display_y != newdispy) mapB.display_y = newdispy;
				}
			}
		}
		@fixup = false;
	}

	public void setMapChanging(newID, newMap) {
		EventHandlers.trigger(:on_leave_map, newID, newMap);
	}

	public void setMapChanged(prevMap) {
		EventHandlers.trigger(:on_enter_map, prevMap);
		@mapChanged = true;
	}

	public void setSceneStarted(scene) {
		EventHandlers.trigger(:on_map_or_spriteset_change, scene, @mapChanged);
		@mapChanged = false;
	}

	// Similar to Game_Player#passable(), but supports map connections
	public bool isPassableFromEdge(x, y, dir = 0) {
		if (Game.GameData.game_map.valid(x, y)) return true;
		newmap = getNewMap(x, y, Game.GameData.game_map.map_id);
		if (!newmap) return false;
		return isPassable(newmap[0].map_id, newmap[1], newmap[2], dir);
	}

	public bool isPassable(mapID, x, y, dir = 0, thisEvent = null) {
		if (!thisEvent) thisEvent = Game.GameData.game_player;
		map = getMapNoAdd(mapID);
		if (!map) return false;
		if (!map.valid(x, y)) return false;
		if (thisEvent.through) return true;
		// Check passability of tile
		if (Core.DEBUG && Input.press(Input.CTRL) && thisEvent.is_a(Game_Player)) return true;
		if (!map.passable(x, y, dir, thisEvent)) return false;
		// Check passability of event(s) in that spot
		foreach (var event in map.events) { //map.events.each_value do => |event|
			if (event == thisEvent || !event.at_coordinate(x, y)) continue;
			if (!event.through && event.character_name != "") return false;
		}
		// Check passability of player
		if (!thisEvent.is_a(Game_Player) &&
			Game.GameData.game_map.map_id == mapID && Game.GameData.game_player.x == x && Game.GameData.game_player.y == y &&
			!Game.GameData.game_player.through && Game.GameData.game_player.character_name != "") {
			return false;
		}
		return true;
	}

	// Only used by follower events
	public bool isPassableStrict(mapID, x, y, thisEvent = null) {
		if (!thisEvent) thisEvent = Game.GameData.game_player;
		map = getMapNoAdd(mapID);
		if (!map) return false;
		if (!map.valid(x, y)) return false;
		if (thisEvent.through) return true;
		if (Core.DEBUG && Input.press(Input.CTRL) && thisEvent.is_a(Game_Player)) return true;
		if (!map.passableStrict(x, y, 0, thisEvent)) return false;
		foreach (var event in map.events) { //map.events.each_value do => |event|
			if (event == thisEvent || !event.at_coordinate(x, y)) continue;
			if (!event.through && event.character_name != "") return false;
		}
		return true;
	}

	public void getTerrainTag(mapid, x, y, countBridge = false) {
		map = getMapNoAdd(mapid);
		return map.terrain_tag(x, y, countBridge);
	}

	// NOTE: Assumes the event is 1x1 tile in size. Only returns one terrain tag.
	public void getFacingTerrainTag(dir = null, event = null) {
		tile = getFacingTile(dir, event);
		if (!tile) return GameData.TerrainTag.get(:None);
		return getTerrainTag(tile[0], tile[1], tile[2]);
	}

	public void getTerrainTagFromCoords(mapid, x, y, countBridge = false) {
		tile = getRealTilePos(mapid, x, y);
		if (!tile) return GameData.TerrainTag.get(:None);
		return getTerrainTag(tile[0], tile[1], tile[2]);
	}

	public bool areConnected(mapID1, mapID2) {
		if (mapID1 == mapID2) return true;
		return MapFactoryHelper.mapsConnected(mapID1, mapID2);
	}

	// Returns the coordinate change to go from this position to other position
	public void getRelativePos(thisMapID, thisX, thisY, otherMapID, otherX, otherY) {
		if (thisMapID == otherMapID) {   // Both events share the same map
			return new {otherX - thisX, otherY - thisY};
		}
		foreach (var conn in MapFactoryHelper.eachConnectionForMap(thisMapID)) { //MapFactoryHelper.eachConnectionForMap(thisMapID) do => |conn|
			if (conn[0] == otherMapID) {
				posX = conn[4] - conn[1] + otherX - thisX;
				posY = conn[5] - conn[2] + otherY - thisY;
				return new {posX, posY};
			} else if (conn[3] == otherMapID) {
				posX =  conn[1] - conn[4] + otherX - thisX;
				posY =  conn[2] - conn[5] + otherY - thisY;
				return new {posX, posY};
			}
		}
		return new {0, 0};
	}

	// Gets the distance from this event to another event.  Example: If this event's
	// coordinates are (2,5) and the other event's coordinates are (5,1), returns
	// the array (3,-4), because (5-2=3) and (1-5=-4).
	public void getThisAndOtherEventRelativePos(thisEvent, otherEvent) {
		if (!thisEvent || !otherEvent) return new {0, 0};
		return getRelativePos(thisEvent.map.map_id, thisEvent.x, thisEvent.y,
													otherEvent.map.map_id, otherEvent.x, otherEvent.y);
	}

	public void getThisAndOtherPosRelativePos(thisEvent, otherMapID, otherX, otherY) {
		if (!thisEvent) return new {0, 0};
		return getRelativePos(thisEvent.map.map_id, thisEvent.x, thisEvent.y,
													otherMapID, otherX, otherY);
	}

	// Unused
	public void getOffsetEventPos(event, xOffset, yOffset) {
		if (!event) event = Game.GameData.game_player;
		if (!event) return null;
		return getRealTilePos(event.map.map_id, event.x + xOffset, event.y + yOffset);
	}

	// NOTE: Assumes the event is 1x1 tile in size. Only returns one tile.
	public void getFacingTile(direction = null, event = null, steps = 1) {
		if (event.null()) event = Game.GameData.game_player;
		if (!event) return new {0, 0, 0};
		x = event.x;
		y = event.y;
		id = event.map.map_id;
		if (direction.null()) direction = event.direction;
		return getFacingTileFromPos(id, x, y, direction, steps);
	}

	public void getFacingTileFromPos(mapID, x, y, direction = 0, steps = 1) {
		id = mapID;
		switch (direction) {
			case 1:
				x -= steps;
				y += steps;
				break;
			case 2:
				y += steps;
				break;
			case 3:
				x += steps;
				y += steps;
				break;
			case 4:
				x -= steps;
				break;
			case 6:
				x += steps;
				break;
			case 7:
				x -= steps;
				y -= steps;
				break;
			case 8:
				y -= steps;
				break;
			case 9:
				x += steps;
				y -= steps;
				break;
			default:
				return new {id, x, y};
		}
		return getRealTilePos(mapID, x, y);
	}

	public void getRealTilePos(mapID, x, y) {
		id = mapID;
		if (getMapNoAdd(id).valid(x, y)) return new {id, x, y};
		foreach (var conn in MapFactoryHelper.eachConnectionForMap(id)) { //MapFactoryHelper.eachConnectionForMap(id) do => |conn|
			if (conn[0] == id) {
				newX = x + conn[4] - conn[1];
				newY = y + conn[5] - conn[2];
				if (newX < 0 || newY < 0) continue;
				dims = MapFactoryHelper.getMapDims(conn[3]);
				if (newX >= dims[0] || newY >= dims[1]) continue;
				return new {conn[3], newX, newY};
			} else {
				newX = x + conn[1] - conn[4];
				newY = y + conn[2] - conn[5];
				if (newX < 0 || newY < 0) continue;
				dims = MapFactoryHelper.getMapDims(conn[0]);
				if (newX >= dims[0] || newY >= dims[1]) continue;
				return new {conn[0], newX, newY};
			}
		}
		return null;
	}

	public void getFacingCoords(x, y, direction = 0, steps = 1) {
		switch (direction) {
			case 1:
				x -= steps;
				y += steps;
				break;
			case 2:
				y += steps;
				break;
			case 3:
				x += steps;
				y += steps;
				break;
			case 4:
				x -= steps;
				break;
			case 6:
				x += steps;
				break;
			case 7:
				x -= steps;
				y -= steps;
				break;
			case 8:
				y -= steps;
				break;
			case 9:
				x += steps;
				y -= steps;
				break;
		}
		return new {x, y};
	}

	public void updateMaps(scene) {
		updateMapsInternal;
		if (@mapChanged) setSceneStarted(scene);
	}

	public void updateMapsInternal() {
		if (Game.GameData.game_player.moving()) return;
		if (!MapFactoryHelper.hasConnections(Game.GameData.game_map.map_id)) {
			if (@maps.length == 1) return;
			@maps.delete_if(map => map.map_id != Game.GameData.game_map.map_id);
			@mapIndex = getMapIndex(Game.GameData.game_map.map_id);
			return;
		}
		old_num_maps = @maps.length;
		@maps.delete_if(map => !MapFactoryHelper.mapsConnected(Game.GameData.game_map.map_id, map.map_id));
		if (@maps.length != old_num_maps) @mapIndex = getMapIndex(Game.GameData.game_map.map_id);
		setMapsInRange;
		old_num_maps = @maps.length;
		@maps.delete_if(map => !MapFactoryHelper.mapInRange(map));
		if (@maps.length != old_num_maps) @mapIndex = getMapIndex(Game.GameData.game_map.map_id);
	}
}

//===============================================================================
// Map Factory Helper (stores map connection and size data and calculations
// involving them)
//===============================================================================
public static partial class MapFactoryHelper {
	@@MapConnections = null;
	@@MapDims        = null;

	#region Class Functions
	#endregion

	public void clear() {
		@@MapConnections = null;
		@@MapDims        = null;
	}

	public void getMapConnections() {
		if (!@@MapConnections) {
			@@MapConnections = new List<string>();
			conns = load_data("Data/map_connections.dat");
			foreach (var conn in conns) { //'conns.each' do => |conn|
				// Ensure both maps in a connection are valid
				dimensions = getMapDims(conn[0]);
				if (dimensions[0] == 0 || dimensions[1] == 0) continue;
				dimensions = getMapDims(conn[3]);
				if (dimensions[0] == 0 || dimensions[1] == 0) continue;
				// Convert first map's edge and coordinate to pair of coordinates
				edge = getMapEdge(conn[0], conn[1]);
				switch (conn[1]) {
					case "N": case "S":
						conn[1] = conn[2];
						conn[2] = edge;
						break;
					case "E": case "W":
						conn[1] = edge;
						break;
				}
				// Convert second map's edge and coordinate to pair of coordinates
				edge = getMapEdge(conn[3], conn[4]);
				switch (conn[4]) {
					case "N": case "S":
						conn[4] = conn[5];
						conn[5] = edge;
						break;
					case "E": case "W":
						conn[4] = edge;
						break;
				}
				// Add connection to arrays for both maps
				if (!@@MapConnections[conn[0]]) @@MapConnections[conn[0]] = new List<string>();
				@@MapConnections[conn[0]].Add(conn);
				if (!@@MapConnections[conn[3]]) @@MapConnections[conn[3]] = new List<string>();
				@@MapConnections[conn[3]].Add(conn);
			}
		}
		return @@MapConnections;
	}

	public bool hasConnections(id) {
		conns = MapFactoryHelper.getMapConnections;
		return conns[id] ? true : false;
	}

	public bool mapsConnected(id1, id2) {
		foreach (var conn in MapFactoryHelper.eachConnectionForMap(id1)) { //MapFactoryHelper.eachConnectionForMap(id1) do => |conn|
			if (conn[0] == id2 || conn[3] == id2) return true;
		}
		return false;
	}

	public void eachConnectionForMap(id) {
		conns = MapFactoryHelper.getMapConnections;
		if (!conns[id]) return;
		conns[id].each(conn => yield conn);
	}

	// Gets the height and width of the map with id.
	public void getMapDims(id) {
		// Create cache if doesn't exist
		if (!@@MapDims) @@MapDims = new List<string>();
		// Add map to cache if can't be found
		if (!@@MapDims[id]) {
			begin;
				map = load_data(string.Format("Data/Map{0:3}.rxdata", id));
				@@MapDims[id] = new {map.width, map.height};
			rescue;
				@@MapDims[id] = new {0, 0};
			}
		}
		// Return map in cache
		return @@MapDims[id];
	}

	// Returns the X or Y coordinate of an edge on the map with id.
	// Considers the special strings "N","W","E","S"
	public void getMapEdge(id, edge) {
		if (new []{"N", "W"}.Contains(edge)) return 0;
		dims = getMapDims(id);   // Get dimensions
		if (edge == "E") return dims[0];
		if (edge == "S") return dims[1];
		return dims[0];   // real dimension (use width)
	}

	public bool mapInRange(map) {
		range = 6;   // Number of tiles
		dispx = map.display_x;
		dispy = map.display_y;
		if (dispx >= (map.width + range) * Game_Map.REAL_RES_X) return false;
		if (dispy >= (map.height + range) * Game_Map.REAL_RES_Y) return false;
		if (dispx <= -(Graphics.width + (range * Game_Map.TILE_WIDTH)) * Game_Map.X_SUBPIXELS) return false;
		if (dispy <= -(Graphics.height + (range * Game_Map.TILE_HEIGHT)) * Game_Map.Y_SUBPIXELS) return false;
		return true;
	}

	public bool mapInRangeById(id, dispx, dispy) {
		range = 6;   // Number of tiles
		dims = MapFactoryHelper.getMapDims(id);
		if (dispx >= (dims[0] + range) * Game_Map.REAL_RES_X) return false;
		if (dispy >= (dims[1] + range) * Game_Map.REAL_RES_Y) return false;
		if (dispx <= -(Graphics.width + (range * Game_Map.TILE_WIDTH)) * Game_Map.X_SUBPIXELS) return false;
		if (dispy <= -(Graphics.height + (range * Game_Map.TILE_HEIGHT)) * Game_Map.Y_SUBPIXELS) return false;
		return true;
	}
}

//===============================================================================
//
//===============================================================================
// Unused
public void updateTilesets() {
	maps = Game.GameData.map_factory.maps;
	foreach (var map in maps) { //'maps.each' do => |map|
		map&.updateTileset;
	}
}
