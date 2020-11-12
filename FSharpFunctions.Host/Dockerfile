FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

RUN mkdir /functions

WORKDIR /function-host

COPY ./output/ ./

ENV ASPNETCORE_ENVIRONMENT="Production" \
    APPINSIGHTS_INSTRUMENTATIONKEY="" \
    FUNCTIONS_DLL="" 

ENTRYPOINT ["dotnet", "FSharpFunctions.Host.dll"]

# docker run --rm -d -p 5600:80 -e "FUNCTIONS_DLL=..\functions\Functions.dll" -v C:\Publish:/functions adepena/fsharp-functions-host:dev