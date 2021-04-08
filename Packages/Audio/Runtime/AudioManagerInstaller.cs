using of2.AssetManagement;
using Zenject;

namespace of2.Audio
{
    public class AudioManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            var ampa = gameObject.GetComponent<AudioManagerProjectAdapter>();
            Container.Bind<IAudioManager>().To<AudioManagerProjectAdapter>().FromInstance(ampa).AsSingle();
            Container.QueueForInject(ampa);
            
            var gam = GetComponentInChildren<GlobalAudioManager>();
            Container.Bind<GlobalAudioManager>().FromInstance(gam).WhenInjectedInto<AudioManagerProjectAdapter>();
            Container.QueueForInject(gam);
            
            Container.Bind<IAssetManager>().To<AssetManager>().AsSingle().IfNotBound();

            Container.DeclareSignal<AudioBeganPlayingSignal>().OptionalSubscriber();
            Container.DeclareSignal<AudioStoppedPlayingSignal>().OptionalSubscriber();
            Container.DeclareSignal<AudioPreloadedSignal>().OptionalSubscriber();
            Container.DeclareSignal<AudioUnloadedSignal>().OptionalSubscriber();
        }
    }
}