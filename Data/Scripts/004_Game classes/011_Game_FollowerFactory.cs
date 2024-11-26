//===============================================================================
// Data saved in Game.GameData.PokemonGlobal.followers.
//===============================================================================
public partial class FollowerData {
	public int original_map_id		{ get { return _original_map_id; } set { _original_map_id = value; } }			protected int _original_map_id;
	public int event_id		{ get { return _event_id; } set { _event_id = value; } }			protected int _event_id;
	public int event_name		{ get { return _event_name; } set { _event_name = value; } }			protected int _event_name;
	public int current_map_id		{ get { return _current_map_id; } set { _current_map_id = value; } }			protected int _current_map_id;
	public int x		{ get { return _x; } set { _x = value; } }			protected int _x;
	public int y		{ get { return _y; } set { _y = value; } }			protected int _y;
	public int direction		{ get { return _direction; } set { _direction = value; } }			protected int _direction;
	public int character_name		{ get { return _character_name; } set { _character_name = value; } }			protected int _character_name;
	public int character_hue		{ get { return _character_hue; } set { _character_hue = value; } }			protected int _character_hue;
	public int name		{ get { return _name; } set { _name = value; } }			protected int _name;
	public int common_event_id		{ get { return _common_event_id; } set { _common_event_id = value; } }			protected int _common_event_id;
	public int visible		{ get { return _visible; } set { _visible = value; } }			protected int _visible;
	public int invisible_after_transfer		{ get { return _invisible_after_transfer; } set { _invisible_after_transfer = value; } }			protected int _invisible_after_transfer;

	public void initialize(original_map_id, event_id, event_name, current_map_id, x, y,
								direction, character_name, character_hue) {
		@original_map_id          = original_map_id;
		@event_id                 = event_id;
		@event_name               = event_name;
		@current_map_id           = current_map_id;
		@x                        = x;
		@y                        = y;
		@direction                = direction;
		@character_name           = character_name;
		@character_hue            = character_hue;
		@name                     = null;
		@common_event_id          = null;
		@visible                  = true;
		@invisible_after_transfer = false;
	}

	public bool visible() {
		return @visible && !@invisible_after_transfer;
	}

	public void interact(event) {
		if (!event || event.list.size <= 1) return;
		if (!@common_event_id) return;
		// Start event
		if (Game.GameData.game_map.need_refresh) Game.GameData.game_map.refresh;
		event.lock;
		MapInterpreter.setup(event.list, event.id, event.map.map_id);
	}
}

//===============================================================================
// Permanently stores data of follower events (i.e. in save files).
//===============================================================================
public partial class PokemonGlobalMetadata {
	public int followers		{ get { return _followers; } }			protected int _followers;

	public void followers() {
		if (!@followers) @followers = new List<string>();
		return @followers;
	}
}

//===============================================================================
// Stores Game_Follower instances just for the current play session.
//===============================================================================
public partial class Game_Temp {
	public int followers		{ get { return _followers; } }			protected int _followers;

	public void followers() {
		if (!@followers) @followers = new Game_FollowerFactory();
		return @followers;
	}
}

//===============================================================================
//
//===============================================================================
public partial class Game_FollowerFactory {
	public int last_update		{ get { return _last_update; } }			protected int _last_update;

	public void initialize() {
		@events = new List<string>();
		foreach (var follower in Game.GameData.PokemonGlobal.followers) { //'Game.GameData.PokemonGlobal.followers.each' do => |follower|
			@events.Add(create_follower_object(follower));
		}
		@last_update = -1;
	}

	//-----------------------------------------------------------------------------

	public void add_follower(event, name = null, common_event_id = null) {
		if (!event) return;
		followers = Game.GameData.PokemonGlobal.followers;
		if (followers.any(data => data.original_map_id == Game.GameData.game_map.map_id && data.event_id == event.id)) {
			return;   // Event is already dependent
		}
		eventData = new FollowerData(Game.GameData.game_map.map_id, event.id, event.name,
																Game.GameData.game_map.map_id, event.x, event.y, event.direction,
																event.character_name.clone, event.character_hue);
		eventData.name            = name;
		eventData.common_event_id = common_event_id;
		newEvent = create_follower_object(eventData);
		followers.Add(eventData);
		@events.Add(newEvent);
		@last_update += 1;
	}

