using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    // 현재 장착된 총
    [SerializeField]
    private Gun currentGun;

    // 연사 속도 계산
    private float currentFireRate;

    // 상태 변수
    private bool isReload = false; // false일때만 발사
    [HideInInspector]
    public bool isFineSightMode = false; // false는 기본 true는 정조준 상태

    // 본래 포지션 값
    private Vector3 originPos;

    // 효과음 재생
    private AudioSource audioSource;

    // 레이저 충돌 정보 받아옴
    private RaycastHit hitInfo;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCam;

    // 피격 이펙트
    [SerializeField]
    private GameObject hit_effect_prefab;

    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFineSight();
    }

    // 연사속도 재계산
    private void GunFireRateCalc()
    {
        //Update는 매 프레임마다 호출 1초 60프레임 기준 deltatime은 60분의 1이라 1씩 감소 된다?
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime; // 1초에 1씩 감소시킴 델타타임은
    }

    // 발사 시도
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && !isReload) // 누르고 있으면 발사
        {
            Fire();
        }
    }

    // 발사 전 계산
    private void Fire() // 발사 전 준비 단계
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot();
            else
            {
                CancelFineSight();
                StartCoroutine(ReloadCoroutine());
            }
        }
    }


    // 발사 후 계산
    private void Shoot() // 발사 후
    {
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; // 연사 속도 재계산
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();

        // 총기 반동 코루틴 실행.
        StopAllCoroutines();
        StartCoroutine(RetroActionCoroutine());

    }
    
    private void Hit() // 미리 생성해서 하는? 오브젝트 풀링이라는 기법이 있음 여기서는 사용 X
    {
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward, out hitInfo, currentGun.range))
        {
            GameObject clone =  Instantiate(hit_effect_prefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2f); // 2초후 파괴
        }
    }

    // 재장전 시도
    private void TryReload()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancelFineSight();
            StartCoroutine(ReloadCoroutine());
        }
    }

    // 재장전
    IEnumerator ReloadCoroutine()
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;

            currentGun.anim.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }

            isReload = false;
        }
        else
        {
            Debug.Log("소유한 총알이 없습니다");
        }
    }

    // 정조준 시도
    private void TryFineSight()
    {
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            FineSight();
        }
    }

    // 정조준 취소
    public void CancelFineSight()
    {
        if (isFineSightMode)
            FineSight();
    }

    // 정조준 로직 가동
    private void FineSight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.anim.SetBool("FineSightMode", isFineSightMode);

        if (isFineSightMode)
        {
            StopAllCoroutines();
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeactivateCoroutine());
        }
    }

    // 정조준 활성화
    IEnumerator FineSightActivateCoroutine()
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
        {
            // 자식 객체인 경우 localPosition을 사용한다
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }

    // 정조준 비활성화
    IEnumerator FineSightDeactivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)
        {
            // 자식 객체인 경우 localPosition을 사용한다
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }

    // 반동 코루틴
    IEnumerator RetroActionCoroutine()
    {
        // 정조준 안했을 때 최대 반동
        Vector3 recoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z);
        // 정조준 했을 때 최대 반동
        Vector3 retroActionRecoilBack = new Vector3(currentGun.retroActionFineSigntForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z);

        if (!isFineSightMode)
        {
            currentGun.transform.localPosition = originPos;

            // 반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, recoilBack, 0.4f);
                yield return null;
            }

            // 원위치
            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.1f);
                yield return null; // 한 프레임마다 반복이 이뤄지게
            }

        }
        else
        {
            currentGun.transform.localPosition = currentGun.fineSightOriginPos;

            // 반동 시작
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSigntForce - 0.02f)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionRecoilBack, 0.4f);
                yield return null;
            }

            // 원위치
            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null; // 한 프레임마다 반복이 이뤄지게
            }
        }
    }

    // 사운드 재생
    private void PlaySE(AudioClip _clip)
    {
        audioSource.clip = _clip;
        audioSource.Play();
    }
}
