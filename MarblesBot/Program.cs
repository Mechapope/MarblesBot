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
        static void Main(string[] args)
        {
            string botName = "";
            string channelName = "";
            string authToken = "";

            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + @"\botinfo.txt";

            using (StreamReader sr = new StreamReader(folder))
            {
                botName = sr.ReadLine().ToLower();
                channelName = sr.ReadLine().ToLower();
                authToken = sr.ReadLine().ToLower();
                sr.Close();
            }

            DateTime lastPostTime = DateTime.Now;
            List<string> pastmessages = new List<string>();

            // Initialize and connect to Twitch chat
            IrcClient irc = new IrcClient("irc.twitch.tv", 6667, botName, authToken, channelName);

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
                    // Messages from the users will look something like this
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
                    if (pastmessages.Count > 3)
                    {
                        pastmessages.RemoveAt(0);
                    }

                    //check last 5 messages have been !play
                    if (!pastmessages.Any(m => !m.StartsWith("!play")) && lastPostTime.AddMinutes(2) <= DateTime.Now)
                    {
                        //only post once every 2 minutes
                        lastPostTime = DateTime.Now;
                        irc.SendPublicChatMessage("!play");
                    }
                }
            }
        }
    }
}
