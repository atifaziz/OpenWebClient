#region License and Terms
//
// OpenWebClient
// Copyright (c) 2013 Atif Aziz. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

namespace OpenWebClient
{
    #region Imports

    using System;
    using System.Linq;
    using System.Net;

    #endregion

    partial class WebClient : System.Net.WebClient
    {
        public Func<WebRequest, WebRequest> WebRequestHandler { get; set; }
        public Func<WebResponse, WebResponse> WebResponseHandler { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            return WithHandlers(base.GetWebRequest(address), WebRequestHandler, Validate);
        }

        static WebRequest Validate(WebRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            return WithHandlers(base.GetWebResponse(request), WebResponseHandler, Validate);
        }

        static WebResponse Validate(WebResponse response)
        {
            if (response == null) throw new ArgumentNullException("response");
            return response;
        }

        static T WithHandlers<T>(T response, Func<T, T> handler, Func<T, T> validator)
        {
            handler = handler ?? (wr => wr);
            var handlers =
                from Func<T, T> h in handler.GetInvocationList()
                select new Func<T, T>(wr => h(validator(wr)));
            return handlers.Aggregate((im, om) => wr => om(im(wr)))(response);

        }
    }

    static partial class WebClientExtensions
    {
        /// <summary>
        /// Adds a simple web request handler called for each request
        /// issued by this <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebRequestHandler(this WebClient client, Action<WebRequest> handler)
        {
            return client.AddWebRequestHandler<WebRequest>(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for each request of
        /// type <see cref="HttpWebRequest"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddHttpWebRequestHandler(this WebClient client, Action<HttpWebRequest> handler)
        {
            return client.AddWebRequestHandler(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for each request of
        /// type <see cref="WebRequest"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebRequestHandler<T>(this WebClient client, Action<T> handler)
            where T : WebRequest
        {
            if (client == null) throw new ArgumentNullException("client");
            client.WebRequestHandler += RequestOf(handler);
            return client;
        }

        /// <summary>
        /// Adds a simple web request handler that is called for only the
        /// next request issued by this <see cref="WebClient"/> and then
        /// discarded.
        /// </summary>
        /// <remarks>
        /// The handler is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebRequestHandler(this WebClient client, Action<WebRequest> handler)
        {
            return client.AddOneTimeWebRequestHandler<WebRequest>(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for only the next
        /// request of type <see cref="HttpWebRequest"/> issued by this
        /// <see cref="WebClient"/> and then discarded.
        /// </summary>
        /// <remarks>
        /// The handler is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeHttpWebRequestHandler(this WebClient client, Action<HttpWebRequest> handler)
        {
            return client.AddOneTimeWebRequestHandler(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for only the next
        /// request of type <typeparamref name="T"/> issued by this
        /// <see cref="WebClient"/> and then discarded, where
        /// <typeparamref name="T"/> is an instance of
        /// <see cref="WebRequest"/>.
        /// </summary>
        /// <remarks>
        /// The handler is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebRequestHandler<T>(this WebClient client, Action<T> handler)
            where T : WebRequest
        {
            if (client == null) throw new ArgumentNullException("client");

            var cell = new Func<WebRequest, WebRequest>[1];
            cell[0] = RequestOf<T>(wr =>
            {
                client.WebRequestHandler -= cell[0];
                handler(wr);
            });
            client.WebRequestHandler += cell[0];

            return client;
        }

        static Func<WebRequest, WebRequest> RequestOf<T>(Action<T> handler) where T : WebRequest
        {
            if (handler == null) throw new ArgumentNullException("handler");

            return request =>
            {
                var typedRequest = request as T;
                if (typedRequest != null)
                    handler(typedRequest);
                return request;
            };
        }

        /// <summary>
        /// Adds a simple web request handler called for each request
        /// issued by this <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebResponseHandler(this WebClient client, Action<WebResponse> handler)
        {
            return client.AddWebResponseHandler<WebResponse>(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for each request of
        /// type <see cref="HttpWebResponse"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddHttpWebResponseHandler(this WebClient client, Action<HttpWebResponse> handler)
        {
            return client.AddWebResponseHandler(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for each request of
        /// type <see cref="WebResponse"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebResponseHandler<T>(this WebClient client, Action<T> handler)
            where T : WebResponse
        {
            if (client == null) throw new ArgumentNullException("client");
            client.WebResponseHandler += ResponseOf(handler);
            return client;
        }

        /// <summary>
        /// Adds a simple web request handler that is called for only the
        /// next request issued by this <see cref="WebClient"/> and then
        /// discarded.
        /// </summary>
        /// <remarks>
        /// The handler is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebResponseHandler(this WebClient client, Action<WebResponse> handler)
        {
            return client.AddOneTimeWebResponseHandler<WebResponse>(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for only the next
        /// request of type <see cref="HttpWebResponse"/> issued by this
        /// <see cref="WebClient"/> and then discarded.
        /// </summary>
        /// <remarks>
        /// The handler is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeHttpWebResponseHandler(this WebClient client, Action<HttpWebResponse> handler)
        {
            return client.AddOneTimeWebResponseHandler(handler);
        }

        /// <summary>
        /// Adds a simple web request handler called for only the next
        /// request of type <typeparamref name="T"/> issued by this
        /// <see cref="WebClient"/> and then discarded, where
        /// <typeparamref name="T"/> is an instance of
        /// <see cref="WebRequest"/>.
        /// </summary>
        /// <remarks>
        /// The handler is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebResponseHandler<T>(this WebClient client, Action<T> handler)
            where T : WebResponse
        {
            if (client == null) throw new ArgumentNullException("client");

            var cell = new Func<WebResponse, WebResponse>[1];
            cell[0] = ResponseOf<T>(wr =>
            {
                client.WebResponseHandler -= cell[0];
                handler(wr);
            });
            client.WebResponseHandler += cell[0];

            return client;
        }

        static Func<WebResponse, WebResponse> ResponseOf<T>(Action<T> handler) where T : WebResponse
        {
            if (handler == null) throw new ArgumentNullException("handler");

            return request =>
            {
                var typedResponse = request as T;
                if (typedResponse != null)
                    handler(typedResponse);
                return request;
            };
        }
    }
}