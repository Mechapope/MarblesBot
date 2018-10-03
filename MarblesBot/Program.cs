using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace MarblesBot
{
    class Program
    {
        private static string _botName = "";
        private static string _broadcasterName = "";
        private static string _twitchOAuth = "";

        static void Main(string[] args)
        {
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\botinfo.txt";

            using (StreamReader sr = new StreamReader(folder))
            {
                _botName = sr.ReadLine();
                _broadcasterName = sr.ReadLine();
                _twitchOAuth = sr.ReadLine();
                sr.Close();
            }

            DateTime lastPostTime = DateTime.Now;
            List<string> pastmessages = new List<string>();

            // Initialize and connect to Twitch chat
            IrcClient irc = new IrcClient("irc.chat.twitch.tv", 6667, _botName, _twitchOAuth, _broadcasterName);

            // Ping to the server to make sure this bot stays connected to the chat
            // Server will respond back to this bot with a PONG (without quotes):
            // Example: ":tmi.twitch.tv PONG tmi.twitch.tv :irc.twitch.tv"
            PingSender ping = new PingSender(irc);
            ping.Start();

            // Listen to the chat until program exits
            while (true)
            {
                // Read any message from the chat room
                string message = irc.ReadMessage();
                Console.WriteLine(message); // Print raw irc messages

                if (message.Contains("PRIVMSG"))
                {
                    // Messages from the users will look something like this (without quotes):
                    // Format: ":[user]![user]@[user].tmi.twitch.tv PRIVMSG #[channel] :[message]"

                    // Modify message to only retrieve user and message
                    int intIndexParseSign = message.IndexOf('!');
                    string userName = message.Substring(1, intIndexParseSign - 1);
                    // parse username from specific section (without quotes)
                    // Format: ":[user]!"
                    // Get user's message
                    intIndexParseSign = message.IndexOf(" :");
                    message = message.Substring(intIndexParseSign + 2);

                    //Console.WriteLine(message); // Print parsed irc message (debugging only)

                    pastmessages.Add(message);
                    if (pastmessages.Count > 5)
                    {
                        pastmessages.RemoveAt(0);
                    }

                    //check last 5 messages have been !play
                    if (!pastmessages.Any(m => m != "!play") && lastPostTime.AddMinutes(2) > DateTime.Now)
                    {
                        //only post once every 2 minutes
                        lastPostTime = DateTime.Now;
                        //irc.SendPublicChatMessage("!play");
                    }
                }
            }
        }
    }
}
