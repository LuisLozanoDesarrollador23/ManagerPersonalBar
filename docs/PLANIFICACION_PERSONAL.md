# Reglas de planificación del personal

## Pertenencia a bar

Cada integrante tiene un **bar principal**. Por defecto sólo puede asignarse a grupos de entrada de ese bar. La marca `Puede trabajar en ambos bares` es una excepción explícita y permite asignarlo temporalmente al otro.

## Barra y cocina

El campo `Área` distingue `Barra` de `Cocina`. Las reglas de nivel, acompañamiento y parejas incompatibles se aplican sólo a barra. Cocina tiene un grupo de entrada recurrente a las 08:00 para cada bar, todos los días; la cantidad se puede configurar como cualquier otro requisito. No se modela hora de salida.

## Horas semanales

`Objetivo de horas semanales` es el número que se desea entregar a cada persona durante la semana. Se almacena para que el planificador pueda repartir el trabajo de forma justa.

Con el requisito actual de no registrar la hora de salida, la aplicación **no puede calcular horas reales asignadas**. Para comparar automáticamente objetivo contra horas planificadas habría que añadir una duración estimada o una hora de salida al grupo/turno.

## Disponibilidad y vacaciones

- Los días no deseados son preferencias recurrentes de lunes a domingo. Una asignación en ese día se rechaza.
- Las vacaciones sí necesitan fechas. Al crear una asignación se indica el lunes de la semana planificada; el sistema obtiene la fecha del día y la rechaza si cae dentro de un periodo de vacaciones.

La fecha de semana se usa sólo para validar vacaciones y separar planificaciones futuras; el horario de entrada continúa siendo una plantilla semanal.
