//===============================================================================
// Additions to class Sprite that allows class AnimationContainerSprite to attach
// overworld animations to itself.
//===============================================================================
public static partial class RPG {
	public partial class Sprite : global::Sprite {
		public override void initialize(viewport = null) {
			base.initialize(viewport);
			@_animation_duration = 0;
			@_animation_frame = 0;
			@animations = new List<string>();
			@loopAnimations = new List<string>();
		}

		public override void dispose() {
			dispose_animation;
			dispose_loop_animation;
			base.dispose();
		}

		public void dispose_animation() {
			@animations.each(a => a&.dispose_animation);
			@animations.clear;
		}

		public void dispose_loop_animation() {
			@loopAnimations.each(a => a&.dispose_loop_animation);
			@loopAnimations.clear;
		}

		public int x { set {
			@animations.each(a => { if (a) a.x = x; });
			@loopAnimations.each(a => { if (a) a.x = x; });
			super;
			}
		}

		public int y { set {
			@animations.each(a => { if (a) a.y = y; });
			@loopAnimations.each(a => { if (a) a.y = y; });
			super;
			}
		}

		public void pushAnimation(array, anim) {
			for (int i = array.length; i < array.length; i++) { //for 'array.length' times do => |i|
				if (array[i]&.active()) continue;
				array[i] = anim;
				return;
			}
			array.Add(anim);
		}

		public void animation(animation, hit, height = 3, no_tone = false) {
			anim = new SpriteAnimation(self);
			anim.animation(animation, hit, height, no_tone);
			pushAnimation(@animations, anim);
		}

		public void loop_animation(animation) {
			anim = new SpriteAnimation(self);
			anim.loop_animation(animation);
			pushAnimation(@loopAnimations, anim);
		}

		public bool effect() {
			@animations.each(a => { if (a.effect()) return true; });
			return false;
		}

		public void update_animation() {
			@animations.each(a => { if (a&.active()) a.update_animation; });
		}

		public void update_loop_animation() {
			@loopAnimations.each(a => { if (a&.active()) a.update_loop_animation; });
		}

		public override void update() {
			base.update();
			@animations.each(a => a.update);
			@loopAnimations.each(a => a.update);
			SpriteAnimation.clear;
		}
	}
}

//===============================================================================
// A version of class Sprite that allows its coordinates to be floats rather than
// integers.
//===============================================================================
public partial class FloatSprite : Sprite {
	public int x { get { return @float_x; } }
	public int y { get { return @float_y; } }

	public override int x { set {
		@float_x = value;
		base.x();
		}
	}

	public override int y { set {
		@float_y = value;
		base.y();
		}
	}
}
