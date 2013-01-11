#region License and Terms
//
// Eggado
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
        public Func<WebRequest, WebRequest> WebRequestModifier { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            var modifier = WebRequestModifier ?? (wr => wr);
            return modifier.GetInvocationList()
                           .Cast<Func<WebRequest, WebRequest>>()
                           .Select(m => new Func<WebRequest, WebRequest>(wr => m(Validate(wr))))
                           .Aggregate((im, om) => wr => om(im(wr)))(request);
        }

        static WebRequest Validate(WebRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            return request;
        }
    }
}