namespace Patterns.Creational;

/// <summary>
/// Demonstrates the Builder pattern for constructing complex objects step-by-step.
///
/// Use Builder when:
/// • A class has many optional parameters (avoid telescoping constructors).
/// • Construction requires multiple steps that must be ordered.
/// • You want to reuse the same construction process for different representations.
///
/// Modern C# alternative: use `init` properties and object initialiser syntax
/// for simple objects. Use a dedicated builder for complex multi-step construction.
/// </summary>
public static class BuilderPattern
{
    // ── Product ───────────────────────────────────────────────────────────────
    public sealed class HttpRequest
    {
        public string                      Method  { get; init; } = "GET";
        public string                      Url     { get; init; } = "/";
        public IReadOnlyDictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();
        public string?                     Body    { get; init; }
        public int                         TimeoutMs { get; init; } = 30_000;

        public override string ToString() =>
            $"{Method} {Url} (timeout={TimeoutMs}ms, headers={Headers.Count}, body={Body?.Length ?? 0}B)";
    }

    // ── Builder ───────────────────────────────────────────────────────────────
    // Fluent API: each method returns `this` so calls can be chained.
    public sealed class HttpRequestBuilder
    {
        private string                       _method    = "GET";
        private string                       _url       = "/";
        private readonly Dictionary<string, string> _headers = [];
        private string?                      _body;
        private int                          _timeoutMs = 30_000;

        public HttpRequestBuilder WithMethod(string method)
        {
            _method = method;
            return this;
        }

        public HttpRequestBuilder WithUrl(string url)
        {
            _url = url;
            return this;
        }

        public HttpRequestBuilder WithHeader(string name, string value)
        {
            _headers[name] = value;
            return this;
        }

        public HttpRequestBuilder WithBearerToken(string token) =>
            WithHeader("Authorization", $"Bearer {token}");

        public HttpRequestBuilder WithJsonBody(string json) =>
            WithHeader("Content-Type", "application/json").WithBody(json);

        public HttpRequestBuilder WithBody(string body)
        {
            _body = body;
            return this;
        }

        public HttpRequestBuilder WithTimeout(TimeSpan timeout)
        {
            _timeoutMs = (int)timeout.TotalMilliseconds;
            return this;
        }

        public HttpRequest Build()
        {
            if (string.IsNullOrWhiteSpace(_url))
                throw new InvalidOperationException("URL must be set before building.");

            return new HttpRequest
            {
                Method    = _method,
                Url       = _url,
                Headers   = new Dictionary<string, string>(_headers),
                Body      = _body,
                TimeoutMs = _timeoutMs,
            };
        }
    }

    // ── Usage ─────────────────────────────────────────────────────────────────
    public static HttpRequest BuildApiRequest(string token, string resourceUrl, string jsonBody) =>
        new HttpRequestBuilder()
            .WithMethod("POST")
            .WithUrl(resourceUrl)
            .WithBearerToken(token)
            .WithJsonBody(jsonBody)
            .WithTimeout(TimeSpan.FromSeconds(15))
            .Build();
}
