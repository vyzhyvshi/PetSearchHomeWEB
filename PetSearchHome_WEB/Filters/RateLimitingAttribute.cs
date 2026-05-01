using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Concurrent;

namespace PetSearchHome_WEB.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RateLimitingAttribute : ActionFilterAttribute
    {
        private static readonly ConcurrentDictionary<string, RequestBucket> RequestsByIpAddress = new();
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        public RateLimitingAttribute(int maxRequestsPerMinute)
        {
            if (maxRequestsPerMinute <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRequestsPerMinute), "Request limit must be greater than zero.");
            }

            MaxRequestsPerMinute = maxRequestsPerMinute;
        }

        public int MaxRequestsPerMinute { get; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var now = DateTimeOffset.UtcNow;
            var ipAddress = GetClientIpAddress(context);
            var bucket = RequestsByIpAddress.GetOrAdd(ipAddress, static _ => new RequestBucket());

            if (!bucket.TryRegisterRequest(now, Window, MaxRequestsPerMinute))
            {
                context.Result = new RedirectToActionResult("RateLimitExceeded", "Home", null);
                return;
            }

            base.OnActionExecuting(context);
        }

        private static string GetClientIpAddress(ActionExecutingContext context)
        {
            return context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        private sealed class RequestBucket
        {
            private readonly Queue<DateTimeOffset> _timestamps = new();
            private readonly object _syncRoot = new();

            public bool TryRegisterRequest(DateTimeOffset now, TimeSpan window, int maxRequests)
            {
                lock (_syncRoot)
                {
                    while (_timestamps.Count > 0 && now - _timestamps.Peek() >= window)
                    {
                        _timestamps.Dequeue();
                    }

                    if (_timestamps.Count >= maxRequests)
                    {
                        return false;
                    }

                    _timestamps.Enqueue(now);
                    return true;
                }
            }
        }
    }
}
