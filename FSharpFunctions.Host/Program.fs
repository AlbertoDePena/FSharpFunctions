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

    let getFunctionsDll (configuration : IConfiguration) = 
        let dll = configuration.GetValue<string>("dll")
        if String.IsNullOrWhiteSpace(dll) then 
            let dll = configuration.GetValue<string>("FUNCTIONS_DLL")
            if String.IsNullOrWhiteSpace(dll) then
                failwith "FUNCTIONS_DLL environment variable not found" 
            else dll
        else dll

    let configureServices (context : WebHostBuilderContext) (services: IServiceCollection) =
        
        services.AddCors(fun options -> 
            options.AddPolicy("CorsPolicy", fun builder -> 
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() |> ignore)) |> ignore
        
        services.AddMvcCore() |> ignore
        services.AddApplicationInsightsTelemetry() |> ignore

    let configure (context : WebHostBuilderContext) (builder : IApplicationBuilder) =
        
        if (context.HostingEnvironment.IsDevelopment()) then
            builder.UseDeveloperExceptionPage() |> ignore

        builder.UseCors("CorsPolicy") |> ignore
        builder.UseRouting() |> ignore

        let functions = 
            context.Configuration
            |> getFunctionsDll
            |> Functions.load

        builder.UseEndpoints(fun endpoints ->
            printfn "HTTP Triggers:\n"

            for httpTrigger in functions.HttpTriggers do
                let methodInfo = httpTrigger.MethodInfo
                let methods = httpTrigger.Attribute.Methods
                let endpoint = sprintf "api/%s" httpTrigger.Attribute.Name

                for method in methods do
                    match method with
                    | "OPTIONS" -> endpoints.Map(endpoint, RequestDelegate HttpTrigger.handleCors) |> ignore
                    | "GET" -> endpoints.MapGet(endpoint, RequestDelegate (HttpTrigger.handle methodInfo)) |> ignore
                    | "POST" -> endpoints.MapPost(endpoint, RequestDelegate (HttpTrigger.handle methodInfo)) |> ignore
                    | "PUT" -> endpoints.MapPut(endpoint, RequestDelegate (HttpTrigger.handle methodInfo)) |> ignore
                    | "DELETE" -> endpoints.MapDelete(endpoint, RequestDelegate (HttpTrigger.handle methodInfo)) |> ignore                    
                    | _ -> ()

                printfn "http://localhost:<port>/%s - %s\n" endpoint (String.Join(" ", methods))

            ) |> ignore

    let configureAppConfiguration (context : WebHostBuilderContext) (builder : IConfigurationBuilder) =
        if context.HostingEnvironment.IsDevelopment() then           
            let assemblyFile =  builder.Build() |> getFunctionsDll
            let directory = Path.GetDirectoryName(assemblyFile)

            builder.SetBasePath(directory).AddJsonFile("local.settings.json", optional = true, reloadOnChange = true) |> ignore

    [<EntryPoint>]
    let main args =
        
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun builder ->
                builder
                    .Configure(Action<WebHostBuilderContext, IApplicationBuilder> configure)
                    .ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
                    .ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureAppConfiguration)
                    |> ignore)
            .Build()
            .Run()

        0 // exit code
