using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Engine.Interfaces
{
    public interface IKeyboardDevice : IBusDevice
    {
		IKeyboardState KeyboardState { get; set; }
    }

	public interface IKeyboardState
	{
		bool this[Key key] { get; }
	}
	
	public enum Key
	{
		D1 = Microsoft.DirectX.DirectInput.Key.D1,
		D2 = Microsoft.DirectX.DirectInput.Key.D2,
		D3 = Microsoft.DirectX.DirectInput.Key.D3,
		D4 = Microsoft.DirectX.DirectInput.Key.D4,
		D5 = Microsoft.DirectX.DirectInput.Key.D5,
		D6 = Microsoft.DirectX.DirectInput.Key.D6,
		D7 = Microsoft.DirectX.DirectInput.Key.D7,
		D8 = Microsoft.DirectX.DirectInput.Key.D8,
		D9 = Microsoft.DirectX.DirectInput.Key.D9,
		D0 = Microsoft.DirectX.DirectInput.Key.D0,

		Q = Microsoft.DirectX.DirectInput.Key.Q,
		W = Microsoft.DirectX.DirectInput.Key.W,
		E = Microsoft.DirectX.DirectInput.Key.E,
		R = Microsoft.DirectX.DirectInput.Key.R,
		T = Microsoft.DirectX.DirectInput.Key.T,
		Y = Microsoft.DirectX.DirectInput.Key.Y,
		U = Microsoft.DirectX.DirectInput.Key.U,
		I = Microsoft.DirectX.DirectInput.Key.I,
		O = Microsoft.DirectX.DirectInput.Key.O,
		P = Microsoft.DirectX.DirectInput.Key.P,
		A = Microsoft.DirectX.DirectInput.Key.A,
		S = Microsoft.DirectX.DirectInput.Key.S,
		D = Microsoft.DirectX.DirectInput.Key.D,
		F = Microsoft.DirectX.DirectInput.Key.F,
		G = Microsoft.DirectX.DirectInput.Key.G,
		H = Microsoft.DirectX.DirectInput.Key.H,
		J = Microsoft.DirectX.DirectInput.Key.J,
		K = Microsoft.DirectX.DirectInput.Key.K,
		L = Microsoft.DirectX.DirectInput.Key.L,
		Z = Microsoft.DirectX.DirectInput.Key.Z,
		X = Microsoft.DirectX.DirectInput.Key.X,
		C = Microsoft.DirectX.DirectInput.Key.C,
		V = Microsoft.DirectX.DirectInput.Key.V,
		B = Microsoft.DirectX.DirectInput.Key.B,
		N = Microsoft.DirectX.DirectInput.Key.N,
		M = Microsoft.DirectX.DirectInput.Key.M,
		Space = Microsoft.DirectX.DirectInput.Key.Space,
		Return = Microsoft.DirectX.DirectInput.Key.Return,

		F1 = Microsoft.DirectX.DirectInput.Key.F1,
		F2 = Microsoft.DirectX.DirectInput.Key.F2,
		F3 = Microsoft.DirectX.DirectInput.Key.F3,
		F4 = Microsoft.DirectX.DirectInput.Key.F4,
		F5 = Microsoft.DirectX.DirectInput.Key.F5,
		F6 = Microsoft.DirectX.DirectInput.Key.F6,
		F7 = Microsoft.DirectX.DirectInput.Key.F7,
		F8 = Microsoft.DirectX.DirectInput.Key.F8,
		F9 = Microsoft.DirectX.DirectInput.Key.F9,
		F10 = Microsoft.DirectX.DirectInput.Key.F10,
		F11 = Microsoft.DirectX.DirectInput.Key.F11,
		F12 = Microsoft.DirectX.DirectInput.Key.F12,
		F13 = Microsoft.DirectX.DirectInput.Key.F13,
		F14 = Microsoft.DirectX.DirectInput.Key.F14,
		F15 = Microsoft.DirectX.DirectInput.Key.F15,

		LeftShift = Microsoft.DirectX.DirectInput.Key.LeftShift,
		RightShift = Microsoft.DirectX.DirectInput.Key.RightShift,
		LeftAlt = Microsoft.DirectX.DirectInput.Key.LeftAlt,
		RightAlt = Microsoft.DirectX.DirectInput.Key.RightAlt,
		LeftControl = Microsoft.DirectX.DirectInput.Key.LeftControl,
		RightControl = Microsoft.DirectX.DirectInput.Key.RightControl,
		LeftMenu = Microsoft.DirectX.DirectInput.Key.LeftMenu,
		RightMenu = Microsoft.DirectX.DirectInput.Key.RightMenu,
		LeftWindows = Microsoft.DirectX.DirectInput.Key.LeftWindows,
		RightWindows = Microsoft.DirectX.DirectInput.Key.RightWindows,

		UpArrow = Microsoft.DirectX.DirectInput.Key.UpArrow,
		LeftArrow = Microsoft.DirectX.DirectInput.Key.LeftArrow,
		RightArrow = Microsoft.DirectX.DirectInput.Key.RightArrow,
		DownArrow = Microsoft.DirectX.DirectInput.Key.DownArrow,

		Insert = Microsoft.DirectX.DirectInput.Key.Insert,
		Delete = Microsoft.DirectX.DirectInput.Key.Delete,
		Home = Microsoft.DirectX.DirectInput.Key.Home,
		End = Microsoft.DirectX.DirectInput.Key.End,
		PageUp = Microsoft.DirectX.DirectInput.Key.PageUp,
		PageDown = Microsoft.DirectX.DirectInput.Key.PageDown,

		Escape = Microsoft.DirectX.DirectInput.Key.Escape,
		Tab = Microsoft.DirectX.DirectInput.Key.Tab,
		Minus = Microsoft.DirectX.DirectInput.Key.Minus,
		Equals = Microsoft.DirectX.DirectInput.Key.Equals,
		BackSpace = Microsoft.DirectX.DirectInput.Key.BackSpace,
		
		CapsLock = Microsoft.DirectX.DirectInput.Key.CapsLock,
		NumPadPlus = Microsoft.DirectX.DirectInput.Key.NumPadPlus,
		NumPadMinus = Microsoft.DirectX.DirectInput.Key.NumPadMinus,
		NumPadStar = Microsoft.DirectX.DirectInput.Key.NumPadStar,
		NumPadSlash = Microsoft.DirectX.DirectInput.Key.NumPadSlash,
		Period = Microsoft.DirectX.DirectInput.Key.Period,
		Comma = Microsoft.DirectX.DirectInput.Key.Comma,
		SemiColon = Microsoft.DirectX.DirectInput.Key.SemiColon,
		Apostrophe = Microsoft.DirectX.DirectInput.Key.Apostrophe,
		Slash = Microsoft.DirectX.DirectInput.Key.Slash,
		LeftBracket = Microsoft.DirectX.DirectInput.Key.LeftBracket,
		RightBracket = Microsoft.DirectX.DirectInput.Key.RightBracket,
		NumPadEnter = Microsoft.DirectX.DirectInput.Key.NumPadEnter,
        BackSlash = Microsoft.DirectX.DirectInput.Key.BackSlash,
        Grave = Microsoft.DirectX.DirectInput.Key.Grave,


        NumPad0 = Microsoft.DirectX.DirectInput.Key.NumPad0,
        NumPad1 = Microsoft.DirectX.DirectInput.Key.NumPad1,
        NumPad2 = Microsoft.DirectX.DirectInput.Key.NumPad2,
        NumPad3 = Microsoft.DirectX.DirectInput.Key.NumPad3,
        NumPad4 = Microsoft.DirectX.DirectInput.Key.NumPad4,
        NumPad5 = Microsoft.DirectX.DirectInput.Key.NumPad5,
        NumPad6 = Microsoft.DirectX.DirectInput.Key.NumPad6,
        NumPad7 = Microsoft.DirectX.DirectInput.Key.NumPad7,
        NumPad8 = Microsoft.DirectX.DirectInput.Key.NumPad8,
        NumPad9 = Microsoft.DirectX.DirectInput.Key.NumPad9,

        NumPadComma = Microsoft.DirectX.DirectInput.Key.NumPadComma,
        NumPadPeriod = Microsoft.DirectX.DirectInput.Key.NumPadPeriod,
	}
}
