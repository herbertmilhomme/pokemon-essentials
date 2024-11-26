//===============================================================================
//
//===============================================================================
public void GetLegalMoves(species) {
	species_data = GameData.Species.get(species);
	moves = new List<string>();
	if (!species_data) return moves;
	species_data.moves.each(m => moves.Add(m[1]));
	species_data.tutor_moves.each(m => moves.Add(m));
	babyspecies = species_data.get_baby_species;
	GameData.Species.get(babyspecies).egg_moves.each(m => moves.Add(m));
	moves |= new List<string>();   // Remove duplicates
	return moves;
}

public void SafeCopyFile(x, y, z = null) {
	if (FileTest.exist(x)) {
		safetocopy = true;
		filedata = null;
		if (FileTest.exist(y)) {
			different = false;
			if (FileTest.size(x) == FileTest.size(y)) {
				filedata2 = "";
				File.open(x, "rb", f => { filedata  = f.read; });
				File.open(y, "rb", f => { filedata2 = f.read; });
				if (filedata != filedata2) different = true;
			} else {
				different = true;
			}
			if (different) {
				safetocopy = ConfirmMessage(_INTL("A different file named '{1}' already exists. Overwrite it?", y));
			} else {
				// No need to copy
				return;
			}
		}
		if (safetocopy) {
			if (!filedata) {
				File.open(x, "rb", f => { filedata = f.read; });
			}
			File.open((z) ? z : y, "wb", f => { f.write(filedata); });
		}
	}
}

public void AllocateAnimation(animations, name) {
	for (int i = 1; i < animations.length; i++) { //each 'animations.length' do => |i|
		anim = animations[i];
		if (!anim) return i;
//    if (name && name!="" && anim.name==name) {
//      // use animation with same name
//      return i
//    }
		if (anim.length == 1 && anim[0].length == 2 && anim.name == "") {
			// assume empty
			return i;
		}
	}
	oldlength = animations.length;
	animations.resize(10);
	return oldlength;
}

public void MapTree() {
	mapinfos = LoadMapInfos;
	maplevels = new List<string>();
	retarray = new List<string>();
	foreach (var i in mapinfos) { //mapinfos.each_key do => |i|
		info = mapinfos[i];
		level = -1;
		while (info) {
			info = mapinfos[info.parent_id];
			level += 1;
		}
		if (level >= 0) {
			info = mapinfos[i];
			maplevels.Add(new {i, level, info.parent_id, info.order});
		}
	}
	maplevels.sort! do |a, b|
		if (a[1] != b[1]) next a[1] <=> b[1]; // level
		if (a[2] != b[2]) next a[2] <=> b[2]; // parent ID
		next a[3] <=> b[3]; // order
	}
	stack = new List<string>();
	stack.Add(0, 0);
	while (stack.length > 0) {
		parent = stack[stack.length - 1];
		index = stack[stack.length - 2];
		if (index >= maplevels.length) {
			stack.pop;
			stack.pop;
			continue;
		}
		maplevel = maplevels[index];
		stack[stack.length - 2] += 1;
		if (maplevel[2] != parent) {
			stack.pop;
			stack.pop;
			continue;
		}
		retarray.Add(new {maplevel[0], mapinfos[maplevel[0]].name, maplevel[1]});
		for (int i = index + 1; i < maplevels.length; i++) { //each 'maplevels.length' do => |i|
			if (maplevels[i][2] != maplevel[0]) continue;
			stack.Add(i);
			stack.Add(maplevel[0]);
			break;
		}
	}
	return retarray;
}

//===============================================================================
// List all members of a class.
//===============================================================================
public void ChooseFromGameDataList(game_data, default = null) {
	if (!GameData.const_defined(game_data.to_sym)) {
		Debug.LogError(_INTL("Couldn't find class {1} in module GameData.", game_data.ToString()));
		//throw new ArgumentException(_INTL("Couldn't find class {1} in module GameData.", game_data.ToString()));
	}
	game_data_module = GameData.const_get(game_data.to_sym);
	commands = new List<string>();
	foreach (var data in game_data_module) { //'game_data_module.each' do => |data|
		name = data.real_name;
		if (block_given()) name = yield(data);
		if (!name) continue;
		commands.Add(new {commands.length + 1, name, data.id});
	}
	return ChooseList(commands, default, null, -1);
}

// Displays a list of all Pokémon species, and returns the ID of the species
// selected (or null if the selection was canceled). "default", if specified, is
// the ID of the species to initially select. Pressing Input.ACTION will toggle
// the list sorting between numerical and alphabetical.
public void ChooseSpeciesList(default = null) {
	return ChooseFromGameDataList(:Species, default) do |data|
		next (data.form > 0) ? null : data.real_name;
	}
}

public void ChooseSpeciesFormList(default = null) {
	return ChooseFromGameDataList(:Species, default) do |data|
		next (data.form > 0) ? string.Format("{0}_{0}", data.real_name, data.form) : data.real_name;
	}
}

