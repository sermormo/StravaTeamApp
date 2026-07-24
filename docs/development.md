# Guía de desarrollo

## Rama de trabajo

El desarrollo activo se realiza en:

```text
development
```

Antes de comenzar:

```powershell
git checkout development
git pull --ff-only origin development
git status --short
```

## Versionamiento

La versión se declara una sola vez en `StravaTeamApp.csproj` mediante la
propiedad `<Version>`. La aplicación la muestra automáticamente en el pie de
página.

Antes de publicar una versión:

1. Actualizar `<Version>`.
2. Mover los cambios desde `Unreleased` a la nueva versión en `CHANGELOG.md`.
3. Actualizar la versión actual en `docs/README.md`.
4. Compilar y completar las pruebas funcionales.
5. Crear el commit y la etiqueta anotada de Git.

La política completa está en [versioning.md](versioning.md).

## Convenciones

- Código en inglés: clases, métodos, variables, propiedades, DTOs y handlers.
- Interfaz en español: títulos, etiquetas, validaciones y mensajes.
- Cambios pequeños o bloques claramente delimitados.
- Compilar y probar antes de cada commit.
- No mezclar cambios no relacionados.
- No publicar secretos, tokens ni bases de datos locales.

## Comandos habituales

Restaurar y compilar:

```powershell
dotnet restore
dotnet build
```

Ejecutar con recarga:

```powershell
dotnet watch
```

Crear una migración:

```powershell
dotnet ef migrations add NombreDeMigracion
```

Aplicar migraciones:

```powershell
dotnet ef database update
```

## Revisión antes del commit

```powershell
git status --short
git diff --stat
git diff --check
dotnet build
```

Después se agregan únicamente los archivos del bloque:

```powershell
git add ruta/al/archivo1 ruta/al/archivo2
git diff --cached --stat
git diff --cached --check
```

## Checklist funcional de insignias

- Crear una insignia con una combinación válida.
- Confirmar el icono automático.
- Editar texto sin perder otorgamientos.
- Editar la regla y comprobar la reevaluación.
- Activar y desactivar.
- Sincronizar actividades al abrir Métricas.
- Confirmar que no aparecen otorgamientos duplicados.
- Expandir atleta y mes.
- Abrir el detalle del icono.
- Confirmar el valor, periodo y actividad decisiva.
- Confirmar el agrupamiento `×N`.
- Confirmar el resaltado de la carrera decisiva.

## Archivos sensibles

No deben versionarse:

- `StravaTeam.db`
- `*.db-shm`
- `*.db-wal`
- Credenciales de Strava.
- Contraseña del administrador inicial.
- Access tokens y refresh tokens.
- Archivos locales de secretos.

## Criterio de finalización

Un bloque está listo para publicarse cuando:

1. Compila correctamente.
2. La prueba funcional acordada fue aprobada.
3. `git diff --check` no informa errores.
4. El estado de Git contiene solamente archivos relacionados.
5. La documentación describe el comportamiento nuevo.
6. Si es una publicación, la versión y `CHANGELOG.md` están actualizados.
