namespace OsuTaikoDaniDojo.Web.Worker;

public abstract class WorkerBase
{
    protected bool IsCanceling { get; set; }

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

                while (!IsCanceling && DateTime.Now - startTime < duration && await timer.WaitForNextTickAsync())
                {
                    await Execute();
                }

                await OnCompleted();
            });
    }

    protected abstract Task Execute();

    protected virtual Task OnCompleted()
    {
        return Task.CompletedTask;
    }
}
