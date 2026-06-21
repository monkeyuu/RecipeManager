namespace RecipeManager
{
    partial class FormDetail
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(744, 641);
            this.Name = "FormDetail";
            this.Text = "食譜詳細";
            this.ResumeLayout(false);
        }
    }
}
