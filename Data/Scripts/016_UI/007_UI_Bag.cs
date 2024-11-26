//===============================================================================
//
//===============================================================================
public partial class Window_PokemonBag : Window_DrawableCommand {
	public int pocket		{ get { return _pocket; } }			protected int _pocket;
	public int sorting		{ get { return _sorting; } set { _sorting = value; } }			protected int _sorting;

	public override void initialize(bag, filterlist, pocket, x, y, width, height) {
		@bag        = bag;
		@filterlist = filterlist;
		@pocket     = pocket;
		@sorting = false;
		@adapter = new PokemonMartAdapter();
		base.initialize(x, y, width, height);
		@selarrow  = new AnimatedBitmap("Graphics/UI/Bag/cursor");
		@swaparrow = new AnimatedBitmap("Graphics/UI/Bag/cursor_swap");
		self.windowskin = null;
	}

	public override void dispose() {
		@swaparrow.dispose;
		base.dispose();
	}

	public int pocket { set {
		@pocket = value;
		@item_max = (@filterlist) ? @filterlist[@pocket].length + 1 : @bag.pockets[@pocket].length + 1;
		self.index = @bag.last_viewed_index(@pocket);
		}
	}

	public int page_row_max { get { return PokemonBag_Scene.ITEMSVISIBLE; } }
	public int page_item_max { get { return PokemonBag_Scene.ITEMSVISIBLE; } }

	public void item() {
		if (@filterlist && !@filterlist[@pocket][self.index]) return null;
		thispocket = @bag.pockets[@pocket];
		item = (@filterlist) ? thispocket[@filterlist[@pocket][self.index]] : thispocket[self.index];
		return (item) ? item[0] : null;
	}

	public void itemCount() {
		return (@filterlist) ? @filterlist[@pocket].length + 1 : @bag.pockets[@pocket].length + 1;
	}

	public void itemRect(item) {
		if (item < 0 || item >= @item_max || item < self.top_item - 1 ||
			item > self.top_item + self.page_item_max) {
			return new Rect(0, 0, 0, 0);
		} else {
			cursor_width = (self.width - self.borderX - ((@column_max - 1) * @column_spacing)) / @column_max;
			x = item % @column_max * (cursor_width + @column_spacing);
			y = (item / @column_max * @row_height) - @virtualOy;
			return new Rect(x, y, cursor_width, @row_height);
		}
	}

	public void drawCursor(index, rect) {
		if (self.index == index) {
			bmp = (@sorting) ? @swaparrow.bitmap : @selarrow.bitmap;
			CopyBitmap(self.contents, bmp, rect.x, rect.y + 2);
		}
	}

	public void drawItem(index, _count, rect) {
		textpos = new List<string>();
		rect = new Rect(rect.x + 16, rect.y + 16, rect.width - 16, rect.height);
		thispocket = @bag.pockets[@pocket];
		if (index == self.itemCount - 1) {
			textpos.Add(new {_INTL("CLOSE BAG"), rect.x, rect.y + 2, :left, self.baseColor, self.shadowColor});
		} else {
			item = (@filterlist) ? thispocket[@filterlist[@pocket][index]][0] : thispocket[index][0];
			baseColor   = self.baseColor;
			shadowColor = self.shadowColor;
			if (@sorting && index == self.index) {
				baseColor   = new Color(224, 0, 0);
				shadowColor = new Color(248, 144, 144);
			}
			textpos.Add(
				new {@adapter.getDisplayName(item), rect.x, rect.y + 2, :left, baseColor, shadowColor}
			);
			item_data = GameData.Item.get(item);
			showing_register_icon = false;
			if (item_data.is_important()) {
				if (@bag.registered(item)) {
					DrawImagePositions(
						self.contents,
						new {_INTL("Graphics/UI/Bag/icon_register"), rect.x + rect.width - 72, rect.y + 8, 0, 0, -1, 24}
					);
					showing_register_icon = true;
				} else if (CanRegisterItem(item)) {
					DrawImagePositions(
						self.contents,
						new {_INTL("Graphics/UI/Bag/icon_register"), rect.x + rect.width - 72, rect.y + 8, 0, 24, -1, 24}
					);
					showing_register_icon = true;
				}
			}
			if (item_data.show_quantity() && !showing_register_icon) {
				qty = (@filterlist) ? thispocket[@filterlist[@pocket][index]][1] : thispocket[index][1];
				qtytext = string.Format("x{1: 3d}", qty);
				xQty    = rect.x + rect.width - self.contents.text_size(qtytext).width - 16;
				textpos.Add(new {qtytext, xQty, rect.y + 2, :left, baseColor, shadowColor});
			}
		}
		DrawTextPositions(self.contents, textpos);
	}

