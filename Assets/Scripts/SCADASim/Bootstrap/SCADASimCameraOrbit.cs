using UnityEngine;

namespace SCADASim.Bootstrap
{
    public sealed class SCADASimCameraOrbit : MonoBehaviour
    {
        [Header("Free Camera")]
        [SerializeField] private float lookSensitivity = 0.16f;
        [SerializeField] private float orbitSensitivity = 0.22f;
        [SerializeField] private float moveSpeed = 14f;
        [SerializeField] private float fastMoveMultiplier = 3.2f;
        [SerializeField] private float verticalMoveSpeed = 10f;
        [SerializeField] private float panSpeed = 0.09f;
        [SerializeField] private float wheelSpeed = 7f;

        [Header("Focus")]
        [SerializeField] private Vector3 target = new Vector3(0f, 5f, 0f);
        [SerializeField] private float yaw = 140f;
        [SerializeField] private float pitch = 28f;
        [SerializeField] private float focusDistance = 34f;

        public void Configure(Vector3 orbitTarget)
        {
            target = orbitTarget;
            Focus(target, 34f);
        }

        public void SetTarget(Vector3 orbitTarget)
        {
            target = orbitTarget;
            focusDistance = Mathf.Max(3f, Vector3.Distance(transform.position, target));
        }

        public void SetView(float newYaw, float newPitch, float newDistance)
        {
            yaw = newYaw;
            pitch = Mathf.Clamp(newPitch, -82f, 82f);
            Focus(target, newDistance);
        }

        private void LateUpdate()
        {
            float dt = Time.unscaledDeltaTime;
            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                UpdateOrbit();
            }
            else
            {
                UpdateMouseLook();
            }

            UpdateKeyboardMove(dt);
            UpdateMousePan(dt);
            UpdateWheelMove();

            if (Input.GetKeyDown(KeyCode.F))
            {
                Focus(target, 34f);
            }
        }

        private void UpdateMouseLook()
        {
            if (!Input.GetMouseButton(1))
            {
                return;
            }

            yaw += Input.GetAxisRaw("Mouse X") * lookSensitivity * 100f;
            pitch -= Input.GetAxisRaw("Mouse Y") * lookSensitivity * 100f;
            pitch = Mathf.Clamp(pitch, -82f, 82f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void UpdateOrbit()
        {
            if (!Input.GetMouseButton(0))
            {
                return;
            }

            yaw += Input.GetAxisRaw("Mouse X") * orbitSensitivity * 100f;
            pitch -= Input.GetAxisRaw("Mouse Y") * orbitSensitivity * 100f;
            pitch = Mathf.Clamp(pitch, -82f, 82f);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = rotation;
            transform.position = target + rotation * new Vector3(0f, 0f, -focusDistance);
        }

        private void UpdateKeyboardMove(float dt)
        {
            Vector3 localMove = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                localMove += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.S))
            {
                localMove += Vector3.back;
            }

            if (Input.GetKey(KeyCode.A))
            {
                localMove += Vector3.left;
            }

            if (Input.GetKey(KeyCode.D))
            {
                localMove += Vector3.right;
            }

            if (Input.GetKey(KeyCode.E))
            {
                localMove += Vector3.up;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                localMove += Vector3.down;
            }

            if (localMove.sqrMagnitude < 0.001f)
            {
                return;
            }

            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                ? fastMoveMultiplier
                : 1f);

            Vector3 worldMove =
                transform.forward * localMove.z +
                transform.right * localMove.x +
                Vector3.up * localMove.y * (verticalMoveSpeed / Mathf.Max(0.01f, moveSpeed));

            transform.position += worldMove.normalized * speed * dt;
        }

        private void UpdateMousePan(float dt)
        {
            if (!Input.GetMouseButton(2))
            {
                return;
            }

            Vector3 pan =
                -transform.right * Input.GetAxisRaw("Mouse X") +
                -transform.up * Input.GetAxisRaw("Mouse Y");

            Vector3 worldPan = pan * panSpeed * 100f * dt * Mathf.Max(0.35f, focusDistance / 24f);
            transform.position += worldPan;
            target += worldPan;
        }

        private void UpdateWheelMove()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) < 0.01f)
            {
                return;
            }

            float distanceScale = Mathf.Max(0.45f, focusDistance / 28f);
            Vector3 dolly = transform.forward * scroll * wheelSpeed * distanceScale;
            transform.position += dolly;
            focusDistance = Mathf.Max(3f, Vector3.Distance(transform.position, target));
        }

        private void Focus(Vector3 focusTarget, float distance)
        {
            focusDistance = Mathf.Max(3f, distance);
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.rotation = rotation;
            transform.position = focusTarget + rotation * new Vector3(0f, 0f, -focusDistance);
        }
    }
}
