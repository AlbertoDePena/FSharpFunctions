namespace FSharpFunctions.Core

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open System
open System.Threading

type HttpHandler = HttpRequest -> Async<IActionResult>

type JobHandler = IServiceProvider -> CancellationToken -> Async<unit>

[<AttributeUsage(AttributeTargets.Method)>]
type HttpTriggerAttribute(route : string, methods : string) =
    inherit Attribute()

    /// The HTTP trigger route
    member _.Route = 
        if String.IsNullOrWhiteSpace(route) then
            invalidArg "name" "HTTP trigger route is required"
        else route.Trim()

    /// The HTTP trigger methods/verbs. Comma separated multiple values can be provided (GET, POST, PUT, DELETE).
    member _.Methods = 
        if String.IsNullOrWhiteSpace(methods) then
            invalidArg "methods" "At least one HTTP method/verb must be provided"
        else methods.Split(',') |> Array.map (fun value -> value.Trim())

[<AttributeUsage(AttributeTargets.Method)>]
type JobTriggerAttribute(name : string) =
    inherit Attribute()

    /// The job trigger name
    member _.Name = 
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "Job trigger name is required"
        else name.Trim()