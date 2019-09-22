using System;

namespace Zicore.Neat
{
    public class NeatMath
    {
        //public static double Sigmoid(double z)
        //{
        //    z = System.Math.Max(-60.0, System.Math.Min(60.0, 5.0 * z));
        //    return 1.0 / (1.0 + System.Math.Exp(-z));
        //}

        public static double Sigmoid2(double value)
        {
            double k = Math.Exp(value * -4.9f);
            return (1f / (1.0f + k));
        }

        public static double SigmoidSteep(double value)
        {
            double k = Math.Exp(value * -4.9);
            return 1.0/(1.0 + k);
        }
        public static double Sigmoid(double value)
        {
            double k = Math.Exp(value);
            return k / (1.0f + k);
        }

        public static double Tanh(double value)
        {
            double k = Math.Exp(value * -4.9);
            return (2f / (1.0f + k)) - 1;
        }

        //Rectified Linear Unit
        public static double ReLU(double x)
        {
            return Math.Max(0, x);// x < 0 ? 0 : x;
        }
        public static double DReLU(double x)
        {
            return Math.Max(0, 1);// x < 0 ? 0 : x;
        }
        //Parameteric Rectified Linear Unit 
        public static double PReLU(double x, double a)
        {
            return x < 0 ? a * x : x;
        }
        public static double DPReLU(double x, double a)
        {
            return x < 0 ? a : 1;
        }

        public static double Clamp(double value, double min, double max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        public static float Clamp(float value, float min, float max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}