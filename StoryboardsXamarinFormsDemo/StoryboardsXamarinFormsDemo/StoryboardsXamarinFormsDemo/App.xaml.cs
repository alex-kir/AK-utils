using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using StoryboardsXamarinFormsDemo.Storyboards;
using Xamarin.Forms;

namespace StoryboardsXamarinFormsDemo
{
    public partial class App : Application
    {
        readonly MainStoryboard storyboard;

        public App()
        {
            InitializeComponent();

            storyboard = new MainStoryboard(this);
            storyboard.RunFromStartOne("run from start", CancellationToken.None);
            //storyboard.RunFromStartTwo(CancellationToken.None);
            //MainPage = new StoryboardsXamarinFormsDemo.MainPage();
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
