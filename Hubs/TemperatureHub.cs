using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace TemperatureBlazorApp.Hubs;

public class TemperatureHub : Hub<IMonitoringClient>
{
    private static readonly ConcurrentDictionary<string, string> OnlineUsers = new();

    public override async Task OnConnectedAsync()
    {
        var defaultName = $"Гость_{Context.ConnectionId[..4]}";
        OnlineUsers[Context.ConnectionId] = defaultName;

        await Clients.All.UpdateOnlineUsers(new Dictionary<string, string>(OnlineUsers));
        await Clients.All.ReceiveSystemNotification($"{defaultName} зашел на сервер.");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (OnlineUsers.TryRemove(Context.ConnectionId, out var userName))
        {
            await Clients.All.UpdateOnlineUsers(new Dictionary<string, string>(OnlineUsers));
            await Clients.All.ReceiveSystemNotification($"{userName} вышел из чата.");
        }
        await base.OnDisconnectedAsync(exception);
    }

    // Регистрация / смена имени пользователя
    public async Task RegisterUser(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName)) return;

        var oldName = OnlineUsers.GetValueOrDefault(Context.ConnectionId, "Гость");
        OnlineUsers[Context.ConnectionId] = userName.Trim();

        await Clients.All.UpdateOnlineUsers(new Dictionary<string, string>(OnlineUsers));
        await Clients.All.ReceiveSystemNotification($"Пользователь «{oldName}» теперь известен как «{userName.Trim()}».");
    }

    // Вход в чат-комнату (группу)
    public async Task JoinRoom(string roomName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        var user = OnlineUsers.GetValueOrDefault(Context.ConnectionId, "Пользователь");
        await Clients.Group(roomName).ReceiveSystemNotification($"{user} вошел в комнату «{roomName}».");
    }

    // Выход из чат-комнаты
    public async Task LeaveRoom(string roomName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        var user = OnlineUsers.GetValueOrDefault(Context.ConnectionId, "Пользователь");
        await Clients.Group(roomName).ReceiveSystemNotification($"{user} покинул комнату «{roomName}».");
    }

    // Отправка сообщения в комнату (группу)
    public async Task SendToRoom(string roomName, string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        var sender = OnlineUsers.GetValueOrDefault(Context.ConnectionId, "Аноним");
        var time = DateTime.Now.ToString("HH:mm:ss");

        await Clients.Group(roomName).ReceiveMessage(sender, $"Комната: {roomName}", message, time, false);
    }

    // Адресная отправка конкретному клиенту (личное сообщение)
    public async Task SendPrivate(string targetConnectionId, string message)
    {
        if (string.IsNullOrWhiteSpace(targetConnectionId) || string.IsNullOrWhiteSpace(message)) return;

        var sender = OnlineUsers.GetValueOrDefault(Context.ConnectionId, "Аноним");
        var receiver = OnlineUsers.GetValueOrDefault(targetConnectionId, "Получатель");
        var time = DateTime.Now.ToString("HH:mm:ss");

        await Clients.Client(targetConnectionId).ReceiveMessage(sender, "Лично вам", message, time, true);
        if (targetConnectionId != Context.ConnectionId)
        {
            await Clients.Caller.ReceiveMessage($"Вы -> {receiver}", "Личное сообщение", message, time, true);
        }
    }
}
