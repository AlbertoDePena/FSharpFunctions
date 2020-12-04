# FSharp - Functions

I got sick and tired of creating a DOT NET web API and having to fill in the same shell every time. This project aims to make the routine of creating HTTP handlers and background services as minimal as possible.

[FSharpFunctions.Core](https://www.nuget.org/packages/FSharpFunctions.Core/1.0.2) is a NuGet package with a couple of custom attributes for aiding in this approach:

* `HttpTriggerAttribute`: used for exposing HTTP request handlers
* `JobTriggerAttribute`: used for running background services

## Creating Custom Triggers

[FSharp Functions Demo](https://github.com/AlbertoDePena/FSharpFunctions/tree/master/FSharpFunctions.Demo)

Create a dotnet core library project and reference `FSharpFunctions.Core` NuGet package. 

```fsproj
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <None Include="local.settings.json" />
  </ItemGroup>

  <ItemGroup>
    <Compile Include="Triggers.fs" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FSharpFunctions.Core" Version="1.0.4" />
  </ItemGroup>

  <ItemGroup Condition="'$(Configuration)' == 'Debug'">
    <Content Include="local.settings.json">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

To provide configuration values to the custom triggers, a `local.settings.json` file must be created.

```JSON
// local.settings.json

{
    "USER_NAME": "Some User",
    "USER_AGE": "32"
}
```

```F#
// Triggers.fs

namespace MyCustomTriggers

open FSharpFunctions.Core
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration

[<RequireQualifiedAccess>]
module HttpTriggers =

    [<HttpTrigger(route = "api/HelloWorld", methods = "GET, POST")>]
    let helloWorld : HttpHandler =
        fun httpContext -> async {
            let logger = httpContext.CreateLogger("HelloWorld")
            let userName = httpContext.Configuration.GetValue<string>("USER_NAME")
            let userAge = httpContext.Configuration.GetValue<int>("USER_AGE")

            logger.LogInformation(
                "Processing HelloWorld - Correlation Id {CorrelationId}", 
                    System.Guid.NewGuid())

            let message =
                sprintf "Hello %s, you are %i years old!" userName userAge

            return OkObjectResult(message) :> IActionResult
        }

[<RequireQualifiedAccess>]
module JobTriggers =

    [<JobTrigger(name = "CurrentTime")>]
    let currentTime : JobHandler =
        fun jobContext -> async {
            let logger = jobContext.CreateLogger("CurrentTime")
            let userName = httpContext.Configuration.GetValue<string>("USER_NAME")

            while not jobContext.CancellationToken.IsCancellationRequested do
                logger.LogInformation(
                    "Hey {UserName}, the current time is {CurrentTime}", 
                        userName, DateTime.Now)

                do! Async.Sleep(1000)
        }
```

## Debug Custom Triggers

See the `.vscode` folder for task/launch configuration.

## Hosting Custom Triggers

Publish the `FSharpFunctions.Host` project and place the publish output somewhere useful

```bash
\fsharp-functions-host.exe

FSharp Functions Host

Usage: fsharp-functions-host --dll <functions DLL path> --urls <ASP NET Core URLS>

--dll      The file path of the DLL that contains FSharp functions. Example: .\MyFunctions.dll
--urls     The ASP NET Core application URLS. Default: http://localhost:5000
```
