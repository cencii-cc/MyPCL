using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MyPCL.ViewModules
{
    /// <summary>
    /// 支持小数与常见类型隐式转换的颜色。
    /// </summary>
    public class MyColor
    {
        public double A = 255;
        public double R = 0;
        public double G = 0;
        public double B = 0;

        // 类型转换
        public static implicit operator MyColor(string str)
        {
            return new MyColor(str);
        }

        public static implicit operator MyColor(Color col)
        {
            return new MyColor(col);
        }

        public static implicit operator Color(MyColor conv)
        {
            return Color.FromArgb(MathByte(conv.A), MathByte(conv.R), MathByte(conv.G), MathByte(conv.B));
        }

        public static implicit operator System.Drawing.Color(MyColor conv)
        {
            return System.Drawing.Color.FromArgb(MathByte(conv.A), MathByte(conv.R), MathByte(conv.G), MathByte(conv.B));
        }

        public static implicit operator MyColor(SolidColorBrush bru)
        {
            return new MyColor(bru.Color);
        }

        public static implicit operator SolidColorBrush(MyColor conv)
        {
            return new SolidColorBrush(Color.FromArgb(MathByte(conv.A), MathByte(conv.R), MathByte(conv.G), MathByte(conv.B)));
        }

        public static implicit operator MyColor(Brush bru)
        {
            return new MyColor(bru);
        }

        public static implicit operator Brush(MyColor conv)
        {
            return new SolidColorBrush(Color.FromArgb(MathByte(conv.A), MathByte(conv.R), MathByte(conv.G), MathByte(conv.B)));
        }

        // 颜色运算
        public static MyColor operator +(MyColor a, MyColor b)
        {
            return new MyColor
            {
                A = a.A + b.A,
                B = a.B + b.B,
                G = a.G + b.G,
                R = a.R + b.R
            };
        }

        public static MyColor operator -(MyColor a, MyColor b)
        {
            return new MyColor
            {
                A = a.A - b.A,
                B = a.B - b.B,
                G = a.G - b.G,
                R = a.R - b.R
            };
        }

        public static MyColor operator *(MyColor a, double b)
        {
            return new MyColor
            {
                A = a.A * b,
                B = a.B * b,
                G = a.G * b,
                R = a.R * b
            };
        }

        public static MyColor operator /(MyColor a, double b)
        {
            return new MyColor
            {
                A = a.A / b,
                B = a.B / b,
                G = a.G / b,
                R = a.R / b
            };
        }

        public static bool operator ==(MyColor a, MyColor b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.A == b.A && a.R == b.R && a.G == b.G && a.B == b.B;
        }

        public static bool operator !=(MyColor a, MyColor b)
        {
            return !(a == b);
        }

        // 构造函数
        public MyColor() { }

        public MyColor(Color col)
        {
            A = col.A;
            R = col.R;
            G = col.G;
            B = col.B;
        }

        public MyColor(string HexString)
        {
            Color StringColor = (Color)ColorConverter.ConvertFromString(HexString);
            A = StringColor.A;
            R = StringColor.R;
            G = StringColor.G;
            B = StringColor.B;
        }

        public MyColor(double newA, MyColor col)
        {
            A = newA;
            R = col.R;
            G = col.G;
            B = col.B;
        }

        public MyColor(double newR, double newG, double newB)
        {
            A = 255;
            R = newR;
            G = newG;
            B = newB;
        }

        public MyColor(double newA, double newR, double newG, double newB)
        {
            A = newA;
            R = newR;
            G = newG;
            B = newB;
        }

        public MyColor(Brush brush)
        {
            Color Color = ((SolidColorBrush)brush).Color;
            A = Color.A;
            R = Color.R;
            G = Color.G;
            B = Color.B;
        }

        public MyColor(SolidColorBrush brush)
        {
            Color Color = brush.Color;
            A = Color.A;
            R = Color.R;
            G = Color.G;
            B = Color.B;
        }

        public MyColor(object obj)
        {
            if (obj == null)
            {
                A = 255;
                R = 255;
                G = 255;
                B = 255;
            }
            else
            {
                if (obj is SolidColorBrush)
                {
                    Color Color = ((SolidColorBrush)obj).Color;
                    A = Color.A;
                    R = Color.R;
                    G = Color.G;
                    B = Color.B;
                }
                else
                {
                    dynamic dynObj = obj;
                    A = dynObj.A;
                    R = dynObj.R;
                    G = dynObj.G;
                    B = dynObj.B;
                }
            }
        }

        // HSL
        public double Hue(double v1, double v2, double vH)
        {
            if (vH < 0) vH += 1;
            if (vH > 1) vH -= 1;
            if (vH < 0.16667) return v1 + (v2 - v1) * 6 * vH;
            if (vH < 0.5) return v2;
            if (vH < 0.66667) return v1 + (v2 - v1) * (4 - vH * 6);
            return v1;
        }

        public MyColor FromHSL(double sH, double sS, double sL)
        {
            if (sS == 0)
            {
                R = sL * 2.55;
                G = R;
                B = R;
            }
            else
            {
                double H = sH / 360;
                double S = sS / 100;
                double L = sL / 100;
                S = L < 0.5 ? S * L + L : S * (1.0 - L) + L;
                L = 2 * L - S;
                R = 255 * Hue(L, S, H + 1 / 3);
                G = 255 * Hue(L, S, H);
                B = 255 * Hue(L, S, H - 1 / 3);
            }
            A = 255;
            return this;
        }

        public MyColor FromHSL2(double sH, double sS, double sL)
        {
            if (sS == 0)
            {
                R = sL * 2.55;
                G = R;
                B = R;
            }
            else
            {
                // 初始化
                sH = (sH + 3600000) % 360;
                double[] cent = {
                +0.1, -0.06, -0.3, // 0, 30, 60
                -0.19, -0.15, -0.24, // 90, 120, 150
                -0.32, -0.09, +0.18, // 180, 210, 240
                +0.05, -0.12, -0.02, // 270, 300, 330
                +0.1, -0.06 // 最后两位与前两位一致，加是变亮，减是变暗
            };
                // 计算色调对应的亮度片区
                double center = sH / 30.0;
                int intCenter = (int)Math.Floor(center); // 亮度片区编号
                center = 50 - (
                    (1 - center + intCenter) * cent[intCenter] + (center - intCenter) * cent[intCenter + 1]
                ) * sS;
                // center = 50 + (cent[intCenter] + (center - intCenter) * (cent[intCenter + 1] - cent[intCenter])) * sS
                sL = sL < center ? sL / center : 1 + (sL - center) / (100 - center) * 50;
                FromHSL(sH, sS, sL);
            }
            A = 255;
            return this;
        }

        public override string ToString()
        {
            return "(" + A + "," + R + "," + G + "," + B + ")";
        }

        public override bool Equals(object obj)
        {
            return this == (MyColor)obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(A, R, G, B);
        }

        private static byte MathByte(double value)
        {
            return (byte)Math.Max(0, Math.Min(255, value));
        }
    }
}
