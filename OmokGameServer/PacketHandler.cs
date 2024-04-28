namespace OmokGameServer
{
    public class PacketHandler
    {
        protected MainServer mainServer;
        protected UserManager userManager;
        
        public void Init(MainServer mainServer, UserManager userManager)
        {
            this.mainServer = mainServer;
            this.userManager = userManager;
        }
    }
}