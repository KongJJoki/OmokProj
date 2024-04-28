namespace OmokGameServer
{
    public class PacketHandler
    {
        protected UserManager userManager;

        public static Func<string, byte[], bool> sendFunc;
        public void Init(UserManager userManager)
        {
            this.userManager = userManager;
        }
    }
}