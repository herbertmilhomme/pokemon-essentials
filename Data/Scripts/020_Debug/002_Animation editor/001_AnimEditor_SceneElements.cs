//===============================================================================
//
//===============================================================================
public static partial class BattleAnimationEditor {
	#region Class Functions
	#endregion

	//=============================================================================
	// Controls.
	//=============================================================================
	public partial class Window_Menu : Window_CommandPokemon {
		public void initialize(commands, x, y) {
			tempbitmap = new Bitmap(32, 32);
			w = 0;
			foreach (var i in commands) { //'commands.each' do => |i|
				width = tempbitmap.text_size(i).width;
				if (w < width) w = width;
			}
			w += 16 + self.borderX;
			super(commands, w);
			h = (int)Math.Min(commands.length * 32, 480);
			h += self.borderY;
			right = (int)Math.Min(x + w, 640);
			bottom = (int)Math.Min(y + h, 480);
			left = right - w;
			top = bottom - h;
			self.x = left;
			self.y = top;
			self.width = w;
			self.height = h;
			tempbitmap.dispose;
		}

		public void hittest() {
			mousepos = Mouse.getMousePos;
			if (!mousepos) return -1;
			toprow = self.top_row;
			for (int i = toprow; i < toprow + @item_max; i++) { //each 'toprow + @item_max' do => |i|
				rc = new Rect(0, 32 * (i - toprow), self.contents.width, 32);
				rc.x += self.x + self.leftEdge;
				rc.y += self.y + self.topEdge;
				if (rc.contains(mousepos[0], mousepos[1])) return i;
			}
			return -1;
		}
	}

	//=============================================================================
	// Clipboard.
	//=============================================================================
	public static partial class Clipboard {
		@data = null;
		@typekey = "";

		public static void data() {
			if (!@data) return null;
			return Marshal.load(@data);
		}

		public static void typekey() {
			return @typekey;
		}

