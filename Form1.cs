using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnDefult_Click(object sender, EventArgs e)
        {
            //输入距离预设值
            double[] Distance = new double[4] { 1.6, 2.1, 1.7, 2.0 };
            TextBox[] txtDistance = new TextBox[4] { txtDis0, txtDis1,txtDis2,txtDis3 };
            for (int i = 0; i < Distance.Length; i++)
            {
                txtDistance[i].Text = Distance[i].ToString();
            }

            //输入A、B点高程预设值
            double[] KnownResultHeight = new double[2] {204.286, 208.579 };
            TextBox[] txtKnownResultHeight = new TextBox[2] { txtResltHeightA, txtResltHeightB };
            for (int i = 0; i < KnownResultHeight.Length; i++)
            {
                txtKnownResultHeight[i].Text = KnownResultHeight[i].ToString();
            }

            //输入观测高差预设值
            double[] Height = new double[4] { +5.331, +1.813, -4.244, +1.430 };
            TextBox[] txtHeight = new TextBox[4] { txtHeight0, txtHeight1, txtHeight2, txtHeight3 };
            for (int i = 0; i < Height.Length; i++)
            {
                txtHeight[i].Text = Height[i].ToString();
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            //计算距离和
            double[] Distance = new double[4];
            TextBox[] txtDistance = new TextBox[4] { txtDis0, txtDis1, txtDis2, txtDis3 };
            for (int i = 0; i < Distance.Length; i++)
            {
                Distance[i] = double.Parse(txtDistance[i].Text);
            }
            double DistanceSigma = 0.0;//统计DistanceSigma
            foreach (double distance in Distance)
            {
                DistanceSigma += distance;
            }
            txtDisSig.Text = DistanceSigma.ToString();

            //计算观测高差和
            double[] Height = new double[4];
            TextBox[] txtHeight = new TextBox[4] { txtHeight0, txtHeight1, txtHeight2, txtHeight3 };
            for (int i = 0; i < Distance.Length; i++)
            {
                Height[i] = double.Parse(txtHeight[i].Text);
            }
            double HeightSigma = 0.000;//统计HeightSigma
            foreach (double height in Height)
            {
                HeightSigma+= height;
            }
            txtHeightSig.Text = HeightSigma.ToString();

            //计算高差闭合差
            double DealtaResultHeight = 0.00;
            TextBox[] txtDeltaResultHeight = new TextBox[2] { txtResltHeightA, txtResltHeightB };//计算AB点高差，但是前面已经有一个textbox是写高差的，所以这一步可以省略
            DealtaResultHeight = double.Parse(txtDeltaResultHeight[1].Text) - double.Parse(txtDeltaResultHeight[0].Text);
            double ResultFH = HeightSigma - DealtaResultHeight;
            int ResultFH_mm = Convert.ToInt32(ResultFH * 1000);//高精度转换
            labelFH.Text = ResultFH_mm.ToString();

            //计算高程闭合差容差
            double SquartDistanceSigma = Math.Sqrt(DistanceSigma);
            double FH_Rong = 40 * SquartDistanceSigma;
            int FH_Rong_mm = Convert.ToInt32(Math.Truncate(FH_Rong));//高精度转换
            labelFH_Rong.Text = "";
            labelFH_Rong.Text += FH_Rong_mm.ToString();
            labelFH_Rong.Text += ",";
            if (Math.Abs(ResultFH_mm) < Math.Abs(FH_Rong_mm))
            {
                labelFH_Rong.Text += "满足精度要求";
                //计算并分配改正数
                double[] RawCorrection = new double[4];
                double[] RawCorrectionRound = new double[4];
                TextBox[] txtRawCorrection = new TextBox[4] { txtRawCorection0, txtRawCorection1, txtRawCorection2, txtRawCorection3 };
                int[] RawCorrection_mm = new int[RawCorrection.Length];

                //反符号FH
                int InvertResultFH_mm = -ResultFH_mm;

                //计算初改正数
                TextBox[] txtLeftCorrection = new TextBox[4] { txtLeftCorection0, txtLeftCorection1, txtLeftCorection2, txtLeftCorection3 };
                for (int i = 0; i < RawCorrection.Length; i++)
                {
                    RawCorrection[i] = (InvertResultFH_mm / DistanceSigma * Distance[i]);
                    RawCorrectionRound[i] = CustomRound.CustomRound_(RawCorrection[i], 3);//进行小数位保留（3位小数）
                    RawCorrection_mm[i] = Convert.ToInt32(RawCorrectionRound[i]);
                    txtRawCorrection[i].Text = RawCorrection_mm[i].ToString();
                    txtRawCorectionSig.Text = RawCorrection_mm.Sum().ToString();
                }

                //计算再改正数
                int LeftCorrectionSig = InvertResultFH_mm - RawCorrection_mm.Sum();
                txtLeftCorectionSig.Text = LeftCorrectionSig.ToString();

                //清空上一次输出的再改正数
                TextBox[] RawtxtLeftCorrection = new TextBox[4] { txtLeftCorection0, txtLeftCorection1, txtLeftCorection2, txtLeftCorection3 };
                for (int i = 0; i < RawCorrection.Length; i++)
                {
                    RawtxtLeftCorrection[i].Text = "0";
                }

                // 声明再改正数组
                double[] LeftCorrection = new double[4];

                // 计算当前RawCorrection与RawCorrection_mm之间的距离修正
                double[] DistanceCorrection = new double[4];
                for (int i = 0; i < DistanceCorrection.Length; i++)
                {
                    DistanceCorrection[i] = Math.Abs(Math.Abs(RawCorrection[i]) - Math.Abs(RawCorrection_mm[i]));
                }

                // 添加一个循环来确保LeftCorrectionSig达到目标
                while (LeftCorrectionSig != 0)
                {
                    // 找到最需要调节的误差
                    double maxDistanceCorretion = double.MinValue;
                    int maxDistanceCorrectionIndex = -1;
                    for (int j = 0; j < DistanceCorrection.Length; j++)
                    {
                        if (DistanceCorrection[j] > maxDistanceCorretion)
                        {
                            maxDistanceCorretion = DistanceCorrection[j];
                            maxDistanceCorrectionIndex = j;
                        }
                    }

                    // 根据原有修正方向进行修正数的增减
                    if (RawCorrection_mm[maxDistanceCorrectionIndex] > 0)// 如果原改正数为正数，则增加1
                    {
                        LeftCorrection[maxDistanceCorrectionIndex] += 1;
                        LeftCorrectionSig -= 1; // 更新再改正数信号
                    }
                    else
                    {
                        LeftCorrection[maxDistanceCorrectionIndex] -= 1;// 如果原改正数为负数，则减少1
                        LeftCorrectionSig += 1; // 更新再改正数信号
                    }

                    // 将当前已调整的距离设为最小值以避免重复计算
                    DistanceCorrection[maxDistanceCorrectionIndex] = double.MinValue;

                    // 重新计算LeftCorrectionSig
                    LeftCorrectionSig = InvertResultFH_mm - Convert.ToInt32(RawCorrection_mm.Sum() + LeftCorrection.Sum());

                    // 更新每个再改正文本框的值
                    for (int i = 0; i < RawCorrection.Length; i++)
                    {
                        LeftCorrection[i] = Convert.ToInt32(LeftCorrection[i]);
                        txtLeftCorrection[i].Text = LeftCorrection[i].ToString();
                    }
                }

                //计算改正数
                TextBox[] txtCorrection = new TextBox[4] { txtCorection0, txtCorection1, txtCorection2, txtCorection3 };
                int[] Correction = new int[4];
                for (int i = 0; i < 4; i++)
                {
                    Correction[i] = Convert.ToInt32(RawCorrection_mm[i] + LeftCorrection[i]);
                    txtCorrection[i].Text = Correction[i].ToString();
                    txtCorectionSig.Text = Correction.Sum().ToString();
                }

                //计算改正后高差
                double[] HeightCorrection = new double[4];

                TextBox[] txtHeightCorrection = new TextBox[4] { txtCorHeight0, txtCorHeight1, txtCorHeight2, txtCorHeight3 };
                double[] displayHeight = new double[4];
                TextBox[] DisplaytxtHeight = new TextBox[4] { txtHeight0, txtHeight1, txtHeight2, txtHeight3 };
                for (int i = 0; i < displayHeight.Length; i++)
                {
                    displayHeight[i] = double.Parse(DisplaytxtHeight[i].Text);
                }


                double[] displatCorrection = new double[4];
                TextBox[] DisplayCorrection = new TextBox[4] { txtCorection0, txtCorection1, txtCorection2, txtCorection3 };
                for (int i = 0; i < displatCorrection.Length; i++)
                {
                    displatCorrection[i] = double.Parse(DisplayCorrection[i].Text);
                }

                for (int i = 0; i < displayHeight.Length; i++)
                {
                    HeightCorrection[i] = displayHeight[i] + (displatCorrection[i] / 1000);
                    txtHeightCorrection[i].Text = HeightCorrection[i].ToString();
                }

                double SigmaHeightCorrection = 0.0;
                SigmaHeightCorrection = HeightCorrection.Sum();
                txtCorHeightSig.Text = SigmaHeightCorrection.ToString();

                //计算高程
                double[] ResultHeight = new double[3];
                TextBox[] txtResultHeight = new TextBox[3] { txtResltHeight0, txtResltHeight1, txtResltHeight2 };
                double[] KnownResultHeight = new double[2];
                KnownResultHeight[0] = double.Parse(txtResltHeightA.Text);
                KnownResultHeight[1] = double.Parse(txtResltHeightB.Text);

                for (int i = 0; i < ResultHeight.Length; i++)
                {
                    if (i == 0)//第一个点
                    {
                        ResultHeight[0] = KnownResultHeight[0] + HeightCorrection[0];
                    }
                    else
                    {
                        ResultHeight[i] = ResultHeight[i - 1] + HeightCorrection[i];
                    }
                    txtResultHeight[i].Text = ResultHeight[i].ToString();

                }
            }

            else
            {
                labelFH_Rong.Text += "不满足精度要求";
            }
            //高差闭合差超限则停止计算
            
                
                
            }


        private void ClearAllTextBoxes(Control control)
        {
            foreach (Control c in control.Controls)
            {
                if (c is TextBox)
                {
                    ((TextBox)c).Clear();
                }
                else
                {
                    // 如果该控件包含子控件，则递归调用
                    ClearAllTextBoxes(c);
                }
            }
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearAllTextBoxes(this);
            labelFH.Text = "";
            labelFH_Rong.Text = "";
        }

        private void labelFH_Click(object sender, EventArgs e)
        {

        }
    }
}

