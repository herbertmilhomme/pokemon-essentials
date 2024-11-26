//===============================================================================
// Abstraction layer for Pokemon Essentials.
//===============================================================================
public partial class BattlePointShopAdapter {
	public void getBP() {
		return Game.GameData.player.battle_points;
	}

	public void getBPString() {
		return _INTL("{1} BP", Game.GameData.player.battle_points.to_s_formatted);
	}

	public void setBP(value) {
		Game.GameData.player.battle_points = value;
	}

	public void getInventory() {
		return Game.GameData.bag;
	}

	public void getName(item) {
		return GameData.Item.get(item).portion_name;
	}

	public void getNamePlural(item) {
		return GameData.Item.get(item).portion_name_plural;
	}

	public void getDisplayName(item) {
		item_name = GameData.Item.get(item).name;
		if (GameData.Item.get(item).is_machine()) {
			machine = GameData.Item.get(item).move;
			item_name = _INTL("{1} {2}", item_name, GameData.Move.get(machine).name);
		}
		return item_name;
	}

	public void getDisplayNamePlural(item) {
		item_name_plural = GameData.Item.get(item).name_plural;
		if (GameData.Item.get(item).is_machine()) {
			machine = GameData.Item.get(item).move;
			item_name_plural = _INTL("{1} {2}", item_name_plural, GameData.Move.get(machine).name);
		}
		return item_name_plural;
	}

	public void getDescription(item) {
		return GameData.Item.get(item).description;
	}

	public void getItemIcon(item) {
		return (item) ? GameData.Item.icon_filename(item) : null;
	}

	// Unused
	public void getItemIconRect(_item) {
		return new Rect(0, 0, 48, 48);
	}

	public void getQuantity(item) {
		return Game.GameData.bag.quantity(item);
	}

	public bool showQuantity(item) {
		return !GameData.Item.get(item).is_important();
	}

	public void getPrice(item) {
		if (Game.GameData.game_temp.mart_prices && Game.GameData.game_temp.mart_prices[item]) {
			if (Game.GameData.game_temp.mart_prices[item][0] > 0) {
				return Game.GameData.game_temp.mart_prices[item][0];
			}
		}
		return GameData.Item.get(item).bp_price;
	}

	public void getDisplayPrice(item, selling = false) {
		price = getPrice(item).to_s_formatted;
		return _INTL("{1} BP", price);
	}

	public void addItem(item) {
		return Game.GameData.bag.add(item);
	}

	public void removeItem(item) {
		return Game.GameData.bag.remove(item);
	}
}

//===============================================================================
// Battle Point Shop.
//===============================================================================
public partial class Window_BattlePointShop : Window_DrawableCommand {
	public override void initialize(stock, adapter, x, y, width, height, viewport = null) {
		@stock       = stock;
		@adapter     = adapter;
		base.initialize(x, y, width, height, viewport);
		@selarrow    = new AnimatedBitmap("Graphics/UI/Mart/cursor");
		@baseColor   = new Color(88, 88, 80);
		@shadowColor = new Color(168, 184, 184);
		self.windowskin = null;
	}

	public void itemCount() {
		return @stock.length + 1;
	}

	public void item() {
		return (self.index >= @stock.length) ? null : @stock[self.index];
	}

