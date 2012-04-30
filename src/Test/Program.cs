using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using ZXMAK2.Engine.Interfaces;

namespace Test
{
	public class Program
	{
		static void Main(string[] args)
		{
            if (args.Length >= 1 && args[0].ToLower() == "/zexall")
            {
                runZexall();
                return;
            }

			SanityUla("NOP      ", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum48(), new byte[] { 0x00 }, s_patternUla48_NOP);
			SanityUla("NOP      ", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum128(), new byte[] { 0x00 }, s_patternUla128_NOP);
			SanityUla("INC HL   ", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum128(), new byte[] { 0x23 }, s_patternUla128_INCHL);
			SanityUla("LD A,(HL)", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum128(), new byte[] { 0x7E }, s_patternUla128_LDA_HL_);
			SanityUla("LD (HL),A", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum128(), new byte[] { 0x77 }, s_patternUla128_LDA_HL_); // pattern the same as ld a,(hl)
			SanityUla("OUT (C),A", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum128(), new byte[] { 0xED, 0x79, 0x03 }, s_patternUla128_OUTCA);
			SanityUla("IN A,(C) ", new ZXMAK2.Engine.Devices.Ula.UlaSpectrum128(), new byte[] { 0xED, 0x78, 0x03 }, s_patternUla128_OUTCA); // pattern the same as out (c),a

			Console.WriteLine("=====Engine Performance Benchmark (500 frames rendering time)=====");
			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;
			int frameCount = 50 * 10;
			ExecTests("testVideo.z80", frameCount);
			ExecTests("testVideo.z80", frameCount);
			ExecTests("testVideo.z80", frameCount);
			ExecTests("zexall.sna", frameCount);
			ExecTests("zexall.sna", frameCount);
			ExecTests("zexall.sna", frameCount);
		}

		private static void SanityUla(string name, IUlaDevice ula, byte[] opcode, int[] pattern)
		{
			IMemoryDevice mem = new ZXMAK2.Engine.Devices.Memory.MemoryPentagon128();
			SpectrumConcrete p128 = new SpectrumConcrete();
			p128.Init();
			p128.BusManager.Disconnect();
			p128.BusManager.Clear(); 
			p128.BusManager.Add((BusDeviceBase)mem);
			p128.BusManager.Add((BusDeviceBase)ula);
			p128.BusManager.Connect();
			p128.IsRunning = true;
			p128.DoReset();
			p128.ExecuteFrame();
			p128.IsRunning = false;
		 	
			ushort offset = 0x4000;
			for (int i = 0; i < pattern.Length; i++)
			{
				for(int j=0; j < opcode.Length; j++)
					mem.WRMEM_DBG(offset++, opcode[j]);
			}
			p128.CPU.regs.PC = 0x4000;
			p128.CPU.regs.IR = 0x4000;
			p128.CPU.regs.SP = 0x4000;
			p128.CPU.regs.AF = 0x4000;
			p128.CPU.regs.HL = 0x4000;
			p128.CPU.regs.DE = 0x4000;
			p128.CPU.regs.BC = 0x4000;
			p128.CPU.regs.IX = 0x4000;
			p128.CPU.regs.IY = 0x4000;
			p128.CPU.regs._AF = 0x4000;
			p128.CPU.regs._HL = 0x4000;
			p128.CPU.regs._DE = 0x4000;
			p128.CPU.regs._BC = 0x4000;
			p128.CPU.regs.MW = 0x4000;
			p128.CPU.IFF1 = p128.CPU.IFF2 = false;
			p128.CPU.IM = 2;
			p128.CPU.BINT = false;
			p128.CPU.FX = ZXMAK2.Engine.Z80.OPFX.NONE;
			p128.CPU.XFX = ZXMAK2.Engine.Z80.OPXFX.NONE;
			
			long needsTact = pattern[0];
			long frameTact = p128.CPU.Tact % ula.FrameTactCount;
			long deltaTact = needsTact-frameTact;
			if(deltaTact < 0)
				deltaTact += ula.FrameTactCount;
			p128.CPU.Tact += deltaTact;

			for (int i = 0; i < pattern.Length-1; i++)
			{
				p128.DoStepInto();
				frameTact = p128.CPU.Tact % ula.FrameTactCount;
				if (frameTact != pattern[i + 1])
				{
					ConsoleColor tmp = Console.ForegroundColor;
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(
						"Sanity ULA {0} [{1}]:\tfailed @ {2}->{3} (should be {2}->{4})",
						ula.GetType().Name,
						name,
						pattern[i],
						frameTact,
						pattern[i + 1]);
					Console.ForegroundColor = tmp;
					return;
				}
			}
			ConsoleColor tmp2 = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Sanity ULA {0} [{1}]:\tpassed", ula.GetType().Name, name);
			Console.ForegroundColor = tmp2;
		}

		#region ULA PATTERNS

