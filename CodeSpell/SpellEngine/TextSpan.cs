public struct TextSpan
{
	public TextSpan(int start, int end) : this()
	{
		End = end;
		Start = start;
	}

	public int Start { get; set; }
	public int End { get; set; }

	public int Length { get { return End - Start; } }
}