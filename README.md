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

## Slash Commands
There are various slash commands that can be used to interact with and configure the bot.

### `/manga-search`
Searches for a manga based on the provided image link.

| Parameter | Data Type           | Required | Notes |
|-----------|---------------------|----------|-------|
| url       | Fully Qualified URL | Required | The URL of the image to search for |

### `/manga-search-private`
Same as `/manga-search` but only sends the result to the user who requested it.

| Parameter | Data Type           | Required | Notes |
|-----------|---------------------|----------|-------|
| url       | Fully Qualified URL | Required | The URL of the image to search for |

### `/about`
Displays information about the bot and it's current configuration in the server.
There are no parameters for this command. 

### `/manga-config`
Configures the discord bot for the current server. You have to be an authorized user or have the `Manage Server` permission to use this command.

| Parameter | Data Type            | Required | Notes |
|-----------|----------------------|----------|-------|
| reactions | `Enable` / `Disable` | Optional | Whether the bot should lookup manga based on emote reactions |
| pings		| `Enable` / `Disable` | Optional | Whether the bot should lookup manga based mentioning the bot |
| emotes    | Space separated list | Optional | The emotes the bot should use for reactions. Supports both custom server emotes and Unicode emojis (e.g. `🍝`) |

Not specifying any of the above will just show the configuration for the current server.

### `/manga-config-channels`
Configures the channels the bot should respond to lookup requests in. You have to be an authorized user or have the `Manage Server` permission to use this command.

| Parameter | Data Type                                          | Required | Notes |
|-----------|----------------------------------------------------|----------|-------|
| action    | One of: `State`, `Clear`, `Blacklist`, `Whitelist` | Required | What to do with the channel |
| channel   | The discord channel                                | Required | The discord channel to configure |

* `State` - Will show you if the channel is blacklisted or whitelisted
* `Clear` - Will remove the channel from the blacklist and/or whitelist
* `Blacklist` - Will add the channel to the blacklist (and remove it from the whitelist if it's there)
* `Whitelist` - Will add the channel to the whitelist (and remove it from the blacklist if it's there)

If you add channels to both the white and black list, the bot will only respond in the whitelisted channels and ignore blacklisted channels.
You can see all of the channels that are configured by using the `/manga-config` command with no parameters specified.

### `/manga-config-responses`
Configures the responses the bot gives when looking up a manga. You have to be an authorized user or have the `Manage Server` permission to use this command.

| Parameter | Data Type																				 | Required | Notes |
|-----------|----------------------------------------------------------------------------------------|----------|-------|
| message   | One of: `Duplicates`, `Loading`, `Download Failed`, `No Results`, `Succeeded`, `Error` | Required | What to do with the channel |
| value     | Text                                                                                   | Optional | The message to send or leave off to reset to default |

You can see what each of the `message`s are used for below.

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

Here is an example docker compose file you can use:
```yaml
services:
  mb-bot:
    image: ghcr.io/cardboards-box/manga-reverse-img-search-bot/bot:latest
    restart: always
    environment:
      - Discord:Token=${DISCORD_TOKEN}
      - Search:Api=${SEARCH_API}
      - Database:ConnectionString=Data Source=persist/database.db
    volumes:
      - ./persist:/app/persist
```
.env file:
```bash
DISCORD_TOKEN=abcdefghijklmnopqrstuvqxyz.123456.1234567890abcdefghijklmnopqrstuv-xyz12
SEARCH_API=https://some-api.com
```

### Configuration
You will need your own bot token (you can get those [here](https://discord.com/developers/applications) or Google it)

There are various configuration options that can be changed:
* `Discord:Token` - This is the bot's authentication token (Required)
* `Discord:Local` - If you're getting issues with the snowflake interactions, set this to true
* `Discord:Intents` - This is a collection of gateway intents that the bot will use (leave as default)
* `Discord:CommandParam` - You can ignore this, it's no longer used.
* `Database:ConnectionString` - The connection string for the SQLite database used for tracking lookups
* `Search:AuthorizedUsers` - An array of discord user IDs that are allowed to configure the bot in any server
* `Search:Api` - The API url to the reverse image search service (Required)
* `Search:UserAgent` - The user agent when requesting stuff from MangaDex (DO NOT SPOOF BROWSERS - You will be blocked by MangaDex)
* `Search:Title` - Customize the title and footer of the embeds
* `Search:JoinLink` - Customize the link to add the bot to a server
* `Search:Emotes` - A space or comma separated list of emotes to use for reactions

You can edit the default responses the bot gives by changing the following values:
* `Search:Responses:Idiots` - The message to send when someone has requested something that's already been looked up
	* Supports string-format option `{0}` for the user's discord ID (You can use this to mention the user)
	* Example: `Uh, <@{0}>, it's right here...`
* `Search:Responses:Loading` - The message to send when a lookup request is loading
	* Supports string-format option `{0}` for the user's discord ID (You can use this to mention the user)
	* Example: `<@{0}> Processing your request...`
* `Search:Responses:DownloadFailed` - The message to send when the bot fails to download the image
* `Search:Responses:NoResults` - The message to send when the bot fails to find any results
* `Search:Responses:Succeeded` - The message to send when the bot successfully finds results
* `Search:Responses:Error` - The message to send when an error occurs during lookup
	* Supports string-format option `{0}` for the error message
	* Example: `An error occurred while looking up the image :( - Error Message: {0}`

You can either set these in the `appsettings.json` file or use environment variables.

## Need help?
You can reach out via one of the following:
* My [Discord Server](https://discord.gg/RV9MvvYXsp)
* The [MangaDex Forum](https://forums.mangadex.org/threads/manga-reverse-image-lookup-service.1146452/)
* GitHub issue / discussion.