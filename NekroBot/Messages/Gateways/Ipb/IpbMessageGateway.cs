namespace NekroBot.Messages.Gateways.Ipb
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Net;
    using System.Net.Cache;
    using System.Net.Http;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Common.Logging;

    internal class IpbMessageGateway : IMessageGateway
    {
        private static readonly ILog Log = LogManager.GetLogger<IpbMessageGateway>();

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

        private readonly Uri origin;

        private readonly Formatter formatter;

        private readonly string userName;

        private readonly string password;

        private readonly HttpClient client;

        private readonly CookieContainer cookieContainer;

        private string session;

        private string secret;

        private int lastChatId;

        public IpbMessageGateway(string origin, string userName, string password, Formatter formatter, MessageGatewayCapabilities capabilities)
        {
            this.origin = new Uri(origin);
            this.userName = userName;
            this.password = password;
            this.formatter = formatter;
            Capabilities = capabilities;

            this.cookieContainer = new CookieContainer();
            WebRequestHandler handler = new WebRequestHandler { UseCookies = true, CachePolicy = new HttpRequestCachePolicy(HttpCacheAgeControl.MaxAge, TimeSpan.Zero) };
            handler.CookieContainer = cookieContainer;
            this.client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("Accept", "text/javascript, text/html, application/xml, text/xml, */*");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            client.DefaultRequestHeaders.Add("Accept-Charset", "utf-8");
            client.DefaultRequestHeaders.Add("Referer", this.origin + "/index.php");
            client.DefaultRequestHeaders.Add("Origin", origin);
            client.DefaultRequestHeaders.Add("X-Prototype-Version", "1.7.1");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.112 Safari/537.36");
            client.DefaultRequestHeaders.ExpectContinue = false;
        }

        public IEnumerable<MessageUpdate> GetMessageUpdates()
        {
            this.EnsureAuth().Wait();

            var chatUpdatesResponse = client.GetAsync(origin + $"/index.php?s={session}&&app=shoutbox&module=ajax&section=coreAjax&secure_key={secret}&type=getShouts&lastid={lastChatId}&global=1").Result;
            chatUpdatesResponse.EnsureSuccessStatusCode();

            Log.Trace(m => m("Запрошены обновления сообщений чата"));

            using (var responseStream = chatUpdatesResponse.Content.ReadAsStreamAsync().Result)
            using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(decompressedStream))
            {
                var chatResponseString = streamReader.ReadToEndAsync().Result;

                if (!string.IsNullOrEmpty(chatResponseString.Trim()))
                {
                    var charResponseToLower = chatResponseString.ToLower();
                    if (charResponseToLower.Contains("error") || charResponseToLower.Contains("nopermission"))
                    {
                        session = null;
                        secret = null;
                        Log.Error(m => m($"Ошибка при получении обновления чата. Ответ сервера: {chatResponseString}"));
                    }
                    else
                    {
                        UpdateLastChatId(chatResponseString);

                        var shouts = Regex.Matches(chatResponseString, @"<a href=""#"" class=""at_member"" data-store=""(?<username>.*?)"".*?<span class='shoutbox_text'>(?<text>.*?)</span>", RegexOptions.Singleline);

                        for (int i = 0; i < shouts.Count; i++)
                        {
                            var shout = shouts[i];
                            if (shout.Success)
                            {
                                var userName = shout.Groups["username"].Value;
                                var text = shout.Groups["text"].Value;

                                Log.Trace(m => m($"Получено новое сообщение в чате от '{userName}' с текстом '{text}'"));

                                yield return new MessageUpdate { Sender = userName, Source = Name, Text = text };
                            }
                        }
                    }
                }
            }
        }

        private void UpdateLastChatId(string response)
        {
            var previousChatId = this.lastChatId;
            var shoutRows = Regex.Matches(response, @"id=(""|')shout-row-(?<rowNumber>\d+)(""|')");
            for (int i = 0; i < shoutRows.Count; i++)
            {
                var row = shoutRows[i];
                if (row.Success)
                {
                    int rowNumber;
                    if (int.TryParse(row.Groups["rowNumber"].Value, out rowNumber))
                    {
                        if (this.lastChatId < rowNumber)
                        {
                            this.lastChatId = rowNumber;
                        }
                    }
                }
            }

            if (previousChatId != this.lastChatId)
            {
                Log.Debug(m => m($"Изменён идентификатор последнего собщения в чате с {previousChatId} на {this.lastChatId}"));
            }
        }

        public async Task Send(MessageUpdate messageUpdate)
        {
            await this.EnsureAuth();

            var postShoutUri = new Uri(this.origin + "index.php?s=" + session + $"&&app=shoutbox&module=ajax&section=coreAjax&secure_key={secret}&type=submit&lastid={lastChatId}&global=1");

            var text = this.formatter.Format(messageUpdate);

            if (string.IsNullOrEmpty(text))
            {
                Log.Trace(m => m($"Отформатированный текст сообщения [{messageUpdate}] является пустым. Сообщение не будет отправлено"));
                return;
            }

            var shoutContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("shout", text) });
            shoutContent.Headers.ContentEncoding.Add("utf-8");

            var shoutResponse = await client.PostAsync(postShoutUri, shoutContent);
            Log.Trace(m => m($"Отправлено сообщение [{messageUpdate}] в чат"));

            shoutResponse.EnsureSuccessStatusCode();

            using (var responseStream = await shoutResponse.Content.ReadAsStreamAsync())
            using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
            using (var streamReader = new StreamReader(decompressedStream))
            {
                var shoutResponseString = await streamReader.ReadToEndAsync();
                var shoutResponseToLower = shoutResponseString.ToLower();
                if (shoutResponseToLower.Contains("error") || shoutResponseToLower.Contains("nopermission"))
                {
                    session = null;
                    secret = null;

                    Log.Error(m => m($"Отправка сообщения в чат завершилась с ошибкой. Ответ: {shoutResponseString}"));
                    
                    await Task.Delay(1000);
                    Log.Debug(m => m("Производится повторная отправка сообщения"));
                    await Send(messageUpdate);
                }
            }
        }

        public string Name { get; } = "IPB";

        public MessageGatewayCapabilities Capabilities { get; }

        private async Task EnsureAuth()
        {
            await Semaphore.WaitAsync();

            try
            {
                if (string.IsNullOrEmpty(session) || string.IsNullOrEmpty(secret))
                {
                    Log.Trace(m => m("Сессия или секретный код не заполнены. Производится их запрос"));

                    var originResponse = await this.client.GetAsync(origin);
                    originResponse.EnsureSuccessStatusCode();

                    if (string.IsNullOrEmpty(session))
                    {
                        var cookieHeaders = originResponse.Headers.Where(h => h.Key == "Set-Cookie");
                        foreach (var cookie in cookieHeaders)
                        {
                            foreach (var cs in cookie.Value)
                            {
                                var cm = Regex.Match(cs, @"session_id=(?<session>\w+);");
                                if (cm.Success)
                                {
                                    session = cm.Groups["session"].Value;
                                    break;
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(session))
                        {
                            Log.Error(m => m("Не возвращено куки с идентификаторм сессии"));
                        }
                    }

                    using (var responseStream = originResponse.Content.ReadAsStreamAsync()
                        .Result)
                    using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(decompressedStream))
                    {
                        var originResponseString = streamReader.ReadToEndAsync()
                            .Result;
                        UpdateLastChatId(originResponseString);
                    }

                    this.cookieContainer.Add(new Cookie("session_id", session, "/", "ipb.local"));
                    this.cookieContainer.Add(new Cookie("coppa", "0", "/", "ipb.local"));
                    this.cookieContainer.Add(new Cookie("rteStatus", "rte", "/", "ipb.local"));

                    var loginUri = new Uri(origin + "index.php?app=core&module=global&section=login&do=process");

                    var authContent = new StringContent($"auth_key=880ea6a14ea49e853634fbdc5015a024&referer=http%3A%2F%2F{this.origin.Host}%2F&ips_username={this.userName}&ips_password={this.password}&rememberMe=1", Encoding.UTF8, "application/x-www-form-urlencoded");
                    authContent.Headers.Add("Upgrade-Insecure-Requests", "1");
                    var response = await client.PostAsync(loginUri, authContent);
                    response.EnsureSuccessStatusCode();

                    Log.Debug(m => m("Произведён логин на сайт"));

                    if (string.IsNullOrEmpty(secret))
                    {
                        string content;
                        using (var responseStream = await response.Content.ReadAsStreamAsync())
                        using (var decompressedStream = new GZipStream(responseStream, CompressionMode.Decompress))
                        using (var streamReader = new StreamReader(decompressedStream))
                        {
                            content = await streamReader.ReadToEndAsync();
                        }

                        var match = Regex.Match(content, @"ipb\.vars\['secure_hash'\]\s*=\s*'(?<secret_hash>\w+)';");
                        secret = match.Groups["secret_hash"].Value;

                        if (string.IsNullOrEmpty(secret))
                        {
                            Log.Error(m => m("Не найден secure_hash"));
                        }

                        this.UpdateLastChatId(content);
                    }

                    if (!string.IsNullOrEmpty(session) && !string.IsNullOrEmpty(secret))
                    {
                        Log.Trace(m => m("Идентификатор сессии и secret_hash успешно заполнены"));
                    }
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }
    }
}