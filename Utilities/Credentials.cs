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

using IBM.Cloud.SDK.Utilities;
using IBM.Cloud.SDK.Authentication;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Utility = IBM.Cloud.SDK.Utilities.Utility;

namespace IBM.Cloud.SDK
{
    /// <summary>
    /// Helper class for holding a user and password or authorization token, used by both the WSCOnnector and RESTConnector.
    /// </summary>
    public class Credentials
    {
        public IamTokenManager iamTokenManager;
        public Icp4dTokenManager icp4dTokenManager;

        #region Private Data
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
        public Credentials(IamTokenOptions iamTokenOptions, string serviceUrl = null)
        {
            if (!string.IsNullOrEmpty(serviceUrl))
                Url = serviceUrl;
            SetCredentials(iamTokenOptions, serviceUrl);
        }

        /// <summary>
        /// Constructor that takes IAM token options.
        /// </summary>
        /// <param name="TokenOptions"></param>
        public Credentials(TokenOptions tokenOptions, string serviceUrl = null)
        {
            if (!string.IsNullOrEmpty(serviceUrl))
                Url = serviceUrl;
            SetCredentials(tokenOptions, serviceUrl);
        }

        /// <summary>
        /// Constructor that takes ICP4D token options.
        /// </summary>
        /// <param name="icp4dTokenOptions"></param>
        public Credentials(Icp4dTokenOptions icp4dTokenOptions, string serviceUrl = null)
        {
            if (!string.IsNullOrEmpty(serviceUrl))
                Url = serviceUrl;
            SetCredentials(icp4dTokenOptions, serviceUrl);
        }
        #endregion

        #region SetCredentials
        private void SetCredentials(string username, string password, string url = null)
        {
            if (username == APIKEY_AS_USERNAME && !password.StartsWith(ICP_PREFIX))
            {
                IamTokenOptions tokenOptions = new IamTokenOptions()
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

        private void SetCredentials(IamTokenOptions iamTokenOptions, string serviceUrl = null)
        {
            if (iamTokenOptions.IamApiKey.StartsWith(ICP_PREFIX))
            {
                SetCredentials(APIKEY_AS_USERNAME, iamTokenOptions.IamApiKey, serviceUrl);
            }
            else
            {
                iamTokenManager = new IamTokenManager(iamTokenOptions);
                iamTokenManager.GetToken();
            }
        }

        private void SetCredentials(TokenOptions tokenOptions, string serviceUrl = null)
        {
            if (tokenOptions.IamApiKey.StartsWith(ICP_PREFIX))
            {
                SetCredentials(APIKEY_AS_USERNAME, tokenOptions.IamApiKey, serviceUrl);
            }
            else
            {
                string iamApiKey = tokenOptions.IamApiKey;
                IamTokenOptions iamTokenOptions = new IamTokenOptions()
                {
                    IamApiKey = iamApiKey
                };
                iamTokenManager = new IamTokenManager(iamTokenOptions);
                iamTokenManager.GetToken();
            }
        }

        private void SetCredentials(Icp4dTokenOptions icp4dTokenOptions, string serviceUrl = null)
        {
            icp4dTokenManager = new Icp4dTokenManager(icp4dTokenOptions);
            icp4dTokenManager.GetToken();
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
        /// Do we have Token Data?
        /// </summary>
        /// <returns>true if the class has a username and password.</returns>
        public bool HasTokenData()
        {
            return HasIamTokenData() || HasIcp4dTokenData();
        }

        /// <summary>
        /// Do we have IAM token data?
        /// </summary>
        /// <returns>true if the class has a username and password.</returns>
        public bool HasIamTokenData()
        {
            if (iamTokenManager != null)
            {
                return iamTokenManager.HasTokenData();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Do we have ICP4D token data?
        /// </summary>
        /// <returns>true if the class has a username and password.</returns>
        public bool HasIcp4dTokenData()
        {
            if (icp4dTokenManager != null)
            {
                return icp4dTokenManager.HasTokenData();
            }
            else
            {
                return false;
            }
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
    /// IAM token options. // Support legacy code
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
}