# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

FROM mcr.microsoft.com/dotnet/framework/runtime:4.8-windowsservercore-ltsc2019
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["C:\\app\\Teams-Context-Assistant.exe"]
