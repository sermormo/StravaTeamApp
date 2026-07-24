# Roadmap

## Completado

### Identidad y acceso

- [x] User Secrets.
- [x] Login con Strava.
- [x] Validación de membresía del club.
- [x] Roles `Member` y `Administrator`.
- [x] Administrador inicial.
- [x] Login administrativo.
- [x] Vinculación de Strava con una cuenta existente.
- [x] Renovación automática de tokens.

### Administración

- [x] Panel administrativo.
- [x] Gestión básica de administradores.
- [x] Consulta de registros.
- [x] Menú y páginas administrativas de insignias.

### Actividades y métricas

- [x] Sincronización de carreras.
- [x] Actualización de actividades existentes.
- [x] Distancia, duración y elevación.
- [x] Consolidado mensual por atleta.
- [x] Detalle de actividades por atleta y mes.

### Insignias

- [x] Modelo `Badge`, `BadgeRule` y `UserBadge`.
- [x] Métricas configurables.
- [x] Operaciones configurables.
- [x] Periodos semanales, mensuales y personalizados.
- [x] Creación, edición, activación y desactivación.
- [x] Iconos automáticos por métrica.
- [x] Motor genérico de evaluación.
- [x] Prevención de otorgamientos duplicados.
- [x] Registro del valor y actividad decisiva.
- [x] Reevaluación después de cambiar una regla.
- [x] Visualización por atleta y mes.
- [x] Agrupación de logros repetidos.
- [x] Detalle de cada logro.
- [x] Resaltado de la actividad decisiva.

## Próxima prioridad

- [ ] Agregar pruebas unitarias para `BadgeEvaluationService`.
- [ ] Agregar pruebas de integración para creación, edición y estado.
- [ ] Probar límites de semana, mes y periodo personalizado.
- [ ] Implementar eliminación solo para insignias que nunca fueron otorgadas.
- [ ] Registrar auditoría de cambios administrativos.
- [ ] Reemplazar autorización directa por roles con policies.

## Sincronización

- [ ] Implementar paginación e importación incremental más allá de 50
  actividades.
- [ ] Guardar el punto de última sincronización.
- [ ] Ejecutar sincronización y evaluación en segundo plano.
- [ ] Mejorar reintentos y manejo de límites de Strava.

## Seguridad y calidad

- [ ] Resolver la advertencia de seguridad de
  `SQLitePCLRaw.lib.e_sqlite3`.
- [ ] Agregar protección explícita y auditoría para borrar registros.
- [ ] Evitar datos personales sensibles en mensajes de log.
- [ ] Agregar análisis estático y validación automatizada en CI.
- [ ] Agregar health checks.

## Producción

- [ ] Definir la base de datos de producción.
- [ ] Configurar secretos administrados.
- [ ] Agregar Docker.
- [ ] Configurar backups.
- [ ] Configurar observabilidad y alertas.
- [ ] Preparar despliegue automatizado.
