using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{

    public float countDownLength = 5f;
    public Text countdownText;

    public float timerLength = 30.0f;
    public Text timerText;
    public Text setupTimerText;

    private float currentTimer;
    private bool countdown = false;
    private bool timer = false;
    
    // Start is called before the first frame update
    void Start()
    {
        countDownLength = countDownLength + 0.49f;
        timerText.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown)
        {
            countDownLength -= 1 * Time.deltaTime;
            if (countDownLength < 0.5f)
            {
                countdown = false;
                countdownText.text = "GO!";
          
                StartTimer();
                //TODO invoke game start
                return;
            }
            countdownText.text = countDownLength.ToString("0");
        }

        if (timer)
        {
            timerText.enabled = true;
            currentTimer -= 1 * Time.deltaTime;
            if(timerLength - currentTimer < 1.0f)
            {
                countdownText.enabled = false;
            }

            if(currentTimer < 0.5f)
            {
                timer = false;
                timerText.text = "Finish!";
                
                return;
            }
            timerText.text = "Time left: "+ currentTimer.ToString("0");

        }
    }

    public void StartCountdown()
    {
        countdown = true;
    }

    public void StartTimer()
    {
        timer = true;
        currentTimer = timerLength;
    }

    public void IncreaseTimer()
    {
        timerLength += 30.0f;
        setupTimerText.text = string.Format("Timer: {0}", timerLength);

    }

    public void DecreaseTimer()
    {
        if (timerLength > 30.1f)
        {
            timerLength -= 30.0f;
            setupTimerText.text = string.Format("Timer: {0}", timerLength);
        }    
    }
}
