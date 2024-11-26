//===============================================================================
//
//===============================================================================
public partial class ReadyMenuButton : Sprite {
	/// <summary>ID of button</summary>
	public int index		{ get { return _index; } }			protected int _index;
	public int selected		{ get { return _selected; } }			protected int _selected;
	public int side		{ get { return _side; } }			protected int _side;

	public override void initialize(index, command, selected, side, viewport = null) {
		base.initialize(viewport);
		@index = index;
		@command = command;   // Item/move ID, name, mode (T move/F item), pkmnIndex
		@selected = selected;
		@side = side;
		if (@command[2]) {
			@button = new AnimatedBitmap("Graphics/UI/Ready Menu/icon_movebutton");
		} else {
			@button = new AnimatedBitmap("Graphics/UI/Ready Menu/icon_itembutton");
		}
		@contents = new Bitmap(@button.width, @button.height / 2);
		self.bitmap = @contents;
		SetSystemFont(self.bitmap);
		if (@command[2]) {
			@icon = new PokemonIconSprite(Game.GameData.player.party[@command[3]], viewport);
			@icon.setOffset(PictureOrigin.CENTER);
		} else {
			@icon = new ItemIconSprite(0, 0, @command[0], viewport);
		}
		@icon.z = self.z + 1;
		refresh;
	}

	public override void dispose() {
		@button.dispose;
		@contents.dispose;
		@icon.dispose;
		base.dispose();
	}

	public override int visible { set {
		@icon.visible = val;
		base.visible(val);
		}
	}

	public int selected { set {
		oldsel = @selected;
		@selected = val;
		if (oldsel != val) refresh;
		}
	}

	public int side { set {
		oldsel = @side;
		@side = val;
		if (oldsel != val) refresh;
		}
	}

	public void refresh() {
		sel = (@selected == @index && (@side == 0) == @command[2]);
		self.y = ((Graphics.height - (@button.height / 2)) / 2) - ((@selected - @index) * ((@button.height / 2) + 4));
		if (@command[2]) {   // Pokémon
			self.x = (sel) ? 0 : -16;
			@icon.x = self.x + 52;
			@icon.y = self.y + 32;
		} else {   // Item
			self.x = (sel) ? Graphics.width - @button.width : Graphics.width + 16 - @button.width;
			@icon.x = self.x + 32;
			@icon.y = self.y + (@button.height / 4);
		}
		self.bitmap.clear;
		rect = new Rect(0, (sel) ? @button.height / 2 : 0, @button.width, @button.height / 2);
		self.bitmap.blt(0, 0, @button.bitmap, rect);
		textx = 164;
		if (!@command[2]) {
			textx = (GameData.Item.get(@command[0]).is_important()) ? 146 : 124;
		}
		textpos = new {
			new {@command[1], textx, 24, :center, new Color(248, 248, 248), new Color(40, 40, 40), :outline};
		}
		if (!@command[2] && !GameData.Item.get(@command[0]).is_important()) {
			qty = Game.GameData.bag.quantity(@command[0]);
			if (qty > 99) {
				textpos.Add(new {_INTL(">99"), 230, 24, :right,
											new Color(248, 248, 248), new Color(40, 40, 40), :outline});
			} else {
				textpos.Add(new {_INTL("x{1}", qty), 230, 24, :right,
											new Color(248, 248, 248), new Color(40, 40, 40), :outline});
			}
		}
		DrawTextPositions(self.bitmap, textpos);
	}

