//===============================================================================
// Sprite_Shadow (Sprite_Ombre )
// Based on Genzai Kawakami's shadows, dynamisme & features by Rataime, extra
// features Boushy
// Modified by Peter O. to be compatible with Pokémon Essentials
//===============================================================================
public partial class Sprite_Shadow : RPG.Sprite {
	public int character		{ get { return _character; } set { _character = value; } }			protected int _character;

	public override void initialize(viewport, character = null, params = new List<string>()) {
		base.initialize(viewport);
		@source       = params[0];
		@anglemin     = (params.size > 1) ? params[1] : 0;
		@anglemax     = (params.size > 2) ? params[2] : 0;
		@self_opacity = (params.size > 4) ? params[4] : 100;
		@distancemax  = (params.size > 3) ? params[3] : 350;
		@character    = character;
		update;
	}

	public override void dispose() {
		@chbitmap&.dispose;
		base.dispose();
	}

	public void update() {
		if (!in_range(@character, @source, @distancemax)) {
			self.opacity = 0;
			return;
		}
		super;
		if (@tile_id != @character.tile_id ||
			@character_name != @character.character_name ||
			@character_hue != @character.character_hue) {
			@tile_id        = @character.tile_id;
			@character_name = @character.character_name;
			@character_hue  = @character.character_hue;
			@chbitmap&.dispose;
			if (@tile_id >= 384) {
				@chbitmap = GetTileBitmap(@character.map.tileset_name,
																		@tile_id, @character.character_hue);
				self.src_rect.set(0, 0, 32, 32);
				@ch = 32;
				@cw = 32;
				self.ox = 16;
				self.oy = 32;
			} else {
				@chbitmap = new AnimatedBitmap("Graphics/Characters/" + @character.character_name,
																			@character.character_hue);
				@cw = @chbitmap.width / 4;
				@ch = @chbitmap.height / 4;
				self.ox = @cw / 2;
				self.oy = @ch;
			}
		}
		if (@chbitmap.is_a(AnimatedBitmap)) {
			@chbitmap.update;
			self.bitmap = @chbitmap.bitmap;
		} else {
			self.bitmap = @chbitmap;
		}
		self.visible = !@character.transparent;
		if (@tile_id == 0) {
			sx = @character.pattern * @cw;
			sy = (@character.direction - 2) / 2 * @ch;
			if (self.angle > 90 || angle < -90) {
				switch (@character.direction) {
					case 2:  sy = @ch * 3; break;
					case 4:  sy = @ch * 2; break;
					case 6:  sy = @ch; break;
					case 8:  sy = 0; break;
				}
			}
			self.src_rect.set(sx, sy, @cw, @ch);
		}
		self.x = ScreenPosHelper.ScreenX(@character);
		self.y = ScreenPosHelper.ScreenY(@character) - 5;
		self.z = ScreenPosHelper.ScreenZ(@character, @ch) - 1;
		self.zoom_x = ScreenPosHelper.ScreenZoomX(@character);
		self.zoom_y = ScreenPosHelper.ScreenZoomY(@character);
		self.blend_type = @character.blend_type;
		self.bush_depth = @character.bush_depth;
		if (@character.animation_id != 0) {
			animation = Game.GameData.data_animations[@character.animation_id];
			animation(animation, true);
			@character.animation_id = 0;
		}
		@deltax = ScreenPosHelper.ScreenX(@source) - self.x;
		@deltay = ScreenPosHelper.ScreenY(@source) - self.y;
		self.color = Color.black;
		@distance = ((@deltax**2) + (@deltay**2));
		self.opacity = @self_opacity * 13_000 / ((@distance * 370 / @distancemax) + 6000);
		self.angle = 57.3 * Math.atan2(@deltax, @deltay);
		@angle_trigo = self.angle + 90;
		if (@angle_trigo < 0) @angle_trigo += 360;
		if (@anglemin != 0 || @anglemax != 0) {
			if ((@angle_trigo < @anglemin || @angle_trigo > @anglemax) && @anglemin < @anglemax) {
				self.opacity = 0;
				return;
			}
			if (@angle_trigo < @anglemin && @angle_trigo > @anglemax && @anglemin > @anglemax) {
				self.opacity = 0;
				return;
			}
		}
	}

	// From Near's Anti Lag Script, edited.
	public bool in_range(element, object, range) {
		elemScreenX = ScreenPosHelper.ScreenX(element);
		elemScreenY = ScreenPosHelper.ScreenY(element);
		objScreenX  = ScreenPosHelper.ScreenX(object);
		objScreenY  = ScreenPosHelper.ScreenY(object);
		x = (elemScreenX - objScreenX) * (elemScreenX - objScreenX);
		y = (elemScreenY - objScreenY) * (elemScreenY - objScreenY);
		r = x + y;
		return r <= range * range;
	}
}

