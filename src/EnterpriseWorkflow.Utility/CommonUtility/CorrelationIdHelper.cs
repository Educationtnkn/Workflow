using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Utilities.CommonUtility;

public class CorrelationIdHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string HeaderName = "CorrelationId";

    public CorrelationIdHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public string GetCorrelationId(string? correlationId = null)
    {
        // 1. If provided explicitly, use it.
        if (!string.IsNullOrWhiteSpace(correlationId))
            return correlationId;

        // 2. Try from HTTP Context → Request Headers
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.Request?.Headers != null &&
            httpContext.Request.Headers.TryGetValue(HeaderName, out var cidHeader) &&
            !string.IsNullOrWhiteSpace(cidHeader))
        {
            return cidHeader.ToString();
        }

        // 3. If nothing found → generate a NEW Correlation ID
        var newCorrelationId = Guid.NewGuid().ToString();

        // Optional best practice – add the ID to response headers
        if (httpContext?.Response?.Headers != null)
        {
            httpContext.Response.Headers[HeaderName] = newCorrelationId;
        }

        return newCorrelationId;
    }
}
