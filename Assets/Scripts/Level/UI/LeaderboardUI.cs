using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    public Transform content;
    public GameObject entryPrefab;

    void Start()
    {
        entryPrefab.SetActive(false); // Hide the prefab template
        BuildLeaderboard();
    }

    void BuildLeaderboard()
    {
        var entries = LeaderboardManager.instance.GetEntries();

        // ❌ REMOVE EMPTY ONES
        entries.RemoveAll(e => string.IsNullOrWhiteSpace(e.playerName));

        entries.Sort((a, b) => b.score.CompareTo(a.score));

        int playerRank = -1; // Initialize player rank to -1 (not found)

        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].playerName == PlayerProfile.PlayerName)
            {
                playerRank = i;
                break; // Exit the loop once the player's rank is found
            }
        }

        // 🔥 STEP 2: Show ONLY top 10
        int displayCount = Mathf.Min(10, entries.Count);

        for (int i = 0; i < displayCount; i++)
        {
            CreateEntry(entries[i], i);
        }

        // 🔥 STEP 3: PUT IT RIGHT HERE 👇
        if (playerRank >= 10)
        {
            CreateSpacer(); // optional "..."

            CreateEntry(entries[playerRank], playerRank);
        }

        /*var entry = entries[i];

        GameObject obj = Instantiate(entryPrefab, content);
        obj.SetActive(true); // Show the instantiated entry

        TMP_Text[] texts = obj.GetComponentsInChildren<TMP_Text>();

        // Rank
        if (i == 0) texts[0].text = "🥇";
        else if (i == 1) texts[0].text = "🥈";
        else if (i == 2) texts[0].text = "🥉";
        else texts[0].text = (i + 1).ToString();

        texts[1].text = entry.playerName;
        texts[2].text = FormatTime(entry.totalTime);   // ⏱
        texts[3].text = entry.totalDeaths.ToString();  // 💀
        texts[4].text = entry.score.ToString();

        // Highlight player
        if (entry.playerName == PlayerProfile.PlayerName)
        {
            foreach (var t in texts)
                t.color = Color.yellow;
        }*/
    }

    public void RefreshUI()
    {
        // ❌ Remove old entries
        foreach (Transform child in content)
        {
            if (child.gameObject != entryPrefab)
                Destroy(child.gameObject);
        }

        // ✅ Rebuild UI
        BuildLeaderboard();
    }

    void CreateEntry(LeaderboardEntry entry, int index)
    {
        GameObject obj = Instantiate(entryPrefab, content);
        obj.SetActive(true);

        TMP_Text[] texts = obj.GetComponentsInChildren<TMP_Text>();

        // Rank
        if (index == 0) texts[0].text = "🥇";
        else if (index == 1) texts[0].text = "🥈";
        else if (index == 2) texts[0].text = "🥉";
        else texts[0].text = (index + 1).ToString();

        texts[1].text = entry.playerName;
        texts[2].text = FormatTime(entry.totalTime);
        texts[3].text = entry.totalDeaths.ToString();
        texts[4].text = entry.score.ToString();

        // Highlight player
        if (entry.playerName == PlayerProfile.PlayerName)
        {
            foreach (var t in texts)
                t.color = Color.yellow;
        }
    }

    void CreateSpacer()
    {
        GameObject obj = Instantiate(entryPrefab, content);
        obj.SetActive(true);

        TMP_Text[] texts = obj.GetComponentsInChildren<TMP_Text>();

        texts[0].text = "...";
        texts[1].text = "";
        texts[2].text = "";
        texts[3].text = "";
        texts[4].text = "";
    }

    string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        return $"{hours:00}:{minutes:00}:{seconds:00}";

    }

}
