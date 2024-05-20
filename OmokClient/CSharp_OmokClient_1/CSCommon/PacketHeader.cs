using MessagePack;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSCommon
{
    public struct PacketHeaderInfo
    {
        const int StartPos = 0;
        public const int HeadSize = 4;

        public UInt16 TotalSize;
        public int ID;

        public static UInt16 GetTotalSize(byte[] data, int startPos)
        {
            return FastBinaryRead.UInt16(data, startPos + StartPos);
        }
                
        /*public static void Write(byte[] data, UInt16 totalSize, UInt16 packetID)
        {
            FastBinaryWrite.UInt16(data, StartPos, totalSize);
            FastBinaryWrite.UInt16(data, StartPos + 2, packetID);
        }

        public static UInt16 ReadPacketID(byte[] data)
        {
            return FastBinaryRead.UInt16(data, StartPos + 2);
        }

        public static UInt16 ReadPacketID(byte[] data , int offset)
        {
            return FastBinaryRead.UInt16(data, offset + StartPos + 2);
        }*/

        public void Read(byte[] headerData)
        {
            var pos = StartPos;

            TotalSize = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

            ID = FastBinaryRead.UInt16(headerData, pos);
            pos += 2;

        }

        /*public void Write(byte[] packetData)
        {
            var pos = StartPos;

            FastBinaryWrite.UInt16(packetData, pos, TotalSize);
            pos += 2;

            FastBinaryWrite.UInt16(packetData, pos, ID);
            pos += 2;
        }

        public static void Write(byte[] packetData, UInt16 totalSize, UInt16 id, byte type)
        {
            var pos = StartPos;

            FastBinaryWrite.UInt16(packetData, pos, totalSize);
            pos += 2;

            FastBinaryWrite.UInt16(packetData, pos, id);
            pos += 2;
        }

        public byte[] Write()
        {
            var packetData = new byte[HeadSize];
            var pos = StartPos;

            FastBinaryWrite.UInt16(packetData, pos, TotalSize);
            pos += 2;

            FastBinaryWrite.UInt16(packetData, pos, ID);
            pos += 2;

            return packetData;
        }*/
    }
}
