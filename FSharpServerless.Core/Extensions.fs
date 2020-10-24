namespace FSharpServerless.Core

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Primitives

[<AutoOpen>]
module HttpRequestExtensions =

    type HttpRequest with

        member this.GetLogger (categoryName : string) =
            let loggerFactory = this.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            loggerFactory.CreateLogger categoryName

        member this.TryGetBearerToken () =
            this.Headers 
            |> Seq.tryFind (fun q -> q.Key = "Authorization")
            |> Option.map (fun q -> if Seq.isEmpty q.Value then String.Empty else q.Value |> Seq.head)
            |> Option.map (fun h -> h.Substring("Bearer ".Length).Trim())

        member this.TryGetQueryStringValue (name : string) =
            let hasValue, values = this.Query.TryGetValue(name)
            if hasValue
            then values |> Seq.tryHead
            else None

        member this.TryGetHeaderValue (name : string) =
            let hasHeader, values = this.Headers.TryGetValue(name)
            if hasHeader
            then values |> Seq.tryHead
            else None

        member this.TryGetFormValue (key : string) =
            match this.HasFormContentType with
            | false -> None
            | true  ->
                match this.Form.TryGetValue key with
                | true , value -> Some (value.ToString())
                | false, _     -> None

        member this.SetHttpHeader (key : string) (value : obj) =
            this.Headers.[key] <- StringValues(value.ToString())

[<RequireQualifiedAccess>]
module Async =
    
    let singleton x = async {
        return x
    }

    let map f (computation: Async<'t>) = async {
        let! x = computation
        return f x
    }

    let bind f (computation: Async<'t>) = async {
        let! x = computation
        return! f x
    }
    
    /// <summary>
    /// Async.StartAsTask and up-cast from Task<unit> to plain Task.
    /// </summary>
    /// <param name="task">The asynchronous computation.</param>
    let AsTask (task : Async<unit>) = Async.StartAsTask task :> Task

[<RequireQualifiedAccess>]
module AppSettings =

    let private getValue parser defaultValue variableName =
        let parsed, value = variableName |> Environment.GetEnvironmentVariable |> parser
        if parsed then 
            value
        else 
            defaultValue

    let getString variableName =
        getValue (fun value -> not (String.IsNullOrWhiteSpace value), value) String.Empty variableName

    let getInt variableName =
        getValue Int32.TryParse 0 variableName

    let getLong variableName =
        getValue Int64.TryParse 0L variableName

    let getBool variableName =
        getValue bool.TryParse false variableName