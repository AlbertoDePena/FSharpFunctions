namespace FSharpServerless.Demo

open FSharpServerless.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(name = "HelloWorld", methods = "GET")>]
    let helloWorld : HttpHandler =
        fun httpRequest -> task {
            let loggerFactory = httpRequest.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            let logger = loggerFactory.CreateLogger("HelloWorld")

            logger.LogInformation("Processing HelloWorld - Correlation Id {CorrelationId}", System.Guid.NewGuid())

            return OkObjectResult("Hello World!") :> IActionResult
        }
