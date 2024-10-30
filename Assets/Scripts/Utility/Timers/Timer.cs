using System;
using UnityEngine;

// From adammyhre (https://github.com/adammyhre/Unity-Improved-Timers/blob/master/Runtime/Timer.cs)
public abstract class Timer : IDisposable {
    public float CurrentTime { get; protected set; }
    public bool IsRunning { get; private set; }

    protected float initialTime;

    public float Progress => Mathf.Clamp(CurrentTime / initialTime, 0f, 1f);

    public Action OnTimerStart = delegate { };
    public Action OnTimerStop = delegate { };

    protected Timer(float value) {
        initialTime = value;
        CurrentTime = initialTime;
    }

    /// <summary>
    /// Starts the timer by setting the current time to the initial time and invoking the OnTimerStart action.
    /// </summary>
    public void Start() {
        CurrentTime = initialTime;
        if (IsRunning) return;
        IsRunning = true;
        OnTimerStart.Invoke();
    }

    /// <summary>
    /// Stops the timer by setting the IsRunning flag to false and invoking the OnTimerStop action.
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) return;
        IsRunning = false;
        OnTimerStop.Invoke();
    }

    public abstract void Tick();
    public abstract bool IsFinished { get; }

    public void Resume() => IsRunning = true;
    public void Pause() => IsRunning = false;

    public virtual void Reset() => CurrentTime = initialTime;

    public virtual void Reset(float newTime) {
        initialTime = newTime;
        Reset();
    }

    bool disposed;

    ~Timer() {
        Dispose(false);
    }

    // Call Dispose 
    // when the consumer is done with the timer or being destroyed
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposed) return;

        disposed = true;
    }
}