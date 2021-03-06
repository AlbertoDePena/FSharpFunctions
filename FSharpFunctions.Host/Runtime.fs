﻿namespace FSharpFunctions.Host

open System
open System.Reflection
open FSharpFunctions.Core
open Microsoft.AspNetCore.Routing
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Mvc.Abstractions
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Hosting

type HttpTriggerMetadata = {
    Attribute : HttpTriggerAttribute
    MethodInfo : MethodInfo
}

type JobTriggerMetadata = {
    Attribute : JobTriggerAttribute
    MethodInfo : MethodInfo
}

type Functions = {
    HttpTriggers : HttpTriggerMetadata array
    JobTriggers : JobTriggerMetadata array
}

[<RequireQualifiedAccess>]
module Functions =

    let load (assemblyFile : string) =

        let loadAttributeMetadata (attributeType : 't) =
            Assembly.LoadFrom(assemblyFile).GetTypes()
            |> Array.collect (fun t -> t.GetMethods())
            |> Array.choose (fun methodInfo ->
                let attribute = methodInfo.GetCustomAttribute(attributeType)
                if isNull attribute then
                    None
                else Some (attribute, methodInfo))

        { HttpTriggers =
            loadAttributeMetadata(typedefof<HttpTriggerAttribute>)
            |> Array.map (fun (attribute, methodInfo) -> 
                { Attribute = attribute :?> HttpTriggerAttribute; MethodInfo = methodInfo })
          JobTriggers =
            loadAttributeMetadata(typedefof<JobTriggerAttribute>)
            |> Array.map (fun (attribute, methodInfo) -> 
                { Attribute = attribute :?> JobTriggerAttribute; MethodInfo = methodInfo }) }

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
            fun httpContext ->
                methodInfo.Invoke(null, [| httpContext |]) :?> Async<IActionResult>
            
        let computation = async {
            let! actionResult = httpHandler httpContext
                
            let routeData = 
                let data = httpContext.GetRouteData()
                if isNull data then
                    emptyRouteData
                else data
                
            return! actionResult.ExecuteResultAsync(ActionContext(httpContext, routeData, emptyActionDescriptor)) |> Async.AwaitTask
        } 
        
        Async.StartAsTask (computation) :> Task

[<RequireQualifiedAccess>]
module JobTrigger =

    type Worker(serviceProvider : IServiceProvider, methodInfos : MethodInfo []) =
        inherit BackgroundService()

        override _.ExecuteAsync cancellationToken =
            
            let jobHandlers : JobHandler [] =
                methodInfos
                |> Array.map (fun methodInfo ->
                    fun jobContext ->
                        methodInfo.Invoke(null, [| jobContext |]) :?> Async<unit>)

            let jobContext : JobContext = {
                RequestServices = serviceProvider
                CancellationToken = cancellationToken
            }

            let computation = 
                jobHandlers
                |> Array.map (fun jobHandler -> jobHandler jobContext)
                |> Async.Parallel

            Async.StartAsTask (computation, cancellationToken = cancellationToken) :> Task