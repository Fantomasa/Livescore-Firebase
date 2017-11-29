using LiveScore.Classes;
using LiveScore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace LiveScore
{
    static class Program
    {
        public const string API_KEY = "AIzaSyDtVnmpuWoPlpbJjANjkUgbcjrXEcNyNjk";
        public static string AUTH_TOKEN = String.Empty;
        private const string LIVE_URL = "https://livescore-9a96e.firebaseio.com/Data.json?auth=";
        private const string TODAY_URL = "https://livescore-9a96e.firebaseio.com/Today.json?auth=";
        private const string RESULT_URL = "https://livescore-9a96e.firebaseio.com/Result.json?auth=";
        private const string PROGRAM_URL = "https://livescore-9a96e.firebaseio.com/Program.json?auth=";

        private static RequestData requestLiveData;
        private static RequestData requestTodayData;
        private static RequestData requestProgramData;

        private static List<Event> liveEventList = new List<Event>();
        private static List<Event> todayEventList = new List<Event>();
        private static List<Event> resultEventList = new List<Event>();

        private static bool isFirstLiveData = true;
        private static bool isFirstTodayData = true;

        public static async Task ClearData()
        {
            await requestLiveData.ClearData(AUTH_TOKEN, LIVE_URL, API_KEY);
            await requestTodayData.ClearData(AUTH_TOKEN, TODAY_URL, API_KEY);
        }

        public static async Task ClearResult()
        {
            await requestLiveData.ClearData(AUTH_TOKEN, RESULT_URL, API_KEY);
        }

        public static async void StartAsync()
        {
            requestProgramData = new RequestData("true", "false", "false");
            await requestProgramData.GetDataAsync();
            await requestProgramData.CheckAuthToken();
            await UploadData(requestProgramData, PROGRAM_URL);

            while (true)
            {
                //change the comment to check commit
                await RequestLiveEventsAsync();
                await RequestTodayEventsAsync();
                await Task.Delay(3000);
            }
        }

        private static async Task RequestTodayEventsAsync()
        {
            requestTodayData = new RequestData("true", "false", "true");
            await requestTodayData.CheckAuthToken();

            if (isFirstTodayData)
            {
                todayEventList = await requestTodayData.GetDataAsync();

                if (todayEventList == null)
                    return;

                await UploadData(requestTodayData, TODAY_URL);
                isFirstTodayData = false;
                return;
            }

            //new check diff
            List<Event> oldData = todayEventList.ToList();
            List<Event> newData = await requestTodayData.GetDataAsync();
            if(newData == null)            
                return;

            todayEventList.Clear();
            todayEventList = newData.ToList();

            if (oldData.Count != newData.Count)
            {
                todayEventList = newData.ToList();
                await UploadData(requestTodayData, TODAY_URL);
                return;
            }

            for (int eventIdx = 0; eventIdx < newData.Count; eventIdx++)
            {
                if (oldData[eventIdx].Id == newData[eventIdx].Id)
                {
                    if (!oldData[eventIdx].StartTime.Equals(newData[eventIdx].StartTime))
                    {
                        todayEventList = newData.ToList();
                        await UploadData(requestTodayData, TODAY_URL);
                        break;
                    }
                }
            }
        }

        private static async Task RequestLiveEventsAsync()
        {
            requestLiveData = new RequestData("false", "true", "false");
            await requestLiveData.CheckAuthToken();

            //Upload first data
            if (isFirstLiveData)
            {
                liveEventList = await requestLiveData.GetDataAsync();

                if (liveEventList == null)
                    return;

                await UploadData(requestLiveData, LIVE_URL);
                isFirstLiveData = false;
                return;
            }

            //new check diff
            List<Event> oldData = liveEventList.ToList();
            List<Event> newData = await requestLiveData.GetDataAsync();
            if (newData == null)
                return;

            liveEventList.Clear();
            liveEventList = newData.ToList();

            //check for finished events
            await CheckEventsResult(oldData, newData);

            if (oldData.Count != newData.Count)
            {
                liveEventList = newData.ToList();
                await UploadData(requestLiveData, LIVE_URL);
                return;
            }

            for (int eventIdx = 0; eventIdx < newData.Count; eventIdx++)
            {
                if (oldData[eventIdx].Id == newData[eventIdx].Id)
                {
                    if (!oldData[eventIdx].HomeGoals.Equals(newData[eventIdx].HomeGoals) ||
                        !oldData[eventIdx].AwayGoals.Equals(newData[eventIdx].AwayGoals) ||
                        !oldData[eventIdx].HomeCorners.Equals(newData[eventIdx].HomeCorners) ||
                        !oldData[eventIdx].AwayCorners.Equals(newData[eventIdx].AwayCorners)
                       )
                    {
                        liveEventList = newData.ToList();
                        await UploadData(requestLiveData, LIVE_URL);
                        break;
                    }
                }
            }
        }

        private static async Task CheckEventsResult(List<Event> oldData, List<Event> newData)
        {
            List<Event> resultEventList = new List<Event>();

            foreach (Event oldEvent in oldData)
            {
                bool hasEvent = false;

                foreach (Event newEvent in newData)
                {
                    if (oldEvent.Id == newEvent.Id)
                    {
                        hasEvent = true;
                        break;
                    }
                }

                if (!hasEvent)
                {
                    string playTime = oldEvent.PlayTime.ToLower();
                    if (playTime.StartsWith("2h"))
                    {
                        playTime = playTime.Remove(0, playTime.IndexOf(" ")).Replace("'", "").Trim();
                        int min = 0;
                        bool hasMin = int.TryParse(playTime, out min);
                        if (hasMin)
                        {
                            if (min >= 45)
                            {
                                resultEventList.Add(oldEvent);
                            }
                        }
                    }
                }
            }

            if(resultEventList.Count > 0)
            {
                RequestData resultData = new RequestData("false", "false", "false");
                resultData.eventList = resultEventList.ToList();
                await UploadData(resultData, RESULT_URL);
            }
        }

        private static async Task UploadData(RequestData requestData, string url)
        {
            try
            {
                await requestData.UploadDataAsync(AUTH_TOKEN, url);
            }
            catch (WebException e)
            {
                AUTH_TOKEN = String.Empty;
                AUTH_TOKEN = await requestLiveData.ConnectApiAsync(API_KEY);
            }
            catch (Exception e)
            {
                Application.Restart();
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            StartAsync();
            Application.Run(new Form1());
        }
    }
}
