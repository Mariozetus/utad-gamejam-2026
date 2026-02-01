using System.Collections;
using UnityEngine;

public class Horus : Enemy
{
    [SerializeField] protected GameObject bulletprefab;
    [SerializeField] protected GameObject specialbulletprefab;
    [SerializeField] protected float speed;
    [SerializeField] protected float Specialbulletspeed;

    // Cambiamos esto para que sea un contador simple
    [SerializeField] protected float specialCoolDownDuration = 30f;
    private float _specialTimer;

    private void Start()
    {
        // Inicializamos el timer para que el primer ataque especial tarde 30s
        _specialTimer = specialCoolDownDuration;

        // Si usas NavMeshAgent, recuerda desactivar la rotación automática
        // _agent.updateRotation = false; 
    }

    private void Update()
    {
        
        // 2. Lógica del Ataque Normal (Raycast)
        if (shotCoolDown <= 0)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attackrange, playerLayer))
            {
                Attack();
                shotCoolDown = startShotCoolDown;
            }
        }
        shotCoolDown -= Time.deltaTime;

        // 3. Lógica del Ataque Especial (Temporizador simple)
        _specialTimer -= Time.deltaTime;
        if (_specialTimer <= 0)
        {
            SpecialAttack();
            _specialTimer = specialCoolDownDuration; // Reinicia el ciclo de 30s
        }
    }

    protected override void Attack()
    {
        var bullet = Instantiate(bulletprefab, attackpoint.position, attackpoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = attackpoint.forward * speed;
        Debug.Log("Disparo normal");
    }

    protected void SpecialAttack()
    {
        var bullet = Instantiate(specialbulletprefab, attackpoint.position, attackpoint.rotation);
        bullet.GetComponent<Rigidbody>().linearVelocity = attackpoint.forward * Specialbulletspeed;
        Debug.Log("¡ATAQUE ESPECIAL!");
    }
}