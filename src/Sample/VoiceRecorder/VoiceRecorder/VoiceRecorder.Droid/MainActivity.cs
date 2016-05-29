using System;
using System.Reactive.Subjects;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Javax.Security.Auth;
using Microsoft.Practices.Unity;
using VoiceRecorder.Dependency;
using VoiceRecorder.Droid.Dependency;

namespace VoiceRecorder.Droid
{
    [Activity(Label = "VoiceRecorder.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Subject<Action> _backSubject;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            _backSubject = new Subject<Action>();
            var container = new UnityContainer();
            container.RegisterType(typeof(IFile), typeof(File), new ContainerControlledLifetimeManager());
            LoadApplication(new App(container, _backSubject));
        }

        public override void OnBackPressed()
        {
            _backSubject.OnNext(base.OnBackPressed);
        }
    }
}

