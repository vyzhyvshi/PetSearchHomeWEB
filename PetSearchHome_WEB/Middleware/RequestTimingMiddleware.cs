using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace PetSearchHome_WEB.Middleware;

public class RequestTimingMiddleware
{
 private readonly RequestDelegate _next;
 private readonly ILogger<RequestTimingMiddleware> _logger;

 public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
 {
    _next = next;
    _logger = logger;
 }

 public async Task InvokeAsync(HttpContext context)
 {
    var sw = Stopwatch.StartNew();
    await _next(context);
    sw.Stop();
    var elapsedMs = sw.Elapsed.TotalMilliseconds;
    _logger.LogInformation("Request {Method} {Path} executed in {Elapsed} ms", context.Request.Method, context.Request.Path, elapsedMs);
 }
}
