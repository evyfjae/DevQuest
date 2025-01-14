using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;
public class MoveControl : MonoBehaviour
{
    [Header("Preset Fields")]
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private CapsuleCollider col;
    
    [Header("Settings")]
    [SerializeField][Range(1f, 10f)] private float moveSpeed;
    [SerializeField][Range(1f, 10f)] private float jumpAmount;
    //FSM(finite state machine)에 대한 더 자세한 내용은 세션 3회차에서 배울 것입니다!
    public enum State 
    {
        None,
        Idle,
        Jump
    }
    
    [Header("Debug")]
    public State state = State.None;
    public State nextState = State.None;
    public bool landed = false;
    public bool moving = false;
    
    private float stateTime;
    private Vector3 forward, right;

    IEnumerator coroutine;
    public float skill_coolTime;
    private float coolTime = 0;
    public TextMeshProUGUI cool_text;
    public GameObject skill_VFX;
    public Image JumpSkill_BG;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        
        state = State.None;
        nextState = State.Idle;
        stateTime = 0f;
        forward = transform.forward;
        right = transform.right;

        cool_text.text = "ON";
        skill_VFX.SetActive(false);
        JumpSkill_BG.fillAmount = 0;
    }

    private void Update()
    {
        UpdateInput();
        //0. 글로벌 상황 판단
        stateTime += Time.deltaTime;
        CheckLanded();
        //insert code here...

        //1. 스테이트 전환 상황 판단
        if (nextState == State.None) 
        {
            switch (state) 
            {
                case State.Idle:
                    if (landed) 
                    {
                        if (Input.GetKey(KeyCode.Space)) 
                        {
                            nextState = State.Jump;
                        }
                    }
                    break;
                case State.Jump:
                    if (landed) 
                    {
                        nextState = State.Idle;
                    }
                    break;
                //insert code here...
            }
        }
        
        //2. 스테이트 초기화
        if (nextState != State.None) 
        {
            state = nextState;
            nextState = State.None;
            switch (state) 
            {
                case State.Jump:
                    var vel = rigid.velocity;
                    vel.y = jumpAmount;
                    rigid.velocity = vel;
                    break;
                //insert code here...
            }
            stateTime = 0f;
        }
        
        if(Input.GetKeyDown(KeyCode.R))
        {
            if(coolTime == 0)
            {
                JumpSkill_BG.fillAmount = 1;
                coolTime = skill_coolTime;
                coroutine = Jump_up();
                CoroutineStart();
            }
        }
        Debug.Log("coolTime" + coolTime);
        
        if(coolTime != 0)
        {
            JumpSkill_BG.fillAmount -= 1/ skill_coolTime * Time.deltaTime;
            if (JumpSkill_BG.fillAmount <= 0)
            {
                JumpSkill_BG.fillAmount = 0;
            }
        }

        //3. 글로벌 & 스테이트 업데이트
        //insert code here...
    }

    void CoroutineStart()
    {
        if (coroutine != null)
        {
            skill_VFX.SetActive(true);
            StartCoroutine(coroutine);
            StartCoroutine(cool_Time());
        }
    }
    IEnumerator cool_Time()
    {
        for(int i = 0; i< skill_coolTime; i++)
        {
            Debug.Log(skill_coolTime - i);
            coolTime = skill_coolTime - i;
            cool_text.text = coolTime.ToString();
            yield return new WaitForSeconds(1f);
            coolTime--;
            cool_text.text = "ON";
        }
        Debug.Log("쿨타임 종료");
    }


    IEnumerator Jump_up()
    {
        Debug.Log("Jump_up 실행");
        jumpAmount *= 2;
        yield return new WaitForSeconds(5f);
        Debug.Log("효과 종료, 쿨타임 시작");
        jumpAmount /= 2;
        skill_VFX.SetActive(false);
    }

    //speedUp 아이템 충돌 시
    public void speedChange(float speed)
    {
        moveSpeed += speed;
        if (moveSpeed <= 0) moveSpeed = 0.5f;
    }


/*    private void FixedUpdate()
    {
        
    }*/

    private void CheckLanded() {
        //발 위치에 작은 구를 하나 생성한 후, 그 구가 땅에 닿는지 검사한다.
        //1 << 3은 Ground의 레이어가 3이기 때문, << 는 비트 연산자
        var center = col.bounds.center;
        var origin = new Vector3(center.x, center.y - ((col.height - 1f) / 2 + 0.15f), center.z);
        landed = Physics.CheckSphere(origin, 0.45f, 1 << 3, QueryTriggerInteraction.Ignore);
    }
    
    private void UpdateInput()
    {
        var direction = Vector3.zero;
        
        if (Input.GetKey(KeyCode.W)) direction += forward; //Forward
        if (Input.GetKey(KeyCode.A)) direction += -right; //Left
        if (Input.GetKey(KeyCode.S)) direction += -forward; //Back
        if (Input.GetKey(KeyCode.D)) direction += right; //Right
        
        direction.Normalize(); //대각선 이동(Ex. W + A)시에도 동일한 이동속도를 위해 direction을 Normalize
        
        transform.Translate( moveSpeed * Time.deltaTime * direction); //Move
    }
}