// Displays a list of all types, and returns the ID of the type selected (or null
// if (the selection was canceled). "default", if specified, is the ID of the type) {
// to initially select. Pressing Input.ACTION will toggle the list sorting
// between numerical and alphabetical.
public void ChooseTypeList(default = null) {
	return ChooseFromGameDataList(:Type, default) do |data|
		next (data.pseudo_type) ? null : data.real_name;
	}
}

// Displays a list of all items, and returns the ID of the item selected (or null
// if (the selection was canceled). "default", if specified, is the ID of the item) {
// to initially select. Pressing Input.ACTION will toggle the list sorting
// between numerical and alphabetical.
public void ChooseItemList(default = null) {
	return ChooseFromGameDataList(:Item, default);
}

// Displays a list of all abilities, and returns the ID of the ability selected
// (or null if the selection was canceled). "default", if specified, is the ID of
// the ability to initially select. Pressing Input.ACTION will toggle the list
// sorting between numerical and alphabetical.
public void ChooseAbilityList(default = null) {
	return ChooseFromGameDataList(:Ability, default);
}

// Displays a list of all moves, and returns the ID of the move selected (or null
// if (the selection was canceled). "default", if specified, is the ID of the move) {
// to initially select. Pressing Input.ACTION will toggle the list sorting
// between numerical and alphabetical.
public void ChooseMoveList(default = null) {
	return ChooseFromGameDataList(:Move, default);
}

public void ChooseMoveListForSpecies(species, defaultMoveID = null) {
	cmdwin = ListWindow(new List<string>(), 200);
	commands = new List<string>();
	index = 1;
	// Get all legal moves
	legalMoves = GetLegalMoves(species);
	foreach (var move in legalMoves) { //'legalMoves.each' do => |move|
		move_data = GameData.Move.get(move);
		commands.Add(new {index, move_data.name, move_data.id});
		index += 1;
	}
	commands.sort! { |a, b| a[1] <=> b[1] };
	moveDefault = 0;
	if (defaultMoveID) {
		commands.each_with_index do |item, i|
			if (moveDefault == 0 && item[2] == defaultMoveID) moveDefault = i;
		}
	}
	// Get all moves
	commands2 = new List<string>();
	foreach (var move_data in GameData.Move) { //'GameData.Move.each' do => |move_data|
		commands2.Add(new {index, move_data.name, move_data.id});
		index += 1;
	}
	commands2.sort! { |a, b| a[1] <=> b[1] };
	if (defaultMoveID) {
		commands2.each_with_index do |item, i|
			if (moveDefault == 0 && item[2] == defaultMoveID) moveDefault = i;
		}
	}
	// Choose from all moves
	commands.concat(commands2);
	realcommands = new List<string>();
	commands.each(cmd => realcommands.Add(cmd[1]));
	ret = Commands2(cmdwin, realcommands, -1, moveDefault, true);
	cmdwin.dispose;
	return (ret >= 0) ? commands[ret][2] : null;
}

public void ChooseBallList(defaultMoveID = null) {
	cmdwin = ListWindow(new List<string>(), 200);
	commands = new List<string>();
	moveDefault = 0;
	foreach (var item_data in GameData.Item) { //'GameData.Item.each' do => |item_data|
		if (item_data.is_poke_ball()) commands.Add(new {item_data.id, item_data.name});
	}
	commands.sort! { |a, b| a[1] <=> b[1] };
	if (defaultMoveID) {
		commands.each_with_index do |cmd, i|
			if (cmd[0] != defaultMoveID) continue;
			moveDefault = i;
			break;
		}
	}
	realcommands = new List<string>();
	foreach (var i in commands) { //'commands.each' do => |i|
		realcommands.Add(i[1]);
	}
	ret = Commands2(cmdwin, realcommands, -1, moveDefault, true);
	cmdwin.dispose;
	return (ret >= 0) ? commands[ret][0] : defaultMoveID;
}

//===============================================================================
// General list methods.
//===============================================================================
public void Commands2(cmdwindow, commands, cmdIfCancel, defaultindex = -1, noresize = false) {
	cmdwindow.commands = commands;
	if (defaultindex >= 0) cmdwindow.index    = defaultindex;
	cmdwindow.x        = 0;
	cmdwindow.y        = 0;
	if (noresize) {
		cmdwindow.height = Graphics.height;
	} else {
		cmdwindow.width  = Graphics.width / 2;
	}
	if (cmdwindow.height > Graphics.height) cmdwindow.height   = Graphics.height;
	cmdwindow.z        = 99999;
	cmdwindow.visible  = true;
	cmdwindow.active   = true;
	command = 0;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		cmdwindow.update;
		if (Input.trigger(Input.BACK)) {
			if (cmdIfCancel > 0) {
				command = cmdIfCancel - 1;
				break;
			} else if (cmdIfCancel < 0) {
				command = cmdIfCancel;
				break;
			}
		} else if (Input.trigger(Input.USE)) {
			command = cmdwindow.index;
			break;
		}
	}
	ret = command;
	cmdwindow.active = false;
	return ret;
}

