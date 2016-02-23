# OpenWebClient

OpenWebClient is a small library that provides a [`System.Net.WebClient`][wc]
subclass (incidentally also called `WebClient`) so that you don't have to
subclass for 90% of the simple scenarios.

OpenWebClient is distributed as two NuGet packages:

- [Class library package][nupkg] requiring at least .NET Framework 3.5 (Client
  Profile supported)
- [C# source package][src-nupkg] that adds a single C# source file to a C#
  project

## Motiviation

To do any customization of requests and responses used by
`System.Net.WebClient`, you have subclass it and override its virtual members
[`GetWebRequest`][gwreq] and [`GetWebResponse`][gwrsp]. You have to subclass
because protected both members are protected.

OpenWebClient solves the problem for once and for all by providing a subclass
with the following two properties:

```c#
public Func<WebRequest, WebRequest> WebRequestHandler { get; set; }
public Func<WebResponse, WebResponse> WebResponseHandler { get; set; }
```

The delegate instance assigned to `WebRequestHandler` will be called whenever a
`WebRequest` object is created by `System.Net.WebClient` and before it goes
into flight. Similarly, the delegate instance assigned to `WebResponseHandler`
will be called whenever a `WebResponse` object has been obtained by
`System.Net.WebClient` for a request that was sent previously.

## Usage

Suppose you want to set the time-out for a download. Here's how you can do it
using the `WebRequestHandler` property:

```c#
var wc = new OpenWebClient.WebClient();
var timeout = TimeSpan.FromMinutes(2);
wc.WebRequestHandler += wr =>
{
    wr.Timeout = (int) timeout.TotalMilliseconds;
    return wr;
};
Console.WriteLine(wc.DownloadString("http://www.example.com"));
```

Note that you can use `+=` to attach multiple handlers. OpenWebClient also
provides short-cuts via helper extension methods so the above can be simply
written as:

```c#
var timeout = TimeSpan.FromMinutes(2);
wc.AddWebRequestHandler(wr => wr.Timeout = (int) timeout.TotalMilliseconds);
Console.WriteLine(wc.DownloadString("http://www.example.com"));
```

The handlers will be called for every request and/or response through the same
`WebClient` instance. If you want to set the time-out only for the next request
only and reset to default thereafter, use `AddOneTimeWebRequestHandler`
instead:

```c#
wc.AddOneTimeWebRequestHandler(wr => wr.Timeout = (int) timeout.TotalMilliseconds);
```

When making HTTP requests, you want to work with an [`HttpWebRequest`][hwebreq]
because some HTTP-specific properties like [`AutomaticDecompression`][autodecomp]
are not available `WebRequest`. Instead of adding a regular handler and
down-casting its argument, you can use `AddHttpWebRequestHandler`:

```c#
wc.AddHttpWebRequestHandler(wr => wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate);
```

Since the handler adding methods return the `WebClient` instance on which they
act, you can also chain the calls. The example below sets a time-out (for the
next request only) and automatic decompressions and cookie management for all
requests:

```c#
var timeout = TimeSpan.FromMinutes(2);
var cookies = new CookieContainer();
var wc = new OpenWebClient.WebClient()
    .AddOneTimeWebRequestHandler(wr => wr.Timeout = (int) timeout.TotalMilliseconds)
    .AddHttpWebRequestHandler(wr => wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate)
    .AddHttpWebRequestHandler(wr => wr.CookieContainer = cookies);
Console.WriteLine(wc.DownloadString("http://www.example.com"));
```

The same is all true for response handlers.

OpenWebClient's subclass of `WebClient` is deliberately named the same so you
can start using it incrementally and with minimal changes to your existing
source files. Simply importing the `OpenWebClient` namespace however will
cause a conflict if you are importing `System.Net` too and using `WebClient`
already. By adding an [alias directive][using-alias] for `WebClient` to point
to `OpenWebClient.WebClient`, you can resolve the conflict and start using
OpenWebClient's version immediately. You imports section should therefore look
like this:

```c#
using System.Net;
using OpenWebClient;
using WebClient = OpenWebClient.WebClient;
```


[wc]: http://msdn.microsoft.com/en-us/library/system.net.webclient.aspx
[nupkg]: http://www.nuget.org/packages/OpenWebClient/
[src-nupkg]: http://www.nuget.org/packages/OpenWebClient.Source/
[gwreq]: http://msdn.microsoft.com/en-us/library/system.net.webclient.getwebrequest.aspx
[gwrsp]: http://msdn.microsoft.com/en-us/library/system.net.webclient.getwebresponse.aspx
[hwebreq]: http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.aspx
[autodecomp]: http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.automaticdecompression.aspx
[using-alias]: http://msdn.microsoft.com/en-us/library/aa664765.aspx