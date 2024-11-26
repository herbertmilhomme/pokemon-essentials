//===============================================================================
// HTTP utility functions
//===============================================================================
public void PostData(url, postdata, filename = null, depth = 0) {
	if (System.Text.RegularExpressions.Regex.IsMatch(url,@"^http:\/\/([^\/]+)(.*)$")) {
		host = Game.GameData.1;
//    path = Game.GameData.2
//    path = "/" if path.length == 0
		userAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.14) Gecko/2009082707 Firefox/3.0.14";
		body = postdata.map do |key, value|
			keyString   = key.ToString();
			valueString = value.ToString();
			keyString = System.Text.RegularExpressions.Regex.Replace(keyString, "[^a-zA-Z0-9_\.\-]"n, s => string.Format("{0:X2}", (int)s.Value[0]),RegexOptions.IgnoreCase);
			valueString = System.Text.RegularExpressions.Regex.Replace(valueString, "[^a-zA-Z0-9_\.\-]"n, s => string.Format("{0:X2}", (int)s.Value[0]),RegexOptions.IgnoreCase);
			next $"{keyString}={valueString}";
		}.join("&");
		ret = HTTPLite.post_body(
			url,
			body,
			"application/x-www-form-urlencoded",
			{
				"Host"             => host, // might not be necessary
				"Proxy-Connection" => "Close",
				"Content-Length"   => body.bytesize.ToString(),
				"Pragma"           => "no-cache",
				"User-Agent"       => userAgent;
			}
		) rescue "";
		if (!ret.is_a(Hash)) return ret;
		if (ret.status != 200) return "";
		if (!filename) return ret.body;
		File.open(filename, "wb", f => { f.write(ret.body); });
		return "";
	}
	return "";
}

public void DownloadData(url, filename = null, authorization = null, depth = 0, Action block = null) {
	headers = {
		"Proxy-Connection" => "Close",
		"Pragma"           => "no-cache",
		"User-Agent"       => "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.9.0.14) Gecko/2009082707 Firefox/3.0.14";
	}
	if (authorization) headers["authorization"] = authorization;
	ret = HTTPLite.get(url, headers) rescue "";
	if (!ret.is_a(Hash)) return ret;
	if (ret.status != 200) return "";
	if (!filename) return ret.body;
	File.open(filename, "wb", f => { f.write(ret.body); });
	return "";
}

public void DownloadToString(url) {
	begin;
		data = DownloadData(url);
		return data;
	rescue;
		return "";
	}
}

public void DownloadToFile(url, file) {
	begin;
		DownloadData(url, file);
	rescue;
	}
}

public void PostToString(url, postdata) {
	begin;
		data = PostData(url, postdata);
		return data;
	rescue;
		return "";
	}
}

public void PostToFile(url, postdata, file) {
	begin;
		PostData(url, postdata, file);
	rescue;
	}
}
