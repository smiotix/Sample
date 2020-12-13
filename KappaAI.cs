using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using TMPro.Examples;

public class KappaAI : MonoBehaviour
{
    private enum UType
    {
        Hummer,
        Shooter01,
        Guner,
        Bomber
    }
    private enum Com
    {
        Move,
        Attack,
        Idle
    }
    private Transform PT;
    private Rigidbody Rb;
    private Animator Anim;
    private bool isGround;
    private AnimatorStateInfo AS;
    private Vector3 Tpos;
    [System.NonSerialized]
    public bool[] FLAGS;
    private int FLAGS_NUM = 5;
    [SerializeField]
    private UType utype;
    [SerializeField]
    private Transform weapongenpoint;
    [SerializeField]
    private GameObject HummerPrefab;
    [SerializeField]
    private float FrontRayDis;
    private float MoveTimer;
    [SerializeField]
    private float MoveSpeed;
    private Com Command;
    [SerializeField]
    private bool RandomUtype;
    private Vector3 pos;
    private float Move03dis = 0;
    [SerializeField]
    private Transform ShootingPoint;
    [SerializeField]
    private float ShotSpeed01 = 2000;
    [SerializeField]
    private GameObject Shot01Prefab;
    [SerializeField]
    private GameObject BombPrefab;
    [SerializeField]
    private float MovingTime;
    [SerializeField]
    private float TposDis = 3.5f;
    [SerializeField]
    private GameObject WaterGun;
    private WaterGun WG;
    private string MoveF;
    private EnemyStatus ES;
    private GameObject Weapon;
    // Start is called before the first frame update
    private async UniTask Start()
    {
        int type = 1;
        if (RandomUtype)
        {
            type = UnityEngine.Random.Range(0, 20);
            if (type < 7)
            {
                //utype = UType.Hummer;
            }
            else if (type > 6 && type < 11)
            {
                utype = UType.Shooter01;
            }
            else if (type > 10 && 16 < type)
            {
                utype = UType.Guner;
            }
            else if (type > 15 && type < 18)
            {
                utype = UType.Bomber;
            }
            else
            {
                utype = UType.Hummer;
            }
        }
        if (utype != UType.Guner)
        {
            WaterGun.SetActive(false);
        }
        int m = UnityEngine.Random.Range(0, 3);
        if (m == 0)
        {
            MoveF = "Move_A";
        }
        else if (m == 1)
        {
            MoveF = "Move_B";
        }
        else if (m == 2)
        {
            MoveF = "Move_C";
        }
        FLAGS = new bool[FLAGS_NUM];
        for (int i = 0; i < FLAGS.Length; i++)
        {
            FLAGS[i] = true;
        }
        Rb = GetComponent<Rigidbody>();
        Anim = GetComponent<Animator>();
        ES = GetComponent<EnemyStatus>();
    }

