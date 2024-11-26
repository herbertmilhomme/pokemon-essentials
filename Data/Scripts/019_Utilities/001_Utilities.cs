//===============================================================================
// General purpose utilities.
//===============================================================================
public void _pbNextComb(comb, length) {
	i = comb.length - 1;
	do { //loop; while (true);
		valid = true;
		for (int j = i; j < comb.length; j++) { //each 'comb.length' do => |j|
			if (j == i) {
				comb[j] += 1;
			} else {
				comb[j] = comb[i] + (j - i);
			}
			if (comb[j] >= length) {
				valid = false;
				break;
			}
		}
		if (valid) return true;
		i -= 1;
		unless (i >= 0) break;
	}
	return false;
}

// Iterates through the array and yields each combination of _num_ elements in
// the array.
public void EachCombination(array, num) {
	if (array.length < num || num <= 0) return;
	if (array.length == num) {
		yield array;
		return;
	} else if (num == 1) {
		foreach (var x in array) { //'array.each' do => |x|
			yield [x];
		}
		return;
	}
	currentComb = new List<string>();
	arr = new List<string>();
	num.times(i => currentComb[i] = i);
	do { //loop; while (true);
		num.times(i => arr[i] = array[currentComb[i]]);
		yield arr;
		unless (_pbNextComb(currentComb, array.length)) break;
	}
}

// Returns a language ID
public void GetLanguage() {
	switch (System.user_language[0..1]) {
		case "ja":  return 1;   // Japanese
		case "en":  return 2;   // English
		case "fr":  return 3;   // French
		case "it":  return 4;   // Italian
		case "de":  return 5;   // German
		case "es":  return 7;   // Spanish
		case "ko":  return 8;   // Korean
	}
	return 2; // Use 'English' by default
}

// Converts a Celsius temperature to Fahrenheit.
public void toFahrenheit(celsius) {
	return (int)Math.Round(celsius * 9.0 / 5.0) + 32;
}

// Converts a Fahrenheit temperature to Celsius.
public void toCelsius(fahrenheit) {
	return (int)Math.Round((fahrenheit - 32) * 5.0 / 9.0);
}

//===============================================================================
// This class is designed to favor different values more than a uniform
// random generator does.
//===============================================================================
public partial class AntiRandom {
	public void initialize(size) {
		@old = new List<string>();
		@new = new Array(size, i => { i; });
	}

	public void get() {
		if (@new.length == 0) {   // No new values
			@new = @old.clone;
			@old.clear;
		}
		if (@old.length > 0 && rand(7) == 0) {   // Get old value
			return @old[rand(@old.length)];
		}
		if (@new.length > 0) {   // Get new value
			ret = @new.delete_at(rand(@new.length));
			@old.Add(ret);
			return ret;
		}
		return @old[rand(@old.length)];   // Get old value
	}
}

//===============================================================================
// Constants utilities.
//===============================================================================
// Unused
public bool isConst(val, mod, constant) {
	begin;
		if (!mod.const_defined(constant.to_sym)) return false;
	rescue;
		return false;
	}
	return (val == mod.const_get(constant.to_sym));
}

// Unused
public bool hasConst(mod, constant) {
	if (!mod || constant.null()) return false;
	return mod.const_defined(constant.to_sym) rescue false;
}

// Unused
public void getConst(mod, constant) {
	if (!mod || constant.null()) return null;
	return mod.const_get(constant.to_sym) rescue null;
}

// Unused
public void getID(mod, constant) {
	if (!mod || constant.null()) return null;
	if (constant.is_a(Symbol) || constant.is_a(String)) {
		if ((mod.const_defined(constant.to_sym) rescue false)) {
			return mod.const_get(constant.to_sym) rescue 0;
		}
		return 0;
	}
	return constant;
}

