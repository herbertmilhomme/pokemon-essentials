//===============================================================================
// ** Game_System
//------------------------------------------------------------------------------
//  This class handles data surrounding the system. Backround music, etc.
//  is managed here as well. Refer to "Game.GameData.game_system" for the instance of
//  this class.
//===============================================================================
public partial class Game_System {
	/// <summary>map event interpreter</summary>
	public int map_interpreter		{ get { return _map_interpreter; } }			protected int _map_interpreter;
	/// <summary>battle event interpreter</summary>
	public int battle_interpreter		{ get { return _battle_interpreter; } }			protected int _battle_interpreter;
	/// <summary>Game.GameData.stats.play_time when timer was started, or null</summary>
	public int timer_start		{ get { return _timer_start; } set { _timer_start = value; } }			protected int _timer_start;
	/// <summary>Time (in seconds) the timer is initially set to</summary>
	public int timer_duration		{ get { return _timer_duration; } set { _timer_duration = value; } }			protected int _timer_duration;
	/// <summary>save forbidden</summary>
	public int save_disabled		{ get { return _save_disabled; } set { _save_disabled = value; } }			protected int _save_disabled;
	/// <summary>menu forbidden</summary>
	public int menu_disabled		{ get { return _menu_disabled; } set { _menu_disabled = value; } }			protected int _menu_disabled;
	/// <summary>encounter forbidden</summary>
	public int encounter_disabled		{ get { return _encounter_disabled; } set { _encounter_disabled = value; } }			protected int _encounter_disabled;
	/// <summary>text option: positioning</summary>
	public int message_position		{ get { return _message_position; } set { _message_position = value; } }			protected int _message_position;
	/// <summary>text option: window frame</summary>
	public int message_frame		{ get { return _message_frame; } set { _message_frame = value; } }			protected int _message_frame;
	/// <summary>save count</summary>
	public int save_count		{ get { return _save_count; } set { _save_count = value; } }			protected int _save_count;
	/// <summary>magic number</summary>
	public int magic_number		{ get { return _magic_number; } set { _magic_number = value; } }			protected int _magic_number;
	public int autoscroll_x_speed		{ get { return _autoscroll_x_speed; } set { _autoscroll_x_speed = value; } }			protected int _autoscroll_x_speed;
	public int autoscroll_y_speed		{ get { return _autoscroll_y_speed; } set { _autoscroll_y_speed = value; } }			protected int _autoscroll_y_speed;
	public int bgm_position		{ get { return _bgm_position; } set { _bgm_position = value; } }			protected int _bgm_position;

	public void initialize() {
		@map_interpreter    = new Interpreter(0, true);
		@battle_interpreter = new Interpreter(0, false);
		@timer_start        = null;
		@timer_duration     = 0;
		@save_disabled      = false;
		@menu_disabled      = false;
		@encounter_disabled = false;
		@message_position   = 2;
		@message_frame      = 0;
		@save_count         = 0;
		@magic_number       = 0;
		@autoscroll_x_speed = 0;
		@autoscroll_y_speed = 0;
		@bgm_position       = 0;
		@bgs_position       = 0;
	}

	public void battle_bgm() {
		return (@battle_bgm) ? @battle_bgm : Game.GameData.data_system.battle_bgm;
	}

	public int battle_bgm		{ get { return _battle_bgm; } }			protected int _battle_bgm;

	public void battle_end_me() {
		return (@battle_end_me) ? @battle_end_me : Game.GameData.data_system.battle_end_me;
	}

	public int battle_end_me		{ get { return _battle_end_me; } }			protected int _battle_end_me;

	public void windowskin_name() {
		if (@windowskin_name.null()) return Game.GameData.data_system.windowskin_name;
		return @windowskin_name;
	}

	public int windowskin_name		{ get { return _windowskin_name; } }			protected int _windowskin_name;

	public void timer() {
		if (!@timer_start || !Game.GameData.stats) return 0;
		return @timer_duration - Game.GameData.stats.play_time + @timer_start;
	}

	//-----------------------------------------------------------------------------

	public void bgm_play(bgm, track = null) {
		old_pos = @bgm_position;
		@bgm_position = 0;
		bgm_play_internal(bgm, 0, track);
		@bgm_position = old_pos;
	}

