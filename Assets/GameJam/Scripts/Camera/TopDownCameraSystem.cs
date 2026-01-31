using UnityEngine;

public class TopDownCameraSystem : MonoBehaviour
{
    [Header("TARGET SETTINGS")]
    [SerializeField] private Transform player_target;

    [Header("CAMERA SETTINGS")]
    [SerializeField] private Vector3 offSet = new Vector3(0f, 20f, -20f);

    [Header("TOP-DOWN CAMERA SETTINGS")]
    [SerializeField] private float angleX = 50f;
    [SerializeField] private float angleY = 50f;
    [SerializeField] private float pushCam = 0.35f; 

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

    private void LateUpdate()
    {
        if (player_target == null){
        var player = GameObject.FindGameObjectWithTag("Player");
        if(player == null)
        {
            player_target = player.transform;
        }
        return;
        }

        // The camera CANT rotate -> isometric view.
        transform.rotation = Quaternion.Euler(angleX, angleY, 0f);

        // The camera desired position.
        Vector3 cameraPosDesired = player_target.position + offSet + player_target.forward * pushCam;

        // The camera need an smooth delay -> Vector.Lerp -> from the initial position to the desired position.
        transform.position = Vector3.Lerp(transform.position, cameraPosDesired, Time.deltaTime);  
    }
}