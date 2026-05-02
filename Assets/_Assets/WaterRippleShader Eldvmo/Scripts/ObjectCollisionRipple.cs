using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eldvmo.Ripples
{
    public class ObjectCollisionRipple : MonoBehaviour
    {
        private bool isInWater = false;
        [SerializeField] private MeshRenderer ripplePlane;
        private Collider ripplePlaneCollider;
        private Vector4[] ripplePoints = new Vector4[10];
        private int rippleIndex = 0;
        private Vector2 _oldInputCentre;
        private int waterLayerMask;
        [SerializeField] private Collider waterTrigger;
        [SerializeField] bool isFloatingWithWater = true;
        [SerializeField] float moveUpHeight = 2f;
        private Rigidbody rb;

        void Start()
        {
            ripplePlaneCollider = ripplePlane.GetComponent<Collider>();
            waterLayerMask = LayerMask.GetMask("Water");
            rb = GetComponent<Rigidbody>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (ripplePlaneCollider != null && other == waterTrigger)
            {
                isInWater = true;
                Debug.Log("Вошел в воду");
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (ripplePlaneCollider != null && other == waterTrigger)
            {
                isInWater = false;
                Debug.Log("Вышел из воды");
            }
        }

        void FixedUpdate()
        {
            if (!isInWater) return;

            float waterY = ripplePlane.transform.position.y;
            Vector3 origin = transform.position + Vector3.up * 1f;
            Vector3 direction = Vector3.down;
            float distance = Mathf.Abs(waterY - transform.position.y) + 0.5f;

            Ray ray = new Ray(origin, direction);
            Debug.DrawRay(origin, direction * distance, Color.red, 0.5f);

            if (Physics.Raycast(ray, out RaycastHit hit, distance, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;
                if (_oldInputCentre != null && Vector2.Distance(_oldInputCentre, uv) < 0.05f) return;

                ripplePoints[rippleIndex] = new Vector4(uv.x, uv.y, Time.time, 0);
                rippleIndex = (rippleIndex + 1) % ripplePoints.Length;
                _oldInputCentre = uv;

                ripplePlane.material.SetVectorArray("_InputCentre", ripplePoints);
               // Debug.Log($"Ripple at {uv} | direction={direction}");
            }
            else
            {
               // Debug.Log($"Missed | direction={direction} origin={origin} waterY={waterY} myY={transform.position.y}");
            }
        }

        private void SetObjectHeight(float targetHeight)
        {
            Vector3 currentPos = transform.position;
            currentPos.y = Mathf.Lerp(currentPos.y, targetHeight, Time.fixedDeltaTime * 0.5f);
            transform.position = currentPos;
        }

        //Fake boat skipping wave
        private IEnumerator EnableGravity()
        {
            yield return new WaitForSeconds(0.5f);
            rb.useGravity = true;
        }
    }
}