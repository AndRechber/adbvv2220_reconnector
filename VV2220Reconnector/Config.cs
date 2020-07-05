using System;
using System.IO;

namespace VV2220Reconnector
{
    internal class Config
    {
        internal string modemAddress { get; }
        internal int reconnectWaitInMs { get; }

        internal string username { get; }
        internal string passwort { get; }

        internal Config(string modemAddress, int reconnectWaitInMs, string username, string passwort)
        {
            this.modemAddress = modemAddress;
            this.reconnectWaitInMs = reconnectWaitInMs;
            this.username = username;
            this.passwort = passwort;
        }

        internal static Config read(string pathOfFile)
        {
            string[] lines = File.ReadAllLines(pathOfFile);
            string modemAddress = "";
            int reconnectWaitInMs = 0;
            string username = "";
            string passwort = "";
            foreach (var line in lines)
            {
                string formattedLine = line.Trim();
                if (isField("modemAddress", formattedLine))
                {
                    modemAddress = getField("modemAddress", formattedLine);
                }
                else if (isField("reconnectWaitInMs", formattedLine))
                {
                    reconnectWaitInMs = Convert.ToInt32(getField("reconnectWaitInMs", formattedLine));
                }
                else if (isField("username", formattedLine))
                {
                    username = getField("username", formattedLine);
                }
                else if (isField("passwort", formattedLine))
                {
                    passwort = getField("passwort", formattedLine);
                }
            }
            return new Config(modemAddress, reconnectWaitInMs, username, passwort);
        }

        private static bool isField(string fieldName, string line)
        {
            return line.StartsWith(fieldName + "=");
        }

        private static string getField(string fieldName, string line)
        {
            return line.Replace(fieldName + "=", "");
        }
    }
}