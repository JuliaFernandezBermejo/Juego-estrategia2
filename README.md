# Juego de Estrategia por Turnos

Proyecto de Inteligencia Artificial - Curso 2024/2025

## Descripción

Juego de estrategia por turnos donde diferentes jugadores (humanos y NPCs) luchan por el dominio de un territorio. El proyecto implementa un sistema de IA jerárquico de tres niveles:

- **Nivel Operacional (Movimiento):** Pathfinding táctico con A*, Dijkstra y BFS
- **Nivel Táctico (Toma de decisiones):** Behavior Trees y FSM jerárquicos
- **Nivel Estratégico (Estrategia global):** Mapas de influencia, waypoints tácticos, fog of war

## Especificaciones Técnicas

- **Motor:** Unity 6000.0.40f1
- **Pipeline de Render:** Universal Render Pipeline (URP) 17.0.4
- **Sistema de Input:** New Input System 1.13.0
- **IA y Navegación:** AI Navigation 2.0.6

## Características del Juego

### Unidades
Cada unidad tiene:
- Coste de producción
- Puntos de Movimiento (PM)
- Puntos de Ataque (PA)
- Puntos de Defensa (PD)
- Alcance de Ataque
- Terreno preferido/penalizado

### Mapa
- Tablero basado en celdas (cuadrícula o hexagonal)
- Diferentes tipos de terreno que afectan movimiento y combate
- Celdas especiales de recursos
- Estructuras de producción

### Recursos
- Tipos: madera, oro, comida, energía, etc.
- Recolección mediante ocupación de celdas especiales o construcción de estructuras
- Necesarios para producir unidades y estructuras

### Objetivos
- Dominio territorial
- Eliminación de jugadores
- Captura de casillas específicas

## Estructura del Proyecto

```
Assets/
├── Scripts/
│   ├── AI/
│   │   ├── Movement/      # Pathfinding táctico
│   │   ├── DecisionMaking/  # Behavior trees, FSM
│   │   └── Strategy/      # Mapas de influencia, waypoints
│   ├── GameLogic/
│   │   ├── Units/         # Clases de unidades
│   │   ├── Map/           # Sistema de grid y terreno
│   │   └── Resources/     # Gestión de recursos
│   └── UI/                # Interfaz de usuario
├── Scenes/                # Escenas de Unity
└── Settings/              # Configuración URP
```

## Desarrollo

### Requisitos
- Unity 6000.0.40f1
- Sistema operativo: Windows 10/11, macOS, o Linux

### Cómo abrir el proyecto
1. Instalar Unity Hub
2. Instalar Unity 6000.0.40f1
3. Abrir este directorio desde Unity Hub

### Referencias
- "Artificial Intelligence for Games" - Ian Millington & John Funge
  - Capítulo 5: Decision Making
  - Sección 6.1: Waypoint Tactics
  - Sección 6.2.2: Simple Influence Maps
  - Sección 6.3: Tactical Pathfinding
- [Introduction to A* Algorithm](https://www.redblobgames.com/pathfinding/a-star/introduction.html)

## Estado Actual

Proyecto inicializado con:
- [x] Configuración base de Unity
- [x] URP configurado
- [x] Paquetes de IA instalados
- [ ] Sistema de grid/mapa
- [ ] Clases de unidades y terreno
- [ ] Pathfinding (A*)
- [ ] Sistema de decisiones (Behavior Trees)
- [ ] IA estratégica (Mapas de influencia)

## Licencia

Proyecto académico - Universidad [Nombre] 2024/2025
