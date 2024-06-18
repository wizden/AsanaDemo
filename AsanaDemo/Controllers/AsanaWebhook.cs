using AsanaDemo.Controllers.TaskTransformers;
using AsanaDemo.Controllers.WebhookTransformers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AsanaDemo.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AsanaWebhook : ControllerBase
    {
        private readonly IConfiguration configuration;

        string webhookSecret = string.Empty;

        public AsanaWebhook(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook()
        {
            if (Request.Headers.TryGetValue("X-Hook-Secret", out var _secret))
            {
                return await CreateNewWebhook(_secret);
            }
            else if (Request.Headers.TryGetValue("X-Hook-Signature", out var _signature))
            {
                return await ProcessWebhook(_signature);
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskInfo(string taskGid)
        {
            AsanaTask? taskDetails = await GetAsanaTaskDetailsAsync(taskGid);

            if (taskDetails == null)
            {
                return NotFound();
            }

            return Ok(taskDetails);
        }

        private async Task<IActionResult> CreateNewWebhook(StringValues _secret)
        {
            string bearerToken = configuration["AsanaBearerToken"];
            webhookSecret = _secret.First() ?? string.Empty;
            Response.Headers.Append("X-Hook-Secret", webhookSecret);
            configuration["Asana-X-Hook-Secret"] = webhookSecret;
            LogDebug(webhookSecret);
            return Ok();
        }

        private async Task<IActionResult> ProcessWebhook(StringValues _signature)
        {
            using StreamReader reader = new(Request.Body);
            var bodyAsString = await reader.ReadToEndAsync();
            if (!await IsRequestSignatureValid(_signature, bodyAsString))
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(bodyAsString))
            {
                string error = "Empty request body while processing webhook.";
                LogDebug(error);
                return BadRequest(error);
            }

            AsanaEvents? myDeserializedClass = JsonSerializer.Deserialize<AsanaEvents>(bodyAsString);

            if (myDeserializedClass == null)
            {
                string error = $"Unable to deserialise request body to type {nameof(AsanaEvents)}.{Environment.NewLine}Body: {bodyAsString}";
                LogDebug(error);
                return BadRequest(error);
            }

            await GetTaskChanges(myDeserializedClass);

            LogDebug(bodyAsString);
            return Ok();
        }

        private async Task GetTaskChanges(AsanaEvents? myDeserializedClass)
        {
            Event? sectionChangedEvent = myDeserializedClass.events.Where(e => e.resource.resource_type == "story"
                                && e.resource.resource_subtype == "section_changed").FirstOrDefault();

            var taskGid = myDeserializedClass.events.Where(e => e.parent is Resource && ((Resource)e.parent).resource_type == "task"
                && ((Resource)e.parent).resource_subtype == "default_task")
                .Select(e => ((Resource)e.parent).gid).FirstOrDefault();

            if (sectionChangedEvent != null && !string.IsNullOrWhiteSpace(taskGid))
            {
                LogDebug($"Section changed for task {taskGid} from story {sectionChangedEvent.resource.gid}");
                AsanaTask? task = await GetAsanaTaskDetailsAsync(taskGid);

                if (task != null)
                {
                    string sectionName = task.data.memberships.Where(m => m.section != null)
                        .First().section.name;
                    LogDebug($"Task moved to {sectionName}.");
                }
            }
        }

        private async Task<bool> IsRequestSignatureValid(StringValues _signature, string bodyAsString)
        {
            var computedSecret = GetHash(bodyAsString, configuration["Asana-X-Hook-Secret"] ?? string.Empty);
            string signature = _signature.First() ?? "NoSignatureFound";
            //LogDebug(computedSecret);
            //LogDebug(signature);

            if (computedSecret != signature)
            {
                LogDebug($"Signature received {signature} does not match stored signature.");
                return false;
            }

            return true;
        }

        private async Task<AsanaTask?> GetAsanaTaskDetailsAsync(string taskGid)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", "application/json");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", configuration["AsanaBearerToken"]);
                string taskDetails = await client.GetStringAsync(new Uri($"https://app.asana.com/api/1.0/tasks/{taskGid}"));
                return JsonSerializer.Deserialize<AsanaTask>(taskDetails);
            }
        }

        private string GetHash(string requestBody, string secret)
        {
            UTF8Encoding encoding = new UTF8Encoding();

            Byte[] requestBodyBytes = encoding.GetBytes(requestBody);
            Byte[] secretBytes = encoding.GetBytes(secret);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(secretBytes))
                hashBytes = hash.ComputeHash(requestBodyBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }

        private void LogDebug(string message)
        {
            Debug.WriteLine($"{DateTime.UtcNow.ToString("s")} {message}");
        }
    }
}