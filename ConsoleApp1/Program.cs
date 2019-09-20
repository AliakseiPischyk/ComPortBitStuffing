

using System;
using System.IO.Ports;
using System.Threading;

public enum ByteStufferWorkStrategy
{
    CODE,
    DECODE
};

public class StringContentException : System.Exception
{
    public StringContentException(String description =
        "Wrong string passed") : base(description) { }

    String Messege() { return Messege(); }
};

public class ByteStuffer
{
    private String pattern;

    bool consistsOf(String data, String ofWhat)
    {
        foreach (char letter in data)
        {
            if (!ofWhat.Contains(letter.ToString()))
            {
                return false;
            }
        }
        return true;
	}

	public ByteStuffer(String pattern = "01111110") { this.pattern = pattern; }

    public String CodeBytes(String bytes, ByteStufferWorkStrategy strategy, String pattern) 
        {
		if (!consistsOf(bytes, pattern)) {
			throw new StringContentException
            ("Exception in CodeBytrs method. Only ones and zeroes containing strings can be passed.");
		}

		int pattern_begin_pos = 0;
        int pattern_end_pos = 0;

        String data = new String(bytes.ToCharArray());

		while(true){
			pattern_begin_pos = data.IndexOf(pattern, pattern_end_pos);
			if (pattern_begin_pos == -1) {
				break;
			}
			pattern_end_pos = pattern_begin_pos + pattern.Length;
			if (strategy == ByteStufferWorkStrategy.CODE) {
				data = data.Insert(pattern_end_pos-1, "1");
			}
			else {	//strategy == DECODE
				data = data.Remove(pattern_end_pos-1, 1);
			}
		} 
		return data;
	}
};

public class PortChat
{
    static bool _continue;
    static SerialPort _serialPort;

    public static void Main()
    {
        string name;
        string message;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        Thread readThread = new Thread(Read);
        ByteStuffer byteStuffer = new ByteStuffer();

        // Create a new SerialPort object with default settings.
        _serialPort = new SerialPort();

        // Allow the user to set the appropriate properties.
        _serialPort.PortName = SetPortName(_serialPort.PortName);
        _serialPort.BaudRate = SetPortBaudRate(_serialPort.BaudRate);
        _serialPort.Parity = SetPortParity(_serialPort.Parity);
        _serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
        _serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
        _serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

        // Set the read/write timeouts
        _serialPort.ReadTimeout = 500;
        _serialPort.WriteTimeout = 500;

        _serialPort.Open();
        _continue = true;
        readThread.Start();

        Console.Write("Name: ");
        name = Console.ReadLine();

        Console.WriteLine("Type QUIT to exit");

        //data sending loop
        while (_continue)
        {
            message = Console.ReadLine();

            if (stringComparer.Equals("quit", message))
            {
                _continue = false;
            }
            else
            {
                String begin_end_flag = "01111110";
                String afterStuffing = byteStuffer.CodeBytes(message, ByteStufferWorkStrategy.CODE, begin_end_flag);
                Console.WriteLine("data after stuffing");
                Console.WriteLine(afterStuffing);
                String afterStuffingWithBeginFlag = afterStuffing.Insert(0, begin_end_flag);
                String afterStuffingWithBeginEndFlag = afterStuffingWithBeginFlag.Insert(afterStuffingWithBeginFlag.Length, begin_end_flag);
                Console.WriteLine("data after stuffing with flags");
                Console.WriteLine(afterStuffingWithBeginEndFlag);
                _serialPort.WriteLine(
                    String.Format("{1}", name, afterStuffingWithBeginEndFlag));
            }
        }

        readThread.Join();
        _serialPort.Close();
    }

    public static void Read()
    {
        ByteStuffer byteStuffer = new ByteStuffer();
        //data recieving loop
        while (_continue)
        {
            try

            {
                string message = _serialPort.ReadLine();
                Console.WriteLine("raw data");
                Console.WriteLine(message);

                String begin_end_flag = "01111110";
                String messageWithoutBeginFlag = message.Replace(begin_end_flag, "");
                Console.WriteLine("data w/o flags");
                Console.WriteLine(messageWithoutBeginFlag);
                Console.WriteLine("decoded data");
                Console.WriteLine(byteStuffer.CodeBytes(messageWithoutBeginFlag, ByteStufferWorkStrategy.DECODE,"0111111"));
            }
            catch (TimeoutException) { }
        }
    }

    
    public static string SetPortName(string defaultPortName)
    {
        string portName;

        Console.WriteLine("Available Ports:");
        foreach (string s in SerialPort.GetPortNames())
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter COM port name (Default: {0}): ", defaultPortName);
        portName = Console.ReadLine();

        if (portName == "" || !(portName.ToLower()).StartsWith("com"))
        {
            portName = defaultPortName;
        }
        return portName;
    }
    

    public static int SetPortBaudRate(int defaultPortBaudRate)
    {
        string baudRate;

        Console.Write("Baud Rate(default:{0}): ", defaultPortBaudRate);
        baudRate = Console.ReadLine();

        if (baudRate == "")
        {
            baudRate = defaultPortBaudRate.ToString();
        }

        return int.Parse(baudRate);
    }

   
    public static Parity SetPortParity(Parity defaultPortParity)
    {
        string parity;

        Console.WriteLine("Available Parity options:");
        foreach (string s in Enum.GetNames(typeof(Parity)))
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter Parity value (Default: {0}):", defaultPortParity.ToString(), true);
        parity = Console.ReadLine();

        if (parity == "")
        {
            parity = defaultPortParity.ToString();
        }

        return (Parity)Enum.Parse(typeof(Parity), parity, true);
    }
    

    public static int SetPortDataBits(int defaultPortDataBits)
    {
        string dataBits;

        Console.Write("Enter DataBits value (Default: {0}): ", defaultPortDataBits);
        dataBits = Console.ReadLine();

        if (dataBits == "")
        {
            dataBits = defaultPortDataBits.ToString();
        }

        return int.Parse(dataBits.ToUpperInvariant());
    }

    
    public static StopBits SetPortStopBits(StopBits defaultPortStopBits)
    {
        string stopBits;

        Console.WriteLine("Available StopBits options:");
        foreach (string s in Enum.GetNames(typeof(StopBits)))
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter StopBits value (None is not supported and \n" +
         "raises an ArgumentOutOfRangeException. \n (Default: {0}):", defaultPortStopBits.ToString());
        stopBits = Console.ReadLine();

        if (stopBits == "")
        {
            stopBits = defaultPortStopBits.ToString();
        }

        return (StopBits)Enum.Parse(typeof(StopBits), stopBits, true);
    }
    public static Handshake SetPortHandshake(Handshake defaultPortHandshake)
    {
        string handshake;

        Console.WriteLine("Available Handshake options:");
        foreach (string s in Enum.GetNames(typeof(Handshake)))
        {
            Console.WriteLine("   {0}", s);
        }

        Console.Write("Enter Handshake value (Default: {0}):", defaultPortHandshake.ToString());
        handshake = Console.ReadLine();

        if (handshake == "")
        {
            handshake = defaultPortHandshake.ToString();
        }

        return (Handshake)Enum.Parse(typeof(Handshake), handshake, true);
    }
}