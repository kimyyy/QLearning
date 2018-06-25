using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
namespace QLearning
{
	public partial class ResultForm : Form
	{

		private PictureBox[] pictureBoxes = new PictureBox[36];

		public ResultForm()
		{
			InitializeComponent();
		}



		private void ResultForm_Load(object sender, EventArgs e)
		{
			SuspendLayout();
			for (int i = 0; i < 6; i++)
			{
				for (int j = 0; j < 6; j++)
				{
					int scale = 150;
					int num = 6 * i + j;
					pictureBoxes[num] = new PictureBox();
					pictureBoxes[num].Name = "pictureBox" + (6 * i + j).ToString();
					pictureBoxes[num].Size = new Size(scale, scale);
					pictureBoxes[num].Location = new Point(j * scale, i * scale);
					Bitmap canvas = new Bitmap(pictureBoxes[num].Width, pictureBoxes[num].Height);

					Graphics g = Graphics.FromImage(canvas);

					Point[] dash = { new Point(scale/2, scale/4),
					new Point(scale/4,scale/2),new Point(scale/2,scale*3/4), new Point(scale*3/4,scale/2)};
					double abs = Math.Abs(Agent.Q[j + 1, i + 1, 1]);
					// Qの絶対値のMaxを決める
					for (int k = 2; k < 5; k++)
					{
						double newabs = Math.Abs(Agent.Q[j + 1, i + 1, k]);
						if(newabs > abs)
						{
							abs = newabs;
						}
					}
					double rate = scale/8.0;
					Point[] q =
					{

						new Point(scale/2, scale/4-(int)(Agent.Q[j+1, i+1, 1]*rate)),
						new Point(scale*3/4+(int)(Agent.Q[j+1, i+1, 2]*rate), scale/2),
						new Point(scale/2, scale*3/4+(int)(Agent.Q[j+1,i+1,4]*rate)),
						new Point(scale/4-(int)(Agent.Q[j+1,i+1,3]*rate),scale/2)
					};
					Pen dashPen = new Pen(Color.Black, 1);
					dashPen.DashStyle = DashStyle.Dash;
					g.DrawPolygon(dashPen, dash);
					g.DrawPolygon(Pens.Black, q);
					var font = new Font("MS UI Gothinc", 8);
					var round = 2;
					g.DrawString(Math.Round(Agent.Q[j + 1, i + 1, 1],round).ToString(), font, Brushes.Blue, scale/2 - 10, 10);
					g.DrawString(Math.Round(Agent.Q[j + 1, i + 1, 2], round).ToString(), font, Brushes.Blue, scale/2 + 20, scale/2 - 5);
					g.DrawString(Math.Round(Agent.Q[j + 1, i + 1, 3], round).ToString(), font, Brushes.Blue, 15, scale/2 -5);
					g.DrawString(Math.Round(Agent.Q[j + 1, i + 1, 4], round).ToString(), font, Brushes.Blue, scale/2 -5, scale*3/4);
					g.DrawString(Agent.visit[j + 1, i + 1].ToString(), new Font("MS UI Gothinc", 9), Brushes.Red, scale/2 -15, scale/2 -20);
					var rect = new Rectangle(0, 0, scale, scale);
					if(Agent.Stage[j+1,i+1] < 0)
					{
						g.FillRectangle(Brushes.Brown, rect);
					}
					if(Agent.Stage[j+1,i+1] == 2)
					{
						g.FillRectangle(Brushes.White, rect);
						g.DrawString("G1", font, Brushes.Blue, scale/2 - 10, scale/2 - 10);
					}
					if (Agent.Stage[j + 1, i + 1] == 1)
					{
						g.FillRectangle(Brushes.White, rect);
						g.DrawString("G2", font, Brushes.Blue, scale/2 - 10, scale/2 - 10);
					}
					g.Dispose();
					pictureBoxes[num].Image = canvas;
				}
			}
			Controls.AddRange(pictureBoxes);
			ResumeLayout(false);
		}
	}
}
