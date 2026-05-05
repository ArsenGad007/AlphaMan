using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Eldvmo.Ripples
{
    public class BoatFloatControl : MonoBehaviour
    {
        private float startY;
        private int waterLayerMask;
        private Vector4 ripplePoint;
        [SerializeField] private MeshRenderer ripplePlane;
        Material ripplePlaneMaterial;
        [SerializeField] private float moveUpStrength = 0.2f;

        void Start()
        {
            startY = gameObject.transform.position.y;
            waterLayerMask = LayerMask.GetMask("Water");
            ripplePlaneMaterial = ripplePlane.gameObject.GetComponent<MeshRenderer>().material;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            //Raycast from the object toward the plane
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 direction = Vector3.down;

            Ray ray = new Ray(origin, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 2f, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;

                ripplePoint = new Vector4(uv.x, uv.y, Time.time, 0);

                // Send ripple centre to shader
                ripplePlane.material.SetVector("_InputCentre", ripplePoint);

                // Calculate boat Y offset matching the shader's wave
                Vector2 rippleUV = new Vector2(ripplePoint.x, ripplePoint.y);
                Vector2 boatUV = new Vector2(uv.x, uv.y); // Assume boat directly above hit point (simplification)

                Vector2 offset = boatUV - rippleUV;
                float distance = offset.magnitude;

                float waveFrequency = ripplePlaneMaterial.GetFloat("_WaveFrequency");
                float waveSpeed = ripplePlaneMaterial.GetFloat("_WaveSpeed");
                float waveStrength = ripplePlaneMaterial.GetFloat("_WaveStrength");

                float wave = Mathf.Cos(distance * waveFrequency - Time.time * waveSpeed) * 0.5f + 0.5f;

                float verticalOffset = wave * waveStrength * moveUpStrength;

                Vector3 currentPos = transform.position;
                currentPos.y = startY + verticalOffset;

                transform.position = currentPos;
            }
        }
    }
}
