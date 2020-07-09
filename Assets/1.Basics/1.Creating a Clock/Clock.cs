using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clock : MonoBehaviour
{
    const float degreesPerHour = 30f,
                degreesPerMinute = 6f,
                degreesPerSecond = 6f;

    public Transform hoursTransform;
    public Transform minutesTransform;
    public Transform secondsTransform;
    public bool Continuous;

    private void Awake()
    {
        //Debug.Log(DateTime.Now.Hour);

        UpdateDiscrete();
    }

    private void Update()
    {
        if (Continuous)
            UpdateContinous();
        else
            UpdateDiscrete();
    }

    private void UpdateContinous()
    {
        TimeSpan time = DateTime.Now.TimeOfDay;
        hoursTransform.localRotation = Quaternion.Euler(0f, (float)time.TotalHours * degreesPerHour, 0f);
        minutesTransform.localRotation = Quaternion.Euler(0f, (float)time.TotalMinutes * degreesPerMinute, 0f);
        secondsTransform.localRotation = Quaternion.Euler(0f, (float)time.TotalSeconds * degreesPerSecond, 0f);
    }

    private void UpdateDiscrete()
    {
        DateTime time = DateTime.Now;
        hoursTransform.localRotation = Quaternion.Euler(0f, time.Hour * degreesPerHour, 0f);
        minutesTransform.localRotation = Quaternion.Euler(0f, time.Minute * degreesPerMinute, 0f);
        secondsTransform.localRotation = Quaternion.Euler(0f, time.Second * degreesPerSecond, 0f);
    }
}
