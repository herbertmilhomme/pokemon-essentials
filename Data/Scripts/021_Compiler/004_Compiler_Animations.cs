//===============================================================================
//
//===============================================================================
public static partial class Compiler {
	@@categories.animations = {
		should_compile = compiling => { next false },
		header_text    = () => { next _INTL("Compiling animations") },
		skipped_text   = () => { next _INTL("Not compiled") },
		compile        = () => compile_animations;
	}

	#region Class Functions
	#endregion

	//-----------------------------------------------------------------------------
	// Compile battle animations.
	//-----------------------------------------------------------------------------
	public void compile_animations() {
		Console.echo_li(_INTL("Compiling animations..."));
		begin;
			anims = load_data("Data/PkmnAnimations.rxdata");
		rescue;
			anims = new Animations();
		}
		changed = false;
		move2anim = new {{}, new List<string>()}
//    anims = load_data("Data/Animations.rxdata")
//    for anim in anims
//      next if !anim || anim.frames.length == 1
//      found = false
//      for (int i = 0; i < anims.length; i++) {
//        if (anims[i] && anims[i].id == anim.id) {
//          found = true if anims[i].array.length > 1
//          break
//        }
//      }
//      anims[anim.id] = ConvertRPGAnimation(anim) if !found
//    }
		idx = 0;
		for (int i = anims.length; i < anims.length; i++) { //for 'anims.length' times do => |i|
			if (idx % 100 == 0) echo ".";
			if (idx % 500 == 0) Graphics.update;
			idx += 1;
			if (!anims[i]) continue;
			if (System.Text.RegularExpressions.Regex.IsMatch(anims[i].name,@"^OppMove\:\s*(.*)$")) {
				if (GameData.Move.exists($~[1])) {
					moveid = GameData.Move.get($~[1]).id;
					if (!move2anim[0][moveid] || move2anim[1][moveid] != i) changed = true;
					move2anim[1][moveid] = i;
				}
			} else if (System.Text.RegularExpressions.Regex.IsMatch(anims[i].name,@"^Move\:\s*(.*)$")) {
				if (GameData.Move.exists($~[1])) {
					moveid = GameData.Move.get($~[1]).id;
					if (!move2anim[0][moveid] || move2anim[0][moveid] != i) changed = true;
					move2anim[0][moveid] = i;
				}
			}
		}
		if (changed) {
			save_data(move2anim, "Data/move2anim.dat");
			save_data(anims, "Data/PkmnAnimations.rxdata");
		}
		process_pbs_file_message_end;
	}
}
