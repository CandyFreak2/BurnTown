using Fusion;
using UnityEngine;

public struct CustomTickTimer : INetworkStruct {
    private int _target;
    private int _initialTick;

    public bool Expired(NetworkRunner runner) => runner.IsRunning && _target > 0 && (Tick)_target <= runner.Tick;
    public bool IsRunning => _target > 0;

    public bool ExpiredOrNotRunning(NetworkRunner runner) {
        return _target == 0 || !runner.IsRunning || Expired(runner);
    }

    public static CustomTickTimer CreateFromTicks(NetworkRunner runner, int ticks) {
        if (runner == false || runner.IsRunning == false)
            return new CustomTickTimer();

        CustomTickTimer fromTicks = new() {
            _target = (int)runner.Tick + ticks,
            _initialTick = runner.Tick
        };
        return fromTicks;
    }

    public float NormalizedValue(NetworkRunner runner) {
        if (runner == null || runner.IsRunning == false || IsRunning == false)
            return 0;

        if (Expired(runner))
            return 1;

        return ElapsedTicks(runner) / (_target - (float)_initialTick);
    }

    public int ElapsedTicks(NetworkRunner runner) {
        if (runner == false || runner.IsRunning == false) {
            return 0;
        }

        if (IsRunning == false || Expired(runner)) {
            return 0;
        }

        return runner.Tick - _initialTick;
    }

    public static CustomTickTimer CreateFromSeconds(NetworkRunner runner, float delayInSeconds) {
        if (!runner.IsRunning) {
            return default;
        }

        CustomTickTimer result = default;
        result._target = (int)runner.Tick + Mathf.CeilToInt(delayInSeconds / runner.DeltaTime);
        return result;
    }

    public int? RemainingTicks(NetworkRunner runner) {
        if (!runner.IsRunning) {
            return null;
        }

        if (IsRunning) {
            return Mathf.Max(0, _target + (int)runner.Tick);
        }

        return null;
    }

    public float? RemainingTime(NetworkRunner runner) {
        int? num = RemainingTicks(runner);
        if (num.HasValue) {
            return num.Value * runner.DeltaTime;
        }

        return null;
    }
}