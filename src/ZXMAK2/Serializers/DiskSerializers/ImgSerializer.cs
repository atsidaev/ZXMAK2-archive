// IMG serializer
// Author: zx.pk.ru\Дмитрий
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using ZXMAK2.Entities;


namespace ZXMAK2.Serializers.DiskSerializers
{
    public class ImgSerializer:FormatSerializer
    {
        private DiskImage _diskImage;
        private byte[] il = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

        public ImgSerializer(DiskImage diskImage)
        {
            this._diskImage = diskImage;
            //_diskImage.Init
        }

        public override void Deserialize(Stream stream)
        {
            this.loadFromStream(stream);
            this._diskImage.ModifyFlag = ModifyFlag.None;
            this._diskImage.Present = true;
        }

        public void format_HD()
        {
            for (int i = 0; i < this._diskImage.CylynderCount; i++)
            {
//                Track[] trackArray = this._diskImage this._cylynderList[i];
                
                for (int m = 0; m < _diskImage.SideCount; m++)
                {
                    ArrayList sectorList = new ArrayList();
                    for (int n = 0; n < 0x12; n++)
                    {
                        SimpleSector sector = new SimpleSector(i, m, this.il[n], 2, new byte[0x200]);
                        for (int num4 = 0; num4 < 0x200; num4++)
                        {
                            sector.Data[num4] = 0;
                        }
                        sector.SetAdCrc(true);
                        sector.SetDataCrc(true);
                        sectorList.Add(sector);
                    }
                    

                    _diskImage.GetTrackImage(i, m).AssignSectors(sectorList);
//                    trackArray[m].AssignSectors(sectorList);
                }
            }
            _diskImage.ModifyFlag = ZXMAK2.Entities.ModifyFlag.None;
        }


        private void loadFromStream(Stream stream)
        {
            int cylynderCount = ((int)stream.Length) / 0x4800;
//            int ti;
            if ((stream.Length % 0x4800L) > 0L)
            {
                this._diskImage.SetPhysics(cylynderCount + 1, 2);
            }
            else
            {
                this._diskImage.SetPhysics(cylynderCount, 2);
            }
            //this._diskImage.format_trdos();
            //this._diskImage.Format(); //?
            //this._diskImage.SetPhysics(80, 2);
//            this._diskImage.GetTrackImage(0,0).AssignSectors(
            //this.format_HD();
            for (int i = 0; i < this._diskImage.CylynderCount; i++)
            {
                for (int m = 0; m < _diskImage.SideCount; m++)
                {
                    ArrayList sectorList = new ArrayList();
                    for (int n = 0; n < 0x12; n++)
                    {
                        SimpleSector sector = new SimpleSector(i, m, this.il[n], 2, new byte[0x200]);
//                        byte[] buffer = new byte[0x200];
                        stream.Read(sector.Data, 0, 0x200);
//                        sector.Data = buffer;
                        sector.SetAdCrc(true);
                        sector.SetDataCrc(true);
                        sectorList.Add(sector);
                    }
                    _diskImage.GetTrackImage(i, m).AssignSectors(sectorList);
                    //_diskImage.GetTrackImage(i, m).
                }
            }
            _diskImage.ModifyFlag = ZXMAK2.Entities.ModifyFlag.None;

/*            for (int i = 0; stream.Position < stream.Length; i += 0x200)
            {
                byte[] buffer = new byte[0x200];
                stream.Read(buffer, 0, 0x200);
                //this._diskImage.writeLogicalSector(i >> 13, (i >> 12) & 1, ((i >> 8) & 15) + 1, buffer);
                ti = i >> 9;
                //this._diskImage.writeLogicalSector(ti / 36, (ti / 18) & 1, (ti - ((ti / 18) * 18)) + 1,buffer);
                this._diskImage.writeLogicalSector(ti / 36, (ti / 18) & 1, (ti % 18) + 1, buffer);

            }*/
        }

        private void saveToStream(Stream stream)
        {
            int ti;
            for (int i = 0; i < (0x4800 * this._diskImage.CylynderCount); i += 0x200)
            {
                byte[] buffer = new byte[0x200];
                //this._diskImage.readLogicalSector(i >> 13, (i >> 12) & 1, ((i >> 8) & 15) + 1, buffer);
                ti = i >> 9;
                this._diskImage.readLogicalSector(ti / 36, (ti / 18) & 1, (ti % 18) + 1, buffer);
                //this._diskImage.readLogicalSector(ti / 36, (ti / 18) & 1, (ti - ((ti / 18) * 18)) + 1, buffer);
                stream.Write(buffer, 0, 0x200);
            }
        }

        public override void Serialize(Stream stream)
        {
            this.saveToStream(stream);
            this._diskImage.ModifyFlag = ModifyFlag.None;
        }

        public override void SetReadOnly(bool readOnly)
        {
            this._diskImage.IsWP = readOnly;
        }

        public override void SetSource(string fileName)
        {
            this._diskImage.FileName = fileName;
        }

        public override bool CanDeserialize
        {
            get
            {
                return true;
            }
        }

        public override bool CanSerialize
        {
            get
            {
                return true;
            }
        }

        public override string FormatExtension
        {
            get
            {
                return "IMG";
            }
        }

        public override string FormatGroup
        {
            get
            {
                return "Disk images";
            }
        }

        public override string FormatName
        {
            get
            {
                return "IMG disk image";
            }
        }



    }
}
