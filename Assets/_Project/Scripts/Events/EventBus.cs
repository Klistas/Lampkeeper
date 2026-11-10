using System;

namespace Lampkeeper
{
    /// <summary>이벤트 버스로 보낼 수 있는 타입 표시용 인터페이스.</summary>
    public interface IGameEvent { }

    /// <summary>
    /// 타입별 정적 이벤트 버스.
    /// 사용: OnEnable에서 Subscribe, OnDisable에서 Unsubscribe. 해제를 빼먹으면 파괴된 오브젝트가 호출된다.
    /// 이벤트는 struct로 만들어 매 호출마다 GC 할당이 생기지 않게 한다.
    /// </summary>
    public static class EventBus<T> where T : struct, IGameEvent
    {
        private static Action<T> _handlers;

        public static void Subscribe(Action<T> handler) => _handlers += handler;
        public static void Unsubscribe(Action<T> handler) => _handlers -= handler;
        public static void Publish(T evt) => _handlers?.Invoke(evt);

        /// <summary>Enter Play Mode 옵션에서 Domain Reload를 끈 경우, 이전 플레이의 구독이 남을 수 있어 직접 비울 때 사용.</summary>
        public static void Clear() => _handlers = null;
    }
}
