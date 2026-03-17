FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

ARG APP_UID=21001

EXPOSE 8080 5000

ENV ASPNETCORE_URLS=http://+:5000

RUN apt-get update && \
    apt-get --no-install-recommends install -y nginx adduser && \
    rm -rf /var/lib/apt/lists/*

RUN addgroup --gid $APP_UID appgroup \
    && adduser --uid $APP_UID --gid $APP_UID --disabled-password --gecos "" appuser

RUN mkdir -p /var/lib/homebook \
    && chown -R $APP_UID /var/lib/homebook \
    && chmod -R 770 /var/lib/homebook

RUN rm -rf /usr/share/nginx/html/* \
    && rm /etc/nginx/sites-enabled/default

COPY nginx.conf /etc/nginx/conf.d/default.conf

RUN mkdir -p /var/cache/nginx /var/lib/nginx /var/log/nginx /var/run /usr/share/nginx/html \
    && chown -R $APP_UID /var/cache/nginx /var/lib/nginx /var/log/nginx /var/run /usr/share/nginx/html \
    && sed -ri 's|^\s*user\s+.+;|# user disabled (running as non-root);|g' /etc/nginx/nginx.conf \
    && sed -ri 's|^\s*pid\s+.+;|pid /tmp/nginx.pid;|g' /etc/nginx/nginx.conf || true \
    && printf "client_body_temp_path /tmp/client_temp;\nproxy_temp_path /tmp/proxy_temp;\nfastcgi_temp_path /tmp/fastcgi_temp;\nuwsgi_temp_path /tmp/uwsgi_temp;\nscgi_temp_path /tmp/scgi_temp;\n" > /etc/nginx/conf.d/zz-temp-paths.conf \
    && sed -ri 's|listen\s+80;|listen 8080;|g' /etc/nginx/conf.d/default.conf || true

COPY ./publish/frontend/wwwroot /usr/share/nginx/html
COPY ./publish/backend /opt/homebook

WORKDIR /opt/homebook

RUN chown -R $APP_UID /opt/homebook

COPY docker-entrypoint.sh /usr/local/bin/

RUN chmod +x /usr/local/bin/docker-entrypoint.sh \
    && chown $APP_UID /usr/local/bin/docker-entrypoint.sh

USER $APP_UID

ENTRYPOINT ["/usr/local/bin/docker-entrypoint.sh"]
