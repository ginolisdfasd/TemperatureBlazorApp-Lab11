FROM bitnami/dotnet-sdk:8 AS build
WORKDIR /src

COPY ["TemperatureBlazorApp.csproj", "./"]
RUN dotnet restore "TemperatureBlazorApp.csproj"

COPY . .
RUN dotnet publish "TemperatureBlazorApp.csproj" -c Release -o /app/publish

FROM bitnami/dotnet-sdk:8 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TemperatureBlazorApp.dll"]
