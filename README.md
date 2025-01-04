# MangaBox | Search
A discord bot for reverse searching manga!

You can invite it to a server [using this link](https://discord.com/oauth2/authorize?client_id=1324859527480475710&permissions=1126864127313984&integration_type=0&scope=applications.commands+bot).

If you're looking for the web version, you can access it [here](https://mangabox.app)

## Interactions
There are various ways to interact with the bot and get that sweet sweet sauce.
* React to a message with some predefined emojis (default: 🍝, 🔍, and 🔎)
* Mention the bot while providing the image or image link 
* Mention the bot while replying to a message with an image or image link
* Use the `/manga-search` command with the image link
* Use the `/manga-search-private` command with the image link (this will only send the result to you)

There is also a command you can use to find information about the bot: `/about`

## Known Issues
These are some known issues with the reverse image search database:
* Cropped images and long-strip content don't work very well
* Only English manga are indexed
* Sites other than MangaDex are iffy at best.
* MangaDex content before Dec 2022 has not been indexed.

## Self hosting
There is an included docker image that can be used to host the bot yourself.

```bash
docker pull ghcr.io/cardboards-box/manga-reverse-img-search-bot/bot:latest
```

You can run the source code directly if you prefer that as well.
Run the `MangaBox.Bot.Cli` project after filling in the `appsettings.json` file (or environment variables).


### Configuration
You will need your own bot token (you can get those [here](https://discord.com/developers/applications) or Google it)

There are various configuration options that can be changed:
* `Discord:Token` - This is the bot's authentication token (Required)
* `Discord:Local` - If you're getting issues with the snowflake interactions, set this to true
* `Discord:Intents` - This is a collection of gateway intents that the bot will use (leave as default)
* `Discord:CommandParam` - You can ignore this, it's no longer used.
* `Database:ConnectionString` - The connection string for the SQLite database used for tracking lookups
* `Search:Api` - The API url to the reverse image search service (Required)
* `Search:UserAgent` - The user agent when requesting stuff from MangaDex (DO NOT SPOOF BROWSERS - You will be banned)
* `Search:Title` - Customize the title and footer of the embeds
* `Search:JoinLink` - Customize the link to add the bot to a server
* `Search:Emotes` - A space or comma separated list of emotes to use for reactions

You can either set these in the `appsettings.json` file or use environment variables.

## Need help?
You can reach out via one of the following:
* My [Discord Server](https://discord.gg/RV9MvvYXsp)
* The [MangaDex Forum](https://forums.mangadex.org/threads/manga-reverse-image-lookup-service.1146452/)
* GitHub issue / discussion.