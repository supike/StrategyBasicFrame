using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }
        
        [Header("Camera Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float smoothDamping = 0.1f;
        [SerializeField] private float edgePanSpeed = 15f;
        [SerializeField] private float edgePanThreshold = 50f; // 화면 끝에서 팬할 픽셀 범위
        
        [Header("Camera Bounds")]
        [SerializeField] private Vector2 minBounds = new Vector2(-50, -50);
        [SerializeField] private Vector2 maxBounds = new Vector2(50, 50);
        [SerializeField] private bool useMapBounds = false;
        
        private Vector3 targetPosition;
        private Vector3 velocity = Vector3.zero;
        private Vector3 lastMousePosition;
        private bool isRightMousePressed = false;
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
            }
            
            targetPosition = transform.position;
        }

        private void Update()
        {
            HandleRightMouseDrag();
            HandleEdgePanning();
            UpdateCameraPosition();
        }

        /// <summary>
        /// 마우스 우클릭 드래그로 카메라 이동
        /// </summary>
        private void HandleRightMouseDrag()
        {
            // 우클릭 시작
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                isRightMousePressed = true;
                lastMousePosition = Input.mousePosition;
            }
            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI가 입력을 차단 중");
            }

            // 우클릭 중
            if ((Input.GetMouseButton(0) || Input.GetMouseButtonDown(1)) && isRightMousePressed)
            {
                Vector3 mouseDelta = lastMousePosition - Input.mousePosition;
                
                // 월드 좌표로 변환
                Vector3 worldDelta = mainCamera.ScreenToWorldPoint(
                    new Vector3(mouseDelta.x, mouseDelta.y, 0)) 
                    - mainCamera.ScreenToWorldPoint(Vector3.zero);
                
                targetPosition += worldDelta * moveSpeed * Time.deltaTime;
                lastMousePosition = Input.mousePosition;
            }
            
            // 우클릭 종료
            if (Input.GetMouseButtonUp(1))
            {
                isRightMousePressed = false;
            }
        }

        /// <summary>
        /// 화면 끝 근처에서 자동으로 카메라 팬
        /// </summary>
        private void HandleEdgePanning()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 screenSize = new Vector3(Screen.width, Screen.height, 0);
            Vector3 panDirection = Vector3.zero;
            
            // 왼쪽/오른쪽 팬
            if (mousePos.x < edgePanThreshold)
            {
                panDirection.x = -1;
            }
            else if (mousePos.x > screenSize.x - edgePanThreshold)
            {
                panDirection.x = 1;
            }
            
            // 위/아래 팬
            if (mousePos.y < edgePanThreshold)
            {
                panDirection.y = -1;
            }
            else if (mousePos.y > screenSize.y - edgePanThreshold)
            {
                panDirection.y = 1;
            }
            
            if (panDirection.magnitude > 0 && !isRightMousePressed)
            {
                targetPosition += panDirection.normalized * edgePanSpeed * Time.deltaTime;
            }
        }

        /// <summary>
        /// 부드러운 카메라 이동 적용
        /// </summary>
        private void UpdateCameraPosition()
        {
            // 경계 적용
            if (useMapBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }
            
            // 부드럽게 이동
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref velocity, 
                smoothDamping
            );
        }

        /// <summary>
        /// 특정 위치로 카메라 이동
        /// </summary>
        public void MoveCameraTo(Vector3 position)
        {
            targetPosition = position;
        }

        /// <summary>
        /// 특정 위치로 즉시 이동
        /// </summary>
        public void SetCameraPosition(Vector3 position)
        {
            targetPosition = position;
            transform.position = position;
        }

        /// <summary>
        /// 카메라 이동 속도 설정
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// 맵 경계 설정
        /// </summary>
        public void SetMapBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useMapBounds = true;
        }

        /// <summary>
        /// 맵 경계 비활성화
        /// </summary>
        public void DisableMapBounds()
        {
            useMapBounds = false;
        }
    }
}