public void getConstantName(mod, value, raise_if_none = true) {
	if (mod.is_a(Symbol)) mod = Object.const_get(mod);
	foreach (var c in mod.constants) { //'mod.constants.each' do => |c|
		if (mod.const_get(c.to_sym) == value) return c.ToString();
	}
	if (raise_if_none) raise _INTL("Value {1} not defined by a constant in {2}", value, mod.name);
	return null;
}

public void getConstantNameOrValue(mod, value) {
	if (mod.is_a(Symbol)) mod = Object.const_get(mod);
	foreach (var c in mod.constants) { //'mod.constants.each' do => |c|
		if (mod.const_get(c.to_sym) == value) return c.ToString();
	}
	return value.inspect;
}

//===============================================================================
// Event utilities.
//===============================================================================
public void TimeEvent(variableNumber, secs = 86_400) {
	if (!Game.GameData.game_variables) return;
	if (!variableNumber || variableNumber < 0) return;
	if (secs < 0) secs = 0;
	timenow = GetTimeNow;
	Game.GameData.game_variables[variableNumber] = new {timenow.to_f, secs};
	Game.GameData.game_map&.refresh;
}

public void TimeEventDays(variableNumber, days = 0) {
	if (!Game.GameData.game_variables) return;
	if (!variableNumber || variableNumber < 0) return;
	if (days < 0) days = 0;
	timenow = GetTimeNow;
	time = timenow.to_f;
	expiry = (time % 86_400.0) + (days * 86_400.0);
	Game.GameData.game_variables[variableNumber] = new {time, expiry - time};
	Game.GameData.game_map&.refresh;
}

public void TimeEventValid(variableNumber) {
	if (!Game.GameData.game_variables) return false;
	if (!variableNumber || variableNumber < 0) return false;
	ret = false;
	value = Game.GameData.game_variables[variableNumber];
	if (value.Length > 0) {
		timenow = GetTimeNow;
		ret = (timenow.to_f - value[0] > value[1]);   // value[1] is age in seconds
		if (value[1] <= 0) ret = false;   // zero age
	}
	if (!ret) {
		Game.GameData.game_variables[variableNumber] = 0;
		Game.GameData.game_map&.refresh;
	}
	return ret;
}

public void Exclaim(events, anim = Settings.EXCLAMATION_ANIMATION_ID, tinting = false) {
	if (!events.Length > 0) events = [events];
	foreach (var ev in events) { //'events.each' do => |ev|
		ev.animation_id = anim;
		ev.animation_height = 3;
		ev.animation_regular_tone = !tinting;
	}
	anim_data = Game.GameData.data_animations[anim];
	frame_count = anim_data.frame_max;
	frame_rate = 20;
	if (System.Text.RegularExpressions.Regex.IsMatch(anim_data.name,@"\[\s*(\d+?)\s*\]\s*$")) {
		frame_rate = $~[1].ToInt();
	}
	Wait(frame_count / frame_rate.to_f);
	events.each(i => i.animation_id = 0);
}

public void NoticePlayer(event, always_show_exclaim = false) {
	if (always_show_exclaim || !FacingEachOther(event, Game.GameData.game_player)) {
		Exclaim(event);
	}
	TurnTowardEvent(Game.GameData.game_player, event);
	MoveTowardPlayer(event);
}

//===============================================================================
// Player-related utilities, random name generator.
//===============================================================================
// Unused
public void GetPlayerGraphic() {
	id = Game.GameData.player.character_ID;
	if (id < 1) return "";
	meta = GameData.PlayerMetadata.get(id);
	if (!meta) return "";
	return GameData.TrainerType.player_front_sprite_filename(meta.trainer_type);
}

public void GetTrainerTypeGender(trainer_type) {
	return GameData.TrainerType.get(trainer_type).gender;
}

public void ChangePlayer(id) {
	if (id < 1) return false;
	meta = GameData.PlayerMetadata.get(id);
	if (!meta) return false;
	Game.GameData.player.character_ID = id;
	return true;
}