	public void refresh() {
		@item_max = itemCount;
		self.update_cursor_rect;
		dwidth  = self.width - self.borderX;
		dheight = self.height - self.borderY;
		self.contents = DoEnsureBitmap(self.contents, dwidth, dheight);
		self.contents.clear;
		for (int i = @item_max; i < @item_max; i++) { //for '@item_max' times do => |i|
			if (i < self.top_item - 1 || i > self.top_item + self.page_item_max) continue;
			drawItem(i, @item_max, itemRect(i));
		}
		drawCursor(self.index, itemRect(self.index));
	}

	public override void update() {
		base.update();
		@uparrow.visible   = false;
		@downarrow.visible = false;
	}
}

//===============================================================================
// Bag visuals.
//===============================================================================
public partial class PokemonBag_Scene {
	ITEMLISTBASECOLOR     = new Color(88, 88, 80);
	ITEMLISTSHADOWCOLOR   = new Color(168, 184, 184);
	ITEMTEXTBASECOLOR     = new Color(248, 248, 248);
	ITEMTEXTSHADOWCOLOR   = new Color(0, 0, 0);
	POCKETNAMEBASECOLOR   = new Color(88, 88, 80);
	POCKETNAMESHADOWCOLOR = new Color(168, 184, 184);
	public const int ITEMSVISIBLE          = 7;

