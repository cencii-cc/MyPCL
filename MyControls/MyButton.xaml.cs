using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPCL.MyControls
{
    /// <summary>
    /// MyButton.xaml 的交互逻辑
    /// </summary>
    [ContentProperty("Inlines")]
    public partial class MyButton : Border
    {
        public MyButton()
        {
            InitializeComponent();
        }
    }
}
