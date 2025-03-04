using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using DiscordBotBase;

namespace SwarmHelpBot;

public class SwarmBot
{
    public DiscordBot Bot;

    public void InitAndRun(string[] args)
    {
        DiscordBotBaseHelper.StartBotHandler(args, new DiscordBotConfig()
        {
            CommandPrefix = null,
            Initialize = (bot) =>
            {
                Console.WriteLine("SwarmHelpBot initializing...");
                Bot = bot;
                bot.Client.Ready += () =>
                {
                    Console.WriteLine("SwarmHelpBot got 'ready' signal!");
                    bot.Client.SetGameAsync("my Swarm gens roll in", type: ActivityType.Watching).Wait();
                    try
                    {
                        const string commandVersionFile = "./config/command_registered_version.dat";
                        int confVersion = bot.ConfigFile.GetInt("slash_cmd_version", 0).Value;
                        string fullVers = $"{CommandVersionId}_{confVersion}";
                        if (!File.Exists(commandVersionFile) || commandVersionFile != fullVers)
                        {
                            Console.WriteLine("SwarmHelpBot re-registering commands...");
                            RegisterSlashCommands().Wait();
                            File.WriteAllText(commandVersionFile, fullVers);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Failed to update slash commands: {ex}");
                    }
                    return Task.CompletedTask;
                };
            },
            UnknownCommandMessage = null,
            ShouldPayAttentionToMessage = (message) =>
            {
                return message.Channel is IGuildChannel;
            }
        });
    }

    public static int CommandVersionId = 1;

    /// <summary>Registers all applicable slash commands.</summary>
    public async Task RegisterSlashCommands()
    {
        List<ApplicationCommandProperties> cmds = [];
        void registerInfoCmd(string name, string description, string content)
        {
            cmds.Add(new SlashCommandBuilder().WithName(name).WithDescription(description)
                .AddOption("user", ApplicationCommandOptionType.User, "(Optional) A user to ping the information to.", isRequired: false).Build());
            Bot.RegisterSlashCommand(async cmd =>
            {
                string message = content;
                if (cmd.Data.Options.FirstOrDefault(o => o.Name == "user")?.Value is IUser user)
                {
                    message = $"{user.Mention} {message}";
                }
                await cmd.RespondAsync(message);
            }, name);
        }
        registerInfoCmd("helpwithlog", "Tell a user how to get help and pastebin a log.", "Hey, it looks like you need help with SwarmUI. If so, here's what to do:\n\nFirst, hit CTRL+F and search to see if anybody else has already asked the same thing, and check if it's in [the Troubleshooting Doc](<https://github.com/mcmonkeyprojects/SwarmUI/blob/master/docs/Troubleshooting.md>).\n\nIf not, open a post in <#1255969955808022679>.\n\nWhen making your post, go to your Swarm interface, click **Server**, then **Logs**, then the **Pastebin** button, then **Submit**. It will generate a link - copy that link, and include it in your <#1255969955808022679> post.\n\nMake sure to also describe the problem in your post, and include any relevant information, such as screenshots of the interface or your gen parameters.\nhttps://i.alexgoodwin.media/i/misc/fc1e69.png");
        registerInfoCmd("troubleshooting", "Tell a user to check the troubleshooting doc.", "Check if your issue is already covered in the Troubleshooting doc: <https://github.com/mcmonkeyprojects/SwarmUI/blob/master/docs/Troubleshooting.md>");
        registerInfoCmd("modelsupport", "Tell a user to check the model support docs.", "AI Image models supported by SwarmUI are documented on [the Model Support doc](<https://github.com/mcmonkeyprojects/SwarmUI/blob/master/docs/Model%20Support.md>), and video models on [the Video Model Support doc](<https://github.com/mcmonkeyprojects/SwarmUI/blob/master/docs/Video%20Model%20Support.md>). These docs should be the first place to check when you're unsure how to use a model.");
        registerInfoCmd("install", "Give a user a link to the install guide.", "If you're new to SwarmUI, you can learn how to install it here: <https://github.com/mcmonkeyprojects/SwarmUI/blob/master/README.md#installing-on-windows> -- pick the category that matches your setup (Windows/Linux/etc), and then make sure to follow the steps listed exactly!");
        registerInfoCmd("license", "Info about the SwarmUI license.", "SwarmUI is 100% free and open source under [The MIT License](<https://github.com/mcmonkeyprojects/SwarmUI/blob/master/LICENSE.txt>). This means you can do whatever you want with it, including for commercial purposes, just don't lie about it or try to sue me about it. See also [the legal notes on dependencies](<https://github.com/mcmonkeyprojects/SwarmUI/blob/master/README.md#legal>) which may affect some usages.\n\nAny images you generate within SwarmUI are your own images, however note that copyright issues may or may not apply depending on the model you used and whether your image resembles pre-existing images.\n\nSome models or extensions may have licenses of their own that apply to your usage.");
        await Bot.Client.BulkOverwriteGlobalApplicationCommandsAsync([.. cmds]);
        Console.WriteLine($"Registered slash commands: {string.Join(", ", cmds.Select(c => c.Name))}");
    }
}
