namespace NexHire.API.Middleware;
public class SecurityHeadersMiddleware
{
 private readonly RequestDelegate _next;public SecurityHeadersMiddleware(RequestDelegate next)=>_next=next;
 public async Task InvokeAsync(HttpContext c){c.Response.Headers["X-Content-Type-Options"]="nosniff";c.Response.Headers["X-Frame-Options"]="DENY";c.Response.Headers["Referrer-Policy"]="no-referrer";await _next(c);}
}
