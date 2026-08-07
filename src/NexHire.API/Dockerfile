# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["NexHire.sln", "./"]
COPY ["src/NexHire.API/NexHire.API.csproj", "src/NexHire.API/"]
COPY ["src/NexHire.Application/NexHire.Application.csproj", "src/NexHire.Application/"]
COPY ["src/NexHire.Domain/NexHire.Domain.csproj", "src/NexHire.Domain/"]
COPY ["src/NexHire.Infrastructure/NexHire.Infrastructure.csproj", "src/NexHire.Infrastructure/"]
COPY ["tests/NexHire.UnitTests/NexHire.UnitTests.csproj", "tests/NexHire.UnitTests/"]
COPY ["tests/NexHire.IntegrationTests/NexHire.IntegrationTests.csproj", "tests/NexHire.IntegrationTests/"]

RUN dotnet restore "NexHire.sln"
COPY . .
RUN dotnet publish "src/NexHire.API/NexHire.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NexHire.API.dll"]