	public void bgm_play_internal2(name, volume, pitch, position, track = null) {// :nodoc:
		vol = volume;
		vol *= Game.GameData.PokemonSystem.bgmvolume / 100.0;
		vol = vol.ToInt();
		begin;
			Audio.bgm_play(name, vol, pitch, position, track);
		rescue ArgumentError;
			Audio.bgm_play(name, vol, pitch, 0, track);
		}
	}

	public void bgm_play_internal(bgm, position, track = null) {// :nodoc:
		if (!track || track == 0) {
			if (!@bgm_paused) @bgm_position = position;
			@playing_bgm = bgm&.clone;
		}
		if (bgm && bgm.name != "") {
			if (!@defaultBGM && FileTest.audio_exist("Audio/BGM/" + bgm.name)) {
				bgm_play_internal2("Audio/BGM/" + bgm.name, bgm.volume, bgm.pitch, @bgm_position, track);
			}
		} else {
			if (!track || track == 0) {
				if (!@bgm_paused) @bgm_position = position;
				@playing_bgm = null;
			}
			if (!@defaultBGM) Audio.bgm_stop(track);
		}
		if (@defaultBGM) {
			bgm_play_internal2("Audio/BGM/" + @defaultBGM.name,
												@defaultBGM.volume, @defaultBGM.pitch, @bgm_position, track);
		}
		Graphics.frame_reset;
	}

	public void bgm_pause(fadetime = 0.0) {// :nodoc:
		pos = Audio.bgm_pos rescue 0;
		if (fadetime > 0.0) self.bgm_fade(fadetime);
		@bgm_position = pos;
		@bgm_paused   = true;
	}

	public void bgm_unpause  () {// :nodoc:
		@bgm_position = 0;
		@bgm_paused   = false;
	}

	public void bgm_resume(bgm) {// :nodoc:
		if (@bgm_paused) {
			self.bgm_play_internal(bgm, @bgm_position);
			@bgm_position = 0;
			@bgm_paused   = false;
		}
	}

	public void bgm_stop(track = null) {// :nodoc:
		if (!track || track == 0) {
			if (!@bgm_paused) @bgm_position = 0;
			@playing_bgm  = null;
		}
		if (!@defaultBGM) Audio.bgm_stop(track);
	}

	public void bgm_fade(time, track = null) {// :nodoc:
		if (!track || track == 0) {
			if (!@bgm_paused) @bgm_position = 0;
			@playing_bgm = null;
		}
		if (!@defaultBGM) Audio.bgm_fade((int)Math.Floor(time * 1000), track);
	}

	public void playing_bgm() {
		return @playing_bgm;
	}

	// Saves the currently playing background music for later playback.
	public void bgm_memorize() {
		@memorized_bgm = @playing_bgm;
	}

	// Plays the currently memorized background music
	public void bgm_restore() {
		bgm_play(@memorized_bgm);
	}

	// Returns an RPG.AudioFile object for the currently playing background music
	public void getPlayingBGM() {
		return (@playing_bgm) ? @playing_bgm.clone : null;
	}

	public void setDefaultBGM(bgm, volume = 80, pitch = 100) {
		if (bgm.is_a(String)) bgm = new RPG.AudioFile(bgm, volume, pitch);
		@defaultBGM = null;
		if (bgm && bgm.name != "") {
			self.bgm_play(bgm);
			@defaultBGM = bgm.clone;
		} else {
			self.bgm_play(@playing_bgm);
		}
	}

	//-----------------------------------------------------------------------------

	public void me_play(me) {
		if (me.is_a(String)) me = new RPG.AudioFile(me);
		if (me && me.name != "") {
			if (FileTest.audio_exist("Audio/ME/" + me.name)) {
				vol = me.volume;
				vol *= Game.GameData.PokemonSystem.bgmvolume / 100.0;
				vol = vol.ToInt();
				Audio.me_play("Audio/ME/" + me.name, vol, me.pitch);
			}
		} else {
			Audio.me_stop;
		}
		Graphics.frame_reset;
	}

	//-----------------------------------------------------------------------------

	public void bgs_play(bgs) {
		@playing_bgs = (bgs.null()) ? null : bgs.clone;
		if (bgs && bgs.name != "") {
			if (FileTest.audio_exist("Audio/BGS/" + bgs.name)) {
				vol = bgs.volume;
				vol *= Game.GameData.PokemonSystem.sevolume / 100.0;
				vol = vol.ToInt();
				Audio.bgs_play("Audio/BGS/" + bgs.name, vol, bgs.pitch);
			}
		} else {
			@bgs_position = 0;
			@playing_bgs  = null;
			Audio.bgs_stop;
		}
		Graphics.frame_reset;
	}

	public void bgs_pause(fadetime = 0.0) {// :nodoc:
		if (fadetime > 0.0) {
			self.bgs_fade(fadetime);
		} else {
			self.bgs_stop;
		}
		@bgs_paused = true;
	}

	public void bgs_unpause  () {// :nodoc:
		@bgs_paused = false;
	}

	public void bgs_resume(bgs) {// :nodoc:
		if (@bgs_paused) {
			self.bgs_play(bgs);
			@bgs_paused = false;
		}
	}

	public void bgs_stop() {
		@bgs_position = 0;
		@playing_bgs  = null;
		Audio.bgs_stop;
	}

	public void bgs_fade(time) {
		@bgs_position = 0;
		@playing_bgs  = null;
		Audio.bgs_fade((int)Math.Floor(time * 1000));
	}

	public void playing_bgs() {
		return @playing_bgs;
	}

	public void bgs_memorize() {
		@memorized_bgs = @playing_bgs;
	}

	public void bgs_restore() {
		bgs_play(@memorized_bgs);
	}

	public void getPlayingBGS() {
		return (@playing_bgs) ? @playing_bgs.clone : null;
	}

	//-----------------------------------------------------------------------------

	public void se_play(se) {
		if (se.is_a(String)) se = new RPG.AudioFile(se);
		if (se && se.name != "" && FileTest.audio_exist("Audio/SE/" + se.name)) {
			vol = se.volume;
			vol *= Game.GameData.PokemonSystem.sevolume / 100.0;
			vol = vol.ToInt();
			Audio.se_play("Audio/SE/" + se.name, vol, se.pitch);
		}
	}

	public void se_stop() {
		Audio.se_stop;
	}

	//-----------------------------------------------------------------------------

	public void update() {
		if (Input.trigger(Input.SPECIAL) && CurrentEventCommentInput(1, "Cut Scene")) {
			event = @map_interpreter.get_self;
			@map_interpreter.SetSelfSwitch(event.id, "A", true);
			@map_interpreter.command_end;
			event.start;
		}
	}
}
