namespace FSharpFunctions.Host

open System
open System.IO
open System.Reflection
open FSharpFunctions.Core
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.Abstractions
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Mvc
open Newtonsoft.Json
open System.Collections.Generic

type HttpTriggerMetadata = {
    Attribute : HttpTriggerAttribute
    MethodInfo : MethodInfo
}

type Functions = {
    HttpTriggers : HttpTriggerMetadata array
}

[<RequireQualifiedAccess>]
module Functions =

    let configure (assemblyFile : string) =
        if Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") = "Development" then   
            printfn "Setting environment variables from local.settings.json\n"
            Path.Join(Path.GetDirectoryName(assemblyFile), "local.settings.json")
            |> File.ReadAllText
            |> JsonConvert.DeserializeObject<Dictionary<string, string>>
            |> Seq.iter (fun x -> Environment.SetEnvironmentVariable(x.Key, x.Value))

    let load (assemblyFile : string) =
        { HttpTriggers =
            Assembly.LoadFrom(assemblyFile).GetTypes()
            |> Array.collect (fun t -> t.GetMethods())
            |> Array.choose (fun methodInfo ->
                let attribute = methodInfo.GetCustomAttribute(typedefof<HttpTriggerAttribute>)
                if isNull attribute then
                    None
                else Some {
                    Attribute = attribute :?> HttpTriggerAttribute
                    MethodInfo = methodInfo }) }

[<RequireQualifiedAccess>]
module HttpTrigger =

    let private emptyRouteData = RouteData()
    
    let private emptyActionDescriptor = ActionDescriptor()

    let handleCors (httpContext : HttpContext) =
        if String.Equals(httpContext.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase) then
            httpContext.Response.StatusCode <- 200
        else
            httpContext.Response.StatusCode <- 405
        
        Task.CompletedTask
            
    let handle (methodInfo : MethodInfo) (httpContext : HttpContext) = 

        let httpHandler : HttpHandler =
            fun httpRequest ->
                methodInfo.Invoke(null, [| httpRequest |]) :?> Task<IActionResult>
            
        let computation = task {
            let! actionResult = httpHandler httpContext.Request
                
            let routeData = 
                let data = httpContext.GetRouteData()
                if isNull data then
                    emptyRouteData
                else data
                
            return! actionResult.ExecuteResultAsync(ActionContext(httpContext, routeData, emptyActionDescriptor)) 
        } 
                
        computation :> Task


