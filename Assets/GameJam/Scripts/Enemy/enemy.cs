using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class enemy : MonoBehaviour
{
    public int health;
    public Transform attackpoint;
    public float attackrange = 0.5f;
    public LayerMask playerLayer;
    public float startShotCoolDown;
    public float shotCoolDown;

    private void Start()
    {
        shotCoolDown = startShotCoolDown;
    }

    void attack()
    {

    }

}
