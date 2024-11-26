//===============================================================================
// Exceptions and critical code
//===============================================================================
public partial class Reset : Exception {
}

//===============================================================================
//
//===============================================================================
public partial class EventScriptError : Exception {
	public int event_message		{ get { return _event_message; } set { _event_message = value; } }			protected int _event_message;

	public override void initialize(message) {
		base.initialize(null);
		@event_message = message;
	}
}

//===============================================================================
//
//===============================================================================
public void GetExceptionMessage(e, _script = "") {
	if (e.is_a(EventScriptError)) return e.event_message.dup;   // Message with map/event ID generated elsewhere
	emessage = e.message.dup;
	emessage.force_encoding(Encoding.UTF_8);
	switch (e) {
		case Hangup:
			emessage = "The script is taking too long. The game will restart.";
			break;
		case Errno.ENOENT:
			filename = emessage.sub("No such file or directory - ", "");
			emessage = $"File {filename} not found.";
			break;
	}
	emessage = System.Text.RegularExpressions.Regex.Replace(emessage, "Section(\d+)", Game.GameData.RGSS_SCRIPTS[Game.GameData.1.ToInt()][1]); //rescue null
	return emessage;
}

public void PrintException(e) {
	emessage = GetExceptionMessage(e);
	// begin message formatting
	message = $"[Pokémon Essentials version {Essentials.VERSION}]\r\n";
	message += $"{Essentials.ERROR_TEXT}\r\n";   // For third party scripts to add to
	if (!e.is_a(EventScriptError)) {
		message += $"Exception: {e.class}\r\n";
		message += "Message: ";
	}
	message += emessage;
	// show last 10/25 lines of backtrace
	if (!e.is_a(EventScriptError)) {
		message += "\r\n\r\nBacktrace:\r\n";
		backtrace_text = "";
		if (e.backtrace) {
			maxlength = (Core.INTERNAL) ? 25 : 10;
			e.backtrace[0, maxlength].each { |i| backtrace_text += $"{i}\r\n" };
		}
		backtrace_text = System.Text.RegularExpressions.Regex.Replace(backtrace_text, "Section(\d+)", Game.GameData.RGSS_SCRIPTS[Game.GameData.1.ToInt()][1]); //rescue null
		message += backtrace_text;
	}
	// output to log
	errorlog = "errorlog.txt";
	File.open(errorlog, "ab") do |f|
		f.write($"\r\n=================\r\n\r\n[{Time.now}]\r\n");
		f.write(message);
	}
	// format/censor the error log directory
	errorlogline = errorlog.gsub("/", "\\");
	errorlogline.sub!(Dir.pwd + "\\", "");
	errorlogline.sub!(GetUserName, "USERNAME");
	if (errorlogline.length > 20) errorlogline = "\r\n" + errorlogline;
	// output message
	print($"{message}\r\nThis exception was logged in {errorlogline}.\r\nHold Ctrl when closing this message to copy it to the clipboard.");
	// Give a ~500ms coyote time to start holding Control
	t = System.uptime;
	until System.uptime - t >= 0.5;
		Input.update;
		if (Input.press(Input.CTRL)) {
			Input.clipboard = message;
			break;
		}
	}
}

public void CriticalCode() {
	ret = 0;
	begin;
		yield;
		ret = 1;
	rescue Exception;
		e = $!;
		if (e.is_a(Reset) || e.is_a(SystemExit)) {
			Debug.LogError($"Exception Error Thrown on '{System.Reflection.MethodBase.GetCurrentMethod().Name}'");
			//throw new Exception();
		} else {
			PrintException(e);
			if (e.is_a(Hangup)) {
				ret = 2;
				Debug.LogError(new Reset());
				//throw new Exception(new Reset());
			}
		}
	}
	return ret;
}