	public override void update() {
		@icon&.update;
		base.update();
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonReadyMenu_Scene {
	public int sprites		{ get { return _sprites; } }			protected int _sprites;

	public void StartScene(commands) {
		@commands = commands;
		@movecommands = new List<string>();
		@itemcommands = new List<string>();
		for (int i = @commands[0].length; i < @commands[0].length; i++) { //for '@commands[0].length' times do => |i|
			@movecommands.Add(@commands[0][i][1]);
		}
		for (int i = @commands[1].length; i < @commands[1].length; i++) { //for '@commands[1].length' times do => |i|
			@itemcommands.Add(@commands[1][i][1]);
		}
		@index = Game.GameData.bag.ready_menu_selection;
		if (@index[0] >= @movecommands.length && @movecommands.length > 0) {
			@index[0] = @movecommands.length - 1;
		}
		if (@index[1] >= @itemcommands.length && @itemcommands.length > 0) {
			@index[1] = @itemcommands.length - 1;
		}
		if (@index[2] == 0 && @movecommands.length == 0) {
			@index[2] = 1;
		} else if (@index[2] == 1 && @itemcommands.length == 0) {
			@index[2] = 0;
		}
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["cmdwindow"] = new Window_CommandPokemon((@index[2] == 0) ? @movecommands : @itemcommands);
		@sprites["cmdwindow"].height = 192;
		@sprites["cmdwindow"].visible = false;
		@sprites["cmdwindow"].viewport = @viewport;
		for (int i = @commands[0].length; i < @commands[0].length; i++) { //for '@commands[0].length' times do => |i|
			@sprites[$"movebutton{i}"] = new ReadyMenuButton(i, @commands[0][i], @index[0], @index[2], @viewport);
		}
		for (int i = @commands[1].length; i < @commands[1].length; i++) { //for '@commands[1].length' times do => |i|
			@sprites[$"itembutton{i}"] = new ReadyMenuButton(i, @commands[1][i], @index[1], @index[2], @viewport);
		}
		SEPlay("GUI menu open");
	}

	public void ShowMenu() {
		@sprites["cmdwindow"].visible = false;
		for (int i = @commands[0].length; i < @commands[0].length; i++) { //for '@commands[0].length' times do => |i|
			@sprites[$"movebutton{i}"].visible = true;
		}
		for (int i = @commands[1].length; i < @commands[1].length; i++) { //for '@commands[1].length' times do => |i|
			@sprites[$"itembutton{i}"].visible = true;
		}
	}

	public void HideMenu() {
		@sprites["cmdwindow"].visible = false;
		for (int i = @commands[0].length; i < @commands[0].length; i++) { //for '@commands[0].length' times do => |i|
			@sprites[$"movebutton{i}"].visible = false;
		}
		for (int i = @commands[1].length; i < @commands[1].length; i++) { //for '@commands[1].length' times do => |i|
			@sprites[$"itembutton{i}"].visible = false;
		}
	}

	public void ShowCommands() {
		ret = -1;
		cmdwindow = @sprites["cmdwindow"];
		cmdwindow.commands = (@index[2] == 0) ? @movecommands : @itemcommands;
		cmdwindow.index    = @index[@index[2]];
		cmdwindow.visible  = false;
		do { //loop; while (true);
			Update;
			if (Input.trigger(Input.LEFT) && @index[2] == 1 && @movecommands.length > 0) {
				@index[2] = 0;
				ChangeSide;
			} else if (Input.trigger(Input.RIGHT) && @index[2] == 0 && @itemcommands.length > 0) {
				@index[2] = 1;
				ChangeSide;
			} else if (Input.trigger(Input.BACK)) {
				PlayCloseMenuSE;
				ret = -1;
				break;
			} else if (Input.trigger(Input.USE)) {
				ret = new {@index[2], cmdwindow.index};
				break;
			}
		}
		return ret;
	}

	public void EndScene() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void ChangeSide() {
		for (int i = @commands[0].length; i < @commands[0].length; i++) { //for '@commands[0].length' times do => |i|
			@sprites[$"movebutton{i}"].side = @index[2];
		}
		for (int i = @commands[1].length; i < @commands[1].length; i++) { //for '@commands[1].length' times do => |i|
			@sprites[$"itembutton{i}"].side = @index[2];
		}
		@sprites["cmdwindow"].commands = (@index[2] == 0) ? @movecommands : @itemcommands;
		@sprites["cmdwindow"].index = @index[@index[2]];
	}

	public void Refresh() { }

	public void Update() {
		oldindex = @index[@index[2]];
		@index[@index[2]] = @sprites["cmdwindow"].index;
		if (@index[@index[2]] != oldindex) {
			switch (@index[2]) {
				case 0:
					for (int i = @commands[0].length; i < @commands[0].length; i++) { //for '@commands[0].length' times do => |i|
						@sprites[$"movebutton{i}"].selected = @index[@index[2]];
					}
					break;
				case 1:
					for (int i = @commands[1].length; i < @commands[1].length; i++) { //for '@commands[1].length' times do => |i|
						@sprites[$"itembutton{i}"].selected = @index[@index[2]];
					}
					break;
			}
		}
		UpdateSpriteHash(@sprites);
		Graphics.update;
		Input.update;
		UpdateSceneMap;
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonReadyMenu {
	public void initialize(scene) {
		@scene = scene;
	}

	public void HideMenu() {
		@scene.HideMenu;
	}

	public void ShowMenu() {
		@scene.Refresh;
		@scene.ShowMenu;
	}

	public void StartReadyMenu(moves, items) {
		commands = new {[], new List<string>()};   // Moves, items
		foreach (var i in moves) { //'moves.each' do => |i|
			commands[0].Add(new {i[0], GameData.Move.get(i[0]).name, true, i[1]});
		}
		commands[0].sort! { |a, b| a[1] <=> b[1] };
		foreach (var i in items) { //'items.each' do => |i|
			commands[1].Add(new {i, GameData.Item.get(i).name, false});
		}
		commands[1].sort! { |a, b| a[1] <=> b[1] };
		@scene.StartScene(commands);
		do { //loop; while (true);
			command = @scene.ShowCommands;
			if (command == -1) break;
			if (command[0] == 0) {   // Use a move
				move = commands[0][command[1]][0];
				user = Game.GameData.player.party[commands[0][command[1]][3]];
				if (move == moves.FLY) {
					ret = null;
					FadeOutInWithUpdate(99999, @scene.sprites) do;
						HideMenu;
						scene = new PokemonRegionMap_Scene(-1, false);
						screen = new PokemonRegionMapScreen(scene);
						ret = screen.StartFlyScreen;
						if (!ret) ShowMenu;
					}
					if (ret) {
						Game.GameData.game_temp.fly_destination = ret;
						Game.GameData.game_temp.in_menu = false;
						UseHiddenMove(user, move);
						break;
					}
				} else {
					HideMenu;
					if (ConfirmUseHiddenMove(user, move)) {
						Game.GameData.game_temp.in_menu = false;
						UseHiddenMove(user, move);
						break;
					} else {
						ShowMenu;
					}
				}
			} else {   // Use an item
				item = commands[1][command[1]][0];
				HideMenu;
				if (ItemHandlers.triggerConfirmUseInField(item)) {
					Game.GameData.game_temp.in_menu = false;
					if (UseKeyItemInField(item)) break;
					Game.GameData.game_temp.in_menu = true;
				}
			}
			ShowMenu;
		}
		@scene.EndScene;
	}
}

//===============================================================================
// Using a registered item.
//===============================================================================
public void UseKeyItem() {
	moves = new {:CUT, :DEFOG, :DIG, :DIVE, :FLASH, :FLY, :HEADBUTT, :ROCKCLIMB,
					:ROCKSMASH, :SECRETPOWER, :STRENGTH, :SURF, :SWEETSCENT, :TELEPORT,
					:WATERFALL, :WHIRLPOOL};
	real_moves = new List<string>();
	foreach (var move in moves) { //'moves.each' do => |move|
		Game.GameData.player.party.each_with_index do |pkmn, i|
			if (pkmn.egg() || !pkmn.hasMove(move)) continue;
			if (CanUseHiddenMove(pkmn, move, false)) real_moves.Add(new {move, i});
		}
	}
	real_items = new List<string>();
	foreach (var i in Game.GameData.bag.registered_items) { //'Game.GameData.bag.registered_items.each' do => |i|
		itm = GameData.Item.get(i).id;
		if (Game.GameData.bag.has(itm)) real_items.Add(itm);
	}
	if (real_items.length == 0 && real_moves.length == 0) {
		Message(_INTL("An item in the Bag can be registered to this key for instant use."));
	} else {
		Game.GameData.game_temp.in_menu = true;
		Game.GameData.game_map.update;
		sscene = new PokemonReadyMenu_Scene();
		sscreen = new PokemonReadyMenu(sscene);
		sscreen.StartReadyMenu(real_moves, real_items);
		Game.GameData.game_temp.in_menu = false;
	}
}
