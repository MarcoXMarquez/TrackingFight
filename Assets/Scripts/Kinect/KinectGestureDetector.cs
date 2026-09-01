using System;
using UnityEngine;
using FightingGame.Core;

namespace FightingGame.Kinect
{
    /// <summary>
    /// Algoritmo de reconocimiento de gestos de combate en tiempo real
    /// basado en velocidad, aceleración y distancia relativa de articulaciones de Azure Kinect.
    /// </summary>
    public class KinectGestureDetector : MonoBehaviour
    {
        [Header("Punch Detection (Puños)")]
        [Tooltip("Velocidad mínima hacia adelante (m/s) para considerar un puño")]
        [SerializeField] private float punchSpeedThreshold = 2.2f;
        [Tooltip("Distancia mínima entre la mano y el hombro (extensión del brazo en metros)")]
        [SerializeField] private float punchExtensionThreshold = 0.42f;

        [Header("Kick Detection (Patadas)")]
        [Tooltip("Velocidad mínima del pie (m/s) para considerar una patada")]
        [SerializeField] private float kickSpeedThreshold = 2.0f;
        [Tooltip("Altura mínima del pie respecto a la cadera para considerar patada")]
        [SerializeField] private float kickHeightThreshold = -0.35f;

        [Header("Locomotion Detection (Salto, Agachado, Desplazamiento)")]
        [Tooltip("Descenso de la cabeza respecto a la postura base para agacharse (metros)")]
        [SerializeField] private float crouchDropThreshold = 0.22f;
        [Tooltip("Velocidad hacia arriba de la cadera para saltar (m/s)")]
        [SerializeField] private float jumpSpeedThreshold = 1.4f;
        [Tooltip("Desplazamiento lateral del cuerpo respecto al centro (metros)")]
        [SerializeField] private float lateralMoveDeadzone = 0.15f;

        [Header("Cooldowns (Anti-Spam de gestos)")]
        [SerializeField] private float attackCooldown = 0.25f;

        // Eventos para conectar con FighterController o cualquier otro sistema
        public event Action<AttackType> OnAttackDetected;
        public event Action<bool> OnCrouchStateChanged;
        public event Action OnJumpDetected;
        public event Action<float> OnHorizontalMove;
        public event Action<bool> OnDashDetected; // true = forward, false = backward

        // Almacenamiento interno de posiciones previas para cálculo de velocidad
        private Vector3[] currentJoints = new Vector3[(int)KinectJointType.Count];
        private Vector3[] previousJoints = new Vector3[(int)KinectJointType.Count];
        private Vector3[] jointVelocities = new Vector3[(int)KinectJointType.Count];

        private float lastAttackTime = -1f;
        private float calibratedHeadY = 0f;
        private float calibratedPelvisX = 0f;
        private bool isCalibrated = false;
        private bool isCurrentlyCrouching = false;

        public bool IsCalibrated => isCalibrated;

        private void Update()
        {
            if (!isCalibrated) return;

            CalculateVelocities();
            DetectPunches();
            DetectKicks();
            DetectCrouch();
            DetectJump();
            DetectLateralMovement();
        }

        /// <summary>
        /// Calibra la postura de pie inicial del jugador frente a la Kinect.
        /// </summary>
        public void CalibrateNeutralStance()
        {
            calibratedHeadY = currentJoints[(int)KinectJointType.Head].y;
            calibratedPelvisX = currentJoints[(int)KinectJointType.Pelvis].x;
            isCalibrated = true;
            Debug.Log($"[KinectGestureDetector] Calibración Neutral Completada. Altura Cabeza: {calibratedHeadY:F2}m");
        }

        /// <summary>
        /// Actualiza las posiciones de los joints recibidos del SDK de Azure Kinect.
        /// </summary>
        public void UpdateJointPositions(Vector3[] newJoints)
        {
            if (newJoints == null || newJoints.Length < (int)KinectJointType.Count) return;

            Array.Copy(currentJoints, previousJoints, currentJoints.Length);
            Array.Copy(newJoints, currentJoints, currentJoints.Length);

            if (!isCalibrated && currentJoints[(int)KinectJointType.Head] != Vector3.zero)
            {
                CalibrateNeutralStance();
            }
        }

        private void CalculateVelocities()
        {
            float dt = Time.deltaTime;
            if (dt <= 0.0001f) return;

            for (int i = 0; i < currentJoints.Length; i++)
            {
                // Filtro de media exponencial suave para reducir ruido del sensor
                Vector3 instantVelocity = (currentJoints[i] - previousJoints[i]) / dt;
                jointVelocities[i] = Vector3.Lerp(jointVelocities[i], instantVelocity, 0.7f);
            }
        }

