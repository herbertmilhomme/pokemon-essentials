//===============================================================================
//
//===============================================================================
public static partial class Input {
	USE      = C;
	BACK     = B;
	ACTION   = A;
	JUMPUP   = X;
	JUMPDOWN = Y;
	SPECIAL  = Z;
	AUX1     = L;
	AUX2     = R;

	unless (defined(update_KGC_ScreenCapture)) {
		public partial class : Input {
			alias update_KGC_ScreenCapture update;
		}
	}

	public static void update() {
		update_KGC_ScreenCapture;
		if (trigger(Input.F8)) ScreenCapture;
	}
}

//===============================================================================
//
//===============================================================================
public static partial class Mouse {
	// Returns the position of the mouse relative to the game window.
	public static void getMousePos(catch_anywhere = false) {
		unless (Input.mouse_in_window || catch_anywhere) return null;
		return Input.mouse_x, Input.mouse_y;
	}
}