public void TrainerName(name = null, outfit = 0) {
	if (Game.GameData.player.character_ID < 1) ChangePlayer(1);
	if (name.null()) {
		name = EnterPlayerName(_INTL("Your name?"), 0, Settings.MAX_PLAYER_NAME_SIZE);
		if (name.null() || name.empty()) {
			player_metadata = GameData.PlayerMetadata.get(Game.GameData.player.character_ID);
			trainer_type = (player_metadata) ? player_metadata.trainer_type : null;
			gender = GetTrainerTypeGender(trainer_type);
			name = SuggestTrainerName(gender);
		}
	}
	Game.GameData.player.name   = name;
	Game.GameData.player.outfit = outfit;
}

public void SuggestTrainerName(gender) {
	userName = GetUserName;
	userName = System.Text.RegularExpressions.Regex.Replace(userName, "\s+.*$", "");
	if (userName.length > 0 && userName.length < Settings.MAX_PLAYER_NAME_SIZE) {
		userName[0, 1] = userName[0, 1].upcase;
		return userName;
	}
	userName = System.Text.RegularExpressions.Regex.Replace(userName, "\d+$", "");
	if (userName.length > 0 && userName.length < Settings.MAX_PLAYER_NAME_SIZE) {
		userName[0, 1] = userName[0, 1].upcase;
		return userName;
	}
	userName = System.user_name.capitalize;
	userName = userName[0, Settings.MAX_PLAYER_NAME_SIZE];
	return userName;
	// Unreachable
//  return getRandomNameEx(gender, null, 1, Settings.MAX_PLAYER_NAME_SIZE)
}

public void GetUserName() {
	return System.user_name;
}

