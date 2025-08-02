# This Dockerfile is used for a combined multi-arch build of Blazor WASM and REST API
# Recommended to use Docker Buildx for multi-architecture builds

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

# Restore dependencies
RUN dotnet restore "source/HomeBook.Backend/HomeBook.Backend.csproj"
RUN dotnet restore "source/HomeBook.Frontend/HomeBook.Frontend.csproj"

# Publish Blazor frontend
RUN dotnet publish "source/HomeBook.Frontend/HomeBook.Frontend.csproj" -c $BUILD_CONFIGURATION -o /frontend_dist

# Publish backend
RUN dotnet publish "source/HomeBook.Backend/HomeBook.Backend.csproj" -c $BUILD_CONFIGURATION -o /backend_dist /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
EXPOSE 80 5000
ENV ASPNETCORE_URLS=http://+:5000

RUN apt-get update && \
    apt-get install -y nginx && \
    rm -rf /var/lib/apt/lists/*

RUN rm -rf /usr/share/nginx/html/*
RUN rm /etc/nginx/sites-enabled/default
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /frontend_dist/wwwroot /usr/share/nginx/html

COPY --from=build /backend_dist /app
WORKDIR /app

ARG FRONTEND_APPSETTINGS_FILE=./source/HomeBook.Frontend/wwwroot/appsettings.Docker.json
COPY $FRONTEND_APPSETTINGS_FILE /usr/share/nginx/html/wwwroot/appsettings.json

ARG BACKEND_APPSETTINGS_FILE=./source/HomeBook.Backend/appsettings.Docker.json
COPY $BACKEND_APPSETTINGS_FILE /app/appsettings.json

CMD ["/bin/sh", "-c", "dotnet /app/HomeBook.Backend.dll & nginx -g \"daemon off;\""]
