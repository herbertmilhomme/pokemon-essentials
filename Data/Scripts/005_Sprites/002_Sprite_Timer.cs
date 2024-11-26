//===============================================================================
//
//===============================================================================
public partial class Sprite_Timer {
	public void initialize(viewport = null) {
		@viewport = viewport;
		@timer = null;
		@total_sec = null;
		@disposed = false;
	}

	public void dispose() {
		@timer&.dispose;
		@timer = null;
		@disposed = true;
	}

	public bool disposed() {
		@disposed;
	}

	public void update() {
		if (disposed()) return;
		if (Game.GameData.game_system.timer_start) {
			if (@timer) @timer.visible = true;
			if (!@timer) {
				@timer = Window_AdvancedTextPokemon.newWithSize("", Graphics.width - 120, 0, 120, 64);
				@timer.width = @timer.borderX + 96;
				@timer.x = Graphics.width - @timer.width;
				@timer.viewport = @viewport;
				@timer.z = 99998;
			}
			curtime = Game.GameData.game_system.timer;
			if (curtime < 0) curtime = 0;
			if (curtime != @total_sec) {
				// Calculate total number of seconds
				@total_sec = curtime;
				// Make a string for displaying the timer
				min = @total_sec / 60;
				sec = @total_sec % 60;
				@timer.text = string.Format("<ac>{1:02d}:{2:02d}", min, sec);
			}
			@timer.update;
		} else if (@timer) {
			@timer.visible = false;
		}
	}
}
