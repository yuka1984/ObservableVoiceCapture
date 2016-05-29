using System;
using System.Collections.Generic;
using System.Linq;
using AudioToolbox;
using Prism.Mvvm;
using Foundation;
using Microsoft.Practices.Unity;
using UIKit;
using VoiceRecorder.Dependency;
using VoiceRecorder.iOS.Dependency;

namespace VoiceRecorder.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            AudioSession.Initialize();
            AudioSession.SetActive(true);
            AudioSession.Category = AudioSessionCategory.PlayAndRecord;

            global::Xamarin.Forms.Forms.Init();
            
            var container = new UnityContainer();
            container.RegisterType(typeof(IFile), typeof(File), new ContainerControlledLifetimeManager());
            LoadApplication(new App(container));

            return base.FinishedLaunching(app, options);
        }
    }
}
