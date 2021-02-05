namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public interface IMediator<out TView, in TParam> where TView : View<TParam>
    {
        void SetParam(TParam param);

        void OnEnable();

        void OnDisable();
    }
    
    public interface IMediator<out TView> where TView : View
    {
        void OnEnable();

        void OnDisable();
    }    
}