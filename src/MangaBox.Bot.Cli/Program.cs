using MangaBox.Bot;
using MangaBox.Bot.Cli;
//Create an instance of the bot
var bot = MangaBoxBot.CreateBot();
//Log the bot in
await bot.Login();
//Setup the triggers for looking up based on reactions and messages
bot.Background<ILookupHooks>(t => t.Setup(), out _);
//Wait forever so the bot doesn't exit
await Task.Delay(-1);