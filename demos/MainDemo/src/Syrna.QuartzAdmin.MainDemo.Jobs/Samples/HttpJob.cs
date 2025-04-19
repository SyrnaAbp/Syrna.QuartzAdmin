using Microsoft.Extensions.Logging;
using Quartz;
using Syrna.QuartzAdmin.Jobs;
using Syrna.QuartzAdmin.Jobs.Abstractions;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace Syrna.QuartzAdmin.MainDemo.Jobs.Samples
{
    public class HttpJob(IHttpClientFactory httpClientFactory,
		ILogger<HttpJob> logger,
		IDataMapValueResolver dmvResolver) : IJob
    {
        private async Task ExecuteJob(IJobExecutionContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            try
            {
                var data = context.MergedJobDataMap;

                int? timeoutInSec = data.TryGetInt(HttpJobKeys.PropertyRequestTimeoutInSec, out var x) ? x : null;
                var dmvUrl = data.GetDataMapValue(HttpJobKeys.PropertyRequestUrl);
                var url = dmvResolver.Resolve(dmvUrl);
                if (string.IsNullOrEmpty(url))
                {
                    logger.LogWarning("[{runInstanceId}]. Cannot run HttpJob. No request url specified.",
                        context.FireInstanceId);
                    throw new JobExecutionException("No request url specified");
                }
                url = url.StartsWith("http") ? url : "http://" + url;

                var parameters = dmvResolver.Resolve(data.GetDataMapValue(HttpJobKeys.PropertyRequestParameters));
                var strHeaders = dmvResolver.Resolve(data.GetDataMapValue(HttpJobKeys.PropertyRequestHeaders));
                var headers = string.IsNullOrEmpty(strHeaders) ? null :
                    JsonSerializer.Deserialize<Dictionary<string, string>>(strHeaders.Trim());

                var strAction = data.GetString(HttpJobKeys.PropertyRequestAction);
                if (strAction == null)
                {
                    logger.LogWarning("[{runInstanceId}]. Cannot run HttpJob. No http action specified.",
                        context.FireInstanceId);
                    throw new JobExecutionException("No http action specified");
                }
                var action = Enum.Parse<HttpAction>(strAction);

                logger.LogDebug("[{runInstanceId}]. Creating HttpClient...", context.FireInstanceId);
                HttpClient httpClient;
                if (data.TryGetBoolean(HttpJobKeys.PropertyIgnoreVerifySsl, out var ignoreVerifySsl) && ignoreVerifySsl)
                {
                    httpClient = httpClientFactory.CreateClient(Constants.HttpClientIgnoreVerifySsl);
                    logger.LogInformation("[{runInstanceId}]. Created ignore SSL validation HttpClient.",
                        context.FireInstanceId);
                }
                else
                {
                    httpClient = httpClientFactory.CreateClient();
                    logger.LogInformation("[{runInstanceId}]. Created HttpClient.",
                        context.FireInstanceId);
                }

                // configure time out. Default 100 secs
                if (timeoutInSec.HasValue)
                {
                    httpClient.Timeout = timeoutInSec > 0 ? TimeSpan.FromSeconds(timeoutInSec.Value) : Timeout.InfiniteTimeSpan;
                }

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpContent reqParam = null;
                if (!string.IsNullOrEmpty(parameters))
                {
                    reqParam = new StringContent(parameters, Encoding.UTF8, Application.Json);
                }

                var response = new HttpResponseMessage();
                logger.LogInformation("[{runInstanceId}]. Sending '{action}' request to specified url '{url}'.",
                    context.FireInstanceId, action, url);
                response = action switch
                {
                    HttpAction.Get => await httpClient.GetAsync(url, context.CancellationToken),
                    HttpAction.Post => await httpClient.PostAsync(url, reqParam, context.CancellationToken),
                    HttpAction.Put => await httpClient.PutAsync(url, reqParam, context.CancellationToken),
                    HttpAction.Delete => await httpClient.DeleteAsync(url, context.CancellationToken),
                    _ => response
                };

                var result = await response.Content.ReadAsStringAsync(context.CancellationToken);
                logger.LogInformation("[{runInstanceId}]. Response status code '{code}'.",
                    context.FireInstanceId, response.StatusCode);
                context.Result = result;
                context.SetIsSuccess(response.IsSuccessStatusCode);
                context.SetReturnCode((int)response.StatusCode);
                context.SetExecutionDetails($"Request: [{response.RequestMessage}]");
            }
            catch (JobExecutionException)
            {
                context.SetIsSuccess(false);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to run HttpJob. [{runInstanceId}]",
                    context.FireInstanceId);
                context.SetIsSuccess(false);
                throw new JobExecutionException("Failed to execute http job", ex);
            }
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var taskCompletionSource = new TaskCompletionSource();
            context.CancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });
            var completedTask = await Task.WhenAny(ExecuteJob(context), taskCompletionSource.Task);

            await completedTask;
        }
    }
}

