# Build context: Program/src

FROM mcr.microsoft.com/dotnet/sdk:10.0.401 AS build
WORKDIR /src

# Restore first, from project files only, so package layers stay cached
# until a .csproj changes.
COPY Innowise.slnx ./
COPY BuildingBlocks/BuildingBlocks/BuildingBlocks.csproj BuildingBlocks/BuildingBlocks/
COPY BuildingBlocks/BuildingBlocks.AspNetCore/BuildingBlocks.AspNetCore.csproj BuildingBlocks/BuildingBlocks.AspNetCore/
COPY Innowise.Domain/Innowise.Domain.csproj Innowise.Domain/
COPY Innowise.Application/Innowise.Application.csproj Innowise.Application/
COPY Innowise.Infrastructure/Innowise.Infrastructure.csproj Innowise.Infrastructure/
COPY Innowise.Api/Innowise.Api.csproj Innowise.Api/
RUN dotnet restore Innowise.Api/Innowise.Api.csproj

COPY . .
RUN dotnet publish Innowise.Api/Innowise.Api.csproj -c Release -o /app/publish --no-restore \
    -p:DebugType=None -p:DebugSymbols=false

# EF Core migration bundle: a single executable that applies pending migrations.
FROM build AS migrations-build
RUN dotnet tool install --global dotnet-ef --version 10.0.9
ENV PATH="${PATH}:/root/.dotnet/tools"
# Program.cs refuses to start without a connection string, and the bundle
# builder boots it to discover the DbContext. Nothing connects at build time;
# the real string is passed to the bundle at run time.
RUN ConnectionStrings__InnoPaint="Server=placeholder;Database=InnoPaint" \
    dotnet ef migrations bundle \
        --project Innowise.Infrastructure \
        --startup-project Innowise.Api \
        --configuration Release \
        --output /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS migrations
WORKDIR /app
COPY --from=migrations-build /app/efbundle .
USER $APP_UID
ENTRYPOINT ["./efbundle"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0.12 AS api
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Innowise.Api.dll"]
