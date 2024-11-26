//===============================================================================
//
//===============================================================================
public partial class Window_PokemonItemStorage : Window_DrawableCommand {
	public int bag		{ get { return _bag; } }			protected int _bag;
	public int pocket		{ get { return _pocket; } }			protected int _pocket;
	public int sortIndex		{ get { return _sortIndex; } }			protected int _sortIndex;

	public int sortIndex { set {
		@sortIndex = value;
		refresh;
		}
	}

	public override void initialize(bag, x, y, width, height) {
		@bag = bag;
		@sortIndex = -1;
		@adapter = new PokemonMartAdapter();
		base.initialize(x, y, width, height);
		self.windowskin = null;
	}

	public void item() {
		item = @bag[self.index];
		return item ? item[0] : null;
	}

	public void itemCount() {
		return @bag.length + 1;
	}

	public void drawItem(index, _count, rect) {
		rect = drawCursor(index, rect);
		textpos = new List<string>();
		if (index == @bag.length) {
			textpos.Add(new {_INTL("CANCEL"), rect.x, rect.y, :left, self.baseColor, self.shadowColor});
		} else {
			item     = @bag[index][0];
			itemname = @adapter.getDisplayName(item);
			baseColor = (index == @sortIndex) ? new Color(248, 24, 24) : self.baseColor;
			textpos.Add(new {itemname, rect.x, rect.y, :left, self.baseColor, self.shadowColor});
			if (GameData.Item.get(item).show_quantity()) {
				qty     = string.Format("x{1: 2d}", @bag[index][1]);
				sizeQty = self.contents.text_size(qty).width;
				xQty = rect.x + rect.width - sizeQty - 2;
				textpos.Add(new {qty, xQty, rect.y, :left, baseColor, self.shadowColor});
			}
		}
		DrawTextPositions(self.contents, textpos);
	}
}

//===============================================================================
//
//===============================================================================
public partial class ItemStorage_Scene {
	ITEMLISTBASECOLOR   = new Color(88, 88, 80);
	ITEMLISTSHADOWCOLOR = new Color(168, 184, 184);
	ITEMTEXTBASECOLOR   = new Color(248, 248, 248);
	ITEMTEXTSHADOWCOLOR = new Color(0, 0, 0);
	TITLEBASECOLOR      = new Color(248, 248, 248);
	TITLESHADOWCOLOR    = new Color(0, 0, 0);
	public const int ITEMSVISIBLE        = 7;

	public void initialize(title) {
		@title = title;
	}

	public void update() {
		UpdateSpriteHash(@sprites);
	}

	public void StartScene(bag) {
		@viewport   = new Viewport(0, 0, Graphics.width, Graphics.height);
		@viewport.z = 99999;
		@bag = bag;
		@sprites = new List<string>();
		@sprites["background"] = new IconSprite(0, 0, @viewport);
		@sprites["background"].setBitmap("Graphics/UI/itemstorage_bg");
		@sprites["icon"] = new ItemIconSprite(50, 334, null, @viewport);
		// Item list
		@sprites["itemwindow"] = new Window_PokemonItemStorage(@bag, 98, 14, 334, 32 + (ITEMSVISIBLE * 32));
		@sprites["itemwindow"].viewport    = @viewport;
		@sprites["itemwindow"].index       = 0;
		@sprites["itemwindow"].baseColor   = ITEMLISTBASECOLOR;
		@sprites["itemwindow"].shadowColor = ITEMLISTSHADOWCOLOR;
		@sprites["itemwindow"].refresh;
		// Title
		@sprites["pocketwindow"] = new BitmapSprite(88, 64, @viewport);
		@sprites["pocketwindow"].x = 14;
		@sprites["pocketwindow"].y = 16;
		SetNarrowFont(@sprites["pocketwindow"].bitmap);
		// Item description
		@sprites["itemtextwindow"] = Window_UnformattedTextPokemon.newWithSize("", 84, 272, Graphics.width - 84, 128, @viewport);
		@sprites["itemtextwindow"].baseColor   = ITEMTEXTBASECOLOR;
		@sprites["itemtextwindow"].shadowColor = ITEMTEXTSHADOWCOLOR;
		@sprites["itemtextwindow"].windowskin  = null;
		@sprites["helpwindow"] = new Window_UnformattedTextPokemon("");
		@sprites["helpwindow"].visible  = false;
		@sprites["helpwindow"].viewport = @viewport;
		// Letter-by-letter message window
		@sprites["msgwindow"] = new Window_AdvancedTextPokemon("");
		@sprites["msgwindow"].visible  = false;
		@sprites["msgwindow"].viewport = @viewport;
		BottomLeftLines(@sprites["helpwindow"], 1);
		DeactivateWindows(@sprites);
		Refresh;
		FadeInAndShow(@sprites);
	}