	public void drawItem(index, count, rect) {
		textpos = new List<string>();
		rect = drawCursor(index, rect);
		ypos = rect.y;
		if (index == count - 1) {
			textpos.Add(new {_INTL("CANCEL"), rect.x, ypos + 2, :left, self.baseColor, self.shadowColor});
		} else {
			item = @stock[index];
			itemname = @adapter.getDisplayName(item);
			qty = @adapter.getDisplayPrice(item);
			sizeQty = self.contents.text_size(qty).width;
			xQty = rect.x + rect.width - sizeQty - 2 - 16;
			textpos.Add(new {itemname, rect.x, ypos + 2, :left, self.baseColor, self.shadowColor});
			textpos.Add(new {qty, xQty, ypos + 2, :left, self.baseColor, self.shadowColor});
		}
		DrawTextPositions(self.contents, textpos);
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattlePointShop_Scene {
	public void update() {
		UpdateSpriteHash(@sprites);
		@subscene&.Update;
	}

	public void Refresh() {
		if (@subscene) {
			@subscene.Refresh;
		} else {
			itemwindow = @sprites["itemwindow"];
			@sprites["icon"].item = itemwindow.item;
			@sprites["itemtextwindow"].text =;
				(itemwindow.item) ? @adapter.getDescription(itemwindow.item) : _INTL("Quit shopping.")
			@sprites["qtywindow"].visible = !itemwindow.item.null();
			@sprites["qtywindow"].text    = _INTL("In Bag:<r>{1}", @adapter.getQuantity(itemwindow.item));
			@sprites["qtywindow"].y       = Graphics.height - 102 - @sprites["qtywindow"].height;
			itemwindow.refresh;
		}
		@sprites["battlepointwindow"].text = _INTL("Battle Points:\n<r>{1}", @adapter.getBPString);
	}

	public void StartScene(stock, adapter) {
		// Scroll right before showing screen
		ScrollMap(6, 5, 5);
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@stock = stock;
		@adapter = adapter;
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["background"].setBitmap("Graphics/UI/Mart/bg");
		@sprites["icon"] = new ItemIconSprite(36, Graphics.height - 50, null, @viewport);
		winAdapter = new BattlePointShopAdapter();
		@sprites["itemwindow"] = new Window_BattlePointShop(
			stock, winAdapter, Graphics.width - 316 - 16, 10, 330 + 16, Graphics.height - 124
		);
		@sprites["itemwindow"].viewport = @viewport;
		@sprites["itemwindow"].index = 0;
		@sprites["itemwindow"].refresh;
		@sprites["itemtextwindow"] = Window_UnformattedTextPokemon.newWithSize(
			"", 64, Graphics.height - 96 - 16, Graphics.width - 64, 128, @viewport
		);
		PrepareWindow(@sprites["itemtextwindow"]);
		@sprites["itemtextwindow"].baseColor = new Color(248, 248, 248);
		@sprites["itemtextwindow"].shadowColor = new Color(0, 0, 0);
		@sprites["itemtextwindow"].windowskin = null;
		@sprites["helpwindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["helpwindow"]);
		@sprites["helpwindow"].visible = false;
		@sprites["helpwindow"].viewport = @viewport;
		BottomLeftLines(@sprites["helpwindow"], 1);
		@sprites["battlepointwindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["battlepointwindow"]);
		@sprites["battlepointwindow"].setSkin("Graphics/Windowskins/goldskin");
		@sprites["battlepointwindow"].visible = true;
		@sprites["battlepointwindow"].viewport = @viewport;
		@sprites["battlepointwindow"].x = 0;
		@sprites["battlepointwindow"].y = 0;
		@sprites["battlepointwindow"].width = 190;
		@sprites["battlepointwindow"].height = 96;
		@sprites["battlepointwindow"].baseColor = new Color(88, 88, 80);
		@sprites["battlepointwindow"].shadowColor = new Color(168, 184, 184);
		@sprites["qtywindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["qtywindow"]);
		@sprites["qtywindow"].setSkin("Graphics/Windowskins/goldskin");
		@sprites["qtywindow"].viewport = @viewport;
		@sprites["qtywindow"].width = 190;
		@sprites["qtywindow"].height = 64;
		@sprites["qtywindow"].baseColor = new Color(88, 88, 80);
		@sprites["qtywindow"].shadowColor = new Color(168, 184, 184);
		@sprites["qtywindow"].text = _INTL("In Bag:<r>{1}", @adapter.getQuantity(@sprites["itemwindow"].item));
		@sprites["qtywindow"].y = Graphics.height - 102 - @sprites["qtywindow"].height;
		DeactivateWindows(@sprites);
		Refresh;
		Graphics.frame_reset;
	}

	public void EndScene() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		// Scroll left after showing screen
		ScrollMap(4, 5, 5);
	}

	public void PrepareWindow(window) {
		window.visible = true;
		window.letterbyletter = false;
	}

	public void ShowBattlePoints() {
		Refresh;
		@sprites["battlepointwindow"].visible = true;
	}

	public void HideBattlePoints() {
		Refresh;
		@sprites["battlepointwindow"].visible = false;
	}

	public void ShowQuantity() {
		Refresh;
		@sprites["qtywindow"].visible = true;
	}

	public void HideQuantity() {
		Refresh;
		@sprites["qtywindow"].visible = false;
	}

	public void Display(msg, brief = false) {
		cw = @sprites["helpwindow"];
		cw.letterbyletter = true;
		cw.text = msg;
		BottomLeftLines(cw, 2);
		cw.visible = true;
		PlayDecisionSE;
		refreshed_after_busy = false;
		timer_start = System.uptime;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			self.update;
			if (!cw.busy()) {
				if (brief) return;
				if (!refreshed_after_busy) {
					Refresh;
					timer_start = System.uptime;
					refreshed_after_busy = true;
				}
			}
			if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
				if (cw.busy()) cw.resume;
			}
			if (refreshed_after_busy && System.uptime - timer_start >= 1.5) return;
		}
	}

	public void DisplayPaused(msg) {
		cw = @sprites["helpwindow"];
		cw.letterbyletter = true;
		cw.text = msg;
		BottomLeftLines(cw, 2);
		cw.visible = true;
		yielded = false;
		PlayDecisionSE;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			wasbusy = cw.busy();
			self.update;
			if (!cw.busy() && !yielded) {
				if (block_given()) yield;   // For playing SE as soon as the message is all shown
				yielded = true;
			}
			if (!cw.busy() && wasbusy) Refresh;
			if (Input.trigger(Input.USE) || Input.trigger(Input.BACK)) {
				if (cw.resume && !cw.busy()) {
					@sprites["helpwindow"].visible = false;
					break;
				}
			}
		}
	}

	public void Confirm(msg) {
		dw = @sprites["helpwindow"];
		dw.letterbyletter = true;
		dw.text = msg;
		dw.visible = true;
		BottomLeftLines(dw, 2);
		commands = new {_INTL("Yes"), _INTL("No")};
		cw = new Window_CommandPokemon(commands);
		cw.viewport = @viewport;
		BottomRight(cw);
		cw.y -= dw.height;
		cw.index = 0;
		PlayDecisionSE;
		do { //loop; while (true);
			cw.visible = !dw.busy();
			Graphics.update;
			Input.update;
			cw.update;
			self.update;
			if (Input.trigger(Input.BACK) && dw.resume && !dw.busy()) {
				cw.dispose;
				@sprites["helpwindow"].visible = false;
				return false;
			}
			if (Input.trigger(Input.USE) && dw.resume && !dw.busy()) {
				cw.dispose;
				@sprites["helpwindow"].visible = false;
				return (cw.index == 0);
			}
		}
	}

	public void ChooseNumber(helptext, item, maximum) {
		curnumber = 1;
		ret = 0;
		helpwindow = @sprites["helpwindow"];
		itemprice = @adapter.getPrice(item);
		if (!@buying) itemprice /= 2;
		Display(helptext, true);
		using(numwindow = new Window_AdvancedTextPokemon("")) do;   // Showing number of items
			PrepareWindow(numwindow);
			numwindow.viewport = @viewport;
			numwindow.width = 224;
			numwindow.height = 64;
			numwindow.baseColor = new Color(88, 88, 80);
			numwindow.shadowColor = new Color(168, 184, 184);
			numwindow.text = _INTL("x{1}<r>{2} BP", curnumber, (curnumber * itemprice).to_s_formatted);
			BottomRight(numwindow);
			numwindow.y -= helpwindow.height;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				numwindow.update;
				update;
				oldnumber = curnumber;
				if (Input.repeat(Input.LEFT)) {
					curnumber -= 10;
					if (curnumber < 1) curnumber = 1;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>{2} BP", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.repeat(Input.RIGHT)) {
					curnumber += 10;
					if (curnumber > maximum) curnumber = maximum;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>{2} BP", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.repeat(Input.UP)) {
					curnumber += 1;
					if (curnumber > maximum) curnumber = 1;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>{2} BP", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.repeat(Input.DOWN)) {
					curnumber -= 1;
					if (curnumber < 1) curnumber = maximum;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>{2} BP", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.trigger(Input.USE)) {
					ret = curnumber;
					break;
				} else if (Input.trigger(Input.BACK)) {
					PlayCancelSE
					ret = 0;
					break;
				}
			}
		}
		helpwindow.visible = false;
		return ret;
	}

	public void ChooseItem() {
		itemwindow = @sprites["itemwindow"];
		@sprites["helpwindow"].visible = false;
		ActivateWindow(@sprites, "itemwindow") do;
			Refresh;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				olditem = itemwindow.item;
				self.update;
				if (itemwindow.item != olditem) Refresh;
				if (Input.trigger(Input.BACK)) {
					PlayCloseMenuSE;
					return null;
				} else if (Input.trigger(Input.USE)) {
					if (itemwindow.index < @stock.length) {
						Refresh;
						return @stock[itemwindow.index];
					} else {
						return null;
					}
				}
			}
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class BattlePointShopScreen {
	public void initialize(scene, stock) {
		@scene = scene;
		@stock = stock;
		@adapter = new BattlePointShopAdapter();
	}

	public void Confirm(msg) {
		return @scene.Confirm(msg);
	}

	public void Display(msg) {
		return @scene.Display(msg);
	}

	public void DisplayPaused(msg, Action block = null) {
		return @scene.DisplayPaused(msg, &block);
	}

	public void BuyScreen() {
		@scene.StartScene(@stock, @adapter);
		item = null;
		do { //loop; while (true);
			item = @scene.ChooseItem;
			if (!item) break;
			quantity       = 0;
			itemname       = @adapter.getName(item);
			itemnameplural = @adapter.getNamePlural(item);
			price = @adapter.getPrice(item);
			if (@adapter.getBP < price) {
				DisplayPaused(_INTL("You don't have enough BP."));
				continue;
			}
			if (GameData.Item.get(item).is_important()) {
				if ((!Confirm(_INTL("You would like the {1}?\nThat will be {2} BP.",
																itemname, price.to_s_formatted))) continue;
				quantity = 1;
			} else {
				maxafford = (price <= 0) ? Settings.BAG_MAX_PER_SLOT : @adapter.getBP / price;
				if (maxafford > Settings.BAG_MAX_PER_SLOT) maxafford = Settings.BAG_MAX_PER_SLOT;
				quantity = @scene.ChooseNumber(
					_INTL("How many {1} would you like?", itemnameplural), item, maxafford
				);
				if (quantity == 0) continue;
				price *= quantity;
				if (quantity > 1) {
					if ((!Confirm(_INTL("You would like {1} {2}?\nThey'll be {3} BP.",
																	quantity, itemnameplural, price.to_s_formatted))) continue;
				} else if (quantity > 0) {
					if ((!Confirm(_INTL("So you want {1} {2}?\nIt'll be {3} BP.",
																	quantity, itemname, price.to_s_formatted))) continue;
				}
			}
			if (@adapter.getBP < price) {
				DisplayPaused(_INTL("I'm sorry, you don't have enough BP."));
				continue;
			}
			added = 0;
			quantity.times do;
				if (!@adapter.addItem(item)) break;
				added += 1;
			}
			if (added == quantity) {
				Game.GameData.stats.battle_points_spent += price;
				Game.GameData.stats.mart_items_bought += quantity;
				@adapter.setBP(@adapter.getBP - price);
				@stock.delete_if(itm => GameData.Item.get(itm).is_important() && Game.GameData.bag.has(itm));
				DisplayPaused(_INTL("Here you are! Thank you!")) { SEPlay("Mart buy item") };
			} else {
				added.times do;
					if (!@adapter.removeItem(item)) {
						Debug.LogError(_INTL("Failed to delete stored items"));
						//throw new ArgumentException(_INTL("Failed to delete stored items"));
					}
				}
				DisplayPaused(_INTL("You have no room in your Bag."));
			}
		}
		@scene.EndScene;
	}
}

//===============================================================================
//
//===============================================================================
public void BattlePointShop(stock, speech = null) {
	stock.delete_if(item => GameData.Item.get(item).is_important() && Game.GameData.bag.has(item));
	if (speech.null()) {
		Message(_INTL("Welcome to the Exchange Service Corner!"));
		Message(_INTL("We can exchange your BP for fabulous items."));
	} else {
		Message(speech);
	}
	scene = new BattlePointShop_Scene();
	screen = new BattlePointShopScreen(scene, stock);
	screen.BuyScreen;
	Message(_INTL("Thank you for visiting."));
	Message(_INTL("Please visit us again when you have saved up more BP."));
	Game.GameData.game_temp.clear_mart_prices;
}
