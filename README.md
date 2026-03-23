# HomeBook

![homebook](https://raw.githubusercontent.com/homebook-app/homebook/main/icon_128.png)

**HomeBook** is a self-hosted web application for organizing household tasks and joint financial planning. Developed for
families, it creates structure, transparency, and collaboration in everyday life. Easily deployable on your own server
via Docker.

---

[![Docker](https://img.shields.io/badge/Docker-alpha-blue)](https://hub.docker.com/u/homebookapp)
![License](https://img.shields.io/github/license/homebook-app/homebook)

[![Version](https://img.shields.io/docker/v/homebookapp/app)](https://hub.docker.com/u/homebookapp)
[![Docker Pulls](https://img.shields.io/docker/pulls/homebookio/app)](https://hub.docker.com/u/homebookapp)
[![Docker Star](https://img.shields.io/docker/stars/homebookapp/app)](https://hub.docker.com/u/homebookapp)

[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=lk-code_homebook&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=lk-code_homebook)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=lk-code_homebook&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=lk-code_homebook)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=lk-code_homebook&metric=coverage)](https://sonarcloud.io/summary/new_code?id=lk-code_homebook)

[![buy me a coffe](https://cdn.buymeacoffee.com/buttons/v2/default-yellow.png)](https://www.buymeacoffee.com/lk.code)
---

## ✨ Features

- ✅ Collaborative household task management
- 💰 Budget planning and expense tracking
- 👨‍👩‍👧‍👦 Family-friendly user interface
- 🔒 Fully self-hosted – keep your data private
- 🐳 Easy deployment via Docker

---

## Translate

you can help to translate HomeBook into your language.

translate via [Weblate](https://hosted.weblate.org/projects/homebook/)

---

## 🚀 Quick Start

### Requirements

- Docker & Docker Compose
- Optional: your own server or Raspberry Pi

### Installation via Docker & nginx Proxy Manager

#### add to Docker Compose

```yaml
services:
  homebook:
    image: homebookio/app:latest
    container_name: homebook
    restart: unless-stopped
    volumes:
        - homebook-data:/var/lib/homebook

volumes:
    homebook-data:
```

#### add to nginx proxy manager

1. Create a new proxy host in Nginx Proxy Manager
2. Set the domain name (e.g., `my-homebook.mydomain.com`)
3. Set the scheme to `http`
4. Set the forward hostname to `homebook` (must be the same as the container name)
5. Set the forward port to `8080`
6. disable `Cache Assets`
7. enable `Block Common Exploits`
7. enable `Websockets Support`

open your domain and you should see the HomeBook Setup.

### Installation via UmbrelOS

**coming soon...**

### Installation via CasaOS

**coming soon...**

### Supported Databases

- PostgreSQL
- MySQL - (Notice: MariaDB can be used, but is not officially tested)

### Setup

1. **Server Connection Check** - the server checks that all required dependencies are installed and that no setup is
   currently running.
2. **Admin User** - create the first admin user with a username, password, and email address.
3. **Database Setup** - configure the database connection and check the connection.
4. **Configuration** - configure the application settings, such as the homebook instance name and language.

## User Guide

### Environment Variables

if all environment variables are set, correctly, the setup will be running automatically.

| Variable                                 | Description                                                               |
|------------------------------------------|---------------------------------------------------------------------------|
| `DATABASE_PROVIDER`                      | the database provider (one of `POSTGRESQL`,`SQLITE` or `MYSQL`)           |
| `DATABASE_HOST`                          | the database host (e.g., `my-db.server.com`)                              |
| `DATABASE_PORT`                          | the database port (e.g., `5432`)                                          |
| `DATABASE_NAME`                          | the database name (e.g., `homebook`)                                      |
| `DATABASE_USER`                          | the database user (e.g., `db_user`)                                       |
| `DATABASE_PASSWORD`                      | the database password (e.g., `db_password`)                               |
| `DATABASE_FILE`                          | the database file path (e.g. for sqlite, `/var/lib/homebook/homebook.db`) |
| `HOMEBOOK_USER_NAME`                     | the admin username (e.g., `admin`)                                        |
| `HOMEBOOK_USER_PASSWORD`                 | the admin password (e.g., `password`)                                     |
| `HOMEBOOK_CONFIGURATION_ACCEPT_LICENSES` | no value needed. if added, the licenses will be accepted automatically.   |
| `HOMEBOOK_CONFIGURATION_NAME`            | the name of the homebook instance (e.g., `My HomeBook`)                   |
| `HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE`  | the default ui locale (see languages)                                     |

#### Additional Environment Variables

if all environment variables are set, correctly, the setup will be running automatically.

| Variable     | Description                                             |
|--------------|---------------------------------------------------------|
| `HB_DevMode` | with value 'true' the devmode for homebook is activated |

### Allowed Password Characters

```
a-z
A-Z
0-9
!@#$%^&*()_+-=[]{}|;':",./<>?~`
```

### Optional Docker Settings

#### Bind Volume to Host

1. Create a directory on your host machine to store HomeBook data, e.g., `mkdir -p /your/path/homebook`.
2. Execute command to set rights `chown -R 21001 /your/path/homebook && chmod -R 770 /your/path/homebook` (21001 is the
   standard homebook user-id for docker, replace if needed).
3. Modify your Docker Compose file to bind the volume to the host directory:

```yaml
services:
  homebook:
    ...
    volumes:
      - /host/path/homebook:/var/lib/homebook
```

## Languages

### Available Languages

**notice:** use the ENV VALUE in the environment variable `HOMEBOOK_CONFIGURATION_DEFAULT_LOCALE`

| Language | ENV VALUE |
|----------|-----------|
| English  | en-EN     |
| German   | de-DE     |

### Translation Process

we plan to support more languages in the future. For translations, we plan to use a community-driven approach via
Weblate.
