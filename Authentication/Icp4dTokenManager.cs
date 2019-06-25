/**
* Copyright 2019 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using IBM.Cloud.SDK.Connection;
using IBM.Cloud.SDK.Utilities;
using System;
using System.Net.Http;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using Utility = IBM.Cloud.SDK.Utilities.Utility;


namespace IBM.Cloud.SDK.Authentication
{
    public class Icp4dTokenManager : JwtTokenManager
    {
        private string username;
        private string password;

        public Icp4dTokenManager(Icp4dTokenOptions options) : base(options)
        {

            if (!string.IsNullOrEmpty(options.Url))
            {
                url = options.Url + "/v1/preauth/validateAuth";
            }
            else if (string.IsNullOrEmpty(userAccessToken))
            {
                // url is not needed if the user specifies their own access token
                throw new ArgumentNullException("`url` is a required parameter for Icp4dTokenManagerV1");
            }

            // username and password are required too, unless there's access token
            if (!string.IsNullOrEmpty(options.Username))
            {
                username = options.Username;
            }
            if (!string.IsNullOrEmpty(options.Password))
            {
                password = options.Password;
            }
            if (!string.IsNullOrEmpty(options.AccessToken))
            {
                userAccessToken = options.AccessToken;
            }
            disableSslVerification = options.DisableSslVerification;
        }

        override protected bool RequestToken(Callback<TokenData> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("successCallback");

            RESTConnector connector = new RESTConnector();
            connector.URL = url;
            if (connector == null)
                return false;

            RequestIcp4dTokenRequest req = new RequestIcp4dTokenRequest();
            req.HttpMethod = UnityWebRequest.kHttpVerbGET;
            req.Callback = callback;
            req.Headers.Add("Content-type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", Utility.CreateAuthorization(username, password));
            req.OnResponse = OnRequestIcp4dTokenResponse;
            req.DisableSslVerification = disableSslVerification;
            return connector.Send(req);
        }

        private class RequestIcp4dTokenRequest : RESTConnector.Request
        {
            public Callback<TokenData> Callback { get; set; }
        }

        private void OnRequestIcp4dTokenResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<TokenData> response = new DetailedResponse<TokenData>();
            response.Result = new TokenData();
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<TokenData>(json);
                response.Response = json;
            }
            catch (Exception e)
            {
                Log.Error("Credentials.OnRequestIamTokenResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }
            if (((RequestIcp4dTokenRequest)req).Callback != null)
                ((RequestIcp4dTokenRequest)req).Callback(response, resp.Error);
        }
    }

    public class Icp4dTokenOptions : JwtTokenOptions
    {
        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                if (!Utility.HasBadFirstOrLastCharacter(value))
                {
                    username = value;
                }
                else
                {
                    throw new ArgumentException("The username shouldn't start or end with curly brackets or quotes. Be sure to remove any {} and \" characters surrounding your username");
                }
            }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set
            {
                if (!Utility.HasBadFirstOrLastCharacter(value))
                {
                    password = value;
                }
                else
                {
                    throw new ArgumentException("The password shouldn't start or end with curly brackets or quotes. Be sure to remove any {} and \" characters surrounding your password");
                }
            }
        }

        public bool DisableSslVerification { get; set; }
    }
}