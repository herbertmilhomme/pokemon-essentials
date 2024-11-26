//===============================================================================
//
//===============================================================================
public partial class Plane {
	public void update() { }
	public void refresh() { }
}

//===============================================================================
// A plane class that displays a single color.
//===============================================================================
public partial class ColoredPlane : Plane {
	public override void initialize(color, viewport = null) {
		base.initialize(viewport);
		self.bitmap = new Bitmap(32, 32);
		set_plane_color(color);
	}

	public override void dispose() {
		self.bitmap&.dispose;
		base.dispose();
	}

	public void set_plane_color(value) {
		self.bitmap.fill_rect(0, 0, self.bitmap.width, self.bitmap.height, value);
		refresh;
	}
}

//===============================================================================
// A plane class that supports animated images.
//===============================================================================
public partial class AnimatedPlane : Plane {
	public override void initialize(viewport) {
		base.initialize(viewport);
		@bitmap = null;
	}

	public override void dispose() {
		clear_bitmap;
		base.dispose();
	}

	public void setBitmap(file, hue = 0) {
		clear_bitmap;
		if (file.null()) return;
		@bitmap = new AnimatedBitmap(file, hue);
		if (@bitmap) self.bitmap = @bitmap.bitmap;
	}

	public void set_panorama(file, hue = 0) {
		if (file.is_a(String) && file.length > 0) {
			setBitmap("Graphics/Panoramas/" + file, hue);
		} else {
			clear_bitmap;
		}
	}

	public void set_fog(file, hue = 0) {
		if (file.is_a(String) && file.length > 0) {
			setBitmap("Graphics/Fogs/" + file, hue);
		} else {
			clear_bitmap;
		}
	}

	//-----------------------------------------------------------------------------

	private;

	public void clear_bitmap() {
		@bitmap&.dispose;
		@bitmap = null;
		if (!self.disposed()) self.bitmap = null;
	}
}
