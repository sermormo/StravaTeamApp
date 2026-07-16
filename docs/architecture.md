# Arquitectura

## Tecnologías

-   ASP.NET Core Razor Pages
-   ASP.NET Identity
-   Entity Framework Core
-   SQLite (desarrollo)

## Capas

    Razor Pages
        ↓
    Services
        ↓
    Entity Framework
        ↓
    SQLite

## Componentes principales

-   Areas/Identity: autenticación.
-   Pages: interfaz.
-   Services: integración con Strava.
-   Data: DbContext.
-   Models: entidades.

## Evolución

-   Azure SQL
-   Docker
-   Panel administrativo
-   Sincronizaciones automáticas
