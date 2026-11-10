using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Lampkeeper
{
    /// <summary>풀에서 꺼내고 돌려줄 때 호출받고 싶은 컴포넌트가 구현한다.</summary>
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }

    /// <summary>
    /// 풀이 생성한 오브젝트에 자동으로 붙는다. 반환은 PooledObject.Despawn(gameObject)로 한다.
    /// 풀 소속이 아닌 오브젝트에 호출하면 Destroy로 처리되므로 어디서든 같은 코드를 쓸 수 있다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PooledObject : MonoBehaviour
    {
        private Action _release;

        internal void Bind(Action release) => _release = release;

        public static void Despawn(GameObject go)
        {
            if (go.TryGetComponent(out PooledObject pooled) && pooled._release != null) pooled._release();
            else Object.Destroy(go);
        }
    }

    /// <summary>
    /// 프리팹 기반 컴포넌트 풀. 생성된 오브젝트는 현재 씬에 속하므로 씬이 바뀌면 함께 사라진다.
    /// 풀은 그 오브젝트를 쓰는 쪽(플레이어, 스포너)이 들고 있는다.
    /// </summary>
    public class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new Stack<T>();
        private readonly List<IPoolable> _callbackBuffer = new List<IPoolable>();

        public int CountAll { get; private set; }
        public int CountInactive => _inactive.Count;

        public ComponentPool(T prefab, int prewarm = 0, Transform parent = null)
        {
            if (prefab == null) throw new ArgumentNullException(nameof(prefab));
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarm; i++) _inactive.Push(Create());
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T item = null;
            while (item == null)
            {
                item = _inactive.Count > 0 ? _inactive.Pop() : Create(); // 외부에서 Destroy된 항목은 건너뜀
            }

            item.transform.SetPositionAndRotation(position, rotation);
            item.gameObject.SetActive(true);
            Notify(item, spawned: true);
            return item;
        }

        public void Release(T item)
        {
            if (item == null || !item.gameObject.activeSelf) return; // 이미 반환된 항목(중복 반환 방지)
            Notify(item, spawned: false);
            item.gameObject.SetActive(false);
            _inactive.Push(item);
        }

        private T Create()
        {
            T item = Object.Instantiate(_prefab, _parent);
            item.gameObject.SetActive(false);
            if (!item.TryGetComponent(out PooledObject pooled)) pooled = item.gameObject.AddComponent<PooledObject>();
            pooled.Bind(() => Release(item));
            CountAll++;
            return item;
        }

        private void Notify(T item, bool spawned)
        {
            item.GetComponents(_callbackBuffer);
            for (int i = 0; i < _callbackBuffer.Count; i++)
            {
                if (spawned) _callbackBuffer[i].OnSpawned();
                else _callbackBuffer[i].OnDespawned();
            }
        }
    }
}
