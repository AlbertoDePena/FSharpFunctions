namespace FSharpFunctions.Core

open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc
open System
open System.Threading
open System.Threading.Tasks

type HttpHandler = HttpRequest -> Task<IActionResult>

type JobHandler = IServiceProvider -> CancellationToken -> Async<unit>

[<AttributeUsage(AttributeTargets.Method)>]
type HttpTriggerAttribute(name : string, methods : string) =
    inherit Attribute()

    member _.Name = 
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "HTTP trigger name is required"
        else name.Trim()

    member _.Methods = 
        if String.IsNullOrWhiteSpace(methods) then
            invalidArg "methods" "At least one HTTP method must be provided"
        else methods.Split(',') |> Array.map (fun value -> value.Trim())

[<AttributeUsage(AttributeTargets.Method)>]
type JobTriggerAttribute(name : string) =
    inherit Attribute()

    member _.Name = 
        if String.IsNullOrWhiteSpace(name) then
            invalidArg "name" "Job trigger name is required"
        else name.Trim()