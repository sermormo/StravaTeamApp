# Seguridad

## Secretos

Nunca deben almacenarse en el repositorio:

- Client secret de Strava.
- Access tokens y refresh tokens.
- Contraseñas.
- Credenciales del administrador inicial.
- Cadenas de conexión con credenciales.
- Bases de datos locales con información de usuarios.

## Desarrollo local

El proyecto ya contiene un `UserSecretsId`. Configura:

```powershell
dotnet user-secrets set "Strava:ClientId" "..."
dotnet user-secrets set "Strava:ClientSecret" "..."
dotnet user-secrets set "InitialAdmin:Email" "..."
dotnet user-secrets set "InitialAdmin:Password" "..."
dotnet user-secrets set "InitialAdmin:UPIN" "..."
```

Para revisar únicamente los nombres y valores configurados en tu equipo:

```powershell
dotnet user-secrets list
```

No copies esa salida en commits, issues, capturas o registros compartidos.

## OAuth y tokens

- Strava OAuth utiliza HTTPS.
- Los tokens se almacenan mediante ASP.NET Core Identity.
- El access token se renueva antes de expirar.
- El refresh token nuevo se guarda antes que los demás valores porque Strava
  puede invalidar inmediatamente el anterior.
- Los tokens no deben escribirse en `SystemLogs`.

## Autorización

- Las páginas de administración requieren el rol `Administrator`.
- El login local administrativo verifica el rol antes de autenticar.
- Un administrador no puede quitarse a sí mismo su propio rol desde el panel.
- Las páginas de Métricas y Perfil requieren autenticación.
- El acceso con Strava requiere pertenecer al club oficial.

## Integridad de las insignias

- La meta debe ser mayor que cero.
- Cantidad de actividades solo admite Conteo.
- El periodo personalizado requiere fechas válidas.
- Un índice único impide duplicar la misma insignia, usuario y periodo.
- Las relaciones con otorgamientos usan eliminación restringida.
- Una insignia inactiva conserva su historial.

## Datos y registros

- `StravaTeam.db` debe permanecer fuera de Git.
- Los logs deben identificar el incidente sin revelar tokens, contraseñas ni
  secretos.
- La pantalla de logs está restringida a administradores.
- La eliminación total de logs es una acción sensible y debe auditarse en una
  versión futura.

## Configuración de producción

Antes de desplegar:

- Usar variables de entorno o un almacén de secretos administrado.
- Usar una base de datos respaldada y con acceso restringido.
- Aplicar HTTPS y HSTS.
- Definir rotación de secretos.
- Configurar backups y restauración.
- Limitar el acceso a registros.
- Revisar permisos y cuentas administrativas.

## Riesgos conocidos

- El proyecto referencia `SQLitePCLRaw.lib.e_sqlite3` versión `2.1.11` y el
  build actual informa la advertencia `NU1903`. Debe evaluarse y actualizarse
  de forma compatible.
- La autorización utiliza nombres de rol directamente; la migración a
  policies está pendiente.
- La sincronización y evaluación se ejecutan durante la solicitud web y aún
  no tienen un job aislado con reintentos.
- No existe todavía una suite automatizada de seguridad o regresión.

## Respuesta ante exposición

Si un secreto o token se publica accidentalmente:

1. Revocarlo o rotarlo inmediatamente.
2. Eliminarlo del código y de la configuración versionada.
3. Revisar el historial y los registros de acceso.
4. Reemplazarlo mediante User Secrets o el almacén de producción.
5. Documentar el incidente sin incluir el valor comprometido.
