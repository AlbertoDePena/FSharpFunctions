FROM adepena/functions-fsharp:dev

COPY ./output/ ../functions

ENV FUNCTIONS_DLL="..\functions\FSharpServerless.Demo.dll" 

# docker run --rm -d -p 5600:80 adepena/functions-fsharp:dev