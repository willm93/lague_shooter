using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public NamedMenu[] menus;
    public Dictionary<string, GameObject> menuLookup = new Dictionary<string, GameObject>();

    public Image fadePlane;
    Color fadedColor = new Color(0, 0, 0, 0.75f);

    Player player;

    void Start()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }

        for(int i = 0; i < menus.Length; i++)
        {
            menuLookup.Add(menus[i].menuName, menus[i].menuHolder);
        }

        player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            player.OnDeath += OnGameOver;
            player.GetComponent<PlayerInput>().OnPause += OnPause;
            player.GetComponent<PlayerInput>().OnResume += OnResume;
        } else {
            Cursor.visible = true;
        }
    }
    
    public void OpenMenu(string name, string callerName)
    {
        menuLookup[callerName].SetActive(false);
        menuLookup[name].SetActive(true);
        menuLookup[name].GetComponent<Menu>().callingMenuName = callerName;
    }

    void OnPause()
    {
        Cursor.visible = true;
        fadePlane.color = fadedColor;
        menuLookup["pause"].SetActive(true);
    }

    void OnResume()
    {
        Cursor.visible = false;
        fadePlane.color = Color.clear;
        menuLookup["pause"].SetActive(false);
        menuLookup["options"].SetActive(false);
    }
    
    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, fadedColor, 1));
        menuLookup["game_over"].GetComponent<GameOverMenu>().scoreUI.text = $"Kill Count: {ScoreKeeper.score}";
        menuLookup["game_over"].SetActive(true);
    }

    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1 / time;
        float percent = 0;

        while(percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null;
        }

    }

    [System.Serializable]
    public class NamedMenu 
    {
        public string menuName;
        public GameObject menuHolder;
    }
}
