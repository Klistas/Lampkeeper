using UnityEngine;

namespace Lampkeeper
{
    /// <summary>
    /// 등불: 연료가 시간에 따라 줄고, 연료 비율에 따라 빛 반경이 줄어든다.
    /// 플레이어 자식 오브젝트에 붙인다. lightVisual은 지름 1 유닛짜리 원 스프라이트(반투명)를 쓴다.
    /// </summary>
    public class Lantern : MonoBehaviour
    {
        [SerializeField] private PlayerData data;
        [SerializeField] private Transform lightVisual;

        public float Fuel { get; private set; }
        public float MaxFuel { get; private set; }
        public float Radius { get; private set; }

        private bool _emptied;

        private void Awake()
        {
            MaxFuel = data.maxFuel;
            Fuel = MaxFuel;
            UpdateRadius();
        }

        private void Start() => PublishFuel();

        private void Update()
        {
            if (_emptied) return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameState.Playing) return;

            Fuel = Mathf.Max(0f, Fuel - data.fuelDrainPerSecond * Time.deltaTime);
            UpdateRadius();
            PublishFuel();

            if (Fuel <= 0f)
            {
                _emptied = true;
                EventBus<LanternEmptiedEvent>.Publish(new LanternEmptiedEvent());
            }
        }

        public bool Contains(Vector2 worldPoint)
        {
            return ((Vector2)transform.position - worldPoint).sqrMagnitude <= Radius * Radius;
        }

        public void AddFuel(float amount)
        {
            if (_emptied || amount <= 0f) return;
            Fuel = Mathf.Min(MaxFuel, Fuel + amount);
            UpdateRadius();
            PublishFuel();
        }

        public void AddMaxFuel(float amount)
        {
            MaxFuel += amount;
            AddFuel(amount);
        }

        private void UpdateRadius()
        {
            Radius = Mathf.Lerp(data.minLightRadius, data.maxLightRadius, Fuel / MaxFuel);
            if (lightVisual != null) lightVisual.localScale = Vector3.one * (Radius * 2f);
        }

        private void PublishFuel() => EventBus<LanternFuelChangedEvent>.Publish(new LanternFuelChangedEvent(Fuel, MaxFuel));
    }
}
