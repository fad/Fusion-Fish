using UnityEngine;

// From adammyhre (https://github.com/adammyhre/Unity-Improved-Timers/blob/master/Runtime/Timers/CountdownTimer.cs)
/// <summary>
/// Timer that counts down from a specific value to zero.
/// </summary>
public class CountdownTimer : Timer {
    public CountdownTimer(float value) : base(value) { }

    public override void Tick() {
        if (IsRunning && CurrentTime > 0) {
            CurrentTime -= Time.deltaTime;
        }

        if (IsRunning && CurrentTime <= 0) {
            Stop();
        }
    }

    public override bool IsFinished => CurrentTime <= 0;
}
