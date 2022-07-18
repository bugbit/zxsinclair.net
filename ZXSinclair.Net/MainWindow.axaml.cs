using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;

namespace ZXSinclair.Net
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public void MyButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("dotnet "+typeof(ZX80_81.Class1).Assembly.Location); 
            // do something
        }
    }
}
