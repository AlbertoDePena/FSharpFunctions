namespace FSharpFunctions.Demo

open FSharpFunctions.Core
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(route = "api/HelloWorld", methods = "GET")>]
    let helloWorld : HttpHandler =
        fun httpContext -> async {
            let logger = httpContext.CreateLogger("HelloWorld")
            
            let userName = httpContext.Configuration.GetValue<string>("USER_NAME")
            let userAge = httpContext.Configuration.GetValue<int>("USER_AGE")

            logger.LogInformation(
                "Processing HelloWorld - Correlation Id {CorrelationId}", 
                    System.Guid.NewGuid())

            let message =
                sprintf "Hello %s, you are %i years old!" userName userAge

            return OkObjectResult(message) :> IActionResult
        }
