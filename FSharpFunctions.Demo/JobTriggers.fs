namespace FSharpFunctions.Demo

open System
open FSharpFunctions.Core
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

[<RequireQualifiedAccess>]
module JobTriggers =

    [<JobTrigger(name = "CurrentTime")>]
    let currentTime : JobHandler =
        fun jobContext -> async {
            let logger = jobContext.CreateLogger("CurrentTime")
            
            let userName = jobContext.Configuration.GetValue<string>("USER_NAME")

            while not jobContext.CancellationToken.IsCancellationRequested do
                logger.LogInformation(
                    "Hey {UserName}, the current time is {CurrentTime}", 
                        userName, DateTime.Now)

                do! Async.Sleep(30000)
        }

