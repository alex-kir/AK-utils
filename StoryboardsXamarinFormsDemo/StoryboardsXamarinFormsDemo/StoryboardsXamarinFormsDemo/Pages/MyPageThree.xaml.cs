using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace StoryboardsXamarinFormsDemo.Pages
{
    public partial class MyPageThree : ContentPage
    {
        private Action completeWithBack;

        public MyPageThree(Action completeWithBack)
        {
            InitializeComponent();
            this.completeWithBack = completeWithBack;

            button.Clicked += (s, e) => OnBackButtonPressed();
        }

        protected override bool OnBackButtonPressed()
        {
            completeWithBack();
            return true;
        }
    }
}
