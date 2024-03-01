using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;



public class DayCycleHandler : MonoBehaviour
{
    private const float HOW_LONG_IS_THE_DAY_IN_MINUTES = 10f; //change here for how long you want the day to last in minutes. if you leave at 10f, it will take 10 minutes to do a full day
    private float secondsPerDay;
    private float elapsedTime = 0f;
    private int currentHour =0;
    private int currentMinute = 0f;
    private int currentSecond = 0f;

    public override void OnNetworkSpawn(){
        if(!IsServer)return;

        secondsPerDay = HOW_LONG_IS_THE_DAY_IN_MINUTES * 60f;
    }

    void Update()
    {
        if(!IsServer) return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= secondsPerDay)
        {
            elapsedTime -= secondsPerDay;
        }

        UpdateTimeServerRpc();
    }

    //it needs to be serverRpc otherwise we would get an error, since serverRpc are only 
    //replicated on the server instance of this script, so the clients wont even have this. 
    [ServerRpc]
    private void UpdateTimeServerRpc()
    {
        // Check if the DayCycleHandler component is enabled
        if (!enabled || !gameObject)
        {
            Debug.LogWarning("DayCycleHandler component is not enabled.");
            return;
        }

        // Check if any of the required variables are null
        if (currentHour == null || currentMinute == null || currentSecond == null)
        {
            // Handle the null case
            Debug.LogWarning("One or more network variables is null in UpdateTime.");
            return;
        }

        float normalizedTime = elapsedTime / secondsPerDay;
        currentHour = Mathf.FloorToInt(normalizedTime * 24f);
        float remainingMinutes = (normalizedTime * 24f - currentHour) * 60f;
        currentMinute = Mathf.FloorToInt(remainingMinutes);
        currentSecond = Mathf.FloorToInt((remainingMinutes - currentMinute) * 60f);
    }

    private string GetAmPm(int hour)
    {
        if (hour >= 12)
            return "PM";
        else
            return "AM";
    }

    private string FormatTime(int hour, int minute)
    {
        return string.Format("{0:D2}:{1:D2}", hour % 12 == 0 ? 12 : hour % 12, minute);
    }

    public string getTime(){
        return FormatTime(currentHour, currentMinute) + " " + GetAmPm(currentHour);
    }

}
