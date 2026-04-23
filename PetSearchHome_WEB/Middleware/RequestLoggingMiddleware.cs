using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PetSearchHome_WEB.Middleware;

public class RequestLoggingMiddleware
{
 private readonly RequestDelegate _next;
 private readonly ILogger<RequestLoggingMiddleware> _logger;

 public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
 {
 _next = next;
 _logger = logger;
 }

 public async Task InvokeAsync(HttpContext context)
 {
 var request = context.Request;
 var method = request.Method;
 var url = request.Path + request.QueryString;
 var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

 // headers
 var headersSb = new StringBuilder();
 foreach (var header in request.Headers)
 {
 headersSb.Append(header.Key);
 headersSb.Append(": ");
 headersSb.AppendLine(header.Value);
 }

 // read body (enable buffering)
 string body = string.Empty;
 if (request.ContentLength.GetValueOrDefault() >0 && request.Body.CanRead)
 {
 request.EnableBuffering();
 using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
 body = await reader.ReadToEndAsync();
 request.Body.Position =0;
 }

 // user id if present
 string? userId = null;
 try
 {
 userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
 }
 catch { }

 _logger.LogInformation("Incoming request {Method} {Url} from {IP}. UserId: {UserId}\nHeaders:\n{Headers}\nBody:\n{Body}",
 method, url, ip, userId ?? "anonymous", headersSb.ToString(), string.IsNullOrWhiteSpace(body) ? "(empty)" : body);

 await _next(context);
 }
}
