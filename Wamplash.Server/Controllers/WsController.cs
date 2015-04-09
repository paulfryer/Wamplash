using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Web.WebSockets;

namespace Wamplash.Server.Controllers
{
    public class WsController : ApiController
    {
    [System.Web.Http.Route("ws")]
        public HttpResponseMessage Get()
        {
            if (HttpContext.Current.IsWebSocketRequest)
            {
                var handler = DependencyResolver.Current.GetService<WebSocketHandler>();
                //var webSocketProtocol = HttpContext.Current.Request.Headers["Sec-WebSocket-Protocol"];
                //if (webSocketProtocol.ToLower() != "wamp.2.json")
                //    throw new Exception("Unsupported websocket protocol!");
                //HttpContext.Current.Response.AddHeader("Sec-WebSocket-Protocol", webSocketProtocol);
                HttpContext.Current.AcceptWebSocketRequest(handler);
                return Request.CreateResponse(HttpStatusCode.SwitchingProtocols);
            }
            return Request.CreateResponse(HttpStatusCode.NotAcceptable);
        }
    }
}