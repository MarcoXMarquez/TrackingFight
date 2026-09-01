using UnityEngine;

namespace FightingGame.Kinect
{
    /// <summary>
    /// Tipos de articulaciones estándar del Azure Kinect Body Tracking SDK (32 joints).
    /// </summary>
    public enum KinectJointType
    {
        Pelvis = 0,
        SpineNavel = 1,
        SpineChest = 2,
        Neck = 3,
        ClavicleLeft = 4,
        ShoulderLeft = 5,
        ElbowLeft = 6,
        WristLeft = 7,
        HandLeft = 8,
        HandTipLeft = 9,
        ThumbLeft = 10,
        ClavicleRight = 11,
        ShoulderRight = 12,
        ElbowRight = 13,
        WristRight = 14,
        HandRight = 15,
        HandTipRight = 16,
        ThumbRight = 17,
        HipLeft = 18,
        KneeLeft = 19,
        AnkleLeft = 20,
        FootLeft = 21,
        HipRight = 22,
        KneeRight = 23,
        AnkleRight = 24,
        FootRight = 25,
        Head = 26,
        Nose = 27,
        EyeLeft = 28,
        EarLeft = 29,
        EyeRight = 30,
        EarRight = 31,
        Count = 32
    }

    /// <summary>
    /// Datos individuales de una articulación rastreada por Kinect.
    /// </summary>
    [System.Serializable]
    public struct KinectJointData
    {
        public KinectJointType jointType;
        public Vector3 position;       // Posición en metros (espacio 3D)
        public Quaternion rotation;   // Orientación del hueso
        public Vector3 velocity;       // Velocidad calculada frame-a-frame (m/s)
        public float confidence;      // Nivel de confianza del tracking (0 a 1)
    }
}
