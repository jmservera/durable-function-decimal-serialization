# Durable Functions Decimal Serialization

The current Json serializer in Durable Functions is not correctly serializing Decimal values. This is an example code of how to use a custom JsonConverter
to serialize the values as string to convert them back in the correct number, using a converter attribute.

You can find a small class with two decimals, one with the converter and another one without it:

```csharp	
public class MyData{
    [JsonConverter(typeof(DecimalFormatConverter))]
    public Decimal MyNumber {get;set;}
    public Decimal MyNumber2 {get;set;}
    public string? Name {get;set;}
}
```	

The converter is quite simple, it just converts the decimal to a string and back to a decimal:

```csharp	
public class DecimalFormatConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value=reader.GetString()!;
        return decimal.Parse(value, CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
    {
        FormattableString formattableString = $"{value}";
        writer.WriteStringValue(formattableString.ToString(CultureInfo.InvariantCulture));
    }
}
```

Then in the function I send and receive the data between the two durable functions:

```csharp
[Function(nameof(First))]
public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
{
    ILogger logger = context.CreateReplaySafeLogger(nameof(First));
    logger.LogInformation("Saying hello.");
    var outputs = new List<string>();

    // Replace name and input with values relevant for your Durable Functions Activity
    logger.LogInformation("Sending 999999999999999999 to Carl");
    outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData(){Name="Carl",MyNumber=999999999999999999, MyNumber2=999999999999999999}));
    logger.LogInformation("Sending 999999999999999999.0 to John");
    outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData(){Name="John",MyNumber=999999999999999999.0m, MyNumber2=999999999999999999.0m}));
    logger.LogInformation("Sending 1.000000000000000000000033288 to Mary");
    outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData(){Name="Mary",MyNumber=1.000000000000000000000033288m,MyNumber2=1.000000000000000000000033288m}));
    logger.LogInformation("Sending 2.2 to Bob");
    outputs.Add(await context.CallActivityAsync<string>(nameof(SayHello), new MyData(){Name="Bob",MyNumber=2.2m,MyNumber2=2.2m}));

    return outputs;
}

[Function(nameof(SayHello))]
public static string SayHello([ActivityTrigger] MyData helloData, FunctionContext executionContext)
{
    ILogger logger = executionContext.GetLogger("SayHello");
    logger.LogInformation($"Your values are Name: {helloData.Name} Number1: {helloData.MyNumber} Number2: {helloData.MyNumber2}.");
    return $"Hello {helloData.Name}! Your values are Number1: {helloData.MyNumber} Number2: {helloData.MyNumber2}.";
}
```

And in the log you can see the difference between the two serializations:

```log
[2023-06-22T07:14:55.671Z] Executing 'Functions.First' (Reason='(null)', Id=2fe76fae-217d-475c-be23-f8bb7c5712b2)
[2023-06-22T07:14:55.815Z] Saying hello.
[2023-06-22T07:14:55.817Z] Sending 999999999999999999 to Carl
[2023-06-22T07:14:55.853Z] Executed 'Functions.First' (Succeeded, Id=2fe76fae-217d-475c-be23-f8bb7c5712b2, Duration=191ms)
[2023-06-22T07:14:55.882Z] Executing 'Functions.SayHello' (Reason='(null)', Id=a6dc89bc-df79-4804-a6b1-3e7cb4793b29)
[2023-06-22T07:14:55.902Z] Your values are Name: Carl Number1: 999999999999999999 Number2: 999999999999999999.
[2023-06-22T07:14:55.906Z] Executed 'Functions.SayHello' (Succeeded, Id=a6dc89bc-df79-4804-a6b1-3e7cb4793b29, Duration=24ms)
[2023-06-22T07:14:55.939Z] Executing 'Functions.First' (Reason='(null)', Id=f21668bd-01af-4107-aa40-939a4abbf699)
[2023-06-22T07:14:55.954Z] Sending 999999999999999999.0 to John
[2023-06-22T07:14:55.958Z] Executed 'Functions.First' (Succeeded, Id=f21668bd-01af-4107-aa40-939a4abbf699, Duration=20ms)
[2023-06-22T07:14:55.973Z] Executing 'Functions.SayHello' (Reason='(null)', Id=cf1317d8-ab8f-41fc-8c32-43b3f3c955c3)
[2023-06-22T07:14:55.979Z] Your values are Name: John Number1: 999999999999999999.0 Number2: 1000000000000000000.
[2023-06-22T07:14:55.981Z] Executed 'Functions.SayHello' (Succeeded, Id=cf1317d8-ab8f-41fc-8c32-43b3f3c955c3, Duration=8ms)
[2023-06-22T07:14:55.994Z] Executing 'Functions.First' (Reason='(null)', Id=be129426-cf8b-4514-be0b-119d292a0fc9)
[2023-06-22T07:14:56.025Z] Sending 1.000000000000000000000033288 to Mary
[2023-06-22T07:14:56.029Z] Executed 'Functions.First' (Succeeded, Id=be129426-cf8b-4514-be0b-119d292a0fc9, Duration=35ms)
[2023-06-22T07:14:56.040Z] Executing 'Functions.SayHello' (Reason='(null)', Id=19386afc-2bbd-4973-a8ca-feaae15167ed)
[2023-06-22T07:14:56.045Z] Your values are Name: Mary Number1: 1.000000000000000000000033288 Number2: 1.0.
[2023-06-22T07:14:56.047Z] Executed 'Functions.SayHello' (Succeeded, Id=19386afc-2bbd-4973-a8ca-feaae15167ed, Duration=7ms)
[2023-06-22T07:14:56.060Z] Executing 'Functions.First' (Reason='(null)', Id=9b0abfea-b8e5-4993-b911-153f5aba6eeb)
[2023-06-22T07:14:56.110Z] Sending 2.2 to Bob
[2023-06-22T07:14:56.113Z] Executed 'Functions.First' (Succeeded, Id=9b0abfea-b8e5-4993-b911-153f5aba6eeb, Duration=53ms)
[2023-06-22T07:14:56.123Z] Executing 'Functions.SayHello' (Reason='(null)', Id=ff4a5c13-6bc5-49a0-aa1c-2d21ae915ed9)
[2023-06-22T07:14:56.171Z] Your values are Name: Bob Number1: 2.2 Number2: 2.2.
[2023-06-22T07:14:56.172Z] Executed 'Functions.SayHello' (Succeeded, Id=ff4a5c13-6bc5-49a0-aa1c-2d21ae915ed9, Duration=49ms)
[2023-06-22T07:14:56.191Z] Executing 'Functions.First' (Reason='(null)', Id=eab1f5b5-5aba-4223-94bc-d380816bc0ee)
[2023-06-22T07:14:56.251Z] Executed 'Functions.First' (Succeeded, Id=eab1f5b5-5aba-4223-94bc-d380816bc0ee, Duration=59ms)
```

## Other workarounds

You can enforce the converter generally for all functions by adding the following to your `Startup.cs`:

```csharp
.ConfigureServices((hostBuilderContext,services) =>
{
    services.Configure<JsonSerializerOptions>(options =>
    {
        options.Converters.Add(new DecimalFormatConverter());
    });
})
```

Or you can also format all the numbers as strings:

```csharp
.ConfigureServices((hostBuilderContext,services) =>
{
    services.Configure<JsonSerializerOptions>(options =>
    {
        options.NumberHandling= JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;
    });
})
```
