# Use .NET SDK for build step
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Use ASP.NET runtime for execution
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Set environment variable for port
ENV ASPNETCORE_URLS=http://+:5000

COPY --from=build /app/out .
CMD ["dotnet", "WebSocketApi.dll"]