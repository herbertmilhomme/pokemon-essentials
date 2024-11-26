//===============================================================================
//
//===============================================================================
public void MapInterpreter() {
	return Game.GameData.game_system&.map_interpreter;
}

public bool MapInterpreterRunning() {
	interp = MapInterpreter;
	return interp&.running();
}

// Unused
public void RefreshSceneMap() {
	if (Game.GameData.scene.is_a(Scene_Map)) Game.GameData.scene.miniupdate;
}

public void UpdateSceneMap() {
	if (Game.GameData.scene.is_a(Scene_Map) && !IsFaded()) Game.GameData.scene.miniupdate;
}

//===============================================================================
//
//===============================================================================
public void EventCommentInput(*args) {
	parameters = new List<string>();
	list = args[0].list;   // List of commands for event or event page
	elements = args[1];    // Number of elements
	trigger = args[2];     // Trigger
	if (list.null()) return null;
	unless (list.Length > 0) return null;
	foreach (var item in list) { //'list.each' do => |item|
		if (!new []{108, 408}.Contains(item.code)) continue;
		if (item.parameters[0] != trigger) continue;
		start = list.index(item) + 1;
		finish = start + elements;
		for (int id = start; id < finish; id++) { //each 'finish' do => |id|
			if (list[id]) parameters.Add(list[id].parameters[0]);
		}
		return parameters;
	}
	return null;
}

public void CurrentEventCommentInput(elements, trigger) {
	if (!MapInterpreterRunning()) return null;
	event = MapInterpreter.get_self;
	if (!event) return null;
	return EventCommentInput(event, elements, trigger);
}

//===============================================================================
//
//===============================================================================
public partial class ChooseNumberParams {
	/// <summary>Set the full path for the message's window skin</summary>
	public int messageSkin		{ get { return _messageSkin; } }			protected int _messageSkin;
	public int skin		{ get { return _skin; } }			protected int _skin;

	public void initialize() {
		@maxDigits = 0;
		@minNumber = 0;
		@maxNumber = 0;
		@skin = null;
		@messageSkin = null;
		@negativesAllowed = false;
		@initialNumber = 0;
		@cancelNumber = null;
	}

	public void setMessageSkin(value) {
		@messageSkin = value;
	}

	public void setSkin(value) {
		@skin = value;
	}

	public void setNegativesAllowed(value) {
		@negativeAllowed = value;
	}

	public void negativesAllowed() {
		@negativeAllowed ? true : false;
	}

	public void setRange(minNumber, maxNumber) {
		if (minNumber > maxNumber) maxNumber = minNumber;
		@maxDigits = 0;
		@minNumber = minNumber;
		@maxNumber = maxNumber;
	}

	public void setDefaultValue(number) {
		@initialNumber = number;
		@cancelNumber = null;
	}

	public void setInitialValue(number) {
		@initialNumber = number;
	}

	public void setCancelValue(number) {
		@cancelNumber = number;
	}

	public void initialNumber() {
		return @initialNumber.clamp(self.minNumber, self.maxNumber);
	}

	public void cancelNumber() {
		return @cancelNumber || self.initialNumber;
	}

	public void minNumber() {
		ret = 0;
		if (@maxDigits > 0) {
			ret = -((10**@maxDigits) - 1);
		} else {
			ret = @minNumber;
		}
		if (!@negativeAllowed && ret < 0) ret = 0;
		return ret;
	}

	public void maxNumber() {
		ret = 0;
		if (@maxDigits > 0) {
			ret = ((10**@maxDigits) - 1);
		} else {
			ret = @maxNumber;
		}
		if (!@negativeAllowed && ret < 0) ret = 0;
		return ret;
	}

	public void setMaxDigits(value) {
		@maxDigits = (int)Math.Max(1, value);
	}

