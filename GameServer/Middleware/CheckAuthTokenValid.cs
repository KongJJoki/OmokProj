using GameServer.Repository;
using Microsoft.AspNetCore.Http.Extensions;

namespace GameServer.Middleware
{
    public class CheckAuthTokenValid
    {
        private readonly IRedisDB redisDB;
        private readonly RequestDelegate nextMiddleware;

        public CheckAuthTokenValid(IRedisDB redisDB, RequestDelegate nextMiddleware)
        {
            this.redisDB = redisDB;
            this.nextMiddleware = nextMiddleware;
        }

        public async Task Invoke(HttpContext context)
        {
            var urlString = context.Request.Path.Value;

            if (urlString == "/")
            {
                await nextMiddleware(context);
                return;
            }
            if (string.Compare(urlString, "/GameLogin", StringComparison.OrdinalIgnoreCase) == 0)
            {
                await nextMiddleware(context);
                return;
            }

            string receivedUid;
            string receivedAuthToken;
            (receivedUid, receivedAuthToken) = GetUidAuthTokenFromReq(context);

            if(receivedUid == null || receivedAuthToken == null)
            {
                return;
            }

            string originAuthToken = await redisDB.GetAuthToken(receivedUid);
            if(originAuthToken == null || receivedAuthToken != originAuthToken)
            {
                return;
            }

            await nextMiddleware(context);
        }

        (string, string) GetUidAuthTokenFromReq(HttpContext context)
        {
            string receivedUid;
            if (context.Request.Headers.TryGetValue("Uid", out var uidValue))
            {
                receivedUid = uidValue.FirstOrDefault();
            }
            else
            {
                receivedUid = null;
            }

            string receivedAuthToken;
            if(context.Request.Headers.TryGetValue("AuthToken", out var authTokenValue))
            {
                receivedAuthToken = authTokenValue.FirstOrDefault();
            }
            else
            {
                receivedAuthToken = null;
            }

            return (receivedUid, receivedAuthToken);
        }
    }
}
