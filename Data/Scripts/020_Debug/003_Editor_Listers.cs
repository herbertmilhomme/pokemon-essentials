//===============================================================================
// Core lister script.
//===============================================================================
public void ListWindow(cmds, width = Graphics.width / 2) {
	list = Window_CommandPokemon.newWithSize(cmds, 0, 0, width, Graphics.height);
	list.index     = 0;
	list.rowHeight = 24;
	SetSmallFont(list.contents);
	list.refresh;
	return list;
}

public void ListScreen(title, lister) {
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	list = ListWindow(new List<string>());
	list.viewport = viewport;
	list.z        = 2;
	title = Window_UnformattedTextPokemon.newWithSize(
		title, Graphics.width / 2, 0, Graphics.width / 2, 64, viewport
	);
	title.z = 2;
	lister.setViewport(viewport);
	selectedmap = -1;
	commands = lister.commands;
	selindex = lister.startIndex;
	if (commands.length == 0) {
		value = lister.value(-1);
		lister.dispose;
		title.dispose;
		list.dispose;
		viewport.dispose;
		return value;
	}
	list.commands = commands;
	list.index    = selindex;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		list.update;
		if (list.index != selectedmap) {
			lister.refresh(list.index);
			selectedmap = list.index;
		}
		if (Input.trigger(Input.BACK)) {
			selectedmap = -1;
			break;
		} else if (Input.trigger(Input.USE)) {
			break;
		}
	}
	value = lister.value(selectedmap);
	lister.dispose;
	title.dispose;
	list.dispose;
	viewport.dispose;
	Input.update;
	return value;
}

public void ListScreenBlock(title, lister) {
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	list = ListWindow(new List<string>(), Graphics.width / 2);
	list.viewport = viewport;
	list.z        = 2;
	title = Window_UnformattedTextPokemon.newWithSize(
		title, Graphics.width / 2, 0, Graphics.width / 2, 64, viewport
	);
	title.z = 2;
	lister.setViewport(viewport);
	selectedmap = -1;
	commands = lister.commands;
	selindex = lister.startIndex;
	if (commands.length == 0) {
		value = lister.value(-1);
		lister.dispose;
		title.dispose;
		list.dispose;
		viewport.dispose;
		return value;
	}
	list.commands = commands;
	list.index = selindex;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		list.update;
		if (list.index != selectedmap) {
			lister.refresh(list.index);
			selectedmap = list.index;
		}
		if (Input.trigger(Input.ACTION)) {
			yield(Input.ACTION, lister.value(selectedmap));
			list.commands = lister.commands;
			if (list.index == list.commands.length) list.index = list.commands.length;
			lister.refresh(list.index);
		} else if (Input.trigger(Input.BACK)) {
			break;
		} else if (Input.trigger(Input.USE)) {
			yield(Input.USE, lister.value(selectedmap));
			list.commands = lister.commands;
			if (list.index == list.commands.length) list.index = list.commands.length;
			lister.refresh(list.index);
		}
	}
	lister.dispose;
	title.dispose;
	list.dispose;
	viewport.dispose;
	Input.update;
}

//===============================================================================
//
//===============================================================================
public partial class GraphicsLister {
	public void initialize(folder, selection) {
		@sprite = new IconSprite(0, 0);
		@sprite.bitmap = null;
		@sprite.x      = Graphics.width * 3 / 4;
		@sprite.y      = ((Graphics.height - 64) / 2) + 64;
		@sprite.z      = 2;
		@folder = folder;
		@selection = selection;
		@commands = new List<string>();
		@index = 0;
	}

	public void dispose() {
		@sprite.bitmap&.dispose;
		@sprite.dispose;
	}

	public void setViewport(viewport) {
		@sprite.viewport = viewport;
	}

	public void startIndex() {
		return @index;
	}

