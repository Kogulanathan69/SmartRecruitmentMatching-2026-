namespace NexHire.API.Middleware;
public class CorrelationIdMiddleware
{
 private readonly RequestDelegate _next;public CorrelationIdMiddleware(RequestDelegate next)=>_next=next;
 public async Task InvokeAsync(HttpContext c){const string h="X-Correlation-ID";var id=c.Request.Headers[h].FirstOrDefault()??Guid.NewGuid().ToString("N");c.TraceIdentifier=id;c.Response.Headers[h]=id;await _next(c);}
}