	public void maxDigits() {
		if (@maxDigits > 0) {
			return @maxDigits;
		} else {
			return (int)Math.Max(numDigits(self.minNumber), numDigits(self.maxNumber));
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public void numDigits(number) {
		ans = 1;
		number = number.abs;
		while (number >= 10) {
			ans += 1;
			number /= 10;
		}
		return ans;
	}
}

//===============================================================================
//
//===============================================================================
public void ChooseNumber(msgwindow, params) {
	if (!params) return 0;
	ret = 0;
	maximum = params.maxNumber;
	minimum = params.minNumber;
	defaultNumber = params.initialNumber;
	cancelNumber = params.cancelNumber;
	cmdwindow = new Window_InputNumberPokemon(params.maxDigits);
	cmdwindow.z = 99999;
	cmdwindow.visible = true;
	if (params.skin) cmdwindow.setSkin(params.skin);
	cmdwindow.sign = params.negativesAllowed; // must be set before number
	cmdwindow.number = defaultNumber;
	PositionNearMsgWindow(cmdwindow, msgwindow, :right);
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		UpdateSceneMap;
		cmdwindow.update;
		msgwindow&.update;
		if (block_given()) yield;
		if (Input.trigger(Input.USE)) {
			ret = cmdwindow.number;
			if (ret > maximum) {
				PlayBuzzerSE;
			} else if (ret < minimum) {
				PlayBuzzerSE;
			} else {
				PlayDecisionSE;
				break;
			}
		} else if (Input.trigger(Input.BACK)) {
			PlayCancelSE
			ret = cancelNumber;
			break;
		}
	}
	cmdwindow.dispose;
	Input.update;
	return ret;
}

//===============================================================================
//
//===============================================================================
public partial class FaceWindowVX : SpriteWindow_Base {
	public override void initialize(face) {
		base.initialize(0, 0, 128, 128);
		faceinfo = face.split(",");
		facefile = ResolveBitmap("Graphics/Faces/" + faceinfo[0]);
		if (!facefile) facefile = ResolveBitmap("Graphics/Pictures/" + faceinfo[0]);
		self.contents&.dispose;
		@faceIndex = faceinfo[1].ToInt();
		@facebitmaptmp = new AnimatedBitmap(facefile);
		@facebitmap = new Bitmap(96, 96);
		@facebitmap.blt(0, 0, @facebitmaptmp.bitmap,
										new Rect((@faceIndex % 4) * 96, (@faceIndex / 4) * 96, 96, 96));
		self.contents = @facebitmap;
	}

	public override void update() {
		base.update();
		if (@facebitmaptmp.totalFrames > 1) {
			@facebitmaptmp.update;
			@facebitmap.blt(0, 0, @facebitmaptmp.bitmap,
											new Rect((@faceIndex % 4) * 96, (@faceIndex / 4) * 96, 96, 96));
		}
	}

	public override void dispose() {
		@facebitmaptmp.dispose;
		@facebitmap&.dispose;
		base.dispose();
	}
}

//===============================================================================
//
//===============================================================================
public void GetBasicMapNameFromId(id) {
	begin;
		map = LoadMapInfos;
		if (!map) return "";
		return map[id].name;
	rescue;
		return "";
	}
}

public void GetMapNameFromId(id) {
	name = GameData.MapMetadata.try_get(id)&.name;
	if (nil_or_empty(name)) {
		name = GetBasicMapNameFromId(id);
		if (Game.GameData.player) name = System.Text.RegularExpressions.Regex.Replace(name, "\\PN", Game.GameData.player.name);
	}
	return name;
}