		public static void setData(data, key) {
			@data = Marshal.dump(data);
			@typekey = key;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public void TrackPopupMenu(commands) {
		mousepos = Mouse.getMousePos;
		if (!mousepos) return -1;
		menuwindow = new Window_Menu(commands, mousepos[0], mousepos[1]);
		menuwindow.z = 99999;
		do { //loop; while (true);
			Graphics.update;
			Input.update;
			menuwindow.update;
			hit = menuwindow.hittest;
			if (hit >= 0) menuwindow.index = hit;
			if (Input.trigger(Input.MOUSELEFT) || Input.trigger(Input.MOUSERIGHT)) {   // Left or right button
				menuwindow.dispose;
				return hit;
			}
			if (Input.trigger(Input.USE)) {
				hit = menuwindow.index;
				menuwindow.dispose;
				return hit;
			}
			if (Input.trigger(Input.BACK)) {   // Escape
				break;
			}
		}
		menuwindow.dispose;
		return -1;
	}

	//=============================================================================
	// Sprite sheet scrolling bar.
	//=============================================================================
	public partial class AnimationWindow : Sprite {
		public int animbitmap		{ get { return _animbitmap; } }			protected int _animbitmap;
		public int start		{ get { return _start; } }			protected int _start;
		public int selected		{ get { return _selected; } }			protected int _selected;

		public const int NUMFRAMES = 5;

		public override void initialize(x, y, width, height, viewport = null) {
			base.initialize(viewport);
			@animbitmap = null;
			@arrows = new AnimatedBitmap("Graphics/UI/Debug/anim_editor_arrows");
			self.x = x;
			self.y = y;
			@start = 0;
			@selected = 0;
			@contents = new Bitmap(width, height);
			self.bitmap = @contents;
			refresh;
		}

		public int animbitmap { set {
			@animbitmap = val;
			@start = 0;
			refresh;
			}
		}

		public int selected { set {
			@selected = val;
			refresh;
			}
		}

		public override void dispose() {
			@contents.dispose;
			@arrows.dispose;
			@start = 0;
			@selected = 0;
			@changed = false;
			base.dispose();
		}

		public void drawrect(bm, x, y, width, height, color) {
			bm.fill_rect(x, y, width, 1, color);
			bm.fill_rect(x, y + height - 1, width, 1, color);
			bm.fill_rect(x, y, 1, height, color);
			bm.fill_rect(x + width - 1, y, 1, height, color);
		}

		public void drawborder(bm, x, y, width, height, color) {
			bm.fill_rect(x, y, width, 2, color);
			bm.fill_rect(x, y + height - 2, width, 2, color);
			bm.fill_rect(x, y, 2, height, color);
			bm.fill_rect(x + width - 2, y, 2, height, color);
		}

		public void refresh() {
			arrowwidth = @arrows.bitmap.width / 2;
			@contents.clear;
			@contents.fill_rect(0, 0, @contents.width, @contents.height, new Color(180, 180, 180));
			@contents.blt(0, 0, @arrows.bitmap, new Rect(0, 0, arrowwidth, 96));
			@contents.blt(arrowwidth + (NUMFRAMES * 96), 0, @arrows.bitmap,
										new Rect(arrowwidth, 0, arrowwidth, 96));
			havebitmap = (self.animbitmap && !self.animbitmap.disposed());
			if (havebitmap) {
				rect = new Rect(0, 0, 0, 0);
				rectdst = new Rect(0, 0, 0, 0);
				x = arrowwidth;
				for (int i = NUMFRAMES; i < NUMFRAMES; i++) { //for 'NUMFRAMES' times do => |i|
					j = i + @start;
					rect.set((j % 5) * 192, (j / 5) * 192, 192, 192);
					rectdst.set(x, 0, 96, 96);
					@contents.stretch_blt(rectdst, self.animbitmap, rect);
					x += 96;
				}
			}
			for (int i = NUMFRAMES; i < NUMFRAMES; i++) { //for 'NUMFRAMES' times do => |i|
				drawrect(@contents, arrowwidth + (i * 96), 0, 96, 96, new Color(100, 100, 100));
				if (@start + i == @selected && havebitmap) {
					drawborder(@contents, arrowwidth + (i * 96), 0, 96, 96, new Color(255, 0, 0));
				}
			}
		}

		public bool changed() {
			return @changed;
		}

		public void update() {
			mousepos = Mouse.getMousePos;
			@changed = false;
			if (!Input.repeat(Input.MOUSELEFT)) return;
			if (!mousepos) return;
			if (!self.animbitmap) return;
			arrowwidth = @arrows.bitmap.width / 2;
			maxindex = (self.animbitmap.height / 192) * 5;
			left = new Rect(0, 0, arrowwidth, 96);
			right = new Rect(arrowwidth + (NUMFRAMES * 96), 0, arrowwidth, 96);
			left.x += self.x;
			left.y += self.y;
			right.x += self.x;
			right.y += self.y;
			swatchrects = new List<string>();
			repeattime = Input.time(Input.MOUSELEFT);
			for (int i = NUMFRAMES; i < NUMFRAMES; i++) { //for 'NUMFRAMES' times do => |i|
				swatchrects.Add(new Rect(arrowwidth + (i * 96) + self.x, self.y, 96, 96));
			}
			for (int i = NUMFRAMES; i < NUMFRAMES; i++) { //for 'NUMFRAMES' times do => |i|
				if (!swatchrects[i].contains(mousepos[0], mousepos[1])) continue;
				@selected = @start + i;
				@changed = true;
				refresh;
				return;
			}
			// Left arrow
			if (left.contains(mousepos[0], mousepos[1])) {
				if (repeattime > 0.75) {
					@start -= 3;
				} else {
					@start -= 1;
				}
				if (@start < 0) @start = 0;
				refresh;
			}
			// Right arrow
			if (right.contains(mousepos[0], mousepos[1])) {
				if (repeattime > 0.75) {
					@start += 3;
				} else {
					@start += 1;
				}
				if (@start >= maxindex) @start = maxindex;
				refresh;
			}
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class CanvasAnimationWindow : AnimationWindow {
		public void animbitmap() {
			return @canvas.animbitmap;
		}

		public override void initialize(canvas, x, y, width, height, viewport = null) {
			@canvas = canvas;
			base.initialize(x, y, width, height, viewport);
		}
	}

	//=============================================================================
	// Cel sprite.
	//=============================================================================
	public partial class InvalidatableSprite : Sprite {
		public override void initialize(viewport = null) {
			base.initialize(viewport);
			@invalid = false;
		}

		// Marks that the control must be redrawn to reflect current logic.
		public void invalidate() {
			@invalid = true;
		}

		// Determines whether the control is invalid
		public bool invalid() {
			return @invalid;
		}

		// Marks that the control is valid.  Normally called only by repaint.
		public void validate() {
			@invalid = false;
		}

		// Redraws the sprite only if it is invalid, and then revalidates the sprite
		public void repaint() {
			if (self.invalid()) {
				refresh;
				validate;
			}
		}

		// Redraws the sprite.  This method should not check whether
		// the sprite is invalid, to allow it to be explicitly called.
		public void refresh() { }
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class SpriteFrame : InvalidatableSprite {
		public int id		{ get { return _id; } }			protected int _id;
		public int locked		{ get { return _locked; } }			protected int _locked;
		public int selected		{ get { return _selected; } }			protected int _selected;
		public int sprite		{ get { return _sprite; } }			protected int _sprite;

		NUM_ROWS = (Animation.MAX_SPRITES.to_f / 10).ceil;   // 10 frame number icons in each row

		public override void initialize(id, sprite, viewport, previous = false) {
			base.initialize(viewport);
			@id = id;
			@sprite = sprite;
			@previous = previous;
			@locked = false;
			@selected = false;
			@selcolor = Color.black;
			@unselcolor = new Color(220, 220, 220);
			@prevcolor = new Color(64, 128, 192);
			@contents = new Bitmap(64, 64);
			self.z = (@previous) ? 49 : 50;
			@iconbitmap = new AnimatedBitmap("Graphics/UI/Debug/anim_editor_frame_icons");
			self.bitmap = @contents;
			self.invalidate;
		}

		public override void dispose() {
			@contents.dispose;
			base.dispose();
		}

		public int sprite { set {
			@sprite = value;
			self.invalidate;
			}
		}

		public int locked { set {
			@locked = value;
			self.invalidate;
			}
		}

		public int selected { set {
			@selected = value;
			self.invalidate;
			}
		}

		public void refresh() {
			@contents.clear;
			self.z = (@previous) ? 49 : (@selected) ? 51 : 50;
			// Draw frame
			color = (@previous) ? @prevcolor : (@selected) ? @selcolor : @unselcolor;
			@contents.fill_rect(0, 0, 64, 1, color);
			@contents.fill_rect(0, 63, 64, 1, color);
			@contents.fill_rect(0, 0, 1, 64, color);
			@contents.fill_rect(63, 0, 1, 64, color);
			// Determine frame number graphic to use from @iconbitmap
			yoffset = (@previous) ? (NUM_ROWS + 1) * 16 : 0;   // 1 is for padlock icon
			bmrect = new Rect((@id % 10) * 16, yoffset + ((@id / 10) * 16), 16, 16);
			@contents.blt(0, 0, @iconbitmap.bitmap, bmrect);
			// Draw padlock if frame is locked
			if (@locked && !@previous) {
				bmrect = new Rect(0, NUM_ROWS * 16, 16, 16);
				@contents.blt(16, 0, @iconbitmap.bitmap, bmrect);
			}
		}
	}

	//=============================================================================
	// Canvas.
	//=============================================================================
	public partial class AnimationCanvas : Sprite {
		public int viewport		{ get { return _viewport; } }			protected int _viewport;
		public int sprites		{ get { return _sprites; } }			protected int _sprites;
		/// <summary>Currently active frame</summary>
		public int currentframe		{ get { return _currentframe; } }			protected int _currentframe;
		public int currentcel		{ get { return _currentcel; } }			protected int _currentcel;
		/// <summary>Currently selected animation</summary>
		public int animation		{ get { return _animation; } }			protected int _animation;
		/// <summary>Currently selected animation bitmap</summary>
		public int animbitmap		{ get { return _animbitmap; } }			protected int _animbitmap;
		/// <summary>Currently selected pattern</summary>
		public int pattern		{ get { return _pattern; } set { _pattern = value; } }			protected int _pattern;

		public const int BORDERSIZE = 64;

		public override void initialize(animation, viewport = null) {
			base.initialize(viewport);
			@currentframe = 0;
			@currentcel = -1;
			@pattern = 0;
			@sprites = new List<string>();
			@celsprites = new List<string>();
			@framesprites = new List<string>();
			@lastframesprites = new List<string>();
			@dirty = new List<string>();
			@viewport = viewport;
			@selecting = false;
			@selectOffsetX = 0;
			@selectOffsetY = 0;
			@playing = false;
			@playingframe = 0;
			@player = null;
			@battle = new MiniBattle();
			@user = new AnimatedBitmap("Graphics/UI/Debug/anim_editor_battler_back").deanimate;
			@target = new AnimatedBitmap("Graphics/UI/Debug/anim_editor_battler_front").deanimate;
			@testscreen = new AnimatedBitmap("Graphics/UI/Debug/anim_editor_battle_bg");
			self.bitmap = @testscreen.bitmap;
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				@lastframesprites[i] = new SpriteFrame(i, @celsprites[i], viewport, true);
				@lastframesprites[i].selected = false;
				@lastframesprites[i].visible = false;
			}
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				@celsprites[i] = new Sprite(viewport);
				@celsprites[i].visible = false;
				@celsprites[i].src_rect = new Rect(0, 0, 0, 0);
				@celsprites[i].bitmap = null;
				@framesprites[i] = new SpriteFrame(i, @celsprites[i], viewport);
				@framesprites[i].selected = false;
				@framesprites[i].visible = false;
				@dirty[i] = true;
			}
			loadAnimation(animation);
		}

		public void loadAnimation(anim) {
			@animation = anim;
			@animbitmap&.dispose;
			if (@animation.graphic == "") {
				@animbitmap = null;
			} else {
				begin;
					@animbitmap = new AnimatedBitmap("Graphics/Animations/" + @animation.graphic,
																					@animation.hue).deanimate;
				rescue;
					@animbitmap = null;
				}
			}
			@currentcel = -1;
			self.currentframe = 0;
			@selecting = false;
			@pattern = 0;
			self.invalidate;
		}

		public int animbitmap { set {
			@animbitmap&.dispose;
			@animbitmap = value;
			for (int i = 2; i < Animation.MAX_SPRITES; i++) { //each 'Animation.MAX_SPRITES' do => |i|
				if (@celsprites[i]) @celsprites[i].bitmap = @animbitmap;
			}
			self.invalidate;
			}
		}

		public void dispose() {
			@user.dispose;
			@target.dispose;
			@animbitmap&.dispose;
			@selectedbitmap&.dispose;
			@celbitmap&.dispose;
			self.bitmap&.dispose;
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				@celsprites[i]&.dispose;
			}
			super;
		}

		public void play(oppmove = false) {
			if (!@playing) {
				@sprites["pokemon_0"] = new Sprite(@viewport);
				@sprites["pokemon_0"].bitmap = @user;
				@sprites["pokemon_0"].z = 21;
				@sprites["pokemon_1"] = new Sprite(@viewport);
				@sprites["pokemon_1"].bitmap = @target;
				@sprites["pokemon_1"].z = 16;
				SpriteSetAnimFrame(@sprites["pokemon_0"],
														CreateCel(Battle.Scene.FOCUSUSER_X,
																				Battle.Scene.FOCUSUSER_Y, -1, 2),
														@sprites["pokemon_0"], @sprites["pokemon_1"]);
				SpriteSetAnimFrame(@sprites["pokemon_1"],
														CreateCel(Battle.Scene.FOCUSTARGET_X,
																				Battle.Scene.FOCUSTARGET_Y, -2, 1),
														@sprites["pokemon_0"], @sprites["pokemon_1"]);
				usersprite = @sprites[$"pokemon_{oppmove ? 1 : 0}"];
				targetsprite = @sprites[$"pokemon_{oppmove ? 0 : 1}"];
				olduserx = usersprite ? usersprite.x : 0;
				oldusery = usersprite ? usersprite.y : 0;
				oldtargetx = targetsprite ? targetsprite.x : 0;
				oldtargety = targetsprite ? targetsprite.y : 0;
				@player = new AnimationPlayerX(@animation,
																				@battle.battlers[oppmove ? 1 : 0],
																				@battle.battlers[oppmove ? 0 : 1],
																				self, oppmove, true);
				@player.setLineTransform(
					Battle.Scene.FOCUSUSER_X, Battle.Scene.FOCUSUSER_Y,
					Battle.Scene.FOCUSTARGET_X, Battle.Scene.FOCUSTARGET_Y,
					olduserx, oldusery,
					oldtargetx, oldtargety
				);
				@player.start;
				@playing = true;
				@sprites["pokemon_0"].x += BORDERSIZE;
				@sprites["pokemon_0"].y += BORDERSIZE;
				@sprites["pokemon_1"].x += BORDERSIZE;
				@sprites["pokemon_1"].y += BORDERSIZE;
				oldstate = new List<string>();
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					oldstate.Add(new {@celsprites[i].visible, @framesprites[i].visible, @lastframesprites[i].visible});
					@celsprites[i].visible = false;
					@framesprites[i].visible = false;
					@lastframesprites[i].visible = false;
				}
				do { //loop; while (true);
					Graphics.update;
					self.update;
					if (!@playing) break;
				}
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					@celsprites[i].visible = oldstate[i][0];
					@framesprites[i].visible = oldstate[i][1];
					@lastframesprites[i].visible = oldstate[i][2];
				}
				@sprites["pokemon_0"].dispose;
				@sprites["pokemon_1"].dispose;
				@player.dispose;
				@player = null;
			}
		}

