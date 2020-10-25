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
            let logger = httpRequest.GetLogger "HelloWorld"

            logger.LogInformation("Processing HelloWorld - Correlation Id {CorrelationId}", System.Guid.NewGuid())

            return OkObjectResult("Hello World!") :> IActionResult
        }
