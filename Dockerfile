$reg = "mcr." + "microsoft." + "com/dotnet"
@"
FROM $reg/sdk:8.0 AS build
WORKDIR /src
COPY ["TemperatureBlazorApp.csproj", "./"]
RUN dotnet restore "TemperatureBlazorApp.csproj"
COPY . .
RUN dotnet publish "TemperatureBlazorApp.csproj" -c Release -o /app/publish

FROM $reg/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TemperatureBlazorApp.dll"]
"@ | Set-Content -Path Dockerfile -Encoding utf8
