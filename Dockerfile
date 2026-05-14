# Etapa 1: Construcción (Build)
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY ["FuelTrack.Api.csproj", "./"]
RUN dotnet restore "FuelTrack.Api.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet publish "FuelTrack.Api.csproj" -c Release -o /app/publish

# Etapa 2: Ejecución (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Configuración importante para Render (Puerto 8080)
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "FuelTrack.Api.dll"]
