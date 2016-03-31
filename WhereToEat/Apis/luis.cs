using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Newtonsoft.Json;

namespace WhereToEat.Apis
{
    public static class Luis
    {
        public static LuisResult GetResult(string message)
        {
            string json = "";
            string encodedMessage = HttpUtility.UrlEncode(message);
            using (var client = new WebClient())
            {
                json = client.DownloadString(
                    $"https://api.projectoxford.ai/luis/v1/application?id={PrivateKeys.LUIS_ID}&subscription-key={PrivateKeys.LUIS_SUBSCRIPTION_KEY}&q={encodedMessage}");
            }

            return JsonConvert.DeserializeObject<LuisResult>(json);
        }

        public static Intent GetBestIntent(string message)
        {
            var luisResult = GetResult(message);
            if (luisResult == null) { return null; }

            return luisResult.intents.SingleOrDefault(r => r.score == luisResult.intents.Max(lr => lr.score));
        }
    }

    public class Value
    {
        public string entity { get; set; }
        public string type { get; set; }
        public double score { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public bool required { get; set; }
        public List<Value> value { get; set; }
    }

    public class Action
    {
        public bool triggered { get; set; }
        public string name { get; set; }
        public List<Parameter> parameters { get; set; }

        public Parameter this[string action]
        {
            get { return parameters.SingleOrDefault(p => p.name == action); }
        }
    }

    public class Intent
    {
        public string intent { get; set; }
        public double score { get; set; }
        public List<Action> actions { get; set; }

        public Parameter this[string action]
        {
            get { return actions.FirstOrDefault()?[action]; }
        }
    }

    public class Entity
    {
        public string entity { get; set; }
        public string type { get; set; }
        public int startIndex { get; set; }
        public int endIndex { get; set; }
        public double score { get; set; }
    }

    public class LuisResult
    {
        public string query { get; set; }
        public List<Intent> intents { get; set; }
        public List<Entity> entities { get; set; }
    }

}