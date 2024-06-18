using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class ClockHandler : MonoBehaviour
{
    [SerializeField] private GameObject hourHand;
    [SerializeField] private GameObject minuteHand;
    [SerializeField] private GameObject secondHand;
    [SerializeField] private TMP_InputField textString;
    [SerializeField] private Button syncButton;
    [SerializeField] private Button settingsButton;

    private const string requestUrl = "https://time.yandex.ru/sync.json";

    private DateTime currentTime;
    private int hour;
    private int minute;
    private int second;
    private bool isCountingTime = true;

    private Color defaultTextColor;
    private Color activeTextColor;


    [Serializable]
    public class YandexTime
    {
        public long time;
    }

    private void InitButtons()
    {
        syncButton.onClick.AddListener(() => UpdateServerTime());
        settingsButton.onClick.AddListener(() => SwitchTimeCounting());
        
    }

    private void InitColors()
    {
        defaultTextColor = Color.black;
        activeTextColor = Color.magenta;
        // rgb(118,27,127)
    }

    private void Awake()
    {
        InitButtons();
        InitColors();
        GetLocalTime();
        AnimateClockHands();
        StartCoroutine(CountTime());
    }


    private IEnumerator GetServerTime()
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(requestUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                string response = webRequest.downloadHandler.text;
                YandexTime yandexTime = JsonUtility.FromJson<YandexTime>(response);
                Debug.Log("Server time: " + yandexTime.time);

                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long ticks = yandexTime.time * TimeSpan.TicksPerMillisecond;
                DateTime result = epoch.AddTicks(ticks);

                currentTime = result;
            }
            else
            {
                Debug.LogError("WebRequest Error: " + webRequest.error);
            }
        }
    }

    public void UpdateServerTime()
    {
        StartCoroutine(GetServerTime());
    }

    private void GetLocalTime()
    {
        currentTime = DateTime.Now;
    }

    public void SwitchTimeCounting()
    {
        isCountingTime = !isCountingTime;
        textString.interactable = !isCountingTime;

        textString.textComponent.color = isCountingTime ? defaultTextColor : activeTextColor;
    }


    private void SyncTime()
    {
        textString.text = currentTime.ToString("HH:mm:ss");
        AnimateClockHands();
    }
    
    private void AnimateClockHands()
    {
        hour = currentTime.Hour;
        minute = currentTime.Minute;
        second = currentTime.Second;
        
        var ho