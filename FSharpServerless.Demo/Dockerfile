FROM serverless-web:dev

COPY ./output/ ../serverless

ENV FUNCTIONS_DLL="..\serverless\FSharpServerless.Demo.dll" 

# docker run --rm -d -p 5600:80 serverless-demo:dev