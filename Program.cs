using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Globalization;

using jm.First;

var host = new HostBuilder()
    // .ConfigureServices((hostBuilderContext,services) =>
    // {
    //     services.AddSingleton<JsonSerializerSettings>(new JsonSerializerSettings()
    //     {
    //         Converters = { new DecimalFormatConverter() }
    //     });
    // })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
