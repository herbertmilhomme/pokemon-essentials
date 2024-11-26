//===============================================================================
//
//===============================================================================
public static partial class FilenameUpdater {
	#region Class Functions
	#endregion

	public void readDirectoryFiles(directory, formats) {
		files = new List<string>();
		Dir.chdir(directory) do;
			foreach (var format in formats) { //'formats.each' do => |format|
				Dir.glob(format, f => { files.Add(f); });
			}
		}
		return files;
	}

	public void rename_berry_plant_charsets() {
		src_dir = "Graphics/Characters/";
		if (!FileTest.directory(src_dir)) return false;
		Console.echo_li(_INTL("Renaming berry tree charsets..."));
		ret = false;
		// generates a list of all graphic files
		files = readDirectoryFiles(src_dir, ["berrytree*.png"]);
		// starts automatic renaming
		files.each_with_index do |file, i|
			if (System.Text.RegularExpressions.Regex.IsMatch(file,@"^berrytree_")) continue;
			if (new []{"berrytreewet", "berrytreedamp", "berrytreedry", "berrytreeplanted"}.Contains(file.split(".")[0])) continue;
			new_file = file.gsub("berrytree", "berrytree_");
			File.move(src_dir + file, src_dir + new_file);
			ret = true;
		}
		Console.echo_done(true);
		return ret;
	}

	public void update_berry_tree_event_charsets() {
		ret = new List<string>();
		mapData = new Compiler.MapData();
		t = System.uptime;
		Graphics.update;
		Console.echo_li(_INTL("Checking {1} maps for used berry tree charsets...", mapData.mapinfos.keys.length));
		idx = 0;
		foreach (var id in mapData.mapinfos.keys.sort) { //'mapData.mapinfos.keys.sort.each' do => |id|
			if (idx % 20 == 0) echo ".";
			idx += 1;
			if (idx % 250 == 0) Graphics.update;
			map = mapData.getMap(id);
			if (!map || !mapData.mapinfos[id]) continue;
			changed = false;
			foreach (var key in map.events) { //map.events.each_key do => |key|
				if (System.uptime - t >= 5) {
					t += 5;
					Graphics.update;
				}
				foreach (var page in map.events[key].pages) { //'map.events[key].pages.each' do => |page|
					if (nil_or_empty(page.graphic.character_name)) continue;
					char_name = page.graphic.character_name;
					if (!System.Text.RegularExpressions.Regex.IsMatch(char_name,@"^berrytree[^_]+")) continue;
					if (new []{"berrytreewet", "berrytreedamp", "berrytreedry", "berrytreeplanted"}.Contains(char_name.split(".")[0])) continue;
					new_file = page.graphic.character_name.gsub("berrytree", "berrytree_");
					page.graphic.character_name = new_file;
					changed = true;
				}
			}
			if (!changed) continue;
			mapData.saveMap(id);
			ret.Add(_INTL("Map {1}: '{2}' was modified and saved.", id, mapData.mapinfos[id].name));
		}
		Console.echo_done(true);
		return ret;
	}

	public void rename_files() {
		Console.echo_h1(_INTL("Updating file names and locations"));
		change_record = new List<string>();
		// Add underscore to berry plant charsets
		if (rename_berry_plant_charsets) {
			Console.echo_warn(_INTL("Berry plant charset files were renamed."));
		}
		change_record += update_berry_tree_event_charsets;
		// Warn if any map data has been changed
		if (!change_record.empty()) {
			change_record.each(msg => Console.echo_warn(msg));
			Console.echo_warn(_INTL("RMXP data was altered. Close RMXP now to ensure changes are applied."));
		}
		echoln "";
		Console.echo_h2(_INTL("Finished updating file names and locations"), text: :green);
	}
}
