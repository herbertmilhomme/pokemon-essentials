//===============================================================================
// Mystery Gift system.
// By Maruno.
//-------------------------------------------------------------------------------
// This url is the location of an example Mystery Gift file.
// You should change it to your file's url once you upload it.
//===============================================================================
public static partial class MysteryGift {
	public const string URL = "https://pastebin.com/raw/w6BqqUsm";
}

//===============================================================================
// Creating a new Mystery Gift for the Master file, and editing an existing one.
//===============================================================================
// type: 0=Pokémon; 1 or higher=item (is the item's quantity).
// item: The thing being turned into a Mystery Gift (Pokémon object or item ID).
public void EditMysteryGift(type, item, id = 0, giftname = "") {
	begin;
		if (type == 0) {   // Pokémon
			commands = new {_INTL("Mystery Gift"),
									_INTL("Faraway place")};
			if (item.obtain_text && !item.obtain_text.empty()) commands.Add(item.obtain_text);
			commands.Add(_INTL("[Custom]"));
			do { //loop; while (true);
				command = Message(
					_INTL("Choose a phrase to be where the gift Pokémon was obtained from."),
					commands, -1
				);
				if (command < 0) {
					if (ConfirmMessage(_INTL("Stop editing this gift?"))) return null;
				} else if (command < commands.length - 1) {
					item.obtain_text = commands[command];
					break;
				} else if (command == commands.length - 1) {
					obtainname = MessageFreeText(_INTL("Enter a phrase."), "", false, 30);
					if (obtainname != "") {
						item.obtain_text = obtainname;
						break;
					}
					if (ConfirmMessage(_INTL("Stop editing this gift?"))) return null;
				}
			}
		} else if (type > 0) {   // Item
			params = new ChooseNumberParams();
			params.setRange(1, 99_999);
			params.setDefaultValue(type);
			params.setCancelValue(0);
			do { //loop; while (true);
				newtype = MessageChooseNumber(_INTL("Choose a quantity of {1}.",
																							GameData.Item.get(item).name), params);
				if (newtype == 0) {
					if (ConfirmMessage(_INTL("Stop editing this gift?"))) return null;
				} else {
					type = newtype;
					break;
				}
			}
		}
		if (id == 0) {
			master = new List<string>();
			idlist = new List<string>();
			if (FileTest.exist("MysteryGiftMaster.txt")) {
				master = IO.read("MysteryGiftMaster.txt");
				master = MysteryGiftDecrypt(master);
			}
			foreach (var i in master) { //'master.each' do => |i|
				idlist.Add(i[0]);
			}
			params = new ChooseNumberParams();
			params.setRange(0, 99_999);
			params.setDefaultValue(id);
			params.setCancelValue(0);
			do { //loop; while (true);
				newid = MessageChooseNumber(_INTL("Choose a unique ID for this gift."), params);
				if (newid == 0) {
					if (ConfirmMessage(_INTL("Stop editing this gift?"))) return null;
				} else if (idlist.Contains(newid)) {
					Message(_INTL("That ID is already used by a Mystery Gift."));
				} else {
					id = newid;
					break;
				}
			}
		}
		do { //loop; while (true);
			newgiftname = MessageFreeText(_INTL("Enter a name for the gift."), giftname, false, 250);
			if (newgiftname != "") {
				giftname = newgiftname;
				break;
			}
			if (ConfirmMessage(_INTL("Stop editing this gift?"))) return null;
		}
		return new {id, type, item, giftname};
	rescue;
		Message(_INTL("Couldn't edit the gift."));
		return null;
	}
}

public void CreateMysteryGift(type, item) {
	gift = EditMysteryGift(type, item);
	if (gift) {
		begin;
			if (FileTest.exist("MysteryGiftMaster.txt")) {
				master = IO.read("MysteryGiftMaster.txt");
				master = MysteryGiftDecrypt(master);
				master.Add(gift);
			} else {
				master = [gift];
			}
			string = MysteryGiftEncrypt(master);
			File.open("MysteryGiftMaster.txt", "wb", f => { f.write(string); });
			Message(_INTL("The gift was saved to MysteryGiftMaster.txt."));
		rescue;
			Message(_INTL("Couldn't save the gift to MysteryGiftMaster.txt."));
		}
	} else {
		Message(_INTL("Didn't create a gift."));
	}
}

