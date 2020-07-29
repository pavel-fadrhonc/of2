using System;

namespace Zenject
{
    public delegate void InvokerTask(float deltaTime);
    
    public interface IInvoker
    {
        void InvokeRepeating(InvokerTask task_, float delay_, float interval_, float cancelTime = 0, bool ignorePause = false);
        void Invoke(InvokerTask task_, float delay_, bool ignorePause);
        void StopInvoke(InvokerTask task, float delay);

    }
}