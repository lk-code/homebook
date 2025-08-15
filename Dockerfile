# This Dockerfile is used for a combined multi-arch build of Blazor WASM and REST API
# Recommended to use Docker Buildx for multi-architecture builds

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /homebook-src

COPY . .

# Restore dependencies
#RUN dotnet tool install --global Microsoft.OpenApi.Kiota
#ENV PATH="$PATH:/root/.dotnet/tools"
RUN dotnet restore "source/HomeBook.Backend/HomeBook.Backend.csproj"
RUN dotnet restore "source/HomeBook.Frontend/HomeBook.Frontend.csproj"

# Publish Blazor frontend
RUN dotnet publish "source/HomeBook.Frontend/HomeBook.Frontend.csproj" -c $BUILD_CONFIGURATION -o /frontend_dist

# Publish backend
RUN dotnet publish "source/HomeBook.Backend/HomeBook.Backend.csproj" -c $BUILD_CONFIGURATION -o /backend_dist /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
ARG APP_UID=21001
ARG APP_GID=21001
EXPOSE 8080 5000
ENV ASPNETCORE_URLS=http://+:5000

RUN apt-get update && \
    apt-get install -y nginx && \
    rm -rf /var/lib/apt/lists/*

RUN addgroup --gid $APP_GID appgroup \
    && adduser --uid $APP_UID --gid $APP_GID --disabled-password --gecos "" appuser

RUN mkdir -p /var/log/homebook \
    && chown -R appuser:appgroup /var/log/homebook

RUN mkdir -p /var/lib/homebook \
    && chown -R appuser:appgroup /var/lib/homebook

RUN rm -rf /usr/share/nginx/html/*
RUN rm /etc/nginx/sites-enabled/default
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Make nginx non-root friendly: ensure writable dirs, disable 'user' directive, move pid and temp paths
RUN mkdir -p /var/cache/nginx /var/lib/nginx /var/log/nginx /var/run /usr/share/nginx/html \
    && chown -R $APP_UID:$APP_GID /var/cache/nginx /var/lib/nginx /var/log/nginx /var/run /usr/share/nginx/html \
    && sed -ri 's|^\s*user\s+.+;|# user disabled (running as non-root);|g' /etc/nginx/nginx.conf \
    && sed -ri 's|^\s*pid\s+.+;|pid /tmp/nginx.pid;|g' /etc/nginx/nginx.conf || true \
    && printf "client_body_temp_path /tmp/client_temp;\nproxy_temp_path /tmp/proxy_temp;\nfastcgi_temp_path /tmp/fastcgi_temp;\nuwsgi_temp_path /tmp/uwsgi_temp;\nscgi_temp_path /tmp/scgi_temp;\n" > /etc/nginx/conf.d/zz-temp-paths.conf \
    && sed -ri 's|listen\s+80;|listen 8080;|g' /etc/nginx/conf.d/default.conf || true

COPY --from=build /frontend_dist/wwwroot /usr/share/nginx/html

COPY --from=build /backend_dist /opt/homebook
WORKDIR /opt/homebook

RUN chown -R appuser:appgroup /opt/homebook

ARG FRONTEND_APPSETTINGS_FILE="./source/HomeBook.Frontend/wwwroot/appsettings.json"
COPY $FRONTEND_APPSETTINGS_FILE /usr/share/nginx/html/wwwroot/appsettings.json

ARG BACKEND_APPSETTINGS_FILE="./source/HomeBook.Backend/appsettings.json"
COPY $BACKEND_APPSETTINGS_FILE /opt/homebook/appsettings.json

USER appuser
CMD ["/bin/sh", "-c", "dotnet /opt/homebook/HomeBook.Backend.dll & nginx -g 'daemon off;'"]
