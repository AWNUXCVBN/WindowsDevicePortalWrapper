﻿//----------------------------------------------------------------------------------------------
// <copyright file="AppDeployment.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Tools.WindowsDevicePortal.Tests;

namespace Microsoft.Tools.WindowsDevicePortal
{
    /// <content>
    /// MOCK implementation of App Deployment methods.
    /// </content>
    public partial class DevicePortal
    {
        /// <summary>
        /// API for getting installation status.
        /// </summary>
        /// <returns>The status</returns>
        public async Task<ApplicationInstallStatus> GetInstallStatus()
        {
            ApplicationInstallStatus status = ApplicationInstallStatus.None;

            Uri uri = Utilities.BuildEndpoint(
                this.deviceConnection.Connection,
                InstallStateApi);

            WebRequestHandler handler = new WebRequestHandler();
            handler.UseDefaultCredentials = false;
            handler.Credentials = this.deviceConnection.Credentials;
            handler.ServerCertificateValidationCallback = this.ServerCertificateValidation;

            using (HttpClient client = new HttpClient(handler))
            {
                this.ApplyCsrfToken(client, "GET");

                Task<HttpResponseMessage> getTask = TestHelpers.MockHttpResponder.GetAsync(uri);
                await getTask.ConfigureAwait(false);
                getTask.Wait();

                using (HttpResponseMessage response = getTask.Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            // Status code: 200
                            status = ApplicationInstallStatus.Completed;
                        }
                        else if (response.StatusCode == HttpStatusCode.NoContent)
                        {
                            // Status code: 204
                            status = ApplicationInstallStatus.InProgress;
                        }
                    }
                    else
                    {
                        status = ApplicationInstallStatus.Failed; 
                    }
                }
            }

            return status;
        }
    }
}