public void CsvField!(str() {
	ret = "";
	str = System.Text.RegularExpressions.Regex.Replace(str, "\A\s*", "");
	if (str[0, 1] == "\"") {
		str[0, 1] = "";
		escaped = false;
		fieldbytes = 0;
		foreach (var s in str.scan(/./)) { //str.scan(/./) do => |s|
			fieldbytes += s.length;
			if (s == "\"" && !escaped) break;
			if (s == "\\" && !escaped) {
				escaped = true;
			} else {
				ret += s;
				escaped = false;
			}
		}
		str[0, fieldbytes] = "";
		if (!System.Text.RegularExpressions.Regex.IsMatch(str,@"\A\s*,") && !System.Text.RegularExpressions.Regex.IsMatch(str,@"\A\s*$")) {
			Debug.LogError(_INTL("Invalid quoted field (in: {1})", ret));
			//throw new ArgumentException(_INTL("Invalid quoted field (in: {1})", ret));
		}
		str[0, str.length] = $~.post_match;
	} else {
		if (System.Text.RegularExpressions.Regex.IsMatch(str,@",")) {
			str[0, str.length] = $~.post_match;
			ret = $~.pre_match;
		} else {
			ret = str.clone;
			str[0, str.length] = "";
		}
		ret = System.Text.RegularExpressions.Regex.Replace(ret, "\s+$", "");
	}
	return ret;
}

public void CsvPosInt!(str() {
	ret = CsvField!(str);
	if (!System.Text.RegularExpressions.Regex.IsMatch(ret,@"\A\d+$")) {
		Debug.LogError(_INTL("Field {1} is not a positive integer", ret));
		//throw new ArgumentException(_INTL("Field {1} is not a positive integer", ret));
	}
	return ret.ToInt();
}

//===============================================================================
// Money and coins windows.
//===============================================================================
public void GetGoldString() {
	return _INTL("${1}", Game.GameData.player.money.to_s_formatted);
}

public void DisplayGoldWindow(msgwindow) {
	moneyString = GetGoldString;
	goldwindow = new Window_AdvancedTextPokemon(_INTL("Money:\n<ar>{1}</ar>", moneyString));
	goldwindow.setSkin("Graphics/Windowskins/goldskin");
	goldwindow.resizeToFit(goldwindow.text, Graphics.width);
	if (goldwindow.width <= 160) goldwindow.width = 160;
	if (msgwindow.y == 0) {
		goldwindow.y = Graphics.height - goldwindow.height;
	} else {
		goldwindow.y = 0;
	}
	goldwindow.viewport = msgwindow.viewport;
	goldwindow.z = msgwindow.z;
	return goldwindow;
}

public void DisplayCoinsWindow(msgwindow, goldwindow) {
	coinString = (Game.GameData.player) ? Game.GameData.player.coins.to_s_formatted : "0";
	coinwindow = new Window_AdvancedTextPokemon(_INTL("Coins:\n<ar>{1}</ar>", coinString));
	coinwindow.setSkin("Graphics/Windowskins/goldskin");
	coinwindow.resizeToFit(coinwindow.text, Graphics.width);
	if (coinwindow.width <= 160) coinwindow.width = 160;
	if (msgwindow.y == 0) {
		coinwindow.y = (goldwindow) ? goldwindow.y - coinwindow.height : Graphics.height - coinwindow.height;
	} else {
		coinwindow.y = (goldwindow) ? goldwindow.height : 0;
	}
	coinwindow.viewport = msgwindow.viewport;
	coinwindow.z = msgwindow.z;
	return coinwindow;
}

public void DisplayBattlePointsWindow(msgwindow) {
	pointsString = (Game.GameData.player) ? Game.GameData.player.battle_points.to_s_formatted : "0";
	pointswindow = new Window_AdvancedTextPokemon(_INTL("Battle Points:\n<ar>{1}</ar>", pointsString));
	pointswindow.setSkin("Graphics/Windowskins/goldskin");
	pointswindow.resizeToFit(pointswindow.text, Graphics.width);
	if (pointswindow.width <= 160) pointswindow.width = 160;
	if (msgwindow.y == 0) {
		pointswindow.y = Graphics.height - pointswindow.height;
	} else {
		pointswindow.y = 0;
	}
	pointswindow.viewport = msgwindow.viewport;
	pointswindow.z = msgwindow.z;
	return pointswindow;
}

//===============================================================================
//
//===============================================================================
public void CreateStatusWindow(viewport = null) {
	msgwindow = new Window_AdvancedTextPokemon("");
	if (viewport) {
		msgwindow.viewport = viewport;
	} else {
		msgwindow.z = 99999;
	}
	msgwindow.visible = false;
	msgwindow.letterbyletter = false;
	BottomLeftLines(msgwindow, 2);
	skinfile = MessageConfig.GetSpeechFrame;
	msgwindow.setSkin(skinfile);
	return msgwindow;
}

public void CreateMessageWindow(viewport = null, skin = null) {
	msgwindow = new Window_AdvancedTextPokemon("");
	if (viewport) {
		msgwindow.viewport = viewport;
	} else {
		msgwindow.z = 99999;
	}
	msgwindow.visible = true;
	msgwindow.letterbyletter = true;
	msgwindow.back_opacity = MessageConfig.WINDOW_OPACITY;
	BottomLeftLines(msgwindow, 2);
	if (Game.GameData.game_temp) Game.GameData.game_temp.message_window_showing = true;
	if (!skin) skin = MessageConfig.GetSpeechFrame;
	msgwindow.setSkin(skin);
	return msgwindow;
}

public void DisposeMessageWindow(msgwindow) {
	if (Game.GameData.game_temp) Game.GameData.game_temp.message_window_showing = false;
	msgwindow.dispose;
}

//===============================================================================
// Main message-displaying function.
//===============================================================================
public void MessageDisplay(msgwindow, message, letterbyletter = true, commandProc = null) {
	if (!msgwindow) return;
	oldletterbyletter = msgwindow.letterbyletter;
	msgwindow.letterbyletter = (letterbyletter) ? true : false;
	ret = null;
	commands = null;
	facewindow = null;
	goldwindow = null;
	coinwindow = null;
	battlepointswindow = null;
	cmdvariable = 0;
	cmdIfCancel = 0;
	msgwindow.waitcount = 0;
	autoresume = false;
	text = message.clone;
	linecount = (Graphics.height > 400) ? 3 : 2;
	//## Text replacement
	text = System.Text.RegularExpressions.Regex.Replace(text, "\sign\[([^\]]*)\]", match => {      // do => \sign[something] gets turned into
		return "\\op\\cl\\ts[]\\w[" + match.Value[0] + "]";    // \op\cl\ts[]\w[something]
	},RegexOptions.IgnoreCase);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\\\\", "\5");
	text = System.Text.RegularExpressions.Regex.Replace(text, "\\1", "\1");
	if (Game.GameData.game_actors) {
		text = System.Text.RegularExpressions.Regex.Replace(text, "\n\[([1-8])\]", match => { return Game.GameData.game_actors[match.Value[0].ToInt()].name; },RegexOptions.IgnoreCase);
	}
	if (Game.GameData.player) text = System.Text.RegularExpressions.Regex.Replace(text, "\pn",  Game.GameData.player.name,RegexOptions.IgnoreCase);
	if (Game.GameData.player) text = System.Text.RegularExpressions.Regex.Replace(text, "\pm",  _INTL("${1}", Game.GameData.player.money.to_s_formatted),RegexOptions.IgnoreCase);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\n",   "\n");
	text = System.Text.RegularExpressions.Regex.Replace(text, "\\[([0-9a-f]{8,8})\]", match => "<c2=" + match.Value[0] + ">",RegexOptions.IgnoreCase);
	if (Game.GameData.player&.male()) text = System.Text.RegularExpressions.Regex.Replace(text, "\pg",  "\\b",RegexOptions.IgnoreCase);
	if (Game.GameData.player&.female()) text = System.Text.RegularExpressions.Regex.Replace(text, "\pg",  "\\r",RegexOptions.IgnoreCase);
	if (Game.GameData.player&.male()) text = System.Text.RegularExpressions.Regex.Replace(text, "\pog", "\\r",RegexOptions.IgnoreCase);
	if (Game.GameData.player&.female()) text = System.Text.RegularExpressions.Regex.Replace(text, "\pog", "\\b",RegexOptions.IgnoreCase);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\pg",  "",RegexOptions.IgnoreCase);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\pog", "",RegexOptions.IgnoreCase);
	male_text_tag = shadowc3tag(MessageConfig.MALE_TEXT_MAIN_COLOR, MessageConfig.MALE_TEXT_SHADOW_COLOR);
	female_text_tag = shadowc3tag(MessageConfig.FEMALE_TEXT_MAIN_COLOR, MessageConfig.FEMALE_TEXT_SHADOW_COLOR);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\b",   male_text_tag,RegexOptions.IgnoreCase);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\r",   female_text_tag,RegexOptions.IgnoreCase);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\[Ww]\[([^\]]*)\]", match => {
		w = match.Value[0].ToString();
		if (w == "") {
			msgwindow.windowskin = null;
		} else {
			msgwindow.setSkin($"Graphics/Windowskins/{w}", false);
		}
		return "";
	});
	isDarkSkin = isDarkWindowskin(msgwindow.windowskin);
	text = System.Text.RegularExpressions.Regex.Replace(text, "\c\[([0-9]+)\]", match => { //do =>
		main_color, shadow_color = get_text_colors_for_windowskin(msgwindow.windowskin, match.Value[0].ToInt(), isDarkSkin);
		next shadowc3tag(main_color, shadow_color);
	},RegexOptions.IgnoreCase);
	do { //loop; while (true);
		last_text = text.clone;
		text = System.Text.RegularExpressions.Regex.Replace(text, "\v\[([0-9]+)\]", match => { Game.GameData.game_variables[match.Value[0].ToInt()]; },RegexOptions.IgnoreCase);
		if (text == last_text) break;
	}
	do { //loop; while (true);
		last_text = text.clone;
		text = System.Text.RegularExpressions.Regex.Replace(text, "\l\[([0-9]+)\]", match => { //do =>
			linecount = (int)Math.Max(1, match.Value[0].ToInt());
			next "";
		},RegexOptions.IgnoreCase);
		if (text == last_text) break;
	}
	colortag = "";
	if (Game.GameData.game_system && Game.GameData.game_system.message_frame != 0) {
		main_color, shadow_color = get_text_colors_for_windowskin(msgwindow.windowskin, 0, true);
		colortag = shadowc3tag(main_color, shadow_color);
	} else {
		main_color, shadow_color = get_text_colors_for_windowskin(msgwindow.windowskin, 0, isDarkSkin);
		colortag = shadowc3tag(main_color, shadow_color);
	}
	text = colortag + text;
	//## Controls
	textchunks = new List<string>();
	controls = new List<string>();
	while (System.Text.RegularExpressions.Regex.IsMatch(text,"(?:\\(f|ff|ts|cl|me|se|wt|wtnp|ch)\[([^\]]*)\]|\\(g|cn|pt|wd|wm|op|cl|wu|\.|\||\!|\^))",RegexOptions.IgnoreCase)) {
		textchunks.Add($~.pre_match);
		if ($~[1]) {
			controls.Add(new {$~[1].downcase, $~[2], -1});
		} else {
			controls.Add(new {$~[3].downcase, "", -1});
		}
		text = $~.post_match;
	}
	textchunks.Add(text);
	foreach (var chunk in textchunks) { //'textchunks.each' do => |chunk|
		chunk = System.Text.RegularExpressions.Regex.Replace(chunk, "\005", "\\");
	}
	textlen = 0;
	for (int i = controls.length; i < controls.length; i++) { //for 'controls.length' times do => |i|
		control = controls[i][0];
		switch (control) {
			case "wt": case "wtnp": case ".": case "|":
				textchunks[i] += "\2";
				break;
			case "!":
				textchunks[i] += "\1";
				break;
		}
		textlen += toUnformattedText(textchunks[i]).scan(/./m).length;
		controls[i][2] = textlen;
	}
	text = textchunks.join;
	appear_timer_start = null;
	appear_duration = 0.5;   // In seconds
	haveSpecialClose = false;
	specialCloseSE = "";
	startSE = null;
	for (int i = controls.length; i < controls.length; i++) { //for 'controls.length' times do => |i|
		control = controls[i][0];
		param = controls[i][1];
		switch (control) {
			case "op":
				appear_timer_start = System.uptime;
				break;
			case "cl":
				text = System.Text.RegularExpressions.Regex.Replace(text, "\001\z", "", count: 1);   // fix: '$' can match end of line as well
				haveSpecialClose = true;
				specialCloseSE = param;
				break;
			case "f":
				facewindow&.dispose;
				facewindow = new PictureWindow($"Graphics/Pictures/{param}");
				break;
			case "ff":
				facewindow&.dispose;
				facewindow = new FaceWindowVX(param);
				break;
			case "ch":
				cmds = param.clone;
				cmdvariable = CsvPosInt!(cmds);
				cmdIfCancel = CsvField!(cmds).ToInt();
				commands = new List<string>();
				while (cmds.length > 0) {
					commands.Add(CsvField!(cmds));
				}
				break;
			case "wtnp": case "^":
				text = System.Text.RegularExpressions.Regex.Replace(text, "\001\z", "", count: 1);   // fix: '$' can match end of line as well
				break;
			case "se":
				if (controls[i][2] == 0) {
					startSE = param;
					controls[i] = null;
				}
				break;
		}
	}
	if (startSE) {
		SEPlay(StringToAudioFile(startSE));
	} else if (!appear_timer_start && letterbyletter) {
		PlayDecisionSE;
	}
	// Position message window
	RepositionMessageWindow(msgwindow, linecount);
	if (facewindow) {
		PositionNearMsgWindow(facewindow, msgwindow, :left);
		facewindow.viewport = msgwindow.viewport;
		facewindow.z        = msgwindow.z;
	}
	atTop = (msgwindow.y == 0);
	// Show text
	msgwindow.text = text;
	do { //loop; while (true);
		if (appear_timer_start) {
			y_start = (atTop) ? -msgwindow.height : Graphics.height;
			y_end = (atTop) ? 0 : Graphics.height - msgwindow.height;
			msgwindow.y = lerp(y_start, y_end, appear_duration, appear_timer_start, System.uptime);
			if (msgwindow.y == y_end) appear_timer_start = null;
		}
		for (int i = controls.length; i < controls.length; i++) { //for 'controls.length' times do => |i|
			if (!controls[i]) continue;
			if (controls[i][2] > msgwindow.position || msgwindow.waitcount != 0) continue;
			control = controls[i][0];
			param = controls[i][1];
			switch (control) {
				case "f":
					facewindow&.dispose;
					facewindow = new PictureWindow($"Graphics/Pictures/{param}");
					PositionNearMsgWindow(facewindow, msgwindow, :left);
					facewindow.viewport = msgwindow.viewport;
					facewindow.z        = msgwindow.z;
					break;
				case "ff":
					facewindow&.dispose;
					facewindow = new FaceWindowVX(param);
					PositionNearMsgWindow(facewindow, msgwindow, :left);
					facewindow.viewport = msgwindow.viewport;
					facewindow.z        = msgwindow.z;
					break;
				case "g":      // Display gold window
					goldwindow&.dispose;
					goldwindow = DisplayGoldWindow(msgwindow);
					break;
				case "cn":     // Display coins window
					coinwindow&.dispose;
					coinwindow = DisplayCoinsWindow(msgwindow, goldwindow);
					break;
				case "pt":     // Display battle points window
					battlepointswindow&.dispose;
					battlepointswindow = DisplayBattlePointsWindow(msgwindow);
					break;
				case "wu":
					atTop = true;
					msgwindow.y = 0;
					PositionNearMsgWindow(facewindow, msgwindow, :left);
					if (appear_timer_start) {
						msgwindow.y = lerp(y_start, y_end, appear_duration, appear_timer_start, System.uptime);
					}
					break;
				case "wm":
					atTop = false;
					msgwindow.y = (Graphics.height - msgwindow.height) / 2;
					PositionNearMsgWindow(facewindow, msgwindow, :left);
					break;
				case "wd":
					atTop = false;
					msgwindow.y = Graphics.height - msgwindow.height;
					PositionNearMsgWindow(facewindow, msgwindow, :left);
					if (appear_timer_start) {
						msgwindow.y = lerp(y_start, y_end, appear_duration, appear_timer_start, System.uptime);
					}
					break;
				case "ts":     // Change text speed
					msgwindow.textspeed = (param == "") ? 0 : param.ToInt() / 80.0;
					break;
				case ".":      // Wait 0.25 seconds
					msgwindow.waitcount += 0.25;
					break;
				case "|":      // Wait 1 second
					msgwindow.waitcount += 1.0;
					break;
				case "wt":     // Wait X/20 seconds
					param = System.Text.RegularExpressions.Regex.Replace(param, "\A\s+", "", count: 1); param = System.Text.RegularExpressions.Regex.Replace(param, "\s+\z", "", count: 1);
					msgwindow.waitcount += param.ToInt() / 20.0;
					break;
				case "wtnp":   // Wait X/20 seconds, no pause
					param = System.Text.RegularExpressions.Regex.Replace(param, "\A\s+", "", count: 1); param = System.Text.RegularExpressions.Regex.Replace(param, "\s+\z", "", count: 1);
					msgwindow.waitcount = param.ToInt() / 20.0;
					autoresume = true;
					break;
				case "^":      // Wait, no pause
					autoresume = true;
					break;
				case "se":     // Play SE
					SEPlay(StringToAudioFile(param));
					break;
				case "me":     // Play ME
					MEPlay(StringToAudioFile(param));
					break;
			}
			controls[i] = null;
		}
		if (!letterbyletter) break;
		Graphics.update;
		Input.update;
		facewindow&.update;
		if (autoresume && msgwindow.waitcount == 0) {
			if (msgwindow.busy()) msgwindow.resume;
			if (!msgwindow.busy()) break;
		}
		if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
			if (msgwindow.busy()) {
				if (msgwindow.pausing()) PlayDecisionSE;
				msgwindow.resume;
			} else if (!appear_timer_start) {
				break;
			}
		}
		UpdateSceneMap;
		msgwindow.update;
		if (block_given()) yield;
		if ((!letterbyletter || commandProc || commands) && !msgwindow.busy()) break;
	}
	Input.update;   // Must call Input.update again to avoid extra triggers
	msgwindow.letterbyletter = oldletterbyletter;
	if (commands) {
		Game.GameData.game_variables[cmdvariable] = ShowCommands(msgwindow, commands, cmdIfCancel);
		if (Game.GameData.game_map) Game.GameData.game_map.need_refresh = true;
	}
	if (commandProc) ret = commandProc.call(msgwindow);
	goldwindow&.dispose;
	coinwindow&.dispose;
	battlepointswindow&.dispose;
	facewindow&.dispose;
	if (haveSpecialClose) {
		SEPlay(StringToAudioFile(specialCloseSE));
		atTop = (msgwindow.y == 0);
		y_start = (atTop) ? 0 : Graphics.height - msgwindow.height;
		y_end = (atTop) ? -msgwindow.height : Graphics.height;
		disappear_duration = 0.5;   // In seconds
		disappear_timer_start = System.uptime;
		do { //loop; while (true);
			msgwindow.y = lerp(y_start, y_end, disappear_duration, disappear_timer_start, System.uptime);
			Graphics.update;
			Input.update;
			UpdateSceneMap;
			msgwindow.update;
			if (msgwindow.y == y_end) break;
		}
	}
	return ret;
}

