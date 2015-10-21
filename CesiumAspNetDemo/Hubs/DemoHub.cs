using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CesiumAspNetDemo.Hubs
{
    class UserProfile
    {
        public string Name { get; set; }
        public string Color { get; set; }
    }

    [HubName(nameof(DemoHub))]
    public class DemoHub : Hub
    {
        static string RandomColor()
        {
            var random = new Random();
            return string.Format("#{0:X6}", random.Next(0x1000000));
        }

        public async Task Subscribe(string groupName)
        {
            await Groups.Add(Context.ConnectionId, groupName);
            await Clients.Caller.OnSubscribed(groupName);
        }

        public async Task Unsubscribe(string groupName)
        {
            await Groups.Remove(Context.ConnectionId, groupName);
            await Clients.Caller.OnUnsubscribed(groupName);
        }

        public async Task SendLocation(string groupName, string name, string color, double longitude, double latitude)
        {
            var userAgent = this.Context.Request.Headers["User-Agent"];
            var parser = UAParser.Parser.GetDefault();
            var clientInfo = parser.Parse(userAgent);

            var response = new
            {
                ClientID = Context.ConnectionId,
                Name = name,
                UserColor = color,
                Browser = clientInfo.UserAgent.Family,
                BrowserVersion = $"{clientInfo.UserAgent.Major}.{clientInfo.UserAgent.Minor}.{clientInfo.UserAgent.Patch}",
                OS = clientInfo.OS.Family,
                OSVersion = $"{clientInfo.OS.Major}.{clientInfo.OS.Minor}.{clientInfo.OS.Patch}",
                Device = clientInfo.Device.Family,
                Longitude = longitude,
                Latitude = latitude,
                Date = DateTime.UtcNow
            };

            //Send this information to interested subscribers
            await Clients.OthersInGroup(groupName).NewLocation(response);
        }
    }
}