	public void Update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(bag, choosing = false, filterproc = null, resetpocket = true) {
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@bag        = bag;
		@choosing   = choosing;
		@filterproc = filterproc;
		RefreshFilter;
		lastpocket = @bag.last_viewed_pocket;
		numfilledpockets = @bag.pockets.length - 1;
		if (@choosing) {
			numfilledpockets = 0;
			if (@filterlist.null()) {
				for (int i = 1; i < @bag.pockets.length; i++) { //each '@bag.pockets.length' do => |i|
					if (@bag.pockets[i].length > 0) numfilledpockets += 1;
				}
			} else {
				for (int i = 1; i < @bag.pockets.length; i++) { //each '@bag.pockets.length' do => |i|
					if (@filterlist[i].length > 0) numfilledpockets += 1;
				}
			}
			lastpocket = (resetpocket) ? 1 : @bag.last_viewed_pocket;
			if ((@filterlist && @filterlist[lastpocket].length == 0) ||
				(!@filterlist && @bag.pockets[lastpocket].length == 0)) {
				for (int i = 1; i < @bag.pockets.length; i++) { //each '@bag.pockets.length' do => |i|
					if (@filterlist && @filterlist[i].length > 0) {
						lastpocket = i;
						break;
					} else if (!@filterlist && @bag.pockets[i].length > 0) {
						lastpocket = i;
						break;
					}
				}
			}
		}
		@bag.last_viewed_pocket = lastpocket;
		@sliderbitmap = new AnimatedBitmap("Graphics/UI/Bag/icon_slider");
		@pocketbitmap = new AnimatedBitmap("Graphics/UI/Bag/icon_pocket");
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, @viewport);
		SetSystemFont(@sprites["overlay"].bitmap);
		@sprites["bagsprite"] = new IconSprite(30, 20, @viewport);
		@sprites["pocketicon"] = new BitmapSprite(186, 32, @viewport);
		@sprites["pocketicon"].x = 0;
		@sprites["pocketicon"].y = 224;
		@sprites["leftarrow"] = new AnimatedSprite("Graphics/UI/left_arrow", 8, 40, 28, 2, @viewport);
		@sprites["leftarrow"].x       = -4;
		@sprites["leftarrow"].y       = 76;
		@sprites["leftarrow"].visible = (!@choosing || numfilledpockets > 1);
		@sprites["leftarrow"].play;
		@sprites["rightarrow"] = new AnimatedSprite("Graphics/UI/right_arrow", 8, 40, 28, 2, @viewport);
		@sprites["rightarrow"].x       = 150;
		@sprites["rightarrow"].y       = 76;
		@sprites["rightarrow"].visible = (!@choosing || numfilledpockets > 1);
		@sprites["rightarrow"].play;
		@sprites["itemlist"] = new Window_PokemonBag(@bag, @filterlist, lastpocket, 168, -8, 314, 40 + 32 + (ITEMSVISIBLE * 32));
		@sprites["itemlist"].viewport    = @viewport;
		@sprites["itemlist"].pocket      = lastpocket;
		@sprites["itemlist"].index       = @bag.last_viewed_index(lastpocket);
		@sprites["itemlist"].baseColor   = ITEMLISTBASECOLOR;
		@sprites["itemlist"].shadowColor = ITEMLISTSHADOWCOLOR;
		@sprites["itemicon"] = new ItemIconSprite(48, Graphics.height - 48, null, @viewport);
		@sprites["itemtext"] = Window_UnformattedTextPokemon.newWithSize(
			"", 72, 272, Graphics.width - 72 - 24, 128, @viewport
		);
		@sprites["itemtext"].baseColor   = ITEMTEXTBASECOLOR;
		@sprites["itemtext"].shadowColor = ITEMTEXTSHADOWCOLOR;
		@sprites["itemtext"].visible     = true;
		@sprites["itemtext"].windowskin  = null;
		@sprites["helpwindow"] = new Window_UnformattedTextPokemon("");
		@sprites["helpwindow"].visible  = false;
		@sprites["helpwindow"].viewport = @viewport;
		@sprites["msgwindow"] = new Window_AdvancedTextPokemon("");
		@sprites["msgwindow"].visible  = false;
		@sprites["msgwindow"].viewport = @viewport;
		BottomLeftLines(@sprites["helpwindow"], 1);
		DeactivateWindows(@sprites);
		Refresh;
		FadeInAndShow(@sprites);
	}

	public void FadeOutScene() {
		@oldsprites = FadeOutAndHide(@sprites);
	}

	public void FadeInScene() {
		FadeInAndShow(@sprites, @oldsprites);
		@oldsprites = null;
	}

	public void EndScene() {
		if (!@oldsprites) FadeOutAndHide(@sprites);
		@oldsprites = null;
		dispose;
	}

	public void dispose() {
		DisposeSpriteHash(@sprites);
		@sliderbitmap.dispose;
		@pocketbitmap.dispose;
		@viewport.dispose;
	}

	public void Display(msg, brief = false) {
		UIHelper.Display(@sprites["msgwindow"], msg, brief) { Update };
	}

	public void Confirm(msg) {
		UIHelper.Confirm(@sprites["msgwindow"], msg) { Update };
	}

	public void ChooseNumber(helptext, maximum, initnum = 1) {
		return UIHelper.ChooseNumber(@sprites["helpwindow"], helptext, maximum, initnum) { Update };
	}

	public void ShowCommands(helptext, commands, index = 0) {
		return UIHelper.ShowCommands(@sprites["helpwindow"], helptext, commands, index) { Update };
	}

	public void Refresh() {
		// Set the background image
		@sprites["background"].setBitmap(string.Format("Graphics/UI/Bag/bg_{0}", @bag.last_viewed_pocket));
		// Set the bag sprite
		fbagexists = ResolveBitmap(string.Format("Graphics/UI/Bag/bag_{0}_f", @bag.last_viewed_pocket));
		if (Game.GameData.player.female() && fbagexists) {
			@sprites["bagsprite"].setBitmap(string.Format("Graphics/UI/Bag/bag_{0}_f", @bag.last_viewed_pocket));
		} else {
			@sprites["bagsprite"].setBitmap(string.Format("Graphics/UI/Bag/bag_{0}", @bag.last_viewed_pocket));
		}
		// Draw the pocket icons
		@sprites["pocketicon"].bitmap.clear;
		if (@choosing && @filterlist) {
			for (int i = 1; i < @bag.pockets.length; i++) { //each '@bag.pockets.length' do => |i|
				if (@filterlist[i].length > 0) continue;
				@sprites["pocketicon"].bitmap.blt(
					6 + ((i - 1) * 22), 6, @pocketbitmap.bitmap, new Rect((i - 1) * 20, 28, 20, 20)
				);
			}
		}
		@sprites["pocketicon"].bitmap.blt(
			2 + ((@sprites["itemlist"].pocket - 1) * 22), 2, @pocketbitmap.bitmap,
			new Rect((@sprites["itemlist"].pocket - 1) * 28, 0, 28, 28)
		);
		// Refresh the item window
		@sprites["itemlist"].refresh;
		// Refresh more things
		RefreshIndexChanged;
	}

	public void RefreshIndexChanged() {
		itemlist = @sprites["itemlist"];
		overlay = @sprites["overlay"].bitmap;
		overlay.clear;
		// Draw the pocket name
		DrawTextPositions(
			overlay,
			new {PokemonBag.pocket_names[@bag.last_viewed_pocket - 1], 94, 186, :center, POCKETNAMEBASECOLOR, POCKETNAMESHADOWCOLOR}
		);
		// Draw slider arrows
		showslider = false;
		if (itemlist.top_row > 0) {
			overlay.blt(470, 16, @sliderbitmap.bitmap, new Rect(0, 0, 36, 38));
			showslider = true;
		}
		if (itemlist.top_item + itemlist.page_item_max < itemlist.itemCount) {
			overlay.blt(470, 228, @sliderbitmap.bitmap, new Rect(0, 38, 36, 38));
			showslider = true;
		}
		// Draw slider box
		if (showslider) {
			sliderheight = 174;
			boxheight = (int)Math.Floor(sliderheight * itemlist.page_row_max / itemlist.row_max);
			boxheight += (int)Math.Min((sliderheight - boxheight) / 2, sliderheight / 6);
			boxheight = (int)Math.Max(boxheight.floor, 38);
			y = 54;
			y += (int)Math.Floor((sliderheight - boxheight) * itemlist.top_row / (itemlist.row_max - itemlist.page_row_max));
			overlay.blt(470, y, @sliderbitmap.bitmap, new Rect(36, 0, 36, 4));
			i = 0;
			while (i * 16 < boxheight - 4 - 18) {
				height = (int)Math.Min(boxheight - 4 - 18 - (i * 16), 16);
				overlay.blt(470, y + 4 + (i * 16), @sliderbitmap.bitmap, new Rect(36, 4, 36, height));
				i += 1;
			}
			overlay.blt(470, y + boxheight - 18, @sliderbitmap.bitmap, new Rect(36, 20, 36, 18));
		}
		// Set the selected item's icon
		@sprites["itemicon"].item = itemlist.item;
		// Set the selected item's description
		@sprites["itemtext"].text =;
			(itemlist.item) ? GameData.Item.get(itemlist.item).description : _INTL("Close bag.")
	}

	public void RefreshFilter() {
		@filterlist = null;
		if (!@choosing) return;
		if (@filterproc.null()) return;
		@filterlist = new List<string>();
		for (int i = 1; i < @bag.pockets.length; i++) { //each '@bag.pockets.length' do => |i|
			@filterlist[i] = new List<string>();
			for (int j = @bag.pockets[i].length; j < @bag.pockets[i].length; j++) { //for '@bag.pockets[i].length' times do => |j|
				if (@filterproc.call(@bag.pockets[i][j][0])) @filterlist[i].Add(j);
			}
		}
	}

	// Called when the item screen wants an item to be chosen from the screen
	public void ChooseItem() {
		@sprites["helpwindow"].visible = false;
		itemwindow = @sprites["itemlist"];
		thispocket = @bag.pockets[itemwindow.pocket];
		swapinitialpos = -1;
		ActivateWindow(@sprites, "itemlist") do;
			do { //loop; while (true);
				oldindex = itemwindow.index;
				Graphics.update;
				Input.update;
				Update;
				if (itemwindow.sorting && itemwindow.index >= thispocket.length) {
					itemwindow.index = (oldindex == thispocket.length - 1) ? 0 : thispocket.length - 1;
				}
				if (itemwindow.index != oldindex) {
					// Move the item being switched
					if (itemwindow.sorting) {
						thispocket.insert(itemwindow.index, thispocket.delete_at(oldindex));
					}
					// Update selected item for current pocket
					@bag.set_last_viewed_index(itemwindow.pocket, itemwindow.index);
					Refresh;
				}
				if (itemwindow.sorting) {
					if (Input.trigger(Input.ACTION) ||
						Input.trigger(Input.USE)) {
						itemwindow.sorting = false;
						PlayDecisionSE;
						Refresh;
					} else if (Input.trigger(Input.BACK)) {
						thispocket.insert(swapinitialpos, thispocket.delete_at(itemwindow.index));
						itemwindow.index = swapinitialpos;
						itemwindow.sorting = false;
						PlayCancelSE
						Refresh;
					}
				} else {   // Change pockets
					if (Input.trigger(Input.LEFT)) {
						newpocket = itemwindow.pocket;
						do { //loop; while (true);
							newpocket = (newpocket == 1) ? PokemonBag.pocket_count : newpocket - 1;
							if (!@choosing || newpocket == itemwindow.pocket) break;
							if (@filterlist) {
								if (@filterlist[newpocket].length > 0) break;
							} else if (@bag.pockets[newpocket].length > 0) {
								break;
							}
						}
						if (itemwindow.pocket != newpocket) {
							itemwindow.pocket = newpocket;
							@bag.last_viewed_pocket = itemwindow.pocket;
							thispocket = @bag.pockets[itemwindow.pocket];
							PlayCursorSE;
							Refresh;
						}
					} else if (Input.trigger(Input.RIGHT)) {
						newpocket = itemwindow.pocket;
						do { //loop; while (true);
							newpocket = (newpocket == PokemonBag.pocket_count) ? 1 : newpocket + 1;
							if (!@choosing || newpocket == itemwindow.pocket) break;
							if (@filterlist) {
								if (@filterlist[newpocket].length > 0) break;
							} else if (@bag.pockets[newpocket].length > 0) {
								break;
							}
						}
						if (itemwindow.pocket != newpocket) {
							itemwindow.pocket = newpocket;
							@bag.last_viewed_pocket = itemwindow.pocket;
							thispocket = @bag.pockets[itemwindow.pocket];
							PlayCursorSE;
							Refresh;
						}
//          } else if (Input.trigger(Input.SPECIAL)) {   // Register/unregister selected item
//            if (!@choosing && itemwindow.index<thispocket.length) {
//              if (@bag.registered(itemwindow.item)) {
//                @bag.unregister(itemwindow.item)
//              } else if (CanRegisterItem(itemwindow.item)) {
//                @bag.register(itemwindow.item)
//              }
//              PlayDecisionSE
//              Refresh
//            }
					} else if (Input.trigger(Input.ACTION)) {   // Start switching the selected item
						if (!@choosing && thispocket.length > 1 && itemwindow.index < thispocket.length &&
							!Settings.BAG_POCKET_AUTO_SORT[itemwindow.pocket - 1]) {
							itemwindow.sorting = true;
							swapinitialpos = itemwindow.index;
							PlayDecisionSE;
							Refresh;
						}
					} else if (Input.trigger(Input.BACK)) {   // Cancel the item screen
						PlayCloseMenuSE;
						return null;
					} else if (Input.trigger(Input.USE)) {   // Choose selected item
						(itemwindow.item) ? PlayDecisionSE : PlayCloseMenuSE
						return itemwindow.item;
					}
				}
			}
		}
	}
}

