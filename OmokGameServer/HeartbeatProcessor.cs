using SuperSocket.SocketBase.Logging;
using System.Threading.Tasks.Dataflow;

namespace OmokGameServer
{
    public class HeartbeatProcessor
    {
        ILog mainLogger;
        Timer heartbeatCheckTimer;
        Func<string, bool> disconnectFunc;

        Dictionary<User, DateTime> lastHeartbeatTime = new Dictionary<User, DateTime>();
        TimeSpan heartbeatLimitTime = TimeSpan.FromSeconds(1);

        public void HandleHeartbeatPacket(object sender, HeartbeatEventArgs e)
        {
            ReceiveHeartbeat(e.User);
        }

        public void AddUserHeartbeatDictionary(User user)
        {
            lastHeartbeatTime[user] = DateTime.Now;
        }
        public void RemoveUserHeartbeatDictionary(User user)
        {
            lastHeartbeatTime.Remove(user);
        }

        public void ReceiveHeartbeat(User user)
        {
            lastHeartbeatTime[user] = DateTime.Now;
        }

        void CheckHeartbeats(object state)
        {
            foreach(var pair in lastHeartbeatTime)
            {
                User user = pair.Key;
                DateTime lastHeartbeatTime = pair.Value;
                TimeSpan timeSinceLastHeartbeat = DateTime.Now - lastHeartbeatTime;
                if(timeSinceLastHeartbeat > heartbeatLimitTime)
                {
                    disconnectFunc(user.sessionId);
                    RemoveUserHeartbeatDictionary(user);
                    mainLogger.Debug($"sessionId({user.sessionId}) Force Close");
                }
            }
        }

        public void ProcessorStart(UserManager userManager, ILog mainLogger, Func<string, bool> disconnectFunc)
        {
            this.mainLogger = mainLogger;
            this.disconnectFunc = disconnectFunc;

            heartbeatCheckTimer = new Timer(CheckHeartbeats, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public void ProcessorStop()
        {
            heartbeatCheckTimer.Dispose();
        }
    }
}