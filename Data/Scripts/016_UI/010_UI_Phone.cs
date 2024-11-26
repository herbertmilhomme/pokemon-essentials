//===============================================================================
// Phone list of contacts.
//===============================================================================
public partial class Window_PhoneList : Window_CommandPokemon {
	public int switching		{ get { return _switching; } set { _switching = value; } }			protected int _switching;

	public void drawCursor(index, rect) {
		if (self.index == index) {
			selarrow = new AnimatedBitmap("Graphics/UI/Phone/cursor");
			CopyBitmap(self.contents, selarrow.bitmap, rect.x, rect.y + 2);
		}
		return new Rect(rect.x + 28, rect.y + 8, rect.width - 16, rect.height);
	}

	public override void drawItem(index, count, rect) {
		if (index >= self.top_row + self.page_item_max) return;
		if (self.index == index && @switching) {
			rect = drawCursor(index, rect);
			DrawShadowText(self.contents, rect.x, rect.y + (self.contents.text_offset_y || 0),
											rect.width, rect.height, @commands[index], new Color(224, 0, 0), new Color(224, 144, 144));
		} else {
			base.drawItem();
		}
		drawCursor(index - 1, itemRect(index - 1));
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPhone_Scene {
	public void StartScene() {
		@sprites = new List<string>();
		// Create viewport
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		// Background
		addBackgroundPlane(@sprites, "bg", "Phone/bg", @viewport);
		// List of contacts
		@sprites["list"] = Window_PhoneList.newEmpty(152, 32, Graphics.width - 142, Graphics.height - 80, @viewport);
		@sprites["list"].windowskin = null;
		// Rematch readiness icons
		if (Phone.rematches_enabled) {
			for (int i = @sprites["list"].page_item_max; i < @sprites["list"].page_item_max; i++) { //for '@sprites["list"].page_item_max' times do => |i|
				@sprites[$"rematch_{i}"] = new IconSprite(468, 62 + (i * 32), @viewport);
			}
		}
		// Phone signal icon
		@sprites["signal"] = new IconSprite(Graphics.width - 32, 0, @viewport);
		if (Phone.Call.can_make()) {
			@sprites["signal"].setBitmap("Graphics/UI/Phone/icon_signal");
		} else {
			@sprites["signal"].setBitmap("Graphics/UI/Phone/icon_nosignal");
		}
		// Title text
		@sprites["header"] = Window_UnformattedTextPokemon.newWithSize(
			_INTL("Phone"), 2, -18, 128, 64, @viewport
		);
		@sprites["header"].baseColor   = new Color(248, 248, 248);
		@sprites["header"].shadowColor = Color.black;
		@sprites["header"].windowskin = null;
		// Info text about all contacts
		@sprites["info"] = Window_AdvancedTextPokemon.newWithSize("", -8, 224, 180, 160, @viewport);
		@sprites["info"].windowskin = null;
		// Portrait of contact
		@sprites["icon"] = new IconSprite(70, 102, @viewport);
		// Contact's location text
		@sprites["bottom"] = Window_AdvancedTextPokemon.newWithSize(
			"", 162, Graphics.height - 64, Graphics.width - 158, 64, @viewport
		);
		@sprites["bottom"].windowskin = null;
		// Start scene
		RefreshList;
		FadeInAndShow(@sprites) { Update };
	}

	public void RefreshList() {
		@contacts = new List<string>();
		foreach (var contact in Game.GameData.PokemonGlobal.phone.contacts) { //'Game.GameData.PokemonGlobal.phone.contacts.each' do => |contact|
			if (contact.visible()) @contacts.Add(contact);
		}
		// Create list of commands (display names of contacts) and count rematches
		commands = new List<string>();
		rematch_count = 0;
		@contacts.each do |contact|
			commands.Add(contact.display_name);
			if (contact.can_rematch()) rematch_count += 1;
		}
		// Set list's commands
		@sprites["list"].commands = commands;
		if (@sprites["list"].index >= commands.length) @sprites["list"].index = commands.length - 1;
		if (@sprites["list"].top_row > @sprites["list"].itemCount - @sprites["list"].page_item_max) {
			@sprites["list"].top_row = @sprites["list"].itemCount - @sprites["list"].page_item_max;
		}
		// Set info text
		infotext = _INTL("Registered") + "<br>";
		infotext += "<r>" + @sprites["list"].commands.length.ToString() + "<br>";
		infotext += _INTL("Waiting for a rematch") + "<r>" + rematch_count.ToString();
		@sprites["info"].text = infotext;
		RefreshScreen;
	}

	public void RefreshScreen() {
		@sprites["list"].refresh;
		// Redraw rematch readiness icons
		if (@sprites["rematch_0"]) {
			for (int i = @sprites["list"].page_item_max; i < @sprites["list"].page_item_max; i++) { //for '@sprites["list"].page_item_max' times do => |i|
				@sprites[$"rematch_{i}"].clearBitmaps;
				j = i + @sprites["list"].top_item;
				if (j < @contacts.length && @contacts[j].can_rematch()) {
					@sprites[$"rematch_{i}"].setBitmap("Graphics/UI/Phone/icon_rematch");
				}
			}
		}
		// Get the selected contact
		contact = @contacts[@sprites["list"].index];
		if (contact) {
			// Redraw contact's portrait
			if (contact.trainer()) {
				filename = GameData.TrainerType.charset_filename(contact.trainer_type);
			} else {
				filename = string.Format("Graphics/Characters/phone{0:3}", contact.common_event_id);
			}
			@sprites["icon"].setBitmap(filename);
			charwidth  = @sprites["icon"].bitmap.width;
			charheight = @sprites["icon"].bitmap.height;
			@sprites["icon"].x        = 86 - (charwidth / 8);
			@sprites["icon"].y        = 134 - (charheight / 8);
			@sprites["icon"].src_rect = new Rect(0, 0, charwidth / 4, charheight / 4);
			// Redraw contact's location text
			map_name = (contact.map_id > 0) ? GetMapNameFromId(contact.map_id) : "";
			@sprites["bottom"].text = "<ac>" + map_name;
		} else {
			@sprites["icon"].setBitmap(null);
			@sprites["bottom"].text = "";
		}
	}

	public void ChooseContact() {
		ActivateWindow(@sprites, "list") do;
			index = -1;
			switch_index = -1;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				UpdateSpriteHash(@sprites);
				// Cursor moved, update display
				if (@sprites["list"].index != index) {
					if (switch_index >= 0) {
						real_contacts = Game.GameData.PokemonGlobal.phone.contacts;
						real_contacts.insert(@sprites["list"].index, real_contacts.delete_at(index));
						RefreshList;
					} else {
						RefreshScreen;
					}
				}
				index = @sprites["list"].index;
				// Get inputs
				if (switch_index >= 0) {
					if (Input.trigger(Input.ACTION) ||
						Input.trigger(Input.USE)) {
						PlayDecisionSE;
						@sprites["list"].switching = false;
						switch_index = -1;
						RefreshScreen;
					} else if (Input.trigger(Input.BACK)) {
						PlayCancelSE
						real_contacts = Game.GameData.PokemonGlobal.phone.contacts;
						real_contacts.insert(switch_index, real_contacts.delete_at(@sprites["list"].index));
						@sprites["list"].index = switch_index;
						@sprites["list"].switching = false;
						switch_index = -1;
						RefreshList;
					}
				} else {
					if (Input.trigger(Input.ACTION)) {
						switch_index = @sprites["list"].index;
						@sprites["list"].switching = true;
						RefreshScreen;
					} else if (Input.trigger(Input.BACK)) {
						PlayCloseMenuSE;
						return null;
					} else if (Input.trigger(Input.USE)) {
						if (index >= 0) return @contacts[index];
					}
				}
			}
		}
	}

	public void EndScene() {
		FadeOutAndHide(@sprites) { Update };
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void Update() {
		UpdateSpriteHash(@sprites);
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonPhoneScreen {
	public void initialize(scene) {
		@scene = scene;
	}

	public void StartScreen() {
		if (Game.GameData.PokemonGlobal.phone.contacts.none(con => con.visible())) {
			Message(_INTL("There are no phone numbers stored."));
			return;
		}
		@scene.StartScene;
		do { //loop; while (true);
			contact = @scene.ChooseContact;
			if (!contact) break;
			commands = new List<string>();
			commands.Add(_INTL("Call"));
			if (contact.can_hide()) commands.Add(_INTL("Delete"));
			commands.Add(_INTL("Sort Contacts"));
			commands.Add(_INTL("Cancel"));
			cmd = ShowCommands(null, commands, -1);
			if (cmd >= 1 && !contact.can_hide()) cmd += 1;
			switch (cmd) {
				case 0:   // Call
					Phone.Call.make_outgoing(contact);
					break;
				case 1:   // Delete
					name = contact.display_name;
					if (ConfirmMessage(_INTL("Are you sure you want to delete {1} from your phone?", name))) {
						contact.visible = false;
						Game.GameData.PokemonGlobal.phone.sort_contacts;
						@scene.RefreshList;
						Message(_INTL("{1} was deleted from your phone contacts.", name));
						if (Game.GameData.PokemonGlobal.phone.contacts.none(con => con.visible())) {
							Message(_INTL("There are no phone numbers stored."));
							break;
						}
					}
					break;
				case 2:   // Sort Contacts
					switch (Message(_INTL("How do you want to sort the contacts?"),
												new {_INTL("By name"),
													_INTL("By Trainer type"),
													_INTL("Special contacts first"),
													_INTL("Cancel")}, -1, null, 0)) {
						case 0:   // By name
							Game.GameData.PokemonGlobal.phone.contacts.sort! { |a, b| a.name <=> b.name };
							Game.GameData.PokemonGlobal.phone.sort_contacts;
							@scene.RefreshList;
							break;
						case 1:   // By trainer type
							Game.GameData.PokemonGlobal.phone.contacts.sort! { |a, b| a.display_name <=> b.display_name };
							Game.GameData.PokemonGlobal.phone.sort_contacts;
							@scene.RefreshList;
							break;
						case 2:   // Special contacts first
							new_contacts = new List<string>();
							for (int i = 2; i < 2; i++) { //for '2' times do => |i|
								foreach (var con in Game.GameData.PokemonGlobal.phone.contacts) { //'Game.GameData.PokemonGlobal.phone.contacts.each' do => |con|
									if ((i == 0 && con.trainer()) || (i == 1 && !con.trainer())) continue;
									new_contacts.Add(con);
								}
							}
							Game.GameData.PokemonGlobal.phone.contacts = new_contacts;
							Game.GameData.PokemonGlobal.phone.sort_contacts;
							@scene.RefreshList;
							break;
					}
					break;
			}
		}
		@scene.EndScene;
	}
}
