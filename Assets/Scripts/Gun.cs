using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Bullet Prefab")]
    public GameObject bullet;

    [Header("Gun Parts")]
    public Transform muzzelPoint;

    [Header("Gun Stats")]
    public float bulletSpeed = 10f;
    public float damage = 2f;
    public int clipSize = 10;
    public float range = 2f;
    public float fireRate = .6f;
    public float reloadSpeed = 1f;
    public float defultSpred = 5f;

    [Header("Gun Settings")]
    public bool autoFire = false;
    public int bulletsAtOnes = 1;

    [Header("VFX")]
    public ParticleSystem muzzleFlash;
    public TextMeshProUGUI ammoText;
    public Image reloadImage;

    [Header("KeyBinds")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode reloadKey = KeyCode.R;


    int curretAmmo;

    float reloadTimeLeft;
    float currenSpred;

    bool canShoot = true;
    bool canReload = true;
    bool isReloading = false;

    AudioSource gunShot;

    GameObject UI;

    // Start is called before the first frame update
    void Start()
    {
        reloadTimeLeft = reloadSpeed;
        curretAmmo = clipSize;
        currenSpred = defultSpred;
        gunShot = GetComponent<AudioSource>();
        NetworkTransform nT = GetComponent<NetworkTransform>();

        UI = ammoText.gameObject.transform.parent.gameObject;
        UI.transform.parent = null;
        UI.SetActive(nT.isOwner);
    }

    // Update is called once per frame
    void Update()
    {
        if (canShoot)
        {
            if (autoFire)
            {
                Autofire();
            }
            else
            {
                Siglefire();
            }
        }

        if (canReload)
        {
            Reload();
        }

        ammoText.text = curretAmmo + " / " + clipSize;
        if (isReloading) 
        {
            reloadTimeLeft += Time.deltaTime;
            reloadImage.fillAmount = 1 - reloadTimeLeft / reloadSpeed;
        }
        else
        {
            reloadImage.fillAmount = 0;
            reloadTimeLeft = 0;
        }
    }

    private void LateUpdate()
    {
        UI.transform.position = transform.position + new Vector3(0f, 1f,-.3f);
    }

    private void Reload()
    {
        if (Input.GetKeyDown(reloadKey) && curretAmmo < clipSize)
        {
            StartCoroutine(ReloadTime(reloadSpeed));
        }
    }

    IEnumerator ReloadTime(float reloadSpeed)
    {
        canShoot = false;
        canReload = false;
        isReloading = true;
        yield return new WaitForSeconds(reloadSpeed);
        curretAmmo = clipSize;
        isReloading = false;
        canReload = true;
        canShoot = true;
    }

    private void Siglefire()
    {
        if (Input.GetKeyDown(fireKey) && curretAmmo > 0 && canShoot)
        {
            Shoot();
        }
    }

    private void Autofire()
    {
        if (Input.GetKey(fireKey) && curretAmmo > 0 && canShoot)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        gunShot.Stop();
        for (int i = 0; i < bulletsAtOnes; i++)
        {
            if (curretAmmo > 0)
            {
                float ofset = UnityEngine.Random.Range(-currenSpred / 2, currenSpred / 2);
                Quaternion muzzelRot = Quaternion.Euler(muzzelPoint.rotation.eulerAngles.x, muzzelPoint.rotation.eulerAngles.y + ofset, muzzelPoint.rotation.eulerAngles.z);
                NetworkServer.Instantiate("Bullet", muzzelPoint.position, muzzelRot);

                curretAmmo--;
            }
        }
        muzzleFlash.Play();
        gunShot.Play();
        
        StartCoroutine(Firerate(fireRate));
    }

    IEnumerator Firerate(float firerate)
    {
        canShoot = false;
        canReload = false;
        yield return new WaitForSeconds(firerate);
        canShoot = true;
        canReload = true;
    }

    public int GetCurretAmmo() { return curretAmmo; }

    public float GetReloadTimeLeft() { return reloadTimeLeft; }

    public void SetCanShoot(bool b) { canShoot = b; }
}
