# OpenWebClient

OpenWebClient is a small library that provides a [`System.Net.WebClient`](http://msdn.microsoft.com/en-us/library/system.net.webclient.aspx) subclass (incidentally also called `WebClient`) so that you don't have to subclass for 90% of the simple scenarios.

OpenWebClient is distributed as two NuGet packages:

- [Class library package](http://www.nuget.org/packages/OpenWebClient/) requiring at leadt .NET Framework 3.5 (Client Profile supported)
- [C# source package](http://www.nuget.org/packages/OpenWebClient.Source/) that adds a single C# source file to a C# project

## Motiviation

To do any customization of requests and responses used by `System.Net.WebClient`, you have subclass it and override its virtual members [`GetWebRequest`](http://msdn.microsoft.com/en-us/library/system.net.webclient.getwebrequest.aspx) and [`GetWebResponse`](http://msdn.microsoft.com/en-us/library/system.net.webclient.getwebresponse.aspx). You have to subclass because protected both members are protected.

OpenWebClient solves the problem for once and for all by providing a subclass with the following two properties:

    public Func<WebRequest, WebRequest> WebRequestHandler { get; set; }
    public Func<WebResponse, WebResponse> WebResponseHandler { get; set; }

The delegate instance assigned to `WebRequestHandler` will be called whenever a `WebRequest` object is created by `System.Net.WebClient` and before it goes into flight. Similarly, the delegate instance assigned to `WebResponseHandler` will be called whenever a `WebResponse` object has been obtained by `System.Net.WebClient` for a request that was sent previously.

## Usage

Suppose you want to set the time-out for a download. Here's how you can do it using the `WebRequestHandler` property:

    var wc = new OpenWebClient.WebClient();
    var timeout = TimeSpan.FromMinutes(2);
    wc.WebRequestHandler += wr =>
    {
        wr.Timeout = (int) timeout.TotalMilliseconds;
        return wr;
    };
    Console.WriteLine(wc.DownloadString("http://www.example.com"));

Note that you can use `+=` to attach multiple handlers. OpenWebClient also provides short-cuts via helper extension methods so the above can be simply written as:

    var timeout = TimeSpan.FromMinutes(2);
    wc.AddWebRequestHandler(wr => wr.Timeout = (int) timeout.TotalMilliseconds);
    Console.WriteLine(wc.DownloadString("http://www.example.com"));

The handlers will be called for every request and/or response through the same `WebClient` instance. If you want to set the time-out only for the next request only and reset to default thereafter, use `AddOneTimeWebRequestHandler` instead:

    wc.AddOneTimeWebRequestHandler(wr => wr.Timeout = (int) timeout.TotalMilliseconds);

When making HTTP requests, you want to work with an [HttpWebRequest](`http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.aspx`) because some HTTP-specific properties like [`AutomaticDecompression`](http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.automaticdecompression.aspx) are not available `WebRequest`. Instead of adding a regular handler and down-casting its argument, you can use `AddHttpWebRequestHandler`:

    wc.AddHttpWebRequestHandler(wr => wr.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate);

The same is true for responses.

OpenWebClient's subclass of `WebClient` is deliberately named the same so you can start using it incrementally and with minimal changes to your existing source files. Simply importing the `OpenWebClient` namespace however will cause a conflict if you are importing `System.Net` too and using `WebClient` already. By adding an [alias directive](http://msdn.microsoft.com/en-us/library/aa664765.aspx) for `WebClient` to point to `OpenWebClient.WebClient`, you can resolve the conflict and start using OpenWebClient's version immediately. You imports section should therefore look like this:

    using System.Net;
    using OpenWebClient;
    using WebClient = OpenWebClient.WebClient;