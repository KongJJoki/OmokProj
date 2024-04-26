namespace OmokGameServer
{
    public class PacketHandler
    {
        protected MainServer GameServer;
        
        public void Init(MainServer mainServer)
        {
            GameServer = mainServer;
        }
    }
}