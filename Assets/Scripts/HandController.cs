using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandController : MonoBehaviour
{
    // 활성화 여부
    public static bool isActivate = false;

    // 현재 장창된 Hand형 타입 무기
    [SerializeField]
    private Hand currentHand;

    // 공격중??
    private bool isAttack = false; // false일 때 공격 가능
    private bool isSwing = false; // 팔을 휘두르는지 아닌지 체크

    // 레이저 쏘는거에 닿은거에 대한 정보가 hitInfo 에 들어감
    private RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        if (isActivate)
            TryAttack();
    }

    private void TryAttack()
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

    IEnumerator AttackCoroutine()
    {
        isAttack = true;
        currentHand.anim.SetTrigger("Attack");

        yield return new WaitForSeconds(currentHand.attackDelayA);
        isSwing = true; // 이때부터 공격이 들어가게
        // 공격이 적중했는지 안했는지 구분할수 있는 함수 코루틴으로 짬
        // 공격 활성화 시점.
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDelayB);
        isSwing = false;

        yield return new WaitForSeconds(currentHand.attackDelay - currentHand.attackDelayA - currentHand.attackDelayB);
        isAttack = false;
    }

    IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                isSwing = false;
                // 충돌했음
                Debug.Log(hitInfo.transform.name);
            }
            // 기본 문법이 코루틴은 대기를 해야함
            yield return null;
        }
    }

    private bool CheckObject()
    {
        // 레이저 발사 자기위치/어느방향/충돌물체/범위
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range))
        {
            return true;
        }
        return false;
    }

    public void HandChange(Hand _hand)
    {
        if (WeaponManager.currentWeapon != null)
        {
            WeaponManager.currentWeapon.gameObject.SetActive(false);
        }
        currentHand = _hand;
        WeaponManager.currentWeapon = currentHand.GetComponent<Transform>();
        WeaponManager.currentWeaponAnim = currentHand.anim;

        // 다른 걸로 바꿨다가 총으로 돌아왔을 때 다른 위치에 있을 수 있어서? 0으로 초기화
        currentHand.transform.localPosition = Vector3.zero;
        currentHand.gameObject.SetActive(true);
        isActivate = true;
    }
}