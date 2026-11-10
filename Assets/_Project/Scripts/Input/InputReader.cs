using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Lampkeeper
{
    /// <summary>
    /// Input System 래퍼. 게임 코드는 이 클래스만 보고, Keyboard/Mouse 같은 장치를 직접 읽지 않는다.
    /// 액션은 코드로 정의했다. .inputactions 에셋으로 바꾸고 싶으면 이 클래스 내부만 고치면 된다.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        /// <summary>Esc / 게임패드 Start. 게임 상태와 관계없이 발생한다.</summary>
        public event Action PausePressed;

        public Vector2 Move => _gameplayEnabled ? Vector2.ClampMagnitude(_move.ReadValue<Vector2>(), 1f) : Vector2.zero;
        public bool FireHeld => _gameplayEnabled && _fire.IsPressed();

        /// <summary>마우스(포인터) 화면 좌표(픽셀).</summary>
        public Vector2 PointerScreenPosition => _point.ReadValue<Vector2>();

        private InputAction _move;
        private InputAction _point;
        private InputAction _fire;
        private InputAction _pause;
        private bool _gameplayEnabled;

        private void Awake()
        {
            _move = new InputAction("Move", InputActionType.Value);
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _move.AddBinding("<Gamepad>/leftStick");

            _point = new InputAction("Point", InputActionType.PassThrough, "<Pointer>/position");

            _fire = new InputAction("Fire", InputActionType.Button, "<Mouse>/leftButton");
            _fire.AddBinding("<Gamepad>/rightTrigger");

            _pause = new InputAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            _pause.AddBinding("<Gamepad>/start");
            _pause.performed += OnPausePerformed;
        }

        private void OnEnable()
        {
            _move.Enable();
            _point.Enable();
            _fire.Enable();
            _pause.Enable();
        }

        private void OnDisable()
        {
            _move.Disable();
            _point.Disable();
            _fire.Disable();
            _pause.Disable();
        }

        private void OnDestroy()
        {
            _pause.performed -= OnPausePerformed;
            _move.Dispose();
            _point.Dispose();
            _fire.Dispose();
            _pause.Dispose();
        }

        /// <summary>GameManager가 Playing 상태일 때만 true로 켠다.</summary>
        public void SetGameplayEnabled(bool enabled) => _gameplayEnabled = enabled;

        private void OnPausePerformed(InputAction.CallbackContext context) => PausePressed?.Invoke();
    }
}
