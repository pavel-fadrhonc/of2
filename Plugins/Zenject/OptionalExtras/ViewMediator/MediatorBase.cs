using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public abstract class MediatorBase<TView> : IMediator<TView> 
        where TView : View
    {
        [Inject] protected TView view;
        
        public virtual void OnEnable()
        {
            
        }

        public virtual void OnDisable()
        {
            
        }
    }

    public abstract class MediatorBase<TView, TParam> : IMediator<TView, TParam> 
        where TView : View<TParam>
    {
        [Inject] protected TView view;

        protected TParam param;
        
        public void SetParam(TParam param)
        {
            this.param = param;
        }

        public virtual void OnEnable()
        {
            
        }

        public virtual void OnDisable()
        {
            
        }
    }

}