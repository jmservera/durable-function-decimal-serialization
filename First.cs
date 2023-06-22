using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace jm.First
{
    public class DecimalFormatConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()!;
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            FormattableString formattableString = $"{value}";
            writer.WriteStringValue(formattableString.ToString(CultureInfo.InvariantCulture));
        }
    }

    public class MyData
    {
        [JsonConverter(typeof(DecimalFormatConverter))]
        public Decimal MyNumber { get; set; }

        public Decimal MyNumber2 { get; set; }
        public string? Name { get; set; }
    }

    public static class First
    {
        [Function(nameof(First))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(First));
            logger.LogInformation("Saying hello.");
            var outputs = new List<string>();

            // Replace name and input with values relevant for your Durable Functions Activity
            logger.LogInformation("Sending 999999999999999999 to Carl");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData() { Name = "Carl", MyNumber = 999999999999999999, MyNumber2 = 999999999999999999 }));
            logger.LogInformation("Sending 999999999999999999.0 to John");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData() { Name = "John", MyNumber = 999999999999999999.0m, MyNumber2 = 999999999999999999.0m }));
            logger.LogInformation("Sending 1.000000000000000000000033288 to Mary");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData() { Name = "Mary", MyNumber = 1.000000000000000000000033288m, MyNumber2 = 1.000000000000000000000033288m }));
            logger.LogInformation("Sending 2.2 to Bob");
            outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData() { Name = "Bob", MyNumber = 2.2m, MyNumber2 = 2.2m }));

            return outputs;
        }

        [Function(nameof(SayHello))]
        public static string SayHello([ActivityTrigger] MyData helloData, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("SayHello");
            logger.LogInformation($"Your values are Name: {helloData.Name} Number1: {helloData.MyNumber} Number2: {helloData.MyNumber2}.");
            return $"Hello {helloData.Name}! Your values are Number1: {helloData.MyNumber} Number2: {helloData.MyNumber2}.";
        }

        [Function("First_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("First_HttpStart");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(First));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return client.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
