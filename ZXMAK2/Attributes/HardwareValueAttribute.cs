using System;

namespace ZXMAK2.Attributes
{
	[AttributeUsage(System.AttributeTargets.Property)]
	public class HardwareValueAttribute : Attribute
	{
		private string m_name;
		public string Description;


		public HardwareValueAttribute(string name)
		{
			m_name = name;
		}
	}
}
