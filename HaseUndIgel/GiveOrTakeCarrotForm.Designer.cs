namespace HaseUndIgel
{
    partial class GiveOrTakeCarrotForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnTake = new System.Windows.Forms.Button();
            this.btnGave = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnTake
            // 
            this.btnTake.Location = new System.Drawing.Point(12, 11);
            this.btnTake.Name = "btnTake";
            this.btnTake.Size = new System.Drawing.Size(84, 23);
            this.btnTake.TabIndex = 0;
            this.btnTake.Text = "Получить 10";
            this.btnTake.UseVisualStyleBackColor = true;
            this.btnTake.Click += new System.EventHandler(this.BtnTakeClick);
            // 
            // btnGave
            // 
            this.btnGave.Location = new System.Drawing.Point(135, 11);
            this.btnGave.Name = "btnGave";
            this.btnGave.Size = new System.Drawing.Size(84, 23);
            this.btnGave.TabIndex = 1;
            this.btnGave.Text = "Отдать 10";
            this.btnGave.UseVisualStyleBackColor = true;
            this.btnGave.Click += new System.EventHandler(this.BtnGaveClick);
            // 
            // GiveOrTakeCarrotForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 46);
            this.Controls.Add(this.btnGave);
            this.Controls.Add(this.btnTake);
            this.Name = "GiveOrTakeCarrotForm";
            this.Text = "Получить / отдать?";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTake;
        private System.Windows.Forms.Button btnGave;
    }
}