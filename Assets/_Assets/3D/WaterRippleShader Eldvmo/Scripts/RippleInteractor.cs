using UnityEngine;
using System.Collections.Generic;

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

        [Header("Personal slot range in shared array (64 total)")]
        [Tooltip("Начальный индекс диапазона этого объекта в общем массиве.")]
        [SerializeField] private int slotStart = 0;
        [Tooltip("Сколько последовательных ripple-точек оставляет за собой этот объект (как хвост следа).")]
        [SerializeField] private int slotCount = 4;

        private const int TOTAL_SLOTS = 64;

        private float baseY;
        private bool isInWater = false;
        private WaterData currentWater;

        private int localIndex = 0; // локальный индекс внутри своего диапазона

        private int waterLayerMask;
        private Vector2 lastUV;

        // Один общий массив на материал — каждый объект пишет только в свой диапазон
        private static Dictionary<MeshRenderer, Vector4[]> sharedArrays
            = new Dictionary<MeshRenderer, Vector4[]>();

        void Start()
        {
            waterLayerMask = LayerMask.GetMask("Water");
            baseY = transform.position.y;

            foreach (var water in waters)
            {
                if (water.ripplePlane != null && !sharedArrays.ContainsKey(water.ripplePlane))
                {
                    var arr = new Vector4[TOTAL_SLOTS];
                    for (int i = 0; i < TOTAL_SLOTS; i++)
                        arr[i] = new Vector4(0, 0, -1, 0);
                    sharedArrays[water.ripplePlane] = arr;
                    water.ripplePlane.sharedMaterial.SetVectorArray("_InputCentre", arr);
                }

                if (water.waterTrigger != null &&
                    water.waterTrigger.bounds.Contains(transform.position))
                {
                    currentWater = water;
                    isInWater = true;
                    baseY = transform.position.y;
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

        void OnApplicationQuit()
        {
            foreach (var water in waters)
            {
                if (water.ripplePlane == null) continue;
                water.ripplePlane.sharedMaterial
                    .SetVector("_ContinuousCentre", new Vector4(0, 0, -1, 0));

                var empty = new Vector4[TOTAL_SLOTS];
                for (int i = 0; i < TOTAL_SLOTS; i++)
                    empty[i] = new Vector4(0, 0, -1, 0);
                water.ripplePlane.sharedMaterial.SetVectorArray("_InputCentre", empty);
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

                    // Пишем в следующую ячейку СВОЕГО диапазона — старые точки в этом же
                    // диапазоне остаются жить своё время и расплываются, как в оригинале
                    if (sharedArrays.TryGetValue(currentWater.ripplePlane, out var arr))
                    {
                        arr[slotStart + localIndex] = new Vector4(uv.x, uv.y, Time.time, 0);
                        localIndex = (localIndex + 1) % slotCount;

                        currentWater.ripplePlane.sharedMaterial
                            .SetVectorArray("_InputCentre", arr);
                    }
                }
                else
                {
                    currentWater.ripplePlane.sharedMaterial
                        .SetVector("_ContinuousCentre",
                            new Vector4(uv.x, uv.y, Time.time, 0));
                }

                lastUV = uv;
            }
        }
    }
}