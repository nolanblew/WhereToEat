using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Utilities;
using Newtonsoft.Json;
using WhereToEat.Apis;

namespace WhereToEat
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        Random _rnd = new Random();

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<Message> Post([FromBody]Message message)
        {
            if (message.Type == "Message")
            {
                try
                {
                    var conversationMessage = HandleChatMessage(message);
                    if (conversationMessage != null)
                    {
                        return conversationMessage;
                    }

                    var luisJson = Luis.GetBestIntent(message.Text);
                    if (luisJson.intent == "None")
                    {
                        return message.CreateReplyMessage("Sorry, I didn't get that. What is it that you find to find?");
                    }

                    var myLocation = message.Location?.Name ?? "San Francisco";

                    // Call Yelp API
                    YelpResult yelpResult = null;
                    switch (luisJson.intent)
                    {
                        case "FindByType":
                            yelpResult = Yelp.GetYelpByType(luisJson["FoodType"].value.First().entity, myLocation);
                            break;
                        case "FindByLocation":
                            var location = luisJson["Location"].value.FirstOrDefault()?.entity;
                            if (string.IsNullOrEmpty(location))
                            {
                                return
                                    message.CreateReplyMessage("Sorry, I didn't get that! What was it that you wanted?");
                            }
                            if (location.Contains("me"))
                            {
                                location = myLocation;
                            }
                            yelpResult = Yelp.GetYelpByType(
                                luisJson["FoodType"].value.First().entity,
                                location);
                            break;
                    }

                    if (yelpResult == null)
                    {
                        return
                            message.CreateReplyMessage(
                                "Sorry I couldn't find anything for you. What would you like to find exactly?");
                    }

                    // return our reply to the user
                    var topRated = yelpResult.businesses[_rnd.Next(0, yelpResult.businesses.Length)];
                    return
                        message.CreateReplyMessage(
                            $"I found a few places you might like. The top rated place place is {topRated.name} {GetLocationString(topRated.location)}. It has {topRated.rating} stars.\nYou can find out more at: {topRated.url}.");
                }
                catch
                {
                    // Something failed. Let's see if it's in one of our pre-defined messages
                    var chatMessage = HandleChatMessage(message);
                    if (chatMessage != null) { return chatMessage; }

                    return message.CreateReplyMessage(
                        "Oh sorry, I was blanking out. What was it that you wanted again?");
                }
            }
            else
            {
                return HandleSystemMessage(message);
            }
        }

        Message HandleChatMessage(Message message)
        {
            var txt = message.Text.ToLower().Trim();

            if (txt.Length < 10 && (txt.Contains("hi") || txt.Contains("hey") || txt.Contains("hello")))
            {
                return message.CreateReplyMessage("Hey there! What can I find for you?");
            }
            if (txt == "food" || txt.Contains("hungry"))
            {
                var yelpResult = Yelp.GetYelpByType("food", "San Francisco");
                var topRated = yelpResult.businesses[_rnd.Next(0, yelpResult.businesses.Length)];

                return
                    message.CreateReplyMessage(
                        $"I found a few places you might like. The top rated place place is {topRated.name} off of {GetLocationString(topRated.location)}. It has {topRated.rating} stars.\nYou can find out more at: {topRated.url}.");
            }
            if (txt.Contains("how are you") || txt.Contains("how r u"))
            {
                return message.CreateReplyMessage("I'm great! Where can I direct you today?");
            }

            return null;
        }

        string GetLocationString(Apis.Location location)
        {
            if (!string.IsNullOrEmpty(location.cross_streets))
            {
                return $"off of {location.cross_streets}";
            }
            StringBuilder sb = new StringBuilder();
            if (location.display_address.Any())
            {
                sb.Append(location.display_address.First() + ", ");
            }
            for (int i = 1; i < location.display_address.Length; i++)
            {
                var address = location.display_address[i];
                sb.Append(address + " ");
            }

            return $"at {sb.ToString()}";
        }

        private Message HandleSystemMessage(Message message)
        {
            if (message.Type == "Ping")
            {
                Message reply = message.CreateReplyMessage();
                reply.Type = "Ping";
                return reply;
            }
            else if (message.Type == "DeleteUserData")
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == "BotAddedToConversation")
            {
            }
            else if (message.Type == "BotRemovedFromConversation")
            {
            }
            else if (message.Type == "UserAddedToConversation")
            {
                return message.CreateReplyMessage("Hey there. What are you looking for?");
            }
            else if (message.Type == "UserRemovedFromConversation")
            {
            }
            else if (message.Type == "EndOfConversation")
            {
            }

            return null;
        }
    }
}