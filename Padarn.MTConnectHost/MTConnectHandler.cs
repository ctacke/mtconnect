using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNETCF.Web;

namespace Padarn.MTConnectHost
{
    public class MTConnectHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            string requestData = null;
            if (context.Request.InputStream.Length > 0)
            {
                var buffer = new byte[context.Request.InputStream.Length];
                context.Request.InputStream.Read(buffer, 0, buffer.Length);
                requestData = Encoding.UTF8.GetString(buffer);
            }

            MTConnectService.Instance.Host.HandleHttpGet(
                context.Request.Url,
                requestData,
                context.Response);

        }
    }
}