	public void EndScene() {
		FadeOutAndHide(@sprites);
		DisposeSpriteHash(@sprites);
		@viewport.dispose;
	}

	public void ChooseNumber(helptext, maximum) {
		return UIHelper.ChooseNumber(@sprites["helpwindow"], helptext, maximum) { update };
	}

	public void Display(msg, brief = false) {
		UIHelper.Display(@sprites["msgwindow"], msg, brief) { update };
	}

	public void Confirm(msg) {
		UIHelper.Confirm(@sprites["msgwindow"], msg) { update };
	}

	public void ShowCommands(helptext, commands) {
		return UIHelper.ShowCommands(@sprites["helpwindow"], helptext, commands) { update };
	}

	public void Refresh() {
		bm = @sprites["pocketwindow"].bitmap;
		// Draw title at upper left corner ("Toss Item/Withdraw Item")
		drawTextEx(bm, 0, 8, bm.width, 2, @title, TITLEBASECOLOR, TITLESHADOWCOLOR);
		itemwindow = @sprites["itemwindow"];
		// Draw item icon
		@sprites["icon"].item = itemwindow.item;
		// Get item description
		if (itemwindow.item) {
			@sprites["itemtextwindow"].text = GameData.Item.get(itemwindow.item).description;
		} else {
			@sprites["itemtextwindow"].text = _INTL("Close storage.");
		}
		itemwindow.refresh;
	}

