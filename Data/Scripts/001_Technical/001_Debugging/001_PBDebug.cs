//===============================================================================
//
//===============================================================================
public static partial class Debug {
	@@log = new List<string>();

	public static void logonerr() {
		begin;
			yield;
		rescue;
			Debug.Log($"");
			Debug.Log($"**Exception: {$!.message}");
			backtrace = "";
			$!.backtrace.each(line => backtrace += line + "\r\n");
			Debug.Log(backtrace);
			Debug.Log($"");
			PrintException($!);   // if Core.INTERNAL
			Debug.flush;
		}
	}

	public static void flush() {
		if (Core.DEBUG && Core.INTERNAL && @@log.length > 0) {
			File.open("Data/debuglog.txt", "a+b", f => { f.write(@@log.join); });
		}
		@@log.clear;
	}

	public static void log(msg) {
		if (Core.DEBUG && Core.INTERNAL) {
			echoln msg.gsub("%", "%%");
			@@log.Add(msg + "\r\n");
			Debug.flush;   // if @@log.length > 1024
		}
	}

	public static void log_header(msg) {
		if (Core.DEBUG && Core.INTERNAL) {
			echoln Console.markup_style(msg.gsub("%", "%%"), text: :light_purple);
			@@log.Add(msg + "\r\n");
			Debug.flush;   // if @@log.length > 1024
		}
	}

	public static void log_message(msg) {
		if (Core.DEBUG && Core.INTERNAL) {
			msg = "\"" + msg + "\"";
			echoln Console.markup_style(msg.gsub("%", "%%"), text: :dark_gray);
			@@log.Add(msg + "\r\n");
			Debug.flush;   // if @@log.length > 1024
		}
	}

	public static void log_ai(msg) {
		if (Core.DEBUG && Core.INTERNAL) {
			msg = "[AI] " + msg;
			echoln msg.gsub("%", "%%");
			@@log.Add(msg + "\r\n");
			Debug.flush;   // if @@log.length > 1024
		}
	}

	public static void log_score_change(amt, msg) {
		if (amt == 0) return;
		if (Core.DEBUG && Core.INTERNAL) {
			sign = (amt > 0) ? "+" : "-";
			amt_text = string.Format("{0:3}", amt.abs);
			msg = $"     {sign}{amt_text}: {msg}";
			echoln msg.gsub("%", "%%");
			@@log.Add(msg + "\r\n");
			Debug.flush;   // if @@log.length > 1024
		}
	}

	public static void dump(msg) {
		if (Core.DEBUG && Core.INTERNAL) {
			File.open("Data/dumplog.txt", "a+b") { |f| f.write($"{msg}\r\n") };
		}
	}
}