//===============================================================================
// Debug option for managing gifts in the Master file and exporting them to a
// file to be uploaded.
//===============================================================================
public void ManageMysteryGifts() {
	if (!FileTest.exist("MysteryGiftMaster.txt")) {
		Message(_INTL("There are no Mystery Gifts defined."));
		return;
	}
	// Load all gifts from the Master file.
	master = IO.read("MysteryGiftMaster.txt");
	master = MysteryGiftDecrypt(master);
	if (!master || !master.Length > 0 || master.length == 0) {
		Message(_INTL("There are no Mystery Gifts defined."));
		return;
	}
	// Download all gifts from online
	msgwindow = CreateMessageWindow;
	MessageDisplay(msgwindow, _INTL("Searching for online gifts...\\wtnp[0]"));
	online = DownloadToString(MysteryGift.URL);
	DisposeMessageWindow(msgwindow);
	if (nil_or_empty(online)) {
		Message(_INTL("No online Mystery Gifts found.\\wtnp[20]"));
		online = new List<string>();
	} else {
		Message(_INTL("Online Mystery Gifts found.\\wtnp[20]"));
		online = MysteryGiftDecrypt(online);
		t = new List<string>();
		online.each(gift => t.Add(gift[0]));
		online = t;
	}
	// Show list of all gifts.
	command = 0;
	do { //loop; while (true);
		commands = RefreshMGCommands(master, online);
		command = Message("\\ts[]" + _INTL("Manage Mystery Gifts (X=online)."), commands, -1, null, command);
		// Gift chosen
		if (command == -1 || command == commands.length - 1) {   // Cancel
			break;
		} else if (command == commands.length - 2) {   // Export selected to file
			begin;
				newfile = new List<string>();
				foreach (var gift in master) { //'master.each' do => |gift|
					if (online.Contains(gift[0])) newfile.Add(gift);
				}
				string = MysteryGiftEncrypt(newfile);
				File.open("MysteryGift.txt", "wb", f => { f.write(string); });
				Message(_INTL("The gifts were saved to MysteryGift.txt."));
				Message(_INTL("Upload MysteryGift.txt to the Internet."));
			rescue;
				Message(_INTL("Couldn't save the gifts to MysteryGift.txt."));
			}
		} else if (command >= 0 && command < commands.length - 2) {   // A gift
			cmd = 0;
			do { //loop; while (true);
				commands = RefreshMGCommands(master, online);
				gift = master[command];
				cmds = new {_INTL("Toggle on/offline"),
								_INTL("Edit"),
								_INTL("Receive"),
								_INTL("Delete"),
								_INTL("Cancel")};
				cmd = Message("\\ts[]" + commands[command], cmds, -1, null, cmd);
				switch (cmd) {
					case -1: case cmds.length - 1:
						break;
						break;
					case 0:   // Toggle on/offline
						if (online.Contains(gift[0])) {
							online.delete(gift[0]);
						} else {
							online.Add(gift[0]);
						}
						break;
					case 1:   // Edit
						newgift = EditMysteryGift(gift[1], gift[2], gift[0], gift[3]);
						if (newgift) master[command] = newgift;
						break;
					case 2:   // Receive
						if (!Game.GameData.player) {
							Message(_INTL("There is no save file loaded. Cannot receive any gifts."));
							continue;
						}
						replaced = false;
						for (int i = Game.GameData.player.mystery_gifts.length; i < Game.GameData.player.mystery_gifts.length; i++) { //for 'Game.GameData.player.mystery_gifts.length' times do => |i|
							if (Game.GameData.player.mystery_gifts[i][0] == gift[0]) {
								Game.GameData.player.mystery_gifts[i] = gift;
								replaced = true;
							}
						}
						if (!replaced) Game.GameData.player.mystery_gifts.Add(gift);
						ReceiveMysteryGift(gift[0]);
						break;
					case 3:   // Delete
						if (ConfirmMessage(_INTL("Are you sure you want to delete this gift?"))) master.delete_at(command);
						break;
						break;
				}
			}
		}
	}
}

public void RefreshMGCommands(master, online) {
	commands = new List<string>();
	foreach (var gift in master) { //'master.each' do => |gift|
		itemname = "BLANK";
		if (gift[1] == 0) {
			itemname = gift[2].speciesName;
		} else if (gift[1] > 0) {
			itemname = GameData.Item.get(gift[2]).name + string.Format(" x{0}", gift[1]);
		}
		ontext = new {"[  ]", "[X]"}[(online.Contains(gift[0])) ? 1 : 0];
		commands.Add(_INTL("{1} {2}: {3} ({4})", ontext, gift[0], gift[3], itemname));
	}
	commands.Add(_INTL("Export selected to file"));
	commands.Add(_INTL("Cancel"));
	return commands;
}

