using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Trivial.Diagnostics;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        MainViewer.Set(DiagnosticsUtility.TestData);
    }
}