using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace VV2220Reconnector
{
    internal class WebClient
    {
        private readonly HttpClient client;
        private readonly string loginUri;
        private readonly string inetSettingsUri;

        internal WebClient(HttpClient client, string basicModemUri)
        {
            this.client = client;
            this.loginUri = basicModemUri + "ui/login";
            this.inetSettingsUri = basicModemUri + "ui/dboard/html/wan?edit=wan&if=2";
        }

        internal async Task<Dictionary<string, string>> loadLoginPage()
        {
            var loginPageTask = client.GetStringAsync(loginUri);

            var loginPage = await loginPageTask;
            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(loginPage);
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["nonce"] = getFieldValue(pageDocument, "//*[@id=\"content\"]/form/input[2]");
            result["code1"] = getFieldValue(pageDocument, "//*[@id=\"content\"]/form/input[3]");
            result["code3"] = getFieldValue(pageDocument, "//*[@id=\"content\"]/form/input[4]");
            return result;
        }

        internal async Task<Dictionary<string, string>> loadInetConnectionPage()
        {
            var settingsPage = await client.GetStringAsync(this.inetSettingsUri);

            HtmlDocument pageDocument = new HtmlDocument();
            pageDocument.LoadHtml(settingsPage);
            Dictionary<string, string> result = new Dictionary<string, string>();
            result["action__key"] = getFieldValue(pageDocument, "//*[@id=\"myform\"]/div/input[1]");
            result["username"] = getFieldValue(pageDocument, "//*[@id=\"username\"]/input");
            result["password"] = getFieldValue(pageDocument, "//*[@id=\"password\"]/div/input");
            return result;
        }

        internal async Task<string> postLoginFormData(List<KeyValuePair<string, string>> postData)
        {
            return await postFormData(this.loginUri, postData);
        }

        internal async Task<string> postInetSettingsFormData(List<KeyValuePair<string, string>> postData)
        {
            return await postFormData(this.inetSettingsUri, postData);
        }

        private async Task<string> postFormData(string uri, List<KeyValuePair<string, string>> postData)
        {
            using (var content = new FormUrlEncodedContent(postData))
            {
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string getFieldValue(HtmlDocument pageDocument, string xPath)
        {
            HtmlNode fieldNode = pageDocument.DocumentNode.SelectSingleNode(xPath);
            return fieldNode.Attributes["value"].Value;
        }
    }
}