//===============================================================================
// Downloads all available Mystery Gifts that haven't been downloaded yet.
//===============================================================================
// Called from the Continue/New Game screen.
public void DownloadMysteryGift(trainer) {
	sprites = new List<string>();
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	addBackgroundPlane(sprites, "background", "mysterygift_bg", viewport);
	FadeInAndShow(sprites);
	sprites["msgwindow"] = CreateMessageWindow;
	MessageDisplay(sprites["msgwindow"], _INTL("Searching for a gift.\nPlease wait...") + "\\wtnp[0]");
	string = DownloadToString(MysteryGift.URL);
	if (nil_or_empty(string)) {
		MessageDisplay(sprites["msgwindow"], _INTL("No new gifts are available."));
	} else {
		online = MysteryGiftDecrypt(string);
		pending = new List<string>();
		foreach (var gift in online) { //'online.each' do => |gift|
			notgot = true;
			foreach (var j in trainer.mystery_gifts) { //'trainer.mystery_gifts.each' do => |j|
				if (j[0] == gift[0]) notgot = false;
			}
			if (notgot) pending.Add(gift);
		}
		if (pending.length == 0) {
			MessageDisplay(sprites["msgwindow"], _INTL("No new gifts are available."));
		} else {
			do { //loop; while (true);
				commands = new List<string>();
				foreach (var gift in pending) { //'pending.each' do => |gift|
					commands.Add(gift[3]);
				}
				commands.Add(_INTL("Cancel"));
				MessageDisplay(sprites["msgwindow"], _INTL("Choose the gift you want to receive.") + "\\wtnp[0]");
				command = ShowCommands(sprites["msgwindow"], commands, -1);
				if (command == -1 || command == commands.length - 1) {
					break;
				} else {
					gift = pending[command];
					sprites["msgwindow"].visible = false;
					if (gift[1] == 0) {
						sprite = new PokemonSprite(viewport);
						sprite.setOffset(PictureOrigin.CENTER);
						sprite.setPokemonBitmap(gift[2]);
						sprite.x = Graphics.width / 2;
						sprite.y = -sprite.bitmap.height / 2;
					} else {
						sprite = new ItemIconSprite(0, 0, gift[2], viewport);
						sprite.x = Graphics.width / 2;
						sprite.y = -sprite.height / 2;
					}
					timer_start = System.uptime;
					start_y = sprite.y;
					do { //loop; while (true);
						sprite.y = lerp(start_y, Graphics.height / 2, 1.5, timer_start, System.uptime);
						Graphics.update;
						Input.update;
						sprite.update;
						if (sprite.y >= Graphics.height / 2) break;
					}
					MEPlay("Battle capture success");
					Wait(3.0) { sprite.update };
					sprites["msgwindow"].visible = true;
					MessageDisplay(sprites["msgwindow"], _INTL("The gift has been received!") + "\1") { sprite.update };
					MessageDisplay(sprites["msgwindow"], _INTL("Please pick up your gift from the deliveryman in any Poké Mart.")) { sprite.update };
					trainer.mystery_gifts.Add(gift);
					pending.delete_at(command);
					timer_start = System.uptime;
					do { //loop; while (true);
						sprite.opacity = lerp(255, 0, 1.5, timer_start, System.uptime);
						Graphics.update;
						Input.update;
						sprite.update;
						if (sprite.opacity <= 0) break;
					}
					sprite.dispose;
				}
				if (pending.length == 0) {
					MessageDisplay(sprites["msgwindow"], _INTL("No new gifts are available."));
					break;
				}
			}
		}
	}
	FadeOutAndHide(sprites);
	DisposeMessageWindow(sprites["msgwindow"]);
	DisposeSpriteHash(sprites);
	viewport.dispose;
}

//===============================================================================
// Converts an array of gifts into a string and back.
//===============================================================================
public void MysteryGiftEncrypt(gift) {
	ret = [Zlib.Deflate.deflate(Marshal.dump(gift))].pack("m");
	return ret;
}