	public void ChooseItem() {
		Refresh;
		@sprites["helpwindow"].visible = false;
		itemwindow = @sprites["itemwindow"];
		itemwindow.refresh;
		ActivateWindow(@sprites, "itemwindow") do;
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				olditem = itemwindow.item;
				self.update;
				if (itemwindow.item != olditem) Refresh;
				if (Input.trigger(Input.BACK)) {
					return null;
				} else if (Input.trigger(Input.USE)) {
					if (itemwindow.index < @bag.length) {
						Refresh;
						return @bag[itemwindow.index][0];
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
public partial class WithdrawItemScene : ItemStorage_Scene {
	public override void initialize() {
		base.initialize(_INTL("Withdraw\nItem"));
	}
}

//===============================================================================
//
//===============================================================================
public partial class TossItemScene : ItemStorage_Scene {
	public override void initialize() {
		base.initialize(_INTL("Toss\nItem"));
	}
}

//===============================================================================
// Common UI functions used in both the Bag and item storage screens.
// Displays messages and allows the user to choose a number/command.
// The window _helpwindow_ will display the _helptext_.
//===============================================================================
public static partial class UIHelper {
	#region Class Functions
	#endregion

	// Letter by letter display of the message _msg_ by the window _helpwindow_.
	public void Display(helpwindow, msg, brief) {
		cw = helpwindow;
		oldvisible = cw.visible;
		cw.letterbyletter = true;
		cw.text           = msg + "\1";
		cw.visible        = true;
		BottomLeftLines(cw, 2);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			(block_given()) ? yield : cw.update
			if (!cw.busy() && (brief || (Input.trigger(Input.USE) && cw.resume))) {
				break;
			}
		}
		cw.visible = oldvisible;
	}

	public void DisplayStatic(msgwindow, message) {
		oldvisible = msgwindow.visible;
		msgwindow.visible        = true;
		msgwindow.letterbyletter = false;
		msgwindow.width          = Graphics.width;
		msgwindow.resizeHeightToFit(message, Graphics.width);
		msgwindow.text = message;
		BottomRight(msgwindow);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			(block_given()) ? yield : msgwindow.update
			if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) {
				break;
			}
		}
		msgwindow.visible = oldvisible;
		Input.update;
	}

	// Letter by letter display of the message _msg_ by the window _helpwindow_,
	// used to ask questions.  Returns true if the user chose yes, false if no.
	public void Confirm(helpwindow, msg) {
		dw = helpwindow;
		oldvisible = dw.visible;
		dw.letterbyletter = true;
		dw.text           = msg;
		dw.visible        = true;
		BottomLeftLines(dw, 2);
		commands = new {_INTL("Yes"), _INTL("No")};
		cw = new Window_CommandPokemon(commands);
		cw.index = 0;
		cw.viewport = helpwindow.viewport;
		BottomRight(cw);
		cw.y -= dw.height;
		ret = false;
		do { //loop; while (true);
			cw.visible = (!dw.busy());
			Graphics.update;
			Input.update;
			cw.update;
			(block_given()) ? yield : dw.update
			if (!dw.busy() && dw.resume) {
				if (Input.trigger(Input.BACK)) {
					PlayCancelSE
					break;
				} else if (Input.trigger(Input.USE)) {
					PlayDecisionSE;
					ret = (cw.index == 0);
					break;
				}
			}
		}
		cw.dispose;
		dw.visible = oldvisible;
		return ret;
	}

	public void ChooseNumber(helpwindow, helptext, maximum, initnum = 1) {
		oldvisible = helpwindow.visible;
		helpwindow.visible        = true;
		helpwindow.text           = helptext;
		helpwindow.letterbyletter = false;
		curnumber = initnum;
		ret = 0;
		numwindow = new Window_UnformattedTextPokemon("x000");
		numwindow.viewport       = helpwindow.viewport;
		numwindow.letterbyletter = false;
		numwindow.text           = string.Format("x{1:03d}", curnumber);
		numwindow.resizeToFit(numwindow.text, Graphics.width);
		BottomRight(numwindow);
		helpwindow.resizeHeightToFit(helpwindow.text, Graphics.width - numwindow.width);
		BottomLeft(helpwindow);
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			numwindow.update;
			helpwindow.update;
			if (block_given()) yield;
			oldnumber = curnumber;
			if (Input.trigger(Input.BACK)) {
				ret = 0;
				PlayCancelSE
				break;
			} else if (Input.trigger(Input.USE)) {
				ret = curnumber;
				PlayDecisionSE;
				break;
			} else if (Input.repeat(Input.UP)) {
				curnumber += 1;
				if (curnumber > maximum) curnumber = 1;
				if (curnumber != oldnumber) {
					numwindow.text = string.Format("x{1:03d}", curnumber);
					PlayCursorSE;
				}
			} else if (Input.repeat(Input.DOWN)) {
				curnumber -= 1;
				if (curnumber < 1) curnumber = maximum;
				if (curnumber != oldnumber) {
					numwindow.text = string.Format("x{1:03d}", curnumber);
					PlayCursorSE;
				}
			} else if (Input.repeat(Input.LEFT)) {
				curnumber -= 10;
				if (curnumber < 1) curnumber = 1;
				if (curnumber != oldnumber) {
					numwindow.text = string.Format("x{1:03d}", curnumber);
					PlayCursorSE;
				}
			} else if (Input.repeat(Input.RIGHT)) {
				curnumber += 10;
				if (curnumber > maximum) curnumber = maximum;
				if (curnumber != oldnumber) {
					numwindow.text = string.Format("x{1:03d}", curnumber);
					PlayCursorSE;
				}
			}
		}
		numwindow.dispose;
		helpwindow.visible = oldvisible;
		return ret;
	}

	public void ShowCommands(helpwindow, helptext, commands, initcmd = 0) {
		ret = -1;
		oldvisible = helpwindow.visible;
		helpwindow.visible        = helptext ? true : false;
		helpwindow.letterbyletter = false;
		helpwindow.text           = helptext || "";
		cmdwindow = new Window_CommandPokemon(commands);
		cmdwindow.index = initcmd;
		begin;
			cmdwindow.viewport = helpwindow.viewport;
			BottomRight(cmdwindow);
			helpwindow.resizeHeightToFit(helpwindow.text, Graphics.width - cmdwindow.width);
			BottomLeft(helpwindow);
			do { //loop; while (true);
				Graphics.update;
				Input.update;
				yield;
				cmdwindow.update;
				if (Input.trigger(Input.BACK)) {
					ret = -1;
					PlayCancelSE
					break;
				}
				if (Input.trigger(Input.USE)) {
					ret = cmdwindow.index;
					PlayDecisionSE;
					break;
				}
			}
		ensure;
			cmdwindow&.dispose;
		}
		helpwindow.visible = oldvisible;
		return ret;
	}
}
