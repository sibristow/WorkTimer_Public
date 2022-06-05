using System;
using System.Drawing;
using System.Windows.Forms;
using WorkTimer4.API.Data;

namespace WorkTimer4.Controls
{
    public class DoubleHeightMenuItem : ToolStripMenuItem
    {
        private const int CategoryWidth = 8;

        public string ProjectText { get; set; }
        
        public string CodeText { get; set; }

        public Color CategoryColour { get; set; }

        public string CategoryText { get; set; }
        
        internal Project Project { get; }


        public DoubleHeightMenuItem(Project project)
        {
            this.Project = project ?? throw new ArgumentNullException(nameof(project));
            this.ProjectText = project.Name;
            this.CodeText = project.ProjectCode;
            this.Text = this.ProjectText + "\n";
            this.CategoryText = project.Category;
            this.CategoryColour = Assets.WinFormsAssets.ColourFromCategory(project.Colour);
            this.Image = Assets.WinFormsAssets.FromEncodedImage(project.Icon);
        }
       

        protected override void OnPaint(PaintEventArgs e)
        {
            Font codeFont = new Font(SystemFonts.DefaultFont.FontFamily, 7f, FontStyle.Regular);
            var categoryBrush = new SolidBrush(this.CategoryColour);
            PointF codePoint = new PointF(40, 18);

            // find the longer of the two strings
            var textSize = e.Graphics.MeasureString(this.ProjectText, SystemFonts.DefaultFont).Width;
            var codeSize = e.Graphics.MeasureString(this.CodeText, codeFont).Width;

            // append extra space to the end
            if (textSize > codeSize)
            {
                this.Text = this.ProjectText + "  \n";
            }
            else
            {
                this.CodeText += "  ";
            }

            base.OnPaint(e);
  

            e.Graphics.DrawString(this.CodeText, codeFont, SystemBrushes.MenuText, codePoint);

            e.Graphics.FillRectangle(categoryBrush, this.Width - CategoryWidth - 4, 4, CategoryWidth, this.Height - 8);

            //tidy up
            categoryBrush.Dispose();
            codeFont.Dispose();
        }
    }
}