public void MysteryGiftDecrypt(gift) {
	if (nil_or_empty(gift)) return [];
	ret = Marshal.restore(Zlib.Inflate.inflate(gift.unpack("m")[0]));
	if (ret) {
		foreach (var gft in ret) { //'ret.each' do => |gft|
			if (gft[1] == 0) {   // Pokémon
				gft[2] = gft[2];
			} else {   // Item
				gft[2] = GameData.Item.get(gft[2]).id;
			}
		}
	}
	return ret;
}

//===============================================================================
// Collecting a Mystery Gift from the deliveryman.
//===============================================================================
public void NextMysteryGiftID() {
	foreach (var i in Game.GameData.player.mystery_gifts) { //'Game.GameData.player.mystery_gifts.each' do => |i|
		if (i.length > 1) return i[0];
	}
	return 0;
}

public void ReceiveMysteryGift(id) {
	index = -1;
	for (int i = Game.GameData.player.mystery_gifts.length; i < Game.GameData.player.mystery_gifts.length; i++) { //for 'Game.GameData.player.mystery_gifts.length' times do => |i|
		if (Game.GameData.player.mystery_gifts[i][0] == id && Game.GameData.player.mystery_gifts[i].length > 1) {
			index = i;
			break;
		}
	}
	if (index == -1) {
		Message(_INTL("Couldn't find an unclaimed Mystery Gift with ID {1}.", id));
		return false;
	}
	gift = Game.GameData.player.mystery_gifts[index];
	if (gift[1] == 0) {   // Pokémon
		gift[2].personalID = rand(Math.Pow(2, 16)) | (rand(Math.Pow(2, 16)) << 16);
		gift[2].calc_stats;
		gift[2].timeReceived = Time.now.ToInt();
		gift[2].obtain_method = 4;   // Fateful encounter
		gift[2].record_first_moves;
		gift[2].obtain_level = gift[2].level;
		gift[2].obtain_map = Game.GameData.game_map&.map_id || 0;
		was_owned = Game.GameData.player.owned(gift[2].species);
		if (AddPokemonSilent(gift[2])) {
			Message(_INTL("{1} received {2}!", Game.GameData.player.name, gift[2].name) + "\\me[Pkmn get]\\wtnp[80]");
			Game.GameData.player.mystery_gifts[index] = [id];
			// Show Pokédex entry for new species if it hasn't been owned before
			if (Settings.SHOW_NEW_SPECIES_POKEDEX_ENTRY_MORE_OFTEN && !was_owned &&
				Game.GameData.player.has_pokedex && Game.GameData.player.pokedex.species_in_unlocked_dex(gift[2].species)) {
				Message(_INTL("{1}'s data was added to the Pokédex.", gift[2].name));
				Game.GameData.player.pokedex.register_last_seen(gift[2]);
				FadeOutIn do;
					scene = new PokemonPokedexInfo_Scene();
					screen = new PokemonPokedexInfoScreen(scene);
					screen.DexEntry(gift[2].species);
				}
			}
			return true;
		}
	} else if (gift[1] > 0) {   // Item
		item = gift[2];
		qty = gift[1];
		if (Game.GameData.bag.can_add(item, qty)) {
			Game.GameData.bag.add(item, qty);
			itm = GameData.Item.get(item);
			itemname = (qty > 1) ? itm.portion_name_plural : itm.portion_name;
			if (item == items.DNASPLICERS) {
				Message("\\me[Item get]" + _INTL("You obtained \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
			} else if (itm.is_machine()) {   // TM or HM
				if (qty > 1) {
					Message("\\me[Machine get]" + _INTL("You obtained {1} \\c[1]{2} {3}\\c[0]!",
																								qty, itemname, GameData.Move.get(itm.move).name) + "\\wtnp[70]");
				} else {
					Message("\\me[Machine get]" + _INTL("You obtained \\c[1]{1} {2}\\c[0]!", itemname,
																								GameData.Move.get(itm.move).name) + "\\wtnp[70]");
				}
			} else if (qty > 1) {
				Message("\\me[Item get]" + _INTL("You obtained {1} \\c[1]{2}\\c[0]!", qty, itemname) + "\\wtnp[40]");
			} else if (itemname.starts_with_vowel()) {
				Message("\\me[Item get]" + _INTL("You obtained an \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
			} else {
				Message("\\me[Item get]" + _INTL("You obtained a \\c[1]{1}\\c[0]!", itemname) + "\\wtnp[40]");
			}
			Game.GameData.player.mystery_gifts[index] = [id];
			return true;
		}
	}
	return false;
}
