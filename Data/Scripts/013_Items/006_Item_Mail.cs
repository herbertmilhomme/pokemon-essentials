//===============================================================================
// Data structure representing mail that the Pokémon can hold.
//===============================================================================
public partial class Mail {
	public int item		{ get { return _item; } set { _item = value; } }			protected int _item;
	public int message		{ get { return _message; } set { _message = value; } }			protected int _message;
	public int sender		{ get { return _message; } set { _message = value; } }			protected int _message;
	public int poke1		{ get { return _poke1; } set { _poke1 = value; } }			protected int _poke1;
	public int poke2		{ get { return _poke2; } set { _poke2 = value; } }			protected int _poke2;
	public int poke3		{ get { return _poke3; } set { _poke3 = value; } }			protected int _poke3;

	public void initialize(item, message, sender, poke1 = null, poke2 = null, poke3 = null) {
		@item    = GameData.Item.get(item).id;   // Item represented by this mail
		@message = message;   // Message text
		@sender  = sender;    // Name of the message's sender
		@poke1   = poke1;     // [species,gender,shininess,form,shadowness,is egg]
		@poke2   = poke2;
		@poke3   = poke3;
	}
}

//===============================================================================
//
//===============================================================================
public void MoveToMailbox(pokemon) {
	if (!Game.GameData.PokemonGlobal.mailbox) Game.GameData.PokemonGlobal.mailbox = new List<string>();
	if (Game.GameData.PokemonGlobal.mailbox.length >= 10) return false;
	if (!pokemon.mail) return false;
	Game.GameData.PokemonGlobal.mailbox.Add(pokemon.mail);
	pokemon.mail = null;
	return true;
}

public void StoreMail(pkmn, item, message, poke1 = null, poke2 = null, poke3 = null) {
	if (pkmn.mail) raise _INTL("Pokémon already has mail");
	pkmn.mail = new Mail(item, message, Game.GameData.player.name, poke1, poke2, poke3);
}

public void DisplayMail(mail, _bearer = null) {
	sprites = new List<string>();
	viewport = new Viewport(0, 0, Graphics.width, Graphics.height);
	viewport.z = 99999;
	addBackgroundPlane(sprites, "background", "Mail/bg", viewport);
	sprites["card"] = new IconSprite(0, 0, viewport);
	sprites["card"].setBitmap(GameData.Item.mail_filename(mail.item));
	sprites["overlay"] = new BitmapSprite(Graphics.width, Graphics.height, viewport);
	overlay = sprites["overlay"].bitmap;
	SetSystemFont(overlay);
	if (GameData.Item.get(mail.item).is_icon_mail()) {
		if (mail.poke1) {
			sprites["bearer"] = new IconSprite(64, 288, viewport);
			bitmapFileName = GameData.Species.icon_filename(
				mail.poke1[0], mail.poke1[3], mail.poke1[1], mail.poke1[2], mail.poke1[4], mail.poke1[5]
			);
			sprites["bearer"].setBitmap(bitmapFileName);
			sprites["bearer"].src_rect.set(0, 0, 64, 64);
		}
		if (mail.poke2) {
			sprites["bearer2"] = new IconSprite(144, 288, viewport);
			bitmapFileName = GameData.Species.icon_filename(
				mail.poke2[0], mail.poke2[3], mail.poke2[1], mail.poke2[2], mail.poke2[4], mail.poke2[5]
			);
			sprites["bearer2"].setBitmap(bitmapFileName);
			sprites["bearer2"].src_rect.set(0, 0, 64, 64);
		}
		if (mail.poke3) {
			sprites["bearer3"] = new IconSprite(224, 288, viewport);
			bitmapFileName = GameData.Species.icon_filename(
				mail.poke3[0], mail.poke3[3], mail.poke3[1], mail.poke3[2], mail.poke3[4], mail.poke3[5]
			);
			sprites["bearer3"].setBitmap(bitmapFileName);
			sprites["bearer3"].src_rect.set(0, 0, 64, 64);
		}
	}
	baseForDarkBG    = new Color(248, 248, 248);
	shadowForDarkBG  = new Color(72, 80, 88);
	baseForLightBG   = new Color(80, 80, 88);
	shadowForLightBG = new Color(168, 168, 176);
	if (mail.message && mail.message != "") {
		isDark = isDarkBackground(sprites["card"].bitmap, new Rect(48, 48, Graphics.width - 96, 224));
		drawTextEx(overlay, 48, 52, Graphics.width - 94, 7, mail.message,
							(isDark) ? baseForDarkBG : baseForLightBG,
							(isDark) ? shadowForDarkBG : shadowForLightBG)
	}
	if (mail.sender && mail.sender != "") {
		isDark = isDarkBackground(sprites["card"].bitmap, new Rect(336, 322, 144, 32));
		drawTextEx(overlay, 336, 328, 144, 1, mail.sender,
							(isDark) ? baseForDarkBG : baseForLightBG,
							(isDark) ? shadowForDarkBG : shadowForLightBG)
	}
	FadeInAndShow(sprites);
	do { //loop; while (true);
		Graphics.update;
		Input.update;
		UpdateSpriteHash(sprites);
		if (Input.trigger(Input.BACK) || Input.trigger(Input.USE)) {
			break;
		}
	}
	FadeOutAndHide(sprites);
	DisposeSpriteHash(sprites);
	viewport.dispose;
}

public void WriteMail(item, pkmn, pkmnid, scene) {
	message = "";
	do { //loop; while (true);
		message = MessageFreeText(_INTL("Please enter a message (max. 250 characters)."),
																"", false, 250, Graphics.width) { scene.Update };
		if (message != "") {
			// Store mail if a message was written
			poke1 = poke2 = null;
			if (Game.GameData.player.party[pkmnid + 2]) {
				p = Game.GameData.player.party[pkmnid + 2];
				poke1 = new {p.species, p.gender, p.shiny(), p.form, p.shadowPokemon()};
				if (p.egg()) poke1.Add(true);
			}
			if (Game.GameData.player.party[pkmnid + 1]) {
				p = Game.GameData.player.party[pkmnid + 1];
				poke2 = new {p.species, p.gender, p.shiny(), p.form, p.shadowPokemon()};
				if (p.egg()) poke2.Add(true);
			}
			poke3 = new {pkmn.species, pkmn.gender, pkmn.shiny(), pkmn.form, pkmn.shadowPokemon()};
			if (pkmn.egg()) poke3.Add(true);
			StoreMail(pkmn, item, message, poke1, poke2, poke3);
			return true;
		}
		if (scene.Confirm(_INTL("Stop giving the Pokémon Mail?"))) return false;
	}
}
