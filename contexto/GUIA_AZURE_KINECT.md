# 🎮 Guía Maestra: Integración de Microsoft Azure Kinect DK en TrackingFight

Este documento describe la arquitectura, instalación de SDKs, configuración de hardware y el sistema de reconocimiento de gestos en tiempo real para controlar a los luchadores mediante movimiento corporal.

---

## 📌 1. Checklist de Instalación en Windows

Antes de iniciar la integración en Unity, se deben instalar los siguientes paquetes oficiales en la máquina:

1. **Azure Kinect Sensor SDK (v1.4.1)**:
   - Drivers de cámara de profundidad, cámara RGB e IMU.
   - Herramienta de diagnóstico: `k4aviewer.exe`.
   - [Descarga oficial del Sensor SDK](https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/docs/usage.md)

2. **Azure Kinect Body Tracking SDK (v1.1.2)**:
   - Red neuronal ONNX para estimación de postura corporal en 3D (32 articulaciones a 30 FPS).
   - Visor 3D de prueba: `k4abt_simple_3d_viewer.exe`.
   - [Descarga oficial del Body Tracking SDK](https://docs.microsoft.com/en-us/azure/kinect-dk/body-sdk-download)

3. **Visual C++ Redistributable 2015–2022 (x64)**.

4. **Controladores de GPU (NVIDIA)**:
   - Actualizados con soporte CUDA / DirectML para procesar el Body Tracking en GPU con latencia mínima (< 20ms).

---

## 🔌 2. Requisitos de Hardware y Espacio Físico

- **Conexión USB**: Puerto USB 3.0 dedicado directo a la placa base (puerto azul / SS). Evitar hubs o extensiones USB.
- **Alimentación**: Adaptador de corriente oficial conectado a la toma de corriente.
- **Distancia de juego recomendada**: 1.8 a 2.5 metros frente al sensor.
- **Altura de la cámara**: 1.0 a 1.4 metros del suelo, ligeramente inclinada hacia el pecho del jugador.

---

## 🧠 3. Arquitectura del Reconocimiento de Gestos

El flujo de datos se desacopla en tres capas independientes:

```
[Azure Kinect DK] (Hardware de Profundidad)
       │
       ▼ (30 FPS / Profundidad NFOV)
[Azure Kinect Body Tracking Runtime] (32 Joints en coordenadas 3D de cámara en metros)
       │
       ▼
[KinectGestureDetector.cs] (Cálculo de velocidades m/s, extensiones y umbrales)
       │
       ├─► Puño Izquierdo (LP)  : Mano Izq acelera hacia adelante (> 2.2 m/s) + extensión brazo.
       ├─► Puño Derecho (RP)    : Mano Der acelera hacia adelante (> 2.2 m/s) + extensión brazo.
       ├─► Patada Izquierda (LK): Tobillo/Pie Izq elevado + velocidad de patada.
       ├─► Patada Derecha (RK)  : Tobillo/Pie Der elevado + velocidad de patada.
       ├─► Agachado (Crouch)    : Descenso de la cabeza respecto a la postura neutral (> 0.22 m).
       ├─► Salto (Jump)         : Velocidad vertical ascendente de la pelvis (> 1.4 m/s).
       └─► Desplazamiento       : Desplazamiento lateral de la pelvis respecto al centro calibrado.
       │
       ▼
[KinectFighterBridge.cs] (Conexión directa con FighterController)
       │
       ▼
[FighterController.cs] (Ejecuta animaciones, locomoción y activa Hitboxes en Unity)
```

---

## 🛠️ 4. Scripts Implementados en el Proyecto

Los scripts de integración se encuentran en `Assets/Scripts/Kinect/`:

1. **[`KinectJointType.cs`](file:///c:/Users/marco/Desktop/TrackingFight/Assets/Scripts/Kinect/KinectJointType.cs)**:
   - Enum con los 32 joints de Azure Kinect (`Pelvis`, `SpineChest`, `ShoulderLeft`, `HandLeft`, `AnkleRight`, etc.) y estructura `KinectJointData`.

2. **[`KinectGestureDetector.cs`](file:///c:/Users/marco/Desktop/TrackingFight/Assets/Scripts/Kinect/KinectGestureDetector.cs)**:
   - Algoritmo de reconocimiento de gestos por velocidad y distancias relativas.
   - Filtro exponencial de velocidad para eliminar temblor/ruido del sensor.
   - Cooldowns anti-spam para evitar ráfagas accidentales de golpes en un solo movimiento.
   - Sistema de calibración de postura neutral (`CalibrateNeutralStance()`).

3. **[`KinectFighterBridge.cs`](file:///c:/Users/marco/Desktop/TrackingFight/Assets/Scripts/Kinect/KinectFighterBridge.cs)**:
   - Componente asignado al Dummy que suscribe los eventos del detector a `FighterController.ExecuteAttack()`, `MoveHorizontal()`, `Crouch()` y `Jump()`.

---

## 🚀 5. Plan de Ejecución para la Sesión de Integración

1. **Paso 1**: Conectar la cámara Azure Kinect, verificar con `k4aviewer` y `k4abt_simple_3d_viewer`.
2. **Paso 2**: Importar el wrapper de Azure Kinect en Unity (DLLs nativas de Body Tracking).
3. **Paso 3**: Vincular la salida del wrapper al `KinectGestureDetector`.
4. **Paso 4**: Calibrar los umbrales de velocidad y distancia según la fuerza y alcance de tus golpes.
5. **Paso 5**: Prueba en Play Mode: combate real por captura de movimiento.
