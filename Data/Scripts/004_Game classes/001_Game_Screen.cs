//===============================================================================
// ** Game_Screen
//-------------------------------------------------------------------------------
//  This class handles screen maintenance data, such as change in color tone,
//  flashing, etc. Refer to "Game.GameData.game_screen" for the instance of this class.
//===============================================================================
public partial class Game_Screen {
	/// <summary>brightness</summary>
	public int brightness		{ get { return _brightness; } }			protected int _brightness;
	/// <summary>color tone</summary>
	public int tone		{ get { return _tone; } }			protected int _tone;
	/// <summary>flash color</summary>
	public int flash_color		{ get { return _flash_color; } }			protected int _flash_color;
	/// <summary>shake positioning</summary>
	public int shake		{ get { return _shake; } }			protected int _shake;
	/// <summary>pictures</summary>
	public int pictures		{ get { return _pictures; } }			protected int _pictures;
	/// <summary>weather type</summary>
	public int weather_type		{ get { return _weather_type; } }			protected int _weather_type;
	/// <summary>max number of weather sprites</summary>
	public int weather_max		{ get { return _weather_max; } }			protected int _weather_max;
	/// <summary>ticks in which the weather should fade in</summary>
	public int weather_duration		{ get { return _weather_duration; } set { _weather_duration = value; } }			protected int _weather_duration;

	public void initialize() {
		@brightness        = 255;
		@tone              = new Tone(0, 0, 0, 0);
		@tone_target       = new Tone(0, 0, 0, 0);
		@tone_duration     = 0;
		@tone_timer_start  = null;
		@flash_color       = new Color(0, 0, 0, 0);
		@flash_duration    = 0;
		@flash_timer_start = null;
		@shake_power       = 0;
		@shake_speed       = 0;
		@shake_duration    = 0;
		@shake             = 0;
		@pictures          = [null];
		(1..100).each(i => @pictures.Add(new Game_Picture(i)))
		@weather_type      = :None;
		@weather_max       = 0.0;
		@weather_duration  = 0;
	}

	// duration is time in 1/20ths of a second.
	public void start_tone_change(tone, duration) {
		if (duration == 0) {
			@tone = tone.clone;
			return;
		}
		@tone_initial     = @tone.clone;
		@tone_target      = tone.clone;
		@tone_duration    = duration / 20.0;
		@tone_timer_start = Game.GameData.stats.play_time;
	}

	// duration is time in 1/20ths of a second.
	public void start_flash(color, duration) {
		@flash_color         = color.clone;
		@flash_initial_alpha = @flash_color.alpha;
		@flash_duration      = duration / 20.0;
		@flash_timer_start   = Game.GameData.stats.play_time;
	}

	// duration is time in 1/20ths of a second.
	public void start_shake(power, speed, duration) {
		@shake_power       = power;
		@shake_speed       = speed;
		@shake_duration    = duration / 20.0;
		@shake_timer_start = Game.GameData.stats.play_time;
	}

	// duration is time in 1/20ths of a second.
	public void weather(type, power, duration) {
		@weather_type     = GameData.Weather.get(type).id;
		@weather_max      = (power + 1) * RPG.Weather.MAX_SPRITES / 10;
		@weather_duration = duration;
	}

	public void update() {
		now = Game.GameData.stats.play_time;
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
		if (@flash_timer_start) {
			@flash_color.alpha = lerp(@flash_initial_alpha, 0, @flash_duration, @flash_timer_start, now);
			if (now - @flash_timer_start >= @flash_duration) {
				@flash_initial_alpha = null;
				@flash_timer_start = null;
			}
		}
		if (@shake_timer_start) {
			delta_t = now - @shake_timer_start;
			movement_per_second = @shake_power * @shake_speed * 4;
			limit = @shake_power * 2.5;   // Maximum pixel displacement
			phase = (delta_t * movement_per_second / limit).ToInt() % 4;
			switch (phase) {
				case 0: case 2:
					@shake = (movement_per_second * delta_t) % limit;
					if (phase == 2) @shake *= -1;
				default:
					@shake = limit - ((movement_per_second * delta_t) % limit);
					if (phase == 3) @shake *= -1;
					break;
			}
			if (delta_t >= @shake_duration) {
				if (!@shake_phase || phase == 1 || phase == 3) @shake_phase = phase;
				if (phase != @shake_phase || @shake < 2) {
					@shake_timer_start = null;
					@shake = 0;
				}
			}
		}
		if (Game.GameData.game_temp.in_battle) {
			(51..100).each(i => @pictures[i].update)
		} else {
			(1..50).each(i => @pictures[i].update)
		}
	}
}

//===============================================================================
//
//===============================================================================
public void ToneChangeAll(tone, duration) {
	Game.GameData.game_screen.start_tone_change(tone, duration);
	Game.GameData.game_screen.pictures.each(picture => picture&.start_tone_change(tone, duration));
}

public void Flash(color, frames) {
	Game.GameData.game_screen.start_flash(color, frames);
}

public void Shake(power, speed, frames) {
	Game.GameData.game_screen.start_shake(power, speed, frames);
}
