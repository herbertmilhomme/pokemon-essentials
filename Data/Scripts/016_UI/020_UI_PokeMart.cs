//===============================================================================
// Abstraction layer for Pokemon Essentials.
//===============================================================================
public partial class PokemonMartAdapter {
	public void getMoney() {
		return Game.GameData.player.money;
	}

	public void getMoneyString() {
		return GetGoldString;
	}

	public void setMoney(value) {
		Game.GameData.player.money = value;
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

	public void getPrice(item, selling = false) {
		if (Game.GameData.game_temp.mart_prices && Game.GameData.game_temp.mart_prices[item]) {
			if (selling) {
				if (Game.GameData.game_temp.mart_prices[item][1] >= 0) return Game.GameData.game_temp.mart_prices[item][1];
			} else if (Game.GameData.game_temp.mart_prices[item][0] > 0) {
				return Game.GameData.game_temp.mart_prices[item][0];
			}
		}
		if (selling) return GameData.Item.get(item).sell_price;
		return GameData.Item.get(item).price;
	}

	public void getDisplayPrice(item, selling = false) {
		price = getPrice(item, selling).to_s_formatted;
		return _INTL("$ {1}", price);
	}

	public bool canSell(item) {
		return getPrice(item, true) > 0 && !GameData.Item.get(item).is_important();
	}

	public void addItem(item) {
		return Game.GameData.bag.add(item);
	}

	public void removeItem(item) {
		return Game.GameData.bag.remove(item);
	}
}

//===============================================================================
// Buy and Sell adapters.
//===============================================================================
public partial class BuyAdapter {
	public void initialize(adapter) {
		@adapter = adapter;
	}

	// For showing in messages
	public void getName(item) {
		@adapter.getName(item);
	}

	// For showing in messages
	public void getNamePlural(item) {
		@adapter.getNamePlural(item);
	}

	// For showing in the list of items
	public void getDisplayName(item) {
		@adapter.getDisplayName(item);
	}

	// For showing in the list of items
	public void getDisplayNamePlural(item) {
		@adapter.getDisplayNamePlural(item);
	}

	public void getDisplayPrice(item) {
		@adapter.getDisplayPrice(item, false);
	}

	public bool isSelling() {
		return false;
	}
}

//===============================================================================
//
//===============================================================================
public partial class SellAdapter {
	public void initialize(adapter) {
		@adapter = adapter;
	}

	// For showing in messages
	public void getName(item) {
		@adapter.getName(item);
	}

	// For showing in messages
	public void getNamePlural(item) {
		@adapter.getNamePlural(item);
	}

	// For showing in the list of items
	public void getDisplayName(item) {
		@adapter.getDisplayName(item);
	}

	// For showing in the list of items
	public void getDisplayNamePlural(item) {
		@adapter.getDisplayNamePlural(item);
	}

	public void getDisplayPrice(item) {
		if (@adapter.showQuantity(item)) {
			return string.Format("x{0}", @adapter.getQuantity(item));
		} else {
			return "";
		}
	}

	public bool isSelling() {
		return true;
	}
}

//===============================================================================
// Pokémon Mart.
//===============================================================================
public partial class Window_PokemonMart : Window_DrawableCommand {
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
public partial class PokemonMart_Scene {
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
		@sprites["moneywindow"].text = _INTL("Money:\n<r>{1}", @adapter.getMoneyString);
	}

