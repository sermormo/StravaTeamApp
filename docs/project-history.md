# Historial del proyecto

## Julio de 2026

### Versión 0.1.0

- Se estableció Semantic Versioning como política oficial.
- La versión se centralizó en `StravaTeamApp.csproj`.
- La aplicación muestra su versión en el pie de página.
- Se agregó `CHANGELOG.md`.
- Se definió el uso de etiquetas anotadas de Git para identificar releases.

### Base de la aplicación

- Se consolidó el proyecto sobre ASP.NET Core Razor Pages, Identity, Entity
  Framework Core y SQLite.
- Se retiró la base de datos local del control de versiones.
- Se agregaron perfiles de usuario con nombre, apellido, género y UPIN.
- Se incorporaron métricas mensuales y registros del sistema.

### Seguridad y autenticación

- Las credenciales de Strava se movieron a User Secrets.
- Se agregaron los roles `Member` y `Administrator`.
- Se implementó un administrador inicial compatible con bases existentes.
- Se agregó inicio de sesión administrativo con correo y contraseña.
- Se permitió vincular Strava a una cuenta administrativa existente.
- Se validó la membresía en el club oficial antes de permitir el acceso.
- Se unificó la integración en `StravaService`.
- Se implementó la renovación automática de tokens de Strava.

### Administración

- Se creó el panel administrativo.
- Se agregó gestión de permisos administrativos.
- Se agregó consulta de los 100 registros más recientes.
- Se incorporó el menú de Insignias.

### Modelo configurable de insignias

- Se agregaron `Badge`, `BadgeRule` y `UserBadge`.
- Se agregaron métricas de distancia, elevación, duración y cantidad.
- Se agregaron operaciones de suma, conteo y máximo.
- Se agregaron periodos semanales, mensuales y personalizados.
- Se añadió `TotalElevationGain` a las actividades.
- Se creó la migración del sistema de insignias.
- Se agregaron iconos SVG por métrica.

### Administración de insignias

- Se implementó la creación y el listado.
- Se implementó la edición de la insignia y de su regla.
- Se implementó la activación y desactivación.
- Al cambiar el criterio se eliminan los otorgamientos anteriores para
  reevaluarlos con la nueva regla.
- El commit `10eb1fe` publicó la edición y gestión de estado en
  `development`.

### Motor y visualización

- Se creó `BadgeEvaluationService`.
- El motor evalúa reglas genéricas sin código específico por insignia.
- Se agregó evaluación semanal, mensual y personalizada.
- Se registran el valor alcanzado y la actividad decisiva.
- Se evita duplicar un logro en el mismo periodo.
- La evaluación se ejecuta después de sincronizar las carreras.
- Métricas muestra tipos de insignia, logros totales e iconos por mes.
- Los logros repetidos se agrupan con `×N`.
- El detalle muestra cada fecha, periodo, valor y actividad decisiva.
- La carrera decisiva se resalta visualmente en la tabla inferior.

## Decisiones vigentes

- Código interno en inglés e interfaz en español.
- La rama de integración es `development`.
- Se compila y prueba antes de cada commit.
- Los otorgamientos históricos se conservan al desactivar una insignia.
- Cambiar una regla obliga a reevaluar sus otorgamientos.

## Lección operativa

Antes de investigar una causa compleja, comprobar nombres, asignaciones,
configuración, estado de Git y procesos que puedan mantener bloqueado el
ejecutable.
