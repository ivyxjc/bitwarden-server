using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Bit.Core.Enums;
using Bit.Core.Models.Api;
using Bit.Core.Settings;
using Microsoft.Extensions.Logging;

namespace Bit.Core.Services
{
    public class RelayPushRegistrationService : BaseIdentityClientService, IPushRegistrationService
    {

        public RelayPushRegistrationService(
            IHttpClientFactory httpFactory,
            GlobalSettings globalSettings,
            ILogger<RelayPushRegistrationService> logger)
            : base(
                httpFactory,
                globalSettings.PushRelayBaseUri,
                globalSettings.Installation.IdentityUri,
                "api.push",
                $"installation.{globalSettings.Installation.Id}",
                globalSettings.Installation.Key,
                logger)
        {
        }

        public async Task CreateOrUpdateRegistrationAsync(string pushToken, string deviceId, string userId,
            string identifier, DeviceType type)
        {
            var requestModel = new PushRegistrationRequestModel
            {
                DeviceId = deviceId,
                Identifier = identifier,
                PushToken = pushToken,
                Type = type,
                UserId = userId
            };
            await SafeSendAsync(HttpMethod.Post, "push/register", requestModel);
        }

        public async Task DeleteRegistrationAsync(string deviceId)
        {
            try
            {
                await SendAsync(HttpMethod.Delete, string.Concat("push/", deviceId));
            }
            catch { }
        }

        public async Task AddUserRegistrationOrganizationAsync(IEnumerable<string> deviceIds, string organizationId)
        {
            if (!deviceIds.Any())
            {
                return;
            }

            var requestModel = new PushUpdateRequestModel(deviceIds, organizationId);
            await SendAsync(HttpMethod.Put, "push/add-organization", requestModel);
        }

        public async Task DeleteUserRegistrationOrganizationAsync(IEnumerable<string> deviceIds, string organizationId)
        {
            if (!deviceIds.Any())
            {
                return;
            }

            var requestModel = new PushUpdateRequestModel(deviceIds, organizationId);
            await SafeSendAsync(HttpMethod.Put, "push/delete-organization", requestModel);
        }

        private async Task SafeSendAsync<T>(HttpMethod method, string requestUri, T request)
        {
            try
            {
                await SendAsync(method, requestUri, request);
            }
            catch { }
        }
    }
}
