//===============================================================================
//
//===============================================================================
public partial class Scene_DebugIntro {
	public void main() {
		Graphics.transition(0);
		sscene = new PokemonLoad_Scene();
		sscreen = new PokemonLoadScreen(sscene);
		sscreen.StartLoadScreen;
		Graphics.freeze;
	}
}

//===============================================================================
//
//===============================================================================
public void CallTitle() {
	if (Core.DEBUG && Settings.SKIP_TITLE_SCREEN) return new Scene_DebugIntro();
	return new Scene_Intro();
}

public void mainFunction() {
	if (Core.DEBUG) {
		CriticalCode { mainFunctionDebug };
	} else {
		mainFunctionDebug;
	}
	return 1;
}

public void mainFunctionDebug() {
	begin;
		if (FileTest.exist("Data/messages_core.dat")) MessageTypes.load_default_messages;
		if (Core.DEBUG && !FileTest.exist("Game.rgssad") && Settings.PROMPT_TO_COMPILE) {
			SetResizeFactor(1);   // Needed to make the message look good
			if (ConfirmMessage("\\ts[]" + "Do you want to compile your data and plugins?")) {
				Game.GameData.full_compile = true;
			}
		}
		PluginManager.runPlugins;
		Compiler.main;
		Game.initialize;
		Game.set_up_system;
		Graphics.update;
		Graphics.freeze;
		Game.GameData.scene = CallTitle;
		Game.GameData.scene.main until Game.GameData.scene.null();
		Graphics.transition;
	rescue Hangup;
		if (!Core.DEBUG) PrintException($!);
		EmergencySave;
		Debug.LogError($"Exception Error Thrown on '{System.Reflection.MethodBase.GetCurrentMethod().Name}'");
		//throw new Exception();
	}
}

//===============================================================================
//
//===============================================================================
do { //loop; while (true);
	retval = mainFunction;
	switch (retval) {
		case 0:   // failed
			do { //loop; while (true);
				Graphics.update;
			}
			break;
		case 1:   // ended successfully
			break;
			break;
	}
}
