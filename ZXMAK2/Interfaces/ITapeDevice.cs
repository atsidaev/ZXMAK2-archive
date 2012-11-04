using System;
using System.Collections.Generic;
using ZXMAK2.Entities;


namespace ZXMAK2.Interfaces
{
	public interface ITapeDevice
	{
		void Play();
		void Stop();
		void Rewind();
		void Reset();       // loaded new image
		bool IsPlay { get; }
		int TactsPerSecond { get; }
		List<TapeBlock> Blocks { get; }
		event EventHandler TapeStateChanged;
		int CurrentBlock { get; set; }
		int Position { get; }
		bool UseTraps { get; set; }
		bool UseAutoPlay { get; set; }
	}

	public enum TapeCommand
	{
		NONE,
		STOP_THE_TAPE,
		STOP_THE_TAPE_48K,
		BEGIN_GROUP,
		END_GROUP,
		SHOW_MESSAGE,
	}
}
