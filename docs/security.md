# Seguridad

## Secretos

Nunca almacenar:

-   Client Secret
-   Tokens
-   Contraseñas

## Desarrollo

Usar:

``` powershell
dotnet user-secrets init
dotnet user-secrets set "Strava:ClientId" "..."
dotnet user-secrets set "Strava:ClientSecret" "..."
```

## Producción

-   Azure App Settings
-   Azure Key Vault (futuro)

## Buenas prácticas

-   No subir secretos a Git.
-   Regenerar secretos expuestos.
