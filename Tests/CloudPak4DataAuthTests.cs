/**
* Copyright 2021 IBM Corp. All Rights Reserved.
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

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using IBM.Cloud.SDK.Connection;
using IBM.Cloud.SDK.Authentication;
using IBM.Cloud.SDK.Authentication.Cp4d;

namespace IBM.Cloud.SDK.Tests
{
    public class CloudPak4DataAuthTests
    {
        [Test]
        public void TestConstructionRequried()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var password = "password";
            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator(
                url: url,
                username: username,
                password: password);

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Password == password);
        }

        [Test]
        public void TestConstructionDisableSslVerification()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var password = "password";
            var disableSslVerification = true;
            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator(
                url: url,
                username: username,
                password: password,
                disableSslVerification: disableSslVerification);

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Password == password);
            Assert.IsTrue(authenticator.DisableSslVerification == disableSslVerification);
        }

        [Test]
        public void TestConstructorHeadersWithPassword()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var password = "password";
            var headerName = "headerName";
            var headervalue = "headerValue";
            var headers = new Dictionary<string, string>();
            headers.Add(headerName, headervalue);

            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator(
                url: url,
                username: username,
                password: password,
                headers: headers
                );

            authenticator.Headers.TryGetValue(headerName, out string retrievedHeaderValue);
            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Password == password);
            Assert.IsTrue(authenticator.Headers.ContainsKey(headerName));
            Assert.IsTrue(authenticator.Headers.ContainsValue(headervalue));
            Assert.IsTrue(retrievedHeaderValue == headervalue);
        }

        [Test]
        public void TestConstructionDictionaryWithPassword()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var password = "password";
            var disableSslVerification = true;

            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add(Authenticator.PropNameUrl, url);
            config.Add(Authenticator.PropNameUsername, username);
            config.Add(Authenticator.PropNamePassword, password);
            config.Add(Authenticator.PropNameDisableSslVerification, disableSslVerification.ToString());

            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator(config);

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Password == password);
            Assert.IsTrue(authenticator.DisableSslVerification == disableSslVerification);
            Assert.IsNull(authenticator.Apikey);
        }

        [Test]
        public void TestConstructionDictionaryWithApikey()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var apikey = "apikey";
            var disableSslVerification = true;

            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add(Authenticator.PropNameUrl, url);
            config.Add(Authenticator.PropNameUsername, username);
            config.Add(Authenticator.PropNameApikey, apikey);
            config.Add(Authenticator.PropNameDisableSslVerification, disableSslVerification.ToString());

            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator(config);

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Apikey == apikey);
            Assert.IsTrue(authenticator.DisableSslVerification == disableSslVerification);
            Assert.IsNull(authenticator.Password);
        }


        [Test]
        public void TestConstructionDictionaryMissingProperty()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var password = "password";

            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add(Authenticator.PropNameUrl, url);
            config.Add(Authenticator.PropNameUsername, username);
            config.Add(Authenticator.PropNamePassword, password);

            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator(config);

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Password == password);
            Assert.IsTrue(authenticator.DisableSslVerification == false);
        }

        [Test]
        public void TestBuilderRequired()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var password = "password";

            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator()
                .WithUrl(url)
                .WithUserName(username)
                .WithPassword(password)
                .Build();

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Password == password);
        }

        [Test]
        public void TestBuilderWithApikeyRequired()
        {
            var url = "http://www.service-endpoint.com";
            var username = "username";
            var apikey = "apikey";

            CloudPakForDataAuthenticator authenticator = new CloudPakForDataAuthenticator()
                .WithUrl(url)
                .WithUserName(username)
                .WithApikey(apikey)
                .Build();

            Assert.IsNotNull(authenticator);
            Assert.IsTrue(authenticator.AuthenticationType == Authenticator.AuthTypeCp4d);
            Assert.IsTrue(authenticator.Url == url);
            Assert.IsTrue(authenticator.Username == username);
            Assert.IsTrue(authenticator.Apikey == apikey);
        }
    }
}
