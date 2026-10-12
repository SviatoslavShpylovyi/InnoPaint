# Build context: Program/frontend

FROM node:22.23.3-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:1.27.5-alpine AS web
# Where nginx proxies /api and /health; override per platform.
ENV API_UPSTREAM=http://api:8080
COPY nginx.conf.template /etc/nginx/templates/default.conf.template
# Sourced before the template step: sets BASIC_AUTH_REALM and writes
# .htpasswd. The entrypoint skips .envsh files that are not executable.
COPY --chmod=755 05-basic-auth.envsh /docker-entrypoint.d/05-basic-auth.envsh
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
