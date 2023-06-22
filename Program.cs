using jm.First;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()

    // With this option you would format all the numbers as strings    
    // .ConfigureServices((hostBuilderContext,services) =>
    // {
    //     services.Configure<JsonSerializerOptions>(options =>
    //     {
    //         options.NumberHandling= JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;
    //     });
    // })

    // With this one you only format the decimal numbers as strings
    // .ConfigureServices((hostBuilderContext,services) =>
    // {
    //     services.Configure<JsonSerializerOptions>(options =>
    //     {
    //         options.Converters.Add(new DecimalFormatConverter());
    //     });
    // })
    .Build();

host.Run();
