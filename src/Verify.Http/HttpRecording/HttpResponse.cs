﻿namespace VerifyTests.Http;

public class HttpResponse
{
    public HttpResponse(HttpResponseMessage response)
    {
        Status = response.StatusCode;
        if (response.Headers.Any())
        {
            Headers = response.Headers;
        }

        var content = (HttpContent?) response.Content;
        if (content != null && content.Headers.Any())
        {
            ContentHeaders = content.Headers;
        }

#if NET6_0_OR_GREATER
        if (response.TrailingHeaders.Any())
        {
            TrailingHeaders = response.TrailingHeaders;
        }
#endif

        var stringContent = content.TryReadStringContent();
        ContentStringParsed = stringContent.prettyContent;
        ContentString = stringContent.content;
    }

    public HttpStatusCode Status { get; }
    public HttpResponseHeaders? Headers { get; }
#if NET6_0_OR_GREATER
    public HttpResponseHeaders? TrailingHeaders { get; }
#endif
    public HttpContentHeaders? ContentHeaders { get; }
    public object? ContentStringParsed { get; }
    [JsonIgnore] public string? ContentString { get; }
}