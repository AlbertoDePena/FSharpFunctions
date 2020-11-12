FROM adepena/fsharp-functions-host:dev

COPY ./output/ ../functions

ENV FUNCTIONS_DLL="..\functions\FSharpFunctions.Demo.dll" 

# docker run --rm -d -p 5600:80 adepena/fsharp-functions-host:dev