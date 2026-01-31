using UnityEngine;

public class TopDownCameraSystem : MonoBehaviour
{
    [Header("TARGET SETTINGS")]
    [SerializeField] private Transform player_target;

    [Header("OFFSET SETTINGS")]
    [SerializeField] private Vector3 offSet = new Vector3(0f, 20f, -20f);

    [Header("ANGLES SETTINGS")]
    [SerializeField] private float angleX = 50f;
    [SerializeField] private float angleY = 50f;

    private void Start()
    {
        if (player_target == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player_target = player.transform;
            }
            else
            {
                return;
            }
        }

        // Top-Down angle -> 
        transform.rotation = Quaternion.Euler(angleX, angleY, 0f);
    }

    private void Update()
    {
        if (player_target == null){
        var player = GameObject.FindGameObjectWithTag("Player");
        if(player == null)
        {
            player_target = player.transform;
        }
        return;
        }

        transform.rotation = Quaternion.Euler(angleX, angleY, 0f);

        // The camera desired position.
        Vector3 cameraPosDesired = player_target.position + offSet + player_target.forward;
        transform.position = cameraPosDesired;    
        }
}