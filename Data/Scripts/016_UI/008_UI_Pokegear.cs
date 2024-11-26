//===============================================================================
//
//===============================================================================
public partial class PokegearButton : Sprite {
	public int index		{ get { return _index; } }			protected int _index;
	public int name		{ get { return _name; } }			protected int _name;
	public int selected		{ get { return _selected; } }			protected int _selected;

	TEXT_BASE_COLOR = new Color(248, 248, 248);
	TEXT_SHADOW_COLOR = new Color(40, 40, 40);

	public override void initialize(command, x, y, viewport = null) {
		base.initialize(viewport);
		@image = command[0];
		@name  = command[1];
		@selected = false;
		if (Game.GameData.player.female() && ResolveBitmap("Graphics/UI/Pokegear/icon_button_f")) {
			@button = new AnimatedBitmap("Graphics/UI/Pokegear/icon_button_f");
		} else {
			@button = new AnimatedBitmap("Graphics/UI/Pokegear/icon_button");
		}
		@contents = new Bitmap(@button.width, @button.height);
		self.bitmap = @contents;
		self.x = x - (@button.width / 2);
		self.y = y;
		SetSystemFont(self.bitmap);
		refresh;
	}

	public override void dispose() {
		@button.dispose;
		@contents.dispose;
		base.dispose();
	}

	public int selected { set {
		oldsel = @selected;
		@selected = val;
		if (oldsel != val) refresh;
		}
	}

	public void refresh() {
		self.bitmap.clear;
		rect = new Rect(0, 0, @button.width, @button.height / 2);
		if (@selected) rect.y = @button.height / 2;
		self.bitmap.blt(0, 0, @button.bitmap, rect);
		textpos = new {
			new {@name, rect.width / 2, (rect.height / 2) - 10, :center, TEXT_BASE_COLOR, TEXT_SHADOW_COLOR};
		}
		DrawTextPositions(self.bitmap, textpos);
		imagepos = new {
			new {string.Format("Graphics/UI/Pokegear/icon_{0}", @image), 18, 10};
		}
		DrawImagePositions(self.bitmap, imagepos);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPokegear_Scene {
	public void Update() {
		for (int i = @commands.length; i < @commands.length; i++) { //for '@commands.length' times do => |i|
			@sprites[$"button{i}"].selected = (i == @index);
		}
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(commands) {
		@commands = commands;
		@index = 0;
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		if (Game.GameData.player.female() && ResolveBitmap("Graphics/UI/Pokegear/bg_f")) {
			@sprites["background"].setBitmap("Graphics/UI/Pokegear/bg_f");
		} else {
			@sprites["background"].setBitmap("Graphics/UI/Pokegear/bg");
		}
		for (int i = @commands.length; i < @commands.length; i++) { //for '@commands.length' times do => |i|
			@sprites[$"button{i}"] = new PokegearButton(@commands[i], Graphics.width / 2, 0, @viewport);
			button_height = @sprites[$"button{i}"].bitmap.height / 2;
			@sprites[$"button{i}"].y = ((Graphics.height - (@commands.length * button_height)) / 2) + (i * button_height);
		}
		FadeInAndShow(@sprites) { Update };
	}

	public void Scene() {
		ret = -1;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			Update;
			if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				break;
			} else if (Input.trigger(Input.USE)) {
				PlayDecisionSE;
				ret = @index;
				break;
			} else if (Input.trigger(Input.UP)) {
				if (@commands.length > 1) PlayCursorSE;
				@index -= 1;
				if (@index < 0) @index = @commands.length - 1;
			} else if (Input.trigger(Input.DOWN)) {
				if (@commands.length > 1) PlayCursorSE;
				@index += 1;
				if (@index >= @commands.length) @index = 0;
			}
		}
		return ret;
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		dispose;
	}

	public void dispose() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPokegearScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		// Get all commands
		command_list = new List<string>();
		commands = new List<string>();
		MenuHandlers.each_available(:pokegear_menu) do |option, hash, name|
			command_list.Add(new {hash["icon_name"] || "", name});
			commands.Add(hash);
		}
		@scene.StartScene(command_list);
		// Main loop
		end_scene = false;
		do { //loop; while (true);
			choice = @scene.Scene;
			if (choice < 0) {
				end_scene = true;
				break;
			}
			if (commands[choice]["effect"].call(@scene)) break;
		}
		if (end_scene) @scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================

MenuHandlers.add(:pokegear_menu, :map, {
	"name"      => _INTL("Map"),
	"icon_name" => "map",
	"order"     => 10,
	"effect"    => menu => {
		FadeOutIn do;
			scene = new PokemonRegionMap_Scene(-1, false);
			screen = new PokemonRegionMapScreen(scene);
			ret = screen.StartScreen;
			if (ret) {
				Game.GameData.game_temp.fly_destination = ret;
				menu.dispose;
				next 99999;
			}
		}
		next Game.GameData.game_temp.fly_destination;
	}
});

MenuHandlers.add(:pokegear_menu, :phone, {
	"name"      => _INTL("Phone"),
	"icon_name" => "phone",
	"order"     => 20,
//  "condition" => () => next Game.GameData.PokemonGlobal.phone && Game.GameData.PokemonGlobal.phone.contacts.length > 0,
	"effect"    => menu => {
		FadeOutIn do;
			scene = new PokemonPhone_Scene();
			screen = new PokemonPhoneScreen(scene);
			screen.StartScreen;
		}
		next false;
	}
});

MenuHandlers.add(:pokegear_menu, :jukebox, {
	"name"      => _INTL("Jukebox"),
	"icon_name" => "jukebox",
	"order"     => 30,
	"effect"    => menu => {
		FadeOutIn do;
			scene = new PokemonJukebox_Scene();
			screen = new PokemonJukeboxScreen(scene);
			screen.StartScreen;
		}
		next false;
	}
});
