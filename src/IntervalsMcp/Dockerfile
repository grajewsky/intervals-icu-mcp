# --- Etap 1: build ---
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Najpierw sam .csproj → restore jest cache'owany, dopóki nie zmienią się paczki
COPY IntervalsMcp.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# --- Etap 2: runtime (mniejszy obraz, bez SDK) ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER app

ENTRYPOINT ["dotnet", "IntervalsMcp.dll"]
