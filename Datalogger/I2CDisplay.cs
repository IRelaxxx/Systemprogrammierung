using System;

namespace Datalogger
{
    // 10x12 pixel display
    public partial class I2CDisplay
    {
        private readonly I2CDevice dev;

        // 12 Rows with a length of 10
        // Coordinate origin top right
        // 12                  0
        // <-------------------
        //                    | 0
        //                    |
        //                    | 0xff <- die ersten 8 leds
        //                    |
        //                    | 0x03 <- die untern 2 leds an
        //                    \/ 10
        private bool[,] pixels = new bool[12, 10];

        private readonly bool[][,] symbols = new bool[17][,];
        private int offset = 11;
        private int offsetLow = 11;

        public I2CDisplay(byte address)
        {
            dev = new I2CDevice(address);
            // 0
            symbols[0] = new bool[3, 5];
            symbols[0][0, 1] = true;
            symbols[0][0, 2] = true;
            symbols[0][0, 3] = true;
            symbols[0][1, 0] = true;
            symbols[0][1, 4] = true;
            symbols[0][2, 1] = true;
            symbols[0][2, 2] = true;
            symbols[0][2, 3] = true;

            // 1
            symbols[1] = new bool[3, 5];
            symbols[1][1, 0] = true;
            symbols[1][1, 1] = true;
            symbols[1][1, 2] = true;
            symbols[1][1, 3] = true;
            symbols[1][1, 4] = true;

            // 2
            symbols[2] = new bool[3, 5];
            symbols[2][0, 3] = true;
            symbols[2][1, 0] = true;
            symbols[2][1, 2] = true;
            symbols[2][1, 4] = true;
            symbols[2][2, 1] = true;
            symbols[2][2, 4] = true;

            // 3
            symbols[3] = new bool[3, 5];
            symbols[3][1, 0] = true;
            symbols[3][1, 2] = true;
            symbols[3][1, 4] = true;
            symbols[3][2, 1] = true;
            symbols[3][2, 3] = true;

            // 4
            symbols[4] = new bool[3, 5];
            symbols[4][0, 0] = true;
            symbols[4][0, 1] = true;
            symbols[4][1, 2] = true;
            symbols[4][2, 0] = true;
            symbols[4][2, 1] = true;
            symbols[4][2, 2] = true;
            symbols[4][2, 3] = true;
            symbols[4][2, 4] = true;

            // 5
            symbols[5] = new bool[3, 5];
            symbols[5][0, 1] = true;
            symbols[5][0, 4] = true;
            symbols[5][1, 0] = true;
            symbols[5][1, 2] = true;
            symbols[5][1, 4] = true;
            symbols[5][2, 0] = true;
            symbols[5][2, 2] = true;
            symbols[5][2, 3] = true;
            symbols[5][2, 4] = true;

            // 6
            symbols[6] = new bool[3, 5];
            symbols[6][0, 1] = true;
            symbols[6][0, 2] = true;
            symbols[6][0, 3] = true;
            symbols[6][1, 0] = true;
            symbols[6][1, 2] = true;
            symbols[6][1, 4] = true;
            symbols[6][2, 3] = true;

            // 7
            symbols[7] = new bool[3, 5];
            symbols[7][0, 0] = true;
            symbols[7][1, 0] = true;
            symbols[7][2, 1] = true;
            symbols[7][2, 2] = true;
            symbols[7][2, 3] = true;
            symbols[7][2, 4] = true;

            // 8
            symbols[8] = new bool[3, 5];
            symbols[8][0, 1] = true;
            symbols[8][0, 3] = true;
            symbols[8][1, 0] = true;
            symbols[8][1, 2] = true;
            symbols[8][1, 4] = true;
            symbols[8][2, 1] = true;
            symbols[8][2, 3] = true;

            // 9
            symbols[9] = new bool[3, 5];
            symbols[9][0, 1] = true;
            symbols[9][1, 0] = true;
            symbols[9][1, 2] = true;
            symbols[9][1, 4] = true;
            symbols[9][2, 1] = true;
            symbols[9][2, 2] = true;
            symbols[9][2, 3] = true;

            // -
            symbols[10] = new bool[3, 5];
            symbols[10][0, 2] = true;
            symbols[10][1, 2] = true;
            symbols[10][2, 2] = true;

            // .
            symbols[11] = new bool[3, 5];
            symbols[11][1, 4] = true;

            // circ
            symbols[12] = new bool[4, 5];
            symbols[12][0, 1] = true;
            symbols[12][1, 0] = true;
            symbols[12][1, 2] = true;
            symbols[12][2, 1] = true;

            // C
            symbols[13] = new bool[4, 5];
            symbols[13][0, 1] = true;
            symbols[13][0, 2] = true;
            symbols[13][0, 3] = true;
            symbols[13][1, 0] = true;
            symbols[13][1, 4] = true;
            symbols[13][2, 0] = true;
            symbols[13][2, 4] = true;

            // h
            symbols[14] = new bool[4, 5];
            symbols[14][0, 0] = true;
            symbols[14][0, 1] = true;
            symbols[14][0, 2] = true;
            symbols[14][0, 3] = true;
            symbols[14][0, 4] = true;
            symbols[14][1, 2] = true;
            symbols[14][2, 2] = true;
            symbols[14][2, 3] = true;
            symbols[14][2, 4] = true;

            // P
            symbols[15] = new bool[4, 5];
            symbols[15][0, 0] = true;
            symbols[15][0, 1] = true;
            symbols[15][0, 2] = true;
            symbols[15][0, 3] = true;
            symbols[15][0, 4] = true;
            symbols[15][1, 0] = true;
            symbols[15][1, 2] = true;
            symbols[15][2, 1] = true;

            // a
            symbols[16] = new bool[4, 5];
            symbols[16][0, 3] = true;
            symbols[16][0, 4] = true;
            symbols[16][1, 2] = true;
            symbols[16][1, 4] = true;
            symbols[16][2, 2] = true;
            symbols[16][2, 3] = true;
            symbols[16][2, 4] = true;
        }