public void Commands3(cmdwindow, commands, cmdIfCancel, defaultindex = -1, noresize = false) {
	cmdwindow.commands = commands;
	if (defaultindex >= 0) cmdwindow.index    = defaultindex;
	cmdwindow.x        = 0;
	cmdwindow.y        = 0;
	if (noresize) {
		cmdwindow.height = Graphics.height;
	} else {
		cmdwindow.width  = Graphics.width / 2;
	}
	if (cmdwindow.height > Graphics.height) cmdwindow.height   = Graphics.height;
	cmdwindow.z        = 99999;
	cmdwindow.visible  = true;
	cmdwindow.active   = true;
	command = 0;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		cmdwindow.update;
		if (Input.trigger(Input.SPECIAL)) {
			command = new {5, cmdwindow.index};
			break;
		} else if (Input.press(Input.ACTION)) {
			if (Input.repeat(Input.UP)) {
				command = new {1, cmdwindow.index};
				break;
			} else if (Input.repeat(Input.DOWN)) {
				command = new {2, cmdwindow.index};
				break;
			} else if (Input.trigger(Input.LEFT)) {
				command = new {3, cmdwindow.index};
				break;
			} else if (Input.trigger(Input.RIGHT)) {
				command = new {4, cmdwindow.index};
				break;
			}
		} else if (Input.trigger(Input.BACK)) {
			if (cmdIfCancel > 0) {
				command = new {0, cmdIfCancel - 1};
				break;
			} else if (cmdIfCancel < 0) {
				command = new {0, cmdIfCancel};
				break;
			}
		} else if (Input.trigger(Input.USE)) {
			command = new {0, cmdwindow.index};
			break;
		}
	}
	ret = command;
	cmdwindow.active = false;
	return ret;
}

public void ChooseList(commands, default = 0, cancelValue = -1, sortType = 1) {
	cmdwin = ListWindow(new List<string>());
	itemID = default;
	itemIndex = 0;
	sortMode = (sortType >= 0) ? sortType : 0;   // 0=ID, 1=alphabetical
	sorting = true;
	do { //loop; while (true);
		if (sorting) {
			switch (sortMode) {
				case 0:
					commands.sort! { |a, b| a[0] <=> b[0] };
					break;
				case 1:
					commands.sort! { |a, b| a[1] <=> b[1] };
					break;
			}
			if (itemID.is_a(Symbol)) {
				commands.each_with_index((command, i) => { if (command[2] == itemID) itemIndex = i; });
			} else if (itemID && itemID > 0) {
				commands.each_with_index((command, i) => { if (command[0] == itemID) itemIndex = i; });
			}
			realcommands = new List<string>();
			foreach (var command in commands) { //'commands.each' do => |command|
				if (sortType <= 0) {
					realcommands.Add(string.Format("{0:3}: {0}", command[0], command[1]));
				} else {
					realcommands.Add(command[1]);
				}
			}
			sorting = false;
		}
		cmd = CommandsSortable(cmdwin, realcommands, -1, itemIndex, (sortType < 0));
		switch (cmd[0]) {
			case 0:   // Chose an option or cancelled
				itemID = (cmd[1] < 0) ? cancelValue : (commands[cmd[1]][2] || commands[cmd[1]][0]);
				//break //break loop
				break;
			case 1:   // Toggle sorting
				itemID = commands[cmd[1]][2] || commands[cmd[1]][0];
				sortMode = (sortMode + 1) % 2;
				sorting = true;
				break;
		}
	}
	cmdwin.dispose;
	return itemID;
}

public void CommandsSortable(cmdwindow, commands, cmdIfCancel, defaultindex = -1, sortable = false) {
	cmdwindow.commands = commands;
	if (defaultindex >= 0) cmdwindow.index    = defaultindex;
	cmdwindow.x        = 0;
	cmdwindow.y        = 0;
	if (cmdwindow.width < Graphics.width / 2) cmdwindow.width    = Graphics.width / 2;
	cmdwindow.height   = Graphics.height;
	cmdwindow.z        = 99999;
	cmdwindow.active   = true;
	command = 0;
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		cmdwindow.update;
		if (Input.trigger(Input.ACTION) && sortable) {
			command = new {1, cmdwindow.index};
			break;
		} else if (Input.trigger(Input.BACK)) {
			command = new {0, (cmdIfCancel > 0) ? cmdIfCancel - 1 : cmdIfCancel};
			break;
		} else if (Input.trigger(Input.USE)) {
			command = new {0, cmdwindow.index};
			break;
		}
	}
	ret = command;
	cmdwindow.active = false;
	return ret;
}
