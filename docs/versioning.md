# Versionamiento

## Versión actual

La primera versión formal de StravaTeamApp es:

```text
0.1.0
```

Mientras la aplicación continúe en desarrollo activo y todavía no se considere
estable para producción, permanecerá dentro de la serie `0.x`.

## Formato

El proyecto usa Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

- `PATCH`: corrección compatible, por ejemplo `0.1.0` → `0.1.1`.
- `MINOR`: funcionalidad nueva compatible, por ejemplo `0.1.1` → `0.2.0`.
- `MAJOR`: versión estable o cambio incompatible importante. La primera
  versión estable será `1.0.0`.

Ejemplos:

| Cambio | Versión resultante |
| --- | --- |
| Corregir un cálculo incorrecto | `0.1.1` |
| Agregar notificaciones | `0.2.0` |
| Agregar reportes sin romper funciones existentes | `0.3.0` |
| Declarar la aplicación estable para producción | `1.0.0` |
| Introducir un cambio incompatible después de 1.0 | `2.0.0` |

## Fuente de la versión

La versión se declara en `StravaTeamApp.csproj`:

```xml
<Version>0.1.0</Version>
```

El SDK de .NET usa ese valor para generar la versión del ensamblado. El layout
lee la versión informativa del ensamblado y la muestra en el pie de página.
No se debe duplicar la versión en `appsettings.json` ni en código C#.

## Historial de cambios

`CHANGELOG.md` es el registro de cambios orientado a releases.

- Los cambios en desarrollo se agregan bajo `Unreleased`.
- Al publicar, esos cambios pasan a una sección con versión y fecha.
- Se documentan funcionalidades, cambios, correcciones y eliminaciones
  relevantes para el comportamiento de la aplicación.

`docs/project-history.md` conserva decisiones y evolución técnica más amplia;
no reemplaza al changelog.

## Publicación de una versión

1. Confirmar que `development` contiene únicamente el alcance aprobado.
2. Elegir la versión según el tipo de cambio.
3. Actualizar `<Version>` en `StravaTeamApp.csproj`.
4. Actualizar `docs/README.md` y `CHANGELOG.md`.
5. Ejecutar:

```powershell
git diff --check
dotnet build
git status --short
```

6. Probar funcionalmente la aplicación.
7. Crear el commit de publicación.
8. Crear una etiqueta anotada sobre ese mismo commit:

```powershell
git tag -a v0.1.0 -m "StravaTeamApp 0.1.0"
git push origin development
git push origin v0.1.0
```

La etiqueta no debe crearse antes del build ni sobre un commit distinto al que
contiene la versión documentada.

## Flujo de ramas

- `development`: rama de integración y pruebas.
- Las etiquetas `vX.Y.Z` identifican versiones exactas e inmutables.
- Cuando exista un despliegue estable, se podrá incorporar `main` como rama de
  producción sin cambiar la numeración ya establecida.
