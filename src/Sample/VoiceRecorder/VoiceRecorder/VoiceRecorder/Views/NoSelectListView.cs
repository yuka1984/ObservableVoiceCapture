using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Practices.ObjectBuilder2;
using Xamarin.Forms;

namespace VoiceRecorder.Views
{
    public class NoSelectListView : ListView
    {

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create<NoSelectListView, ICommand>(p => p.Command, default(ICommand));
        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set
            {
                SetValue(CommandProperty, value);
            }
        }

        public NoSelectListView()
        {
            this.ItemTapped += OnItemTapped;
            this.ItemSelected += OnItemSelected;
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs selectedItemChangedEventArgs)
        {
            if (selectedItemChangedEventArgs.SelectedItem != null)
            {
                this.SelectedItem = null;
            }
        }

        private void OnItemTapped(object sender, ItemTappedEventArgs itemTappedEventArgs)
        {
            if (this.Command != null && this.Command.CanExecute(itemTappedEventArgs.Item))
            {
                Command.Execute(itemTappedEventArgs.Item);
            }
        }
    }
}