	public void StartBuyOrSellScene(buying, stock, adapter) {
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
		winAdapter = buying ? new BuyAdapter(adapter) : new SellAdapter(adapter);
		@sprites["itemwindow"] = new Window_PokemonMart(
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
		@sprites["itemtextwindow"].shadowColor = Color.black;
		@sprites["itemtextwindow"].windowskin = null;
		@sprites["helpwindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["helpwindow"]);
		@sprites["helpwindow"].visible = false;
		@sprites["helpwindow"].viewport = @viewport;
		BottomLeftLines(@sprites["helpwindow"], 1);
		@sprites["moneywindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["moneywindow"]);
		@sprites["moneywindow"].setSkin("Graphics/Windowskins/goldskin");
		@sprites["moneywindow"].visible = true;
		@sprites["moneywindow"].viewport = @viewport;
		@sprites["moneywindow"].x = 0;
		@sprites["moneywindow"].y = 0;
		@sprites["moneywindow"].width = 190;
		@sprites["moneywindow"].height = 96;
		@sprites["moneywindow"].baseColor = new Color(88, 88, 80);
		@sprites["moneywindow"].shadowColor = new Color(168, 184, 184);
		@sprites["qtywindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["qtywindow"]);
		@sprites["qtywindow"].setSkin("Graphics/Windowskins/goldskin");
		@sprites["qtywindow"].viewport = @viewport;
		@sprites["qtywindow"].width = 190;
		@sprites["qtywindow"].height = 64;
		@sprites["qtywindow"].baseColor = new Color(88, 88, 80);
		@sprites["qtywindow"].shadowColor = new Color(168, 184, 184);
		@sprites["qtywindow"].text = _INTL("In Bag:<r>{1}", @adapter.getQuantity(@sprites["itemwindow"].item));
		@sprites["qtywindow"].y    = Graphics.height - 102 - @sprites["qtywindow"].height;
		DeactivateWindows(@sprites);
		@buying = buying;
		Refresh;
		Graphics.frame_reset;
	}

	public void StartBuyScene(stock, adapter) {
		StartBuyOrSellScene(true, stock, adapter);
	}

	public void StartSellScene(bag, adapter) {
		if (Game.GameData.bag) {
			StartSellScene2(bag, adapter);
		} else {
			StartBuyOrSellScene(false, bag, adapter);
		}
	}

	public void StartSellScene2(bag, adapter) {
		@subscene = new PokemonBag_Scene();
		@adapter = adapter;
		@viewport2 = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport2.z = 99999;
		foreach (var delta_t in Wait(0.4)) { //Wait(0.4) do => |delta_t|
			@viewport2.color.alpha = lerp(0, 255, 0.4, delta_t);
		}
		@viewport2.color.alpha = 255;
		@subscene.StartScene(bag);
		@viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@sprites = new List<string>();
		@sprites["helpwindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["helpwindow"]);
		@sprites["helpwindow"].visible = false;
		@sprites["helpwindow"].viewport = @viewport;
		BottomLeftLines(@sprites["helpwindow"], 1);
		@sprites["moneywindow"] = new Window_AdvancedTextPokemon("");
		PrepareWindow(@sprites["moneywindow"]);
		@sprites["moneywindow"].setSkin("Graphics/Windowskins/goldskin");
		@sprites["moneywindow"].visible = false;
		@sprites["moneywindow"].viewport = @viewport;
		@sprites["moneywindow"].x = 0;
		@sprites["moneywindow"].y = 0;
		@sprites["moneywindow"].width = 186;
		@sprites["moneywindow"].height = 96;
		@sprites["moneywindow"].baseColor = new Color(88, 88, 80);
		@sprites["moneywindow"].shadowColor = new Color(168, 184, 184);
		DeactivateWindows(@sprites);
		@buying = false;
		Refresh;
	}

	public void EndBuyScene() {
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
		// Scroll left after showing screen
		ScrollMap(4, 5, 5);
	}

	public void EndSellScene() {
		@subscene&.EndScene;
		DisposeSpriteHash(@sprites);
		if (@viewport2) {
			foreach (var delta_t in Wait(0.4)) { //Wait(0.4) do => |delta_t|
				@viewport2.color.alpha = lerp(255, 0, 0.4, delta_t);
			}
			@viewport2.dispose;
		}
		@viewport.dispose;
		if (!@subscene) ScrollMap(4, 5, 5);
	}

	public void PrepareWindow(window) {
		window.visible = true;
		window.letterbyletter = false;
	}

	public void ShowMoney() {
		Refresh;
		@sprites["moneywindow"].visible = true;
	}

	public void HideMoney() {
		Refresh;
		@sprites["moneywindow"].visible = false;
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
		itemprice = @adapter.getPrice(item, !@buying);
		Display(helptext, true);
		using(numwindow = new Window_AdvancedTextPokemon("")) do;   // Showing number of items
			PrepareWindow(numwindow);
			numwindow.viewport = @viewport;
			numwindow.width = 224;
			numwindow.height = 64;
			numwindow.baseColor = new Color(88, 88, 80);
			numwindow.shadowColor = new Color(168, 184, 184);
			numwindow.text = _INTL("x{1}<r>$ {2}", curnumber, (curnumber * itemprice).to_s_formatted);
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
						numwindow.text = _INTL("x{1}<r>$ {2}", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.repeat(Input.RIGHT)) {
					curnumber += 10;
					if (curnumber > maximum) curnumber = maximum;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>$ {2}", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.repeat(Input.UP)) {
					curnumber += 1;
					if (curnumber > maximum) curnumber = 1;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>$ {2}", curnumber, (curnumber * itemprice).to_s_formatted);
						PlayCursorSE;
					}
				} else if (Input.repeat(Input.DOWN)) {
					curnumber -= 1;
					if (curnumber < 1) curnumber = maximum;
					if (curnumber != oldnumber) {
						numwindow.text = _INTL("x{1}<r>$ {2}", curnumber, (curnumber * itemprice).to_s_formatted);
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

	public void ChooseBuyItem() {
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

	public void ChooseSellItem() {
		if (@subscene) {
			return @subscene.ChooseItem;
		} else {
			return ChooseBuyItem;
		}
	}
}

//===============================================================================
//
//===============================================================================
public partial class PokemonMartScreen {
	public void initialize(scene, stock) {
		@scene = scene;
		@stock = stock;
		@adapter = new PokemonMartAdapter();
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
		@scene.StartBuyScene(@stock, @adapter);
		item = null;
		do { //loop; while (true);
			item = @scene.ChooseBuyItem;
			if (!item) break;
			quantity       = 0;
			itemname       = @adapter.getName(item);
			itemnameplural = @adapter.getNamePlural(item);
			price = @adapter.getPrice(item);
			if (@adapter.getMoney < price) {
				DisplayPaused(_INTL("You don't have enough money."));
				continue;
			}
			if (GameData.Item.get(item).is_important()) {
				if ((!Confirm(_INTL("So you want the {1}?\nIt'll be ${2}. All right?",
																itemname, price.to_s_formatted))) continue;
				quantity = 1;
			} else {
				maxafford = (price <= 0) ? Settings.BAG_MAX_PER_SLOT : @adapter.getMoney / price;
				if (maxafford > Settings.BAG_MAX_PER_SLOT) maxafford = Settings.BAG_MAX_PER_SLOT;
				quantity = @scene.ChooseNumber(
					_INTL("So how many {1}?", itemnameplural), item, maxafford
				);
				if (quantity == 0) continue;
				price *= quantity;
				if (quantity > 1) {
					if ((!Confirm(_INTL("So you want {1} {2}?\nThey'll be ${3}. All right?",
																	quantity, itemnameplural, price.to_s_formatted))) continue;
				} else if (quantity > 0) {
					if ((!Confirm(_INTL("So you want {1} {2}?\nIt'll be ${3}. All right?",
																	quantity, itemname, price.to_s_formatted))) continue;
				}
			}
			if (@adapter.getMoney < price) {
				DisplayPaused(_INTL("You don't have enough money."));
				continue;
			}
			added = 0;
			quantity.times do;
				if (!@adapter.addItem(item)) break;
				added += 1;
			}
			if (added == quantity) {
				Game.GameData.stats.money_spent_at_marts += price;
				Game.GameData.stats.mart_items_bought += quantity;
				@adapter.setMoney(@adapter.getMoney - price);
				@stock.delete_if(itm => GameData.Item.get(itm).is_important() && Game.GameData.bag.has(itm));
				DisplayPaused(_INTL("Here you are! Thank you!")) { SEPlay("Mart buy item") };
				if (quantity >= 10 && GameData.Items.exists(Items.PREMIERBALL)) {
					if (Settings.MORE_BONUS_PREMIER_BALLS && GameData.Item.get(item).is_poke_ball()) {
						premier_balls_added = 0;
						(quantity / 10).times do
							if (!@adapter.addItem(:PREMIERBALL)) break;
							premier_balls_added += 1;
						}
						ball_name = GameData.Item.get(:PREMIERBALL).portion_name;
						if (premier_balls_added > 1) ball_name = GameData.Item.get(:PREMIERBALL).portion_name_plural;
						Game.GameData.stats.premier_balls_earned += premier_balls_added;
						DisplayPaused(_INTL("And have {1} {2} on the house!", premier_balls_added, ball_name));
					} else if (!Settings.MORE_BONUS_PREMIER_BALLS && GameData.Item.get(item) == :POKEBALL) {
						if (@adapter.addItem(:PREMIERBALL)) {
							ball_name = GameData.Item.get(:PREMIERBALL).name;
							Game.GameData.stats.premier_balls_earned += 1;
							DisplayPaused(_INTL("And have 1 {1} on the house!", ball_name));
						}
					}
				}
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
		@scene.EndBuyScene;
	}

	public void SellScreen() {
		item = @scene.StartSellScene(@adapter.getInventory, @adapter);
		do { //loop; while (true);
			item = @scene.ChooseSellItem;
			if (!item) break;
			itemname       = @adapter.getName(item);
			itemnameplural = @adapter.getNamePlural(item);
			if (!@adapter.canSell(item)) {
				DisplayPaused(_INTL("Oh, no. I can't buy {1}.", itemnameplural));
				continue;
			}
			price = @adapter.getPrice(item, true);
			qty = @adapter.getQuantity(item);
			if (qty == 0) continue;
			@scene.ShowMoney;
			if (qty > 1) {
				qty = @scene.ChooseNumber(
					_INTL("How many {1} would you like to sell?", itemnameplural), item, qty
				);
			}
			if (qty == 0) {
				@scene.HideMoney;
				continue;
			}
			price *= qty;
			if (Confirm(_INTL("I can pay ${1}.\nWould that be OK?", price.to_s_formatted))) {
				old_money = @adapter.getMoney;
				@adapter.setMoney(@adapter.getMoney + price);
				Game.GameData.stats.money_earned_at_marts += @adapter.getMoney - old_money;
				qty.times(() => @adapter.removeItem(item));
				sold_item_name = (qty > 1) ? itemnameplural : itemname;
				DisplayPaused(_INTL("You turned over the {1} and got ${2}.",
															sold_item_name, price.to_s_formatted)) { SEPlay("Mart buy item") };
				@scene.Refresh;
			}
			@scene.HideMoney;
		}
		@scene.EndSellScene;
	}
}

//===============================================================================
//
//===============================================================================
public void PokemonMart(stock, speech = null, cantsell = false) {
	stock.delete_if(item => GameData.Item.get(item).is_important() && Game.GameData.bag.has(item));
	commands = new List<string>();
	cmdBuy  = -1;
	cmdSell = -1;
	cmdQuit = -1;
	commands[cmdBuy = commands.length]  = _INTL("I'm here to buy");
	if (!cantsell) commands[cmdSell = commands.length] = _INTL("I'm here to sell");
	commands[cmdQuit = commands.length] = _INTL("No, thanks");
	cmd = Message(speech || _INTL("Welcome! How may I help you?"), commands, cmdQuit + 1);
	do { //loop; while (true);
		if (cmdBuy >= 0 && cmd == cmdBuy) {
			scene = new PokemonMart_Scene();
			screen = new PokemonMartScreen(scene, stock);
			screen.BuyScreen;
		} else if (cmdSell >= 0 && cmd == cmdSell) {
			scene = new PokemonMart_Scene();
			screen = new PokemonMartScreen(scene, stock);
			screen.SellScreen;
		} else {
			Message(_INTL("Do come again!"));
			break;
		}
		cmd = Message(_INTL("Is there anything else I can do for you?"), commands, cmdQuit + 1);
	}
	Game.GameData.game_temp.clear_mart_prices;
}
