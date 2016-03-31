using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WhereToEat
{
    public static class PrivateKeys
    {
        static PrivateKeys()
        {
            LoadKeys();
        }

        /// <summary>
        /// Load the private keys from file, if available
        /// </summary>
        public static void LoadKeys()
        {
            try
            {
                var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("WhereToEat.data.pvkeys");

                string json = "";
                using (var sr = new StreamReader(file))
                {
                    json = sr.ReadToEnd();
                }
                file.Close();

                dynamic deserialized = JObject.Parse(json);
                LUIS_SUBSCRIPTION_KEY = deserialized.luis_private;
                YELP_PRIVATE_KEY = deserialized.yelp_private;
                YELP_PRIVATE_TOKEN = deserialized.yelp_token_private;
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Cannot access private keys");
            }
        }

        public static string LUIS_ID { get; } = "d4742f33-b914-4d61-a259-493f6d010d93";

        public static string LUIS_SUBSCRIPTION_KEY { get; private set; } = "";

        public static string YELP_PRIVATE_KEY { get; private set; } = "";

        public static string YELP_PUBLIC_KEY { get; } = "gTnxVz6rYVMfAUPzJN1R8g";

        public static string YELP_PRIVATE_TOKEN { get; private set; } = "";

        public static string YELP_PUBLIC_TOKEN { get; } = "AZS02lonjrKG_4LUDbNznGKNdJKz7P4h";
    }
}