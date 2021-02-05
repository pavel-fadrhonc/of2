using Zenject;

namespace Plugins.Zenject.OptionalExtras.ViewMediator
{
    public class ViewInstallerNoParams<TView, TMediator1> : MonoInstaller
        where TMediator1 : IMediator<View>
        where TView : View<View>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View>>().To<TMediator1>().AsSingle();
            Container.BindInstance(GetComponent<View>()).WhenInjectedInto<TMediator1>();
            
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstallerNoParams<TView, TMediator1, TMediator2> : MonoInstaller
        where TMediator1 : IMediator<View>
        where TMediator2 : IMediator<View>
        where TView : View<View>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View>>().To<TMediator1>().AsSingle();
            Container.Bind<IMediator<View>>().To<TMediator2>().AsSingle();
            
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstaller<TParam1, TView, TMediator1> : MonoInstaller
        where TMediator1 : IMediator<View<TParam1>, TParam1>
        where TView : View<TParam1>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View<TParam1>, TParam1>>().To(typeof(TMediator1)).AsSingle();
            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
    
    public class ViewInstaller<TParam1, TView, TMediator1, TMediator2> : MonoInstaller
        where TMediator1 : IMediator<View<TParam1>, TParam1>
        where TMediator2 : IMediator<View<TParam1>, TParam1>
    {
        public override void InstallBindings()
        {
            Container.Bind<IMediator<View<TParam1>, TParam1>>().To(typeof(TMediator1), typeof(TMediator2)).AsSingle();

            Container.Bind<TView>().FromInstance(GetComponent<TView>());
        }
    }
}