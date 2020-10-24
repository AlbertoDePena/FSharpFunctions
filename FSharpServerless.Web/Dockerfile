FROM mcr.microsoft.com/dotnet/core/aspnet:3.1

RUN mkdir /serverless

WORKDIR /serverless-web

COPY ./output/ ./

ENV ASPNETCORE_ENVIRONMENT="Production" \
    APPINSIGHTS_INSTRUMENTATIONKEY="" \
    FUNCTIONS_DLL="" 

ENTRYPOINT ["dotnet", "FSharpServerless.Web.dll"]

# docker run --rm -d -p 5600:80 -e "FUNCTIONS_DLL=..\serverless\Functions.dll" -v C:\Publish\Functions.dll:/serverless serverless-web:dev