using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Unity.Netcode;

/**
  this is a clock that will work on multiplayer, you only need to copy this to a gameobject in the scene and
  call the getTime() from some client script
**/

public class ClockNetcodeHandler : NetworkBehaviour
{
    private const float HOW_LONG_IS_THE_DAY_IN_MINUTES = 10f;
    private float secondsPerDay;
    private float elapsedTime = 0f;
    private NetworkVariable<int> currentHour = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentMinute = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentSecond = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        currentHour.Value = Mathf.FloorToInt(normalizedTime * 24f);
        float remainingMinutes = (normalizedTime * 24f - currentHour.Value) * 60f;
        currentMinute.Value = Mathf.FloorToInt(remainingMinutes);
        currentSecond.Value = Mathf.FloorToInt((remainingMinutes - currentMinute.Value) * 60f);
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
        return FormatTime(currentHour.Value, currentMinute.Value) + " " + GetAmPm(currentHour.Value);
    }

}
