FROM mcr.microsoft.com/dotnet/runtime:6.0
COPY bin/Release/net6.0/linux-x64/publish/ app/
RUN apt-get update && apt-get install -y libc6-dev libgdiplus
ENTRYPOINT ["app/UKLepraBotConsole", "--service=1"]