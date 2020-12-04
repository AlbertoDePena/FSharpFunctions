namespace FSharpFunctions.Host

open System
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Http
open System.IO

module Program =

    let getFunctionsFilePath (configuration : IConfiguration) = 
        let dll = configuration.GetValue<string>("dll")
        if String.IsNullOrWhiteSpace(dll) then 
            let dll = configuration.GetValue<string>("FUNCTIONS_DLL_FILE_PATH")
            if String.IsNullOrWhiteSpace(dll) then
                failwith "FUNCTIONS_DLL_FILE_PATH environment variable not found" 
            else dll
        else dll

    let configureServices (context : WebHostBuilderContext) (services: IServiceCollection) =
        
        let functions = 
            context.Configuration
            |> getFunctionsFilePath
            |> Functions.load

        services.AddCors(fun options -> 
            options.AddPolicy("CorsPolicy", fun builder -> 
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() |> ignore)) |> ignore
        
        services.AddMvcCore() |> ignore
        services.AddApplicationInsightsTelemetry() |> ignore

        if Array.isEmpty functions.JobTriggers |> not then
            printfn "Job Triggers:\n"

            let methodInfos = 
                functions.JobTriggers 
                |> Array.map (fun jobTrigger -> 
                    printfn "\t%s\n" jobTrigger.Attribute.Name
                    jobTrigger.MethodInfo)

            services.AddHostedService(fun serviceProvider ->
                new JobTrigger.Worker(serviceProvider, methodInfos)) |> ignore

    let configure (context : WebHostBuilderContext) (builder : IApplicationBuilder) =
        
        let functions = 
            context.Configuration
            |> getFunctionsFilePath
            |> Functions.load

        if (context.HostingEnvironment.IsDevelopment()) then
            builder.UseDeveloperExceptionPage() |> ignore

        builder.UseCors("CorsPolicy") |> ignore
        builder.UseRouting() |> ignore

        if Array.isEmpty functions.HttpTriggers |> not then
            builder.UseEndpoints(fun endpoints ->
                printfn "HTTP Triggers:\n"

                for httpTrigger in functions.HttpTriggers do
                    let methods = httpTrigger.Attribute.Methods
                    let endpoint = httpTrigger.Attribute.Route

                    for method in methods do
                        match method with
                        | "OPTIONS" -> endpoints.Map(endpoint, RequestDelegate HttpTrigger.handleCors) |> ignore
                        | "GET" -> endpoints.MapGet(endpoint, RequestDelegate (HttpTrigger.handle httpTrigger.MethodInfo)) |> ignore
                        | "POST" -> endpoints.MapPost(endpoint, RequestDelegate (HttpTrigger.handle httpTrigger.MethodInfo)) |> ignore
                        | "PUT" -> endpoints.MapPut(endpoint, RequestDelegate (HttpTrigger.handle httpTrigger.MethodInfo)) |> ignore
                        | "DELETE" -> endpoints.MapDelete(endpoint, RequestDelegate (HttpTrigger.handle httpTrigger.MethodInfo)) |> ignore                    
                        | _ -> ()

                    printfn "\thttp://localhost:<port>/%s - %s\n" endpoint (String.Join(" ", methods))

                ) |> ignore

    let configureAppConfiguration (context : WebHostBuilderContext) (builder : IConfigurationBuilder) =
        let directory = 
            builder.Build() 
            |> getFunctionsFilePath 
            |> Path.GetDirectoryName

        builder
            .SetBasePath(directory)
            .AddJsonFile("local.settings.json", optional = true, reloadOnChange = true) |> ignore

    [<EntryPoint>]
    let main args =
        
        let showHelp =
            Environment.GetEnvironmentVariable("FUNCTIONS_DLL_FILE_PATH") 
            |> String.IsNullOrWhiteSpace && Array.isEmpty args

        if showHelp then
            printfn "FSharp Functions Host\n"
            printfn "Usage: fsharp-functions-host --dll <functions DLL path> --urls <ASP NET Core URLS>\n"
            printfn "--dll      The file path of the DLL that contains FSharp functions. Example: .\MyFunctions.dll"
            printfn "--urls     The ASP NET Core application URLS. Default: http://localhost:5000"
        else
            try
                Host.CreateDefaultBuilder(args)
                    .ConfigureWebHostDefaults(fun builder ->
                        builder
                            .Configure(Action<WebHostBuilderContext, IApplicationBuilder> configure)
                            .ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
                            .ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureAppConfiguration)
                            |> ignore)
                    .Build()
                    .Run()
            with
            | ex ->
                Console.WriteLine(ex)

        0 // exit code
