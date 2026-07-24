# Sistema de insignias

## Objetivo

El sistema permite crear retos desde la interfaz administrativa sin agregar
código específico para cada insignia. Una regla describe qué medir, cómo
calcularlo, cuál es la meta y en qué periodo se evalúa.

## Modelo de datos

### `Badge`

Contiene la información visible:

- Nombre.
- Descripción.
- Ruta del icono.
- Estado activo o inactivo.
- Fechas de creación y actualización.

### `BadgeRule`

Define el criterio:

- Métrica.
- Operación.
- Periodo.
- Valor meta.
- Tipo de actividad.
- Fechas de inicio y fin cuando el periodo es personalizado.

Cada insignia tiene exactamente una regla.

### `UserBadge`

Representa un logro real:

- Usuario.
- Insignia.
- Inicio y fin del periodo evaluado.
- Fecha en que el sistema registró el logro.
- Valor alcanzado.
- Actividad que permitió alcanzar la meta.

## Métricas y unidades

| Métrica | Valor interno de la actividad | Unidad de configuración |
| --- | --- | --- |
| Distancia | Metros de Strava convertidos a kilómetros | km |
| Elevación | Ganancia total de elevación | m |
| Duración | Segundos de movimiento convertidos a minutos | min |
| Cantidad de actividades | Una unidad por carrera | actividades |

## Operaciones

| Operación | Comportamiento |
| --- | --- |
| Suma | Acumula cronológicamente hasta alcanzar la meta. |
| Conteo | Cuenta carreras cronológicamente hasta alcanzar la cantidad. |
| Máximo | Otorga el logro con la primera carrera cuyo valor individual alcanza la meta. |

Combinaciones admitidas:

| Métrica | Suma | Conteo | Máximo |
| --- | :---: | :---: | :---: |
| Distancia | Sí | No | Sí |
| Elevación | Sí | No | Sí |
| Duración | Sí | No | Sí |
| Cantidad de actividades | No | Sí | No |

La meta para cantidad de actividades debe ser un número entero.

## Periodos

### Semanal

- Comienza el lunes a las 00:00 UTC.
- Termina el lunes siguiente.
- La misma insignia puede obtenerse una vez por semana.

### Mensual

- Comienza el primer día del mes.
- Termina el primer día del mes siguiente.
- La misma insignia puede obtenerse una vez por mes.

### Personalizado

- El administrador indica fecha inicial y fecha final.
- La fecha final visible se considera inclusiva.
- Internamente se almacena el límite final exclusivo, sumando un día.
- La insignia se puede obtener una vez para ese periodo.

## Actividad decisiva

Las actividades se evalúan por `StartDate` y luego por identificador.

- En una suma, es la carrera que hace que el acumulado alcance o supere la
  meta.
- En un conteo, es la carrera número N requerida por la meta.
- En un máximo, es la primera carrera individual que alcanza o supera la
  meta.

`TriggerActivityId` conserva la relación exacta; la interfaz no depende de
comparar nombres o fechas aproximadas.

## Prevención de duplicados

La combinación siguiente tiene un índice único:

```text
UserId + BadgeId + PeriodStartUtc + PeriodEndUtc
```

El servicio también carga los periodos ya otorgados antes de evaluar. Esto
permite abrir Métricas repetidamente sin crear duplicados.

## Momento de evaluación

La evaluación ocurre al abrir `Running/Métricas`:

1. Se obtiene o renueva el token de Strava.
2. Se consultan y guardan las carreras recientes.
3. Se evalúan todas las insignias activas para el usuario actual.
4. Se cargan las métricas y los otorgamientos del equipo.

No existe todavía un job en segundo plano; un corredor debe entrar a la
plataforma para sincronizar y evaluar sus actividades.

## Administración

Desde **Administración → Insignias** se puede:

- Crear una insignia.
- Consultar su regla y estado.
- Editar nombre, descripción, métrica, operación, periodo y meta.
- Activarla o desactivarla.

El icono se asigna automáticamente según la métrica:

- `distance.svg`
- `elevation.svg`
- `duration.svg`
- `activity-count.svg`

### Cambio de regla

Cuando cambia cualquier parte del criterio:

- Se eliminan los otorgamientos anteriores de esa insignia.
- La insignia conserva su configuración actualizada.
- Los logros se vuelven a calcular con el criterio nuevo cuando cada atleta
  abre Métricas.

Cambiar solamente el nombre o la descripción no elimina otorgamientos.

### Desactivación

Una insignia inactiva:

- No genera logros nuevos.
- Conserva los logros existentes.
- Se presenta como histórica en la interfaz.

La eliminación administrativa definitiva todavía no está implementada.

## Visualización

La pantalla de Métricas presenta:

- Cantidad de tipos de insignia y cantidad total de logros por atleta.
- Iconos agrupados por tipo dentro de cada mes.
- Indicador `×N` cuando la misma insignia se obtuvo varias veces en el mes.
- Modal con cada fecha, periodo, valor y actividad decisiva.
- Panel de insignias del mes seleccionado.
- Fila resaltada e icono en la actividad que produjo el logro.
- Estado activa o histórica.

Un logro semanal puede aparecer varias veces en el mismo mes. Se muestra un
solo icono con su cantidad, pero cada otorgamiento conserva su propio detalle.

## Ejemplos

### 100 km mensuales

| Campo | Valor |
| --- | --- |
| Métrica | Distancia |
| Operación | Suma |
| Meta | 100 |
| Periodo | Mensual |

### 3 horas semanales

| Campo | Valor |
| --- | --- |
| Métrica | Duración |
| Operación | Suma |
| Meta | 180 |
| Periodo | Semanal |

La duración se configura en minutos: `3 × 60 = 180`.

### 10 carreras mensuales

| Campo | Valor |
| --- | --- |
| Métrica | Cantidad de actividades |
| Operación | Conteo |
| Meta | 10 |
| Periodo | Mensual |

### Media maratón en una actividad

| Campo | Valor |
| --- | --- |
| Métrica | Distancia |
| Operación | Máximo |
| Meta | 21.1 |
| Periodo | Mensual |

## Casos de prueba recomendados

- Alcanzar exactamente la meta.
- Superar la meta con la última carrera del periodo.
- No alcanzar la meta.
- Obtener la misma insignia en dos semanas diferentes.
- Abrir Métricas varias veces sin crear duplicados.
- Cambiar una regla con otorgamientos existentes.
- Desactivar y reactivar una insignia.
- Probar los límites entre domingo y lunes.
- Probar los límites entre el último y el primer día del mes.
- Probar un periodo personalizado de un solo día.
