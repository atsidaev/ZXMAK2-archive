using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;

﻿﻿namespace ZXMAK2.Engine
{
	public interface IRzxFrameSource
	{
		RzxFrame[] GetNextFrameArray();
	}

	public class RzxFrame
	{
		public int FetchCount { get; set; }
		public byte[] InputData { get; set; }
	}

	public class RzxHandler : IRzxState
	{
		private Z80CPU m_cpu;
		private BusManager m_busMgr;

		public bool IsPlayback;
		public bool IsRecording;

		private RzxFrame[] m_frameArray;
		private int m_playFrame;
		private int m_playIndex;
		private IRzxFrameSource m_frameSource;

		public RzxHandler(Z80CPU cpu, BusManager busMgr)
		{
			m_cpu = cpu;
			m_busMgr = busMgr;
			IsPlayback = false;
			IsRecording = false;
		}

		public void Play(IRzxFrameSource frameSource)
		{
			m_frameSource = frameSource;
			IsPlayback = false; // avoid reenter for CheckInt
			m_frameArray = frameSource.GetNextFrameArray();
			m_playFrame = 0;
			m_playIndex = 0;
			m_cpu.RzxCounter = 0;
			IsPlayback = m_frameArray != null && m_frameArray.Length > 0;
			IsRecording = false;
		}

		public byte GetInput()
		{
			//if (m_playFrame==117 && m_playIndex==6)
			//{
			//    Spec.AddBreakpoint(0x0296);
			//}

			RzxFrame frame = m_frameArray[m_playFrame];
			if (m_playIndex < frame.InputData.Length)
			{
				//LogAgent.Info("RZX: get {0}:{1}=#{2:X2}  RZX={3} PC=#{4:X4}", m_playFrame, m_playIndex, frame.IOData[m_playIndex], m_cpu.RzxCounter, m_cpu.regs.PC);
				return frame.InputData[m_playIndex++];
			}
			LogAgent.Warn("RZX: frame {0}  RZX={1} PC=#{2:X4} - unexpected end of input", m_playFrame, m_cpu.RzxCounter, m_cpu.regs.PC);
			DialogProvider.Show(
				string.Format("RZX playback stopped!\nReason: unexpected end of input - synchronization lost!\n\nFrame:\t{0}\nFetch:\t{1}\nPC:\t#{2:X4}", m_playFrame, m_cpu.RzxCounter, m_cpu.regs.PC),
				"RZX",
				DlgButtonSet.OK,
				DlgIcon.Error);
			IsPlayback = false;
			return m_cpu.BUS;
		}

		public void SetInput(byte value)
		{
		}

		public void Reset()
		{
			IsPlayback = false;
			IsRecording = false;
		}

		public bool CheckInt(int frameTact)
		{
			if (!IsPlayback)
				return false;
			RzxFrame frame = m_frameArray[m_playFrame];
			if (m_cpu.RzxCounter < frame.FetchCount)
				return false;
			if (m_cpu.FX != OPFX.NONE || m_cpu.XFX != OPXFX.NONE)
				return true;
			//LogAgent.Info("RZX: ---- int  RZX={0} PC=#{1:X4} ----", m_cpu.RzxCounter, m_cpu.regs.PC, m_busMgr.GetFrameTact());
			if (m_playIndex != frame.InputData.Length)
			{
				LogAgent.Error("RZX: frame {0} interrupt at pos {1} of {2}", m_playFrame, m_playIndex, frame.InputData.Length);
				DialogProvider.Show(
					string.Format("RZX playback stopped!\nReason: unexpected frame - synchronization lost!\n\nFrame:\t{0}\nPosition:\t{1}\nTotal:\t{2}", m_playFrame, m_playIndex, frame.InputData.Length),
					"RZX",
					DlgButtonSet.OK,
					DlgIcon.Error);
				IsPlayback = false;
				return false;
			}
			m_playIndex = 0;
			m_cpu.RzxCounter -= frame.FetchCount;
			if (++m_playFrame >= m_frameArray.Length)
			{
				Play(m_frameSource);
				if (!IsPlayback)
				{
					DialogProvider.Show("RZX playback end", "RZX", DlgButtonSet.OK, DlgIcon.Information);
				}
			}
			return true;
		}

		#region IRzxState Members

		bool IRzxState.IsPlayback { get { return IsPlayback; } }
		bool IRzxState.IsRecording { get { return IsRecording; } }
		public int Frame { get { return IsPlayback ? m_playFrame : 0; } }
		public int Fetch { get { return IsPlayback ? m_cpu.RzxCounter : 0; } }
		public int Input { get { return IsPlayback ? m_playIndex : 0; } }
		public int FrameCount { get { return IsPlayback ? m_frameArray.Length : 0; } }
		public int FetchCount { get { return IsPlayback ? m_frameArray[m_playFrame].FetchCount : 0; } }
		public int InputCount { get { return IsPlayback ? m_frameArray[m_playFrame].InputData.Length : 0; } }

		#endregion
	}
}
