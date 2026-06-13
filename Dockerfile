# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy solution and restore dependencies
COPY OsuDojo.slnx ./
COPY OsuDojo.Application/*.csproj ./OsuDojo.Application/
COPY OsuDojo.Domain/*.csproj ./OsuDojo.Domain/
COPY OsuDojo.Infrastructure/*.csproj ./OsuDojo.Infrastructure/
COPY OsuDojo.Web/*.csproj ./OsuDojo.Web/
RUN dotnet restore

# Copy all source files and build
COPY OsuDojo.Application/. ./OsuDojo.Application/
COPY OsuDojo.Domain/. ./OsuDojo.Domain/
COPY OsuDojo.Infrastructure/. ./OsuDojo.Infrastructure/
COPY OsuDojo.Web/. ./OsuDojo.Web/
WORKDIR /app/OsuDojo.Web
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

ENTRYPOINT ["dotnet", "OsuDojo.Web.dll"]
