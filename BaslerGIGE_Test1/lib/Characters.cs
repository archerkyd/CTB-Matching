using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HalconDotNet;
using System.Windows.Forms;

namespace BaslerGIGE_Test1.lib
{
    public class Characters
    {
        public Characters(Camera came)
        {
            comp = new bool[10];
            hv_Phi = new HTuple();
            hv_Row = new HTuple();
            hv_Column = new HTuple();
            camera = came;
        }
        public bool[] comp;
        public HTuple hv_Phi;
        public HTuple hv_Row;
        public HTuple hv_Column;
        public HObject ho_ImageChaged;
        private Camera camera = null;
        public void ImageToCharacters(HObject ho_Image, HTuple hv_Width, HTuple hv_Height, out int[] classA,out double[] confidence,out HTuple hv_Phi, out HTuple hv_Row, out HTuple hv_Column,out HObject ho_ImageChaged)
        {
            classA = new int[10];
            confidence = new double[10];
            
            //下面为halcon自动生成参数
            HObject[] OTemp = new HObject[20];
            HObject ho_Rectangle = null, ho_ImageReduced = null; 
            HObject ho_GrayImage = null, ho_ImageScaled = null, ho_Regions = null;
            HObject ho_ConnectedRegions = null, ho_SelectedRegions = null;
            HObject ho_RegionFillUp = null;
            HObject ho_Rectangle1 = null, ho_RegionAffineTrans = null, ho_ImagePart = null, ho_ImageScaled2 = null;
            HObject ho_SelectedRegions1 = null, ho_SelectedRegions2 = null;
            HObject ho_SelectedRegions3 = null, ho_SelectedRegions4 = null;
            HObject ho_SelectedRegions5 = null;
            HTuple hv_index1 = new HTuple();

            HTuple hv_Area = new HTuple();
            HTuple hv_Row1 = new HTuple(), hv_Column1 = new HTuple();
            HTuple hv_Length1 = new HTuple();
            HTuple hv_Length2 = new HTuple(), hv_Phi_Rad = new HTuple();
            HTuple hv_HomMat2D = new HTuple(), hv_j = new HTuple();
            HTuple hv_FeatureVector = new HTuple(), hv_Class = new HTuple();
            HTuple hv_Confidence = new HTuple();
            ho_ImageChaged = null;
            HTuple hv_MLPHandle1 = new HTuple();
            //初始化

            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_GrayImage);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled);
            HOperatorSet.GenEmptyObj(out ho_ImageScaled2);
            HOperatorSet.GenEmptyObj(out ho_Regions);
            HOperatorSet.GenEmptyObj(out ho_ConnectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions1);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions2);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions3);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions4);
            HOperatorSet.GenEmptyObj(out ho_SelectedRegions5);
            HOperatorSet.GenEmptyObj(out ho_RegionFillUp);
            HOperatorSet.GenEmptyObj(out ho_ImageChaged);
            HOperatorSet.GenEmptyObj(out ho_Rectangle1);
            HOperatorSet.GenEmptyObj(out ho_RegionAffineTrans);
            HOperatorSet.GenEmptyObj(out ho_ImagePart);


            HOperatorSet.ReadClassMlp("C:/Users/Administrator/Desktop/模板/CTB_Model1.mlp", out hv_MLPHandle1);

            // HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                ho_Rectangle.Dispose();
                HOperatorSet.GenRectangle1(out ho_Rectangle, hv_Height / 3, 0, (hv_Height * 3) / 4, hv_Width);
            }
            ho_ImageReduced.Dispose();
            HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out ho_ImageReduced);
            ho_GrayImage.Dispose();
            HOperatorSet.Rgb1ToGray(ho_ImageReduced, out ho_GrayImage);
            ho_ImageScaled.Dispose();
            HOperatorSet.ScaleImage(ho_GrayImage, out ho_ImageScaled, 5, -400);
            ho_Regions.Dispose();
            HOperatorSet.Threshold(ho_ImageScaled, out ho_Regions, 230, 255);
            ho_ConnectedRegions.Dispose();
            HOperatorSet.Connection(ho_Regions, out ho_ConnectedRegions);
            ho_SelectedRegions.Dispose();
            HOperatorSet.SelectShape(ho_ConnectedRegions, out ho_SelectedRegions, "area","and", 1000, 2000);
            ho_SelectedRegions1.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions, out ho_SelectedRegions1, "width","and", 0, 40);
            ho_SelectedRegions2.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions1, out ho_SelectedRegions2, "height","and", 50, 100);
            ho_SelectedRegions3.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions2, out ho_SelectedRegions3, "rectangularity","and", 0.7, 0.9);
            ho_SelectedRegions4.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions3, out ho_SelectedRegions4, "rect2_phi","and", 1, 2);
            ho_SelectedRegions5.Dispose();
            HOperatorSet.SelectShape(ho_SelectedRegions4, out ho_SelectedRegions5, "holes_num","and", 5, 100);
            ho_RegionFillUp.Dispose();
            HOperatorSet.FillUp(ho_SelectedRegions5, out ho_RegionFillUp);




            
            HOperatorSet.AreaCenter(ho_RegionFillUp, out hv_Area, out hv_Row, out hv_Column);
            //获取角度
            HOperatorSet.SmallestRectangle2(ho_RegionFillUp, out hv_Row1, out hv_Column1,out hv_Phi, out hv_Length1, out hv_Length2);

            hv_Phi_Rad = 0;
            if ((int)(new HTuple(hv_Phi.TupleGreater((new HTuple(135)).TupleRad()))) != 0)
            {

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Phi_Rad = (new HTuple(180)).TupleRad();
                }
            }
            else if ((int)(new HTuple(hv_Phi.TupleLess((new HTuple(-135)).TupleRad()))) != 0)
            {

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Phi_Rad = (new HTuple(180)).TupleRad();
                }
            }
            else if ((int)(new HTuple(hv_Phi.TupleGreater((new HTuple(45)).TupleRad()))) != 0)
            {

                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Phi_Rad = (new HTuple(90)).TupleRad();
                }
            }
            else if ((int)(new HTuple(hv_Phi.TupleLess((new HTuple(-45)).TupleRad()))) != 0)
            {
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    hv_Phi_Rad = (new HTuple(-90)).TupleRad();
                }
            }
            if ((int)(new HTuple((new HTuple(hv_Phi.TupleLength())).TupleGreater(1))) != 0)
            {
                //这里要输出错误，因为没有找准标定点。
                MessageBox.Show("找到了多处标定点，请重新拍照。");
                return;
            }

            HOperatorSet.VectorAngleToRigid(hv_Row, hv_Column, hv_Phi, hv_Row, hv_Column, hv_Phi_Rad, out hv_HomMat2D);
            ho_ImageChaged.Dispose();
            HOperatorSet.AffineTransImage(ho_Image, out ho_ImageChaged, hv_HomMat2D, "constant", "false");
            //每张图片10个捕捉点
            for (hv_j = 1; (int)hv_j <= 10; hv_j = (int)hv_j + 1)
            {

                //创建区域
                using (HDevDisposeHelper dh = new HDevDisposeHelper())
                {
                    ho_Rectangle1.Dispose();
                    HOperatorSet.GenRectangle1(out ho_Rectangle1, hv_Row + 80, (hv_Column - 10) + ((hv_j - 1) * 40),hv_Row + 127, (hv_Column + 25) + ((hv_j - 1) * 40));

                }
                ho_RegionAffineTrans.Dispose();
                HOperatorSet.AffineTransRegion(ho_Rectangle1, out ho_RegionAffineTrans, hv_HomMat2D,"nearest_neighbor");
                ho_ImageReduced.Dispose();
                HOperatorSet.ReduceDomain(ho_ImageChaged, ho_RegionAffineTrans, out ho_ImageReduced);
                ho_ImagePart.Dispose();
                HOperatorSet.CropDomain(ho_ImageReduced, out ho_ImagePart);
                HOperatorSet.ScaleImage(ho_ImagePart, out ho_ImageScaled2, 5, -400);

                HOperatorSet.WriteImage(ho_ImageScaled2, "bmp", 0, "C:/Users/Administrator/Desktop/problem/111.bmp");
                gen_features(ho_ImageScaled2, out hv_FeatureVector);
                HOperatorSet.ClassifyClassMlp(hv_MLPHandle1, hv_FeatureVector, 1, out hv_Class, out hv_Confidence);
                if ((int)(new HTuple(((hv_Class.TupleSelect(0))).TupleEqual(1))) != 0)
                {
                    classA[hv_j - 1] = 0;

                }
                else
                {
                    classA[hv_j - 1] = 1;
                }
                confidence[hv_j - 1] = (double)hv_Confidence;
                

            }
            HOperatorSet.ClearClassMlp(hv_MLPHandle1);

            ho_Rectangle.Dispose();
            ho_ImageReduced.Dispose();
            ho_GrayImage.Dispose();
            ho_ImageScaled.Dispose();
            ho_ImageScaled2.Dispose();

            ho_Regions.Dispose();
            ho_ConnectedRegions.Dispose();
            ho_SelectedRegions.Dispose();
            ho_SelectedRegions1.Dispose();
            ho_SelectedRegions2.Dispose();
            ho_SelectedRegions3.Dispose();
            ho_SelectedRegions4.Dispose();
            ho_SelectedRegions5.Dispose();
            ho_RegionFillUp.Dispose();
            ho_Rectangle1.Dispose();
            ho_RegionAffineTrans.Dispose();
            ho_ImagePart.Dispose();
        }


        private void gen_features(HObject ho_Image, out HTuple hv_FeatureVector)
        {
            HObject ho_Zoomed1;
            HOperatorSet.GenEmptyObj(out ho_Zoomed1);
            hv_FeatureVector = new HTuple();
            {
                HTuple ExpTmpOutVar_0;
                gen_sobel_features(ho_Image, hv_FeatureVector, out ExpTmpOutVar_0);
                hv_FeatureVector = ExpTmpOutVar_0;
            }
            ho_Zoomed1.Dispose();
            HOperatorSet.ZoomImageFactor(ho_Image, out ho_Zoomed1, 0.5, 0.5, "constant");
            {
                HTuple ExpTmpOutVar_0;
                gen_sobel_features(ho_Zoomed1, hv_FeatureVector, out ExpTmpOutVar_0);
                hv_FeatureVector = ExpTmpOutVar_0;
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                {
                    HTuple ExpTmpLocalVar_FeatureVector = hv_FeatureVector.TupleReal();
                    hv_FeatureVector = ExpTmpLocalVar_FeatureVector;
                }
            }
            ho_Zoomed1.Dispose();
            return;
        }

        public void gen_sobel_features(HObject ho_Image, HTuple hv_Features, out HTuple hv_FeaturesExtended)
        {
            HObject ho_EdgeAmplitude;
            HTuple hv_Energy = new HTuple(), hv_Correlation = new HTuple();
            HTuple hv_Homogeneity = new HTuple(), hv_Contrast = new HTuple();
            HTuple hv_AbsoluteHistoEdgeAmplitude = new HTuple();
            HOperatorSet.GenEmptyObj(out ho_EdgeAmplitude);
            hv_FeaturesExtended = new HTuple();
            HOperatorSet.CoocFeatureImage(ho_Image, ho_Image, 6, 90, out hv_Energy, out hv_Correlation,out hv_Homogeneity, out hv_Contrast);
            ho_EdgeAmplitude.Dispose();
            HOperatorSet.SobelAmp(ho_Image, out ho_EdgeAmplitude, "sum_abs", 3);
            HOperatorSet.GrayHistoAbs(ho_EdgeAmplitude, ho_EdgeAmplitude, 8, out hv_AbsoluteHistoEdgeAmplitude);
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                hv_FeaturesExtended = new HTuple();
                hv_FeaturesExtended = hv_FeaturesExtended.TupleConcat(hv_Features, hv_Energy, hv_Correlation, hv_Homogeneity, hv_Contrast);
            }
            using (HDevDisposeHelper dh = new HDevDisposeHelper())
            {
                {
                    HTuple ExpTmpLocalVar_FeaturesExtended = hv_FeaturesExtended.TupleConcat(hv_AbsoluteHistoEdgeAmplitude);
                    hv_FeaturesExtended = ExpTmpLocalVar_FeaturesExtended;
                }
            }
            ho_EdgeAmplitude.Dispose();
            return;
        }


    }
}
