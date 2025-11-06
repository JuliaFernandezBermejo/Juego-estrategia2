# PROPUESTA DE JUEGO
# JUEGO DE ESTRATEGIA POR TURNOS CON IA JERÁRQUICA

---

**Asignatura:** Inteligencia Artificial
**Curso:** 4º Cuarto
**Desarrollador:** Julia
**Fecha:** Enero 2025

**Motor:** Unity 6000.0.40f1
**Lenguaje:** C#

---

## 1. INFORMACIÓN BÁSICA

**Nombre:** Juego de Estrategia por Turnos con IA Jerárquica

**Género:** Estrategia por turnos

**Plataforma:** PC

**Jugadores:** 1 vs IA

**Duración:** 15-30 minutos

---

## 2. DESCRIPCIÓN

Juego de estrategia por turnos en tablero hexagonal (10x10) donde el jugador enfrenta a una IA con sistema jerárquico de tres niveles. El objetivo es capturar la base enemiga gestionando recursos, produciendo unidades y controlando territorio estratégico.

**Características principales:**
- Sistema de IA de 3 niveles (Operacional, Táctico, Estratégico)
- Mapa hexagonal con 4 tipos de terreno
- 3 tipos de unidades con roles diferenciados
- Sistema de influencia con decaimiento lineal
- Pathfinding Dijkstra adaptado a hexágonos

## 3. MECÁNICAS DE JUEGO

### Sistema de Turnos
- Turnos alternados entre jugador (azul) e IA (rojo)
- Al final del turno: recuperación de movimiento/ataque y +10 recursos
- Terminar turno: ESPACIO

### Movimiento
- Coste según terreno: Llanuras (1.0), Bosque (2.0), Montaña (3.0), Agua (impasable)
- Pathfinding automático con Dijkstra
- Restricciones: no atravesar agua, unidades o base propia

### Combate
- Fórmula: DAÑO = Ataque
- Una vez por turno
- Rangos: Infantería/Caballería (1 hex), Artillería (2 hex)

### Recursos
- +10 por turno (pasivo)
- +10 al ocupar nodos de recursos (una vez)
- Uso: producción de unidades

### Producción
- En la base, requiere recursos suficientes
- La IA selecciona tipo de unidad según situación táctica

### Victoria
- Capturar la base enemiga ocupándola con una unidad

## 4. TIPOS DE UNIDADES

| Unidad      | Coste | HP | Ataque | Alcance | Movimiento | Rol |
|-------------|-------|-----|--------|---------|------------|-----|
| **Infantería** | 20 | 20 | 10 | 1 | 4 | Unidad balanceada, base del ejército |
| **Caballería** | 30 | 30 | 12 | 1 | 6 | Unidad rápida para flanqueos |
| **Artillería** | 40 | 40 | 15 | 2 | 3 | Ataque a distancia, lento |

## 5. SISTEMA DE MAPA

### Cuadrícula Hexagonal
- Sistema odd-r offset (6 direcciones de movimiento)
- Tamaño: 10x10 hexágonos
- Generación aleatoria de terreno

### Tipos de Terreno

| Terreno | Coste Movimiento | Color | Características |
|---------|------------------|-------|-----------------|
| Llanuras | 1.0 | Verde claro | Terreno estándar |
| Bosque | 2.0 | Verde oscuro | Ralentiza movimiento |
| Montaña | 3.0 | Gris | Muy difícil de atravesar |
| Agua | Impasable | Azul | No se puede atravesar |

### Elementos Estratégicos
- **Bases:** Esquinas opuestas (0,0) y (9,9), producción de unidades, objetivo de victoria
- **Nodos de Recursos:** Distribuidos aleatoriamente, otorgan +10 recursos al ocuparlos

## 6. SISTEMA DE INTELIGENCIA ARTIFICIAL

### Arquitectura Jerárquica de 3 Niveles

**Nivel Operacional (Pathfinding):**
- **Técnica:** Dijkstra adaptado para hexágonos
- **Función:** Calcular rutas óptimas considerando costes de terreno
- **Implementación:** `DijkstraPathfinding.cs`

**Nivel Táctico (Decisiones por Unidad):**
- **Técnica:** Behavior Trees
- **Órdenes:** AttackBase, DefendZone, GatherResources, Retreat
- **Autonomía:** Las unidades interpretan órdenes genéricas y deciden según contexto local
- **Implementación:** `UnitAI.cs`

**Nivel Estratégico (Coordinación Global):**
- **Función:** Analizar estado del juego, distribuir roles, asignar órdenes
- **Decisiones:** % unidades a ataque/defensa/recursos según ventaja numérica y recursos
- **Producción:** Selección inteligente de tipo de unidad (Artillería si amenaza, counter según composición enemiga)
- **Implementación:** `StrategicManager.cs`

---

## 7. TÉCNICAS DE IA

### Pathfinding (Dijkstra)
- Garantiza camino de menor coste total
- Considera costes de terreno (Llanuras 1.0, Bosque 2.0, Montaña 3.0)
- Adaptado para coordenadas hexagonales odd-r offset

### Influence Mapping
- **Fórmula:** `influence = attackPower × (HP%) × (1 - cost/maxRange)`
- Decaimiento lineal con coste de movimiento (no distancia geométrica)
- Factor de salud: unidades heridas proyectan menos influencia
- Doble capa: influencia amiga + enemiga para calcular control territorial neto
- Optimizado: 1 expansión Dijkstra por unidad (~14x más rápido)

### Tactical Waypoints
- **Ataque:** Base enemiga (prioridad alta)
- **Defensa:** Base propia (prioridad media-alta)
- **Rally:** Zonas seguras (influencia neta > 0)
- **Recursos:** Nodos no recolectados

### Behavior Trees
- Estructura modular para decisiones de unidades
- Evaluación en orden de prioridad: Atacar si enemigo en rango → Ejecutar orden → Idle
- Permite autonomía: unidades deciden cómo ejecutar órdenes genéricas

## 8. REFERENCIAS

**Literatura:**
- Millington, Ian & Funge, John. "Artificial Intelligence for Games"
  - Cap. 5: Decision Making
  - Sec. 6.1: Waypoint Tactics, 6.2.2: Influence Maps, 6.3: Tactical Pathfinding
- "The Core Mechanics of Influence Mapping" (Paper)

**Recursos:**
- Red Blob Games: Hexagonal Grids y Pathfinding
- Unity Documentation

---

**FIN DEL DOCUMENTO**