public void getRandomNameEx(type, variable, upper, maxLength = 100) {
	if (maxLength <= 0) return "";
	name = "";
	50.times do;
		name = "";
		formats = new List<string>()
		switch (type) {
			case 0:  formats = new {"F5", "BvE", "FE", "FE5", "FEvE"}; break;                    // Names for males
			case 1:  formats = new {"vE6", "vEvE6", "BvE6", "B4", "v3", "vEv3", "Bv3"}; break;   // Names for females
			case 2:  formats = new {"WE", "WEU", "WEvE", "BvE", "BvEU", "BvEvE"}; break;         // Neutral gender names
			default:        return "";
		}
		format = formats.sample
		foreach (var c in format.scan(/./)) { //format.scan(/./) do => |c|
			switch (c) {
				case "c":   // consonant
					set = new {"b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r",
								"s", "t", "v", "w", "x", "z"};
					name += set.sample;
					break;
				case "v":   // vowel
					set = new {"a", "a", "a", "e", "e", "e", "i", "i", "i", "o", "o", "o", "u", "u", "u"};
					name += set.sample;
					break;
				case "W":   // beginning vowel
					set = new {"a", "a", "a", "e", "e", "e", "i", "i", "i", "o", "o", "o", "u",
								"u", "u", "au", "au", "ay", "ay", "ea", "ea", "ee", "ee", "oo",
								"oo", "ou", "ou"};
					name += set.sample;
					break;
				case "U":   // ending vowel
					set = new {"a", "a", "a", "a", "a", "e", "e", "e", "i", "i", "i", "o", "o",
								"o", "o", "o", "u", "u", "ay", "ay", "ie", "ie", "ee", "ue", "oo"};
					name += set.sample;
					break;
				case "B":   // beginning consonant
					set1 = new {"b", "c", "d", "f", "g", "h", "j", "k", "l", "l", "m", "n", "n",
									"p", "r", "r", "s", "s", "t", "t", "v", "w", "y", "z"};
					set2 = new {"bl", "br", "ch", "cl", "cr", "dr", "fr", "fl", "gl", "gr", "kh",
									"kl", "kr", "ph", "pl", "pr", "sc", "sk", "sl", "sm", "sn", "sp",
									"st", "sw", "th", "tr", "tw", "vl", "zh"};
					name += (rand(3) > 0) ? set1.sample : set2.sample;
					break;
				case "E":   // ending consonant
					set1 = new {"b", "c", "d", "f", "g", "h", "j", "k", "k", "l", "l", "m", "n",
									"n", "p", "r", "r", "s", "s", "t", "t", "v", "z"};
					set2 = new {"bb", "bs", "ch", "cs", "ds", "fs", "ft", "gs", "gg", "ld", "ls",
									"nd", "ng", "nk", "rn", "kt", "ks", "ms", "ns", "ph", "pt", "ps",
									"sk", "sh", "sp", "ss", "st", "rd", "rn", "rp", "rm", "rt", "rk",
									"ns", "th", "zh"};
					name += (rand(3) > 0) ? set1.sample : set2.sample;
					break;
				case "f":   // consonant and vowel
					set = new {"iz", "us", "or"};
					name += set.sample;
					break;
				case "F":   // consonant and vowel
					set = new {"bo", "ba", "be", "bu", "re", "ro", "si", "mi", "zho", "se", "nya",
								"gru", "gruu", "glee", "gra", "glo", "ra", "do", "zo", "ri", "di",
								"ze", "go", "ga", "pree", "pro", "po", "pa", "ka", "ki", "ku",
								"de", "da", "ma", "mo", "le", "la", "li"};
					name += set.sample;
					break;
				case "2":
					set = new {"c", "f", "g", "k", "l", "p", "r", "s", "t"};
					name += set.sample;
					break;
				case "3":
					set = new {"nka", "nda", "la", "li", "ndra", "sta", "cha", "chie"};
					name += set.sample;
					break;
				case "4":
					set = new {"una", "ona", "ina", "ita", "ila", "ala", "ana", "ia", "iana"};
					name += set.sample;
					break;
				case "5":
					set = new {"e", "e", "o", "o", "ius", "io", "u", "u", "ito", "io", "ius", "us"};
					name += set.sample;
					break;
				case "6":
					set = new {"a", "a", "a", "elle", "ine", "ika", "ina", "ita", "ila", "ala", "ana"};
					name += set.sample;
					break;
			}
		}
		if (name.length <= maxLength) break;
	}
	name = name[0, maxLength];
	switch (upper) {
		case 0:  name = name.upcase; break;
		case 1:  name[0, 1] = name[0, 1].upcase; break;
	}
	if (Game.GameData.game_variables && variable) {
		Game.GameData.game_variables[variable] = name;
		if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
	}
	return name;
}

public void getRandomName(maxLength = 100) {
	return getRandomNameEx(2, null, null, maxLength);
}

//===============================================================================
// Regional and National Pokédexes utilities.
//===============================================================================
// Returns the ID number of the region containing the player's current location,
// as determined by the current map's metadata.
public void GetCurrentRegion(default = -1) {
	if (!Game.GameData.game_map) return default;
	map_pos = Game.GameData.game_map.metadata&.town_map_position;
	return (map_pos) ? map_pos[0] : default;
}

// Returns the Regional Pokédex number of the given species in the given Regional
// Dex. The parameter "region" is zero-based. For example, if two regions are
// defined, they would each be specified as 0 and 1.
public void GetRegionalNumber(region, species) {
	dex_list = LoadRegionalDexes[region];
	if (!dex_list || dex_list.length == 0) return 0;
	species_data = GameData.Species.try_get(species);
	if (!species_data) return 0;
	dex_list.each_with_index do |s, index|
		if (s == species_data.species) return index + 1;
	}
	return 0;
}

// Returns an array of all species in the given Regional Dex in that Dex's order.
public void AllRegionalSpecies(region_dex) {
	if (region_dex < 0) return null;
	dex_list = LoadRegionalDexes[region_dex];
	if (!dex_list || dex_list.length == 0) return null;
	return dex_list.clone;
}