        ~I2CDisplay()
        {
            clear();
        }

        private int getIndex(char input)
        {
            switch (input)
            {
                case '0': return 0;
                case '1': return 1;
                case '2': return 2;
                case '3': return 3;
                case '4': return 4;
                case '5': return 5;
                case '6': return 6;
                case '7': return 7;
                case '8': return 8;
                case '9': return 9;
                case '-': return 10;
                case '.': return 11;
                case '<': return 12; // circ
                case 'C': return 13;
                case 'h': return 14;
                case 'P': return 15;
                case 'a': return 16;
            }
            return -1;
        }

        public void clear()
        {
            Console.WriteLine("clear");
            for (int x = 0; x < pixels.GetLength(0); x++)
            {
                for (int y = 0; y < pixels.GetLength(1); y++)
                {
                    pixels[x, y] = false;
                }
            }
            flush();
        }

        // TODO: prevent the last symbol from being a dot
        public void write(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int index = getIndex(text[i]);
                writeChar(index, 0, offset);
                offset -= symbols[index].GetLength(0);
            }
        }

        public void writeLow(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int index = getIndex(text[i]);
                writeChar(index, 5, offsetLow);
                offsetLow -= symbols[index].GetLength(0);
            }
        }

        private void writeChar(int input, int offsetY, int offsetX)
        {
            if (offsetX < 0) return;
            for (int x = 0; x < symbols[input].GetLength(0); x++)
            {
                for (int y = 0; y < symbols[input].GetLength(1); y++)
                {
                    pixels[-x + offsetX, y + offsetY] = symbols[input][x, y];
                }
            }
        }

        public void flush()
        {
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    Console.Write(pixels[i, j] ? "1" : "0");
                }
                Console.WriteLine("");
            }

            // write data

            byte addr = 0;
            for (int i = 0; i < 12; i++)
            {
                uint dataBytes = 0;
                for (int j = 9; j >= 0; j--)
                {
                    dataBytes <<= 1;
                    if (pixels[i, j]) dataBytes++;
                }
                /*byte[] data = new byte[3];
                data[0] = addr;
                data[1] = (byte)(dataBytes & 0b1111_1111);
                data[2] = (byte)((dataBytes >> 8) & 0b1111_1111);
                //int ret = dev.writeData(3, data);*/
                byte[] data = new byte[2];
                data[0] = (byte)(dataBytes & 0b1111_1111);
                data[1] = (byte)((dataBytes >> 8) & 0b1111_1111);
                //int ret = dev.writeData(3, data);*/
                int ret = dev.i2c_reg_write(addr, data, 2);
                addr += 2;
            }

            // reset data
            offset = 11;
            offsetLow = 11;

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