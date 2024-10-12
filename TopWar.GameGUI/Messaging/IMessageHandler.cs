using System.IO;
using System.Text.Json.Nodes;

namespace TopWar.GameGUI.Messaging
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(JsonNode jsonNode, StreamWriter writer);
    }
}