//===============================================================================
// Message-displaying functions.
//===============================================================================
public void Message(message, commands = null, cmdIfCancel = 0, skin = null, defaultCmd = 0, &block) {
	ret = 0;
	msgwindow = CreateMessageWindow(null, skin);
	if (commands) {
		ret = MessageDisplay(msgwindow, message, true,
													block: (msgwndw) => {
														next Kernel.ShowCommands(msgwndw, commands, cmdIfCancel, defaultCmd, &block);
													}, &block);
	} else {
		MessageDisplay(msgwindow, message, &block);
	}
	DisposeMessageWindow(msgwindow);
	Input.update;
	return ret;
}

public void ConfirmMessage(message, Action block = null) {
	return (Message(message, new {_INTL("Yes"), _INTL("No")}, 2, &block) == 0);
}

public void ConfirmMessageSerious(message, Action block = null) {
	return (Message(message, new {_INTL("No"), _INTL("Yes")}, 1, &block) == 1);
}

public void MessageChooseNumber(message, params, Action block = null) {
	msgwindow = CreateMessageWindow(null, params.messageSkin);
	ret = MessageDisplay(msgwindow, message, true,
												block: (msgwndw) => {
													next ChooseNumber(msgwndw, params, &block);
												}, &block);
	DisposeMessageWindow(msgwindow);
	return ret;
}

