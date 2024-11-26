//==============================================================================
// * Scene_Controls
//------------------------------------------------------------------------------
// Shows a help screen listing the keyboard controls.
// Display with:
//      EventScreen(ButtonEventScene)
//==============================================================================
public partial class ButtonEventScene : EventScene {
	public override void initialize(viewport = null) {
		base.initialize();
		Graphics.freeze;
		@current_screen = 1;
		addImage(0, 0, "Graphics/UI/Controls help/bg");
		@labels = new List<string>();
		@label_screens = new List<string>();
		@keys = new List<string>();
		@key_screens = new List<string>();

		addImageForScreen(1, 44, 122, _INTL("Graphics/UI/Controls help/help_f1"));
		addImageForScreen(1, 44, 252, _INTL("Graphics/UI/Controls help/help_f8"));
		addLabelForScreen(1, 134, 84, 352, _INTL("Opens the Key Bindings window, where you can choose which keyboard keys to use for each control."));
		addLabelForScreen(1, 134, 244, 352, _INTL("Take a screenshot. It is put in the same folder as the save file."));

		addImageForScreen(2, 16, 158, _INTL("Graphics/UI/Controls help/help_arrows"));
		addLabelForScreen(2, 134, 100, 352, _INTL("Use the Arrow keys to move the main character.\n\nYou can also use the Arrow keys to select entries and navigate menus."));

		addImageForScreen(3, 16, 90, _INTL("Graphics/UI/Controls help/help_usekey"));
		addImageForScreen(3, 16, 236, _INTL("Graphics/UI/Controls help/help_backkey"));
		addLabelForScreen(3, 134, 68, 352, _INTL("Used to confirm a choice, interact with people and things, and move through text. (Default: C)"));
		addLabelForScreen(3, 134, 196, 352, _INTL("Used to exit, cancel a choice, and cancel a mode. While moving around, hold to move at a different speed. (Default: X)"));

		addImageForScreen(4, 16, 90, _INTL("Graphics/UI/Controls help/help_actionkey"));
		addImageForScreen(4, 16, 236, _INTL("Graphics/UI/Controls help/help_specialkey"));
		addLabelForScreen(4, 134, 68, 352, _INTL("Used to open the Pause Menu. Also has various functions depending on context. (Default: Z)"));
		addLabelForScreen(4, 134, 196, 352, _INTL("Press to open the Ready Menu, where registered items and available field moves can be used. (Default: D)"));

		set_up_screen(@current_screen);
		Graphics.transition;
		// Go to next screen when user presses USE
		onCTrigger.set(method(:OnScreenEnd));
	}

	public void addLabelForScreen(number, x, y, width, text) {
		@labels.Add(addLabel(x, y, width, text));
		@label_screens.Add(number);
		@picturesprites[@picturesprites.length - 1].opacity = 0;
	}

	public void addImageForScreen(number, x, y, filename) {
		@keys.Add(addImage(x, y, filename));
		@key_screens.Add(number);
		@picturesprites[@picturesprites.length - 1].opacity = 0;
	}

	public void set_up_screen(number) {
		@label_screens.each_with_index do |screen, i|
			@labels[i].moveOpacity((screen == number) ? 10 : 0, 10, (screen == number) ? 255 : 0);
		}
		@key_screens.each_with_index do |screen, i|
			@keys[i].moveOpacity((screen == number) ? 10 : 0, 10, (screen == number) ? 255 : 0);
		}
		pictureWait;   // Update event scene with the changes
	}

	public void OnScreenEnd(scene, *args) {
		last_screen = (int)Math.Max(@label_screens.max, @key_screens.max);
		if (@current_screen >= last_screen) {
			// End scene
			Game.GameData.game_temp.background_bitmap = Graphics.snap_to_bitmap;
			Graphics.freeze;
			@viewport.color = Color.black;   // Ensure screen is black
			Graphics.transition(8, "fadetoblack");
			Game.GameData.game_temp.background_bitmap.dispose;
			scene.dispose;
		} else {
			// Next screen
			@current_screen += 1;
			onCTrigger.clear;
			set_up_screen(@current_screen);
			onCTrigger.set(method(:OnScreenEnd));
		}
	}
}
