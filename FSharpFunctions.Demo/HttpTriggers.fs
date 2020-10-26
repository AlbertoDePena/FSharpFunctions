namespace FSharpFunctions.Demo

open FSharpFunctions.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(name = "HelloWorld", methods = "GET")>]
    let helloWorld : HttpHandler =
        fun httpRequest -> task {
            let logger = httpRequest.HttpContext.GetLogger "HelloWorld"

            logger.LogInformation(
                "Processing HelloWorld - Correlation Id {CorrelationId}", System.Guid.NewGuid())

            let message =
                sprintf "Hello %s, you are %i years old!" 
                    (Environment.GetEnvironmentVariableAsString("USER_NAME"))
                    (Environment.GetEnvironmentVariableAsInt("USER_AGE"))

            return OkObjectResult(message) :> IActionResult
        }