// Returns the number of species in the given Regional Dex. Returns 0 if that
// Regional Dex doesn't exist. If region_dex is a negative number, returns the
// number of species in the National Dex (i.e. all species).
public void GetRegionalDexLength(region_dex) {
	if (region_dex < 0) {
		ret = 0;
		GameData.Species.each_species(s => ret += 1);
		return ret;
	}
	dex_list = LoadRegionalDexes[region_dex];
	return (dex_list) ? dex_list.length : 0;
}

//===============================================================================
// Other utilities.
//===============================================================================
public void TextEntry(helptext, minlength, maxlength, variableNumber) {
	Game.GameData.game_variables[variableNumber] = EnterText(helptext, minlength, maxlength);
	if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
}

public void MoveTutorAnnotations(move, movelist = null) {
	ret = new List<string>();
	Game.GameData.player.party.each_with_index do |pkmn, i|
		if (pkmn.egg()) {
			ret[i] = _INTL("NOT ABLE");
		} else if (pkmn.hasMove(move)) {
			ret[i] = _INTL("LEARNED");
		} else {
			species = pkmn.species;
			if (movelist&.any(j => j == species)) {
				// Checked data from movelist given in parameter
				ret[i] = _INTL("ABLE");
			} else if (pkmn.compatible_with_move(move)) {
				// Checked data from Pokémon's tutor moves in pokemon.txt
				ret[i] = _INTL("ABLE");
			} else {
				ret[i] = _INTL("NOT ABLE");
			}
		}
	}
	return ret;
}

public void MoveTutorChoose(move, movelist = null, bymachine = false, oneusemachine = false) {
	ret = false;
	move = GameData.Move.get(move).id;
	if (movelist.Length > 0) {
		movelist.map! { |m| GameData.Move.get(m).id };
	}
	FadeOutIn do;
		movename = GameData.Move.get(move).name;
		annot = MoveTutorAnnotations(move, movelist);
		scene = new PokemonParty_Scene();
		screen = new PokemonPartyScreen(scene, Game.GameData.player.party);
		screen.StartScene(_INTL("Teach which Pokémon?"), false, annot);
		do { //loop; while (true);
			chosen = screen.ChoosePokemon;
			if (chosen < 0) break;
			pokemon = Game.GameData.player.party[chosen];
			if (pokemon.egg()) {
				Message(_INTL("Eggs can't be taught any moves.")) { screen.Update };
			} else if (pokemon.shadowPokemon()) {
				Message(_INTL("Shadow Pokémon can't be taught any moves.")) { screen.Update };
			} else if (movelist && movelist.none(j => j == pokemon.species)) {
				Message(_INTL("{1} can't learn {2}.", pokemon.name, movename)) { screen.Update };
			} else if (!pokemon.compatible_with_move(move)) {
				Message(_INTL("{1} can't learn {2}.", pokemon.name, movename)) { screen.Update };
			} else if (LearnMove(pokemon, move, false, bymachine) { screen.Update }) {
				if (bymachine) Game.GameData.stats.moves_taught_by_item += 1;
				if (!bymachine) Game.GameData.stats.moves_taught_by_tutor += 1;
				if (oneusemachine) pokemon.add_first_move(move);
				ret = true;
				break;
			}
		}
		screen.EndScene;
	}
	return ret;   // Returns whether the move was learned by a Pokemon
}

public void ConvertItemToItem(variable, array) {
	item = GameData.Item.get(Get(variable));
	Set(variable, null);
	for (int i = (array.length / 2); i < (array.length / 2); i++) { //for '(array.length / 2)' times do => |i|
		if (item != array[2 * i]) continue;
		Set(variable, array[(2 * i) + 1]);
		return;
	}
}

public void ConvertItemToPokemon(variable, array) {
	item = GameData.Item.get(Get(variable));
	Set(variable, null);
	for (int i = (array.length / 2); i < (array.length / 2); i++) { //for '(array.length / 2)' times do => |i|
		if (item != array[2 * i]) continue;
		Set(variable, GameData.Species.get(array[(2 * i) + 1]).id);
		return;
	}
}

