using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Logging;
using NLog;

namespace OmokGameServer
{
    public class NLogLogFactory : LogFactoryBase
    {
        public NLogLogFactory()
            : this("NLog.config")
        {
        }

        public NLogLogFactory(string nlogConfig)
            : base(nlogConfig)
        {
            if (!IsSharedConfig)
            {
                LogManager.Setup().LoadConfigurationFromFile(new[] { ConfigFile });
            }
            else
            {
            }
        }

        public override ILog GetLog(string name)
        {
            return new NLogLogger(LogManager.GetLogger(name));
        }
    }

    public class NLogLogger : ILog
    {
        private Logger _logger;

        public NLogLogger(Logger logger)
        {
            _logger = logger;
        }

        public bool IsDebugEnabled
        {
            get { return _logger.IsDebugEnabled; }
        }

        public bool IsErrorEnabled
        {
            get { return _logger.IsErrorEnabled; }
        }

        public bool IsFatalEnabled
        {
            get { return _logger.IsFatalEnabled; }
        }

        public bool IsInfoEnabled
        {
            get { return _logger.IsInfoEnabled; }
        }

        public bool IsWarnEnabled
        {
            get { return _logger.IsWarnEnabled; }
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }
        public void Error(string message, Exception exception)
        {
            _logger.Error($"msg:{message}, exception:{exception.ToString()}");
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }
        public void Fatal(string message, Exception exception)
        {
            _logger.Fatal($"msg:{message}, exception:{exception.ToString()}");
        }
    }
}