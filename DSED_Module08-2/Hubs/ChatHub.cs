using DSED_Module08_2.Controllers;
using Microsoft.AspNetCore.SignalR;

namespace DSED_Module08_2.Hubs
{
    public class ChatHub : Hub
    {
        public static Dictionary<string, string> m_ChatRooms = new Dictionary<string, string>();
        public static Dictionary<string, List<Message>> m_Messages = new Dictionary<string, List<Message>>();

        public async Task CreateJoinChatRoom(string chatRoomName)
        {
            string connexionId = this.Context.ConnectionId;
            if (!m_ChatRooms.ContainsKey(connexionId))
            {
                m_ChatRooms.Add(connexionId, null);
            }
            if (!m_Messages.ContainsKey(chatRoomName))
            {
                m_Messages.Add(chatRoomName, new List<Message>());
            }
            if (m_ChatRooms[connexionId] is not null)
            {
                await Groups.RemoveFromGroupAsync(connexionId, m_ChatRooms[connexionId]);
            }

            await Groups.AddToGroupAsync(connexionId, chatRoomName);
            m_ChatRooms[connexionId] = chatRoomName;
            await Clients.Caller.SendAsync("DemarrageChat", m_Messages[chatRoomName]);
            MettreAJour();
        }
        public async Task SendMessage(Message p_message)
        {
            string connexionId = this.Context.ConnectionId;
            if (m_ChatRooms.ContainsKey(connexionId))
            {
                string nomChatRoom = m_ChatRooms[connexionId];
                m_Messages[nomChatRoom].Add(p_message);
                await Clients.OthersInGroup(nomChatRoom).SendAsync("AfficherMessageAutres", p_message);
                await Clients.Caller.SendAsync("AfficherMessageMoi", p_message);
            }
        }

        public async Task MettreAJour()
        {
            await Clients.All.SendAsync("MettreAJour", m_Messages.Keys);
        }

        public async Task OnConnectedAsync()
        {
            string connexionId = this.Context.ConnectionId;
            if (m_ChatRooms.ContainsKey(connexionId))
            {
                string nomChat = m_ChatRooms[connexionId];
                await Clients.Caller.SendAsync("DemarrageChat", m_Messages[nomChat]);
                await base.OnConnectedAsync();
            }
        }
    }
}
