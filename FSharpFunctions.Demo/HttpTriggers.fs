namespace FSharpFunctions.Demo

open FSharpFunctions.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(name = "HelloWorld", methods = "GET")>]
    let helloWorld : HttpHandler =
        fun httpRequest -> task {
            let logger = httpRequest.HttpContext.GetLogger "HelloWorld"
            let configuration = httpRequest.HttpContext.Configuration

            logger.LogInformation(
                "Processing HelloWorld - Correlation Id {CorrelationId}", System.Guid.NewGuid())

            let message =
                sprintf "Hello %s, you are %i years old!" 
                    (configuration.GetValue<string>("USER_NAME"))
                    (configuration.GetValue<int>("USER_AGE"))

            return OkObjectResult(message) :> IActionResult
        }
