using BaslerGIGE_Test1.lib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using HalconDotNet;



namespace BaslerGIGE_Test1
{
    public partial class Camera_Form : Form
    {
        public Camera_Form()
        {
            
            InitializeComponent();
            this.TCPStatus.BackColor= Color.Red;
            this.CameraStatus.BackColor = Color.Red;

            cam = new Camera(this);
            TCP_ccd = new ServerTCP();
            TCP_ccd.OpenServer(3333);//打开TCP端口开始通讯
            xl = new XMLToList();
            
            timer1.Enabled = true;

        }

        /// <summary>
        /// 相机对象
        /// </summary>
        private Camera cam = null;


        /// <summary>
        /// TCP对象
        /// </summary>
        private ServerTCP TCP_ccd = null;
        private string data;
        /// <summary>
        /// xml对象
        /// </summary>
        private XMLToList xl=null;


        /// <summary>
        /// 参数
        /// </summary>
        private bool test_result;
        private int count;
        private List<string> XMLList = new List<string>();

        private void OpenButton_Click(object sender, EventArgs e)
        {
            cam.Camera_Open();   //打开相机
        }

        private void OneShot_Click(object sender, EventArgs e)
        {

            data = TCP_ccd.msReceiveData+"采集一张";
            this.BeginInvoke(new MethodInvoker(delegate { listBox1.Items.Insert(0,data); }));
            cam.Camera_OneShot();
                
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            cam.Camera_Close();
        }

        private void Camera_Form_Load(object sender, EventArgs e)
        {
            cam.Camera_Open();  //打开摄像头
            Task task1 = new Task(()=> 
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (TCP_ccd.TCPConnected)
                        TCPStatus.BackColor = Color.Green;
                    else
                        TCPStatus.BackColor = Color.Red;
                    if (cam.Camera_Im.IsOpen)
                        CameraStatus.BackColor = Color.Green;
                    else
                        CameraStatus.BackColor = Color.Red;
                    
                }
            });

