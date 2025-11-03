# Memoria - Decisiones de Implementación del Pathfinding

## Cambio de A* a Dijkstra

Inicialmente se implementó el algoritmo A* para el pathfinding, pero se decidió cambiar a Dijkstra por las siguientes razones:

1. **Simplicidad conceptual**: Dijkstra no requiere diseñar ni mantener una heurística, eliminando la posibilidad de errores por heurísticas inadmisibles.

2. **Garantía de optimalidad**: Aunque A* también garantiza el camino óptimo con una heurística admisible, Dijkstra elimina cualquier duda al no depender de estimaciones.

3. **Requisitos académicos**: Para el proyecto de IA, se consideró más apropiado usar Dijkstra puro para demostrar comprensión del algoritmo fundamental.

## Problemas Encontrados y Solucionados

### 1. Descorrelación entre terreno visual y datos

**Problema**: Los bordes de los hexágonos se creaban antes de randomizar el terreno, causando que el color visual no coincidiera con el tipo de terreno almacenado.

**Solución**: Se modificó `HexCell.SetTerrain()` para actualizar también el color del borde hijo cuando cambia el terreno, asegurando sincronización visual-datos.

### 2. Cálculo incorrecto de vecinos en filas impares

**Problema**: El sistema de coordenadas odd-r offset para hexágonos requiere diferentes offsets para filas pares e impares, pero solo se usaba un conjunto de direcciones.

**Solución**: Se separaron las direcciones de vecinos en `directionsEven` y `directionsOdd` en `HexCoordinates.cs`, seleccionando el conjunto correcto según `coord.r % 2`.

### 3. Cálculos redundantes de pathfinding

**Problema**: Para cada movimiento se calculaba el camino 3 veces:
- Una vez en `GameManager.CanMoveTo()` (validación)
- Una vez dentro de `MoveTo()` llamando a `CanMoveTo()` (validación redundante)
- Una vez más en `MoveTo()` para ejecutar el movimiento

**Solución**: Se eliminó la validación redundante. Ahora `GameManager` llama directamente a `MoveTo()`, que internamente hace todas las validaciones y calcula el camino una sola vez.

### 4. Bases amigas no eran impasables

**Problema**: Las celdas base no tenían unidades sobre ellas, por lo que `IsPassable()` las consideraba transitables. Esto permitía caminar sobre la base aliada e impedía capturar la base enemiga.

**Solución**: Se creó `IsPassableForPlayer(playerID)` que:
- Bloquea bases amigas (mismo `OwnerPlayerID`)
- Permite bases enemigas (para poder capturarlas y ganar)
- Mantiene el bloqueo de agua y celdas ocupadas

## Resultado

El sistema de pathfinding ahora funciona correctamente con:
- Detección precisa de todos los vecinos hexagonales
- Coincidencia perfecta entre visualización y datos de terreno
- Un único cálculo de camino por movimiento (mejora de rendimiento del 67%)
- Lógica correcta de pasabilidad considerando propiedad de bases
