# StravaTeamApp

Aplicación web para administrar un equipo de atletismo, sincronizar
actividades de Strava y reconocer el progreso de los corredores mediante
insignias configurables.

La rama principal de trabajo es `development`.

Versión actual: **0.1.0**

## Estado actual

La aplicación incluye:

- Inicio de sesión y vinculación con Strava mediante OAuth.
- Validación de membresía en el club oficial.
- Renovación automática de tokens de Strava.
- Roles `Member` y `Administrator` sobre una misma cuenta.
- Inicio de sesión administrativo con correo y contraseña.
- Administración de usuarios y permisos administrativos.
- Consulta de registros del sistema.
- Sincronización y consolidación mensual de carreras.
- Creación, edición, activación y desactivación de insignias.
- Motor genérico para evaluar reglas semanales, mensuales y personalizadas.
- Métricas de distancia, elevación, duración y cantidad de actividades.
- Registro del valor alcanzado y de la actividad que completó cada meta.
- Visualización de insignias por atleta y mes, con agrupación de logros
  repetidos y detalle de la actividad decisiva.

## Tecnologías

- .NET 10
- ASP.NET Core Razor Pages
- ASP.NET Core Identity
- Entity Framework Core 10
- SQLite
- OAuth con Strava
- Bootstrap

## Requisitos

- .NET 10 SDK.
- Una aplicación registrada en Strava.
- Credenciales de administrador inicial para el entorno local.

## Configuración local

Clona el repositorio y cambia a la rama de desarrollo:

```powershell
git clone https://github.com/sermormo/StravaTeamApp.git
cd StravaTeamApp
git checkout development
```

Configura los secretos locales:

```powershell
dotnet user-secrets set "Strava:ClientId" "TU_CLIENT_ID"
dotnet user-secrets set "Strava:ClientSecret" "TU_CLIENT_SECRET"
dotnet user-secrets set "InitialAdmin:Email" "admin@ejemplo.com"
dotnet user-secrets set "InitialAdmin:Password" "TU_CONTRASENA"
dotnet user-secrets set "InitialAdmin:UPIN" "TU_UPIN"
```

Aplica las migraciones y ejecuta la aplicación:

```powershell
dotnet ef database update
dotnet watch
```

La base de datos local usa por defecto:

```text
Data Source=StravaTeam.db
```

`StravaTeam.db`, secretos, tokens y contraseñas no deben incluirse en Git.

## Flujo principal

1. El corredor inicia sesión con Strava.
2. La aplicación valida que pertenezca al club oficial.
3. Identity crea o vincula la cuenta y guarda los tokens de Strava.
4. Al abrir Métricas, la aplicación renueva el token cuando es necesario.
5. Las carreras recientes se sincronizan y actualizan en SQLite.
6. El motor evalúa todas las insignias activas para el corredor.
7. Los logros nuevos se guardan sin duplicar el mismo periodo.
8. La pantalla de Métricas presenta el acumulado, las insignias y la
   actividad que permitió alcanzar cada meta.

## Convenciones del proyecto

- Clases, métodos, variables, propiedades y handlers se escriben en inglés.
- La interfaz y los mensajes visibles para el usuario se mantienen en
  español.
- Cada bloque se compila y prueba antes de crear el commit.
- Los cambios activos se integran en la rama `development`.
- Los secretos y la base de datos local nunca se publican.

## Documentación

- [Arquitectura](architecture.md)
- [Autenticación](authentication.md)
- [Sistema de insignias](badges.md)
- [Seguridad](security.md)
- [Guía de desarrollo](development.md)
- [Versionamiento](versioning.md)
- [Historial de cambios](../CHANGELOG.md)
- [Historial del proyecto](project-history.md)
- [Roadmap](roadmap.md)
