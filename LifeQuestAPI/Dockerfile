FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Presentation/LifeQuestAPI.API/LifeQuestAPI.API.csproj", "Presentation/LifeQuestAPI.API/"]
COPY ["Core/LifeQuestAPI.Application/LifeQuestAPI.Application.csproj", "Core/LifeQuestAPI.Application/"]
COPY ["Core/LifeQuestAPI.Domain/LifeQuestAPI.Domain.csproj", "Core/LifeQuestAPI.Domain/"]
COPY ["Infrastructure/LifeQuestAPI.Infrastructure/LifeQuestAPI.Infrastructure.csproj", "Infrastructure/LifeQuestAPI.Infrastructure/"]
COPY ["Infrastructure/LifeQuestAPI.Persistence/LifeQuestAPI.Persistence.csproj", "Infrastructure/LifeQuestAPI.Persistence/"]

# Restore dependencies
RUN dotnet restore "Presentation/LifeQuestAPI.API/LifeQuestAPI.API.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/Presentation/LifeQuestAPI.API"
RUN dotnet build "LifeQuestAPI.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LifeQuestAPI.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
# Generate certificate here while we have the SDK
RUN dotnet dev-certs https -ep /app/publish/cert.pfx -p password123

# Build the final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 5020
EXPOSE 7071

# Set URLs and Certificate Config
ENV ASPNETCORE_URLS="https://+:7071;http://+:5020"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="password123"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/cert.pfx"

# Add timezone data
ENV TZ=Europe/Istanbul
RUN apt-get update && apt-get install -y tzdata && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "LifeQuestAPI.API.dll"]
