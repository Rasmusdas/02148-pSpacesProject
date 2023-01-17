using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("GameObjects")]
    public Transform gunTip;

    [Header("Stats")]
    public float health = 10f;
    public float maxHealth = 10f;
    public float moveSpeed = 5f;
    public float maxSpeed = 10f;
    public float sprintMult = 1.8f;
    public int shielded = 0;
    public float fireratePistol = 0.5f;
    Animator anim;

    [Header("KeyBinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("VFXs")]
    public GameObject death;
    public Material shieldMat;
    public Image privateHealthBar;
    public Image publicHealthBar;

    [Header("Sound")]
    public AudioSource hitSound;
    
    //Material playerMat;
    Material playerMat2;
    //MeshRenderer meshRenderer;
    SkinnedMeshRenderer skinnedMeshRenderer;

    NetworkTransform nT;

    bool isSprinting = false;
    bool canShoot = true;

    Vector3 movement;

    CharacterController characterController;

    Camera cam;

    Gun gun;

    // Start is called before the first frame update
    void Start()
    {
        nT = GetComponent<NetworkTransform>();
        gun = GetComponent<Gun>();
        //meshRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        skinnedMeshRenderer = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        //playerMat = meshRenderer.material;
        playerMat2 = skinnedMeshRenderer.material;
        TryGetComponent<Animator>(out anim);
        if (!nT.isOwner) return;
        characterController = GetComponent<CharacterController>();
        cam = Camera.main;
        cam.GetComponent<CamController>().player = gameObject;
        //gunShot = GetComponent<AudioSource>();
        
        publicHealthBar.transform.parent.parent.gameObject.SetActive(!nT.isOwner);

    }

    // Update is called once per frame
    void Update()
    {
        privateHealthBar.fillAmount = health / maxHealth;
        publicHealthBar.fillAmount = health / maxHealth;

        if (!nT.isOwner) return;
        GetInputs();
        moveSpeed = Mathf.Min(maxSpeed, moveSpeed);
        Move();

    }


    private void FixedUpdate()
    {
        Rotate();
        if (health <= 0 && transform.position.y > 0)
        {
            transform.position = new Vector3(0, -3, 0);
            gun.GetUI().SetActive(false);
            gun.enabled = false;
        }
    }

    private void Rotate()
    {
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayLenght;

        if (groundPlane.Raycast(camRay, out rayLenght))
        {
            Vector3 lookAtPoint = camRay.GetPoint(rayLenght);
            lookAtPoint.y = transform.position.y;
            transform.LookAt(lookAtPoint);
            if (Vector3.Distance(transform.position, lookAtPoint) > 2)
            {
                gunTip.transform.LookAt(lookAtPoint + new Vector3(0, 1, 0));
            }
            
        }
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        movement = new Vector3(1, 0, 0) * x + new Vector3(0, 0, 1) * y;
        movement = movement.normalized;

        if (movement != Vector3.zero)
        {

            anim?.SetBool("Move", true);
            if (isSprinting)
            {
                movement *= moveSpeed * sprintMult;

            }
            else
            {
                movement *= moveSpeed;
            }
        }
        else
        {
            anim?.SetBool("Move", false);
        }

        movement = new Vector3(movement.x, -3, movement.z);

        movement *= Time.deltaTime;
        characterController.Move(movement);
    }

    private void GetInputs()
    {
        //Sprint Key check
        if (Input.GetKeyDown(sprintKey))
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp(sprintKey))
        {
            isSprinting = false;
        }
    }

    public void UpdateHealth(float dmg)
    {
        if (0 < shielded)
        {
            return;
        }

        if(dmg > 0)
        {
            hitSound.Play();
        }
        
        if(health - dmg > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health -= dmg;
        }
    }

    public void TakeDamge(int dmg)
    {
        NetworkServer.DamagePlayer(nT.id,dmg);

    }

    public void AddHealth(float health)
    {
        TakeDamge(-(int)health);
    }

    public void Shield(float time)
    {
        StartCoroutine(ShieldWaiter(time));
    }

    IEnumerator ShieldWaiter(float time)
    {
        shielded++;
        //meshRenderer.material = shieldMat;
        skinnedMeshRenderer.material = shieldMat;
        yield return new WaitForSeconds(time);
        shielded--;
        if (shielded == 0)
        {
           // meshRenderer.material = playerMat;
            skinnedMeshRenderer.material = playerMat2;
        }
    }

}