	public void remove_follower_by_event(event) {
		followers = Game.GameData.PokemonGlobal.followers;
		map_id = Game.GameData.game_map.map_id;
		followers.each_with_index do |follower, i|
			if (follower.current_map_id != map_id) continue;
			if (follower.original_map_id != event.map_id) continue;
			if (follower.event_id != event.id) continue;
			followers[i] = null;
			@events[i] = null;
			@last_update += 1;
		}
		followers.compact!;
		@events.compact!;
	}

	public void remove_follower_by_name(name) {
		followers = Game.GameData.PokemonGlobal.followers;
		followers.each_with_index do |follower, i|
			if (follower.name != name) continue;
			followers[i] = null;
			@events[i] = null;
			@last_update += 1;
		}
		followers.compact!;
		@events.compact!;
	}

	public void remove_all_followers() {
		Game.GameData.PokemonGlobal.followers.clear;
		@events.clear;
		@last_update += 1;
	}

	public void get_follower_by_index(index = 0) {
		@events.each_with_index((event, i) => { if (i == index) return event; });
		return null;
	}

	public void get_follower_by_name(name) {
		if (follower&.name == name }) each_follower { |event, follower| return event;
		return null;
	}

	public void each_follower() {
		Game.GameData.PokemonGlobal.followers.each_with_index((follower, i) => yield @events[i], follower);
	}

	//-----------------------------------------------------------------------------

	public void turn_followers() {
		leader = Game.GameData.game_player;
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			event.turn_towards_leader(leader);
			follower.direction = event.direction;
			leader = event;
		}
	}

	public void move_followers() {
		leader = Game.GameData.game_player;
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			event.follow_leader(leader, false, (i == 0));
			follower.x              = event.x;
			follower.y              = event.y;
			follower.current_map_id = event.map.map_id;
			follower.direction      = event.direction;
			leader = event;
		}
	}

	public void map_transfer_followers() {
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			event.map = Game.GameData.game_map;
			event.moveto(Game.GameData.game_player.x, Game.GameData.game_player.y);
			event.direction = Game.GameData.game_player.direction;
			event.opacity   = 255;
			follower.x                        = event.x;
			follower.y                        = event.y;
			follower.current_map_id           = event.map.map_id;
			follower.direction                = event.direction;
			follower.invisible_after_transfer = true;
		}
	}

	public void follow_into_door() {
		// Setting an event's move route also makes it start along that move route,
		// so we need to record all followers' current positions first before setting
		// any move routes
		follower_pos = new List<string>();
		follower_pos.Add(new {Game.GameData.game_player.map.map_id, Game.GameData.game_player.x, Game.GameData.game_player.y});
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			follower_pos.Add(new {event.map.map_id, event.x, event.y});
		}
		// Calculate and set move route from each follower to player
		move_route = new List<string>();
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			leader = follower_pos[i];
			vector = Game.GameData.map_factory.getRelativePos(event.map.map_id, event.x, event.y,
																					leader[0], leader[1], leader[2]);
			if (vector[0] != 0) {
				move_route.prepend((vector[0] > 0) ? MoveRoute.RIGHT : MoveRoute.LEFT);
			} else if (vector[1] != 0) {
				move_route.prepend((vector[1] > 0) ? MoveRoute.DOWN : MoveRoute.UP);
			}
			MoveRoute(event, move_route + new {MoveRoute.OPACITY, 0});
		}
	}

	// Used when coming out of a door.
	public void hide_followers() {
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			event.opacity = 0;
		}
	}

	// Used when coming out of a door. Makes all followers invisible until the
	// player starts moving.
	public void put_followers_on_player() {
		Game.GameData.PokemonGlobal.followers.each_with_index do |follower, i|
			event = @events[i];
			event.moveto(Game.GameData.game_player.x, Game.GameData.game_player.y);
			event.opacity = 255;
			follower.x                        = event.x;
			follower.y                        = event.y;
			follower.invisible_after_transfer = true;
		}
	}

	//-----------------------------------------------------------------------------

	public void update() {
		if (Game.GameData.game_temp.in_menu) return;
		followers = Game.GameData.PokemonGlobal.followers;
		if (followers.length == 0) return;
		// Update all followers
		leader = Game.GameData.game_player;
		player_moving = Game.GameData.game_player.moving() || Game.GameData.game_player.jumping();
		followers.each_with_index do |follower, i|
			event = @events[i];
			if (!@events[i]) continue;
			if (follower.invisible_after_transfer && player_moving) {
				follower.invisible_after_transfer = false;
				event.turn_towards_leader(Game.GameData.game_player);
			}
			event.move_speed  = leader.move_speed;
			event.transparent = !follower.visible();
			if (Game.GameData.PokemonGlobal.ice_sliding) {
				event.straighten;
				event.walk_anime = false;
			} else {
				event.walk_anime = true;
			}
			if (event.jumping() || event.moving() || !player_moving) {
				event.update;
			} else if (!event.starting) {
				event.set_starting;
				event.update;
				event.clear_starting;
			}
			follower.direction = event.direction;
			leader = event;
		}
		// Check event triggers
		if (Input.trigger(Input.USE) && !Game.GameData.game_temp.in_menu && !Game.GameData.game_temp.in_battle &&
			!Game.GameData.game_player.move_route_forcing && !Game.GameData.game_temp.message_window_showing &&
			!MapInterpreterRunning()) {
			// Get position of tile facing the player
			facing_tile = Game.GameData.map_factory.getFacingTile;
			// Assumes player is 1x1 tile in size
			each_follower do |event, follower|
				if (!facing_tile || event.map.map_id != facing_tile[0] ||
								!event.at_coordinate(facing_tile[1], facing_tile[2])) continue;   // Not on facing tile
				if (event.jumping()) continue;
				follower.interact(event);
			}
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public void create_follower_object(event_data) {
		return new Game_Follower(event_data);
	}
}