		public void invalidate() {
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				invalidateCel(i);
			}
		}

		public void invalidateCel(i) {
			@dirty[i] = true;
		}

		public int currentframe { set {
			@currentframe = value;
			invalidate;
			}
		}

		public void getCurrentFrame() {
			if (@currentframe >= @animation.length) return null;
			return @animation[@currentframe];
		}

		public void setFrame(i) {
			if (@celsprites[i]) {
				@framesprites[i].ox = 32;
				@framesprites[i].oy = 32;
				@framesprites[i].selected = (i == @currentcel);
				@framesprites[i].locked = self.locked(i);
				@framesprites[i].x = @celsprites[i].x;
				@framesprites[i].y = @celsprites[i].y;
				@framesprites[i].visible = @celsprites[i].visible;
				@framesprites[i].repaint;
			}
		}

		public void setPreviousFrame(i) {
			if (@currentframe > 0) {
				cel = @animation[@currentframe - 1][i];
				if (cel.null()) {
					@lastframesprites[i].visible = false;
				} else {
					@lastframesprites[i].ox = 32;
					@lastframesprites[i].oy = 32;
					@lastframesprites[i].selected = false;
					@lastframesprites[i].locked = false;
					@lastframesprites[i].x = cel[AnimFrame.X] + 64;
					@lastframesprites[i].y = cel[AnimFrame.Y] + 64;
					@lastframesprites[i].visible = true;
					@lastframesprites[i].repaint;
				}
			} else {
				@lastframesprites[i].visible = false;
			}
		}

		public void offsetFrame(frame, ox, oy) {
			if (frame >= 0 && frame < @animation.length) {
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					if (!self.locked(i) && @animation[frame][i]) {
						@animation[frame][i][AnimFrame.X] += ox;
						@animation[frame][i][AnimFrame.Y] += oy;
					}
					if (frame == @currentframe) @dirty[i] = true;
				}
			}
		}

		// Clears all items in the frame except locked items.
		public void clearFrame(frame) {
			if (frame >= 0 && frame < @animation.length) {
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					if (self.deletable(i)) {
						@animation[frame][i] = null;
					} else {
						ResetCel(@animation[frame][i]);
					}
					if (frame == @currentframe) @dirty[i] = true;
				}
			}
		}

		public void insertFrame(frame) {
			if (frame >= @animation.length) return;
			@animation.insert(frame, @animation[frame].clone);
			self.invalidate;
		}

		public void copyFrame(src, dst) {
			if (dst >= @animation.length) return;
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				clonedframe = @animation[src][i];
				if (clonedframe && clonedframe != true) clonedframe = clonedframe.clone;
				@animation[dst][i] = clonedframe;
			}
			if (dst == @currentframe) self.invalidate;
		}

		public void pasteFrame(frame) {
			if (frame < 0 || frame >= @animation.length) return;
			if (Clipboard.typekey != "AnimFrame") return;
			@animation[frame] = Clipboard.data;
			if (frame == @currentframe) self.invalidate;
		}

		public void deleteFrame(frame) {
			if (frame < 0 || frame >= @animation.length || @animation.length <= 1) return;
			if (frame == @animation.length - 1) self.currentframe -= 1;
			@animation.delete_at(frame);
			@currentcel = -1;
			self.invalidate;
		}

		// This frame becomes a copy of the previous frame.
		public void pasteLast() {
			if (@currentframe > 0) copyFrame(@currentframe - 1, @currentframe);
		}

		public void currentCel() {
			if (@currentcel < 0) return null;
			if (@currentframe >= @animation.length) return null;
			return @animation[@currentframe][@currentcel];
		}

		public void pasteCel(x, y) {
			if (@currentframe >= @animation.length) return;
			if (Clipboard.typekey != "AnimCel") return;
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				if (@animation[@currentframe][i]) continue;
				@animation[@currentframe][i] = Clipboard.data;
				cel = @animation[@currentframe][i];
				cel[AnimFrame.X] = x;
				cel[AnimFrame.Y] = y;
				cel[AnimFrame.LOCKED] = 0;
				if (cel[AnimFrame.PATTERN] == -1) @celsprites[i].bitmap = @user;
				if (cel[AnimFrame.PATTERN] == -2) @celsprites[i].bitmap = @target;
				@currentcel = i;
				break;
			}
			invalidate;
		}

		public void deleteCel(cel) {
			if (cel < 0) return;
			if (@currentframe < 0 || @currentframe >= @animation.length) return;
			if (!deletable(cel)) return;
			@animation[@currentframe][cel] = null;
			@dirty[cel] = true;
		}

		public void swapCels(cel1, cel2) {
			if (cel1 < 0 || cel2 < 0) return;
			if (@currentframe < 0 || @currentframe >= @animation.length) return;
			t = @animation[@currentframe][cel1];
			@animation[@currentframe][cel1] = @animation[@currentframe][cel2];
			@animation[@currentframe][cel2] = t;
			@currentcel = cel2;
			@dirty[cel1] = true;
			@dirty[cel2] = true;
		}

		public bool locked(celindex) {
			cel = @animation[self.currentframe];
			if (!cel) return false;
			cel = cel[celindex];
			return cel ? (cel[AnimFrame.LOCKED] != 0) : false;
		}

		public bool deletable(celindex) {
			cel = @animation[self.currentframe];
			if (!cel) return true;
			cel = cel[celindex];
			if (!cel) return true;
			if (cel[AnimFrame.LOCKED] != 0) return false;
			if (cel[AnimFrame.PATTERN] < 0) {
				count = 0;
				pattern = cel[AnimFrame.PATTERN];
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					othercel = @animation[self.currentframe][i];
					if (othercel && othercel[AnimFrame.PATTERN] == pattern) count += 1;
				}
				if (count <= 1) return false;
			}
			return true;
		}

		public void setBitmap(i, frame) {
			if (@celsprites[i]) {
				cel = @animation[frame][i];
				@celsprites[i].bitmap = @animbitmap;
				if (cel) {
					if (cel[AnimFrame.PATTERN] == -1) @celsprites[i].bitmap = @user;
					if (cel[AnimFrame.PATTERN] == -2) @celsprites[i].bitmap = @target;
				}
			}
		}

		public void setSpriteBitmap(sprite, cel) {
			if (sprite && !sprite.disposed()) {
				sprite.bitmap = @animbitmap;
				if (cel) {
					if (cel[AnimFrame.PATTERN] == -1) sprite.bitmap = @user;
					if (cel[AnimFrame.PATTERN] == -2) sprite.bitmap = @target;
				}
			}
		}

		public void addSprite(x, y) {
			if (@currentframe >= @animation.length) return false;
			for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
				if (@animation[@currentframe][i]) continue;
				@animation[@currentframe][i] = CreateCel(x, y, @pattern, @animation.position);
				@dirty[i] = true;
				@currentcel = i;
				return true;
			}
			return false;
		}

		public void SpriteHitTest(sprite, x, y, usealpha = true, wholecanvas = false) {
			if (!sprite || sprite.disposed()) return false;
			if (!sprite.bitmap) return false;
			if (!sprite.visible) return false;
			if (sprite.bitmap.disposed()) return false;
			width = sprite.src_rect.width;
			height = sprite.src_rect.height;
			if (wholecanvas) {
				xwidth = 0;
				xheight = 0;
			} else {
				xwidth = width - 64;
				xheight = height - 64;
				if (width > 64 && !usealpha) width = 64;
				if (height > 64 && !usealpha) height = 64;
			}
			if (width > sprite.bitmap.width) width = sprite.bitmap.width;
			if (height > sprite.bitmap.height) height = sprite.bitmap.height;
			if (usealpha) {
				spritex = sprite.x - (sprite.ox * sprite.zoom_x);
				spritey = sprite.y - (sprite.oy * sprite.zoom_y);
				width *= sprite.zoom_x;
				height *= sprite.zoom_y;
			} else {
				spritex = sprite.x - sprite.ox;
				spritey = sprite.y - sprite.oy;
				spritex += xwidth / 2;
				spritey += xheight / 2;
			}
			if (!(x >= spritex && x <= spritex + width && y >= spritey && y <= spritey + height)) {
				return false;
			}
			if (usealpha) {
				bitmapX = sprite.src_rect.x;
				bitmapY = sprite.src_rect.y;
				bitmapX += sprite.ox;
				bitmapY += sprite.oy;
				if (sprite.zoom_x > 0) bitmapX += (x - sprite.x) / sprite.zoom_x;
				if (sprite.zoom_y > 0) bitmapY += (y - sprite.y) / sprite.zoom_y;
				bitmapX = bitmapX.round;
				bitmapY = bitmapY.round;
				if (sprite.mirror) {
					xmirror = bitmapX - sprite.src_rect.x;
					bitmapX = sprite.src_rect.x + 192 - xmirror;
				}
				color = sprite.bitmap.get_pixel(bitmapX, bitmapY);
				if (color.alpha == 0) return false;
			}
			return true;
		}

		public void updateInput() {
			cel = currentCel;
			mousepos = Mouse.getMousePos;
			if (Input.trigger(Input.MOUSELEFT) && mousepos &&
				SpriteHitTest(self, mousepos[0], mousepos[1], false, true)) {
				selectedcel = -1;
				usealpha = Input.press(Input.ALT);
				for (int j = Animation.MAX_SPRITES; j < Animation.MAX_SPRITES; j++) { //for 'Animation.MAX_SPRITES' times do => |j|
					if (SpriteHitTest(@celsprites[j], mousepos[0], mousepos[1], usealpha, false)) {
						selectedcel = j;
					}
				}
				if (selectedcel < 0) {
					if (@animbitmap && addSprite(mousepos[0] - BORDERSIZE, mousepos[1] - BORDERSIZE)) {
						if (!self.locked(@currentcel)) @selecting = true;
						@selectOffsetX = 0;
						@selectOffsetY = 0;
						cel = currentCel;
						invalidate;
					}
				} else {
					@currentcel = selectedcel;
					if (!self.locked(@currentcel)) @selecting = true;
					cel = currentCel;
					@selectOffsetX = cel[AnimFrame.X] - mousepos[0] + BORDERSIZE;
					@selectOffsetY = cel[AnimFrame.Y] - mousepos[1] + BORDERSIZE;
					invalidate;
				}
			}
			currentFrame = getCurrentFrame;
			if (currentFrame && !@selecting &&
				(Input.triggerex(:TAB) || Input.repeatex(:TAB))) {
				currentFrame.length.times do;
					@currentcel += 1;
					if (@currentcel >= currentFrame.length) @currentcel = 0;
					if (currentFrame[@currentcel]) break;
				}
				invalidate;
				return;
			}
			if (cel && @selecting && mousepos) {
				cel[AnimFrame.X] = mousepos[0] - BORDERSIZE + @selectOffsetX;
				cel[AnimFrame.Y] = mousepos[1] - BORDERSIZE + @selectOffsetY;
				@dirty[@currentcel] = true;
			}
			if (!Input.press(Input.MOUSELEFT) && @selecting) {
				@selecting = false;
			}
			if (cel) {
				if ((Input.triggerex(:DELETE) || Input.repeatex(:DELETE)) && self.deletable(@currentcel)) {
					@animation[@currentframe][@currentcel] = null;
					@dirty[@currentcel] = true;
					return;
				}
				if (Input.triggerex(:P) || Input.repeatex(:P)) {   // Properties
					BattleAnimationEditor.CellProperties(self);
					@dirty[@currentcel] = true;
					return;
				}
				if (Input.triggerex(:L) || Input.repeatex(:L)) {   // Lock
					cel[AnimFrame.LOCKED] = (cel[AnimFrame.LOCKED] == 0) ? 1 : 0;
					@dirty[@currentcel] = true;
				}
				if (Input.triggerex(:R) || Input.repeatex(:R)) {   // Rotate right
					cel[AnimFrame.ANGLE] += 10;
					cel[AnimFrame.ANGLE] %= 360;
					@dirty[@currentcel] = true;
				}
				if (Input.triggerex(:E) || Input.repeatex(:E)) {   // Rotate left
					cel[AnimFrame.ANGLE] -= 10;
					cel[AnimFrame.ANGLE] %= 360;
					@dirty[@currentcel] = true;
				}
				if (Input.triggerex(:KP_PLUS) || Input.repeatex(:KP_PLUS)) {   // Zoom in
					cel[AnimFrame.ZOOMX] += 10;
					if (cel[AnimFrame.ZOOMX] > 1000) cel[AnimFrame.ZOOMX] = 1000;
					cel[AnimFrame.ZOOMY] += 10;
					if (cel[AnimFrame.ZOOMY] > 1000) cel[AnimFrame.ZOOMY] = 1000;
					@dirty[@currentcel] = true;
				}
				if (Input.triggerex(:KP_MINUS) || Input.repeatex(:KP_MINUS)) {   // Zoom out
					cel[AnimFrame.ZOOMX] -= 10;
					if (cel[AnimFrame.ZOOMX] < 10) cel[AnimFrame.ZOOMX] = 10;
					cel[AnimFrame.ZOOMY] -= 10;
					if (cel[AnimFrame.ZOOMY] < 10) cel[AnimFrame.ZOOMY] = 10;
					@dirty[@currentcel] = true;
				}
				if (!self.locked(@currentcel)) {
					if (Input.trigger(Input.UP) || Input.repeat(Input.UP)) {
						increment = (Input.press(Input.ALT)) ? 1 : 8;
						cel[AnimFrame.Y] -= increment;
						@dirty[@currentcel] = true;
					}
					if (Input.trigger(Input.DOWN) || Input.repeat(Input.DOWN)) {
						increment = (Input.press(Input.ALT)) ? 1 : 8;
						cel[AnimFrame.Y] += increment;
						@dirty[@currentcel] = true;
					}
					if (Input.trigger(Input.LEFT) || Input.repeat(Input.LEFT)) {
						increment = (Input.press(Input.ALT)) ? 1 : 8;
						cel[AnimFrame.X] -= increment;
						@dirty[@currentcel] = true;
					}
					if (Input.trigger(Input.RIGHT) || Input.repeat(Input.RIGHT)) {
						increment = (Input.press(Input.ALT)) ? 1 : 8;
						cel[AnimFrame.X] += increment;
						@dirty[@currentcel] = true;
					}
				}
			}
		}

		public override void update() {
			base.update();
			if (@playing) {
				if (@player.animDone()) {
					@playing = false;
					invalidate;
				} else {
					@player.update;
				}
				return;
			}
			updateInput;
	//    @testscreen.update
	//    self.bitmap = @testscreen.bitmap
			if (@currentframe < @animation.length) {
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					if (!@dirty[i]) continue;
					if (@celsprites[i]) {
						setBitmap(i, @currentframe);
						SpriteSetAnimFrame(@celsprites[i], @animation[@currentframe][i], @celsprites[0], @celsprites[1], true);
						@celsprites[i].x += BORDERSIZE;
						@celsprites[i].y += BORDERSIZE;
					}
					setPreviousFrame(i);
					setFrame(i);
					@dirty[i] = false;
				}
			} else {
				for (int i = Animation.MAX_SPRITES; i < Animation.MAX_SPRITES; i++) { //for 'Animation.MAX_SPRITES' times do => |i|
					SpriteSetAnimFrame(@celsprites[i], null, @celsprites[0], @celsprites[1], true);
					@celsprites[i].x += BORDERSIZE;
					@celsprites[i].y += BORDERSIZE;
					setPreviousFrame(i);
					setFrame(i);
					@dirty[i] = false;
				}
			}
		}
	}

	//=============================================================================
	// Window classes.
	//=============================================================================
	public partial class BitmapDisplayWindow : SpriteWindow_Base {
		public int bitmapname		{ get { return _bitmapname; } }			protected int _bitmapname;
		public int hue		{ get { return _hue; } }			protected int _hue;

		public override void initialize(x, y, width, height) {
			base.initialize(x, y, width, height);
			@bitmapname = "";
			@hue = 0;
			self.contents = new Bitmap(width - 32, height - 32);
		}

		public int bitmapname { set {
			if (@bitmapname != value) {
				@bitmapname = value;
				refresh;
			}
			}
		}

		public int hue { set {
			if (@hue != value) {
				@hue = value;
				refresh;
			}
			}
		}

		public void refresh() {
			self.contents.clear;
			bmap = new AnimatedBitmap("Graphics/Animations/" + @bitmapname, @hue).deanimate;
			if (!bmap) return;
			ww = bmap.width;
			wh = bmap.height;
			sx = self.contents.width / ww.to_f;
			sy = self.contents.height / wh.to_f;
			if (sx > sy) {
				ww = sy * ww;
				wh = self.contents.height;
			} else {
				wh = sx * wh;
				ww = self.contents.width;
			}
			dest = new Rect((self.contents.width - ww) / 2,
											(self.contents.height - wh) / 2,
											ww, wh);
			src = new Rect(0, 0, bmap.width, bmap.height);
			self.contents.stretch_blt(dest, bmap, src);
			bmap.dispose;
		}
	}

	//=============================================================================
	//
	//=============================================================================
	public partial class AnimationNameWindow {
		public void initialize(canvas, x, y, width, height, viewport = null) {
			@canvas = canvas;
			@oldname = null;
			@window = Window_UnformattedTextPokemon.newWithSize(
				_INTL("Name: {1}", @canvas.animation.name), x, y, width, height, viewport
			);
		}

		public int viewport { set { @window.viewport = value; } }

		public void update() {
			newtext = _INTL("Name: {1}", @canvas.animation.name);
			if (@oldname != newtext) {
				@window.text = newtext;
				@oldname = newtext;
			}
			@window.update;
		}

		public int refresh { get { return @window.refresh; } }
		public int dispose { get { return @window.dispose; } }
		public int disposed { get { return @window.disposed(); } }
	}
}
