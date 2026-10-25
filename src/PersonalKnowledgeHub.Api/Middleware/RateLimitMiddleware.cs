using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StackExchange.Redis;

namespace PersonalKnowledgeHub.Middleware
{
    public sealed class RateLimitMiddleware
    {
        private readonly Dictionary<string, (int, int)> _policies = new()
        {
            { "auth", (10, 60_000) },
            { "crud", (100, 60_000) },
            { "expensive", (30, 60_000) },
        };

        private const string IncrementScript = """
           local count = redis.call('INCR', KEYS[1])

           if count == 1 then 
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
           end

           local ttl = redis.call('PTTL', KEYS[1])
           return { count, ttl }
           """;

        private readonly IDatabase _redis;
        private readonly RequestDelegate _next;
        private readonly IProblemDetailsService _problemDetailsService;

        public RateLimitMiddleware(IConnectionMultiplexer connection, RequestDelegate next, IProblemDetailsService problemDetailsService)
        {
            _redis = connection.GetDatabase();
            _next = next;
            _problemDetailsService = problemDetailsService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string policyName = context.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName ?? "crud";
            
            if (!_policies.TryGetValue(policyName, out var policy))
            {
                throw new InvalidOperationException($"Rate limiting policy '{policyName}' is not configured");
            }

            int requestLimit = policy.Item1;
            int ttl = policy.Item2;
            
            string key = $"rate-limit:{policyName}:";

            string identity;

            if (context.User.Identity?.IsAuthenticated == true)
            {
                identity = "user:" + context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                if (context.Connection.RemoteIpAddress != null)
                {
                    identity = "ip:" + context.Connection.RemoteIpAddress;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    var problemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Invalid Request",
                        Detail = "Invalid ip address",
                        Instance = context.Request.Path
                    };

                    var written = await _problemDetailsService.TryWriteAsync(
                        new ProblemDetailsContext
                        {
                            HttpContext = context,
                            ProblemDetails = problemDetails
                        });

                    if (!written)
                    {
                        await context.Response.WriteAsJsonAsync(problemDetails);
                    }
                    
                    return;
                }
            }

            key += identity;

            RedisResult result = await _redis.ScriptEvaluateAsync(
                IncrementScript,
                [ key ],
                [ ttl ]);
            
            RedisResult[] values = (RedisResult[])result!;
            long count = (long)values[0];
            long ttlMilliseconds = Math.Max(0, (long)values[1]);

            if (count > requestLimit)
            {
                context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(ttlMilliseconds / 1000d)).ToString();
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "Too Many Requests",
                    Detail = "Please try again later",
                    Instance = context.Request.Path
                };

                var written = await _problemDetailsService.TryWriteAsync(
                    new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = problemDetails
                    });

                if (!written)
                {
                    await context.Response.WriteAsJsonAsync(problemDetails);
                }

                return;
            }
            
            await _next(context);
        }
    }
}
