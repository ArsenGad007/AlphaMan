using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Eldvmo.Ripples
{
    public class RaindropRipple : MonoBehaviour
    {
        [SerializeField] private MeshRenderer ripplePlane;
        private Vector4[] ripplePoints = new Vector4[100];
        private int rippleIndex = 0;
        private Vector2 _oldInputCentre;
        private int waterLayerMask;

        [SerializeField] private float raindropFrequency = 10f;
        private float rainDropTimer = 0f;
        Bounds bounds;



        void Start()
        {
            waterLayerMask = LayerMask.GetMask("Water");
            bounds = ripplePlane.bounds;
        }
        void Update()
        {
            rainDropTimer += Time.deltaTime;
            float interval = 1f / raindropFrequency;

            while (rainDropTimer >= interval)
            {
                rainDropTimer -= interval;
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomZ = Random.Range(bounds.min.z, bounds.max.z);
                Vector3 randomPos = new Vector3(randomX, bounds.max.y + 0.5f, randomZ); //shoot ray from top of plane

                Ray ray = new Ray(randomPos, Vector3.down);

                if (Physics.Raycast(ray, out RaycastHit hit, 10f, waterLayerMask))
                {
                    // Don't add to ripple if mouse position is too closed to the old mouse position
                    if (_oldInputCentre == null || Vector2.Distance(_oldInputCentre, hit.textureCoord) < 0.05f) return;

                    ripplePoints[rippleIndex] = new Vector4(hit.textureCoord.x, hit.textureCoord.y, Time.time, 0);
                    rippleIndex = (rippleIndex + 1) % ripplePoints.Length;
                    _oldInputCentre = hit.textureCoord;
                }

                ripplePlane.material.SetVectorArray("_InputCentre", ripplePoints);
            }
        }
    }
}