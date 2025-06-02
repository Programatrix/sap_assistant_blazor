# Etapa de build con .NET 7
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY . .
RUN dotnet publish SAPAssistant/SAPAssistant.csproj -c Release -o /app/publish

# Etapa de runtime con .NET 7
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SAPAssistant.dll"]
