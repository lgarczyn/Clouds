using UnityEngine;
using UnityEngine.SceneManagement;
using Text = TMPro.TextMeshProUGUI;
using UnityEngine.InputSystem;

public class CreditsController : MonoBehaviour
{

  [SerializeField][RequiredComponent] Text reqText;

  void Start()
  {
    PlaythroughData data = PlaythroughData.instance ?? GameObject.FindObjectOfType<PlaythroughData>();

    if (data)
    {
      string text = reqText.text;

      text = text.Replace("{MINUTES}", "" + Mathf.Floor(data.timeSinceGameStart / 60f));
      text = text.Replace("{SECONDS}", "" + Mathf.Floor(data.timeSinceGameStart % 60f));

      reqText.text = text;
    }
  }

  void Update()
  {
    if (Keyboard.current.spaceKey.wasPressedThisFrame ||
        Keyboard.current.escapeKey.wasPressedThisFrame)
    {
      SceneManager.LoadScene(0);
    }

  }
}
