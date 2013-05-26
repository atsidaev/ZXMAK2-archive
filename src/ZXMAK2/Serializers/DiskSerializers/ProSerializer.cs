using System;
using System.Collections.Generic;
using System.IO;
using ZXMAK2.Entities;


namespace ZXMAK2.Serializers.DiskSerializers
{
    /// <summary>
    /// PROFI disk image 80 track with 5 x 1024kb sectors = 819200kb
    /// Track 000 Side 0 1,2,3,4,9
    /// Track 001 Side 1 1,2,3,4,5
    /// ...
    /// Track 159 Side 1 1,2,3,4,5
    /// </summary>
    public class ProSerializer : FormatSerializer
    {
        #region Fields

        private DiskImage m_diskImage;

        private static byte[] s_secMap0 = new byte[] { 1, 2, 3, 4, 9 };
        private static byte[] s_secMapX = new byte[] { 1, 2, 3, 4, 5 };

        #endregion


        #region Properties

        public override string FormatExtension
        {
            get { return "PRO"; }
        }

        public override string FormatGroup
        {
            get { return "Disk images"; }
        }

        public override string FormatName
        {
            get { return "PRO disk image"; }
        }

        public override bool CanDeserialize
        {
            get { return true; }
        }

        public override bool CanSerialize
        {
            get { return true; }
        }

        #endregion


        #region Public

        public ProSerializer(DiskImage diskImage)
        {
            m_diskImage = diskImage;
        }

        public override void Deserialize(Stream stream)
        {
            LoadFromStream(stream);
            m_diskImage.ModifyFlag = ModifyFlag.None;
            m_diskImage.Present = true;
        }

        public override void Serialize(Stream stream)
        {
            SaveToStream(stream);
            m_diskImage.ModifyFlag = ModifyFlag.None;
        }

        public override void SetReadOnly(bool readOnly)
        {
            m_diskImage.IsWP = readOnly;
        }

        public override void SetSource(string fileName)
        {
            m_diskImage.FileName = fileName;
        }

        #endregion Public


        #region Private

        private void LoadFromStream(Stream stream)
        {
            var cylSize = s_secMap0.Length * 0x400 * 2;
            var cylCount = (int)(stream.Length / cylSize);
            if ((stream.Length % cylSize) > 0L)
            {
                cylCount += 1;
            }
            m_diskImage.SetPhysics(cylCount, 2);

            for (var c = 0; c < m_diskImage.CylynderCount; c++)
            {
                for (var h = 0; h < m_diskImage.SideCount; h++)
                {
                    var sectorList = new List<Sector>();
                    var il = (c == 0 && h == 0) ? s_secMap0 : s_secMapX;
                    for (var s = 0; s < il.Length; s++)
                    {
                        var sector = new SimpleSector(
                            c,
                            h,
                            il[s],
                            3,
                            new byte[0x400]);
                        stream.Read(sector.Data, 0, sector.Data.Length);
                        sector.SetAdCrc(true);
                        sector.SetDataCrc(true);
                        sectorList.Add(sector);
                    }
                    m_diskImage
                        .GetTrackImage(c, h)
                        .AssignSectors(sectorList);
                }
            }
            m_diskImage.ModifyFlag = ModifyFlag.None;
        }

        private void SaveToStream(Stream stream)
        {
            // save at least 80 cylinders
            var cylCount = m_diskImage.CylynderCount < 80 ?
                80 : m_diskImage.CylynderCount;
            for (var c = 0; c < cylCount; c++)
            {
                for (var h = 0; h < 2; h++)
                {
                    var il = (c == 0 && h == 0) ? s_secMap0 : s_secMapX;
                    for (var s = 0; s < il.Length; s++)
                    {
                        var buffer = new byte[0x400];
                        if (c < m_diskImage.CylynderCount)
                        {
                            m_diskImage
                                .ReadLogicalSector(c, h, il[s], buffer);
                        }
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
            }
        }

        #endregion Private
    }
}
