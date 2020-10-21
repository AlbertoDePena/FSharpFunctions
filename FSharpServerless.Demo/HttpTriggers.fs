namespace FSharpServerless.Demo

open FSharpServerless.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Mvc

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(name = "HelloWorld", methods = "GET")>]
    let helloWorld : HttpHandler =
        fun httpRequest -> task {
            
            return BadRequestObjectResult("Not good") :> IActionResult
        }

    [<HttpTrigger(name = "HelloLaz", methods = "GET")>]
    let helloLaz : HttpHandler =
        fun httpRequest -> task {
            
            return OkObjectResult("It Works") :> IActionResult
        }
