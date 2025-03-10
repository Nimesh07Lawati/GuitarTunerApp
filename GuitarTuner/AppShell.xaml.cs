using GuitarTuner.Views;

namespace GuitarTuner
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("HomePage", typeof(HomePageView));
        }
    }
}
