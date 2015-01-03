using System;
using System.Collections.Generic;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Entities
{
	public class TapeBlock
	{
		public string Description;
		public int TzxId = -1;
		public List<int> Periods = new List<int>();
		public byte[] TapData = null;
		public TapeCommand Command = TapeCommand.NONE;
	}
}
