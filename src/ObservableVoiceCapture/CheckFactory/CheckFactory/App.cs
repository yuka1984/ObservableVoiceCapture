using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace CheckFactory
{
    public class App : Application
    {
        public App()
        {
            var recorder = ObservableVoiceCapture.CaptureFactory.Get(8000, 250);
            // The root page of your application
            MainPage = new ContentPage
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    Children = {
                         new Label {
                             HorizontalTextAlignment = TextAlignment.Center,
                             Text = "Welcome to Xamarin Forms!"
                         }
                     }
                }
            };
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
