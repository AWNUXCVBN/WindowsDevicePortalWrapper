﻿//----------------------------------------------------------------------------------------------
// <copyright file="RestPost.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal.Tests;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// MOCK implementation of HTTP Post
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// Submits the http post request to the specified uri.
        /// </summary>
        /// <param name="uri">The uri to which the post request will be issued.</param>
        /// <returns>Task tracking the completion of the POST request</returns>
        private async Task Post(Uri uri)
        {
            WebRequestHandler requestSettings = new WebRequestHandler();
            requestSettings.UseDefaultCredentials = false;
            requestSettings.Credentials = this.deviceConnection.Credentials;
            requestSettings.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(requestSettings))
            {
                this.ApplyCsrfToken(client, "POST");

                Task<HttpResponseMessage> postTask = TestHelpers.MockHttpResponder.PostAsync(uri, null);
                await postTask.ConfigureAwait(false);
                postTask.Wait();

                using (HttpResponseMessage response = postTask.Result)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new DevicePortalException(response);
                    }
                }
            }
        }
    }
}
