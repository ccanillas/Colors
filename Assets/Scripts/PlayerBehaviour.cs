﻿using UnityEngine;
using System.Collections;
using ColorType;
using UnityEngine.Experimental.Rendering.Universal;

public class PlayerBehaviour : MonoBehaviour
{
    public bool m_GODMODE = false;
    private ShootManager m_shoot;
    public bool m_dead = false;
    private float m_slowMo = 1f;
    public float m_moveSpeed = 5f;
    private float m_fixedDeltaTime = 0.02f;
    private Rigidbody2D m_myRigidbody;
    private Camera m_worldCamera;
    private Vector2 m_movement;
    private Vector2 m_mousePosition;
    
    private SpriteRenderer m_sprite;

    private ColorType.ColorType.Type m_type = ColorType.ColorType.Type.White;

    private Light2D m_halo;

    public int m_ammo;
    private UIShowAmmo m_uiAmmo;
    private UISlowMo m_uiSlowMo;

    private UIDeath m_uiDeath;
    

    //private Vector2 m_stopPushing = Vector2.zero;

    

    void Start()
    {
        m_halo = transform.GetChild(0).gameObject.GetComponent<Light2D>();
        m_myRigidbody = GetComponent<Rigidbody2D>();
        m_worldCamera = Camera.main;
        m_sprite = GetComponent<SpriteRenderer>();
        m_ammo = ColorType.ColorType.m_associatedAmmo[(int)m_type];
        m_uiAmmo.UpdateAmmo(m_ammo, ColorType.ColorType.m_associatedColor[(int)m_type]);
        m_slowMo = 1f;
        m_uiSlowMo.UpdateColor(ColorType.ColorType.m_associatedColor[(int)m_type]);
        m_shoot = GetComponent<ShootManager>();
    }



    void FixedUpdate()
    {
        m_movement.x = Input.GetAxisRaw("Horizontal");
        m_movement.y = Input.GetAxisRaw("Vertical");

        m_mousePosition = m_worldCamera.ScreenToWorldPoint(Input.mousePosition);

        Move();
        Aim();
        TryShoot();
        SlowMo();        
    }

    private void TryShoot()
    {
        if(m_ammo > 0)
        {
            if(Input.GetButtonDown("Fire1") && !ColorType.ColorType.m_associatedAutomatic[(int)m_type])
            {
                if(m_shoot.Shoot(m_type))
                {
                    m_ammo--;
                    m_uiAmmo.UpdateAmmo(m_ammo);
                }
            }
            else if (Input.GetButton("Fire1") && ColorType.ColorType.m_associatedAutomatic[(int)m_type])
            {
                if(m_shoot.Shoot(m_type))
                {
                    m_ammo--;
                    m_uiAmmo.UpdateAmmo(m_ammo);
                }
            } 
        }
    }

    private void SlowMo()
    {
        if(Input.GetButton("SlowMo") && m_slowMo > 0f)
        {
            Time.timeScale = 0.3f;
            m_slowMo = m_slowMo - Time.unscaledDeltaTime * 0.5f;
            m_uiSlowMo.UpdateValue(m_slowMo);
        }
        else
        {
            Time.timeScale = 1f;
            if(m_slowMo < 1f)
            {
                m_slowMo = m_slowMo + Time.unscaledDeltaTime * 0.1f;
                m_uiSlowMo.UpdateValue(m_slowMo);
            }
        }
        Time.fixedDeltaTime = m_fixedDeltaTime * Time.timeScale;
    }

    private void OnCollisionEnter2D(Collision2D collider)
    {
        if(collider.gameObject.tag == "Enemy")
        {
            StealColor(collider.gameObject);
        }
        else if(collider.gameObject.tag == "Bullet")
        {
            Die();
        }
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.tag == "Enemy")
        {
            StealColor(collider.gameObject);
            collider.gameObject.GetComponent<EnemyBehaviour>().Delete();
        }
    }


    private void Move()
    {
        m_myRigidbody.MovePosition(m_myRigidbody.position + m_movement * m_moveSpeed * Time.fixedDeltaTime);
        m_myRigidbody.velocity = Vector2.zero;
    }

    private void Aim()
    {
        Vector2 lookDirection = m_mousePosition - m_myRigidbody.position;
        m_myRigidbody.rotation = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
    }

    private void Die()
    {
        if(!m_dead && !LevelManager.Instance.m_levelFinished && !m_GODMODE)
        {
            m_dead = true;
            this.enabled = false;
            m_sprite.enabled = false;
            LevelManager.Instance.m_levelFinished = true;
            transform.GetChild(0).gameObject.SetActive(false); // C MOCHE
            transform.GetChild(2).gameObject.SetActive(false);
            m_uiAmmo.gameObject.SetActive(false);
            m_uiDeath.Enable();
        }
    }
    private void StealColor(GameObject enemy)
    {
        ChangeColor(enemy.GetComponent<EnemyBehaviour>().m_type);
        enemy.GetComponent<EnemyBehaviour>().ChangeColor(ColorType.ColorType.Type.Black);
        m_shoot.m_canShoot = true;
    }

    public void ChangeColor(ColorType.ColorType.Type newColor)
    {
        m_sprite.color = ColorType.ColorType.m_associatedColor[(int)newColor];
        m_halo.color = ColorType.ColorType.m_associatedColor[(int)newColor];
        m_ammo = ColorType.ColorType.m_associatedAmmo[(int)newColor];
        m_uiAmmo.UpdateAmmo(m_ammo,ColorType.ColorType.m_associatedColor[(int)newColor]);
        m_slowMo = 1f;
        m_uiSlowMo.UpdateColor(ColorType.ColorType.m_associatedColor[(int)newColor]);
        m_type = newColor;
    }

    public void SetAmmoUi(UIShowAmmo ui)
    {
        m_uiAmmo = ui;
    }

    public void SetSlowMoUi(UISlowMo ui)
    {
        m_uiSlowMo = ui;
    }

    public void SetDeathUI(UIDeath ui)
    {
        m_uiDeath = ui;
    }

    
}
