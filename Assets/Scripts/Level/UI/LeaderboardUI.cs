using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public Transform container;
    public GameObject entryPrefab;

private void Start()
    {
        ShowLeaderboard();
    }

    public void ShowLeaderboard()
    {
        foreach (Transform child in container)
            Destroy(child.gameObject);

        var entries = LeaderboardManager.instance.GetEntries();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            GameObject obj = Instantiate(entryPrefab, container);

            TMP_Text text = obj.GetComponent<TMP_Text>();
            text.text = (i + 1) + ". " + entry.playerName + " - " + entry.score;
        }
    }

}
