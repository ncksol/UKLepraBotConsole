FROM mcr.microsoft.com/dotnet/core/runtime:3.1
COPY bin/Release/netcoreapp3.1/publish/ app/
RUN apt-get update && apt-get install -y libc6-dev libgdiplus
ENTRYPOINT ["app/UKLepraBotConsole", "--service=1"]