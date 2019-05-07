/**
* Copyright 2015 IBM Corp. All Rights Reserved.
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using Utility = IBM.Cloud.SDK.Utilities.Utility;

namespace IBM.Cloud.SDK
{
    /// <summary>
    /// Helper class for holding a user and password or authorization token, used by both the WSCOnnector and RESTConnector.
    /// </summary>
    public class Credentials
    {
        #region Private Data
        private string _iamUrl;
        private IamTokenData _iamTokenData;
        private string _iamApiKey;
        private string _userAcessToken;
        private string url;
        private string username;
        private string password;
        private const string APIKEY_AS_USERNAME = "apikey";
        private const string ICP_PREFIX = "icp-";
        #endregion

        #region Public Fields
        /// <summary>
        /// The user name.
        /// </summary>
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
                    throw new IBMException("The username shouldn't start or end with curly brackets or quotes. Be sure to remove any {} and \" characters surrounding your username.");
                }
            }
        }
        /// <summary>
        /// The password.
        /// </summary>
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
                    throw new IBMException("The password shouldn't start or end with curly brackets or quotes. Be sure to remove any {} and \" characters surrounding your password.");
                }
            }
        }
        /// <summary>
        /// The Api Key.
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// The service endpoint.
        /// </summary>
        public string Url
        {
            get { return url; }
            set
            {
                if (!Utility.HasBadFirstOrLastCharacter(value))
                {
                    url = value;
                }
                else
                {
                    throw new IBMException("The service URL shouldn't start or end with curly brackets or quotes. Be sure to remove any {} and \" characters surrounding your service url.");
                }
            }
        }

        /// <summary>
        /// The IAM access token.
        /// </summary>
        public string IamAccessToken { get; set; }

        /// <summary>
        /// IAM token data.
        /// </summary>
        public IamTokenData TokenData
        {
            set
            {
                _tokenData = value;
                if (!string.IsNullOrEmpty(_tokenData.AccessToken))
                    IamAccessToken = _tokenData.AccessToken;
            }
        }
        private IamTokenData _tokenData = null;
        private bool disableSslVerification = false;
        /// <summary>
        /// Gets and sets the option to disable ssl verification for getting an IAM token.
        /// </summary>
        public bool DisableSslVerification
        {
            get { return disableSslVerification; }
            set { disableSslVerification = value; }
        }
        #endregion

        #region Callback delegates
        /// <summary>
        /// Success callback delegate.
        /// </summary>
        /// <typeparam name="T">Type of the returned object.</typeparam>
        /// <param name="response">The returned DetailedResponse.</param>
        public delegate void Callback<T>(DetailedResponse<T> response, IBMError error);
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor that takes the URL. Used for token authentication.
        /// </summary>
        public Credentials(string url = null)
        {
            Url = url;
        }

        /// <summary>
        /// Constructor that takes the user name and password.
        /// </summary>
        /// <param name="username">The string containing the user name.</param>
        /// <param name="password">A string containing the password.</param>
        /// <param name="url">The service endpoint.</param>
        public Credentials(string username, string password, string url = null)
        {
            SetCredentials(username, password, url);
        }

        /// <summary>
        /// Constructor that takes an authentication token created by the user or an ApiKey.
        /// If no URL is set then default to the non-IAM Visual Recognition endpoint.
        /// </summary>
        /// <param name="url">The service endpoint.</param>
        [Obsolete("Authentication using legacy apikey is deprecated. Please authenticate using TokenOptions.")]
        public Credentials(string apiKey, string url = null)
        {
            ApiKey = apiKey;
            Url = !string.IsNullOrEmpty(url) ? url : "https://gateway-a.watsonplatform.net/visual-recognition/api";
        }

        /// <summary>
        /// Constructor that takes IAM token options.
        /// </summary>
        /// <param name="iamTokenOptions"></param>
        public Credentials(TokenOptions iamTokenOptions, string serviceUrl = null)
        {
            SetCredentials(iamTokenOptions, serviceUrl);
        }
        #endregion

        #region SetCredentials
        private void SetCredentials(string username, string password, string url = null)
        {
            if (username == APIKEY_AS_USERNAME && !password.StartsWith(ICP_PREFIX))
            {
                TokenOptions tokenOptions = new TokenOptions()
                {
                    IamApiKey = password
                };

                SetCredentials(tokenOptions, url);
            }
            else
            {
                Username = username;
                Password = password;
            }

            if (!string.IsNullOrEmpty(url))
                Url = url;
        }

        private void SetCredentials(TokenOptions iamTokenOptions, string serviceUrl = null)
        {
            if (iamTokenOptions.IamApiKey.StartsWith(ICP_PREFIX))
            {
                SetCredentials(APIKEY_AS_USERNAME, iamTokenOptions.IamApiKey, serviceUrl);
            }
            else
            {
                if (!string.IsNullOrEmpty(serviceUrl))
                    Url = serviceUrl;
                _iamUrl = !string.IsNullOrEmpty(iamTokenOptions.IamUrl) ? iamTokenOptions.IamUrl : "https://iam.cloud.ibm.com/identity/token";
                _iamTokenData = new IamTokenData();

                if (!string.IsNullOrEmpty(iamTokenOptions.IamApiKey))
                    _iamApiKey = iamTokenOptions.IamApiKey;

                if (!string.IsNullOrEmpty(iamTokenOptions.IamAccessToken))
                    this._userAcessToken = iamTokenOptions.IamAccessToken;

                GetToken();
            }
        }
        #endregion

        #region Get Token
        /// <summary>
        /// This function sends an access token back through a callback. The source of the token
        /// is determined by the following logic:
        /// 1. If user provides their own managed access token, assume it is valid and send it
        /// 2. If this class is managing tokens and does not yet have one, make a request for one
        /// 3. If this class is managing tokens and the token has expired, refresh it
        /// 4. If this class is managing tokens and has a valid token stored, send it
        /// </summary>
        public void GetToken()
        {
            if (!string.IsNullOrEmpty(_userAcessToken))
            {
                // 1. use user-managed token
                OnGetToken(new DetailedResponse<IamTokenData>() { Result = new IamTokenData() { AccessToken = _userAcessToken } }, new IBMError());
            }
            else if (!string.IsNullOrEmpty(_iamTokenData.AccessToken) || IsRefreshTokenExpired())
            {
                // 2. request an initial token
                RequestIamToken(OnGetToken);
            }
            else if (IsTokenExpired())
            {
                // 3. refresh a token
                RefreshIamToken(OnGetToken);
            }
            else
            {
                //  4. use valid managed token

                OnGetToken(new DetailedResponse<IamTokenData>() { Result = new IamTokenData() { AccessToken = _iamTokenData.AccessToken } }, new IBMError());
            }
        }

        private void OnGetToken(DetailedResponse<IamTokenData> response, IBMError error)
        {
            SaveTokenInfo(response.Result);
        }

        #endregion

        #region Request Token
        /// <summary>
        /// Request an IAM token using an API key.
        /// </summary>
        /// <param name="callback">The request callback.</param>
        /// <param name="error"> The request error.</param>
        /// <returns></returns>
        public bool RequestIamToken(Callback<IamTokenData> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("successCallback");

            RESTConnector connector = new RESTConnector();
            connector.URL = _iamUrl;
            if (connector == null)
                return false;

            RequestIamTokenRequest req = new RequestIamTokenRequest();
            req.Callback = callback;
            req.HttpMethod = UnityWebRequest.kHttpVerbGET;
            req.Headers.Add("Content-type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Basic Yng6Yng=");
            req.OnResponse = OnRequestIamTokenResponse;
            req.DisableSslVerification = DisableSslVerification;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["grant_type"] = new RESTConnector.Form("urn:ibm:params:oauth:grant-type:apikey");
            req.Forms["apikey"] = new RESTConnector.Form(_iamApiKey);
            req.Forms["response_type"] = new RESTConnector.Form("cloud_iam");

            return connector.Send(req);
        }

        private class RequestIamTokenRequest : RESTConnector.Request
        {
            public Callback<IamTokenData> Callback { get; set; }
        }

        private void OnRequestIamTokenResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<IamTokenData> response = new DetailedResponse<IamTokenData>();
            response.Result = new IamTokenData();
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<IamTokenData>(json);
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

        #region Refresh Token
        /// <summary>
        /// Refresh an IAM token using a refresh token.
        /// </summary>
        /// <param name="callback">The success callback.</param>
        /// <param name="failCallback">The fail callback.</param>
        /// <returns></returns>
        public bool RefreshIamToken(Callback<IamTokenData> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            RESTConnector connector = new RESTConnector();
            connector.URL = _iamUrl;
            if (connector == null)
                return false;

            RefreshIamTokenRequest req = new RefreshIamTokenRequest();
            req.Callback = callback;
            req.HttpMethod = UnityWebRequest.kHttpVerbGET;
            req.Headers.Add("Content-type", "application/x-www-form-urlencoded");
            req.Headers.Add("Authorization", "Basic Yng6Yng=");
            req.OnResponse = OnRefreshIamTokenResponse;
            req.DisableSslVerification = DisableSslVerification;
            req.Forms = new Dictionary<string, RESTConnector.Form>();
            req.Forms["grant_type"] = new RESTConnector.Form("refresh_token");
            req.Forms["refresh_token"] = new RESTConnector.Form(_iamTokenData.RefreshToken);

            return connector.Send(req);
        }

        private class RefreshIamTokenRequest : RESTConnector.Request
        {
            public Callback<IamTokenData> Callback { get; set; }
        }

        private void OnRefreshIamTokenResponse(RESTConnector.Request req, RESTConnector.Response resp)
        {
            DetailedResponse<IamTokenData> response = new DetailedResponse<IamTokenData>();
            response.Result = new IamTokenData();
            foreach (KeyValuePair<string, string> kvp in resp.Headers)
            {
                response.Headers.Add(kvp.Key, kvp.Value);
            }
            response.StatusCode = resp.HttpResponseCode;

            try
            {
                string json = Encoding.UTF8.GetString(resp.Data);
                response.Result = JsonConvert.DeserializeObject<IamTokenData>(json);
                response.Response = json;
            }
            catch (Exception e)
            {
                Log.Error("Credentials.OnRefreshIamTokenResponse()", "Exception: {0}", e.ToString());
                resp.Success = false;
            }

            if (((RefreshIamTokenRequest)req).Callback != null)
                ((RefreshIamTokenRequest)req).Callback(response, resp.Error);
        }
        #endregion

        #region Token Operations
        /// <summary>
        /// Check if currently stored token is expired.
        /// 
        /// Using a buffer to prevent the edge case of the 
        /// token expiring before the request could be made.
        /// 
        /// The buffer will be a fraction of the total TTL. Using 80%.
        /// </summary>
        /// <returns></returns>
        public bool IsTokenExpired()
        {
            if (_iamTokenData.ExpiresIn == null || _iamTokenData.Expiration == null)
                return true;

            float fractionOfTtl = 0.8f;
            long? timeToLive = _iamTokenData.ExpiresIn;
            long? expireTime = _iamTokenData.Expiration;
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            long currentTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;

            double? refreshTime = expireTime - (timeToLive * (1.0 - fractionOfTtl));
            return refreshTime < currentTime;
        }

        /// <summary>
        /// Used as a fail-safe to prevent the condition of a refresh token expiring,
        /// which could happen after around 30 days.This function will return true
        /// if it has been at least 7 days and 1 hour since the last token was
        /// retrieved.
        /// </summary>
        /// <returns></returns>
        public bool IsRefreshTokenExpired()
        {
            if (_iamTokenData.Expiration == null)
            {
                return true;
            };

            long sevenDays = 7 * 24 * 3600;
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            long currentTime = (int)(System.DateTime.UtcNow - epochStart).TotalSeconds;
            long? newTokenTime = _iamTokenData.Expiration + sevenDays;
            return newTokenTime < currentTime;
        }

        /// <summary>
        /// Save the response from the IAM service request to the object's state.
        /// </summary>
        /// <param name="iamTokenData">Response object from IAM service request</param>
        public void SaveTokenInfo(IamTokenData iamTokenData)
        {
            TokenData = iamTokenData;
        }

        /// <summary>
        /// Set a self-managed IAM access token.
        /// The access token should be valid and not yet expired.
        /// 
        /// By using this method, you accept responsibility for managing the
        /// access token yourself.You must set a new access token before this
        /// one expires. Failing to do so will result in authentication errors
        /// after this token expires.
        /// </summary>
        /// <param name="iamAccessToken">A valid, non-expired IAM access token.</param>
        public void SetAccessToken(string iamAccessToken)
        {
            _userAcessToken = iamAccessToken;
        }
        #endregion

        /// <summary>
        /// Create basic authentication header data for REST requests.
        /// </summary>
        /// <returns>The authentication data base64 encoded.</returns>
        public string CreateAuthorization()
        {
            return "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(Username + ":" + Password));
        }

        /// <summary>
        /// Do we have credentials?
        /// </summary>
        /// <returns>true if the class has a username and password.</returns>
        public bool HasCredentials()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }

        /// <summary>
        /// Do we have an ApiKey?
        /// </summary>
        /// <returns>True if the class has a Authentication Token</returns>
        public bool HasApiKey()
        {
            return !string.IsNullOrEmpty(ApiKey);
        }

        /// <summary>
        /// Do we have IamTokenData?
        /// </summary>
        /// <returns></returns>
        public bool HasIamTokenData()
        {
            return _tokenData != null;
        }

        /// <summary>
        /// Do we have an IAM apikey?
        /// </summary>
        /// <returns></returns>
        public bool HasIamApikey()
        {
            return !string.IsNullOrEmpty(_iamApiKey);
        }

        /// <summary>
        /// Do we have an IAM authentication token?
        /// </summary>
        /// <returns></returns>
        public bool HasIamAuthorizationToken()
        {
            return !string.IsNullOrEmpty(_userAcessToken);
        }
    }

    /// <summary>
    /// Vcap credentials object.
    /// </summary>
    public class VcapCredentials
    {
        /// <summary>
        /// List of credentials by service name.
        /// </summary>
        [JsonProperty("VCAP_SERVICES", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<VcapCredential>> VCAP_SERVICES { get; set; }

        /// <summary>
        /// Gets a credential by name.
        /// </summary>
        /// <param name="name">Name of requested credential</param>
        /// <returns>A List of credentials who's names match the request name.</returns>
        public List<VcapCredential> GetCredentialByname(string name)
        {
            List<VcapCredential> credentialsList = new List<VcapCredential>();
            foreach (KeyValuePair<string, List<VcapCredential>> kvp in VCAP_SERVICES)
            {
                foreach (VcapCredential credential in kvp.Value)
                {
                    if (credential.Name == name)
                        credentialsList.Add(credential);
                }
            }

            return credentialsList;
        }
    }

    /// <summary>
    /// The Credential to a single service.
    /// </summary>
    public class VcapCredential
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public string Label { get; set; }
        [JsonProperty("plan", NullValueHandling = NullValueHandling.Ignore)]
        public string Plan { get; set; }
        [JsonProperty("credentials", NullValueHandling = NullValueHandling.Ignore)]
        public Credential Credentials { get; set; }
    }

    /// <summary>
    /// The Credentials.
    /// </summary>
    public class Credential
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }
        [JsonProperty("password", NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }
        [JsonProperty("workspace_id", NullValueHandling = NullValueHandling.Ignore)]
        public string WorkspaceId { get; set; }
        [JsonProperty("api_key", NullValueHandling = NullValueHandling.Ignore)]
        [Obsolete("Authentication using legacy apikey is deprecated. Please authenticate using TokenOptions.")]
        public string ApiKey { get; set; }
        [JsonProperty("apikey", NullValueHandling = NullValueHandling.Ignore)]
        public string IamApikey { get; set; }
        [JsonProperty("iam_url", NullValueHandling = NullValueHandling.Ignore)]
        public string IamUrl { get; set; }
        [JsonProperty("assistant_id", NullValueHandling = NullValueHandling.Ignore)]
        public string AssistantId { get; set; }
    }

    /// <summary>
    /// IAM token options.
    /// </summary>
    public class TokenOptions
    {
        private string iamApiKey;
        [JsonProperty("iamApiKey", NullValueHandling = NullValueHandling.Ignore)]
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
        [JsonProperty("iamAcessToken", NullValueHandling = NullValueHandling.Ignore)]
        public string IamAccessToken { get; set; }
        [JsonProperty("iamUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string IamUrl { get; set; }
    }

    /// <summary>
    /// IAM Token data.
    /// </summary>
    public class IamTokenData
    {
        [JsonProperty("access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }
        [JsonProperty("refresh_token", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshToken { get; set; }
        [JsonProperty("token_type", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenType { get; set; }
        [JsonProperty("expires_in", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExpiresIn { get; set; }
        [JsonProperty("expiration", NullValueHandling = NullValueHandling.Ignore)]
        public long? Expiration { get; set; }
    }
}