        private void DetectPunches()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;

            // 1. Puño Izquierdo (Mano Izquierda)
            Vector3 handLeftPos = currentJoints[(int)KinectJointType.HandLeft];
            Vector3 shoulderLeftPos = currentJoints[(int)KinectJointType.ShoulderLeft];
            Vector3 handLeftVel = jointVelocities[(int)KinectJointType.HandLeft];

            float leftArmExtension = Vector3.Distance(handLeftPos, shoulderLeftPos);
            // Hacia adelante en el espacio del sensor (Z positivo o negativo según orientación)
            float leftForwardSpeed = -handLeftVel.z; 

            if (leftForwardSpeed > punchSpeedThreshold && leftArmExtension > punchExtensionThreshold)
            {
                TriggerAttack(AttackType.LeftPunch);
                return;
            }

            // 2. Puño Derecho (Mano Derecha)
            Vector3 handRightPos = currentJoints[(int)KinectJointType.HandRight];
            Vector3 shoulderRightPos = currentJoints[(int)KinectJointType.ShoulderRight];
            Vector3 handRightVel = jointVelocities[(int)KinectJointType.HandRight];

            float rightArmExtension = Vector3.Distance(handRightPos, shoulderRightPos);
            float rightForwardSpeed = -handRightVel.z;

            if (rightForwardSpeed > punchSpeedThreshold && rightArmExtension > punchExtensionThreshold)
            {
                TriggerAttack(AttackType.RightPunch);
                return;
            }
        }

        private void DetectKicks()
        {
            if (Time.time - lastAttackTime < attackCooldown) return;

            Vector3 pelvisPos = currentJoints[(int)KinectJointType.Pelvis];

            // 1. Patada Izquierda (Pie Izquierdo)
            Vector3 footLeftPos = currentJoints[(int)KinectJointType.AnkleLeft];
            Vector3 footLeftVel = jointVelocities[(int)KinectJointType.AnkleLeft];
            float footLeftHeightRel = footLeftPos.y - pelvisPos.y;
            float footLeftSpeed = footLeftVel.magnitude;

            if (footLeftHeightRel > kickHeightThreshold && footLeftSpeed > kickSpeedThreshold && -footLeftVel.z > 1.0f)
            {
                TriggerAttack(AttackType.LeftKick);
                return;
            }

            // 2. Patada Derecha (Pie Derecho)
            Vector3 footRightPos = currentJoints[(int)KinectJointType.AnkleRight];
            Vector3 footRightVel = jointVelocities[(int)KinectJointType.AnkleRight];
            float footRightHeightRel = footRightPos.y - pelvisPos.y;
            float footRightSpeed = footRightVel.magnitude;

            if (footRightHeightRel > kickHeightThreshold && footRightSpeed > kickSpeedThreshold && -footRightVel.z > 1.0f)
            {
                TriggerAttack(AttackType.RightKick);
                return;
            }
        }

        private void DetectCrouch()
        {
            float currentHeadY = currentJoints[(int)KinectJointType.Head].y;
            bool shouldCrouch = (calibratedHeadY - currentHeadY) > crouchDropThreshold;

            if (shouldCrouch != isCurrentlyCrouching)
            {
                isCurrentlyCrouching = shouldCrouch;
                OnCrouchStateChanged?.Invoke(isCurrentlyCrouching);
            }
        }

        private void DetectJump()
        {
            Vector3 pelvisVel = jointVelocities[(int)KinectJointType.Pelvis];
            if (pelvisVel.y > jumpSpeedThreshold && !isCurrentlyCrouching)
            {
                OnJumpDetected?.Invoke();
            }
        }

        private void DetectLateralMovement()
        {
            float currentPelvisX = currentJoints[(int)KinectJointType.Pelvis].x;
            float lateralOffset = currentPelvisX - calibratedPelvisX;

            if (Mathf.Abs(lateralOffset) > lateralMoveDeadzone)
            {
                float moveSign = Mathf.Sign(lateralOffset);
                OnHorizontalMove?.Invoke(moveSign);
            }
            else
            {
                OnHorizontalMove?.Invoke(0f);
            }
        }

        private void TriggerAttack(AttackType attack)
        {
            lastAttackTime = Time.time;
            OnAttackDetected?.Invoke(attack);
            Debug.Log($"[KinectGestureDetector] 🎯 GESTO DETECTADO: {attack} a las {Time.time:F2}s");
        }
    }
}
