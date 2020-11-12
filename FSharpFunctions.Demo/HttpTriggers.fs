namespace FSharpFunctions.Demo

open FSharpFunctions.Core
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(name = "HelloWorld", methods = "GET")>]
    let helloWorld : HttpHandler =
        fun httpRequest -> async {
            let logger = httpRequest.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("HelloWorld")
            let configuration = httpRequest.HttpContext.RequestServices.GetRequiredService<IConfiguration>()

            logger.LogInformation(
                "Processing HelloWorld - Correlation Id {CorrelationId}", 
                    System.Guid.NewGuid())

            let message =
                sprintf "Hello %s, you are %i years old!" 
                    (configuration.GetValue<string>("USER_NAME"))
                    (configuration.GetValue<int>("USER_AGE"))

            return OkObjectResult(message) :> IActionResult
        }
