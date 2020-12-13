using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Security.Cryptography;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Runtime.CompilerServices;

public class Gate : MonoBehaviour
{
    public enum LockType
    {
        KEY,
        ENEMY,
        None,
    }
    [System.NonSerialized]
    public LockType LKT;
    private Vector3 Rspos;
    private Vector3 Lspos;
    private Vector3 Repos;
    private Vector3 Lepos;
    private GameObject LeftDoor;
    private GameObject RightDoor;
    private float OpenSpeed = 60;
    private float CloseSpeed = 60;
    private Vector3 LFpos;
    private Vector3 RFpos;
    [System.NonSerialized]
    public bool Lock = false;
    private int GateMode = 0;
    private BoxCollider LeftBC;
    private BoxCollider RightBC;
    private float Range = 10;
    float t = 0;
    private bool StartFlag = false;
    private GameObject LockObj;
    [SerializeField]
    private Material KLockMat;
    [SerializeField]
    private Material ELockMat;
#if UNITY_EDITOR
    [CustomEditor(typeof(Gate))]
    public class ObjectGeneratorEditor : Editor
    {


        public override void OnInspectorGUI()
        {
            Gate obj = target as Gate;
            obj.OpenSpeed = EditorGUILayout.Slider("open speed", obj.OpenSpeed, 1, 1000);
            obj.CloseSpeed = EditorGUILayout.Slider("close speed", obj.CloseSpeed, 1, 1000);
            obj.Range = EditorGUILayout.Slider("Range", obj.Range, 1, 100);
            obj.KLockMat = EditorGUILayout.ObjectField("KeyLockMat", obj.KLockMat, typeof(Material), true) as Material;
            obj.ELockMat = EditorGUILayout.ObjectField("EnemyLockMat", obj.ELockMat, typeof(Material), true) as Material;
            EditorUtility.SetDirty(target);
        }
    }
#endif
    // Start is called before the first frame update
    public async UniTask Init()
    {
        LKT = LockType.None;
        LeftDoor = transform.Find("DoorLeft").gameObject;
        RightDoor = transform.Find("DoorRight").gameObject;
        LockObj = transform.Find("Lock").gameObject;
        //Debug.Log(LockObj.activeSelf);
        LockObj.SetActive(false);
        Rspos = RightDoor.transform.position;
        Lspos = LeftDoor.transform.position;
        Lepos = (transform.right * -5f) + LeftDoor.transform.position;
        Repos = (transform.right * 5f) + RightDoor.transform.position;
        StartFlag = true;
    }

    // Update is called once per frame
    private async UniTask Update()
    {

        if (GateMode == 1)
        {
            if (!Lock)
            {
                bool tFlag = OpenDoor().GetAwaiter().GetResult();
                if (tFlag)
                {
                    t = 0;
                    GateMode = 0;
                }
            }
            else
            {
                t = 0;
                GateMode = 2;
            }
        }
        if (GateMode == 2 && LeftDoor.transform.position != LFpos && RightDoor.transform.position != RFpos)
        {
            await CloseDoor();
        }
        else if (GateMode != 1 && GateMode != 0)
        {
            t = 0;
            GateMode = 0;
        }

    }
    private async UniTask<bool> OpenDoor()
    {
        //Debug.Log(Lspos.ToString() + " " + Repos.ToString());
        if (t <= 1.0f)
        {
            t += 0.1f * OpenSpeed * Time.deltaTime;
            Vector3 Lpos = Vector3.Lerp(Lspos, Lepos, t);
            Vector3 Rpos = Vector3.Lerp(Rspos, Repos, t);
            LeftDoor.transform.position = Lpos;
            RightDoor.transform.position = Rpos;
            return false;
        }
        return true;
    }
    private async UniTask CloseDoor()
    {
        if (t <= 1.0f)
        {
            t += 0.1f * CloseSpeed * Time.deltaTime;
            Vector3 Lpos = Vector3.Lerp(Lepos, Lspos, t);
            Vector3 Rpos = Vector3.Lerp(Repos, Rspos, t);
            LeftDoor.transform.position = Lpos;
            RightDoor.transform.position = Rpos;
        }
    }
    public async UniTask KeyLock()
    {
        LKT = LockType.KEY;
        if (LockObj != null)
        {
            LockObj.SetActive(true);
            Renderer Rnd = LockObj.GetComponent<Renderer>();
            Rnd.material = KLockMat;
            Lock = true;
        }
    }
    public async UniTask KeyOpen()
    {
        LKT = LockType.None;
        if (LockObj.activeSelf)
        {
            LockObj.SetActive(false);
        }
        Lock = false;
    }
    public async UniTask EnemyLock()
    {
        if (LockObj != null)
        {
            Invoke("ELActive", 1.0f);
        }
    }
    private async UniTask ELActive()
    {
        LockObj.SetActive(true);
        Renderer Rnd = LockObj.GetComponent<Renderer>();
        Rnd.material = ELockMat;
        Lock = true;
    }
    public async UniTask EnemyOpen()
    {
        LKT = LockType.None;
        if (LockObj.activeSelf)
        {
            LockObj.SetActive(false);
        }
        Lock = false;
    }
    private async UniTask OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (!Lock)
            {
                t = 0;
                GateMode = 1;
            }
        }
    }
    private async UniTask OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GateMode = 2;
            t = 0;
        }
    }
}