//===============================================================================
// Bag mechanics.
//===============================================================================
public partial class PokemonBagScreen {
	public void initialize(scene, bag) {
		@bag   = bag;
		@scene = scene;
	}

	public void StartScreen() {
		@scene.StartScene(@bag);
		item = null;
		do { //loop; while (true);
			item = @scene.ChooseItem;
			if (!item) break;
			itm = GameData.Item.get(item);
			cmdRead     = -1;
			cmdUse      = -1;
			cmdRegister = -1;
			cmdGive     = -1;
			cmdToss     = -1;
			cmdDebug    = -1;
			commands = new List<string>();
			// Generate command list
			if (itm.is_mail()) commands[cmdRead = commands.length] = _INTL("Read");
			if (ItemHandlers.hasOutHandler(item) || (itm.is_machine() && Game.GameData.player.party.length > 0)) {
				if (ItemHandlers.hasUseText(item)) {
					commands[cmdUse = commands.length]    = ItemHandlers.getUseText(item);
				} else {
					commands[cmdUse = commands.length]    = _INTL("Use");
				}
			}
			if (Game.GameData.player.pokemon_party.length > 0 && itm.can_hold()) commands[cmdGive = commands.length]       = _INTL("Give");
			if (!itm.is_important() || Core.DEBUG) commands[cmdToss = commands.length]       = _INTL("Toss");
			if (@bag.registered(item)) {
				commands[cmdRegister = commands.length] = _INTL("Deselect");
			} else if (CanRegisterItem(item)) {
				commands[cmdRegister = commands.length] = _INTL("Register");
			}
			if (Core.DEBUG) commands[cmdDebug = commands.length]      = _INTL("Debug");
			commands[commands.length]                 = _INTL("Cancel");
			// Show commands generated above
			itemname = itm.name;
			command = @scene.ShowCommands(_INTL("{1} is selected.", itemname), commands);
			if (cmdRead >= 0 && command == cmdRead) {   // Read mail
				FadeOutIn do;
					DisplayMail(new Mail(item, "", ""));
				}
			} else if (cmdUse >= 0 && command == cmdUse) {   // Use item
				ret = UseItem(@bag, item, @scene);
				// ret: 0=Item wasn't used; 1=Item used; 2=Close Bag to use in field
				if (ret == 2) break;   // End screen
				@scene.Refresh;
				continue;
			} else if (cmdGive >= 0 && command == cmdGive) {   // Give item to Pokémon
				if (Game.GameData.player.pokemon_count == 0) {
					@scene.Display(_INTL("There is no Pokémon."));
				} else if (itm.is_important()) {
					@scene.Display(_INTL("The {1} can't be held.", itm.portion_name));
				} else {
					FadeOutIn do;
						sscene = new PokemonParty_Scene();
						sscreen = new PokemonPartyScreen(sscene, Game.GameData.player.party);
						sscreen.PokemonGiveScreen(item);
						@scene.Refresh;
					}
				}
			} else if (cmdToss >= 0 && command == cmdToss) {   // Toss item
				qty = @bag.quantity(item);
				if (qty > 1) {
					helptext = _INTL("Toss out how many {1}?", itm.portion_name_plural);
					qty = @scene.ChooseNumber(helptext, qty);
				}
				if (qty > 0) {
					itemname = (qty > 1) ? itm.portion_name_plural : itm.portion_name;
					if (Confirm(_INTL("Is it OK to throw away {1} {2}?", qty, itemname))) {
						Display(_INTL("Threw away {1} {2}.", qty, itemname));
						qty.times(() => @bag.remove(item));
						@scene.Refresh;
					}
				}
			} else if (cmdRegister >= 0 && command == cmdRegister) {   // Register item
				if (@bag.registered(item)) {
					@bag.unregister(item);
				} else {
					@bag.register(item);
				}
				@scene.Refresh;
			} else if (cmdDebug >= 0 && command == cmdDebug) {   // Debug
				command = 0;
				do { //loop; while (true);
					command = @scene.ShowCommands(_INTL("Do what with {1}?", itemname),
																					new {_INTL("Change quantity"),
																					_INTL("Make Mystery Gift"),
																					_INTL("Cancel")}, command);
					switch (command) {
						//## Cancel ###
						case -1: case 2:
							break;
							break;
						//## Change quantity ###
						case 0:
							qty = @bag.quantity(item);
							itemplural = itm.name_plural;
							params = new ChooseNumberParams();
							params.setRange(0, Settings.BAG_MAX_PER_SLOT);
							params.setDefaultValue(qty);
							newqty = MessageChooseNumber(
								_INTL("Choose new quantity of {1} (max. {2}).", itemplural, Settings.BAG_MAX_PER_SLOT), params,
								() => @scene.Update))
							if (newqty > qty) {
								@bag.add(item, newqty - qty);
							} else if (newqty < qty) {
								@bag.remove(item, qty - newqty);
							}
							@scene.Refresh;
							if (newqty == 0) break;
							break;
						//## Make Mystery Gift ###
						case 1:
							CreateMysteryGift(1, item);
							break;
					}
				}
			}
		}
		(Game.GameData.game_temp.fly_destination) ? @scene.dispose : @scene.EndScene
		return item;
	}

	public void Display(text) {
		@scene.Display(text);
	}

	public void Confirm(text) {
		return @scene.Confirm(text);
	}

	// UI logic for the item screen for choosing an item.
	public void ChooseItemScreen(proc = null) {
		oldlastpocket = @bag.last_viewed_pocket;
		oldchoices = @bag.last_pocket_selections.clone;
		if (proc) @bag.reset_last_selections;
		@scene.StartScene(@bag, true, proc);
		item = @scene.ChooseItem;
		@scene.EndScene;
		@bag.last_viewed_pocket = oldlastpocket;
		@bag.last_pocket_selections = oldchoices;
		return item;
	}

	// UI logic for withdrawing an item in the item storage screen.
	public void WithdrawItemScreen() {
		if (!Game.GameData.PokemonGlobal.pcItemStorage) {
			Game.GameData.PokemonGlobal.pcItemStorage = new PCItemStorage();
		}
		storage = Game.GameData.PokemonGlobal.pcItemStorage;
		@scene.StartScene(storage);
		do { //loop; while (true);
			item = @scene.ChooseItem;
			if (!item) break;
			itm = GameData.Item.get(item);
			qty = storage.quantity(item);
			if (qty > 1 && !itm.is_important()) {
				qty = @scene.ChooseNumber(_INTL("How many do you want to withdraw?"), qty);
			}
			if (qty <= 0) continue;
			if (@bag.can_add(item, qty)) {
				if (!storage.remove(item, qty)) {
					Debug.LogError("Can't delete items from storage");
					//throw new ArgumentException("Can't delete items from storage");
				}
				if (!@bag.add(item, qty)) {
					Debug.LogError("Can't withdraw items from storage");
					//throw new ArgumentException("Can't withdraw items from storage");
				}
				@scene.Refresh;
				dispqty = (itm.is_important()) ? 1 : qty;
				itemname = (dispqty > 1) ? itm.portion_name_plural : itm.portion_name;
				Display(_INTL("Withdrew {1} {2}.", dispqty, itemname));
			} else {
				Display(_INTL("There's no more room in the Bag."));
			}
		}
		@scene.EndScene;
	}

	// UI logic for depositing an item in the item storage screen.
	public void DepositItemScreen() {
		@scene.StartScene(@bag);
		if (!Game.GameData.PokemonGlobal.pcItemStorage) {
			Game.GameData.PokemonGlobal.pcItemStorage = new PCItemStorage();
		}
		storage = Game.GameData.PokemonGlobal.pcItemStorage;
		do { //loop; while (true);
			item = @scene.ChooseItem;
			if (!item) break;
			itm = GameData.Item.get(item);
			qty = @bag.quantity(item);
			if (qty > 1 && !itm.is_important()) {
				qty = @scene.ChooseNumber(_INTL("How many do you want to deposit?"), qty);
			}
			if (qty > 0) {
				if (storage.can_add(item, qty)) {
					if (!@bag.remove(item, qty)) {
						Debug.LogError("Can't delete items from Bag");
						//throw new ArgumentException("Can't delete items from Bag");
					}
					if (!storage.add(item, qty)) {
						Debug.LogError("Can't deposit items to storage");
						//throw new ArgumentException("Can't deposit items to storage");
					}
					@scene.Refresh;
					dispqty  = (itm.is_important()) ? 1 : qty;
					itemname = (dispqty > 1) ? itm.portion_name_plural : itm.portion_name;
					Display(_INTL("Deposited {1} {2}.", dispqty, itemname));
				} else {
					Display(_INTL("There's no room to store items."));
				}
			}
		}
		@scene.EndScene;
	}

	// UI logic for tossing an item in the item storage screen.
	public void TossItemScreen() {
		if (!Game.GameData.PokemonGlobal.pcItemStorage) {
			Game.GameData.PokemonGlobal.pcItemStorage = new PCItemStorage();
		}
		storage = Game.GameData.PokemonGlobal.pcItemStorage;
		@scene.StartScene(storage);
		do { //loop; while (true);
			item = @scene.ChooseItem;
			if (!item) break;
			itm = GameData.Item.get(item);
			if (itm.is_important()) {
				@scene.Display(_INTL("That's too important to toss out!"));
				continue;
			}
			qty = storage.quantity(item);
			itemname       = itm.portion_name;
			itemnameplural = itm.portion_name_plural;
			if (qty > 1) {
				qty = @scene.ChooseNumber(_INTL("Toss out how many {1}?", itemnameplural), qty);
			}
			if (qty <= 0) continue;
			if (qty > 1) itemname = itemnameplural;
			if (!Confirm(_INTL("Is it OK to throw away {1} {2}?", qty, itemname))) continue;
			if (!storage.remove(item, qty)) {
				Debug.LogError("Can't delete items from storage");
				//throw new ArgumentException("Can't delete items from storage");
			}
			@scene.Refresh;
			Display(_INTL("Threw away {1} {2}.", qty, itemname));
		}
		@scene.EndScene;
	}
}
