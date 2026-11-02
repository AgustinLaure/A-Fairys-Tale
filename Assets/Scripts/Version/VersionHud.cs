using TMPro;
using UnityEngine;

public class VersionHud : MonoBehaviour
{
    private string _version;
    [SerializeField]
    private TextMeshProUGUI _TextMeshProUGUI;

    private void Awake()
    {
        //DontDestroyOnLoad(this);
    }
    void Start()
    {
        _version = "v" + Application.version;

        if (_TextMeshProUGUI != null) _TextMeshProUGUI.text = _version;
    }
}