//===============================================================================
// ? CLASS Sprite_Character edit
//===============================================================================
public partial class Sprite_Character : RPG.Sprite {
	unless (private_method_defined(:shadow_initialize)) alias shadow_initialize initialize;

	public void initialize(viewport, character = null) {
		@ombrelist = new List<string>();
		@character = character;
		shadow_initialize(viewport, @character);
	}

	public void setShadows(map, shadows) {
		if (character.is_a(Game_Event) && shadows.length > 0) {
			params = XPML_read(map, "Shadow", @character, 4);
			if (params) {
				foreach (var shadow in shadows) { //'shadows.each' do => |shadow|
					@ombrelist.Add(new Sprite_Shadow(viewport, @character, shadows));
				}
			}
		}
		if (character.is_a(Game_Player) && shadows.length > 0) {
			foreach (var shadow in shadows) { //'shadows.each' do => |shadow|
				@ombrelist.Add(new Sprite_Shadow(viewport, Game.GameData.game_player, shadow));
			}
		}
		update;
	}

	public void clearShadows() {
		@ombrelist.each(s => s&.dispose);
		@ombrelist.clear;
	}

	unless (method_defined(:shadow_update)) alias shadow_update update;

	public void update() {
		shadow_update;
		@ombrelist.each(ombre => ombre.update);
	}
}

//===============================================================================
// ? CLASS Game_Event edit
//===============================================================================
public partial class Game_Event {
	public int id		{ get { return _id; } set { _id = value; } }			protected int _id;
}

//===============================================================================
// ? CLASS Spriteset_Map edit
//===============================================================================
public partial class Spriteset_Map {
	public int shadows		{ get { return _shadows; } set { _shadows = value; } }			protected int _shadows;

	unless (private_method_defined(:shadow_initialize)) alias shadow_initialize initialize;

	public void initialize(map = null) {
		@shadows = new List<string>();
		warn = false;
		if (!map) map = Game.GameData.game_map;
		foreach (var k in map.events.keys.sort) { //'map.events.keys.sort.each' do => |k|
			ev = map.events[k];
			if (ev.list && ev.list.length > 0 && ev.list[0].code == 108 &&
										(ev.list[0].parameters == ["s"] || ev.list[0].parameters == ["o"])) warn = true;
			params = XPML_read(map, "Shadow Source", ev, 4);
			if (params) @shadows.Add([ev] + params);
		}
		if (warn == true) {
			p "Warning : At least one event on this map uses the obsolete way to add shadows";
		}
		shadow_initialize(map);
		@character_sprites.each do |sprite|
			sprite.setShadows(map, @shadows);
		}
		Game.GameData.scene.spritesetGlobal.playersprite.setShadows(map, @shadows);
	}
}

//===============================================================================
// ? XPML Definition, by Rataime, using ideas from Near Fantastica
//
//   Returns null if the markup wasn't present at all,
//   returns [] if there wasn't any parameters, else
//   returns a parameters list with "int" converted as int
//   eg :
//   begin first
//   begin second
//   param1 1
//   param2 two
//   begin third
//   anything 3
//
//   p XPML_read("first", event_id) -> []
//   p XPML_read("second", event_id) -> [1, "two"]
//   p XPML_read("third", event_id) -> [3]
//   p XPML_read("forth", event_id) -> null
//===============================================================================
public void XPML_read(map, markup, event, max_param_number = 0) {
	parameter_list = null;
	if (!event || event.list.null()) return null;
	for (int i = event.list.size; i < event.list.size; i++) { //for 'event.list.size' times do => |i|
		unless (event.list[i].code == 108 &&) continue;
								event.list[i].parameters[0].downcase == "begin " + markup.downcase;
		if (parameter_list.null()) parameter_list = new List<string>();
		for (int j = (i + 1); j < event.list.size; j++) { //each 'event.list.size' do => |j|
			if (event.list[j].code != 108) return parameter_list;
			parts = event.list[j].parameters[0].split;
			if (parts.size == 1 || parts[0].downcase == "begin") return parameter_list;
			if (parts[1].ToInt() != 0 || parts[1] == "0") {
				parameter_list.Add(parts[1].ToInt());
			} else {
				parameter_list.Add(parts[1]);
			}
			if (max_param_number != 0 && j == i + max_param_number) return parameter_list;
		}
	}
	return parameter_list;
}
