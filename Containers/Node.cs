﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Container
{
    /// <summary>
    /// Represents a superset of binary data which usually comes from a Media File.
    /// </summary>
    public class Node : Common.BaseDisposable
    {
        /// <summary>
        /// The <see cref="IMediaContainer"/> from which this instance was created.
        /// </summary>
        readonly IMediaContainer Master;

        /// <summary>
        /// The Offset in which the Node occurs in the <see cref="Master"/>
        /// </summary>
        public readonly long Offset;
            
        /// <summary>
        /// The amount of bytes contained in the Node's <see cref="RawData" />
        /// </summary>
        public readonly long DataSize;

        /// <summary>
        /// The amount of bytes used to describe the <see cref="Identifer"/> of the Node.
        /// </summary>
        public readonly int IdentifierSize;

        /// <summary>
        /// The amount of bytes used to describe the <see cref="DataSize"/> of the Node.
        /// </summary>
        public readonly int LengthSize;

        /// <summary>
        /// The Total amount of bytes in the Node including the <see cref="Identifer"/> and <see cref="LengthSize"/>
        /// </summary>
        public long TotalSize { get { return DataSize + IdentifierSize + LengthSize; } }

        byte[] m_Data;

        /// <summary>
        /// The binary data of the Node, in some instances the Identifier and Length are contained and preceed the value.
        /// </summary>
        public byte[] RawData
        {
            get
            {
                if (m_Data != null) return m_Data;
                else if (DataSize <= 0 || Master.BaseStream == null) return Utility.Empty;

                //If data is larger then a certain amount then it may just make sense to return the data itself?

                //Slow, use from cached somehow
                long offsetPrevious = Master.BaseStream.Position;

                Master.BaseStream.Position = Offset;

                m_Data = new byte[DataSize];

                Master.BaseStream.Read(m_Data, 0, (int)DataSize);

                Master.BaseStream.Position = offsetPrevious;

                return m_Data;
            }
            //set
            //{
            //    int write;
            //    if (Size > 0 && value != null && (write = value.Length) > 0) using (var stream = Data) stream.Write(value, 0, write);
            //}
        }

        /// <summary>
        /// Provides a <see cref="System.IO.MemoryStream"/> to <see cref="RawData"/>
        /// </summary>
        public System.IO.MemoryStream DataStream
        {
            get
            {
                return new System.IO.MemoryStream(RawData);
            }
        }

        /// <summary>
        /// Indicates if this Node instance contains all requried data. (could calculate)
        /// </summary>
        public readonly bool IsComplete;

        /// <summary>
        /// Identifies this Node instance.
        /// </summary>
        public readonly byte[] Identifier;

        /// <summary>
        /// Constucts a Node instance from the given parameters
        /// </summary>
        /// <param name="master"></param>
        /// <param name="identifier"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="complete"></param>
        public Node(IMediaContainer master, byte[] identifier, int lengthSize, long offset, long size, bool complete)
        {
            if (master == null) throw new ArgumentNullException("master");
            if (identifier == null) throw new ArgumentNullException("identifier");
            Master = master;
            Offset = offset;
            Identifier = identifier;
            IdentifierSize = identifier.Length;
            LengthSize = lengthSize;
            DataSize = size;
            IsComplete = complete; //Should calulcate here?
        }

        /// <summary>
        /// Writes all <see cref="RawData"/> if <see cref="DataSize"/> is > 0.
        /// </summary>
        public void UpdateData()
        {
            if (DataSize > 0 && m_Data != null)
            {
                //Slow, use from cached somehow
                long offsetPrevious = Master.BaseStream.Position;

                Master.BaseStream.Position = Offset;

                Master.BaseStream.Write(m_Data, 0, (int)DataSize);

                Master.BaseStream.Position = offsetPrevious;
            }
        }

        /// <summary>
        /// Disposes of the resources used by the Node
        /// </summary>
        public override void Dispose()
        {
            if (Disposed) return;

            base.Dispose();

            m_Data = null;
        }

    }
}