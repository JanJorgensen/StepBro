namespace StepBro.SimpleWorkbench
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            var backColor = splitContainerTopButtom.Panel1.BackColor;
            splitContainerTopButtom.Panel1.BackColor = Color.Yellow;
            splitContainerTopButtom.Panel2.BackColor = Color.Yellow;
            splitContainerBottomLeftRight.Panel1.BackColor = Color.Yellow;
            splitContainerBottomLeftRight.Panel2.BackColor = Color.Yellow;
            splitContainerLeftRest.Panel1.BackColor = Color.Yellow;
            splitContainerLeftRest.Panel2.BackColor = Color.Yellow;
            splitContainerMainRight.Panel1.BackColor = Color.Yellow;
            splitContainerMainRight.Panel2.BackColor = Color.Yellow;

            var splitterColor = Color.DarkGray;
            splitContainerTopButtom.BackColor = splitterColor;
            splitContainerBottomLeftRight.BackColor = splitterColor;
            splitContainerLeftRest.BackColor = splitterColor;
            splitContainerMainRight.BackColor = splitterColor;

            splitContainerTopButtom.Panel1.BackColor = backColor;
            splitContainerTopButtom.Panel2.BackColor = backColor;
            splitContainerBottomLeftRight.Panel1.BackColor = backColor;
            splitContainerBottomLeftRight.Panel2.BackColor = backColor;
            splitContainerLeftRest.Panel1.BackColor = backColor;
            splitContainerLeftRest.Panel2.BackColor = backColor;
            splitContainerMainRight.Panel1.BackColor = backColor;
            splitContainerMainRight.Panel2.BackColor = backColor;

            panelCustomToolstrips.Height = panelCustomToolstrips.Controls[0].Bounds.Bottom;

            //splitContainerTopButtom.Panel2Collapsed = true;
            splitContainerBottomLeftRight.Panel2Collapsed = true;
            splitContainerLeftRest.Panel1Collapsed = true;
        }

        private void helpBrowser_Load(object sender, EventArgs e)
        {

        }
    }
}