		private static int[] s_patternUla48_NOP = new int[]
		{
			14332, 
			14336, 14346, 14354, 14362, 14370, 14378, 14386, 14394, 14402, 14410, 14418, 14426, 14434, 14442, 14450, 14458, 14466,
			14470, 14474, 14478, 14482, 14486, 14490, 14494, 14498, 14502, 14506, 14510, 14514, 14518, 14522, 14526, 14530, 14534, 14538, 14542, 14546, 14550, 14554, 14558, 14562, 14570, 
			14578, 14586, 14594, 14602, 14610, 14618, 14626, 14634, 14642, 14650, 14658, 14666, 14674, 14682, 14690, 
			14694, 14698, 14702, 14706, 14710, 14714, 14718, 14722, 14726, 14730,
		};

		private static int[] s_patternUla128_NOP = new int[]
		{
			14362, 14372, 14380, 14388, 14396, 14404, 14412, 14420, 14428, 14436, 14444, 14452, 14460, 14468, 14476, 14484, 14492, 
			14496, 14500, 14504, 14508, 14512, 14516, 14520, 14524, 14528, 14532, 14536, 14540, 14544, 14548, 14552, 14556, 14560, 14564, 14568, 14572, 14576, 14580, 14584, 14588, 14592,
			14600, 14608, 14616, 14624, 14632, 14640, 14648, 14656, 14664, 14672, 14680, 14688, 14696, 14704, 14712, 14720, 14724, 
			14728, 14732, 14736,
		};

		private static int[] s_patternUla128_INCHL = new int[]
		{
			14362, 14378, 14394, 14410, 14426, 14442, 14458, 14474, 14490, 
			14496, 14502, 14508, 14514, 14520, 14526, 14532, 14538, 14544, 14550, 14556, 14562, 14568, 14574, 14580, 14586, 14598,
			14614, 14630, 14646, 14662, 14678, 14694, 14710, 14722, 14728,
			14734, 14740, 14746,
		};

		private static int[] s_patternUla128_LDA_HL_ = new int[]
		{
			14362, 14379, 14395, 14411, 14427, 14443, 14459, 14475, 14491, 
			14498, 14505, 14512, 14519, 14526, 14533, 14540, 14547, 14554, 14561, 14568, 14575, 14582, 14589, 14599,
			14615, 14631, 14647, 14663, 14679, 14695, 14711, 14723, 14730, 
			14737,
		};

		private static int[] s_patternUla128_OUTCA = new int[]
		{
			14362, 14388, 14402, 14434, 14450, 14476, 14490, 
			14502, 14508, 14520, 14526, 14538, 14544, 14556, 14562, 14574, 14580, 14592, 
			14606, 14638, 14654, 14680, 14694, 14720, 
			14726, 14738, 14744, 14756, 14762, 14774, 14780, 14792, 14798, 14810, 14816,
			14842, 14858, 14884, 14898, 14930, 14946, 14958, 14964, 14976, 14982,
		};

		#endregion

		private static void runZexall()
        {
            SpectrumConcrete p128 = new SpectrumConcrete();
            p128.Init();
            p128.IsRunning = true;
            p128.DoReset();
            p128.ExecuteFrame();
            p128.IsRunning = false;
			foreach (IKeyboardDevice kbd in p128.BusManager.FindDevices(typeof(IKeyboardDevice)))
				kbd.KeyboardState = new FakeKeyboardState(Key.Y);
			
            using (Stream testStream = GetTestStream("zexall.sna"))
                p128.Loader.GetSerializer(Path.GetExtension("zexall.sna")).Deserialize(testStream);
            p128.IsRunning = true;
            int frame;
            for (frame = 0; frame < 700000; frame++)
            {
                p128.ExecuteFrame();
                if (frame % 30000 == 0 || ((frame>630000 && frame <660000) && frame%10000==0))
                {
                    Console.WriteLine(string.Format("{0:D8}", frame));
                    p128.Loader.SaveFileName(string.Format("{0:D8}.PNG", frame));
                }
            }
            p128.Loader.SaveFileName(string.Format("{0:D8}.PNG", frame));
        }

		private static void ExecTests(string testName, int frameCount)
		{
			SpectrumConcrete p128 = new SpectrumConcrete();
			p128.Init();
			p128.IsRunning = true;
			p128.DoReset();
			p128.ExecuteFrame();

			p128.IsRunning = false;
			using (Stream testStream = GetTestStream(testName))
				p128.Loader.GetSerializer(Path.GetExtension(testName)).Deserialize(testStream);
			p128.IsRunning = true;


			Stopwatch watch = new Stopwatch();
			watch.Start();
			for (int frame = 0; frame < frameCount; frame++)
				p128.ExecuteFrame();
			watch.Stop();
			Console.WriteLine("{0}:\t{1} [ms]", testName, watch.ElapsedMilliseconds);
			//p128.Loader.SaveFileName(testName);
		}

		private static Stream GetTestStream(string testName)
		{
			testName = string.Format("Test.{0}", testName);
			return Assembly.GetExecutingAssembly().GetManifestResourceStream(testName);
		}
	}

	public class FakeKeyboardState : IKeyboardState
	{
		private bool m_pressSimulation = false;
		private Key m_key;

		public FakeKeyboardState()
		{
			m_pressSimulation = false;
		}
		
		public FakeKeyboardState(Key keyPressed)
		{
			m_key = keyPressed;
			m_pressSimulation = true;
		}

		public bool this[Key key]
		{
			get { return m_pressSimulation ? (key == m_key) : false; }
		}
	}
}
