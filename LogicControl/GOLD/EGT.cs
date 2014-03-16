using System;
using System.IO;


internal enum EGTRecord : byte
{
	InitialStates = 73,
	//I
	Symbol = 83,
	//S
	Production = 82,
	//R   R for Rule (related productions)
	DFAState = 68,
	//D
	LRState = 76,
	//L
	Property = 112,
	//p
	CharRanges = 99,
	//c 
	Group = 103,
	//g
	TableCounts = 116
	//t   Table Counts
}

internal class EGTReader
{
	public enum EntryType : byte
	{
		Empty = 69,
		//E
		UInt16 = 73,
		//I - Unsigned, 2 byte
		String = 83,
		//S - Unicode format
		Boolean = 66,
		//B - 1 Byte, Value is 0 or 1
		Byte = 98,
		//b
		Error = 0
	}


	public class IOException : System.Exception
	{

		public IOException(string Message, System.Exception Inner) : base(Message, Inner)
		{
		}

		public IOException(EntryType Type, BinaryReader Reader) : base("Type mismatch in file. Read '" + Type + "' at " + Reader.BaseStream.Position)
		{
		}
	}

	public class Entry
	{
		public EntryType Type;

		public object Value;
		public Entry()
		{
			Type = EntryType.Empty;
			Value = "";
		}

		public Entry(EntryType Type, object Value)
		{
			this.Type = Type;
			this.Value = Value;
		}
	}

		//M
	private const byte kRecordContentMulti = 77;
	private string m_FileHeader;

	private BinaryReader m_Reader;
	//Current record 
	private int m_EntryCount;

	private int m_EntriesRead;
	public bool RecordComplete()
	{
		return m_EntriesRead >= m_EntryCount;
	}

	public void Close()
	{
		if ((m_Reader != null)) {
			m_Reader.Close();
			m_Reader = null;
		}
	}

	public short EntryCount()
	{
		return (short)this.m_EntryCount;
	}

	public bool EndOfFile()
	{
		return m_Reader.BaseStream.Position == m_Reader.BaseStream.Length;
	}

	public string Header()
	{
		return m_FileHeader;
	}

	public void Open(BinaryReader Reader)
	{
		m_Reader = Reader;

		m_EntryCount = 0;
		m_EntriesRead = 0;
		m_FileHeader = RawReadCString();
	}

	public void Open(string Path)
	{
		Open(new BinaryReader(File.Open(Path, FileMode.Open, FileAccess.Read)));
	}

	public Entry RetrieveEntry()
	{
		byte Type = 0;
		Entry Result = new Entry();

		if (RecordComplete()) {
			Result.Type = EntryType.Empty;
			Result.Value = "";
		} else {
			m_EntriesRead += 1;
			Type = m_Reader.ReadByte();
			//Entry Type Prefix
			Result.Type = (EntryType)Type;

            switch ((EntryType)Type)
            {
				case EntryType.Empty:
					Result.Value = "";

					break;
				case EntryType.Boolean:
					byte b = 0;

					b = m_Reader.ReadByte();
					Result.Value = (b == 1);

					break;
				case EntryType.UInt16:
					Result.Value = RawReadUInt16();

					break;
				case EntryType.String:
					Result.Value = RawReadCString();

					break;
				case EntryType.Byte:
					Result.Value = m_Reader.ReadByte();

					break;
				default:
					Result.Type = EntryType.Error;
					Result.Value = "";
					break;
			}
		}

		return Result;
	}

	private UInt16 RawReadUInt16()
	{
		//Read a uint in little endian. This is the format already used
		//by the .NET BinaryReader. However, it is good to specificially
		//define this given byte order can change depending on platform.

		int b0 = 0;
		int b1 = 0;
		UInt16 Result = default(UInt16);

		b0 = m_Reader.ReadByte();
		//Least significant byte first
		b1 = m_Reader.ReadByte();

		Result = (ushort)((b1 << 8) + b0);

		return Result;
	}

	private string RawReadCString()
	{
		UInt16 Char16 = default(UInt16);
		string Text = "";
		bool Done = false;

		while (!(Done)) {
			Char16 = RawReadUInt16();
			if (Char16 == 0) {
				Done = true;
			} else {
				Text += (char)Char16;
			}
		}

		return Text;
	}


	public string RetrieveString()
	{
		Entry e = null;

		e = RetrieveEntry();
		if (e.Type == EntryType.String) {
			return (string)e.Value;
		} else {
			throw new IOException(e.Type, m_Reader);
		}
	}

    public int RetrieveInt16()
	{
		Entry e = null;

		e = RetrieveEntry();
		if (e.Type == EntryType.UInt16) {
			return (ushort)e.Value;
		} else {
			throw new IOException(e.Type, m_Reader);
		}
	}

	public bool RetrieveBoolean()
	{
		Entry e = null;

		e = RetrieveEntry();
		if (e.Type == EntryType.Boolean) {
			return (bool)e.Value;
		} else {
			throw new IOException(e.Type, m_Reader);
		}
	}

	public byte RetrieveByte()
	{
		Entry e = null;

		e = RetrieveEntry();
		if (e.Type == EntryType.Byte) {
			return (byte)e.Value;
		} else {
			throw new IOException(e.Type, m_Reader);
		}
	}

	public bool GetNextRecord()
	{
		byte ID = 0;
		bool Success = false;

		//==== Finish current record
		while (m_EntriesRead < m_EntryCount) {
			RetrieveEntry();
		}

		//==== Start next record
		ID = m_Reader.ReadByte();

		if (ID == kRecordContentMulti) {
			m_EntryCount = RawReadUInt16();
			m_EntriesRead = 0;
			Success = true;
		} else {
			Success = false;
		}

		return Success;
	}

	~EGTReader()
	{
		Close();
	}
}

