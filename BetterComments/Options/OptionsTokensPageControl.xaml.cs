namespace BetterComments.Options
{
   public partial class OptionsTokensPageControl
   {
      public OptionsTokensPageControl()
      {
         DataContext = Settings.Instance;

         InitializeComponent();
      }
   }
}