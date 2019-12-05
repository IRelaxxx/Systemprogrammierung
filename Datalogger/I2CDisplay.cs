using System;
using System.Collections.Generic;
using System.Text;

namespace Datalogger
{
    // 10x12 pixel display
    public partial class I2CDisplay
    {
        private readonly I2CDevice dev;
        private bool[,] pixels = new bool[10, 12];
        private readonly bool[][,] symbols = new bool[10][,];
        private int offset = 0;

        public I2CDisplay(int address)
        {
            dev = new I2CDevice(address);
            // 0
            symbols[0] = new bool[3, 10];
            symbols[0][0, 1] = true;
            symbols[0][0, 2] = true;
            symbols[0][0, 3] = true;
            symbols[0][0, 4] = true;
            symbols[0][1, 0] = true;
            symbols[0][1, 5] = true;
            symbols[0][2, 1] = true;
            symbols[0][2, 2] = true;
            symbols[0][2, 3] = true;
            symbols[0][2, 4] = true;

            // 1
            symbols[1] = new bool[3, 10];
            symbols[1][0, 1] = true;
            symbols[1][1, 0] = true;
            symbols[1][2, 0] = true;
            symbols[1][2, 1] = true;
            symbols[1][2, 2] = true;
            symbols[1][2, 3] = true;
            symbols[1][2, 4] = true;
            symbols[1][2, 5] = true;

            // 2
            symbols[2] = new bool[3, 10];
            symbols[2][0, 1] = true;
            symbols[2][0, 4] = true;
            symbols[2][0, 5] = true;
            symbols[2][1, 0] = true;
            symbols[2][1, 3] = true;
            symbols[2][1, 5] = true;
            symbols[2][2, 1] = true;
            symbols[2][2, 2] = true;
            symbols[2][2, 5] = true;

            // 3
            symbols[3] = new bool[3, 10];
            symbols[3][0, 0] = true;
            symbols[3][0, 2] = true;
            symbols[3][0, 5] = true;
            symbols[3][1, 0] = true;
            symbols[3][1, 2] = true;
            symbols[3][1, 5] = true;
            symbols[3][2, 0] = true;
            symbols[3][2, 1] = true;
            symbols[3][2, 2] = true;
            symbols[3][2, 3] = true;
            symbols[3][2, 4] = true;
            symbols[3][2, 5] = true;

            // 4
            symbols[4] = new bool[3, 10];
            symbols[4][0, 2] = true;
            symbols[4][0, 3] = true;
            symbols[4][1, 1] = true;
            symbols[4][1, 3] = true;
            symbols[4][2, 0] = true;
            symbols[4][2, 1] = true;
            symbols[4][2, 2] = true;
            symbols[4][2, 3] = true;
            symbols[4][2, 4] = true;
            symbols[4][2, 5] = true;

            // 5
            symbols[5] = new bool[3, 10];
            symbols[5][0, 0] = true;
            symbols[5][0, 1] = true;
            symbols[5][0, 2] = true;
            symbols[5][0, 5] = true;
            symbols[5][1, 0] = true;
            symbols[5][1, 2] = true;
            symbols[5][1, 5] = true;
            symbols[5][2, 0] = true;
            symbols[5][2, 3] = true;
            symbols[5][2, 4] = true;

            // 6
            symbols[6] = new bool[3, 10];
            symbols[6][0, 1] = true;
            symbols[6][0, 2] = true;
            symbols[6][0, 3] = true;
            symbols[6][0, 4] = true;
            symbols[6][0, 5] = true;
            symbols[6][1, 0] = true;
            symbols[6][1, 2] = true;
            symbols[6][1, 5] = true;
            symbols[6][2, 0] = true;
            symbols[6][2, 2] = true;
            symbols[6][2, 3] = true;
            symbols[6][2, 4] = true;
            symbols[6][2, 5] = true;

            // 7
            symbols[7] = new bool[3, 10];
            symbols[7][0, 0] = true;
            symbols[7][0, 2] = true;
            symbols[7][0, 5] = true;
            symbols[7][1, 0] = true;
            symbols[7][1, 2] = true;
            symbols[7][1, 3] = true;
            symbols[7][1, 4] = true;
            symbols[7][2, 0] = true;
            symbols[7][2, 1] = true;
            symbols[7][2, 2] = true;

            // 8
            symbols[8] = new bool[3, 10];
            symbols[8][0, 1] = true;
            symbols[8][0, 2] = true;
            symbols[8][0, 4] = true;
            symbols[8][1, 0] = true;
            symbols[8][1, 3] = true;
            symbols[8][1, 5] = true;
            symbols[8][2, 1] = true;
            symbols[8][2, 2] = true;
            symbols[8][2, 4] = true;

            // 9
            symbols[9] = new bool[3, 10];
            symbols[9][0, 1] = true;
            symbols[9][0, 5] = true;
            symbols[9][1, 0] = true;
            symbols[9][1, 2] = true;
            symbols[9][1, 5] = true;
            symbols[9][2, 1] = true;
            symbols[9][2, 2] = true;
            symbols[9][2, 3] = true;
            symbols[9][2, 4] = true;
        }

        public void writeNumber(int input)
        {
            if (input > 9) return; // only accept a single number
            if (offset > 9) return; // do not write out of bounds
            for (int x = 0; x < symbols[input].GetLength(0); x++)
            {
                for (int y = 0; y < symbols[input].GetLength(1); y++)
                {
                    pixels[x + offset, y] = symbols[input][x, y];
                }
            }

            offset += 3;
        }

        public void flush()
        {
            offset = 0;

            // write data

            // reset data
            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    pixels[x, y] = false;
                }
            }
        }
    }
}