public void ShowCommands(msgwindow, commands = null, cmdIfCancel = 0, defaultCmd = 0) {
	if (!commands) return 0;
	cmdwindow = new Window_CommandPokemonEx(commands);
	cmdwindow.z = 99999;
	cmdwindow.visible = true;
	cmdwindow.resizeToFit(cmdwindow.commands);
	PositionNearMsgWindow(cmdwindow, msgwindow, :right);
	cmdwindow.index = defaultCmd;
	command = 0;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		cmdwindow.update;
		msgwindow&.update;
		if (block_given()) yield;
		if (Input.trigger(Input.BACK)) {
			if (cmdIfCancel > 0) {
				command = cmdIfCancel - 1;
				break;
			} else if (cmdIfCancel < 0) {
				command = cmdIfCancel;
				break;
			}
		}
		if (Input.trigger(Input.USE)) {
			command = cmdwindow.index;
			break;
		}
		UpdateSceneMap;
	}
	ret = command;
	cmdwindow.dispose;
	Input.update;
	return ret;
}

public void ShowCommandsWithHelp(msgwindow, commands, help, cmdIfCancel = 0, defaultCmd = 0) {
	msgwin = msgwindow;
	if (!msgwindow) msgwin = CreateMessageWindow(null);
	oldlbl = msgwin.letterbyletter;
	msgwin.letterbyletter = false;
	if (commands) {
		cmdwindow = new Window_CommandPokemonEx(commands);
		cmdwindow.z = 99999;
		cmdwindow.visible = true;
		cmdwindow.resizeToFit(cmdwindow.commands);
		if (cmdwindow.height > msgwin.y) cmdwindow.height = msgwin.y;
		cmdwindow.index = defaultCmd;
		command = 0;
		msgwin.text = help[cmdwindow.index];
		msgwin.width = msgwin.width;   // Necessary evil to make it use the proper margins
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			oldindex = cmdwindow.index;
			cmdwindow.update;
			if (oldindex != cmdwindow.index) msgwin.text = help[cmdwindow.index];
			msgwin.update;
			if (block_given()) yield;
			if (Input.trigger(Input.BACK)) {
				if (cmdIfCancel > 0) {
					command = cmdIfCancel - 1;
					break;
				} else if (cmdIfCancel < 0) {
					command = cmdIfCancel;
					break;
				}
			}
			if (Input.trigger(Input.USE)) {
				command = cmdwindow.index;
				break;
			}
			UpdateSceneMap;
		}
		ret = command;
		cmdwindow.dispose;
		Input.update;
	}
	msgwin.letterbyletter = oldlbl;
	if (!msgwindow) DisposeMessageWindow(msgwin);
	return ret;
}

