using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public ParticleSystem muzzelFlash;

    [Header("KeyBinds")]
    public KeyCode fireKey = KeyCode.Mouse0;
    public KeyCode reloadKey = KeyCode.R;


    int curretAmmo;

    float reloadTimeLeft;
    float currenSpred;

    bool canShoot = true;

    ParticleSystem muzzleFlash;

    // Start is called before the first frame update
    void Start()
    {
        reloadTimeLeft = reloadSpeed;
        curretAmmo = clipSize;
        currenSpred = defultSpred;
        muzzleFlash = GetComponentInChildren<ParticleSystem>();
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


            Reload();

        }
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
        yield return new WaitForSeconds(reloadSpeed);
        curretAmmo = clipSize;
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
        for (int i = 0; i < bulletsAtOnes; i++)
        {
            float ofset = UnityEngine.Random.Range(-currenSpred / 2, currenSpred / 2);
            Quaternion muzzelRot = Quaternion.Euler(muzzelPoint.rotation.eulerAngles.x, muzzelPoint.rotation.eulerAngles.y + ofset, muzzelPoint.rotation.eulerAngles.z);
            NetworkServer.Instantiate("Bullet", muzzelPoint.position, muzzelRot);
        }
        muzzleFlash.Play();

        curretAmmo -= bulletsAtOnes;
        StartCoroutine(Firerate(fireRate));
    }

    IEnumerator Firerate(float firerate)
    {
        canShoot = false;
        yield return new WaitForSeconds(firerate);
        canShoot = true;
    }

    public int GetCurretAmmo() { return curretAmmo; }

    public float GetReloadTimeLeft() { return reloadTimeLeft; }

    public void SetCanShoot(bool b) { canShoot = b; }
}
