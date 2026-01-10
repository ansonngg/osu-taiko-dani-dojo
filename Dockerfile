# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and restore dependencies
COPY OsuTaikoDaniDojo.sln ./
COPY src/Application/*.csproj ./src/Application/
COPY src/Domain/*.csproj ./src/Domain/
COPY src/Infrastructure/*.csproj ./src/Infrastructure/
COPY src/Web/*.csproj ./src/Web/
RUN dotnet restore

# Copy all source files and build
COPY src/. ./src/
WORKDIR /app/src/Web
RUN dotnet publish -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy the published output only
COPY --from=build /app/publish ./

# Set ASP.NET Core to listen on Render's injected PORT, fallback to 5051 for local
ENV ASPNETCORE_HTTP_PORTS=${PORT:-5051}

# Expose default port
EXPOSE 5051

ENTRYPOINT ["dotnet", "Web.dll"]
