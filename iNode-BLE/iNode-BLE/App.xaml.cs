using Autofac;
using ERGBLE.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ERGBLE
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            var container = DIConfiguration.Configure();
            MainPage = container.Resolve<MainPage>();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
