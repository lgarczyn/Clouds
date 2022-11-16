using UnityEngine;
using UnityEngine.SceneManagement;
using Text = TMPro.TextMeshProUGUI;

public class CreditsController : MonoBehaviour
{

  [SerializeField][RequiredComponent] Text reqText;

  void Start()
  {
    PlaythroughData data = GameObject.FindObjectOfType<PlaythroughData>();

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
    if (Input.GetKeyDown(KeyCode.Space) ||
        Input.GetKeyDown(KeyCode.Escape))
    {
      SceneManager.LoadScene(0);
    }

  }
}