//===============================================================================
//
//===============================================================================
public partial class FollowerSprites {
	public void initialize(viewport) {
		@viewport    = viewport;
		@sprites     = new List<string>();
		@last_update = null;
		@disposed    = false;
	}

	public void dispose() {
		if (@disposed) return;
		@sprites.each(sprite => sprite.dispose);
		@sprites.clear;
		@disposed = true;
	}

	public bool disposed() {
		return @disposed;
	}

	public void refresh() {
		@sprites.each(sprite => sprite.dispose);
		@sprites.clear;
		Game.GameData.game_temp.followers.each_follower do |event, follower|
			@sprites.Add(new Sprite_Character(@viewport, event));
		}
	}

	public void update() {
		if (Game.GameData.game_temp.followers.last_update != @last_update) {
			refresh;
			@last_update = Game.GameData.game_temp.followers.last_update;
		}
		@sprites.each(sprite => sprite.update);
	}
}

//===============================================================================
// Helper module for adding/removing/getting followers.
//===============================================================================
public static partial class Followers {
	#region Class Functions
	#endregion

	/// <param name="event_id">ID of the event on the current map to be added as a follower</param>
	/// <param name="name">identifier name of the follower to be added</param>
	// @param common_event_id [Integer] ID of the Common Event triggered when interacting with this follower
	public void add(event_id, String name, common_Integer event_id) {
		Game.GameData.game_temp.followers.add_follower(Game.GameData.game_map.events[event_id], name, common_event_id);
	}

	/// <param name="event">map event to be added as a follower</param>
	public void add_event(Game_Event event) {
		Game.GameData.game_temp.followers.add_follower(event);
	}

	/// <param name="name">identifier name of the follower to be removed</param>
	public void remove(String name) {
		Game.GameData.game_temp.followers.remove_follower_by_name(name);
	}

	/// <param name="event">map event to be removed as a follower</param>
	public void remove_event(Game_Event event) {
		Game.GameData.game_temp.followers.remove_follower_by_event(event);
	}

	// Removes all followers.
	public void clear() {
		Game.GameData.game_temp.followers.remove_all_followers;
		DeregisterPartner rescue null;
	}

	/// <param name="name">name of the follower to get, or null for the first follower | String, null</param>
	// @return [Game_Follower, null] follower object
	public void get(String name = null) {
		if (name) return Game.GameData.game_temp.followers.get_follower_by_name(name);
		return Game.GameData.game_temp.followers.get_follower_by_index;
	}

	public void follow_into_door() {
		Game.GameData.game_temp.followers.follow_into_door;
	}

	public void hide_followers() {
		Game.GameData.game_temp.followers.hide_followers;
	}

	public void put_followers_on_player() {
		Game.GameData.game_temp.followers.put_followers_on_player;
	}
}
