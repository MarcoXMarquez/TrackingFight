# 🥊 Sistema de Contexto y Reportes del Proyecto (TrackingFight)

Este directorio (`contexto/`) sirve como la fuente de verdad y memoria compartida para cualquier instancia de **Antigravity** (o cualquier desarrollador/agente) que trabaje en este proyecto en diferentes equipos.

---

## 📌 1. Visión General del Proyecto
- **Nombre del Proyecto**: TrackingFight
- **Motor / Versión**: Unity (URP - Universal Render Pipeline)
- **Paquetes Clave**: `com.unity.inputsystem` (New Input System), URP 17.5+, Azure Kinect Body Tracking SDK.
- **Género**: Juego de Pelea 2.5D estilo *Mortal Kombat* / *Street Fighter* (modelos y físicas en 3D restringidos rígidamente a un plano 2D, con eje Z bloqueado).
- **Esquema de Control**: 
  - 4 botones clásicos (1: Left Punch, 2: Right Punch, 3: Left Kick, 4: Right Kick) en Teclado y Gamepad.
  - **Control por Captura de Movimiento en Tiempo Real con Microsoft Azure Kinect DK** (reconocimiento de velocidad y extensión de puños, patadas, saltos y agachado).

---

## 📜 2. Reglas de Interacción Fundamentales (¡IMPORTANTE PARA ANTIGRAVITY!)

> [!IMPORTANT]
> **Condición de Trabajo del Usuario**:
> El usuario trabaja de forma **estrictamente paso a paso**. 
> **NO AVANZAR al siguiente paso o instrucción hasta que el usuario confirme explícitamente que ha completado el paso actual.**
> Brindar instrucciones directas, claras y con la configuración exacta para Unity Inspector / Animator.

---

## 🗂️ 3. Estructura de este Directorio `contexto/`

```
TrackingFight/
└── contexto/
    ├── README.md                 # Este archivo (visión global, reglas y arquitectura)
    ├── GUIA_AZURE_KINECT.md      # Guía maestra de instalación, hardware y arquitectura de Azure Kinect
    ├── PLANTILLA_REPORTE.md      # Plantilla estandarizada para registrar futuras sesiones
    └── reporte_YYYY-MM-DD.md     # Reportes fechados de cada sesión de desarrollo
```

---

## 🏗️ 4. Arquitectura de Código Implementada

Los scripts se encuentran organizados en `Assets/Scripts/`:

1. **`Assets/Scripts/Core/`**:
   - `FighterState.cs`: Enums `FighterState` (Idle, WalkForward, WalkBackward, Crouch, JumpNeutral, JumpForward, JumpBackward, DashForward, DashBackward, Attack, Hitstun, Block, Dead) y `AttackType` (None, LeftPunch, RightPunch, LeftKick, RightKick).

2. **`Assets/Scripts/Character/`**:
   - `FighterController.cs`: Núcleo de locomoción con `CharacterController`, bloqueo de eje Z (`lockedZ`), control de gravedad, auto-facing hacia el rival (`opponent`), máquina de estados y gestión de `Hitstun` / `Knockback`.
   - `FighterAnimatorSync.cs`: Puente optimizado por hashes de parámetros entre el código y el `Animator Controller`. Soporta Animation Events (`AnimEvent_OnHitboxStart`, `AnimEvent_OnHitboxEnd`, `AnimEvent_OnAttackFinished`).
   - `FighterCombat.cs`: Gestión de vida (HP), recepción de daño, cálculo de knockback y activación selectiva de hitboxes por tipo de ataque.

3. **`Assets/Scripts/Input/`**:
   - `FighterInputHandler.cs`: Soporte para Teclado y Gamepad (Xbox/PlayStation), detección de **Double-Tap** para Dashes, soporte para botones dedicados y un **Input Buffer** para combos fluidos.

4. **`Assets/Scripts/Combat/`**:
   - `Hitbox.cs`: Colisionador de ataque en manos y pies (trigger temporal activo solo durante el golpe, con gizmos rojos en Scene view).
   - `Hurtbox.cs`: Colisionador receptor de daño en el cuerpo (cabeza, torso, piernas, con gizmos verdes en Scene view).

5. **`Assets/Scripts/Kinect/`**:
   - `KinectJointType.cs`: Definición de los 32 joints de Azure Kinect y estructura de datos de posición/velocidad.
   - `KinectGestureDetector.cs`: Algoritmo de detección de puños (velocidad/extensión), patadas, agachado, saltos y desplazamiento en tiempo real con filtros anti-ruido.
   - `KinectFighterBridge.cs`: Puente directo que conecta los eventos de Kinect con `FighterController`.

---

## 📝 5. Historial de Reportes y Guías
- [`GUIA_AZURE_KINECT.md`](file:///c:/Users/marco/Desktop/TrackingFight/contexto/GUIA_AZURE_KINECT.md): Guía maestra de hardware, SDKs y detección de gestos con Azure Kinect.
- [`reporte_2026-08-31.md`](file:///c:/Users/marco/Desktop/TrackingFight/contexto/reporte_2026-08-31.md): Sesión 1 (arquitectura base, inputs, scripts de combate, animaciones Mixamo y setup de Animator).
