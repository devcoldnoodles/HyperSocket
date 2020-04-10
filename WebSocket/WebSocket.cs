using System;
using System.Text;
using System.Security.Cryptography;

namespace HyperSocket.WebSocket
{
    public class WebSocket : WebClient
    {
        internal static string decoding(byte[] buf, long offset, long size)
        {
            if (buf.Length < 3)
                return null;
            int second = buf[1] & 127;
            int maskIndex;
            int messageLength;
            if (second < 126)
            {
                maskIndex = 2;
                messageLength = second;
            }
            else if (second == 126)
            {
                maskIndex = 4;
                messageLength = (buf[2] << 8) | buf[3];
            }
            else
            {
                maskIndex = 10;
                messageLength = buf[2] << 56 | buf[3] << 48 | buf[4] << 40 | buf[5] << 32 | buf[6] << 24 | buf[7] << 16 | buf[8] << 8 | buf[9];
            }
            byte[] decode = new byte[size - maskIndex - 4];
            for (int idx = 0; idx < size - maskIndex - 4; ++idx)
                decode[idx] = (byte)(buf[idx + maskIndex - 4] ^ buf[idx % 4 + maskIndex]);
            return Encoding.UTF8.GetString(decode, 0, decode.Length);
        }

        public int ReadSync(byte[] buffer, int offset, int size)
        {
            byte[] temp = new byte[size];
            int recv = socket.Receive(temp, offset, size, SocketFlags.None);
            if (recv < 3)
                return -1;
            int second = temp[1] & 127;
            int masked;
            int length;
            if (second < 126)
            {
                masked = 2;
                length = second;
            }
            else if (second == 126)
            {
                masked = 4;
                length = (temp[2] << 8) | temp[3];
            }
            else
            {
                masked = 10;
                length = temp[2] << 56 | temp[3] << 48 | temp[4] << 40 | temp[5] << 32 | temp[6] << 24 | temp[7] << 16 | temp[8] << 8 | temp[9];
            }
            for (int index = 0; index < length; ++index)
                buffer[offset + index] = (byte)(temp[index + masked + 4] ^ temp[masked + index % 4]);
            return length;
        }

        private static byte[] encoding(byte frame, string msg)
        {
            byte[] binary = Encoding.UTF8.GetBytes(msg);
            byte[] temp = null;
            int binary_Size = binary.Length;
            int frameSize = 0;
            if (binary.Length <= 126)
            {
                frameSize = 2;
                temp = new byte[binary_Size + frameSize];
                temp[1] = (byte)binary_Size;
            }
            else if (binary.Length <= 65532)
            {
                frameSize = 4;
                temp = new byte[binary_Size + frameSize];
                temp[1] = 126;
            }
            else
            {
                frameSize = 10;
                temp = new byte[binary_Size + frameSize];
                temp[1] = 127;
            }
            temp[0] = frame;
            for (int index = 2; index < frameSize; ++index)
                temp[index] = (byte)((binary_Size >> (frameSize - index - 1) * 8) & 255);
            for (int index = 0; index < binary_Size; ++index)
                temp[index + frameSize] = binary[index];
            return temp;
        }


        public bool GET(HttpRequest request, HttpResponse response)
        {
            if (request.Upgrade.ToLower() == "websocket")
            {
                response.StatusCode = HttpStatusCode.SwitchingProtocol;
                response.Upgrade = "WebSocket";
                response.Connection = "Upgrade";
                response.SecWebSocketAccept = Convert.ToBase64String(SHA1CryptoServiceProvider.Create().ComputeHash(Encoding.UTF8.GetBytes(request.SecWebSocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11")));
                return true;
            }
            return false;
        }
        public abstract void RunningThread(WebSocketRequest request, WebSocketResponse response);
    }
}