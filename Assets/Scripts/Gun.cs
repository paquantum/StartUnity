using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public string gunName; // 총의 이름
    public float range; // 사정 거리
    public float accuracy; // 정확도, 반동같은
    public float fireRate; // 연사속도
    public float reloadTime; // 재장전 속도

    public int damage; // 총의 데미지

    public int reloadBulletCount; // 총알 재장전 개수
    public int currentBulletCount; // 현재 탄알집에 남아있는 총알의 개수
    public int maxBulletCount; // 최대 소유 가능 총알 개수
    public int carryBulletCount; // 현재 소유하고 있는 총알 개수

    public float retroActionForce; // 반동 세기
    public float retroActionFineSigntForce; // 정주준시의 반동 세기

    public Vector3 fineSightOriginPos; // 정주준시 총의 위치?'
    public Animator anim; // 애니메이션 구현 
    //화염, 총알 발사효과 이펙트등 관리하기 위해
    public ParticleSystem muzzleFlash;

    public AudioClip fire_Sound;

    
}
