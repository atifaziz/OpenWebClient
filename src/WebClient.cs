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
    using System.Diagnostics;
    using System.Linq;
    using System.Net;

    #endregion

    partial class WebClient : System.Net.WebClient
    {
        public Func<WebRequest, WebRequest> WebRequestModifier { get; set; }
        public Func<WebResponse, WebResponse> WebResponseModifier { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            return WithModifiers(base.GetWebRequest(address), WebRequestModifier, Validate);
        }

        static WebRequest Validate(WebRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            return WithModifiers(base.GetWebResponse(request), WebResponseModifier, Validate);
        }

        static WebResponse Validate(WebResponse response)
        {
            if (response == null) throw new ArgumentNullException("response");
            return response;
        }

        static T WithModifiers<T>(T response, Func<T, T> modifier, Func<T, T> validator)
        {
            modifier = modifier ?? (wr => wr);
            return modifier.GetInvocationList()
                           .Cast<Func<T, T>>()
                           .Select(m => new Func<T, T>(wr => m(validator(wr))))
                           .Aggregate((im, om) => wr => om(im(wr)))(response);

        }
    }

    static partial class WebClientExtensions
    {
        /// <summary>
        /// Adds a simple web request modifier called for each request
        /// issued by this <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebRequestModifier(this WebClient client, Action<WebRequest> modifier)
        {
            return client.AddWebRequestModifier<WebRequest>(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for each request of
        /// type <see cref="HttpWebRequest"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddHttpWebRequestModifier(this WebClient client, Action<HttpWebRequest> modifier)
        {
            return client.AddWebRequestModifier(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for each request of
        /// type <see cref="WebRequest"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebRequestModifier<T>(this WebClient client, Action<T> modifier)
            where T : WebRequest
        {
            if (client == null) throw new ArgumentNullException("client");
            client.WebRequestModifier += RequestOf(modifier);
            return client;
        }

        /// <summary>
        /// Adds a simple web request modifier that is called for only the
        /// next request issued by this <see cref="WebClient"/> and then
        /// discarded.
        /// </summary>
        /// <remarks>
        /// The modifier is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebRequestModifier(this WebClient client, Action<WebRequest> modifier)
        {
            return client.AddOneTimeWebRequestModifier<WebRequest>(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for only the next
        /// request of type <see cref="HttpWebRequest"/> issued by this
        /// <see cref="WebClient"/> and then discarded.
        /// </summary>
        /// <remarks>
        /// The modifier is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeHttpWebRequestModifier(this WebClient client, Action<HttpWebRequest> modifier)
        {
            return client.AddOneTimeWebRequestModifier(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for only the next
        /// request of type <typeparamref name="T"/> issued by this
        /// <see cref="WebClient"/> and then discarded, where
        /// <typeparamref name="T"/> is an instance of
        /// <see cref="WebRequest"/>.
        /// </summary>
        /// <remarks>
        /// The modifier is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebRequestModifier<T>(this WebClient client, Action<T> modifier)
            where T : WebRequest
        {
            if (client == null) throw new ArgumentNullException("client");

            var cell = new Func<WebRequest, WebRequest>[1];
            cell[0] = RequestOf<T>(wr =>
            {
                client.WebRequestModifier -= cell[0];
                modifier(wr);
            });
            client.WebRequestModifier += cell[0];

            return client;
        }

        static Func<WebRequest, WebRequest> RequestOf<T>(Action<T> modifier) where T : WebRequest
        {
            if (modifier == null) throw new ArgumentNullException("modifier");

            return request =>
            {
                var typedRequest = request as T;
                if (typedRequest != null)
                    modifier(typedRequest);
                return request;
            };
        }

        /// <summary>
        /// Adds a simple web request modifier called for each request
        /// issued by this <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebResponseModifier(this WebClient client, Action<WebResponse> modifier)
        {
            return client.AddWebResponseModifier<WebResponse>(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for each request of
        /// type <see cref="HttpWebResponse"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddHttpWebResponseModifier(this WebClient client, Action<HttpWebResponse> modifier)
        {
            return client.AddWebResponseModifier(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for each request of
        /// type <see cref="WebResponse"/> issued by this
        /// <see cref="WebClient"/>.
        /// </summary>

        public static WebClient AddWebResponseModifier<T>(this WebClient client, Action<T> modifier)
            where T : WebResponse
        {
            if (client == null) throw new ArgumentNullException("client");
            client.WebResponseModifier += ResponseOf(modifier);
            return client;
        }

        /// <summary>
        /// Adds a simple web request modifier that is called for only the
        /// next request issued by this <see cref="WebClient"/> and then
        /// discarded.
        /// </summary>
        /// <remarks>
        /// The modifier is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebResponseModifier(this WebClient client, Action<WebResponse> modifier)
        {
            return client.AddOneTimeWebResponseModifier<WebResponse>(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for only the next
        /// request of type <see cref="HttpWebResponse"/> issued by this
        /// <see cref="WebClient"/> and then discarded.
        /// </summary>
        /// <remarks>
        /// The modifier is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeHttpWebResponseModifier(this WebClient client, Action<HttpWebResponse> modifier)
        {
            return client.AddOneTimeWebResponseModifier(modifier);
        }

        /// <summary>
        /// Adds a simple web request modifier called for only the next
        /// request of type <typeparamref name="T"/> issued by this
        /// <see cref="WebClient"/> and then discarded, where
        /// <typeparamref name="T"/> is an instance of
        /// <see cref="WebRequest"/>.
        /// </summary>
        /// <remarks>
        /// The modifier is only called once regardless of whether it throws
        /// an exception or not.
        /// </remarks>

        public static WebClient AddOneTimeWebResponseModifier<T>(this WebClient client, Action<T> modifier)
            where T : WebResponse
        {
            if (client == null) throw new ArgumentNullException("client");

            var cell = new Func<WebResponse, WebResponse>[1];
            cell[0] = ResponseOf<T>(wr =>
            {
                client.WebResponseModifier -= cell[0];
                modifier(wr);
            });
            client.WebResponseModifier += cell[0];

            return client;
        }

        static Func<WebResponse, WebResponse> ResponseOf<T>(Action<T> modifier) where T : WebResponse
        {
            if (modifier == null) throw new ArgumentNullException("modifier");

            return request =>
            {
                var typedResponse = request as T;
                if (typedResponse != null)
                    modifier(typedResponse);
                return request;
            };
        }
    }
}