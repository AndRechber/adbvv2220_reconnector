using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VV2220Reconnector
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Reconnect tool for modem ADB VV2220v2, FW: E_9.5.2");
            try
            {
                Config config = Config.read(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "config.ini"));
                var wait = reconnect(config);
                wait.Wait();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        private static async Task reconnect(Config config)
        {
            WebClient webClient = createWebClient(config);

            await login(webClient, config);
            Dictionary<string, string> properties = await webClient.loadInetConnectionPage();
            await enableInternetConnection(webClient, properties, false);
            await Task.Delay(config.reconnectWaitInMs);
            await enableInternetConnection(webClient, properties, true);
        }

        private static WebClient createWebClient(Config config)
        {
            string basicModemUri = "http://" + config.modemAddress + "/";
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            return new WebClient(client, basicModemUri);
        }

        private static async Task login(WebClient webClient, Config config)
        {
            Console.WriteLine("Try to login");
            Dictionary<string, string> properties = await webClient.loadLoginPage();
            string passwordHash = computeHash(config.passwort, properties["nonce"]);

            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("userName", config.username));
            postData.Add(new KeyValuePair<string, string>("language", "DE"));
            postData.Add(new KeyValuePair<string, string>("login", "Login"));
            postData.Add(new KeyValuePair<string, string>("userPwd", passwordHash));
            foreach (var property in properties)
            {
                postData.Add(property);
            }
            string result = await webClient.postLoginFormData(postData);
            if (result.Contains("Benutzername und Kennwort sind nicht gültig"))
            {
                throw new Exception("Login not successful. " + result);
            }
            Console.WriteLine("Login successful");
        }

        private static async Task enableInternetConnection(WebClient webClient, Dictionary<string, string> settings, bool connected)
        {
            Console.WriteLine("Enable internet connection: " + connected);
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("enable", Convert.ToString(connected).ToLower(CultureInfo.CurrentCulture)));
            foreach (var property in settings)
            {
                postData.Add(property);
            }
            postData.Add(new KeyValuePair<string, string>("apply", "Anwenden"));
            string result = await webClient.postInetSettingsFormData(postData);
            Console.WriteLine("Enable internet connection to " + connected + " successful");
        }

        private static string computeHash(string message, string secret)
        {
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var cryptographer = new System.Security.Cryptography.HMACSHA256(keyBytes))
            {
                byte[] bytes = cryptographer.ComputeHash(messageBytes);
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}