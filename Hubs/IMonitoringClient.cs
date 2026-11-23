using System.Collections.Generic;
using System.Threading.Tasks;

namespace TemperatureBlazorApp.Hubs;

public interface IMonitoringClient
{
    Task ReceiveMessage(string senderName, string targetInfo, string message, string time, bool isPrivate);
    Task UpdateOnlineUsers(Dictionary<string, string> users);
    Task ReceiveSystemNotification(string message);
}
