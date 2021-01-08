using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 미완성 클래스는 UNITY Inspector창에서 끌어오는?거 스크립트 사용 못해서
// 상속받은 자식을 이용해서 하기 때문에 Update()함수 등 실행 안됨
public abstract class CloseWeaponController : MonoBehaviour
{ // 미완성 클래스 = 추상 클래스
    

    // 현재 장창된 Hand형 타입 무기
    [SerializeField]
    protected CloseWeapon currentCloseWeapon;

    // 공격중??
    protected bool isAttack = false; // false일 때 공격 가능
    protected bool isSwing = false; // 팔을 휘두르는지 아닌지 체크

    // 레이저 쏘는거에 닿은거에 대한 정보가 hitInfo 에 들어감
    protected RaycastHit hitInfo;

    protected void TryAttack()
    {
        // 누르고 있는 동안에도 가능하게, Fire1은 마우스좌클링 
        if (Input.GetButton("Fire1"))
        {
            // false일 경우 딜레이때문에 수정
            if (!isAttack)
            {
                // true로 바꾸는데 딜레이가 있기때문에 코루틴 실행
                StartCoroutine(AttackCoroutine());
            }
        }
    }

    protected IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentCloseWeapon.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayA);
        isSwing = true; // 이때부터 공격이 들어가게
        // 공격이 적중했는지 안했는지 구분할수 있는 함수 코루틴으로 짬
        // 공격 활성화 시점.
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentCloseWeapon.attackDelayB);
        isSwing = false;

        yield return new WaitForSeconds(currentCloseWeapon.attackDelay - currentCloseWeapon.attackDelayA - currentCloseWeapon.attackDelayB);
        isAttack = false;
    }

    // 미완성 = 추상 코루틴, 자식이 완성시켜라
    protected abstract IEnumerator HitCoroutine();
    
    protected bool CheckObject()
    {
        // 레이저 발사 자기위치/어느방향/충돌물체/범위
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentCloseWeapon.range))
        {
            return true;
        }
        return false;
    }

    // 가상함수, 완성 함수이지만, 추가 편집이 가능한 함수
    public virtual void CloseWeaponChange(CloseWeapon _CloseWeapon)
    {
        if (WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        }
        currentCloseWeapon = _CloseWeapon;
        WeaponManager.currentWeapon = currentCloseWeapon.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentCloseWeapon.anim;

        // 다른 걸로 바꿨다가 총으로 돌아왔을 때 다른 위치에 있을 수 있어서? 0으로 초기화
        currentCloseWeapon.transform.localPosition = Vector3.zero;
        currentCloseWeapon.gameObject.SetActive(true);
    }
}
