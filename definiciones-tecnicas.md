# Proyecto [sin título] — Definiciones Técnicas y Objetivo

> Estado: borrador de trabajo. Documento de decisiones tomadas, no de implementación.

---

## 1. Objetivo del proyecto

Desarrollar un **RPG táctico por turnos en grilla**, campaña corta, estética anime, con temática de robots de combate pilotados.

**Criterio de éxito primario:** terminar un juego. El alcance está deliberadamente acotado para que sea completable por un desarrollador solo o un equipo muy chico.

**Criterio de éxito secundario:** que el vertical slice sea suficientemente bueno como para justificar seguir. Si el slice no es divertido, se replantea antes de escalar.

---

## 2. Alcance objetivo

| Elemento | Cantidad objetivo |
|---|---|
| Misiones | 8–10 |
| Chasis jugables | 4–5 |
| Armas y módulos | ~20 |
| Tipos de enemigo | 6, con 3 variantes de nivel cada uno |
| Jefes | Los Siete (funcionan como jefes de etapa) |

**Fuera de alcance (explícito):**
- Mundo abierto o overworld explorable.
- Historia contada entre misiones mediante pantallas de diálogo (retrato + texto). Esto ahorra aproximadamente el 60% del arte.
- Sin mapa continuo, sin NPCs deambulando, sin pueblos.

### Vertical slice inicial

Antes de escalar al alcance completo: **una misión jugable de punta a punta**, con 2 chasis, 5 armas, 2 tipos de enemigo y una pantalla de configuración funcional. Si eso funciona, se escala.

---

## 3. Plataforma

**PC como target primario.**

Razones:
- El género vive en PC. Grillas, comparación de módulos, rangos superpuestos y planificación de posicionamiento necesitan espacio de pantalla y precisión de mouse.
- Descubribilidad: en mobile, un indie sin presupuesto de marketing es invisible — el mercado se define por adquisición paga. Steam todavía permite descubrimiento orgánico para nicho táctico/mecha.
- Monetización: mobile empuja a F2P con IAP, lo que obliga a diseñar la progresión alrededor de la fricción (gacha, energía, timers). Eso pelea contra el diseño que se quiere.
- Costo: mobile suma UI táctil, múltiples resoluciones y certificaciones de store.

**Arquitectura tolerante a port futuro** (no se cierra la puerta, pero no se paga el costo ahora):
- Áreas de toque generosas en UI.
- Nunca usar hover como única fuente de información crítica.
- Controles mapeados a acciones abstractas, no a input directo.

---

## 4. Stack

| Componente | Elección | Nota |
|---|---|---|
| Motor | **Godot 4 — build .NET** | ⚠️ Son dos descargas distintas. La estándar solo corre GDScript. Hay que bajar la que dice **.NET** para usar C#. |
| Lenguaje | **C#** | Aprovecha experiencia previa en .NET. |
| SDK | **.NET SDK** | Godot fija un target framework en el `.csproj`. Los SDK son retrocompatibles, así que un SDK más nuevo suele compilar targets anteriores — pero **verificar la versión soportada en la doc oficial de Godot antes de asumir**. Se pueden tener múltiples SDK instalados en paralelo. |
| Editor de código | Rider o VS Code + extensión C# | Configurar en Godot: `Editor Settings → Dotnet → Editor`. |
| Control de versiones | Git + **Git LFS** | LFS para assets binarios (sprites, audio). Usar el `.gitignore` oficial de Godot (ignora `.godot/` y binarios compilados). |
| Pixel art | Aseprite | ~20 USD. Alternativas libres: LibreSprite, Krita. |
| Pipeline 3D→sprite | Blender | Opcional. Renderizar chasis low-poly a sprite da consistencia y rotaciones "gratis". |

**Trampa conocida:** el error más común al arrancar es descargar el build equivocado de Godot. Verificar que diga .NET y que `dotnet --list-sdks` devuelva algo antes de empezar.

---

## 5. Diseño de sistemas

### 5.1 Principio arquitectónico central

**Armas, módulos y chasis son datos, nunca código hardcodeado.**

Los efectos se modelan como **efectos componibles** (data + composición), no como un `switch` gigante. Esta decisión define si el proyecto escala o se muere en el mes 3.

La lógica de combate se escribe como **estado puro en C#, desacoplada de los nodos de Godot**. Así se puede testear sin abrir el motor.

### 5.2 Configuración del robot

