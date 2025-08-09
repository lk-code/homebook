# HomeBook

![homebook](https://raw.githubusercontent.com/lk-code/homebook/main/icon_128.png)

**HomeBook** is a self-hosted web application for organizing household tasks and joint financial planning. Developed for
families, it creates structure, transparency, and collaboration in everyday life. Easily deployable on your own server
via Docker.

---

[![Docker](https://img.shields.io/badge/Docker-alpha-blue)](https://hub.docker.com/r/lkcode/homebook)
![License](https://img.shields.io/github/license/lk-code/homebook)

[![Version](https://img.shields.io/docker/v/lkcode/homebook)](https://img.shields.io/docker/v/lkcode/homebook)
[![Docker Pulls](https://img.shields.io/docker/pulls/lkcode/homebook)](https://hub.docker.com/r/lkcode/homebook)
[![Docker Star](https://img.shields.io/docker/stars/lkcode/homebook)](https://hub.docker.com/r/lkcode/homebook)

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

## 🚀 Quick Start

### Requirements

- Docker & Docker Compose
- Optional: your own server or Raspberry Pi

### Installation via Docker & nginx Proxy Manager

#### add to Docker Compose

```yaml
services:
  homebook:
    image: lkcode/homebook:latest
    container_name: homebook
    restart: unless-stopped
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

### Installation via CasaOS

**coming soon...**

### Setup

1. **Server Connection Check** - the server checks that all required dependencies are installed and that no setup is
   currently running.
2. **Admin User** - create the first admin user with a username, password, and email address.
3. **Database Setup** - configure the database connection and check the connection.
4. **Configuration** - configure the application settings, such as the homebook instance name and language.
