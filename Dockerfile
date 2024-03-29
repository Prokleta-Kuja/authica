FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY src/*.csproj ./
RUN dotnet restore

COPY ./src .
WORKDIR /app

ARG Version=0.0.0
RUN dotnet publish /p:Version=$Version -c Release -o out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://*:50505 \
    LOCALE=en-US \
    TZ=America/Chicago \
    PASSWORD=P@ssw0rd

ENTRYPOINT ["dotnet", "authica.dll"]