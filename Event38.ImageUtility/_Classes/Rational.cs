using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event38.ImageUtility._Classes
{
    public class Rational
    {
        uint dem = 0;
        uint num = 0;

        public Rational(double input)
        {
            Value = input;
        }

        public byte[] GetBytes()
        {
            byte[] answer = new byte[8];

            Array.Copy(BitConverter.GetBytes((int)num), 0, answer, 0, sizeof(int));
            Array.Copy(BitConverter.GetBytes((int)dem), 0, answer, 4, sizeof(int));

            return answer;
        }

        public double Value
        {
            get
            {
                return num / dem;
            }
            set
            {
                if ((value % 1.0) != 0)
                {
                    dem = 100; num = (uint)(value * dem);
                }
                else
                {
                    dem = 1; num = (uint)(value);
                }
            }
        }
    }
}
