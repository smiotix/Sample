using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class EA_TEST02 : MonoBehaviour
{
    private enum UType
    {
        Hummer,
        Gunner
    }
    private enum ComMode
    {
        Attack,
        Move,
        Back,
        Nothing
    }
    [SerializeField]
    private UType type;
    [SerializeField]
    private GameObject HummerPrefab;
    [SerializeField]
    private Transform HummerPoint;
    private GameObject EWeapon;
    private Transform PT;
    private List<GameObject> PLList = new List<GameObject>();
    private bool[] FLAGS = new bool[7];
    private Rigidbody Rb;
    private Animator Anim;
    private GameConfigClass GCC;
    private bool isGround = false;
    private float t = 0;
    private Vector3 BSSpos = Vector3.zero;
    private Vector3 BSEpos = Vector3.zero;
    [SerializeField]
    private float JumpGain;
    private ComMode commode;
    [SerializeField]
    private float MoveSpeed;
    private EnemyStatus ES;
    // Start is called before the first frame update
    private async UniTask Start()
    {
        ES = GetComponent<EnemyStatus>();
        commode = ComMode.Attack;
        for(int i=0;i< FLAGS.Length; i++)
        {
            FLAGS[i] = true;
        }
        Rb = GetComponent<Rigidbody>();
        Anim = GetComponent<Animator>();
        GCC = GameObject.Find("System").GetComponent<GameConfigClass>();
        if(type == UType.Hummer)
        {
            await HummerStart();
        }
    }

    // Update is called once per frame
    private async UniTask Update()
    {
        await IsGround();
        if (FLAGS[0])
        {
            SearchPT().Forget();
        }        
        //Debug.Log(isGround);
        if (type == UType.Hummer)
        {
            await HummerUpdate();
        }
        if (ES.BashF && FLAGS[6])
        {
            await HasBushed();
        }
    }
    private async UniTask ChkHP()
    {
        if(ES != null)
        {
            if(ES.HP <= 0)
            {
                Destroy(this.gameObject);
            }
        }
    }
    private async UniTask HummerUpdate()
    {
        if (!FLAGS[5])
        {
            if (PT != null && commode != ComMode.Move && commode != ComMode.Back && commode != ComMode.Nothing)
            {
                float dis = Vector3.Distance(Rb.position, PT.position);
                if (dis > 3f)
                {
                    commode = ComMode.Move;
                    FLAGS[1] = true;
                    FLAGS[2] = true;
                    FLAGS[3] = true;
                }
            }
            if (commode == ComMode.Nothing)
            {
                
            }
            else if (commode == ComMode.Back)
            {
                await BackStep();
            }
            else if (commode == ComMode.Move)
            {
                await Move();
            }
            else if(commode == ComMode.Attack)
            {
                await MassiveAttack();
            }
        }
    }
    private async UniTask SearchPT()
    {
        FLAGS[0] = false;
        PLList.Clear();
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players)
        {
            PLList.Add(player);
        }
        if (PLList.Count > 1)
        {
            PLList.Sort(delegate (GameObject a, GameObject b)
            {
                return Vector3.Distance(Rb.position, a.transform.position)
                .CompareTo(
                  Vector3.Distance(Rb.position, b.transform.position));
            });
        }
        if (PLList.Count > 0)
        {
            PT = PLList[0].transform;
        }
        FLAGS[0] = true;
    }
    private async UniTask Move()
    {
        float dis = Vector3.Distance(PT.position,Rb.position);
        if (dis > 4)
        {
            transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
            Rb.velocity = transform.forward * MoveSpeed;
            if (FLAGS[1])
            {
                Anim.CrossFade("Run", 0.14f);
                FLAGS[1] = false;
            }
        }
        else
        {
            Anim.Play("Idla");
            commode = ComMode.Attack;
            FLAGS[1] = true;
        }
    }
    private async UniTask MassiveAttack()
    {
        AnimatorStateInfo ASI = Anim.GetCurrentAnimatorStateInfo(0);
        if (FLAGS[1])
        {
            ES.WATK = true;
            if(PT != null)
            {
                transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
            }
            if (!Anim.applyRootMotion)
            {
                Anim.applyRootMotion = true;
            }      
            Anim.Play("Atck01");
            FLAGS[1] = false;
        }
        else if(FLAGS[2] && !FLAGS[1] && ASI.normalizedTime > 1)                     
        {
            if (Anim.applyRootMotion)
            {
                Anim.applyRootMotion = false;
            }            
            Anim.Play("Atck02");
            Rb.velocity = transform.forward * 5.6f;
            FLAGS[2] = false;
        }
        else if (FLAGS[3] && !FLAGS[2] && ASI.normalizedTime > 1)
        {
            Anim.Play("Atck03");
            Rb.velocity = transform.forward * 5.6f;
            FLAGS[3] = false;
        }
        else if(!FLAGS[3] && ASI.normalizedTime > 1)
        {
            commode = ComMode.Back;
            ES.WATK = false;
            FLAGS[1] = true;
            FLAGS[2] = true;
            FLAGS[3] = true;
        }
        //Debug.Log(ASI.normalizedTime);
    }
    private async UniTask BackStep()
    {
        if (FLAGS[1])
        {
            t = 0;
            RaycastHit hit;
            Ray ray = new Ray(Rb.position, transform.forward * -1);
            if (Physics.Raycast(ray, out hit, 5f))
            {
                if (hit.collider.gameObject.activeSelf)
                {
                    FLAGS[2] = false;
                }
            }
            BSSpos = Rb.position;
            BSEpos = Rb.position + (transform.forward * -5f);
            FLAGS[1] = false;
        }
        if (FLAGS[2])
        {
            if (FLAGS[3])
            {
                Anim.CrossFade("BackStep", 0.16f);
                Rb.isKinematic = true;
                FLAGS[3] = false;
            }
            float angle = Mathf.Deg2Rad * 180 * t;
            Vector3 pos = Vector3.Lerp(BSSpos, BSEpos, t);
            if (t <= 1f)
            {
                Rb.position = new Vector3(pos.x, BSSpos.y + (Mathf.Sin(angle) * JumpGain), pos.z);
                t += 0.1f * 9 * Time.deltaTime;
            }
            else
            {
                if (Rb.isKinematic)
                {
                    Rb.isKinematic = false;
                    Anim.CrossFade("Idle", 0.1f);
                }
                if (isGround)
                {
                    commode = ComMode.Attack;
                    FLAGS[1] = true;
                    FLAGS[2] = true;
                    FLAGS[3] = true;
                }
            }
        }
        else
        {
            commode = ComMode.Attack;
            FLAGS[1] = true;
            FLAGS[2] = true;
        }
    }
    private async UniTask HummerStart()
    {
        EWeapon = Instantiate(HummerPrefab, HummerPoint.position, HummerPoint.rotation);
        EWeapon.GetComponent<EW_TEST02>().Ts = HummerPoint;
    }
    private async UniTask IsGround()
    {
        isGround = false;
        RaycastHit hit;
        Ray ray = new Ray(Rb.position + new Vector3(0,0.3f,0), Vector3.down);
        if(Physics.Raycast(ray ,out hit, 1.2f))
        {
            if (hit.collider.gameObject.activeSelf)
            {
                isGround = true;
            }
        }
        if(isGround && FLAGS[5])
        {
            FLAGS[5] = false;
        }
        //Debug.Log(isGround.ToString());
        //Debug.DrawRay(Rb.position + new Vector3(0, 0.3f, 0), Vector3.down,Color.red,1.2f);
    }
    private async UniTask ChComMove()
    {
        Anim.Play("Idle");
        commode = ComMode.Move;
        FLAGS[1] = true;
        FLAGS[2] = true;
        FLAGS[3] = true;
        FLAGS[6] = true;
    }
    private async UniTask HasBushed()
    {
        //Debug.Log("LLL");       
        if (Rb.isKinematic)
        {
            Rb.isKinematic = false;
        }
        Anim.Play("Damage");
        Rb.velocity = ES.BushDirection * 8f;
        commode = ComMode.Nothing;
        Invoke("ChComMove", 1.0f);
        FLAGS[6] = false;
        ES.BashF = false;
    }
    private async UniTask OnCollisionEnter(Collision collision)
    {

    }
}
