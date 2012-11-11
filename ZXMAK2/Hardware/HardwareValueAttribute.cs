using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Hardware
{
	[System.AttributeUsage(System.AttributeTargets.Property)]
	public class HardwareValueAttribute : System.Attribute
	{
		private string m_name;
		public string Description;


		public HardwareValueAttribute(string name)
		{
			m_name = name;
		}
	}
}
