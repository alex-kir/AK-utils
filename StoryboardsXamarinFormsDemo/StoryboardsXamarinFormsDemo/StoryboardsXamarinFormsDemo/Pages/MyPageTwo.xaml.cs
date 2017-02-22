using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace StoryboardsXamarinFormsDemo.Pages
{
    public partial class MyPageTwo : ContentPage
    {
        private Action<object> showThree;
        private Action<string> showTwo;
        private int lastInt;

        public MyPageTwo(Action<string> showOne, Action showThree, string lastValue)
        {
            InitializeComponent();

            label.Text = lastValue;
            button1.Clicked += (s, e) =>
            {
                showOne(entry.Text);
            };
            button3.Clicked += (s, e) =>
            {
                showThree();
            };
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