// Gets the value of a variable.
public void Get(id) {
	if (!id || !Game.GameData.game_variables) return 0;
	return Game.GameData.game_variables[id];
}

// Sets the value of a variable.
public void Set(id, value) {
	if (!id || id < 0) return;
	if (Game.GameData.game_variables) Game.GameData.game_variables[id] = value;
	if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
}

// Runs a common event and waits until the common event is finished.
// Requires the script "Messages"
public void CommonEvent(id) {
	if (id < 0) return false;
	ce = Game.GameData.data_common_events[id];
	if (!ce) return false;
	celist = ce.list;
	interp = new Interpreter();
	interp.setup(celist, 0);
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		interp.update;
		UpdateSceneMap;
		unless (interp.running()) break;
	}
	return true;
}

public void HideVisibleObjects() {
	visibleObjects = new List<string>();
	foreach (var o in ObjectSpace.each_object(Sprite)) { //ObjectSpace.each_object(Sprite) do => |o|
		if (!o.disposed() && o.visible) {
			visibleObjects.Add(o);
			o.visible = false;
		}
	}
	foreach (var o in ObjectSpace.each_object(Viewport)) { //ObjectSpace.each_object(Viewport) do => |o|
		if (!Disposed(o) && o.visible) {
			visibleObjects.Add(o);
			o.visible = false;
		}
	}
	foreach (var o in ObjectSpace.each_object(Plane)) { //ObjectSpace.each_object(Plane) do => |o|
		if (!o.disposed() && o.visible) {
			visibleObjects.Add(o);
			o.visible = false;
		}
	}
	foreach (var o in ObjectSpace.each_object(Tilemap)) { //ObjectSpace.each_object(Tilemap) do => |o|
		if (!o.disposed() && o.visible) {
			visibleObjects.Add(o);
			o.visible = false;
		}
	}
	foreach (var o in ObjectSpace.each_object(Window)) { //ObjectSpace.each_object(Window) do => |o|
		if (!o.disposed() && o.visible) {
			visibleObjects.Add(o);
			o.visible = false;
		}
	}
	return visibleObjects;
}

public void ShowObjects(visibleObjects) {
	foreach (var o in visibleObjects) { //'visibleObjects.each' do => |o|
		if (Disposed(o)) continue;
		o.visible = true;
	}
}

public void LoadRpgxpScene(scene) {
	if (!Game.GameData.scene.is_a(Scene_Map)) return;
	oldscene = Game.GameData.scene;
	Game.GameData.scene = scene;
	Graphics.freeze;
	oldscene.dispose;
	visibleObjects = HideVisibleObjects;
	Graphics.transition;
	Graphics.freeze;
	while (Game.GameData.scene && !Game.GameData.scene.is_a(Scene_Map)) {
		Game.GameData.scene.main;
	}
	Graphics.transition;
	Graphics.freeze;
	Game.GameData.scene = oldscene;
	Game.GameData.scene.createSpritesets;
	ShowObjects(visibleObjects);
	Graphics.transition;
}

public void ChooseLanguage() {
	commands = new List<string>();
	foreach (var lang in Settings.LANGUAGES) { //'Settings.LANGUAGES.each' do => |lang|
		commands.Add(lang[0]);
	}
	return ShowCommands(null, commands);
}

public void ScreenCapture() {
	t = Time.now;
	filestart = t.strftime("[%Y-%m-%d] %H_%M_%S.%L");
	begin;
		folder_name = "Screenshots";
		if (!Dir.safe(folder_name)) Dir.create(folder_name);
		capturefile = folder_name + "/" + string.Format("{0}.png", filestart);
		Graphics.screenshot(capturefile);
	rescue;
		capturefile = RTP.getSaveFileName(string.Format("{0}.png", filestart));
		Graphics.screenshot(capturefile);
	}
	if (FileTest.audio_exist("Audio/SE/Screenshot")) SEPlay("Screenshot");
}
