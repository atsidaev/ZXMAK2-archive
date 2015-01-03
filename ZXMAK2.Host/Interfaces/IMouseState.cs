using System;


namespace ZXMAK2.Interfaces
{
    public interface IMouseDevice //: BusDeviceBase
    {
		IMouseState MouseState { get; set; }
    }

	public interface IMouseState
	{
		int X { get; }
		int Y { get; }
		int Buttons { get; }
	}
}
