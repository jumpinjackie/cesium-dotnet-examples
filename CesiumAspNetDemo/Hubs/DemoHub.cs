using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CesiumAspNetDemo.Hubs
{
    [HubName("DemoHub")]
    public class DemoHub : Hub
    {
        public Task Subscribe(string groupName) => Groups.Add(Context.ConnectionId, groupName);

        public Task Unsubscribe(string groupName) => Groups.Remove(Context.ConnectionId, groupName);

        public void SendLocation(string groupName, double longitude, double latitude)
        {
            var userAgent = this.Context.Request.Headers["User-Agent"];
            var parser = UAParser.Parser.GetDefault();
            var clientInfo = parser.Parse(userAgent);

            var response = new
            {
                ClientID = Context.ConnectionId,
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
            Clients.OthersInGroup(groupName).NewLocation(response);
        }
    }
}