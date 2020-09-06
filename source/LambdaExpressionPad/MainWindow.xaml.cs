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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Linq.Expressions;
using StepBro.Core.Data;
using StepBro.Core.Parser;

namespace LambdaExpressionPad
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {

            //var text = "";
            //using (var writer = new System.IO.StringWriter())
            //{
            //    MyClass.GetExpressionTree().Dump(writer);
            //    text = writer.ToString();
            //}
            tree.Items.Clear();
            tree.Items.Add(GetLeaf(MyClass.GetExpressionTree()));
        }

        TreeViewItem GetLeaf(ITreeDataElement element)
        {
            TreeViewItem item = new TreeViewItem() { Header = element.Name };
            if (element.HasValue) item.Header += (" = " + element.Value);
            if (element.ElementCount > 0)
            {
                foreach (var child in element.SubElements)
                {
                    item.Items.Add(GetLeaf(child));
                }
            }
            return item;
        }
    }

    public static class MyClass
    {
        public static ITreeDataElement GetExpressionTree()
        {

            List<string> strings = new List<string>(new string[] { "Chris", "Anders", "Jens", "Anton" });


            //Expression<Predicate<int>> expression = n => n < 10;
            //Expression<Predicate<int>> expression = n => n < strings.Where(s => s.StartsWith("An")).Count();

            //string error = null;
            //Expression<Func<int, bool>> expression = n => SomeMethod(n, ref error);

            //TreeViewItem
            //int? i = null;
            Expression<Func<int, int[]>> expression = n => new int[] { 2, 3, 4 };


            return expression.CreateDataTree(null);
            //using (var writer = new System.IO.StringWriter())
            //{
            //    treeElement.Dump(writer);
            //    return writer.ToString();
            //}
        }

        public static bool SomeMethod(int v, ref string error)
        {
            if (v > 14)
            {
                error = "Error !!!!";
                return false;
            }
            return (v > 6);
        }
    }
}
