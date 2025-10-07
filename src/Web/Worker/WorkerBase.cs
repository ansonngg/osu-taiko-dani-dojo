namespace OsuTaikoDaniDojo.Web.Worker;

public abstract class WorkerBase
{
    private bool _isCanceling;

    public void Run(TimeSpan interval, TimeSpan duration, TimeSpan delay = default)
    {
        Task.Run(
            async () =>
            {
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay);
                }

                var timer = new PeriodicTimer(interval);
                var startTime = DateTime.Now;

                while (!_isCanceling && DateTime.Now - startTime < duration && await timer.WaitForNextTickAsync())
                {
                    await Execute();
                }

                OnCompleted();
            });
    }

    protected abstract Task Execute();

    protected virtual void OnCompleted()
    {
    }

    protected void Cancel()
    {
        _isCanceling = true;
    }
}
