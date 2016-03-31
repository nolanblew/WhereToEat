using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleOAuth;

namespace WhereToEat.Apis
{
    public class Yelp
    {
        private const string API_HOST = "https://api.yelp.com";

        /// <summary>
        /// Relative path for the Search API.
        /// </summary>
        private const string SEARCH_PATH = "/v2/search/";

        /// <summary>
        /// Relative path for the Business API.
        /// </summary>
        private const string BUSINESS_PATH = "/v2/business/";

        /// <summary>
        /// Search limit that dictates the number of businesses returned.
        /// </summary>
        private const int SEARCH_LIMIT = 3;

        /// <summary>
        /// Prepares OAuth authentication and sends the request to the API.
        /// </summary>
        /// <param name="baseURL">The base URL of the API.</param>
        /// <param name="queryParams">The set of query parameters.</param>
        /// <returns>The JSON response from the API.</returns>
        /// <exception>Throws WebException if there is an error from the HTTP request.</exception>
        static JObject PerformRequest(string baseURL, Dictionary<string, string> queryParams = null)
        {
            var query = System.Web.HttpUtility.ParseQueryString(String.Empty);

            if (queryParams == null)
            {
                queryParams = new Dictionary<string, string>();
            }

            foreach (var queryParam in queryParams)
            {
                query[queryParam.Key] = queryParam.Value;
            }

            var uriBuilder = new UriBuilder(baseURL);
            uriBuilder.Query = query.ToString();

            var request = WebRequest.Create(uriBuilder.ToString());
            request.Method = "GET";

            request.SignRequest(
                new Tokens
                {
                    ConsumerKey = PrivateKeys.YELP_PUBLIC_KEY,
                    ConsumerSecret = PrivateKeys.YELP_PRIVATE_KEY,
                    AccessToken = PrivateKeys.YELP_PUBLIC_TOKEN,
                    AccessTokenSecret = PrivateKeys.YELP_PRIVATE_TOKEN
                }
            ).WithEncryption(EncryptionMethod.HMACSHA1).InHeader();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            return JObject.Parse(stream.ReadToEnd());
        }


        public static YelpResult GetYelpByType(string type, string location)
        {
            var query = new Dictionary<string, string>()
            {
                { "term", type },
                { "location", location },
                { "limit", SEARCH_LIMIT.ToString() }
            };

            var jResult = PerformRequest(API_HOST + SEARCH_PATH, query);
            var yelpResult = jResult.ToObject<YelpResult>();

            return yelpResult;
        }
    }


    public class YelpResult
    {
        public Region region { get; set; }
        public int total { get; set; }
        public Business[] businesses { get; set; }
    }

    public class Region
    {
        public Span span { get; set; }
        public Center center { get; set; }
    }

    public class Span
    {
        public float latitude_delta { get; set; }
        public float longitude_delta { get; set; }
    }

    public class Center
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
    }

    public class Business
    {
        public bool is_claimed { get; set; }
        public float rating { get; set; }
        public string mobile_url { get; set; }
        public string rating_img_url { get; set; }
        public int review_count { get; set; }
        public string name { get; set; }
        public string rating_img_url_small { get; set; }
        public string url { get; set; }
        public string[][] categories { get; set; }
        public int menu_date_updated { get; set; }
        public string phone { get; set; }
        public string snippet_text { get; set; }
        public string image_url { get; set; }
        public string snippet_image_url { get; set; }
        public string display_phone { get; set; }
        public string rating_img_url_large { get; set; }
        public string menu_provider { get; set; }
        public string id { get; set; }
        public bool is_closed { get; set; }
        public Location location { get; set; }
    }

    public class Location
    {
        public string cross_streets { get; set; }
        public string city { get; set; }
        public string[] display_address { get; set; }
        public float geo_accuracy { get; set; }
        public string[] neighborhoods { get; set; }
        public string postal_code { get; set; }
        public string country_code { get; set; }
        public string[] address { get; set; }
        public Coordinate coordinate { get; set; }
        public string state_code { get; set; }
    }

    public class Coordinate
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
    }
}