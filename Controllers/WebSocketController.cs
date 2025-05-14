using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using WebSocketRabbitMQ.Services;

[Route("api/websocket")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly RabbitMQService _rabbitMqService;
    private static List<WebSocket> _connectedSockets = new();

    public WebSocketController(RabbitMQService rabbitMqService)
    {
        _rabbitMqService = rabbitMqService;
        _rabbitMqService.Subscribe(SendMessageToWebSocketsAsync);
    }

    [HttpGet]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _connectedSockets.Add(webSocket);
            Console.WriteLine("🔗 WebSocket connected.");

            await ListenForMessagesAsync(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = 400;
        }
    }

    private async Task SendMessageToWebSocketsAsync(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var socket in _connectedSockets)
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    private async Task ListenForMessagesAsync(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];
        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                _connectedSockets.Remove(webSocket);
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                Console.WriteLine("❌ WebSocket disconnected.");
            }
        }
    }
}