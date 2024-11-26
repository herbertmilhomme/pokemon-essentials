//===============================================================================
// ** Game_Picture
//-------------------------------------------------------------------------------
//  This class handles the picture. It's used within the Game_Screen class
//  (Game.GameData.game_screen).
//===============================================================================
public partial class Game_Picture {
	/// <summary>picture number</summary>
	public int number		{ get { return _number; } }			protected int _number;
	/// <summary>file name</summary>
	public int name		{ get { return _name; } }			protected int _name;
	/// <summary>starting point</summary>
	public int origin		{ get { return _origin; } }			protected int _origin;
	/// <summary>x-coordinate</summary>
	public int x		{ get { return _x; } }			protected int _x;
	/// <summary>y-coordinate</summary>
	public int y		{ get { return _y; } }			protected int _y;
	/// <summary>x directional zoom rate</summary>
	public int zoom_x		{ get { return _zoom_x; } }			protected int _zoom_x;
	/// <summary>y directional zoom rate</summary>
	public int zoom_y		{ get { return _zoom_y; } }			protected int _zoom_y;
	/// <summary>opacity level</summary>
	public int opacity		{ get { return _opacity; } }			protected int _opacity;
	/// <summary>blend method</summary>
	public int blend_type		{ get { return _blend_type; } }			protected int _blend_type;
	/// <summary>color tone</summary>
	public int tone		{ get { return _tone; } }			protected int _tone;
	/// <summary>rotation angle</summary>
	public int angle		{ get { return _angle; } }			protected int _angle;

	public void initialize(number) {
		@number = number;
		@name = "";
		@origin = 0;
		@x = 0.0;
		@y = 0.0;
		@zoom_x = 100.0;
		@zoom_y = 100.0;
		@opacity = 255.0;
		@blend_type = 1;
		@duration = 0;
		@move_timer_start = null;
		@target_x = @x;
		@target_y = @y;
		@target_zoom_x = @zoom_x;
		@target_zoom_y = @zoom_y;
		@target_opacity = @opacity;
		@tone = new Tone(0, 0, 0, 0);
		@tone_target = new Tone(0, 0, 0, 0);
		@tone_duration = 0;
		@tone_timer_start = null;
		@angle = 0;
		@rotate_speed = 0;
	}

	// Show Picture
	//     name       : file name
	//     origin     : starting point
	//     x          : x-coordinate
	//     y          : y-coordinate
	//     zoom_x     : x directional zoom rate
	//     zoom_y     : y directional zoom rate
	//     opacity    : opacity level
	//     blend_type : blend method
	public void show(name, origin, x, y, zoom_x, zoom_y, opacity, blend_type) {
		@name = name;
		@origin = origin;
		@x = x.to_f;
		@y = y.to_f;
		@zoom_x = zoom_x.to_f;
		@zoom_y = zoom_y.to_f;
		@opacity = opacity.to_f;
		@blend_type = blend_type || 0;
		@duration = 0;
		@target_x = @x;
		@target_y = @y;
		@target_zoom_x = @zoom_x;
		@target_zoom_y = @zoom_y;
		@target_opacity = @opacity;
		@tone = new Tone(0, 0, 0, 0);
		@tone_target = new Tone(0, 0, 0, 0);
		@tone_duration = 0;
		@tone_timer_start = null;
		@angle = 0;
		@rotate_speed = 0;
	}

	// Move Picture
	//     duration   : time in 1/20ths of a second
	//     origin     : starting point
	//     x          : x-coordinate
	//     y          : y-coordinate
	//     zoom_x     : x directional zoom rate
	//     zoom_y     : y directional zoom rate
	//     opacity    : opacity level
	//     blend_type : blend method
	public void move(duration, origin, x, y, zoom_x, zoom_y, opacity, blend_type) {
		@duration         = duration / 20.0;
		@origin           = origin;
		@initial_x        = @x;
		@initial_y        = @y;
		@target_x         = x.to_f;
		@target_y         = y.to_f;
		@initial_zoom_x   = @zoom_x;
		@initial_zoom_y   = @zoom_y;
		@target_zoom_x    = zoom_x.to_f;
		@target_zoom_y    = zoom_y.to_f;
		@initial_opacity  = @opacity;
		@target_opacity   = opacity.to_f;
		@blend_type       = blend_type || 0;
		@move_timer_start = Game.GameData.stats.play_time;
	}

	// Change Rotation Speed
	//     speed : rotation speed (degrees to change per 1/20th of a second)
	public void rotate(speed) {
		@rotate_timer = (speed == 0) ? null : System.uptime;   // Time since last frame
		@rotate_speed = speed;
	}

	// Start Change of Color Tone
	//     tone     : color tone
	//     duration : time in 1/20ths of a second
	public void start_tone_change(tone, duration) {
		if (duration == 0) {
			@tone = tone.clone;
			return;
		}
		@tone_initial = @tone.clone;
		@tone_target = tone.clone;
		@tone_duration = duration / 20.0;
		@tone_timer_start = Game.GameData.stats.play_time;
	}

	public void erase() {
		@name = "";
	}

	public void update() {
		if (@name == "") return;
		now = Game.GameData.stats.play_time;
		if (@move_timer_start) {
			@x = lerp(@initial_x, @target_x, @duration, @move_timer_start, now);
			@y = lerp(@initial_y, @target_y, @duration, @move_timer_start, now);
			@zoom_x = lerp(@initial_zoom_x, @target_zoom_x, @duration, @move_timer_start, now);
			@zoom_y = lerp(@initial_zoom_y, @target_zoom_y, @duration, @move_timer_start, now);
			@opacity = lerp(@initial_opacity, @target_opacity, @duration, @move_timer_start, now);
			if (now - @move_timer_start >= @duration) {
				@initial_x        = null;
				@initial_y        = null;
				@initial_zoom_x   = null;
				@initial_zoom_y   = null;
				@initial_opacity  = null;
				@move_timer_start = null;
			}
		}
		if (@tone_timer_start) {
			@tone.red = lerp(@tone_initial.red, @tone_target.red, @tone_duration, @tone_timer_start, now);
			@tone.green = lerp(@tone_initial.green, @tone_target.green, @tone_duration, @tone_timer_start, now);
			@tone.blue = lerp(@tone_initial.blue, @tone_target.blue, @tone_duration, @tone_timer_start, now);
			@tone.gray = lerp(@tone_initial.gray, @tone_target.gray, @tone_duration, @tone_timer_start, now);
			if (now - @tone_timer_start >= @tone_duration) {
				@tone_initial = null;
				@tone_timer_start = null;
			}
		}
		if (@rotate_speed != 0) {
			if (!@rotate_timer) @rotate_timer = System.uptime;
			@angle += @rotate_speed * (System.uptime - @rotate_timer) * 20.0;
			@rotate_timer = System.uptime;
			while (@angle < 0) {
				@angle += 360;
			}
			@angle %= 360;
		}
	}
}
