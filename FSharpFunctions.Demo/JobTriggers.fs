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
        fun serviceProvider cancellationToken -> async {
            let logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CurrentTime")
            let configuration = serviceProvider.GetRequiredService<IConfiguration>()

            let userName = configuration.GetValue<string>("USER_NAME")

            while not cancellationToken.IsCancellationRequested do
                logger.LogInformation(
                    "Hey {UserName}, the current time is {CurrentTime}", 
                        userName, DateTime.Now)

                do! Async.Sleep(30000)
        }

    [<JobTrigger(name = "CurrentHour")>]
    let currentHour : JobHandler =
        fun serviceProvider cancellationToken -> async {
            let logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CurrentHour")
            let configuration = serviceProvider.GetRequiredService<IConfiguration>()

            let userName = configuration.GetValue<string>("USER_NAME")

            while not cancellationToken.IsCancellationRequested do
                logger.LogInformation(
                    "Hey {UserName}, the current hour is {CurrentTime}", 
                        userName, DateTime.Today)

                do! Async.Sleep(30000)
        }

