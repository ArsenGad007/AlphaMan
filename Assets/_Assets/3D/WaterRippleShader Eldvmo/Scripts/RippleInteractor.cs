using UnityEngine;

namespace Eldvmo.Ripples
{
    [System.Serializable]
    public class WaterData
    {
        public MeshRenderer ripplePlane;
        public Collider waterTrigger;
    }

    public class RippleInteractor : MonoBehaviour
    {
        [SerializeField] private WaterData[] waters;

        [Header("Mode")]
        [SerializeField] private bool continuousMode = false;

        [SerializeField] private bool floatWithWater = false;
        [SerializeField] private float floatAmplitude = 0.2f;
        [SerializeField] private float floatSpeed = 2f;
        private float baseY;
        private bool isInWater = false;

        private WaterData currentWater;

        private Vector4[] ripplePoints = new Vector4[10];
        private int rippleIndex = 0;

        private int waterLayerMask;
        private Vector2 lastUV;

        void Start()
        {
            waterLayerMask = LayerMask.GetMask("Water");
            baseY = transform.position.y;

            foreach (var water in waters)
            {
                if (water.waterTrigger != null &&
                    water.waterTrigger.bounds.Contains(transform.position))
                {
                    currentWater = water;
                    isInWater = true;
                    Debug.Log("[Ripple] Лодка стартовала внутри воды — isInWater = true");
                    break;
                }
            }
        }

        void OnTriggerEnter(Collider other)
        {
            foreach (var water in waters)
            {
                if (other == water.waterTrigger)
                {
                    currentWater = water;
                    isInWater = true;          
                    baseY = transform.position.y; 
                    return;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (currentWater != null && other == currentWater.waterTrigger)
            {
                currentWater = null;
                isInWater = false; 
            }
        }

        // Сбрасывает материал когда останавливаешь игру в редакторе
        void OnApplicationQuit()
        {
            foreach (var water in waters)
            {
                if (water.ripplePlane != null)
                {
                    water.ripplePlane.sharedMaterial
                        .SetVector("_ContinuousCentre", new Vector4(0, 0, -1, 0));

                    var empty = new Vector4[10];
                    for (int i = 0; i < empty.Length; i++)
                        empty[i] = new Vector4(0, 0, -1, 0);
                    water.ripplePlane.sharedMaterial
                        .SetVectorArray("_InputCentre", empty);
                }
            }
        }

        void FixedUpdate()
        {
            if (floatWithWater && continuousMode && isInWater)
            {
                float wave = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.position = new Vector3(
                    transform.position.x,
                    baseY + wave,
                    transform.position.z
                );
            }

            if (currentWater == null) return;

            Vector3 origin = transform.position + Vector3.up * 1f;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f, waterLayerMask))
            {
                Vector2 uv = hit.textureCoord;

                float uvJump = Vector2.Distance(lastUV, uv);
                if (lastUV != Vector2.zero && uvJump > 0.3f)
                {
                    lastUV = uv;
                    return;
                }

                if (!continuousMode)
                {
                    if (uvJump < 0.03f) return;

                    ripplePoints[rippleIndex] = new Vector4(uv.x, uv.y, Time.time, 0);
                    rippleIndex = (rippleIndex + 1) % ripplePoints.Length;

                    currentWater.ripplePlane.sharedMaterial
                        .SetVectorArray("_InputCentre", ripplePoints);
                }
                else
                {
                    Vector4 ripple = new Vector4(uv.x, uv.y, Time.time, 0);
                    currentWater.ripplePlane.sharedMaterial
                        .SetVector("_ContinuousCentre", ripple);
                }

                lastUV = uv;
            }
            if (floatWithWater && continuousMode)
            {
                float wave = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
                transform.position = new Vector3(
                    transform.position.x,
                    baseY + wave,
                    transform.position.z
                );
            }
        }
    }
}