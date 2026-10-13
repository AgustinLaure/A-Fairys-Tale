using System.Collections;
using UnityEngine;

public class Teletransportable : MonoBehaviour
{
    [SerializeField] private Transform tpTransform = null;

    [SerializeField] private Rigidbody rb;

    private LevelManager levelManager;
    
    public float telelporResetTime = 0.5f;
    
    public int layerIndex = -1;
    public int LayerIndex { get { return layerIndex; } }


    private bool telelportReady = true;
    private bool reseting = false;

    void Awake()
    {
        levelManager = ServiceLocator.Instance.GetService<LevelManager>();
    }
    private void Start()
    {
        if (!tpTransform) tpTransform = transform;

        if (!rb) rb = tpTransform.GetComponent<Rigidbody>();

        if (!rb) Debug.LogWarning("No attached rigidbody to teleportable!");
    }
    public void SetLayer(int newIndex)
    {
        layerIndex = newIndex;
         //tpTransform.SetParent(levelManager.Layers[layerIndex].transform);
    }

    public void EnterPortal(LayerPortal portal)
    {
        if (!telelportReady) return;
        telelportReady = false;

        Vector3 newPos = portal.target.transform.position;

        newPos.y += portal.transform.position.y - tpTransform.position.y;

        tpTransform.position = newPos;

        if (portal.flipSpeedOnTeleport)
        {
            if (!rb)
            {
                Debug.LogError("tried flpping teleportable but no rigidbody was found!");
                return;
            }
            rb.linearVelocity *= new Vector2(-1.0f,1.0f);
        }

        if (portal.multiplySpeedOnTeleport != 1.0f)
        {
            if (!rb)
            {
                Debug.LogError("tried speeding up teleportable but no rigidbody was found!");
                return;
            }
            rb.linearVelocity *= portal.multiplySpeedOnTeleport;
        }

        SetLayer(portal.target.LayerIndex);

        //spriteFader.FadeFromLayer(LayerIndex);

        ServiceLocator.Instance.GetService<GameManager>().levelManager.SetCurrentLayer(portal.target.LayerIndex);
    }

    public void ExitPortal(LayerPortal portal)
    {
        if (telelportReady) return;
        if (!reseting) StartCoroutine(ResetTeleport());
        reseting = true;
    }

    IEnumerator ResetTeleport()
    {
        yield return new WaitForSeconds(telelporResetTime);

        telelportReady = true;
        reseting = false;
    }
}
