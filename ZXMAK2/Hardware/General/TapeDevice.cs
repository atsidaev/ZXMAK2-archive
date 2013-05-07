/// Description: Tape emulator
/// Author: Alex Makeev
/// Date: 16.04.2007
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

using ZXMAK2.Serializers;
using ZXMAK2.Interfaces;
using ZXMAK2.Serializers.TapeSerializers;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine;
using ZXMAK2.Entities;


namespace ZXMAK2.Hardware.General
{
	public class TapeDevice : SoundDeviceBase, ITapeDevice, IGuiExtension
	{
		#region IBusDevice

		public override string Name { get { return "Tape Player"; } }
		public override string Description { get { return "Generic Tape Device"; } }
		public override BusDeviceCategory Category { get { return BusDeviceCategory.Tape; } }

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);
			m_cpu = bmgr.CPU;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

			bmgr.SubscribeRdIo(0x0001, 0x0000, readPortFE);

			bmgr.SubscribePreCycle(busPreCycle);

			bmgr.AddSerializer(new TapSerializer(this));
			bmgr.AddSerializer(new TzxSerializer(this));
			bmgr.AddSerializer(new CswSerializer(this));
			bmgr.AddSerializer(new WavSerializer(this));
			bmgr.RegisterIcon(m_iconTape);
		}

		#endregion

		#region ITapeDevice

		public bool UseTraps
		{
			get { return m_trapsAllowed; }
			set { m_trapsAllowed = value; }
		}

		public bool UseAutoPlay
		{
			get { return m_autoPlay; }
			set { m_autoPlay = value; detectorReset(); }
		}

		#endregion

		#region IConfigurable

		public override void LoadConfig(XmlNode itemNode)
		{
			base.LoadConfig(itemNode);
			UseTraps = Utils.GetXmlAttributeAsBool(itemNode, "useTraps", UseTraps);
			UseAutoPlay = Utils.GetXmlAttributeAsBool(itemNode, "useAutoPlay", UseAutoPlay);
		}

		public override void SaveConfig(XmlNode itemNode)
		{
			base.SaveConfig(itemNode);
			Utils.SetXmlAttribute(itemNode, "useTraps", UseTraps);
			Utils.SetXmlAttribute(itemNode, "useAutoPlay", UseAutoPlay);
		}

		#endregion

		protected override void OnVolumeChanged(int oldVolume, int newVolume)
		{
			m_dacValue0 = 0;
			m_dacValue1 = (ushort)((0x1FFF * newVolume) / 100);
		}
		
		#region Bus Handlers

		private void readPortFE(ushort addr, ref byte value, ref bool iorqge)
		{
			if (!iorqge || m_memory.DOSEN)
				return;
			//iorqge = false;

			if (tape_bit(m_cpu.Tact))
				value |= 0x40;
			else
				value &= 0xBF;
			detectorRead();
		}

		private void busPreCycle(int frameTact)
		{
			if (!m_isPlay)
				return;
			//bmgr.SubscribeRDMEM_M1(0xFFFF, 0x056B, tapeTrap);
			//bmgr.SubscribeRDMEM_M1(0xFFFF, 0x059E, tapeTrap);

			ushort val = tape_bit(m_cpu.Tact) ? m_dacValue1 : m_dacValue0;
			UpdateDac(val, val);

			ushort addr = m_cpu.regs.PC;
			if (!UseTraps || !m_memory.IsRom48 || !(addr == 0x056B || addr == 0x059E))
				return;

			TapeBlock tb = Blocks[CurrentBlock];
			if (tb.TapData == null)
				return;

			ushort rIX = m_cpu.regs.IX;
			ushort rDE = m_cpu.regs.DE;
			if (rDE != (tb.TapData.Length - 2))
				return;

			int offset = 0;
			byte crc = 0;
			crc = tb.TapData[offset++];
			for (int i = 0; i < rDE; i++)
			{
				crc ^= tb.TapData[offset];
				m_cpu.WRMEM(rIX++, tb.TapData[offset++]);
			}
			crc ^= tb.TapData[offset];

			m_cpu.regs.PC = 0x05DF;//0x05DF - 1;
			m_cpu.regs.IX = rIX;
			m_cpu.regs.DE = 0;
			m_cpu.regs.H = crc;
			m_cpu.regs.L = tb.TapData[offset];
			m_cpu.regs.BC = 0xB001;

			int newBlock = CurrentBlock + 1;
			if (newBlock < Blocks.Count)
			{
				CurrentBlock = newBlock;
			}
			else
			{
				Stop();
				Rewind();
			}
		}

		protected override void BeginFrame()
		{
			base.BeginFrame();
			m_iconTape.Visible = false;
		}

		protected override void EndFrame()
		{
			//ushort val = tape_bit(m_cpu.Tact) ? m_dacValue1 : m_dacValue0;
			//UpdateDAC(val, val);
			base.EndFrame();
			detectorFrame();
		}
		
		#endregion

		#region private data

		private Z80CPU m_cpu;
		private IMemoryDevice m_memory;
		private bool m_trapsAllowed = true;
		private bool m_autoPlay = true;

		// sound related
		private ushort m_dacValue0 = 0;
		private ushort m_dacValue1 = 0x1FFF;

		// data related
		private int c_Z80FQ = 3500000;

		private List<TapeBlock> m_blocks = new List<TapeBlock>();
		private int m_index = 0;
		private int m_playPosition = 0;

		private bool m_isPlay = false;

		private long m_lastTact = 0;
		private int m_waitEdge = 0;
		private bool m_state = false;

		private IconDescriptor m_iconTape = new IconDescriptor("TAPE", Utils.GetIconStream("cassette.png"));

		#endregion


		public TapeDevice()
		{
			Volume = 20;
		}


		#region Events

		public event EventHandler TapeStateChanged;

		protected virtual void OnTapeStateChanged()
		{
			if (TapeStateChanged != null)
				TapeStateChanged(this, EventArgs.Empty);
		}

		#endregion

		#region Properties

		public List<TapeBlock> Blocks
		{
			get { return m_blocks; }
			set { m_blocks = value; }
		}

		public int CurrentBlock
		{
			get
			{
				if (m_blocks.Count > 0)
					return m_index;
				else
					return -1;
			}
			set
			{
				if (value == m_index)
					return;
				if (value >= 0 && value < m_blocks.Count)
				{
					m_index = value;
					m_playPosition = 0;
					OnTapeStateChanged();
					//if(Play)
					//   _currentBlock = _blocks[_index] as TapeBlock;
				}
			}
		}

		public int Position
		{
			get
			{
				if (m_playPosition >= m_blocks[m_index].Periods.Count)
					return 0;
				return m_playPosition;
			}
		}

		public bool IsPlay
		{
			get { return m_isPlay; }
		}

		public int TactsPerSecond
		{
			get { return c_Z80FQ; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Called on load new image
		/// </summary>
		public void Reset()
		{
			m_index = -1;
			Stop();
		}

		public void Rewind()
		{
			m_index = -1;
			Stop();
		}

		public void Play()
		{
			m_lastTact = m_cpu.Tact;
			m_waitEdge = 0;
			m_playPosition = 0;

			if (m_blocks.Count > 0 && m_index >= 0)
			{
				while (m_playPosition >= m_blocks[m_index].Periods.Count)
				{
					m_playPosition = 0;
					m_index++;
					if (m_index >= m_blocks.Count)
						break;
				}
				if (m_index >= m_blocks.Count)
				{
					// end of tape -> rewind & stop
					m_index = -1;
					Stop();
					return;
				}

				//m_state = !m_state;
				m_waitEdge = m_blocks[m_index].Periods[m_playPosition];
				m_isPlay = true;
				OnTapeStateChanged();
			}
		}

		public void Stop()
		{
			m_isPlay = false;
			if (m_index >= 0 && m_index < m_blocks.Count &&
				m_playPosition >= m_blocks[m_index].Periods.Count - 1)
			{
				m_index++;
				if (m_index >= m_blocks.Count)
					m_index = -1;
			}
			m_lastTact = m_cpu.Tact;
			m_waitEdge = 0;
			m_playPosition = 0;
			if (m_index < 0 && m_blocks.Count > 0)
				m_index = 0;
			OnTapeStateChanged();
		}

		#endregion

		#region private methods

		private bool tape_bit(long globalTact)
		{
			int delta = (int)(globalTact - m_lastTact);

			if (!m_isPlay)
			{
				m_lastTact = globalTact;
				return false;
			}
			if (m_index < 0)
			{
				// end of tape -> rewind & stop
				Stop();
				return m_state;
			}

			while (delta >= m_waitEdge)
			{
				delta -= m_waitEdge;
				m_state = !m_state;

				m_playPosition++;
				if (m_playPosition >= m_blocks[m_index].Periods.Count) // endof block?
				{
					while (m_playPosition >= m_blocks[m_index].Periods.Count)
					{
						// skip empty blocks
						m_playPosition = 0;
						m_index++;
						if (m_index >= m_blocks.Count)
							break;
					}
					if (m_index >= m_blocks.Count)
					{
						// end of tape -> rewind & stop
						m_index = -1;
						Stop();
						return m_state;
					}
					OnTapeStateChanged();
				}
				m_waitEdge = m_blocks[m_index].Periods[m_playPosition];
			}
			m_lastTact = globalTact - (long)delta;
			return m_state;
		}

		#endregion


		#region AutoPlay

		private long m_lastInTact = 0;
		private int m_detectCounter;
		private int m_detectTimeOut;
		private ushort m_lastPC;
		private byte[] m_lastRegs;

		private void detectorReset()
		{
			m_lastInTact = 0;
			m_detectCounter = 0;
			m_lastPC = 0;
			m_lastRegs = null;
		}

		private void detectorRead()
		{
			long cpuTact = m_cpu.Tact;
			int delta = (int)(cpuTact - m_lastInTact);
			m_lastInTact = cpuTact;

			byte[] newRegs = new byte[] {
				m_cpu.regs.A,
				m_cpu.regs.B, m_cpu.regs.C,
				m_cpu.regs.D, m_cpu.regs.E,
				m_cpu.regs.H, m_cpu.regs.L,
			};
			if (delta > 0 && delta < 96 && m_cpu.regs.PC == m_lastPC && m_lastRegs != null)
			{
				int diffCount = 0;
				int diffValue = 0;
				for (int i = 0; i < newRegs.Length; i++)
				{
					if (m_lastRegs[i] != newRegs[i])
					{
						diffValue = m_lastRegs[i] - newRegs[i];
						diffCount++;
					}
				}
				if (diffCount == 1 && (diffValue == 1 || diffValue == -1))
				{
					m_iconTape.Visible = true;
					m_detectCounter++;
					if (m_detectCounter >= 8 && m_autoPlay)
					{
						if (!m_isPlay)
							Play();
						m_detectTimeOut = 50;
					}
				}
				else
				{
					m_detectCounter = 0;
				}
			}
			m_lastRegs = newRegs;
			m_lastPC = m_cpu.regs.PC;
		}

		private void detectorFrame()
		{
			if (m_isPlay && m_autoPlay)
			{
				m_detectTimeOut--;
				if (m_detectTimeOut < 0)
					Stop();
			}
		}

		#endregion


		#region IGuiExtension Members

		private GuiData m_guiData;
		private System.Windows.Forms.MenuItem m_subMenuItem;
		private Controls.TapeForm m_form;

		public void AttachGui(GuiData guiData)
		{
			m_guiData = guiData;
			if (m_guiData.MainWindow is System.Windows.Forms.Form)
			{
				System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
				if (menuItem != null)
				{
					m_subMenuItem = new System.Windows.Forms.MenuItem("Tape", menu_Click);
					menuItem.MenuItems.Add(m_subMenuItem);
				}
			}
		}

		public void DetachGui()
		{
			if (m_guiData.MainWindow is System.Windows.Forms.Form)
			{
				if (m_subMenuItem != null)
				{
					m_subMenuItem.Parent.MenuItems.Remove(m_subMenuItem);
					m_subMenuItem.Dispose();
					m_subMenuItem = null;
				}
				if (m_form != null)
				{
					m_form.Close();
					m_form = null;
				}
			}
			m_guiData = null;
		}

		private void menu_Click(object sender, EventArgs e)
		{
			if (m_guiData.MainWindow is System.Windows.Forms.Form)
			{
				if (m_form == null)
				{
					m_form = new Controls.TapeForm(this);
					m_form.FormClosed += delegate(object obj, System.Windows.Forms.FormClosedEventArgs arg) { m_form = null; };
					m_form.Show((System.Windows.Forms.Form)m_guiData.MainWindow);
				}
				else
				{
					m_form.Show();
					m_form.Activate();
				}
			}
		}

		#endregion
	}
}