// frames is the number of 1/20 seconds to wait for
public void MessageWaitForInput(msgwindow, frames, showPause = false) {
	if (!frames || frames <= 0) return;
	if (msgwindow && showPause) msgwindow.startPause;
	timer_start = System.uptime;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		msgwindow&.update;
		UpdateSceneMap;
		if (block_given()) yield;
		if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) break;
		if (System.uptime - timer_start >= frames / 20.0) break;
	}
	if (msgwindow && showPause) msgwindow.stopPause;
}

public void FreeText(msgwindow, currenttext, passwordbox, maxlength, width = 240) {
	window = new Window_TextEntry_Keyboard(currenttext, 0, 0, width, 64);
	ret = "";
	window.maxlength = maxlength;
	window.visible = true;
	window.z = 99999;
	PositionNearMsgWindow(window, msgwindow, :right);
	window.text = currenttext;
	if (passwordbox) window.passwordChar = "*";
	Input.text_input = true;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		if (Input.triggerex(:ESCAPE)) {
			ret = currenttext;
			break;
		} else if (Input.triggerex(:RETURN)) {
			ret = window.text;
			break;
		}
		window.update;
		msgwindow&.update;
		if (block_given()) yield;
	}
	Input.text_input = false;
	window.dispose;
	Input.update;
	return ret;
}

public void MessageFreeText(message, currenttext, passwordbox, maxlength, width = 240, Action block = null) {
	msgwindow = CreateMessageWindow;
	retval = MessageDisplay(msgwindow, message, true,
														block: (msgwndw) => {
															next FreeText(msgwndw, currenttext, passwordbox, maxlength, width, &block);
														}, &block);
	DisposeMessageWindow(msgwindow);
	return retval;
}