            task1.Start();
        }

        private void Camera_Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            TCP_ccd.CloseServer();
            if(cam.ho_Image!=null)
                cam.ho_Image.Dispose();
            if(cam.characters.ho_ImageChaged!=null)
                cam.characters.ho_ImageChaged.Dispose();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if(TCP_ccd.msReceiveData!=null)
                listBox1.Items.Insert(0,TCP_ccd.msReceiveData);
            TCP_ccd.msReceiveData = null;

        }

        private void Continuebutton_Click(object sender, EventArgs e)
        {

            data = TCP_ccd.msReceiveData + "摄像";
            this.BeginInvoke(new MethodInvoker(delegate { listBox1.Items.Insert(0,data); }));
            cam.Camera_Continue();
                
        }

        private void 加载图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cam.Camera_Load();
        }

        private void 保存图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cam.Camera_Save();
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                label3.ForeColor = Color.Yellow;
                label3.Text = "匹配中...";
                test_result = false;
                string temp = richTextBox1.Text.Replace("\r", "").Replace("\n", "");
                richTextBox1.Clear();
                richTextBox1.Text = temp;
                if (temp.Length < 9 || temp.Length > 11)
                {
                    listBox1.Items.Insert(0, "订单格式不正确");
                    return;
                }
                string path = @"C:\Users\Administrator\Desktop";
                //OneShot.PerformClick();
                string file = "*" + temp + "*.xml";
                try
                {
                    string[] dir = Directory.GetFiles(path, file, SearchOption.AllDirectories);
                    if (dir[0] == null)
                    {
                        MessageBox.Show("未找到订单文件。\r\n请确认网络以及订单输入。");
                    }

                    xl.XMLRead(dir[0]);
                    xl.XMLInfo();
                    dataGridView1.Rows.Clear();

                    foreach (Data1 i in xl.xmlList)
                    {
                        int addr = 0;
                        int index = dataGridView1.Rows.Add();
                        dataGridView1.Rows[index].Cells[addr++].Value = i.ID;
                        dataGridView1.Rows[index].Cells[addr++].Value = i.Material;
                    }
                    listBox1.Items.Insert(0, "载入文件" + dir[0] + "成功！");


                    //camera oneshot

                    cam.Camera_OneShot();
                    

                    //匹配
                    while (true)
                    { 
                        if(cam.bmp!=null&&cam.grabIsRuning==false)
                        count = 0;
                        cam.Compare();
                        xl.xmlList.Clear();
                        if (cam.confidence[0] > 0)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                if (xl.xslist[i] == null)
                                    xl.xslist[i] = new Data1();
                                if (cam.classA[9 - i] != 0)
                                {
                                    xl.xslist[i].Exist = "有";
                                    if (xl.xslist[i].Material != "无")
                                    {
                                        count++;
                                        cam.characters.comp[i] = true;
                                    }
                                    else
                                        cam.characters.comp[i] = false;
                                }
                                else
                                {
                                    xl.xslist[i].Exist = "无";
                                    if (xl.xslist[i].Material == "无")
                                    {
                                        count++;
                                        cam.characters.comp[i] = true;
                                    }
                                    else
                                        cam.characters.comp[i] = false;
                                }
                                xl.xslist[i].Confidence = cam.confidence[9 - i];
                                xl.xmlList.Add(xl.xslist[i]);

                            }
                            cam.dispResult(cam.characters.ho_ImageChaged, cam.Image_Width_Changed, cam.Image_Height_Changed, cam.characters.hv_Phi, cam.characters.hv_Row, cam.characters.hv_Column);

                            dataGridView1.Rows.Clear();
                            foreach (Data1 i in xl.xmlList)
                            {
                                int addr = 0;
                                int index = dataGridView1.Rows.Add();
                                dataGridView1.Rows[index].Cells[addr++].Value = i.ID;
                                dataGridView1.Rows[index].Cells[addr++].Value = i.Material;
                                dataGridView1.Rows[index].Cells[addr++].Value = i.Exist;
                                dataGridView1.Rows[index].Cells[addr++].Value = i.Confidence;
                            }
                            listBox1.Items.Insert(0, "匹配完成.");

                            //label3 标注匹配结果
                            if (count == 10)
                            {
                                test_result = true;
                                label3.ForeColor = Color.Green;
                                label3.Text = "匹配通过！";
                            }
                            else
                            {
                                test_result = false;
                                label3.ForeColor = Color.Red;
                                label3.Text = "匹配失败！";

                            }



                            //保存图片
                            cam.Camera_AutoSave(richTextBox1.Text, cam.ho_Image);
                        }
                        break;
                    } 
                }
                catch (Exception)
                { throw; }
            }
        }

        private void 订单载入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            string temp = richTextBox1.Text.Replace("\r", "").Replace("\n", "");
            richTextBox1.Clear();
            richTextBox1.Text = temp;
            if (temp.Length < 9 || temp.Length > 11)
            {
                listBox1.Items.Insert(0, "订单格式不正确");
                return;
            }
            string path = @"C:\Users\Administrator\Desktop";
            //OneShot.PerformClick();
            string file = "*" + temp + "*.xml";
            try
            {
                string[] dir = Directory.GetFiles(path, file, SearchOption.AllDirectories);
                if (dir[0] == null)
                {
                    MessageBox.Show("未找到订单文件。\r\n请确认网络以及订单输入。");
                }

                xl.XMLRead(dir[0]);
                xl.XMLInfo();
                dataGridView1.Rows.Clear();

                foreach (Data1 i in xl.xmlList)
                {
                    int addr = 0;
                    int index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[addr++].Value = i.ID;
                    dataGridView1.Rows[index].Cells[addr++].Value = i.Material;
                }
                listBox1.Items.Insert(0, "载入文件" + dir[0] + "成功！");



            }
            catch (Exception)
            { throw; }
        }

        private void 匹配ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            label3.ForeColor = Color.Yellow;
            label3.Text = "匹配中...";
            test_result = false;
            count = 0;
            cam.Compare();
            xl.xmlList.Clear();
            if (cam.confidence[0] > 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (xl.xslist[i] == null)
                        xl.xslist[i] = new Data1();
                    if (cam.classA[9-i] != 0)
                    {
                        xl.xslist[i].Exist = "有";
                        if (xl.xslist[i].Material != "无")
                        {
                            count++;
                            cam.characters.comp[i] = true;
                        }
                        else
                            cam.characters.comp[i] = false;
                    }
                    else
                    {
                        xl.xslist[i].Exist = "无";
                        if (xl.xslist[i].Material == "无")
                        {
                            count++;
                            cam.characters.comp[i] = true;
                        }
                        else
                            cam.characters.comp[i] = false;
                    }
                    xl.xslist[9-i].Confidence = cam.confidence[9-i];
                    xl.xmlList.Add(xl.xslist[i]);

                }
                cam.dispResult(cam.characters.ho_ImageChaged,cam.Image_Width_Changed,cam.Image_Height_Changed,cam.characters.hv_Phi,cam.characters.hv_Row,cam.characters.hv_Column);

                dataGridView1.Rows.Clear();
                foreach (Data1 i in xl.xmlList)
                {
                    int addr = 0;
                    int index = dataGridView1.Rows.Add();
                    dataGridView1.Rows[index].Cells[addr++].Value = i.ID;
                    dataGridView1.Rows[index].Cells[addr++].Value = i.Material;
                    dataGridView1.Rows[index].Cells[addr++].Value = i.Exist;
                    dataGridView1.Rows[index].Cells[addr++].Value = i.Confidence;
                }
                listBox1.Items.Insert(0, "匹配完成.");

                //label3 
                if (count == 10)
                {
                    test_result = true;
                    label3.ForeColor = Color.Green;
                    label3.Text = "匹配通过！";
                }
                else
                {
                    test_result = false;
                    label3.ForeColor = Color.Red;
                    label3.Text = "匹配失败！";

                }



                //保存图片
                cam.Camera_AutoSave(richTextBox1.Text, cam.ho_Image);

            }
        }


    }
}
