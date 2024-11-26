//===============================================================================
// Day and night system.
//===============================================================================
public void GetTimeNow() {
	return Time.now;
}

//===============================================================================
//
//===============================================================================
public static partial class DayNight {
	HOURLY_TONES = new {
		new Tone(-70, -90,  15, 55),   // Night           // Midnight
		new Tone(-70, -90,  15, 55),   // Night
		new Tone(-70, -90,  15, 55),   // Night
		new Tone(-70, -90,  15, 55),   // Night
		new Tone(-60, -70,  -5, 50),   // Night
		new Tone(-40, -50, -35, 50),   // Day/morning
		new Tone(-40, -50, -35, 50),   // Day/morning     // 6AM
		new Tone(-40, -50, -35, 50),   // Day/morning
		new Tone(-40, -50, -35, 50),   // Day/morning
		new Tone(-20, -25, -15, 20),   // Day/morning
		new Tone(  0,   0,   0,  0),   // Day
		new Tone(  0,   0,   0,  0),   // Day
		new Tone(  0,   0,   0,  0),   // Day             // Noon
		new Tone(  0,   0,   0,  0),   // Day
		new Tone(  0,   0,   0,  0),   // Day/afternoon
		new Tone(  0,   0,   0,  0),   // Day/afternoon
		new Tone(  0,   0,   0,  0),   // Day/afternoon
		new Tone(  0,   0,   0,  0),   // Day/afternoon
		new Tone( -5, -30, -20,  0),   // Day/evening     // 6PM
		new Tone(-15, -60, -10, 20),   // Day/evening
		new Tone(-15, -60, -10, 20),   // Day/evening
		new Tone(-40, -75,   5, 40),   // Night
		new Tone(-70, -90,  15, 55),   // Night
		new Tone(-70, -90,  15, 55);    // Night
	}
	public const int CACHED_TONE_LIFETIME = 30;   // In seconds; recalculates overworld tone once per this time

	@cachedTone = null;
	@dayNightToneLastUpdate = null;
	@oneOverSixty = 1 / 60.0;

	#region Class Functions
	#endregion

	// Returns true if it's day.
	public bool isDay(time = null) {
		if (!time) time = GetTimeNow;
		return (time.hour >= 5 && time.hour < 20);
	}

	// Returns true if it's night.
	public bool isNight(time = null) {
		if (!time) time = GetTimeNow;
		return (time.hour >= 20 || time.hour < 5);
	}

	// Returns true if it's morning.
	public bool isMorning(time = null) {
		if (!time) time = GetTimeNow;
		return (time.hour >= 5 && time.hour < 10);
	}

	// Returns true if it's the afternoon.
	public bool isAfternoon(time = null) {
		if (!time) time = GetTimeNow;
		return (time.hour >= 14 && time.hour < 17);
	}

	// Returns true if it's the evening.
	public bool isEvening(time = null) {
		if (!time) time = GetTimeNow;
		return (time.hour >= 17 && time.hour < 20);
	}

	// Gets a number representing the amount of daylight (0=full night, 255=full day).
	public void getShade() {
		time = GetDayNightMinutes;
		if (time > (12 * 60)) time = (24 * 60) - time;
		return 255 * time / (12 * 60);
	}

	// Gets a Tone object representing a suggested shading
	// tone for the current time of day.
	public void getTone() {
		if (!@cachedTone) @cachedTone = new Tone(0, 0, 0);
		if (!Settings.TIME_SHADING) return @cachedTone;
		if (!@dayNightToneLastUpdate || (System.uptime - @dayNightToneLastUpdate >= CACHED_TONE_LIFETIME)) {
			getToneInternal;
			@dayNightToneLastUpdate = System.uptime;
		}
		return @cachedTone;
	}

	public void GetDayNightMinutes() {
		now = GetTimeNow;   // Get the current in-game time
		return (now.hour * 60) + now.min;
	}

	public void getToneInternal() {
		// Calculates the tone for the current frame, used for day/night effects
		realMinutes = GetDayNightMinutes;
		hour   = realMinutes / 60;
		minute = realMinutes % 60;
		tone         = DayNight.HOURLY_TONES[hour];
		nexthourtone = DayNight.HOURLY_TONES[(hour + 1) % 24];
		// Calculate current tint according to current and next hour's tint and
		// depending on current minute
		@cachedTone.red   = ((nexthourtone.red - tone.red) * minute * @oneOverSixty) + tone.red;
		@cachedTone.green = ((nexthourtone.green - tone.green) * minute * @oneOverSixty) + tone.green;
		@cachedTone.blue  = ((nexthourtone.blue - tone.blue) * minute * @oneOverSixty) + tone.blue;
		@cachedTone.gray  = ((nexthourtone.gray - tone.gray) * minute * @oneOverSixty) + tone.gray;
	}
}

//===============================================================================
//
//===============================================================================
public void DayNightTint(object) {
	if (!Game.GameData.scene.is_a(Scene_Map)) return;
	if (Settings.TIME_SHADING && Game.GameData.game_map.metadata&.outdoor_map) {
		tone = DayNight.getTone;
		object.tone.set(tone.red, tone.green, tone.blue, tone.gray);
	} else {
		object.tone.set(0, 0, 0, 0);
	}
}

//===============================================================================
// Days of the week.
//===============================================================================
public void IsWeekday(wdayVariable, *arg) {
	timenow = GetTimeNow;
	wday = timenow.wday;
	ret = false;
	foreach (var wd in arg) { //'arg.each' do => |wd|
		if (wd == wday) ret = true;
	}
	if (wdayVariable > 0) {
		Game.GameData.game_variables[wdayVariable] = new {
			_INTL("Sunday"),
			_INTL("Monday"),
			_INTL("Tuesday"),
			_INTL("Wednesday"),
			_INTL("Thursday"),
			_INTL("Friday"),
			_INTL("Saturday");
		}[wday];
		if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
	}
	return ret;
}

