# This Dockerfile is used for a combined multi-arch build of Blazor WASM and REST API
# Recommended to use Docker Buildx for multi-architecture builds

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

# Restore dependencies
RUN dotnet restore "source/WebApp/WebApp.csproj"
RUN dotnet restore "source/AccountService/AccountService.csproj"
RUN dotnet restore "source/FinanceService/FinanceService.csproj"

# Publish Blazor frontend
RUN dotnet publish "source/WebApp/WebApp.csproj" -c $BUILD_CONFIGURATION -o /webapp_dist

# Publish backend
RUN dotnet publish "source/AccountService/AccountService.csproj" -c $BUILD_CONFIGURATION -o /account_service_dist /p:UseAppHost=false
RUN dotnet publish "source/FinanceService/FinanceService.csproj" -c $BUILD_CONFIGURATION -o /finance_service_dist /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
EXPOSE 80 5000
ENV ASPNETCORE_URLS=http://+:5000

RUN apt-get update && \
    apt-get install -y nginx && \
    rm -rf /var/lib/apt/lists/*

RUN rm -rf /usr/share/nginx/html/*
RUN rm /etc/nginx/sites-enabled/default
COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=build /webapp_dist/wwwroot /usr/share/nginx/html

COPY --from=build /account_service_dist /app/account
COPY --from=build /finance_service_dist /app/finance
WORKDIR /app

ARG WEBAPP_APPSETTINGS_FILE=./source/WebApp/wwwroot/appsettings.Docker.json
COPY $WEBAPP_APPSETTINGS_FILE /usr/share/nginx/html/wwwroot/appsettings.json

ARG ACCOUNTSERVICE_APPSETTINGS_FILE=./source/AccountService/appsettings.Docker.json
COPY $ACCOUNTSERVICE_APPSETTINGS_FILE /app/account/appsettings.json

ARG FINANCESERVICE_APPSETTINGS_FILE=./source/FinanceService/appsettings.Docker.json
COPY $FINANCESERVICE_APPSETTINGS_FILE /app/finance/appsettings.json

CMD ["/bin/sh", "-c", "dotnet /app/account/AccountService.dll & dotnet /app/finance/FinanceService.dll & nginx -g \"daemon off;\""]
