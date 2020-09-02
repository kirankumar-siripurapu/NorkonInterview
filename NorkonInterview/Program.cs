using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NorkonInterview
{
    class Program
    {
        private static int Counter = 0;
        private static string cookieData = string.Empty;
        private static string[] cookiekeys;
        private static List<string> cookies;
        private static HttpClient client = new HttpClient();

        private static NorkonServerInfo norkonServerInfo;
        public static void Main(string[] args)
        {
            try
            {
                /*Test Code*/

                // string s = @"{'server':'17cf5910c7b22ce5426b4142b3486f89fac148ae0051ded962b5d0febddea8b4','serverName':'dninvestornorway','process':{'cpu':15144332.03,'uptime':151660888,'totalMemory':5354264976,'workingSet':8836395008,'peakWorkingSet':12241920000},'stats':{'dnApiCalls':7290,'iotCalls':0},'liveCenter':{'connected':49,'requests':7941},'quantHubs':{'connected':2458,'reconnected':0,'totalConnected':441180,'categories':[['overview',1230],['valuta',68],['ticker',909],['aksjer',118],['importance',4],['portfolio',51],[null,14],['2020corona',4],['aksjonaer',23],['allTrades',4],['analyze',20],['tegningsretter',0],['ek',2],['indekser',1],['ravarer',1],['intl-aksjer',0],['etner',0],['sub',1],['signals',2],['favtickers',3],['fond',3],['renter',0],['etfer',0],['warranter',0]]},'rt':{'users':248,'connected':369},'fallbackMgr':{'primaryReady':true,'fallbackReady':true},'reqCount':{'http':2720505,'normal':687652,'rt':269709},'updates':{'frag':176185735,'channel':445116,'area':33730,'rtFrag':178244115,'rtChannel':956616,'rtArea':175319},'mitoRequests':0}";

                // string s2 = @"{ 'uptime': 100, 'serverName': 'vinz1'}";


                /*End of test code*/

                var appsettings = ConfigurationManager.AppSettings;
                cookiekeys = appsettings.AllKeys;

                cookies = new List<string>();

                foreach (string cookieKey in cookiekeys)
                {
                    cookies.Add(appsettings[cookieKey]);
                }

                // Get Stats of All Servers
                LoadCumulativeServerstats();
                //Timer t = new Timer(TimerCallback, null, 0, 1000);
                var keyStroke = Console.ReadKey(false);
                HandleToggleKeys(keyStroke.Key);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Retriving and populating the cummulative server data
        private static async void LoadCumulativeServerstats(CancellationToken cancellationToken = default)
        {
            Console.WriteLine("cumulative server metric loading... ");
            var getServerTasks = new List<Task<NorkonServerInfo>>();
            for(int i = 0; i < cookiekeys.Length; i++)
            {
                Task<NorkonServerInfo> norkonServerTask = GetServerStats(cookies[i], cancellationToken);
                getServerTasks.Add(norkonServerTask);
            }

            var listNorkenServerInfo = await Task.WhenAll(getServerTasks);
            foreach (NorkonServerInfo norkonServerInfoItem in listNorkenServerInfo)
            {
                norkonServerInfo.Updates.Frag += norkonServerInfoItem.Updates.Frag;
                norkonServerInfo.Process.Uptime += norkonServerInfoItem.Process.Uptime;
                norkonServerInfo.QuantHubs.Connected += norkonServerInfoItem.QuantHubs.Connected;
                norkonServerInfo.HttpRecCount.Http += norkonServerInfoItem.HttpRecCount.Http;

                norkonServerInfo.FallbackMgr.FallbackReady = norkonServerInfo.FallbackMgr.FallbackReady &&
                    norkonServerInfoItem.FallbackMgr.FallbackReady;
            }
            norkonServerInfo.ServerName = "ALL";

            DisplayServerStats(norkonServerInfo);
        }

        //Displaying the Server info on the console
        private static void DisplayServerStats(NorkonServerInfo serverInfo)
        {
            Console.Clear();
            if (serverInfo.ServerName.ToUpper() == "ALL")
            {
                Console.ForegroundColor = ConsoleColor.Black;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write(serverInfo.ServerName + " ");
                Console.ResetColor();
            }
            else
            {
                Console.Write("ALL ");
            }
            foreach (string cookieKey in cookiekeys)
            {
                if (cookieKey.Substring(6, 4).ToUpper() == serverInfo.ServerName.ToUpper())
                {
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.Write(cookieKey.Substring(6, 4) + " ", ConsoleColor.Green);
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(cookieKey.Substring(6, 4) + " ");
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Server Name: " + serverInfo.ServerName);
            Console.WriteLine("Connection Count: " + serverInfo.QuantHubs.Connected.ToString());
            TimeSpan timespan = TimeSpan.FromSeconds(serverInfo.Process.Uptime);
            string Uptime = string.Format("{0:D2}:{1:D2}:{2:D2}:{2:D2}",
                            timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
            Console.WriteLine("Uptime: " + Uptime);
            Console.WriteLine("Fragment Updates : " + serverInfo.Updates.Frag.ToString());
            Console.WriteLine("Http Request Count : " + serverInfo.HttpRecCount.Http.ToString());
            Console.WriteLine("Fallback ready  : " + serverInfo.FallbackMgr.FallbackReady.ToString());

            Console.WriteLine("");
            Console.WriteLine("use only <- or -> to toggle between servers or ESC to exit ");
        }

        private static async void LoadNextOrPreviousServerStats(CancellationToken cancellationToken = default)
        {
            if (Counter == 0)
            {
                Console.Clear();
                LoadCumulativeServerstats(cancellationToken);
            }
            else
            {
                norkonServerInfo = await GetServerStats(cookies[Counter - 1], cancellationToken);
                Console.Clear();
                DisplayServerStats(norkonServerInfo);
            }
            var keyStroke = Console.ReadKey(false);
            HandleToggleKeys(keyStroke.Key);
        }


        //Fetching the norkonServerInfo .net object by deserializing Json text by fetching from URL
        private static async Task<NorkonServerInfo> GetServerStats(string cookie, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                client.CancelPendingRequests();
            client.DefaultRequestHeaders.Remove("Cookie");
            client.DefaultRequestHeaders.Add("Cookie", cookie);
            var response = await client.GetAsync("https://investor.dn.no/JsonServer/GetStats");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            norkonServerInfo = JsonConvert.DeserializeObject<NorkonServerInfo>(content);
            return norkonServerInfo;
        }


        //Navigating from the servers by clicking left and right arrow Keys 
        private static void HandleToggleKeys(ConsoleKey key)
        {
            CancellationToken cancellationToken = new CancellationToken(true);
            switch (key)
            {
                case ConsoleKey.RightArrow:
                    Console.Clear();
                    Console.WriteLine("next server info loading...");

                    if (Counter < cookiekeys.Length)
                    {
                        Counter++;
                    }
                    else
                    {
                        Counter = 0;
                    }
                    LoadNextOrPreviousServerStats(cancellationToken);
                    break;
                case ConsoleKey.LeftArrow:

                    Console.Clear();
                    Console.WriteLine("previous server info loading...");
                    if (Counter > 0)
                        Counter--;
                    else if (Counter == 0)
                    {
                        Counter = cookiekeys.Length;
                    }
                    LoadNextOrPreviousServerStats(cancellationToken);
                    break;
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("use only <- or -> to toggle between servers or ESC to exit ");
                    var keyHit = Console.ReadKey(false);
                    HandleToggleKeys(keyHit.Key);
                    break;
            }
        }

        //Timer call back call at the interval of 1 second
        private static async void TimerCallback(Object o)
        {
            Console.WriteLine("Refreshing...");

            if (Counter == 0)
            {
                LoadCumulativeServerstats();
            }
            else
            {
                 norkonServerInfo = await GetServerStats(cookies[Counter - 1]);
                 DisplayServerStats(norkonServerInfo);
            }
            GC.Collect();
        }

    }
}
