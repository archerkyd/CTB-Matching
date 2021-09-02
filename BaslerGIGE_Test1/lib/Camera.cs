using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using PylonC.NETSupportLibrary;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace BaslerGIGE_Test1.lib
{
    public class Camera
    {
        Camera_Form c_f = null;
        public ImageProvider Camera_Im = new ImageProvider();
        private string camera = "23396099";   //相机序列号
        public static List<string> Device_Name = new List<string>();
        public bool grabIsRuning = false;
        /// <summary>
        /// 图像处理相关
        /// </summary>
        public Bitmap bmp = null;
        private bool bmp_flag = false;
        public HObject ho_Image=null;
        private HTuple Image_Width;
        private HTuple Image_Height;
        public HTuple Image_Width_Changed;
        public HTuple Image_Height_Changed;
        private HWindow image = null;
        public Characters characters = null;


        //characters 相关数组
        public int[] classA;
        public double[] confidence;

        public Camera(Camera_Form cf)
        {
            c_f = cf;
            image = c_f.hWindow.HalconWindow;
            Camera_Im.GrabbingStartedEvent += new ImageProvider.GrabbingStartedEventHandler(OnGrabStartEvent);
            Camera_Im.GrabbingStoppedEvent += new ImageProvider.GrabbingStoppedEventHandler(OnGrabStopEvent);
            Camera_Im.DeviceClosedEvent += new ImageProvider.DeviceClosedEventHandler(OnDeviceCloseEvent);
            Camera_Im.DeviceOpenedEvent += new ImageProvider.DeviceOpenedEventHandler(OnDeviceOpenEvent);
            Camera_Im.DeviceRemovedEvent += new ImageProvider.DeviceRemovedEventHandler(OnDeviceRemoveEvent);
            Camera_Im.GrabErrorEvent += new ImageProvider.GrabErrorEventHandler(OnGrabErrorEvent);
            Camera_Im.ImageReadyEvent += new ImageProvider.ImageReadyEventHandler(OnImageReadyEvent);
            characters = new Characters(this);
            classA = new int[10];
            confidence = new double[10];
        }
        private void OnGrabStartEvent()
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.GrabbingStartedEventHandler(OnGrabStartEvent));
                return;
            }
        }
        private void OnGrabStopEvent()
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.GrabbingStoppedEventHandler(OnGrabStopEvent));
                return;
            }
        }
        private void OnDeviceCloseEvent()
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.DeviceClosingEventHandler(OnDeviceCloseEvent));
                return;
            }
        }
        private void OnDeviceOpenEvent()
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.DeviceOpenedEventHandler(OnDeviceOpenEvent));
                return;
            }
        }
        private void OnDeviceRemoveEvent()
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.DeviceRemovedEventHandler(OnDeviceRemoveEvent));
                return;
            }
            try
            {
                Camera_Im.Stop();
                Camera_Im.Close();
            }
            catch (Exception e)
            {
                ShowException(e, Camera_Im.GetLastErrorMessage());
            }
        }
        public void ShowException(Exception e, string additionalErrorMessage)
        {
            string more = "\n\nLast error message (may not belong to the exception):\n" + additionalErrorMessage;
            MessageBox.Show("Exception caught:\n" + e.Message + (additionalErrorMessage.Length > 0 ? more : ""), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void OnGrabErrorEvent(Exception GrabException, string addErrorMsg)
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.GrabErrorEventHandler(OnGrabErrorEvent), GrabException, addErrorMsg);
                return;
            }
            ShowException(GrabException, addErrorMsg);
        }
        private void OnImageReadyEvent()
        {
            if (c_f.InvokeRequired)
            {
                c_f.BeginInvoke(new ImageProvider.ImageReadyEventHandler(OnImageReadyEvent));
                return;
            }
            try
            {
                object locked = new object();
                ImageProvider.Image img = Camera_Im.GetLatestImage();
                if (img != null)
                {
                    lock (locked)
                    {
                        bmp_flag = false;
                        if (BitmapFactory.IsCompatible(bmp, img.Width, img.Height, img.Color))
                        {
                            BitmapFactory.UpdateBitmap(bmp, img.Buffer, img.Width, img.Height, img.Color);
                        }
                        else
                        {
                            BitmapFactory.CreateBitmap(out bmp, img.Width, img.Height, img.Color);
                            BitmapFactory.UpdateBitmap(bmp, img.Buffer, img.Width, img.Height, img.Color);
                        }
                        HOperatorSet.GenEmptyObj(out ho_Image);
                        FormatConvert.BitmapToHObject(bmp, out ho_Image);
                        HOperatorSet.GetImageSize(ho_Image, out Image_Width, out Image_Height);
                        HOperatorSet.SetPart(image, 0, 0, Image_Height, Image_Width);
                        bmp_flag = true;
                    }
                    Camera_Im.ReleaseImage();
                }
            }
            catch (Exception e)
            {
                ShowException(e, Camera_Im.GetLastErrorMessage());
            }
        }
        public void Camera_Open()
        {
            try
            {
                Camera_Im.Stop();
                Camera_Im.Close();
                List<DeviceEnumerator.Device> list = DeviceEnumerator.EnumerateDevices();
                foreach (DeviceEnumerator.Device device in list)
                {
                    if (!Device_Name.Contains(device.Name))
                        Device_Name.Add(device.Name);
                    if (device.Name.Contains(camera))
                        Camera_Im.Open((uint)list.IndexOf(device));//不太确定是否是这个清单中的index
                    c_f.BeginInvoke(new MethodInvoker(delegate { c_f.listBox1.Items.Insert(0, "打开设备" + device.Name.ToString() + "成功"); }));
                }
                if (list.Count == 0)
                {
                    c_f.BeginInvoke(new MethodInvoker(delegate { c_f.listBox1.Items.Insert(0,"未找到摄像设备。。"); }));
                }
                if (PylonC.NET.Pylon.DeviceFeatureIsWritable(Camera_Im.m_hDevice, "ExposureTimeAbs"))
                {
                    PylonC.NET.Pylon.DeviceSetFloatFeature(Camera_Im.m_hDevice, "ExposureTimeAbs", 12000);
                }

            }
            catch (Exception e)
            {
                ShowException(e, Camera_Im.GetLastErrorMessage());
            }
        }
        public void Camera_Load()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            HOperatorSet.GenEmptyObj(out ho_Image);

            openFileDialog.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                HOperatorSet.ReadImage(out ho_Image, openFileDialog.FileName);
                HOperatorSet.GetImageSize(ho_Image, out Image_Width, out Image_Height);
                HOperatorSet.SetPart(image, 0, 0, Image_Height, Image_Width);
                HOperatorSet.DispObj(ho_Image, image);

            }
        }
        public void Camera_Save()
        {
            if (ho_Image != null)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "All Pictures|*.bmp;*.png; *.jpg;";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        HOperatorSet.WriteImage(ho_Image, "bmp", 0, sfd.FileName);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("保存图片失败~");
                        return;
                    }
                }
            }
            else
                MessageBox.Show("图片不存在~");
        }

        public void Camera_AutoSave(string order,HObject ho_Image_Save)
        {
            if (ho_Image != null)
            {
                
                string path = @"D:\image\"+DateTime.Now.ToString("yyyyMMddhhmmss")+"-order-"+order+".jpg";

                    try
                    {
                        HOperatorSet.WriteImage(ho_Image_Save, "jpg", 0, path);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("保存图片失败~");
                        return;
                    }

            }
            else
                MessageBox.Show("图片不存在~");
        }
        public void Camera_Close()
        {
            try
            {
                grabIsRuning = false;
                Camera_Im.Stop();
                Camera_Im.Close();
            }
            catch (Exception ex)
            {
                ShowException(ex, Camera_Im.GetLastErrorMessage());
            }
        }
        public void Camera_OneShot()
        {
            try
            {
                grabIsRuning = true;
                Camera_Im.OneShot();
                Task task2 = new Task(() =>
                {
                    
                    while (grabIsRuning)
                    {
                        if (bmp != null && bmp_flag == true)
                        {
                            HOperatorSet.DispObj(ho_Image, image);
                            grabIsRuning = false;
                        }
                    }
                });
                task2.Start();
            }
            catch (Exception ex)
            {
                ShowException(ex, Camera_Im.GetLastErrorMessage());
            }
        }
        public void Camera_Continue()
        {
            try
            {
                Camera_Im.ContinuousShot();
                Task task3 = new Task(() =>
                {
                    grabIsRuning = true;
                    while (grabIsRuning)
                    {
                        if (bmp != null && bmp_flag != false)
                        {
                            HOperatorSet.DispObj(ho_Image, image);
                            Thread.Sleep(100);
                        }
                    }
                });
                task3.Start();
            }
            catch (Exception ex)
            {
                ShowException(ex, Camera_Im.GetLastErrorMessage());
            }
        }

        public void Compare()
        {
            if (ho_Image != null)
            {
                characters.ImageToCharacters(ho_Image,Image_Width,Image_Height, out classA,out confidence,out characters.hv_Phi,out characters.hv_Row,out characters.hv_Column,out characters.ho_ImageChaged);
            }

        }

        public void dispResult(HObject ho_ImageChanged, HTuple hv_Width, HTuple hv_Height, HTuple hv_Phi, HTuple hv_Row, HTuple hv_Column)
        {
            if (ho_ImageChanged == null)
            {
                MessageBox.Show("特征提取错误，请重新读入图片~");
                return;
            }
            HOperatorSet.GetImageSize(ho_ImageChanged, out Image_Width_Changed, out Image_Height_Changed);
            HOperatorSet.SetPart(image, 0, 0, Image_Height_Changed, Image_Width_Changed);
            HOperatorSet.DispObj(ho_ImageChanged, image);
            HDevWindowStack.Push(image);
            if (HDevWindowStack.IsOpen())
            {
                HOperatorSet.SetDraw(HDevWindowStack.GetActive(), "margin");

            }
            for (int i = 0; i < 10; i++)
            {
                if (confidence[0] != 0)
                    if (characters.comp[9 - i] == false)
                    {
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetColor(HDevWindowStack.GetActive(), "red");
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.DispRectangle2(image, hv_Row + 100, (hv_Column + 50) + ((i - 1) * 40), hv_Phi, 20, 23);
                        }
                    }
                    else
                    {
                        if (HDevWindowStack.IsOpen())
                        {
                            HOperatorSet.SetColor(HDevWindowStack.GetActive(), "green");
                        }
                        using (HDevDisposeHelper dh = new HDevDisposeHelper())
                        {
                            HOperatorSet.DispRectangle2(image, hv_Row + 100, (hv_Column + 50) + ((i - 1) * 40), hv_Phi, 20, 23);
                        }
                    }
            }
        }
    }
}
