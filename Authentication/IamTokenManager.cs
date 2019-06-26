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
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using Utility = IBM.Cloud.SDK.Utilities.Utility;


namespace IBM.Cloud.SDK.Authentication
{
    public class IamTokenManager : JwtTokenManager
    {
        private string iamApikey;
        private string iamClientId;
        private string iamClientSecret;
        private string iamDefaultUrl = "https://iam.cloud.ibm.com/identity/token";

        private const string CLIENT_ID_SECRET_WARNING = "Warning: Client ID and Secret must BOTH be given, or the defaults will be used.";

        public IamTokenManager(IamTokenOptions options) : base(options)
        {
            if (string.IsNullOrEmpty(url))
            {
                if (!string.IsNullOrEmpty(options.IamUrl))
                {
                    url = options.IamUrl;
                }
                else
                {
                    url = iamDefaultUrl;
                }
            }

            if (!string.IsNullOrEmpty(options.IamApiKey))
            {
                iamApikey = options.IamApiKey;
            }

            if (!string.IsNullOrEmpty(options.IamAccessToken))
            {
                userAccessToken = options.IamAccessToken;
            }

            if (!string.IsNullOrEmpty(options.IamClientId))
            {
                iamClientId = options.IamClientId;
            }

            if (!string.IsNullOrEmpty(options.IamClientSecret))
            {
                iamClientSecret = options.IamClientSecret;
            }

            if (string.IsNullOrEmpty(options.IamClientSecret) || string.IsNullOrEmpty(options.IamClientId))
            {
                iamClientId = "bx";
                iamClientSecret = "bx";
                Log.Warning("IamTokenManager():", CLIENT_ID_SECRET_WARNING);
            }
        }

        public void SetIamAuthorizationInfo(string IamClientId, string IamClientSecret)
        {
            iamClientId = IamClientId;
            iamClientSecret = IamClientSecret;
            if (string.IsNullOrEmpty(iamClientSecret) || string.IsNullOrEmpty(iamClientId))
            {
                Log.Warning("SetIamAuthorizationInfo():", CLIENT_ID_SECRET_WARNING);
            }
        }

        #region Request Token
        /// <summary>
        /// Request an IAM token using an API key.
        /// </summary>
        /// <param name="callback">The request callback.</param>
        /// <param name="error"> The request error.</param>
        /// <returns></returns>
        override protected bool RequestToken(Callback<TokenData> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("successCallback");

            RESTConnector connector = new RESTConnector();
            connector.URL = url;
            if (connector == null)
                return false;

            RequestIamTokenRequest req = new RequestIamTokenRequest();
            req.Callback = callback;
            req.HttpMethod = UnityWebRequest.kHttpVerbGET;
            req.Headers.Add("Content-type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", Utility.CreateAuthorization(iamClientId, iamClientSecret));
            req.OnResponse = OnRequestIamTokenResponse;
            req.DisableSslVerification = disableSslVerification;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["grant_type"] = new RESTConnector.Form("urn:ibm:params:oauth:grant-type:apikey");
            req.Forms["apikey"] = new RESTConnector.Form(iamApikey);
            req.Forms["response_type"] = new RESTConnector.Form("cloud_iam");

            return connector.Send(req);
        }

        private class RequestIamTokenRequest : RESTConnector.Request
        {
            public Callback<TokenData> Callback { get; set; }
        }

        private void OnRequestIamTokenResponse(RESTConnector.Request req, RESTConnector.Response resp)
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

            if (((RequestIamTokenRequest)req).Callback != null)
                ((RequestIamTokenRequest)req).Callback(response, resp.Error);
        }
        #endregion
    }

    public class IamTokenOptions : JwtTokenOptions
    {
        private string iamApiKey;
        public string IamApiKey
        {
            get
            {
                return iamApiKey;
            }
            set
            {
                if (!Utility.HasBadFirstOrLastCharacter(value))
                {
                    iamApiKey = value;
                }
                else
                {
                    throw new IBMException("The credentials shouldn't start or end with curly brackets or quotes. Be sure to remove any {} and \" characters surrounding your credentials");
                }
            }
        }
        public string IamAccessToken { get; set; }
        public string IamUrl { get; set; }
        public string IamClientId { get; set; }
        public string IamClientSecret { get; set; }
    }
}