using System;

namespace Zenject
{
    public interface IInvoker
    {
        void InvokeRepeating(Action task_, float delay_, float interval_, float cancelTime = 0, bool ignorePause = false);
        void Invoke(Action task_, float delay_, bool ignorePause);
        void StopInvoke(Action task, float delay);

    }
}