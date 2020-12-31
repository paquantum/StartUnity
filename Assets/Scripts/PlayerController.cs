using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class PlayerController : MonoBehaviour
{
    // 스피드 조정 변수
    [SerializeField] // private하며 insperctor에서 안보이는걸 보이게 해줌
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    [SerializeField]
    private float crouchSpeed;

    private float applySpeed; // apply에 walk나 run을 대입하면 됨

    [SerializeField]
    private float jumpForce;

    // 상태 변수
    private bool isWalk = false;
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true; // true일 때만 점프 가능

    // 움직임 체크 변수,, 전 프레임의 플래이어 현재 위치
    private Vector3 lastPos;

    // 앚았을 때 얼마나 앚을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 땅 착지 여부
    private CapsuleCollider capsuleCollider;

    // 민감도
    [SerializeField]
    private float lookSensitivity; //민감도 같은 기능

    // 카메라 한계
    [SerializeField]
    private float cameraRotationLimit; // 화면을 올릴 때 일정 각도까지만 내릴때도 일정 각도까지만 하도록
    private float currentCameraRotationX = 0;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    private Rigidbody myRigid; // Rigidbody는 인간의 몸과 유사
    private GunController theGunController;
    private Crosshair theCrosshair;

    // Start is called before the first frame update
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        myRigid = GetComponent<Rigidbody>();
        theGunController = FindObjectOfType<GunController>();
        theCrosshair = FindObjectOfType<Crosshair>();

        // 초기화
        applySpeed = walkSpeed;
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    void Update()
    {
        IsGround();
        TryJump();
        TryRun(); // 뛰는지 걷는지 판단 하고 Move()를 할거라 Move() 위에 있어야 함
        TryCrouch();
        Move();
        MoveCheck();
        CameraRotation();
        CharacterRotation();
    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // 앉기 동작
    private void Crouch()
    {
        isCrouch = !isCrouch;
        theCrosshair.CrouchingAnimation(isCrouch);

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        }
        else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }
        StartCoroutine(CrouchCoroutine());
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);
        // 부자연스러움으로 Coroutine를 쓰기위해 주석처리후 StartCoroutine 사용
    }

    // 부드러운 앉기 동작 실행
    // 병렬 처리로 만들기 위해 있는게 coroutine 
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;
        while (_posY != applyCrouchPosY)
        {
            count++;
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f); // 1 -> 2 까지 30%씩 증가? 보간법
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);
            if (count > 15)
                break;
            yield return null; // 1 프레임씩 대기
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);
    }

    // 지면 체크
    private void IsGround()
    {
        // 캡슐콜라이더의 바운드 경계를 따라 extends.y는 y사이즈에서 절반을 down 아래로 쏨,, 0.1f는 경사면에서 위치오류로 여유를 주기 위해?
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
        theCrosshair.JumpingAnimation(!isGround);
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        // 앉아있을 때 점프할 면 앉은자세 해제
        if (isCrouch)
            Crouch();

        myRigid.velocity = transform.up * jumpForce;
    }

    // 달리기 시도
    private void TryRun()
    {
        if (Input.GetKey(KeyCode.LeftShift)) // GetKey는 눌러져있는 상태
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) // GetKey는 눌러져있는 상태
        {
            RunningCancel();
        }
    }

    // 달리기 실행
    private void Running()
    {
        if (isCrouch)
            Crouch();

        theGunController.CancelFineSight();

        isRun = true;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = runSpeed;
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        theCrosshair.RunningAnimation(isRun);
        applySpeed = walkSpeed;
    }

    // 움직임 실행
    private void Move()
    {
        // 1, -1, 0
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        //    오른쪽 왼쪽          (1, 0, 0)     *    -1 or 1
        Vector3 _moveHorizontal = transform.right * _moveDirX;
        //     위 아래             (0, 0, 1)     *     -1 or 1
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        // (1, 0, 0)    (0, 0, 1)
        // (1, 0, 1) = 2
        // 노멀라이즈 -> (0.5, 0, 0.5) = 1 // 1로 맞추는걸 권장함 속도면?
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        // 그냥 velocity로는 순간이동하듯이 움직여서
        // 약 1초 60번 프레임 동안 적절하게 움직이도록
        // Time.deltaTime의 값은 약 0.016
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);
    }

    private void MoveCheck()
    {
        if (!isRun && !isCrouch && isGround)
        {
            // 원래 lastPos != transform.position에서 vector로 바꾼건
            // 경사면등 0.000001.. 위치가 미끄려져도 걷는걸로 판단하면 안돼서
            if (Vector3.Distance(lastPos, transform.position) >= 0.01f)
                isWalk = true;
            else
                isWalk = false;

            theCrosshair.WalkingAnimation(isWalk);
            lastPos = transform.position;
        }
    }

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));
    }

    // 상하 카메라 회전
    private void CameraRotation()
    {
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        // 45도 줬다고 한번에 올리는게 아니라 lookSensitivity로 천천히 올라가도록 
        float _cameraRotationX = _xRotation * lookSensitivity;
        currentCameraRotationX -= _cameraRotationX;
        // currentcamerarotation을 -camerarotationlimit ~ +camerarotationlimit 사이에 가둠
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);

    }

}