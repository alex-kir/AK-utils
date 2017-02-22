using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace StoryboardsXamarinFormsDemo.Pages
{
    public partial class MyPageOne : ContentPage
    {
        public MyPageOne(Action<string> openTwo, string text)
        {
            InitializeComponent();

            label.Text = text;
            button.Clicked += (s, e) =>
            {
                openTwo(entry.Text);
            };
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }
    }
}
