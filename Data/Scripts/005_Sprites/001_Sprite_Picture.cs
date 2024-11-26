//===============================================================================
//
//===============================================================================
public partial class Sprite_Picture {
	public void initialize(viewport, picture) {
		@viewport = viewport;
		@picture = picture;
		@sprite = null;
		update;
	}

	public void dispose() {
		@sprite&.dispose;
	}

	public void update() {
		@sprite&.update;
		// If picture file name is different from current one
		if (@picture_name != @picture.name) {
			// Remember file name to instance variables
			@picture_name = @picture.name;
			// If file name is not empty
			if (@picture_name != "") {
				// Get picture graphic
				if (!@sprite) @sprite = new IconSprite(0, 0, @viewport);
				@sprite.setBitmap("Graphics/Pictures/" + @picture_name);
			}
		}
		// If file name is empty
		if (@picture_name == "") {
			// Set sprite to invisible
			if (@sprite) {
				@sprite&.dispose;
				@sprite = null;
			}
			return;
		}
		// Set sprite to visible
		@sprite.visible = true;
		// Set transfer starting point
		if (@picture.origin == 0) {
			@sprite.ox = 0;
			@sprite.oy = 0;
		} else {
			@sprite.ox = @sprite.bitmap.width / 2;
			@sprite.oy = @sprite.bitmap.height / 2;
		}
		// Set sprite coordinates
		@sprite.x = @picture.x;
		@sprite.y = @picture.y;
		@sprite.z = @picture.number;
		// Set zoom rate, opacity level, and blend method
		@sprite.zoom_x = @picture.zoom_x / 100.0;
		@sprite.zoom_y = @picture.zoom_y / 100.0;
		@sprite.opacity = @picture.opacity;
		@sprite.blend_type = @picture.blend_type;
		// Set rotation angle and color tone
		@sprite.angle = @picture.angle;
		@sprite.tone = @picture.tone;
	}
}
