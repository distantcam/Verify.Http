# <img src="/src/icon.png" height="30px"> Verify.Http

[![Discussions](https://img.shields.io/badge/Verify-Discussions-yellow?svg=true&label=)](https://github.com/orgs/VerifyTests/discussions)
[![Build status](https://ci.appveyor.com/api/projects/status/rfmvbst3od5vpl7p?svg=true)](https://ci.appveyor.com/project/SimonCropp/verify-http)
[![NuGet Status](https://img.shields.io/nuget/v/Verify.Http.svg)](https://www.nuget.org/packages/Verify.Http/)

Extends [Verify](https://github.com/VerifyTests/Verify) to allow verification of Http bits.


## NuGet package

https://nuget.org/packages/Verify.Http/


## Enable

Enable VerifyHttp once at assembly load time:

<!-- snippet: Enable -->
<a id='snippet-enable'></a>
```cs
[ModuleInitializer]
public static void Initialize()
{
    VerifyHttp.Initialize();
#if NET7_0
    HttpRecording.Enable();
#endif
}
```
<sup><a href='/src/Tests/ModuleInitializer.cs#L3-L14' title='Snippet source file'>snippet source</a> | <a href='#snippet-enable' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Converters

Includes converters for the following

  * `HttpMethod`
  * `Uri`
  * `HttpHeaders`
  * `HttpContent`
  * `HttpRequestMessage`
  * `HttpResponseMessage`

For example:

<!-- snippet: HttpResponse -->
<a id='snippet-httpresponse'></a>
```cs
[Fact]
public async Task HttpResponse()
{
    using var client = new HttpClient();

    var result = await client.GetAsync("https://raw.githubusercontent.com/VerifyTests/Verify/main/license.txt");

    await Verify(result);
}
```
<sup><a href='/src/Tests/Tests.cs#L238-L250' title='Snippet source file'>snippet source</a> | <a href='#snippet-httpresponse' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: Tests.HttpResponse.verified.txt -->
<a id='snippet-Tests.HttpResponse.verified.txt'></a>
```txt
{
  Version: 1.1,
  Status: 200 OK,
  Headers: {
    Accept-Ranges: bytes,
    Access-Control-Allow-Origin: *,
    Cache-Control: max-age=300,
    Connection: keep-alive,
    Content-Security-Policy: default-src 'none'; style-src 'unsafe-inline'; sandbox,
    Cross-Origin-Resource-Policy: cross-origin,
    ETag: "753bbd67cc0d102eb769ba5301b846ee859521533eb8cacb3fd26ae32f6ebc35",
    Strict-Transport-Security: max-age=31536000,
    Vary: Authorization,Accept-Encoding,Origin,
    Via: 1.1 varnish,
    X-Cache: HIT,
    X-Content-Type-Options: nosniff,
    X-Frame-Options: deny,
    X-XSS-Protection: 1; mode=block
  },
  Content: {
    Headers: {
      Content-Type: text/plain; charset=utf-8,
      Expires: DateTime_1
    },
    Value:
MIT License

Copyright (c) .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

  },
  Request: https://raw.githubusercontent.com/VerifyTests/Verify/main/license.txt
}
```
<sup><a href='/src/Tests/Tests.HttpResponse.verified.txt#L1-L50' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.HttpResponse.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Ignoring Headers

Headers are treated as properties, and hence can be ignored using `IgnoreMember`:

<!-- snippet: IgnoreHeader -->
<a id='snippet-ignoreheader'></a>
```cs
[Fact]
public async Task IgnoreHeader()
{
    using var client = new HttpClient();

    var result = await client.GetAsync("https://raw.githubusercontent.com/VerifyTests/Verify/main/license.txt");

    await Verify(result)
        .IgnoreMembers("Server");
}
```
<sup><a href='/src/Tests/Tests.cs#L6-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-ignoreheader' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## HttpClient recording via Service

For code that does web calls via HttpClient, these calls can be recorded and verified.


### Service that does http

Given a class that does some Http calls:

<!-- snippet: ServiceThatDoesHttp -->
<a id='snippet-servicethatdoeshttp'></a>
```cs
public class MyService
{
    HttpClient client;

    // Resolve a HttpClient. All http calls done at any
    // resolved client will be added to `recording.Sends`
    public MyService(HttpClient client) =>
        this.client = client;

    public Task MethodThatDoesHttp() =>
        // Some code that does some http calls
        client.GetAsync("https://raw.githubusercontent.com/VerifyTests/Verify/main/license.txt");
}
```
<sup><a href='/src/Tests/Tests.cs#L46-L62' title='Snippet source file'>snippet source</a> | <a href='#snippet-servicethatdoeshttp' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Add to IHttpClientBuilder

Http recording can be added to a `IHttpClientBuilder`:

<!-- snippet: HttpClientRecording -->
<a id='snippet-httpclientrecording'></a>
```cs
var collection = new ServiceCollection();
collection.AddScoped<MyService>();
var httpBuilder = collection.AddHttpClient<MyService>();

// Adds a AddHttpClient and adds a RecordingHandler using AddHttpMessageHandler
var recording = httpBuilder.AddRecording();

await using var provider = collection.BuildServiceProvider();

var myService = provider.GetRequiredService<MyService>();

await myService.MethodThatDoesHttp();

await Verify(recording.Sends);
```
<sup><a href='/src/Tests/Tests.cs#L177-L194' title='Snippet source file'>snippet source</a> | <a href='#snippet-httpclientrecording' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Add globally

Http can also be added globally `IHttpClientBuilder`:

Note: This only seems to work in net5 and up.

<!-- snippet: HttpClientRecordingGlobal -->
<a id='snippet-httpclientrecordingglobal'></a>
```cs
var collection = new ServiceCollection();
collection.AddScoped<MyService>();

// Adds a AddHttpClient and adds a RecordingHandler using AddHttpMessageHandler
var (builder, recording) = collection.AddRecordingHttpClient();

await using var provider = collection.BuildServiceProvider();

var myService = provider.GetRequiredService<MyService>();

await myService.MethodThatDoesHttp();

await Verify(recording.Sends);
```
<sup><a href='/src/Tests/Tests.cs#L153-L169' title='Snippet source file'>snippet source</a> | <a href='#snippet-httpclientrecordingglobal' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: Tests.HttpClientRecording.verified.txt -->
<a id='snippet-Tests.HttpClientRecording.verified.txt'></a>
```txt
[
  {
    RequestUri: https://raw.githubusercontent.com/VerifyTests/Verify/main/license.txt,
    RequestMethod: GET,
    ResponseStatus: OK 200,
    ResponseHeaders: {
      Accept-Ranges: bytes,
      Access-Control-Allow-Origin: *,
      Cache-Control: max-age=300,
      Connection: keep-alive,
      Content-Security-Policy: default-src 'none'; style-src 'unsafe-inline'; sandbox,
      Cross-Origin-Resource-Policy: cross-origin,
      ETag: "753bbd67cc0d102eb769ba5301b846ee859521533eb8cacb3fd26ae32f6ebc35",
      Strict-Transport-Security: max-age=31536000,
      Vary: Authorization|Accept-Encoding|Origin,
      Via: 1.1 varnish,
      X-Cache: HIT,
      X-Content-Type-Options: nosniff,
      X-Frame-Options: deny,
      X-XSS-Protection: 1; mode=block
    },
    ResponseContent:
MIT License

Copyright (c) .NET Foundation and Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

  }
]
```
<sup><a href='/src/Tests/Tests.HttpClientRecording.verified.txt#L1-L46' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.HttpClientRecording.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There a Pause/Resume semantics:

<!-- snippet: HttpClientPauseResume -->
<a id='snippet-httpclientpauseresume'></a>
```cs
var collection = new ServiceCollection();
collection.AddScoped<MyService>();
var httpBuilder = collection.AddHttpClient<MyService>();

// Adds a AddHttpClient and adds a RecordingHandler using AddHttpMessageHandler
var recording = httpBuilder.AddRecording();

await using var provider = collection.BuildServiceProvider();

var myService = provider.GetRequiredService<MyService>();

// Recording is enabled by default. So Pause to stop recording
recording.Pause();
await myService.MethodThatDoesHttp();

// Resume recording
recording.Resume();
await myService.MethodThatDoesHttp();

await Verify(recording.Sends);
```
<sup><a href='/src/Tests/Tests.cs#L260-L283' title='Snippet source file'>snippet source</a> | <a href='#snippet-httpclientpauseresume' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

If the `AddRecordingHttpClient` helper method does not meet requirements, the `RecordingHandler` can be explicitly added:

<!-- snippet: HttpClientRecordingExplicit -->
<a id='snippet-httpclientrecordingexplicit'></a>
```cs
var collection = new ServiceCollection();

var builder = collection.AddHttpClient("name");

// Change to not recording at startup
var recording = new RecordingHandler(recording: false);

builder.AddHttpMessageHandler(() => recording);

await using var provider = collection.BuildServiceProvider();

var factory = provider.GetRequiredService<IHttpClientFactory>();

var client = factory.CreateClient("name");

await client.GetAsync("https://www.google.com/");

recording.Resume();
await client.GetAsync("https://raw.githubusercontent.com/VerifyTests/Verify/main/license.txt");

await Verify(recording.Sends);
```
<sup><a href='/src/Tests/Tests.cs#L289-L313' title='Snippet source file'>snippet source</a> | <a href='#snippet-httpclientrecordingexplicit' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Http Recording via listener

Http Recording allows, when a method is being tested, for any http requests made as part of that method call to be recorded and verified.

**Supported in net5 and up**


### Usage

Call `HttpRecording.StartRecording();` before the method being tested is called.

The perform the verification as usual:

<!-- snippet: HttpRecording -->
<a id='snippet-httprecording'></a>
```cs
[Fact]
public async Task TestHttpRecording()
{
    HttpRecording.StartRecording();

    var sizeOfResponse = await MethodThatDoesHttpCalls();

    await Verify(
            new
            {
                sizeOfResponse
            });
}

static async Task<int> MethodThatDoesHttpCalls()
{
    using var client = new HttpClient();

    var jsonResult = await client.GetStringAsync("https://github.com/VerifyTests/Verify.Http/raw/main/src/global.json");
    var ymlResult = await client.GetStringAsync("https://github.com/VerifyTests/Verify.Http/raw/main/src/appveyor.yml");
    return jsonResult.Length + ymlResult.Length;
}
```
<sup><a href='/src/Tests/Tests.cs#L94-L119' title='Snippet source file'>snippet source</a> | <a href='#snippet-httprecording' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

The requests/response pairs will be appended to the verified file.

<!-- snippet: Tests.TestHttpRecording.verified.txt -->
<a id='snippet-Tests.TestHttpRecording.verified.txt'></a>
```txt
{
  target: {
    sizeOfResponse: 771
  },
  httpCalls: [
    {
      Request: {
        Uri: https://github.com/VerifyTests/Verify.Http/raw/main/src/global.json,
        Headers: {}
      },
      Response: {
        Status: 302 Redirect,
        Headers: {
          Access-Control-Allow-Origin: https://render.githubusercontent.com,
          Cache-Control: no-cache,
          Content-Security-Policy: default-src 'none'; base-uri 'self'; child-src github.com/assets-cdn/worker/ gist.github.com/assets-cdn/worker/; connect-src 'self' uploads.github.com objects-origin.githubusercontent.com www.githubstatus.com collector.github.com raw.githubusercontent.com api.github.com github-cloud.s3.amazonaws.com github-production-repository-file-5c1aeb.s3.amazonaws.com github-production-upload-manifest-file-7fdce7.s3.amazonaws.com github-production-user-asset-6210df.s3.amazonaws.com cdn.optimizely.com logx.optimizely.com/v1/events *.actions.githubusercontent.com productionresultssa0.blob.core.windows.net/ productionresultssa1.blob.core.windows.net/ productionresultssa2.blob.core.windows.net/ productionresultssa3.blob.core.windows.net/ productionresultssa4.blob.core.windows.net/ wss://*.actions.githubusercontent.com github-production-repository-image-32fea6.s3.amazonaws.com github-production-release-asset-2e65be.s3.amazonaws.com insights.github.com wss://alive.github.com; font-src github.githubassets.com; form-action 'self' github.com gist.github.com objects-origin.githubusercontent.com; frame-ancestors 'none'; frame-src viewscreen.githubusercontent.com notebooks.githubusercontent.com; img-src 'self' data: github.githubassets.com media.githubusercontent.com camo.githubusercontent.com identicons.github.com avatars.githubusercontent.com github-cloud.s3.amazonaws.com objects.githubusercontent.com objects-origin.githubusercontent.com secured-user-images.githubusercontent.com/ user-images.githubusercontent.com/ private-user-images.githubusercontent.com opengraph.githubassets.com github-production-user-asset-6210df.s3.amazonaws.com customer-stories-feed.github.com spotlights-feed.github.com *.githubusercontent.com; manifest-src 'self'; media-src github.com user-images.githubusercontent.com/ secured-user-images.githubusercontent.com/ private-user-images.githubusercontent.com; script-src github.githubassets.com; style-src 'unsafe-inline' github.githubassets.com; upgrade-insecure-requests; worker-src github.com/assets-cdn/worker/ gist.github.com/assets-cdn/worker/,
          Location: https://raw.githubusercontent.com/VerifyTests/Verify.Http/main/src/global.json,
          Referrer-Policy: no-referrer-when-downgrade,
          Strict-Transport-Security: max-age=31536000; includeSubdomains; preload,
          Vary: X-PJAX,X-PJAX-Container,Turbo-Visit,Turbo-Frame,Accept-Encoding,Accept,X-Requested-With,
          X-Content-Type-Options: nosniff,
          X-Frame-Options: deny,
          X-XSS-Protection: 0
        },
        ContentHeaders: {
          Content-Type: text/html; charset=utf-8
        },
        ContentString: 
      }
    },
    {
      Request: {
        Uri: https://raw.githubusercontent.com/VerifyTests/Verify.Http/main/src/global.json,
        Headers: {}
      },
      Response: {
        Status: 200 OK,
        Headers: {
          Accept-Ranges: bytes,
          Access-Control-Allow-Origin: *,
          Cache-Control: max-age=300,
          Connection: keep-alive,
          Content-Security-Policy: default-src 'none'; style-src 'unsafe-inline'; sandbox,
          Cross-Origin-Resource-Policy: cross-origin,
          ETag: "4ec419eb28702d0b77f9f3ecb771f0a29465c2dbd60aae2d6801ff57a7d43d42",
          Strict-Transport-Security: max-age=31536000,
          Vary: Authorization,Accept-Encoding,Origin,
          Via: 1.1 varnish,
          X-Cache: HIT,
          X-Content-Type-Options: nosniff,
          X-Frame-Options: deny,
          X-XSS-Protection: 1; mode=block
        },
        ContentHeaders: {
          Content-Type: text/plain; charset=utf-8,
          Expires: DateTime_1
        },
        ContentString:
{
  "sdk": {
    "version": "7.0.306",
    "allowPrerelease": true,
    "rollForward": "latestFeature"
  }
}
      }
    },
    {
      Request: {
        Uri: https://github.com/VerifyTests/Verify.Http/raw/main/src/appveyor.yml,
        Headers: {}
      },
      Response: {
        Status: 302 Redirect,
        Headers: {
          Access-Control-Allow-Origin: https://render.githubusercontent.com,
          Cache-Control: no-cache,
          Content-Security-Policy: default-src 'none'; base-uri 'self'; child-src github.com/assets-cdn/worker/ gist.github.com/assets-cdn/worker/; connect-src 'self' uploads.github.com objects-origin.githubusercontent.com www.githubstatus.com collector.github.com raw.githubusercontent.com api.github.com github-cloud.s3.amazonaws.com github-production-repository-file-5c1aeb.s3.amazonaws.com github-production-upload-manifest-file-7fdce7.s3.amazonaws.com github-production-user-asset-6210df.s3.amazonaws.com cdn.optimizely.com logx.optimizely.com/v1/events *.actions.githubusercontent.com productionresultssa0.blob.core.windows.net/ productionresultssa1.blob.core.windows.net/ productionresultssa2.blob.core.windows.net/ productionresultssa3.blob.core.windows.net/ productionresultssa4.blob.core.windows.net/ wss://*.actions.githubusercontent.com github-production-repository-image-32fea6.s3.amazonaws.com github-production-release-asset-2e65be.s3.amazonaws.com insights.github.com wss://alive.github.com; font-src github.githubassets.com; form-action 'self' github.com gist.github.com objects-origin.githubusercontent.com; frame-ancestors 'none'; frame-src viewscreen.githubusercontent.com notebooks.githubusercontent.com; img-src 'self' data: github.githubassets.com media.githubusercontent.com camo.githubusercontent.com identicons.github.com avatars.githubusercontent.com github-cloud.s3.amazonaws.com objects.githubusercontent.com objects-origin.githubusercontent.com secured-user-images.githubusercontent.com/ user-images.githubusercontent.com/ private-user-images.githubusercontent.com opengraph.githubassets.com github-production-user-asset-6210df.s3.amazonaws.com customer-stories-feed.github.com spotlights-feed.github.com *.githubusercontent.com; manifest-src 'self'; media-src github.com user-images.githubusercontent.com/ secured-user-images.githubusercontent.com/ private-user-images.githubusercontent.com; script-src github.githubassets.com; style-src 'unsafe-inline' github.githubassets.com; upgrade-insecure-requests; worker-src github.com/assets-cdn/worker/ gist.github.com/assets-cdn/worker/,
          Location: https://raw.githubusercontent.com/VerifyTests/Verify.Http/main/src/appveyor.yml,
          Referrer-Policy: no-referrer-when-downgrade,
          Strict-Transport-Security: max-age=31536000; includeSubdomains; preload,
          Vary: X-PJAX,X-PJAX-Container,Turbo-Visit,Turbo-Frame,Accept-Encoding,Accept,X-Requested-With,
          X-Content-Type-Options: nosniff,
          X-Frame-Options: deny,
          X-XSS-Protection: 0
        },
        ContentHeaders: {
          Content-Type: text/html; charset=utf-8
        },
        ContentString: 
      }
    },
    {
      Request: {
        Uri: https://raw.githubusercontent.com/VerifyTests/Verify.Http/main/src/appveyor.yml,
        Headers: {}
      },
      Response: {
        Status: 200 OK,
        Headers: {
          Accept-Ranges: bytes,
          Access-Control-Allow-Origin: *,
          Cache-Control: max-age=300,
          Connection: keep-alive,
          Content-Security-Policy: default-src 'none'; style-src 'unsafe-inline'; sandbox,
          Cross-Origin-Resource-Policy: cross-origin,
          ETag: "5e3ae0644317bb5701cde0b57eae20773e7f2c2b2ca0ecbfca34ce3b0b683c75",
          Strict-Transport-Security: max-age=31536000,
          Vary: Authorization,Accept-Encoding,Origin,
          Via: 1.1 varnish,
          X-Cache: HIT,
          X-Content-Type-Options: nosniff,
          X-Frame-Options: deny,
          X-XSS-Protection: 1; mode=block
        },
        ContentHeaders: {
          Content-Type: text/plain; charset=utf-8,
          Expires: DateTime_1
        },
        ContentString:
image: Visual Studio 2022
environment:
  DOTNET_NOLOGO: true
  DOTNET_CLI_TELEMETRY_OPTOUT: true
  DOTNET_SKIP_FIRST_TIME_EXPERIENCE: true
build_script:
- pwsh: |
    Invoke-WebRequest "https://dot.net/v1/dotnet-install.ps1" -OutFile "./dotnet-install.ps1"
    ./dotnet-install.ps1 -JSonFile src/global.json -Architecture x64 -InstallDir 'C:\Program Files\dotnet'
- dotnet build src --configuration Release
- dotnet test src --configuration Release --no-build --no-restore --filter Category!=Integration
test: off
on_failure:
  - ps: Get-ChildItem *.received.* -recurse | % { Push-AppveyorArtifact $_.FullName -FileName $_.Name }
artifacts:
- path: nugets\*.nupkg
      }
    }
  ]
}
```
<sup><a href='/src/Tests/Tests.TestHttpRecording.verified.txt#L1-L140' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.TestHttpRecording.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Explicit Usage

The above usage results in the http calls being automatically added snapshot file. Calls can also be explicitly read and recorded using `HttpRecording.FinishRecording()`. This enables:

 * Filtering what http calls are included in the snapshot.
 * Only verifying a subset of information for each http call.
 * Performing additional asserts on http calls.

For example:

<!-- snippet: HttpRecordingExplicit -->
<a id='snippet-httprecordingexplicit'></a>
```cs
[Fact]
public async Task TestHttpRecordingExplicit()
{
    HttpRecording.StartRecording();

    var responseSize = await MethodThatDoesHttpCalls();

    var httpCalls = HttpRecording.FinishRecording().ToList();

    // Ensure all calls finished in under 5 seconds
    var threshold = TimeSpan.FromSeconds(5);
    foreach (var call in httpCalls)
    {
        Assert.True(call.Duration < threshold);
    }

    await Verify(
        new
        {
            responseSize,
            // Only use the Uri in the snapshot
            httpCalls = httpCalls.Select(_ => _.Request.Uri)
        });
}
```
<sup><a href='/src/Tests/Tests.cs#L121-L148' title='Snippet source file'>snippet source</a> | <a href='#snippet-httprecordingexplicit' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: Tests.TestHttpRecordingExplicit.verified.txt -->
<a id='snippet-Tests.TestHttpRecordingExplicit.verified.txt'></a>
```txt
{
  responseSize: 771,
  httpCalls: [
    https://github.com/VerifyTests/Verify.Http/raw/main/src/global.json,
    https://raw.githubusercontent.com/VerifyTests/Verify.Http/main/src/global.json,
    https://github.com/VerifyTests/Verify.Http/raw/main/src/appveyor.yml,
    https://raw.githubusercontent.com/VerifyTests/Verify.Http/main/src/appveyor.yml
  ]
}
```
<sup><a href='/src/Tests/Tests.TestHttpRecordingExplicit.verified.txt#L1-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-Tests.TestHttpRecordingExplicit.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Mocking

`MockHttpClient` allows mocking of http responses and recording of http requests.


### Default Response

The default behaviour is to return a `HttpResponseMessage` with a status code of `200 OK`.

<!-- snippet: DefaultContent -->
<a id='snippet-defaultcontent'></a>
```cs
[Fact]
public async Task DefaultContent()
{
    using var client = new MockHttpClient();

    var result = await client.GetAsync("https://fake/get");

    await Verify(result);
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L40-L52' title='Snippet source file'>snippet source</a> | <a href='#snippet-defaultcontent' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.DefaultContent.verified.txt -->
<a id='snippet-MockHttpClientTests.DefaultContent.verified.txt'></a>
```txt
{
  Version: 1.1,
  Status: 200 OK
}
```
<sup><a href='/src/Tests/MockHttpClientTests.DefaultContent.verified.txt#L1-L4' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.DefaultContent.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Verifying Calls

Request-Response pairs can be verified using `MockHttpClient.Calls`

<!-- snippet: RecordedCalls -->
<a id='snippet-recordedcalls'></a>
```cs
[Fact]
public async Task RecordedCalls()
{
    using var client = new MockHttpClient();

    await client.GetAsync("https://fake/get1");
    await client.GetAsync("https://fake/get2");

    await Verify(client.Calls);
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L56-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-recordedcalls' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.RecordedCalls.verified.txt -->
<a id='snippet-MockHttpClientTests.RecordedCalls.verified.txt'></a>
```txt
[
  {
    Request: https://fake/get1,
    Response: 200 Ok
  },
  {
    Request: https://fake/get2,
    Response: 200 Ok
  }
]
```
<sup><a href='/src/Tests/MockHttpClientTests.RecordedCalls.verified.txt#L1-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.RecordedCalls.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Explicit Content Response

Always return an explicit `StringContent` and media-type:

<!-- snippet: ExplicitContent -->
<a id='snippet-explicitcontent'></a>
```cs
[Fact]
public async Task ExplicitContent()
{
    using var client = new MockHttpClient(
        content: @"{ ""a"": ""b"" }",
        mediaType: "application/json");

    var result = await client.GetAsync("https://fake/get");

    await Verify(result);
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L24-L38' title='Snippet source file'>snippet source</a> | <a href='#snippet-explicitcontent' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.ExplicitContent.verified.txt -->
<a id='snippet-MockHttpClientTests.ExplicitContent.verified.txt'></a>
```txt
{
  Version: 1.1,
  Status: 200 OK,
  Content: {
    Headers: {
      Content-Type: application/json; charset=utf-8
    },
    Value: {
      a: b
    }
  }
}
```
<sup><a href='/src/Tests/MockHttpClientTests.ExplicitContent.verified.txt#L1-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.ExplicitContent.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Explicit HttpStatusCode Response

Always return an explicit `HttpStatusCode`:

<!-- snippet: ExplicitStatusCode -->
<a id='snippet-explicitstatuscode'></a>
```cs
[Fact]
public async Task ExplicitStatusCode()
{
    using var client = new MockHttpClient(HttpStatusCode.Ambiguous);

    var result = await client.GetAsync("https://fake/get");

    await Verify(result);
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L72-L84' title='Snippet source file'>snippet source</a> | <a href='#snippet-explicitstatuscode' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.ExplicitStatusCode.verified.txt -->
<a id='snippet-MockHttpClientTests.ExplicitStatusCode.verified.txt'></a>
```txt
{
  Version: 1.1,
  Status: 300 Multiple Choices
}
```
<sup><a href='/src/Tests/MockHttpClientTests.ExplicitStatusCode.verified.txt#L1-L4' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.ExplicitStatusCode.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Explicit HttpResponseMessage

Alwars return an explicit `HttpResponseMessage`:

<!-- snippet: ExplicitResponse -->
<a id='snippet-explicitresponse'></a>
```cs
[Fact]
public async Task ExplicitResponse()
{
    var response = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent("Hello")
    };
    using var client = new MockHttpClient(response);

    var result = await client.GetAsync("https://fake/get");

    await Verify(result);
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L86-L102' title='Snippet source file'>snippet source</a> | <a href='#snippet-explicitresponse' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.ExplicitResponse.verified.txt -->
<a id='snippet-MockHttpClientTests.ExplicitResponse.verified.txt'></a>
```txt
{
  Version: 1.1,
  Status: 200 OK,
  Content: {
    Headers: {
      Content-Type: text/plain; charset=utf-8
    },
    Value: Hello
  }
}
```
<sup><a href='/src/Tests/MockHttpClientTests.ExplicitResponse.verified.txt#L1-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.ExplicitResponse.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### HttpResponseMessage builder

Use custom code to create a `HttpResponseMessage` base on a `HttpRequestMessage`:

<!-- snippet: ResponseBuilder -->
<a id='snippet-responsebuilder'></a>
```cs
[Fact]
public async Task ResponseBuilder()
{
    using var client = new MockHttpClient(
        request =>
        {
            var content = $"Hello to {request.RequestUri}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content),
            };
            return response;
        });

    var result1 = await client.GetAsync("https://fake/get1");
    var result2 = await client.GetAsync("https://fake/get2");

    await Verify(new
    {
        result1,
        result2
    });
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L131-L157' title='Snippet source file'>snippet source</a> | <a href='#snippet-responsebuilder' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.ResponseBuilder.verified.txt -->
<a id='snippet-MockHttpClientTests.ResponseBuilder.verified.txt'></a>
```txt
{
  result1: {
    Version: 1.1,
    Status: 200 OK,
    Content: {
      Headers: {
        Content-Type: text/plain; charset=utf-8
      },
      Value: Hello to https://fake/get1
    }
  },
  result2: {
    Version: 1.1,
    Status: 200 OK,
    Content: {
      Headers: {
        Content-Type: text/plain; charset=utf-8
      },
      Value: Hello to https://fake/get2
    }
  }
}
```
<sup><a href='/src/Tests/MockHttpClientTests.ResponseBuilder.verified.txt#L1-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.ResponseBuilder.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Enumeration of HttpResponseMessage

Use a sequence of `HttpResponseMessage` to return a sequence of requests:

<!-- snippet: EnumerableResponses -->
<a id='snippet-enumerableresponses'></a>
```cs
[Fact]
public async Task EnumerableResponses()
{
    using var client = new MockHttpClient(
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("Hello")
        },
        new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("World")
        });

    var result1 = await client.GetAsync("https://fake/get1");
    var result2 = await client.GetAsync("https://fake/get2");

    await Verify(new
    {
        result1,
        result2
    });
}
```
<sup><a href='/src/Tests/MockHttpClientTests.cs#L104-L129' title='Snippet source file'>snippet source</a> | <a href='#snippet-enumerableresponses' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Resulting verified file

<!-- snippet: MockHttpClientTests.EnumerableResponses.verified.txt -->
<a id='snippet-MockHttpClientTests.EnumerableResponses.verified.txt'></a>
```txt
{
  result1: {
    Version: 1.1,
    Status: 200 OK,
    Content: {
      Headers: {
        Content-Type: text/plain; charset=utf-8
      },
      Value: Hello
    }
  },
  result2: {
    Version: 1.1,
    Status: 200 OK,
    Content: {
      Headers: {
        Content-Type: text/plain; charset=utf-8
      },
      Value: World
    }
  }
}
```
<sup><a href='/src/Tests/MockHttpClientTests.EnumerableResponses.verified.txt#L1-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-MockHttpClientTests.EnumerableResponses.verified.txt' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Icon

[Spider](https://thenounproject.com/term/spider/904683/) designed by [marialuisa iborra](https://thenounproject.com/marialuisa.iborra/) from [The Noun Project](https://thenounproject.com).
