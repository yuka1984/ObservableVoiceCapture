using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using FormsToolkit;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Reactive.Bindings.Notifiers;
using VoiceRecorder.Core;
using VoiceRecorder.ViewModels;
using VoiceRecorder.Views;
using Xamarin.Forms;
[assembly: Xamarin.Forms.Xaml.XamlCompilation(Xamarin.Forms.Xaml.XamlCompilationOptions.Compile)]

namespace VoiceRecorder
{
    public static class MessageKey
    {
        public const string ListPage = "ListPage";
        public const string License = "License";
        public const string TestPage = "TestPage";
        public const string RecordPage = "RecordPage";
        public const string RecordPageClose = "RecordPageClose";
        public const string Question = "Question";
        public const string GoBack = "GoBack";

    }

    public class TestPage : ContentPage
    {
        protected override void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine("OnAppearing");
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            System.Diagnostics.Debug.WriteLine("OnDisappearing");
            base.OnDisappearing();
        }
    }

    public class App : Application
    {
        public readonly IUnityContainer _container;
        private readonly MasterDetailPage _masterDetailPage;
        private bool _ismodal = false;

        public App(IUnityContainer container, IObservable<Action> backObservable = null)
        {
            MessagingService.Init();
            _container = container;
            ViewModelLocationProvider.SetDefaultViewModelFactory(t => container.Resolve(t));

            var listpage = new NavigationPage(new ListPage());
            MessagingService.Current.Subscribe(MessageKey.ListPage, service =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (_masterDetailPage.Detail != (listpage))
                    {
                        _masterDetailPage.Detail = listpage;
                    }                    
                    _masterDetailPage.IsPresented = false;
                });
            });
            var licencepage = new NavigationPage(new License());            
            MessagingService.Current.Subscribe(MessageKey.License, service =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _masterDetailPage.Detail = licencepage;
                    _masterDetailPage.IsPresented = false;
                });
            });
            var testpage = new TestPage();
            MessagingService.Current.Subscribe(MessageKey.TestPage, service =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _masterDetailPage.Navigation.PushModalAsync(testpage) ;                   
                });
            });
            MessagingService.Current.Subscribe(MessageKey.RecordPage, service =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _masterDetailPage.Navigation.PushModalAsync(new NavigationPage(new RecordPage()));
                    _ismodal = true;
                });
            });
            MessagingService.Current.Subscribe(MessageKey.RecordPageClose, s =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    (_masterDetailPage).Navigation.PopModalAsync();
                    _ismodal = false;
                });
            });

            MessagingService.Current.Subscribe<MessagingServiceQuestion>(MessageKey.Question, (service, question) =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    question.OnCompleted(await _masterDetailPage.DisplayAlert(question.Title, question.Question, question.Positive, question.Negative));
                });                
            });
            if (backObservable != null)
            {
                backObservable.Subscribe(x =>
                {
                    if (!_ismodal)
                    {
                        x.Invoke(); x.Invoke();
                    }
                    else
                    {
                        MessagingService.Current.SendMessage(MessageKey.GoBack);
                    }
                });
            }
            


            _masterDetailPage = new MasterDetailPage();

            if (backObservable == null)
            {
                backObservable = new AnonymousObservable<Action>(x => Disposable.Empty);
            }

            container.RegisterType<Recorder>(new DisposingTransientLifetimeManager());
            container.RegisterInstance(new BusyNotifier(), new ContainerControlledLifetimeManager());

            _masterDetailPage.Master = new MenuPage();
            _masterDetailPage.Detail = listpage;

            MainPage = _masterDetailPage;
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

    public class MasterDetailNavigation2
    {
        private readonly Dictionary<string, Type> _nameTypes = new Dictionary<string, Type>();
        private readonly IUnityContainer _container;

        private readonly MasterDetailPage _masterDetailPage;
        private int _modalCount;
        private readonly BusyNotifier busyNotifier = new BusyNotifier();

        public MasterDetailNavigation2(MasterDetailPage masterdetail, IUnityContainer container, IObservable<Action> backObservable)
        {
            _masterDetailPage = masterdetail;
            
            _container = container;
            backObservable.Subscribe(x =>
            {
                if (IsModal)
                {
                    MessagingCenter.Send(this, "Back", x);
                }
                else
                {
                    x.Invoke();
                }
            });
        }

        public IReadOnlyDictionary<string, Type> NameTypes => _nameTypes;
        public bool IsModal => _modalCount > 0;

        public void RegistView()
        {
            _container.RegisterInstance(AddNameType(typeof(ListPage)), new ListPage(),
                new ContainerControlledLifetimeManager());
            _container.RegisterInstance(AddNameType(typeof(License)), new License(),
                new ContainerControlledLifetimeManager());
            _container.RegisterInstance(AddNameType(typeof(RecordPage)), new RecordPage(),
                new PerResolveLifetimeManager());
        }

        public void ChangeCurrentPage(string menu)
        {            
            if (!NameTypes.ContainsKey(menu))
            {
                throw new Exception("Not Found Menu Name");
            }
            if (IsModal) return;
            if (busyNotifier.IsBusy) return;
            using (busyNotifier.ProcessStart())
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    var detail = _container.Resolve(NameTypes[menu], menu) as Page;
                    _masterDetailPage.Detail = new NavigationPage(detail);
                    _masterDetailPage.IsPresented = false;
                });                
            }
        }

        public void PushModalPage(string name)
        {
            if (!NameTypes.ContainsKey(name))
            {
                throw new Exception("Not Found Page");
            }
            Device.BeginInvokeOnMainThread(() =>
            {
                var page = _container.Resolve(NameTypes[name], name) as Page;
                _masterDetailPage.Detail = page;
                _modalCount += 1;
            });
                
        }

        public async Task PopModalPage()
        {
            if (busyNotifier.IsBusy) return;
            using (busyNotifier.ProcessStart())
            {
                if (IsModal)
                {
                    var detail = _container.Resolve(NameTypes["ListPage"], "ListPage") as Page;
                    _masterDetailPage.Detail = new NavigationPage(detail);
                    _masterDetailPage.IsPresented = false;
                    _modalCount -= 1;
                }
            }
        }

        public async Task<bool> Confirm(string title, string message)
        {
            return await _masterDetailPage.DisplayAlert(title, message, "yes", "no");
        }

        private string AddNameType(string name, Type type)
        {
            _nameTypes.Add(name, type);
            return name;
        }

        private string AddNameType(Type type)
        {
            return AddNameType(type.Name, type);
        }
    }

    public class DisposingTransientLifetimeManager : LifetimeManager, IDisposable
    {
        private object older;

        public void Dispose()
        {
            if (older is IDisposable)
            {
                ((IDisposable) older).Dispose();
                older = null;
            }
        }

        public override object GetValue()
        {
            if (older is IDisposable)
            {
                ((IDisposable) older).Dispose();
                older = null;
            }
            return null;
        }

        public override void SetValue(object newValue)
        {
            older = newValue;
        }

        public override void RemoveValue()
        {
        }
    }
}