	public void commands() {
		@commands.clear;
		Dir.chdir(@folder) do;
			Dir.glob("*.png", f => { @commands.Add(f); });
			Dir.glob("*.gif", f => { @commands.Add(f); });
//      Dir.glob("*.jpg", f => { @commands.Add(f); });
//      Dir.glob("*.jpeg", f => { @commands.Add(f); });
//      Dir.glob("*.bmp", f => { @commands.Add(f); });
		}
		@commands.sort!;
		for (int i = @commands.length; i < @commands.length; i++) { //for '@commands.length' times do => |i|
			if (@commands[i] == @selection) @index = i;
		}
		if (@commands.length == 0) Message(_INTL("There are no files."));
		return @commands;
	}

	public void value(index) {
		return (index < 0) ? "" : @commands[index];
	}

	public void refresh(index) {
		if (index < 0) return;
		@sprite.setBitmap(@folder + @commands[index]);
		sprite_width = @sprite.bitmap.width;
		sprite_height = @sprite.bitmap.height;
		@sprite.ox = sprite_width / 2;
		@sprite.oy = sprite_height / 2;
		scale_x = (Graphics.width / 2).to_f / sprite_width;
		scale_y = (Graphics.height - 64).to_f / sprite_height;
		if (scale_x < 1.0 || scale_y < 1.0) {
			min_scale = (int)Math.Min(scale_x, scale_y);
			@sprite.zoom_x = @sprite.zoom_y = min_scale;
		} else {
			@sprite.zoom_x = @sprite.zoom_y = 1.0;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class MusicFileLister {
	public void initialize(bgm, setting) {
		@oldbgm = getPlayingBGM;
		@commands = new List<string>();
		@bgm = bgm;
		@setting = setting;
		@index = 0;
	}

	public void dispose() {
		PlayBGM(@oldbgm);
	}

	public void setViewport(viewport) {}

	public void getPlayingBGM() {
		(Game.GameData.game_system) ? Game.GameData.game_system.getPlayingBGM : null
	}

	public void PlayBGM(bgm) {
		(bgm) ? BGMPlay(bgm) : BGMStop
	}

	public void startIndex() {
		return @index;
	}

	public void commands() {
		folder = (@bgm) ? "Audio/BGM/" : "Audio/ME/";
		@commands.clear;
		Dir.chdir(folder) do;
			Dir.glob("*.wav", f => { @commands.Add(f); });
			Dir.glob("*.ogg", f => { @commands.Add(f); });
			Dir.glob("*.mp3", f => { @commands.Add(f); });
			Dir.glob("*.midi", f => { @commands.Add(f); });
			Dir.glob("*.mid", f => { @commands.Add(f); });
			Dir.glob("*.wma", f => { @commands.Add(f); });
		}
		@commands.uniq!;
		@commands.sort! { |a, b| a.downcase <=> b.downcase };
		for (int i = @commands.length; i < @commands.length; i++) { //for '@commands.length' times do => |i|
			if (@commands[i] == @setting) @index = i;
		}
		if (@commands.length == 0) Message(_INTL("There are no files."));
		return @commands;
	}

	public void value(index) {
		return (index < 0) ? "" : @commands[index];
	}

	public void refresh(index) {
		if (index < 0) return;
		if (@bgm) {
			PlayBGM(@commands[index]);
		} else {
			PlayBGM("../../Audio/ME/" + @commands[index]);
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class MetadataLister {
	public void initialize(sel_player_id = -1, new_player = false) {
		@index = 0;
		@commands = new List<string>();
		@player_ids = new List<string>();
		foreach (var player in GameData.PlayerMetadata) { //'GameData.PlayerMetadata.each' do => |player|
			if (sel_player_id > 0 && player.id == sel_player_id) @index = @commands.length + 1;
			@player_ids.Add(player.id);
		}
		@new_player = new_player;
	}

	public void dispose() { }

	public void setViewport(viewport) {}

	public void startIndex() {
		return @index;
	}

	public void commands() {
		@commands.clear;
		@commands.Add(_INTL("[GLOBAL METADATA]"));
		@player_ids.each { |id| @commands.Add(_INTL("Player {1}", id)) };
		if (@new_player) @commands.Add(_INTL("[ADD NEW PLAYER]"));
		return @commands;
	}

	// Cancel: -1
	// New player: -2
	// Global metadata: 0
	// Player character: 1+ (the player ID itself)
	public void value(index) {
		if (index < 1) return index;
		if (@new_player && index == @commands.length - 1) return -2;
		return @player_ids[index - 1];
	}

	public void refresh(index) {}
}

//===============================================================================
//
//===============================================================================
public partial class MapLister {
	public void initialize(selmap, addGlobal = false) {
		@sprite = new Sprite();
		@sprite.bitmap = null;
		@sprite.x      = Graphics.width * 3 / 4;
		@sprite.y      = ((Graphics.height - 64) / 2) + 64;
		@sprite.z      = -2;
		@commands = new List<string>();
		@maps = MapTree;
		@addGlobalOffset = (addGlobal) ? 1 : 0;
		@index = 0;
		for (int i = @maps.length; i < @maps.length; i++) { //for '@maps.length' times do => |i|
			if (@maps[i][0] == selmap) @index = i + @addGlobalOffset;
		}
	}

	public void dispose() {
		@sprite.bitmap&.dispose;
		@sprite.dispose;
	}

	public void setViewport(viewport) {
		@sprite.viewport = viewport;
	}

	public void startIndex() {
		return @index;
	}

	public void commands() {
		@commands.clear;
		if (@addGlobalOffset == 1) @commands.Add(_INTL("[GLOBAL]"));
		for (int i = @maps.length; i < @maps.length; i++) { //for '@maps.length' times do => |i|
			@commands.Add(string.Format("{0}{0:3} {0}", ("  " * @maps[i][2]), @maps[i][0], @maps[i][1]));
		}
		return @commands;
	}

	public void value(index) {
		if (@addGlobalOffset == 1 && index == 0) return 0;
		return (index < 0) ? -1 : @maps[index - @addGlobalOffset][0];
	}

	public void refresh(index) {
		@sprite.bitmap&.dispose;
		if (index < 0) return;
		if (index == 0 && @addGlobalOffset == 1) return;
		@sprite.bitmap = createMinimap(@maps[index - @addGlobalOffset][0]);
		@sprite.ox = @sprite.bitmap.width / 2;
		@sprite.oy = @sprite.bitmap.height / 2;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SpeciesLister {
	public void initialize(selection = 0, includeNew = false) {
		@selection = selection;
		@commands = new List<string>();
		@ids = new List<string>();
		@includeNew = includeNew;
		@index = 0;
	}

	public void dispose() { }
	public void setViewport(viewport) {}

	public void startIndex() {
		return @index;
	}

	// Sorted alphabetically.
	public void commands() {
		@commands.clear;
		@ids.clear;
		cmds = new List<string>();
		idx = 1;
		foreach (var species in GameData.Species) { //GameData.Species.each_species do => |species|
			cmds.Add(new {idx, species.id, species.real_name});
			idx += 1;
		}
		cmds.sort! { |a, b| a[2].downcase <=> b[2].downcase };
		if (@includeNew) {
			@commands.Add(_INTL("[NEW SPECIES]"));
			@ids.Add(true);
		}
		foreach (var i in cmds) { //'cmds.each' do => |i|
			@commands.Add(string.Format("{0:3}: {0}", i[0], i[2]));
			@ids.Add(i[1]);
		}
		@index = @selection;
		if (@index >= @commands.length) @index = @commands.length - 1;
		if (@index < 0) @index = 0;
		return @commands;
	}

	public void value(index) {
		if (index < 0) return null;
		return @ids[index];
	}

	public void refresh(index) {}
}

//===============================================================================
//
//===============================================================================
public partial class ItemLister {
	public void initialize(selection = 0, includeNew = false) {
		@sprite = new ItemIconSprite(Graphics.width * 3 / 4, Graphics.height / 2, null);
		@sprite.z = 2;
		@selection = selection;
		@commands = new List<string>();
		@ids = new List<string>();
		@includeNew = includeNew;
		@index = 0;
	}

	public void dispose() {
		@sprite.bitmap&.dispose;
		@sprite.dispose;
	}

	public void setViewport(viewport) {
		@sprite.viewport = viewport;
	}

	public void startIndex() {
		return @index;
	}

	// Sorted alphabetically.
	public void commands() {
		@commands.clear;
		@ids.clear;
		cmds = new List<string>();
		idx = 1;
		foreach (var item in GameData.Item) { //'GameData.Item.each' do => |item|
			cmds.Add(new {idx, item.id, item.real_name});
			idx += 1;
		}
		cmds.sort! { |a, b| a[2].downcase <=> b[2].downcase };
		if (@includeNew) {
			@commands.Add(_INTL("[NEW ITEM]"));
			@ids.Add(true);
		}
		foreach (var i in cmds) { //'cmds.each' do => |i|
			@commands.Add(string.Format("{0:3}: {0}", i[0], i[2]));
			@ids.Add(i[1]);
		}
		@index = @selection;
		if (@index >= @commands.length) @index = @commands.length - 1;
		if (@index < 0) @index = 0;
		return @commands;
	}

	public void value(index) {
		if (index < 0) return null;
		return @ids[index];
	}

	public void refresh(index) {
		@sprite.item = (@ids[index].is_a(Symbol)) ? @ids[index] : null;
	}
}

//===============================================================================
//
//===============================================================================
public partial class TrainerTypeLister {
	public void initialize(selection = 0, includeNew = false) {
		@sprite = new IconSprite(Graphics.width * 3 / 4, ((Graphics.height - 64) / 2) + 64);
		@sprite.z = 2;
		@selection = selection;
		@commands = new List<string>();
		@ids = new List<string>();
		@includeNew = includeNew;
		@index = 0;
	}

	public void dispose() {
		@sprite.bitmap&.dispose;
		@sprite.dispose;
	}

	public void setViewport(viewport) {
		@sprite.viewport = viewport;
	}

	public void startIndex() {
		return @index;
	}

	public void commands() {
		@commands.clear;
		@ids.clear;
		cmds = new List<string>();
		idx = 1;
		foreach (var tr_type in GameData.TrainerType) { //'GameData.TrainerType.each' do => |tr_type|
			cmds.Add(new {idx, tr_type.id, tr_type.real_name});
			idx += 1;
		}
		cmds.sort! { |a, b| a[2] == b[2] ? a[0] <=> b[0] : a[2].downcase <=> b[2].downcase };
		if (@includeNew) {
			@commands.Add(_INTL("[NEW TRAINER TYPE]"));
			@ids.Add(true);
		}
		foreach (var t in cmds) { //'cmds.each' do => |t|
			@commands.Add(string.Format("{0:3}: {0}", t[0], t[2]));
			@ids.Add(t[1]);
		}
		@index = @selection;
		if (@index >= @commands.length) @index = @commands.length - 1;
		if (@index < 0) @index = 0;
		return @commands;
	}

	public void value(index) {
		if (index < 0) return null;
		return @ids[index];
	}

	public void refresh(index) {
		@sprite.bitmap&.dispose;
		if (index < 0) return;
		begin;
			if (@ids[index].is_a(Symbol)) {
				@sprite.setBitmap(GameData.TrainerType.front_sprite_filename(@ids[index]), 0);
			} else {
				@sprite.setBitmap(null);
			}
		rescue;
			@sprite.setBitmap(null);
		}
		if (@sprite.bitmap) {
			@sprite.ox = @sprite.bitmap.width / 2;
			@sprite.oy = @sprite.bitmap.height / 2;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class TrainerBattleLister {
	public void initialize(selection, includeNew) {
		@sprite = new IconSprite(Graphics.width * 3 / 4, (Graphics.height / 2) + 32);
		@sprite.z = 2;
		@pkmnList = Window_UnformattedTextPokemon.newWithSize(
			"", Graphics.width / 2, Graphics.height - 64, Graphics.width / 2, 64
		);
		@pkmnList.z = 3;
		@selection = selection;
		@commands = new List<string>();
		@ids = new List<string>();
		@includeNew = includeNew;
		@index = 0;
	}

	public void dispose() {
		@sprite.bitmap&.dispose;
		@sprite.dispose;
		@pkmnList.dispose;
	}

	public void setViewport(viewport) {
		@sprite.viewport   = viewport;
		@pkmnList.viewport = viewport;
	}

	public void startIndex() {
		return @index;
	}

	public void commands() {
		@commands.clear;
		@ids.clear;
		cmds = new List<string>();
		idx = 1;
		foreach (var trainer in GameData.Trainer) { //'GameData.Trainer.each' do => |trainer|
			cmds.Add(new {idx, trainer.trainer_type, trainer.real_name, trainer.version});
			idx += 1;
		}
		cmds.sort! do |a, b|
			if (a[1] == b[1]) {
				if (a[2] == b[2]) {
					(a[3] == b[3]) ? a[0] <=> b[0] : a[3] <=> b[3]
				} else {
					a[2].downcase <=> b[2].downcase;
				}
			} else {
				a[1].ToString().downcase <=> b[1].ToString().downcase;
			}
		}
		if (@includeNew) {
			@commands.Add(_INTL("[NEW TRAINER BATTLE]"));
			@ids.Add(true);
		}
		foreach (var t in cmds) { //'cmds.each' do => |t|
			if (t[3] > 0) {
				@commands.Add(_INTL("{1} {2} ({3}) x{4}",
														GameData.TrainerType.get(t[1]).name, t[2], t[3],
														GameData.Trainer.get(t[1], t[2], t[3]).pokemon.length));
			} else {
				@commands.Add(_INTL("{1} {2} x{3}",
														GameData.TrainerType.get(t[1]).name, t[2],
														GameData.Trainer.get(t[1], t[2], t[3]).pokemon.length));
			}
			@ids.Add(new {t[1], t[2], t[3]});
		}
		@index = @selection;
		if (@index >= @commands.length) @index = @commands.length - 1;
		if (@index < 0) @index = 0;
		return @commands;
	}

	public void value(index) {
		if (index < 0) return null;
		return @ids[index];
	}

	public void refresh(index) {
		// Refresh trainer sprite
		@sprite.bitmap&.dispose;
		if (index < 0) return;
		begin;
			if (@ids[index].Length > 0) {
				@sprite.setBitmap(GameData.TrainerType.front_sprite_filename(@ids[index][0]), 0);
			} else {
				@sprite.setBitmap(null);
			}
		rescue;
			@sprite.setBitmap(null);
		}
		if (@sprite.bitmap) {
			@sprite.ox = @sprite.bitmap.width / 2;
			@sprite.oy = @sprite.bitmap.height;
		}
		// Refresh list of Pokémon
		text = "";
		if (!@includeNew || index > 0) {
			tr_data = GameData.Trainer.get(@ids[index][0], @ids[index][1], @ids[index][2]);
			if (tr_data) {
				tr_data.pokemon.each_with_index do |pkmn, i|
					if (i > 0) text += "\n";
					text += string.Format("{0} Lv.{0}", GameData.Species.get(pkmn.species).real_name, pkmn.level);
				}
			}
		}
		@pkmnList.text = text;
		@pkmnList.resizeHeightToFit(text, Graphics.width / 2);
		@pkmnList.y = Graphics.height - @pkmnList.height;
	}
}