//===============================================================================
// Months.
//===============================================================================
public void IsMonth(monVariable, *arg) {
	timenow = GetTimeNow;
	thismon = timenow.mon;
	ret = false;
	foreach (var wd in arg) { //'arg.each' do => |wd|
		if (wd == thismon) ret = true;
	}
	if (monVariable > 0) {
		Game.GameData.game_variables[monVariable] = GetMonthName(thismon);
		if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
	}
	return ret;
}

public void GetMonthName(month) {
	return new {_INTL("January"),
					_INTL("February"),
					_INTL("March"),
					_INTL("April"),
					_INTL("May"),
					_INTL("June"),
					_INTL("July"),
					_INTL("August"),
					_INTL("September"),
					_INTL("October"),
					_INTL("November"),
					_INTL("December")}[month - 1];
}

public void GetAbbrevMonthName(month) {
	return new {_INTL("Jan."),
					_INTL("Feb."),
					_INTL("Mar."),
					_INTL("Apr."),
					_INTL("May"),
					_INTL("Jun."),
					_INTL("Jul."),
					_INTL("Aug."),
					_INTL("Sep."),
					_INTL("Oct."),
					_INTL("Nov."),
					_INTL("Dec.")}[month - 1];
}

//===============================================================================
// Seasons.
//===============================================================================
public void GetSeason() {
	return (GetTimeNow.mon - 1) % 4;
}

public void IsSeason(seasonVariable, *arg) {
	thisseason = GetSeason;
	ret = false;
	foreach (var wd in arg) { //'arg.each' do => |wd|
		if (wd == thisseason) ret = true;
	}
	if (seasonVariable > 0) {
		Game.GameData.game_variables[seasonVariable] = new {_INTL("Spring"),
																			_INTL("Summer"),
																			_INTL("Autumn"),
																			_INTL("Winter")}[thisseason];
		if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
	}
	return ret;
}

public int IsSpring { get { return IsSeason(0, 0); } }; // Jan, May, Sep
public int IsSummer { get { return IsSeason(0, 1); } }; // Feb, Jun, Oct
public int IsAutumn { get { return IsSeason(0, 2); } }; // Mar, Jul, Nov
public int IsFall { get { return IsAutumn; } };
public int IsWinter { get { return IsSeason(0, 3); } }; // Apr, Aug, Dec

public void GetSeasonName(season) {
	return new {_INTL("Spring"),
					_INTL("Summer"),
					_INTL("Autumn"),
					_INTL("Winter")}[season];
}

//===============================================================================
// Moon phases and Zodiac.
//===============================================================================
// Calculates the phase of the moon. time is in UTC.
// 0 - New Moon
// 1 - Waxing Crescent
// 2 - First Quarter
// 3 - Waxing Gibbous
// 4 - Full Moon
// 5 - Waning Gibbous
// 6 - Last Quarter
// 7 - Waning Crescent
public void moonphase(time = null) {
	if (!time) time = GetTimeNow;
	transitions = new {
		1.8456618033125,
		5.5369854099375,
		9.2283090165625,
		12.9196326231875,
		16.6109562298125,
		20.3022798364375,
		23.9936034430625,
		27.6849270496875;
	}
	yy = time.year - (int)Math.Floor((12 - time.mon) / 10.0);
	j = (int)Math.Floor(365.25 * (4712 + yy)) + (int)Math.Floor((((time.mon + 9) % 12) * 30.6) + 0.5) + time.day + 59;
	if (j > 2_299_160) j -= (int)Math.Floor((int)Math.Floor((yy / 100.0) + 49) * 0.75) - 38;
	j += (((time.hour * 60) + (time.min * 60)) + time.sec) / 86_400.0;
	v = (j - 2_451_550.1) / 29.530588853;
	v = ((v - v.floor) + (v < 0 ? 1 : 0));
	ag = v * 29.53;
	for (int i = transitions.length; i < transitions.length; i++) { //for 'transitions.length' times do => |i|
		if (ag <= transitions[i]) return i;
	}
	return 0;
}

// Calculates the zodiac sign based on the given month and day:
// 0 is Aries, 11 is Pisces. Month is 1 if January, and so on.
public void zodiac(month, day) {
	time = new {
		3, 21, 4, 19,   // Aries
		4, 20, 5, 20,   // Taurus
		5, 21, 6, 20,   // Gemini
		6, 21, 7, 20,   // Cancer
		7, 23, 8, 22,   // Leo
		8, 23, 9, 22,   // Virgo
		9, 23, 10, 22,  // Libra
		10, 23, 11, 21, // Scorpio
		11, 22, 12, 21, // Sagittarius
		12, 22, 1, 19,  // Capricorn
		1, 20, 2, 18,   // Aquarius
		2, 19, 3, 20;    // Pisces
	}
	for (int i = (time.length / 4); i < (time.length / 4); i++) { //for '(time.length / 4)' times do => |i|
		if (month == time[i * 4] && day >= time[(i * 4) + 1]) return i;
		if (month == time[(i * 4) + 2] && day <= time[(i * 4) + 3]) return i;
	}
	return 0;
}

// Returns the opposite of the given zodiac sign.
// 0 is Aries, 11 is Pisces.
public void zodiacOpposite(sign) {
	return (sign + 6) % 12;
}

// 0 is Aries, 11 is Pisces.
public void zodiacPartners(sign) {
	return new {(sign + 4) % 12, (sign + 8) % 12};
}

// 0 is Aries, 11 is Pisces.
public void zodiacComplements(sign) {
	return new {(sign + 1) % 12, (sign + 11) % 12};
}
