namespace FSharpFunctions.Core

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc

type HttpHandler = HttpRequest -> Task<IActionResult>

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