    // Update is called once per frame
    private async UniTask Update()
    {
        SerchPlayer().Forget();
        //Debug.Log(utype.ToString() + " " + MoveF);
        await IsGround();
        await WatchHP();
        if (isGround && utype == UType.Hummer)
        {
            await Hummer();
        }
        else if (isGround && utype == UType.Shooter01)
        {
            await Shooter01();
        }
        else if (isGround && utype == UType.Guner)
        {
            await Guner();
        }
        else if (isGround && utype == UType.Bomber)
        {
            await Bomber();
        }
        //Debug.Log(Command.ToString() + " " + FLAGS[1].ToString() + " " + MoveTimer.ToString() + "" + FLAGS[2].ToString() + " " + FLAGS[3].ToString());
    }
    private async UniTask SerchPlayer()
    {
        GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
        float dis = 9999;
        for(int i = 0; i < Players.Length; i++)
        {
            float tdis = Vector3.Distance(Rb.position, Players[i].transform.position);
            if(tdis < dis)
            {
                dis = tdis;
                PT = Players[i].transform;
            }
        }
    }
    private async UniTask WatchHP()
    {
        if (ES != null)
        {
            if (ES.HP < 0.0f)
            {
                if (Weapon != null)
                {
                    Destroy(Weapon);
                }
                Destroy(this.gameObject);
            }
        }
        else
        {
            ES = GetComponent<EnemyStatus>();
        }
        if (transform.position.y < -2)
        {
            if (Weapon != null)
            {
                Destroy(Weapon);
            }
            Destroy(this.gameObject);
        }
    }
    private async UniTask Hummer()
    {
        if (!Rb.freezeRotation)
        {
            Rb.freezeRotation = true;
        }
        if (FLAGS[0])
        {
            GameObject Hummer = (GameObject)Instantiate(HummerPrefab, weapongenpoint.position, weapongenpoint.rotation);
            EW_TEST01 EW01 = Hummer.GetComponent<EW_TEST01>();
            EW01.body = this.gameObject;
            EW01.bone = weapongenpoint.transform.parent;
            Weapon = Hummer;
            Command = Com.Move;
            FLAGS[0] = false;
        }
        else
        {
            if (Command == Com.Move)
            {
                StartCoroutine(MoveF);
            }
            else if (Command == Com.Attack)
            {
                await Attack01();
            }
            else if (Command == Com.Idle)
            {
                await Idle01();
            }
        }

    }
    private async UniTask Bomber()
    {
        if (!Rb.freezeRotation)
        {
            Rb.freezeRotation = true;
        }
        if (FLAGS[0])
        {
            int i = UnityEngine.Random.Range(1, 3);
            if (i == 1)
            {
                Command = Com.Move;
            }
            else if (i == 2)
            {
                Command = Com.Attack;
            }
            FLAGS[0] = false;
        }
        if (Command == Com.Move)
        {
            StartCoroutine(MoveF);
        }
        else if (Command == Com.Attack)
        {
            await Shot02();
        }
        else if (Command == Com.Idle)
        {
            await Idle01();
        }
    }
    private async UniTask Shooter01()
    {
        if (!Rb.freezeRotation)
        {
            Rb.freezeRotation = true;
        }
        if (FLAGS[0])
        {
            int i = UnityEngine.Random.Range(1, 3);
            if (i == 1)
            {
                Command = Com.Move;
            }
            else if (i == 2)
            {
                Command = Com.Attack;
            }
            FLAGS[0] = false;
        }
        if (Command == Com.Move)
        {
            StartCoroutine(MoveF);
            //his.GetType().GetMethod(MoveF, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).Invoke(this, null);
        }
        else if (Command == Com.Attack)
        {
            await Shot01();
        }
        else if (Command == Com.Idle)
        {
            await Idle01();
        }
    }
    private async UniTask Guner()
    {
        //Debug.Log(Command.ToString() +" "+ MoveF + " " + PT.gameObject.name);
        if (!Rb.freezeRotation)
        {
            Rb.freezeRotation = true;
        }
        if (FLAGS[0])
        {
            Command = Com.Move;
            if (!WaterGun.activeSelf)
            {
                WaterGun.SetActive(true);
            }

            WG = WaterGun.GetComponent<WaterGun>();
            if (WG != null)
            {
                WG.Body = this.gameObject;
                FLAGS[0] = false;
            }
        }
        if (Command == Com.Move)
        {
            StartCoroutine(MoveF);
        }
        else if (Command == Com.Attack)
        {
            await GunShoot();
        }
        else if (Command == Com.Idle)
        {
            await Idle01();
        }
    }
    private async UniTask IsGround()
    {
        RaycastHit hit;
        Ray ray = new Ray(Rb.position + new Vector3(0, 0.3f, 0), Vector3.down);
        if (Physics.Raycast(ray, out hit, 1f))
        {
            if (hit.collider.CompareTag("Wall"))
            {
                isGround = true;
            }
            else
            {
                isGround = false;
            }
        }
    }
    private async UniTask BulletBomb()
    {
        GameObject bomb = (GameObject)Instantiate(BombPrefab, ShootingPoint.position, transform.rotation);
    }
    private async UniTask Bullet01()
    {
        GameObject shot01 = (GameObject)Instantiate(Shot01Prefab, ShootingPoint.position, transform.rotation);
        Rigidbody SRb = shot01.GetComponent<Rigidbody>();
        SRb.AddForce(transform.forward * ShotSpeed01);
        Destroy(shot01, 3.0f);
    }
    private async UniTask Shot01()
    {
        transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
        if (Anim.GetBool("is_run"))
        {
            Anim.SetBool("is_run", false);
            Rb.isKinematic = true;
            Rb.isKinematic = false;
        }
        if (FLAGS[1])
        {
            Anim.Play("Shot");
            FLAGS[1] = false;
        }
        AnimatorStateInfo AS = Anim.GetCurrentAnimatorStateInfo(0);
        if (AS.fullPathHash == -1107255104)
        {
            if (AS.normalizedTime > 0.7f && FLAGS[2])
            {
                await Bullet01();
                Invoke("AfterShot01", 1.2f);
                FLAGS[2] = false;
            }
        }

    }
    private async UniTask Shot02()
    {
        transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
        if (Anim.GetBool("is_run"))
        {
            Anim.SetBool("is_run", false);
            Rb.isKinematic = true;
            Rb.isKinematic = false;
        }
        if (FLAGS[1])
        {
            Anim.Play("Shot");
            FLAGS[1] = false;
        }
        AnimatorStateInfo AS = Anim.GetCurrentAnimatorStateInfo(0);
        if (AS.fullPathHash == -1107255104)
        {
            if (AS.normalizedTime > 0.7f && FLAGS[2])
            {
                await BulletBomb();
                Invoke("AfterShot01", 1.2f);
                FLAGS[2] = false;
            }
        }

    }
    private async UniTask AfterShot01()
    {
        Anim.Play("Idle");
        FLAGS[1] = true;
        FLAGS[2] = true;
        Command = Com.Move;
    }
    private async UniTask Attack01()
    {
        if (FLAGS[2])
        {
            if (FLAGS[3])
            {
                MoveTimer = MovingTime;
                FLAGS[3] = false;
            }
            else
            {
                //AnimatorStateInfo AS = Anim.GetCurrentAnimatorStateInfo(0);
                transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
                float dis = Vector3.Distance(Rb.position, PT.position);
                if (dis > 1.5f && FLAGS[1])
                {
                    if (!Anim.GetBool("is_run"))
                    {
                        Anim.SetBool("is_run", true);
                    }
                    await AvoidObs();
                    Rb.velocity = transform.forward * MoveSpeed;
                    MoveTimer -= 0.1f * 60 * Time.deltaTime;
                }
                else if (MoveTimer < 0.0f)
                {
                    Command = Com.Move;
                    FLAGS[3] = true;
                }
                else
                {
                    if (Anim.GetBool("is_run"))
                    {
                        Anim.SetBool("is_run", false);
                    }
                    if (FLAGS[1])
                    {
                        FLAGS[1] = false;
                        Rb.isKinematic = true;
                        Rb.isKinematic = false;
                        Anim.Play("Attack01");
                        Invoke("Attack01Off", 1.4f);
                    }
                }
            }
        }
        else
        {
            float dis = Vector3.Distance(Rb.position, pos);
            if (MoveTimer < 0f || dis < 1.28)
            {
                int i = UnityEngine.Random.Range(0, 3);
                if (i < 2)
                {
                    Command = Com.Move;
                }
                else
                {
                    Command = Com.Attack;
                }
                FLAGS[2] = true;
                FLAGS[1] = true;
            }
            else
            {
                await AvoidObs();
                if (!Anim.GetBool("is_run"))
                {
                    Anim.SetBool("is_run", true);
                }
                MoveTimer -= 0.1f * 60 * Time.deltaTime;
                Rb.velocity = transform.forward * MoveSpeed;
            }
        }
    }
    private async UniTask Idle01()
    {
        if (FLAGS[1])
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
            }
            MoveTimer = 1f;
            FLAGS[1] = false;
        }
        else
        {
            MoveTimer -= 0.1f * 60 * Time.deltaTime;
        }
        if (MoveTimer > 0.9f && FLAGS[4])
        {
            FLAGS[4] = false;
        }
        else if (MoveTimer < 0.7f && !FLAGS[4])
        {
            FLAGS[4] = true;
        }
        else if (MoveTimer < 0.0f)
        {
            Command = Com.Attack;
            FLAGS[1] = true;
        }
    }
    private async UniTask Attack01Off()
    {
        Rb.isKinematic = true;
        Rb.isKinematic = false;
        RaycastHit hit;
        Ray ray = new Ray(Rb.position, transform.forward * -1);
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.gameObject.activeSelf)
            {
                pos = Vector3.Lerp(Rb.position, new Vector3(hit.collider.transform.position.x, Rb.position.y, hit.collider.transform.position.z), 0.5f);
            }
            else
            {
                pos = transform.forward * -50f;
            }
        }
        transform.LookAt(pos);
        MoveTimer = MovingTime;
        FLAGS[2] = false;
    }
    private async UniTask GunShoot()
    {
        if (WG != null)
        {
            if (Anim.GetBool("is_run"))
            {
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            if (FLAGS[1])
            {
                MoveTimer = -1;
                Anim.Play("Gun");
                await WG.Trigger();
                MoveTimer = 15f;
                FLAGS[1] = false;
            }
            if (MoveTimer < 0.0f)
            {
                await WG.Off();
                Anim.Play("Idle");
                FLAGS[1] = true;
                Command = Com.Move;
            }
            else
            {
                transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
            }

            MoveTimer -= 0.1f * 60 * Time.deltaTime;
        }
        else
        {
            WG = WaterGun.GetComponent<WaterGun>();
        }
    }
    private async UniTask Move01()
    {
        if (FLAGS[1])
        {
            RaycastHit[] hit = new RaycastHit[4];
            Vector3[] Pdir = new Vector3[4];
            Ray[] rays = new Ray[4];
            Vector3 ray_center = PT.position + new Vector3(0, 0.4f, 0);
            rays[0] = new Ray(ray_center, Vector3.forward);
            rays[1] = new Ray(ray_center, Vector3.back);
            rays[2] = new Ray(ray_center, Vector3.right);
            rays[3] = new Ray(ray_center, Vector3.left);
            for (int i = 0; i < 4; i++)
            {
                if (Physics.Raycast(rays[i], out hit[i], 100f))
                {
                    if (hit[i].collider.gameObject.activeSelf)
                    {
                        Pdir[i] = hit[i].point;
                    }
                }
            }
            float dis0 = 0f;
            Vector3 tpos = PT.position;
            for (int i = 0; i < Pdir.Length; i++)
            {
                float tdis = Vector3.Distance(Rb.position, Pdir[i]);
                if (dis0 < tdis)
                {
                    tpos = Pdir[i];
                    dis0 = tdis;
                }
            }
            Tpos = Vector3.Lerp(Rb.position, new Vector3(tpos.x, Rb.position.y, tpos.z), 0.7f);
            MoveTimer = MovingTime;
            FLAGS[1] = false;
        }
        transform.LookAt(Tpos);
        float dis = Vector3.Distance(Rb.position, Tpos);
        if ((dis > TposDis) && !FLAGS[1])
        {
            if (!Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", true);
            }
            await AvoidObs();
            Rb.velocity = transform.forward * MoveSpeed;
        }
        else if (MoveTimer < 0.0f)
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            Command = Com.Attack;
            FLAGS[1] = true;
        }
        else
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            Command = Com.Attack;
            FLAGS[1] = true;
        }
        MoveTimer -= 0.1f * 60 * Time.deltaTime;
    }
    private async UniTask Move02()
    {
        if (FLAGS[1])
        {
            transform.LookAt(new Vector3(PT.position.x, Rb.position.y, PT.position.z));
            Tpos = Vector3.zero;
            RaycastHit hit;
            Ray ray = new Ray(Rb.position + new Vector3(0, 1, 0), transform.forward * -1);
            if (Physics.Raycast(ray, out hit, 80f))
            {
                if (hit.collider.gameObject.activeSelf)
                {
                    Tpos = Vector3.Lerp(Rb.position, new Vector3(hit.point.x, Rb.position.y, hit.point.z), 0.6f);
                }
            }
            float dis0 = Vector3.Distance(Tpos, Rb.position);
            if (dis0 < 5f)
            {
                ray = new Ray(PT.position + new Vector3(0, 1, 0), (transform.forward + transform.right).normalized * -1);
                if (Physics.Raycast(ray, out hit, 80f))
                {
                    if (hit.collider.gameObject.activeSelf)
                    {
                        Tpos = Vector3.Lerp(Rb.position, new Vector3(hit.point.x, Rb.position.y, hit.point.z), 0.8f);
                    }
                }
            }
            MoveTimer = MovingTime;
            FLAGS[1] = false;
            if (Tpos == Vector3.zero)
            {
                Command = Com.Attack;
                FLAGS[1] = true;
            }
        }
        transform.LookAt(Tpos);
        float dis = Vector3.Distance(Rb.position, Tpos);
        if ((dis > TposDis) && !FLAGS[1])
        {
            if (!Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", true);
            }
            await AvoidObs();
            Rb.velocity = transform.forward * MoveSpeed;
        }
        else if (MoveTimer < 0.0f)
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            Command = Com.Attack;
            FLAGS[1] = true;
        }
        else
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            Command = Com.Attack;
            FLAGS[1] = true;
        }
        MoveTimer -= 0.1f * 60 * Time.deltaTime;
    }
    private async UniTask Move03()
    {
        if (FLAGS[1])
        {
            Vector3 pos = new Vector3(PT.position.x, Rb.position.y, PT.position.z);
            int i = UnityEngine.Random.Range(1, 3);
            if (i == 2)
            {
                pos = pos + new Vector3(8, 0, 0);
            }
            else
            {
                pos = pos + new Vector3(-8, 0, 0);
            }
            RaycastHit hit;
            if(Physics.Linecast(Rb.position + new Vector3(0,0.5f,0),Vector3.Lerp(Rb.position + new Vector3(0, 0.5f, 0),pos + new Vector3(0,0.5f,0),0.7f),out hit))
            {
                if (hit.collider.gameObject.activeSelf)
                {
                    pos = new Vector3(PT.position.x, Rb.position.y, PT.position.z);
                }
            }
            Tpos = pos;
            MoveTimer = MovingTime;
            FLAGS[1] = false;
        }
        transform.LookAt(Tpos);
        float dis = Vector3.Distance(Rb.position, Tpos);
        if ((dis > TposDis) && !FLAGS[1])
        {
            if (!Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", true);
            }
            await AvoidObs();
            Rb.velocity = transform.forward * MoveSpeed;
        }
        else if (MoveTimer < 0.0f)
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            Command = Com.Attack;
            FLAGS[1] = true;
        }
        else
        {
            if (Anim.GetBool("is_run"))
            {
                Anim.SetBool("is_run", false);
                Rb.isKinematic = true;
                Rb.isKinematic = false;
            }
            Command = Com.Attack;
            FLAGS[1] = true;
        }
        MoveTimer -= 0.1f * 60 * Time.deltaTime;
    }
    private async UniTask AvoidObs()
    {
        RaycastHit hit;
        Ray ray = new Ray(Rb.position + new Vector3(0, 0.5f, 0), transform.forward);
        if (Physics.Raycast(ray, out hit, FrontRayDis))
        {
            if (hit.collider.gameObject.activeSelf && !hit.collider.gameObject.CompareTag("Player"))
            {
                Vector3 Front = new Vector3(hit.normal.x, 0, hit.normal.z) + transform.forward;
                transform.rotation = Quaternion.LookRotation(Front);
            }
        }
    }
    private async UniTask OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("PlayerWeapon") && FLAGS[4])
        {
            Anim.SetTrigger("is_damage");
            Command = Com.Idle;
            FLAGS[1] = true;
            FLAGS[2] = true;
            FLAGS[3] = true;
        }
    }
    private async UniTask Move_A01()
    {
        await Move01();
    }
    IEnumerator Move_A()
    {
        Move_A01();
        yield break;
    }
    private async UniTask Move_B01()
    {
        await Move02();
    }
    IEnumerator Move_B()
    {
        Move_B01();
        yield break;
    }
    private async UniTask Move_C01()
    {
        await Move03();
    }
    IEnumerator Move_C()
    {
        Move_B01();
        yield break;
    }
}