La progresión no es "conseguir más unidades", es **configurar mejor**.

- **Chasis:** stats base + cantidad de slots.
- **Armas:** rango, patrón de área, coste de recurso.
- **Módulos:** booster, escudo, sensor, etc.

**Dos ejes de progresión con poco contenido nuevo:**
1. El robot sube desbloqueando slots.
2. El piloto sube aparte, con habilidades pasivas.

### 5.3 Combate

- Grilla, turnos por iniciativa.
- **Daño por tipo** (energía / cinético / explosivo) contra tipos de armadura.
- **Rango + línea de visión + terreno con cobertura** — da profundidad táctica sin requerir más assets.
- **Recurso de tensión** (calor o energía) que obliga a elegir entre disparar fuerte o moverse. Este es el elemento que separa el juego de un táctico genérico. **[abierto: calor vs energía]**
- TileMap de Godot con capas de terreno resuelve grilla y pathfinding (A* nativo).

### 5.4 Sincronización — sistema puente

**La sincronización es el recurso central del juego y a la vez la estructura política del mundo.** Es la decisión de diseño más importante del proyecto: la misma barra mide progresión mecánica y posición narrativa.

Reglas:
- Sync bajo → comandos limitados, drenaje de recursos.
- Sync alto → desbloquea módulos del chasis.
- **El piloto debe ser 100% tejido humano.** Cualquier prótesis sintética degrada la sincronización.
- Los pilotos heridos reciben **tejido de donantes humanos incompatibles**, no prótesis.
- El trasplante también se usa **electivamente** para subir sync (reemplazar tejido cansado por tejido joven).

**Consecuencia jugable:** al protagonista se le puede ofrecer el mismo upgrade como decisión de juego, con costo narrativo real. Los Siete tienen sync altísimo y llevan partes ajenas — legible en su diseño visual.

---

## 6. Arte

- **Vista:** isométrica o top-down 3/4. **[abierto]**
- **Sprites:** chicos (48–64px), paleta acotada.
- **Piezas reusables** por el sistema modular de chasis/armas/módulos.
- **Retratos de personajes:** es el rubro caro. Busto estático con ~3 expresiones por personaje.
- **Requisito de legibilidad:** cada personaje debe ser identificable en 64px — silueta única, paleta propia, un elemento visual memorable.
- **Identidad visual compartida entre piloto y su robot.**
- Los Siete deben mostrar visualmente su "contaminación": suturas, tonos de piel desparejos, asimetrías.

### Diseño de personajes — criterio establecido

Protagonistas y elenco femenino: **sí**, con diseño fuerte y memorable. El eje es carisma, silueta y presencia visual, no fanservice.

Razones de diseño, no solo de criterio:
- El público de tácticos es exigente con sistemas, no con escote. Los referentes que rompen (Into the Breach, Battle Brothers, Fire Emblem) lo hacen por mecánicas y personajes memorables.
- Un táctico con mecánica floja y personajes hipersexualizados se lee como asset flip.
- Contenido explícito complica el rating de Steam, descarta consolas y limita streamers y prensa — que es el canal de difusión real de un indie sin presupuesto.

**Convención práctica:** los pilotos usan traje de vuelo/piloto, que es lo que el género pide y es coherente con el mundo. La distinción entre personajes sale de detalles de traje, insignias, peinado, cicatrices y postura.

---

## 7. Riesgos identificados

| Riesgo | Severidad | Mitigación |
|---|---|---|
| Arte de retratos | Alta | Bustos estáticos, pocas expresiones, definir estilo temprano |
| Balance de misiones | Alta | Vertical slice primero, iterar sobre una misión antes de escalar |
| Scope creep narrativo | Media | El documento de historia está cerrado en alcance: 8–10 misiones |
| Sistema de efectos mal arquitecturado | Alta | Datos componibles desde el día 1, nunca `switch` |

**El riesgo no es el código.** Es el arte y el balance.

---

## 8. Próximos pasos

- [ ] Verificar build .NET de Godot y `dotnet --list-sdks`.
- [ ] Definir modelo de datos: chasis, armas, módulos, efectos.
- [ ] Prototipar el loop de una misión (grilla, movimiento, un ataque, condición de victoria).
- [ ] Decidir recurso de tensión: calor vs energía.
- [ ] Decidir vista: isométrica vs top-down 3/4.
- [ ] Definir estilo de retrato y hacer una prueba de personaje completo (retrato + sprite 64px).
