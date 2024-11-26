//===============================================================================
//
//===============================================================================
public partial class PictureSprite : Sprite {
	public override void initialize(viewport, picture) {
		base.initialize(viewport);
		@picture = picture;
		@pictureBitmap = null;
		@customBitmap = null;
		@customBitmapIsBitmap = true;
		@hue = 0;
		update;
	}

	public override void dispose() {
		@pictureBitmap&.dispose;
		base.dispose();
	}

	// Doesn't free the bitmap
	public void setCustomBitmap(bitmap) {
		@customBitmap = bitmap;
		@customBitmapIsBitmap = @customBitmap.is_a(Bitmap);
	}

	public override void update() {
		base.update();
		@pictureBitmap&.update;
		// If picture file name is different from current one
		if (@customBitmap && @picture.name == "") {
			self.bitmap = (@customBitmapIsBitmap) ? @customBitmap : @customBitmap.bitmap;
		} else if (@picture_name != @picture.name || @picture.hue.ToInt() != @hue.ToInt()) {
			// Remember file name to instance variables
			@picture_name = @picture.name;
			@hue = @picture.hue.ToInt();
			// If file name is not empty
			if (@picture_name == "") {
				@pictureBitmap&.dispose;
				@pictureBitmap = null;
				self.visible = false;
				return;
			}
			// Get picture graphic
			@pictureBitmap&.dispose;
			@pictureBitmap = new AnimatedBitmap(@picture_name, @hue);
			self.bitmap = (@pictureBitmap) ? @pictureBitmap.bitmap : null;
		} else if (@picture_name == "") {
			// Set sprite to invisible
			self.visible = false;
			return;
		}
		setPictureSprite(self, @picture);
	}
}

public void TextBitmap(text, maxwidth = Graphics.width) {
	tmp = new Bitmap(maxwidth, Graphics.height);
	SetSystemFont(tmp);
	drawFormattedTextEx(tmp, 0, 4, maxwidth, text, new Color(248, 248, 248), new Color(168, 184, 184));
	return tmp;
}

//===============================================================================
//
//===============================================================================
public partial class EventScene {
	public int onCTrigger		{ get { return _onCTrigger; } set { _onCTrigger = value; } }			protected int _onCTrigger;
	public int onBTrigger		{ get { return _onBTrigger; } set { _onBTrigger = value; } }			protected int _onBTrigger;
	public int onUpdate			{ get { return _onUpdate; } set { _onUpdate = value; } }			protected int _onUpdate;

	public void initialize(viewport = null) {
		@viewport       = viewport;
		@onCTrigger     = new Event();
		@onBTrigger     = new Event();
		@onUpdate       = new Event();
		@pictures       = new List<string>();
		@picturesprites = new List<string>();
		@usersprites    = new List<string>();
		@disposed       = false;
	}

	public void dispose() {
		if (disposed()) return;
		@picturesprites.each(sprite => sprite.dispose);
		@usersprites.each(sprite => sprite.dispose);
		@onCTrigger.clear;
		@onBTrigger.clear;
		@onUpdate.clear;
		@pictures.clear;
		@picturesprites.clear;
		@usersprites.clear;
		@disposed = true;
	}

	public bool disposed() {
		return @disposed;
	}

	public void addBitmap(x, y, bitmap) {
		// _bitmap_ can be a Bitmap or an AnimatedBitmap
		// (update method isn't called if it's animated)
		// EventScene doesn't take ownership of the passed-in bitmap
		num = @pictures.length;
		picture = new PictureEx(num);
		picture.setXY(0, x, y);
		picture.setVisible(0, true);
		@pictures[num] = picture;
		@picturesprites[num] = new PictureSprite(@viewport, picture);
		@picturesprites[num].setCustomBitmap(bitmap);
		return picture;
	}

	public void addLabel(x, y, width, text) {
		addBitmap(x, y, TextBitmap(text, width));
	}

	public void addImage(x, y, name) {
		num = @pictures.length;
		picture = new PictureEx(num);
		picture.name = name;
		picture.setXY(0, x, y);
		picture.setVisible(0, true);
		@pictures[num] = picture;
		@picturesprites[num] = new PictureSprite(@viewport, picture);
		return picture;
	}

	public void addUserSprite(sprite) {
		@usersprites.Add(sprite);
	}

	public void getPicture(num) {
		return @pictures[num];
	}

	// ticks is in 1/20ths of a second.
	public void wait(ticks) {
		if (ticks <= 0) return;
		timer_start = System.uptime;
		do { //loop; while (true);
			update;
			if (System.uptime - timer_start >= ticks / 20.0) break;
		}
	}

	// extra_ticks is in 1/20ths of a second.
	public void pictureWait(extra_ticks = 0) {
		do { //loop; while (true);
			hasRunning = false;
			@pictures.each(pic => { if (pic.running()) hasRunning = true; });
			if (!hasRunning) break;
			update;
		}
		wait(extra_ticks);
	}

	public void update() {
		if (disposed()) return;
		Graphics.update;
		Input.update;
		@pictures.each(picture => picture.update);
		@picturesprites.each(sprite => sprite.update);
		@usersprites.each do |sprite|
			if (!sprite || sprite.disposed() || !sprite.is_a(Sprite)) continue;
			sprite.update;
		}
		@usersprites.delete_if(sprite => sprite.disposed());
		@onUpdate.trigger(self);
		if (Input.trigger(Input.BACK)) {
			@onBTrigger.trigger(self);
		} else if (Input.trigger(Input.USE)) {
			@onCTrigger.trigger(self);
		}
	}

	public void main() {
		do { //loop; while (true);
			update;
			if (disposed()) break;
		}
	}
}

//===============================================================================
//
//===============================================================================
public void EventScreen(cls) {
	FadeOutIn do;
		viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
		viewport.z = 99999;
		Debug.logonerr(() => new cls(viewport).main);
		viewport.dispose;
	}
}
