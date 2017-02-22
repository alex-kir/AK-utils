using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StoryboardsXamarinFormsDemo.Pages;
using Xamarin.Forms;

namespace StoryboardsXamarinFormsDemo.Storyboards
{
    class MainStoryboard : MainStoryboardTemplate
    {
        readonly App _app;

        Page MainPage { get { return _app.MainPage; } set { _app.MainPage = value; } }

        string lastValue = "";

        public MainStoryboard(App app)
        {
            _app = app;
        }

        protected override void MyPageOne(MyPageOneState state, string arg1)
        {
            MainPage = new MyPageOne(state.CompleteWithShowTwo, arg1);
        }

        protected override void MyPageTwo(MyPageTwoState state)
        {
            MainPage = new MyPageTwo(state.CompleteWithShowOne, state.CompleteWithShowThree, lastValue);
        }

        protected override void MyPageTwo(MyPageTwoState state, string arg1)
        {
            lastValue = arg1;
            MyPageTwo(state);
        }

        protected override void MyPageThree(MyPageThreeState state)
        {
            MainPage = new MyPageThree(state.CompleteWithBack);
        }

    }
}
