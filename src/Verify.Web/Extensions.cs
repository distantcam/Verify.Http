﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;

static class Extensions
{
    public static bool IsText(this HttpContent content)
    {
        var contentType = content.Headers.ContentType;
        if (contentType?.MediaType == null)
        {
            return false;
        }

        return contentType.MediaType.StartsWith("text");
    }

    public static Dictionary<string, string> ToDictionary(this HttpHeaders headers)
    {
        return headers
            .ToDictionary(x => x.Key, x => string.Join("|", x.Value));
    }

    public static Dictionary<string, string> NotCookies(this HttpHeaders headers)
    {
        return headers
            .Where(x => x.Key != "Set-Cookie")
            .ToDictionary(x => x.Key, x => x.Value.ToString()!);
    }
    
    public static Dictionary<string, string> Cookies(this HttpHeaders headers)
    {
        return headers
            .Where(x => x.Key == "Set-Cookie")
            .Select(x =>
            {
                var stringSegment = x.Value.Single();
                return SetCookieHeaderValue.Parse(stringSegment);
            })
            .ToDictionary(x => x.Name.Value, x => x.Value.Value);
    }
}